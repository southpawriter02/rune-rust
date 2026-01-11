namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of vertical connections between dungeon levels.
/// </summary>
/// <remarks>
/// Different stair types provide varied descriptions and may have
/// gameplay implications such as requiring climbing checks or being
/// one-way passages.
/// </remarks>
public enum StairType
{
    /// <summary>
    /// Standard stone stairs ascending to a higher level.
    /// </summary>
    StairsUp = 0,

    /// <summary>
    /// Standard stone stairs descending to a lower level.
    /// </summary>
    StairsDown = 1,

    /// <summary>
    /// A vertical wooden or metal ladder.
    /// </summary>
    Ladder = 2,

    /// <summary>
    /// An open vertical shaft, may require climbing skill.
    /// </summary>
    Shaft = 3,

    /// <summary>
    /// A one-way drop to a lower level (cannot ascend).
    /// </summary>
    Pit = 4,

    /// <summary>
    /// A spiral staircase winding up or down.
    /// </summary>
    SpiralStairs = 5,

    /// <summary>
    /// A magical portal or teleporter between levels.
    /// </summary>
    Portal = 6
}
