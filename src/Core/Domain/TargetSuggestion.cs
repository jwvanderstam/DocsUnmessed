namespace DocsUnmessed.Core.Domain;

/// <summary>
/// Represents a recommended target location and name for an item
/// </summary>
public sealed class TargetSuggestion
{
    public required string TargetPath { get; init; }
    public required string TargetName { get; init; }
    public required string RuleName { get; init; }
    public required double Confidence { get; init; }
    public List<string> Reasons { get; init; } = new();
    public Dictionary<string, string> TokensApplied { get; init; } = new();
    public ConflictResolution ConflictPolicy { get; init; }
}

public enum ConflictResolution
{
    VersionSuffix,
    TimestampSuffix,
    Manual,
    Skip
}
