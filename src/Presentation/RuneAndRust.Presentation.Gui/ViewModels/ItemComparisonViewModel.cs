namespace RuneAndRust.Presentation.Gui.ViewModels;

using RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// ViewModel for item comparison display.
/// </summary>
public class ItemComparisonViewModel
{
    /// <summary>Name of the item being compared against.</summary>
    public string ComparedTo { get; }

    /// <summary>Comparison lines.</summary>
    public IReadOnlyList<ComparisonLine> Lines { get; }

    /// <summary>Creates an item comparison ViewModel.</summary>
    public ItemComparisonViewModel(string comparedTo, IReadOnlyList<ComparisonLine> lines)
    {
        ComparedTo = comparedTo;
        Lines = lines;
    }

    /// <summary>Creates a comparison between two items.</summary>
    public static ItemComparisonViewModel? Create(ShopItemViewModel? newItem, ShopItemViewModel? equippedItem)
    {
        if (newItem is null || equippedItem is null) return null;

        var lines = new List<ComparisonLine>
        {
            new("Buy", $"{newItem.BuyPrice}g", $"{equippedItem.BuyPrice}g",
                CompareValues(newItem.BuyPrice, equippedItem.BuyPrice, invertBetter: true)),
            new("Sell", $"{newItem.SellPrice}g", $"{equippedItem.SellPrice}g",
                CompareValues(newItem.SellPrice, equippedItem.SellPrice))
        };

        return new ItemComparisonViewModel(equippedItem.Name, lines);
    }

    private static ComparisonResult CompareValues(int newValue, int oldValue, bool invertBetter = false)
    {
        if (newValue == oldValue) return ComparisonResult.Same;
        var better = newValue > oldValue;
        if (invertBetter) better = !better;
        return better ? ComparisonResult.Better : ComparisonResult.Worse;
    }
}
