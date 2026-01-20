namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents an ambient atmospheric event.
/// </summary>
/// <remarks>
/// Ambient events add life and atmosphere to the environment through
/// random occurrences that match the current biome and conditions.
/// </remarks>
public readonly record struct AmbientEvent
{
    /// <summary>
    /// The category of this event.
    /// </summary>
    public AmbientEventType EventType { get; init; }

    /// <summary>
    /// The event description text.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// The intensity level (0.0 to 1.0) for UI emphasis.
    /// </summary>
    /// <remarks>
    /// 0.0 = subtle, background event
    /// 0.5 = noticeable event
    /// 1.0 = attention-grabbing event
    /// </remarks>
    public float Intensity { get; init; }

    /// <summary>
    /// Whether this event should interrupt the current action display.
    /// </summary>
    public bool IsInterruptive { get; init; }

    /// <summary>
    /// Creates an empty/no event result.
    /// </summary>
    public static AmbientEvent None => new()
    {
        EventType = AmbientEventType.Environmental,
        Description = string.Empty,
        Intensity = 0f,
        IsInterruptive = false
    };

    /// <summary>
    /// Returns true if this represents an actual event.
    /// </summary>
    public bool HasEvent => !string.IsNullOrEmpty(Description);
}
