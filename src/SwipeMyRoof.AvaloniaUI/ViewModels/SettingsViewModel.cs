using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwipeMyRoof.AvaloniaUI.Services.ModelSelection;

namespace SwipeMyRoof.AvaloniaUI.ViewModels;

/// <summary>
/// View model for the settings screen
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ModelSelectionService _modelSelectionService;
    
    /// <summary>
    /// Available AI providers
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _providers = new();
    
    /// <summary>
    /// Available models for the selected provider
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _models = new();
    
    /// <summary>
    /// Selected provider
    /// </summary>
    [ObservableProperty]
    private string _selectedProvider = string.Empty;
    
    /// <summary>
    /// Selected model
    /// </summary>
    [ObservableProperty]
    private string _selectedModel = string.Empty;
    
    /// <summary>
    /// API key
    /// </summary>
    [ObservableProperty]
    private string _apiKey = string.Empty;
    
    /// <summary>
    /// Base URL for custom endpoints
    /// </summary>
    [ObservableProperty]
    private string _baseUrl = string.Empty;
    
    /// <summary>
    /// Whether to use a custom endpoint
    /// </summary>
    [ObservableProperty]
    private bool _useCustomEndpoint = false;
    
    /// <summary>
    /// Whether to show the confidence indicator
    /// </summary>
    [ObservableProperty]
    private bool _showConfidenceIndicator = true;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="modelSelectionService">Model selection service</param>
    public SettingsViewModel(ModelSelectionService modelSelectionService)
    {
        _modelSelectionService = modelSelectionService;
        
        // Initialize providers
        Providers = new ObservableCollection<string>(_modelSelectionService.GetProviders());
        
        // Set default provider
        SelectedProvider = _modelSelectionService.GetDefaultProvider();
        
        // Load models for the selected provider
        LoadModelsForSelectedProvider();
    }
    
    /// <summary>
    /// Load models for the selected provider
    /// </summary>
    private void LoadModelsForSelectedProvider()
    {
        if (string.IsNullOrEmpty(SelectedProvider))
        {
            Models.Clear();
            return;
        }
        
        Models = new ObservableCollection<string>(_modelSelectionService.GetModelsForProvider(SelectedProvider));
        
        // Set default model
        if (Models.Count > 0)
        {
            SelectedModel = Models[0];
        }
        else
        {
            SelectedModel = string.Empty;
        }
    }
    
    /// <summary>
    /// Called when the selected provider changes
    /// </summary>
    partial void OnSelectedProviderChanged(string value)
    {
        LoadModelsForSelectedProvider();
    }
    
    /// <summary>
    /// Save settings command
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        // TODO: Save settings to a configuration file or service
        
        // For now, just show a message
        Console.WriteLine($"Settings saved: {SelectedProvider}/{SelectedModel}, API Key: {ApiKey}");
    }
    
    /// <summary>
    /// Add custom model command
    /// </summary>
    [RelayCommand]
    private void AddCustomModel(string modelName)
    {
        if (string.IsNullOrEmpty(modelName))
        {
            return;
        }
        
        _modelSelectionService.AddCustomModel(SelectedProvider, modelName);
        LoadModelsForSelectedProvider();
        SelectedModel = modelName;
    }
    
    /// <summary>
    /// Get the full model identifier
    /// </summary>
    /// <returns>The full model identifier</returns>
    public string GetFullModelIdentifier()
    {
        if (string.IsNullOrEmpty(SelectedProvider) || string.IsNullOrEmpty(SelectedModel))
        {
            return string.Empty;
        }
        
        return $"{SelectedProvider}/{SelectedModel}";
    }
}