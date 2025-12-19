using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages save game operations including saving and loading game state.
/// Translates between runtime GameState and persistent SaveGame entities.
/// </summary>
public class SaveManager
{
    private readonly ISaveGameRepository _saveRepo;
    private readonly ILogger<SaveManager> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveManager"/> class.
    /// </summary>
    /// <param name="saveRepo">The save game repository.</param>
    /// <param name="logger">The logger instance.</param>
    public SaveManager(ISaveGameRepository saveRepo, ILogger<SaveManager> logger)
    {
        _saveRepo = saveRepo;
        _logger = logger;
    }

    /// <summary>
    /// Saves the current game state to the specified slot.
    /// </summary>
    /// <param name="slot">The save slot number (1-3).</param>
    /// <param name="currentState">The current game state to save.</param>
    /// <returns>True if save succeeded; otherwise, false.</returns>
    public async Task<bool> SaveGameAsync(int slot, GameState currentState)
    {
        var stopwatch = Stopwatch.StartNew();
        var saveName = currentState.CurrentCharacter?.Name ?? "Unknown";

        _logger.LogInformation("Starting save to slot {Slot} ('{SaveName}')", slot, saveName);

        try
        {
            var jsonState = JsonSerializer.Serialize(currentState, JsonOptions);

            var existingSave = await _saveRepo.GetBySlotAsync(slot);

            if (existingSave != null)
            {
                _logger.LogDebug("Updating existing save in slot {Slot}", slot);

                existingSave.CharacterName = saveName;
                existingSave.LastPlayed = DateTime.UtcNow;
                existingSave.SerializedState = jsonState;

                await _saveRepo.UpdateAsync(existingSave);
            }
            else
            {
                _logger.LogDebug("Creating new save in slot {Slot}", slot);

                var newSave = new SaveGame
                {
                    SlotNumber = slot,
                    CharacterName = saveName,
                    CreatedAt = DateTime.UtcNow,
                    LastPlayed = DateTime.UtcNow,
                    SerializedState = jsonState
                };

                await _saveRepo.AddAsync(newSave);
            }

            await _saveRepo.SaveChangesAsync();

            stopwatch.Stop();
            var saveId = existingSave?.Id ?? Guid.Empty;

            _logger.LogInformation("Save completed in {Duration}ms (ID: {SaveId})",
                stopwatch.ElapsedMilliseconds, saveId);

            return true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Save failed: {Error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Loads a game state from the specified slot.
    /// </summary>
    /// <param name="slot">The save slot number to load.</param>
    /// <returns>The loaded game state if found; otherwise, null.</returns>
    public async Task<GameState?> LoadGameAsync(int slot)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting load from slot {Slot}", slot);

        try
        {
            var saveGame = await _saveRepo.GetBySlotAsync(slot);

            if (saveGame == null)
            {
                _logger.LogWarning("No save found in slot {Slot}", slot);
                return null;
            }

            var gameState = JsonSerializer.Deserialize<GameState>(saveGame.SerializedState, JsonOptions);

            if (gameState == null)
            {
                _logger.LogError("Failed to deserialize save data from slot {Slot}", slot);
                return null;
            }

            stopwatch.Stop();
            _logger.LogInformation("Load completed in {Duration}ms from slot {Slot} ('{CharacterName}')",
                stopwatch.ElapsedMilliseconds, slot, saveGame.CharacterName);

            return gameState;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Load failed: {Error}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets information about all save slots.
    /// </summary>
    /// <returns>A collection of save game summaries.</returns>
    public async Task<IEnumerable<SaveGameSummary>> GetSaveSlotSummariesAsync()
    {
        _logger.LogDebug("Fetching save slot summaries");

        var saves = await _saveRepo.GetAllOrderedByLastPlayedAsync();

        var summaries = saves.Select(s => new SaveGameSummary
        {
            SlotNumber = s.SlotNumber,
            CharacterName = s.CharacterName,
            LastPlayed = s.LastPlayed,
            IsEmpty = false
        }).ToList();

        _logger.LogDebug("Found {Count} save slots", summaries.Count);

        return summaries;
    }

    /// <summary>
    /// Deletes a save game from the specified slot.
    /// </summary>
    /// <param name="slot">The save slot number to delete.</param>
    /// <returns>True if deletion succeeded; otherwise, false.</returns>
    public async Task<bool> DeleteSaveAsync(int slot)
    {
        _logger.LogInformation("Deleting save from slot {Slot}", slot);

        try
        {
            var saveGame = await _saveRepo.GetBySlotAsync(slot);

            if (saveGame == null)
            {
                _logger.LogWarning("No save found in slot {Slot} to delete", slot);
                return false;
            }

            await _saveRepo.DeleteAsync(saveGame.Id);
            await _saveRepo.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted save from slot {Slot}", slot);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed: {Error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Checks if a save exists in the specified slot.
    /// </summary>
    /// <param name="slot">The save slot number to check.</param>
    /// <returns>True if a save exists; otherwise, false.</returns>
    public async Task<bool> SaveExistsAsync(int slot)
    {
        return await _saveRepo.SlotExistsAsync(slot);
    }
}

/// <summary>
/// Summary information about a save game slot.
/// </summary>
public class SaveGameSummary
{
    /// <summary>
    /// Gets or sets the slot number.
    /// </summary>
    public int SlotNumber { get; set; }

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the save was last played.
    /// </summary>
    public DateTime LastPlayed { get; set; }

    /// <summary>
    /// Gets or sets whether the slot is empty.
    /// </summary>
    public bool IsEmpty { get; set; } = true;
}
