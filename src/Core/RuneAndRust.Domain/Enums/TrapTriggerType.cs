namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Describes how a runic trap is triggered when a creature interacts with its space.
/// </summary>
/// <remarks>
/// <para>Used by <see cref="ValueObjects.RunicTrap"/> to define triggering behavior.</para>
/// <para>Currently only <see cref="Movement"/> is active; other types are reserved for
/// future expansion in v0.20.2c+ enhancements.</para>
/// </remarks>
public enum TrapTriggerType
{
    /// <summary>
    /// Triggers when a creature enters the trap's space.
    /// Standard trigger type for Runic Trap ability (Tier 2).
    /// </summary>
    Movement = 1,

    /// <summary>
    /// Triggers when a creature moves within an adjacent space.
    /// Reserved for future enhancement.
    /// </summary>
    Proximity = 2,

    /// <summary>
    /// Triggers after a set delay from placement.
    /// Reserved for future enhancement.
    /// </summary>
    Timed = 3
}
