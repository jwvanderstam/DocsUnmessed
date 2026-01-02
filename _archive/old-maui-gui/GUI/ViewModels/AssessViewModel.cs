namespace DocsUnmessed.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocsUnmessed.Core.Interfaces;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Assess/Scan page
/// </summary>
public partial class AssessViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly IConnector[] _connectors;

    [ObservableProperty]
    private string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    [ObservableProperty]
    private string selectedProvider = "fs_local";

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private int filesScanned;

    [ObservableProperty]
    private int foldersScanned;

    [ObservableProperty]
    private string totalSize = "0 B";

    [ObservableProperty]
    private double scanProgress;

    [ObservableProperty]
    private string scanStatus = "Ready to scan";

    [ObservableProperty]
    private string? currentScanId;

    [ObservableProperty]
    private bool computeHash = false;

    [ObservableProperty]
    private bool excludeDefault = true;

    [ObservableProperty]
    private ObservableCollection<string> availableProviders = new()
    {
        "fs_local",
        "onedrive_local",
        "googledrive",
        "dropbox",
        "icloud_local"
    };

    public AssessViewModel(IInventoryService inventoryService, IConnector[] connectors)
    {
        _inventoryService = inventoryService;
        _connectors = connectors;
    }

    [RelayCommand]
    private async Task BrowseDirectoryAsync()
    {
        // In a real implementation, use FolderPicker
        // For now, using a simple dialog approach
        try
        {
            // Simplified for demo - in production use proper folder picker
            ScanStatus = "Browse directory - not implemented in this demo";
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            ScanStatus = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            ScanStatus = "Initializing scan...";
            FilesScanned = 0;
            FoldersScanned = 0;
            ScanProgress = 0;

            // Create scan
            var providers = new[] { SelectedProvider };
            CurrentScanId = await _inventoryService.CreateScanAsync(providers, CancellationToken.None);

            ScanStatus = "Scanning files...";

            // Find connector
            var connector = Array.Find(_connectors, c => c.Id == SelectedProvider);
            if (connector == null)
            {
                ScanStatus = $"Error: Connector '{SelectedProvider}' not found";
                return;
            }

            // Enumerate items
            var items = new List<Core.Domain.Item>();
            long totalBytes = 0;

            await foreach (var item in connector.EnumerateAsync(
                RootPath,
                new Connectors.EnumerationFilters
                {
                    ComputeHash = ComputeHash,
                    ExcludeConfig = ExcludeDefault ? new Core.Configuration.ExcludeConfig
                    {
                        UseDefaultExclusions = true
                    } : null
                },
                CancellationToken.None))
            {
                items.Add(item);

                if (item.Type == Core.Domain.ItemType.File)
                {
                    FilesScanned++;
                    totalBytes += item.Size;
                }
                else
                {
                    FoldersScanned++;
                }

                TotalSize = FormatBytes(totalBytes);
                ScanProgress = Math.Min(100, (items.Count / 100.0) * 100); // Simplified progress

                if (items.Count % 50 == 0)
                {
                    ScanStatus = $"Scanned {items.Count} items...";
                }
            }

            // Save to database
            ScanStatus = "Saving to database...";
            await _inventoryService.AddItemsAsync(CurrentScanId, items, CancellationToken.None);

            // Complete scan
            await _inventoryService.CompleteScanAsync(CurrentScanId, CancellationToken.None);

            ScanProgress = 100;
            ScanStatus = $"Complete! Scan ID: {CurrentScanId}";
        }
        catch (Exception ex)
        {
            ScanStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void CancelScan()
    {
        ScanStatus = "Scan cancelled";
        IsScanning = false;
    }

    [RelayCommand]
    private async Task ViewResultsAsync()
    {
        if (string.IsNullOrEmpty(CurrentScanId))
        {
            return;
        }

        // Navigate to results or show details
        await Shell.Current.GoToAsync($"//migration?scanId={CurrentScanId}");
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
