// ═══════════════════════════════════════════════════════════════════════════════
// CpsState.cs
// Represents a character's current Cognitive Paradox Syndrome (CPS) state as an
// immutable value object. CPS tracks mental deterioration from processing
// reality-bending paradoxes. Unlike CorruptionState (physical taint), CpsState
// derives from Psychic Stress and can recover when stress is reduced.
// Version: 0.18.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an immutable snapshot of a character's Cognitive Paradox Syndrome state.
/// </summary>
/// <remarks>
/// <para>
/// CpsState is derived from Psychic Stress and represents the mind's deterioration
/// from processing reality-bending paradoxes. Unlike CorruptionState, CPS can
/// improve when stress is reduced through rest or abilities.
/// </para>
/// <para>
/// <strong>Stage Thresholds:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>None: 0-19 stress — Clear-minded</description></item>
///   <item><description>WeightOfKnowing: 20-39 stress — Reality feels "off"</description></item>
///   <item><description>GlimmerMadness: 40-59 stress — Reality flickers</description></item>
///   <item><description>RuinMadness: 60-79 stress — Panic Table active</description></item>
///   <item><description>HollowShell: 80+ stress — Terminal state</description></item>
/// </list>
/// <para>
/// <strong>Key Differences from CorruptionState:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>CPS derives from Psychic Stress (recoverable)</description></item>
///   <item><description>Corruption is near-permanent physical taint</description></item>
///   <item><description>CPS uses 5 stages (vs 6 for Corruption)</description></item>
///   <item><description>CPS terminal state is HollowShell at 80+ (vs Consumed at 100)</description></item>
/// </list>
/// <para>
/// <strong>Clamping Behavior:</strong> The stress value is automatically clamped
/// to [0, 100]. Negative values become 0, values above 100 become 100. This ensures
/// the CPS state is always in a valid range without throwing exceptions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic usage
/// var cpsState = CpsState.Create(stress: 65);
/// // cpsState.Stage == CpsStage.RuinMadness
/// // cpsState.RequiresPanicCheck == true
/// // cpsState.IsTerminal == false
/// // cpsState.PercentageToHollowShell == 0.8125
///
/// // Boundary examples
/// var none = CpsState.Create(19);   // Stage: None
/// var weight = CpsState.Create(20); // Stage: WeightOfKnowing
/// var terminal = CpsState.Create(80); // Stage: HollowShell, IsTerminal: true
///
/// // Clamping
/// var clamped = CpsState.Create(150);
/// // clamped.CurrentStress == 100 (clamped to max)
/// </code>
/// </example>
/// <seealso cref="CpsStage"/>
/// <seealso cref="PanicEffect"/>
/// <seealso cref="StressState"/>
/// <seealso cref="CorruptionState"/>
public readonly record struct CpsState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS — Stress Range and Stage Thresholds
    // ═══════════════════════════════════════════════════════════════════════════

    #region Constants

    /// <summary>
    /// Minimum stress value.
    /// </summary>
    public const int MinStress = 0;

    /// <summary>
    /// Maximum stress value.
    /// </summary>
    public const int MaxStress = 100;

    /// <summary>
    /// Stress threshold for WeightOfKnowing stage.
    /// </summary>
    /// <remarks>
    /// At 20+ stress, the character enters the first stage of CPS.
    /// Reality begins to feel subtly "off".
    /// </remarks>
    public const int WeightOfKnowingThreshold = 20;

    /// <summary>
    /// Stress threshold for GlimmerMadness stage.
    /// </summary>
    /// <remarks>
    /// At 40+ stress, reality actively flickers and distorts.
    /// Objects may appear duplicated or distorted.
    /// </remarks>
    public const int GlimmerMadnessThreshold = 40;

    /// <summary>
    /// Stress threshold for RuinMadness stage (Panic Table active).
    /// </summary>
    /// <remarks>
    /// At 60+ stress, the mind fractures under paradox weight.
    /// Panic Table rolls are triggered on stress-inducing events.
    /// </remarks>
    public const int RuinMadnessThreshold = 60;

    /// <summary>
    /// Stress threshold for HollowShell stage (Terminal).
    /// </summary>
    /// <remarks>
    /// At 80+ stress, the mind has shattered. Character must
    /// pass survival check or be lost permanently.
    /// </remarks>
    public const int HollowShellThreshold = 80;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — CPS State Data
    // ═══════════════════════════════════════════════════════════════════════════

    #region Properties

    /// <summary>
    /// Gets the current psychic stress level (0-100).
    /// </summary>
    /// <value>
    /// An integer between <see cref="MinStress"/> and <see cref="MaxStress"/> inclusive.
    /// </value>
    public int CurrentStress { get; private init; }

    /// <summary>
    /// Gets the current CPS stage based on stress level.
    /// </summary>
    /// <value>
    /// The <see cref="CpsStage"/> corresponding to the current stress level.
    /// </value>
    public CpsStage Stage { get; private init; }

    /// <summary>
    /// Gets whether the character requires Panic Table checks.
    /// </summary>
    /// <remarks>
    /// True when Stage is RuinMadness or HollowShell.
    /// Panic checks are triggered by combat/horror events.
    /// </remarks>
    /// <value>
    /// <c>true</c> if the character is in RuinMadness or HollowShell stage;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool RequiresPanicCheck { get; private init; }

    /// <summary>
    /// Gets whether the character is in terminal CPS state.
    /// </summary>
    /// <remarks>
    /// True when Stage is HollowShell. Character must pass
    /// survival check or become unplayable.
    /// </remarks>
    /// <value>
    /// <c>true</c> if the character is in HollowShell stage;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsTerminal { get; private init; }

    /// <summary>
    /// Gets the percentage progress toward HollowShell threshold.
    /// </summary>
    /// <remarks>
    /// Calculated as CurrentStress / HollowShellThreshold (80).
    /// Capped at 1.0 (100%) when stress ≥ 80.
    /// </remarks>
    /// <value>
    /// A double between 0.0 and 1.0 representing progress toward terminal state.
    /// </value>
    public double PercentageToHollowShell { get; private init; }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // ARROW-EXPRESSION PROPERTIES — Convenience Accessors
    // ═══════════════════════════════════════════════════════════════════════════

    #region Arrow-Expression Properties

    /// <summary>
    /// Gets whether the character has no CPS symptoms.
    /// </summary>
    /// <value>
    /// <c>true</c> if Stage is None (stress 0-19); otherwise, <c>false</c>.
    /// </value>
    public bool IsClear => Stage == CpsStage.None;

    /// <summary>
    /// Gets whether the character is in HollowShell stage.
    /// </summary>
    /// <value>
    /// <c>true</c> if Stage is HollowShell (stress 80+); otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Alias for <see cref="IsTerminal"/> for naming consistency with design docs.
    /// </remarks>
    public bool IsHollow => Stage == CpsStage.HollowShell;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS — State Creation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Factory Methods

    /// <summary>
    /// Creates a new CpsState from the given stress value.
    /// </summary>
    /// <param name="stress">
    /// The current psychic stress (0-100). Values outside this range are clamped.
    /// </param>
    /// <returns>An immutable CpsState snapshot.</returns>
    /// <example>
    /// <code>
    /// var state = CpsState.Create(45);
    /// // state.Stage == CpsStage.GlimmerMadness
    /// // state.RequiresPanicCheck == false
    /// // state.IsTerminal == false
    ///
    /// var terminal = CpsState.Create(85);
    /// // terminal.Stage == CpsStage.HollowShell
    /// // terminal.IsTerminal == true
    /// </code>
    /// </example>
    public static CpsState Create(int stress)
    {
        var clampedStress = Math.Clamp(stress, MinStress, MaxStress);
        var stage = DetermineStage(clampedStress);

        return new CpsState
        {
            CurrentStress = clampedStress,
            Stage = stage,
            RequiresPanicCheck = stage >= CpsStage.RuinMadness,
            IsTerminal = stage == CpsStage.HollowShell,
            PercentageToHollowShell = Math.Min(1.0, clampedStress / (double)HollowShellThreshold)
        };
    }

    /// <summary>
    /// Determines the CPS stage for a given stress value.
    /// </summary>
    /// <param name="stress">The psychic stress value.</param>
    /// <returns>The corresponding CpsStage.</returns>
    /// <remarks>
    /// Stage thresholds:
    /// <list type="bullet">
    ///   <item><description>0-19: None</description></item>
    ///   <item><description>20-39: WeightOfKnowing</description></item>
    ///   <item><description>40-59: GlimmerMadness</description></item>
    ///   <item><description>60-79: RuinMadness</description></item>
    ///   <item><description>80+: HollowShell</description></item>
    /// </list>
    /// </remarks>
    public static CpsStage DetermineStage(int stress) =>
        stress switch
        {
            >= HollowShellThreshold => CpsStage.HollowShell,
            >= RuinMadnessThreshold => CpsStage.RuinMadness,
            >= GlimmerMadnessThreshold => CpsStage.GlimmerMadness,
            >= WeightOfKnowingThreshold => CpsStage.WeightOfKnowing,
            _ => CpsStage.None
        };

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY — String Representation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Display

    /// <summary>
    /// Returns a string representation of the CPS state for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string in the format:
    /// <c>"CPS[{Stage}]: Stress={CurrentStress}"</c>,
    /// with optional <c>[PANIC CHECK]</c> and <c>[TERMINAL!]</c> suffixes.
    /// </returns>
    /// <example>
    /// <code>
    /// var clear = CpsState.Create(15);
    /// // Returns "CPS[None]: Stress=15"
    ///
    /// var ruin = CpsState.Create(65);
    /// // Returns "CPS[RuinMadness]: Stress=65 [PANIC CHECK]"
    ///
    /// var hollow = CpsState.Create(85);
    /// // Returns "CPS[HollowShell]: Stress=85 [PANIC CHECK] [TERMINAL!]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"CPS[{Stage}]: Stress={CurrentStress}" +
        (RequiresPanicCheck ? " [PANIC CHECK]" : "") +
        (IsTerminal ? " [TERMINAL!]" : "");

    #endregion
}
