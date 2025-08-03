using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Storage.Models;

namespace SwipeMyRoof.Storage.Services;

/// <summary>
/// Interface for storage service operations
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Create a new validation session
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isPracticeMode">Whether this is a practice session</param>
    /// <param name="areaSelection">Area selection</param>
    /// <returns>The created session</returns>
    Task<ValidationSession> CreateSessionAsync(string userId, bool isPracticeMode, AreaSelection areaSelection);
    
    /// <summary>
    /// Get a validation session by ID
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>The session, or null if not found</returns>
    Task<ValidationSession?> GetSessionAsync(string sessionId);
    
    /// <summary>
    /// Get all validation sessions for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of sessions</returns>
    Task<List<ValidationSession>> GetSessionsForUserAsync(string userId);
    
    /// <summary>
    /// Add a building candidate to a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="buildingCandidate">Building candidate</param>
    /// <returns>True if successful</returns>
    Task<bool> AddBuildingCandidateAsync(string sessionId, BuildingCandidate buildingCandidate);
    
    /// <summary>
    /// Update a building candidate in a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="buildingCandidate">Building candidate</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateBuildingCandidateAsync(string sessionId, BuildingCandidate buildingCandidate);
    
    /// <summary>
    /// Update session statistics
    /// </summary>
    /// <param name="session">Session to update</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateSessionStatisticsAsync(ValidationSession session);
    
    /// <summary>
    /// Add an item to the upload queue
    /// </summary>
    /// <param name="buildingCandidate">Building candidate</param>
    /// <returns>The created queue item</returns>
    Task<UploadQueueItem> AddToUploadQueueAsync(BuildingCandidate buildingCandidate);
    
    /// <summary>
    /// Get all items in the upload queue
    /// </summary>
    /// <returns>List of queue items</returns>
    Task<List<UploadQueueItem>> GetUploadQueueAsync();
    
    /// <summary>
    /// Update an upload queue item
    /// </summary>
    /// <param name="item">Upload queue item</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateUploadQueueItemAsync(UploadQueueItem item);
    
    /// <summary>
    /// Remove an item from the upload queue
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <returns>True if successful</returns>
    Task<bool> RemoveFromUploadQueueAsync(string itemId);
    
    /// <summary>
    /// Get the next item from the upload queue
    /// </summary>
    /// <returns>The next queue item, or null if queue is empty</returns>
    Task<UploadQueueItem?> GetNextUploadQueueItemAsync();
    
    /// <summary>
    /// Update the reliability score for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="score">Reliability score</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateReliabilityScoreAsync(string userId, double score);
    
    /// <summary>
    /// Get the reliability score for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Reliability score</returns>
    Task<double> GetReliabilityScoreAsync(string userId);
}