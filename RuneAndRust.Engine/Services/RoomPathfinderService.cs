using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for pathfinding through the room graph using BFS.
/// Respects fog of war constraints by only considering visited rooms.
/// </summary>
/// <remarks>See: SPEC-NAV-002 for Pathfinder (Fast Travel) design (v0.3.20c).</remarks>
public class RoomPathfinderService : IRoomPathfinderService
{
    private readonly ILogger<RoomPathfinderService> _logger;
    private readonly IRoomRepository _roomRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomPathfinderService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="roomRepository">The room repository for batch fetching.</param>
    public RoomPathfinderService(
        ILogger<RoomPathfinderService> logger,
        IRoomRepository roomRepository)
    {
        _logger = logger;
        _roomRepository = roomRepository;
    }

    /// <inheritdoc/>
    public async Task<RoomPathResult> FindPathAsync(
        Guid startRoomId,
        Guid destinationRoomId,
        HashSet<Guid> visitedRoomIds)
    {
        _logger.LogDebug(
            "[Path] Finding path from {Start} to {Destination}",
            startRoomId, destinationRoomId);

        // Already at destination
        if (startRoomId == destinationRoomId)
        {
            _logger.LogDebug("[Path] Already at destination");
            return RoomPathResult.AlreadyAtDestination();
        }

        // Destination must be visited (fog of war)
        if (!visitedRoomIds.Contains(destinationRoomId))
        {
            _logger.LogDebug("[Path] Destination {Id} not in visited rooms", destinationRoomId);
            return RoomPathResult.Failed("Destination has not been explored yet.");
        }

        // Start must also be visited
        if (!visitedRoomIds.Contains(startRoomId))
        {
            _logger.LogDebug("[Path] Start {Id} not in visited rooms", startRoomId);
            return RoomPathResult.Failed("Current location is not in explored areas.");
        }

        // Fetch all visited rooms for BFS
        var rooms = await _roomRepository.GetBatchAsync(visitedRoomIds);
        var roomLookup = rooms.ToDictionary(r => r.Id);

        _logger.LogDebug("[Path] Loaded {Count} visited rooms for pathfinding", roomLookup.Count);

        // BFS to find shortest path
        var queue = new Queue<Guid>();
        var cameFrom = new Dictionary<Guid, (Guid PreviousRoomId, Direction DirectionTaken)>();

        queue.Enqueue(startRoomId);
        cameFrom[startRoomId] = (Guid.Empty, default);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            if (currentId == destinationRoomId)
            {
                var result = ReconstructPath(cameFrom, startRoomId, destinationRoomId);
                _logger.LogDebug(
                    "[Path] Route from {Start} to {End}: {Count} steps",
                    startRoomId, destinationRoomId, result.Directions?.Count ?? 0);
                return result;
            }

            if (!roomLookup.TryGetValue(currentId, out var currentRoom))
            {
                continue;
            }

            foreach (var (direction, nextRoomId) in currentRoom.Exits)
            {
                // Fog of war constraint
                if (!visitedRoomIds.Contains(nextRoomId))
                {
                    continue;
                }

                // Already visited in BFS
                if (cameFrom.ContainsKey(nextRoomId))
                {
                    continue;
                }

                cameFrom[nextRoomId] = (currentId, direction);
                queue.Enqueue(nextRoomId);
            }
        }

        _logger.LogDebug("[Path] No route from {Start} to {End}", startRoomId, destinationRoomId);
        return RoomPathResult.Failed("No path through explored areas.");
    }

    /// <inheritdoc/>
    public async Task<Room?> FindNearestFeatureAsync(
        Guid startRoomId,
        RoomFeature feature,
        HashSet<Guid> visitedRoomIds)
    {
        _logger.LogDebug(
            "[Path] Finding nearest {Feature} from {Start}",
            feature, startRoomId);

        if (!visitedRoomIds.Contains(startRoomId))
        {
            _logger.LogDebug("[Path] Start room not in visited set");
            return null;
        }

        var rooms = await _roomRepository.GetBatchAsync(visitedRoomIds);
        var roomLookup = rooms.ToDictionary(r => r.Id);

        // Check if starting room has the feature
        if (roomLookup.TryGetValue(startRoomId, out var startRoom) &&
            startRoom.HasFeature(feature))
        {
            _logger.LogDebug("[Path] Start room has {Feature}", feature);
            return startRoom;
        }

        // BFS to find nearest room with feature
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
                    if (nextRoom.HasFeature(feature))
                    {
                        _logger.LogDebug(
                            "[Path] Found nearest {Feature} at {Room}",
                            feature, nextRoom.Name);
                        return nextRoom;
                    }

                    queue.Enqueue(nextRoomId);
                }
            }
        }

        _logger.LogDebug("[Path] No room with {Feature} found in explored areas", feature);
        return null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Room>> FindRoomsByNameAsync(
        string namePart,
        HashSet<Guid> visitedRoomIds)
    {
        _logger.LogDebug("[Path] Searching for rooms matching '{Name}'", namePart);

        if (string.IsNullOrWhiteSpace(namePart))
        {
            return Enumerable.Empty<Room>();
        }

        var rooms = await _roomRepository.GetBatchAsync(visitedRoomIds);
        var normalizedSearch = namePart.Trim().ToLowerInvariant();

        var matches = rooms
            .Where(r => r.Name.ToLowerInvariant().Contains(normalizedSearch))
            .OrderBy(r => r.Name.Length) // Prefer shorter (more specific) matches
            .ThenBy(r => r.Name)
            .ToList();

        _logger.LogDebug("[Path] Found {Count} rooms matching '{Name}'", matches.Count, namePart);

        return matches;
    }

    /// <summary>
    /// Reconstructs the path from BFS traversal data.
    /// </summary>
    private static RoomPathResult ReconstructPath(
        Dictionary<Guid, (Guid PreviousRoomId, Direction DirectionTaken)> cameFrom,
        Guid startRoomId,
        Guid destinationRoomId)
    {
        var roomPath = new List<Guid>();
        var directions = new List<Direction>();

        var currentId = destinationRoomId;

        while (currentId != startRoomId)
        {
            var (prevId, direction) = cameFrom[currentId];
            roomPath.Add(currentId);
            directions.Add(direction);
            currentId = prevId;
        }

        // Reverse to get start-to-end order
        roomPath.Reverse();
        directions.Reverse();

        return RoomPathResult.Succeeded(roomPath, directions);
    }
}
