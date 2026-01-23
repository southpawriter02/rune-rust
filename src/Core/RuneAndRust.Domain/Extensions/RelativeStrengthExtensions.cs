// ------------------------------------------------------------------------------
// <copyright file="RelativeStrengthExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the RelativeStrength enum providing dice modifiers,
// display names, and level comparison logic.
// Part of v0.15.3d Intimidation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="RelativeStrength"/> enum providing
/// dice modifiers, display names, and factory methods for level comparison.
/// </summary>
public static class RelativeStrengthExtensions
{
    /// <summary>
    /// The level difference threshold for determining relative strength.
    /// A difference of 2+ levels in either direction triggers a bonus.
    /// </summary>
    public const int LevelThreshold = 2;

    /// <summary>
    /// Gets the bonus dice the player receives for this relative strength.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>
    /// +1 if player is stronger (outmatches target), 0 otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown relative strength is provided.
    /// </exception>
    /// <remarks>
    /// When the player is 2+ levels above the NPC, they gain +1d10 to their
    /// intimidation pool as the target recognizes they are outmatched.
    /// </remarks>
    public static int GetPlayerBonusDice(this RelativeStrength strength)
    {
        return strength switch
        {
            RelativeStrength.PlayerWeaker => 0,
            RelativeStrength.Equal => 0,
            RelativeStrength.PlayerStronger => 1,
            _ => throw new ArgumentOutOfRangeException(
                nameof(strength),
                strength,
                "Unknown relative strength")
        };
    }

    /// <summary>
    /// Gets the bonus dice the NPC receives for this relative strength.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>
    /// +1 if NPC is stronger (player is weaker), 0 otherwise.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown relative strength is provided.
    /// </exception>
    /// <remarks>
    /// When the player is 2+ levels below the NPC, the NPC gains +1d10 to their
    /// resistance as they feel capable of handling the threat.
    /// </remarks>
    public static int GetNpcBonusDice(this RelativeStrength strength)
    {
        return strength switch
        {
            RelativeStrength.PlayerWeaker => 1,
            RelativeStrength.Equal => 0,
            RelativeStrength.PlayerStronger => 0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(strength),
                strength,
                "Unknown relative strength")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>Human-readable name describing the power dynamic.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown relative strength is provided.
    /// </exception>
    public static string GetDisplayName(this RelativeStrength strength)
    {
        return strength switch
        {
            RelativeStrength.PlayerWeaker => "Outmatched",
            RelativeStrength.Equal => "Evenly Matched",
            RelativeStrength.PlayerStronger => "Dominant",
            _ => throw new ArgumentOutOfRangeException(
                nameof(strength),
                strength,
                "Unknown relative strength")
        };
    }

    /// <summary>
    /// Gets a narrative description of the power dynamic.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>Narrative description for display.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown relative strength is provided.
    /// </exception>
    public static string GetDescription(this RelativeStrength strength)
    {
        return strength switch
        {
            RelativeStrength.PlayerWeaker =>
                "The target sees through your bravado. They know they can handle you.",
            RelativeStrength.Equal =>
                "You appear to be evenly matched. The outcome depends on other factors.",
            RelativeStrength.PlayerStronger =>
                "The target's survival instincts recognize the danger. You clearly outmatch them.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(strength),
                strength,
                "Unknown relative strength")
        };
    }

    /// <summary>
    /// Gets a short description including the dice modifier effect.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>Short description with modifier information.</returns>
    public static string GetShortDescription(this RelativeStrength strength)
    {
        var playerBonus = strength.GetPlayerBonusDice();
        var npcBonus = strength.GetNpcBonusDice();

        if (playerBonus > 0)
        {
            return $"{strength.GetDisplayName()} (Player +{playerBonus}d10)";
        }

        if (npcBonus > 0)
        {
            return $"{strength.GetDisplayName()} (Target +{npcBonus}d10)";
        }

        return $"{strength.GetDisplayName()} (No modifier)";
    }

    /// <summary>
    /// Determines the relative strength based on level comparison.
    /// </summary>
    /// <param name="playerLevel">The player's current level.</param>
    /// <param name="npcLevel">The NPC's level.</param>
    /// <returns>
    /// <see cref="RelativeStrength.PlayerWeaker"/> if player level is 2+ below NPC,
    /// <see cref="RelativeStrength.PlayerStronger"/> if player level is 2+ above NPC,
    /// <see cref="RelativeStrength.Equal"/> if within ±1 level.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Level comparison rules:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>Player level &lt; NPC level - 1 → PlayerWeaker</description>
    ///   </item>
    ///   <item>
    ///     <description>Player level within ±1 of NPC level → Equal</description>
    ///   </item>
    ///   <item>
    ///     <description>Player level &gt; NPC level + 1 → PlayerStronger</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static RelativeStrength FromLevels(int playerLevel, int npcLevel)
    {
        var levelDifference = playerLevel - npcLevel;

        if (levelDifference >= LevelThreshold)
        {
            return RelativeStrength.PlayerStronger;
        }

        if (levelDifference <= -LevelThreshold)
        {
            return RelativeStrength.PlayerWeaker;
        }

        return RelativeStrength.Equal;
    }

    /// <summary>
    /// Gets whether this relative strength favors the player.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>True if the player has the advantage.</returns>
    public static bool FavorsPlayer(this RelativeStrength strength)
    {
        return strength == RelativeStrength.PlayerStronger;
    }

    /// <summary>
    /// Gets whether this relative strength favors the NPC.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>True if the NPC has the advantage.</returns>
    public static bool FavorsNpc(this RelativeStrength strength)
    {
        return strength == RelativeStrength.PlayerWeaker;
    }

    /// <summary>
    /// Gets the NPC's typical response to an intimidation attempt
    /// based on relative strength.
    /// </summary>
    /// <param name="strength">The relative strength assessment.</param>
    /// <returns>Narrative of how the NPC perceives the threat.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown relative strength is provided.
    /// </exception>
    public static string GetNpcPerception(this RelativeStrength strength)
    {
        return strength switch
        {
            RelativeStrength.PlayerWeaker =>
                "The target may even find your attempt amusing. They've faced worse.",
            RelativeStrength.Equal =>
                "The target weighs their options carefully, uncertain of the outcome.",
            RelativeStrength.PlayerStronger =>
                "The target's eyes betray their fear. They know they're outmatched.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(strength),
                strength,
                "Unknown relative strength")
        };
    }
}
