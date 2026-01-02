namespace DocsUnmessed.Connectors.Cloud.Dropbox;

/// <summary>
/// Configuration for Dropbox connector
/// </summary>
public sealed class DropboxConfig
{
    /// <summary>
    /// OAuth 2.0 App Key from Dropbox App Console
    /// </summary>
    public required string AppKey { get; init; }

    /// <summary>
    /// OAuth 2.0 App Secret
    /// </summary>
    public required string AppSecret { get; init; }

    /// <summary>
    /// Redirect URI for OAuth flow
    /// </summary>
    public string RedirectUri { get; init; } = "http://localhost:8080";

    /// <summary>
    /// Dropbox API v2 base URL
    /// </summary>
    public string ApiBaseUrl { get; init; } = "https://api.dropboxapi.com/2";

    /// <summary>
    /// Dropbox content API base URL
    /// </summary>
    public string ContentBaseUrl { get; init; } = "https://content.dropboxapi.com/2";

    /// <summary>
    /// Upload chunk size (max 150MB)
    /// </summary>
    public int UploadChunkSize { get; init; } = 8 * 1024 * 1024; // 8MB

    /// <summary>
    /// Maximum entries per list request (1-2000)
    /// </summary>
    public int MaxEntriesPerRequest { get; init; } = 2000;
}

/// <summary>
/// Dropbox file/folder metadata
/// </summary>
public sealed class DropboxItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string PathDisplay { get; set; }
    public string Tag { get; set; } = "file"; // "file" or "folder"
    public long Size { get; set; }
    public DateTime? ClientModified { get; set; }
    public DateTime? ServerModified { get; set; }
    public string? ContentHash { get; set; }
    public bool IsFolder => Tag == "folder";
}

/// <summary>
/// Dropbox API response for list folder
/// </summary>
public sealed class DropboxListFolderResult
{
    public required DropboxItem[] Entries { get; set; }
    public string? Cursor { get; set; }
    public bool HasMore { get; set; }
}
