namespace DocsUnmessed.Core.Configuration;

/// <summary>
/// Configuration for file mapping rules
/// </summary>
public sealed class MappingRule
{
    public required string Name { get; init; }
    public required MappingMatch Match { get; init; }
    public required MappingTarget Target { get; init; }
    public required string ConflictPolicy { get; init; }
    public int Priority { get; init; } = 100;
}

public sealed class MappingMatch
{
    public string? Provider { get; init; }
    public string? PathRegex { get; init; }
    public string[]? Extensions { get; init; }
    public int? AgeDaysMin { get; init; }
    public int? AgeDaysMax { get; init; }
    public long? MinSize { get; init; }
    public long? MaxSize { get; init; }
    public string[]? Keywords { get; init; }
}

public sealed class MappingTarget
{
    public required string Location { get; init; }
    public string? NamingTemplate { get; init; }
    public bool CreateShortcut { get; init; }
}
