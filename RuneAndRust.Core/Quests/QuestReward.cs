namespace RuneAndRust.Core.Quests;

/// <summary>
/// Represents rewards for completing a quest (v0.8, v0.9)
/// </summary>
public class QuestReward
{
    public int Experience { get; set; } = 0;
    public List<string> ItemIds { get; set; } = new(); // Item IDs to grant
    public int ReputationChange { get; set; } = 0;
    public FactionType? Faction { get; set; } = null;
    public int Currency { get; set; } = 0; // Dvergr Cogs (v0.9)
}
