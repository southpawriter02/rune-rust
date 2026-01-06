using Microsoft.EntityFrameworkCore;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Infrastructure.Persistence.Seeders;

namespace RuneAndRust.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<ExaminationDescriptor> ExaminationDescriptors => Set<ExaminationDescriptor>();
    public DbSet<PerceptionDescriptor> PerceptionDescriptors => Set<PerceptionDescriptor>();
    public DbSet<FloraFaunaDescriptor> FloraFaunaDescriptors => Set<FloraFaunaDescriptor>();
    public DbSet<HiddenElement> HiddenElements => Set<HiddenElement>();
    public DbSet<InteractionDescriptor> InteractionDescriptors => Set<InteractionDescriptor>();

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);

        // Seed interaction descriptors
        modelBuilder.Entity<InteractionDescriptor>()
            .HasData(InteractionDescriptorSeeder.GetAllDescriptors());

        base.OnModelCreating(modelBuilder);
    }
}
