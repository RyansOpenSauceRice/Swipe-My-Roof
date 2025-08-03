using Microsoft.EntityFrameworkCore;
using SwipeMyRoof.Core.Data;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Service for initializing and managing the SQLite database
/// </summary>
public interface IDatabaseInitializationService
{
    /// <summary>
    /// Initialize the database, creating tables if they don't exist
    /// </summary>
    Task InitializeDatabaseAsync();
    
    /// <summary>
    /// Get the database file path for the current platform
    /// </summary>
    string GetDatabasePath();
}

/// <summary>
/// SQLite database initialization service
/// </summary>
public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly ValidationDbContext _context;
    
    public DatabaseInitializationService(ValidationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Initialize the SQLite database, creating tables if they don't exist
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
            
            // Run any pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize SQLite database", ex);
        }
    }
    
    /// <summary>
    /// Get the platform-appropriate database file path
    /// </summary>
    public string GetDatabasePath()
    {
        // For Android: Use app's private data directory
        // For desktop: Use user's app data directory
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "SwipeMyRoof");
        
        // Ensure directory exists
        Directory.CreateDirectory(appFolder);
        
        return Path.Combine(appFolder, "validations.db");
    }
}