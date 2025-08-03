using Microsoft.EntityFrameworkCore;
using SwipeMyRoof.Core.Data;
using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.Core.Services;

/// <summary>
/// Implementation of validation data service using Entity Framework
/// </summary>
public class ValidationDataService : IValidationDataService
{
    private readonly ValidationDbContext _context;
    
    public ValidationDataService(ValidationDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<ValidatedBuilding> SaveValidatedBuildingAsync(ValidatedBuilding validatedBuilding)
    {
        // Check if building already exists
        var existing = await _context.ValidatedBuildings
            .FirstOrDefaultAsync(b => b.OsmId == validatedBuilding.OsmId);
        
        if (existing != null)
        {
            // Update existing building
            existing.RoofColorHex = validatedBuilding.RoofColorHex;
            existing.ColorDescription = validatedBuilding.ColorDescription;
            existing.ValidationMethod = validatedBuilding.ValidationMethod;
            existing.AiConfidence = validatedBuilding.AiConfidence;
            existing.PickedPixelCoordinates = validatedBuilding.PickedPixelCoordinates;
            existing.ValidatedAt = DateTime.UtcNow;
            existing.ValidatedBy = validatedBuilding.ValidatedBy;
            existing.Notes = validatedBuilding.Notes;
            existing.UploadedToOsm = false; // Reset upload status since data changed
            existing.UploadedAt = null;
            existing.OsmChangesetId = null;
            
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Add new building
            _context.ValidatedBuildings.Add(validatedBuilding);
            await _context.SaveChangesAsync();
            return validatedBuilding;
        }
    }
    
    /// <inheritdoc />
    public async Task<ValidatedBuilding?> GetValidatedBuildingAsync(long osmId)
    {
        return await _context.ValidatedBuildings
            .FirstOrDefaultAsync(b => b.OsmId == osmId);
    }
    
    /// <inheritdoc />
    public async Task<bool> IsBuildingValidatedAsync(long osmId)
    {
        return await _context.ValidatedBuildings
            .AnyAsync(b => b.OsmId == osmId);
    }
    
    /// <inheritdoc />
    public async Task<List<ValidatedBuilding>> GetValidatedBuildingsInAreaAsync(double minLat, double maxLat, double minLon, double maxLon)
    {
        return await _context.ValidatedBuildings
            .Where(b => b.Latitude >= minLat && b.Latitude <= maxLat &&
                       b.Longitude >= minLon && b.Longitude <= maxLon)
            .OrderBy(b => b.ValidatedAt)
            .ToListAsync();
    }
    
    /// <inheritdoc />
    public async Task<List<ValidatedBuilding>> GetPendingUploadBuildingsAsync(int limit = 100)
    {
        return await _context.ValidatedBuildings
            .Where(b => !b.UploadedToOsm)
            .OrderBy(b => b.ValidatedAt)
            .Take(limit)
            .ToListAsync();
    }
    
    /// <inheritdoc />
    public async Task<int> MarkBuildingsAsUploadedAsync(List<int> buildingIds, long changesetId)
    {
        var buildings = await _context.ValidatedBuildings
            .Where(b => buildingIds.Contains(b.Id))
            .ToListAsync();
        
        foreach (var building in buildings)
        {
            building.UploadedToOsm = true;
            building.UploadedAt = DateTime.UtcNow;
            building.OsmChangesetId = changesetId;
        }
        
        await _context.SaveChangesAsync();
        return buildings.Count;
    }
    
    /// <inheritdoc />
    public async Task<ValidationStatistics> GetValidationStatisticsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        
        var stats = new ValidationStatistics
        {
            TotalValidated = await _context.ValidatedBuildings.CountAsync(),
            PendingUpload = await _context.ValidatedBuildings.CountAsync(b => !b.UploadedToOsm),
            UploadedToOsm = await _context.ValidatedBuildings.CountAsync(b => b.UploadedToOsm),
            ValidatedToday = await _context.ValidatedBuildings.CountAsync(b => b.ValidatedAt >= today),
            ValidatedThisWeek = await _context.ValidatedBuildings.CountAsync(b => b.ValidatedAt >= weekAgo)
        };
        
        // Get most common validation method
        var methodCounts = await _context.ValidatedBuildings
            .GroupBy(b => b.ValidationMethod)
            .Select(g => new { Method = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();
        
        stats.MostCommonMethod = methodCounts?.Method;
        
        // Get average AI confidence
        var avgConfidence = await _context.ValidatedBuildings
            .Where(b => b.AiConfidence.HasValue)
            .AverageAsync(b => b.AiConfidence);
        
        stats.AverageAiConfidence = avgConfidence;
        
        return stats;
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteValidatedBuildingAsync(long osmId)
    {
        var building = await _context.ValidatedBuildings
            .FirstOrDefaultAsync(b => b.OsmId == osmId);
        
        if (building != null)
        {
            _context.ValidatedBuildings.Remove(building);
            await _context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }
    
    /// <inheritdoc />
    public async Task<ValidatedBuilding> UpdateValidatedBuildingAsync(ValidatedBuilding validatedBuilding)
    {
        _context.ValidatedBuildings.Update(validatedBuilding);
        await _context.SaveChangesAsync();
        return validatedBuilding;
    }
    
    /// <inheritdoc />
    public async Task<List<ValidatedBuilding>> GetRecentValidationsAsync(int limit = 20)
    {
        return await _context.ValidatedBuildings
            .OrderByDescending(b => b.ValidatedAt)
            .Take(limit)
            .ToListAsync();
    }
}