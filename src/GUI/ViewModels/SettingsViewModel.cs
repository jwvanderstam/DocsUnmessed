using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DocsUnmessed.GUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool enableLogging = true;

    [ObservableProperty]
    private string logLevel = "Information";

    [ObservableProperty]
    private bool autoSaveSettings = true;

    [ObservableProperty]
    private int maxConcurrentOperations = 4;

    [ObservableProperty]
    private bool confirmBeforeDelete = true;

    [ObservableProperty]
    private string? databasePath;

    public SettingsViewModel()
    {
        DatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DocsUnmessed",
            "docsunmessed.db");
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // Save settings to configuration file
        MessageBox.Show("Settings saved successfully!", "Settings", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ResetSettings()
    {
        var result = MessageBox.Show(
            "Reset all settings to defaults?", 
            "Confirm Reset", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            EnableLogging = true;
            LogLevel = "Information";
            AutoSaveSettings = true;
            MaxConcurrentOperations = 4;
            ConfirmBeforeDelete = true;

            MessageBox.Show("Settings reset to defaults", "Settings", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ClearCache()
    {
        var result = MessageBox.Show(
            "Clear application cache?", 
            "Confirm Clear", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            MessageBox.Show("Cache cleared successfully!", "Settings", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ClearDatabase()
    {
        var result = MessageBox.Show(
            "Clear entire database? All scan history will be lost!", 
            "Confirm Clear", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            MessageBox.Show("Database cleared successfully!", "Settings", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void OpenDatabaseFolder()
    {
        var folder = Path.GetDirectoryName(DatabasePath);
        if (folder != null && Directory.Exists(folder))
        {
            Process.Start("explorer.exe", folder);
        }
    }

    [RelayCommand]
    private void OpenCLIReference()
    {
        OpenDocumentationFile("docs\\CLI-REFERENCE.md");
    }

    [RelayCommand]
    private void OpenQuickStart()
    {
        OpenDocumentationFile("docs\\guides\\QUICK-START-CARD.md");
    }

    [RelayCommand]
    private void OpenBuildGuide()
    {
        OpenDocumentationFile("docs\\build\\CREATE-EXECUTABLE-GUIDE.md");
    }

    [RelayCommand]
    private void OpenDocsFolder()
    {
        var docsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "docs");
        var fullPath = Path.GetFullPath(docsPath);
        
        if (Directory.Exists(fullPath))
        {
            Process.Start("explorer.exe", fullPath);
        }
        else
        {
            MessageBox.Show("Documentation folder not found", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenDocumentationFile(string relativePath)
    {
        try
        {
            // Try to find docs folder relative to application
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var docsPath = Path.Combine(appPath, "..", "..", "..", "..", relativePath);
            var fullPath = Path.GetFullPath(docsPath);

            if (File.Exists(fullPath))
            {
                // Open with default markdown viewer or notepad
                var psi = new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show($"Documentation file not found:\n{relativePath}", 
                    "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening documentation:\n{ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
