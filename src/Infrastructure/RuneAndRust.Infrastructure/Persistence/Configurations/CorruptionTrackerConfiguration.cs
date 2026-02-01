// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionTrackerConfiguration.cs
// EF Core entity type configuration for CorruptionTracker. Maps the domain
// entity to the CorruptionTrackers database table with application-managed Guid
// primary key, corruption range constraint (0-100), boolean threshold trigger
// flags, and a cascading foreign key to the Player entity. Includes a unique
// index on CharacterId for one-to-one character relationship enforcement.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="CorruptionTracker"/>.
/// </summary>
/// <remarks>
/// <para>
/// Maps <see cref="CorruptionTracker"/> to the <c>CorruptionTrackers</c> database table.
/// Configures:
/// </para>
/// <list type="bullet">
/// <item><description>Guid primary key with <c>ValueGeneratedNever()</c> (application-managed).</description></item>
/// <item><description>Unique index on <c>CharacterId</c> for one-to-one character relationship enforcement.</description></item>
/// <item><description>Corruption range constraint via check constraint (0-100).</description></item>
/// <item><description>Boolean threshold trigger flags (25/50/75) with default <c>false</c>.</description></item>
/// <item><description>Cascading foreign key to <see cref="Player"/> for referential integrity.</description></item>
/// </list>
/// <para>
/// <strong>Table Schema:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Column</term>
/// <description>Type / Constraint</description>
/// </listheader>
/// <item><term>Id</term><description>UUID PK (application-managed)</description></item>
/// <item><term>CharacterId</term><description>UUID FK → Players, UNIQUE, CASCADE DELETE</description></item>
/// <item><term>CurrentCorruption</term><description>INT NOT NULL DEFAULT 0, CHECK (0-100)</description></item>
/// <item><term>Threshold25Triggered</term><description>BOOLEAN NOT NULL DEFAULT false</description></item>
/// <item><term>Threshold50Triggered</term><description>BOOLEAN NOT NULL DEFAULT false</description></item>
/// <item><term>Threshold75Triggered</term><description>BOOLEAN NOT NULL DEFAULT false</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionTracker"/>
/// <seealso cref="CorruptionHistoryEntryConfiguration"/>
/// <seealso cref="StressHistoryEntryConfiguration"/>
public class CorruptionTrackerConfiguration : IEntityTypeConfiguration<CorruptionTracker>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CorruptionTracker> builder)
    {
        // Table mapping
        builder.ToTable("CorruptionTrackers");

        // Primary key — application-managed Guid
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        // Character reference — required foreign key with unique constraint
        builder.Property(e => e.CharacterId)
            .IsRequired();

        builder.HasIndex(e => e.CharacterId)
            .IsUnique()
            .HasDatabaseName("IX_CorruptionTrackers_CharacterId");

        // Corruption value — required, default 0, range constraint
        builder.Property(e => e.CurrentCorruption)
            .IsRequired()
            .HasDefaultValue(0);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_CorruptionTrackers_CorruptionRange",
            "\"CurrentCorruption\" >= 0 AND \"CurrentCorruption\" <= 100"));

        // Threshold trigger flags — required, default false
        builder.Property(e => e.Threshold25Triggered)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Threshold50Triggered)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Threshold75Triggered)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign key to Player entity with cascade delete
        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(e => e.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties — these are calculated from CurrentCorruption
        builder.Ignore(e => e.Stage);
        builder.Ignore(e => e.MaxHpPenaltyPercent);
        builder.Ignore(e => e.MaxApPenaltyPercent);
        builder.Ignore(e => e.ResolveDicePenalty);
        builder.Ignore(e => e.TechBonus);
        builder.Ignore(e => e.SocialPenalty);
        builder.Ignore(e => e.IsFactionLocked);
        builder.Ignore(e => e.IsTerminalError);
    }
}
