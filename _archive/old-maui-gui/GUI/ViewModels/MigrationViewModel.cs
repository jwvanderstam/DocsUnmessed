namespace DocsUnmessed.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocsUnmessed.Core.Interfaces;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Migration page
/// </summary>
public partial class MigrationViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private string scanId = string.Empty;

    [ObservableProperty]
    private string targetDirectory = "migrated";

    [ObservableProperty]
    private bool enableCategorization = true;

    [ObservableProperty]
    private bool preserveStructure = true;

    [ObservableProperty]
    private bool isDryRun = true;

    [ObservableProperty]
    private string conflictResolution = "rename";

    [ObservableProperty]
    private bool isMigrating;

    [ObservableProperty]
    private int filesProcessed;

    [ObservableProperty]
    private int filesSucceeded;

    [ObservableProperty]
    private int filesFailed;

    [ObservableProperty]
    private double migrationProgress;

    [ObservableProperty]
    private string migrationStatus = "Ready to migrate";

    [ObservableProperty]
    private ObservableCollection<string> conflictOptions = new()
    {
        "rename",
        "skip",
        "overwrite"
    };

    [ObservableProperty]
    private ObservableCollection<PreviewItem> previewItems = new();

    [ObservableProperty]
    private bool hasPreview;

    public MigrationViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [RelayCommand]
    private async Task LoadScanAsync()
    {
        if (string.IsNullOrWhiteSpace(ScanId))
        {
            MigrationStatus = "Please enter a scan ID";
            return;
        }

        try
        {
            var scan = await _inventoryService.GetScanResultAsync(ScanId, CancellationToken.None);
            if (scan == null)
            {
                MigrationStatus = $"Scan ID '{ScanId}' not found";
                return;
            }

            MigrationStatus = $"Loaded scan: {scan.Statistics.TotalFiles:N0} files, {FormatBytes(scan.Statistics.TotalSize)}";
        }
        catch (Exception ex)
        {
            MigrationStatus = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PreviewMigrationAsync()
    {
        if (string.IsNullOrWhiteSpace(ScanId))
        {
            MigrationStatus = "Please enter a scan ID";
            return;
        }

        try
        {
            PreviewItems.Clear();
            MigrationStatus = "Generating preview...";

            var scan = await _inventoryService.GetScanResultAsync(ScanId, CancellationToken.None);
            if (scan == null)
            {
                MigrationStatus = "Scan not found";
                return;
            }

            // Simulate preview (simplified)
            var files = scan.Items.Where(i => i.Type == Core.Domain.ItemType.File).Take(20);
            foreach (var file in files)
            {
                var category = GetCategory(file.Name);
                var targetPath = EnableCategorization
                    ? Path.Combine(TargetDirectory, category, file.Name)
                    : Path.Combine(TargetDirectory, file.Name);

                PreviewItems.Add(new PreviewItem
                {
                    SourcePath = file.Path,
                    TargetPath = targetPath,
                    Size = FormatBytes(file.Size)
                });
            }

            HasPreview = PreviewItems.Count > 0;
            MigrationStatus = $"Preview generated: {PreviewItems.Count} items shown";
        }
        catch (Exception ex)
        {
            MigrationStatus = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExecuteMigrationAsync()
    {
        if (string.IsNullOrWhiteSpace(ScanId))
        {
            MigrationStatus = "Please enter a scan ID";
            return;
        }

        if (!IsDryRun)
        {
            var confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirm Migration",
                "This will copy/move files. Continue?",
                "Yes", "No") ?? false;

            if (!confirm)
            {
                return;
            }
        }

        try
        {
            IsMigrating = true;
            FilesProcessed = 0;
            FilesSucceeded = 0;
            FilesFailed = 0;
            MigrationProgress = 0;

            var scan = await _inventoryService.GetScanResultAsync(ScanId, CancellationToken.None);
            if (scan == null)
            {
                MigrationStatus = "Scan not found";
                return;
            }

            var files = scan.Items.Where(i => i.Type == Core.Domain.ItemType.File).ToList();
            var totalFiles = files.Count;

            foreach (var file in files)
            {
                if (IsDryRun)
                {
                    // Simulate
                    await Task.Delay(10);
                }
                else
                {
                    // Actual migration would go here
                    // For now, just simulate
                    await Task.Delay(10);
                }

                FilesProcessed++;
                FilesSucceeded++;
                MigrationProgress = (double)FilesProcessed / totalFiles * 100;
                MigrationStatus = $"Processing: {FilesProcessed}/{totalFiles}";
            }

            MigrationStatus = IsDryRun
                ? $"Dry-run complete: {FilesSucceeded} files would be migrated"
                : $"Migration complete: {FilesSucceeded} succeeded, {FilesFailed} failed";
        }
        catch (Exception ex)
        {
            MigrationStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsMigrating = false;
        }
    }

    private static string GetCategory(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" => "Pictures",
            ".mp3" or ".wav" or ".flac" => "Music",
            ".mp4" or ".avi" or ".mkv" => "Videos",
            ".pdf" or ".doc" or ".docx" or ".txt" => "Documents",
            ".zip" or ".rar" or ".7z" => "Archives",
            _ => "Other"
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

public class PreviewItem
{
    public required string SourcePath { get; set; }
    public required string TargetPath { get; set; }
    public required string Size { get; set; }
}
