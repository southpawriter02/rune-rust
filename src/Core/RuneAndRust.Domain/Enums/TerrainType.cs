namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of terrain on grid cells affecting movement and combat.
/// </summary>
/// <remarks>
/// <para>
/// Terrain types determine movement costs, passability, and hazard effects:
/// <list type="bullet">
/// <item><description><see cref="Normal"/>: Standard 1x movement cost</description></item>
/// <item><description><see cref="Difficult"/>: 2x movement cost (rubble, water, mud)</description></item>
/// <item><description><see cref="Impassable"/>: Cannot be entered (walls, pits)</description></item>
/// <item><description><see cref="Hazardous"/>: Deals damage on entry (fire, acid)</description></item>
/// </list>
/// </para>
/// </remarks>
public enum TerrainType
{
    /// <summary>
    /// Normal terrain with standard movement cost (1x multiplier).
    /// </summary>
    /// <remarks>
    /// Examples: stone floor, grass, tile
    /// </remarks>
    Normal = 0,

    /// <summary>
    /// Difficult terrain that costs double movement points to traverse.
    /// </summary>
    /// <remarks>
    /// Examples: rubble, shallow water, mud, dense vegetation
    /// </remarks>
    Difficult = 1,

    /// <summary>
    /// Impassable terrain that cannot be entered by entities.
    /// </summary>
    /// <remarks>
    /// Examples: walls, deep pits, chasms, solid obstacles
    /// </remarks>
    Impassable = 2,

    /// <summary>
    /// Hazardous terrain that deals damage when an entity enters the cell.
    /// </summary>
    /// <remarks>
    /// Examples: fire, acid pools, spike traps, lava
    /// </remarks>
    Hazardous = 3
}
