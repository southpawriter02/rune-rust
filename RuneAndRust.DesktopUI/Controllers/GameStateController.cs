using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.1: Master controller for game state management.
/// Maintains the authoritative game state and coordinates state transitions.
/// </summary>
public class GameStateController : IDisposable
{
    private readonly ILogger _logger;
    private readonly ISaveGameService _saveGameService;
    private GameState? _currentGameState;
    private System.Timers.Timer? _autoSaveTimer;
    private bool _disposed;

    /// <summary>
    /// Gets the current game state. Throws if no active game.
    /// </summary>
    public GameState CurrentGameState
    {
        get => _currentGameState ?? throw new InvalidOperationException("No active game");
    }

    /// <summary>
    /// Gets whether there is an active game session.
    /// </summary>
    public bool HasActiveGame => _currentGameState != null;

    /// <summary>
    /// Raised when the game state changes.
    /// </summary>
    public event EventHandler<GameState>? GameStateChanged;

    /// <summary>
    /// Raised when the game phase changes.
    /// </summary>
    public event EventHandler<GamePhase>? PhaseChanged;

    /// <summary>
    /// Creates a new GameStateController.
    /// </summary>
    public GameStateController(ILogger logger, ISaveGameService saveGameService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
    }

    /// <summary>
    /// Initializes a new game with the given state.
    /// </summary>
    public void InitializeNewGame(GameState gameState)
    {
        if (gameState == null)
            throw new ArgumentNullException(nameof(gameState));

        _logger.Information("Initializing new game, SessionId: {SessionId}, RunNumber: {RunNumber}",
            gameState.SessionId, gameState.RunNumber);

        // Validate initial state
        if (gameState.CurrentPhase != GamePhase.CharacterCreation)
        {
            _logger.Warning("New game should start in CharacterCreation phase, got {Phase}", gameState.CurrentPhase);
        }

        _currentGameState = gameState;
        StartAutoSaveTimer();

        GameStateChanged?.Invoke(this, gameState);
        PhaseChanged?.Invoke(this, gameState.CurrentPhase);
    }

    /// <summary>
    /// Loads an existing game state from a save.
    /// </summary>
    public void LoadGame(GameState gameState)
    {
        if (gameState == null)
            throw new ArgumentNullException(nameof(gameState));

        _logger.Information("Loading game, SessionId: {SessionId}, Phase: {Phase}",
            gameState.SessionId, gameState.CurrentPhase);

        // Validate loaded state
        gameState.Validate();

        _currentGameState = gameState;
        StartAutoSaveTimer();

        GameStateChanged?.Invoke(this, gameState);
        PhaseChanged?.Invoke(this, gameState.CurrentPhase);
    }

    /// <summary>
    /// Updates the game phase with validation and logging.
    /// </summary>
    public async Task UpdatePhaseAsync(GamePhase newPhase, string reason = "")
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot update phase without active game");

        var oldPhase = _currentGameState.CurrentPhase;

        // Validate transition
        if (!IsValidTransition(oldPhase, newPhase))
        {
            _logger.Error("Invalid phase transition: {OldPhase} → {NewPhase}", oldPhase, newPhase);
            throw new InvalidOperationException($"Invalid phase transition from {oldPhase} to {newPhase}");
        }

        _currentGameState.PreviousPhase = oldPhase;
        _currentGameState.CurrentPhase = newPhase;

        _logger.Information("Phase transition: {OldPhase} → {NewPhase}. Reason: {Reason}",
            oldPhase, newPhase, string.IsNullOrEmpty(reason) ? "(none)" : reason);

        // Validate new state
        try
        {
            _currentGameState.Validate();
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, "State validation failed after phase transition");
            // Rollback
            _currentGameState.CurrentPhase = oldPhase;
            _currentGameState.PreviousPhase = null;
            throw;
        }

        PhaseChanged?.Invoke(this, newPhase);
        GameStateChanged?.Invoke(this, _currentGameState);

        // Auto-save on significant transitions
        if (ShouldAutoSaveOnTransition(newPhase))
        {
            await AutoSaveAsync();
        }
    }

    /// <summary>
    /// Sets the player character (during character creation).
    /// </summary>
    public void SetPlayer(PlayerCharacter player)
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot set player without active game");

        if (player == null)
            throw new ArgumentNullException(nameof(player));

        _currentGameState.Player = player;
        _logger.Information("Player set: {Name} ({Class})", player.Name, player.Class);

        GameStateChanged?.Invoke(this, _currentGameState);
    }

    /// <summary>
    /// Sets the current dungeon.
    /// </summary>
    public void SetDungeon(DungeonGraph dungeon, Room startingRoom)
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot set dungeon without active game");

        _currentGameState.CurrentDungeon = dungeon ?? throw new ArgumentNullException(nameof(dungeon));
        _currentGameState.CurrentRoom = startingRoom ?? throw new ArgumentNullException(nameof(startingRoom));

        _logger.Information("Dungeon set with {NodeCount} rooms, starting in: {RoomName}",
            dungeon.NodeCount, startingRoom.Name);

        GameStateChanged?.Invoke(this, _currentGameState);
    }

    /// <summary>
    /// Sets the current room (during exploration).
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot set room without active game");

        _currentGameState.CurrentRoom = room ?? throw new ArgumentNullException(nameof(room));
        _logger.Debug("Moved to room: {RoomName} ({RoomId})", room.Name, room.RoomId);

        GameStateChanged?.Invoke(this, _currentGameState);
    }

    /// <summary>
    /// Starts combat with the given state.
    /// </summary>
    public void StartCombat(CombatState combatState)
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot start combat without active game");

        _currentGameState.CurrentCombat = combatState ?? throw new ArgumentNullException(nameof(combatState));
        _logger.Information("Combat started with {EnemyCount} enemies", combatState.Enemies.Count);

        GameStateChanged?.Invoke(this, _currentGameState);
    }

    /// <summary>
    /// Ends the current combat.
    /// </summary>
    public void EndCombat()
    {
        if (_currentGameState == null)
            throw new InvalidOperationException("Cannot end combat without active game");

        _currentGameState.CurrentCombat = null;
        _logger.Information("Combat ended");

        GameStateChanged?.Invoke(this, _currentGameState);
    }

    /// <summary>
    /// Performs an auto-save of the current game state.
    /// </summary>
    public async Task AutoSaveAsync()
    {
        if (_currentGameState == null || !_currentGameState.AutoSaveEnabled)
            return;

        try
        {
            await _saveGameService.AutoSaveAsync();
            _currentGameState.LastSaved = DateTime.UtcNow;
            _logger.Debug("Auto-save completed at {Time}", _currentGameState.LastSaved);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Auto-save failed");
            // Don't throw - auto-save failures shouldn't crash the game
        }
    }

    /// <summary>
    /// Creates a snapshot of the current state.
    /// </summary>
    public GameStateSnapshot? CreateSnapshot()
    {
        return _currentGameState?.CreateSnapshot();
    }

    /// <summary>
    /// Resets the game state (for returning to main menu).
    /// </summary>
    public void Reset()
    {
        _logger.Information("Resetting game state");

        StopAutoSaveTimer();
        _currentGameState?.Reset();
        _currentGameState = null;
    }

    /// <summary>
    /// Adds play time to the current session.
    /// </summary>
    public void AddPlayTime(TimeSpan elapsed)
    {
        if (_currentGameState != null)
        {
            _currentGameState.PlayTime += elapsed;
        }
    }

    private bool IsValidTransition(GamePhase from, GamePhase to)
    {
        // Define valid transitions
        return (from, to) switch
        {
            // From MainMenu
            (GamePhase.MainMenu, GamePhase.CharacterCreation) => true,

            // From CharacterCreation
            (GamePhase.CharacterCreation, GamePhase.DungeonExploration) => true,
            (GamePhase.CharacterCreation, GamePhase.MainMenu) => true, // Cancel

            // From DungeonExploration
            (GamePhase.DungeonExploration, GamePhase.Combat) => true,
            (GamePhase.DungeonExploration, GamePhase.CharacterProgression) => true,
            (GamePhase.DungeonExploration, GamePhase.Victory) => true,
            (GamePhase.DungeonExploration, GamePhase.Death) => true,
            (GamePhase.DungeonExploration, GamePhase.MainMenu) => true, // Quit to menu

            // From Combat
            (GamePhase.Combat, GamePhase.LootCollection) => true, // Victory
            (GamePhase.Combat, GamePhase.DungeonExploration) => true, // Fled
            (GamePhase.Combat, GamePhase.Death) => true,

            // From LootCollection
            (GamePhase.LootCollection, GamePhase.DungeonExploration) => true,
            (GamePhase.LootCollection, GamePhase.CharacterProgression) => true, // Level up

            // From CharacterProgression
            (GamePhase.CharacterProgression, GamePhase.DungeonExploration) => true,

            // From Death
            (GamePhase.Death, GamePhase.MainMenu) => true,

            // From Victory
            (GamePhase.Victory, GamePhase.EndgameMenu) => true,
            (GamePhase.Victory, GamePhase.MainMenu) => true,

            // From EndgameMenu
            (GamePhase.EndgameMenu, GamePhase.CharacterCreation) => true, // NG+
            (GamePhase.EndgameMenu, GamePhase.DungeonExploration) => true, // Challenge sector
            (GamePhase.EndgameMenu, GamePhase.MainMenu) => true,

            _ => false
        };
    }

    private bool ShouldAutoSaveOnTransition(GamePhase newPhase)
    {
        // Auto-save on significant phase transitions
        return newPhase switch
        {
            GamePhase.DungeonExploration => true,
            GamePhase.LootCollection => true,
            GamePhase.CharacterProgression => true,
            _ => false
        };
    }

    private void StartAutoSaveTimer()
    {
        StopAutoSaveTimer();

        _autoSaveTimer = new System.Timers.Timer(60000); // 60 seconds
        _autoSaveTimer.Elapsed += async (s, e) => await AutoSaveAsync();
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();

        _logger.Debug("Auto-save timer started (60s interval)");
    }

    private void StopAutoSaveTimer()
    {
        if (_autoSaveTimer != null)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Dispose();
            _autoSaveTimer = null;
            _logger.Debug("Auto-save timer stopped");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAutoSaveTimer();
            _disposed = true;
        }
    }
}
