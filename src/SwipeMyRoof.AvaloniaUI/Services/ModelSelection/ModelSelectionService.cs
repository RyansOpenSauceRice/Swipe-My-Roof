using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwipeMyRoof.AvaloniaUI.Services.ModelSelection;

/// <summary>
/// Service for selecting AI models
/// </summary>
public class ModelSelectionService
{
    private readonly Dictionary<string, ProviderInfo> _organizedModels = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ModelSelectionService()
    {
        // Initialize with default models
        InitializeDefaultModels();
    }
    
    /// <summary>
    /// Initialize with default models
    /// </summary>
    private void InitializeDefaultModels()
    {
        // Add OpenAI models
        _organizedModels["openai"] = new ProviderInfo
        {
            Separator = "/",
            Models = VerifiedModels.OpenAI.ToList()
        };
        
        // Add Anthropic models
        _organizedModels["anthropic"] = new ProviderInfo
        {
            Separator = "/",
            Models = VerifiedModels.Anthropic.ToList()
        };
        
        // Add Mistral models
        _organizedModels["mistral"] = new ProviderInfo
        {
            Separator = "/",
            Models = VerifiedModels.Mistral.ToList()
        };
    }
    
    /// <summary>
    /// Get all supported models
    /// </summary>
    /// <returns>List of model identifiers</returns>
    public List<string> GetSupportedModels()
    {
        var result = new List<string>();
        
        foreach (var provider in _organizedModels)
        {
            foreach (var model in provider.Value.Models)
            {
                result.Add($"{provider.Key}{provider.Value.Separator}{model}");
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get all providers
    /// </summary>
    /// <returns>List of providers</returns>
    public List<string> GetProviders()
    {
        return _organizedModels.Keys.ToList();
    }
    
    /// <summary>
    /// Get models for a provider
    /// </summary>
    /// <param name="provider">The provider</param>
    /// <returns>List of models</returns>
    public List<string> GetModelsForProvider(string provider)
    {
        if (_organizedModels.TryGetValue(provider, out var providerInfo))
        {
            return providerInfo.Models;
        }
        
        return new List<string>();
    }
    
    /// <summary>
    /// Get the default model for a provider
    /// </summary>
    /// <param name="provider">The provider</param>
    /// <returns>The default model</returns>
    public string GetDefaultModelForProvider(string provider)
    {
        if (_organizedModels.TryGetValue(provider, out var providerInfo) && 
            providerInfo.Models.Count > 0)
        {
            return providerInfo.Models[0];
        }
        
        return string.Empty;
    }
    
    /// <summary>
    /// Get the default provider
    /// </summary>
    /// <returns>The default provider</returns>
    public string GetDefaultProvider()
    {
        // Prefer Anthropic if available
        if (_organizedModels.ContainsKey("anthropic"))
        {
            return "anthropic";
        }
        
        // Fall back to OpenAI
        if (_organizedModels.ContainsKey("openai"))
        {
            return "openai";
        }
        
        // Fall back to first provider
        return _organizedModels.Keys.FirstOrDefault() ?? string.Empty;
    }
    
    /// <summary>
    /// Add a custom model
    /// </summary>
    /// <param name="provider">The provider</param>
    /// <param name="model">The model</param>
    public void AddCustomModel(string provider, string model)
    {
        if (!_organizedModels.TryGetValue(provider, out var providerInfo))
        {
            providerInfo = new ProviderInfo
            {
                Separator = "/",
                Models = new List<string>()
            };
            
            _organizedModels[provider] = providerInfo;
        }
        
        if (!providerInfo.Models.Contains(model))
        {
            providerInfo.Models.Add(model);
        }
    }
}

/// <summary>
/// Information about a provider and its models
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// The separator used in model identifiers
    /// </summary>
    public string Separator { get; set; } = "/";
    
    /// <summary>
    /// The models for this provider
    /// </summary>
    public List<string> Models { get; set; } = new();
}