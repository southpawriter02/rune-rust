// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionAddResult.cs
// Immutable value object representing the result of adding corruption to a
// character's CorruptionTracker. Captures the complete before/after state of a
// corruption change, including threshold crossings, stage transitions, faction
// lock status, and Terminal Error detection. Used by CorruptionTracker.AddCorruption
// and consumed by services, UI, and combat log systems.
// Version: 0.18.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of adding corruption to a character.
/// </summary>
/// <remarks>
/// <para>
/// This immutable record captures the complete before/after state of a
/// corruption change, including threshold crossings and stage transitions.
/// It is returned by <see cref="Entities.CorruptionTracker.AddCorruption"/>
/// and consumed by the service layer for logging, event emission, and UI updates.
/// </para>
/// <para>
/// Key information captured:
/// <list type="bullet">
///   <item><description>Previous and new corruption values with computed delta.</description></item>
///   <item><description>Corruption source for history tracking and analytics.</description></item>
///   <item><description>Threshold crossing (25/50/75 or null) — each fires exactly once per character.</description></item>
///   <item><description>Stage transition detection (e.g., Tainted to Infected).</description></item>
///   <item><description>Terminal Error flag when corruption reaches 100.</description></item>
///   <item><description>Faction lock detection when corruption first crosses 50.</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Threshold Crossings vs Stage Crossings:</strong> Threshold crossings
/// (25/50/75) are one-time flags that trigger narrative events. Stage crossings
/// occur whenever the corruption moves between stage boundaries (every 20 points).
/// A single corruption addition can trigger both a threshold crossing and a stage
/// crossing simultaneously.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = tracker.AddCorruption(20, CorruptionSource.Environmental);
///
/// if (result.ThresholdCrossed.HasValue)
///     Console.WriteLine($"Crossed {result.ThresholdCrossed}% threshold!");
///
/// if (result.StageCrossed)
///     Console.WriteLine($"Stage changed: {result.PreviousStage} -> {result.NewStage}");
///
/// if (result.IsTerminalError)
///     Console.WriteLine("TERMINAL ERROR - Initiate survival check!");
///
/// if (result.NowFactionLocked)
///     Console.WriteLine("Faction reputation is now locked!");
/// </code>
/// </example>
/// <seealso cref="Entities.CorruptionTracker"/>
/// <seealso cref="CorruptionSource"/>
/// <seealso cref="CorruptionStage"/>
public readonly record struct CorruptionAddResult
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the corruption value before the change.
    /// </summary>
    /// <value>
    /// The character's corruption level immediately before this corruption
    /// addition was applied. Range: [0, 100].
    /// </value>
    public int PreviousCorruption { get; }

    /// <summary>
    /// Gets the corruption value after the change.
    /// </summary>
    /// <value>
    /// The character's corruption level after applying the addition and
    /// clamping to [0, 100]. This is the new current corruption value.
    /// </value>
    public int NewCorruption { get; }

    /// <summary>
    /// Gets the actual amount of corruption gained (after clamping).
    /// </summary>
    /// <value>
    /// The difference between <see cref="NewCorruption"/> and
    /// <see cref="PreviousCorruption"/>. May be less than the requested
    /// amount if clamping occurred at the upper bound (100). May be
    /// negative if corruption was reduced (rare removal operations).
    /// </value>
    public int AmountGained { get; }

    /// <summary>
    /// Gets the source category of the corruption.
    /// </summary>
    /// <value>
    /// The <see cref="CorruptionSource"/> enum value categorizing the
    /// origin of this corruption (e.g., MysticMagic, HereticalAbility,
    /// Environmental, BlightTransfer).
    /// </value>
    public CorruptionSource Source { get; }

    /// <summary>
    /// Gets the threshold that was crossed, if any (25, 50, 75, or null).
    /// </summary>
    /// <value>
    /// The threshold value (25, 50, or 75) if this corruption addition
    /// caused the character to cross that threshold for the first time;
    /// <c>null</c> if no threshold was crossed.
    /// </value>
    /// <remarks>
    /// Each threshold can only be crossed once per character lifetime.
    /// Used to trigger narrative events and unlock certain mechanics:
    /// <list type="bullet">
    ///   <item><description>25: UI warning — "You feel the Blight's touch..."</description></item>
    ///   <item><description>50: Faction reputation lock — human faction gains blocked.</description></item>
    ///   <item><description>75: Machine Affinity trauma acquired.</description></item>
    /// </list>
    /// </remarks>
    public int? ThresholdCrossed { get; }

    /// <summary>
    /// Gets whether a stage boundary was crossed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the corruption change caused the character to
    /// move from one <see cref="CorruptionStage"/> to another (e.g.,
    /// Tainted to Infected); <c>false</c> if they remain in the same stage.
    /// </value>
    public bool StageCrossed { get; }

    /// <summary>
    /// Gets the corruption stage before the change.
    /// </summary>
    /// <value>
    /// The <see cref="CorruptionStage"/> the character was in immediately
    /// before this corruption addition.
    /// </value>
    public CorruptionStage PreviousStage { get; }

    /// <summary>
    /// Gets the corruption stage after the change.
    /// </summary>
    /// <value>
    /// The <see cref="CorruptionStage"/> the character is in after this
    /// corruption addition. Compare with <see cref="PreviousStage"/>
    /// to detect stage transitions.
    /// </value>
    public CorruptionStage NewStage { get; }

    /// <summary>
    /// Gets whether corruption reached 100 (Terminal Error).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="NewCorruption"/> is 100 or higher,
    /// indicating the character must immediately resolve a Terminal Error
    /// survival check; <c>false</c> otherwise.
    /// </value>
    public bool IsTerminalError { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether faction reputation became locked as a result of this change.
    /// </summary>
    /// <value>
    /// <c>true</c> if the corruption crossed from below 50 to 50 or above
    /// in this specific change, meaning the character's faction reputation
    /// is now locked for the first time; <c>false</c> if it was already
    /// locked or did not cross the 50 threshold.
    /// </value>
    public bool NowFactionLocked => NewCorruption >= 50 && PreviousCorruption < 50;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor to enforce factory pattern.
    /// </summary>
    /// <param name="previousCorruption">Corruption value before the change.</param>
    /// <param name="newCorruption">Corruption value after the change.</param>
    /// <param name="source">The corruption source category.</param>
    /// <param name="thresholdCrossed">The threshold crossed, if any.</param>
    /// <param name="previousStage">The stage before the change.</param>
    /// <param name="newStage">The stage after the change.</param>
    private CorruptionAddResult(
        int previousCorruption,
        int newCorruption,
        CorruptionSource source,
        int? thresholdCrossed,
        CorruptionStage previousStage,
        CorruptionStage newStage)
    {
        PreviousCorruption = previousCorruption;
        NewCorruption = newCorruption;
        AmountGained = newCorruption - previousCorruption;
        Source = source;
        ThresholdCrossed = thresholdCrossed;
        StageCrossed = newStage != previousStage;
        PreviousStage = previousStage;
        NewStage = newStage;
        IsTerminalError = newCorruption >= Entities.CorruptionTracker.MaxCorruption;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a corruption add result.
    /// </summary>
    /// <param name="previousCorruption">Corruption value before the change.</param>
    /// <param name="newCorruption">Corruption value after the change.</param>
    /// <param name="source">The corruption source category.</param>
    /// <param name="thresholdCrossed">The threshold crossed, if any (25, 50, 75, or null).</param>
    /// <param name="previousStage">The <see cref="CorruptionStage"/> before the change.</param>
    /// <param name="newStage">The <see cref="CorruptionStage"/> after the change.</param>
    /// <returns>
    /// A new <see cref="CorruptionAddResult"/> with all properties computed from the inputs.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = CorruptionAddResult.Create(
    ///     previousCorruption: 20,
    ///     newCorruption: 55,
    ///     source: CorruptionSource.HereticalAbility,
    ///     thresholdCrossed: 25,
    ///     previousStage: CorruptionStage.Tainted,
    ///     newStage: CorruptionStage.Infected);
    /// </code>
    /// </example>
    public static CorruptionAddResult Create(
        int previousCorruption,
        int newCorruption,
        CorruptionSource source,
        int? thresholdCrossed,
        CorruptionStage previousStage,
        CorruptionStage newStage) =>
        new(previousCorruption, newCorruption, source, thresholdCrossed, previousStage, newStage);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the corruption add result for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the corruption transition, source, and any
    /// threshold/stage crossings or Terminal Error status.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = tracker.AddCorruption(20, CorruptionSource.Environmental);
    /// var display = result.ToString();
    /// // Returns "Corruption: 20 -> 40 [Environmental] (Crossed 25%) Stage: Tainted -> Infected"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Corruption: {PreviousCorruption} -> {NewCorruption} [{Source}]" +
        (ThresholdCrossed.HasValue ? $" (Crossed {ThresholdCrossed}%)" : "") +
        (StageCrossed ? $" Stage: {PreviousStage} -> {NewStage}" : "") +
        (IsTerminalError ? " [TERMINAL ERROR!]" : "");
}
