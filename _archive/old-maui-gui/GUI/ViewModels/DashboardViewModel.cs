namespace DocsUnmessed.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocsUnmessed.Core.Interfaces;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Dashboard page
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private string welcomeMessage = "Welcome to DocsUnmessed!";

    [ObservableProperty]
    private int totalScans;

    [ObservableProperty]
    private long totalFiles;

    [ObservableProperty]
    private string totalSize = "0 B";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private ObservableCollection<RecentScanInfo> recentScans = new();

    public DashboardViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        try
        {
            // Load statistics from database
            // This is a placeholder - you'll implement actual database queries
            TotalScans = 0; // await _inventoryService.GetTotalScansAsync();
            TotalFiles = 0; // await _inventoryService.GetTotalFilesAsync();
            TotalSize = "0 B"; // FormatBytes(await _inventoryService.GetTotalSizeAsync());

            // Load recent scans
            RecentScans.Clear();
            // var scans = await _inventoryService.GetRecentScansAsync(5);
            // foreach (var scan in scans)
            // {
            //     RecentScans.Add(scan);
            // }
        }
        finally
        {
            IsLoading = false;
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

public class RecentScanInfo
{
    public required string ScanId { get; set; }
    public DateTime Date { get; set; }
    public int FileCount { get; set; }
    public string Size { get; set; } = "0 B";
}
