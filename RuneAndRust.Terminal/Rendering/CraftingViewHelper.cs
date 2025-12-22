using RuneAndRust.Core.Enums;

namespace RuneAndRust.Terminal.Rendering;

/// <summary>
/// Static helper for crafting display formatting (v0.3.7b).
/// Provides color mapping, icons, and text formatting utilities.
/// </summary>
public static class CraftingViewHelper
{
    /// <summary>
    /// Gets the Spectre.Console color name for a crafting trade.
    /// </summary>
    /// <param name="trade">The crafting trade to map.</param>
    /// <returns>A Spectre.Console color name string.</returns>
    public static string GetTradeColor(CraftingTrade trade) => trade switch
    {
        CraftingTrade.Bodging => "orange1",      // Improvised crafting - rust/orange
        CraftingTrade.Alchemy => "green",        // Potions and chemicals - green
        CraftingTrade.Runeforging => "magenta1", // Magical enchanting - purple/magenta
        CraftingTrade.FieldMedicine => "cyan",   // Medical/healing - cyan
        _ => "white"
    };

    /// <summary>
    /// Gets the Unicode icon for a crafting trade.
    /// </summary>
    /// <param name="trade">The crafting trade to map.</param>
    /// <returns>A Unicode icon character.</returns>
    public static string GetTradeIcon(CraftingTrade trade) => trade switch
    {
        CraftingTrade.Bodging => "\u2692",       // Hammer and pick (improvised tools)
        CraftingTrade.Alchemy => "\u2697",       // Alembic (chemistry)
        CraftingTrade.Runeforging => "\u2726",   // Black four-pointed star (magic)
        CraftingTrade.FieldMedicine => "\u2695", // Staff of Aesculapius (medical)
        _ => "\u25C6"                             // Diamond (default)
    };

    /// <summary>
    /// Gets the display name for a crafting trade.
    /// </summary>
    /// <param name="trade">The crafting trade to map.</param>
    /// <returns>A human-readable trade name.</returns>
    public static string GetTradeDisplayName(CraftingTrade trade) => trade switch
    {
        CraftingTrade.Bodging => "Bodging",
        CraftingTrade.Alchemy => "Alchemy",
        CraftingTrade.Runeforging => "Runeforging",
        CraftingTrade.FieldMedicine => "Field Medicine",
        _ => trade.ToString()
    };

    /// <summary>
    /// Formats the difficulty class with estimated success chance based on WITS.
    /// Uses simple probability estimation: P(success) = 1 - (DC / (WITS + 1))
    /// </summary>
    /// <param name="dc">The recipe's difficulty class.</param>
    /// <param name="wits">The crafter's WITS attribute.</param>
    /// <returns>Formatted string like "DC 3 (83%)".</returns>
    public static string FormatDifficultyWithChance(int dc, int wits)
    {
        // Simple estimation: Each die has ~30% chance of success (8-10 on d10)
        // Expected successes = wits * 0.3
        // Rough success chance = clamp((wits * 0.3 - dc + 1) / (wits * 0.3 + 1), 0, 100)
        if (wits <= 0)
            return $"DC {dc} (0%)";

        var expectedSuccesses = wits * 0.3;
        var margin = expectedSuccesses - dc;

        // Convert margin to percentage - positive margin = good chance
        var chance = (int)Math.Clamp((margin + expectedSuccesses) / (expectedSuccesses * 2) * 100, 5, 95);

        return $"DC {dc} ({chance}%)";
    }

    /// <summary>
    /// Gets the availability indicator for a recipe.
    /// </summary>
    /// <param name="canCraft">Whether all ingredients are available.</param>
    /// <returns>"[green]\u2713[/]" for available, "[red]\u2717[/]" for unavailable.</returns>
    public static string GetAvailabilityIndicator(bool canCraft)
    {
        return canCraft ? "[green]\u2713[/]" : "[red]\u2717[/]";
    }

    /// <summary>
    /// Gets the ingredient status indicator.
    /// </summary>
    /// <param name="isSatisfied">Whether the ingredient requirement is met.</param>
    /// <returns>"[green]\u2713[/]" for satisfied, "[red]\u2717[/]" for unsatisfied.</returns>
    public static string GetIngredientIndicator(bool isSatisfied)
    {
        return isSatisfied ? "[green]\u2713[/]" : "[red]\u2717[/]";
    }

    /// <summary>
    /// Formats an ingredient line with availability status.
    /// </summary>
    /// <param name="itemName">Name of the required item.</param>
    /// <param name="required">Number required.</param>
    /// <param name="available">Number available in inventory.</param>
    /// <param name="isSatisfied">Whether requirement is met.</param>
    /// <returns>Formatted string like "  - Herb x2 ([green]\u2713[/] 5 available)".</returns>
    public static string FormatIngredientLine(string itemName, int required, int available, bool isSatisfied)
    {
        var indicator = GetIngredientIndicator(isSatisfied);
        var color = isSatisfied ? "white" : "red";
        return $"  [{color}]- {itemName} x{required}[/] ({indicator} {available} avail)";
    }

    /// <summary>
    /// Gets the color for difficulty based on DC vs WITS comparison.
    /// </summary>
    /// <param name="dc">The recipe's difficulty class.</param>
    /// <param name="wits">The crafter's WITS attribute.</param>
    /// <returns>Color name: green (easy), yellow (moderate), red (hard).</returns>
    public static string GetDifficultyColor(int dc, int wits)
    {
        var expectedSuccesses = wits * 0.3;
        var margin = expectedSuccesses - dc;

        if (margin >= 1) return "green";   // Easy - likely to succeed
        if (margin >= 0) return "yellow";  // Moderate - even odds
        return "red";                       // Hard - likely to fail
    }
}
