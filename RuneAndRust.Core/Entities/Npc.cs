using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a non-player character that can be interacted with.
/// NPCs may have dialogue trees, faction affiliations, and room assignments.
/// </summary>
/// <remarks>See: v0.4.2e (The Archive) for NPC seeding and dialogue integration.</remarks>
public class Npc
{
    /// <summary>
    /// Unique identifier for this NPC instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the NPC (e.g., "Old Scavenger", "Kjartan").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional title shown below name (e.g., "Iron-Bane Elder", "Rust-Smith").
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Description of the NPC's appearance and demeanor.
    /// Must be Domain 4 compliant (no precision measurements).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The dialogue tree ID for this NPC (v0.4.2e).
    /// References a DialogueTree.TreeId for conversation lookup.
    /// Null if NPC has no dialogue.
    /// </summary>
    public string? DialogueTreeId { get; set; }

    /// <summary>
    /// The faction this NPC belongs to, if any.
    /// </summary>
    public FactionType? Faction { get; set; }

    /// <summary>
    /// Whether this NPC is hostile by default.
    /// Hostile NPCs cannot be talked to.
    /// </summary>
    public bool IsHostile { get; set; }

    /// <summary>
    /// Whether this NPC can be talked to.
    /// True if has a dialogue tree and is not hostile.
    /// </summary>
    public bool CanTalk => !string.IsNullOrEmpty(DialogueTreeId) && !IsHostile;

    /// <summary>
    /// The room this NPC is currently in.
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// Navigation property to the room.
    /// </summary>
    public Room? Room { get; set; }

    /// <summary>
    /// When this NPC record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Combined display string for the NPC (e.g., "Old Scavenger, Iron-Bane Elder").
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(Title)
        ? Name
        : $"{Name}, {Title}";
}
