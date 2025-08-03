using SwipeMyRoof.Settings.Models;

namespace SwipeMyRoof.Settings.Services;

/// <summary>
/// Interface for settings service operations
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Get the application settings
    /// </summary>
    /// <returns>Application settings</returns>
    Task<AppSettings> GetSettingsAsync();
    
    /// <summary>
    /// Save the application settings
    /// </summary>
    /// <param name="settings">Application settings</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveSettingsAsync(AppSettings settings);
    
    /// <summary>
    /// Get the LLM settings
    /// </summary>
    /// <returns>LLM settings</returns>
    Task<LlmSettings> GetLlmSettingsAsync();
    
    /// <summary>
    /// Save the LLM settings
    /// </summary>
    /// <param name="settings">LLM settings</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveLlmSettingsAsync(LlmSettings settings);
    
    /// <summary>
    /// Get the OSM settings
    /// </summary>
    /// <returns>OSM settings</returns>
    Task<OsmSettings> GetOsmSettingsAsync();
    
    /// <summary>
    /// Save the OSM settings
    /// </summary>
    /// <param name="settings">OSM settings</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveOsmSettingsAsync(OsmSettings settings);
    
    /// <summary>
    /// Get the image settings
    /// </summary>
    /// <returns>Image settings</returns>
    Task<ImageSettings> GetImageSettingsAsync();
    
    /// <summary>
    /// Save the image settings
    /// </summary>
    /// <param name="settings">Image settings</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveImageSettingsAsync(ImageSettings settings);
    
    /// <summary>
    /// Get the UI settings
    /// </summary>
    /// <returns>UI settings</returns>
    Task<UiSettings> GetUiSettingsAsync();
    
    /// <summary>
    /// Save the UI settings
    /// </summary>
    /// <param name="settings">UI settings</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveUiSettingsAsync(UiSettings settings);
    
    /// <summary>
    /// Get the decoy settings
    /// </summary>
    /// <returns>Decoy settings</returns>
    Task<DecoySettings> GetDecoySettingsAsync();
    
    /// <summary>
    /// Save the decoy settings
    /// </summary>
    /// <param name="settings">Decoy settings</param>
    /// <returns>True if successful</returns>
    Task<bool> SaveDecoySettingsAsync(DecoySettings settings);
    
    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    /// <returns>True if successful</returns>
    Task<bool> ResetSettingsAsync();
}