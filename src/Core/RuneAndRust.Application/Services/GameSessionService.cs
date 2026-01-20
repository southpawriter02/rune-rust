using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

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
    private readonly IExaminationService? _examinationService;
    private readonly RuneAndRust.Domain.Services.SkillCheckService _skillCheckService;
    private readonly IDungeonGenerator? _dungeonGenerator;

    // Optional service dependencies - intended to be null unless explicitly provided via DI
#pragma warning disable CS0649 // Fields are intentionally unassigned - they're optional DI dependencies
    /// <summary>
    /// Service for applying item effects.
    /// </summary>
    private readonly ItemEffectService? _itemEffectService;

    /// <summary>
    /// Service for managing abilities.
    /// </summary>
    private readonly AbilityService? _abilityService;

    /// <summary>
    /// Service for managing resources.
    /// </summary>
    private readonly ResourceService? _resourceService;

    /// <summary>
    /// Dice service for combat rolls.
    /// </summary>
    private readonly IDiceService? _diceService;

    /// <summary>
    /// Service for managing equipment operations.
    /// </summary>
    private readonly EquipmentService? _equipmentService;

    /// <summary>
    /// Service for managing experience points (v0.0.8a).
    /// </summary>
    private readonly ExperienceService? _experienceService;

    /// <summary>
    /// Service for managing level-up progression (v0.0.8b).
    /// </summary>
    private readonly ProgressionService? _progressionService;

    /// <summary>
    /// Service for generating and collecting loot (v0.0.9d).
    /// </summary>
    private readonly ILootService? _lootService;

    /// <summary>
    /// Event logger for comprehensive game event tracking.
    /// </summary>
    private readonly IGameEventLogger? _eventLogger;
#pragma warning restore CS0649

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
    /// Gets the current player entity, or null if no session is active.
    /// </summary>
    /// <remarks>
    /// Use this for services that need the actual domain entity rather than a DTO.
    /// Added in v0.12.0c for StatisticsService integration.
    /// </remarks>
    public Player? GetCurrentPlayer() => _currentSession?.Player;

    /// <summary>
    /// Creates a new instance of the GameSessionService.
    /// </summary>
    /// <param name="repository">The repository for persisting game sessions.</param>
    /// <param name="logger">The logger for service diagnostics.</param>
    /// <param name="examinationService">Optional examination service for advanced object inspection.</param>
    /// <param name="dungeonGenerator">Optional dungeon generator for procedural level creation.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public GameSessionService(
        IGameRepository repository,
        ILogger<GameSessionService> logger,
        IExaminationService? examinationService = null,
        IDungeonGenerator? dungeonGenerator = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _combatService = new CombatService();
        _examinationService = examinationService;
        _skillCheckService = new RuneAndRust.Domain.Services.SkillCheckService();
        _dungeonGenerator = dungeonGenerator;
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

        _eventLogger?.SetSession(_currentSession.Id, _currentSession.Player.Id);
        _eventLogger?.LogSession("SessionStarted", $"New game started for {playerName}");
        _eventLogger?.LogExploration("RoomEntered", $"Started in {_currentSession.CurrentRoom?.Name}", _currentSession.CurrentRoom?.Id);

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
    /// Starts a new game with a procedurally generated dungeon.
    /// </summary>
    /// <param name="playerName">The player's name.</param>
    /// <param name="biome">The biome for the dungeon.</param>
    /// <param name="difficulty">The difficulty tier.</param>
    /// <param name="roomCount">Target number of rooms (default 15).</param>
    /// <param name="seed">Optional seed for reproducible generation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The initial game state.</returns>
    public async Task<GameStateDto> StartGeneratedGameAsync(
        string playerName,
        Biome biome,
        DifficultyTier difficulty,
        int roomCount = 15,
        int? seed = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Starting generated game for player: {PlayerName}, Biome: {Biome}, Difficulty: {Difficulty}",
            playerName, biome, difficulty);

        if (_dungeonGenerator == null)
            throw new InvalidOperationException("Dungeon generator is required for generated games. Inject IDungeonGenerator into GameSessionService.");

        var generator = _dungeonGenerator;

        var dungeonName = biome switch
        {
            Biome.Citadel => "The Fallen Citadel",
            Biome.TheRoots => "The Tangled Roots",
            Biome.Muspelheim => "Muspelheim Depths",
            Biome.Niflheim => "Niflheim's Frozen Halls",
            Biome.Jotunheim => "Jotunheim Ruins",
            _ => "The Unknown Depths"
        };

        var dungeon = await generator.GenerateDungeonAsync(
            dungeonName, biome, difficulty, roomCount, seed, ct);

        _currentSession = GameSession.CreateWithDungeon(playerName, dungeon);
        await _repository.SaveAsync(_currentSession, ct);

        _logger.LogInformation(
            "Generated game session created: {SessionId}, Rooms: {RoomCount}",
            _currentSession.Id, dungeon.RoomCount);

        return _currentSession.ToDto();
    }

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

            _eventLogger?.LogExploration("Moved", $"Moved {direction} to {newRoom?.Name}", newRoom?.Id,
                data: new Dictionary<string, object>
                {
                    ["direction"] = direction.ToString(),
                    ["fromRoom"] = fromRoom,
                    ["toRoom"] = newRoom?.Name ?? "Unknown"
                });

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
    /// <returns>A tuple indicating success, a combat description message, optional XP gain info, optional level-up info, and optional loot info.</returns>
    /// <remarks>
    /// Attacks the first alive monster in the room using dice-based combat:
    /// Attack roll (1d10 + Finesse) vs Defense, damage roll (1d6 + Might) - armor.
    /// If the player is defeated, the game state is set to GameOver.
    /// If the monster is defeated, experience points are awarded to the player and loot is generated.
    /// If the player gains enough XP to level up, level-up is applied automatically.
    /// </remarks>
    public (bool Success, string Message, ExperienceGainDto? ExperienceGain, LevelUpDto? LevelUp, LootDropDto? LootDrop) TryAttack()
    {
        _logger.LogDebug("TryAttack called");

        if (_currentSession == null)
        {
            _logger.LogWarning("TryAttack failed: No active game session");
            return (false, "No active game session.", null, null, null);
        }

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
        {
            _logger.LogError("TryAttack failed: Current room is null for session {SessionId}", _currentSession.Id);
            return (false, "Error: Current room not found.", null, null, null);
        }

        var monster = currentRoom.GetAliveMonsters().FirstOrDefault();
        if (monster == null)
        {
            _logger.LogDebug("No monsters to attack in room: {RoomName}", currentRoom.Name);
            return (false, "There is nothing to attack here.", null, null, null);
        }

        var playerHealthBefore = _currentSession.Player.Health;
        var monsterHealthBefore = monster.Health;

        _logger.LogDebug(
            "Combat initiated - Player: {PlayerName} (HP:{PlayerHealth}, Finesse:{Finesse}, Might:{Might}) vs " +
            "Monster: {MonsterName} (HP:{MonsterHealth}, ATK:{MonsterAtk}, DEF:{MonsterDef})",
            _currentSession.Player.Name,
            playerHealthBefore,
            _currentSession.Player.Attributes.Finesse,
            _currentSession.Player.Attributes.Might,
            monster.Name,
            monsterHealthBefore,
            monster.Stats.Attack,
            monster.Stats.Defense);

        // Use dice-based combat
        var result = _combatService.ResolveCombatRound(_currentSession.Player, monster, _diceService!);
        var description = _combatService.GetCombatDescription(
            result,
            _currentSession.Player.Name,
            monster.Name
        );

        _logger.LogInformation(
            "Combat result - Attack: [{Roll}]+{Mod}={Total} ({AttackResult}), Damage dealt: {DamageDealt}, Received: {DamageReceived}, " +
            "Player HP: {PlayerBefore} -> {PlayerAfter}, Monster HP: {MonsterBefore} -> {MonsterAfter}",
            result.AttackRoll.Rolls[0],
            result.AttackTotal - result.AttackRoll.Total,
            result.AttackTotal,
            result.IsCriticalHit ? "CRIT" : result.IsCriticalMiss ? "MISS" : result.IsHit ? "Hit" : "Miss",
            result.DamageDealt,
            result.DamageReceived,
            playerHealthBefore,
            _currentSession.Player.Health,
            monsterHealthBefore,
            monster.Health);

        ExperienceGainDto? experienceGainDto = null;
        LevelUpDto? levelUpDto = null;
        LootDropDto? lootDropDto = null;

        if (result.MonsterDefeated)
        {
            _logger.LogInformation("Monster defeated: {MonsterName}", monster.Name);

            // Award experience points (v0.0.8a)
            var xpResult = _experienceService!.AwardExperienceFromMonster(_currentSession.Player, monster);
            if (xpResult.DidGainExperience)
            {
                // Get terminology from progression config (v0.0.8c)
                var progression = _progressionService!.Progression;

                experienceGainDto = ExperienceGainDto.FromResult(
                    xpResult,
                    _currentSession.Player.Level,
                    _currentSession.Player.GetExperienceToNextLevel(progression),
                    _currentSession.Player.ExperienceProgressPercent,
                    progression.ExperienceTerminology,
                    progression.LevelTerminology);

                // Check for level-up after XP gain (v0.0.8b)
                levelUpDto = CheckAndHandleLevelUp();
            }

            // Generate and drop loot (v0.0.9d)
            var loot = _lootService!.GenerateLoot(monster);
            if (!loot.IsEmpty)
            {
                currentRoom.AddLoot(loot);
                lootDropDto = LootDropDto.FromDomain(loot);
                _logger.LogInformation("Loot generated for {MonsterName}: {ItemCount} items, {CurrencyTypes} currency types",
                    monster.Name,
                    loot.Items?.Count ?? 0,
                    loot.Currency?.Count ?? 0);
            }
        }

        if (result.PlayerDefeated)
        {
            _logger.LogWarning(
                "Player defeated! Setting game state to GameOver. Session: {SessionId}, Player: {PlayerName}",
                _currentSession.Id,
                _currentSession.Player.Name);
            _currentSession.SetState(GameState.GameOver);
        }

        return (true, description, experienceGainDto, levelUpDto, lootDropDto);
    }

    /// <summary>
    /// Checks if the player has enough XP to level up and applies it if so.
    /// </summary>
    /// <returns>A LevelUpDto if leveling occurred, null otherwise.</returns>
    private LevelUpDto? CheckAndHandleLevelUp()
    {
        if (_currentSession == null) return null;

        var player = _currentSession.Player;
        var oldStats = player.Stats;
        var progression = _progressionService!.Progression;

        // Get unlocked abilities callback using ability service
        IReadOnlyList<string> GetAbilitiesAtLevel(int level)
        {
            if (string.IsNullOrEmpty(player.ClassId)) return [];

            var abilities = _abilityService!.GetUnlockedAbilitiesAtLevel(player.ClassId, level);
            return abilities.Select(a => a.Name).ToList();
        }

        var levelUpResult = _progressionService!.CheckAndApplyLevelUp(player, null, GetAbilitiesAtLevel);

        if (!levelUpResult.DidLevelUp)
        {
            return null;
        }

        // Get ability names for display
        var abilityNames = levelUpResult.NewAbilities.ToList();

        // Get custom rewards and title for the new level (v0.0.8c)
        var customRewards = _progressionService.GetCustomRewardsForLevel(levelUpResult.NewLevel);
        var title = _progressionService.GetTitleForLevel(levelUpResult.NewLevel);

        // Create DTO for display with terminology (v0.0.8c)
        var levelUpDto = LevelUpDto.FromResult(
            levelUpResult,
            oldStats,
            player.Stats,
            abilityNames,
            player.GetExperienceToNextLevel(progression),
            customRewards,
            title,
            progression.ExperienceTerminology,
            progression.LevelTerminology);

        _logger.LogInformation(
            "Player {Name} leveled up: {OldLevel} -> {NewLevel}. Stats: HP {OldHP}->{NewHP}, ATK {OldATK}->{NewATK}, DEF {OldDEF}->{NewDEF}",
            player.Name,
            levelUpResult.OldLevel,
            levelUpResult.NewLevel,
            oldStats.MaxHealth,
            player.Stats.MaxHealth,
            oldStats.Attack,
            player.Stats.Attack,
            oldStats.Defense,
            player.Stats.Defense);

        return levelUpDto;
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

        var effectResult = _itemEffectService!.ApplyEffect(item, _currentSession.Player);

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

    /// <summary>
    /// Gets the player's abilities as DTOs.
    /// </summary>
    /// <returns>List of player abilities, or empty list if no active session.</returns>
    public IReadOnlyList<PlayerAbilityDto> GetPlayerAbilities()
    {
        if (_currentSession == null)
        {
            _logger.LogWarning("GetPlayerAbilities failed: No active game session");
            return [];
        }

        return _abilityService!.GetPlayerAbilities(_currentSession.Player);
    }

    /// <summary>
    /// Attempts to use an ability.
    /// </summary>
    /// <param name="abilityName">The name or ID of the ability to use.</param>
    /// <returns>A tuple indicating success and a descriptive message.</returns>
    public (bool Success, string Message) TryUseAbility(string abilityName)
    {
        _logger.LogDebug("TryUseAbility called for ability: {AbilityName}", abilityName);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryUseAbility failed: No active game session");
            return (false, "No active game session.");
        }

        // Find the ability by name
        var definition = _abilityService!.FindAbilityByName(_currentSession.Player, abilityName);
        if (definition == null)
        {
            _logger.LogDebug("Ability not found: {AbilityName}", abilityName);
            return (false, $"Unknown ability: {abilityName}");
        }

        // Get target monster if ability targets enemies
        Monster? target = null;
        if (definition.TargetType == AbilityTargetType.SingleEnemy ||
            definition.TargetType == AbilityTargetType.AllEnemies)
        {
            var room = _currentSession.CurrentRoom;
            target = room?.GetAliveMonsters().FirstOrDefault();

            if (target == null && definition.TargetType == AbilityTargetType.SingleEnemy)
            {
                _logger.LogDebug("No target available for ability: {AbilityName}", abilityName);
                return (false, "There is no target to use this ability on.");
            }
        }

        // Use the ability
        var result = _abilityService.UseAbility(_currentSession.Player, definition.Id, target);

        if (!result.Success)
        {
            _logger.LogDebug("Ability use failed: {Message}", result.Message);
            return (false, result.Message);
        }

        _logger.LogInformation(
            "Ability used: {PlayerName} used {AbilityName}. Result: {Message}",
            _currentSession.Player.Name, definition.Name, result.Message);

        var messageBuilder = new System.Text.StringBuilder(result.Message);

        // Check if monster was defeated
        if (target != null && !target.IsAlive)
        {
            _logger.LogInformation("Monster defeated by ability: {MonsterName}", target.Name);
            messageBuilder.AppendLine().Append($"{target.Name} has been defeated!");
        }
        // Monster counterattack if still alive after an offensive ability
        else if (target != null && target.IsAlive)
        {
            // Use full dice-based combat round for monster counterattack
            var combatResult = _combatService.ResolveCombatRound(_currentSession.Player, target, _diceService!);

            // For ability counterattack, we only care about monster's counterattack portion
            if (combatResult.MonsterCounterAttack != null)
            {
                var counter = combatResult.MonsterCounterAttack;
                string counterDescription;

                if (counter.IsCriticalHit)
                {
                    counterDescription = $"The {target.Name} lands a CRITICAL HIT for {counter.DamageDealt} damage!";
                }
                else if (counter.IsCriticalMiss)
                {
                    counterDescription = $"The {target.Name} stumbles and misses!";
                }
                else if (counter.IsHit)
                {
                    counterDescription = $"The {target.Name} strikes back for {counter.DamageDealt} damage!";
                }
                else
                {
                    counterDescription = $"The {target.Name} attacks but {_currentSession.Player.Name} dodges!";
                }

                _logger.LogInformation(
                    "Monster counterattack: {MonsterName} [{Roll}]+{Mod}={Total} -> {Damage} damage to {PlayerName}",
                    target.Name, counter.AttackRoll.Rolls[0],
                    counter.AttackTotal - counter.AttackRoll.Total, counter.AttackTotal,
                    counter.DamageDealt, _currentSession.Player.Name);

                messageBuilder.AppendLine().Append(counterDescription);
            }

            if (_currentSession.Player.Health <= 0)
            {
                _logger.LogWarning("Player defeated by monster counterattack!");
                _currentSession.SetState(GameState.GameOver);
            }
        }

        return (true, messageBuilder.ToString());
    }

    /// <summary>
    /// Checks whether the player is currently in combat (monsters present in current room).
    /// </summary>
    /// <returns>True if there are alive monsters in the current room; otherwise, false.</returns>
    public bool IsInCombat()
    {
        if (_currentSession == null) return false;

        var room = _currentSession.CurrentRoom;
        return room?.GetAliveMonsters().Any() ?? false;
    }

    /// <summary>
    /// Processes end-of-turn effects including resource regeneration/decay and cooldown reduction.
    /// </summary>
    /// <returns>A structured result containing all turn-end changes.</returns>
    public TurnEndResult ProcessTurnEnd()
    {
        if (_currentSession == null)
        {
            _logger.LogWarning("ProcessTurnEnd called with no active session");
            return TurnEndResult.Empty(0);
        }

        var player = _currentSession.Player;
        var inCombat = IsInCombat();

        // Advance turn counter
        _currentSession.AdvanceTurn();
        var newTurnCount = _currentSession.TurnCount;
        _logger.LogDebug("Turn advanced to {TurnCount}", newTurnCount);

        // Process resource regeneration/decay
        var resourceResult = _resourceService!.ProcessTurnEnd(player, inCombat);
        var resourceChanges = resourceResult.Changes
            .Select(c => MapResourceChange(c))
            .ToList();

        // Process ability cooldowns
        var cooldownChanges = _abilityService!.ProcessTurnEnd(player);
        var abilitiesNowReady = cooldownChanges
            .Where(c => c.IsNowReady)
            .Select(c => c.AbilityName)
            .ToList();

        _logger.LogInformation(
            "Turn {Turn} ended: {ResourceChanges} resource changes, {CooldownChanges} cooldown changes, {AbilitiesReady} abilities now ready",
            newTurnCount, resourceChanges.Count, cooldownChanges.Count, abilitiesNowReady.Count);

        return new TurnEndResult(newTurnCount, resourceChanges, cooldownChanges.ToList(), abilitiesNowReady);
    }

    private TurnResourceChangeDto MapResourceChange(ResourceChange change)
    {
        var resourceType = _resourceService!.GetResourceType(change.ResourceTypeId);
        return new TurnResourceChangeDto(
            resourceType?.DisplayName ?? change.ResourceTypeId,
            resourceType?.Abbreviation ?? "??",
            change.PreviousValue,
            change.NewValue,
            resourceType?.DefaultMax ?? 100,
            change.ChangeType.ToString(),
            resourceType?.Color ?? "#FFFFFF");
    }

    // ===== Equipment Methods (v0.0.7a) =====

    /// <summary>
    /// Attempts to equip an item from the player's inventory.
    /// </summary>
    /// <param name="itemName">The name of the item to equip.</param>
    /// <returns>The result of the equip operation as a DTO.</returns>
    public EquipResultDto TryEquipItem(string itemName)
    {
        _logger.LogDebug("TryEquipItem called for item: {ItemName}", itemName);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryEquipItem failed: No active game session");
            return new EquipResultDto(false, "No active game session.");
        }

        var result = _equipmentService!.TryEquipByName(_currentSession.Player, itemName);
        return EquipResultDto.FromResult(result);
    }

    /// <summary>
    /// Attempts to unequip an item from the specified equipment slot.
    /// </summary>
    /// <param name="slot">The equipment slot to unequip from.</param>
    /// <returns>The result of the unequip operation as a DTO.</returns>
    public EquipResultDto TryUnequipItem(EquipmentSlot slot)
    {
        _logger.LogDebug("TryUnequipItem called for slot: {Slot}", slot);

        if (_currentSession == null)
        {
            _logger.LogWarning("TryUnequipItem failed: No active game session");
            return new EquipResultDto(false, "No active game session.");
        }

        var result = _equipmentService!.TryUnequip(_currentSession.Player, slot);
        return EquipResultDto.FromResult(result);
    }

    /// <summary>
    /// Gets the player's current equipment.
    /// </summary>
    /// <returns>Equipment slots DTO, or null if no active session.</returns>
    public EquipmentSlotsDto? GetEquipment()
    {
        if (_currentSession == null)
        {
            _logger.LogWarning("GetEquipment failed: No active game session");
            return null;
        }

        return EquipmentSlotsDto.FromPlayer(_currentSession.Player);
    }

    public (bool Success, string? Description, string? ErrorMessage) TryLookAtTarget(string target)
    {
        if (_currentSession == null)
            return (false, null, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, null, "Error: Current room not found.");

        // Check for items in the room
        var item = currentRoom.GetItemByName(target);
        if (item != null)
        {
            var description = $"{item.Name.ToUpper()}\n{new string('─', 40)}\n{item.Description}";
            return (true, description, null);
        }

        // Check for monsters
        var monster = currentRoom.Monsters.FirstOrDefault(m =>
            m.Name.Equals(target, StringComparison.OrdinalIgnoreCase) && m.IsAlive);
        if (monster != null)
        {
            var hpBar = new string('█', (int)(monster.Health / (float)monster.MaxHealth * 20));
            var emptyBar = new string('░', 20 - hpBar.Length);
            var description = $"{monster.Name.ToUpper()}\n{new string('─', 40)}\n" +
                            $"HP: {hpBar}{emptyBar} {monster.Health}/{monster.MaxHealth}\n" +
                            $"\nA hostile creature that must be dealt with.";
            return (true, description, null);
        }

        // Check for exits
        var exitDirection = target.ToLowerInvariant() switch
        {
            "n" or "north" => Direction.North,
            "s" or "south" => Direction.South,
            "e" or "east" => Direction.East,
            "w" or "west" => Direction.West,
            "u" or "up" => Direction.Up,
            "d" or "down" => Direction.Down,
            "ne" or "northeast" => Direction.Northeast,
            "nw" or "northwest" => Direction.Northwest,
            "se" or "southeast" => Direction.Southeast,
            "sw" or "southwest" => Direction.Southwest,
            _ => (Direction?)null
        };

        if (exitDirection.HasValue && currentRoom.HasExit(exitDirection.Value))
        {
            var description = $"EXIT: {exitDirection.Value.ToString().ToUpper()}\n{new string('─', 40)}\n" +
                            $"A passage leading {exitDirection.Value.ToString().ToLower()}.";
            return (true, description, null);
        }

        return (false, null, $"Cannot find '{target}' here.");
    }

    public (bool Success, string Message) TrySearch(string? target)
    {
        if (_currentSession == null)
            return (false, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, "Error: Current room not found.");

        if (string.IsNullOrWhiteSpace(target))
        {
            // Search room for containers
            if (currentRoom.HasItems || currentRoom.Monsters.Any(m => !m.IsAlive))
            {
                var itemNames = currentRoom.Items.Select(i => i.Name);
                var corpseNames = currentRoom.Monsters.Where(m => !m.IsAlive).Select(m => $"{m.Name} (corpse)");
                var allSearchables = itemNames.Concat(corpseNames);
                var searchables = string.Join(", ", allSearchables);
                return (true, $"You can search: {searchables}");
            }
            return (false, "There is nothing to search here.");
        }

        // Search specific target (simplified for now)
        return (false, $"Search functionality for '{target}' is not yet fully implemented.");
    }

    public (bool Success, string Message) TryInvestigate(string target)
    {
        if (_currentSession == null)
            return (false, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, "Error: Current room not found.");

        // Simplified investigation for now
        return (false, $"Investigation functionality for '{target}' is not yet fully implemented. This feature requires WITS checks and secret discovery mechanics.");
    }

    /// <summary>
    /// Examines an object with a WITS check for layered detail.
    /// </summary>
    public async Task<(bool Success, ExaminationResultDto? Result, string? ErrorMessage)> TryExamineAsync(
        string target,
        CancellationToken ct = default)
    {
        if (_currentSession == null)
            return (false, null, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, null, "Error: Current room not found.");

        var witsValue = _currentSession.Player.Stats.Wits;
        var biome = currentRoom.Biome;

        // Check if examination service is available
        if (_examinationService == null)
        {
            // Fallback to basic examination without layered descriptions
            return await TryBasicExamineAsync(target, witsValue, currentRoom);
        }

        // Try to find the target as an item
        var item = currentRoom.GetItemByName(target);
        if (item != null)
        {
            var category = item.Type switch
            {
                ItemType.Weapon => ObjectCategory.Item,
                ItemType.Armor => ObjectCategory.Item,
                ItemType.Consumable => ObjectCategory.Container,
                ItemType.Quest => ObjectCategory.Decorative,
                ItemType.Misc => ObjectCategory.Item,
                _ => ObjectCategory.Item
            };

            var result = await _examinationService.ExamineObjectAsync(
                item.Id, item.Name, category, witsValue, biome, ct);

            // Track revealed solutions
            if (!string.IsNullOrWhiteSpace(result.RevealedSolutionId))
            {
                _currentSession.RevealSolution(result.RevealedSolutionId);
            }

            return (true, result, null);
        }

        // Try to find the target as a monster (alive or corpse)
        var monster = currentRoom.Monsters.FirstOrDefault(m =>
            m.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
        if (monster != null)
        {
            var category = monster.IsAlive ? ObjectCategory.Fauna : ObjectCategory.Corpse;
            var result = await _examinationService.ExamineObjectAsync(
                monster.Id, monster.Name, category, witsValue, biome, ct);

            return (true, result, null);
        }

        // Try to find the target as a revealed hidden element
        var hiddenElement = currentRoom.GetRevealedElements()
            .FirstOrDefault(h => h.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
        if (hiddenElement != null)
        {
            var category = hiddenElement.ElementType switch
            {
                HiddenElementType.Trap => ObjectCategory.Machinery,
                HiddenElementType.SecretDoor => ObjectCategory.Door,
                HiddenElementType.Cache => ObjectCategory.Container,
                _ => ObjectCategory.Decorative
            };

            var result = await _examinationService.ExamineObjectAsync(
                hiddenElement.Id, hiddenElement.Name, category, witsValue, biome, ct);

            return (true, result, null);
        }

        // Try to find the target as a room feature
        var feature = currentRoom.GetFeatureByName(target);
        if (feature != null)
        {
            var category = feature.FeatureType switch
            {
                RoomFeatureType.Interactable => ObjectCategory.Machinery,
                RoomFeatureType.Decoration => ObjectCategory.Decorative,
                RoomFeatureType.LightSource => ObjectCategory.Decorative,
                RoomFeatureType.Hazard => ObjectCategory.Machinery,
                _ => ObjectCategory.Decorative
            };

            var result = await _examinationService.ExamineObjectAsync(
                feature.Id, feature.FeatureId, category, witsValue, biome, ct);

            feature.MarkExamined();
            return (true, result, null);
        }

        // Check universal structural elements (walls, floor, ceiling)
        if (IsUniversalStructuralElement(target))
        {
            var result = await _examinationService.ExamineObjectAsync(
                Guid.NewGuid(), target.ToLower(), ObjectCategory.Structural, witsValue, biome, ct);

            return (true, result, null);
        }

        // Check if target is an exit direction
        var exitDirection = ParseDirection(target);
        if (exitDirection.HasValue && currentRoom.HasExit(exitDirection.Value))
        {
            var result = await _examinationService.ExamineObjectAsync(
                Guid.NewGuid(), $"{exitDirection.Value} exit", ObjectCategory.Door, witsValue, biome, ct);

            return (true, result, null);
        }

        return (false, null, $"Cannot find '{target}' to examine.");
    }

    /// <summary>
    /// Performs an active search of the room with WITS check.
    /// </summary>
    public async Task<(bool Success, SearchResultDto? Result, string? ErrorMessage)> TryActiveSearchAsync(
        CancellationToken ct = default)
    {
        if (_currentSession == null)
            return (false, null, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, null, "Error: Current room not found.");

        var witsValue = _currentSession.Player.Stats.Wits;

        if (_examinationService == null)
        {
            // Fallback: perform basic search with skill check
            var checkResult = _skillCheckService.PerformCheck(witsValue, 0);
            var revealed = currentRoom.PerformActiveSearch(checkResult.TotalResult);

            if (revealed.Count > 0)
            {
                var revealedDtos = revealed.Select(h => h.ToDto()).ToList();
                var narrative = string.Join("\n", revealed.Select(h => h.DiscoveryText));

                return (true, new SearchResultDto(
                    currentRoom.Id,
                    checkResult.RollResult,
                    checkResult.TotalResult,
                    revealedDtos,
                    narrative,
                    true
                ), null);
            }

            return (true, new SearchResultDto(
                currentRoom.Id,
                checkResult.RollResult,
                checkResult.TotalResult,
                [],
                "You search the area carefully but find nothing hidden.",
                false
            ), null);
        }

        var result = await _examinationService.PerformActiveSearchAsync(
            currentRoom.Id, witsValue, currentRoom.HiddenElements, ct);

        return (true, result, null);
    }

    /// <summary>
    /// Checks passive perception when entering a room.
    /// </summary>
    public async Task<PassivePerceptionResultDto?> CheckRoomPerceptionAsync(CancellationToken ct = default)
    {
        if (_currentSession == null)
            return null;

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return null;

        var passivePerception = _currentSession.Player.Stats.PassivePerception;

        if (_examinationService == null)
        {
            // Fallback: basic passive perception without service
            var revealed = currentRoom.CheckPassivePerception(passivePerception);
            if (revealed.Count == 0)
                return null;

            return new PassivePerceptionResultDto(
                currentRoom.Id,
                passivePerception,
                revealed.Select(h => h.ToDto()).ToList(),
                string.Join("\n", revealed.Select(h => h.DiscoveryText))
            );
        }

        return await _examinationService.CheckPassivePerceptionAsync(
            currentRoom.Id, passivePerception, currentRoom.HiddenElements, ct);
    }

    private Task<(bool Success, ExaminationResultDto? Result, string? ErrorMessage)> TryBasicExamineAsync(
        string target,
        int witsValue,
        Room currentRoom)
    {
        // Basic examination without the full service - uses skill check for layer determination
        var (highestLayer, checkResult) = _skillCheckService.DetermineExaminationLayer(witsValue);
        var maxLayer = (ExaminationLayer)highestLayer;

        var item = currentRoom.GetItemByName(target);
        if (item != null)
        {
            var layerTexts = new List<LayerTextDto>
            {
                new(ExaminationLayer.Cursory, item.Description, 0)
            };

            if (maxLayer >= ExaminationLayer.Detailed)
            {
                layerTexts.Add(new LayerTextDto(
                    ExaminationLayer.Detailed,
                    $"This {item.Type.ToString().ToLower()} appears to be in serviceable condition.",
                    12));
            }

            var composite = string.Join("\n\n", layerTexts.Select(l => l.Text));

            var result = new ExaminationResultDto(
                item.Id, item.Name, checkResult.RollResult, checkResult.TotalResult,
                maxLayer, composite, false, null, layerTexts);

            return Task.FromResult<(bool, ExaminationResultDto?, string?)>((true, result, null));
        }

        var monster = currentRoom.Monsters.FirstOrDefault(m =>
            m.Name.Equals(target, StringComparison.OrdinalIgnoreCase));
        if (monster != null)
        {
            var description = monster.IsAlive
                ? $"A {monster.Name.ToLower()} watches you warily."
                : $"The {monster.Name.ToLower()} lies motionless.";

            var layerTexts = new List<LayerTextDto> { new(ExaminationLayer.Cursory, description, 0) };
            var result = new ExaminationResultDto(
                monster.Id, monster.Name, checkResult.RollResult, checkResult.TotalResult,
                maxLayer, description, false, null, layerTexts);

            return Task.FromResult<(bool, ExaminationResultDto?, string?)>((true, result, null));
        }

        return Task.FromResult<(bool, ExaminationResultDto?, string?)>(
            (false, null, $"Cannot find '{target}' to examine."));
    }

    private static Direction? ParseDirection(string input)
    {
        return input.ToLowerInvariant() switch
        {
            "n" or "north" => Direction.North,
            "s" or "south" => Direction.South,
            "e" or "east" => Direction.East,
            "w" or "west" => Direction.West,
            "u" or "up" => Direction.Up,
            "d" or "down" => Direction.Down,
            "ne" or "northeast" => Direction.Northeast,
            "nw" or "northwest" => Direction.Northwest,
            "se" or "southeast" => Direction.Southeast,
            "sw" or "southwest" => Direction.Southwest,
            _ => null
        };
    }

    private static bool IsUniversalStructuralElement(string target)
    {
        var structuralElements = new[] { "walls", "wall", "floor", "ceiling", "ground" };
        return structuralElements.Contains(target.ToLowerInvariant());
    }
}
