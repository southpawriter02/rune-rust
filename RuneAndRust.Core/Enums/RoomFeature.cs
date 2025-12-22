namespace RuneAndRust.Core.Enums;

/// <summary>
/// Features that can be present in a room, enabling specific gameplay mechanics.
/// </summary>
public enum RoomFeature
{
    /// <summary>
    /// No special feature.
    /// </summary>
    None = 0,

    /// <summary>
    /// A Runic Anchor that allows Sanctuary rest with full recovery.
    /// </summary>
    RunicAnchor = 1,

    /// <summary>
    /// A workbench that allows crafting activities.
    /// </summary>
    Workbench = 2,

    /// <summary>
    /// An alchemy table that allows potion brewing.
    /// </summary>
    AlchemyTable = 3,

    /// <summary>
    /// Stairs leading to a higher Z-level (v0.3.5b).
    /// </summary>
    StairsUp = 4,

    /// <summary>
    /// Stairs leading to a lower Z-level (v0.3.5b).
    /// </summary>
    StairsDown = 5,

    /// <summary>
    /// A boss encounter room (v0.3.5b).
    /// </summary>
    BossLair = 6,

    /// <summary>
    /// A safe zone with vendors or services (v0.3.5b).
    /// </summary>
    Settlement = 7
}
