namespace DocsUnmessed.Core.Configuration;

/// <summary>
/// Configuration for directory and file exclusions
/// </summary>
public sealed class ExcludeConfig
{
    /// <summary>
    /// Gets or sets whether to use default exclusions
    /// </summary>
    public bool UseDefaultExclusions { get; init; } = true;

    /// <summary>
    /// Gets or sets custom excluded directories (relative or absolute paths)
    /// </summary>
    public IReadOnlyList<string> ExcludedDirectories { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets custom excluded file patterns (wildcards supported)
    /// </summary>
    public IReadOnlyList<string> ExcludedFilePatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether to enable category-based migration
    /// </summary>
    public bool EnableCategoryMigration { get; init; } = true;

    /// <summary>
    /// Gets or sets the target directory for migrated files
    /// </summary>
    public string MigratedDirectory { get; init; } = "migrated";

    /// <summary>
    /// Gets or sets whether this is a dry run (test mode - no actual moves)
    /// </summary>
    public bool IsDryRun { get; init; } = false;

    /// <summary>
    /// Gets the default system directories to exclude
    /// </summary>
    public static IReadOnlyList<string> DefaultSystemDirectories { get; } = new[]
    {
        // Windows system directories
        "$Recycle.Bin",
        "System Volume Information",
        "Windows",
        "Program Files",
        "Program Files (x86)",
        "ProgramData",
        "AppData",
        
        // macOS system directories
        ".Spotlight-V100",
        ".Trashes",
        ".fseventsd",
        "Library",
        "System",
        "Applications",
        
        // Linux system directories
        "proc",
        "sys",
        "dev",
        "boot",
        "bin",
        "sbin",
        
        // Common hidden/system
        ".git",
        ".svn",
        ".hg",
        "node_modules",
        ".vs",
        ".vscode",
        "obj",
        "bin"
    };

    /// <summary>
    /// Gets the default user directories to exclude
    /// </summary>
    public static IReadOnlyList<string> DefaultUserDirectories { get; } = new[]
    {
        "Downloads",
        "Desktop",
        "Pictures",
        "Music",
        "Videos",
        "Documents" // Can be overridden by user
    };

    /// <summary>
    /// Gets all excluded directories (default + custom)
    /// </summary>
    public IReadOnlyList<string> GetAllExcludedDirectories()
    {
        var excluded = new List<string>();

        if (UseDefaultExclusions)
        {
            excluded.AddRange(DefaultSystemDirectories);
            excluded.AddRange(DefaultUserDirectories);
        }

        excluded.AddRange(ExcludedDirectories);

        return excluded;
    }

    /// <summary>
    /// Gets default configuration with all exclusions enabled
    /// </summary>
    public static ExcludeConfig Default => new()
    {
        UseDefaultExclusions = true,
        EnableCategoryMigration = true,
        IsDryRun = false
    };

    /// <summary>
    /// Gets configuration for dry run mode
    /// </summary>
    public static ExcludeConfig DryRun => new()
    {
        UseDefaultExclusions = true,
        EnableCategoryMigration = true,
        IsDryRun = true
    };

    /// <summary>
    /// Gets configuration with no exclusions
    /// </summary>
    public static ExcludeConfig None => new()
    {
        UseDefaultExclusions = false,
        EnableCategoryMigration = false,
        IsDryRun = false
    };
}

/// <summary>
/// File category for automatic organization
/// </summary>
public enum FileCategory
{
    Unknown,
    Picture,
    Music,
    Video,
    Document,
    Archive,
    Code,
    Data
}

/// <summary>
/// Service for detecting file categories
/// </summary>
public static class FileCategoryDetector
{
    private static readonly Dictionary<string, FileCategory> _extensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Pictures
        [".jpg"] = FileCategory.Picture,
        [".jpeg"] = FileCategory.Picture,
        [".png"] = FileCategory.Picture,
        [".gif"] = FileCategory.Picture,
        [".bmp"] = FileCategory.Picture,
        [".svg"] = FileCategory.Picture,
        [".webp"] = FileCategory.Picture,
        [".ico"] = FileCategory.Picture,
        [".tif"] = FileCategory.Picture,
        [".tiff"] = FileCategory.Picture,
        [".raw"] = FileCategory.Picture,
        [".heic"] = FileCategory.Picture,
        [".heif"] = FileCategory.Picture,
        
        // Music
        [".mp3"] = FileCategory.Music,
        [".wav"] = FileCategory.Music,
        [".flac"] = FileCategory.Music,
        [".aac"] = FileCategory.Music,
        [".ogg"] = FileCategory.Music,
        [".wma"] = FileCategory.Music,
        [".m4a"] = FileCategory.Music,
        [".opus"] = FileCategory.Music,
        [".aiff"] = FileCategory.Music,
        
        // Videos
        [".mp4"] = FileCategory.Video,
        [".avi"] = FileCategory.Video,
        [".mkv"] = FileCategory.Video,
        [".mov"] = FileCategory.Video,
        [".wmv"] = FileCategory.Video,
        [".flv"] = FileCategory.Video,
        [".webm"] = FileCategory.Video,
        [".m4v"] = FileCategory.Video,
        [".mpg"] = FileCategory.Video,
        [".mpeg"] = FileCategory.Video,
        
        // Documents
        [".pdf"] = FileCategory.Document,
        [".doc"] = FileCategory.Document,
        [".docx"] = FileCategory.Document,
        [".xls"] = FileCategory.Document,
        [".xlsx"] = FileCategory.Document,
        [".ppt"] = FileCategory.Document,
        [".pptx"] = FileCategory.Document,
        [".txt"] = FileCategory.Document,
        [".rtf"] = FileCategory.Document,
        [".odt"] = FileCategory.Document,
        [".ods"] = FileCategory.Document,
        [".odp"] = FileCategory.Document,
        
        // Archives
        [".zip"] = FileCategory.Archive,
        [".rar"] = FileCategory.Archive,
        [".7z"] = FileCategory.Archive,
        [".tar"] = FileCategory.Archive,
        [".gz"] = FileCategory.Archive,
        [".bz2"] = FileCategory.Archive,
        [".xz"] = FileCategory.Archive,
        
        // Code
        [".cs"] = FileCategory.Code,
        [".java"] = FileCategory.Code,
        [".py"] = FileCategory.Code,
        [".js"] = FileCategory.Code,
        [".ts"] = FileCategory.Code,
        [".cpp"] = FileCategory.Code,
        [".c"] = FileCategory.Code,
        [".h"] = FileCategory.Code,
        [".go"] = FileCategory.Code,
        [".rs"] = FileCategory.Code,
        [".rb"] = FileCategory.Code,
        [".php"] = FileCategory.Code,
        [".html"] = FileCategory.Code,
        [".css"] = FileCategory.Code,
        [".sql"] = FileCategory.Code,
        
        // Data
        [".json"] = FileCategory.Data,
        [".xml"] = FileCategory.Data,
        [".csv"] = FileCategory.Data,
        [".yaml"] = FileCategory.Data,
        [".yml"] = FileCategory.Data,
        [".db"] = FileCategory.Data,
        [".sqlite"] = FileCategory.Data,
        [".log"] = FileCategory.Data
    };

    /// <summary>
    /// Detects file category based on extension
    /// </summary>
    public static FileCategory DetectCategory(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return FileCategory.Unknown;
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            return FileCategory.Unknown;
        }

        return _extensionMap.TryGetValue(extension, out var category) 
            ? category 
            : FileCategory.Unknown;
    }

    /// <summary>
    /// Gets the target subdirectory for a category
    /// </summary>
    public static string GetCategoryDirectory(FileCategory category)
    {
        return category switch
        {
            FileCategory.Picture => "Pictures",
            FileCategory.Music => "Music",
            FileCategory.Video => "Videos",
            FileCategory.Document => "Documents",
            FileCategory.Archive => "Archives",
            FileCategory.Code => "Code",
            FileCategory.Data => "Data",
            _ => "Other"
        };
    }
}
