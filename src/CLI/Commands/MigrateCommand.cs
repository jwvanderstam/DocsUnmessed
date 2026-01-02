namespace DocsUnmessed.CLI.Commands;

using DocsUnmessed.Core.Configuration;
using DocsUnmessed.Core.Interfaces;
using System.Text.Json;

/// <summary>
/// Command to migrate files based on scan results and categorization
/// </summary>
public sealed class MigrateCommand
{
    private readonly IInventoryService _inventoryService;

    public MigrateCommand(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<int> ExecuteAsync(MigrateOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine("DocsUnmessed - File Migration");
        Console.WriteLine("==============================\n");

        try
        {
            // Validate scan ID
            var scanResult = await _inventoryService.GetScanResultAsync(options.ScanId, cancellationToken);
            if (scanResult == null)
            {
                Console.WriteLine($"[ERROR] Scan ID '{options.ScanId}' not found");
                return 1;
            }

            // Set source root from first item's path if available and not already set
            string? sourceRoot = options.SourceRoot;
            if (string.IsNullOrEmpty(sourceRoot) && scanResult.Items.Any())
            {
                // Try to determine common root from items
                var firstItem = scanResult.Items.First();
                var commonRoot = Path.GetDirectoryName(firstItem.Path) ?? "";
                
                // Find common parent directory for all items
                foreach (var item in scanResult.Items.Take(100)) // Sample to avoid performance hit
                {
                    while (!string.IsNullOrEmpty(commonRoot) && 
                           !item.Path.StartsWith(commonRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        commonRoot = Path.GetDirectoryName(commonRoot) ?? "";
                    }
                }
                
                sourceRoot = commonRoot;
            }
            
            // Create new options with source root
            var migrateOptions = new MigrateOptions
            {
                ScanId = options.ScanId,
                TargetDirectory = options.TargetDirectory,
                EnableCategorization = options.EnableCategorization,
                PreserveStructure = options.PreserveStructure,
                DryRun = options.DryRun,
                Force = options.Force,
                Verbose = options.Verbose,
                OutputPath = options.OutputPath,
                MaxPreviewItems = options.MaxPreviewItems,
                ConflictResolution = options.ConflictResolution,
                UseDefaultExclusions = options.UseDefaultExclusions,
                ExcludeDirectories = options.ExcludeDirectories,
                ExcludePatterns = options.ExcludePatterns,
                SourceRoot = sourceRoot
            };
            
            options = migrateOptions;

            // Display scan info
            Console.WriteLine($">> Scan Information:");
            Console.WriteLine($"  Scan ID: {options.ScanId}");
            Console.WriteLine($"  Total Files: {scanResult.Statistics.TotalFiles:N0}");
            Console.WriteLine($"  Total Size: {FormatBytes(scanResult.Statistics.TotalSize)}");
            Console.WriteLine($"  Scan Date: {scanResult.CompletedUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? scanResult.StartedUtc.ToString("yyyy-MM-dd HH:mm:ss")}\n");

            // Create exclude configuration
            var excludeConfig = new ExcludeConfig
            {
                UseDefaultExclusions = options.UseDefaultExclusions,
                ExcludedDirectories = options.ExcludeDirectories ?? Array.Empty<string>(),
                ExcludedFilePatterns = options.ExcludePatterns ?? Array.Empty<string>(),
                EnableCategoryMigration = options.EnableCategorization,
                MigratedDirectory = options.TargetDirectory,
                IsDryRun = options.DryRun
            };

            // Get all items from scan
            var items = scanResult.Items.ToList();
            
            // Filter out excluded items
            var itemsToMigrate = FilterItems(items, excludeConfig);
            
            Console.WriteLine($">> Files to migrate: {itemsToMigrate.Count:N0} (after exclusions)\n");

            if (itemsToMigrate.Count == 0)
            {
                Console.WriteLine("[INFO] No files to migrate after applying filters");
                return 0;
            }

            // Categorize files if enabled
            Dictionary<FileCategory, List<Core.Domain.Item>> categorizedFiles = new();
            if (options.EnableCategorization)
            {
                Console.WriteLine(">> Categorizing files...");
                categorizedFiles = CategorizeFiles(itemsToMigrate);
                
                Console.WriteLine("\n>> File Categories:");
                foreach (var category in categorizedFiles.OrderByDescending(c => c.Value.Count))
                {
                    var size = category.Value.Sum(f => f.Size);
                    Console.WriteLine($"  {category.Key,-12}: {category.Value.Count,6:N0} files ({FormatBytes(size),10})");
                }
                Console.WriteLine();
            }

            // Dry run mode - show what would happen
            if (options.DryRun)
            {
                Console.WriteLine(">> DRY RUN MODE - No files will be moved\n");
                await ShowDryRunPreview(itemsToMigrate, excludeConfig, options);
                
                Console.WriteLine($"\n[SUCCESS] Dry run complete. {itemsToMigrate.Count:N0} files would be migrated.");
                Console.WriteLine("   Run without --dry-run to execute the migration.\n");
                return 0;
            }

            // Confirm migration
            if (!options.Force && !await ConfirmMigration(itemsToMigrate.Count))
            {
                Console.WriteLine("\n[CANCELLED] Migration cancelled by user");
                return 1;
            }

            // Execute migration
            Console.WriteLine("\n>> Starting migration...\n");
            var results = await ExecuteMigration(itemsToMigrate, excludeConfig, options, cancellationToken);

            // Display results
            DisplayResults(results);

            // Save migration report
            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                await SaveMigrationReport(results, options.OutputPath, cancellationToken);
                Console.WriteLine($"\n>> Migration report saved to: {options.OutputPath}");
            }

            Console.WriteLine("\n[SUCCESS] Migration complete!");
            return results.FailedCount > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
            if (options.Verbose)
            {
                Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            }
            return 1;
        }
    }

    private static List<Core.Domain.Item> FilterItems(
        List<Core.Domain.Item> items, 
        ExcludeConfig excludeConfig)
    {
        var excludedDirs = excludeConfig.GetAllExcludedDirectories();
        
        return items.Where(item =>
        {
            // Only migrate files
            if (item.Type != Core.Domain.ItemType.File)
                return false;

            // Check if file is in excluded directory
            foreach (var excludedDir in excludedDirs)
            {
                if (item.Path.Contains(excludedDir, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }).ToList();
    }

    private static Dictionary<FileCategory, List<Core.Domain.Item>> CategorizeFiles(
        List<Core.Domain.Item> items)
    {
        var categorized = new Dictionary<FileCategory, List<Core.Domain.Item>>();

        foreach (var item in items)
        {
            var category = FileCategoryDetector.DetectCategory(item.Name);
            
            if (!categorized.ContainsKey(category))
            {
                categorized[category] = new List<Core.Domain.Item>();
            }
            
            categorized[category].Add(item);
        }

        return categorized;
    }

    private static async Task ShowDryRunPreview(
        List<Core.Domain.Item> items,
        ExcludeConfig excludeConfig,
        MigrateOptions options)
    {
        Console.WriteLine($">> Preview (showing up to {options.MaxPreviewItems} files):\n");

        var count = 0;
        foreach (var item in items.Take(options.MaxPreviewItems))
        {
            var targetPath = GetTargetPath(item, excludeConfig, options);
            Console.WriteLine($"[DRY-RUN] {item.Path}");
            Console.WriteLine($"       -> {targetPath}\n");
            count++;
        }

        if (items.Count > options.MaxPreviewItems)
        {
            Console.WriteLine($"... and {items.Count - options.MaxPreviewItems:N0} more files");
        }

        await Task.CompletedTask;
    }

    private static string GetTargetPath(Core.Domain.Item item, ExcludeConfig excludeConfig, MigrateOptions options)
    {
        var fileName = Path.GetFileName(item.Path);
        
        // If no categorization, just use target directory
        if (!excludeConfig.EnableCategoryMigration)
        {
            if (options.PreserveStructure && !string.IsNullOrEmpty(options.SourceRoot))
            {
                // Preserve relative path from source root
                var relativePath = GetRelativePath(options.SourceRoot, item.Path);
                return Path.Combine(excludeConfig.MigratedDirectory, relativePath);
            }
            
            return Path.Combine(excludeConfig.MigratedDirectory, fileName);
        }

        // With categorization
        var category = FileCategoryDetector.DetectCategory(fileName);
        var categoryDir = FileCategoryDetector.GetCategoryDirectory(category);
        
        if (options.PreserveStructure && !string.IsNullOrEmpty(options.SourceRoot))
        {
            // Get relative path and extract subdirectory structure
            var relativePath = GetRelativePath(options.SourceRoot, item.Path);
            var subDir = Path.GetDirectoryName(relativePath) ?? "";
            
            // If there's a meaningful subdirectory, preserve it
            if (!string.IsNullOrEmpty(subDir))
            {
                return Path.Combine(excludeConfig.MigratedDirectory, categoryDir, subDir, fileName);
            }
        }
        
        // Default: category + filename
        return Path.Combine(excludeConfig.MigratedDirectory, categoryDir, fileName);
    }
    
    private static string GetRelativePath(string fromPath, string toPath)
    {
        try
        {
            var from = new Uri(Path.GetFullPath(fromPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var to = new Uri(Path.GetFullPath(toPath));
            var relativeUri = from.MakeRelativeUri(to);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            
            // Convert forward slashes to platform-specific separator
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
        catch
        {
            // Fallback: just return filename
            return Path.GetFileName(toPath);
        }
    }

    private static async Task<bool> ConfirmMigration(int fileCount)
    {
        Console.WriteLine($"\n[WARNING] About to migrate {fileCount:N0} files.");
        Console.Write("Do you want to continue? (y/N): ");
        
        var response = Console.ReadLine()?.Trim().ToLower();
        return response == "y" || response == "yes";
    }

    private static async Task<MigrationResults> ExecuteMigration(
        List<Core.Domain.Item> items,
        ExcludeConfig excludeConfig,
        MigrateOptions options,
        CancellationToken cancellationToken)
    {
        var results = new MigrationResults
        {
            StartTime = DateTime.Now
        };

        var processed = 0;
        var lastUpdate = DateTime.Now;

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var targetPath = GetTargetPath(item, excludeConfig, options);
                var targetDir = Path.GetDirectoryName(targetPath);

                // Create target directory if needed
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // Handle file name conflicts
                targetPath = HandleConflict(targetPath, options.ConflictResolution);

                // Copy file
                File.Copy(item.Path, targetPath, overwrite: false);
                
                results.SuccessCount++;
                results.TotalBytesMoved += item.Size;
            }
            catch (Exception ex)
            {
                results.FailedCount++;
                results.Errors.Add($"{item.Path}: {ex.Message}");
            }

            processed++;

            // Update progress
            var now = DateTime.Now;
            if (processed % 10 == 0 || (now - lastUpdate).TotalMilliseconds > 500)
            {
                var percent = (processed * 100.0) / items.Count;
                var rate = processed / (now - results.StartTime).TotalSeconds;
                Console.Write($"\r>> Progress: {processed:N0}/{items.Count:N0} ({percent:F1}%) | {rate:F0} files/sec | {results.SuccessCount:N0} succeeded, {results.FailedCount:N0} failed");
                lastUpdate = now;
            }
        }

        Console.WriteLine(); // New line after progress
        results.EndTime = DateTime.Now;
        
        return results;
    }

    private static string HandleConflict(string targetPath, string conflictResolution)
    {
        if (!File.Exists(targetPath))
        {
            return targetPath;
        }

        return conflictResolution.ToLower() switch
        {
            "skip" => targetPath, // Will throw exception and be caught
            "overwrite" => targetPath, // Caller will use overwrite: true
            "rename" or _ => GenerateUniqueFileName(targetPath)
        };
    }

    private static string GenerateUniqueFileName(string path)
    {
        var directory = Path.GetDirectoryName(path);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        var counter = 1;
        string newPath;
        
        do
        {
            var newFileName = $"{fileNameWithoutExt} ({counter}){extension}";
            newPath = Path.Combine(directory ?? "", newFileName);
            counter++;
        }
        while (File.Exists(newPath));

        return newPath;
    }

    private static void DisplayResults(MigrationResults results)
    {
        Console.WriteLine("\n>> Migration Results:");
        Console.WriteLine($"  Total files: {results.TotalCount:N0}");
        Console.WriteLine($"  Succeeded: {results.SuccessCount:N0}");
        Console.WriteLine($"  Failed: {results.FailedCount:N0}");
        Console.WriteLine($"  Total size moved: {FormatBytes(results.TotalBytesMoved)}");
        Console.WriteLine($"  Duration: {results.Duration.TotalSeconds:F1} seconds");
        Console.WriteLine($"  Average speed: {results.AverageSpeed:F0} files/sec");

        if (results.Errors.Count > 0)
        {
            Console.WriteLine($"\n[WARNING] Errors ({results.Errors.Count}):");
            foreach (var error in results.Errors.Take(10))
            {
                Console.WriteLine($"  - {error}");
            }
            if (results.Errors.Count > 10)
            {
                Console.WriteLine($"  ... and {results.Errors.Count - 10} more errors");
            }
        }
    }

    private static async Task SaveMigrationReport(
        MigrationResults results,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var report = new
        {
            results.StartTime,
            results.EndTime,
            DurationSeconds = results.Duration.TotalSeconds,
            results.TotalCount,
            results.SuccessCount,
            results.FailedCount,
            results.TotalBytesMoved,
            results.AverageSpeed,
            results.Errors
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(outputPath, json, cancellationToken);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public sealed class MigrateOptions
{
    public required string ScanId { get; init; }
    public string TargetDirectory { get; init; } = "migrated";
    public bool EnableCategorization { get; init; } = true;
    public bool PreserveStructure { get; init; } = true; // NEW: Preserve subdirectory structure
    public bool DryRun { get; init; } = false;
    public bool Force { get; init; } = false;
    public bool Verbose { get; init; } = false;
    public string? OutputPath { get; init; }
    public int MaxPreviewItems { get; init; } = 20;
    public string ConflictResolution { get; init; } = "rename"; // rename, skip, overwrite
    
    public bool UseDefaultExclusions { get; init; } = true;
    public string[]? ExcludeDirectories { get; init; }
    public string[]? ExcludePatterns { get; init; }
    public string? SourceRoot { get; init; } // NEW: Store scan root for relative path calculation
}

public sealed class MigrationResults
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int TotalCount => SuccessCount + FailedCount;
    
    public long TotalBytesMoved { get; set; }
    public double AverageSpeed => Duration.TotalSeconds > 0 
        ? TotalCount / Duration.TotalSeconds 
        : 0;
    
    public List<string> Errors { get; set; } = new();
}
