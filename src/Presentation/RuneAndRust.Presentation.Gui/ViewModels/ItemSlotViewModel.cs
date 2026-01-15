namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// View model for a single inventory slot.
/// </summary>
public partial class ItemSlotViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the slot index.
    /// </summary>
    [ObservableProperty]
    private int _index;

    /// <summary>
    /// Gets or sets the item in this slot.
    /// </summary>
    [ObservableProperty]
    private Item? _item;

    /// <summary>
    /// Gets or sets whether this slot is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets whether the slot is empty.
    /// </summary>
    public bool IsEmpty => Item is null;

    /// <summary>
    /// Gets the icon for the item category.
    /// </summary>
    public string Icon => Item is null ? " " : GetCategoryIcon(Item.Type);

    /// <summary>
    /// Gets the shortened item name (3 chars max).
    /// </summary>
    public string ShortName => Item is null 
        ? "" 
        : Item.Name.Length > 3 ? Item.Name[..3] : Item.Name;

    /// <summary>
    /// Gets the tooltip text for the slot.
    /// </summary>
    public string Tooltip => Item?.Name ?? "Empty Slot";

    partial void OnItemChanged(Item? value)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(ShortName));
        OnPropertyChanged(nameof(Tooltip));
    }

    /// <summary>
    /// Gets the category icon for an item type.
    /// </summary>
    /// <param name="type">The item type.</param>
    /// <returns>An emoji icon representing the item type.</returns>
    public static string GetCategoryIcon(ItemType type) => type switch
    {
        ItemType.Weapon => "⚔",
        ItemType.Armor => "🛡",
        ItemType.Consumable => "🧪",
        ItemType.Key => "🔑",
        ItemType.Quest => "📜",
        ItemType.Misc => "💎",
        _ => "📦"
    };
}
