namespace RuneAndRust.Core;

/// <summary>
/// v0.19: Data-driven ability metadata
/// Extends the original Ability class with full database metadata
/// </summary>
public class AbilityData
{
    public int AbilityID { get; set; }
    public int SpecializationID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TierLevel { get; set; } = 1; // 1, 2, 3, or 4 (Capstone)
    public int PPCost { get; set; } = 3; // PP cost to learn this ability
    public AbilityPrerequisites Prerequisites { get; set; } = new();

    // Ability mechanics
    public string AbilityType { get; set; } = "Active"; // "Active", "Passive", "Reaction"
    public string ActionType { get; set; } = "Standard Action"; // "Standard Action", "Bonus Action", "Free Action", "Performance"
    public string TargetType { get; set; } = "Single Enemy"; // "Single Enemy", "All Enemies", "Self", "Single Ally", "All Allies", etc.
    public AbilityResourceCost ResourceCost { get; set; } = new();
    public string AttributeUsed { get; set; } = string.Empty; // For roll checks
    public int BonusDice { get; set; } = 0;
    public int SuccessThreshold { get; set; } = 2;

    // Effects
    public string MechanicalSummary { get; set; } = string.Empty; // Short mechanical description
    public int DamageDice { get; set; } = 0; // Number of d6 damage dice
    public string DamageType { get; set; } = "Physical"; // "Physical", "Fire", "Lightning", "Poison", etc.
    public bool IgnoresArmor { get; set; } = false;
    public int HealingDice { get; set; } = 0; // Number of d6 healing dice
    public List<string> StatusEffectsApplied { get; set; } = new(); // [Vulnerable], [Analyzed], [Feared], etc.
    public List<string> StatusEffectsRemoved { get; set; } = new(); // [Bleeding], [Poisoned], etc.

    // Progression
    public int MaxRank { get; set; } = 3; // Most abilities: 1-3 ranks
    public int CostToRank2 { get; set; } = 5; // PP cost to upgrade to Rank 2
    public int CostToRank3 { get; set; } = 0; // Usually locked (0 means not available)

    // Cooldowns
    public int CooldownTurns { get; set; } = 0; // 0 = no cooldown
    public string CooldownType { get; set; } = "None"; // "None", "Per Combat", "Per Expedition", "Per Day"

    // Metadata
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = string.Empty; // Developer notes
}

/// <summary>
/// Prerequisites for learning an ability
/// </summary>
public class AbilityPrerequisites
{
    public List<int> RequiredAbilityIDs { get; set; } = new(); // Must have learned these abilities first
    public int RequiredPPInTree { get; set; } = 0; // Must have spent X PP in this spec's tree
    public int RequiredTier { get; set; } = 1; // Tier unlocks at PP thresholds (Tier 2 @ 8 PP, Tier 3 @ 16 PP)

    /// <summary>
    /// Check if character meets prerequisites to learn this ability
    /// </summary>
    public bool IsSatisfiedBy(PlayerCharacter character, int specializationID, List<int> learnedAbilityIDs, int ppSpentInTree)
    {
        // Check PP in tree requirement
        if (ppSpentInTree < RequiredPPInTree)
            return false;

        // Check required abilities
        foreach (var requiredAbilityID in RequiredAbilityIDs)
        {
            if (!learnedAbilityIDs.Contains(requiredAbilityID))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get human-readable description of unmet prerequisites
    /// </summary>
    public string GetUnmetPrerequisitesMessage(int ppSpentInTree, List<int> learnedAbilityIDs, Dictionary<int, string> abilityNames)
    {
        var unmet = new List<string>();

        if (ppSpentInTree < RequiredPPInTree)
            unmet.Add($"Requires {RequiredPPInTree} PP spent in this specialization (you have {ppSpentInTree})");

        foreach (var requiredAbilityID in RequiredAbilityIDs)
        {
            if (!learnedAbilityIDs.Contains(requiredAbilityID))
            {
                var abilityName = abilityNames.GetValueOrDefault(requiredAbilityID, $"Ability #{requiredAbilityID}");
                unmet.Add($"Requires: {abilityName}");
            }
        }

        return unmet.Any() ? string.Join(", ", unmet) : "All prerequisites met";
    }
}

/// <summary>
/// Resource costs for using an ability
/// </summary>
public class AbilityResourceCost
{
    public int Stamina { get; set; } = 0;
    public int Stress { get; set; } = 0; // Psychic Stress cost
    public int Corruption { get; set; } = 0; // Permanent Corruption cost (Heretical abilities)
    public int HP { get; set; } = 0; // Self-damage cost (rare)
    public string SpecialResource { get; set; } = string.Empty; // "Fury", "Momentum", etc. (future)

    public bool IsAffordableBy(PlayerCharacter character)
    {
        if (character.Stamina < Stamina) return false;
        if (character.PsychicStress + Stress > 100) return false; // Would exceed max stress
        if (character.HP <= HP) return false; // Would kill character
        // Corruption is always payable (Heretical choices)
        return true;
    }

    public string GetUnaffordableMessage(PlayerCharacter character)
    {
        var reasons = new List<string>();

        if (character.Stamina < Stamina)
            reasons.Add($"Requires {Stamina} Stamina (you have {character.Stamina})");

        if (character.PsychicStress + Stress > 100)
            reasons.Add($"Would exceed max Stress (you have {character.PsychicStress}/100, cost: {Stress})");

        if (character.HP <= HP)
            reasons.Add($"HP cost would kill you (you have {character.HP} HP, cost: {HP})");

        return reasons.Any() ? string.Join(", ", reasons) : "Affordable";
    }
}

/// <summary>
/// Tracking table: Which abilities has this character learned?
/// </summary>
public class CharacterAbility
{
    public int CharacterID { get; set; }
    public int AbilityID { get; set; }
    public DateTime UnlockedAt { get; set; }
    public int CurrentRank { get; set; } = 1; // 1, 2, or 3
    public int TimesUsed { get; set; } = 0; // For analytics/achievements
}
