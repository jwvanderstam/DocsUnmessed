using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI.Views;

public partial class SettingsPage : System.Windows.Controls.Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
