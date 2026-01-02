namespace DocsUnmessed.Services.Migration;

/// <summary>
/// Detects conflicts in migration operations
/// </summary>
public sealed class ConflictDetector
{
    private readonly MigrationPlanningConfig _config;

    /// <summary>
    /// Initializes a new instance of the ConflictDetector class
    /// </summary>
    /// <param name="config">Migration planning configuration</param>
    public ConflictDetector(MigrationPlanningConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Detects conflicts in a set of operations
    /// </summary>
    /// <param name="operations">Migration operations</param>
    /// <returns>Detected conflicts</returns>
    public List<PathConflict> DetectConflicts(IEnumerable<MigrationOperation> operations)
    {
        if (operations == null)
        {
            throw new ArgumentNullException(nameof(operations));
        }

        var conflicts = new List<PathConflict>();
        var operationList = operations.ToList();

        // Detect duplicate target paths
        conflicts.AddRange(DetectDuplicateTargets(operationList));

        // Detect path length issues
        conflicts.AddRange(DetectPathLengthIssues(operationList));

        // Detect invalid paths
        conflicts.AddRange(DetectInvalidPaths(operationList));

        return conflicts;
    }

    /// <summary>
    /// Checks if a specific path has conflicts
    /// </summary>
    /// <param name="targetPath">Target path to check</param>
    /// <param name="existingPaths">Set of existing paths</param>
    /// <returns>Conflict if found, null otherwise</returns>
    public PathConflict? CheckPathConflict(string targetPath, HashSet<string> existingPaths)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return new PathConflict
            {
                Path = targetPath,
                Type = ConflictType.InvalidPath,
                Description = "Path is null or empty"
            };
        }

        // Check path length
        if (targetPath.Length > _config.MaxPathLength)
        {
            return new PathConflict
            {
                Path = targetPath,
                Type = ConflictType.PathTooLong,
                Description = $"Path length ({targetPath.Length}) exceeds maximum ({_config.MaxPathLength})"
            };
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (targetPath.Any(c => invalidChars.Contains(c)))
        {
            return new PathConflict
            {
                Path = targetPath,
                Type = ConflictType.InvalidPath,
                Description = "Path contains invalid characters"
            };
        }

        // Check if path already exists
        if (existingPaths.Contains(targetPath))
        {
            return new PathConflict
            {
                Path = targetPath,
                Type = ConflictType.PathExists,
                Description = "Target path already exists"
            };
        }

        return null;
    }

    private List<PathConflict> DetectDuplicateTargets(List<MigrationOperation> operations)
    {
        var conflicts = new List<PathConflict>();
        
        var targetGroups = operations
            .GroupBy(op => op.TargetPath, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in targetGroups)
        {
            conflicts.Add(new PathConflict
            {
                Path = group.Key,
                Type = ConflictType.DuplicateTarget,
                Description = $"Multiple operations ({group.Count()}) target the same path",
                SourceOperations = group.Select(op => op.OperationId).ToList()
            });
        }

        return conflicts;
    }

    private List<PathConflict> DetectPathLengthIssues(List<MigrationOperation> operations)
    {
        var conflicts = new List<PathConflict>();

        foreach (var operation in operations)
        {
            if (operation.TargetPath.Length > _config.MaxPathLength)
            {
                conflicts.Add(new PathConflict
                {
                    Path = operation.TargetPath,
                    Type = ConflictType.PathTooLong,
                    Description = $"Path length ({operation.TargetPath.Length}) exceeds maximum ({_config.MaxPathLength})",
                    SourceOperations = new[] { operation.OperationId }
                });
            }
        }

        return conflicts;
    }

    private static List<PathConflict> DetectInvalidPaths(List<MigrationOperation> operations)
    {
        var conflicts = new List<PathConflict>();
        var invalidChars = Path.GetInvalidPathChars();

        foreach (var operation in operations)
        {
            if (string.IsNullOrWhiteSpace(operation.TargetPath))
            {
                conflicts.Add(new PathConflict
                {
                    Path = operation.TargetPath,
                    Type = ConflictType.InvalidPath,
                    Description = "Path is null or empty",
                    SourceOperations = new[] { operation.OperationId }
                });
                continue;
            }

            if (operation.TargetPath.Any(c => invalidChars.Contains(c)))
            {
                conflicts.Add(new PathConflict
                {
                    Path = operation.TargetPath,
                    Type = ConflictType.InvalidPath,
                    Description = "Path contains invalid characters",
                    SourceOperations = new[] { operation.OperationId }
                });
            }
        }

        return conflicts;
    }
}
