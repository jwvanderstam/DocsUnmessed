namespace DocsUnmessed.Services.Duplicates;

/// <summary>
/// Configuration for duplicate detection
/// </summary>
public sealed class DuplicateDetectionConfig
{
    /// <summary>
    /// Gets or sets whether to use exact hash matching
    /// </summary>
    public bool UseExactHash { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to use partial hash comparison
    /// </summary>
    public bool UsePartialHash { get; init; } = true;

    /// <summary>
    /// Gets or sets the partial hash size in bytes
    /// </summary>
    public int PartialHashSize { get; init; } = 4096; // 4KB

    /// <summary>
    /// Gets or sets whether to use name similarity detection
    /// </summary>
    public bool UseNameSimilarity { get; init; } = true;

    /// <summary>
    /// Gets or sets the minimum name similarity threshold (0.0 to 1.0)
    /// </summary>
    public double NameSimilarityThreshold { get; init; } = 0.85;

    /// <summary>
    /// Gets or sets whether to use size-based grouping
    /// </summary>
    public bool UseSizeGrouping { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum size difference percentage
    /// </summary>
    public double MaxSizeDifferencePercent { get; init; } = 0.01; // 1%

    /// <summary>
    /// Gets or sets whether to use date-based grouping
    /// </summary>
    public bool UseDateGrouping { get; init; } = false;

    /// <summary>
    /// Gets or sets the maximum date difference in hours
    /// </summary>
    public int MaxDateDifferenceHours { get; init; } = 24;

    /// <summary>
    /// Gets or sets the minimum file size to consider (bytes)
    /// </summary>
    public long MinimumFileSize { get; init; } = 1024; // 1KB

    /// <summary>
    /// Gets the default configuration
    /// </summary>
    public static DuplicateDetectionConfig Default => new();

    /// <summary>
    /// Gets a conservative configuration (fewer false positives)
    /// </summary>
    public static DuplicateDetectionConfig Conservative => new()
    {
        UseExactHash = true,
        UsePartialHash = false,
        UseNameSimilarity = false,
        UseSizeGrouping = false,
        UseDateGrouping = false
    };

    /// <summary>
    /// Gets an aggressive configuration (more potential duplicates)
    /// </summary>
    public static DuplicateDetectionConfig Aggressive => new()
    {
        UseExactHash = true,
        UsePartialHash = true,
        UseNameSimilarity = true,
        NameSimilarityThreshold = 0.75,
        UseSizeGrouping = true,
        MaxSizeDifferencePercent = 0.05,
        UseDateGrouping = true,
        MaxDateDifferenceHours = 48
    };
}

/// <summary>
/// Represents a potential duplicate match
/// </summary>
public sealed class DuplicateMatch
{
    /// <summary>
    /// Gets or sets the first item ID
    /// </summary>
    public required string ItemId1 { get; init; }

    /// <summary>
    /// Gets or sets the second item ID
    /// </summary>
    public required string ItemId2 { get; init; }

    /// <summary>
    /// Gets or sets the match confidence (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets or sets the detection method
    /// </summary>
    public required DuplicateDetectionMethod Method { get; init; }

    /// <summary>
    /// Gets or sets the match details
    /// </summary>
    public string? Details { get; init; }
}

/// <summary>
/// Duplicate detection methods
/// </summary>
public enum DuplicateDetectionMethod
{
    /// <summary>
    /// Exact hash match
    /// </summary>
    ExactHash,

    /// <summary>
    /// Partial hash match
    /// </summary>
    PartialHash,

    /// <summary>
    /// Name similarity
    /// </summary>
    NameSimilarity,

    /// <summary>
    /// Size and date match
    /// </summary>
    SizeAndDate
}

/// <summary>
/// Result of duplicate detection
/// </summary>
public sealed class DuplicateDetectionResult
{
    /// <summary>
    /// Gets or sets the duplicate matches found
    /// </summary>
    public required IReadOnlyList<DuplicateMatch> Matches { get; init; }

    /// <summary>
    /// Gets or sets the number of items analyzed
    /// </summary>
    public int ItemsAnalyzed { get; init; }

    /// <summary>
    /// Gets or sets the detection duration
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets or sets the configuration used
    /// </summary>
    public required DuplicateDetectionConfig Configuration { get; init; }
}
