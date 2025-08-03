using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SwipeMyRoof.Images.Models;
using SwipeMyRoof.Images.Services;

namespace SwipeMyRoof.UI.ViewModels;

/// <summary>
/// View model for managing tile providers
/// </summary>
public class TileProviderViewModel : ViewModelBase
{
    private readonly ITileProviderService _tileProviderService;
    private TileProvider? _selectedProvider;
    private bool _isAddingProvider;
    private bool _isEditingProvider;
    private TileProvider _newProvider = new();
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tileProviderService">Tile provider service</param>
    public TileProviderViewModel(ITileProviderService tileProviderService)
    {
        _tileProviderService = tileProviderService;
        
        // Initialize commands
        SelectProviderCommand = ReactiveCommand.Create<TileProvider>(SelectProvider);
        AddProviderCommand = ReactiveCommand.Create(StartAddProvider);
        EditProviderCommand = ReactiveCommand.Create<TileProvider>(StartEditProvider);
        RemoveProviderCommand = ReactiveCommand.Create<TileProvider>(RemoveProvider);
        SaveProviderCommand = ReactiveCommand.Create(SaveProvider);
        CancelEditCommand = ReactiveCommand.Create(CancelEdit);
        
        // Load providers
        LoadProviders();
    }
    
    /// <summary>
    /// Command to select a provider
    /// </summary>
    public ICommand SelectProviderCommand { get; }
    
    /// <summary>
    /// Command to add a new provider
    /// </summary>
    public ICommand AddProviderCommand { get; }
    
    /// <summary>
    /// Command to edit a provider
    /// </summary>
    public ICommand EditProviderCommand { get; }
    
    /// <summary>
    /// Command to remove a provider
    /// </summary>
    public ICommand RemoveProviderCommand { get; }
    
    /// <summary>
    /// Command to save a provider
    /// </summary>
    public ICommand SaveProviderCommand { get; }
    
    /// <summary>
    /// Command to cancel editing
    /// </summary>
    public ICommand CancelEditCommand { get; }
    
    /// <summary>
    /// List of available providers
    /// </summary>
    public ObservableCollection<TileProvider> Providers { get; } = new();
    
    /// <summary>
    /// Currently selected provider
    /// </summary>
    public TileProvider? SelectedProvider
    {
        get => _selectedProvider;
        set => this.RaiseAndSetIfChanged(ref _selectedProvider, value);
    }
    
    /// <summary>
    /// Whether a provider is being added
    /// </summary>
    public bool IsAddingProvider
    {
        get => _isAddingProvider;
        private set => this.RaiseAndSetIfChanged(ref _isAddingProvider, value);
    }
    
    /// <summary>
    /// Whether a provider is being edited
    /// </summary>
    public bool IsEditingProvider
    {
        get => _isEditingProvider;
        private set => this.RaiseAndSetIfChanged(ref _isEditingProvider, value);
    }
    
    /// <summary>
    /// New provider being added or edited
    /// </summary>
    public TileProvider NewProvider
    {
        get => _newProvider;
        set => this.RaiseAndSetIfChanged(ref _newProvider, value);
    }
    
    /// <summary>
    /// List of available provider types
    /// </summary>
    public IEnumerable<TileProviderType> ProviderTypes => Enum.GetValues<TileProviderType>();
    
    private void LoadProviders()
    {
        Providers.Clear();
        
        foreach (var provider in _tileProviderService.GetAllProviders())
        {
            Providers.Add(provider);
        }
        
        SelectedProvider = _tileProviderService.GetCurrentProvider();
    }
    
    private void SelectProvider(TileProvider provider)
    {
        _tileProviderService.SetCurrentProvider(provider.Id);
        SelectedProvider = provider;
    }
    
    private void StartAddProvider()
    {
        NewProvider = new TileProvider
        {
            IsUserDefined = true,
            Type = TileProviderType.XYZ,
            MinZoom = 1,
            MaxZoom = 19,
            CacheDays = 30
        };
        
        IsAddingProvider = true;
        IsEditingProvider = false;
    }
    
    private void StartEditProvider(TileProvider provider)
    {
        // Create a copy of the provider for editing
        NewProvider = new TileProvider
        {
            Id = provider.Id,
            Name = provider.Name,
            UrlTemplate = provider.UrlTemplate,
            ApiKey = provider.ApiKey,
            Attribution = provider.Attribution,
            AttributionUrl = provider.AttributionUrl,
            MaxZoom = provider.MaxZoom,
            MinZoom = provider.MinZoom,
            CacheDays = provider.CacheDays,
            Type = provider.Type,
            IsUserDefined = provider.IsUserDefined,
            Bounds = provider.Bounds,
            StartDate = provider.StartDate,
            EndDate = provider.EndDate,
            Headers = new Dictionary<string, string>(provider.Headers)
        };
        
        IsAddingProvider = false;
        IsEditingProvider = true;
    }
    
    private void RemoveProvider(TileProvider provider)
    {
        _tileProviderService.RemoveProvider(provider.Id);
        LoadProviders();
    }
    
    private void SaveProvider()
    {
        _tileProviderService.AddProvider(NewProvider);
        
        IsAddingProvider = false;
        IsEditingProvider = false;
        
        LoadProviders();
    }
    
    private void CancelEdit()
    {
        IsAddingProvider = false;
        IsEditingProvider = false;
    }
}