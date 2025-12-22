using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Immutable snapshot of inventory state for UI rendering (v0.3.7a).
/// Transforms raw inventory data into display-ready format.
/// </summary>
/// <param name="CharacterName">The player character's display name.</param>
/// <param name="EquippedItems">Dictionary of slot to equipped item view (null if empty).</param>
/// <param name="BackpackItems">Ordered list of non-equipped items in the backpack.</param>
/// <param name="CurrentWeight">Current carried weight in grams.</param>
/// <param name="MaxCapacity">Maximum carry capacity in grams.</param>
/// <param name="BurdenPercentage">Weight as percentage of capacity (0-100+).</param>
/// <param name="BurdenState">Current burden state for color-coding.</param>
/// <param name="SelectedIndex">Currently selected item index (0-based, for navigation).</param>
public record InventoryViewModel(
    string CharacterName,
    Dictionary<EquipmentSlot, EquippedItemView?> EquippedItems,
    List<BackpackItemView> BackpackItems,
    int CurrentWeight,
    int MaxCapacity,
    int BurdenPercentage,
    BurdenState BurdenState,
    int SelectedIndex = 0
);

/// <summary>
/// Display-ready view of an equipped item for the equipment panel.
/// </summary>
/// <param name="Name">Display name of the item.</param>
/// <param name="Quality">Quality tier for color coding.</param>
/// <param name="DurabilityPercentage">Current durability as percentage (0-100).</param>
/// <param name="IsBroken">Whether the equipment is broken.</param>
public record EquippedItemView(
    string Name,
    QualityTier Quality,
    int DurabilityPercentage,
    bool IsBroken
);

/// <summary>
/// Display-ready view of a backpack item for the inventory list.
/// </summary>
/// <param name="Index">1-based display index for navigation.</param>
/// <param name="Name">Display name of the item.</param>
/// <param name="Quantity">Stack quantity (1 for non-stackable).</param>
/// <param name="Quality">Quality tier for color coding.</param>
/// <param name="WeightGrams">Total weight of this stack in grams.</param>
/// <param name="ItemType">Type for icon/categorization.</param>
/// <param name="IsEquipable">Whether this item can be equipped.</param>
public record BackpackItemView(
    int Index,
    string Name,
    int Quantity,
    QualityTier Quality,
    int WeightGrams,
    ItemType ItemType,
    bool IsEquipable
);
