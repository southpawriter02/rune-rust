namespace RuneAndRust.Core.Dialogue;

/// <summary>
/// Represents the outcome of a dialogue choice (v0.8)
/// </summary>
public class DialogueOutcome
{
    public OutcomeType Type { get; set; }
    public string Data { get; set; } = string.Empty; // Context-specific data
    public int ReputationChange { get; set; } = 0;
    public FactionType? AffectedFaction { get; set; } = null;
}

/// <summary>
/// Types of dialogue outcomes (v0.8)
/// </summary>
public enum OutcomeType
{
    Information,      // Reveals lore/hints
    QuestGiven,       // Adds quest to log
    QuestComplete,    // Completes quest
    ReputationChange, // Modifies faction standing
    ItemReceived,     // Gain item
    ItemLost,         // Lose item
    InitiateCombat,   // Conversation leads to fight
    EndConversation   // Just ends dialogue
}
