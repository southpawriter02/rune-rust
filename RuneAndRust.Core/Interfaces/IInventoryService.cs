using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Result of an inventory operation with success status and message.
/// </summary>
public record InventoryResult(bool Success, string Message);

/// <summary>
/// Defines the contract for inventory management operations.
/// Handles item addition, removal, equipment, and burden calculation.
/// </summary>
/// <remarks>See: SPEC-INV-001 for Inventory & Equipment System design.</remarks>
public interface IInventoryService
{
    /// <summary>
    /// Adds an item to a character's inventory.
    /// Handles stacking for stackable items.
    /// </summary>
    /// <param name="character">The character to add the item to.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="quantity">The quantity to add (default 1).</param>
    /// <returns>Result indicating success/failure and a message.</returns>
    Task<InventoryResult> AddItemAsync(Character character, Item item, int quantity = 1);

    /// <summary>
    /// Removes an item from a character's inventory.
    /// </summary>
    /// <param name="character">The character to remove the item from.</param>
    /// <param name="itemName">The name of the item to remove.</param>
    /// <param name="quantity">The quantity to remove (default 1).</param>
    /// <returns>Result indicating success/failure and a message.</returns>
    Task<InventoryResult> RemoveItemAsync(Character character, string itemName, int quantity = 1);

    /// <summary>
    /// Drops an item from inventory (removes completely).
    /// </summary>
    /// <param name="character">The character dropping the item.</param>
    /// <param name="itemName">The name of the item to drop.</param>
    /// <returns>Result indicating success/failure and a message.</returns>
    Task<InventoryResult> DropItemAsync(Character character, string itemName);

    /// <summary>
    /// Equips an item from inventory to its appropriate slot.
    /// Unequips any item currently in that slot.
    /// </summary>
    /// <param name="character">The character equipping the item.</param>
    /// <param name="itemName">The name of the item to equip.</param>
    /// <returns>Result indicating success/failure and a message.</returns>
    Task<InventoryResult> EquipItemAsync(Character character, string itemName);

    /// <summary>
    /// Unequips an item from a specific slot back to inventory.
    /// </summary>
    /// <param name="character">The character unequipping the item.</param>
    /// <param name="slot">The equipment slot to unequip.</param>
    /// <returns>Result indicating success/failure and a message.</returns>
    Task<InventoryResult> UnequipSlotAsync(Character character, EquipmentSlot slot);

    /// <summary>
    /// Unequips an item by name back to inventory.
    /// </summary>
    /// <param name="character">The character unequipping the item.</param>
    /// <param name="itemName">The name of the equipped item to unequip.</param>
    /// <returns>Result indicating success/failure and a message.</returns>
    Task<InventoryResult> UnequipItemAsync(Character character, string itemName);

    /// <summary>
    /// Calculates the current burden state based on carried weight.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>The current burden state.</returns>
    Task<BurdenState> CalculateBurdenAsync(Character character);

    /// <summary>
    /// Gets the maximum carry capacity in grams.
    /// Formula: MIGHT × 10,000 grams.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>Maximum capacity in grams.</returns>
    int GetMaxCapacity(Character character);

    /// <summary>
    /// Gets the current carried weight in grams.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>Current weight in grams.</returns>
    Task<int> GetCurrentWeightAsync(Character character);

    /// <summary>
    /// Checks if the character can move based on burden state.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>True if the character can move, false if overburdened.</returns>
    Task<bool> CanMoveAsync(Character character);

    /// <summary>
    /// Gets all inventory items for a character.
    /// </summary>
    /// <param name="character">The character whose inventory to get.</param>
    /// <returns>Collection of inventory items.</returns>
    Task<IEnumerable<InventoryItem>> GetInventoryAsync(Character character);

    /// <summary>
    /// Gets all equipped items for a character.
    /// </summary>
    /// <param name="character">The character whose equipment to get.</param>
    /// <returns>Collection of equipped inventory items.</returns>
    Task<IEnumerable<InventoryItem>> GetEquippedItemsAsync(Character character);

    /// <summary>
    /// Recalculates and updates the character's equipment bonuses.
    /// Should be called after equip/unequip operations.
    /// </summary>
    /// <param name="character">The character to update.</param>
    Task RecalculateEquipmentBonusesAsync(Character character);

    /// <summary>
    /// Formats the inventory for display.
    /// </summary>
    /// <param name="character">The character whose inventory to display.</param>
    /// <returns>Formatted inventory string with burden status.</returns>
    Task<string> FormatInventoryDisplayAsync(Character character);

    /// <summary>
    /// Formats the equipment for display.
    /// </summary>
    /// <param name="character">The character whose equipment to display.</param>
    /// <returns>Formatted equipment string.</returns>
    Task<string> FormatEquipmentDisplayAsync(Character character);

    /// <summary>
    /// Finds an item in a character's inventory by tag.
    /// Used by the Rest system to locate supplies (e.g., "Ration", "Water").
    /// </summary>
    /// <param name="character">The character whose inventory to search.</param>
    /// <param name="tag">The tag to search for (case-insensitive).</param>
    /// <returns>The inventory entry if found, null otherwise.</returns>
    Task<InventoryItem?> FindItemByTagAsync(Character character, string tag);

    /// <summary>
    /// Creates an immutable view model snapshot of the character's inventory (v0.3.7a).
    /// </summary>
    /// <param name="character">The character whose inventory to snapshot.</param>
    /// <param name="selectedIndex">Currently selected item index for navigation.</param>
    /// <returns>An InventoryViewModel for UI rendering.</returns>
    Task<InventoryViewModel> GetViewModelAsync(Character character, int selectedIndex = 0);
}
