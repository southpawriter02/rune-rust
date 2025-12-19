namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the threat level of an area.
/// Influences description details and encounter spawning.
/// </summary>
public enum DangerLevel
{
    /// <summary>
    /// No active threats. Relatively calm environment.
    /// </summary>
    Safe = 0,

    /// <summary>
    /// Environmental hazards present. Structural instability or traps.
    /// </summary>
    Unstable = 1,

    /// <summary>
    /// Active enemies nearby. Combat may be imminent.
    /// </summary>
    Hostile = 2,

    /// <summary>
    /// Immediate danger. Life-threatening situation.
    /// </summary>
    Lethal = 3
}
