namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Duration of a situational modifier.
/// </summary>
/// <remarks>
/// Used by <see cref="ValueObjects.SituationalModifier"/> to track
/// how long a modifier persists.
/// </remarks>
public enum ModifierDuration
{
    /// <summary>
    /// Applies to a single check only.
    /// </summary>
    Instant = 0,

    /// <summary>
    /// Applies for one combat round.
    /// </summary>
    Round = 1,

    /// <summary>
    /// Applies for the current scene or encounter.
    /// </summary>
    Scene = 2,

    /// <summary>
    /// Applies until explicitly removed.
    /// </summary>
    Persistent = 3
}
