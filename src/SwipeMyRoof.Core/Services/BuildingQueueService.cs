using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;
using SwipeMyRoof.OSM.Services;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Service for managing a queue of buildings for validation with adaptive query limits
/// </summary>
public class BuildingQueueService : IBuildingQueueService
{
    private readonly IAdaptiveBuildingQueryService _adaptiveQueryService;
    private readonly Queue<BuildingCandidate> _buildingQueue;
    private readonly HashSet<long> _processedBuildingIds;
    private AreaSelection? _currentArea;
    private int _processedCount;
    private const int MaxQueueSize = 20;
    private const int RefillThreshold = 5;
    private BuildingDensityLevel? _areaDensityLevel;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="adaptiveQueryService">Adaptive query service for smart building queries</param>
    public BuildingQueueService(IAdaptiveBuildingQueryService adaptiveQueryService)
    {
        _adaptiveQueryService = adaptiveQueryService;
        _buildingQueue = new Queue<BuildingCandidate>();
        _processedBuildingIds = new HashSet<long>();
        _processedCount = 0;
    }
    
    /// <inheritdoc />
    public event EventHandler? QueueRunningLow;
    
    /// <inheritdoc />
    public event EventHandler<BuildingProcessedEventArgs>? BuildingProcessed;
    
    /// <inheritdoc />
    public async Task<bool> InitializeQueueAsync(AreaSelection area, CancellationToken cancellationToken = default)
    {
        try
        {
            _currentArea = area;
            ClearQueue();
            
            // Use adaptive query service to get buildings with smart limits
            var queryResult = await _adaptiveQueryService.GetBuildingsAdaptivelyAsync(area, MaxQueueSize, cancellationToken);
            _areaDensityLevel = queryResult.DensityLevel;
            
            // Log query information for user awareness
            if (queryResult.WasLimited)
            {
                Console.WriteLine($"Query limited: {queryResult.LimitReason}");
                Console.WriteLine($"Density: {queryResult.DensityLevel} ({queryResult.EstimatedDensity:F0} buildings/km²)");
            }
            
            foreach (var building in queryResult.Buildings)
            {
                var candidate = ConvertToBuildingCandidate(building);
                _buildingQueue.Enqueue(candidate);
            }
            
            return _buildingQueue.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing building queue: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<BuildingCandidate?> GetNextBuildingAsync()
    {
        // Check if we need to refill the queue
        if (_buildingQueue.Count <= RefillThreshold && _currentArea != null)
        {
            QueueRunningLow?.Invoke(this, EventArgs.Empty);
            await RefillQueueAsync();
        }
        
        return _buildingQueue.Count > 0 ? _buildingQueue.Dequeue() : null;
    }
    
    /// <inheritdoc />
    public async Task<bool> MarkBuildingProcessedAsync(long buildingId, UserFeedback feedback)
    {
        _processedBuildingIds.Add(buildingId);
        _processedCount++;
        
        var args = new BuildingProcessedEventArgs
        {
            BuildingId = buildingId,
            Feedback = feedback,
            RemainingInQueue = _buildingQueue.Count,
            TotalProcessed = _processedCount
        };
        
        BuildingProcessed?.Invoke(this, args);
        
        return await Task.FromResult(true);
    }
    
    /// <inheritdoc />
    public int GetQueueSize()
    {
        return _buildingQueue.Count;
    }
    
    /// <inheritdoc />
    public int GetProcessedCount()
    {
        return _processedCount;
    }
    
    /// <inheritdoc />
    public async Task<bool> RefillQueueAsync(CancellationToken cancellationToken = default)
    {
        if (_currentArea == null)
            return false;
        
        try
        {
            var neededBuildings = MaxQueueSize - _buildingQueue.Count;
            if (neededBuildings <= 0)
                return true;
            
            // Use adaptive query to fetch more buildings with smart limits
            var queryResult = await _adaptiveQueryService.GetBuildingsAdaptivelyAsync(_currentArea, neededBuildings * 2, cancellationToken);
            var buildings = queryResult.Buildings;
            
            var addedCount = 0;
            foreach (var building in buildings)
            {
                // Skip buildings we've already processed
                if (_processedBuildingIds.Contains(building.Id))
                    continue;
                
                // Skip buildings already in queue
                if (_buildingQueue.Any(b => b.OsmId == building.Id))
                    continue;
                
                var candidate = ConvertToBuildingCandidate(building);
                _buildingQueue.Enqueue(candidate);
                addedCount++;
                
                if (addedCount >= neededBuildings)
                    break;
            }
            
            return addedCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refilling building queue: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public void ClearQueue()
    {
        _buildingQueue.Clear();
        _processedBuildingIds.Clear();
        _processedCount = 0;
    }
    
    /// <summary>
    /// Get information about the current area's building density
    /// </summary>
    /// <returns>Density level of current area, or null if no area set</returns>
    public BuildingDensityLevel? GetAreaDensityLevel()
    {
        return _areaDensityLevel;
    }
    
    private static BuildingCandidate ConvertToBuildingCandidate(OsmBuilding building)
    {
        return new BuildingCandidate
        {
            OsmId = building.Id,
            Location = building.Location,
            BoundingBox = building.BoundingBox,
            ExistingRoofColor = building.RoofColor,
            SessionId = Guid.NewGuid().ToString(),
            UploadStatus = UploadStatus.NotStaged
        };
    }
}