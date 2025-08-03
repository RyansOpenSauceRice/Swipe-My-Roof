using System.Text.Json;
using SwipeMyRoof.Settings.Models;

namespace SwipeMyRoof.Settings.Services;

/// <summary>
/// Local implementation of the settings service
/// </summary>
public class LocalSettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings = new();
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="settingsPath">Settings file path</param>
    public LocalSettingsService(string settingsPath = "settings.json")
    {
        _settingsPath = settingsPath;
        
        // Load settings from disk
        LoadSettings();
    }
    
    /// <inheritdoc />
    public Task<AppSettings> GetSettingsAsync()
    {
        return Task.FromResult(_settings);
    }
    
    /// <inheritdoc />
    public async Task<bool> SaveSettingsAsync(AppSettings settings)
    {
        _settings = settings;
        return await SaveSettingsToFileAsync();
    }
    
    /// <inheritdoc />
    public Task<LlmSettings> GetLlmSettingsAsync()
    {
        return Task.FromResult(_settings.Llm);
    }
    
    /// <inheritdoc />
    public async Task<bool> SaveLlmSettingsAsync(LlmSettings settings)
    {
        _settings.Llm = settings;
        return await SaveSettingsToFileAsync();
    }
    
    /// <inheritdoc />
    public Task<OsmSettings> GetOsmSettingsAsync()
    {
        return Task.FromResult(_settings.Osm);
    }
    
    /// <inheritdoc />
    public async Task<bool> SaveOsmSettingsAsync(OsmSettings settings)
    {
        _settings.Osm = settings;
        return await SaveSettingsToFileAsync();
    }
    
    /// <inheritdoc />
    public Task<ImageSettings> GetImageSettingsAsync()
    {
        return Task.FromResult(_settings.Image);
    }
    
    /// <inheritdoc />
    public async Task<bool> SaveImageSettingsAsync(ImageSettings settings)
    {
        _settings.Image = settings;
        return await SaveSettingsToFileAsync();
    }
    
    /// <inheritdoc />
    public Task<UiSettings> GetUiSettingsAsync()
    {
        return Task.FromResult(_settings.Ui);
    }
    
    /// <inheritdoc />
    public async Task<bool> SaveUiSettingsAsync(UiSettings settings)
    {
        _settings.Ui = settings;
        return await SaveSettingsToFileAsync();
    }
    
    /// <inheritdoc />
    public Task<DecoySettings> GetDecoySettingsAsync()
    {
        return Task.FromResult(_settings.Decoy);
    }
    
    /// <inheritdoc />
    public async Task<bool> SaveDecoySettingsAsync(DecoySettings settings)
    {
        _settings.Decoy = settings;
        return await SaveSettingsToFileAsync();
    }
    
    /// <inheritdoc />
    public async Task<bool> ResetSettingsAsync()
    {
        _settings = new AppSettings();
        return await SaveSettingsToFileAsync();
    }
    
    #region Private Methods
    
    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                
                if (settings != null)
                {
                    _settings = settings;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
    }
    
    private async Task<bool> SaveSettingsToFileAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsPath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}