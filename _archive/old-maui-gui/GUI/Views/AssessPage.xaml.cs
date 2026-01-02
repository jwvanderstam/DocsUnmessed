namespace DocsUnmessed.GUI.Views;

using DocsUnmessed.GUI.ViewModels;

public partial class AssessPage : ContentPage
{
    public AssessPage(AssessViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
