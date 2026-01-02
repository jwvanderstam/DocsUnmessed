namespace DocsUnmessed.Core.Domain;

/// <summary>
/// Represents the results of a storage assessment scan
/// </summary>
public sealed class ScanResult
{
    public required string ScanId { get; init; }
    public required DateTime StartedUtc { get; init; }
    public DateTime? CompletedUtc { get; set; }
    public required string[] Providers { get; init; }
    public List<Item> Items { get; init; } = new();
    public ScanStatistics Statistics { get; init; } = new();
    public List<DuplicateSet> Duplicates { get; init; } = new();
    public List<ValidationIssue> Issues { get; init; } = new();
}

public sealed class ScanStatistics
{
    public long TotalFiles { get; set; }
    public long TotalFolders { get; set; }
    public long TotalSize { get; set; }
    public int MaxDepth { get; set; }
    public int FilesWithIssues { get; set; }
    public Dictionary<string, long> SizeByProvider { get; init; } = new();
    public Dictionary<string, long> CountByProvider { get; init; } = new();
}

public sealed class DuplicateSet
{
    public required string Hash { get; init; }
    public required long Size { get; init; }
    public required List<Item> Items { get; init; }
    public DuplicateType Type { get; init; }
}

public enum DuplicateType
{
    Exact,
    Probable
}

public sealed class ValidationIssue
{
    public required string ItemPath { get; init; }
    public required IssueType Type { get; init; }
    public required string Description { get; init; }
    public required string Severity { get; init; }
}

public enum IssueType
{
    NonCompliantName,
    TooDeep,
    OverDense,
    OutsidePrimaryCloud,
    InvalidCharacters,
    PathTooLong,
    MissingMetadata
}
