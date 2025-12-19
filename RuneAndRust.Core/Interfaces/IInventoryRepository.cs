using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for inventory repository operations.
/// Handles the InventoryItem join table between Character and Item.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Gets all inventory entries for a character.
    /// Includes navigation properties for Item.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>A collection of inventory entries with items.</returns>
    Task<IEnumerable<InventoryItem>> GetByCharacterIdAsync(Guid characterId);

    /// <summary>
    /// Gets all equipped items for a character.
    /// Filters to entries where IsEquipped is true.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>A collection of equipped inventory entries.</returns>
    Task<IEnumerable<InventoryItem>> GetEquippedItemsAsync(Guid characterId);

    /// <summary>
    /// Gets the equipped item for a specific slot.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="slot">The equipment slot to check.</param>
    /// <returns>The inventory entry if an item is equipped in that slot, null otherwise.</returns>
    Task<InventoryItem?> GetEquippedInSlotAsync(Guid characterId, EquipmentSlot slot);

    /// <summary>
    /// Gets a specific inventory entry by character and item ID.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="itemId">The item's unique identifier.</param>
    /// <returns>The inventory entry if found, null otherwise.</returns>
    Task<InventoryItem?> GetByCharacterAndItemAsync(Guid characterId, Guid itemId);

    /// <summary>
    /// Finds an item in a character's inventory by name.
    /// Uses case-insensitive matching on the item's name.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="itemName">The item name to search for.</param>
    /// <returns>The inventory entry if found, null otherwise.</returns>
    Task<InventoryItem?> FindByItemNameAsync(Guid characterId, string itemName);

    /// <summary>
    /// Calculates the total weight of all items in a character's inventory.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>The total weight in grams.</returns>
    Task<int> GetTotalWeightAsync(Guid characterId);

    /// <summary>
    /// Gets the count of distinct items in a character's inventory.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <returns>The number of inventory slots used.</returns>
    Task<int> GetItemCountAsync(Guid characterId);

    /// <summary>
    /// Adds an inventory entry.
    /// </summary>
    /// <param name="inventoryItem">The inventory entry to add.</param>
    Task AddAsync(InventoryItem inventoryItem);

    /// <summary>
    /// Updates an existing inventory entry.
    /// </summary>
    /// <param name="inventoryItem">The inventory entry to update.</param>
    Task UpdateAsync(InventoryItem inventoryItem);

    /// <summary>
    /// Removes an inventory entry.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="itemId">The item's unique identifier.</param>
    Task RemoveAsync(Guid characterId, Guid itemId);

    /// <summary>
    /// Removes all inventory entries for a character.
    /// Used when deleting a character.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    Task ClearInventoryAsync(Guid characterId);

    /// <summary>
    /// Persists all pending changes to the data store.
    /// </summary>
    Task SaveChangesAsync();
}
