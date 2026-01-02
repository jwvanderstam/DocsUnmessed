namespace DocsUnmessed.GUI.Views;

using DocsUnmessed.GUI.ViewModels;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is DashboardViewModel vm)
        {
            await vm.LoadDashboardCommand.ExecuteAsync(null);
        }
    }
}
