using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.3: Orchestrates Sector navigation with Aethelgard terminology.
/// Survivor explores rooms, searches for loot, rests at field camps, encounters Undying.
/// Thin controller that coordinates between UI and Engine services.
///
/// Architecture Note: Uses a Dungeon (from ViewModel) for room lookups while GameState
/// tracks the authoritative game state. The DungeonGraph in GameState is for structural
/// validation while Dungeon provides room-level gameplay access.
/// </summary>
public class ExplorationController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly IEncounterService _encounterService;
    private readonly IRoomFeatureService _roomFeatureService;
    private readonly INavigationService _navigationService;
    private DungeonExplorationViewModel? _viewModel;
    private Dungeon? _dungeon; // Room lookup source

    /// <summary>
    /// Event raised when combat should be initiated.
    /// </summary>
    public event EventHandler<CombatInitiationEventArgs>? CombatInitiated;

    /// <summary>
    /// Event raised when a room is entered.
    /// </summary>
    public event EventHandler<Room>? RoomEntered;

    /// <summary>
    /// Event raised when a message should be displayed.
    /// </summary>
    public event EventHandler<string>? MessageRaised;

    public ExplorationController(
        ILogger logger,
        GameStateController gameStateController,
        IEncounterService encounterService,
        IRoomFeatureService roomFeatureService,
        INavigationService navigationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _encounterService = encounterService ?? throw new ArgumentNullException(nameof(encounterService));
        _roomFeatureService = roomFeatureService ?? throw new ArgumentNullException(nameof(roomFeatureService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>
    /// Initializes the controller with the exploration view model and dungeon.
    /// </summary>
    /// <param name="viewModel">The exploration view model for UI updates.</param>
    /// <param name="dungeon">Optional dungeon for room lookups. If null, uses ViewModel's dungeon.</param>
    public void Initialize(DungeonExplorationViewModel viewModel, Dungeon? dungeon = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _dungeon = dungeon ?? viewModel.CurrentDungeon;

        if (!_gameStateController.HasActiveGame)
        {
            _logger.Warning("ExplorationController initialized without active game");
            return;
        }

        var gameState = _gameStateController.CurrentGameState;
        _logger.Information("Sector navigation initialized: Survivor {Name} at depth {Depth}",
            gameState.Player?.Name ?? "Unknown",
            gameState.CurrentRoom?.GetDepthInMeters() ?? 0);
    }

    /// <summary>
    /// Sets the dungeon for room lookups.
    /// </summary>
    public void SetDungeon(Dungeon dungeon)
    {
        _dungeon = dungeon ?? throw new ArgumentNullException(nameof(dungeon));
        _logger.Debug("Dungeon set: {RoomCount} rooms, biome {Biome}",
            dungeon.TotalRoomCount, dungeon.Biome);
    }

    /// <summary>
    /// Handles movement in a cardinal direction (N/S/E/W).
    /// </summary>
    public async Task OnMoveAsync(string direction)
    {
        if (!ValidateExplorationState()) return;

        var gameState = _gameStateController.CurrentGameState;
        var currentRoom = gameState.CurrentRoom!;

        _logger.Debug("Survivor moving {Direction} from room {RoomId}", direction, currentRoom.RoomId);

        // Check if direction is valid
        var normalizedDirection = direction.ToLower();
        if (!currentRoom.Exits.TryGetValue(normalizedDirection, out var targetRoomId))
        {
            RaiseMessage("Cannot move in that direction - no exit exists.");
            return;
        }

        // Get target room from dungeon
        var targetRoom = GetRoom(targetRoomId);
        if (targetRoom == null)
        {
            _logger.Error("Target room {RoomId} not found in dungeon", targetRoomId);
            RaiseMessage("The path ahead is blocked by collapsed debris.");
            return;
        }

        // Execute the move
        await ExecuteMoveAsync(targetRoom);
    }

    /// <summary>
    /// Handles movement via vertical connection (stairs, ladder, etc.).
    /// </summary>
    public async Task OnMoveVerticalAsync(VerticalConnection connection, bool goingUp)
    {
        if (!ValidateExplorationState()) return;

        var gameState = _gameStateController.CurrentGameState;
        var currentRoom = gameState.CurrentRoom!;

        // Determine target room based on direction
        var targetRoomId = goingUp ? connection.ToRoomId : connection.FromRoomId;
        if (currentRoom.RoomId == connection.ToRoomId)
        {
            targetRoomId = connection.FromRoomId;
        }

        var targetRoom = GetRoom(targetRoomId);
        if (targetRoom == null)
        {
            _logger.Error("Vertical connection target room {RoomId} not found", targetRoomId);
            RaiseMessage("The passage is blocked.");
            return;
        }

        // Check if connection can be traversed
        if (!connection.CanTraverse())
        {
            RaiseMessage($"The {connection.Type.ToString().ToLower()} is blocked and cannot be traversed.");
            return;
        }

        _logger.Debug("Survivor moving {Direction} via {Type} to room {RoomId}",
            goingUp ? "up" : "down", connection.Type, targetRoomId);

        // Execute the move
        await ExecuteMoveAsync(targetRoom);
    }

    /// <summary>
    /// Executes a move to the target room, handling encounters.
    /// </summary>
    private async Task ExecuteMoveAsync(Room targetRoom)
    {
        var gameState = _gameStateController.CurrentGameState;
        var previousRoom = gameState.CurrentRoom!;

        // Update game state
        _gameStateController.SetCurrentRoom(targetRoom);
        targetRoom.HasBeenCleared = targetRoom.HasBeenCleared; // Mark as explored implicitly

        // Notify that room was entered
        RoomEntered?.Invoke(this, targetRoom);

        // Check for forced encounter (room has enemies)
        if (targetRoom.Enemies.Count > 0 && !targetRoom.HasBeenCleared)
        {
            _logger.Information("Forced encounter: {Count} Undying in room {RoomId}",
                targetRoom.Enemies.Count, targetRoom.RoomId);

            RaiseMessage("Undying patrol blocks your path!");
            await InitiateCombatAsync(targetRoom.Enemies, canFlee: true);
            return;
        }

        // Roll for random encounter
        var encounterResult = await _encounterService.RollForRandomEncounterAsync(gameState);
        if (encounterResult.EncounterTriggered)
        {
            _logger.Information("Random Undying encounter: difficulty {Difficulty}",
                encounterResult.DifficultyRating);

            RaiseMessage(encounterResult.Description);
            await InitiateCombatAsync(encounterResult.Enemies, encounterResult.CanFlee);
            return;
        }

        // Safe entry
        RaiseMessage($"You enter {targetRoom.Name}.");
    }

    /// <summary>
    /// Handles the search action in the current room.
    /// </summary>
    public async Task<SearchResult> OnSearchRoomAsync()
    {
        if (!ValidateExplorationState())
        {
            return SearchResult.Empty("Cannot search - invalid state.");
        }

        var currentRoom = _gameStateController.CurrentGameState.CurrentRoom!;

        _logger.Information("Survivor searching room {RoomId}", currentRoom.RoomId);

        var searchResult = await _roomFeatureService.SearchRoomAsync(currentRoom);

        // Handle search encounter
        if (searchResult.TriggeredEncounter && searchResult.Encounter != null)
        {
            _logger.Warning("Search disturbed dormant Undying");
            RaiseMessage(searchResult.Message);
            await InitiateCombatAsync(searchResult.Encounter.Enemies, searchResult.Encounter.CanFlee);
        }
        else
        {
            RaiseMessage(searchResult.Message);
        }

        return searchResult;
    }

    /// <summary>
    /// Handles the rest action in the current room.
    /// </summary>
    public async Task<RestResult> OnRestAsync()
    {
        if (!ValidateExplorationState())
        {
            return RestResult.FieldRest(0, 0, 0);
        }

        var gameState = _gameStateController.CurrentGameState;
        var survivor = gameState.Player!;
        var room = gameState.CurrentRoom!;

        // Check if enemies are present
        if (room.Enemies.Count > 0 && !room.HasBeenCleared)
        {
            RaiseMessage("Cannot rest - hostile presence detected!");
            return RestResult.FieldRest(0, 0, 0);
        }

        _logger.Information("Survivor field rest: HP={HP}/{MaxHP}, Stress={Stress}",
            survivor.HP, survivor.MaxHP, survivor.PsychicStress);

        var restResult = await _roomFeatureService.PerformFieldRestAsync(gameState);

        // Apply rest effects
        if (!restResult.InterruptedByEncounter)
        {
            survivor.HP = Math.Min(survivor.HP + restResult.HPRestored, survivor.MaxHP);
            survivor.Stamina = Math.Min(survivor.Stamina + restResult.StaminaRestored, survivor.MaxStamina);
            survivor.PsychicStress = Math.Min(survivor.PsychicStress + restResult.StressGained, 100);

            RaiseMessage(restResult.Message);
        }
        else if (restResult.InterruptingEncounter != null)
        {
            _logger.Warning("Field rest interrupted by Undying!");
            RaiseMessage(restResult.Message);
            await InitiateCombatAsync(restResult.InterruptingEncounter.Enemies, restResult.InterruptingEncounter.CanFlee);
        }

        return restResult;
    }

    /// <summary>
    /// Handles explicit combat engagement with room enemies.
    /// </summary>
    public async Task OnEngageEnemiesAsync()
    {
        if (!ValidateExplorationState()) return;

        var room = _gameStateController.CurrentGameState.CurrentRoom!;

        if (room.Enemies.Count == 0 || room.HasBeenCleared)
        {
            RaiseMessage("No enemies to engage.");
            return;
        }

        _logger.Information("Survivor engaging {Count} Undying in room {RoomId}",
            room.Enemies.Count, room.RoomId);

        await InitiateCombatAsync(room.Enemies, canFlee: true);
    }

    /// <summary>
    /// Handles return from combat (victory or flee).
    /// </summary>
    public async Task OnReturnFromCombatAsync(bool wasVictory)
    {
        if (!_gameStateController.HasActiveGame) return;

        var reason = wasVictory ? "Combat victory" : "Fled combat";
        await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, reason);

        if (wasVictory)
        {
            var room = _gameStateController.CurrentGameState.CurrentRoom;
            if (room != null)
            {
                room.HasBeenCleared = true;
                room.Enemies.Clear();
            }
            RaiseMessage("Victory! The area is now clear.");
        }
        else
        {
            RaiseMessage("You managed to escape...");
        }

        // Notify UI to refresh
        RoomEntered?.Invoke(this, _gameStateController.CurrentGameState.CurrentRoom!);
    }

    /// <summary>
    /// Attempts to solve a puzzle in the current room.
    /// </summary>
    public async Task<(bool Success, string Message, int? DamageTaken)> OnAttemptPuzzleAsync(string attributeUsed)
    {
        if (!ValidateExplorationState())
        {
            return (false, "Cannot attempt puzzle - invalid state.", null);
        }

        var gameState = _gameStateController.CurrentGameState;
        var result = await _roomFeatureService.AttemptPuzzleAsync(
            gameState.CurrentRoom!,
            gameState.Player!,
            attributeUsed);

        if (result.DamageTaken.HasValue)
        {
            gameState.Player!.HP -= result.DamageTaken.Value;

            if (gameState.Player.HP <= 0)
            {
                _logger.Warning("Survivor killed by puzzle trap!");
                await _gameStateController.UpdatePhaseAsync(GamePhase.Death, "Killed by puzzle trap");
            }
        }

        RaiseMessage(result.Message);
        return result;
    }

    /// <summary>
    /// Attempts to disable a hazard in the current room.
    /// </summary>
    public async Task<(bool Success, string Message)> OnDisableHazardAsync()
    {
        if (!ValidateExplorationState())
        {
            return (false, "Cannot disable hazard - invalid state.");
        }

        var gameState = _gameStateController.CurrentGameState;
        var result = await _roomFeatureService.DisableHazardAsync(
            gameState.CurrentRoom!,
            gameState.Player!);

        RaiseMessage(result.Message);
        return result;
    }

    /// <summary>
    /// Collects items from the ground in the current room.
    /// </summary>
    public List<Equipment> OnCollectGroundItems()
    {
        if (!ValidateExplorationState())
        {
            return new List<Equipment>();
        }

        var gameState = _gameStateController.CurrentGameState;
        var items = _roomFeatureService.CollectGroundItems(
            gameState.CurrentRoom!,
            gameState.Player!);

        // Add to player inventory
        foreach (var item in items)
        {
            gameState.Player!.Inventory.Add(item);
        }

        if (items.Count > 0)
        {
            RaiseMessage($"Collected {items.Count} item(s).");
        }

        return items;
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets a room by ID from the dungeon.
    /// </summary>
    private Room? GetRoom(string roomId)
    {
        return _dungeon?.GetRoom(roomId);
    }

    private bool ValidateExplorationState()
    {
        if (!_gameStateController.HasActiveGame)
        {
            _logger.Warning("Exploration action attempted without active game");
            return false;
        }

        var gameState = _gameStateController.CurrentGameState;

        if (gameState.Player == null)
        {
            _logger.Warning("Exploration action attempted without player");
            return false;
        }

        if (gameState.CurrentRoom == null)
        {
            _logger.Warning("Exploration action attempted without current room");
            return false;
        }

        // Note: We no longer require DungeonGraph in GameState for exploration
        // The controller uses a Dungeon instance for room lookups

        return true;
    }

    private async Task InitiateCombatAsync(List<Enemy> enemies, bool canFlee)
    {
        var gameState = _gameStateController.CurrentGameState;

        // Create combat state
        var combatState = new CombatState
        {
            Player = gameState.Player!,
            Enemies = enemies,
            CurrentRoom = gameState.CurrentRoom,
            CanFlee = canFlee,
            IsActive = true
        };

        _gameStateController.StartCombat(combatState);
        await _gameStateController.UpdatePhaseAsync(GamePhase.Combat, "Combat initiated");

        // Raise event for UI handling
        CombatInitiated?.Invoke(this, new CombatInitiationEventArgs(combatState));

        _logger.Information("Combat initiated with {Count} enemies, canFlee={CanFlee}",
            enemies.Count, canFlee);
    }

    private void RaiseMessage(string message)
    {
        MessageRaised?.Invoke(this, message);
        _viewModel?.ShowMessage(message);
    }

    #endregion
}

/// <summary>
/// Event args for combat initiation.
/// </summary>
public class CombatInitiationEventArgs : EventArgs
{
    public CombatState CombatState { get; }

    public CombatInitiationEventArgs(CombatState combatState)
    {
        CombatState = combatState;
    }
}
