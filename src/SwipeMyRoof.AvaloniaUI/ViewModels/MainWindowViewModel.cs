using CommunityToolkit.Mvvm.ComponentModel;
using SwipeMyRoof.AvaloniaUI.Services.ModelSelection;

namespace SwipeMyRoof.AvaloniaUI.ViewModels;

/// <summary>
/// Main window view model
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Selected tab index
    /// </summary>
    [ObservableProperty]
    private int _selectedTabIndex = 0;
    
    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Ready";
    
    /// <summary>
    /// Building validation view model
    /// </summary>
    public BuildingValidationViewModel BuildingValidationViewModel { get; }
    
    /// <summary>
    /// Settings view model
    /// </summary>
    public SettingsViewModel SettingsViewModel { get; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel()
    {
        // Create model selection service
        var modelSelectionService = new ModelSelectionService();
        
        // Create view models
        BuildingValidationViewModel = new BuildingValidationViewModel();
        SettingsViewModel = new SettingsViewModel(modelSelectionService);
    }
}
