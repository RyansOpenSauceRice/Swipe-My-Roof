using Microsoft.EntityFrameworkCore;
using SwipeMyRoof.Core.Models;

namespace SwipeMyRoof.Core.Data;

/// <summary>
/// Database context for storing validated building data
/// </summary>
public class ValidationDbContext : DbContext
{
    public ValidationDbContext(DbContextOptions<ValidationDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Validated buildings with roof colors
    /// </summary>
    public DbSet<ValidatedBuilding> ValidatedBuildings { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure ValidatedBuilding entity
        modelBuilder.Entity<ValidatedBuilding>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Unique constraint on OSM ID to prevent duplicates
            entity.HasIndex(e => e.OsmId)
                  .IsUnique()
                  .HasDatabaseName("IX_ValidatedBuildings_OsmId");
            
            // Index for location-based queries
            entity.HasIndex(e => new { e.Latitude, e.Longitude })
                  .HasDatabaseName("IX_ValidatedBuildings_Location");
            
            // Index for upload status queries
            entity.HasIndex(e => e.UploadedToOsm)
                  .HasDatabaseName("IX_ValidatedBuildings_UploadStatus");
            
            // Index for validation date queries
            entity.HasIndex(e => e.ValidatedAt)
                  .HasDatabaseName("IX_ValidatedBuildings_ValidatedAt");
            
            // Configure string properties
            entity.Property(e => e.OsmType)
                  .HasMaxLength(20)
                  .IsRequired();
            
            entity.Property(e => e.RoofColorHex)
                  .HasMaxLength(7) // #RRGGBB
                  .IsRequired();
            
            entity.Property(e => e.ColorDescription)
                  .HasMaxLength(100);
            
            entity.Property(e => e.ValidationMethod)
                  .HasMaxLength(20)
                  .IsRequired();
            
            entity.Property(e => e.PickedPixelCoordinates)
                  .HasMaxLength(50);
            
            entity.Property(e => e.BuildingType)
                  .HasMaxLength(50);
            
            entity.Property(e => e.ValidatedBy)
                  .HasMaxLength(100);
            
            entity.Property(e => e.Notes)
                  .HasMaxLength(500);
            
            entity.Property(e => e.PreviousRoofColor)
                  .HasMaxLength(100);
            
            // Configure decimal precision for coordinates
            entity.Property(e => e.Latitude)
                  .HasPrecision(10, 7);
            
            entity.Property(e => e.Longitude)
                  .HasPrecision(10, 7);
        });
    }
}