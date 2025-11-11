namespace RuneAndRust.Core.Quests;

/// <summary>
/// Represents a quest objective (collect, kill, talk, explore) (v0.8)
/// </summary>
public class QuestObjective
{
    public string Description { get; set; } = string.Empty;
    public ObjectiveType Type { get; set; }
    public string TargetId { get; set; } = string.Empty; // Item ID, Enemy ID, NPC ID, or Room ID
    public int Required { get; set; } = 1;
    public int Current { get; set; } = 0;

    public bool IsComplete => Current >= Required;

    /// <summary>
    /// Gets progress as a string (e.g., "2/5")
    /// </summary>
    public string GetProgress()
    {
        return $"{Current}/{Required}";
    }
}

/// <summary>
/// Types of quest objectives (v0.8)
/// </summary>
public enum ObjectiveType
{
    CollectItem,  // "Find 3 Scrap Metal"
    KillEnemy,    // "Kill 5 Rust-Horrors"
    TalkToNPC,    // "Speak with Kjartan"
    ExploreRoom   // "Discover the Deep Chamber"
}
