namespace DocsUnmessed.Services;

using System.Security.Cryptography;

public interface IHashService
{
    Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> VerifyHashAsync(string filePath, string expectedHash, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for computing SHA-256 file hashes
/// </summary>
public sealed class HashService : IHashService
{
    private const int BufferSize = 8192;
    private readonly long _largeFileThreshold = 100 * 1024 * 1024; // 100 MB

    public async Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filePath);
        
        // For very large files, consider block hashing or skipping
        if (fileInfo.Length > _largeFileThreshold)
        {
            // Could implement block hashing here for performance
        }

        await using var stream = new FileStream(
            filePath, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            BufferSize, 
            useAsync: true);

        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<bool> VerifyHashAsync(string filePath, string expectedHash, CancellationToken cancellationToken = default)
    {
        var actualHash = await ComputeHashAsync(filePath, cancellationToken);
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}
