using Microsoft.EntityFrameworkCore;
using RuneAndRust.Core.Entities;

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
    }
}
