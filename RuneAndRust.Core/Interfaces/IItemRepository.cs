using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for item repository operations.
/// Extends IRepository with item-specific and TPH-aware queries.
/// </summary>
public interface IItemRepository : IRepository<Item>
{
    /// <summary>
    /// Gets all items of a specific quality tier.
    /// </summary>
    /// <param name="quality">The quality tier to filter by.</param>
    /// <returns>A collection of items at that quality tier.</returns>
    Task<IEnumerable<Item>> GetByQualityAsync(QualityTier quality);

    /// <summary>
    /// Gets all items of a specific type.
    /// </summary>
    /// <param name="itemType">The item type to filter by.</param>
    /// <returns>A collection of items of that type.</returns>
    Task<IEnumerable<Item>> GetByTypeAsync(ItemType itemType);

    /// <summary>
    /// Gets all equipment items for a specific slot.
    /// Uses TPH discriminator to filter to Equipment subtype.
    /// </summary>
    /// <param name="slot">The equipment slot to filter by.</param>
    /// <returns>A collection of equipment for that slot.</returns>
    Task<IEnumerable<Equipment>> GetEquipmentBySlotAsync(EquipmentSlot slot);

    /// <summary>
    /// Gets an item by name using case-insensitive matching.
    /// </summary>
    /// <param name="name">The item name to search for.</param>
    /// <returns>The item if found, null otherwise.</returns>
    Task<Item?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all equipment items.
    /// Uses TPH discriminator to filter to Equipment subtype only.
    /// </summary>
    /// <returns>A collection of all equipment items.</returns>
    Task<IEnumerable<Equipment>> GetAllEquipmentAsync();

    /// <summary>
    /// Adds multiple items in a single operation.
    /// </summary>
    /// <param name="items">The items to add.</param>
    Task AddRangeAsync(IEnumerable<Item> items);
}
