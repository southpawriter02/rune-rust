namespace RuneAndRust.Core;

/// <summary>
/// v0.19: Data-driven specialization metadata
/// Replaces hard-coded enum-based specializations with extensible database model
/// </summary>
public class SpecializationData
{
    public int SpecializationID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ArchetypeID { get; set; } // 1 = Warrior, 2 = Adept, etc.
    public string PathType { get; set; } = "Coherent"; // "Coherent" or "Heretical"
    public string MechanicalRole { get; set; } = string.Empty; // "Tank", "DPS", "Support", "Controller"
    public string PrimaryAttribute { get; set; } = string.Empty; // "MIGHT", "WITS", "WILL", etc.
    public string SecondaryAttribute { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public UnlockRequirements UnlockRequirements { get; set; } = new();
    public string ResourceSystem { get; set; } = "Stamina"; // "Stamina", "Aether Pool", "Fury", "Momentum", etc.
    public string TraumaRisk { get; set; } = "Low"; // "None", "Low", "Medium", "High", "Extreme"
    public string IconEmoji { get; set; } = "⚔️";
    public bool IsActive { get; set; } = true;
    public int PPCostToUnlock { get; set; } = 3; // v0.18: Standard unlock cost
}

/// <summary>
/// Unlock requirements for specializations
/// </summary>
public class UnlockRequirements
{
    public int MinLegend { get; set; } = 0; // Minimum Legend tier required
    public string? RequiredQuestID { get; set; } = null; // Optional quest prerequisite
    public int MinCorruption { get; set; } = 0; // For Heretical specializations
    public int MaxCorruption { get; set; } = 100; // Some Coherent specs may be locked if too corrupted

    /// <summary>
    /// Check if character meets requirements to unlock this specialization
    /// </summary>
    public bool IsSatisfiedBy(PlayerCharacter character)
    {
        // Check Legend requirement
        if (character.CurrentLegend < MinLegend)
            return false;

        // Check Corruption requirements
        if (character.Corruption < MinCorruption || character.Corruption > MaxCorruption)
            return false;

        // Check quest requirement
        if (!string.IsNullOrEmpty(RequiredQuestID))
        {
            if (!character.CompletedQuests.Any(q => q.QuestID.Equals(RequiredQuestID, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get human-readable description of unmet requirements
    /// </summary>
    public string GetUnmetRequirementsMessage(PlayerCharacter character)
    {
        var unmet = new List<string>();

        if (character.CurrentLegend < MinLegend)
            unmet.Add($"Requires Legend {MinLegend}+ (you have {character.CurrentLegend})");

        if (character.Corruption < MinCorruption)
            unmet.Add($"Requires {MinCorruption}+ Corruption (you have {character.Corruption})");

        if (character.Corruption > MaxCorruption)
            unmet.Add($"Requires Corruption below {MaxCorruption} (you have {character.Corruption})");

        if (!string.IsNullOrEmpty(RequiredQuestID))
        {
            if (!character.CompletedQuests.Any(q => q.QuestID.Equals(RequiredQuestID, StringComparison.OrdinalIgnoreCase)))
                unmet.Add($"Requires completion of quest: {RequiredQuestID}");
        }

        return unmet.Any() ? string.Join(", ", unmet) : "All requirements met";
    }
}

/// <summary>
/// Tracking table: Which specializations has this character unlocked?
/// </summary>
public class CharacterSpecialization
{
    public int CharacterID { get; set; }
    public int SpecializationID { get; set; }
    public DateTime UnlockedAt { get; set; }
    public bool IsActive { get; set; } = true; // Allow multi-classing in future
    public int PPSpentInTree { get; set; } = 0; // Track PP investment in this spec
}
