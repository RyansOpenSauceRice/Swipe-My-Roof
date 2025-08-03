using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using System.Net.Http;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SwipeMyRoof.AvaloniaUI.ViewModels;
using SwipeMyRoof.AvaloniaUI.Views;
using SwipeMyRoof.OSM.Services;
using SwipeMyRoof.Storage.Services;
using SwipeMyRoof.UI.ViewModels;

namespace SwipeMyRoof.AvaloniaUI;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Create main window with DI
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider?.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Register HttpClient
        services.AddSingleton<HttpClient>();
        
        // Register services
        services.AddSingleton<IStorageService, FileStorageService>();
        services.AddSingleton<IOsmAuthService, OsmAuthService>();
        services.AddSingleton<IOsmService, OsmService>();
        services.AddSingleton<ITileProviderService, TileProviderService>();
        services.AddSingleton<IImageService, BingImageService>(sp => 
            new BingImageService(sp.GetRequiredService<HttpClient>(), "YOUR_BING_MAPS_KEY"));
        
        // Register view models
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<OsmAuthViewModel>();
        services.AddTransient<BuildingValidationViewModel>();
        services.AddTransient<TileProviderViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}