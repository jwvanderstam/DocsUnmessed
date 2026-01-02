namespace DocsUnmessed.Connectors.Cloud.Dropbox;

using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

/// <summary>
/// Dropbox connector using API v2
/// </summary>
public sealed class DropboxConnector : CloudConnectorBase
{
    private readonly DropboxConfig _config;
    private readonly HttpClient _httpClient;
    private readonly HttpClient _contentClient;

    public override string ProviderName => "Dropbox";

    public DropboxConnector(
        DropboxConfig config,
        RateLimitConfig? rateLimitConfig = null,
        RetryPolicy? retryPolicy = null)
        : base(rateLimitConfig, retryPolicy)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_config.ApiBaseUrl)
        };

        _contentClient = new HttpClient
        {
            BaseAddress = new Uri(_config.ContentBaseUrl)
        };
    }

    public override async Task<CloudAuthResult> AuthenticateAsync(
        CloudCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(credentials.AccessToken))
        {
            return CloudAuthResult.Failure("Access token is required");
        }

        try
        {
            // Set authorization headers
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", credentials.AccessToken);
            _contentClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", credentials.AccessToken);

            // Test connection
            await ExecuteWithRateLimitAsync(async () =>
            {
                var request = new { query = "" };
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/users/get_current_account", content, cancellationToken);
                response.EnsureSuccessStatusCode();
            });

            IsAuthenticated = true;
            return CloudAuthResult.Success();
        }
        catch (HttpRequestException ex)
        {
            IsAuthenticated = false;
            return CloudAuthResult.Failure($"Authentication failed: {ex.Message}");
        }
    }

    public override async Task<IReadOnlyList<CloudFileInfo>> ListItemsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        var items = new List<CloudFileInfo>();
        string? cursor = null;
        bool hasMore = true;

        // Normalize path for Dropbox (must start with / or be empty for root)
        var dropboxPath = string.IsNullOrEmpty(path) || path == "/" ? "" : path;

        while (hasMore)
        {
            DropboxListFolderResult? result;

            if (cursor == null)
            {
                // Initial request
                var request = new
                {
                    path = dropboxPath,
                    recursive = false,
                    include_deleted = false,
                    limit = _config.MaxEntriesPerRequest
                };

                result = await ExecuteWithRateLimitAsync(async () =>
                {
                    var content = new StringContent(
                        JsonSerializer.Serialize(request),
                        Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync("/files/list_folder", content, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<DropboxListFolderResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                });
            }
            else
            {
                // Continue with cursor
                var request = new { cursor };

                result = await ExecuteWithRateLimitAsync(async () =>
                {
                    var content = new StringContent(
                        JsonSerializer.Serialize(request),
                        Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync("/files/list_folder/continue", content, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<DropboxListFolderResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                });
            }

            if (result?.Entries != null)
            {
                items.AddRange(result.Entries.Select(MapToCloudFileInfo));
                cursor = result.Cursor;
                hasMore = result.HasMore;
            }
            else
            {
                hasMore = false;
            }
        }

        return items;
    }

    public override async Task<string> UploadFileAsync(
        string localPath,
        string remotePath,
        IProgress<CloudUploadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        if (!File.Exists(localPath))
        {
            throw new FileNotFoundException("Local file not found", localPath);
        }

        var fileInfo = new FileInfo(localPath);

        // Small files: use upload directly
        if (fileInfo.Length < _config.UploadChunkSize)
        {
            return await UploadSmallFileAsync(localPath, remotePath, fileInfo.Length, progress, cancellationToken);
        }

        // Large files: use upload session
        return await UploadLargeFileAsync(localPath, remotePath, fileInfo.Length, progress, cancellationToken);
    }

    private async Task<string> UploadSmallFileAsync(
        string localPath,
        string remotePath,
        long fileSize,
        IProgress<CloudUploadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var fileBytes = await File.ReadAllBytesAsync(localPath, cancellationToken);

        var arg = new
        {
            path = remotePath,
            mode = "add",
            autorename = false,
            mute = false
        };

        var content = new ByteArrayContent(fileBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var request = new HttpRequestMessage(HttpMethod.Post, "/files/upload")
        {
            Content = content
        };
        request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(arg));

        var response = await _contentClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        progress?.Report(new CloudUploadProgress
        {
            BytesUploaded = fileSize,
            TotalBytes = fileSize,
            PercentComplete = 100
        });

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<DropboxItem>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Id ?? string.Empty;
    }

    private async Task<string> UploadLargeFileAsync(
        string localPath,
        string remotePath,
        long fileSize,
        IProgress<CloudUploadProgress>? progress,
        CancellationToken cancellationToken)
    {
        // Start upload session
        var sessionId = await StartUploadSessionAsync(cancellationToken);

        // Upload chunks
        using var fileStream = File.OpenRead(localPath);
        var buffer = new byte[_config.UploadChunkSize];
        long offset = 0;

        while (offset < fileSize)
        {
            var bytesRead = await fileStream.ReadAsync(buffer, 0, _config.UploadChunkSize, cancellationToken);
            if (bytesRead == 0) break;

            await AppendUploadSessionAsync(sessionId, offset, buffer, bytesRead, cancellationToken);
            offset += bytesRead;

            progress?.Report(new CloudUploadProgress
            {
                BytesUploaded = offset,
                TotalBytes = fileSize,
                PercentComplete = (double)offset / fileSize * 100
            });
        }

        // Finish session
        return await FinishUploadSessionAsync(sessionId, offset, remotePath, cancellationToken);
    }

    private async Task<string> StartUploadSessionAsync(CancellationToken cancellationToken)
    {
        var arg = new { close = false };
        var content = new ByteArrayContent(Array.Empty<byte>());

        var request = new HttpRequestMessage(HttpMethod.Post, "/files/upload_session/start")
        {
            Content = content
        };
        request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(arg));

        var response = await _contentClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        return result?["session_id"] ?? throw new InvalidOperationException("Failed to start upload session");
    }

    private async Task AppendUploadSessionAsync(
        string sessionId,
        long offset,
        byte[] buffer,
        int length,
        CancellationToken cancellationToken)
    {
        var arg = new
        {
            cursor = new { session_id = sessionId, offset },
            close = false
        };

        var content = new ByteArrayContent(buffer, 0, length);

        var request = new HttpRequestMessage(HttpMethod.Post, "/files/upload_session/append_v2")
        {
            Content = content
        };
        request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(arg));

        var response = await _contentClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> FinishUploadSessionAsync(
        string sessionId,
        long offset,
        string remotePath,
        CancellationToken cancellationToken)
    {
        var arg = new
        {
            cursor = new { session_id = sessionId, offset },
            commit = new
            {
                path = remotePath,
                mode = "add",
                autorename = false,
                mute = false
            }
        };

        var content = new ByteArrayContent(Array.Empty<byte>());

        var request = new HttpRequestMessage(HttpMethod.Post, "/files/upload_session/finish")
        {
            Content = content
        };
        request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(arg));

        var response = await _contentClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<DropboxItem>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Id ?? string.Empty;
    }

    public override async Task DownloadFileAsync(
        string remotePath,
        string localPath,
        IProgress<CloudDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        var arg = new { path = remotePath };

        var request = new HttpRequestMessage(HttpMethod.Post, "/files/download");
        request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(arg));

        var response = await _contentClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        var buffer = new byte[8192];

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = File.Create(localPath);

        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalBytesRead += bytesRead;

            progress?.Report(new CloudDownloadProgress
            {
                BytesDownloaded = totalBytesRead,
                TotalBytes = totalBytes,
                PercentComplete = totalBytes > 0 ? (double)totalBytesRead / totalBytes * 100 : 0
            });
        }
    }

    public override async Task DeleteAsync(
        string remotePath,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        var request = new { path = remotePath };

        await ExecuteWithRateLimitAsync(async () =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/files/delete_v2", content, cancellationToken);
            response.EnsureSuccessStatusCode();
        });
    }

    public override async Task<CloudFileInfo?> GetItemAsync(
        string remotePath,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        var request = new { path = remotePath };

        return await ExecuteWithRateLimitAsync(async () =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/files/get_metadata", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var item = JsonSerializer.Deserialize<DropboxItem>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return item != null ? MapToCloudFileInfo(item) : null;
        });
    }

    private static CloudFileInfo MapToCloudFileInfo(DropboxItem item)
    {
        return new CloudFileInfo
        {
            Id = item.Id,
            Name = item.Name,
            Path = item.PathDisplay,
            Size = item.Size,
            IsFolder = item.IsFolder,
            CreatedAt = item.ClientModified ?? DateTime.UtcNow,
            ModifiedAt = item.ServerModified ?? DateTime.UtcNow,
            Hash = item.ContentHash
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
            _contentClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
