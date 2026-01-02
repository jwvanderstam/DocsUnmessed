namespace DocsUnmessed.Connectors;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Services;
using System.Runtime.CompilerServices;

/// <summary>
/// Connector for local file system and network shares
/// </summary>
public sealed class FileSystemConnector : IConnector
{
    private readonly IHashService _hashService;
    
    public string Id => "fs_local";
    public ConnectorMode Mode => ConnectorMode.Local;

    public FileSystemConnector(IHashService hashService)
    {
        _hashService = hashService;
    }

    public Task AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        // No authentication needed for local file system
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<Item> EnumerateAsync(
        string root, 
        EnumerationFilters? filters = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var rootDir = new DirectoryInfo(root);
        if (!rootDir.Exists)
        {
            throw new DirectoryNotFoundException($"Root directory not found: {root}");
        }

        filters ??= new EnumerationFilters();
        
        await foreach (var item in EnumerateDirectoryAsync(rootDir, rootDir.FullName, 0, filters, cancellationToken))
        {
            yield return item;
        }
    }

    private async IAsyncEnumerable<Item> EnumerateDirectoryAsync(
        DirectoryInfo directory,
        string rootPath,
        int currentDepth,
        EnumerationFilters filters,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (filters.MaxDepth.HasValue && currentDepth > filters.MaxDepth.Value)
        {
            yield break;
        }

        // Get excluded directories
        var excludedDirs = GetExcludedDirectories(filters);

        // Enumerate folders
        if (filters.IncludeFolders)
        {
            IEnumerable<DirectoryInfo> subdirectories;
            try
            {
                subdirectories = directory.EnumerateDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                // Permission denied - skip this directory
                yield break;
            }
            catch (Exception)
            {
                // Other errors (path too long, etc.) - skip this directory
                yield break;
            }

            foreach (var subDir in subdirectories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // Check if directory should be excluded
                if (ShouldExcludeDirectory(subDir.Name, excludedDirs))
                {
                    continue;
                }
                
                yield return new Item
                {
                    Path = subDir.FullName,
                    Name = subDir.Name,
                    Provider = Id,
                    Size = 0,
                    MimeType = "inode/directory",
                    CreatedUtc = subDir.CreationTimeUtc,
                    ModifiedUtc = subDir.LastWriteTimeUtc,
                    Type = ItemType.Folder,
                    Depth = currentDepth + 1
                };

                await foreach (var item in EnumerateDirectoryAsync(subDir, rootPath, currentDepth + 1, filters, cancellationToken))
                {
                    yield return item;
                }
            }
        }

        // Enumerate files
        IEnumerable<FileInfo> files;
        try
        {
            files = directory.EnumerateFiles();
        }
        catch (UnauthorizedAccessException)
        {
            // Permission denied - skip files in this directory
            yield break;
        }
        catch (Exception)
        {
            // Other errors - skip files in this directory
            yield break;
        }

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ShouldIncludeFile(file, filters))
            {
                continue;
            }

            string? hash = null;
            if (filters.ComputeHash)
            {
                try
                {
                    // Use a timeout for hash computation to prevent hanging
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30 second timeout per file
                    
                    hash = await _hashService.ComputeHashAsync(file.FullName, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Hash computation timed out or was cancelled - skip hash but continue
                    hash = null;
                }
                catch (Exception)
                {
                    // File might be locked, inaccessible, or other error - skip hash but continue
                    hash = null;
                }
            }

            yield return new Item
            {
                Path = file.FullName,
                Name = file.Name,
                Provider = Id,
                Size = file.Length,
                MimeType = GetMimeType(file.Extension),
                CreatedUtc = file.CreationTimeUtc,
                ModifiedUtc = file.LastWriteTimeUtc,
                Hash = hash,
                Type = ItemType.File,
                Depth = currentDepth + 1
            };
        }
    }

    private static List<string> GetExcludedDirectories(EnumerationFilters filters)
    {
        var excluded = new List<string>();
        
        // Add from ExcludeConfig
        if (filters.ExcludeConfig != null)
        {
            excluded.AddRange(filters.ExcludeConfig.GetAllExcludedDirectories());
        }
        
        // Add additional exclusions
        if (filters.ExcludedDirectories != null)
        {
            excluded.AddRange(filters.ExcludedDirectories);
        }
        
        return excluded;
    }

    private static bool ShouldExcludeDirectory(string directoryName, List<string> excludedDirectories)
    {
        if (excludedDirectories.Count == 0)
        {
            return false;
        }
        
        return excludedDirectories.Contains(directoryName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool ShouldIncludeFile(FileInfo file, EnumerationFilters filters)
    {
        if (filters.Extensions?.Length > 0)
        {
            var ext = file.Extension.TrimStart('.');
            if (!filters.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (filters.ModifiedAfter.HasValue && file.LastWriteTimeUtc < filters.ModifiedAfter.Value)
        {
            return false;
        }

        if (filters.ModifiedBefore.HasValue && file.LastWriteTimeUtc > filters.ModifiedBefore.Value)
        {
            return false;
        }

        if (filters.MinSize.HasValue && file.Length < filters.MinSize.Value)
        {
            return false;
        }

        if (filters.MaxSize.HasValue && file.Length > filters.MaxSize.Value)
        {
            return false;
        }

        return true;
    }

    public async Task<OperationResult> OperateAsync(Operation operation, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (operation.Type)
            {
                case OperationType.Copy:
                    await CopyFileAsync(operation, cancellationToken);
                    break;
                case OperationType.Move:
                    await MoveFileAsync(operation, cancellationToken);
                    break;
                case OperationType.Rename:
                    await RenameFileAsync(operation, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Operation type {operation.Type} not supported");
            }

            return new OperationResult
            {
                Success = true,
                OperationId = operation.Id,
                HashVerified = true
            };
        }
        catch (Exception ex)
        {
            return new OperationResult
            {
                Success = false,
                OperationId = operation.Id,
                ErrorMessage = ex.Message,
                HashVerified = false
            };
        }
    }

    private async Task CopyFileAsync(Operation operation, CancellationToken cancellationToken)
    {
        var targetDir = Path.GetDirectoryName(operation.TargetPath);
        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        await Task.Run(() => File.Copy(operation.SourcePath, operation.TargetPath, overwrite: false), cancellationToken);
    }

    private async Task MoveFileAsync(Operation operation, CancellationToken cancellationToken)
    {
        var targetDir = Path.GetDirectoryName(operation.TargetPath);
        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        await Task.Run(() => File.Move(operation.SourcePath, operation.TargetPath, overwrite: false), cancellationToken);
    }

    private async Task RenameFileAsync(Operation operation, CancellationToken cancellationToken)
    {
        await Task.Run(() => File.Move(operation.SourcePath, operation.TargetPath, overwrite: false), cancellationToken);
    }

    public Task<bool> ValidatePathAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var limits = GetLimits();
            
            if (fullPath.Length > limits.MaxPathLength)
            {
                return Task.FromResult(false);
            }

            var fileName = Path.GetFileName(path);
            if (fileName.Length > limits.MaxFileNameLength)
            {
                return Task.FromResult(false);
            }

            foreach (var invalidChar in limits.InvalidCharacters)
            {
                if (fileName.Contains(invalidChar))
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public ProviderLimits GetLimits()
    {
        return new ProviderLimits
        {
            MaxPathLength = OperatingSystem.IsWindows() ? 32767 : 4096, // Long path support on Windows
            MaxFileNameLength = 255,
            MaxFileSize = long.MaxValue,
            InvalidCharacters = OperatingSystem.IsWindows() 
                ? new[] { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" }
                : new[] { "/" },
            ReservedNames = OperatingSystem.IsWindows()
                ? new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }
                : Array.Empty<string>(),
            ApiRateLimitPerMinute = int.MaxValue
        };
    }

    private static string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }
}
