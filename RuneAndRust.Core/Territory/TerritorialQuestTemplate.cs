namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.3: Template for generating territorial quests from events
/// Simplified quest definition for event-driven quest creation
/// </summary>
public class TerritorialQuestTemplate
{
    public string QuestName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ObjectiveType { get; set; } = string.Empty; // "KillEnemies", "ReachLocation", "Defend"
    public int ObjectiveCount { get; set; } = 1;
    public string? TargetFaction { get; set; }
    public string? TargetLocation { get; set; }
    public string QuestGiver { get; set; } = "Local Settlement Elder";
    public int RewardGold { get; set; } = 100;
    public int RewardReputation { get; set; } = 10;
    public string? RewardFaction { get; set; }
    public List<string> RewardItems { get; set; } = new();
    public string? FactionPenalty { get; set; }
    public int PenaltyAmount { get; set; } = -10;
    public int TimeLimit { get; set; } = 7; // Days
    public int SectorId { get; set; }
}
