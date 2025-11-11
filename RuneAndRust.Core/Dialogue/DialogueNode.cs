namespace RuneAndRust.Core.Dialogue;

/// <summary>
/// Represents a single node in a dialogue tree (v0.8)
/// NPCs speak, player chooses response options, dialogue branches
/// </summary>
public class DialogueNode
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<DialogueOption> Options { get; set; } = new();
    public bool EndsConversation { get; set; } = false;
}
