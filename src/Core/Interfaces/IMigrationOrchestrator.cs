namespace DocsUnmessed.Core.Interfaces;

using DocsUnmessed.Core.Domain;

/// <summary>
/// Orchestrates migration operations with batching, checkpointing, and retry logic
/// </summary>
public interface IMigrationOrchestrator
{
    Task<MigrationPlan> CreatePlanAsync(string scanId, MigrationOptions options, CancellationToken cancellationToken = default);
    Task<MigrationResult> ExecutePlanAsync(MigrationPlan plan, CancellationToken cancellationToken = default);
    Task<MigrationPlan> SimulatePlanAsync(string scanId, MigrationOptions options, CancellationToken cancellationToken = default);
    Task PauseMigrationAsync(string migrationId);
    Task ResumeMigrationAsync(string migrationId);
}

public sealed record MigrationPlan
{
    public required string PlanId { get; init; }
    public required string ScanId { get; init; }
    public required List<Operation> Operations { get; init; }
    public required MigrationMetrics Metrics { get; init; }
    public required bool IsWhatIf { get; init; }
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
}

public sealed record MigrationOptions
{
    public bool WhatIf { get; init; }
    public int BatchSize { get; init; } = 500;
    public TimeSpan? OffHoursStart { get; init; }
    public TimeSpan? OffHoursEnd { get; init; }
    public bool NonDestructive { get; init; } = true;
    public bool VerifyHashes { get; init; } = true;
    public int MaxRetries { get; init; } = 3;
}

public sealed record MigrationResult
{
    public required string MigrationId { get; init; }
    public required int TotalOperations { get; init; }
    public required int CompletedOperations { get; init; }
    public required int FailedOperations { get; init; }
    public required int SkippedOperations { get; init; }
    public required DateTime StartedUtc { get; init; }
    public DateTime? CompletedUtc { get; init; }
    public List<Operation> FailedItems { get; init; } = new();
}

public sealed record MigrationMetrics
{
    public int TotalFiles { get; init; }
    public long TotalSize { get; init; }
    public int DepthReduction { get; init; }
    public int DuplicatesEliminated { get; init; }
    public int ComplianceUplift { get; init; }
}


















