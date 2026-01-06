using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="GameSession"/> entity.
/// </summary>
/// <remarks>
/// This configuration defines the database mapping for game sessions, including
/// table name, primary key, and column constraints. Complex navigation properties
/// (Player, Dungeon, CurrentRoom) are currently ignored pending full implementation.
/// </remarks>
public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    /// <summary>
    /// Configures the GameSession entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder for GameSession.</param>
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("GameSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CurrentRoomId)
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastPlayedAt)
            .IsRequired();

        // For the walking skeleton, ignore complex navigation properties
        // Full EF Core mapping will be implemented when PostgreSQL persistence is needed
        builder.Ignore(x => x.Player);
        builder.Ignore(x => x.Dungeon);
        builder.Ignore(x => x.CurrentRoom);
    }
}
