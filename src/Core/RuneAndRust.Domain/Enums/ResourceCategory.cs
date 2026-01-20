namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of gatherable resources in the game world.
/// </summary>
/// <remarks>
/// <para>
/// Each category represents a distinct source of materials that can be
/// gathered from the environment or harvested from creatures. Categories
/// are used to filter recipe requirements and organize inventory.
/// </para>
/// <para>
/// Categories are designed to support the crafting system:
/// <list type="bullet">
///   <item><description>Ore - Used in weapon and armor smithing</description></item>
///   <item><description>Herb - Used in alchemy and potion crafting</description></item>
///   <item><description>Leather - Used in armor and accessory crafting</description></item>
///   <item><description>Gem - Used in jewelry and enchanting</description></item>
///   <item><description>Wood - Used in weapon handles and construction</description></item>
///   <item><description>Cloth - Used in cloth armor and bags</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ResourceCategory
{
    /// <summary>
    /// Metal ores mined from rock formations and veins.
    /// Used primarily in weapon and armor crafting.
    /// </summary>
    /// <example>Iron Ore, Copper Ore, Gold Ore, Mithril Ore</example>
    Ore,

    /// <summary>
    /// Plants and herbs gathered from the environment.
    /// Used primarily in alchemy and potion crafting.
    /// </summary>
    /// <example>Healing Herb, Mana Leaf, Firebloom, Poison Root</example>
    Herb,

    /// <summary>
    /// Hides and materials harvested from creatures.
    /// Used primarily in armor and accessory crafting.
    /// </summary>
    /// <example>Leather, Thick Hide, Dragon Scale, Demon Hide</example>
    Leather,

    /// <summary>
    /// Precious stones found in veins or as monster drops.
    /// Used primarily in jewelry and enchanting.
    /// </summary>
    /// <example>Ruby, Sapphire, Emerald, Diamond</example>
    Gem,

    /// <summary>
    /// Timber cut from trees and wooden structures.
    /// Used primarily in weapon handles and construction.
    /// </summary>
    /// <example>Oak Wood, Ash Wood, Ironwood</example>
    Wood,

    /// <summary>
    /// Fabrics either crafted or found as loot.
    /// Used primarily in cloth armor and bags.
    /// </summary>
    /// <example>Linen, Silk, Enchanted Silk</example>
    Cloth
}
