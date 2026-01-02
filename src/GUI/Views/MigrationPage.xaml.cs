using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI.Views;

public partial class MigrationPage : System.Windows.Controls.Page
{
    public MigrationPage(MigrationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
