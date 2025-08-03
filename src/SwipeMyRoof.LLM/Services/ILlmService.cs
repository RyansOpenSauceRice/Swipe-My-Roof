using SwipeMyRoof.LLM.Models;

namespace SwipeMyRoof.LLM.Services;

/// <summary>
/// Interface for AI service operations
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Get a roof color suggestion from the AI
    /// </summary>
    /// <param name="request">The roof color request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The roof color response</returns>
    Task<RoofColorResponse> GetRoofColorSuggestionAsync(RoofColorRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a new roof color suggestion after user rejection
    /// </summary>
    /// <param name="request">The re-suggestion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The roof color response</returns>
    Task<RoofColorResponse> GetRoofColorReSuggestionAsync(RoofColorReSuggestionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the estimated token usage for a color request
    /// </summary>
    /// <param name="request">The roof color request</param>
    /// <returns>Estimated token usage</returns>
    int EstimateTokenUsage(RoofColorRequest request);
}