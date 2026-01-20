using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of attempting to change a combatant's combat stance.
/// </summary>
/// <remarks>
/// <para>StanceChangeResult provides detailed feedback about stance change operations,
/// including success/failure status and the stances involved.</para>
/// <para>Possible outcomes:</para>
/// <list type="bullet">
///   <item><description>Success: Stance was changed from OldStance to NewStance</description></item>
///   <item><description>AlreadyInStance: Combatant was already in the requested stance</description></item>
///   <item><description>Failed: Change was not allowed (e.g., already changed this round)</description></item>
/// </list>
/// </remarks>
public record StanceChangeResult
{
    /// <summary>
    /// Gets whether the stance change operation was successful.
    /// </summary>
    /// <remarks>
    /// True if the stance was changed or if already in the requested stance.
    /// False if the change was not allowed.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the combatant's stance before the change attempt.
    /// </summary>
    /// <remarks>
    /// Null if the change failed before the old stance could be determined.
    /// </remarks>
    public CombatStance? OldStance { get; init; }

    /// <summary>
    /// Gets the combatant's stance after the change attempt.
    /// </summary>
    /// <remarks>
    /// Equal to OldStance if already in the requested stance.
    /// Null if the change failed.
    /// </remarks>
    public CombatStance? NewStance { get; init; }

    /// <summary>
    /// Gets the reason the stance change failed, if applicable.
    /// </summary>
    /// <remarks>
    /// Null for successful changes. Contains a user-friendly message for failures.
    /// Examples: "Already changed stance this round", "Unknown stance: X"
    /// </remarks>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Gets whether the combatant's stance actually changed.
    /// </summary>
    /// <remarks>
    /// False when IsSuccess is true but OldStance equals NewStance (already in stance).
    /// </remarks>
    public bool StanceChanged => IsSuccess && OldStance != NewStance;

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a successful stance change result.
    /// </summary>
    /// <param name="oldStance">The stance before the change.</param>
    /// <param name="newStance">The stance after the change.</param>
    /// <returns>A successful StanceChangeResult.</returns>
    public static StanceChangeResult Success(CombatStance oldStance, CombatStance newStance)
        => new()
        {
            IsSuccess = true,
            OldStance = oldStance,
            NewStance = newStance
        };

    /// <summary>
    /// Creates a result indicating the combatant was already in the requested stance.
    /// </summary>
    /// <param name="stance">The current (and requested) stance.</param>
    /// <returns>A StanceChangeResult indicating no change was needed.</returns>
    /// <remarks>
    /// IsSuccess is true since the request was valid, but no state change occurred.
    /// </remarks>
    public static StanceChangeResult AlreadyInStance(CombatStance stance)
        => new()
        {
            IsSuccess = true,
            OldStance = stance,
            NewStance = stance,
            FailureReason = "Already in this stance"
        };

    /// <summary>
    /// Creates a failed stance change result.
    /// </summary>
    /// <param name="reason">The reason the change failed.</param>
    /// <returns>A failed StanceChangeResult.</returns>
    public static StanceChangeResult Failed(string reason)
        => new()
        {
            IsSuccess = false,
            FailureReason = reason
        };
}
