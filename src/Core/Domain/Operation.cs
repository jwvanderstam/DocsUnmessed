namespace DocsUnmessed.Core.Domain;

/// <summary>
/// Represents a file operation to be performed during migration
/// </summary>
public sealed class Operation
{
    public required string Id { get; init; }
    public required OperationType Type { get; init; }
    public required string SourcePath { get; init; }
    public required string SourceProvider { get; init; }
    public required string TargetPath { get; init; }
    public required string TargetProvider { get; init; }
    public required Item SourceItem { get; init; }
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
    public DateTime? ExecutedUtc { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CorrelationId { get; init; }
    public int RetryCount { get; set; }
}

public enum OperationType
{
    Copy,
    Move,
    Rename,
    CreateShortcut,
    Delete,
    Archive
}

public enum OperationStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Skipped,
    Rolled_Back
}
