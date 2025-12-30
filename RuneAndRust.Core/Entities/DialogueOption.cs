using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Effects;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// A player response choice within a dialogue node.
/// Contains conditions for availability and effects on selection.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class DialogueOption
{
    /// <summary>
    /// Unique database identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the parent DialogueNode.
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// The text displayed for this choice.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The NodeId to navigate to when selected.
    /// Null for terminal options (ends conversation).
    /// </summary>
    public string? NextNodeId { get; set; }

    /// <summary>
    /// Display order (lower numbers appear first).
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// How to display this option when conditions fail.
    /// </summary>
    public OptionVisibility VisibilityMode { get; set; } = OptionVisibility.ShowLocked;

    /// <summary>
    /// Conditions that must ALL be met to select this option.
    /// Stored as JSONB in PostgreSQL.
    /// </summary>
    public List<DialogueCondition> Conditions { get; set; } = new();

    /// <summary>
    /// Effects to execute when this option is selected.
    /// Stored as JSONB in PostgreSQL.
    /// </summary>
    public List<DialogueEffect> Effects { get; set; } = new();

    /// <summary>
    /// Navigation property to the parent node.
    /// </summary>
    public DialogueNode Node { get; set; } = null!;

    /// <summary>
    /// Whether this option has any conditions.
    /// </summary>
    public bool HasConditions => Conditions.Count > 0;

    /// <summary>
    /// Whether this option has any effects.
    /// </summary>
    public bool HasEffects => Effects.Count > 0;

    /// <summary>
    /// Whether this option ends the conversation.
    /// </summary>
    public bool IsTerminal => NextNodeId == null;
}
