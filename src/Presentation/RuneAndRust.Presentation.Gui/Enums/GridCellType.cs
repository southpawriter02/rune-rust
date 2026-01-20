namespace RuneAndRust.Presentation.Gui.Enums;

/// <summary>
/// Types of terrain cells for GUI display.
/// </summary>
/// <remarks>
/// Maps from domain <see cref="Domain.Enums.TerrainType"/> to GUI-specific display types.
/// </remarks>
public enum GridCellType
{
    /// <summary>
    /// Standard walkable floor.
    /// </summary>
    Floor = 0,

    /// <summary>
    /// Impassable wall or obstacle.
    /// </summary>
    Wall = 1,

    /// <summary>
    /// Water terrain (difficult terrain).
    /// </summary>
    Water = 2,

    /// <summary>
    /// Hazardous terrain (deals damage).
    /// </summary>
    Hazard = 3,

    /// <summary>
    /// Cover terrain (provides defense bonus).
    /// </summary>
    Cover = 4
}
