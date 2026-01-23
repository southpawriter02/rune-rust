// ------------------------------------------------------------------------------
// <copyright file="SubjectResistanceExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the SubjectResistance enum providing check counts,
// bribery costs, display names, and resistance assessment utilities.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="SubjectResistance"/> enum providing
/// check counts, bribery costs, display names, and resistance assessment utilities.
/// </summary>
public static class SubjectResistanceExtensions
{
    /// <summary>
    /// Gets the minimum number of successful checks required to break this resistance level.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>The minimum checks needed to break the subject.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    /// <remarks>
    /// The actual number of checks required is calculated based on subject WILL
    /// and modifiers, but will always be within the min-max range for the level.
    /// </remarks>
    public static int GetMinChecksToBreak(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal => 1,
            SubjectResistance.Low => 2,
            SubjectResistance.Moderate => 4,
            SubjectResistance.High => 6,
            SubjectResistance.Extreme => 10,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets the maximum number of successful checks that may be required to break this resistance level.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>The maximum checks that might be needed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    /// <remarks>
    /// Extreme resistance subjects can require up to 15 checks if they have
    /// maximum WILL combined with resistance-enhancing modifiers.
    /// </remarks>
    public static int GetMaxChecksToBreak(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal => 1,
            SubjectResistance.Low => 3,
            SubjectResistance.Moderate => 5,
            SubjectResistance.High => 8,
            SubjectResistance.Extreme => 15,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Determines the SubjectResistance level based on the subject's WILL attribute.
    /// </summary>
    /// <param name="will">The subject's WILL attribute value.</param>
    /// <returns>The corresponding resistance level.</returns>
    /// <remarks>
    /// <para>
    /// WILL to Resistance mapping:
    /// <list type="bullet">
    ///   <item><description>WILL 1-2: Minimal</description></item>
    ///   <item><description>WILL 3-4: Low</description></item>
    ///   <item><description>WILL 5-6: Moderate</description></item>
    ///   <item><description>WILL 7-8: High</description></item>
    ///   <item><description>WILL 9+: Extreme</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This is a static method helper, not an extension method, as it creates
    /// a resistance level rather than operating on one.
    /// </para>
    /// </remarks>
    public static SubjectResistance FromWillAttribute(int will)
    {
        return will switch
        {
            <= 2 => SubjectResistance.Minimal,
            <= 4 => SubjectResistance.Low,
            <= 6 => SubjectResistance.Moderate,
            <= 8 => SubjectResistance.High,
            _ => SubjectResistance.Extreme
        };
    }

    /// <summary>
    /// Gets the base bribery cost per round for this resistance level.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>The base gold cost for a bribery attempt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Bribery costs reflect the subject's perceived value of their information
    /// and their resistance to corruption. Higher resistance = higher price.
    /// </para>
    /// <para>
    /// The actual cost varies by ±20% around this base value.
    /// </para>
    /// <para>
    /// Cost ranges by level:
    /// <list type="bullet">
    ///   <item><description>Minimal: ~15 gold (10-25 range)</description></item>
    ///   <item><description>Low: ~35 gold (25-50 range)</description></item>
    ///   <item><description>Moderate: ~75 gold (50-100 range)</description></item>
    ///   <item><description>High: ~150 gold (100-200 range)</description></item>
    ///   <item><description>Extreme: ~350 gold (200-500 range)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int GetBaseBriberyCost(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal => 15,
            SubjectResistance.Low => 35,
            SubjectResistance.Moderate => 75,
            SubjectResistance.High => 150,
            SubjectResistance.Extreme => 350,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>Human-readable resistance level name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    public static string GetDisplayName(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal => "Minimal",
            SubjectResistance.Low => "Low",
            SubjectResistance.Moderate => "Moderate",
            SubjectResistance.High => "High",
            SubjectResistance.Extreme => "Extreme",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets a description of typical subjects at this resistance level.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>Description of typical subjects with this resistance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    public static string GetTypicalSubjectDescription(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal =>
                "Frightened civilian, coward, or already traumatized",
            SubjectResistance.Low =>
                "Hired muscle, petty criminal, opportunistic informant",
            SubjectResistance.Moderate =>
                "Loyal soldier, guild member, devoted servant",
            SubjectResistance.High =>
                "Trained operative, sworn knight, professional spy",
            SubjectResistance.Extreme =>
                "Fanatical believer, elite assassin, devoted cultist",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets the assessment text displayed to the player.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>In-character assessment of the subject's resistance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    /// <remarks>
    /// Assessment text provides player guidance about expected difficulty
    /// without revealing exact mechanical values.
    /// </remarks>
    public static string GetAssessmentText(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal =>
                "This one will break easily.",
            SubjectResistance.Low =>
                "Some resistance expected, but should yield soon.",
            SubjectResistance.Moderate =>
                "A tough nut to crack. Be prepared for a lengthy session.",
            SubjectResistance.High =>
                "This one has been trained to resist. It won't be easy.",
            SubjectResistance.Extreme =>
                "Exceptionally strong will. Breaking them may not be possible through normal means.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets the recommended approach for this resistance level.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>Suggestions for which methods might work best.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    /// <remarks>
    /// Provides tactical guidance to help players choose methods appropriate
    /// for the subject's resistance level.
    /// </remarks>
    public static string GetRecommendedApproach(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal =>
                "Any method should work. Good Cop is recommended for best reliability.",
            SubjectResistance.Low =>
                "Standard methods effective. Consider Bribery for quick results.",
            SubjectResistance.Moderate =>
                "May require multiple rounds. Mix methods based on subject reactions.",
            SubjectResistance.High =>
                "Lengthy interrogation expected. Consider softening with Bad Cop before Good Cop.",
            SubjectResistance.Extreme =>
                "Conventional methods may be insufficient. Consider special abilities or unconventional approaches.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets an estimate of how many rounds the interrogation might take.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>A string describing the expected duration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    public static string GetDurationEstimate(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal =>
                "Quick (1 round)",
            SubjectResistance.Low =>
                "Brief (2-3 rounds)",
            SubjectResistance.Moderate =>
                "Standard (4-5 rounds)",
            SubjectResistance.High =>
                "Lengthy (6-8 rounds)",
            SubjectResistance.Extreme =>
                "Extended (10+ rounds)",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }

    /// <summary>
    /// Gets whether this resistance level is considered difficult.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>True if the resistance is High or Extreme.</returns>
    /// <remarks>
    /// Difficult subjects may require specialized approaches, abilities,
    /// or acceptance of methods with consequences (like Torture).
    /// </remarks>
    public static bool IsDifficult(this SubjectResistance resistance)
    {
        return resistance == SubjectResistance.High ||
               resistance == SubjectResistance.Extreme;
    }

    /// <summary>
    /// Gets the color hint for UI presentation of this resistance level.
    /// </summary>
    /// <param name="resistance">The subject resistance level.</param>
    /// <returns>A suggested color name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown resistance level is provided.
    /// </exception>
    public static string GetColorHint(this SubjectResistance resistance)
    {
        return resistance switch
        {
            SubjectResistance.Minimal => "Green",
            SubjectResistance.Low => "LightGreen",
            SubjectResistance.Moderate => "Yellow",
            SubjectResistance.High => "Orange",
            SubjectResistance.Extreme => "Red",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resistance),
                resistance,
                "Unknown subject resistance level")
        };
    }
}
