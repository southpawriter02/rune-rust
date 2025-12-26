using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for executing auto-travel along calculated paths.
/// Validates preconditions, checks path safety, and orchestrates movement.
/// </summary>
/// <remarks>See: SPEC-NAV-002 for Pathfinder (Fast Travel) design (v0.3.20c).</remarks>
public class AutoTravelService : IAutoTravelService
{
    private readonly ILogger<AutoTravelService> _logger;
    private readonly GameState _gameState;
    private readonly IRoomPathfinderService _pathfinder;
    private readonly INavigationService _navigationService;
    private readonly IInventoryService _inventoryService;
    private readonly IRoomRepository _roomRepository;
    private readonly IHazardService _hazardService;
    private readonly IInputHandler _inputHandler;

    /// <summary>
    /// Delay between travel steps in milliseconds for visual feedback.
    /// </summary>
    private const int TravelStepDelayMs = 75;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoTravelService"/> class.
    /// </summary>
    public AutoTravelService(
        ILogger<AutoTravelService> logger,
        GameState gameState,
        IRoomPathfinderService pathfinder,
        INavigationService navigationService,
        IInventoryService inventoryService,
        IRoomRepository roomRepository,
        IHazardService hazardService,
        IInputHandler inputHandler)
    {
        _logger = logger;
        _gameState = gameState;
        _pathfinder = pathfinder;
        _navigationService = navigationService;
        _inventoryService = inventoryService;
        _roomRepository = roomRepository;
        _hazardService = hazardService;
        _inputHandler = inputHandler;
    }

    /// <inheritdoc/>
    public async Task<string?> ValidateTravelPreconditionsAsync(Character character)
    {
        _logger.LogDebug("[Travel] Validating preconditions for {Character}", character.Name);

        // Check exhaustion
        if (character.HasStatusEffect(StatusEffectType.Exhausted))
        {
            _logger.LogDebug("[Travel] Blocked: Character is exhausted");
            return "Too exhausted to travel fast. Rest first.";
        }

        // Check encumbrance
        var burden = await _inventoryService.CalculateBurdenAsync(character);
        if (burden == BurdenState.Overburdened)
        {
            _logger.LogDebug("[Travel] Blocked: Character is overburdened");
            return "Too overburdened to travel. Drop items first.";
        }

        // Check game phase (must be exploration)
        if (_gameState.Phase != GamePhase.Exploration)
        {
            _logger.LogDebug("[Travel] Blocked: Not in exploration phase");
            return "Cannot travel during combat.";
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<string?> ValidatePathSafetyAsync(IEnumerable<Guid> path)
    {
        var pathList = path.ToList();
        _logger.LogDebug("[Travel] Validating safety of {Count} room path", pathList.Count);

        if (pathList.Count == 0)
        {
            return null;
        }

        var rooms = await _roomRepository.GetBatchAsync(pathList);
        var roomLookup = rooms.ToDictionary(r => r.Id);

        foreach (var roomId in pathList)
        {
            if (!roomLookup.TryGetValue(roomId, out var room))
            {
                continue;
            }

            // Check for lethal danger level
            if (room.DangerLevel == DangerLevel.Lethal)
            {
                _logger.LogDebug("[Travel] Path blocked: {Room} is lethal", room.Name);
                return $"Path blocked: {room.Name} is too dangerous.";
            }

            // Check for dormant movement hazards
            var hazards = await _hazardService.GetActiveHazardsAsync(roomId);
            var movementHazard = hazards.FirstOrDefault(h =>
                h.Trigger == TriggerType.Movement &&
                h.State == HazardState.Dormant);

            if (movementHazard != null)
            {
                _logger.LogDebug("[Travel] Path blocked: Hazard {Hazard} in {Room}", movementHazard.Name, room.Name);
                return $"Hazard detected ahead: {movementHazard.Name} in {room.Name}.";
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<AutoTravelResult> ExecuteTravelAsync(
        IEnumerable<Direction> directions,
        CancellationToken cancellationToken = default)
    {
        var directionList = directions.ToList();
        _logger.LogInformation("[Travel] Beginning journey of {Count} steps", directionList.Count);

        if (directionList.Count == 0)
        {
            return AutoTravelResult.Failed("You are already at your destination.");
        }

        var roomsTraveled = 0;
        var initialTurnCount = _gameState.TurnCount;
        Room? currentRoom = null;

        try
        {
            for (var i = 0; i < directionList.Count; i++)
            {
                var direction = directionList[i];

                // Check for cancellation (ESC key)
                if (cancellationToken.IsCancellationRequested)
                {
                    currentRoom = await _navigationService.GetCurrentRoomAsync();
                    var turnsElapsed = _gameState.TurnCount - initialTurnCount;
                    _logger.LogWarning("[Travel] Interrupted at {Room}. Reason: UserCancelled", currentRoom?.Name);

                    return AutoTravelResult.Interrupted(
                        roomsTraveled,
                        turnsElapsed,
                        currentRoom,
                        TravelInterruptReason.UserCancelled,
                        $"Travel cancelled. You are now in {currentRoom?.Name ?? "unknown location"}.");
                }

                // Check for combat phase change (ambush interrupt)
                if (_gameState.Phase == GamePhase.Combat)
                {
                    currentRoom = await _navigationService.GetCurrentRoomAsync();
                    var turnsElapsed = _gameState.TurnCount - initialTurnCount;
                    _logger.LogWarning("[Travel] Interrupted at {Room}. Reason: CombatTriggered", currentRoom?.Name);

                    return AutoTravelResult.Interrupted(
                        roomsTraveled,
                        turnsElapsed,
                        currentRoom,
                        TravelInterruptReason.CombatTriggered,
                        $"Travel interrupted by combat! You are in {currentRoom?.Name ?? "unknown location"}.");
                }

                _logger.LogTrace("[Travel] Step {N}/{Total}: {Direction}", i + 1, directionList.Count, direction);

                // Execute movement via NavigationService (handles all side effects)
                var moveResult = await _navigationService.MoveAsync(direction);

                // Check if move failed (blocked exit, etc.)
                if (moveResult.Contains("cannot go") || moveResult.Contains("Error"))
                {
                    currentRoom = await _navigationService.GetCurrentRoomAsync();
                    var turnsElapsed = _gameState.TurnCount - initialTurnCount;

                    return AutoTravelResult.Interrupted(
                        roomsTraveled,
                        turnsElapsed,
                        currentRoom,
                        TravelInterruptReason.HazardEncountered,
                        $"Path blocked: {moveResult}");
                }

                roomsTraveled++;

                // Display passage message
                currentRoom = await _navigationService.GetCurrentRoomAsync();
                _inputHandler.DisplayMessage($"[grey]...passing through {currentRoom?.Name ?? "unknown"}...[/]");

                // Brief delay for visual feedback
                await Task.Delay(TravelStepDelayMs, CancellationToken.None);
            }

            // Successful completion
            currentRoom = await _navigationService.GetCurrentRoomAsync();
            var finalTurnsElapsed = _gameState.TurnCount - initialTurnCount;
            _logger.LogInformation("[Travel] Arrived at {Destination} after {Turns} turns", currentRoom?.Name, finalTurnsElapsed);

            return AutoTravelResult.Succeeded(roomsTraveled, finalTurnsElapsed, currentRoom!);
        }
        catch (OperationCanceledException)
        {
            currentRoom = await _navigationService.GetCurrentRoomAsync();
            var turnsElapsed = _gameState.TurnCount - initialTurnCount;

            return AutoTravelResult.Interrupted(
                roomsTraveled,
                turnsElapsed,
                currentRoom,
                TravelInterruptReason.UserCancelled,
                $"Travel cancelled. You are now in {currentRoom?.Name ?? "unknown location"}.");
        }
    }

    /// <inheritdoc/>
    public async Task<AutoTravelResult> TravelToAsync(
        string target,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[Travel] Resolving target: '{Target}'", target);

        // Validate character exists
        if (_gameState.CurrentCharacter == null)
        {
            return AutoTravelResult.Failed("No active character.");
        }

        if (_gameState.CurrentRoomId == null)
        {
            return AutoTravelResult.Failed("No current location.");
        }

        // Validate preconditions
        var preconditionError = await ValidateTravelPreconditionsAsync(_gameState.CurrentCharacter);
        if (preconditionError != null)
        {
            return AutoTravelResult.Failed(preconditionError);
        }

        var startRoomId = _gameState.CurrentRoomId.Value;
        var visitedRooms = _gameState.VisitedRoomIds;

        // Resolve target to destination room
        Room? destinationRoom = await ResolveTargetAsync(target, startRoomId, visitedRooms);

        if (destinationRoom == null)
        {
            return AutoTravelResult.Failed($"Could not find destination: '{target}'.");
        }

        _logger.LogInformation("[Travel] Beginning journey to {Destination}", destinationRoom.Name);

        // Find path
        var pathResult = await _pathfinder.FindPathAsync(startRoomId, destinationRoom.Id, visitedRooms);

        if (!pathResult.Success)
        {
            return AutoTravelResult.Failed(pathResult.FailureReason ?? "No path found.");
        }

        // Already at destination
        if (pathResult.RoomPath == null || pathResult.RoomPath.Count == 0)
        {
            return AutoTravelResult.Failed("You are already there.");
        }

        // Validate path safety
        var safetyError = await ValidatePathSafetyAsync(pathResult.RoomPath);
        if (safetyError != null)
        {
            return AutoTravelResult.Failed(safetyError);
        }

        _inputHandler.DisplayMessage($"[green]Traveling to {destinationRoom.Name}...[/]");

        // Execute travel
        return await ExecuteTravelAsync(pathResult.Directions!, cancellationToken);
    }

    /// <summary>
    /// Resolves a target string to a destination room.
    /// Handles keywords: home, anchor, surface, and partial room names.
    /// </summary>
    private async Task<Room?> ResolveTargetAsync(
        string target,
        Guid startRoomId,
        HashSet<Guid> visitedRoomIds)
    {
        var normalizedTarget = target.Trim().ToLowerInvariant();

        // Handle keywords
        switch (normalizedTarget)
        {
            case "home":
            case "anchor":
                _logger.LogDebug("[Travel] Resolving 'home' keyword to nearest RunicAnchor");
                return await _pathfinder.FindNearestFeatureAsync(
                    startRoomId, RoomFeature.RunicAnchor, visitedRoomIds);

            case "surface":
                _logger.LogDebug("[Travel] Resolving 'surface' keyword to nearest Z=0 room");
                return await FindNearestSurfaceRoomAsync(startRoomId, visitedRoomIds);

            default:
                // Search by room name
                var matches = await _pathfinder.FindRoomsByNameAsync(target, visitedRoomIds);
                var matchList = matches.ToList();

                if (matchList.Count == 0)
                {
                    _logger.LogDebug("[Travel] No rooms found matching '{Target}'", target);
                    return null;
                }

                if (matchList.Count == 1)
                {
                    return matchList[0];
                }

                // Multiple matches - use first (closest name match)
                _logger.LogDebug(
                    "[Travel] Multiple matches for '{Target}', using first: {Room}",
                    target, matchList[0].Name);
                return matchList[0];
        }
    }

    /// <summary>
    /// Finds the nearest room at Z=0 (surface level).
    /// </summary>
    private async Task<Room?> FindNearestSurfaceRoomAsync(
        Guid startRoomId,
        HashSet<Guid> visitedRoomIds)
    {
        var rooms = await _roomRepository.GetBatchAsync(visitedRoomIds);
        var roomLookup = rooms.ToDictionary(r => r.Id);

        // Check if starting room is already at surface
        if (roomLookup.TryGetValue(startRoomId, out var startRoom) && startRoom.PositionZ == 0)
        {
            return startRoom;
        }

        // BFS to find nearest Z=0 room
        var queue = new Queue<Guid>();
        var visited = new HashSet<Guid>();

        queue.Enqueue(startRoomId);
        visited.Add(startRoomId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            if (!roomLookup.TryGetValue(currentId, out var currentRoom))
            {
                continue;
            }

            foreach (var (_, nextRoomId) in currentRoom.Exits)
            {
                if (!visitedRoomIds.Contains(nextRoomId) || visited.Contains(nextRoomId))
                {
                    continue;
                }

                visited.Add(nextRoomId);

                if (roomLookup.TryGetValue(nextRoomId, out var nextRoom))
                {
                    if (nextRoom.PositionZ == 0)
                    {
                        return nextRoom;
                    }

                    queue.Enqueue(nextRoomId);
                }
            }
        }

        return null;
    }
}
