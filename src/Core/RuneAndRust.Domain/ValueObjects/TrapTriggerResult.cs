// ═══════════════════════════════════════════════════════════════════════════════
// TrapTriggerResult.cs
// Immutable result object returned when a runic trap is triggered, containing
// the outcome details including success state, damage dealt, and identifiers.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of attempting to trigger a runic trap.
/// </summary>
/// <remarks>
/// <para>
/// This record captures the outcome of a trap triggering attempt, including
/// whether the attempt succeeded, damage dealt (if applicable), and the
/// identifiers of the involved trap and target.
/// </para>
/// <para>
/// A trigger can fail if the trap has already been triggered or has expired.
/// Successful triggers always deal damage and mark the trap as consumed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = trap.Trigger(targetId);
/// if (result.Success)
///     Console.WriteLine($"Trap dealt {result.DamageRoll} damage to {result.TargetId}!");
/// </code>
/// </example>
/// <seealso cref="RunicTrap"/>
public sealed record TrapTriggerResult
{
    /// <summary>
    /// Gets whether the trap was successfully triggered.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets a human-readable message describing the trigger outcome.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the total damage rolled when the trap triggered.
    /// </summary>
    /// <remarks>
    /// Only meaningful when <see cref="Success"/> is <c>true</c>.
    /// For Runic Trap, this represents the sum of a 3d6 roll.
    /// </remarks>
    public int DamageRoll { get; init; }

    /// <summary>
    /// Gets the ID of the character who triggered the trap, if any.
    /// </summary>
    public Guid? TargetId { get; init; }

    /// <summary>
    /// Gets the ID of the trap that was triggered, if any.
    /// </summary>
    public Guid? TrapId { get; init; }

    /// <summary>
    /// Creates a successful trap trigger result.
    /// </summary>
    /// <param name="trapId">ID of the triggered trap.</param>
    /// <param name="targetId">ID of the character who triggered the trap.</param>
    /// <param name="damageRoll">Total damage rolled (3d6 sum).</param>
    /// <returns>A successful TrapTriggerResult.</returns>
    public static TrapTriggerResult Triggered(Guid trapId, Guid targetId, int damageRoll) => new()
    {
        Success = true,
        Message = $"Runic Trap triggered! {damageRoll} damage dealt.",
        DamageRoll = damageRoll,
        TargetId = targetId,
        TrapId = trapId
    };

    /// <summary>
    /// Creates a failed trap trigger result.
    /// </summary>
    /// <param name="reason">Explanation of why the trigger failed.</param>
    /// <returns>A failed TrapTriggerResult with zero damage.</returns>
    public static TrapTriggerResult Failed(string reason) => new()
    {
        Success = false,
        Message = reason,
        DamageRoll = 0,
        TargetId = null,
        TrapId = null
    };

    /// <summary>
    /// Returns a human-readable representation of the trigger result.
    /// </summary>
    public override string ToString() => Success
        ? $"TRIGGERED: {DamageRoll} damage to {TargetId} (trap {TrapId})"
        : $"FAILED: {Message}";
}
