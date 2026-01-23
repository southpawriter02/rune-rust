// ------------------------------------------------------------------------------
// <copyright file="ItemRarity.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Rarity tiers for items and salvaged components affecting their value,
// availability, and crafting potential.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Rarity tiers for items and salvaged components.
/// </summary>
/// <remarks>
/// <para>
/// Item rarity affects value, crafting potential, and general availability.
/// Rarer items are more valuable and typically provide greater benefits
/// when used in crafting or as equipment.
/// </para>
/// <para>
/// Rarity tier characteristics:
/// <list type="bullet">
///   <item><description>Common: Readily available, base value (most frequent)</description></item>
///   <item><description>Uncommon: Less common, 2x base value</description></item>
///   <item><description>Rare: Difficult to find, 5x base value</description></item>
///   <item><description>Epic: Very rare, often from special sources, 10x base value</description></item>
///   <item><description>Legendary: Extremely rare, unique or Old World origin, 25x base value</description></item>
/// </list>
/// </para>
/// <para>
/// For salvageable components from locks:
/// <list type="bullet">
///   <item><description>Common: Basic mechanical parts from improvised latches</description></item>
///   <item><description>Uncommon: Quality components from simple/standard locks</description></item>
///   <item><description>Rare: Electronic components from complex/master locks</description></item>
///   <item><description>Epic/Legendary: Old World technology from Jötun-forged mechanisms</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ItemRarity
{
    /// <summary>
    /// Common items found everywhere.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Base value multiplier: 1x.
    /// Examples: Wire bundles, small springs, scrap metal.
    /// </para>
    /// </remarks>
    Common = 0,

    /// <summary>
    /// Uncommon items requiring some effort to find.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Value multiplier: 2x.
    /// Examples: High-tension springs, pin sets, quality mechanical parts.
    /// </para>
    /// </remarks>
    Uncommon = 1,

    /// <summary>
    /// Rare items found in difficult locations or from challenging sources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Value multiplier: 5x.
    /// Examples: Circuit fragments, power cell fragments, electronic components.
    /// </para>
    /// </remarks>
    Rare = 2,

    /// <summary>
    /// Epic items of exceptional quality and scarcity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Value multiplier: 10x.
    /// Examples: Intact circuits, working power cells, advanced mechanisms.
    /// </para>
    /// </remarks>
    Epic = 3,

    /// <summary>
    /// Legendary items of unique or Old World origin.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Value multiplier: 25x.
    /// Examples: Encryption chips, biometric sensors, Jötun technology.
    /// </para>
    /// </remarks>
    Legendary = 4
}
