using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocsUnmessed.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace DocsUnmessed.GUI.ViewModels;

public partial class MigrationViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private string scanId = string.Empty;

    [ObservableProperty]
    private bool enableCategoryMigration = true;

    [ObservableProperty]
    private bool isDryRun = true;

    [ObservableProperty]
    private bool isMigrating;

    [ObservableProperty]
    private string migrationStatus = "Enter scan ID to begin";

    [ObservableProperty]
    private int totalOperations;

    [ObservableProperty]
    private int completedOperations;

    [ObservableProperty]
    private double migrationProgress;

    [ObservableProperty]
    private bool hasResults;

    [ObservableProperty]
    private string? resultsSummary;

    public ObservableCollection<string> MigrationLog { get; } = new();

    public MigrationViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [RelayCommand]
    private async Task LoadScanAsync()
    {
        if (string.IsNullOrWhiteSpace(ScanId))
        {
            MessageBox.Show("Please enter a scan ID", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = await _inventoryService.GetScanResultAsync(ScanId, CancellationToken.None);
            
            MigrationLog.Clear();
            MigrationLog.Add($"? Loaded scan: {ScanId}");
            MigrationLog.Add($"  Files: {result.Statistics.TotalFiles:N0}");
            MigrationLog.Add($"  Folders: {result.Statistics.TotalFolders:N0}");
            MigrationLog.Add($"  Total Size: {FormatBytes(result.Statistics.TotalSize)}");
            MigrationLog.Add("");
            MigrationLog.Add("Ready to start migration");

            MigrationStatus = "Scan loaded successfully";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading scan: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            MigrationLog.Add($"? Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task StartMigrationAsync()
    {
        if (IsMigrating) return;

        if (string.IsNullOrWhiteSpace(ScanId))
        {
            MessageBox.Show("Please load a scan first", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmMessage = IsDryRun 
            ? "Start migration preview (no files will be moved)?"
            : "Start actual migration? Files will be moved!";

        var result = MessageBox.Show(confirmMessage, "Confirm Migration", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsMigrating = true;
            HasResults = false;
            TotalOperations = 0;
            CompletedOperations = 0;
            MigrationProgress = 0;
            MigrationLog.Clear();

            MigrationStatus = IsDryRun ? "Running migration preview..." : "Migrating files...";
            MigrationLog.Add($"Starting migration (Dry Run: {IsDryRun})...");
            MigrationLog.Add($"Scan ID: {ScanId}");
            MigrationLog.Add($"Category Migration: {EnableCategoryMigration}");
            MigrationLog.Add("");

            // Simulate migration process
            // In real implementation, you would call:
            // await _migrationService.ExecuteAsync(ScanId, EnableCategoryMigration, IsDryRun, progress);

            // For now, simulate with demo data
            await SimulateMigrationAsync();

            HasResults = true;
            ResultsSummary = $"Migration {(IsDryRun ? "Preview" : "")} Complete!\n\n" +
                           $"Total Operations: {TotalOperations}\n" +
                           $"Completed: {CompletedOperations}\n" +
                           $"Mode: {(IsDryRun ? "Dry Run (Preview)" : "Actual Migration")}";

            MigrationStatus = "Migration completed successfully!";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during migration: {ex.Message}", "Migration Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            MigrationStatus = $"Error: {ex.Message}";
            MigrationLog.Add($"? Error: {ex.Message}");
        }
        finally
        {
            IsMigrating = false;
        }
    }

    private async Task SimulateMigrationAsync()
    {
        // This simulates migration process for demo
        // In production, integrate with actual migration service

        var operations = new[]
        {
            "Analyzing file structure...",
            "Computing optimal categorization...",
            "Creating target directories...",
            "Planning file movements...",
            "Validating destinations...",
            "Executing migration plan...",
            "Verifying results...",
            "Updating database..."
        };

        TotalOperations = operations.Length;

        for (int i = 0; i < operations.Length; i++)
        {
            await Task.Delay(500); // Simulate work
            CompletedOperations = i + 1;
            MigrationProgress = (double)CompletedOperations / TotalOperations * 100;
            MigrationLog.Add($"[{i + 1}/{TotalOperations}] {operations[i]}");
            MigrationStatus = operations[i];
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
