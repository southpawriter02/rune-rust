namespace RuneAndRust.Core.Dialogue;

/// <summary>
/// Represents a player response option in dialogue (v0.8)
/// Can have skill checks and trigger outcomes
/// </summary>
public class DialogueOption
{
    public string Text { get; set; } = string.Empty;
    public SkillCheckRequirement? SkillCheck { get; set; } = null;
    public string? NextNodeId { get; set; } = null;
    public DialogueOutcome? Outcome { get; set; } = null;
    public bool IsVisible { get; set; } = true; // Conditional visibility
}
