namespace RuneAndRust.Core.Enums;

/// <summary>
/// Display status for a specialization node.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public enum NodeStatus
{
    /// <summary>
    /// Prerequisites not met - cannot unlock.
    /// </summary>
    Locked,

    /// <summary>
    /// Prerequisites met but insufficient PP.
    /// </summary>
    InsufficientPP,

    /// <summary>
    /// Can be unlocked now (prerequisites met, PP available).
    /// </summary>
    Available,

    /// <summary>
    /// Already unlocked by character.
    /// </summary>
    Unlocked
}
