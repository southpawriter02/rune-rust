using Microsoft.EntityFrameworkCore;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Infrastructure.Persistence.Seeders;

namespace RuneAndRust.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for game persistence.
/// </summary>
/// <remarks>
/// This context is configured for PostgreSQL and manages the persistence of game sessions.
/// Entity configurations are automatically applied from the assembly containing this class.
/// </remarks>
public class GameDbContext : DbContext
{
    /// <summary>
    /// Gets the DbSet for game sessions.
    /// </summary>
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<ExaminationDescriptor> ExaminationDescriptors => Set<ExaminationDescriptor>();
    public DbSet<PerceptionDescriptor> PerceptionDescriptors => Set<PerceptionDescriptor>();
    public DbSet<FloraFaunaDescriptor> FloraFaunaDescriptors => Set<FloraFaunaDescriptor>();
    public DbSet<HiddenElement> HiddenElements => Set<HiddenElement>();
    public DbSet<InteractionDescriptor> InteractionDescriptors => Set<InteractionDescriptor>();

    /// <summary>
    /// Creates a new instance of the GameDbContext.
    /// </summary>
    /// <param name="options">The options for configuring the context.</param>
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the model using entity configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);

        // Seed interaction descriptors
        modelBuilder.Entity<InteractionDescriptor>()
            .HasData(InteractionDescriptorSeeder.GetAllDescriptors());

        // Seed examination descriptors for room features and structural elements
        modelBuilder.Entity<ExaminationDescriptor>()
            .HasData(RoomFeatureExaminationSeeder.GetAllDescriptors());

        base.OnModelCreating(modelBuilder);
    }
}
