using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
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
