// ------------------------------------------------------------------------------
// <copyright file="LieComplexityExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for LieComplexity enum.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="LieComplexity"/> enum.
/// </summary>
public static class LieComplexityExtensions
{
    /// <summary>
    /// Gets the base DC for this lie complexity.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>The base difficulty class.</returns>
    /// <remarks>
    /// DC values:
    /// <list type="bullet">
    ///   <item><description>WhiteLie: 10</description></item>
    ///   <item><description>Plausible: 14</description></item>
    ///   <item><description>Unlikely: 18</description></item>
    ///   <item><description>Outrageous: 22</description></item>
    /// </list>
    /// </remarks>
    public static int GetBaseDc(this LieComplexity complexity)
    {
        return complexity switch
        {
            LieComplexity.WhiteLie => 10,
            LieComplexity.Plausible => 14,
            LieComplexity.Unlikely => 18,
            LieComplexity.Outrageous => 22,
            _ => 14
        };
    }

    /// <summary>
    /// Gets the typical NPC reaction severity if the lie is detected.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>A value from 1 (mild) to 4 (severe) indicating reaction severity.</returns>
    /// <remarks>
    /// Severity levels:
    /// <list type="bullet">
    ///   <item><description>1: Mild annoyance, slight suspicion</description></item>
    ///   <item><description>2: Suspicion, possible refusal to interact</description></item>
    ///   <item><description>3: Alarm, possible hostility, may report</description></item>
    ///   <item><description>4: Immediate hostility, possible attack</description></item>
    /// </list>
    /// </remarks>
    public static int GetDetectionSeverity(this LieComplexity complexity)
    {
        return complexity switch
        {
            LieComplexity.WhiteLie => 1,
            LieComplexity.Plausible => 2,
            LieComplexity.Unlikely => 3,
            LieComplexity.Outrageous => 4,
            _ => 2
        };
    }

    /// <summary>
    /// Gets whether this lie complexity typically requires supporting evidence.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>True if forged documents or other evidence is highly recommended.</returns>
    public static bool RequiresEvidence(this LieComplexity complexity)
    {
        return complexity >= LieComplexity.Unlikely;
    }

    /// <summary>
    /// Gets the combat initiation chance modifier for [Lie Exposed] fumble.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>A percentage modifier to add to the base 50% combat chance.</returns>
    /// <remarks>
    /// Combat chance modifiers:
    /// <list type="bullet">
    ///   <item><description>WhiteLie: -10% (40% total)</description></item>
    ///   <item><description>Plausible: +0% (50% total)</description></item>
    ///   <item><description>Unlikely: +10% (60% total)</description></item>
    ///   <item><description>Outrageous: +20% (70% total)</description></item>
    /// </list>
    /// </remarks>
    public static int GetCombatChanceModifier(this LieComplexity complexity)
    {
        return complexity switch
        {
            LieComplexity.WhiteLie => -10,
            LieComplexity.Plausible => 0,
            LieComplexity.Unlikely => 10,
            LieComplexity.Outrageous => 20,
            _ => 0
        };
    }

    /// <summary>
    /// Gets a human-readable description of the lie complexity.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>A descriptive string for UI display.</returns>
    public static string GetDescription(this LieComplexity complexity)
    {
        return complexity switch
        {
            LieComplexity.WhiteLie => "White Lie (DC 10) - Minor falsehood, easily believed",
            LieComplexity.Plausible => "Plausible (DC 14) - Believable given context",
            LieComplexity.Unlikely => "Unlikely (DC 18) - Stretches credibility, needs evidence",
            LieComplexity.Outrageous => "Outrageous (DC 22) - Nearly unbelievable audacity",
            _ => "Unknown lie complexity"
        };
    }

    /// <summary>
    /// Gets the short name for this lie complexity.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>A short name string.</returns>
    public static string GetShortName(this LieComplexity complexity)
    {
        return complexity switch
        {
            LieComplexity.WhiteLie => "White Lie",
            LieComplexity.Plausible => "Plausible",
            LieComplexity.Unlikely => "Unlikely",
            LieComplexity.Outrageous => "Outrageous",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets example lies for this complexity tier.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <returns>An array of example lie statements.</returns>
    public static string[] GetExamples(this LieComplexity complexity)
    {
        return complexity switch
        {
            LieComplexity.WhiteLie => new[]
            {
                "I was here the whole time",
                "I didn't see anything",
                "I'm just passing through",
                "We've never met before"
            },
            LieComplexity.Plausible => new[]
            {
                "I'm with the Combine",
                "The captain sent me to relieve you",
                "I'm a merchant from the outer settlements",
                "This is my first time in the city"
            },
            LieComplexity.Unlikely => new[]
            {
                "The Warden sent me",
                "I'm a high-ranking official from the capital",
                "I discovered a cure for the blight",
                "The guards already cleared me to be here"
            },
            LieComplexity.Outrageous => new[]
            {
                "I am the Warden",
                "I single-handedly defeated the Iron-Bane warband",
                "I've been chosen by the Shattered God",
                "The entire settlement owes me a blood debt"
            },
            _ => Array.Empty<string>()
        };
    }
}
