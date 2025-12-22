using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Handles player navigation between rooms.
/// Validates exits, updates game state, and provides room descriptions.
/// Triggers movement hazards on room entry (v0.3.3a).
/// Displays ambient condition effects on room entry (v0.3.3b).
/// </summary>
public class NavigationService : INavigationService
{
    private readonly GameState _gameState;
    private readonly IRoomRepository _roomRepository;
    private readonly IHazardService _hazardService;
    private readonly IConditionService _conditionService;
    private readonly IInputHandler _inputHandler;
    private readonly ILogger<NavigationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="gameState">The current game state.</param>
    /// <param name="roomRepository">The room repository.</param>
    /// <param name="hazardService">The hazard service for movement triggers (v0.3.3a).</param>
    /// <param name="conditionService">The condition service for ambient effects (v0.3.3b).</param>
    /// <param name="inputHandler">The input handler for displaying hazard messages.</param>
    /// <param name="logger">The logger instance.</param>
    public NavigationService(
        GameState gameState,
        IRoomRepository roomRepository,
        IHazardService hazardService,
        IConditionService conditionService,
        IInputHandler inputHandler,
        ILogger<NavigationService> logger)
    {
        _gameState = gameState;
        _roomRepository = roomRepository;
        _hazardService = hazardService;
        _conditionService = conditionService;
        _inputHandler = inputHandler;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> MoveAsync(Direction direction)
    {
        _logger.LogDebug("Attempting to move {Direction}", direction);

        // 1. Get current room
        if (_gameState.CurrentRoomId == null)
        {
            _logger.LogWarning("Move attempted with no current room set");
            return "Error: You are lost in the void. No current location.";
        }

        var currentRoom = await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId.Value);
        if (currentRoom == null)
        {
            _logger.LogError("Current room ID {RoomId} not found in database", _gameState.CurrentRoomId);
            return "Error: Your current location no longer exists.";
        }

        // 2. Check if exit exists in that direction
        if (!currentRoom.Exits.TryGetValue(direction, out var nextRoomId))
        {
            _logger.LogDebug("Invalid move: No exit {Direction} from {RoomName}", direction, currentRoom.Name);
            return $"You cannot go {direction.ToString().ToLower()} from here.";
        }

        // 3. Get the destination room
        var nextRoom = await _roomRepository.GetByIdAsync(nextRoomId);
        if (nextRoom == null)
        {
            _logger.LogError("Exit leads to non-existent room {RoomId}", nextRoomId);
            return "Error: The path leads nowhere. The dungeon seems corrupted.";
        }

        // 4. Update game state
        _gameState.CurrentRoomId = nextRoomId;
        _gameState.TurnCount++;
        _gameState.VisitedRoomIds.Add(nextRoomId);

        _logger.LogInformation("Player moved {Direction} to '{RoomName}' ({RoomId})",
            direction, nextRoom.Name, nextRoom.Id);
        _logger.LogDebug("[Navigation] Room {RoomId} marked as visited. Total visited: {Count}",
            nextRoomId, _gameState.VisitedRoomIds.Count);

        // 5. Trigger movement hazards (v0.3.3a)
        var hazardResults = await _hazardService.TriggerOnRoomEnterAsync(nextRoom);
        foreach (var result in hazardResults.Where(r => r.WasTriggered))
        {
            _inputHandler.DisplayMessage($"[HAZARD] {result.Message}");
            _logger.LogWarning(
                "[Hazard] Movement trigger: {Hazard} in {Room}. Damage: {Damage}",
                result.HazardName, nextRoom.Name, result.TotalDamage);
        }

        // 6. Return the new room description with ambient condition (v0.3.3b)
        return await FormatRoomDescriptionAsync(nextRoom, $"You move {direction.ToString().ToLower()}...\n\n");
    }

    /// <inheritdoc/>
    public async Task<string> LookAsync()
    {
        _logger.LogDebug("Player looking at current room");

        var room = await GetCurrentRoomAsync();
        if (room == null)
        {
            return "You are surrounded by an impenetrable void.";
        }

        return await FormatRoomDescriptionAsync(room);
    }

    /// <inheritdoc/>
    public async Task<Room?> GetCurrentRoomAsync()
    {
        if (_gameState.CurrentRoomId == null)
        {
            _logger.LogDebug("No current room set in game state");
            return null;
        }

        return await _roomRepository.GetByIdAsync(_gameState.CurrentRoomId.Value);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Direction>> GetAvailableExitsAsync()
    {
        var room = await GetCurrentRoomAsync();
        if (room == null)
        {
            return Enumerable.Empty<Direction>();
        }

        return room.Exits.Keys;
    }

    /// <summary>
    /// Formats a room description for display to the player.
    /// Includes ambient condition information if present (v0.3.3b).
    /// </summary>
    /// <param name="room">The room to describe.</param>
    /// <param name="prefix">Optional prefix text (e.g., movement confirmation).</param>
    /// <returns>Formatted room description string.</returns>
    private async Task<string> FormatRoomDescriptionAsync(Room room, string prefix = "")
    {
        var exits = room.Exits.Keys.Select(d => d.ToString().ToLower());
        var exitText = exits.Any()
            ? $"Exits: {string.Join(", ", exits)}"
            : "There are no obvious exits.";

        // Check for ambient condition (v0.3.3b)
        var conditionText = string.Empty;
        var condition = await _conditionService.GetRoomConditionAsync(room.Id);
        if (condition != null)
        {
            conditionText = $"\n[AMBIENT] [{condition.Color}]{condition.Name}[/]: {condition.Description}";
            _logger.LogDebug(
                "[Condition] Room {RoomName} has condition [{ConditionName}]",
                room.Name, condition.Name);
        }

        return $"{prefix}[{room.Name}]\n{room.Description}{conditionText}\n\n{exitText}";
    }
}
