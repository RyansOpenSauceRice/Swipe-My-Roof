using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Service for managing validated building data
/// </summary>
public interface IValidationDataService
{
    /// <summary>
    /// Save a validated building to the database
    /// </summary>
    /// <param name="validatedBuilding">Building with validation data</param>
    /// <returns>Saved building with ID</returns>
    Task<ValidatedBuilding> SaveValidatedBuildingAsync(ValidatedBuilding validatedBuilding);
    
    /// <summary>
    /// Get a validated building by OSM ID
    /// </summary>
    /// <param name="osmId">OSM building ID</param>
    /// <returns>Validated building or null if not found</returns>
    Task<ValidatedBuilding?> GetValidatedBuildingAsync(long osmId);
    
    /// <summary>
    /// Check if a building has already been validated
    /// </summary>
    /// <param name="osmId">OSM building ID</param>
    /// <returns>True if already validated</returns>
    Task<bool> IsBuildingValidatedAsync(long osmId);
    
    /// <summary>
    /// Get all validated buildings in a geographic area
    /// </summary>
    /// <param name="minLat">Minimum latitude</param>
    /// <param name="maxLat">Maximum latitude</param>
    /// <param name="minLon">Minimum longitude</param>
    /// <param name="maxLon">Maximum longitude</param>
    /// <returns>List of validated buildings in area</returns>
    Task<List<ValidatedBuilding>> GetValidatedBuildingsInAreaAsync(double minLat, double maxLat, double minLon, double maxLon);
    
    /// <summary>
    /// Get validated buildings that haven't been uploaded to OSM yet
    /// </summary>
    /// <param name="limit">Maximum number of buildings to return</param>
    /// <returns>List of buildings pending upload</returns>
    Task<List<ValidatedBuilding>> GetPendingUploadBuildingsAsync(int limit = 100);
    
    /// <summary>
    /// Mark buildings as uploaded to OSM
    /// </summary>
    /// <param name="buildingIds">List of building IDs</param>
    /// <param name="changesetId">OSM changeset ID</param>
    /// <returns>Number of buildings updated</returns>
    Task<int> MarkBuildingsAsUploadedAsync(List<int> buildingIds, long changesetId);
    
    /// <summary>
    /// Get validation statistics
    /// </summary>
    /// <returns>Statistics about validated buildings</returns>
    Task<ValidationStatistics> GetValidationStatisticsAsync();
    
    /// <summary>
    /// Delete a validated building (if needed for corrections)
    /// </summary>
    /// <param name="osmId">OSM building ID</param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteValidatedBuildingAsync(long osmId);
    
    /// <summary>
    /// Update an existing validated building
    /// </summary>
    /// <param name="validatedBuilding">Updated building data</param>
    /// <returns>Updated building</returns>
    Task<ValidatedBuilding> UpdateValidatedBuildingAsync(ValidatedBuilding validatedBuilding);
    
    /// <summary>
    /// Get recent validation history for user feedback
    /// </summary>
    /// <param name="limit">Number of recent validations to return</param>
    /// <returns>Recent validated buildings</returns>
    Task<List<ValidatedBuilding>> GetRecentValidationsAsync(int limit = 20);
}

/// <summary>
/// Statistics about validated buildings
/// </summary>
public class ValidationStatistics
{
    /// <summary>
    /// Total number of buildings validated
    /// </summary>
    public int TotalValidated { get; set; }
    
    /// <summary>
    /// Number of buildings pending upload to OSM
    /// </summary>
    public int PendingUpload { get; set; }
    
    /// <summary>
    /// Number of buildings successfully uploaded to OSM
    /// </summary>
    public int UploadedToOsm { get; set; }
    
    /// <summary>
    /// Number of buildings validated today
    /// </summary>
    public int ValidatedToday { get; set; }
    
    /// <summary>
    /// Number of buildings validated this week
    /// </summary>
    public int ValidatedThisWeek { get; set; }
    
    /// <summary>
    /// Most common validation method
    /// </summary>
    public string? MostCommonMethod { get; set; }
    
    /// <summary>
    /// Average AI confidence score
    /// </summary>
    public double? AverageAiConfidence { get; set; }
}