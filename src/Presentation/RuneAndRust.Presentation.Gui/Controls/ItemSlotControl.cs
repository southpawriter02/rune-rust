namespace RuneAndRust.Presentation.Gui.Controls;

using Avalonia;
using Avalonia.Controls.Primitives;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Represents a single inventory slot in the grid.
/// </summary>
public class ItemSlotControl : TemplatedControl
{
    /// <summary>
    /// Defines the Item property.
    /// </summary>
    public static readonly StyledProperty<Item?> ItemProperty =
        AvaloniaProperty.Register<ItemSlotControl, Item?>(nameof(Item));

    /// <summary>
    /// Defines the IsSelected property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<ItemSlotControl, bool>(nameof(IsSelected));

    /// <summary>
    /// Defines the Icon property.
    /// </summary>
    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<ItemSlotControl, string>(nameof(Icon), " ");

    /// <summary>
    /// Defines the ShortName property.
    /// </summary>
    public static readonly StyledProperty<string> ShortNameProperty =
        AvaloniaProperty.Register<ItemSlotControl, string>(nameof(ShortName), "");

    /// <summary>
    /// Gets or sets the item in this slot.
    /// </summary>
    public Item? Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this slot is selected.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets the category icon.
    /// </summary>
    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the short name.
    /// </summary>
    public string ShortName
    {
        get => GetValue(ShortNameProperty);
        set => SetValue(ShortNameProperty, value);
    }

    /// <summary>
    /// Gets whether the slot is empty.
    /// </summary>
    public bool IsEmpty => Item is null;

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip => Item?.Name ?? "Empty Slot";

    static ItemSlotControl()
    {
        ItemProperty.Changed.AddClassHandler<ItemSlotControl>((c, _) => c.OnItemChanged());
        IsSelectedProperty.Changed.AddClassHandler<ItemSlotControl>((c, _) => c.OnIsSelectedChanged());
    }

    private void OnItemChanged()
    {
        // Update computed properties
        Icon = Item is null ? " " : ItemSlotViewModel.GetCategoryIcon(Item.Type);
        ShortName = Item is null 
            ? "" 
            : Item.Name.Length > 3 ? Item.Name[..3] : Item.Name;
        
        Classes.Set("empty", IsEmpty);
        Classes.Set("filled", !IsEmpty);
    }

    private void OnIsSelectedChanged()
    {
        Classes.Set("selected", IsSelected);
    }
}
