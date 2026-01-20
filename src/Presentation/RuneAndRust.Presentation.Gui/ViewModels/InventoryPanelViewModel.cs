namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Domain.Entities;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the inventory panel.
/// </summary>
/// <remarks>
/// Manages the inventory grid display with item selection,
/// keyboard navigation, and drag-and-drop reordering support.
/// </remarks>
public partial class InventoryPanelViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the inventory slots collection.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ItemSlotViewModel> _slots = [];

    /// <summary>
    /// Gets or sets the currently selected slot.
    /// </summary>
    [ObservableProperty]
    private ItemSlotViewModel? _selectedSlot;

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    [ObservableProperty]
    private Item? _selectedItem;

    /// <summary>
    /// Gets or sets the icon for the selected item.
    /// </summary>
    [ObservableProperty]
    private string _selectedItemIcon = "";

    /// <summary>
    /// Gets or sets the current item count.
    /// </summary>
    [ObservableProperty]
    private int _currentCapacity;

    /// <summary>
    /// Gets or sets the maximum capacity.
    /// </summary>
    [ObservableProperty]
    private int _maxCapacity = 15;

    /// <summary>
    /// Gets or sets the current weight carried.
    /// </summary>
    [ObservableProperty]
    private double _currentWeight;

    /// <summary>
    /// Gets or sets the maximum weight capacity.
    /// </summary>
    [ObservableProperty]
    private double _maxWeight = 50.0;

    /// <summary>
    /// Gets or sets whether near weight limit.
    /// </summary>
    [ObservableProperty]
    private bool _isOverweightWarning;

    /// <summary>
    /// Number of columns in the inventory grid.
    /// </summary>
    public int GridColumns { get; } = 5;

    /// <summary>
    /// Design-time constructor with sample data.
    /// </summary>
    public InventoryPanelViewModel()
    {
        InitializeSlots();
        AddSampleItems();
        Log.Debug("InventoryPanelViewModel initialized with sample data");
    }

    /// <summary>
    /// Selects an inventory slot.
    /// </summary>
    [RelayCommand]
    private void SelectSlot(ItemSlotViewModel? slot)
    {
        // Deselect previous
        if (SelectedSlot is not null)
        {
            SelectedSlot.IsSelected = false;
        }

        // Select new
        SelectedSlot = slot;

        if (slot is not null)
        {
            slot.IsSelected = true;
            SelectedItem = slot.Item;
            SelectedItemIcon = slot.Icon;
            Log.Debug("Selected inventory slot {Index}: {ItemName}",
                slot.Index, slot.Item?.Name ?? "Empty");
        }
        else
        {
            SelectedItem = null;
            SelectedItemIcon = "";
        }
    }

    /// <summary>
    /// Navigates selection with arrow keys.
    /// </summary>
    [RelayCommand]
    private void Navigate(string direction)
    {
        if (SelectedSlot is null)
        {
            // Select first slot if none selected
            if (Slots.Count > 0)
                SelectSlot(Slots[0]);
            return;
        }

        var currentIndex = SelectedSlot.Index;
        var newIndex = direction switch
        {
            "left" => currentIndex > 0 ? currentIndex - 1 : currentIndex,
            "right" => currentIndex < Slots.Count - 1 ? currentIndex + 1 : currentIndex,
            "up" => currentIndex >= GridColumns ? currentIndex - GridColumns : currentIndex,
            "down" => currentIndex + GridColumns < Slots.Count ? currentIndex + GridColumns : currentIndex,
            _ => currentIndex
        };

        if (newIndex != currentIndex)
        {
            SelectSlot(Slots[newIndex]);
        }
    }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    [RelayCommand]
    private void ClearSelection()
    {
        SelectSlot(null);
    }

    /// <summary>
    /// Updates the inventory display from an Inventory entity.
    /// </summary>
    /// <param name="inventory">The inventory to display.</param>
    public void UpdateFromInventory(Inventory inventory)
    {
        Log.Debug("Updating inventory display: {Count} items", inventory.Count);

        CurrentCapacity = inventory.Count;
        MaxCapacity = inventory.Capacity;
        
        // Update weight (using item Value as placeholder since no Weight property exists)
        CurrentWeight = inventory.Items.Sum(i => i.Value * 0.1);
        IsOverweightWarning = CurrentWeight > MaxWeight * 0.9;

        // Ensure we have enough slots
        while (Slots.Count < MaxCapacity)
        {
            Slots.Add(new ItemSlotViewModel { Index = Slots.Count });
        }

        // Update slot items
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].Item = i < inventory.Items.Count ? inventory.Items[i] : null;
        }

        // Clear selection if selected item was removed
        if (SelectedSlot?.Item is null && SelectedItem is not null)
        {
            SelectSlot(null);
        }
    }

    private void InitializeSlots()
    {
        Slots.Clear();
        for (int i = 0; i < MaxCapacity; i++)
        {
            Slots.Add(new ItemSlotViewModel { Index = i });
        }
    }

    private void AddSampleItems()
    {
        CurrentCapacity = 8;
        CurrentWeight = 12.5;

        Slots[0].Item = Item.CreateIronSword();
        Slots[1].Item = Item.CreateWoodenShield();
        Slots[2].Item = Item.CreateHealthPotion();
        Slots[3].Item = Item.CreateHealthPotion();
        Slots[4].Item = Item.CreateIronKey("sample-door");
        Slots[5].Item = Item.CreateScroll();
        Slots[6].Item = Item.CreateRingOfStrength();
        Slots[7].Item = new Item(
            "Red Gem",
            "A brilliant red gemstone that sparkles with inner fire.",
            Domain.Enums.ItemType.Misc,
            value: 50
        );
    }

    /// <summary>
    /// Checks whether an item is currently equipped.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is equipped.</returns>
    /// <remarks>
    /// Currently returns false as equipment tracking is not yet implemented.
    /// Will be updated in v0.7.3 to check actual equipment state.
    /// </remarks>
    public bool IsItemEquipped(Item item)
    {
        // TODO: Integrate with equipment service when available
        return false;
    }
}

