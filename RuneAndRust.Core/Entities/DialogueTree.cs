using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Root container for an NPC's dialogue content.
/// Contains all nodes and tracks metadata for the conversation.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class DialogueTree
{
    /// <summary>
    /// Unique database identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Unique string identifier for lookups (e.g., "npc_old_scavenger").
    /// </summary>
    public string TreeId { get; set; } = string.Empty;

    /// <summary>
    /// The NPC's display name shown in dialogue UI.
    /// </summary>
    public string NpcName { get; set; } = string.Empty;

    /// <summary>
    /// Optional NPC title shown below name (e.g., "Iron-Bane Elder").
    /// </summary>
    public string? NpcTitle { get; set; }

    /// <summary>
    /// ID of the starting node when dialogue begins.
    /// </summary>
    public string RootNodeId { get; set; } = "root";

    /// <summary>
    /// Optional faction association for this NPC.
    /// Used for reputation-based dialogue gating.
    /// </summary>
    public FactionType? AssociatedFaction { get; set; }

    /// <summary>
    /// When this dialogue tree was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this dialogue tree was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: All nodes in this tree.
    /// </summary>
    public ICollection<DialogueNode> Nodes { get; set; } = new List<DialogueNode>();
}
