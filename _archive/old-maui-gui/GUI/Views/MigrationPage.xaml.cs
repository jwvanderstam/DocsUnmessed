namespace DocsUnmessed.GUI.Views;

using DocsUnmessed.GUI.ViewModels;

public partial class MigrationPage : ContentPage
{
    public MigrationPage(MigrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
