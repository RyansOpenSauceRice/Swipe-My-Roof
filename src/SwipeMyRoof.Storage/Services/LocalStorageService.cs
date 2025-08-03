using System.Text.Json;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Storage.Models;

namespace SwipeMyRoof.Storage.Services;

/// <summary>
/// Local storage implementation of the storage service
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _sessionStoragePath;
    private readonly string _uploadQueueStoragePath;
    private readonly string _userStoragePath;
    
    private readonly Dictionary<string, ValidationSession> _sessions = new();
    private readonly List<UploadQueueItem> _uploadQueue = new();
    private readonly Dictionary<string, double> _reliabilityScores = new();
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="storagePath">Base storage path</param>
    public LocalStorageService(string storagePath = "storage")
    {
        _sessionStoragePath = Path.Combine(storagePath, "sessions");
        _uploadQueueStoragePath = Path.Combine(storagePath, "upload_queue");
        _userStoragePath = Path.Combine(storagePath, "users");
        
        // Create directories if they don't exist
        Directory.CreateDirectory(_sessionStoragePath);
        Directory.CreateDirectory(_uploadQueueStoragePath);
        Directory.CreateDirectory(_userStoragePath);
        
        // Load data from disk
        LoadData();
    }
    
    /// <inheritdoc />
    public async Task<ValidationSession> CreateSessionAsync(string userId, bool isPracticeMode, AreaSelection areaSelection)
    {
        var session = new ValidationSession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            StartTime = DateTime.UtcNow,
            IsPracticeMode = isPracticeMode,
            AreaSelection = areaSelection
        };
        
        _sessions[session.Id] = session;
        
        await SaveSessionAsync(session);
        
        return session;
    }
    
    /// <inheritdoc />
    public Task<ValidationSession?> GetSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<ValidationSession?>(session);
        }
        
        return Task.FromResult<ValidationSession?>(null);
    }
    
    /// <inheritdoc />
    public Task<List<ValidationSession>> GetSessionsForUserAsync(string userId)
    {
        var sessions = _sessions.Values
            .Where(s => s.UserId == userId)
            .ToList();
        
        return Task.FromResult(sessions);
    }
    
    /// <inheritdoc />
    public async Task<bool> AddBuildingCandidateAsync(string sessionId, BuildingCandidate buildingCandidate)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }
        
        session.BuildingCandidates.Add(buildingCandidate);
        
        await SaveSessionAsync(session);
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateBuildingCandidateAsync(string sessionId, BuildingCandidate buildingCandidate)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }
        
        var existingIndex = session.BuildingCandidates.FindIndex(b => b.OsmId == buildingCandidate.OsmId);
        if (existingIndex == -1)
        {
            return false;
        }
        
        session.BuildingCandidates[existingIndex] = buildingCandidate;
        
        // Update session statistics
        UpdateSessionStatistics(session);
        
        await SaveSessionAsync(session);
        
        return true;
    }
    
    /// <summary>
    /// Update session statistics
    /// </summary>
    /// <param name="session">Session to update</param>
    /// <returns>True if successful</returns>
    public async Task<bool> UpdateSessionStatisticsAsync(ValidationSession session)
    {
        UpdateSessionStatistics(session);
        await SaveSessionAsync(session);
        return true;
    }
    
    /// <inheritdoc />
    public async Task<UploadQueueItem> AddToUploadQueueAsync(BuildingCandidate buildingCandidate)
    {
        var item = new UploadQueueItem
        {
            Id = Guid.NewGuid().ToString(),
            BuildingCandidate = buildingCandidate,
            CreationTime = DateTime.UtcNow,
            Status = UploadStatus.Staged
        };
        
        _uploadQueue.Add(item);
        
        await SaveUploadQueueAsync();
        
        return item;
    }
    
    /// <inheritdoc />
    public Task<List<UploadQueueItem>> GetUploadQueueAsync()
    {
        return Task.FromResult(_uploadQueue.ToList());
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateUploadQueueItemAsync(UploadQueueItem item)
    {
        var existingIndex = _uploadQueue.FindIndex(i => i.Id == item.Id);
        if (existingIndex == -1)
        {
            return false;
        }
        
        _uploadQueue[existingIndex] = item;
        
        await SaveUploadQueueAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> RemoveFromUploadQueueAsync(string itemId)
    {
        var existingIndex = _uploadQueue.FindIndex(i => i.Id == itemId);
        if (existingIndex == -1)
        {
            return false;
        }
        
        _uploadQueue.RemoveAt(existingIndex);
        
        await SaveUploadQueueAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public Task<UploadQueueItem?> GetNextUploadQueueItemAsync()
    {
        var item = _uploadQueue
            .Where(i => i.Status == UploadStatus.Staged)
            .OrderBy(i => i.CreationTime)
            .FirstOrDefault();
        
        return Task.FromResult(item);
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateReliabilityScoreAsync(string userId, double score)
    {
        _reliabilityScores[userId] = score;
        
        await SaveReliabilityScoreAsync(userId, score);
        
        return true;
    }
    
    /// <inheritdoc />
    public Task<double> GetReliabilityScoreAsync(string userId)
    {
        if (_reliabilityScores.TryGetValue(userId, out var score))
        {
            return Task.FromResult(score);
        }
        
        return Task.FromResult(1.0); // Default score
    }
    
    #region Private Methods
    
    private void LoadData()
    {
        // Load sessions
        foreach (var file in Directory.GetFiles(_sessionStoragePath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var session = JsonSerializer.Deserialize<ValidationSession>(json);
                
                if (session != null)
                {
                    _sessions[session.Id] = session;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading session: {ex.Message}");
            }
        }
        
        // Load upload queue
        try
        {
            var queueFile = Path.Combine(_uploadQueueStoragePath, "queue.json");
            if (File.Exists(queueFile))
            {
                var json = File.ReadAllText(queueFile);
                var queue = JsonSerializer.Deserialize<List<UploadQueueItem>>(json);
                
                if (queue != null)
                {
                    _uploadQueue.Clear();
                    _uploadQueue.AddRange(queue);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading upload queue: {ex.Message}");
        }
        
        // Load reliability scores
        foreach (var file in Directory.GetFiles(_userStoragePath, "*.json"))
        {
            try
            {
                var userId = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);
                var userData = JsonSerializer.Deserialize<UserData>(json);
                
                if (userData != null)
                {
                    _reliabilityScores[userId] = userData.ReliabilityScore;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user data: {ex.Message}");
            }
        }
    }
    
    private async Task SaveSessionAsync(ValidationSession session)
    {
        try
        {
            var json = JsonSerializer.Serialize(session);
            var filePath = Path.Combine(_sessionStoragePath, $"{session.Id}.json");
            
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving session: {ex.Message}");
        }
    }
    
    private async Task SaveUploadQueueAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_uploadQueue);
            var filePath = Path.Combine(_uploadQueueStoragePath, "queue.json");
            
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving upload queue: {ex.Message}");
        }
    }
    
    private async Task SaveReliabilityScoreAsync(string userId, double score)
    {
        try
        {
            var userData = new UserData
            {
                UserId = userId,
                ReliabilityScore = score
            };
            
            var json = JsonSerializer.Serialize(userData);
            var filePath = Path.Combine(_userStoragePath, $"{userId}.json");
            
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving reliability score: {ex.Message}");
        }
    }
    
    private void UpdateSessionStatistics(ValidationSession session)
    {
        session.AcceptedCount = session.BuildingCandidates.Count(b => b.UserFeedback == UserFeedback.Accepted);
        session.RejectedCount = session.BuildingCandidates.Count(b => b.UserFeedback == UserFeedback.Rejected);
        session.SkippedCount = session.BuildingCandidates.Count(b => b.UserFeedback == UserFeedback.Skipped);
        session.CorrectedCount = session.BuildingCandidates.Count(b => b.UserFeedback == UserFeedback.Corrected);
        
        session.DecoyCorrectCount = session.BuildingCandidates.Count(b => 
            b.ProposedColor?.IsDecoy == true && 
            (b.UserFeedback == UserFeedback.Rejected || b.UserFeedback == UserFeedback.Skipped));
        
        session.DecoyIncorrectCount = session.BuildingCandidates.Count(b => 
            b.ProposedColor?.IsDecoy == true && 
            b.UserFeedback == UserFeedback.Accepted);
        
        // Calculate reliability score
        if (session.DecoyCorrectCount + session.DecoyIncorrectCount > 0)
        {
            session.ReliabilityScore = (double)session.DecoyCorrectCount / (session.DecoyCorrectCount + session.DecoyIncorrectCount);
        }
    }
    
    #endregion
}

/// <summary>
/// User data for storage
/// </summary>
internal class UserData
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Reliability score
    /// </summary>
    public double ReliabilityScore { get; set; } = 1.0;
}