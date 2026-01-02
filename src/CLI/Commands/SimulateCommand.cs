namespace DocsUnmessed.CLI.Commands;

using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Services;
using System.Text.Json;

/// <summary>
/// Command to simulate migration in what-if mode with rules engine evaluation
/// </summary>
public sealed class SimulateCommand
{
    private readonly IMigrationOrchestrator _orchestrator;
    private readonly IInventoryService _inventoryService;

    public SimulateCommand(IMigrationOrchestrator orchestrator, IInventoryService inventoryService)
    {
        _orchestrator = orchestrator;
        _inventoryService = inventoryService;
    }

    public async Task<int> ExecuteAsync(SimulateOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine("DocsUnmessed - Migration Simulation");
        Console.WriteLine("====================================\n");

        try
        {
            // Load the scan
            Console.WriteLine($"Loading scan: {options.ScanId}");
            var scan = await _inventoryService.GetScanResultAsync(options.ScanId, cancellationToken);
            Console.WriteLine($"Found {scan.Items.Count:N0} items to analyze\n");

            // Load and evaluate rules if rules path provided
            if (!string.IsNullOrEmpty(options.RulesPath))
            {
                Console.WriteLine($"Loading rules from: {options.RulesPath}");
                var rulesEngine = new RulesEngine();
                await rulesEngine.LoadRulesAsync(options.RulesPath, cancellationToken);
                Console.WriteLine("Rules loaded successfully\n");

                Console.WriteLine("Evaluating rules against scanned items...");
                var suggestions = new List<(Core.Domain.Item item, Core.Domain.TargetSuggestion suggestion)>();
                var itemsWithoutSuggestions = 0;

                foreach (var item in scan.Items)
                {
                    if (item.Type == Core.Domain.ItemType.File) // Only evaluate files
                    {
                        var suggestion = await rulesEngine.EvaluateAsync(item, cancellationToken);
                        if (suggestion != null)
                        {
                            suggestions.Add((item, suggestion));
                        }
                        else
                        {
                            itemsWithoutSuggestions++;
                        }
                    }
                }

                Console.WriteLine($"\nRules Evaluation Results:");
                Console.WriteLine($"  Files with suggestions: {suggestions.Count:N0}");
                Console.WriteLine($"  Files without suggestions: {itemsWithoutSuggestions:N0}");
                Console.WriteLine($"  Coverage: {(suggestions.Count * 100.0 / scan.Items.Count(i => i.Type == Core.Domain.ItemType.File)):F1}%\n");

                // Group by target location
                var byTarget = suggestions.GroupBy(s => s.suggestion.TargetPath);
                Console.WriteLine("Suggestions by Target Location:");
                foreach (var group in byTarget.OrderByDescending(g => g.Count()))
                {
                    Console.WriteLine($"  {group.Key}: {group.Count():N0} files");
                }

                // Group by rule
                var byRule = suggestions.GroupBy(s => s.suggestion.RuleName);
                Console.WriteLine("\nSuggestions by Rule:");
                foreach (var group in byRule.OrderByDescending(g => g.Count()))
                {
                    Console.WriteLine($"  {group.Key}: {group.Count():N0} files");
                }

                // Show sample suggestions
                Console.WriteLine("\nSample Suggestions (first 10):");
                foreach (var (item, suggestion) in suggestions.Take(10))
                {
                    Console.WriteLine($"\n  File: {item.Name}");
                    Console.WriteLine($"  Current: {item.Path}");
                    Console.WriteLine($"  Target: {suggestion.TargetPath}{suggestion.TargetName}");
                    Console.WriteLine($"  Rule: {suggestion.RuleName}");
                    Console.WriteLine($"  Confidence: {suggestion.Confidence:P0}");
                    if (suggestion.Reasons.Any())
                    {
                        Console.WriteLine($"  Reason: {suggestion.Reasons.First()}");
                    }
                }

                // Export suggestions if output path provided
                if (!string.IsNullOrEmpty(options.OutputPath))
                {
                    var exportData = new
                    {
                        ScanId = options.ScanId,
                        GeneratedAt = DateTime.UtcNow,
                        RulesPath = options.RulesPath,
                        Summary = new
                        {
                            TotalFiles = scan.Items.Count(i => i.Type == Core.Domain.ItemType.File),
                            FilesWithSuggestions = suggestions.Count,
                            FilesWithoutSuggestions = itemsWithoutSuggestions,
                            CoveragePercent = suggestions.Count * 100.0 / scan.Items.Count(i => i.Type == Core.Domain.ItemType.File)
                        },
                        Suggestions = suggestions.Select(s => new
                        {
                            SourcePath = s.item.Path,
                            SourceName = s.item.Name,
                            TargetPath = s.suggestion.TargetPath,
                            TargetName = s.suggestion.TargetName,
                            RuleName = s.suggestion.RuleName,
                            Confidence = s.suggestion.Confidence,
                            Reasons = s.suggestion.Reasons,
                            ConflictPolicy = s.suggestion.ConflictPolicy.ToString()
                        }).ToList()
                    };

                    var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    await File.WriteAllTextAsync(options.OutputPath, json, cancellationToken);
                    Console.WriteLine($"\n? Suggestions exported to: {options.OutputPath}");
                }
            }
            else
            {
                // Original migration plan simulation (without rules)
                var migrationOptions = new MigrationOptions
                {
                    WhatIf = true,
                    BatchSize = options.BatchSize,
                    NonDestructive = true,
                    VerifyHashes = true
                };

                Console.WriteLine("Creating migration plan (no rules specified)...");
                var plan = await _orchestrator.SimulatePlanAsync(options.ScanId, migrationOptions, cancellationToken);

                // Display plan summary
                Console.WriteLine("\nMigration Plan Summary:");
                Console.WriteLine($"  Plan ID: {plan.PlanId}");
                Console.WriteLine($"  Total Operations: {plan.Operations.Count:N0}");
                Console.WriteLine($"  Total Files: {plan.Metrics.TotalFiles:N0}");
                Console.WriteLine($"  Total Size: {FormatBytes(plan.Metrics.TotalSize)}");
                Console.WriteLine($"  Depth Reduction: {plan.Metrics.DepthReduction}");
                Console.WriteLine($"  Duplicates Eliminated: {plan.Metrics.DuplicatesEliminated:N0}");
                Console.WriteLine($"  Compliance Uplift: {plan.Metrics.ComplianceUplift}%");

                // Group operations by type
                var opsByType = plan.Operations.GroupBy(o => o.Type);
                Console.WriteLine("\nOperations by Type:");
                foreach (var group in opsByType)
                {
                    Console.WriteLine($"  {group.Key}: {group.Count():N0}");
                }

                // Export plan if requested
                if (!string.IsNullOrEmpty(options.OutputPath))
                {
                    var json = JsonSerializer.Serialize(plan, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    await File.WriteAllTextAsync(options.OutputPath, json, cancellationToken);
                    Console.WriteLine($"\n? Plan exported to: {options.OutputPath}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n? Error: {ex.Message}");
            if (options.Verbose)
            {
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            }
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
}

public sealed class SimulateOptions
{
    public required string ScanId { get; init; }
    public int BatchSize { get; init; } = 500;
    public string? OutputPath { get; init; }
    public string? RulesPath { get; init; }
    public bool Verbose { get; init; }
}
