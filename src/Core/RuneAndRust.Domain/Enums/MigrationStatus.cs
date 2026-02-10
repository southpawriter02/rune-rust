// ═══════════════════════════════════════════════════════════════════════════════
// MigrationStatus.cs
// Enum tracking the lifecycle of a character migration from legacy class
// to the Aethelgard archetype/specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the lifecycle state of a character migration from a legacy class
/// to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// Migration proceeds through the following state transitions:
/// </para>
/// <code>
/// Pending → InProgress → Completed
///                      → Failed
/// </code>
/// <para>
/// <see cref="Pending"/>: Migration detected but not yet started.
/// <see cref="InProgress"/>: Migration actively being processed (archetype assigned,
/// awaiting specialization selection).
/// <see cref="Completed"/>: Migration finished successfully with specialization selected.
/// <see cref="Failed"/>: Migration encountered an error and requires intervention.
/// </para>
/// <para>
/// Enum values are explicitly assigned (0-3) to ensure stable serialization
/// and database storage.
/// </para>
/// </remarks>
/// <seealso cref="LegacyClassId"/>
/// <seealso cref="RuneAndRust.Domain.Entities.CharacterMigration"/>
public enum MigrationStatus
{
    /// <summary>
    /// Migration has been detected but not yet started.
    /// The character still has a legacy class assignment.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Migration is actively being processed.
    /// The character has been assigned an archetype and is awaiting
    /// specialization selection.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Migration completed successfully.
    /// The character has an archetype, a selected specialization,
    /// and all ability compatibility checks have been resolved.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Migration encountered an error and failed.
    /// The character's state may be partially migrated and requires
    /// manual intervention or retry.
    /// </summary>
    Failed = 3
}
