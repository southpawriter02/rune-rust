// ═══════════════════════════════════════════════════════════════════════════════
// StressRecoveryResult.cs
// Immutable value object representing the outcome of stress recovery from rest
// or other healing sources. Captures before/after stress values, threshold
// improvements, and recovery metadata.
// Part of the Result Object Pattern for the Trauma Economy system.
// Version: 0.18.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents the result of stress recovery.
/// </summary>
/// <remarks>
/// <para>
/// Created when a character recovers stress through rest or other means.
/// Tracks the recovery amount, source, and any threshold improvements.
/// </para>
/// <para>
/// <strong>Clamping:</strong> The <see cref="NewStress"/> value is clamped to a
/// minimum of <see cref="StressState.MinStress"/> (0). Recovery cannot produce
/// negative stress values.
/// </para>
/// <para>
/// <strong>Threshold Dropping:</strong> <see cref="ThresholdDropped"/> is <c>true</c>
/// when the new threshold tier is strictly less than the previous tier, indicating
/// the character's psychological state has improved.
/// </para>
/// <para>
/// <strong>Recovery Sources:</strong> Each <see cref="RestType"/> provides different
/// recovery amounts:
/// <list type="bullet">
/// <item><description>Short Rest: WILL × 2</description></item>
/// <item><description>Long Rest: WILL × 5</description></item>
/// <item><description>Sanctuary: Full reset to 0</description></item>
/// <item><description>Milestone: Fixed 25 points</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Character with WILL 4 takes Long Rest (WILL × 5 = 20 recovery)
/// var result = StressRecoveryResult.Create(
///     previousStress: 75,
///     newStress: 55,
///     recoverySource: RestType.Long);
///
/// Console.WriteLine(result.AmountRecovered);   // 20
/// Console.WriteLine(result.ThresholdDropped);  // true (Panicked → Anxious)
/// Console.WriteLine(result.IsFullyRecovered);  // false
/// </code>
/// </example>
/// <seealso cref="StressApplicationResult"/>
/// <seealso cref="StressState"/>
/// <seealso cref="RestType"/>
/// <seealso cref="StressThreshold"/>
public readonly record struct StressRecoveryResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored (set in constructor)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the stress value before recovery.
    /// </summary>
    /// <value>
    /// The character's stress level before this recovery event was applied.
    /// Expected to be in [0, 100] from the character's current <see cref="StressState"/>.
    /// </value>
    public int PreviousStress { get; }

    /// <summary>
    /// Gets the stress value after recovery.
    /// </summary>
    /// <value>
    /// The character's stress level after recovery, clamped to a minimum of
    /// <see cref="StressState.MinStress"/> (0).
    /// </value>
    public int NewStress { get; }

    /// <summary>
    /// Gets the amount of stress recovered.
    /// </summary>
    /// <value>
    /// The difference <c>PreviousStress - NewStress</c>. Always non-negative
    /// when the inputs are valid (previousStress &gt;= newStress).
    /// </value>
    public int AmountRecovered { get; }

    /// <summary>
    /// Gets the source of the recovery.
    /// </summary>
    /// <value>
    /// The <see cref="RestType"/> that provided the recovery (Short, Long, Sanctuary, or Milestone).
    /// </value>
    public RestType RecoverySource { get; }

    /// <summary>
    /// Gets the threshold tier before recovery.
    /// </summary>
    /// <value>
    /// The <see cref="StressThreshold"/> corresponding to <see cref="PreviousStress"/>.
    /// </value>
    public StressThreshold PreviousThreshold { get; }

    /// <summary>
    /// Gets the threshold tier after recovery.
    /// </summary>
    /// <value>
    /// The <see cref="StressThreshold"/> corresponding to <see cref="NewStress"/>.
    /// </value>
    public StressThreshold NewThreshold { get; }

    /// <summary>
    /// Gets whether a threshold boundary was crossed downward.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="NewThreshold"/> &lt; <see cref="PreviousThreshold"/>,
    /// indicating the character's psychological state has improved; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Used to trigger positive UI feedback, sound effects, and combat log entries
    /// when a character's stress drops to a better tier.
    /// </para>
    /// </remarks>
    public bool ThresholdDropped { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether stress was fully recovered (now at 0).
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="NewStress"/> equals <see cref="StressState.MinStress"/> (0);
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsFullyRecovered => NewStress == StressState.MinStress;

    /// <summary>
    /// Gets whether the character is now calm (below 20 stress).
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="NewThreshold"/> is <see cref="StressThreshold.Calm"/>
    /// (stress 0-19); otherwise, <c>false</c>.
    /// </value>
    public bool IsNowCalm => NewThreshold == StressThreshold.Calm;

    /// <summary>
    /// Gets the defense penalty improvement from this recovery.
    /// </summary>
    /// <value>
    /// The decrease in defense penalty, calculated as
    /// <c>(PreviousStress / 20) - (NewStress / 20)</c> using integer division.
    /// A positive value indicates the character's defense has improved.
    /// </value>
    public int DefensePenaltyImprovement => (PreviousStress / 20) - (NewStress / 20);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="StressRecoveryResult"/> struct.
    /// </summary>
    /// <param name="previousStress">
    /// The stress value before recovery. Expected to be in [0, 100].
    /// </param>
    /// <param name="newStress">
    /// The stress value after recovery. Will be clamped to a minimum of 0.
    /// </param>
    /// <param name="recoverySource">
    /// The <see cref="RestType"/> that provided the recovery.
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor clamps <paramref name="newStress"/> to a minimum of 0 and
    /// computes all derived properties including threshold tiers and dropping detection.
    /// </para>
    /// </remarks>
    private StressRecoveryResult(
        int previousStress,
        int newStress,
        RestType recoverySource)
    {
        PreviousStress = previousStress;
        NewStress = Math.Max(newStress, StressState.MinStress);
        AmountRecovered = previousStress - NewStress;
        RecoverySource = recoverySource;
        PreviousThreshold = StressThresholdExtensions.FromStressValue(previousStress);
        NewThreshold = StressThresholdExtensions.FromStressValue(NewStress);
        ThresholdDropped = NewThreshold < PreviousThreshold;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a stress recovery result.
    /// </summary>
    /// <param name="previousStress">Stress before recovery. Expected to be in [0, 100].</param>
    /// <param name="newStress">Stress after recovery (will be clamped to min 0).</param>
    /// <param name="recoverySource">The type of rest that provided recovery.</param>
    /// <returns>A new <see cref="StressRecoveryResult"/> instance.</returns>
    /// <example>
    /// <code>
    /// // Long rest recovery
    /// var longRest = StressRecoveryResult.Create(
    ///     previousStress: 75,
    ///     newStress: 55,
    ///     recoverySource: RestType.Long);
    ///
    /// // Sanctuary full recovery
    /// var sanctuary = StressRecoveryResult.Create(
    ///     previousStress: 85,
    ///     newStress: 0,
    ///     recoverySource: RestType.Sanctuary);
    /// // sanctuary.IsFullyRecovered == true
    /// // sanctuary.IsNowCalm == true
    /// </code>
    /// </example>
    public static StressRecoveryResult Create(
        int previousStress,
        int newStress,
        RestType recoverySource) =>
        new(previousStress, newStress, recoverySource);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the stress recovery result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the recovery transition, amount, source, threshold change,
    /// and whether the threshold improved.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = StressRecoveryResult.Create(75, 55, RestType.Long);
    /// var display = result.ToString();
    /// // Returns "Recovery: 75 → 55 (-20) [Long] (Threshold: Panicked → Anxious IMPROVED)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Recovery: {PreviousStress} → {NewStress} (-{AmountRecovered}) [{RecoverySource}] " +
        $"(Threshold: {PreviousThreshold} → {NewThreshold}" +
        $"{(ThresholdDropped ? " IMPROVED" : "")})";
}
