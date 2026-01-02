using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocsUnmessed.Core.Interfaces;
using DocsUnmessed.Core.Configuration;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace DocsUnmessed.GUI.ViewModels;

public partial class AssessViewModel : ObservableObject
{
    private readonly IConnector[] _connectors;
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private string scanPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    [ObservableProperty]
    private bool includeSubdirectories = true;

    [ObservableProperty]
    private bool computeHash = false;

    [ObservableProperty]
    private bool useDefaultExclusions = true;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private string scanStatus = "Ready to scan";

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private int filesProcessed;

    [ObservableProperty]
    private int foldersProcessed;

    [ObservableProperty]
    private double scanProgress;

    [ObservableProperty]
    private string? currentScanId;

    [ObservableProperty]
    private bool hasResults;

    [ObservableProperty]
    private string? resultsSummary;

    [ObservableProperty]
    private string selectedProvider = "fs_local";

    public ObservableCollection<string> AvailableProviders { get; } = new()
    {
        "fs_local",
        "onedrive",
        "googledrive",
        "dropbox",
        "icloud"
    };

    public ObservableCollection<string> ExcludedDirectories { get; } = new();
    public ObservableCollection<string> ExcludedPatterns { get; } = new();

    public AssessViewModel(IConnector[] connectors, IInventoryService inventoryService)
    {
        _connectors = connectors;
        _inventoryService = inventoryService;
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Folder Selection",
            ValidateNames = false
        };

        // Hack to allow folder selection
        if (dialog.ShowDialog() == true)
        {
            ScanPath = Path.GetDirectoryName(dialog.FileName) ?? ScanPath;
        }
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            HasResults = false;
            ScanStatus = "Initializing scan...";
            TotalItems = 0;
            FilesProcessed = 0;
            FoldersProcessed = 0;
            ScanProgress = 0;

            // Create scan
            CurrentScanId = await _inventoryService.CreateScanAsync(
                new[] { SelectedProvider }, 
                CancellationToken.None);

            ScanStatus = $"Scan ID: {CurrentScanId}";

            // Find connector
            var connector = Array.Find(_connectors, c => c.Id == SelectedProvider);
            if (connector == null)
            {
                MessageBox.Show($"Connector '{SelectedProvider}' not found", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create filters
            var excludeConfig = CreateExcludeConfig();
            var filters = new EnumerationFilters
            {
                ComputeHash = ComputeHash,
                IncludeFolders = true,
                ExcludeConfig = excludeConfig,
                ExcludedDirectories = ExcludedDirectories.ToArray(),
                ExcludedFilePatterns = ExcludedPatterns.ToArray()
            };

            ScanStatus = "Scanning files...";
            var items = new List<Core.Domain.Item>();
            var startTime = DateTime.Now;

            await foreach (var item in connector.EnumerateAsync(
                ScanPath, 
                filters, 
                CancellationToken.None))
            {
                items.Add(item);
                TotalItems++;

                if (item.Type == Core.Domain.ItemType.File)
                    FilesProcessed++;
                else
                    FoldersProcessed++;

                // Update UI periodically
                if (TotalItems % 50 == 0)
                {
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    var rate = TotalItems / elapsed;
                    ScanStatus = $"Scanning: {TotalItems:N0} items ({FilesProcessed:N0} files, {FoldersProcessed:N0} folders) | {rate:F0} items/sec";
                }
            }

            // Save to database
            ScanStatus = "Saving results to database...";
            await _inventoryService.AddItemsAsync(CurrentScanId, items, CancellationToken.None);

            // Complete scan
            await _inventoryService.CompleteScanAsync(CurrentScanId, CancellationToken.None);
            var result = await _inventoryService.GetScanResultAsync(CurrentScanId, CancellationToken.None);

            // Display results
            HasResults = true;
            ResultsSummary = $"Scan Complete!\n\n" +
                           $"Total Files: {result.Statistics.TotalFiles:N0}\n" +
                           $"Total Folders: {result.Statistics.TotalFolders:N0}\n" +
                           $"Total Size: {FormatBytes(result.Statistics.TotalSize)}\n" +
                           $"Max Depth: {result.Statistics.MaxDepth}\n" +
                           $"Files with Issues: {result.Statistics.FilesWithIssues:N0}\n\n" +
                           $"Scan ID: {CurrentScanId}\n" +
                           $"(Use this ID in Migration page)";

            ScanStatus = "Scan completed successfully!";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during scan: {ex.Message}", "Scan Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            ScanStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private ExcludeConfig CreateExcludeConfig()
    {
        var excludedDirs = new List<string>();

        if (UseDefaultExclusions)
        {
            excludedDirs.AddRange(ExcludeConfig.DefaultSystemDirectories);
            excludedDirs.AddRange(ExcludeConfig.DefaultUserDirectories);
        }

        excludedDirs.AddRange(ExcludedDirectories);

        return new ExcludeConfig
        {
            UseDefaultExclusions = false,
            ExcludedDirectories = excludedDirs.ToArray(),
            ExcludedFilePatterns = ExcludedPatterns.ToArray(),
            EnableCategoryMigration = false,
            IsDryRun = false
        };
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
