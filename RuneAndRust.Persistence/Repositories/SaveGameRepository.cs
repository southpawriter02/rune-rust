using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for SaveGame entities.
/// Provides slot-based lookups in addition to standard CRUD operations.
/// </summary>
public class SaveGameRepository : GenericRepository<SaveGame>, ISaveGameRepository
{
    private readonly ILogger<SaveGameRepository> _saveGameLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveGameRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance for generic repository.</param>
    /// <param name="saveGameLogger">The logger instance for SaveGame-specific operations.</param>
    public SaveGameRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<SaveGame>> logger,
        ILogger<SaveGameRepository> saveGameLogger)
        : base(context, logger)
    {
        _saveGameLogger = saveGameLogger;
    }

    /// <inheritdoc/>
    public async Task<SaveGame?> GetBySlotAsync(int slotNumber)
    {
        _saveGameLogger.LogDebug("Fetching SaveGame for slot {SlotNumber}", slotNumber);

        var saveGame = await _dbSet
            .FirstOrDefaultAsync(s => s.SlotNumber == slotNumber);

        if (saveGame == null)
        {
            _saveGameLogger.LogDebug("No SaveGame found in slot {SlotNumber}", slotNumber);
        }
        else
        {
            _saveGameLogger.LogDebug("Found SaveGame '{CharacterName}' in slot {SlotNumber}",
                saveGame.CharacterName, slotNumber);
        }

        return saveGame;
    }

    /// <inheritdoc/>
    public async Task<bool> SlotExistsAsync(int slotNumber)
    {
        _saveGameLogger.LogDebug("Checking if slot {SlotNumber} exists", slotNumber);

        var exists = await _dbSet.AnyAsync(s => s.SlotNumber == slotNumber);

        _saveGameLogger.LogDebug("Slot {SlotNumber} exists: {Exists}", slotNumber, exists);

        return exists;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SaveGame>> GetAllOrderedByLastPlayedAsync()
    {
        _saveGameLogger.LogDebug("Fetching all SaveGames ordered by last played");

        var saveGames = await _dbSet
            .OrderByDescending(s => s.LastPlayed)
            .ToListAsync();

        _saveGameLogger.LogDebug("Retrieved {Count} SaveGames ordered by last played", saveGames.Count);

        return saveGames;
    }
}
