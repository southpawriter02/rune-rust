using RuneAndRust.Core.Enums;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for inventory display formatting (v0.3.7a).
/// Provides color mapping and text formatting utilities.
/// </summary>
public static class InventoryViewHelper
{
    /// <summary>
    /// Gets the Spectre.Console color name for a quality tier.
    /// </summary>
    /// <param name="quality">The quality tier to map.</param>
    /// <returns>A Spectre.Console color name string.</returns>
    public static string GetQualityColor(QualityTier quality) => quality switch
    {
        QualityTier.JuryRigged => "grey",
        QualityTier.Scavenged => "white",
        QualityTier.ClanForged => "green",
        QualityTier.Optimized => "blue",
        QualityTier.MythForged => "gold1",
        _ => "white"
    };

    /// <summary>
    /// Gets the Spectre.Console color name for a burden state.
    /// </summary>
    /// <param name="state">The burden state to map.</param>
    /// <returns>A Spectre.Console color name string.</returns>
    public static string GetBurdenColor(BurdenState state) => state switch
    {
        BurdenState.Light => "green",
        BurdenState.Heavy => "yellow",
        BurdenState.Overburdened => "red",
        _ => "white"
    };

    /// <summary>
    /// Gets the display name for an equipment slot.
    /// </summary>
    /// <param name="slot">The equipment slot to map.</param>
    /// <returns>A human-readable slot name.</returns>
    public static string GetSlotDisplayName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.MainHand => "Main Hand",
        EquipmentSlot.OffHand => "Off Hand",
        EquipmentSlot.Head => "Head",
        EquipmentSlot.Body => "Body",
        EquipmentSlot.Hands => "Hands",
        EquipmentSlot.Feet => "Feet",
        EquipmentSlot.Accessory => "Accessory",
        _ => slot.ToString()
    };

    /// <summary>
    /// Formats weight in grams to a display string.
    /// Values >= 1000g are shown in kg, otherwise in grams.
    /// </summary>
    /// <param name="grams">Weight in grams.</param>
    /// <returns>Formatted weight string (e.g., "4.2kg" or "500g").</returns>
    public static string FormatWeight(int grams)
    {
        if (grams >= 1000)
            return $"{grams / 1000.0:F1}kg";
        return $"{grams}g";
    }

    /// <summary>
    /// Renders a Unicode progress bar for burden display.
    /// Uses block characters for filled and light shade for empty.
    /// </summary>
    /// <param name="percentage">Fill percentage (0-100, clamped).</param>
    /// <param name="width">Bar width in characters (default 20).</param>
    /// <returns>A Unicode progress bar string.</returns>
    public static string RenderBurdenBar(int percentage, int width = 20)
    {
        var clamped = Math.Clamp(percentage, 0, 100);
        var filled = (int)(clamped / 100.0 * width);
        var empty = width - filled;
        return new string('\u2588', filled) + new string('\u2591', empty);
    }

    /// <summary>
    /// Formats an item name with quantity suffix if stacked.
    /// </summary>
    /// <param name="name">The item name.</param>
    /// <param name="quantity">Stack quantity.</param>
    /// <returns>Name with "(xN)" suffix if quantity > 1.</returns>
    public static string FormatItemWithQuantity(string name, int quantity)
    {
        return quantity > 1 ? $"{name} (x{quantity})" : name;
    }

    /// <summary>
    /// Gets the icon character for an item type with Spectre markup color.
    /// </summary>
    /// <param name="type">The item type to map.</param>
    /// <returns>A colored Unicode icon string.</returns>
    public static string GetItemTypeIcon(ItemType type) => type switch
    {
        ItemType.Weapon => "[red]\u2694[/]",       // Crossed swords
        ItemType.Armor => "[cyan]\u26E8[/]",       // Shield
        ItemType.Consumable => "[green]\u2615[/]", // Cup (potion)
        ItemType.Material => "[yellow]\u25C6[/]",  // Diamond (resource)
        ItemType.KeyItem => "[gold1]\u2605[/]",    // Star (important)
        ItemType.Junk => "[grey]\u25A0[/]",        // Square (misc)
        _ => "[grey]\u25A0[/]"
    };
}
