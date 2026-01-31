// ═══════════════════════════════════════════════════════════════════════════════
// StressApplicationResult.cs
// Immutable value object representing the complete outcome of applying stress
// to a character. Captures before/after stress values, threshold transitions,
// trauma check triggers, and optional resistance check results.
// Part of the Result Object Pattern for the Trauma Economy system.
// Version: 0.18.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents the result of applying stress to a character.
/// </summary>
/// <remarks>
/// <para>
/// This result object captures the complete state change from a stress application event:
/// <list type="bullet">
/// <item><description>Previous and new stress values (with clamping to 0-100).</description></item>
/// <item><description>Threshold transitions for UI notification (e.g., Uneasy → Panicked).</description></item>
/// <item><description>Trauma check trigger when stress reaches maximum (100).</description></item>
/// <item><description>Optional resistance check result if a WILL check was performed.</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Clamping:</strong> The <see cref="NewStress"/> value is clamped to
/// [0, 100] using <see cref="StressState.MinStress"/> and <see cref="StressState.MaxStress"/>.
/// The <see cref="PreviousStress"/> value is NOT clamped — it is expected to already be
/// in valid range from the character's current <see cref="StressState"/>.
/// </para>
/// <para>
/// <strong>Threshold Crossing:</strong> <see cref="ThresholdCrossed"/> is <c>true</c> when
/// the new threshold tier is strictly greater than the previous tier, indicating the character
/// has entered a worse psychological state.
/// </para>
/// <para>
/// <strong>Trauma Check:</strong> <see cref="TraumaCheckTriggered"/> is <c>true</c> when
/// <see cref="NewStress"/> reaches <see cref="StressState.MaxStress"/> (100), requiring
/// the character to make a Trauma Check.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = StressApplicationResult.Create(
///     previousStress: 35,
///     newStress: 65,
///     source: StressSource.Combat,
///     resistanceResult: checkResult);
///
/// if (result.ThresholdCrossed)
///     ShowThresholdNotification(result.NewThreshold);
///
/// if (result.TraumaCheckTriggered)
///     InitiateTraumaCheck();
/// </code>
/// </example>
/// <seealso cref="StressCheckResult"/>
/// <seealso cref="StressRecoveryResult"/>
/// <seealso cref="StressState"/>
/// <seealso cref="StressThreshold"/>
public readonly record struct StressApplicationResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored (set in constructor)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the stress value before application.
    /// </summary>
    /// <value>
    /// The character's stress level before this stress event was applied.
    /// Expected to be in [0, 100] from the character's current <see cref="StressState"/>.
    /// </value>
    public int PreviousStress { get; }

    /// <summary>
    /// Gets the stress value after application.
    /// </summary>
    /// <value>
    /// The character's stress level after applying stress, clamped to
    /// [<see cref="StressState.MinStress"/>, <see cref="StressState.MaxStress"/>].
    /// </value>
    public int NewStress { get; }

    /// <summary>
    /// Gets the actual stress gained (after any resistance and clamping).
    /// </summary>
    /// <value>
    /// The difference <c>NewStress - PreviousStress</c>. May be less than the
    /// base stress amount due to resistance or clamping at maximum.
    /// </value>
    public int StressGained { get; }

    /// <summary>
    /// Gets the source category of the stress.
    /// </summary>
    /// <value>
    /// The <see cref="StressSource"/> enum value categorizing the origin of this stress
    /// event (e.g., Combat, Exploration, Heretical).
    /// </value>
    public StressSource Source { get; }

    /// <summary>
    /// Gets the threshold tier before stress application.
    /// </summary>
    /// <value>
    /// The <see cref="StressThreshold"/> corresponding to <see cref="PreviousStress"/>.
    /// </value>
    public StressThreshold PreviousThreshold { get; }

    /// <summary>
    /// Gets the threshold tier after stress application.
    /// </summary>
    /// <value>
    /// The <see cref="StressThreshold"/> corresponding to <see cref="NewStress"/>.
    /// </value>
    public StressThreshold NewThreshold { get; }

    /// <summary>
    /// Gets whether a threshold boundary was crossed (stress increased to new tier).
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="NewThreshold"/> &gt; <see cref="PreviousThreshold"/>,
    /// indicating the character has entered a worse psychological state; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Used to trigger UI notifications, sound effects, and combat log entries
    /// when a character's stress enters a new tier.
    /// </para>
    /// </remarks>
    public bool ThresholdCrossed { get; }

    /// <summary>
    /// Gets whether stress reached 100, triggering a Trauma Check.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="NewStress"/> &gt;= <see cref="StressState.MaxStress"/> (100);
    /// otherwise, <c>false</c>.
    /// </value>
    public bool TraumaCheckTriggered { get; }

    /// <summary>
    /// Gets the optional resistance check result, if a check was made.
    /// </summary>
    /// <value>
    /// A <see cref="StressCheckResult"/> if a WILL-based resistance check was performed;
    /// <c>null</c> if no resistance check was attempted (e.g., unavoidable stress from
    /// Narrative or Corruption sources).
    /// </value>
    public StressCheckResult? ResistanceResult { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether resistance was attempted and reduced some stress.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ResistanceResult"/> is not null and its
    /// <see cref="StressCheckResult.Succeeded"/> is <c>true</c>; otherwise, <c>false</c>.
    /// </value>
    public bool WasResisted => ResistanceResult?.Succeeded ?? false;

    /// <summary>
    /// Gets the defense penalty change from this stress application.
    /// </summary>
    /// <value>
    /// The increase in defense penalty, calculated as
    /// <c>(NewStress / 20) - (PreviousStress / 20)</c> using integer division.
    /// A positive value indicates the character's defense has worsened.
    /// </value>
    public int DefensePenaltyChange => (NewStress / 20) - (PreviousStress / 20);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="StressApplicationResult"/> struct.
    /// </summary>
    /// <param name="previousStress">
    /// The stress value before application. Expected to be in [0, 100].
    /// </param>
    /// <param name="newStress">
    /// The stress value after application. Will be clamped to [0, 100].
    /// </param>
    /// <param name="source">
    /// The <see cref="StressSource"/> category for this stress event.
    /// </param>
    /// <param name="resistanceResult">
    /// Optional <see cref="StressCheckResult"/> if a resistance check was performed.
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor clamps <paramref name="newStress"/> to the valid range and
    /// computes all derived properties including threshold tiers, crossing detection,
    /// and trauma check triggers.
    /// </para>
    /// </remarks>
    private StressApplicationResult(
        int previousStress,
        int newStress,
        StressSource source,
        StressCheckResult? resistanceResult)
    {
        PreviousStress = previousStress;
        NewStress = Math.Clamp(newStress, StressState.MinStress, StressState.MaxStress);
        StressGained = NewStress - previousStress;
        Source = source;
        PreviousThreshold = StressThresholdExtensions.FromStressValue(previousStress);
        NewThreshold = StressThresholdExtensions.FromStressValue(NewStress);
        ThresholdCrossed = NewThreshold > PreviousThreshold;
        TraumaCheckTriggered = NewStress >= StressState.MaxStress;
        ResistanceResult = resistanceResult;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a stress application result.
    /// </summary>
    /// <param name="previousStress">Stress before application. Expected to be in [0, 100].</param>
    /// <param name="newStress">Stress after application (will be clamped to [0, 100]).</param>
    /// <param name="source">The source category of the stress.</param>
    /// <param name="resistanceResult">Optional resistance check result.</param>
    /// <returns>A new <see cref="StressApplicationResult"/> instance.</returns>
    /// <example>
    /// <code>
    /// // Stress applied with resistance check
    /// var withResistance = StressApplicationResult.Create(
    ///     previousStress: 30,
    ///     newStress: 55,
    ///     source: StressSource.Combat,
    ///     resistanceResult: StressCheckResult.Create(1, 50));
    ///
    /// // Unavoidable stress (no resistance)
    /// var unavoidable = StressApplicationResult.Create(
    ///     previousStress: 50,
    ///     newStress: 100,
    ///     source: StressSource.Narrative);
    /// </code>
    /// </example>
    public static StressApplicationResult Create(
        int previousStress,
        int newStress,
        StressSource source,
        StressCheckResult? resistanceResult = null) =>
        new(previousStress, newStress, source, resistanceResult);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the stress application result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the stress transition, source, threshold change,
    /// and any triggered conditions (CROSSED, TRAUMA!).
    /// </returns>
    /// <example>
    /// <code>
    /// var result = StressApplicationResult.Create(35, 65, StressSource.Combat);
    /// var display = result.ToString();
    /// // Returns "Stress: 35 → 65 [Combat] (Threshold: Uneasy → Panicked CROSSED)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Stress: {PreviousStress} → {NewStress} [{Source}] " +
        $"(Threshold: {PreviousThreshold} → {NewThreshold}" +
        $"{(ThresholdCrossed ? " CROSSED" : "")}" +
        $"{(TraumaCheckTriggered ? " TRAUMA!" : "")})";
}
