namespace DocsUnmessed.Core.Configuration;

/// <summary>
/// Target Information Architecture blueprint
/// </summary>
public sealed class TiaBlueprint
{
    public required int Version { get; init; }
    public required List<RootCategory> Root { get; init; }
    public required TiaRules Rules { get; init; }
    public required PrimaryStorage Primary { get; init; }
    public ArchiveStorage? Archive { get; init; }
    public TiaOptions? Options { get; init; }
}

public sealed class RootCategory
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public List<string>? Subcategories { get; init; }
}

public sealed class TiaRules
{
    public required int MaxDepth { get; init; }
    public required bool NumericPrefixes { get; init; }
    public int? MaxFilesPerFolder { get; init; }
}

public sealed class PrimaryStorage
{
    public required string OneDriveRoot { get; init; }
}

public sealed class ArchiveStorage
{
    public required List<string> Providers { get; init; }
}

public sealed class TiaOptions
{
    public bool UseLocalSyncFoldersOnly { get; init; }
    public bool EnableExifMetadata { get; init; }
    public bool EnableTelemetry { get; init; }
}
