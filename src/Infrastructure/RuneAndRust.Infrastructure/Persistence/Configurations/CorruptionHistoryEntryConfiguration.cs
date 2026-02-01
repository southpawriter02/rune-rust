// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionHistoryEntryConfiguration.cs
// EF Core entity type configuration for CorruptionHistoryEntry. Maps the domain
// entity to the CorruptionHistory database table with string conversion for the
// CorruptionSource enum column, a composite index on (CharacterId, CreatedAt)
// for efficient per-character history queries, a partial index on transfers,
// and a cascading foreign key to the Player entity.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="CorruptionHistoryEntry"/>.
/// </summary>
/// <remarks>
/// <para>
/// Maps <see cref="CorruptionHistoryEntry"/> to the <c>CorruptionHistory</c> database table.
/// Configures:
/// </para>
/// <list type="bullet">
/// <item><description>Guid primary key with <c>ValueGeneratedNever()</c> (application-managed).</description></item>
/// <item><description>String conversion for <see cref="CorruptionSource"/> enum for readability and schema evolution.</description></item>
/// <item><description>Composite index on <c>(CharacterId, CreatedAt)</c> for efficient per-character queries ordered by date.</description></item>
/// <item><description>Index on <c>IsTransfer</c> for efficient transfer event queries.</description></item>
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
/// <item><term>CharacterId</term><description>UUID FK → Players, NOT NULL, CASCADE DELETE</description></item>
/// <item><term>Amount</term><description>INT NOT NULL</description></item>
/// <item><term>Source</term><description>VARCHAR(50) NOT NULL (enum as string)</description></item>
/// <item><term>NewTotal</term><description>INT NOT NULL</description></item>
/// <item><term>ThresholdCrossed</term><description>INT NULL</description></item>
/// <item><term>IsTransfer</term><description>BOOLEAN NOT NULL DEFAULT false</description></item>
/// <item><term>TransferTargetId</term><description>UUID NULL</description></item>
/// <item><term>CreatedAt</term><description>TIMESTAMP NOT NULL</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionHistoryEntry"/>
/// <seealso cref="CorruptionTrackerConfiguration"/>
/// <seealso cref="StressHistoryEntryConfiguration"/>
public class CorruptionHistoryEntryConfiguration : IEntityTypeConfiguration<CorruptionHistoryEntry>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CorruptionHistoryEntry> builder)
    {
        // Table mapping
        builder.ToTable("CorruptionHistory");

        // Primary key — application-managed Guid
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        // Character reference — required foreign key
        builder.Property(e => e.CharacterId)
            .IsRequired();

        // Corruption amount — required integer field
        builder.Property(e => e.Amount)
            .IsRequired();

        // Source enum — stored as string for readability and schema evolution
        builder.Property(e => e.Source)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // New total — required integer field
        builder.Property(e => e.NewTotal)
            .IsRequired();

        // Threshold crossed — nullable integer
        builder.Property(e => e.ThresholdCrossed);

        // Transfer flag — required, default false
        builder.Property(e => e.IsTransfer)
            .IsRequired()
            .HasDefaultValue(false);

        // Transfer target — nullable Guid
        builder.Property(e => e.TransferTargetId);

        // Timestamp — required, auto-set at creation
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Composite index for efficient per-character history queries ordered by date
        builder.HasIndex(e => new { e.CharacterId, e.CreatedAt })
            .HasDatabaseName("IX_CorruptionHistory_CharacterCreatedAt");

        // Index on IsTransfer for efficient transfer event filtering
        builder.HasIndex(e => e.IsTransfer)
            .HasDatabaseName("IX_CorruptionHistory_IsTransfer");

        // Foreign key to Player entity with cascade delete
        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(e => e.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
