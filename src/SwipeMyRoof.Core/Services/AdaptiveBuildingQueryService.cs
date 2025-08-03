using SwipeMyRoof.Core.Models;
using SwipeMyRoof.OSM.Models;
using SwipeMyRoof.OSM.Services;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Service for querying buildings with consistent limits and density analysis
/// </summary>
public interface IAdaptiveBuildingQueryService
{
    /// <summary>
    /// Get buildings with consistent 200 building limit and density analysis
    /// </summary>
    /// <param name="area">Area to query</param>
    /// <param name="targetCount">Target number of buildings (max 200)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query result with buildings and metadata</returns>
    Task<AdaptiveBuildingQueryResult> GetBuildingsAdaptivelyAsync(
        AreaSelection area, 
        int targetCount = 50, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Estimate building density for an area before querying
    /// </summary>
    /// <param name="area">Area to estimate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Density estimation result</returns>
    Task<BuildingDensityEstimate> EstimateBuildingDensityAsync(
        AreaSelection area, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get recommended query parameters for an area
    /// </summary>
    /// <param name="area">Area to analyze</param>
    /// <param name="targetCount">Desired number of buildings</param>
    /// <returns>Recommended query parameters</returns>
    Task<QueryRecommendation> GetQueryRecommendationAsync(AreaSelection area, int targetCount = 50);
}

/// <summary>
/// Result of adaptive building query
/// </summary>
public class AdaptiveBuildingQueryResult
{
    public List<OsmBuilding> Buildings { get; set; } = new();
    public int RequestedCount { get; set; }
    public int ActualCount { get; set; }
    public BuildingDensityLevel DensityLevel { get; set; }
    public double EstimatedDensity { get; set; }
    public bool WasLimited { get; set; }
    public string? LimitReason { get; set; }
    public TimeSpan QueryTime { get; set; }
    public QueryRecommendation? Recommendation { get; set; }
}

/// <summary>
/// Building density estimation for an area
/// </summary>
public class BuildingDensityEstimate
{
    public double BuildingsPerSquareKm { get; set; }
    public BuildingDensityLevel DensityLevel { get; set; }
    public double AreaSizeKm2 { get; set; }
    public int EstimatedTotalBuildings { get; set; }
    public bool IsHighDensity => DensityLevel >= BuildingDensityLevel.High;
    public bool RequiresLimiting => EstimatedTotalBuildings > 200;
}

/// <summary>
/// Query recommendation based on area analysis
/// </summary>
public class QueryRecommendation
{
    public int RecommendedLimit { get; set; }
    public double RecommendedRadiusKm { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public bool ShouldSplitArea { get; set; }
    public List<AreaSelection>? SuggestedSubAreas { get; set; }
    public BuildingDensityLevel DensityLevel { get; set; }
}

/// <summary>
/// Building density levels
/// </summary>
public enum BuildingDensityLevel
{
    VeryLow,    // Rural areas (< 10 buildings/km²)
    Low,        // Suburban areas (10-50 buildings/km²)
    Medium,     // Small towns (50-200 buildings/km²)
    High,       // Cities (200-1000 buildings/km²)
    VeryHigh,   // Dense urban (1000-5000 buildings/km²)
    Extreme     // Manhattan-level density (> 5000 buildings/km²)
}

/// <summary>
/// Implementation of building query service with consistent limits and density analysis
/// </summary>
public class AdaptiveBuildingQueryService : IAdaptiveBuildingQueryService
{
    private readonly IOverpassService _overpassService;
    private readonly Dictionary<string, BuildingDensityEstimate> _densityCache = new();
    
    // Density thresholds (buildings per km²)
    private static readonly Dictionary<BuildingDensityLevel, (double Min, double Max)> DensityThresholds = new()
    {
        { BuildingDensityLevel.VeryLow, (0, 10) },
        { BuildingDensityLevel.Low, (10, 50) },
        { BuildingDensityLevel.Medium, (50, 200) },
        { BuildingDensityLevel.High, (200, 1000) },
        { BuildingDensityLevel.VeryHigh, (1000, 5000) },
        { BuildingDensityLevel.Extreme, (5000, double.MaxValue) }
    };
    
    // Consistent query limit for all areas
    private const int MaxQueryLimit = 200;
    
    public AdaptiveBuildingQueryService(IOverpassService overpassService)
    {
        _overpassService = overpassService;
    }
    
    /// <inheritdoc />
    public async Task<AdaptiveBuildingQueryResult> GetBuildingsAdaptivelyAsync(
        AreaSelection area, 
        int targetCount = 50, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new AdaptiveBuildingQueryResult
        {
            RequestedCount = targetCount
        };
        
        try
        {
            // Get density estimate
            var densityEstimate = await EstimateBuildingDensityAsync(area, cancellationToken);
            result.EstimatedDensity = densityEstimate.BuildingsPerSquareKm;
            result.DensityLevel = densityEstimate.DensityLevel;
            
            // Get query recommendation
            var recommendation = await GetQueryRecommendationAsync(area, targetCount);
            result.Recommendation = recommendation;
            
            // Apply consistent limit of 200 buildings
            var actualLimit = Math.Min(targetCount, MaxQueryLimit);
            
            if (actualLimit < targetCount)
            {
                result.WasLimited = true;
                result.LimitReason = $"Limited to {actualLimit} buildings (maximum allowed per query)";
            }
            
            // Execute query with adaptive limit
            var buildings = await ExecuteQueryWithLimit(area, actualLimit, cancellationToken);
            
            result.Buildings = buildings;
            result.ActualCount = buildings.Count;
            result.QueryTime = DateTime.UtcNow - startTime;
            
            return result;
        }
        catch (Exception ex)
        {
            result.LimitReason = $"Query failed: {ex.Message}";
            result.QueryTime = DateTime.UtcNow - startTime;
            return result;
        }
    }
    
    /// <inheritdoc />
    public async Task<BuildingDensityEstimate> EstimateBuildingDensityAsync(
        AreaSelection area, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(area);
        
        if (_densityCache.TryGetValue(cacheKey, out var cachedEstimate))
        {
            return cachedEstimate;
        }
        
        var estimate = new BuildingDensityEstimate();
        
        try
        {
            // Calculate area size
            estimate.AreaSizeKm2 = CalculateAreaSizeKm2(area);
            
            // For density estimation, use a small sample query
            var sampleLimit = 10;
            var sampleBuildings = await ExecuteQueryWithLimit(area, sampleLimit, cancellationToken);
            
            if (sampleBuildings.Count >= sampleLimit)
            {
                // Area is dense enough that we hit our sample limit
                // Use a smaller area to estimate density
                var smallerArea = CreateSmallerSampleArea(area);
                var smallerAreaSize = CalculateAreaSizeKm2(smallerArea);
                var smallerSample = await ExecuteQueryWithLimit(smallerArea, sampleLimit, cancellationToken);
                
                if (smallerAreaSize > 0)
                {
                    estimate.BuildingsPerSquareKm = smallerSample.Count / smallerAreaSize;
                    estimate.EstimatedTotalBuildings = (int)(estimate.BuildingsPerSquareKm * estimate.AreaSizeKm2);
                }
            }
            else
            {
                // Use actual sample for density calculation
                estimate.BuildingsPerSquareKm = estimate.AreaSizeKm2 > 0 ? sampleBuildings.Count / estimate.AreaSizeKm2 : 0;
                estimate.EstimatedTotalBuildings = sampleBuildings.Count;
            }
            
            // Determine density level
            estimate.DensityLevel = DetermineDensityLevel(estimate.BuildingsPerSquareKm);
            
            // Cache the result
            _densityCache[cacheKey] = estimate;
            
            return estimate;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error estimating building density: {ex.Message}");
            
            // Return conservative estimate
            estimate.DensityLevel = BuildingDensityLevel.Medium;
            estimate.BuildingsPerSquareKm = 100; // Conservative default
            estimate.EstimatedTotalBuildings = (int)(100 * estimate.AreaSizeKm2);
            
            return estimate;
        }
    }
    
    /// <inheritdoc />
    public async Task<QueryRecommendation> GetQueryRecommendationAsync(AreaSelection area, int targetCount = 50)
    {
        var densityEstimate = await EstimateBuildingDensityAsync(area);
        var recommendation = new QueryRecommendation
        {
            DensityLevel = densityEstimate.DensityLevel
        };
        
        // Apply consistent 200 building limit
        recommendation.RecommendedLimit = Math.Min(targetCount, MaxQueryLimit);
        
        // Calculate recommended radius for circular areas
        if (area.Type == AreaSelectionType.Radius)
        {
            recommendation.RecommendedRadiusKm = CalculateOptimalRadius(densityEstimate, targetCount);
        }
        
        // Determine if area should be split for very large areas
        if (densityEstimate.EstimatedTotalBuildings > 1000)
        {
            recommendation.ShouldSplitArea = true;
            recommendation.SuggestedSubAreas = GenerateSubAreas(area, 4); // Split into 4 quadrants
        }
        
        // Generate reasoning
        recommendation.Reasoning = GenerateRecommendationReasoning(densityEstimate, targetCount, MaxQueryLimit);
        
        return recommendation;
    }
    
    private async Task<List<OsmBuilding>> ExecuteQueryWithLimit(
        AreaSelection area, 
        int limit, 
        CancellationToken cancellationToken)
    {
        return area.Type switch
        {
            AreaSelectionType.Radius when area.Center != null => 
                await _overpassService.GetBuildingsInRadiusAsync(area.Center, area.Radius, limit, true, cancellationToken),
            
            AreaSelectionType.Rectangle when area.BoundingBox != null => 
                await _overpassService.GetBuildingsInBoundingBoxAsync(area.BoundingBox, limit, true, cancellationToken),
            
            _ => new List<OsmBuilding>()
        };
    }
    
    private static double CalculateAreaSizeKm2(AreaSelection area)
    {
        return area.Type switch
        {
            AreaSelectionType.Radius => Math.PI * Math.Pow(area.Radius / 1000.0, 2), // Convert meters to km
            AreaSelectionType.Rectangle when area.BoundingBox != null => CalculateBoundingBoxAreaKm2(area.BoundingBox),
            _ => 0
        };
    }
    
    private static double CalculateBoundingBoxAreaKm2(BoundingBox bbox)
    {
        // Simplified calculation - for more accuracy, use proper geodesic calculations
        var latDiff = Math.Abs(bbox.North - bbox.South);
        var lonDiff = Math.Abs(bbox.East - bbox.West);
        
        // Approximate conversion to km (varies by latitude)
        var avgLat = (bbox.North + bbox.South) / 2;
        var latKm = latDiff * 111.32; // 1 degree latitude ≈ 111.32 km
        var lonKm = lonDiff * 111.32 * Math.Cos(avgLat * Math.PI / 180); // Longitude varies by latitude
        
        return latKm * lonKm;
    }
    
    private static AreaSelection CreateSmallerSampleArea(AreaSelection originalArea)
    {
        return originalArea.Type switch
        {
            AreaSelectionType.Radius => new AreaSelection
            {
                Type = AreaSelectionType.Radius,
                Center = originalArea.Center,
                Radius = Math.Min(originalArea.Radius * 0.1, 500) // 10% of original or 500m max
            },
            AreaSelectionType.Rectangle when originalArea.BoundingBox != null => CreateSmallerBoundingBox(originalArea.BoundingBox),
            _ => originalArea
        };
    }
    
    private static AreaSelection CreateSmallerBoundingBox(BoundingBox originalBox)
    {
        var centerLat = (originalBox.North + originalBox.South) / 2;
        var centerLon = (originalBox.East + originalBox.West) / 2;
        var latRange = (originalBox.North - originalBox.South) * 0.1; // 10% of original
        var lonRange = (originalBox.East - originalBox.West) * 0.1;
        
        return new AreaSelection
        {
            Type = AreaSelectionType.Rectangle,
            BoundingBox = new BoundingBox
            {
                North = centerLat + latRange / 2,
                South = centerLat - latRange / 2,
                East = centerLon + lonRange / 2,
                West = centerLon - lonRange / 2
            }
        };
    }
    
    private static BuildingDensityLevel DetermineDensityLevel(double buildingsPerKm2)
    {
        foreach (var (level, (min, max)) in DensityThresholds)
        {
            if (buildingsPerKm2 >= min && buildingsPerKm2 < max)
                return level;
        }
        return BuildingDensityLevel.Extreme;
    }
    
    private static double CalculateOptimalRadius(BuildingDensityEstimate density, int targetCount)
    {
        if (density.BuildingsPerSquareKm <= 0)
            return 1000; // Default 1km
        
        // Calculate radius needed for target count
        var neededAreaKm2 = targetCount / density.BuildingsPerSquareKm;
        var radiusKm = Math.Sqrt(neededAreaKm2 / Math.PI);
        
        // Clamp to reasonable bounds (100m to 10km)
        return Math.Max(0.1, Math.Min(10.0, radiusKm));
    }
    
    private static List<AreaSelection> GenerateSubAreas(AreaSelection originalArea, int subdivisions)
    {
        var subAreas = new List<AreaSelection>();
        
        if (originalArea.Type == AreaSelectionType.Rectangle && originalArea.BoundingBox != null)
        {
            var bbox = originalArea.BoundingBox;
            var latStep = (bbox.North - bbox.South) / 2;
            var lonStep = (bbox.East - bbox.West) / 2;
            
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    subAreas.Add(new AreaSelection
                    {
                        Type = AreaSelectionType.Rectangle,
                        BoundingBox = new BoundingBox
                        {
                            South = bbox.South + i * latStep,
                            North = bbox.South + (i + 1) * latStep,
                            West = bbox.West + j * lonStep,
                            East = bbox.West + (j + 1) * lonStep
                        }
                    });
                }
            }
        }
        
        return subAreas;
    }
    
    private static string GenerateRecommendationReasoning(
        BuildingDensityEstimate density, 
        int targetCount, 
        int maxLimit)
    {
        var reasoning = $"Area has {density.DensityLevel.ToString().ToLower()} building density " +
                       $"({density.BuildingsPerSquareKm:F0} buildings/km²). ";
        
        if (maxLimit < targetCount)
        {
            reasoning += $"Limited to {maxLimit} buildings (maximum per query). ";
        }
        
        if (density.EstimatedTotalBuildings > 1000)
        {
            reasoning += $"Estimated {density.EstimatedTotalBuildings} total buildings in area. ";
            reasoning += "Consider using smaller areas for better coverage.";
        }
        
        return reasoning;
    }
    
    private static string GenerateCacheKey(AreaSelection area)
    {
        return area.Type switch
        {
            AreaSelectionType.Radius => $"radius_{area.Center?.Latitude:F4}_{area.Center?.Longitude:F4}_{area.Radius}",
            AreaSelectionType.Rectangle when area.BoundingBox != null => 
                $"bbox_{area.BoundingBox.South:F4}_{area.BoundingBox.West:F4}_{area.BoundingBox.North:F4}_{area.BoundingBox.East:F4}",
            _ => Guid.NewGuid().ToString()
        };
    }
}