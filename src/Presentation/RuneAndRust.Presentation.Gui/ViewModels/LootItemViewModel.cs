namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model for displaying a loot item in the combat summary.
/// </summary>
/// <remarks>
/// Displays item name with quantity if greater than 1.
/// </remarks>
public class LootItemViewModel
{
    /// <summary>
    /// Gets the item ID.
    /// </summary>
    public string ItemId { get; }

    /// <summary>
    /// Gets the item name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the item icon.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    /// Gets the quantity of the item.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Gets the formatted display text.
    /// </summary>
    /// <remarks>
    /// Shows "• ItemName" for single items, "• ItemName (x3)" for multiple.
    /// </remarks>
    public string DisplayText => Quantity > 1
        ? $"• {Name} (x{Quantity})"
        : $"• {Name}";

    /// <summary>
    /// Creates a new loot item view model.
    /// </summary>
    /// <param name="itemId">Unique item ID.</param>
    /// <param name="name">Display name.</param>
    /// <param name="quantity">Quantity looted.</param>
    /// <param name="icon">Optional icon.</param>
    public LootItemViewModel(string itemId, string name, int quantity = 1, string? icon = null)
    {
        ItemId = itemId;
        Name = name;
        Quantity = quantity;
        Icon = icon;
    }
}
