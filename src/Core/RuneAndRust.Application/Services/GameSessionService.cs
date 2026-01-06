using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

public class GameSessionService
{
    private readonly IGameRepository _repository;
    private readonly ILogger<GameSessionService> _logger;
    private readonly CombatService _combatService;
    private readonly IExaminationService? _examinationService;
    private readonly SkillCheckService _skillCheckService;
    private readonly IDungeonGenerator? _dungeonGenerator;

    private GameSession? _currentSession;

    public bool HasActiveSession => _currentSession != null;
    public GameStateDto? CurrentState => _currentSession?.ToDto();

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
        _skillCheckService = new SkillCheckService();
        _dungeonGenerator = dungeonGenerator;
    }

    public async Task<GameStateDto> StartNewGameAsync(string playerName, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting new game for player: {PlayerName}", playerName);

        _currentSession = GameSession.CreateNew(playerName);
        await _repository.SaveAsync(_currentSession, ct);

        _logger.LogInformation("New game session created: {SessionId}", _currentSession.Id);

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

        return _currentSession.ToDto();
    }

    public async Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default)
    {
        return await _repository.GetSavedGamesAsync(ct);
    }

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
    }

    public (bool Success, string Message) TryMove(Direction direction)
    {
        if (_currentSession == null)
            return (false, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, "Error: Current room not found.");

        if (!currentRoom.HasExit(direction))
            return (false, $"You cannot go {direction.ToString().ToLower()}. There is no exit in that direction.");

        if (_currentSession.TryMovePlayer(direction))
        {
            var newRoom = _currentSession.CurrentRoom;
            return (true, $"You head {direction.ToString().ToLower()}.");
        }

        return (false, "You cannot move in that direction.");
    }

    public (bool Success, string Message) TryPickUpItem(string itemName)
    {
        if (_currentSession == null)
            return (false, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, "Error: Current room not found.");

        var item = currentRoom.GetItemByName(itemName);
        if (item == null)
            return (false, $"There is no '{itemName}' here.");

        if (_currentSession.Player.Inventory.IsFull)
            return (false, "Your inventory is full!");

        if (_currentSession.TryPickUpItem(itemName))
            return (true, $"You picked up the {item.Name}.");

        return (false, $"Could not pick up '{itemName}'.");
    }

    public (bool Success, string Message) TryAttack()
    {
        if (_currentSession == null)
            return (false, "No active game session.");

        var currentRoom = _currentSession.CurrentRoom;
        if (currentRoom == null)
            return (false, "Error: Current room not found.");

        var monster = currentRoom.GetAliveMonsters().FirstOrDefault();
        if (monster == null)
            return (false, "There is nothing to attack here.");

        var result = _combatService.ResolveCombatRound(_currentSession.Player, monster);
        var description = _combatService.GetCombatDescription(
            result,
            _currentSession.Player.Name,
            monster.Name
        );

        if (result.PlayerDefeated)
        {
            _currentSession.SetState(GameState.GameOver);
        }

        return (true, description);
    }

    public RoomDto? GetCurrentRoom()
    {
        return _currentSession?.CurrentRoom?.ToDto();
    }

    public InventoryDto? GetInventory()
    {
        return _currentSession?.Player.Inventory.ToDto();
    }

    public void EndSession()
    {
        _currentSession = null;
        _logger.LogInformation("Game session ended");
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
}
