using System;
using System.Collections.Generic;
using System.Linq;

namespace SwipeMyRoof.AvaloniaUI.Services.ModelSelection;

/// <summary>
/// Extended model information with quality and cost metrics
/// </summary>
public class ModelInfoExtended
{
    /// <summary>
    /// The base model information
    /// </summary>
    public ModelInfo BaseInfo { get; }
    
    /// <summary>
    /// The quality score of the model (0.0 to 1.0)
    /// </summary>
    public double Quality { get; }
    
    /// <summary>
    /// The token limit of the model
    /// </summary>
    public int TokenLimit { get; }
    
    /// <summary>
    /// The cost per token of the model
    /// </summary>
    public double CostPerToken { get; }
    
    /// <summary>
    /// The name of the model
    /// </summary>
    public string Name => BaseInfo.DisplayName;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ModelInfoExtended(ModelInfo baseInfo, double quality, int tokenLimit, double costPerToken)
    {
        BaseInfo = baseInfo;
        Quality = quality;
        TokenLimit = tokenLimit;
        CostPerToken = costPerToken;
    }
    
    /// <summary>
    /// Constructor with model identifier
    /// </summary>
    public ModelInfoExtended(string modelIdentifier, double quality, int tokenLimit, double costPerToken)
        : this(ModelInfo.FromIdentifier(modelIdentifier), quality, tokenLimit, costPerToken)
    {
    }
    
    /// <summary>
    /// Calculate the cost for a given number of tokens
    /// </summary>
    public double GetCost(int tokens)
    {
        return tokens * CostPerToken;
    }
}

/// <summary>
/// Selects the best model based on constraints
/// </summary>
public class ModelSelector
{
    private readonly List<ModelInfoExtended> _models;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ModelSelector(IEnumerable<ModelInfoExtended> models)
    {
        _models = models.ToList();
    }
    
    /// <summary>
    /// Select the best model based on token count and budget constraints
    /// </summary>
    public ModelInfoExtended SelectModel(int tokenCount, double maxBudget)
    {
        // Filter models that can handle the token count and are within budget
        var eligibleModels = _models
            .Where(m => m.TokenLimit >= tokenCount)
            .Where(m => m.GetCost(tokenCount) <= maxBudget)
            .ToList();
        
        if (!eligibleModels.Any())
        {
            throw new InvalidOperationException($"No model available that can handle {tokenCount} tokens within a budget of ${maxBudget:F2}");
        }
        
        // Select the model with the highest quality
        return eligibleModels.OrderByDescending(m => m.Quality).First();
    }
}