namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Conditions that can trigger ambient events.
/// </summary>
public enum AmbientEventTrigger
{
    /// <summary>Random chance during exploration.</summary>
    Exploration,

    /// <summary>Time-based periodic trigger.</summary>
    Periodic,

    /// <summary>Triggered by combat starting or ending.</summary>
    Combat,

    /// <summary>Triggered by entering a new room.</summary>
    RoomEntry,

    /// <summary>Triggered by player action.</summary>
    PlayerAction
}
