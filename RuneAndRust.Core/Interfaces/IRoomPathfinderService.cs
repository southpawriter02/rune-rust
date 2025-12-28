using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Result of a room pathfinding operation.
/// </summary>
/// <param name="Success">Whether a valid path was found.</param>
/// <param name="RoomPath">Room IDs in traversal order (excluding start, including destination).</param>
/// <param name="Directions">Movement directions for each step in the path.</param>
/// <param name="FailureReason">Reason for failure if Success is false.</param>
public record RoomPathResult(
    bool Success,
    List<Guid>? RoomPath,
    List<Direction>? Directions,
    string? FailureReason)
{
    /// <summary>
    /// Creates a successful path result.
    /// </summary>
    public static RoomPathResult Succeeded(List<Guid> roomPath, List<Direction> directions)
        => new(true, roomPath, directions, null);

    /// <summary>
    /// Creates a failed path result.
    /// </summary>
    public static RoomPathResult Failed(string reason)
        => new(false, null, null, reason);

    /// <summary>
    /// Creates a result for when already at destination.
    /// </summary>
    public static RoomPathResult AlreadyAtDestination()
        => new(true, new List<Guid>(), new List<Direction>(), null);
}

/// <summary>
/// Service for pathfinding through the room graph.
/// Uses BFS algorithm on Room.Exits connections, respecting fog of war constraints.
/// </summary>
/// <remarks>See: SPEC-NAV-002 for Pathfinder (Fast Travel) design (v0.3.20c).</remarks>
public interface IRoomPathfinderService
{
    /// <summary>
    /// Finds the shortest path between two rooms using BFS.
    /// Only considers rooms in the visited set (fog of war constraint).
    /// </summary>
    /// <param name="startRoomId">The starting room ID.</param>
    /// <param name="destinationRoomId">The target room ID.</param>
    /// <param name="visitedRoomIds">Set of room IDs the player has explored.</param>
    /// <returns>Path result containing room IDs and directions, or failure reason.</returns>
    Task<RoomPathResult> FindPathAsync(
        Guid startRoomId,
        Guid destinationRoomId,
        HashSet<Guid> visitedRoomIds);

    /// <summary>
    /// Finds the nearest room with a specific feature using BFS.
    /// </summary>
    /// <param name="startRoomId">The starting room ID.</param>
    /// <param name="feature">The room feature to search for.</param>
    /// <param name="visitedRoomIds">Set of room IDs the player has explored.</param>
    /// <returns>The nearest room with the feature, or null if none found.</returns>
    Task<Room?> FindNearestFeatureAsync(
        Guid startRoomId,
        RoomFeature feature,
        HashSet<Guid> visitedRoomIds);

    /// <summary>
    /// Finds rooms matching a partial name (case-insensitive).
    /// </summary>
    /// <param name="namePart">Partial room name to search for.</param>
    /// <param name="visitedRoomIds">Set of room IDs the player has explored.</param>
    /// <returns>Collection of matching rooms, ordered by relevance.</returns>
    Task<IEnumerable<Room>> FindRoomsByNameAsync(
        string namePart,
        HashSet<Guid> visitedRoomIds);
}
