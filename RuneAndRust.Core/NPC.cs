namespace RuneAndRust.Core;

/// <summary>
/// Represents a non-player character in the game world (v0.8)
/// NPCs can provide quests, dialogue, and faction interaction
/// </summary>
public class NPC
{
    // Identity
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InitialGreeting { get; set; } = string.Empty;

    // v0.38.11: NPC Flavor Text Classification
    public string Archetype { get; set; } = "Citizen"; // Dvergr, Seidkona, Bandit, Raider, Merchant, Guard, Citizen, Forlorn
    public string Subtype { get; set; } = "Laborer"; // Varies by archetype

    // Placement
    public int RoomId { get; set; }
    public bool IsHostile { get; set; } = false;

    // Faction & Disposition
    public FactionType Faction { get; set; }
    public int BaseDisposition { get; set; } = 0; // -100 to +100
    public int CurrentDisposition { get; set; } = 0; // Modified by reputation

    // Dialogue
    public string RootDialogueId { get; set; } = string.Empty; // Entry point for conversation
    public List<string> EncounteredTopics { get; set; } = new(); // Track what player asked

    // State
    public bool HasBeenMet { get; set; } = false;
    public bool IsAlive { get; set; } = true;
    public Dictionary<string, bool> QuestFlags { get; set; } = new(); // Track quest state

    /// <summary>
    /// Gets the disposition tier based on current disposition value
    /// </summary>
    public string GetDispositionTier()
    {
        return CurrentDisposition switch
        {
            >= 50 => "Friendly",
            >= 10 => "Neutral-Positive",
            >= -9 => "Neutral",
            >= -49 => "Unfriendly",
            _ => "Hostile"
        };
    }

    /// <summary>
    /// Updates the current disposition based on base disposition and faction reputation
    /// </summary>
    public void UpdateDisposition(int factionReputation)
    {
        CurrentDisposition = BaseDisposition + (int)(factionReputation * 0.5);
        CurrentDisposition = Math.Clamp(CurrentDisposition, -100, 100);
    }
}
