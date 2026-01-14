namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Entry for placing terrain in a room configuration.
/// </summary>
/// <remarks>
/// Used in room configuration JSON to define terrain placement.
/// Positions use chess-like notation (e.g., "A1", "E5").
/// </remarks>
public class TerrainLayoutEntry
{
    /// <summary>
    /// Gets or sets the positions to place this terrain.
    /// </summary>
    /// <remarks>
    /// Each position uses chess-like notation (e.g., "E1", "F1").
    /// </remarks>
    public string[] Positions { get; set; } = [];

    /// <summary>
    /// Gets or sets the terrain definition ID.
    /// </summary>
    /// <remarks>
    /// References a terrain definition from <c>config/terrain.json</c>.
    /// </remarks>
    public string TerrainId { get; set; } = string.Empty;
}

/// <summary>
/// Entry for placing cover in a room configuration.
/// </summary>
/// <remarks>
/// Used in room configuration JSON to define cover placement.
/// Supports both single position and multiple positions.
/// </remarks>
public class CoverLayoutEntry
{
    /// <summary>
    /// Gets or sets a single position for this cover.
    /// </summary>
    /// <remarks>
    /// Uses chess-like notation (e.g., "B2").
    /// If <see cref="Positions"/> is also set, both are used.
    /// </remarks>
    public string? Position { get; set; }

    /// <summary>
    /// Gets or sets multiple positions for this cover.
    /// </summary>
    /// <remarks>
    /// Each position uses chess-like notation (e.g., "C4", "E4").
    /// </remarks>
    public string[]? Positions { get; set; }

    /// <summary>
    /// Gets or sets the cover definition ID.
    /// </summary>
    /// <remarks>
    /// References a cover definition from <c>config/cover.json</c>.
    /// </remarks>
    public string CoverId { get; set; } = string.Empty;

    /// <summary>
    /// Gets all positions for this cover entry.
    /// </summary>
    /// <returns>All positions (from both Position and Positions).</returns>
    public IEnumerable<string> GetAllPositions()
    {
        if (!string.IsNullOrEmpty(Position))
            yield return Position;

        if (Positions != null)
        {
            foreach (var pos in Positions)
                yield return pos;
        }
    }
}
