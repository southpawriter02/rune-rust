namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of craftable recipes in the game world.
/// </summary>
/// <remarks>
/// <para>
/// Each category represents a distinct type of item that can be crafted
/// at various crafting stations. Categories are used to filter recipes
/// in the recipe book UI and organize available crafting options.
/// </para>
/// <para>
/// Categories map to crafting stations as follows:
/// <list type="bullet">
///   <item><description>Weapon - Crafted at Anvil or Workbench</description></item>
///   <item><description>Armor - Crafted at Anvil or Workbench</description></item>
///   <item><description>Potion - Crafted at Alchemy Table</description></item>
///   <item><description>Consumable - Crafted at Cooking Fire or Workbench</description></item>
///   <item><description>Accessory - Crafted at Enchanting Table</description></item>
///   <item><description>Tool - Crafted at Anvil or Workbench</description></item>
///   <item><description>Material - Crafted at Anvil or Alchemy Table</description></item>
/// </list>
/// </para>
/// </remarks>
public enum RecipeCategory
{
    /// <summary>
    /// Offensive equipment recipes: swords, axes, bows, staffs.
    /// Typically crafted at an Anvil (metal weapons) or Workbench (wooden weapons).
    /// </summary>
    /// <example>Iron Sword, Steel Blade, Oak Bow, Mithril Blade</example>
    Weapon = 0,

    /// <summary>
    /// Defensive equipment recipes: helmets, chestplates, boots, shields.
    /// Typically crafted at an Anvil (metal armor) or Workbench (leather armor).
    /// </summary>
    /// <example>Iron Helmet, Iron Chestplate, Leather Armor, Steel Armor</example>
    Armor = 1,

    /// <summary>
    /// Drinkable concoction recipes: healing, mana, and buff potions.
    /// Crafted at an Alchemy Table.
    /// </summary>
    /// <example>Healing Potion, Mana Potion, Fire Resistance Potion</example>
    Potion = 2,

    /// <summary>
    /// Single-use item recipes: food, scrolls, bombs.
    /// Typically crafted at a Cooking Fire or Workbench.
    /// </summary>
    /// <example>Cooked Meat, Bread, Fire Bomb, Smoke Bomb</example>
    Consumable = 3,

    /// <summary>
    /// Worn accessory recipes: rings, amulets, cloaks.
    /// Typically crafted at an Enchanting Table or Workbench.
    /// </summary>
    /// <example>Silver Ring, Gold Amulet, Enchanted Cloak</example>
    Accessory = 4,

    /// <summary>
    /// Utility item recipes: pickaxes, sickles, fishing rods.
    /// Typically crafted at an Anvil (metal tools) or Workbench (wooden tools).
    /// </summary>
    /// <example>Iron Pickaxe, Skinning Knife, Fishing Rod</example>
    Tool = 5,

    /// <summary>
    /// Intermediate crafting material recipes: ingots, treated leather, refined gems.
    /// Processed resources used in other recipes.
    /// Typically crafted at an Anvil (smelting) or Alchemy Table (refining).
    /// </summary>
    /// <example>Iron Ingot, Steel Ingot, Treated Leather, Refined Ruby</example>
    Material = 6
}
