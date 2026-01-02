namespace DocsUnmessed.Connectors.Cloud.GoogleDrive;

using DocsUnmessed.Connectors.Cloud;
using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

/// <summary>
/// Google Drive connector using Drive API v3
/// </summary>
public sealed class GoogleDriveConnector : CloudConnectorBase
{
    private readonly GoogleDriveConfig _config;
    private readonly HttpClient _httpClient;

    public override string ProviderName => "GoogleDrive";

    public GoogleDriveConnector(
        GoogleDriveConfig config,
        RateLimitConfig? rateLimitConfig = null,
        RetryPolicy? retryPolicy = null)
        : base(rateLimitConfig, retryPolicy)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_config.ApiBaseUrl)
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
            // Set authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", credentials.AccessToken);

            // Test connection by getting user info
            await ExecuteWithRateLimitAsync(async () =>
            {
                var response = await _httpClient.GetAsync(
                    "https://www.googleapis.com/drive/v3/about?fields=user",
                    cancellationToken);

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
        string? pageToken = null;

        do
        {
            var queryParams = new Dictionary<string, string>
            {
                ["pageSize"] = _config.MaxPageSize.ToString(),
                ["fields"] = "nextPageToken,files(id,name,mimeType,size,createdTime,modifiedTime,md5Checksum,parents)"
            };

            if (!string.IsNullOrEmpty(pageToken))
            {
                queryParams["pageToken"] = pageToken;
            }

            // Build query string
            var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            var fileList = await ExecuteWithRateLimitAsync(async () =>
            {
                var response = await _httpClient.GetAsync($"files?{query}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<GoogleDriveFileList>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            });

            if (fileList?.Files != null)
            {
                items.AddRange(fileList.Files.Select(MapToCloudFileInfo));
                pageToken = fileList.NextPageToken;
            }
            else
            {
                break;
            }

        } while (!string.IsNullOrEmpty(pageToken));

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
        var fileName = Path.GetFileName(remotePath);

        // Create metadata
        var metadata = new
        {
            name = fileName,
            mimeType = GetMimeType(localPath)
        };

        var metadataJson = JsonSerializer.Serialize(metadata);

        // Use resumable upload for large files
        return await UploadFileResumableAsync(
            localPath,
            metadataJson,
            fileInfo.Length,
            progress,
            cancellationToken);
    }

    private async Task<string> UploadFileResumableAsync(
        string localPath,
        string metadataJson,
        long fileSize,
        IProgress<CloudUploadProgress>? progress,
        CancellationToken cancellationToken)
    {
        // Step 1: Initiate resumable upload
        var uploadUrl = await InitiateResumableUploadAsync(metadataJson, fileSize, cancellationToken);

        // Step 2: Upload chunks
        using var fileStream = File.OpenRead(localPath);
        var buffer = new byte[_config.UploadChunkSize];
        long totalBytesUploaded = 0;

        while (totalBytesUploaded < fileSize)
        {
            var bytesRead = await fileStream.ReadAsync(buffer, 0, _config.UploadChunkSize, cancellationToken);
            if (bytesRead == 0) break;

            var chunk = new ByteArrayContent(buffer, 0, bytesRead);
            chunk.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            chunk.Headers.ContentRange = new ContentRangeHeaderValue(
                totalBytesUploaded,
                totalBytesUploaded + bytesRead - 1,
                fileSize);

            var response = await _httpClient.PutAsync(uploadUrl, chunk, cancellationToken);

            totalBytesUploaded += bytesRead;

            progress?.Report(new CloudUploadProgress
            {
                BytesUploaded = totalBytesUploaded,
                TotalBytes = fileSize,
                PercentComplete = (double)totalBytesUploaded / fileSize * 100
            });

            if (response.IsSuccessStatusCode)
            {
                // Upload complete
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<GoogleDriveItem>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Id ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private async Task<string> InitiateResumableUploadAsync(
        string metadataJson,
        long fileSize,
        CancellationToken cancellationToken)
    {
        var content = new StringContent(metadataJson, System.Text.Encoding.UTF8, "application/json");
        content.Headers.Add("X-Upload-Content-Length", fileSize.ToString());

        var response = await _httpClient.PostAsync(
            $"{_config.UploadBaseUrl}/files?uploadType=resumable",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        if (response.Headers.Location != null)
        {
            return response.Headers.Location.ToString();
        }

        throw new InvalidOperationException("Failed to get upload URL");
    }

    public override async Task DownloadFileAsync(
        string remotePath,
        string localPath,
        IProgress<CloudDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        // Get file ID from path (simplified - in real scenario, you'd resolve path to ID)
        var fileId = remotePath;

        var response = await _httpClient.GetAsync(
            $"files/{fileId}?alt=media",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

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

        var fileId = remotePath;

        await ExecuteWithRateLimitAsync(async () =>
        {
            var response = await _httpClient.DeleteAsync($"files/{fileId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        });
    }

    public override async Task<CloudFileInfo?> GetItemAsync(
        string remotePath,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAuthenticated();

        var fileId = remotePath;

        return await ExecuteWithRateLimitAsync(async () =>
        {
            var response = await _httpClient.GetAsync(
                $"files/{fileId}?fields=id,name,mimeType,size,createdTime,modifiedTime,md5Checksum",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var item = JsonSerializer.Deserialize<GoogleDriveItem>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return item != null ? MapToCloudFileInfo(item) : null;
        });
    }

    private static CloudFileInfo MapToCloudFileInfo(GoogleDriveItem item)
    {
        return new CloudFileInfo
        {
            Id = item.Id,
            Name = item.Name,
            Path = item.Id, // Google Drive uses IDs, not paths
            Size = item.Size ?? 0,
            IsFolder = item.IsFolder,
            CreatedAt = item.CreatedTime ?? DateTime.UtcNow,
            ModifiedAt = item.ModifiedTime ?? DateTime.UtcNow,
            Hash = item.Md5Checksum
        };
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
