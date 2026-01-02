using System;
using System.Windows;
using System.Windows.Controls;
using DocsUnmessed.GUI.Views;
using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Navigate to Dashboard on startup
        NavigateToDashboard();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        NavigateToDashboard();
    }

    private void Assess_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = App.GetService<AssessViewModel>();
        var page = new AssessPage(viewModel);
        ContentFrame.Navigate(page);
    }

    private void Migration_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = App.GetService<MigrationViewModel>();
        var page = new MigrationPage(viewModel);
        ContentFrame.Navigate(page);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = App.GetService<SettingsViewModel>();
        var page = new SettingsPage(viewModel);
        ContentFrame.Navigate(page);
    }

    private void NavigateToDashboard()
    {
        var viewModel = App.GetService<DashboardViewModel>();
        var page = new DashboardPage(viewModel);
        ContentFrame.Navigate(page);
    }
}