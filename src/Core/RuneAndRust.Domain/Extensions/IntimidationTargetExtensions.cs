// ------------------------------------------------------------------------------
// <copyright file="IntimidationTargetExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the IntimidationTarget enum providing DC values,
// display names, descriptions, and behavioral information.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="IntimidationTarget"/> enum providing
/// DC values, display names, narrative descriptions, and behavioral information.
/// </summary>
public static class IntimidationTargetExtensions
{
    /// <summary>
    /// Gets the base DC for intimidating this target type.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>The base difficulty class (8, 12, 16, 20, or 24).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown target tier is provided.
    /// </exception>
    public static int GetBaseDc(this IntimidationTarget target)
    {
        return target switch
        {
            IntimidationTarget.Coward => 8,
            IntimidationTarget.Common => 12,
            IntimidationTarget.Veteran => 16,
            IntimidationTarget.Elite => 20,
            IntimidationTarget.FactionLeader => 24,
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "Unknown intimidation target tier")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>Human-readable tier name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown target tier is provided.
    /// </exception>
    public static string GetDisplayName(this IntimidationTarget target)
    {
        return target switch
        {
            IntimidationTarget.Coward => "Coward",
            IntimidationTarget.Common => "Common",
            IntimidationTarget.Veteran => "Veteran",
            IntimidationTarget.Elite => "Elite",
            IntimidationTarget.FactionLeader => "Faction Leader",
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "Unknown intimidation target tier")
        };
    }

    /// <summary>
    /// Gets a narrative description of what this tier represents.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>Narrative description for display.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown target tier is provided.
    /// </exception>
    public static string GetDescription(this IntimidationTarget target)
    {
        return target switch
        {
            IntimidationTarget.Coward =>
                "Timid individuals who frighten easily",
            IntimidationTarget.Common =>
                "Average resolve, neither brave nor cowardly",
            IntimidationTarget.Veteran =>
                "Battle-hardened, has faced death before",
            IntimidationTarget.Elite =>
                "Exceptional willpower, rarely intimidated",
            IntimidationTarget.FactionLeader =>
                "Position demands fearlessness, will not yield easily",
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "Unknown intimidation target tier")
        };
    }

    /// <summary>
    /// Gets the typical NPC response when successfully intimidated.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>Narrative of typical compliance behavior.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown target tier is provided.
    /// </exception>
    public static string GetTypicalResponse(this IntimidationTarget target)
    {
        return target switch
        {
            IntimidationTarget.Coward =>
                "Immediate compliance, visible trembling, may flee if given the chance",
            IntimidationTarget.Common =>
                "Reluctant compliance, seeks help if possible, remembers the threat",
            IntimidationTarget.Veteran =>
                "Assesses threat carefully, complies but with dignity intact",
            IntimidationTarget.Elite =>
                "May counter-threaten before yielding, never forgets the insult",
            IntimidationTarget.FactionLeader =>
                "Outraged compliance (if any), certain to seek retribution",
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "Unknown intimidation target tier")
        };
    }

    /// <summary>
    /// Gets whether this target tier is more likely to trigger [Challenge Accepted] on fumble.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>
    /// True if the target is more likely to fight back on fumble.
    /// Veteran, Elite, and FactionLeader targets are more likely to resist.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown target tier is provided.
    /// </exception>
    public static bool IsLikelyToResist(this IntimidationTarget target)
    {
        return target switch
        {
            IntimidationTarget.Coward => false,
            IntimidationTarget.Common => false,
            IntimidationTarget.Veteran => true,
            IntimidationTarget.Elite => true,
            IntimidationTarget.FactionLeader => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "Unknown intimidation target tier")
        };
    }

    /// <summary>
    /// Gets example NPC types for this target tier.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>Array of example NPC types that fall into this tier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown target tier is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExamples(this IntimidationTarget target)
    {
        return target switch
        {
            IntimidationTarget.Coward => new[]
            {
                "Nervous merchants",
                "Sheltered nobles",
                "Young or inexperienced NPCs",
                "Those already visibly afraid"
            },
            IntimidationTarget.Common => new[]
            {
                "Town guards (standard duty)",
                "Craftsmen and laborers",
                "Innkeepers and shopkeepers",
                "Travelers and commoners"
            },
            IntimidationTarget.Veteran => new[]
            {
                "Experienced soldiers",
                "Seasoned mercenaries",
                "Veteran guards (gate captains)",
                "Criminals with violent history"
            },
            IntimidationTarget.Elite => new[]
            {
                "Champions and war heroes",
                "Elite guard commanders",
                "Notorious crime lords",
                "Fanatical zealots"
            },
            IntimidationTarget.FactionLeader => new[]
            {
                "Jarls and settlement rulers",
                "Guild masters",
                "Clan chieftains",
                "High priests and war leaders"
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "Unknown intimidation target tier")
        };
    }

    /// <summary>
    /// Gets a short description including the DC for display.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <returns>Short description with DC value.</returns>
    public static string GetShortDescription(this IntimidationTarget target)
    {
        return $"{target.GetDisplayName()} (DC {target.GetBaseDc()})";
    }
}
