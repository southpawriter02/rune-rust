namespace RuneAndRust.Core.Quests;

/// <summary>
/// Represents a quest given by an NPC (v0.8, enhanced in v0.14)
/// Simple fetch/kill/talk quests - foundation for v1.0 complex quest system
/// </summary>
public class Quest
{
    // Identity
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Quest Giver
    public string GiverNpcId { get; set; } = string.Empty;
    public string GiverNpcName { get; set; } = string.Empty; // v0.14: Display name

    // State & Timestamps
    public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
    public DateTime? AcceptedAt { get; set; } // v0.14: When quest was accepted
    public DateTime? CompletedAt { get; set; } // v0.14: When quest was completed

    // Requirements (v0.14)
    public int MinimumLegend { get; set; } = 0; // Minimum Legend level required
    public List<string> PrerequisiteQuests { get; set; } = new(); // Must complete these first

    // Objectives
    public List<QuestObjective> Objectives { get; set; } = new();

    // Rewards
    public QuestReward? Reward { get; set; }

    // Generation Requirements (v0.14) - For quests that require specific dungeons
    public QuestGenerationRequirements? GenerationReqs { get; set; } = null;

    // Classification (v0.14)
    public QuestType Type { get; set; } = QuestType.Side;
    public QuestCategory Category { get; set; } = QuestCategory.Retrieval;

    // Metadata
    public int EstimatedDuration { get; set; } = 15; // Minutes

    /// <summary>
    /// Checks if all objectives are complete
    /// </summary>
    public bool IsComplete()
    {
        return Objectives.All(o => o.IsComplete);
    }

    /// <summary>
    /// Checks if all required objectives are complete
    /// </summary>
    public bool AreMandatoryObjectivesComplete()
    {
        // For v0.14, all objectives are mandatory (optional objectives in v1.0)
        return IsComplete();
    }
}

/// <summary>
/// Quest status (v0.8, enhanced in v0.14)
/// </summary>
public enum QuestStatus
{
    NotStarted,    // Quest available but not accepted
    Available,     // v0.14: Quest offered to player
    Active,        // Quest accepted and in progress
    Complete,      // v0.14: Objectives done, not turned in
    Completed,     // Quest turned in, rewards claimed (legacy name)
    TurnedIn,      // v0.14: Same as Completed (clearer naming)
    Failed,        // Quest failed (not used in v0.14)
    Abandoned      // v0.14: Player abandoned quest
}

/// <summary>
/// v0.14: Quest type classification
/// </summary>
public enum QuestType
{
    Main,          // Critical path quest
    Side,          // Optional side quest
    Dynamic,       // Procedurally generated quest
    Repeatable     // Can be done multiple times
}

/// <summary>
/// v0.14: Quest category for classification
/// </summary>
public enum QuestCategory
{
    Combat,        // Defeat enemies
    Exploration,   // Discover locations
    Retrieval,     // Collect/retrieve items
    Delivery,      // Transport items
    Investigation, // Examine/interact with objects
    Dialogue       // Talk to NPCs
}
