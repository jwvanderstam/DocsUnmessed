namespace DocsUnmessed.Connectors.Cloud.OneDrive;

using DocsUnmessed.Connectors.Cloud.RateLimiting;
using DocsUnmessed.Connectors.Cloud.Retry;
using DocsUnmessed.Core.Domain;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// OneDrive cloud connector using Microsoft Graph API
/// </summary>
public sealed class OneDriveConnector : CloudConnectorBase
{
    private readonly HttpClient _httpClient;
    private readonly OneDriveConfig _config;
    private string? _accessToken;
    private string? _driveId;

    /// <summary>
    /// Initializes a new instance of the OneDriveConnector class
    /// </summary>
    /// <param name="config">OneDrive configuration</param>
    /// <param name="rateLimitConfig">Rate limiting configuration</param>
    /// <param name="retryPolicy">Retry policy</param>
    public OneDriveConnector(
        OneDriveConfig config,
        RateLimitConfig? rateLimitConfig = null,
        RetryPolicy? retryPolicy = null)
        : base(rateLimitConfig, retryPolicy)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_config.GraphApiBaseUrl)
        };
    }

    /// <inheritdoc/>
    public override string ProviderName => "OneDrive";

    /// <inheritdoc/>
    public override bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

    /// <inheritdoc/>
    public override async Task<AuthenticationResult> AuthenticateAsync(
        CloudCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteApiCallAsync(async () =>
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            if (string.IsNullOrEmpty(credentials.AccessToken))
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Access token is required"
                };
            }

            _accessToken = credentials.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);

            // Get user info
            var userResponse = await _httpClient.GetAsync("me", cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
            {
                _accessToken = null;
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Authentication failed: {userResponse.StatusCode}"
                };
            }

            var userJson = await userResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            
            // Get drive info
            var driveResponse = await _httpClient.GetAsync("me/drive", cancellationToken);
            if (driveResponse.IsSuccessStatusCode)
            {
                var driveJson = await driveResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                _driveId = driveJson.GetProperty("id").GetString();
            }

            // Get quota
            var quotaResponse = await _httpClient.GetAsync("me/drive", cancellationToken);
            QuotaInfo? quota = null;
            if (quotaResponse.IsSuccessStatusCode)
            {
                var quotaJson = await quotaResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                if (quotaJson.TryGetProperty("quota", out var quotaElement))
                {
                    quota = new QuotaInfo
                    {
                        TotalBytes = quotaElement.GetProperty("total").GetInt64(),
                        UsedBytes = quotaElement.GetProperty("used").GetInt64()
                    };
                }
            }

            return new AuthenticationResult
            {
                IsSuccess = true,
                UserInfo = new CloudUserInfo
                {
                    UserId = userJson.GetProperty("id").GetString() ?? "unknown",
                    Email = userJson.TryGetProperty("mail", out var mail) || userJson.TryGetProperty("userPrincipalName", out mail)
                        ? mail.GetString()
                        : null,
                    DisplayName = userJson.TryGetProperty("displayName", out var name)
                        ? name.GetString()
                        : null,
                    Quota = quota
                }
            };
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Item>> ListItemsAsync(
        string path,
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteApiCallAsync(async () =>
        {
            ThrowIfNotAuthenticated();

            var items = new List<Item>();
            var endpoint = string.IsNullOrEmpty(path) || path == "/"
                ? "me/drive/root/children"
                : $"me/drive/root:/{path.TrimStart('/')}:/children";

            await EnumerateItemsAsync(endpoint, items, recursive, 0, cancellationToken);

            return (IReadOnlyList<Item>)items;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task DownloadFileAsync(
        string remotePath,
        string localPath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteApiCallAsync(async () =>
        {
            ThrowIfNotAuthenticated();

            var endpoint = $"me/drive/root:/{remotePath.TrimStart('/')}:/content";
            
            using var response = await _httpClient.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var buffer = new byte[81920]; // 80KB buffer
            var totalRead = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true);

            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                progress?.Report(new TransferProgress
                {
                    TotalBytes = totalBytes,
                    TransferredBytes = totalRead,
                    BytesPerSecond = 0 // Could calculate if needed
                });
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task UploadFileAsync(
        string localPath,
        string remotePath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteApiCallAsync(async () =>
        {
            ThrowIfNotAuthenticated();

            var fileInfo = new FileInfo(localPath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Local file not found", localPath);
            }

            // For small files, use simple upload
            if (fileInfo.Length < _config.UploadChunkSize * 10)
            {
                await SimpleUploadAsync(localPath, remotePath, fileInfo.Length, progress, cancellationToken);
            }
            else
            {
                await ChunkedUploadAsync(localPath, remotePath, fileInfo.Length, progress, cancellationToken);
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        await ExecuteApiCallAsync(async () =>
        {
            ThrowIfNotAuthenticated();

            var endpoint = $"me/drive/root:/{path.TrimStart('/')}";
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<Item?> GetItemAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteApiCallAsync(async () =>
        {
            ThrowIfNotAuthenticated();

            var endpoint = $"me/drive/root:/{path.TrimStart('/')}";
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            return MapToItem(json, path);
        }, cancellationToken);
    }

    private async Task EnumerateItemsAsync(
        string endpoint,
        List<Item> items,
        bool recursive,
        int depth,
        CancellationToken cancellationToken)
    {
        var url = $"{endpoint}?$top={_config.MaxPageSize}";
        
        while (!string.IsNullOrEmpty(url))
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var values = json.GetProperty("value");

            foreach (var itemJson in values.EnumerateArray())
            {
                var path = GetItemPath(itemJson);
                var item = MapToItem(itemJson, path, depth);
                items.Add(item);

                // Recurse into folders if requested
                if (recursive && item.Type == ItemType.Folder)
                {
                    var childEndpoint = $"me/drive/items/{itemJson.GetProperty("id").GetString()}/children";
                    await EnumerateItemsAsync(childEndpoint, items, true, depth + 1, cancellationToken);
                }
            }

            // Handle pagination
            url = json.TryGetProperty("@odata.nextLink", out var nextLink)
                ? nextLink.GetString() ?? string.Empty
                : string.Empty;
        }
    }

    private async Task SimpleUploadAsync(
        string localPath,
        string remotePath,
        long fileSize,
        IProgress<TransferProgress>? progress,
        CancellationToken cancellationToken)
    {
        var endpoint = $"me/drive/root:/{remotePath.TrimStart('/')}:/content";
        
        await using var fileStream = File.OpenRead(localPath);
        using var content = new StreamContent(fileStream);
        
        var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        progress?.Report(new TransferProgress
        {
            TotalBytes = fileSize,
            TransferredBytes = fileSize,
            BytesPerSecond = 0
        });
    }

    private async Task ChunkedUploadAsync(
        string localPath,
        string remotePath,
        long fileSize,
        IProgress<TransferProgress>? progress,
        CancellationToken cancellationToken)
    {
        // Create upload session
        var sessionEndpoint = $"me/drive/root:/{remotePath.TrimStart('/')}:/createUploadSession";
        var sessionResponse = await _httpClient.PostAsync(sessionEndpoint, null, cancellationToken);
        sessionResponse.EnsureSuccessStatusCode();

        var sessionJson = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var uploadUrl = sessionJson.GetProperty("uploadUrl").GetString();

        if (string.IsNullOrEmpty(uploadUrl))
        {
            throw new InvalidOperationException("Failed to create upload session");
        }

        // Upload chunks
        await using var fileStream = File.OpenRead(localPath);
        var buffer = new byte[_config.UploadChunkSize];
        long uploadedBytes = 0;

        while (uploadedBytes < fileSize)
        {
            var bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0) break;

            using var chunkContent = new ByteArrayContent(buffer, 0, bytesRead);
            chunkContent.Headers.ContentLength = bytesRead;
            chunkContent.Headers.ContentRange = new ContentRangeHeaderValue(
                uploadedBytes,
                uploadedBytes + bytesRead - 1,
                fileSize);

            var chunkResponse = await _httpClient.PutAsync(uploadUrl, chunkContent, cancellationToken);
            chunkResponse.EnsureSuccessStatusCode();

            uploadedBytes += bytesRead;

            progress?.Report(new TransferProgress
            {
                TotalBytes = fileSize,
                TransferredBytes = uploadedBytes,
                BytesPerSecond = 0
            });
        }
    }

    private Item MapToItem(JsonElement json, string path, int depth = 0)
    {
        var isFolder = json.TryGetProperty("folder", out _);
        var name = json.GetProperty("name").GetString() ?? "unknown";
        var size = json.TryGetProperty("size", out var sizeElement) ? sizeElement.GetInt64() : 0;

        return new Item
        {
            Path = path,
            Name = name,
            Provider = ProviderName,
            Size = size,
            Type = isFolder ? ItemType.Folder : ItemType.File,
            MimeType = json.TryGetProperty("file", out var file) && file.TryGetProperty("mimeType", out var mime)
                ? mime.GetString()
                : null,
            CreatedUtc = json.TryGetProperty("createdDateTime", out var created)
                ? created.GetDateTime()
                : DateTime.UtcNow,
            ModifiedUtc = json.TryGetProperty("lastModifiedDateTime", out var modified)
                ? modified.GetDateTime()
                : DateTime.UtcNow,
            Hash = json.TryGetProperty("file", out var fileElement) &&
                   fileElement.TryGetProperty("hashes", out var hashes) &&
                   hashes.TryGetProperty("sha1Hash", out var sha1)
                ? sha1.GetString()
                : null,
            ExtendedProperties = new Dictionary<string, string>
            {
                ["OneDriveId"] = json.GetProperty("id").GetString() ?? string.Empty,
                ["WebUrl"] = json.TryGetProperty("webUrl", out var webUrl) ? webUrl.GetString() ?? string.Empty : string.Empty
            },
            Issues = new List<string>(),
            IsShared = json.TryGetProperty("shared", out _),
            Depth = depth
        };
    }

    private static string GetItemPath(JsonElement json)
    {
        if (json.TryGetProperty("parentReference", out var parentRef) &&
            parentRef.TryGetProperty("path", out var pathElement))
        {
            var path = pathElement.GetString() ?? string.Empty;
            // Remove "/drive/root:" prefix
            path = path.Replace("/drive/root:", "");
            var name = json.GetProperty("name").GetString() ?? "unknown";
            return string.IsNullOrEmpty(path) ? $"/{name}" : $"{path}/{name}";
        }

        return $"/{json.GetProperty("name").GetString() ?? "unknown"}";
    }

    private void ThrowIfNotAuthenticated()
    {
        if (!IsAuthenticated)
        {
            throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync first.");
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
