namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object for a dialogue node loaded from config/dialogues.json.
/// Represents a single point in a branching conversation tree.
/// </summary>
public record DialogueNodeDto
{
    /// <summary>Unique node identifier within its dialogue tree.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The NPC's dialogue text displayed to the player.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Available response options for the player.</summary>
    public IReadOnlyList<DialogueOptionDto> Options { get; init; } = [];

    /// <summary>Whether this node ends the conversation (no further interaction).</summary>
    public bool EndsConversation { get; init; }
}

/// <summary>
/// A player response option within a dialogue node.
/// </summary>
public record DialogueOptionDto
{
    /// <summary>Text displayed as a selectable choice.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Node ID to navigate to when selected. Null if this choice ends the conversation.</summary>
    public string? NextNodeId { get; init; }

    /// <summary>Optional skill check required to select this option.</summary>
    public SkillCheckDto? SkillCheck { get; init; }

    /// <summary>Optional outcome triggered when this option is selected.</summary>
    public DialogueOutcomeDto? Outcome { get; init; }
}

/// <summary>
/// Defines a skill check requirement on a dialogue option.
/// When present, the player must pass this check to select the option.
/// </summary>
/// <remarks>
/// This is the *requirement* definition from config/dialogues.json, not the result
/// of performing a check. See <see cref="SkillCheckResultDto"/> for check outcomes.
/// </remarks>
public record SkillCheckDto
{
    /// <summary>
    /// The skill or attribute to check (e.g., "Wits", "Might", "Finesse", "Charm").
    /// Maps to a player attribute or derived stat.
    /// </summary>
    public string Skill { get; init; } = string.Empty;

    /// <summary>
    /// The difficulty class the player's roll must meet or exceed.
    /// Higher values require greater skill or luck to pass.
    /// </summary>
    public int DifficultyClass { get; init; }
}

/// <summary>
/// Outcome triggered by a dialogue choice.
/// </summary>
public record DialogueOutcomeDto
{
    /// <summary>
    /// Outcome type: "EndConversation", "QuestGiven", "ReputationChange",
    /// "OpenShop", "GiveItem", "TakeItem", "Information", "InitiateCombat".
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Optional data payload (quest ID, item ID, etc.).</summary>
    public string? Data { get; init; }

    /// <summary>Reputation change amount (positive = friendlier, negative = hostile).</summary>
    public int ReputationChange { get; init; }

    /// <summary>Faction affected by reputation change.</summary>
    public string? AffectedFaction { get; init; }
}

/// <summary>
/// Tracks the state of an active dialogue session.
/// </summary>
public record ActiveDialogueState
{
    /// <summary>The NPC being spoken to.</summary>
    public string NpcDefinitionId { get; init; } = string.Empty;

    /// <summary>The NPC's display name.</summary>
    public string NpcName { get; init; } = string.Empty;

    /// <summary>The root dialogue tree ID.</summary>
    public string RootDialogueId { get; init; } = string.Empty;

    /// <summary>The current node in the dialogue tree.</summary>
    public string CurrentNodeId { get; init; } = string.Empty;

    /// <summary>The current dialogue node data.</summary>
    public DialogueNodeDto CurrentNode { get; init; } = new();
}
