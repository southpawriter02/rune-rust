// ═══════════════════════════════════════════════════════════════════════════════
// StressHistoryEntryConfiguration.cs
// EF Core entity type configuration for StressHistoryEntry. Maps the domain
// entity to the StressHistory database table with string conversion for enum
// columns, a composite index on (CharacterId, CreatedAt) for efficient
// per-character history queries, and a cascading foreign key to the Player entity.
// Version: 0.18.0f
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="StressHistoryEntry"/>.
/// </summary>
/// <remarks>
/// <para>
/// Maps <see cref="StressHistoryEntry"/> to the <c>StressHistory</c> database table.
/// Configures:
/// </para>
/// <list type="bullet">
/// <item><description>Guid primary key with <c>ValueGeneratedNever()</c> (application-managed).</description></item>
/// <item><description>String conversion for <see cref="StressSource"/> and <see cref="StressThreshold"/> enums.</description></item>
/// <item><description>Composite index on <c>(CharacterId, CreatedAt)</c> for efficient per-character queries ordered by date.</description></item>
/// <item><description>Cascading foreign key to <see cref="Player"/> for referential integrity.</description></item>
/// </list>
/// </remarks>
/// <seealso cref="StressHistoryEntry"/>
/// <seealso cref="HiddenElementConfiguration"/>
public class StressHistoryEntryConfiguration : IEntityTypeConfiguration<StressHistoryEntry>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<StressHistoryEntry> builder)
    {
        // Table mapping
        builder.ToTable("StressHistory");

        // Primary key — application-managed Guid
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        // Character reference — required foreign key
        builder.Property(e => e.CharacterId)
            .IsRequired();

        // Stress amounts — required integer fields
        builder.Property(e => e.Amount)
            .IsRequired();

        builder.Property(e => e.FinalAmount)
            .IsRequired();

        builder.Property(e => e.PreviousStress)
            .IsRequired();

        builder.Property(e => e.NewStress)
            .IsRequired();

        // Source enum — stored as string for readability and schema evolution
        builder.Property(e => e.Source)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Resistance check details — nullable fields
        builder.Property(e => e.ResistDc);

        builder.Property(e => e.Resisted)
            .HasDefaultValue(false);

        // Threshold crossed — nullable enum stored as string
        builder.Property(e => e.ThresholdCrossed)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Timestamp — required, auto-set at creation
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Composite index for efficient per-character history queries ordered by date
        builder.HasIndex(e => new { e.CharacterId, e.CreatedAt })
            .HasDatabaseName("IX_StressHistory_CharacterCreatedAt");

        // Foreign key to Player entity with cascade delete
        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(e => e.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
