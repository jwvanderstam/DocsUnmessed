namespace DocsUnmessed.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary>
/// ViewModel for Settings page
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool useDefaultExclusions = true;

    [ObservableProperty]
    private string defaultTargetDirectory = "migrated";

    [ObservableProperty]
    private string defaultConflictResolution = "rename";

    [ObservableProperty]
    private bool enableCategorization = true;

    [ObservableProperty]
    private bool preserveStructure = true;

    [ObservableProperty]
    private bool computeHashByDefault = false;

    [ObservableProperty]
    private int maxPageSize = 200;

    [ObservableProperty]
    private int uploadChunkSize = 320;

    [ObservableProperty]
    private string settingsStatus = "Settings loaded";

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            // Save settings to preferences/config file
            // For demo purposes, just show success
            SettingsStatus = "Settings saved successfully";
        }
        catch (Exception ex)
        {
            SettingsStatus = $"Error saving settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        UseDefaultExclusions = true;
        DefaultTargetDirectory = "migrated";
        DefaultConflictResolution = "rename";
        EnableCategorization = true;
        PreserveStructure = true;
        ComputeHashByDefault = false;
        MaxPageSize = 200;
        UploadChunkSize = 320;
        
        SettingsStatus = "Settings reset to defaults";
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            // Clear cache implementation
            await Task.Delay(500); // Simulate
            SettingsStatus = "Cache cleared successfully";
        }
        catch (Exception ex)
        {
            SettingsStatus = $"Error clearing cache: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ClearDatabaseAsync()
    {
        var confirm = await Application.Current?.MainPage?.DisplayAlert(
            "Clear Database",
            "This will delete all scan history. Are you sure?",
            "Yes", "No") ?? false;

        if (!confirm) return;

        try
        {
            // Clear database implementation
            await Task.Delay(500); // Simulate
            SettingsStatus = "Database cleared successfully";
        }
        catch (Exception ex)
        {
            SettingsStatus = $"Error clearing database: {ex.Message}";
        }
    }
}
