// ------------------------------------------------------------------------------
// <copyright file="PersuasionRequestExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for PersuasionRequest enum.
// Part of v0.15.3b Persuasion System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="PersuasionRequest"/> enum.
/// </summary>
public static class PersuasionRequestExtensions
{
    /// <summary>
    /// Gets the base DC for this request complexity.
    /// </summary>
    /// <param name="request">The request complexity.</param>
    /// <returns>The base difficulty class.</returns>
    /// <remarks>
    /// DC values:
    /// <list type="bullet">
    ///   <item><description>Trivial: 8</description></item>
    ///   <item><description>Simple: 12</description></item>
    ///   <item><description>Moderate: 16</description></item>
    ///   <item><description>Major: 20</description></item>
    ///   <item><description>Extreme: 24</description></item>
    /// </list>
    /// </remarks>
    public static int GetBaseDc(this PersuasionRequest request)
    {
        return request switch
        {
            PersuasionRequest.Trivial => 8,
            PersuasionRequest.Simple => 12,
            PersuasionRequest.Moderate => 16,
            PersuasionRequest.Major => 20,
            PersuasionRequest.Extreme => 24,
            _ => 12
        };
    }

    /// <summary>
    /// Gets the number of successes typically required.
    /// </summary>
    /// <param name="request">The request complexity.</param>
    /// <returns>The target number of successes.</returns>
    public static int GetRequiredSuccesses(this PersuasionRequest request)
    {
        return request switch
        {
            PersuasionRequest.Trivial => 1,
            PersuasionRequest.Simple => 2,
            PersuasionRequest.Moderate => 3,
            PersuasionRequest.Major => 4,
            PersuasionRequest.Extreme => 4,
            _ => 2
        };
    }

    /// <summary>
    /// Gets the minimum disposition typically required for this request.
    /// </summary>
    /// <param name="request">The request complexity.</param>
    /// <returns>The minimum <see cref="NpcDisposition"/> for reasonable success chances.</returns>
    public static NpcDisposition GetMinimumDisposition(this PersuasionRequest request)
    {
        return request switch
        {
            PersuasionRequest.Trivial => NpcDisposition.Unfriendly,
            PersuasionRequest.Simple => NpcDisposition.Neutral,
            PersuasionRequest.Moderate => NpcDisposition.NeutralPositive,
            PersuasionRequest.Major => NpcDisposition.Friendly,
            PersuasionRequest.Extreme => NpcDisposition.Ally,
            _ => NpcDisposition.Neutral
        };
    }

    /// <summary>
    /// Gets whether this request may be refused outright regardless of roll.
    /// </summary>
    /// <param name="request">The request complexity.</param>
    /// <returns>True if the request may be impossible for some NPCs.</returns>
    /// <remarks>
    /// Only Extreme requests can be flagged as impossible. This happens when
    /// the request fundamentally contradicts the NPC's core values or loyalty.
    /// </remarks>
    public static bool MayBeImpossible(this PersuasionRequest request)
    {
        return request == PersuasionRequest.Extreme;
    }

    /// <summary>
    /// Gets a human-readable description of the request complexity.
    /// </summary>
    /// <param name="request">The request complexity.</param>
    /// <returns>A descriptive string for UI display.</returns>
    public static string GetDescription(this PersuasionRequest request)
    {
        return request switch
        {
            PersuasionRequest.Trivial => "Trivial (DC 8) - Minimal effort for NPC",
            PersuasionRequest.Simple => "Simple (DC 12) - Small favor or minor inconvenience",
            PersuasionRequest.Moderate => "Moderate (DC 16) - Meaningful assistance",
            PersuasionRequest.Major => "Major (DC 20) - Significant personal risk",
            PersuasionRequest.Extreme => "Extreme (DC 24) - Life-altering decision",
            _ => "Unknown request complexity"
        };
    }

    /// <summary>
    /// Gets the short name of the request complexity for compact display.
    /// </summary>
    /// <param name="request">The request complexity.</param>
    /// <returns>A short name string.</returns>
    public static string GetShortName(this PersuasionRequest request)
    {
        return request switch
        {
            PersuasionRequest.Trivial => "Trivial",
            PersuasionRequest.Simple => "Simple",
            PersuasionRequest.Moderate => "Moderate",
            PersuasionRequest.Major => "Major",
            PersuasionRequest.Extreme => "Extreme",
            _ => "Unknown"
        };
    }
}
