namespace DocsUnmessed.GUI.Views;

using DocsUnmessed.GUI.ViewModels;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
