namespace DocsUnmessed.Services.Migration;

using DocsUnmessed.Core.Domain;
using DocsUnmessed.Services.Templates;

/// <summary>
/// Generates target paths for migration using templates
/// </summary>
public sealed class PathGenerator
{
    private readonly TemplateEngine _templateEngine;
    private readonly MigrationPlanningConfig _config;

    /// <summary>
    /// Initializes a new instance of the PathGenerator class
    /// </summary>
    /// <param name="config">Migration planning configuration</param>
    public PathGenerator(MigrationPlanningConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _templateEngine = new TemplateEngine();
    }

    /// <summary>
    /// Generates a target path for an item
    /// </summary>
    /// <param name="item">Source item</param>
    /// <param name="template">Optional template override</param>
    /// <returns>Generated target path</returns>
    public string GenerateTargetPath(Item item, string? template = null)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var templateToUse = template ?? _config.DefaultNamingTemplate;

        // Create template context from item
        var context = new TemplateContext
        {
            FileName = Path.GetFileNameWithoutExtension(item.Name),
            Extension = Path.GetExtension(item.Name)?.TrimStart('.'),
            FileType = item.Type.ToString(),
            Provider = item.Provider,
            Category = DetermineCategory(item),
            Date = item.ModifiedUtc,
            Counter = 0 // Will be set if conflicts occur
        };

        // Process template
        var relativePath = _templateEngine.Process(templateToUse, context);

        // Combine with target root
        var fullPath = Path.Combine(_config.TargetRootPath, relativePath);

        // Normalize path
        fullPath = Path.GetFullPath(fullPath);

        return fullPath;
    }

    /// <summary>
    /// Generates a unique target path by adding a counter
    /// </summary>
    /// <param name="basePath">Base path</param>
    /// <param name="existingPaths">Set of existing paths</param>
    /// <param name="maxAttempts">Maximum number of attempts</param>
    /// <returns>Unique path</returns>
    public string GenerateUniquePath(string basePath, HashSet<string> existingPaths, int maxAttempts = 1000)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            throw new ArgumentException("Base path cannot be null or empty", nameof(basePath));
        }

        if (!existingPaths.Contains(basePath))
        {
            return basePath;
        }

        var directory = Path.GetDirectoryName(basePath) ?? string.Empty;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(basePath);
        var extension = Path.GetExtension(basePath);

        for (int i = 1; i <= maxAttempts; i++)
        {
            var newPath = Path.Combine(directory, $"{fileNameWithoutExt}_{i:D3}{extension}");
            
            if (!existingPaths.Contains(newPath))
            {
                return newPath;
            }
        }

        throw new InvalidOperationException($"Could not generate unique path after {maxAttempts} attempts");
    }

    /// <summary>
    /// Validates a path
    /// </summary>
    /// <param name="path">Path to validate</param>
    /// <returns>Validation result</returns>
    public PathValidationResult ValidatePath(string path)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add("Path cannot be null or empty");
            return new PathValidationResult { IsValid = false, Errors = errors };
        }

        // Check length
        if (path.Length > _config.MaxPathLength)
        {
            errors.Add($"Path exceeds maximum length of {_config.MaxPathLength} characters");
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (path.Any(c => invalidChars.Contains(c)))
        {
            errors.Add("Path contains invalid characters");
        }

        // Check if path is absolute
        if (!Path.IsPathFullyQualified(path))
        {
            errors.Add("Path must be fully qualified");
        }

        return new PathValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    private static string DetermineCategory(Item item)
    {
        // Simple category determination based on extension
        var extension = Path.GetExtension(item.Name)?.ToLowerInvariant();

        return extension switch
        {
            ".pdf" or ".doc" or ".docx" or ".txt" or ".md" => "Documents",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Images",
            ".mp4" or ".avi" or ".mkv" or ".mov" => "Videos",
            ".mp3" or ".wav" or ".flac" or ".m4a" => "Audio",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "Archives",
            ".exe" or ".dll" or ".app" => "Applications",
            ".xlsx" or ".xls" or ".csv" => "Spreadsheets",
            ".pptx" or ".ppt" => "Presentations",
            _ => "Other"
        };
    }
}

/// <summary>
/// Path validation result
/// </summary>
public sealed class PathValidationResult
{
    /// <summary>
    /// Gets or sets whether the path is valid
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Gets or sets validation errors
    /// </summary>
    public required IReadOnlyList<string> Errors { get; init; }
}
