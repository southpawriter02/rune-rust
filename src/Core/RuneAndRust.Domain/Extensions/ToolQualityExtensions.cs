// ------------------------------------------------------------------------------
// <copyright file="ToolQualityExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for ToolQuality enum providing display names, dice modifiers,
// and lock attempt validation.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="ToolQuality"/>.
/// </summary>
public static class ToolQualityExtensions
{
    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the human-readable display name for a tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A human-readable name (e.g., "Proper Tools").</returns>
    public static string GetDisplayName(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => "Bare Hands",
            ToolQuality.Improvised => "Improvised Tools",
            ToolQuality.Proper => "Proper Tools",
            ToolQuality.Masterwork => "Masterwork Tools",
            _ => "Unknown Tools"
        };
    }

    /// <summary>
    /// Gets a description of the tool quality for flavor text.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A descriptive string for the tool quality.</returns>
    public static string GetDescription(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands =>
                "Attempting with fingers only. Only viable for the simplest latches.",
            ToolQuality.Improvised =>
                "Bent wire, hairpins, or scrap metal fashioned into makeshift picks.",
            ToolQuality.Proper =>
                "Quality lockpicks from a [Tinker's Toolkit]. Standard equipment for specialists.",
            ToolQuality.Masterwork =>
                "Dvergr-crafted tools of exceptional quality. Rare and highly prized.",
            _ => "Unknown tool quality."
        };
    }

    // -------------------------------------------------------------------------
    // Dice Modifier Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the dice pool modifier for this tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>
    /// The number of d10s to add (positive) or remove (negative) from the dice pool.
    /// BareHands: -2, Improvised: 0, Proper: +1, Masterwork: +2.
    /// </returns>
    public static int GetDiceModifier(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => -2,
            ToolQuality.Improvised => 0,
            ToolQuality.Proper => 1,
            ToolQuality.Masterwork => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Gets a formatted string describing the dice modifier.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A formatted modifier string (e.g., "+1d10", "-2d10", "+0").</returns>
    public static string GetDiceModifierDisplay(this ToolQuality toolQuality)
    {
        var modifier = toolQuality.GetDiceModifier();
        return modifier switch
        {
            > 0 => $"+{modifier}d10",
            < 0 => $"{modifier}d10",
            _ => "+0"
        };
    }

    /// <summary>
    /// Determines if this tool quality provides a dice bonus.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>True if the tool provides a positive dice modifier.</returns>
    public static bool ProvidesDiceBonus(this ToolQuality toolQuality)
    {
        return toolQuality.GetDiceModifier() > 0;
    }

    /// <summary>
    /// Determines if this tool quality imposes a dice penalty.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>True if the tool imposes a negative dice modifier.</returns>
    public static bool ImposeDicePenalty(this ToolQuality toolQuality)
    {
        return toolQuality.GetDiceModifier() < 0;
    }

    // -------------------------------------------------------------------------
    // Lock Attempt Validation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if this tool quality can attempt the specified lock type.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <param name="lockType">The lock type to attempt.</param>
    /// <returns>True if the tool quality meets the lock's minimum requirement.</returns>
    public static bool CanAttemptLock(this ToolQuality toolQuality, LockType lockType)
    {
        var minimumRequired = lockType.GetMinimumToolQuality();
        return toolQuality >= minimumRequired;
    }

    /// <summary>
    /// Determines if tools are required (i.e., not BareHands).
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>True if the tool quality represents actual tools.</returns>
    public static bool HasTools(this ToolQuality toolQuality)
    {
        return toolQuality > ToolQuality.BareHands;
    }

    /// <summary>
    /// Determines if this tool quality is professional grade.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>True for Proper and Masterwork tools.</returns>
    public static bool IsProfessionalGrade(this ToolQuality toolQuality)
    {
        return toolQuality >= ToolQuality.Proper;
    }

    // -------------------------------------------------------------------------
    // Fumble and Breakage Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if this tool quality can break on fumble.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>True for Improvised tools which can break; false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// Only improvised tools break on fumble. BareHands cannot break,
    /// and Proper/Masterwork tools are durable enough to survive fumbles.
    /// </para>
    /// </remarks>
    public static bool CanBreakOnFumble(this ToolQuality toolQuality)
    {
        return toolQuality == ToolQuality.Improvised;
    }

    /// <summary>
    /// Gets the fumble breakage chance percentage for this tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>Percentage chance (0-100) that tools break on fumble.</returns>
    public static int GetFumbleBreakageChance(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => 0,     // Cannot break
            ToolQuality.Improvised => 50,   // 50% chance to break
            ToolQuality.Proper => 0,        // Durable
            ToolQuality.Masterwork => 0,    // Very durable
            _ => 0
        };
    }

    // -------------------------------------------------------------------------
    // Acquisition Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the typical acquisition method for this tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A description of how these tools are typically obtained.</returns>
    public static string GetAcquisitionMethod(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands =>
                "Always available (no tools required).",
            ToolQuality.Improvised =>
                "Crafted from scrap metal components or found in ruins.",
            ToolQuality.Proper =>
                "Purchased from merchants or obtained from [Tinker's Toolkit].",
            ToolQuality.Masterwork =>
                "Crafted by Scrap-Tinkers with [Master Craftsman] ability and rare components.",
            _ => "Unknown acquisition method."
        };
    }

    /// <summary>
    /// Gets whether this tool quality is craftable by players.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>True if players can craft tools of this quality.</returns>
    public static bool IsCraftable(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => false,    // Not applicable
            ToolQuality.Improvised => true,    // Basic crafting
            ToolQuality.Proper => false,       // Must be purchased/found
            ToolQuality.Masterwork => true,    // Requires Master Craftsman
            _ => false
        };
    }

    /// <summary>
    /// Gets the crafting requirements for this tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A description of crafting requirements, or null if not craftable.</returns>
    public static string? GetCraftingRequirements(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.Improvised => "Scrap metal components, basic crafting skill.",
            ToolQuality.Masterwork => "[Master Craftsman] ability, rare components, dedicated workshop.",
            _ => null
        };
    }

    // -------------------------------------------------------------------------
    // UI Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a color hint for UI display based on tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A color name suggestion (e.g., "gray", "white", "blue", "gold").</returns>
    public static string GetColorHint(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => "gray",
            ToolQuality.Improvised => "white",
            ToolQuality.Proper => "blue",
            ToolQuality.Masterwork => "gold",
            _ => "white"
        };
    }

    /// <summary>
    /// Gets an icon hint for UI display.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>An icon identifier or description.</returns>
    public static string GetIconHint(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => "hand",
            ToolQuality.Improvised => "bent-wire",
            ToolQuality.Proper => "lockpick-set",
            ToolQuality.Masterwork => "masterwork-picks",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets the item rarity tier associated with this tool quality.
    /// </summary>
    /// <param name="toolQuality">The tool quality.</param>
    /// <returns>A rarity string matching inventory system conventions.</returns>
    public static string GetRarityTier(this ToolQuality toolQuality)
    {
        return toolQuality switch
        {
            ToolQuality.BareHands => "none",
            ToolQuality.Improvised => "common",
            ToolQuality.Proper => "uncommon",
            ToolQuality.Masterwork => "rare",
            _ => "common"
        };
    }
}
