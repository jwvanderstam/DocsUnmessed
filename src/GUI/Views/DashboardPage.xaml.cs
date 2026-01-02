using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI.Views;

public partial class DashboardPage : System.Windows.Controls.Page
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
