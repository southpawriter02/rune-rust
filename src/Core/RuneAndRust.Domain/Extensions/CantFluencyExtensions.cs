// ------------------------------------------------------------------------------
// <copyright file="CantFluencyExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the CantFluency enum providing dice modifiers, display
// names, descriptions, and communication capability checks.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="CantFluency"/> enum providing dice modifiers,
/// display names, descriptions, and communication capability checks.
/// </summary>
public static class CantFluencyExtensions
{
    /// <summary>
    /// Gets the dice pool modifier associated with this fluency level.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>
    /// The number of dice to add (positive) or remove (negative) from the pool.
    /// <list type="bullet">
    ///   <item><description><see cref="CantFluency.None"/>: -1</description></item>
    ///   <item><description><see cref="CantFluency.Basic"/>: 0</description></item>
    ///   <item><description><see cref="CantFluency.Fluent"/>: +1</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown fluency level is provided.
    /// </exception>
    /// <example>
    /// <code>
    /// var modifier = CantFluency.Fluent.GetDiceModifier(); // Returns 1
    /// var penalty = CantFluency.None.GetDiceModifier();    // Returns -1
    /// </code>
    /// </example>
    public static int GetDiceModifier(this CantFluency fluency)
    {
        return fluency switch
        {
            CantFluency.None => -1,
            CantFluency.Basic => 0,
            CantFluency.Fluent => 1,
            _ => throw new ArgumentOutOfRangeException(
                nameof(fluency),
                fluency,
                "Unknown cant fluency level")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>Human-readable fluency level name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown fluency level is provided.
    /// </exception>
    public static string GetDisplayName(this CantFluency fluency)
    {
        return fluency switch
        {
            CantFluency.None => "None",
            CantFluency.Basic => "Basic",
            CantFluency.Fluent => "Fluent",
            _ => throw new ArgumentOutOfRangeException(
                nameof(fluency),
                fluency,
                "Unknown cant fluency level")
        };
    }

    /// <summary>
    /// Gets a human-readable description of this fluency level including the modifier.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>A descriptive string suitable for display to the player.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown fluency level is provided.
    /// </exception>
    /// <example>
    /// <code>
    /// var description = CantFluency.Fluent.GetDescription(); // Returns "Fluent (+1d10)"
    /// </code>
    /// </example>
    public static string GetDescription(this CantFluency fluency)
    {
        return fluency switch
        {
            CantFluency.None => "No Knowledge (-1d10)",
            CantFluency.Basic => "Basic Understanding (+0)",
            CantFluency.Fluent => "Fluent (+1d10)",
            _ => throw new ArgumentOutOfRangeException(
                nameof(fluency),
                fluency,
                "Unknown cant fluency level")
        };
    }

    /// <summary>
    /// Gets a narrative description of what this fluency level means in practice.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>Narrative text describing the character's linguistic abilities.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown fluency level is provided.
    /// </exception>
    public static string GetNarrativeDescription(this CantFluency fluency)
    {
        return fluency switch
        {
            CantFluency.None =>
                "You cannot understand or speak this dialect. Cultural references and " +
                "idioms are completely lost on you, and you may accidentally cause offense " +
                "through linguistic ignorance.",
            CantFluency.Basic =>
                "You can communicate basic ideas, but lack nuance. You understand literal " +
                "meanings but miss subtext and cultural implications. You are clearly " +
                "recognized as a learner or outsider.",
            CantFluency.Fluent =>
                "You speak with native-level proficiency, understanding idioms, cultural " +
                "references, and subtle implications. You can use appropriate honorifics " +
                "and demonstrate cultural respect through language.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(fluency),
                fluency,
                "Unknown cant fluency level")
        };
    }

    /// <summary>
    /// Determines if this fluency level allows understanding spoken communication.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>
    /// <c>true</c> if the character can understand at least basic communication;
    /// <c>false</c> if they cannot understand the cant at all.
    /// </returns>
    /// <remarks>
    /// Characters with <see cref="CantFluency.None"/> cannot understand the cant,
    /// but may still attempt interaction with penalty. Characters with Basic or
    /// Fluent can understand communication to varying degrees.
    /// </remarks>
    public static bool CanUnderstand(this CantFluency fluency)
    {
        return fluency >= CantFluency.Basic;
    }

    /// <summary>
    /// Determines if this fluency level allows speaking the cant.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>
    /// <c>true</c> if the character can speak the cant;
    /// <c>false</c> if they cannot.
    /// </returns>
    /// <remarks>
    /// Characters with <see cref="CantFluency.None"/> cannot speak the cant,
    /// though they may attempt communication in Common with penalties.
    /// </remarks>
    public static bool CanSpeak(this CantFluency fluency)
    {
        return fluency >= CantFluency.Basic;
    }

    /// <summary>
    /// Determines if this fluency level provides a bonus (not a penalty).
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>
    /// <c>true</c> if the fluency provides a bonus or no penalty;
    /// <c>false</c> if it incurs a penalty.
    /// </returns>
    public static bool ProvidesBonus(this CantFluency fluency)
    {
        return fluency >= CantFluency.Basic;
    }

    /// <summary>
    /// Determines if this fluency level incurs a penalty.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>
    /// <c>true</c> if the fluency incurs a penalty;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IncursPenalty(this CantFluency fluency)
    {
        return fluency == CantFluency.None;
    }

    /// <summary>
    /// Determines if this fluency level grants access to cultural-specific dialogue options.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>
    /// <c>true</c> if the character can access special dialogue options;
    /// <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// Only fluent speakers can access cultural-specific dialogue options that
    /// reference deep cultural knowledge, idioms, or insider references.
    /// </remarks>
    public static bool GrantsCulturalDialogueAccess(this CantFluency fluency)
    {
        return fluency == CantFluency.Fluent;
    }

    /// <summary>
    /// Gets the color hint for UI presentation of this fluency level.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>A suggested color name for visual indication.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown fluency level is provided.
    /// </exception>
    public static string GetColorHint(this CantFluency fluency)
    {
        return fluency switch
        {
            CantFluency.None => "Red",
            CantFluency.Basic => "Yellow",
            CantFluency.Fluent => "Green",
            _ => throw new ArgumentOutOfRangeException(
                nameof(fluency),
                fluency,
                "Unknown cant fluency level")
        };
    }

    /// <summary>
    /// Gets the icon hint for UI presentation of this fluency level.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <returns>A suggested icon identifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown fluency level is provided.
    /// </exception>
    public static string GetIconHint(this CantFluency fluency)
    {
        return fluency switch
        {
            CantFluency.None => "language_none",
            CantFluency.Basic => "language_partial",
            CantFluency.Fluent => "language_full",
            _ => throw new ArgumentOutOfRangeException(
                nameof(fluency),
                fluency,
                "Unknown cant fluency level")
        };
    }
}
