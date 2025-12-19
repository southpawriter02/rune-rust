using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Entity Framework Core database context for Rune &amp; Rust.
/// Manages entity mappings and database configuration.
/// </summary>
public class RuneAndRustDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuneAndRustDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public RuneAndRustDbContext(DbContextOptions<RuneAndRustDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the SaveGames table.
    /// </summary>
    public DbSet<SaveGame> SaveGames { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Rooms table.
    /// </summary>
    public DbSet<Room> Rooms { get; set; } = null!;

    /// <summary>
    /// Configures the entity mappings and relationships.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SaveGame>(entity =>
        {
            entity.ToTable("SaveGames");

            entity.HasKey(s => s.Id);

            entity.HasIndex(s => s.SlotNumber)
                .IsUnique();

            entity.Property(s => s.SlotNumber)
                .IsRequired();

            entity.Property(s => s.CharacterName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.CreatedAt)
                .IsRequired();

            entity.Property(s => s.LastPlayed)
                .IsRequired();

            entity.Property(s => s.SerializedState)
                .HasColumnType("jsonb")
                .IsRequired();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Rooms");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(r => r.Description)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(r => r.IsStartingRoom)
                .IsRequired();

            // Map Coordinate as owned type (creates PositionX, PositionY, PositionZ columns)
            entity.OwnsOne(r => r.Position, position =>
            {
                position.Property(p => p.X).HasColumnName("PositionX").IsRequired();
                position.Property(p => p.Y).HasColumnName("PositionY").IsRequired();
                position.Property(p => p.Z).HasColumnName("PositionZ").IsRequired();
            });

            // Note: Unique index on position is handled at the application level
            // InMemory provider doesn't support unique indexes on owned types well

            // Store Exits dictionary as JSONB
            entity.Property(r => r.Exits)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<Direction, Guid>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<Direction, Guid>()
                )
                .IsRequired();
        });
    }
}
