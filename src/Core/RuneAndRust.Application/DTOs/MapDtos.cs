namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object for map display.
/// </summary>
/// <param name="MapDisplay">The rendered ASCII map string.</param>
/// <param name="CurrentLevel">The player's current Z level.</param>
/// <param name="TotalLevels">Total number of explored levels.</param>
/// <param name="Statistics">Exploration statistics.</param>
public record MapDto(
    string MapDisplay,
    int CurrentLevel,
    int TotalLevels,
    MapStatisticsDto Statistics);

/// <summary>
/// Statistics about dungeon exploration.
/// </summary>
/// <param name="TotalRooms">Total rooms in dungeon.</param>
/// <param name="VisitedRooms">Number of visited rooms.</param>
/// <param name="ClearedRooms">Number of cleared rooms.</param>
public record MapStatisticsDto(
    int TotalRooms,
    int VisitedRooms,
    int ClearedRooms)
{
    /// <summary>
    /// Gets the exploration percentage.
    /// </summary>
    public float ExplorationPercent => TotalRooms > 0
        ? (float)VisitedRooms / TotalRooms * 100
        : 0;
}

/// <summary>
/// Single level map data.
/// </summary>
/// <param name="Level">The Z level.</param>
/// <param name="MapDisplay">ASCII map for this level.</param>
/// <param name="RoomCount">Total rooms on this level.</param>
/// <param name="VisitedCount">Visited rooms on this level.</param>
public record MapLevelDto(
    int Level,
    string MapDisplay,
    int RoomCount,
    int VisitedCount);
