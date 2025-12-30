using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Tracks the runtime state of an active dialogue conversation.
/// Stored in GameState while a dialogue is in progress.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public class DialogueSession
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    public Guid SessionId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The dialogue tree being traversed.
    /// </summary>
    public required DialogueTree Tree { get; set; }

    /// <summary>
    /// The currently active node.
    /// </summary>
    public required DialogueNode CurrentNode { get; set; }

    /// <summary>
    /// The character participating in the dialogue.
    /// </summary>
    public required Guid CharacterId { get; set; }

    /// <summary>
    /// Timestamp when the dialogue started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// History of visited node IDs in this session.
    /// </summary>
    public List<string> VisitedNodeIds { get; set; } = new();

    /// <summary>
    /// Count of options selected during this session.
    /// </summary>
    public int OptionsSelectedCount { get; set; }
}
