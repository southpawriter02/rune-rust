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
    AlchemyTable = 3
}
