// ═══════════════════════════════════════════════════════════════════════════════
// CpsStageChangeResult.cs
// Represents the result of checking for CPS stage changes. This record detects
// when a character's CPS stage transitions due to stress fluctuations and
// provides computed properties to identify critical transitions requiring
// special handling (RuinMadness entry enables Panic Table, HollowShell requires
// survival check).
// Version: 0.18.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of checking for CPS stage changes.
/// </summary>
/// <remarks>
/// <para>
/// This record is used to detect when a character's CPS stage changes
/// due to stress fluctuations. It provides computed properties to identify
/// critical transitions that require special handling.
/// </para>
/// <para>
/// Critical Transitions:
/// <list type="bullet">
/// <item>EnteredRuinMadness — Enables Panic Table rolls</item>
/// <item>EnteredHollowShell — Requires survival check</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="PreviousStage">The CPS stage before the change.</param>
/// <param name="NewStage">The CPS stage after the change.</param>
/// <seealso cref="CpsStage"/>
/// <seealso cref="CpsState"/>
/// <seealso cref="PanicResult"/>
public readonly record struct CpsStageChangeResult(
    CpsStage PreviousStage,
    CpsStage NewStage)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ARROW-EXPRESSION PROPERTIES — Change Detection
    // ═══════════════════════════════════════════════════════════════════════════

    #region Arrow-Expression Properties

    /// <summary>
    /// Gets whether the CPS stage changed at all.
    /// </summary>
    /// <value>
    /// <c>true</c> if PreviousStage differs from NewStage; otherwise, <c>false</c>.
    /// </value>
    public bool StageChanged => PreviousStage != NewStage;

    /// <summary>
    /// Gets whether the CPS stage worsened (increased severity).
    /// </summary>
    /// <value>
    /// <c>true</c> if NewStage is greater than PreviousStage; otherwise, <c>false</c>.
    /// </value>
    public bool StageWorsened => NewStage > PreviousStage;

    /// <summary>
    /// Gets whether the CPS stage improved (decreased severity).
    /// </summary>
    /// <value>
    /// <c>true</c> if NewStage is less than PreviousStage; otherwise, <c>false</c>.
    /// </value>
    public bool StageImproved => NewStage < PreviousStage;

    /// <summary>
    /// Gets whether the character just entered RuinMadness stage.
    /// </summary>
    /// <remarks>
    /// True only when transitioning FROM a lower stage TO RuinMadness.
    /// This triggers enabling of Panic Table rolls.
    /// </remarks>
    /// <value>
    /// <c>true</c> if transitioning into RuinMadness from a lower stage;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool EnteredRuinMadness =>
        NewStage == CpsStage.RuinMadness && PreviousStage < CpsStage.RuinMadness;

    /// <summary>
    /// Gets whether the character just entered HollowShell stage.
    /// </summary>
    /// <remarks>
    /// True only when transitioning FROM a lower stage TO HollowShell.
    /// This triggers the survival check requirement.
    /// </remarks>
    /// <value>
    /// <c>true</c> if transitioning into HollowShell from a lower stage;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool EnteredHollowShell =>
        NewStage == CpsStage.HollowShell && PreviousStage < CpsStage.HollowShell;

    /// <summary>
    /// Gets whether this represents a critical transition requiring special handling.
    /// </summary>
    /// <value>
    /// <c>true</c> if the character entered RuinMadness or HollowShell;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsCriticalTransition => EnteredRuinMadness || EnteredHollowShell;

    /// <summary>
    /// Gets whether the character left RuinMadness (recovered from panic zone).
    /// </summary>
    /// <value>
    /// <c>true</c> if transitioning from RuinMadness to a lower stage;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool LeftRuinMadness =>
        PreviousStage == CpsStage.RuinMadness && NewStage < CpsStage.RuinMadness;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS — Result Creation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Factory Methods

    /// <summary>
    /// Creates a result representing no change.
    /// </summary>
    /// <param name="currentStage">The current (unchanged) stage.</param>
    /// <returns>A CpsStageChangeResult where PreviousStage equals NewStage.</returns>
    public static CpsStageChangeResult NoChange(CpsStage currentStage) =>
        new(currentStage, currentStage);

    /// <summary>
    /// Creates a result from two stress values.
    /// </summary>
    /// <param name="previousStress">The previous stress value (0-100).</param>
    /// <param name="newStress">The new stress value (0-100).</param>
    /// <returns>
    /// A CpsStageChangeResult with stages determined from the stress values.
    /// </returns>
    /// <remarks>
    /// Uses <see cref="CpsState.DetermineStage"/> to convert stress values
    /// to CPS stages, then creates a result for comparison.
    /// </remarks>
    public static CpsStageChangeResult FromStressChange(int previousStress, int newStress)
    {
        var previousStage = CpsState.DetermineStage(previousStress);
        var newStage = CpsState.DetermineStage(newStress);
        return new CpsStageChangeResult(previousStage, newStage);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY — String Representation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Display

    /// <summary>
    /// Returns a string representation of the stage change result.
    /// </summary>
    /// <returns>
    /// A formatted string describing the stage change, direction, and criticality.
    /// </returns>
    /// <example>
    /// <code>
    /// var noChange = CpsStageChangeResult.NoChange(CpsStage.GlimmerMadness);
    /// // Returns "CPS Stage: GlimmerMadness (unchanged)"
    ///
    /// var worsened = new CpsStageChangeResult(CpsStage.GlimmerMadness, CpsStage.RuinMadness);
    /// // Returns "CPS Stage WORSENED: GlimmerMadness → RuinMadness [CRITICAL!]"
    ///
    /// var improved = new CpsStageChangeResult(CpsStage.WeightOfKnowing, CpsStage.None);
    /// // Returns "CPS Stage IMPROVED: WeightOfKnowing → None"
    /// </code>
    /// </example>
    public override string ToString()
    {
        if (!StageChanged)
            return $"CPS Stage: {NewStage} (unchanged)";

        var direction = StageWorsened ? "WORSENED" : "IMPROVED";
        var critical = IsCriticalTransition ? " [CRITICAL!]" : "";
        return $"CPS Stage {direction}: {PreviousStage} → {NewStage}{critical}";
    }

    #endregion
}
