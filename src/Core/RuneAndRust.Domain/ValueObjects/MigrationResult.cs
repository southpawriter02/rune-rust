// ═══════════════════════════════════════════════════════════════════════════════
// MigrationResult.cs
// Value object encapsulating the complete outcome of a character migration
// from a legacy class to the Aethelgard archetype/specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Encapsulates the complete outcome of a character migration from a legacy
/// class to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="MigrationResult"/> is produced by
/// <c>ICharacterMigrationService.CompleteMigration()</c> and captures every
/// detail of the migration for audit and player communication purposes:
/// </para>
/// <list type="bullet">
///   <item><description>The original legacy class and assigned archetype</description></item>
///   <item><description>The selected specialization (if migration completed)</description></item>
///   <item><description>Total PP refunded from incompatible abilities</description></item>
///   <item><description>Lists of preserved and removed abilities</description></item>
///   <item><description>Final migration status and any error message</description></item>
/// </list>
/// <para>
/// This record is immutable after creation and serves as the canonical
/// migration receipt for both player-facing communication and admin logs.
/// </para>
/// </remarks>
/// <seealso cref="LegacyClassId"/>
/// <seealso cref="MigrationStatus"/>
/// <seealso cref="RuneAndRust.Domain.Entities.CharacterMigration"/>
public sealed record MigrationResult
{
    /// <summary>
    /// The unique identifier of the migrated character.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// The legacy class the character was migrating from.
    /// </summary>
    public required LegacyClassId OriginalClass { get; init; }

    /// <summary>
    /// The Aethelgard archetype assigned during migration.
    /// </summary>
    public required Archetype AssignedArchetype { get; init; }

    /// <summary>
    /// The specialization selected by the player during migration.
    /// Null if migration failed before specialization selection.
    /// </summary>
    public SpecializationId? SelectedSpecialization { get; init; }

    /// <summary>
    /// Total Progression Points refunded from incompatible abilities.
    /// Full refund (100%) for incompatible abilities, partial (50%) for
    /// partially compatible abilities.
    /// </summary>
    public required int PpRefunded { get; init; }

    /// <summary>
    /// Ability IDs that were compatible with the new archetype and preserved.
    /// </summary>
    public required IReadOnlyList<string> PreservedAbilities { get; init; }

    /// <summary>
    /// Ability IDs that were incompatible and removed during migration.
    /// </summary>
    public required IReadOnlyList<string> RemovedAbilities { get; init; }

    /// <summary>
    /// The final status of the migration.
    /// </summary>
    public required MigrationStatus Status { get; init; }

    /// <summary>
    /// Error message if the migration failed. Null on success.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Timestamp when the migration completed (success or failure).
    /// </summary>
    public required DateTime CompletedAt { get; init; }
}
