using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Application.Services;

public class GameSessionService
{
    private readonly IGameRepository _repository;
    private readonly ILogger<GameSessionService> _logger;
    private readonly CombatService _combatService;

    private GameSession? _currentSession;

    public bool HasActiveSession => _currentSession != null;
    public GameStateDto? CurrentState => _currentSession?.ToDto();

    public GameSessionService(
        IGameRepository repository,
        ILogger<GameSessionService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _combatService = new CombatService();
    }

    public async Task<GameStateDto> StartNewGameAsync(string playerName, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting new game for player: {PlayerName}", playerName);

        _currentSession = GameSession.CreateNew(playerName);
        await _repository.SaveAsync(_currentSession, ct);

        _logger.LogInformation("New game session created: {SessionId}", _currentSession.Id);

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
}
