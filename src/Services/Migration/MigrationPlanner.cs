namespace DocsUnmessed.Services.Migration;

using DocsUnmessed.Core.Domain;
using System.Diagnostics;

/// <summary>
/// Plans file migration operations
/// </summary>
public sealed class MigrationPlanner
{
    private readonly PathGenerator _pathGenerator;
    private readonly ConflictDetector _conflictDetector;
    private readonly MigrationPlanningConfig _config;

    /// <summary>
    /// Initializes a new instance of the MigrationPlanner class
    /// </summary>
    /// <param name="config">Migration planning configuration</param>
    public MigrationPlanner(MigrationPlanningConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _pathGenerator = new PathGenerator(config);
        _conflictDetector = new ConflictDetector(config);
    }

    /// <summary>
    /// Creates a migration plan for items
    /// </summary>
    /// <param name="scanId">Scan ID</param>
    /// <param name="items">Items to migrate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Migration plan</returns>
    public async Task<MigrationPlan> CreatePlanAsync(
        string scanId,
        IEnumerable<Item> items,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scanId))
        {
            throw new ArgumentException("Scan ID cannot be null or empty", nameof(scanId));
        }

        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var stopwatch = Stopwatch.StartNew();
        var operations = new List<MigrationOperation>();
        var usedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var itemList = items.Where(i => i.Type == ItemType.File).ToList();

        // Generate operations
        foreach (var item in itemList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var operation = await Task.Run(() => CreateOperation(item, usedPaths), cancellationToken);
            operations.Add(operation);
            usedPaths.Add(operation.TargetPath);
        }

        // Detect conflicts
        var conflicts = _config.DetectConflicts 
            ? _conflictDetector.DetectConflicts(operations)
            : new List<PathConflict>();

        // Mark operations with conflicts
        foreach (var conflict in conflicts)
        {
            foreach (var opId in conflict.SourceOperations)
            {
                var operationIndex = operations.FindIndex(o => o.OperationId == opId);
                if (operationIndex >= 0)
                {
                    var operation = operations[operationIndex];
                    operations[operationIndex] = new MigrationOperation
                    {
                        OperationId = operation.OperationId,
                        SourcePath = operation.SourcePath,
                        TargetPath = operation.TargetPath,
                        OperationType = operation.OperationType,
                        SizeBytes = operation.SizeBytes,
                        HasConflict = true,
                        ConflictDescription = conflict.Description,
                        AppliedRule = operation.AppliedRule
                    };
                }
            }
        }

        stopwatch.Stop();

        // Calculate statistics
        var totalSize = operations.Sum(op => op.SizeBytes);
        var conflictCount = operations.Count(op => op.HasConflict);

        // Validate plan
        var validationErrors = ValidatePlan(operations);

        return new MigrationPlan
        {
            PlanId = Guid.NewGuid().ToString("N"),
            ScanId = scanId,
            Operations = operations,
            TotalSizeBytes = totalSize,
            ConflictCount = conflictCount,
            IsValid = validationErrors.Count == 0,
            ValidationErrors = validationErrors
        };
    }

    /// <summary>
    /// Optimizes a migration plan by resolving conflicts
    /// </summary>
    /// <param name="plan">Original plan</param>
    /// <returns>Optimized plan</returns>
    public MigrationPlan OptimizePlan(MigrationPlan plan)
    {
        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        if (!plan.HasConflicts())
        {
            return plan; // Already optimized
        }

        var operations = new List<MigrationOperation>(plan.Operations);
        var usedPaths = new HashSet<string>(
            operations.Where(o => !o.HasConflict).Select(o => o.TargetPath),
            StringComparer.OrdinalIgnoreCase);

        // Resolve conflicts by generating unique paths
        for (int i = 0; i < operations.Count; i++)
        {
            if (operations[i].HasConflict)
            {
                var operation = operations[i];
                var uniquePath = _pathGenerator.GenerateUniquePath(
                    operation.TargetPath,
                    usedPaths);

                operations[i] = new MigrationOperation
                {
                    OperationId = operation.OperationId,
                    SourcePath = operation.SourcePath,
                    TargetPath = uniquePath,
                    OperationType = operation.OperationType,
                    SizeBytes = operation.SizeBytes,
                    HasConflict = false,
                    ConflictDescription = null,
                    AppliedRule = operation.AppliedRule
                };

                usedPaths.Add(uniquePath);
            }
        }

        var validationErrors = ValidatePlan(operations);

        return new MigrationPlan
        {
            PlanId = Guid.NewGuid().ToString("N"),
            ScanId = plan.ScanId,
            Operations = operations,
            TotalSizeBytes = operations.Sum(op => op.SizeBytes),
            ConflictCount = 0,
            IsValid = validationErrors.Count == 0,
            ValidationErrors = validationErrors
        };
    }

    private MigrationOperation CreateOperation(Item item, HashSet<string> usedPaths)
    {
        var operationId = Guid.NewGuid().ToString("N");
        var targetPath = _pathGenerator.GenerateTargetPath(item);

        // Check for conflicts
        var conflict = _conflictDetector.CheckPathConflict(targetPath, usedPaths);

        return new MigrationOperation
        {
            OperationId = operationId,
            SourcePath = item.Path ?? string.Empty,
            TargetPath = targetPath,
            OperationType = _config.DefaultOperationType,
            SizeBytes = item.Size,
            HasConflict = conflict != null,
            ConflictDescription = conflict?.Description
        };
    }

    private static List<string> ValidatePlan(List<MigrationOperation> operations)
    {
        var errors = new List<string>();

        if (operations.Count == 0)
        {
            errors.Add("Plan contains no operations");
        }

        var conflictedOps = operations.Count(op => op.HasConflict);
        if (conflictedOps > 0)
        {
            errors.Add($"Plan contains {conflictedOps} operations with conflicts");
        }

        // Check for duplicate operation IDs
        var duplicateIds = operations
            .GroupBy(op => op.OperationId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var id in duplicateIds)
        {
            errors.Add($"Duplicate operation ID: {id}");
        }

        return errors;
    }
}

/// <summary>
/// Extension methods for MigrationPlan
/// </summary>
public static class MigrationPlanExtensions
{
    /// <summary>
    /// Checks if the plan has any conflicts
    /// </summary>
    /// <param name="plan">Migration plan</param>
    /// <returns>True if conflicts exist</returns>
    public static bool HasConflicts(this MigrationPlan plan)
    {
        return plan.ConflictCount > 0;
    }

    /// <summary>
    /// Gets operations with conflicts
    /// </summary>
    /// <param name="plan">Migration plan</param>
    /// <returns>Conflicted operations</returns>
    public static IEnumerable<MigrationOperation> GetConflictedOperations(this MigrationPlan plan)
    {
        return plan.Operations.Where(op => op.HasConflict);
    }

    /// <summary>
    /// Gets operations without conflicts
    /// </summary>
    /// <param name="plan">Migration plan</param>
    /// <returns>Non-conflicted operations</returns>
    public static IEnumerable<MigrationOperation> GetValidOperations(this MigrationPlan plan)
    {
        return plan.Operations.Where(op => !op.HasConflict);
    }
}
