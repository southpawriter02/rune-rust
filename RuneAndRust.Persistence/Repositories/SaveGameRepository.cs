using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for SaveGame entities.
/// Provides slot-based lookups in addition to standard CRUD operations.
/// </summary>
/// <remarks>See: SPEC-SAVE-001 for Save/Load System design.</remarks>
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

    /// <inheritdoc/>
    public async Task<List<SaveGameSummary>> GetSummariesAsync()
    {
        _saveGameLogger.LogDebug("[Persistence] Querying SaveGame metadata only (Projected)");

        var summaries = await _dbSet
            .Select(s => new SaveGameSummary
            {
                SlotNumber = s.SlotNumber,
                CharacterName = s.CharacterName,
                LastPlayed = s.LastPlayed,
                IsEmpty = false
            })
            .OrderByDescending(s => s.LastPlayed)
            .ToListAsync();

        _saveGameLogger.LogDebug(
            "[Persistence] Retrieved {Count} SaveGame summaries via projection",
            summaries.Count);

        return summaries;
    }

    /// <inheritdoc/>
    public async Task RotateAutosavesAsync()
    {
        _saveGameLogger.LogTrace("[DB] Starting autosave rotation");

        // Execute as a single transaction to prevent partial rotation
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Delete the oldest backup (-2)
            var oldest = await _dbSet.FirstOrDefaultAsync(s => s.SlotNumber == -2);
            if (oldest != null)
            {
                _dbSet.Remove(oldest);
                _saveGameLogger.LogInformation("[DB] Pruned oldest backup (Slot -2, ID: {Id})", oldest.Id);
            }

            await _context.SaveChangesAsync();

            // 2. Shift Backup 1 to Backup 2 (-1 -> -2)
            var backup = await _dbSet.FirstOrDefaultAsync(s => s.SlotNumber == -1);
            if (backup != null)
            {
                backup.SlotNumber = -2;
                _saveGameLogger.LogDebug("[DB] Shifted Slot -1 to -2 (ID: {Id})", backup.Id);
            }

            // 3. Shift Current to Backup 1 (0 -> -1)
            var current = await _dbSet.FirstOrDefaultAsync(s => s.SlotNumber == 0);
            if (current != null)
            {
                current.SlotNumber = -1;
                _saveGameLogger.LogDebug("[DB] Shifted Slot 0 to -1 (ID: {Id})", current.Id);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _saveGameLogger.LogTrace("[DB] Autosave rotation transaction committed");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _saveGameLogger.LogError(ex, "[DB] Rotation failed. Rolled back transaction. Error: {Error}", ex.Message);
            throw;
        }
    }
}
