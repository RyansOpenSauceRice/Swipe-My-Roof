using System.Text;
using System.Text.Json;
using SwipeMyRoof.LLM.Models;

namespace SwipeMyRoof.LLM.Services;

/// <summary>
/// OpenAI implementation of the LLM service
/// </summary>
public class OpenAiLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _model;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="apiKey">OpenAI API key</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="model">Model name (e.g., "gpt-4o-mini")</param>
    public OpenAiLlmService(HttpClient httpClient, string apiKey, string endpoint = "https://api.openai.com/v1/chat/completions", string model = "gpt-4o-mini")
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _endpoint = endpoint;
        _model = model;
    }
    
    /// <inheritdoc />
    public async Task<RoofColorResponse> GetRoofColorSuggestionAsync(RoofColorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var systemMessage = "You are a tool for tagging roof colors on OpenStreetMap buildings. " +
                               "Given metadata and an image summary, return a JSON object describing the most likely roof color, confidence, and a brief explanation. " +
                               "Do not include text outside the JSON. " +
                               "Use only the approved color palette provided.";
            
            var messages = new[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = JsonSerializer.Serialize(request) }
            };
            
            var requestBody = new
            {
                model = _model,
                messages,
                response_format = new { type = "json_object" },
                temperature = 0.2
            };
            
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            
            var response = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
            
            if (responseObject?.Choices == null || responseObject.Choices.Length == 0)
            {
                return new RoofColorResponse
                {
                    Color = "other",
                    Confidence = 0.0,
                    Explanation = "no ai result",
                    Method = "error"
                };
            }
            
            var llmResponse = responseObject.Choices[0].Message.Content;
            var roofColorResponse = JsonSerializer.Deserialize<RoofColorResponse>(llmResponse);
            
            if (roofColorResponse == null)
            {
                return new RoofColorResponse
                {
                    Color = "other",
                    Confidence = 0.0,
                    Explanation = "invalid response format",
                    Method = "error"
                };
            }
            
            return roofColorResponse;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error in GetRoofColorSuggestionAsync: {ex.Message}");
            
            return new RoofColorResponse
            {
                Color = "other",
                Confidence = 0.0,
                Explanation = "error: " + ex.Message,
                Method = "error"
            };
        }
    }
    
    /// <inheritdoc />
    public async Task<RoofColorResponse> GetRoofColorReSuggestionAsync(RoofColorReSuggestionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var systemMessage = "Re-analyze the building's roof color using the provided image and metadata. " +
                               "Take into account that the previous color suggestion was rejected. " +
                               "Suggest the next most probable color from the palette. " +
                               "Return JSON only.";
            
            var messages = new[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = JsonSerializer.Serialize(request) }
            };
            
            var requestBody = new
            {
                model = _model,
                messages,
                response_format = new { type = "json_object" },
                temperature = 0.3
            };
            
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            
            var response = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
            
            if (responseObject?.Choices == null || responseObject.Choices.Length == 0)
            {
                return new RoofColorResponse
                {
                    Color = "other",
                    Confidence = 0.0,
                    Explanation = "no ai result",
                    Method = "error"
                };
            }
            
            var llmResponse = responseObject.Choices[0].Message.Content;
            var roofColorResponse = JsonSerializer.Deserialize<RoofColorResponse>(llmResponse);
            
            if (roofColorResponse == null)
            {
                return new RoofColorResponse
                {
                    Color = "other",
                    Confidence = 0.0,
                    Explanation = "invalid response format",
                    Method = "error"
                };
            }
            
            return roofColorResponse;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error in GetRoofColorReSuggestionAsync: {ex.Message}");
            
            return new RoofColorResponse
            {
                Color = "other",
                Confidence = 0.0,
                Explanation = "error: " + ex.Message,
                Method = "error"
            };
        }
    }
    
    /// <inheritdoc />
    public int EstimateTokenUsage(RoofColorRequest request)
    {
        // Very rough estimation based on character count
        // In a real implementation, this would use a proper tokenizer
        var jsonString = JsonSerializer.Serialize(request);
        
        // Rough estimate: 1 token per 4 characters
        var estimatedTokens = jsonString.Length / 4;
        
        // Add tokens for system message and response
        estimatedTokens += 100;
        
        // Add tokens for image if present
        if (!string.IsNullOrEmpty(request.ImageSummary.ThumbnailBase64))
        {
            estimatedTokens += request.ImageSummary.ThumbnailBase64.Length / 6;
        }
        
        if (!string.IsNullOrEmpty(request.ImageSummary.FullImageBase64))
        {
            estimatedTokens += request.ImageSummary.FullImageBase64.Length / 6;
        }
        
        return estimatedTokens;
    }
}

/// <summary>
/// OpenAI API response structure
/// </summary>
internal class OpenAiResponse
{
    public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    
    public class Choice
    {
        public Message Message { get; set; } = new();
    }
    
    public class Message
    {
        public string Content { get; set; } = string.Empty;
    }
}