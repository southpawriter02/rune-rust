namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable result record from the Crippling Shot ability execution.
/// Records the movement speed reduction applied to a target after consuming a Quarry Mark.
/// </summary>
/// <remarks>
/// <para>Introduced in v0.20.7c as the Veiðimaðr Tier 3 active attack result.</para>
/// <para>Crippling Shot halves a target's movement speed for 2 turns:</para>
/// <list type="bullet">
/// <item><description>Costs 1 AP and consumes 1 Quarry Mark (mark is removed)</description></item>
/// <item><description>Guaranteed effect — no attack roll required (mark guarantees hit)</description></item>
/// <item><description>Movement halved via integer division (e.g., 5 → 2, 6 → 3, 7 → 3)</description></item>
/// <item><description>Duration is always 2 turns — cannot be extended or stacked</description></item>
/// <item><description>Reapplying refreshes duration (does not stack reduction)</description></item>
/// </list>
/// <para>Key computed properties:</para>
/// <list type="bullet">
/// <item><description><see cref="ReducedMovementSpeed"/>: Always <see cref="OriginalMovementSpeed"/> / 2 (integer division)</description></item>
/// <item><description><see cref="DurationTurns"/>: Always 2 turns</description></item>
/// </list>
/// <para>No Corruption risk — Crippling Shot follows the Coherent path.</para>
/// </remarks>
public sealed record CripplingShotResult
{
    /// <summary>
    /// Default duration of the crippling movement reduction in turns.
    /// </summary>
    private const int DefaultDurationTurns = 2;

    /// <summary>
    /// Divisor applied to the target's movement speed (halves it).
    /// </summary>
    private const int MovementDivisor = 2;

    /// <summary>
    /// Unique identifier of the Veiðimaðr who executed the shot.
    /// </summary>
    public Guid HunterId { get; init; }

    /// <summary>
    /// Unique identifier of the target whose movement was crippled.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the crippled target (for UI and logging).
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// The target's movement speed in spaces before Crippling Shot was applied.
    /// </summary>
    public int OriginalMovementSpeed { get; init; }

    /// <summary>
    /// The target's movement speed in spaces after Crippling Shot.
    /// Computed as <see cref="OriginalMovementSpeed"/> / 2 (integer division).
    /// </summary>
    /// <remarks>
    /// Integer division means odd values are rounded down: 5 → 2, 7 → 3.
    /// A speed of 1 becomes 0 (effectively immobilized), and 0 stays 0.
    /// </remarks>
    public int ReducedMovementSpeed => OriginalMovementSpeed / MovementDivisor;

    /// <summary>
    /// Number of turns the movement reduction lasts. Always 2.
    /// </summary>
    public int DurationTurns => DefaultDurationTurns;

    /// <summary>
    /// Whether the Quarry Mark on the target was consumed by this ability.
    /// Always true for a successful Crippling Shot execution.
    /// </summary>
    public bool MarkConsumed { get; init; }

    /// <summary>
    /// Returns a human-readable description of the Crippling Shot outcome.
    /// Includes target name, movement reduction values, duration, and mark consumption.
    /// </summary>
    /// <returns>A narrative description of the crippling effect.</returns>
    public string GetDescription()
    {
        return $"Crippling Shot: {TargetName}'s movement halved " +
               $"({OriginalMovementSpeed} -> {ReducedMovementSpeed} spaces) " +
               $"for {DurationTurns} turns. Quarry Mark consumed.";
    }

    /// <summary>
    /// Returns a short text describing the remaining duration of the crippling effect.
    /// </summary>
    /// <returns>A duration string (e.g., "2 turns remaining").</returns>
    public string GetDurationText() =>
        $"{DurationTurns} turns remaining";

    /// <summary>
    /// Returns a concise status message summarizing the Crippling Shot effect.
    /// Suitable for combat log entries and UI notifications.
    /// </summary>
    /// <returns>A formatted status message.</returns>
    public string GetStatusMessage() =>
        $"CRIPPLING SHOT: {TargetName} movement reduced! " +
        $"({OriginalMovementSpeed} -> {ReducedMovementSpeed} spaces, {DurationTurns} turns)";
}
