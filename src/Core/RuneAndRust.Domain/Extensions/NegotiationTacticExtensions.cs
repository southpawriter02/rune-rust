// ------------------------------------------------------------------------------
// <copyright file="NegotiationTacticExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the NegotiationTactic enum providing display names,
// descriptions, side effect warnings, and system mappings.
// Part of v0.15.3e Negotiation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="NegotiationTactic"/> enum providing
/// display names, descriptions, side effect warnings, and underlying system mappings.
/// </summary>
public static class NegotiationTacticExtensions
{
    /// <summary>
    /// Gets whether this tactic requires a skill check.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>True if a skill check is required; false for Concede tactic.</returns>
    /// <remarks>
    /// The Concede tactic is automatic and does not require a check.
    /// All other tactics require skill checks using their underlying systems.
    /// </remarks>
    public static bool RequiresCheck(this NegotiationTactic tactic)
    {
        return tactic != NegotiationTactic.Concede;
    }

    /// <summary>
    /// Gets the underlying social system used by this tactic.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>
    /// The SocialInteractionType used by this tactic, or null for Concede.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Persuade uses the Persuasion system</description></item>
    ///   <item><description>Deceive uses the Deception system</description></item>
    ///   <item><description>Pressure uses the Intimidation system</description></item>
    ///   <item><description>Concede has no underlying system (automatic)</description></item>
    /// </list>
    /// </remarks>
    public static SocialInteractionType? GetUnderlyingSystem(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => SocialInteractionType.Persuasion,
            NegotiationTactic.Deceive => SocialInteractionType.Deception,
            NegotiationTactic.Pressure => SocialInteractionType.Intimidation,
            NegotiationTactic.Concede => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Human-readable tactic name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static string GetDisplayName(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => "Persuade",
            NegotiationTactic.Deceive => "Deceive",
            NegotiationTactic.Pressure => "Pressure",
            NegotiationTactic.Concede => "Concede",
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets a description of the tactic for player guidance.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Descriptive text explaining the tactic.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static string GetDescription(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade =>
                "Make an honest appeal to move the NPC's position. Safe but straightforward.",
            NegotiationTactic.Deceive =>
                "Use misleading claims to gain advantage. Risky - incurs stress and may backfire.",
            NegotiationTactic.Pressure =>
                "Apply intimidating pressure. Effective but always costs faction reputation.",
            NegotiationTactic.Concede =>
                "Give ground voluntarily to gain +2d10 and DC reduction on your next check.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets the side effects warning for this tactic.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Warning text about the tactic's side effects.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static string GetSideEffectWarning(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade =>
                "Fumble: [Trust Shattered] - All persuasion locked with this NPC.",
            NegotiationTactic.Deceive =>
                "Always: Liar's Burden stress. Fumble: [Lie Exposed] + Negotiation Collapse.",
            NegotiationTactic.Pressure =>
                "Always: Reputation cost. Fumble: [Challenge Accepted] + Combat + Collapse.",
            NegotiationTactic.Concede =>
                "Your position moves 1 step toward the NPC's position.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets whether this tactic incurs stress cost.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>True if the tactic incurs Psychic Stress.</returns>
    /// <remarks>
    /// Only the Deceive tactic incurs stress due to the Liar's Burden mechanic.
    /// </remarks>
    public static bool IncursStress(this NegotiationTactic tactic)
    {
        return tactic == NegotiationTactic.Deceive;
    }

    /// <summary>
    /// Gets whether this tactic costs reputation.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>True if the tactic always costs reputation.</returns>
    /// <remarks>
    /// Only the Pressure tactic always costs reputation (Cost of Fear).
    /// </remarks>
    public static bool CostsReputation(this NegotiationTactic tactic)
    {
        return tactic == NegotiationTactic.Pressure;
    }

    /// <summary>
    /// Gets whether a fumble with this tactic collapses the negotiation.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>True if a fumble causes immediate negotiation collapse.</returns>
    /// <remarks>
    /// Both Deceive and Pressure fumbles cause immediate negotiation collapse,
    /// in addition to their normal fumble consequences.
    /// </remarks>
    public static bool FumbleCollapsesNegotiation(this NegotiationTactic tactic)
    {
        return tactic == NegotiationTactic.Deceive || tactic == NegotiationTactic.Pressure;
    }

    /// <summary>
    /// Gets the fumble type associated with this tactic.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>The FumbleType for this tactic, or null for Concede.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static FumbleType? GetFumbleType(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => FumbleType.TrustShattered,
            NegotiationTactic.Deceive => FumbleType.LieExposed,
            NegotiationTactic.Pressure => FumbleType.ChallengeAccepted,
            NegotiationTactic.Concede => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets the primary attribute used by this tactic.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Attribute name (WILL, MIGHT, or N/A for Concede).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    /// <remarks>
    /// Persuade and Deceive use WILL. Pressure allows choice of MIGHT or WILL.
    /// Concede doesn't use any attribute (automatic).
    /// </remarks>
    public static string GetPrimaryAttribute(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => "WILL",
            NegotiationTactic.Deceive => "WILL",
            NegotiationTactic.Pressure => "MIGHT or WILL",
            NegotiationTactic.Concede => "N/A",
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets the risk level of this tactic.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Risk level string (Low, Medium, High, None).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static string GetRiskLevel(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => "Low",
            NegotiationTactic.Deceive => "High",
            NegotiationTactic.Pressure => "Medium-High",
            NegotiationTactic.Concede => "None",
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets example dialogue for this tactic.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Array of example dialogue lines.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExampleDialogue(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => new[]
            {
                "Consider our mutual interests in this matter.",
                "I believe this arrangement benefits us both.",
                "Let's find a solution that works for everyone."
            },
            NegotiationTactic.Deceive => new[]
            {
                "I have other offers at twice this price.",
                "My clan has already agreed to support your rival...",
                "This is my final offer - I'm expected elsewhere."
            },
            NegotiationTactic.Pressure => new[]
            {
                "You don't want to see what happens if we can't agree.",
                "My reputation precedes me. Ask around.",
                "Consider carefully who you're refusing."
            },
            NegotiationTactic.Concede => new[]
            {
                "I'll include this item as a sign of good faith.",
                "You drive a hard bargain. Very well.",
                "Let me sweeten the deal."
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }

    /// <summary>
    /// Gets a short name for this tactic suitable for compact displays.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <returns>Short tactic name (e.g., "Persuade", "Deceive").</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown tactic is provided.
    /// </exception>
    public static string GetShortName(this NegotiationTactic tactic)
    {
        return tactic switch
        {
            NegotiationTactic.Persuade => "Persuade",
            NegotiationTactic.Deceive => "Deceive",
            NegotiationTactic.Pressure => "Pressure",
            NegotiationTactic.Concede => "Concede",
            _ => throw new ArgumentOutOfRangeException(
                nameof(tactic),
                tactic,
                "Unknown negotiation tactic")
        };
    }
}
