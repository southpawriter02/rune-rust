namespace RuneAndRust.Application.Services;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for generating ambient events.
/// </summary>
public record AmbientEventContext
{
    /// <summary>
    /// The environment context from v0.0.11a.
    /// </summary>
    public required EnvironmentContext Environment { get; init; }

    /// <summary>
    /// The sensory context from v0.0.11c.
    /// </summary>
    public SensoryContext? Sensory { get; init; }

    /// <summary>
    /// The trigger condition for this event check.
    /// </summary>
    public AmbientEventTrigger Trigger { get; init; }

    /// <summary>
    /// Whether combat is currently active.
    /// </summary>
    public bool InCombat { get; init; }

    /// <summary>
    /// Time since last ambient event (for cooldown).
    /// </summary>
    public TimeSpan TimeSinceLastEvent { get; init; }

    /// <summary>
    /// Number of turns/actions since entering this room.
    /// </summary>
    public int TurnsInRoom { get; init; }
}
