// ------------------------------------------------------------------------------
// <copyright file="ItemRarityExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for ItemRarity enum providing display names, value multipliers,
// and UI hints.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="ItemRarity"/>.
/// </summary>
public static class ItemRarityExtensions
{
    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the human-readable display name for an item rarity.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>A human-readable name (e.g., "Uncommon").</returns>
    public static string GetDisplayName(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "Common",
            ItemRarity.Uncommon => "Uncommon",
            ItemRarity.Rare => "Rare",
            ItemRarity.Epic => "Epic",
            ItemRarity.Legendary => "Legendary",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a description of the rarity tier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>A descriptive string for the rarity tier.</returns>
    public static string GetDescription(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common =>
                "Readily available items found throughout Aethelgard.",
            ItemRarity.Uncommon =>
                "Less common items requiring effort to acquire.",
            ItemRarity.Rare =>
                "Difficult to find items, often from challenging sources.",
            ItemRarity.Epic =>
                "Exceptional items of great value and scarcity.",
            ItemRarity.Legendary =>
                "Extremely rare items, often of Old World origin.",
            _ => "Unknown rarity tier."
        };
    }

    // -------------------------------------------------------------------------
    // Value Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the value multiplier for this rarity tier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>The multiplier applied to base value.</returns>
    public static float GetValueMultiplier(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 1.0f,
            ItemRarity.Uncommon => 2.0f,
            ItemRarity.Rare => 5.0f,
            ItemRarity.Epic => 10.0f,
            ItemRarity.Legendary => 25.0f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Calculates the value of an item with this rarity.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <param name="baseValue">The base value of the item.</param>
    /// <returns>The calculated value with rarity multiplier applied.</returns>
    public static int CalculateValue(this ItemRarity rarity, int baseValue)
    {
        return (int)(baseValue * rarity.GetValueMultiplier());
    }

    // -------------------------------------------------------------------------
    // Crafting Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the crafting bonus provided by items of this rarity tier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>The bonus added to crafting rolls when using this rarity.</returns>
    public static int GetCraftingBonus(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 0,
            ItemRarity.Uncommon => 1,
            ItemRarity.Rare => 2,
            ItemRarity.Epic => 3,
            ItemRarity.Legendary => 4,
            _ => 0
        };
    }

    /// <summary>
    /// Determines if this rarity provides a crafting bonus.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>True if the rarity provides a crafting bonus.</returns>
    public static bool ProvidesCraftingBonus(this ItemRarity rarity)
    {
        return rarity.GetCraftingBonus() > 0;
    }

    // -------------------------------------------------------------------------
    // Drop Rate / Probability Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the approximate drop rate percentage for this rarity tier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>The approximate drop rate as a percentage (0-100).</returns>
    /// <remarks>
    /// These are baseline probabilities that may be modified by various
    /// factors such as luck, location, or player abilities.
    /// </remarks>
    public static float GetBaseDropRate(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 60.0f,
            ItemRarity.Uncommon => 25.0f,
            ItemRarity.Rare => 10.0f,
            ItemRarity.Epic => 4.0f,
            ItemRarity.Legendary => 1.0f,
            _ => 0.0f
        };
    }

    /// <summary>
    /// Gets the relative weight for weighted random selection.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>The weight for random selection algorithms.</returns>
    public static int GetSelectionWeight(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 100,
            ItemRarity.Uncommon => 40,
            ItemRarity.Rare => 15,
            ItemRarity.Epic => 5,
            ItemRarity.Legendary => 1,
            _ => 0
        };
    }

    // -------------------------------------------------------------------------
    // UI Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the display color for this rarity tier (hex format).
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>A hex color string for UI display.</returns>
    public static string GetDisplayColor(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "#FFFFFF",     // White
            ItemRarity.Uncommon => "#1EFF00",   // Green
            ItemRarity.Rare => "#0070DD",       // Blue
            ItemRarity.Epic => "#A335EE",       // Purple
            ItemRarity.Legendary => "#FF8000",  // Orange
            _ => "#FFFFFF"
        };
    }

    /// <summary>
    /// Gets a color hint name for this rarity tier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>A color name for UI styling.</returns>
    public static string GetColorHint(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "white",
            ItemRarity.Uncommon => "green",
            ItemRarity.Rare => "blue",
            ItemRarity.Epic => "purple",
            ItemRarity.Legendary => "orange",
            _ => "white"
        };
    }

    /// <summary>
    /// Gets an icon hint for this rarity tier.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>An icon identifier for UI rendering.</returns>
    public static string GetIconHint(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "circle-dot",
            ItemRarity.Uncommon => "circle-dot-double",
            ItemRarity.Rare => "diamond",
            ItemRarity.Epic => "gem",
            ItemRarity.Legendary => "star",
            _ => "circle"
        };
    }

    /// <summary>
    /// Gets a border style hint for item frames.
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>A border style identifier.</returns>
    public static string GetBorderStyle(this ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "solid",
            ItemRarity.Uncommon => "solid",
            ItemRarity.Rare => "double",
            ItemRarity.Epic => "groove",
            ItemRarity.Legendary => "ridge",
            _ => "solid"
        };
    }

    // -------------------------------------------------------------------------
    // Comparison Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if this rarity is at least the specified tier.
    /// </summary>
    /// <param name="rarity">The item rarity to check.</param>
    /// <param name="minimumRarity">The minimum required rarity.</param>
    /// <returns>True if the rarity meets or exceeds the minimum.</returns>
    public static bool IsAtLeast(this ItemRarity rarity, ItemRarity minimumRarity)
    {
        return rarity >= minimumRarity;
    }

    /// <summary>
    /// Determines if this is a high-value rarity (Rare or above).
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>True for Rare, Epic, and Legendary items.</returns>
    public static bool IsHighValue(this ItemRarity rarity)
    {
        return rarity >= ItemRarity.Rare;
    }

    /// <summary>
    /// Determines if this is an exceptional rarity (Epic or above).
    /// </summary>
    /// <param name="rarity">The item rarity.</param>
    /// <returns>True for Epic and Legendary items.</returns>
    public static bool IsExceptional(this ItemRarity rarity)
    {
        return rarity >= ItemRarity.Epic;
    }

    // -------------------------------------------------------------------------
    // Lock Type Mapping
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the typical rarity of salvage from a lock type.
    /// </summary>
    /// <param name="lockType">The type of lock.</param>
    /// <returns>The typical rarity of salvaged components.</returns>
    public static ItemRarity GetTypicalSalvageRarity(this LockType lockType)
    {
        return lockType switch
        {
            LockType.ImprovisedLatch => ItemRarity.Common,
            LockType.SimpleLock => ItemRarity.Uncommon,
            LockType.StandardLock => ItemRarity.Uncommon,
            LockType.ComplexLock => ItemRarity.Rare,
            LockType.MasterLock => ItemRarity.Rare,
            LockType.JotunForged => ItemRarity.Legendary,
            _ => ItemRarity.Common
        };
    }
}
