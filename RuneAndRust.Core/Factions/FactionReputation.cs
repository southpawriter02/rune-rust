namespace RuneAndRust.Core.Factions;

/// <summary>
/// v0.33.1: Represents a character's reputation with a specific faction
/// Corresponds to database Characters_FactionReputations table
/// </summary>
public class FactionReputation
{
    public int ReputationId { get; set; }
    public int CharacterId { get; set; }
    public int FactionId { get; set; }
    public int ReputationValue { get; set; }
    public FactionReputationTier ReputationTier { get; set; }
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Initializes a new faction reputation with Neutral standing
    /// </summary>
    public FactionReputation()
    {
        ReputationValue = 0;
        ReputationTier = FactionReputationTier.Neutral;
        LastModified = DateTime.Now;
    }

    /// <summary>
    /// Creates a faction reputation for a specific character and faction
    /// </summary>
    public FactionReputation(int characterId, int factionId) : this()
    {
        CharacterId = characterId;
        FactionId = factionId;
    }
}

/// <summary>
/// v0.33: Reputation tiers from -100 to +100
/// Matches database CHECK constraint
/// </summary>
public enum FactionReputationTier
{
    Hated,      // -100 to -76
    Hostile,    // -75 to -26
    Neutral,    // -25 to +24
    Friendly,   // +25 to +49
    Allied,     // +50 to +74
    Exalted     // +75 to +100
}
