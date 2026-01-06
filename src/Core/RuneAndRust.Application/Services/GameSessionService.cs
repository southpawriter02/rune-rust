using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Orchestrates game session operations including creation, loading, saving, and gameplay actions.
/// </summary>
/// <remarks>
/// GameSessionService is the primary application service that coordinates between the presentation
/// layer and domain entities. It manages the current game session state and provides high-level
/// operations for game commands like movement, item pickup, and combat.
/// </remarks>
public class GameSessionService
{
    /// <summary>
    /// Repository for persisting game sessions.
    /// </summary>
    private readonly IGameRepository _repository;

    /// <summary>
    /// Logger for service operations and diagnostics.
    /// </summary>
    private readonly ILogger<GameSessionService> _logger;

    /// <summary>
    /// Combat service for resolving combat encounters.
    /// </summary>
    private readonly CombatService _combatService;

    /// <summary>
    /// Service for applying item effects.
    /// </summary>
    private readonly ItemEffectService _itemEffectService;

    /// <summary>
    /// The currently active game session, or null if no session is active.
    /// </summary>
    private GameSession? _currentSession;

    /// <summary>
    /// Gets a value indicating whether there is an active game session.
    /// </summary>
    public bool HasActiveSession => _currentSession != null;

    /// <summary>
    /// Gets the current game state as a DTO, or null if no session is active.
    /// </summary>
    public GameStateDto? CurrentState => _currentSession?.ToDto();

    /// <summary>
    /// Creates a new instance of the GameSessionService.
    /// </summary>
    /// <param name="repository">The repository for persisting game sessions.</param>
    /// <param name="logger">The logger for service diagnostics.</param>
    /// <param name="itemEffectService">The service for applying item effects.</param>
    /// <param name="combatLogger">Optional logger for combat service diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository, logger, or itemEffectService is null.</exception>
    public GameSessionService(
        IGameRepository repository,
        ILogger<GameSessionService> logger,
        ItemEffectService itemEffectService,
        ILogger<CombatService>? combatLogger = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _itemEffectService = itemEffectService ?? throw new ArgumentNullException(nameof(itemEffectService));
        _combatService = new CombatService(combatLogger);
        _logger.LogDebug("GameSessionService initialized");
    }

    /// <summary>
    /// Creates and starts a new game session with a fresh player and starter dungeon.
    /// </summary>
    /// <param name="playerName">The name for the new player character.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The initial game state as a DTO.</returns>
    public async Task<GameStateDto> StartNewGameAsync(string playerName, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting new game for player: {PlayerName}", playerName);

        _currentSession = GameSession.CreateNew(playerName);
        await _repository.SaveAsync(_currentSession, ct);

        _logger.LogInformation("New game session created: {SessionId}", _currentSession.Id);
        _logger.LogDebug(
            "Initial game state - Player: {PlayerName}, Health: {Health}/{MaxHealth}, " +
            "StartingRoom: {RoomName}, Position: ({X},{Y}), DungeonRooms: {RoomCount}",
            _currentSession.Player.Name,
            _currentSession.Player.Health,
            _currentSession.Player.Stats.MaxHealth,
            _currentSession.CurrentRoom?.Name ?? "Unknown",
            _currentSession.Player.Position.X,
            _currentSession.Player.Position.Y,
            _currentSession.Dungeon.RoomCount);

        return _currentSession.ToDto();
    }

    /// <summary>
    /// Loads an existing game session from storage.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to load.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The loaded game state as a DTO, or null if the session was not found.</returns>
    public async Task<GameStateDto?> LoadGameAsync(Guid sessionId, CancellationToken ct = default)
    {
        _logger.LogInformation("Loading game session: {SessionId}", sessionId);

        _currentSession = await _repository.GetByIdAsync(sessionId, ct);

        if (_currentSession == null)
        {
            _logger.LogWarning("Game session not found: {SessionId}", sessionId);
            return null;
        }

        _currentSession.UpdateLastPlayed();
        await _repository.SaveAsync(_currentSession, ct);

        _logger.LogInformation("Game session loaded successfully: {SessionId}", sessionId);
        _logger.LogDebug(
            "Loaded game state - Player: {PlayerName}, Health: {Health}/{MaxHealth}, " +
            "CurrentRoom: {RoomName}, Position: ({X},{Y}), InventoryCount: {InventoryCount}/{InventoryCapacity}",
            _currentSession.Player.Name,
            _currentSession.Player.Health,
            _currentSession.Player.Stats.MaxHealth,
            _currentSession.CurrentRoom?.Name ?? "Unknown",
            _currentSession.Player.Position.X,
            _currentSession.Player.Position.Y,
            _currentSession.Player.Inventory.Count,
            _currentSession.Player.Inventory.Capacity);

        return _currentSession.ToDto();
    }

    /// <summary>
    /// Retrieves summaries of all saved game sessions.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A read-only list of saved game summaries.</returns>
    public async Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving saved games list");
        var savedGames = await _repository.GetSavedGamesAsync(ct);
        _logger.LogDebug("Found {SavedGameCount} saved games", savedGames.Count);
        return savedGames;
    }

    /// <summary>
    /// Saves the current game session to storage.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <remarks>
    /// If there is no active session, this method logs a warning and returns without action.
    /// </remarks>
    public async Task SaveCurrentGameAsync(CancellationToken ct = default)
    {
        if (_currentSession == null)
        {
            _logger.LogWarning("No active session to save");
            return;
        }

        _currentSession.UpdateLastPlayed();
        await _repository.SaveAsync(_currentSession, ct);
        _logger.LogInformation("Game saved: {SessionId}", _currentSession.Id);
        _logger.LogDebug(
            "Save state - Player: {PlayerName}, Health: {Health}/{MaxHealth}, " +
            "Room: {RoomName}, InventoryCount: {InventoryCount}",
            _currentSession.Player.Name,
            _currentSession.Player.Health,
            _currentSession.Player.Stats.MaxHealth,
            _currentSession.CurrentRoom?.Name ?? "Unknown",
            _currentSession.Player.Inventory.Count);
    }

    /// <summary>
    /// Attempts to move the player in the specified direction.
    /// </summary>
    /// <param name="direction">The cardinal direction to move.</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    /// <remarks>
    /// Returns failure if there is no active session, the current room cannot be found,
    /// or there is no exit in the specified direction.
    /// </remarks>
    public (bool Success, string Message) TryMove(Direction direction)
    {
        _logger.LogDebug("TryMove called with direction: {Direction}", direction);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryMove failed: No active game session");
            return (false, "No active game session.");
        }

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
        {
            _logger.LogError("TryMove failed: Current room is null for session {SessionId}", _currentSession.Id);
            return (false, "Error: Current room not found.");
        }

        var fromRoom = currentRoom.Name;
        var fromPosition = _currentSession.Player.Position;

        if (!currentRoom.HasExit(direction))
        {
            _logger.LogDebug(
                "Move blocked: No exit {Direction} from room {RoomName}. Available exits: {Exits}",
                direction,
                fromRoom,
                string.Join(", ", currentRoom.Exits.Keys));
            return (false, $"You cannot go {direction.ToString().ToLower()}. There is no exit in that direction.");
        }

        if (_currentSession.TryMovePlayer(direction))
        {
            var newRoom = _currentSession.CurrentRoom;
            var toPosition = _currentSession.Player.Position;
            _logger.LogInformation(
                "Player moved {Direction}: {FromRoom} ({FromX},{FromY}) -> {ToRoom} ({ToX},{ToY})",
                direction,
                fromRoom,
                fromPosition.X,
                fromPosition.Y,
                newRoom?.Name ?? "Unknown",
                toPosition.X,
                toPosition.Y);

            if (newRoom?.HasMonsters == true)
            {
                var monsters = newRoom.GetAliveMonsters().ToList();
                _logger.LogDebug(
                    "Room {RoomName} contains {MonsterCount} monster(s): {MonsterNames}",
                    newRoom.Name,
                    monsters.Count,
                    string.Join(", ", monsters.Select(m => $"{m.Name} (HP:{m.Health})")));
            }

            if (newRoom?.HasItems == true)
            {
                _logger.LogDebug(
                    "Room {RoomName} contains {ItemCount} item(s): {ItemNames}",
                    newRoom.Name,
                    newRoom.Items.Count(),
                    string.Join(", ", newRoom.Items.Select(i => i.Name)));
            }

            return (true, $"You head {direction.ToString().ToLower()}.");
        }

        _logger.LogWarning("TryMove failed unexpectedly for direction {Direction}", direction);
        return (false, "You cannot move in that direction.");
    }

    /// <summary>
    /// Attempts to pick up an item from the current room.
    /// </summary>
    /// <param name="itemName">The name of the item to pick up (case-insensitive).</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    /// <remarks>
    /// Returns failure if there is no active session, the item is not found in the room,
    /// or the player's inventory is full.
    /// </remarks>
    public (bool Success, string Message) TryPickUpItem(string itemName)
    {
        _logger.LogDebug("TryPickUpItem called for item: {ItemName}", itemName);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryPickUpItem failed: No active game session");
            return (false, "No active game session.");
        }

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
        {
            _logger.LogError("TryPickUpItem failed: Current room is null for session {SessionId}", _currentSession.Id);
            return (false, "Error: Current room not found.");
        }

        var item = currentRoom.GetItemByName(itemName);
        if (item == null)
        {
            _logger.LogDebug(
                "Item not found in room: {ItemName}. Available items: {AvailableItems}",
                itemName,
                currentRoom.HasItems ? string.Join(", ", currentRoom.Items.Select(i => i.Name)) : "none");
            return (false, $"There is no '{itemName}' here.");
        }

        var inventoryBefore = _currentSession.Player.Inventory.Count;
        if (_currentSession.Player.Inventory.IsFull)
        {
            _logger.LogWarning(
                "Inventory full: Cannot pick up {ItemName}. Inventory: {Count}/{Capacity}",
                itemName,
                inventoryBefore,
                _currentSession.Player.Inventory.Capacity);
            return (false, "Your inventory is full!");
        }

        if (_currentSession.TryPickUpItem(itemName))
        {
            _logger.LogInformation(
                "Item picked up: {ItemName} (Type: {ItemType}, Value: {ItemValue}). Inventory: {Before} -> {After}/{Capacity}",
                item.Name,
                item.Type,
                item.Value,
                inventoryBefore,
                _currentSession.Player.Inventory.Count,
                _currentSession.Player.Inventory.Capacity);
            return (true, $"You picked up the {item.Name}.");
        }

        _logger.LogWarning("TryPickUpItem failed unexpectedly for item: {ItemName}", itemName);
        return (false, $"Could not pick up '{itemName}'.");
    }

    /// <summary>
    /// Attempts to attack a monster in the current room.
    /// </summary>
    /// <returns>A tuple indicating success and a combat description message.</returns>
    /// <remarks>
    /// Attacks the first alive monster in the room. If the player is defeated,
    /// the game state is set to GameOver. Returns failure if there is no active
    /// session or no monsters are present.
    /// </remarks>
    public (bool Success, string Message) TryAttack()
    {
        _logger.LogDebug("TryAttack called");

        if (_currentSession == null)
        {
            _logger.LogWarning("TryAttack failed: No active game session");
            return (false, "No active game session.");
        }

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
        {
            _logger.LogError("TryAttack failed: Current room is null for session {SessionId}", _currentSession.Id);
            return (false, "Error: Current room not found.");
        }

        var monster = currentRoom.GetAliveMonsters().FirstOrDefault();
        if (monster == null)
        {
            _logger.LogDebug("No monsters to attack in room: {RoomName}", currentRoom.Name);
            return (false, "There is nothing to attack here.");
        }

        var playerHealthBefore = _currentSession.Player.Health;
        var monsterHealthBefore = monster.Health;

        _logger.LogDebug(
            "Combat initiated - Player: {PlayerName} (HP:{PlayerHealth}, ATK:{PlayerAtk}, DEF:{PlayerDef}) vs " +
            "Monster: {MonsterName} (HP:{MonsterHealth}, ATK:{MonsterAtk}, DEF:{MonsterDef})",
            _currentSession.Player.Name,
            playerHealthBefore,
            _currentSession.Player.Stats.Attack,
            _currentSession.Player.Stats.Defense,
            monster.Name,
            monsterHealthBefore,
            monster.Stats.Attack,
            monster.Stats.Defense);

        var result = _combatService.ResolveCombatRound(_currentSession.Player, monster);
        var description = _combatService.GetCombatDescription(
            result,
            _currentSession.Player.Name,
            monster.Name
        );

        _logger.LogInformation(
            "Combat result - Damage dealt: {DamageDealt}, Damage received: {DamageReceived}, " +
            "Player HP: {PlayerBefore} -> {PlayerAfter}, Monster HP: {MonsterBefore} -> {MonsterAfter}",
            result.DamageDealt,
            result.DamageReceived,
            playerHealthBefore,
            _currentSession.Player.Health,
            monsterHealthBefore,
            monster.Health);

        if (result.MonsterDefeated)
        {
            _logger.LogInformation("Monster defeated: {MonsterName}", monster.Name);
        }

        if (result.PlayerDefeated)
        {
            _logger.LogWarning(
                "Player defeated! Setting game state to GameOver. Session: {SessionId}, Player: {PlayerName}",
                _currentSession.Id,
                _currentSession.Player.Name);
            _currentSession.SetState(GameState.GameOver);
        }

        return (true, description);
    }

    /// <summary>
    /// Attempts to drop an item from the player's inventory into the current room.
    /// </summary>
    /// <param name="itemName">The name of the item to drop (case-insensitive).</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    public (bool Success, string Message) TryDropItem(string itemName)
    {
        _logger.LogDebug("TryDropItem called for item: {ItemName}", itemName);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryDropItem failed: No active game session");
            return (false, "No active game session.");
        }

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
        {
            _logger.LogError("TryDropItem failed: Current room is null");
            return (false, "Error: Current room not found.");
        }

        var item = _currentSession.Player.Inventory.GetByName(itemName);
        if (item == null)
        {
            _logger.LogDebug("Item not found in inventory: {ItemName}", itemName);
            return (false, $"You don't have '{itemName}' in your inventory.");
        }

        if (_currentSession.Player.Inventory.TryRemove(item))
        {
            currentRoom.AddItem(item);
            _logger.LogInformation(
                "Item dropped: {ItemName} in room {RoomName}. Inventory: {InventoryCount}/{Capacity}",
                item.Name,
                currentRoom.Name,
                _currentSession.Player.Inventory.Count,
                _currentSession.Player.Inventory.Capacity);
            return (true, $"You drop the {item.Name}.");
        }

        _logger.LogWarning("TryDropItem failed unexpectedly for item: {ItemName}", itemName);
        return (false, $"Could not drop '{itemName}'.");
    }

    /// <summary>
    /// Attempts to use an item from the player's inventory.
    /// </summary>
    /// <param name="itemName">The name of the item to use (case-insensitive).</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    public (bool Success, string Message) TryUseItem(string itemName)
    {
        _logger.LogDebug("TryUseItem called for item: {ItemName}", itemName);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryUseItem failed: No active game session");
            return (false, "No active game session.");
        }

        var item = _currentSession.Player.Inventory.GetByName(itemName);
        if (item == null)
        {
            _logger.LogDebug("Item not found in inventory: {ItemName}", itemName);
            return (false, $"You don't have '{itemName}' in your inventory.");
        }

        if (item.Type != ItemType.Consumable)
        {
            _logger.LogDebug("Item not usable: {ItemName} (Type: {ItemType})", itemName, item.Type);
            return (false, $"You cannot use the {item.Name}.");
        }

        var effectResult = _itemEffectService.ApplyEffect(item, _currentSession.Player);

        if (effectResult.Success)
        {
            _currentSession.Player.Inventory.TryRemove(item);
            _logger.LogInformation(
                "Item used: {ItemName}, Effect: {Effect}, Value: {Value}, Player HP: {Health}/{MaxHealth}",
                item.Name,
                item.Effect,
                item.EffectValue,
                _currentSession.Player.Health,
                _currentSession.Player.Stats.MaxHealth);
        }

        return effectResult;
    }

    /// <summary>
    /// Gets detailed examination information about a target.
    /// </summary>
    /// <param name="target">The name of the target to examine.</param>
    /// <returns>Examination result DTO or null if target not found.</returns>
    public ExamineResultDto? GetExamineInfo(string target)
    {
        _logger.LogDebug("GetExamineInfo called for target: {Target}", target);

        if (_currentSession == null)
        {
            _logger.LogWarning("GetExamineInfo failed: No active game session");
            return null;
        }

        var room = _currentSession.CurrentRoom;
        if (room == null) return null;

        // Check for "room" keyword
        if (target.Equals("room", StringComparison.OrdinalIgnoreCase))
        {
            return CreateRoomExamineResult(room);
        }

        // Check room items
        var roomItem = room.GetItemByName(target);
        if (roomItem != null)
        {
            return CreateItemExamineResult(roomItem, "on the ground");
        }

        // Check inventory items
        var invItem = _currentSession.Player.Inventory.GetByName(target);
        if (invItem != null)
        {
            return CreateItemExamineResult(invItem, "in your inventory");
        }

        // Check monsters
        var monster = room.GetAliveMonsters().FirstOrDefault(m =>
            m.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
        if (monster != null)
        {
            return CreateMonsterExamineResult(monster);
        }

        _logger.LogDebug("Examine target not found: {Target}", target);
        return null;
    }

    private ExamineResultDto CreateItemExamineResult(Item item, string location)
    {
        var properties = new Dictionary<string, string>
        {
            ["Location"] = location,
            ["Value"] = item.Value.ToString()
        };

        if (item.Type == ItemType.Consumable && item.Effect != ItemEffect.None)
        {
            properties["Effect"] = $"{item.Effect} ({item.EffectValue})";
        }

        return new ExamineResultDto(item.Name, item.Type.ToString(), item.Description, properties);
    }

    private ExamineResultDto CreateMonsterExamineResult(Monster monster)
    {
        var healthPercent = (double)monster.Health / monster.Stats.MaxHealth;
        var condition = healthPercent switch
        {
            > 0.75 => "healthy",
            > 0.5 => "wounded",
            > 0.25 => "badly wounded",
            _ => "near death"
        };

        var properties = new Dictionary<string, string>
        {
            ["Condition"] = condition,
            ["Health"] = $"{monster.Health}/{monster.Stats.MaxHealth}"
        };

        return new ExamineResultDto(monster.Name, "Monster", monster.Description, properties);
    }

    private ExamineResultDto CreateRoomExamineResult(Room room)
    {
        var properties = new Dictionary<string, string>
        {
            ["Items"] = room.HasItems ? string.Join(", ", room.Items.Select(i => i.Name)) : "None",
            ["Monsters"] = room.HasMonsters ? string.Join(", ", room.GetAliveMonsters().Select(m => m.Name)) : "None",
            ["Exits"] = string.Join(", ", room.Exits.Keys.Select(d => d.ToString()))
        };

        return new ExamineResultDto(room.Name, "Room", room.Description, properties);
    }

    /// <summary>
    /// Gets comprehensive player statistics.
    /// </summary>
    /// <returns>Player stats DTO, or null if no active session.</returns>
    public PlayerStatsDto? GetPlayerStats()
    {
        if (_currentSession == null)
        {
            _logger.LogWarning("GetPlayerStats failed: No active game session");
            return null;
        }

        var player = _currentSession.Player;
        var room = _currentSession.CurrentRoom;

        _logger.LogDebug(
            "GetPlayerStats - Player: {Name}, HP: {Health}/{MaxHealth}, ATK: {Attack}, DEF: {Defense}",
            player.Name, player.Health, player.Stats.MaxHealth, player.Stats.Attack, player.Stats.Defense);

        return new PlayerStatsDto(
            player.Name,
            player.Health,
            player.Stats.MaxHealth,
            player.Stats.Attack,
            player.Stats.Defense,
            player.Position.X,
            player.Position.Y,
            room?.Name ?? "Unknown",
            player.Inventory.Count,
            player.Inventory.Capacity
        );
    }

    /// <summary>
    /// Gets the current room as a DTO.
    /// </summary>
    /// <returns>The current room DTO, or null if no session is active.</returns>
    public RoomDto? GetCurrentRoom()
    {
        var room = _currentSession?.CurrentRoom;
        if (room == null)
        {
            _logger.LogDebug("GetCurrentRoom called, returning: null");
            return null;
        }

        var isFirstVisit = !_currentSession!.HasVisitedRoom(room.Id);
        var dto = room.ToDto(isFirstVisit);
        _logger.LogDebug("GetCurrentRoom called, returning: {RoomName}, IsFirstVisit: {IsFirstVisit}", dto.Name, isFirstVisit);
        return dto;
    }

    /// <summary>
    /// Gets the player's inventory as a DTO.
    /// </summary>
    /// <returns>The inventory DTO, or null if no session is active.</returns>
    public InventoryDto? GetInventory()
    {
        var inventory = _currentSession?.Player.Inventory.ToDto();
        _logger.LogDebug(
            "GetInventory called, returning: {ItemCount}/{Capacity} items",
            inventory?.Count ?? 0,
            inventory?.Capacity ?? 0);
        return inventory;
    }

    /// <summary>
    /// Ends the current game session without saving.
    /// </summary>
    /// <remarks>
    /// After calling this method, <see cref="HasActiveSession"/> will return false
    /// and <see cref="CurrentState"/> will return null.
    /// </remarks>
    public void EndSession()
    {
        var sessionId = _currentSession?.Id;
        var playerName = _currentSession?.Player.Name;
        _currentSession = null;
        _logger.LogInformation("Game session ended: {SessionId}, Player: {PlayerName}", sessionId, playerName);
    }
}
