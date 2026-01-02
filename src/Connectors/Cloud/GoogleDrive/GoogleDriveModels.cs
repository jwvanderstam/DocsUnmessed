namespace DocsUnmessed.Connectors.Cloud.GoogleDrive;

/// <summary>
/// Configuration for Google Drive connector
/// </summary>
public sealed class GoogleDriveConfig
{
    /// <summary>
    /// OAuth 2.0 Client ID from Google Cloud Console
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// OAuth 2.0 Client Secret
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Redirect URI for OAuth flow
    /// </summary>
    public string RedirectUri { get; init; } = "http://localhost:8080";

    /// <summary>
    /// Google Drive API base URL
    /// </summary>
    public string ApiBaseUrl { get; init; } = "https://www.googleapis.com/drive/v3";

    /// <summary>
    /// Upload API base URL
    /// </summary>
    public string UploadBaseUrl { get; init; } = "https://www.googleapis.com/upload/drive/v3";

    /// <summary>
    /// OAuth scopes required
    /// </summary>
    public string[] Scopes { get; init; } = new[]
    {
        "https://www.googleapis.com/auth/drive.file",
        "https://www.googleapis.com/auth/drive.readonly",
        "https://www.googleapis.com/auth/userinfo.profile"
    };

    /// <summary>
    /// Maximum page size for list operations (1-1000)
    /// </summary>
    public int MaxPageSize { get; init; } = 1000;

    /// <summary>
    /// Upload chunk size (must be multiple of 256KB)
    /// </summary>
    public int UploadChunkSize { get; init; } = 5 * 1024 * 1024; // 5MB
}

/// <summary>
/// Google Drive file/folder metadata
/// </summary>
public sealed class GoogleDriveItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? MimeType { get; set; }
    public long? Size { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
    public string[]? Parents { get; set; }
    public bool IsFolder => MimeType == "application/vnd.google-apps.folder";
    public string? Md5Checksum { get; set; }
}

/// <summary>
/// Google Drive API response for file list
/// </summary>
public sealed class GoogleDriveFileList
{
    public required GoogleDriveItem[] Files { get; set; }
    public string? NextPageToken { get; set; }
}
