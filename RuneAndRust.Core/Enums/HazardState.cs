namespace RuneAndRust.Core.Enums;

/// <summary>
/// Lifecycle states for dynamic hazards (v0.3.3a).
/// Controls when a hazard can trigger and its current availability.
/// </summary>
public enum HazardState
{
    /// <summary>
    /// Ready to trigger. Hazard is active and waiting for trigger condition.
    /// </summary>
    Dormant = 0,

    /// <summary>
    /// Currently activating. Transient state during effect execution.
    /// </summary>
    Triggered = 1,

    /// <summary>
    /// Recharging after activation. Cannot trigger until cooldown expires.
    /// </summary>
    Cooldown = 2,

    /// <summary>
    /// Permanently consumed. One-time use hazards enter this state after triggering.
    /// </summary>
    Destroyed = 3
}
