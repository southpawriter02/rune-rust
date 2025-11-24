namespace RuneAndRust.Core;

/// <summary>
/// v0.41: Alternative starting scenario
/// Modifies character creation and early game experience
/// </summary>
public class AlternativeStart
{
    public string StartId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FlavorText { get; set; } = string.Empty;

    // Unlock requirements
    public string RequirementDescription { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = false;

    // Starting modifications
    public int StartingLevel { get; set; } = 1;
    public List<string> StartingEquipment { get; set; } = new();
    public List<string> UnlockedSpecializations { get; set; } = new();
    public int StartingLegend { get; set; } = 0;
    public Dictionary<string, int> StartingResources { get; set; } = new(); // Resource type -> amount
    public int? StartingSectorId { get; set; } = null; // Skip to specific sector
    public List<string> CompletedQuests { get; set; } = new(); // Quest IDs to mark as completed
    public bool SkipTutorial { get; set; } = false;

    // Difficulty modifiers (optional)
    public bool HardModeEnabled { get; set; } = false;
    public bool PermadeathEnabled { get; set; } = false;
    public float RewardMultiplier { get; set; } = 1.0f;
    public bool TimerVisible { get; set; } = false;
}

/// <summary>
/// v0.41: Alternative start unlock progress
/// </summary>
public class AlternativeStartProgress
{
    public int ProgressId { get; set; }
    public int AccountId { get; set; }
    public string StartId { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }
}
