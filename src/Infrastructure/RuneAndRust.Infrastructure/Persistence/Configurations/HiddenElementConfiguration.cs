using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

public class HiddenElementConfiguration : IEntityTypeConfiguration<HiddenElement>
{
    public void Configure(EntityTypeBuilder<HiddenElement> builder)
    {
        builder.ToTable("HiddenElements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ElementType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DiscoveryText)
            .IsRequired();

        builder.Property(x => x.DetectionDC)
            .IsRequired();

        builder.Property(x => x.IsRevealed)
            .HasDefaultValue(false);

        builder.Property(x => x.RoomId)
            .IsRequired();

        // Trap-specific properties
        builder.Property(x => x.TrapDamage);
        builder.Property(x => x.DisarmDC);
        builder.Property(x => x.IsDisarmed)
            .HasDefaultValue(false);

        // SecretDoor-specific properties
        builder.Property(x => x.LeadsToRoomId);

        // Cache-specific properties
        builder.Property(x => x.CacheContents)
            .HasMaxLength(500);
        builder.Property(x => x.IsLooted)
            .HasDefaultValue(false);

        // Index for room-based queries
        builder.HasIndex(x => x.RoomId)
            .HasDatabaseName("IX_HiddenElement_Room");

        builder.HasIndex(x => new { x.RoomId, x.IsRevealed })
            .HasDatabaseName("IX_HiddenElement_RoomRevealed");
    }
}
