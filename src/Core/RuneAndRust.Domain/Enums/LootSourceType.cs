namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of sources that can generate loot in the game.
/// </summary>
/// <remarks>
/// <para>
/// Loot source types are used to determine what items can drop from different
/// sources and what drop rates apply. Different loot sources have different
/// base chances for dropping rare items like recipe scrolls.
/// </para>
/// <para>
/// Drop chance multipliers by source type:
/// <list type="bullet">
///   <item><description>Chest - Standard treasure containers (15% recipe scroll chance)</description></item>
///   <item><description>Boss - Boss monster defeats (30% recipe scroll chance)</description></item>
///   <item><description>Monster - Regular monster kills (2% recipe scroll chance)</description></item>
///   <item><description>Quest - Quest completion rewards (50% recipe scroll chance)</description></item>
///   <item><description>Shop - Shop purchases (no random drops)</description></item>
///   <item><description>Crafting - Crafted item outputs (no random drops)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum LootSourceType
{
    /// <summary>
    /// Treasure chests found in the dungeon.
    /// </summary>
    /// <remarks>
    /// Chests have moderate drop rates for recipe scrolls and are commonly
    /// found throughout dungeon levels. They provide a reliable source of
    /// loot between combat encounters.
    /// </remarks>
    Chest,

    /// <summary>
    /// Boss monster drops upon defeat.
    /// </summary>
    /// <remarks>
    /// Boss monsters have higher drop rates for rare items including recipe
    /// scrolls. Bosses typically appear at the end of dungeon areas and
    /// provide significant rewards for defeating them.
    /// </remarks>
    Boss,

    /// <summary>
    /// Regular monster drops upon defeat.
    /// </summary>
    /// <remarks>
    /// Regular monsters have low drop rates for recipe scrolls but provide
    /// consistent loot opportunities throughout gameplay. The volume of
    /// monsters makes up for the lower individual drop chance.
    /// </remarks>
    Monster,

    /// <summary>
    /// Quest completion rewards.
    /// </summary>
    /// <remarks>
    /// Quest rewards have the highest drop rates for recipe scrolls as they
    /// represent significant player accomplishments. Quest givers may provide
    /// specific recipes as rewards.
    /// </remarks>
    Quest,

    /// <summary>
    /// Shop purchases from vendors.
    /// </summary>
    /// <remarks>
    /// Shop items are purchased directly and do not involve random drops.
    /// Recipe scrolls may be available for purchase from specialized vendors.
    /// </remarks>
    Shop,

    /// <summary>
    /// Crafting output items.
    /// </summary>
    /// <remarks>
    /// Crafted items are produced deterministically based on recipes and
    /// do not involve random drops. This source type is used for tracking
    /// item origins.
    /// </remarks>
    Crafting
}
