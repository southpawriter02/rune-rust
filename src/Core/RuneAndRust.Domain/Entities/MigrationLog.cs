// ═══════════════════════════════════════════════════════════════════════════════
// MigrationLog.cs
// Entity representing an audit trail entry for a migration action, ensuring
// full transparency and traceability of the migration process.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an immutable audit trail entry for a single action taken
/// during a character migration.
/// </summary>
/// <remarks>
/// <para>
/// Every migration action — archetype assignment, ability evaluation,
/// PP refund, specialization selection, completion, or failure — creates
/// a <see cref="MigrationLog"/> entry. These entries are append-only and
/// cannot be modified after creation, ensuring a complete and tamper-proof
/// record of the migration process.
/// </para>
/// <para>
/// Common <see cref="ActionType"/> values include:
/// </para>
/// <list type="bullet">
///   <item><description><c>"MigrationInitiated"</c> — Migration process started</description></item>
///   <item><description><c>"ArchetypeAssigned"</c> — Target archetype set on character</description></item>
///   <item><description><c>"AbilityEvaluated"</c> — Ability compatibility checked</description></item>
///   <item><description><c>"AbilityRemoved"</c> — Incompatible ability removed</description></item>
///   <item><description><c>"AbilityPreserved"</c> — Compatible ability kept</description></item>
///   <item><description><c>"PpRefunded"</c> — Progression Points returned for removed ability</description></item>
///   <item><description><c>"SpecializationSelected"</c> — Player chose specialization</description></item>
///   <item><description><c>"MigrationCompleted"</c> — Migration finished successfully</description></item>
///   <item><description><c>"MigrationFailed"</c> — Migration encountered error</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CharacterMigration"/>
public sealed class MigrationLog
{
    /// <summary>
    /// Unique identifier for this log entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The migration this log entry belongs to.
    /// </summary>
    public required Guid MigrationId { get; init; }

    /// <summary>
    /// The character this action was performed on.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// The type of action performed (e.g., "AbilityRemoved",
    /// "SpecializationSelected", "MigrationCompleted").
    /// </summary>
    public required string ActionType { get; init; }

    /// <summary>
    /// Human-readable description of the action taken.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Net change in Progression Points caused by this action.
    /// Positive values represent refunds; zero for non-PP actions.
    /// </summary>
    public int PpDelta { get; init; }

    /// <summary>
    /// Ability IDs affected by this action, if any.
    /// Null for actions that don't affect specific abilities.
    /// </summary>
    public IReadOnlyList<string>? AffectedAbilities { get; init; }

    /// <summary>
    /// Timestamp when this action was performed.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}
