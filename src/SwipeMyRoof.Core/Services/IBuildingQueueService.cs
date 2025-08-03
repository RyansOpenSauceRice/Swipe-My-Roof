using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Interface for managing a queue of buildings for validation
/// </summary>
public interface IBuildingQueueService
{
    /// <summary>
    /// Initialize the queue with buildings from the specified area
    /// </summary>
    /// <param name="area">Area to search for buildings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeQueueAsync(AreaSelection area, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the next building from the queue
    /// </summary>
    /// <returns>Next building candidate, or null if queue is empty</returns>
    Task<BuildingCandidate?> GetNextBuildingAsync();
    
    /// <summary>
    /// Mark a building as processed and remove it from the queue
    /// </summary>
    /// <param name="buildingId">OSM ID of the building</param>
    /// <param name="feedback">User feedback</param>
    /// <returns>True if building was found and marked</returns>
    Task<bool> MarkBuildingProcessedAsync(long buildingId, UserFeedback feedback);
    
    /// <summary>
    /// Get the current queue size
    /// </summary>
    /// <returns>Number of buildings in the queue</returns>
    int GetQueueSize();
    
    /// <summary>
    /// Get the number of buildings processed in the current session
    /// </summary>
    /// <returns>Number of processed buildings</returns>
    int GetProcessedCount();
    
    /// <summary>
    /// Refill the queue if it's running low
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if refill was successful</returns>
    Task<bool> RefillQueueAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear the queue and reset counters
    /// </summary>
    void ClearQueue();
    
    /// <summary>
    /// Event fired when the queue is running low (< 5 buildings)
    /// </summary>
    event EventHandler? QueueRunningLow;
    
    /// <summary>
    /// Event fired when a building is processed
    /// </summary>
    event EventHandler<BuildingProcessedEventArgs>? BuildingProcessed;
}

/// <summary>
/// Event arguments for building processed event
/// </summary>
public class BuildingProcessedEventArgs : EventArgs
{
    public long BuildingId { get; set; }
    public UserFeedback Feedback { get; set; }
    public int RemainingInQueue { get; set; }
    public int TotalProcessed { get; set; }
}