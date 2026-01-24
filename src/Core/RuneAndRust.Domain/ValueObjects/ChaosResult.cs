// ------------------------------------------------------------------------------
// <copyright file="ChaosResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Captures the result of a chaos roll when exploiting a glitch without
// first identifying the pattern.
// Part of v0.15.4f Glitch Exploitation System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Captures the result of a chaos roll when exploiting a glitch without
/// first identifying the pattern.
/// </summary>
/// <remarks>
/// <para>
/// When a character attempts glitch exploitation without observing the pattern,
/// they roll d6 on the Chaos table:
/// <list type="bullet">
///   <item><description>1-2 (33%): Against - Glitch resists (+4 DC)</description></item>
///   <item><description>3-4 (33%): Neutral - No effect (+0 DC)</description></item>
///   <item><description>5-6 (33%): Helps - Glitch aids (-2 DC)</description></item>
/// </list>
/// </para>
/// <para>
/// The chaos roll represents the unpredictable nature of corrupted technology.
/// Unlike identified patterns where the character can time their actions,
/// chaos rolls are a gamble that may help or hinder the bypass attempt.
/// </para>
/// </remarks>
/// <param name="Roll">The d6 roll result (1-6).</param>
/// <param name="Effect">The effect category (Against, Neutral, Helps).</param>
/// <param name="DcModifier">The DC modifier to apply (+4, 0, or -2).</param>
/// <param name="NarrativeText">Descriptive text for the chaos effect.</param>
public readonly record struct ChaosResult(
    int Roll,
    ChaosEffect Effect,
    int DcModifier,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// DC modifier when the glitch works against the character.
    /// </summary>
    private const int AgainstDcModifier = 4;

    /// <summary>
    /// DC modifier when the glitch has no effect.
    /// </summary>
    private const int NeutralDcModifier = 0;

    /// <summary>
    /// DC modifier when the glitch helps the character.
    /// </summary>
    private const int HelpsDcModifier = -2;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a ChaosResult from a d6 roll.
    /// </summary>
    /// <param name="roll">The d6 roll (1-6).</param>
    /// <returns>A ChaosResult with appropriate effect and DC modifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when roll is not 1-6.</exception>
    /// <remarks>
    /// <para>
    /// Maps d6 results to chaos effects:
    /// <list type="bullet">
    ///   <item><description>1-2: Against (+4 DC) - Glitch resists interference</description></item>
    ///   <item><description>3-4: Neutral (+0 DC) - No particular effect</description></item>
    ///   <item><description>5-6: Helps (-2 DC) - Glitch coincidentally aids</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static ChaosResult FromRoll(int roll)
    {
        if (roll < 1 || roll > 6)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roll),
                roll,
                "Chaos roll must be between 1 and 6 (d6 result).");
        }

        return roll switch
        {
            1 or 2 => new ChaosResult(
                Roll: roll,
                Effect: ChaosEffect.Against,
                DcModifier: AgainstDcModifier,
                NarrativeText: "The glitch pulses angrily as you approach, its erratic behavior " +
                              "intensifying as if the machine itself rejects your interference. " +
                              "Security protocols flare to life in unexpected ways."),

            3 or 4 => new ChaosResult(
                Roll: roll,
                Effect: ChaosEffect.Neutral,
                DcModifier: NeutralDcModifier,
                NarrativeText: "The machine's corrupted patterns continue their chaotic dance, " +
                              "neither helping nor hindering your attempt. You proceed against " +
                              "the normal [Glitched] resistance."),

            5 or 6 => new ChaosResult(
                Roll: roll,
                Effect: ChaosEffect.Helps,
                DcModifier: HelpsDcModifier,
                NarrativeText: "Fortune favors the bold! The glitch cycles into a momentarily " +
                              "permissive state just as you act. The machine's own malfunction " +
                              "becomes your ally."),

            _ => throw new InvalidOperationException(
                $"Unexpected chaos roll value: {roll}. This should not happen.")
        };
    }

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a short summary of the chaos effect.
    /// </summary>
    /// <remarks>
    /// Provides a brief description suitable for UI display or logging.
    /// </remarks>
    public string EffectSummary => Effect switch
    {
        ChaosEffect.Against => $"Glitch works against you (+{DcModifier} DC)",
        ChaosEffect.Neutral => "Neutral effect (no modifier)",
        ChaosEffect.Helps => $"Glitch helps you ({DcModifier} DC)",
        _ => "Unknown effect"
    };

    /// <summary>
    /// Gets whether the chaos effect is beneficial.
    /// </summary>
    /// <remarks>
    /// True when the glitch helps (-2 DC modifier).
    /// </remarks>
    public bool IsBeneficial => Effect == ChaosEffect.Helps;

    /// <summary>
    /// Gets whether the chaos effect is detrimental.
    /// </summary>
    /// <remarks>
    /// True when the glitch works against the character (+4 DC modifier).
    /// </remarks>
    public bool IsDetrimental => Effect == ChaosEffect.Against;

    /// <summary>
    /// Gets whether the chaos effect is neutral.
    /// </summary>
    /// <remarks>
    /// True when the glitch has no particular effect (+0 DC modifier).
    /// </remarks>
    public bool IsNeutral => Effect == ChaosEffect.Neutral;

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a formatted display string for the chaos result.
    /// </summary>
    /// <returns>A human-readable summary of the chaos roll outcome.</returns>
    public string ToDisplayString()
    {
        var effectIcon = Effect switch
        {
            ChaosEffect.Against => "[!]",
            ChaosEffect.Neutral => "[~]",
            ChaosEffect.Helps => "[*]",
            _ => "[?]"
        };

        return $"Chaos Roll: {Roll} {effectIcon} {EffectSummary}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ChaosResult[Roll={Roll} Effect={Effect} Modifier={DcModifier:+0;-0;+0}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
