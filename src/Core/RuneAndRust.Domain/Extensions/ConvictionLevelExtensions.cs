// ------------------------------------------------------------------------------
// <copyright file="ConvictionLevelExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the ConvictionLevel enum providing base DCs, pool
// thresholds, resistance mechanics, and UI presentation information.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="ConvictionLevel"/> enum providing
/// base DCs, pool thresholds, resistance mechanics, and UI presentation information.
/// </summary>
public static class ConvictionLevelExtensions
{
    /// <summary>
    /// Gets the base Difficulty Class (DC) for rhetoric checks when attempting
    /// to influence a belief at this conviction level.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>The base DC required for influence attempts.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Base DC represents the difficulty of making progress against this belief.
    /// The actual DC may be higher due to accumulated resistance modifiers.
    /// </para>
    /// <para>
    /// DC values by conviction level:
    /// <list type="bullet">
    ///   <item><description>WeakOpinion: DC 10 (casual position)</description></item>
    ///   <item><description>ModerateBelief: DC 12 (held with some conviction)</description></item>
    ///   <item><description>StrongConviction: DC 14 (requires evidence)</description></item>
    ///   <item><description>CoreBelief: DC 16 (fundamental worldview)</description></item>
    ///   <item><description>Fanatical: DC 18 (identity-defining)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int GetBaseDc(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => 10,
            ConvictionLevel.ModerateBelief => 12,
            ConvictionLevel.StrongConviction => 14,
            ConvictionLevel.CoreBelief => 16,
            ConvictionLevel.Fanatical => 18,
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets the influence pool threshold required to change this belief.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>The pool points needed to successfully change the belief.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The pool threshold represents the cumulative influence required to shift
    /// the NPC's position. Each successful rhetoric check adds to the pool based
    /// on net successes achieved.
    /// </para>
    /// <para>
    /// Pool thresholds by conviction level:
    /// <list type="bullet">
    ///   <item><description>WeakOpinion: 5 points (quick conversation)</description></item>
    ///   <item><description>ModerateBelief: 10 points (sustained argument)</description></item>
    ///   <item><description>StrongConviction: 15 points (multiple sessions)</description></item>
    ///   <item><description>CoreBelief: 20 points (extended campaign)</description></item>
    ///   <item><description>Fanatical: 25 points (requires life-changing events)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int GetPoolThreshold(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => 5,
            ConvictionLevel.ModerateBelief => 10,
            ConvictionLevel.StrongConviction => 15,
            ConvictionLevel.CoreBelief => 20,
            ConvictionLevel.Fanatical => 25,
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets the resistance increment per failed influence attempt.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>
    /// The amount resistance increases per failure. Returns a decimal value
    /// where fractional values indicate resistance increases every N failures
    /// (e.g., 0.5 means +1 resistance per 2 failures).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Resistance accumulates on failed influence attempts, increasing the
    /// effective DC for future attempts. Higher conviction levels build
    /// resistance faster, making repeated failures more punishing.
    /// </para>
    /// <para>
    /// Resistance rates by conviction level:
    /// <list type="bullet">
    ///   <item><description>WeakOpinion: 0 (no resistance buildup)</description></item>
    ///   <item><description>ModerateBelief: 0 (no resistance buildup)</description></item>
    ///   <item><description>StrongConviction: 0.5 (+1 per 2 failures)</description></item>
    ///   <item><description>CoreBelief: 1 (+1 per failure)</description></item>
    ///   <item><description>Fanatical: 2 (+2 per failure)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static decimal GetResistancePerFailure(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => 0m,
            ConvictionLevel.ModerateBelief => 0m,
            ConvictionLevel.StrongConviction => 0.5m,
            ConvictionLevel.CoreBelief => 1m,
            ConvictionLevel.Fanatical => 2m,
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Determines if this conviction level accumulates resistance on failures.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>True if failed attempts increase resistance; otherwise false.</returns>
    /// <remarks>
    /// WeakOpinion and ModerateBelief do not build resistance, allowing
    /// unlimited attempts without penalty. Stronger convictions become
    /// harder to influence after failed attempts.
    /// </remarks>
    public static bool AccumulatesResistance(this ConvictionLevel level)
    {
        return level.GetResistancePerFailure() > 0;
    }

    /// <summary>
    /// Gets whether this conviction level requires a life-changing event
    /// before influence attempts can succeed.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>True if a transformative event is required.</returns>
    /// <remarks>
    /// <para>
    /// Fanatical beliefs are so central to the NPC's identity that no amount
    /// of rhetoric alone can change them. The NPC must first experience
    /// something that challenges their fundamental worldview.
    /// </para>
    /// <para>
    /// Life-changing events might include:
    /// <list type="bullet">
    ///   <item><description>Direct contradiction of their beliefs by witnessed events</description></item>
    ///   <item><description>Betrayal by someone they trusted completely</description></item>
    ///   <item><description>Discovery of evidence that shatters their assumptions</description></item>
    ///   <item><description>Personal tragedy that reframes their perspective</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static bool RequiresLifeEvent(this ConvictionLevel level)
    {
        return level == ConvictionLevel.Fanatical;
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>Human-readable conviction level name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    public static string GetDisplayName(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => "Weak Opinion",
            ConvictionLevel.ModerateBelief => "Moderate Belief",
            ConvictionLevel.StrongConviction => "Strong Conviction",
            ConvictionLevel.CoreBelief => "Core Belief",
            ConvictionLevel.Fanatical => "Fanatical",
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets a brief description of this conviction level for player guidance.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>Descriptive text explaining the conviction level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    public static string GetDescription(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion =>
                "A casual position the NPC hasn't invested much thought into. Easily swayed with minimal effort.",
            ConvictionLevel.ModerateBelief =>
                "A belief held with some conviction but without deep personal investment. Requires sustained argument.",
            ConvictionLevel.StrongConviction =>
                "A firmly held belief based on experience or reasoning. Requires evidence or compelling testimony.",
            ConvictionLevel.CoreBelief =>
                "Fundamental to how the NPC understands the world. Changing it may transform their behavior.",
            ConvictionLevel.Fanatical =>
                "Inseparable from the NPC's identity. Requires a life-changing event before influence can succeed.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets a narrative description for GM/storytelling purposes.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>Descriptive text for narrative presentation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    public static string GetNarrativeDescription(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion =>
                "They seem to hold this opinion loosely, more from hearsay than conviction.",
            ConvictionLevel.ModerateBelief =>
                "They believe this, though you sense the right argument might shift their view.",
            ConvictionLevel.StrongConviction =>
                "Their jaw sets firm when they speak of this. They've clearly thought it through.",
            ConvictionLevel.CoreBelief =>
                "This belief runs deep—it's woven into how they see the world and their place in it.",
            ConvictionLevel.Fanatical =>
                "This isn't just what they believe—it's who they ARE. Words alone won't reach them.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets example beliefs appropriate for this conviction level.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>Array of example belief statements.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExampleBeliefs(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => new[]
            {
                "I've heard the Combine is powerful.",
                "I don't know much about the Dvergr.",
                "The old tunnels are probably dangerous."
            },
            ConvictionLevel.ModerateBelief => new[]
            {
                "The Combine keeps things orderly.",
                "Magic seems dangerous to me.",
                "Outsiders can't be trusted."
            },
            ConvictionLevel.StrongConviction => new[]
            {
                "The Combine's rules saved my family during the Drought.",
                "The old ways are better than this new chaos.",
                "The Dvergr are the only ones who understand the machines."
            },
            ConvictionLevel.CoreBelief => new[]
            {
                "The Combine IS civilization. Without them, we'd all be dead.",
                "Magic corrupts absolutely. No exceptions.",
                "Blood is thicker than water. Family comes first, always."
            },
            ConvictionLevel.Fanatical => new[]
            {
                "I am Combine. The Overseer's will is my will.",
                "The runes speak through me. I am their vessel.",
                "My clan's honor is my honor. I would die before betraying them."
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets a UI color hint for displaying this conviction level.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>A CSS-compatible color name or hex code.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    /// <remarks>
    /// Colors progress from cool (easy) to warm (difficult):
    /// <list type="bullet">
    ///   <item><description>WeakOpinion: Green (easy)</description></item>
    ///   <item><description>ModerateBelief: Blue (moderate)</description></item>
    ///   <item><description>StrongConviction: Yellow (challenging)</description></item>
    ///   <item><description>CoreBelief: Orange (difficult)</description></item>
    ///   <item><description>Fanatical: Red (extreme)</description></item>
    /// </list>
    /// </remarks>
    public static string GetColorHint(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => "#4CAF50",     // Green
            ConvictionLevel.ModerateBelief => "#2196F3", // Blue
            ConvictionLevel.StrongConviction => "#FFC107", // Yellow/Amber
            ConvictionLevel.CoreBelief => "#FF9800",      // Orange
            ConvictionLevel.Fanatical => "#F44336",       // Red
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets a status indicator emoji for this conviction level.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>An emoji character representing difficulty.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown conviction level is provided.
    /// </exception>
    public static string GetDifficultyIndicator(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => "⚪",
            ConvictionLevel.ModerateBelief => "🔵",
            ConvictionLevel.StrongConviction => "🟡",
            ConvictionLevel.CoreBelief => "🟠",
            ConvictionLevel.Fanatical => "🔴",
            _ => throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Unknown conviction level")
        };
    }

    /// <summary>
    /// Gets a summary string showing the key mechanical values.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>A compact string showing DC, threshold, and resistance rate.</returns>
    /// <remarks>
    /// Format: "DC X | Pool Y | +Z res/fail"
    /// Useful for quick reference in UI displays.
    /// </remarks>
    public static string GetMechanicsSummary(this ConvictionLevel level)
    {
        var resistance = level.GetResistancePerFailure();
        var resistanceText = resistance switch
        {
            0m => "no resistance",
            0.5m => "+1 res/2 fails",
            _ => $"+{resistance:0} res/fail"
        };

        return $"DC {level.GetBaseDc()} | Pool {level.GetPoolThreshold()} | {resistanceText}";
    }

    /// <summary>
    /// Gets the estimated number of successful checks needed to change this belief.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <param name="averageNetSuccesses">
    /// The expected average net successes per check. Defaults to 2.
    /// </param>
    /// <returns>Estimated number of successful checks required.</returns>
    /// <remarks>
    /// This is a rough estimate assuming consistent performance. Actual attempts
    /// may vary based on dice rolls, bonuses, and accumulated resistance.
    /// </remarks>
    public static int GetEstimatedAttempts(this ConvictionLevel level, int averageNetSuccesses = 2)
    {
        if (averageNetSuccesses <= 0)
        {
            averageNetSuccesses = 1;
        }

        var threshold = level.GetPoolThreshold();
        return (int)Math.Ceiling((double)threshold / averageNetSuccesses);
    }

    /// <summary>
    /// Determines if the specified conviction level is considered high difficulty.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>True if CoreBelief or Fanatical; otherwise false.</returns>
    /// <remarks>
    /// High-difficulty beliefs may warrant special UI treatment or additional
    /// player guidance about the challenges involved.
    /// </remarks>
    public static bool IsHighDifficulty(this ConvictionLevel level)
    {
        return level >= ConvictionLevel.CoreBelief;
    }

    /// <summary>
    /// Gets warning text to display when the player targets this conviction level.
    /// </summary>
    /// <param name="level">The conviction level.</param>
    /// <returns>Warning text, or empty string if no warning needed.</returns>
    public static string GetWarningText(this ConvictionLevel level)
    {
        return level switch
        {
            ConvictionLevel.WeakOpinion => string.Empty,
            ConvictionLevel.ModerateBelief => string.Empty,
            ConvictionLevel.StrongConviction =>
                "⚠️ Failed attempts will increase resistance.",
            ConvictionLevel.CoreBelief =>
                "⚠️ This belief is fundamental to their worldview. " +
                "Failed attempts significantly increase resistance.",
            ConvictionLevel.Fanatical =>
                "🔴 This belief defines who they are. " +
                "A life-changing event may be required before influence can succeed. " +
                "Failed attempts rapidly increase resistance.",
            _ => string.Empty
        };
    }
}
