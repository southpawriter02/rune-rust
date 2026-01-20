namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the type of branch for a dungeon path.
/// </summary>
public enum BranchType
{
    /// <summary>
    /// No exit in this direction.
    /// </summary>
    None = 0,

    /// <summary>
    /// Main path continuation (primary route through dungeon).
    /// </summary>
    MainPath = 1,

    /// <summary>
    /// Side path (secondary exploration route).
    /// </summary>
    SidePath = 2,

    /// <summary>
    /// Dead end (terminates after 1-3 rooms with rewards).
    /// </summary>
    DeadEnd = 3,

    /// <summary>
    /// Loop connection (connects to an existing room).
    /// </summary>
    Loop = 4
}
