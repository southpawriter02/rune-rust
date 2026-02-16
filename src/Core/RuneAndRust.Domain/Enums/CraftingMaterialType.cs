namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the types of crafting materials available for Bone-Setter recipes.
/// Used by the Antidote Craft ability (Tier 2) to validate recipe ingredients
/// and calculate output quality.
/// </summary>
/// <remarks>
/// <para>Materials are acquired through salvage, loot, and exploration.
/// The Antidote Craft recipe requires: 1 Herbs (from Medical Supplies),
/// 2 Plant Fiber, and 1 Mineral Powder.</para>
/// <para>Material quality (1–5) affects crafted item quality:
/// if all materials are Quality 3+, a +1 quality bonus is applied to the output.</para>
/// </remarks>
public enum CraftingMaterialType
{
    /// <summary>
    /// Plant fiber for binding, sourced from salvage.
    /// Required component for Basic Antidote recipe (quantity: 2).
    /// </summary>
    PlantFiber = 0,

    /// <summary>
    /// Mineral component for stabilizing compounds.
    /// Required component for Basic Antidote recipe (quantity: 1).
    /// </summary>
    MineralPowder = 1,

    /// <summary>
    /// Alchemical reagent for potency enhancement.
    /// Used in advanced recipes (v0.21+).
    /// </summary>
    AlchemicalReagent = 2,

    /// <summary>
    /// Rare binding agent for master-craft items.
    /// Provides +2 quality bonus when used instead of Plant Fiber.
    /// </summary>
    RareBindingAgent = 3,

    /// <summary>
    /// Salvaged component from the World-Machine.
    /// General-purpose crafting material for various recipes.
    /// </summary>
    MachineSalvage = 4
}
