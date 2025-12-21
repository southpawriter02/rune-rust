namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines what conditions activate a dynamic hazard (v0.3.3a).
/// Each hazard listens for a specific trigger type.
/// </summary>
public enum TriggerType
{
    /// <summary>
    /// Triggered when a combatant enters the room.
    /// Processed by NavigationService on room change.
    /// </summary>
    Movement = 0,

    /// <summary>
    /// Triggered when damage is dealt in the room.
    /// Can be filtered by damage type and threshold.
    /// </summary>
    DamageTaken = 1,

    /// <summary>
    /// Triggered at the start of each combat turn.
    /// Used for periodic environmental effects.
    /// </summary>
    TurnStart = 2,

    /// <summary>
    /// Triggered by explicit player interaction ("activate" command).
    /// Used for levers, switches, and interactive mechanisms.
    /// </summary>
    ManualInteraction = 3
}
