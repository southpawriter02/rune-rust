namespace RuneAndRust.Core.Events;

/// <summary>
/// Event raised when a dialogue session begins.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public record DialogueStartedEvent
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
    /// The NPC's name.
    /// </summary>
    public required string NpcName { get; init; }

    /// <summary>
    /// The character ID participating in the dialogue.
    /// </summary>
    public required Guid CharacterId { get; init; }

    /// <summary>
    /// When the dialogue started.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
