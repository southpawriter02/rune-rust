// ------------------------------------------------------------------------------
// <copyright file="IntimidationApproachExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the IntimidationApproach enum providing attribute
// mapping, display names, descriptions, and example actions.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="IntimidationApproach"/> enum providing
/// attribute mapping, display names, narrative descriptions, and example actions.
/// </summary>
public static class IntimidationApproachExtensions
{
    /// <summary>
    /// Gets the attribute name used for this intimidation approach.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>
    /// "MIGHT" for Physical approach, "WILL" for Mental approach.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    /// <remarks>
    /// The dice pool for intimidation is: [Chosen Attribute] + Rhetoric + Modifiers.
    /// Players choose their approach based on character strengths and narrative preference.
    /// </remarks>
    public static string GetAttributeName(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical => "MIGHT",
            IntimidationApproach.Mental => "WILL",
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>Human-readable approach name with attribute.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    public static string GetDisplayName(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical => "Physical Threat (MIGHT)",
            IntimidationApproach.Mental => "Mental Pressure (WILL)",
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets a narrative description of the approach.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>Narrative description for display.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    public static string GetDescription(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical =>
                "Uses physical presence, size, and the implied threat of violence. " +
                "Best suited for characters with high MIGHT who prefer direct, unsubtle approaches.",
            IntimidationApproach.Mental =>
                "Uses psychological pressure, reputation, and force of personality. " +
                "Best suited for characters with high WILL who prefer subtle, psychological approaches.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets example actions that represent this intimidation approach.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>Array of example intimidation actions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExampleActions(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical => new[]
            {
                "Looming over the target",
                "Drawing or displaying weapons",
                "Flexing muscles, cracking knuckles",
                "Physical blocking of escape routes",
                "Grabbing collar or slamming table"
            },
            IntimidationApproach.Mental => new[]
            {
                "Cold, unflinching stare",
                "Calm threats with implied doom",
                "Referencing reputation or past deeds",
                "Quiet certainty of violence to come",
                "Mentioning what happened to the last one"
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets the short name for this approach.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>Short name string (Physical or Mental).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    public static string GetShortName(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical => "Physical",
            IntimidationApproach.Mental => "Mental",
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets a brief description of what this approach involves.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>Brief narrative description.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    public static string GetBriefDescription(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical =>
                "Overt physical threat and displays of strength",
            IntimidationApproach.Mental =>
                "Psychological pressure and implied consequences",
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets whether this approach benefits from visible weapons or armor.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>True if visible equipment enhances this approach.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown approach is provided.
    /// </exception>
    /// <remarks>
    /// Physical intimidation benefits more from visible weaponry,
    /// while mental intimidation relies more on reputation and presence.
    /// Both can benefit from wielding an [Artifact] (+1d10 bonus).
    /// </remarks>
    public static bool BenefitsFromVisibleWeaponry(this IntimidationApproach approach)
    {
        return approach switch
        {
            IntimidationApproach.Physical => true,
            IntimidationApproach.Mental => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }

    /// <summary>
    /// Gets whether this approach benefits from reputation effects.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <returns>True if reputation provides enhanced benefits.</returns>
    /// <remarks>
    /// While both approaches benefit from [Honored] or [Feared] reputation (+1d10),
    /// mental intimidation is particularly effective when the target knows
    /// of the player's past deeds.
    /// </remarks>
    public static bool BenefitsFromReputation(this IntimidationApproach approach)
    {
        // Both approaches benefit from reputation, but mental especially so
        return approach switch
        {
            IntimidationApproach.Physical => true,
            IntimidationApproach.Mental => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(approach),
                approach,
                "Unknown intimidation approach")
        };
    }
}
