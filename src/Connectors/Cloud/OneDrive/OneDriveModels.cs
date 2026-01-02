namespace DocsUnmessed.Connectors.Cloud.OneDrive;

/// <summary>
/// OneDrive connector configuration
/// </summary>
public sealed class OneDriveConfig
{
    /// <summary>
    /// Gets or sets the Microsoft Graph API base URL
    /// </summary>
    public string GraphApiBaseUrl { get; init; } = "https://graph.microsoft.com/v1.0";

    /// <summary>
    /// Gets or sets the client ID for Azure AD app
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets or sets the tenant ID (or "common" for multi-tenant)
    /// </summary>
    public string TenantId { get; init; } = "common";

    /// <summary>
    /// Gets or sets the redirect URI for OAuth
    /// </summary>
    public string RedirectUri { get; init; } = "http://localhost:8080";

    /// <summary>
    /// Gets or sets the scopes required
    /// </summary>
    public string[] Scopes { get; init; } = new[]
    {
        "Files.Read",
        "Files.ReadWrite",
        "Files.Read.All",
        "Files.ReadWrite.All",
        "User.Read"
    };

    /// <summary>
    /// Gets or sets the chunk size for uploads (bytes)
    /// </summary>
    public int UploadChunkSize { get; init; } = 320 * 1024; // 320KB (Microsoft recommendation)

    /// <summary>
    /// Gets or sets whether to use delta queries for listing
    /// </summary>
    public bool UseDeltaQueries { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum page size for list operations
    /// </summary>
    public int MaxPageSize { get; init; } = 200;
}

/// <summary>
/// OneDrive Drive information
/// </summary>
public sealed class OneDriveDriveInfo
{
    /// <summary>
    /// Gets or sets the drive ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the drive name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the drive type
    /// </summary>
    public required string DriveType { get; init; }

    /// <summary>
    /// Gets or sets the owner information
    /// </summary>
    public string? Owner { get; init; }

    /// <summary>
    /// Gets or sets the web URL
    /// </summary>
    public string? WebUrl { get; init; }
}

/// <summary>
/// OneDrive item metadata
/// </summary>
public sealed class OneDriveItemMetadata
{
    /// <summary>
    /// Gets or sets the item ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the item name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the item size
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// Gets or sets whether this is a folder
    /// </summary>
    public bool IsFolder { get; init; }

    /// <summary>
    /// Gets or sets the parent path
    /// </summary>
    public string? ParentPath { get; init; }

    /// <summary>
    /// Gets or sets the created date time
    /// </summary>
    public DateTime CreatedDateTime { get; init; }

    /// <summary>
    /// Gets or sets the last modified date time
    /// </summary>
    public DateTime LastModifiedDateTime { get; init; }

    /// <summary>
    /// Gets or sets the web URL
    /// </summary>
    public string? WebUrl { get; init; }

    /// <summary>
    /// Gets or sets the download URL (for files)
    /// </summary>
    public string? DownloadUrl { get; init; }

    /// <summary>
    /// Gets or sets the SHA1 hash (for files)
    /// </summary>
    public string? Sha1Hash { get; init; }

    /// <summary>
    /// Gets or sets the quick XOR hash
    /// </summary>
    public string? QuickXorHash { get; init; }

    /// <summary>
    /// Gets or sets whether the item is shared
    /// </summary>
    public bool IsShared { get; init; }

    /// <summary>
    /// Gets or sets the MIME type
    /// </summary>
    public string? MimeType { get; init; }
}
