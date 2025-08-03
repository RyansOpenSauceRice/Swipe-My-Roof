using System;
using System.Collections.Generic;
using System.Linq;

namespace SwipeMyRoof.AvaloniaUI.Services.ModelSelection;

/// <summary>
/// Information about an AI model
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// The provider of the model
    /// </summary>
    public string Provider { get; }
    
    /// <summary>
    /// The model identifier
    /// </summary>
    public string Model { get; }
    
    /// <summary>
    /// The separator used in the model identifier
    /// </summary>
    public string Separator { get; }
    
    /// <summary>
    /// The full model identifier (provider + separator + model)
    /// </summary>
    public string FullIdentifier => string.IsNullOrEmpty(Provider) 
        ? Model 
        : $"{Provider}{Separator}{Model}";
    
    /// <summary>
    /// The display name for the model
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(Provider) 
        ? Model 
        : $"{Provider} {Model}";
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="provider">The provider of the model</param>
    /// <param name="model">The model identifier</param>
    /// <param name="separator">The separator used in the model identifier</param>
    public ModelInfo(string provider, string model, string separator = "/")
    {
        Provider = provider;
        Model = model;
        Separator = separator;
    }
    
    /// <summary>
    /// Extract provider and model information from a model identifier
    /// </summary>
    /// <param name="modelIdentifier">The model identifier string</param>
    /// <returns>A ModelInfo object</returns>
    public static ModelInfo FromIdentifier(string modelIdentifier)
    {
        if (string.IsNullOrEmpty(modelIdentifier))
        {
            return new ModelInfo(string.Empty, string.Empty);
        }
        
        string separator = "/";
        string[] split = modelIdentifier.Split(separator);
        
        if (split.Length == 1)
        {
            // No "/" separator found, try with "."
            separator = ".";
            split = modelIdentifier.Split(separator);
            
            // Check if it's actually a version number
            if (split.Length > 1 && split[1].All(c => char.IsDigit(c)))
            {
                // Undo the split for version numbers
                split = new[] { modelIdentifier };
            }
        }
        
        if (split.Length == 1)
        {
            // No separator found, check if it's a known model
            if (VerifiedModels.OpenAI.Contains(split[0]))
            {
                return new ModelInfo("openai", split[0], "/");
            }
            
            if (VerifiedModels.Anthropic.Contains(split[0]))
            {
                return new ModelInfo("anthropic", split[0], "/");
            }
            
            if (VerifiedModels.Mistral.Contains(split[0]))
            {
                return new ModelInfo("mistral", split[0], "/");
            }
            
            // Return as model only
            return new ModelInfo(string.Empty, modelIdentifier, string.Empty);
        }
        
        string provider = split[0];
        string model = string.Join(separator, split[1..]);
        return new ModelInfo(provider, model, separator);
    }
}

/// <summary>
/// Verified models by provider
/// </summary>
public static class VerifiedModels
{
    /// <summary>
    /// Verified OpenAI models
    /// </summary>
    public static readonly HashSet<string> OpenAI = new(StringComparer.OrdinalIgnoreCase)
    {
        "gpt-4o",
        "gpt-4o-mini",
        "gpt-4-turbo",
        "gpt-4",
        "gpt-3.5-turbo",
    };
    
    /// <summary>
    /// Verified Anthropic models
    /// </summary>
    public static readonly HashSet<string> Anthropic = new(StringComparer.OrdinalIgnoreCase)
    {
        "claude-3-opus-20240229",
        "claude-3-sonnet-20240229",
        "claude-3-haiku-20240307",
        "claude-2.1",
        "claude-2",
    };
    
    /// <summary>
    /// Verified Mistral models
    /// </summary>
    public static readonly HashSet<string> Mistral = new(StringComparer.OrdinalIgnoreCase)
    {
        "mistral-large-latest",
        "mistral-medium-latest",
        "mistral-small-latest",
    };
    
    /// <summary>
    /// Verified providers
    /// </summary>
    public static readonly HashSet<string> Providers = new(StringComparer.OrdinalIgnoreCase)
    {
        "openai",
        "anthropic",
        "mistral",
    };
}