using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocsUnmessed.Core.Interfaces;

namespace DocsUnmessed.GUI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private int totalScans;

    public DashboardViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }
}
