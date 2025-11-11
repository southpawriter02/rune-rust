namespace RuneAndRust.Core;

/// <summary>
/// Manages faction reputation tracking and calculations (v0.8)
/// </summary>
public class FactionReputationSystem
{
    public Dictionary<FactionType, int> Reputations { get; set; } = new()
    {
        { FactionType.MidgardCombine, 0 },
        { FactionType.RustClans, 0 },
        { FactionType.Independents, 0 }
    };

    /// <summary>
    /// Modifies reputation with a faction and applies rival penalties
    /// </summary>
    public void ModifyReputation(FactionType faction, int change, string reason, List<string>? log = null)
    {
        if (!Reputations.ContainsKey(faction))
            Reputations[faction] = 0;

        int oldRep = Reputations[faction];
        Reputations[faction] += change;
        Reputations[faction] = Math.Clamp(Reputations[faction], -100, 100);

        // Log reputation change if log provided
        if (log != null && change != 0)
        {
            string changeStr = change > 0 ? $"+{change}" : change.ToString();
            log.Add($"[Reputation] {faction}: {changeStr} ({reason}) - Now {GetReputationTier(faction)}");
        }

        // Apply rival faction penalties
        ApplyRivalPenalties(faction, change, log);
    }

    /// <summary>
    /// Gets the reputation tier for a faction
    /// </summary>
    public ReputationTier GetReputationTier(FactionType faction)
    {
        int rep = Reputations.GetValueOrDefault(faction, 0);

        if (rep >= 75) return ReputationTier.Revered;
        if (rep >= 50) return ReputationTier.Honored;
        if (rep >= 25) return ReputationTier.Friendly;
        if (rep >= 1) return ReputationTier.Liked;
        if (rep >= -24) return ReputationTier.Neutral;
        if (rep >= -49) return ReputationTier.Disliked;
        if (rep >= -74) return ReputationTier.Hated;
        return ReputationTier.Despised;
    }

    /// <summary>
    /// Gets the numeric reputation value for a faction
    /// </summary>
    public int GetReputation(FactionType faction)
    {
        return Reputations.GetValueOrDefault(faction, 0);
    }

    /// <summary>
    /// Applies penalties to rival factions when gaining reputation
    /// </summary>
    private void ApplyRivalPenalties(FactionType faction, int change, List<string>? log = null)
    {
        // Only apply penalties when gaining positive reputation
        if (change <= 0) return;

        // Calculate rival penalty (half of the gain, minimum 1)
        int penalty = Math.Max(1, change / 2);

        // Midgard Combine vs Rust-Clans rivalry
        if (faction == FactionType.MidgardCombine)
        {
            int oldRep = Reputations[FactionType.RustClans];
            Reputations[FactionType.RustClans] -= penalty;
            Reputations[FactionType.RustClans] = Math.Clamp(Reputations[FactionType.RustClans], -100, 100);

            if (log != null)
            {
                log.Add($"[Reputation] RustClans: -{penalty} (rival penalty) - Now {GetReputationTier(FactionType.RustClans)}");
            }
        }
        else if (faction == FactionType.RustClans)
        {
            int oldRep = Reputations[FactionType.MidgardCombine];
            Reputations[FactionType.MidgardCombine] -= penalty;
            Reputations[FactionType.MidgardCombine] = Math.Clamp(Reputations[FactionType.MidgardCombine], -100, 100);

            if (log != null)
            {
                log.Add($"[Reputation] MidgardCombine: -{penalty} (rival penalty) - Now {GetReputationTier(FactionType.MidgardCombine)}");
            }
        }
        // Independents have no rivals
    }
}

/// <summary>
/// Reputation tiers from -100 to +100 (v0.8)
/// </summary>
public enum ReputationTier
{
    Despised,   // -100 to -75
    Hated,      // -74 to -50
    Disliked,   // -49 to -25
    Neutral,    // -24 to 0
    Liked,      // 1 to 24
    Friendly,   // 25 to 49
    Honored,    // 50 to 74
    Revered     // 75 to 100
}
