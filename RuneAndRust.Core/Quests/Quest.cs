namespace RuneAndRust.Core.Quests;

/// <summary>
/// Represents a quest given by an NPC (v0.8)
/// Simple fetch/kill/talk quests - foundation for v1.0 complex quest system
/// </summary>
public class Quest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GiverNpcId { get; set; } = string.Empty;
    public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
    public List<QuestObjective> Objectives { get; set; } = new();
    public QuestReward? Reward { get; set; }

    /// <summary>
    /// Checks if all objectives are complete
    /// </summary>
    public bool IsComplete()
    {
        return Objectives.All(o => o.IsComplete);
    }
}

/// <summary>
/// Quest status (v0.8 uses NotStarted, Active, Completed)
/// </summary>
public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed  // Not used in v0.8, reserved for v1.0
}
