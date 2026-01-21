// ═══════════════════════════════════════════════════════════════════════════════
// ResourceCategory.cs
// Represents the category of a crafting resource for display purposes.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Represents the category of a crafting resource for display organization.
/// </summary>
/// <remarks>
/// <para>Categories determine how resources are grouped in the inventory panel
/// and what visual styling (icon, color) is applied.</para>
/// <para>Categories are mapped from resource definition configuration strings.</para>
/// <list type="bullet">
///   <item><description><see cref="Ore"/>: Mining and metal resources</description></item>
///   <item><description><see cref="Herb"/>: Plant-based resources</description></item>
///   <item><description><see cref="Leather"/>: Animal hide resources</description></item>
///   <item><description><see cref="Gem"/>: Precious stone resources</description></item>
///   <item><description><see cref="Misc"/>: Miscellaneous crafting materials</description></item>
/// </list>
/// </remarks>
public enum ResourceCategory
{
    /// <summary>
    /// Mining and metal resources such as Iron Ore, Silver Ore, Gold Ore.
    /// </summary>
    /// <remarks>
    /// Displayed with icon [O] and typically gray/brown coloring.
    /// </remarks>
    Ore,

    /// <summary>
    /// Plant-based resources such as Healing Herbs, Mana Bloom, Fire Blossom.
    /// </summary>
    /// <remarks>
    /// Displayed with icon [H] and typically green coloring.
    /// </remarks>
    Herb,

    /// <summary>
    /// Animal hide resources such as Leather, Thick Hide, Dragon Scale.
    /// </summary>
    /// <remarks>
    /// Displayed with icon [L] and typically brown coloring.
    /// </remarks>
    Leather,

    /// <summary>
    /// Precious stone resources such as Ruby, Sapphire, Emerald.
    /// </summary>
    /// <remarks>
    /// Displayed with icon [G] and typically cyan coloring.
    /// </remarks>
    Gem,

    /// <summary>
    /// Miscellaneous crafting materials such as Enchanting Dust, Magic Essence.
    /// </summary>
    /// <remarks>
    /// Displayed with icon [M] and typically white coloring.
    /// This is the default category for resources without a specific category.
    /// </remarks>
    Misc
}
