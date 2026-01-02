namespace DocsUnmessed.Connectors.Cloud;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Interface for cloud storage connectors
/// </summary>
public interface ICloudConnector : IDisposable
{
    /// <summary>
    /// Gets the provider name
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether the connector is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Authenticates with the cloud provider
    /// </summary>
    /// <param name="credentials">Authentication credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result</returns>
    Task<AuthenticationResult> AuthenticateAsync(
        CloudCredentials credentials,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists items in a path
    /// </summary>
    /// <param name="path">Path to list</param>
    /// <param name="recursive">Whether to list recursively</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of items</returns>
    Task<IReadOnlyList<Item>> ListItemsAsync(
        string path,
        bool recursive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file
    /// </summary>
    /// <param name="remotePath">Remote path</param>
    /// <param name="localPath">Local destination path</param>
    /// <param name="progress">Optional progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DownloadFileAsync(
        string remotePath,
        string localPath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file
    /// </summary>
    /// <param name="localPath">Local source path</param>
    /// <param name="remotePath">Remote destination path</param>
    /// <param name="progress">Optional progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadFileAsync(
        string localPath,
        string remotePath,
        IProgress<TransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file or folder
    /// </summary>
    /// <param name="path">Path to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about an item
    /// </summary>
    /// <param name="path">Item path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Item information</returns>
    Task<Item?> GetItemAsync(
        string path,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Cloud provider credentials
/// </summary>
public sealed class CloudCredentials
{
    /// <summary>
    /// Gets or sets the authentication type
    /// </summary>
    public required AuthenticationType Type { get; init; }

    /// <summary>
    /// Gets or sets the access token (for OAuth)
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Gets or sets the refresh token (for OAuth)
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Gets or sets the API key
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Gets or sets the username (for basic auth)
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets or sets the password (for basic auth)
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Gets or sets additional properties
    /// </summary>
    public IReadOnlyDictionary<string, string> AdditionalProperties { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Authentication types
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// OAuth 2.0
    /// </summary>
    OAuth2,

    /// <summary>
    /// API Key
    /// </summary>
    ApiKey,

    /// <summary>
    /// Basic authentication
    /// </summary>
    Basic
}

/// <summary>
/// Authentication result
/// </summary>
public sealed class AuthenticationResult
{
    /// <summary>
    /// Gets or sets whether authentication was successful
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets or sets the error message if failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets the user information
    /// </summary>
    public CloudUserInfo? UserInfo { get; init; }

    /// <summary>
    /// Gets or sets when the token expires
    /// </summary>
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Cloud user information
/// </summary>
public sealed class CloudUserInfo
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets or sets the user email
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets or sets the quota information
    /// </summary>
    public QuotaInfo? Quota { get; init; }
}

/// <summary>
/// Storage quota information
/// </summary>
public sealed class QuotaInfo
{
    /// <summary>
    /// Gets or sets the total space in bytes
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Gets or sets the used space in bytes
    /// </summary>
    public long UsedBytes { get; init; }

    /// <summary>
    /// Gets the available space in bytes
    /// </summary>
    public long AvailableBytes => TotalBytes - UsedBytes;
}

/// <summary>
/// Transfer progress information
/// </summary>
public sealed class TransferProgress
{
    /// <summary>
    /// Gets or sets the total bytes
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Gets or sets the transferred bytes
    /// </summary>
    public long TransferredBytes { get; init; }

    /// <summary>
    /// Gets the progress percentage (0-100)
    /// </summary>
    public double PercentComplete => TotalBytes > 0 
        ? (TransferredBytes / (double)TotalBytes) * 100 
        : 0;

    /// <summary>
    /// Gets or sets the transfer rate in bytes per second
    /// </summary>
    public double BytesPerSecond { get; init; }
}
