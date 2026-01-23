// ------------------------------------------------------------------------------
// <copyright file="RequestComplexityExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the RequestComplexity enum providing DC values,
// gap calculations, round counts, and display names.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="RequestComplexity"/> enum providing
/// DC values, initial gap calculations, default round counts, and display utilities.
/// </summary>
public static class RequestComplexityExtensions
{
    /// <summary>
    /// Gets the base DC for checks during this negotiation.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Base DC value (10, 14, 18, 22, or 26).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    /// <remarks>
    /// DC values scale with complexity:
    /// <list type="bullet">
    ///   <item><description>FairTrade: DC 10</description></item>
    ///   <item><description>SlightAdvantage: DC 14</description></item>
    ///   <item><description>NoticeableAdvantage: DC 18</description></item>
    ///   <item><description>MajorAdvantage: DC 22</description></item>
    ///   <item><description>OneSidedDeal: DC 26</description></item>
    /// </list>
    /// </remarks>
    public static int GetBaseDc(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => 10,
            RequestComplexity.SlightAdvantage => 14,
            RequestComplexity.NoticeableAdvantage => 18,
            RequestComplexity.MajorAdvantage => 22,
            RequestComplexity.OneSidedDeal => 26,
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets the initial gap between positions based on request complexity.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Starting gap value (2, 3, 4, 5, or 6).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    /// <remarks>
    /// The initial gap determines how far apart PC and NPC positions start.
    /// Larger gaps require more successful checks or concessions to bridge.
    /// </remarks>
    public static int GetInitialGap(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => 2,
            RequestComplexity.SlightAdvantage => 3,
            RequestComplexity.NoticeableAdvantage => 4,
            RequestComplexity.MajorAdvantage => 5,
            RequestComplexity.OneSidedDeal => 6,
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets the default number of rounds allowed for this complexity.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Default round count (3, 4, 5, 6, or 7).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    /// <remarks>
    /// More complex requests allow more rounds to account for the larger gap.
    /// If rounds are exhausted without agreement, negotiation collapses.
    /// </remarks>
    public static int GetDefaultRounds(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => 3,
            RequestComplexity.SlightAdvantage => 4,
            RequestComplexity.NoticeableAdvantage => 5,
            RequestComplexity.MajorAdvantage => 6,
            RequestComplexity.OneSidedDeal => 7,
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Human-readable complexity name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    public static string GetDisplayName(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => "Fair Trade",
            RequestComplexity.SlightAdvantage => "Slight Advantage",
            RequestComplexity.NoticeableAdvantage => "Noticeable Advantage",
            RequestComplexity.MajorAdvantage => "Major Advantage",
            RequestComplexity.OneSidedDeal => "One-Sided Deal",
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets a description of what this complexity means.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Descriptive text explaining the complexity level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    public static string GetDescription(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade =>
                "An equal exchange where both parties benefit equally.",
            RequestComplexity.SlightAdvantage =>
                "You're asking for a minor edge in the deal.",
            RequestComplexity.NoticeableAdvantage =>
                "You're seeking a clear benefit over them.",
            RequestComplexity.MajorAdvantage =>
                "You want a significant gain at their expense.",
            RequestComplexity.OneSidedDeal =>
                "You want everything in your favor.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets the difficulty rating for display purposes.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Difficulty rating string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    public static string GetDifficultyRating(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => "Easy",
            RequestComplexity.SlightAdvantage => "Moderate",
            RequestComplexity.NoticeableAdvantage => "Challenging",
            RequestComplexity.MajorAdvantage => "Difficult",
            RequestComplexity.OneSidedDeal => "Extremely Difficult",
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets the default PC starting position for this complexity.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>PC starting position (0-4).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    /// <remarks>
    /// More ambitious requests start the PC closer to Maximum Demand (0),
    /// while fair trades start closer to Compromise (5).
    /// </remarks>
    public static int GetDefaultPcStartPosition(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => 4,         // Favorable
            RequestComplexity.SlightAdvantage => 3,   // Favorable+
            RequestComplexity.NoticeableAdvantage => 2, // Strong
            RequestComplexity.MajorAdvantage => 1,    // Strong+
            RequestComplexity.OneSidedDeal => 0,      // Maximum Demand
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets the default NPC starting position for this complexity.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>NPC starting position (5-8).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    /// <remarks>
    /// NPCs typically start between Unfavorable (6) and Walk Away (8),
    /// depending on how ambitious the player's request is.
    /// </remarks>
    public static int GetDefaultNpcStartPosition(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => 6,         // Unfavorable
            RequestComplexity.SlightAdvantage => 6,   // Unfavorable
            RequestComplexity.NoticeableAdvantage => 6, // Unfavorable
            RequestComplexity.MajorAdvantage => 6,    // Unfavorable
            RequestComplexity.OneSidedDeal => 6,      // Unfavorable
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }

    /// <summary>
    /// Gets example requests for this complexity level.
    /// </summary>
    /// <param name="complexity">The request complexity.</param>
    /// <returns>Array of example request descriptions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown complexity is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExampleRequests(this RequestComplexity complexity)
    {
        return complexity switch
        {
            RequestComplexity.FairTrade => new[]
            {
                "Trade supplies for a map",
                "Even split of salvage rights",
                "Mutual defense agreement"
            },
            RequestComplexity.SlightAdvantage => new[]
            {
                "60% of the profits",
                "Priority access to resources",
                "A small bonus for your trouble"
            },
            RequestComplexity.NoticeableAdvantage => new[]
            {
                "Exclusive access to a trade route",
                "Support in an upcoming vote",
                "First pick of any salvage"
            },
            RequestComplexity.MajorAdvantage => new[]
            {
                "Full control of guild contracts",
                "Oath of non-interference in your territory",
                "Permanent reduced prices"
            },
            RequestComplexity.OneSidedDeal => new[]
            {
                "Unconditional surrender of all assets",
                "Complete control with no strings",
                "Full allegiance with no reciprocation"
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(complexity),
                complexity,
                "Unknown request complexity")
        };
    }
}
