namespace DocsUnmessed.CLI.Commands;

using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Core.Configuration;
using System.Text.Json;

/// <summary>
/// Command to assess current storage setup
/// </summary>
public sealed class AssessCommand
{
    private readonly IConnector[] _connectors;
    private readonly IInventoryService _inventoryService;

    public AssessCommand(IConnector[] connectors, IInventoryService inventoryService)
    {
        _connectors = connectors;
        _inventoryService = inventoryService;
    }

    public async Task<int> ExecuteAsync(AssessOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine("DocsUnmessed - File Assessment");
        Console.WriteLine("==============================\n");

        try
        {
            // Display scan configuration
            Console.WriteLine($">> Scanning root: {options.Root}");
            Console.WriteLine($">> Providers: {string.Join(", ", options.Providers)}\n");

            // Create scan
            var scanId = await _inventoryService.CreateScanAsync(options.Providers, cancellationToken);
            Console.WriteLine($"Scan ID: {scanId}\n");

            // Enumerate each provider
            foreach (var providerName in options.Providers)
            {
                var connector = Array.Find(_connectors, c => c.Id == providerName);
                if (connector == null)
                {
                    Console.WriteLine($"WARNING: Connector '{providerName}' not found, skipping...");
                    continue;
                }

                Console.WriteLine($">> Scanning provider: {providerName}");
                
                // Create exclusion configuration
                var excludeConfig = CreateExcludeConfig(options);
                
                var filters = new EnumerationFilters
                {
                    ComputeHash = options.ComputeHash, // Use option value
                    IncludeFolders = true,
                    ExcludeConfig = excludeConfig,
                    ExcludedDirectories = options.ExcludeDirectories,
                    ExcludedFilePatterns = options.ExcludePatterns
                };
                
                // Display scan configuration
                // Display exclusion info
                if (excludeConfig?.UseDefaultExclusions == true)
                {
                    Console.WriteLine(">> Using default system/user directory exclusions");
                }
                if (options.ExcludeDirectories?.Length > 0)
                {
                    Console.WriteLine($">> Excluding custom directories: {string.Join(", ", options.ExcludeDirectories)}");
                }
                if (!options.ComputeHash)
                {
                    Console.WriteLine(">> Skipping hash computation for faster scan (use --compute-hash to enable)");
                }
                
                var items = new List<Core.Domain.Item>();
                var count = 0;
                var lastUpdate = DateTime.Now;
                var startTime = DateTime.Now;
                var filesProcessed = 0;
                var foldersProcessed = 0;
                
                await foreach (var item in connector.EnumerateAsync(options.Root ?? ".", filters, cancellationToken))
                {
                    items.Add(item);
                    count++;
                    
                    if (item.Type == Core.Domain.ItemType.File)
                        filesProcessed++;
                    else
                        foldersProcessed++;
                    
                    // Update progress every 25 items or every 300ms
                    var now = DateTime.Now;
                    if (count % 25 == 0 || (now - lastUpdate).TotalMilliseconds > 300)
                    {
                        var elapsed = now - startTime;
                        var rate = count / elapsed.TotalSeconds;
                        var progress = $">> Scanning: {count:N0} items ({filesProcessed:N0} files, {foldersProcessed:N0} folders) | {rate:F0} items/sec | {elapsed.TotalSeconds:F0}s elapsed";
                        Console.Write($"\r{progress}".PadRight(120));
                        lastUpdate = now;
                    }
                }
                
                var totalElapsed = DateTime.Now - startTime;
                Console.WriteLine($"\r[SUCCESS] Scan complete: {count:N0} items found ({filesProcessed:N0} files, {foldersProcessed:N0} folders) in {totalElapsed.TotalSeconds:F1}s".PadRight(120));
                
                // Show batch processing progress with size breakdown
                if (count > 0)
                {
                    var totalSize = items.Sum(i => i.Size);
                    Console.WriteLine($">> Saving {count:N0} items ({FormatBytes(totalSize)}) to database...");
                    
                    var saveStart = DateTime.Now;
                    await _inventoryService.AddItemsAsync(scanId, items, cancellationToken);
                    var saveTime = (DateTime.Now - saveStart).TotalSeconds;
                    
                    Console.WriteLine($"[SUCCESS] Items saved successfully in {saveTime:F1}s");
                }
            }

            // Complete scan and get results
            Console.WriteLine("\n[SUCCESS] Finalizing scan and computing statistics...");
            await _inventoryService.CompleteScanAsync(scanId, cancellationToken);
            var result = await _inventoryService.GetScanResultAsync(scanId, cancellationToken);

            // Display summary
            Console.WriteLine("\n>> Scan Summary:");
            Console.WriteLine($"  Total Files: {result.Statistics.TotalFiles:N0}");
            Console.WriteLine($"  Total Folders: {result.Statistics.TotalFolders:N0}");
            Console.WriteLine($"  Total Size: {FormatBytes(result.Statistics.TotalSize)}");
            Console.WriteLine($"  Max Depth: {result.Statistics.MaxDepth}");
            Console.WriteLine($"  Files with Issues: {result.Statistics.FilesWithIssues:N0}");

            // Export results if requested
            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(options.OutputPath, json, cancellationToken);
                Console.WriteLine($"\n>> Results exported to: {options.OutputPath}");
            }

            Console.WriteLine("\n[SUCCESS] Assessment complete!");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
            return 1;
        }
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
    
    private static ExcludeConfig? CreateExcludeConfig(AssessOptions options)
    {
        if (!options.UseDefaultExclusions && 
            options.ExcludeDirectories == null && 
            options.ExcludePatterns == null)
        {
            return null;
        }
        
        var excludedDirs = new List<string>();
        
        // Start with defaults if requested
        if (options.UseDefaultExclusions)
        {
            excludedDirs.AddRange(ExcludeConfig.DefaultSystemDirectories);
            excludedDirs.AddRange(ExcludeConfig.DefaultUserDirectories);
        }
        
        // Remove any directories user wants to include
        if (options.IncludeDirectories != null)
        {
            foreach (var includeDir in options.IncludeDirectories)
            {
                excludedDirs.RemoveAll(d => 
                    d.Equals(includeDir, StringComparison.OrdinalIgnoreCase));
            }
        }
        
        return new ExcludeConfig
        {
            UseDefaultExclusions = false, // We've already processed defaults above
            ExcludedDirectories = excludedDirs.ToArray(),
            ExcludedFilePatterns = options.ExcludePatterns ?? Array.Empty<string>(),
            EnableCategoryMigration = false, // Not used during assess
            IsDryRun = false
        };
    }
}

public sealed class AssessOptions
{
    public required string[] Providers { get; init; }
    public string? Root { get; init; }
    public string? OutputPath { get; init; }
    
    /// <summary>
    /// Gets or sets whether to compute file hashes (slower but enables duplicate detection)
    /// </summary>
    public bool ComputeHash { get; init; } = false; // Default to false for faster scans
    
    /// <summary>
    /// Gets or sets whether to use default exclusions
    /// </summary>
    public bool UseDefaultExclusions { get; init; } = true;
    
    /// <summary>
    /// Gets or sets additional directories to exclude
    /// </summary>
    public string[]? ExcludeDirectories { get; init; }
    
    /// <summary>
    /// Gets or sets directories to include (overrides defaults)
    /// </summary>
    public string[]? IncludeDirectories { get; init; }
    
    /// <summary>
    /// Gets or sets file patterns to exclude
    /// </summary>
    public string[]? ExcludePatterns { get; init; }
}
