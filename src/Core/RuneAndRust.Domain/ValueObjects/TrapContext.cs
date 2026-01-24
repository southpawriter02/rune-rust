// ------------------------------------------------------------------------------
// <copyright file="TrapContext.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Context for trap disarmament checks, including trap type, tool quality,
// and accumulated modifiers.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for trap disarmament checks, including trap type, tool quality,
/// and accumulated modifiers.
/// </summary>
/// <remarks>
/// <para>
/// TrapContext encapsulates all factors affecting a disarmament check:
/// <list type="bullet">
///   <item><description>Trap type (determines base DC)</description></item>
///   <item><description>Current step (Detection, Analysis, Disarmament)</description></item>
///   <item><description>Tool quality (modifier: -2 to +2 dice)</description></item>
///   <item><description>Analysis hint bonus (+1d10 if hint revealed)</description></item>
///   <item><description>Failed attempts (DC escalation: +1 per failure)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="TrapType">The type of trap being disarmed.</param>
/// <param name="Step">The current step in the disarmament procedure.</param>
/// <param name="ToolQuality">The quality of tools being used.</param>
/// <param name="HasAnalysisHint">Whether the character has a hint from analysis.</param>
/// <param name="FailedAttempts">Number of previous failed disarmament attempts.</param>
public readonly record struct TrapContext(
    TrapType TrapType,
    DisarmStep Step,
    ToolQuality ToolQuality,
    bool HasAnalysisHint,
    int FailedAttempts)
{
    // -------------------------------------------------------------------------
    // DC Mapping Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Hint bonus dice granted for successful analysis with hint revealed.
    /// </summary>
    private const int HintBonusDice = 1;

    // -------------------------------------------------------------------------
    // DC Calculation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the base detection DC for the trap type.
    /// </summary>
    /// <returns>The detection DC.</returns>
    /// <remarks>
    /// Detection DCs by trap type:
    /// <list type="bullet">
    ///   <item><description>Tripwire: 8</description></item>
    ///   <item><description>PressurePlate: 10</description></item>
    ///   <item><description>Electrified: 14</description></item>
    ///   <item><description>LaserGrid: 18</description></item>
    ///   <item><description>JotunDefense: 22</description></item>
    /// </list>
    /// </remarks>
    public int GetDetectionDc()
    {
        return TrapType switch
        {
            TrapType.Tripwire => 8,
            TrapType.PressurePlate => 10,
            TrapType.Electrified => 14,
            TrapType.LaserGrid => 18,
            TrapType.JotunDefense => 22,
            _ => 12 // Default fallback
        };
    }

    /// <summary>
    /// Gets the base disarm DC for the trap type (before failed attempt escalation).
    /// </summary>
    /// <returns>The base disarm DC.</returns>
    /// <remarks>
    /// Base disarm DCs by trap type:
    /// <list type="bullet">
    ///   <item><description>Tripwire: 8</description></item>
    ///   <item><description>PressurePlate: 12</description></item>
    ///   <item><description>Electrified: 16</description></item>
    ///   <item><description>LaserGrid: 20</description></item>
    ///   <item><description>JotunDefense: 24</description></item>
    /// </list>
    /// </remarks>
    public int GetBaseDisarmDc()
    {
        return TrapType switch
        {
            TrapType.Tripwire => 8,
            TrapType.PressurePlate => 12,
            TrapType.Electrified => 16,
            TrapType.LaserGrid => 20,
            TrapType.JotunDefense => 24,
            _ => 12 // Default fallback
        };
    }

    /// <summary>
    /// Gets the effective DC for the current step, including DC escalation from failures.
    /// </summary>
    /// <returns>The effective DC for the check.</returns>
    /// <remarks>
    /// For Disarmament, DC increases by +1 for each failed attempt.
    /// Detection and Analysis are not affected by failed attempts.
    /// </remarks>
    public int GetEffectiveDc()
    {
        return Step switch
        {
            DisarmStep.Detection => GetDetectionDc(),
            DisarmStep.Analysis => GetBaseDisarmDc(), // Analysis uses base disarm DC
            DisarmStep.Disarmament => GetBaseDisarmDc() + FailedAttempts, // DC escalation
            _ => GetBaseDisarmDc()
        };
    }

    // -------------------------------------------------------------------------
    // Modifier Calculation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the dice modifier from tool quality.
    /// </summary>
    /// <returns>The dice modifier (-2 to +2).</returns>
    /// <remarks>
    /// Tool modifiers:
    /// <list type="bullet">
    ///   <item><description>BareHands: -2d10</description></item>
    ///   <item><description>Improvised: +0</description></item>
    ///   <item><description>Proper: +1d10</description></item>
    ///   <item><description>Masterwork: +2d10</description></item>
    /// </list>
    /// </remarks>
    public int GetToolModifier()
    {
        return ToolQuality switch
        {
            ToolQuality.BareHands => -2,
            ToolQuality.Improvised => 0,
            ToolQuality.Proper => 1,
            ToolQuality.Masterwork => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the dice modifier from analysis hint.
    /// </summary>
    /// <returns>+1 if hint revealed, 0 otherwise.</returns>
    /// <remarks>
    /// Hint bonus only applies during Disarmament step.
    /// </remarks>
    public int GetHintModifier()
    {
        if (Step == DisarmStep.Disarmament && HasAnalysisHint)
        {
            return HintBonusDice;
        }

        return 0;
    }

    /// <summary>
    /// Gets the total dice modifier (tool + hint).
    /// </summary>
    /// <returns>The combined dice modifier.</returns>
    /// <remarks>
    /// Total modifier = Tool modifier + Hint modifier (if applicable).
    /// </remarks>
    public int GetTotalModifier()
    {
        return GetToolModifier() + GetHintModifier();
    }

    // -------------------------------------------------------------------------
    // Tool Requirement Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether this trap requires tools (DC 4+).
    /// </summary>
    /// <remarks>
    /// Traps with DC 4 or higher require [Tinker's Toolkit] (Improvised or better).
    /// </remarks>
    public bool RequiresTools => GetBaseDisarmDc() >= 4;

    /// <summary>
    /// Whether bare hands are allowed for this trap (DC &lt; 4).
    /// </summary>
    /// <remarks>
    /// Only traps with DC less than 4 can be attempted with bare hands.
    /// Note: All standard trap types have DC 8+, so bare hands are never viable
    /// for standard traps without special abilities.
    /// </remarks>
    public bool AllowsBareHands => GetBaseDisarmDc() < 4;

    /// <summary>
    /// Validates that the tool quality meets requirements for this trap.
    /// </summary>
    /// <returns>True if tools are adequate; false otherwise.</returns>
    /// <remarks>
    /// Returns false if the trap requires tools and bare hands are being used.
    /// </remarks>
    public bool HasAdequateTools()
    {
        if (RequiresTools && ToolQuality == ToolQuality.BareHands)
        {
            return false;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the context.
    /// </summary>
    /// <returns>A human-readable summary of the context.</returns>
    public string ToDisplayString()
    {
        var modStr = GetTotalModifier() switch
        {
            > 0 => $"+{GetTotalModifier()}d10",
            < 0 => $"{GetTotalModifier()}d10",
            _ => "+0"
        };

        return $"{TrapType} ({Step}): DC {GetEffectiveDc()} [{modStr}]";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"TrapContext[{TrapType}] Step={Step} DC={GetEffectiveDc()} " +
               $"Tool={ToolQuality} Hint={HasAnalysisHint} Failures={FailedAttempts}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
