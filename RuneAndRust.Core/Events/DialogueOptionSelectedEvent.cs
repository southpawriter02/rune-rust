namespace RuneAndRust.Core.Events;

/// <summary>
/// Event raised when a player selects a dialogue option.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueOptionSelectedEvent
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// The dialogue tree ID.
    /// </summary>
    public required string TreeId { get; init; }

    /// <summary>
    /// The node ID where the option was selected.
    /// </summary>
    public required string FromNodeId { get; init; }

    /// <summary>
    /// The option ID that was selected.
    /// </summary>
    public required string OptionId { get; init; }

    /// <summary>
    /// The node ID navigated to (null if terminal).
    /// </summary>
    public string? ToNodeId { get; init; }

    /// <summary>
    /// The character ID that made the selection.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// Number of effects that were executed.
    /// </summary>
    public int EffectsExecuted { get; init; }

    /// <summary>
    /// When the option was selected.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
