using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// v0.43.19: Implementation of ISaveGameService.
/// Wraps SaveRepository with async methods, quick save/load, and auto-save support.
/// </summary>
public class SaveGameService : ISaveGameService
{
    private static readonly ILogger _log = Log.ForContext<SaveGameService>();

    private readonly SaveRepository _saveRepository;
    private readonly IConfigurationService _configurationService;
    private const string QuickSavePrefix = "[QuickSave]_";
    private const string AutoSavePrefix = "[AutoSave]_";
    private const int MaxAutoSaveSlots = 3;

    private PlayerCharacter? _currentPlayer;
    private WorldState? _currentWorldState;
    private int _autoSaveSlotIndex = 0;

    /// <inheritdoc/>
    public bool AutoSaveEnabled { get; set; } = true;

    /// <inheritdoc/>
    public bool HasQuickSave => _saveRepository.SaveExists(GetQuickSaveName());

    /// <inheritdoc/>
    public event EventHandler? AutoSaveStarted;

    /// <inheritdoc/>
    public event EventHandler? AutoSaveCompleted;

    /// <inheritdoc/>
    public event EventHandler<SaveFileMetadata>? SaveCompleted;

    /// <summary>
    /// Creates a new SaveGameService instance.
    /// </summary>
    public SaveGameService(
        IConfigurationService configurationService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

        // Initialize SaveRepository with data directory
        var dataPath = GetDataDirectory();
        _saveRepository = new SaveRepository(dataPath);

        // Load auto-save setting from configuration
        var config = _configurationService.LoadConfiguration();
        AutoSaveEnabled = config.Gameplay.AutoSave;

        _log.Information("SaveGameService initialized with data directory: {DataPath}", dataPath);
    }

    /// <inheritdoc/>
    public IReadOnlyList<SaveFileMetadata> GetAllSaveFiles()
    {
        try
        {
            var saves = _saveRepository.ListSaves();
            var metadata = saves
                .Select(ConvertToMetadata)
                .OrderByDescending(s => s.SaveDate)
                .ToList();

            _log.Debug("Retrieved {Count} save files", metadata.Count);
            return metadata;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get save files");
            return Array.Empty<SaveFileMetadata>();
        }
    }

    /// <inheritdoc/>
    public async Task SaveGameAsync(string saveName)
    {
        if (string.IsNullOrWhiteSpace(saveName))
            throw new ArgumentException("Save name cannot be empty.", nameof(saveName));

        await Task.Run(() =>
        {
            try
            {
                _log.Information("Saving game as '{SaveName}'", saveName);

                // For now, create a placeholder character if none exists
                // In full implementation, this would get the current game state from GameStateService
                var player = _currentPlayer ?? CreatePlaceholderPlayer(saveName);
                var worldState = _currentWorldState ?? CreatePlaceholderWorldState();

                _saveRepository.SaveGame(player, worldState);

                var metadata = new SaveFileMetadata
                {
                    FileName = saveName,
                    SaveName = saveName,
                    CharacterName = player.Name,
                    CharacterClass = player.Class.ToString(),
                    Legend = 1, // Player level tracking not yet implemented
                    CurrentFloor = worldState.DungeonsCompleted,
                    SaveDate = DateTime.Now,
                    IsAutoSave = false,
                    IsQuickSave = false
                };

                SaveCompleted?.Invoke(this, metadata);
                _log.Information("Game saved successfully as '{SaveName}'", saveName);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to save game as '{SaveName}'", saveName);
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<bool> LoadGameAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        return await Task.Run(() =>
        {
            try
            {
                _log.Information("Loading game from '{FileName}'", fileName);

                var (player, worldState, _, _, _, _) = _saveRepository.LoadGame(fileName);

                if (player == null || worldState == null)
                {
                    _log.Warning("Failed to load game - save file not found or corrupted: {FileName}", fileName);
                    return false;
                }

                _currentPlayer = player;
                _currentWorldState = worldState;

                _log.Information("Game loaded successfully from '{FileName}': {CharacterName}",
                    fileName, player.Name);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to load game from '{FileName}'", fileName);
                return false;
            }
        });
    }

    /// <inheritdoc/>
    public void DeleteSave(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        try
        {
            _log.Information("Deleting save file '{FileName}'", fileName);
            _saveRepository.DeleteSave(fileName);
            _log.Information("Save file deleted: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to delete save file '{FileName}'", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool SaveExists(string fileName)
    {
        return _saveRepository.SaveExists(fileName);
    }

    /// <inheritdoc/>
    public async Task QuickSaveAsync()
    {
        var quickSaveName = GetQuickSaveName();

        await Task.Run(() =>
        {
            try
            {
                _log.Information("Performing quick save");

                var player = _currentPlayer ?? CreatePlaceholderPlayer(quickSaveName);
                var worldState = _currentWorldState ?? CreatePlaceholderWorldState();

                // Update player name for quick save identification
                player = new PlayerCharacter
                {
                    Name = quickSaveName,
                    Class = player.Class,
                    HP = player.HP,
                    MaxHP = player.MaxHP,
                    Stamina = player.Stamina,
                    MaxStamina = player.MaxStamina
                };

                _saveRepository.SaveGame(player, worldState);

                var metadata = new SaveFileMetadata
                {
                    FileName = quickSaveName,
                    SaveName = "Quick Save",
                    CharacterName = _currentPlayer?.Name ?? "Unknown",
                    SaveDate = DateTime.Now,
                    IsQuickSave = true
                };

                SaveCompleted?.Invoke(this, metadata);
                _log.Information("Quick save completed");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Quick save failed");
                throw;
            }
        });
    }

    /// <inheritdoc/>
    public async Task<bool> QuickLoadAsync()
    {
        var quickSaveName = GetQuickSaveName();

        if (!HasQuickSave)
        {
            _log.Warning("No quick save found");
            return false;
        }

        return await LoadGameAsync(quickSaveName);
    }

    /// <inheritdoc/>
    public async Task AutoSaveAsync()
    {
        if (!AutoSaveEnabled)
        {
            _log.Debug("Auto-save is disabled, skipping");
            return;
        }

        AutoSaveStarted?.Invoke(this, EventArgs.Empty);

        var autoSaveName = GetAutoSaveName();

        await Task.Run(() =>
        {
            try
            {
                _log.Information("Performing auto-save to slot {Slot}", _autoSaveSlotIndex);

                var player = _currentPlayer ?? CreatePlaceholderPlayer(autoSaveName);
                var worldState = _currentWorldState ?? CreatePlaceholderWorldState();

                // Update player name for auto-save identification
                player = new PlayerCharacter
                {
                    Name = autoSaveName,
                    Class = player.Class,
                    HP = player.HP,
                    MaxHP = player.MaxHP,
                    Stamina = player.Stamina,
                    MaxStamina = player.MaxStamina
                };

                _saveRepository.SaveGame(player, worldState);

                // Rotate auto-save slot
                _autoSaveSlotIndex = (_autoSaveSlotIndex + 1) % MaxAutoSaveSlots;

                var metadata = new SaveFileMetadata
                {
                    FileName = autoSaveName,
                    SaveName = $"Auto Save {_autoSaveSlotIndex}",
                    CharacterName = _currentPlayer?.Name ?? "Unknown",
                    SaveDate = DateTime.Now,
                    IsAutoSave = true
                };

                SaveCompleted?.Invoke(this, metadata);
                _log.Information("Auto-save completed to slot {Slot}", _autoSaveSlotIndex);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Auto-save failed");
                // Don't rethrow - auto-save failure shouldn't crash the game
            }
            finally
            {
                AutoSaveCompleted?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    /// <inheritdoc/>
    public SaveFileMetadata? GetMostRecentSave()
    {
        var saves = GetAllSaveFiles();
        return saves.FirstOrDefault();
    }

    /// <summary>
    /// Sets the current game state for saving.
    /// Called by game systems when state changes.
    /// </summary>
    public void SetCurrentGameState(PlayerCharacter player, WorldState worldState)
    {
        _currentPlayer = player;
        _currentWorldState = worldState;
    }

    /// <inheritdoc/>
    public PlayerCharacter? GetLoadedPlayer() => _currentPlayer;

    /// <inheritdoc/>
    public WorldState? GetLoadedWorldState() => _currentWorldState;

    /// <summary>
    /// Gets the data directory for save files.
    /// </summary>
    private static string GetDataDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dataDir = Path.Combine(appData, "RuneAndRust", "saves");

        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        return dataDir;
    }

    /// <summary>
    /// Gets the quick save file name based on current character.
    /// </summary>
    private string GetQuickSaveName()
    {
        var characterName = _currentPlayer?.Name ?? "Default";
        return $"{QuickSavePrefix}{characterName}";
    }

    /// <summary>
    /// Gets the auto-save file name for the current slot.
    /// </summary>
    private string GetAutoSaveName()
    {
        var characterName = _currentPlayer?.Name ?? "Default";
        return $"{AutoSavePrefix}{characterName}_{_autoSaveSlotIndex}";
    }

    /// <summary>
    /// Converts SaveInfo to SaveFileMetadata.
    /// </summary>
    private static SaveFileMetadata ConvertToMetadata(SaveInfo info)
    {
        var isAutoSave = info.CharacterName.StartsWith(AutoSavePrefix);
        var isQuickSave = info.CharacterName.StartsWith(QuickSavePrefix);

        var displayName = info.CharacterName;
        if (isAutoSave)
        {
            displayName = "Auto Save";
        }
        else if (isQuickSave)
        {
            displayName = "Quick Save";
        }

        return new SaveFileMetadata
        {
            FileName = info.CharacterName,
            SaveName = displayName,
            CharacterName = isAutoSave || isQuickSave
                ? ExtractCharacterName(info.CharacterName)
                : info.CharacterName,
            CharacterClass = info.Class.ToString(),
            Specialization = info.Specialization.ToString(),
            Legend = 1, // SaveInfo doesn't include Legend currently
            CurrentFloor = info.CurrentMilestone,
            BossDefeated = info.BossDefeated,
            SaveDate = info.LastPlayed,
            PlayTime = TimeSpan.Zero, // Would need to be tracked separately
            IsAutoSave = isAutoSave,
            IsQuickSave = isQuickSave
        };
    }

    /// <summary>
    /// Extracts the actual character name from prefixed save names.
    /// </summary>
    private static string ExtractCharacterName(string fileName)
    {
        if (fileName.StartsWith(AutoSavePrefix))
        {
            var name = fileName[AutoSavePrefix.Length..];
            // Remove slot number suffix
            var underscoreIndex = name.LastIndexOf('_');
            if (underscoreIndex > 0)
            {
                name = name[..underscoreIndex];
            }
            return name;
        }

        if (fileName.StartsWith(QuickSavePrefix))
        {
            return fileName[QuickSavePrefix.Length..];
        }

        return fileName;
    }

    /// <summary>
    /// Creates a placeholder player for testing/demo purposes.
    /// </summary>
    private static PlayerCharacter CreatePlaceholderPlayer(string name)
    {
        return new PlayerCharacter
        {
            Name = name,
            Class = CharacterClass.Warrior,
            HP = 25,
            MaxHP = 25,
            Stamina = 10,
            MaxStamina = 10
        };
    }

    /// <summary>
    /// Creates a placeholder world state for testing/demo purposes.
    /// </summary>
    private static WorldState CreatePlaceholderWorldState()
    {
        return new WorldState
        {
            CurrentRoomId = 1,
            DungeonsCompleted = 0
        };
    }
}
