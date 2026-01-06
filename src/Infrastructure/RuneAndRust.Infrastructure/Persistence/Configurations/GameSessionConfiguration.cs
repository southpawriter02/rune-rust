using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;
using System.Text.Json;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

        // Store Player as JSON
        builder.OwnsOne(x => x.Player, player =>
        {
            player.Property(p => p.Id).HasColumnName("PlayerId");
            player.Property(p => p.Name).HasColumnName("PlayerName").HasMaxLength(100);
            player.Property(p => p.Health).HasColumnName("PlayerHealth");

            player.OwnsOne(p => p.Stats, stats =>
            {
                stats.Property(s => s.MaxHealth).HasColumnName("PlayerMaxHealth");
                stats.Property(s => s.Attack).HasColumnName("PlayerAttack");
                stats.Property(s => s.Defense).HasColumnName("PlayerDefense");
            });

            player.OwnsOne(p => p.Position, pos =>
            {
                pos.Property(p => p.X).HasColumnName("PlayerPositionX");
                pos.Property(p => p.Y).HasColumnName("PlayerPositionY");
            });

            // Store inventory as JSON for simplicity
            player.OwnsOne(p => p.Inventory, inv =>
            {
                inv.Property(i => i.Capacity).HasColumnName("InventoryCapacity");
                inv.Property<string>("ItemsJson")
                    .HasColumnName("InventoryItems")
                    .HasColumnType("jsonb");

                inv.Ignore(i => i.Items);
                inv.Ignore(i => i.Count);
                inv.Ignore(i => i.IsFull);
                inv.Ignore(i => i.IsEmpty);
            });
        });

        // Store Dungeon as JSON for simplicity in the walking skeleton
        builder.Property<string>("DungeonJson")
            .HasColumnName("DungeonData")
            .HasColumnType("jsonb");

        builder.Ignore(x => x.Dungeon);
        builder.Ignore(x => x.CurrentRoom);
    }
}
