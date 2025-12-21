using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for InventoryItem join table operations.
/// Handles inventory management and equipment state queries.
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly RuneAndRustDbContext _context;
    private readonly DbSet<InventoryItem> _dbSet;
    private readonly ILogger<InventoryRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public InventoryRepository(RuneAndRustDbContext context, ILogger<InventoryRepository> logger)
    {
        _context = context;
        _dbSet = context.InventoryItems;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryItem>> GetByCharacterIdAsync(Guid characterId)
    {
        _logger.LogDebug("Fetching inventory for character {CharacterId}", characterId);

        var items = await _dbSet
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId)
            .OrderBy(ii => ii.SlotPosition)
            .ToListAsync();

        _logger.LogDebug("Retrieved {Count} inventory items for character {CharacterId}", items.Count, characterId);

        return items;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryItem>> GetEquippedItemsAsync(Guid characterId)
    {
        _logger.LogDebug("Fetching equipped items for character {CharacterId}", characterId);

        var items = await _dbSet
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId && ii.IsEquipped)
            .ToListAsync();

        _logger.LogDebug("Retrieved {Count} equipped items for character {CharacterId}", items.Count, characterId);

        return items;
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> GetEquippedInSlotAsync(Guid characterId, EquipmentSlot slot)
    {
        _logger.LogDebug("Checking equipped item in slot {Slot} for character {CharacterId}", slot, characterId);

        var item = await _dbSet
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId && ii.IsEquipped)
            .FirstOrDefaultAsync(ii => ii.Item is Equipment && ((Equipment)ii.Item).Slot == slot);

        if (item == null)
        {
            _logger.LogDebug("No item equipped in slot {Slot} for character {CharacterId}", slot, characterId);
        }
        else
        {
            _logger.LogDebug("Found equipped item '{ItemName}' in slot {Slot}", item.Item.Name, slot);
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> GetByCharacterAndItemAsync(Guid characterId, Guid itemId)
    {
        _logger.LogDebug("Fetching inventory entry for character {CharacterId} and item {ItemId}", characterId, itemId);

        var item = await _dbSet
            .Include(ii => ii.Item)
            .FirstOrDefaultAsync(ii => ii.CharacterId == characterId && ii.ItemId == itemId);

        if (item == null)
        {
            _logger.LogDebug("No inventory entry found for character {CharacterId} and item {ItemId}", characterId, itemId);
        }
        else
        {
            _logger.LogDebug("Found inventory entry for item '{ItemName}'", item.Item.Name);
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> FindByItemNameAsync(Guid characterId, string itemName)
    {
        _logger.LogDebug("Searching for item '{ItemName}' in inventory of character {CharacterId}", itemName, characterId);

        var normalizedName = itemName.Trim().ToLowerInvariant();

        var item = await _dbSet
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId)
            .FirstOrDefaultAsync(ii => ii.Item.Name.ToLower() == normalizedName);

        if (item == null)
        {
            _logger.LogDebug("Item '{ItemName}' not found in inventory of character {CharacterId}", itemName, characterId);
        }
        else
        {
            _logger.LogDebug("Found item '{ItemName}' in inventory", item.Item.Name);
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> FindByTagAsync(Guid characterId, string tag)
    {
        _logger.LogDebug("Searching for item with tag '{Tag}' in inventory of character {CharacterId}", tag, characterId);

        var normalizedTag = tag.Trim().ToLowerInvariant();

        // Note: This query loads all items for the character and filters in memory
        // because JSONB array queries are complex in EF Core. For large inventories,
        // consider a raw SQL query with jsonb_exists.
        var items = await _dbSet
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId)
            .ToListAsync();

        var item = items.FirstOrDefault(ii =>
            ii.Item.Tags.Any(t => t.Equals(normalizedTag, StringComparison.OrdinalIgnoreCase)));

        if (item == null)
        {
            _logger.LogDebug("No item with tag '{Tag}' found in inventory of character {CharacterId}", tag, characterId);
        }
        else
        {
            _logger.LogDebug("Found item '{ItemName}' with tag '{Tag}' in inventory", item.Item.Name, tag);
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalWeightAsync(Guid characterId)
    {
        _logger.LogDebug("Calculating total inventory weight for character {CharacterId}", characterId);

        var totalWeight = await _dbSet
            .Include(ii => ii.Item)
            .Where(ii => ii.CharacterId == characterId)
            .SumAsync(ii => ii.Item.Weight * ii.Quantity);

        _logger.LogDebug("Total inventory weight for character {CharacterId}: {Weight}g", characterId, totalWeight);

        return totalWeight;
    }

    /// <inheritdoc/>
    public async Task<int> GetItemCountAsync(Guid characterId)
    {
        _logger.LogDebug("Counting inventory slots for character {CharacterId}", characterId);

        var count = await _dbSet
            .CountAsync(ii => ii.CharacterId == characterId);

        _logger.LogDebug("Character {CharacterId} has {Count} inventory slots used", characterId, count);

        return count;
    }

    /// <inheritdoc/>
    public async Task AddAsync(InventoryItem inventoryItem)
    {
        _logger.LogDebug("Adding item {ItemId} to inventory of character {CharacterId}",
            inventoryItem.ItemId, inventoryItem.CharacterId);

        await _dbSet.AddAsync(inventoryItem);

        _logger.LogDebug("Successfully added inventory entry to context");
    }

    /// <inheritdoc/>
    public Task UpdateAsync(InventoryItem inventoryItem)
    {
        _logger.LogDebug("Updating inventory entry for character {CharacterId} and item {ItemId}",
            inventoryItem.CharacterId, inventoryItem.ItemId);

        _dbSet.Update(inventoryItem);

        _logger.LogDebug("Successfully updated inventory entry in context");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(Guid characterId, Guid itemId)
    {
        _logger.LogDebug("Removing item {ItemId} from inventory of character {CharacterId}", itemId, characterId);

        var entry = await _dbSet.FirstOrDefaultAsync(ii => ii.CharacterId == characterId && ii.ItemId == itemId);

        if (entry != null)
        {
            _dbSet.Remove(entry);
            _logger.LogDebug("Successfully marked inventory entry for removal");
        }
        else
        {
            _logger.LogWarning("Attempted to remove non-existent inventory entry for character {CharacterId} and item {ItemId}",
                characterId, itemId);
        }
    }

    /// <inheritdoc/>
    public async Task ClearInventoryAsync(Guid characterId)
    {
        _logger.LogInformation("Clearing all inventory entries for character {CharacterId}", characterId);

        var entries = await _dbSet
            .Where(ii => ii.CharacterId == characterId)
            .ToListAsync();

        _dbSet.RemoveRange(entries);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleared {Count} inventory entries for character {CharacterId}", entries.Count, characterId);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        _logger.LogDebug("Saving inventory changes to database");

        var changeCount = await _context.SaveChangesAsync();

        _logger.LogDebug("Saved {ChangeCount} inventory changes to database", changeCount);
    }
}
