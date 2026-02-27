namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object for NPC definitions loaded from config/npcs.json.
/// </summary>
public record NpcDefinitionDto
{
    /// <summary>Unique NPC identifier matching config key (e.g., "thorvald_guard").</summary>
    public string NpcId { get; init; } = string.Empty;

    /// <summary>Display name (e.g., "Thorvald the Guard").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Narrative description of the NPC.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Greeting text for first encounter.</summary>
    public string InitialGreeting { get; init; } = string.Empty;

    /// <summary>NPC archetype string mapping to NpcArchetype enum.</summary>
    public string Archetype { get; init; } = "Citizen";

    /// <summary>Optional faction affiliation.</summary>
    public string? Faction { get; init; }

    /// <summary>Starting disposition toward player (-100 to 100).</summary>
    public int BaseDisposition { get; init; }

    /// <summary>Root dialogue tree ID from config/dialogues.json.</summary>
    public string? RootDialogueId { get; init; }

    /// <summary>Whether this NPC functions as a merchant.</summary>
    public bool IsMerchant { get; init; }

    /// <summary>Quest definition IDs this NPC can offer.</summary>
    public IReadOnlyList<string> QuestIds { get; init; } = [];

    /// <summary>Optional tags for filtering/search (e.g., "the_roots", "hold_npc").</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Optional keywords for player interaction matching (e.g., "thorvald", "guard").</summary>
    public IReadOnlyList<string> Keywords { get; init; } = [];
}
