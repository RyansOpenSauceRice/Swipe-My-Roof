namespace SwipeMyRoof.Settings.Models;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings
{
    /// <summary>
    /// LLM settings
    /// </summary>
    public LlmSettings Llm { get; set; } = new();
    
    /// <summary>
    /// OSM settings
    /// </summary>
    public OsmSettings Osm { get; set; } = new();
    
    /// <summary>
    /// Image settings
    /// </summary>
    public ImageSettings Image { get; set; } = new();
    
    /// <summary>
    /// UI settings
    /// </summary>
    public UiSettings Ui { get; set; } = new();
    
    /// <summary>
    /// Decoy settings
    /// </summary>
    public DecoySettings Decoy { get; set; } = new();
}

/// <summary>
/// LLM settings
/// </summary>
public class LlmSettings
{
    /// <summary>
    /// API endpoint
    /// </summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    
    /// <summary>
    /// API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Model name
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";
    
    /// <summary>
    /// Whether to use high-resolution images
    /// </summary>
    public bool UseHighResolution { get; set; } = false;
    
    /// <summary>
    /// Maximum token budget per session
    /// </summary>
    public int MaxTokenBudget { get; set; } = 10000;
    
    /// <summary>
    /// Warning threshold percentage (0-100)
    /// </summary>
    public int WarningThresholdPercent { get; set; } = 75;
}

/// <summary>
/// OSM settings
/// </summary>
public class OsmSettings
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Default changeset comment
    /// </summary>
    public string DefaultChangesetComment { get; set; } = "Adding roof:colour tag via Swipe My Roof";
    
    /// <summary>
    /// Whether to automatically upload edits
    /// </summary>
    public bool AutoUpload { get; set; } = false;
    
    /// <summary>
    /// Maximum batch size for uploads
    /// </summary>
    public int MaxBatchSize { get; set; } = 10;
}

/// <summary>
/// Image settings
/// </summary>
public class ImageSettings
{
    /// <summary>
    /// Bing Maps API key
    /// </summary>
    public string BingMapsApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Default image width
    /// </summary>
    public int DefaultWidth { get; set; } = 64;
    
    /// <summary>
    /// Default image height
    /// </summary>
    public int DefaultHeight { get; set; } = 64;
    
    /// <summary>
    /// Building buffer ratio
    /// </summary>
    public double BuildingBufferRatio { get; set; } = 0.3;
    
    /// <summary>
    /// JPEG quality (0-100)
    /// </summary>
    public int JpegQuality { get; set; } = 80;
}

/// <summary>
/// UI settings
/// </summary>
public class UiSettings
{
    /// <summary>
    /// Theme (Light, Dark, System)
    /// </summary>
    public string Theme { get; set; } = "System";
    
    /// <summary>
    /// Whether to use swipe gestures
    /// </summary>
    public bool UseSwipeGestures { get; set; } = true;
    
    /// <summary>
    /// Whether to show confidence indicators
    /// </summary>
    public bool ShowConfidenceIndicators { get; set; } = true;
    
    /// <summary>
    /// Whether to show token usage
    /// </summary>
    public bool ShowTokenUsage { get; set; } = true;
    
    /// <summary>
    /// Whether to show practice mode banner
    /// </summary>
    public bool ShowPracticeModeBanner { get; set; } = true;
}

/// <summary>
/// Decoy settings
/// </summary>
public class DecoySettings
{
    /// <summary>
    /// Decoy frequency (0.0-1.0)
    /// </summary>
    public double Frequency { get; set; } = 0.2;
    
    /// <summary>
    /// Minimum number of buildings before introducing decoys
    /// </summary>
    public int MinBuildingsBeforeDecoys { get; set; } = 5;
    
    /// <summary>
    /// Whether to adapt decoy frequency based on reliability
    /// </summary>
    public bool AdaptFrequencyBasedOnReliability { get; set; } = true;
    
    /// <summary>
    /// Maximum decoy frequency (0.0-1.0)
    /// </summary>
    public double MaxFrequency { get; set; } = 0.5;
}
