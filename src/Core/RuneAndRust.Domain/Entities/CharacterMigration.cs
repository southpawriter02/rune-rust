// ═══════════════════════════════════════════════════════════════════════════════
// CharacterMigration.cs
// Entity tracking the state and details of a character's migration from a
// legacy class to the Aethelgard archetype/specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Tracks the state and progress of a character's migration from a legacy
/// class to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="CharacterMigration"/> is created when a character with a
/// legacy class is detected and manages the full migration lifecycle:
/// </para>
/// <code>
/// [Created: Pending] → BeginMigration() → [InProgress]
///                                        → SelectSpecialization() → [InProgress, spec set]
///                                        → RecordRefund() → [InProgress, PP tracked]
///                                        → Complete() → [Completed]
///                                        → Fail() → [Failed]
/// </code>
/// <para>
/// State transitions are enforced by domain methods that validate the
/// current <see cref="Status"/> before allowing a transition. Invalid
/// transitions throw <see cref="InvalidOperationException"/>.
/// </para>
/// <para>
/// Every mutation creates a corresponding <see cref="MigrationLog"/> entry
/// via the migration service for full auditability.
/// </para>
/// </remarks>
/// <seealso cref="LegacyClassId"/>
/// <seealso cref="MigrationStatus"/>
/// <seealso cref="MigrationLog"/>
public sealed class CharacterMigration
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Unique identifier for this migration record.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The character being migrated.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// The legacy class the character is migrating from.
    /// </summary>
    public required LegacyClassId OriginalClass { get; init; }

    /// <summary>
    /// The target Aethelgard archetype assigned during migration.
    /// </summary>
    public required Archetype TargetArchetype { get; init; }

    /// <summary>
    /// Current status of the migration.
    /// Transitions via <see cref="BeginMigration"/>, <see cref="Complete"/>,
    /// and <see cref="Fail"/>.
    /// </summary>
    public MigrationStatus Status { get; private set; } = MigrationStatus.Pending;

    /// <summary>
    /// The specialization selected by the player during migration.
    /// Set via <see cref="SelectSpecialization"/>.
    /// Null until the player makes a selection.
    /// </summary>
    public SpecializationId? SelectedSpecialization { get; private set; }

    /// <summary>
    /// Total Progression Points refunded from incompatible abilities.
    /// Accumulated via <see cref="RecordRefund"/>.
    /// </summary>
    public int PpRefunded { get; private set; }

    /// <summary>
    /// Timestamp when this migration record was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the migration completed (success or failure).
    /// Null while migration is pending or in progress.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Error message if the migration failed. Null on success.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE TRANSITION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Transitions the migration from <see cref="MigrationStatus.Pending"/>
    /// to <see cref="MigrationStatus.InProgress"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the migration is not in <see cref="MigrationStatus.Pending"/> state.
    /// </exception>
    public void BeginMigration()
    {
        if (Status != MigrationStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot begin migration {Id}: current status is {Status}, expected {MigrationStatus.Pending}.");
        }

        Status = MigrationStatus.InProgress;
    }

    /// <summary>
    /// Records the player's specialization selection during migration.
    /// </summary>
    /// <param name="specialization">
    /// The specialization selected by the player.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the migration is not in <see cref="MigrationStatus.InProgress"/> state.
    /// </exception>
    public void SelectSpecialization(SpecializationId specialization)
    {
        if (Status != MigrationStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot select specialization for migration {Id}: current status is {Status}, expected {MigrationStatus.InProgress}.");
        }

        SelectedSpecialization = specialization;
    }

    /// <summary>
    /// Records a PP refund amount from incompatible ability removal.
    /// </summary>
    /// <param name="ppAmount">
    /// The number of Progression Points to refund. Must be non-negative.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the migration is not in <see cref="MigrationStatus.InProgress"/> state.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ppAmount"/> is negative.
    /// </exception>
    public void RecordRefund(int ppAmount)
    {
        if (Status != MigrationStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot record refund for migration {Id}: current status is {Status}, expected {MigrationStatus.InProgress}.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(ppAmount);

        PpRefunded += ppAmount;
    }

    /// <summary>
    /// Marks the migration as successfully completed.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the migration is not in <see cref="MigrationStatus.InProgress"/> state
    /// or when no specialization has been selected.
    /// </exception>
    public void Complete()
    {
        if (Status != MigrationStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot complete migration {Id}: current status is {Status}, expected {MigrationStatus.InProgress}.");
        }

        if (SelectedSpecialization is null)
        {
            throw new InvalidOperationException(
                $"Cannot complete migration {Id}: no specialization has been selected.");
        }

        Status = MigrationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the migration as failed with an error message.
    /// </summary>
    /// <param name="errorMessage">
    /// Description of why the migration failed.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="errorMessage"/> is null or whitespace.
    /// </exception>
    public void Fail(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        Status = MigrationStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }
}
