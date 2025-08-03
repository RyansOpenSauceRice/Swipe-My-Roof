using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.Storage.Models;

/// <summary>
/// Represents a session for roof color validation
/// </summary>
public class ValidationSession
{
    /// <summary>
    /// Session ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Session start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Session end time (null if session is active)
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Whether this is a practice session
    /// </summary>
    public bool IsPracticeMode { get; set; }
    
    /// <summary>
    /// Area selection for the session
    /// </summary>
    public AreaSelection AreaSelection { get; set; } = new();
    
    /// <summary>
    /// List of building candidates in the session
    /// </summary>
    public List<BuildingCandidate> BuildingCandidates { get; set; } = new();
    
    /// <summary>
    /// Number of buildings accepted
    /// </summary>
    public int AcceptedCount { get; set; }
    
    /// <summary>
    /// Number of buildings rejected
    /// </summary>
    public int RejectedCount { get; set; }
    
    /// <summary>
    /// Number of buildings skipped
    /// </summary>
    public int SkippedCount { get; set; }
    
    /// <summary>
    /// Number of buildings corrected
    /// </summary>
    public int CorrectedCount { get; set; }
    
    /// <summary>
    /// Number of decoys correctly identified
    /// </summary>
    public int DecoyCorrectCount { get; set; }
    
    /// <summary>
    /// Number of decoys incorrectly accepted
    /// </summary>
    public int DecoyIncorrectCount { get; set; }
    
    /// <summary>
    /// Reliability score (0.0-1.0)
    /// </summary>
    public double ReliabilityScore { get; set; } = 1.0;
}

/// <summary>
/// Represents an area selection for a session
/// </summary>
public class AreaSelection
{
    /// <summary>
    /// Selection type
    /// </summary>
    public string Type { get; set; } = "Radius";
    
    /// <summary>
    /// Center latitude (for radius selection)
    /// </summary>
    public double? CenterLat { get; set; }
    
    /// <summary>
    /// Center longitude (for radius selection)
    /// </summary>
    public double? CenterLon { get; set; }
    
    /// <summary>
    /// Radius in meters (for radius selection)
    /// </summary>
    public double? Radius { get; set; }
    
    /// <summary>
    /// Minimum latitude (for rectangle selection)
    /// </summary>
    public double? MinLat { get; set; }
    
    /// <summary>
    /// Minimum longitude (for rectangle selection)
    /// </summary>
    public double? MinLon { get; set; }
    
    /// <summary>
    /// Maximum latitude (for rectangle selection)
    /// </summary>
    public double? MaxLat { get; set; }
    
    /// <summary>
    /// Maximum longitude (for rectangle selection)
    /// </summary>
    public double? MaxLon { get; set; }
    
    /// <summary>
    /// City name (for city selection)
    /// </summary>
    public string? CityName { get; set; }
}

/// <summary>
/// Represents an upload queue item
/// </summary>
public class UploadQueueItem
{
    /// <summary>
    /// Item ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Building candidate
    /// </summary>
    public BuildingCandidate BuildingCandidate { get; set; } = new();
    
    /// <summary>
    /// Creation time
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last attempt time
    /// </summary>
    public DateTime? LastAttemptTime { get; set; }
    
    /// <summary>
    /// Number of attempts
    /// </summary>
    public int AttemptCount { get; set; }
    
    /// <summary>
    /// Upload status
    /// </summary>
    public UploadStatus Status { get; set; } = UploadStatus.Staged;
    
    /// <summary>
    /// Error message (if any)
    /// </summary>
    public string? ErrorMessage { get; set; }
}
