namespace RuneAndRust.Core.NewGamePlus;

/// <summary>
/// v0.40.1: Information about a character's NG+ status
/// Used for UI display and tier unlock validation
/// </summary>
public class NewGamePlusInfo
{
    /// <summary>Current active NG+ tier (0-5)</summary>
    public int CurrentTier { get; set; }

    /// <summary>Highest tier ever completed (0-5)</summary>
    public int HighestTierCompleted { get; set; }

    /// <summary>Has defeated final boss at least once</summary>
    public bool HasCompletedCampaign { get; set; }

    /// <summary>Total number of NG+ runs completed</summary>
    public int TotalNGPlusCompletions { get; set; }

    /// <summary>List of unlocked tiers (can access)</summary>
    public List<int> AvailableTiers { get; set; } = new();

    /// <summary>Description of what's needed to unlock next tier</summary>
    public string? NextTierRequirement { get; set; }

    /// <summary>Can the character start a new NG+ run?</summary>
    public bool CanStartNewGamePlus => HasCompletedCampaign;

    /// <summary>Can the character access a specific tier?</summary>
    public bool CanAccessTier(int tier)
    {
        if (tier < 1 || tier > 5) return false;
        if (!HasCompletedCampaign) return false;

        // Must complete previous tier first (no skipping)
        return tier <= HighestTierCompleted + 1;
    }
}
