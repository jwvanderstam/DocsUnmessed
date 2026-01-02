namespace DocsUnmessed.Core.Domain;

/// <summary>
/// Represents a file or folder item discovered during inventory
/// </summary>
public sealed class Item
{
    public required string Path { get; init; }
    public required string Name { get; init; }
    public required string Provider { get; init; }
    public required long Size { get; init; }
    public required string MimeType { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required DateTime ModifiedUtc { get; init; }
    public string? Hash { get; init; }
    public ItemType Type { get; init; }
    public Dictionary<string, string> ExtendedProperties { get; init; } = new();
    public List<string> Issues { get; init; } = new();
    public bool IsShared { get; init; }
    public int Depth { get; init; }
}

public enum ItemType
{
    File,
    Folder,
    Shortcut,
    Link
}
