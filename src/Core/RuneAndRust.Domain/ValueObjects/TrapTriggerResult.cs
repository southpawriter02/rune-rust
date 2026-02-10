namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable result of attempting to trigger a <see cref="RunicTrap"/>.
/// </summary>
/// <remarks>
/// <para>Captures the outcome of a trap trigger attempt, including whether the trigger
/// succeeded, the damage dealt, and identifying information about the target and trap.</para>
/// <para>Use factory methods <see cref="Triggered"/> and <see cref="Failed"/> for construction
/// to ensure consistent result creation.</para>
/// </remarks>
public sealed record TrapTriggerResult
{
    /// <summary>
    /// Whether the trap was successfully triggered.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Human-readable message describing the trigger outcome.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Total damage dealt by the triggered trap (0 if trigger failed).
    /// </summary>
    public int DamageDealt { get; init; }

    /// <summary>
    /// The character ID that triggered the trap (null if trigger failed).
    /// </summary>
    public Guid? TargetId { get; init; }

    /// <summary>
    /// The ID of the trap that was triggered (null if trigger failed).
    /// </summary>
    public Guid? TrapId { get; init; }

    /// <summary>
    /// Creates a successful trigger result.
    /// </summary>
    /// <param name="trapId">The triggered trap's ID.</param>
    /// <param name="targetId">The character who triggered the trap.</param>
    /// <param name="damageDealt">Total damage dealt.</param>
    /// <returns>A successful trigger result with damage information.</returns>
    public static TrapTriggerResult Triggered(Guid trapId, Guid targetId, int damageDealt) =>
        new()
        {
            Success = true,
            Message = $"Runic Trap triggered! Dealt {damageDealt} damage.",
            DamageDealt = damageDealt,
            TargetId = targetId,
            TrapId = trapId
        };

    /// <summary>
    /// Creates a failed trigger result.
    /// </summary>
    /// <param name="reason">The reason the trigger failed.</param>
    /// <returns>A failed trigger result with the failure reason.</returns>
    public static TrapTriggerResult Failed(string reason) =>
        new()
        {
            Success = false,
            Message = reason,
            DamageDealt = 0,
            TargetId = null,
            TrapId = null
        };
}
