// ═══════════════════════════════════════════════════════════════════════════════
// RecipeCategory.cs
// Represents the category of a craftable recipe.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Represents the category of a craftable recipe.
/// </summary>
/// <remarks>
/// <para>Categories are used for filtering recipes in the browser UI.</para>
/// <para>Each category represents a distinct type of craftable item.</para>
/// </remarks>
public enum RecipeCategory
{
    /// <summary>Offensive weapons (swords, daggers, axes, bows).</summary>
    Weapons,

    /// <summary>Defensive armor (shields, helmets, body armor).</summary>
    Armor,

    /// <summary>Consumable potions and elixirs (health, mana, buffs).</summary>
    Potions,

    /// <summary>Rings, amulets, and accessories.</summary>
    Jewelry,

    /// <summary>Crafting tools and utility items.</summary>
    Tools
}
