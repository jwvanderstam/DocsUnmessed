using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI.Views;

public partial class AssessPage : System.Windows.Controls.Page
{
    public AssessPage(AssessViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
