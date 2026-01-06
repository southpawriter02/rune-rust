using Microsoft.EntityFrameworkCore;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public DbSet<GameSession> GameSessions => Set<GameSession>();

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
