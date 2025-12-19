using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Item entity operations.
/// Provides TPH-aware queries for Item and Equipment types.
/// </summary>
public class ItemRepository : GenericRepository<Item>, IItemRepository
{
    private readonly ILogger<ItemRepository> _itemLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The generic repository logger.</param>
    /// <param name="itemLogger">The item-specific logger.</param>
    public ItemRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<Item>> logger,
        ILogger<ItemRepository> itemLogger)
        : base(context, logger)
    {
        _itemLogger = itemLogger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Item>> GetByQualityAsync(QualityTier quality)
    {
        _itemLogger.LogDebug("Fetching items with quality tier {Quality}", quality);

        var items = await _dbSet
            .Where(i => i.Quality == quality)
            .OrderBy(i => i.Name)
            .ToListAsync();

        _itemLogger.LogDebug("Retrieved {Count} items with quality tier {Quality}", items.Count, quality);

        return items;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Item>> GetByTypeAsync(ItemType itemType)
    {
        _itemLogger.LogDebug("Fetching items of type {ItemType}", itemType);

        var items = await _dbSet
            .Where(i => i.ItemType == itemType)
            .OrderBy(i => i.Name)
            .ToListAsync();

        _itemLogger.LogDebug("Retrieved {Count} items of type {ItemType}", items.Count, itemType);

        return items;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Equipment>> GetEquipmentBySlotAsync(EquipmentSlot slot)
    {
        _itemLogger.LogDebug("Fetching equipment for slot {Slot}", slot);

        var equipment = await _context.Equipment
            .Where(e => e.Slot == slot)
            .OrderBy(e => e.Name)
            .ToListAsync();

        _itemLogger.LogDebug("Retrieved {Count} equipment items for slot {Slot}", equipment.Count, slot);

        return equipment;
    }

    /// <inheritdoc/>
    public async Task<Item?> GetByNameAsync(string name)
    {
        _itemLogger.LogDebug("Searching for item by name '{ItemName}'", name);

        var normalizedName = name.Trim().ToLowerInvariant();

        var item = await _dbSet
            .FirstOrDefaultAsync(i => i.Name.ToLower() == normalizedName);

        if (item == null)
        {
            _itemLogger.LogDebug("Item '{ItemName}' not found", name);
        }
        else
        {
            _itemLogger.LogDebug("Found item '{ItemName}' ({ItemId})", item.Name, item.Id);
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Equipment>> GetAllEquipmentAsync()
    {
        _itemLogger.LogDebug("Fetching all equipment items");

        var equipment = await _context.Equipment
            .OrderBy(e => e.Name)
            .ToListAsync();

        _itemLogger.LogDebug("Retrieved {Count} equipment items", equipment.Count);

        return equipment;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<Item> items)
    {
        var itemList = items.ToList();
        _itemLogger.LogDebug("Adding {Count} items to database", itemList.Count);

        await _dbSet.AddRangeAsync(itemList);

        _itemLogger.LogDebug("Successfully added {Count} items to context", itemList.Count);
    }
}
