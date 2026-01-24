// ------------------------------------------------------------------------------
// <copyright file="JuryRigContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Context for a jury-rigging bypass attempt, capturing method, tools, and modifiers.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for a jury-rigging bypass attempt, capturing method, tools, and modifiers.
/// </summary>
/// <remarks>
/// <para>
/// This value object encapsulates all context needed for a bypass attempt:
/// <list type="bullet">
///   <item><description>The chosen bypass method and its DC modifier</description></item>
///   <item><description>Character familiarity with the mechanism type</description></item>
///   <item><description>Tool quality and associated dice modifier</description></item>
///   <item><description>Electrocution risk flag for Wire Manipulation</description></item>
///   <item><description>Complication roll if applicable</description></item>
/// </list>
/// </para>
/// <para>
/// Use the static <see cref="Create"/> factory method to construct contexts
/// with proper validation and modifier calculation.
/// </para>
/// </remarks>
/// <param name="MethodUsed">The bypass method selected for this attempt.</param>
/// <param name="FamiliarMechanism">Whether the character is familiar with this mechanism type.</param>
/// <param name="ToolsAvailable">The quality of tools available for the attempt.</param>
/// <param name="ElectrocutionRisk">Whether this attempt carries electrocution risk.</param>
/// <param name="ComplicationRoll">The d10 complication roll result, if applicable.</param>
public readonly record struct JuryRigContext(
    BypassMethod MethodUsed,
    bool FamiliarMechanism,
    ToolQuality ToolsAvailable,
    bool ElectrocutionRisk,
    int? ComplicationRoll)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The number of bonus dice granted for mechanism familiarity.
    /// </summary>
    /// <remarks>
    /// When a character has previously bypassed a mechanism of the same type,
    /// they gain +2d10 bonus dice on the experiment roll.
    /// </remarks>
    public const int FamiliarityBonusDice = 2;

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the DC modifier from the selected bypass method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Method DC modifiers range from -4 (GlitchExploitation) to +2 (BruteDisassembly).
    /// Negative modifiers make the bypass easier; positive make it harder.
    /// </para>
    /// </remarks>
    public int MethodDcModifier => MethodUsed.GetDcModifier();

    /// <summary>
    /// Gets the dice pool modifier from tool quality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tool quality modifiers:
    /// <list type="bullet">
    ///   <item><description>Improvised: -2 dice</description></item>
    ///   <item><description>Standard: +0 dice</description></item>
    ///   <item><description>Quality: +1 die</description></item>
    ///   <item><description>Masterwork: +2 dice</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int ToolDiceModifier => ToolsAvailable.GetDiceModifier();

    /// <summary>
    /// Gets the bonus dice earned from mechanism familiarity.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="FamiliarityBonusDice"/> (2) if the character is familiar
    /// with this mechanism type, otherwise 0.
    /// </remarks>
    public int EarnedFamiliarityDice => FamiliarMechanism ? FamiliarityBonusDice : 0;

    /// <summary>
    /// Gets the total dice pool modifier from all sources.
    /// </summary>
    /// <remarks>
    /// Combines tool quality modifier and familiarity bonus dice.
    /// Does not include the base dice pool from the character's System Bypass skill.
    /// </remarks>
    public int TotalDiceModifier => ToolDiceModifier + EarnedFamiliarityDice;

    /// <summary>
    /// Gets a value indicating whether the method destroys the mechanism on success.
    /// </summary>
    /// <remarks>
    /// Brute Disassembly tears the mechanism apart to gain access.
    /// The mechanism cannot be reused, but salvage is guaranteed.
    /// </remarks>
    public bool WillDestroyMechanism => MethodUsed.DestroysMechanism();

    /// <summary>
    /// Gets a value indicating whether the method resets mechanism state.
    /// </summary>
    /// <remarks>
    /// Power Cycling resets the mechanism to factory defaults, clearing
    /// lockouts, alerts, and partial progress from previous attempts.
    /// </remarks>
    public bool WillResetMechanism => MethodUsed.ResetsMechanismState();

    /// <summary>
    /// Gets a value indicating whether the method guarantees salvage on success.
    /// </summary>
    /// <remarks>
    /// Brute Disassembly always yields salvage because the mechanism is
    /// systematically torn apart. Other methods only yield salvage on
    /// critical success (net successes >= 5).
    /// </remarks>
    public bool GuaranteesSalvage => MethodUsed.GuaranteesSalvage();

    // -------------------------------------------------------------------------
    // Validation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if the method is valid for the given mechanism state.
    /// </summary>
    /// <param name="isGlitched">Whether the mechanism is in a glitched state.</param>
    /// <returns>True if the method can be used in this context.</returns>
    /// <remarks>
    /// <para>
    /// Method validity depends on:
    /// <list type="bullet">
    ///   <item><description>MemorizedSequence: Requires familiarity with mechanism type</description></item>
    ///   <item><description>GlitchExploitation: Requires mechanism to be [Glitched]</description></item>
    ///   <item><description>Other methods: Always valid</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool IsMethodValid(bool isGlitched)
    {
        return MethodUsed.IsValidForContext(FamiliarMechanism, isGlitched);
    }

    /// <summary>
    /// Gets the reason why the method is invalid, or null if valid.
    /// </summary>
    /// <param name="isGlitched">Whether the mechanism is in a glitched state.</param>
    /// <returns>An error message if invalid, or null if the method is valid.</returns>
    public string? GetInvalidReason(bool isGlitched)
    {
        return MethodUsed.GetInvalidReason(FamiliarMechanism, isGlitched);
    }

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new jury-rig context with the specified parameters.
    /// </summary>
    /// <param name="method">The bypass method to use.</param>
    /// <param name="isFamiliar">Whether the character is familiar with this mechanism type.</param>
    /// <param name="tools">The quality of tools available.</param>
    /// <returns>A new <see cref="JuryRigContext"/> configured with the specified parameters.</returns>
    /// <remarks>
    /// The electrocution risk flag is automatically set based on the bypass method.
    /// Wire Manipulation carries electrocution risk; other methods do not.
    /// </remarks>
    public static JuryRigContext Create(
        BypassMethod method,
        bool isFamiliar,
        ToolQuality tools)
    {
        return new JuryRigContext(
            MethodUsed: method,
            FamiliarMechanism: isFamiliar,
            ToolsAvailable: tools,
            ElectrocutionRisk: method.HasElectrocutionRisk(),
            ComplicationRoll: null);
    }

    /// <summary>
    /// Creates a new context with a complication roll result.
    /// </summary>
    /// <param name="roll">The d10 complication roll result (1-10).</param>
    /// <returns>A new <see cref="JuryRigContext"/> with the complication roll added.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="roll"/> is not between 1 and 10.
    /// </exception>
    /// <remarks>
    /// Complication rolls are made when an experiment fails (net successes &lt;= 0).
    /// The roll determines the consequence of failure from the complication table.
    /// </remarks>
    public JuryRigContext WithComplicationRoll(int roll)
    {
        if (roll < 1 || roll > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roll),
                roll,
                "Complication roll must be between 1 and 10.");
        }

        return this with { ComplicationRoll = roll };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the context.
    /// </summary>
    /// <returns>A human-readable summary of the jury-rig context.</returns>
    public string ToDisplayString()
    {
        var parts = new List<string>
        {
            $"Method: {MethodUsed.GetDisplayName()} ({MethodUsed.GetDcModifierDisplay()})"
        };

        if (FamiliarMechanism)
        {
            parts.Add($"Familiar: +{EarnedFamiliarityDice}d10");
        }

        parts.Add($"Tools: {ToolsAvailable.GetDisplayName()} ({ToolsAvailable.GetDiceModifierDisplay()})");

        if (ElectrocutionRisk)
        {
            parts.Add("⚡ Electrocution Risk");
        }

        if (ComplicationRoll.HasValue)
        {
            parts.Add($"Complication Roll: {ComplicationRoll.Value}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"JuryRigContext[Method={MethodUsed} DCMod={MethodDcModifier} " +
               $"Familiar={FamiliarMechanism} Tools={ToolsAvailable} " +
               $"DiceMod={TotalDiceModifier} Electrocution={ElectrocutionRisk}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
