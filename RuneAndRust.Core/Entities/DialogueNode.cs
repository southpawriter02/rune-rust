namespace RuneAndRust.Core.Entities;

/// <summary>
/// A single node in a dialogue tree, representing one "screen" of conversation.
/// Contains the NPC's text and available player responses.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class DialogueNode
{
    /// <summary>
    /// Unique database identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the parent DialogueTree.
    /// </summary>
    public Guid TreeId { get; set; }

    /// <summary>
    /// String identifier within the tree (e.g., "greeting", "quest_offer").
    /// Must be unique within the tree.
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the speaker for this node (usually NPC name, but can be others).
    /// </summary>
    public string SpeakerName { get; set; } = string.Empty;

    /// <summary>
    /// The dialogue text displayed to the player.
    /// Supports basic markdown formatting.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Whether this node ends the conversation.
    /// Terminal nodes have no options or only "goodbye" options.
    /// </summary>
    public bool IsTerminal { get; set; } = false;

    /// <summary>
    /// Navigation property to the parent tree.
    /// </summary>
    public DialogueTree Tree { get; set; } = null!;

    /// <summary>
    /// Navigation property: Available player responses.
    /// </summary>
    public ICollection<DialogueOption> Options { get; set; } = new List<DialogueOption>();
}
