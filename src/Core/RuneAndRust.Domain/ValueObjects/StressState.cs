// ═══════════════════════════════════════════════════════════════════════════════
// StressState.cs
// Immutable value object representing a character's current psychic stress state.
// Encapsulates the stress value (0-100) and derives all dependent properties:
// threshold tier, defense penalty, skill disadvantage, and trauma check trigger.
// Provides factory methods for creation and mutation methods that return new
// instances with updated stress values (preserving immutability).
// Version: 0.18.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents a character's current psychic stress state as an immutable value object.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StressState"/> encapsulates the stress value (clamped to 0-100) and
/// derives all dependent gameplay properties from it. The struct is immutable: all
/// mutation methods return new instances, preserving the value object contract.
/// </para>
/// <para>
/// <strong>Derived Properties:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Threshold"/>: The <see cref="StressThreshold"/> tier determined
///       by the current stress value (e.g., 0-19 = Calm, 20-39 = Uneasy).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="DefensePenalty"/>: The defense stat reduction (0-5) matching
///       the threshold tier ordinal value.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="HasSkillDisadvantage"/>: Whether the character suffers
///       disadvantage on skill checks (Breaking and Trauma tiers).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RequiresTraumaCheck"/>: Whether the character must make a
///       Trauma Check (stress reaches 100).
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Clamping Behavior:</strong> Stress values passed to <see cref="Create"/>
/// are clamped to the [0, 100] range. Negative values become 0; values above 100
/// become 100. This ensures the stress state is always in a valid range without
/// throwing exceptions for overflow/underflow from combat calculations.
/// </para>
/// <para>
/// <strong>Death Spiral:</strong> The stress system intentionally creates a feedback
/// loop — higher stress reduces Defense, making characters more vulnerable to attacks
/// that cause further stress. This is a core design pillar of the Psychic Stress system.
/// </para>
/// </remarks>
/// <seealso cref="StressThreshold"/>
/// <seealso cref="StressThresholdExtensions"/>
/// <seealso cref="RestType"/>
public readonly record struct StressState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The minimum valid stress value (inclusive).
    /// </summary>
    /// <value>0 — represents a character with no accumulated stress.</value>
    public const int MinStress = 0;

    /// <summary>
    /// The maximum valid stress value (inclusive).
    /// </summary>
    /// <value>100 — represents maximum stress, triggering a Trauma Check.</value>
    public const int MaxStress = 100;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Stored
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current stress value, clamped to [0, 100].
    /// </summary>
    /// <value>
    /// An integer in the range [<see cref="MinStress"/>, <see cref="MaxStress"/>].
    /// This is the single source of truth from which all other properties are derived.
    /// </value>
    public int CurrentStress { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Computed (set in constructor for performance)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the stress threshold tier for the current stress value.
    /// </summary>
    /// <value>
    /// The <see cref="StressThreshold"/> enum value corresponding to the current
    /// stress range (e.g., Calm for 0-19, Uneasy for 20-39, etc.).
    /// </value>
    public StressThreshold Threshold { get; }

    /// <summary>
    /// Gets the defense penalty imposed by the current stress threshold.
    /// </summary>
    /// <value>
    /// An integer from 0 (Calm) to 5 (Trauma), subtracted from the character's
    /// effective Defense stat during combat.
    /// </value>
    public int DefensePenalty { get; }

    /// <summary>
    /// Gets a value indicating whether the character has disadvantage on skill checks.
    /// </summary>
    /// <value>
    /// <c>true</c> when the threshold is <see cref="StressThreshold.Breaking"/> (disadvantage
    /// on non-combat checks) or <see cref="StressThreshold.Trauma"/> (disadvantage on ALL
    /// checks); otherwise, <c>false</c>.
    /// </value>
    public bool HasSkillDisadvantage { get; }

    /// <summary>
    /// Gets a value indicating whether a Trauma Check is required.
    /// </summary>
    /// <value>
    /// <c>true</c> when stress has reached <see cref="MaxStress"/> (100), indicating
    /// the character must make a Trauma Check; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// A Trauma Check determines the outcome when a character's stress reaches maximum:
    /// <list type="bullet">
    ///   <item><description>Pass: Stress resets to 75, no permanent effect.</description></item>
    ///   <item><description>Fail: Character gains a permanent Trauma, stress resets to 50.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool RequiresTraumaCheck { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES — Arrow-Expression (derived from stored/computed properties)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether the character is in the Calm threshold.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="Threshold"/> is <see cref="StressThreshold.Calm"/>
    /// (stress 0-19); otherwise, <c>false</c>.
    /// </value>
    public bool IsCalm => Threshold == StressThreshold.Calm;

    /// <summary>
    /// Gets a value indicating whether the character is at Breaking or Trauma threshold.
    /// </summary>
    /// <value>
    /// <c>true</c> when <see cref="Threshold"/> is <see cref="StressThreshold.Breaking"/>
    /// or <see cref="StressThreshold.Trauma"/>; otherwise, <c>false</c>.
    /// </value>
    public bool IsBreaking => Threshold >= StressThreshold.Breaking;

    /// <summary>
    /// Gets the current stress as a percentage of maximum stress.
    /// </summary>
    /// <value>
    /// A double in the range [0.0, 1.0] representing the stress fill percentage.
    /// Useful for progress bar rendering in the TUI.
    /// </value>
    public double StressPercentage => (double)CurrentStress / MaxStress;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR (private — use factory methods)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="StressState"/> struct.
    /// </summary>
    /// <param name="stress">
    /// The raw stress value. Clamped to [<see cref="MinStress"/>, <see cref="MaxStress"/>].
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor clamps the stress value and computes all derived properties.
    /// This ensures the struct is always in a valid, consistent state regardless
    /// of the input value.
    /// </para>
    /// </remarks>
    private StressState(int stress)
    {
        // Clamp stress to valid range [0, 100]
        CurrentStress = Math.Clamp(stress, MinStress, MaxStress);

        // Compute derived properties from clamped stress value
        Threshold = StressThresholdExtensions.FromStressValue(CurrentStress);
        DefensePenalty = Threshold.GetDefensePenalty();
        HasSkillDisadvantage = Threshold >= StressThreshold.Breaking;
        RequiresTraumaCheck = CurrentStress >= MaxStress;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="StressState"/> with the specified stress value.
    /// </summary>
    /// <param name="stress">
    /// The stress value. Clamped to [0, 100] — negative values become 0,
    /// values above 100 become 100.
    /// </param>
    /// <returns>A new <see cref="StressState"/> instance with derived properties computed.</returns>
    /// <example>
    /// <code>
    /// var state = StressState.Create(45);
    /// // state.CurrentStress == 45
    /// // state.Threshold == StressThreshold.Anxious
    /// // state.DefensePenalty == 2
    ///
    /// var clamped = StressState.Create(150);
    /// // clamped.CurrentStress == 100 (clamped)
    /// // clamped.RequiresTraumaCheck == true
    /// </code>
    /// </example>
    public static StressState Create(int stress) => new(stress);

    /// <summary>
    /// Gets a <see cref="StressState"/> representing zero stress (Calm threshold).
    /// </summary>
    /// <value>
    /// A <see cref="StressState"/> with <see cref="CurrentStress"/> = 0,
    /// <see cref="Threshold"/> = <see cref="StressThreshold.Calm"/>,
    /// and no penalties.
    /// </value>
    /// <example>
    /// <code>
    /// var fresh = StressState.Calm;
    /// // fresh.CurrentStress == 0
    /// // fresh.IsCalm == true
    /// // fresh.DefensePenalty == 0
    /// </code>
    /// </example>
    public static StressState Calm => Create(MinStress);

    // ═══════════════════════════════════════════════════════════════════════════
    // MUTATION METHODS (return new instances — immutability preserved)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="StressState"/> with the specified absolute stress value.
    /// </summary>
    /// <param name="newStress">
    /// The new stress value. Clamped to [0, 100].
    /// </param>
    /// <returns>A new <see cref="StressState"/> with the specified stress value.</returns>
    /// <example>
    /// <code>
    /// var state = StressState.Create(30);
    /// var updated = state.WithStress(75);
    /// // updated.CurrentStress == 75
    /// // state.CurrentStress == 30 (unchanged — immutable)
    /// </code>
    /// </example>
    public StressState WithStress(int newStress) => Create(newStress);

    /// <summary>
    /// Creates a new <see cref="StressState"/> with stress increased by the specified amount.
    /// </summary>
    /// <param name="amount">
    /// The amount of stress to add. Must be non-negative.
    /// The result is clamped to <see cref="MaxStress"/>.
    /// </param>
    /// <returns>A new <see cref="StressState"/> with increased stress.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="amount"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var state = StressState.Create(30);
    /// var stressed = state.WithStressAdded(15);
    /// // stressed.CurrentStress == 45
    ///
    /// var capped = state.WithStressAdded(200);
    /// // capped.CurrentStress == 100 (clamped)
    /// </code>
    /// </example>
    public StressState WithStressAdded(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return Create(CurrentStress + amount);
    }

    /// <summary>
    /// Creates a new <see cref="StressState"/> with stress decreased by the specified amount.
    /// </summary>
    /// <param name="amount">
    /// The amount of stress to remove. Must be non-negative.
    /// The result is clamped to <see cref="MinStress"/>.
    /// </param>
    /// <returns>A new <see cref="StressState"/> with reduced stress.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="amount"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var state = StressState.Create(60);
    /// var rested = state.WithStressReduced(20);
    /// // rested.CurrentStress == 40
    ///
    /// var floored = state.WithStressReduced(200);
    /// // floored.CurrentStress == 0 (clamped)
    /// </code>
    /// </example>
    public StressState WithStressReduced(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return Create(CurrentStress - amount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the stress state for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string in the format:
    /// <c>"Stress: {CurrentStress}/{MaxStress} [{Threshold}] (Def: -{DefensePenalty})"</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var state = StressState.Create(45);
    /// var display = state.ToString();
    /// // Returns "Stress: 45/100 [Anxious] (Def: -2)"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"Stress: {CurrentStress}/{MaxStress} [{Threshold}] (Def: -{DefensePenalty})";
}
