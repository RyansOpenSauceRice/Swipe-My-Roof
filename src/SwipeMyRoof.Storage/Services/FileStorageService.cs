using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SwipeMyRoof.Core.Models;
using SwipeMyRoof.Storage.Models;

namespace SwipeMyRoof.Storage.Services;

/// <summary>
/// File-based implementation of the storage service
/// </summary>
public class FileStorageService : IStorageService
{
    private readonly string _baseDirectory;
    private readonly string _keyValueDirectory;
    private readonly string _sessionsDirectory;
    private readonly string _buildingsDirectory;
    private readonly string _uploadQueueDirectory;
    private readonly string _usersDirectory;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public FileStorageService()
    {
        // Get the app data directory
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _baseDirectory = Path.Combine(appDataDir, "SwipeMyRoof");
        
        // Create subdirectories
        _keyValueDirectory = Path.Combine(_baseDirectory, "KeyValue");
        _sessionsDirectory = Path.Combine(_baseDirectory, "Sessions");
        _buildingsDirectory = Path.Combine(_baseDirectory, "Buildings");
        _uploadQueueDirectory = Path.Combine(_baseDirectory, "UploadQueue");
        _usersDirectory = Path.Combine(_baseDirectory, "Users");
        
        // Ensure directories exist
        Directory.CreateDirectory(_baseDirectory);
        Directory.CreateDirectory(_keyValueDirectory);
        Directory.CreateDirectory(_sessionsDirectory);
        Directory.CreateDirectory(_buildingsDirectory);
        Directory.CreateDirectory(_uploadQueueDirectory);
        Directory.CreateDirectory(_usersDirectory);
    }
    
    #region Key-Value Storage
    
    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        var path = GetKeyValuePath(key);
        if (!File.Exists(path))
        {
            return default;
        }
        
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting value for key {key}: {ex.Message}");
            return default;
        }
    }
    
    /// <inheritdoc />
    public void Set<T>(string key, T value)
    {
        var path = GetKeyValuePath(key);
        try
        {
            var json = JsonSerializer.Serialize(value);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting value for key {key}: {ex.Message}");
        }
    }
    
    /// <inheritdoc />
    public void Delete(string key)
    {
        var path = GetKeyValuePath(key);
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting key {key}: {ex.Message}");
            }
        }
    }
    
    /// <inheritdoc />
    public bool Exists(string key)
    {
        var path = GetKeyValuePath(key);
        return File.Exists(path);
    }
    
    private string GetKeyValuePath(string key)
    {
        // Sanitize the key to make it a valid filename
        var sanitizedKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_keyValueDirectory, $"{sanitizedKey}.json");
    }
    
    #endregion
    
    #region Session Storage
    
    /// <inheritdoc />
    public async Task<ValidationSession> CreateSessionAsync(string userId, bool isPracticeMode, AreaSelection areaSelection)
    {
        var session = new ValidationSession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            IsPracticeMode = isPracticeMode,
            AreaSelection = areaSelection,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            BuildingCandidateIds = new List<string>(),
            Statistics = new SessionStatistics()
        };
        
        await SaveSessionAsync(session);
        return session;
    }
    
    /// <inheritdoc />
    public async Task<ValidationSession?> GetSessionAsync(string sessionId)
    {
        var path = GetSessionPath(sessionId);
        if (!File.Exists(path))
        {
            return null;
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<ValidationSession>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting session {sessionId}: {ex.Message}");
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<List<ValidationSession>> GetSessionsForUserAsync(string userId)
    {
        var sessions = new List<ValidationSession>();
        
        try
        {
            var files = Directory.GetFiles(_sessionsDirectory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var session = JsonSerializer.Deserialize<ValidationSession>(json);
                    if (session != null && session.UserId == userId)
                    {
                        sessions.Add(session);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading session file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting sessions for user {userId}: {ex.Message}");
        }
        
        return sessions;
    }
    
    /// <inheritdoc />
    public async Task<bool> AddBuildingCandidateAsync(string sessionId, BuildingCandidate buildingCandidate)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return false;
        }
        
        // Save the building candidate
        await SaveBuildingCandidateAsync(buildingCandidate);
        
        // Update the session
        session.BuildingCandidateIds.Add(buildingCandidate.SessionId);
        session.LastUpdatedAt = DateTime.UtcNow;
        await SaveSessionAsync(session);
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateBuildingCandidateAsync(string sessionId, BuildingCandidate buildingCandidate)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null || !session.BuildingCandidateIds.Contains(buildingCandidate.SessionId))
        {
            return false;
        }
        
        // Save the building candidate
        await SaveBuildingCandidateAsync(buildingCandidate);
        
        // Update the session
        session.LastUpdatedAt = DateTime.UtcNow;
        await SaveSessionAsync(session);
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateSessionStatisticsAsync(ValidationSession session)
    {
        session.LastUpdatedAt = DateTime.UtcNow;
        await SaveSessionAsync(session);
        return true;
    }
    
    private async Task SaveSessionAsync(ValidationSession session)
    {
        var path = GetSessionPath(session.Id);
        try
        {
            var json = JsonSerializer.Serialize(session);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving session {session.Id}: {ex.Message}");
        }
    }
    
    private string GetSessionPath(string sessionId)
    {
        return Path.Combine(_sessionsDirectory, $"{sessionId}.json");
    }
    
    private async Task SaveBuildingCandidateAsync(BuildingCandidate buildingCandidate)
    {
        var path = GetBuildingCandidatePath(buildingCandidate.SessionId);
        try
        {
            var json = JsonSerializer.Serialize(buildingCandidate);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving building candidate {buildingCandidate.SessionId}: {ex.Message}");
        }
    }
    
    private string GetBuildingCandidatePath(string buildingId)
    {
        return Path.Combine(_buildingsDirectory, $"{buildingId}.json");
    }
    
    #endregion
    
    #region Upload Queue
    
    /// <inheritdoc />
    public async Task<UploadQueueItem> AddToUploadQueueAsync(BuildingCandidate buildingCandidate)
    {
        var item = new UploadQueueItem
        {
            Id = Guid.NewGuid().ToString(),
            BuildingCandidate = buildingCandidate,
            Status = UploadStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
        
        await SaveUploadQueueItemAsync(item);
        return item;
    }
    
    /// <inheritdoc />
    public async Task<List<UploadQueueItem>> GetUploadQueueAsync()
    {
        var items = new List<UploadQueueItem>();
        
        try
        {
            var files = Directory.GetFiles(_uploadQueueDirectory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var item = JsonSerializer.Deserialize<UploadQueueItem>(json);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading upload queue item file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting upload queue: {ex.Message}");
        }
        
        return items;
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdateUploadQueueItemAsync(UploadQueueItem item)
    {
        item.LastUpdatedAt = DateTime.UtcNow;
        await SaveUploadQueueItemAsync(item);
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> RemoveFromUploadQueueAsync(string itemId)
    {
        var path = GetUploadQueueItemPath(itemId);
        if (!File.Exists(path))
        {
            return false;
        }
        
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing upload queue item {itemId}: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<UploadQueueItem?> GetNextUploadQueueItemAsync()
    {
        var items = await GetUploadQueueAsync();
        return items.Find(i => i.Status == UploadStatus.Pending);
    }
    
    private async Task SaveUploadQueueItemAsync(UploadQueueItem item)
    {
        var path = GetUploadQueueItemPath(item.Id);
        try
        {
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving upload queue item {item.Id}: {ex.Message}");
        }
    }
    
    private string GetUploadQueueItemPath(string itemId)
    {
        return Path.Combine(_uploadQueueDirectory, $"{itemId}.json");
    }
    
    #endregion
    
    #region User Reliability
    
    /// <inheritdoc />
    public async Task<bool> UpdateReliabilityScoreAsync(string userId, double score)
    {
        var path = GetUserReliabilityPath(userId);
        try
        {
            await File.WriteAllTextAsync(path, score.ToString());
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating reliability score for user {userId}: {ex.Message}");
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<double> GetReliabilityScoreAsync(string userId)
    {
        var path = GetUserReliabilityPath(userId);
        if (!File.Exists(path))
        {
            return 0.5; // Default reliability score
        }
        
        try
        {
            var scoreText = await File.ReadAllTextAsync(path);
            if (double.TryParse(scoreText, out var score))
            {
                return score;
            }
            
            return 0.5; // Default reliability score
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting reliability score for user {userId}: {ex.Message}");
            return 0.5; // Default reliability score
        }
    }
    
    private string GetUserReliabilityPath(string userId)
    {
        return Path.Combine(_usersDirectory, $"{userId}_reliability.txt");
    }
    
    #endregion
}