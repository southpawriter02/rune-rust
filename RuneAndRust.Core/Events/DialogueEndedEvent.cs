using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Events;

/// <summary>
/// Event raised when a dialogue session ends.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueEndedEvent
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
    /// The character ID that was in the dialogue.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// The reason the dialogue ended.
    /// </summary>
    public required DialogueEndReason Reason { get; init; }

    /// <summary>
    /// Duration of the dialogue session.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Number of nodes visited during the session.
    /// </summary>
    public int NodesVisited { get; init; }

    /// <summary>
    /// Number of options selected during the session.
    /// </summary>
    public int OptionsSelected { get; init; }

    /// <summary>
    /// When the dialogue ended.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
