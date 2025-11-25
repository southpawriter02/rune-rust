using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.19: Extended save file metadata for UI display.
/// Provides additional information beyond basic SaveInfo.
/// </summary>
public class SaveFileMetadata
{
    /// <summary>
    /// Internal file identifier (character name in current implementation).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the save file.
    /// </summary>
    public string SaveName { get; set; } = string.Empty;

    /// <summary>
    /// Character name in this save.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Character class.
    /// </summary>
    public string CharacterClass { get; set; } = string.Empty;

    /// <summary>
    /// Character specialization.
    /// </summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Current legend (level equivalent).
    /// </summary>
    public int Legend { get; set; }

    /// <summary>
    /// Current floor/milestone in the dungeon.
    /// </summary>
    public int CurrentFloor { get; set; }

    /// <summary>
    /// Whether the boss has been defeated.
    /// </summary>
    public bool BossDefeated { get; set; }

    /// <summary>
    /// Date and time the save was created/last modified.
    /// </summary>
    public DateTime SaveDate { get; set; }

    /// <summary>
    /// Total play time for this save (estimated from save count if not tracked).
    /// </summary>
    public TimeSpan PlayTime { get; set; }

    /// <summary>
    /// Whether this is an auto-save file.
    /// </summary>
    public bool IsAutoSave { get; set; }

    /// <summary>
    /// Whether this is a quick-save file.
    /// </summary>
    public bool IsQuickSave { get; set; }
}

/// <summary>
/// v0.43.19: Service interface for save/load operations.
/// Wraps SaveRepository with async methods and additional UI features.
/// </summary>
public interface ISaveGameService
{
    /// <summary>
    /// Gets all save files with metadata.
    /// </summary>
    /// <returns>List of save file metadata, sorted by date descending.</returns>
    IReadOnlyList<SaveFileMetadata> GetAllSaveFiles();

    /// <summary>
    /// Saves the current game state with a specified name.
    /// </summary>
    /// <param name="saveName">The name for the save file.</param>
    /// <returns>Task that completes when save is finished.</returns>
    Task SaveGameAsync(string saveName);

    /// <summary>
    /// Loads a game from the specified save file.
    /// </summary>
    /// <param name="fileName">The file name (character name) to load.</param>
    /// <returns>Task that completes when load is finished. Returns true if successful.</returns>
    Task<bool> LoadGameAsync(string fileName);

    /// <summary>
    /// Deletes a save file.
    /// </summary>
    /// <param name="fileName">The file name to delete.</param>
    void DeleteSave(string fileName);

    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the save exists.</returns>
    bool SaveExists(string fileName);

    /// <summary>
    /// Performs a quick save to a dedicated quick-save slot.
    /// </summary>
    /// <returns>Task that completes when quick save is finished.</returns>
    Task QuickSaveAsync();

    /// <summary>
    /// Loads from the quick-save slot.
    /// </summary>
    /// <returns>Task that completes when quick load is finished. Returns true if successful.</returns>
    Task<bool> QuickLoadAsync();

    /// <summary>
    /// Performs an auto-save to a rotating auto-save slot.
    /// </summary>
    /// <returns>Task that completes when auto-save is finished.</returns>
    Task AutoSaveAsync();

    /// <summary>
    /// Gets the most recent save file for continue functionality.
    /// </summary>
    /// <returns>The most recent save metadata, or null if no saves exist.</returns>
    SaveFileMetadata? GetMostRecentSave();

    /// <summary>
    /// Gets whether there is a quick-save available.
    /// </summary>
    bool HasQuickSave { get; }

    /// <summary>
    /// Gets whether auto-save is currently enabled.
    /// </summary>
    bool AutoSaveEnabled { get; set; }

    /// <summary>
    /// Event raised when auto-save starts.
    /// </summary>
    event EventHandler? AutoSaveStarted;

    /// <summary>
    /// Event raised when auto-save completes.
    /// </summary>
    event EventHandler? AutoSaveCompleted;

    /// <summary>
    /// Event raised when any save operation completes.
    /// </summary>
    event EventHandler<SaveFileMetadata>? SaveCompleted;
}
