namespace RuneAndRust.Core;

/// <summary>
/// v0.34.2: Represents a recruitable NPC companion
/// Companions are AI-controlled party members with progression and tactical behavior
/// </summary>
public class Companion
{
    // Identity
    public int CompanionID { get; set; }
    public string CompanionName { get; set; } = string.Empty; // ASCII name for code
    public string DisplayName { get; set; } = string.Empty; // Unicode name for UI

    // Class & Role
    public string Archetype { get; set; } = string.Empty; // "Warrior", "Adept", "Mystic"
    public string FactionAffiliation { get; set; } = string.Empty;
    public string CombatRole { get; set; } = string.Empty; // "Tank", "DPS", "Support", "Utility"

    // Background
    public string BackgroundSummary { get; set; } = string.Empty;
    public string PersonalityTraits { get; set; } = string.Empty;

    // Recruitment
    public string RecruitmentLocation { get; set; } = string.Empty;
    public string? RequiredFaction { get; set; }
    public int? RequiredReputationValue { get; set; }

    // Base Attributes (Level 1)
    public Attributes BaseAttributes { get; set; } = new();

    // Current Stats (calculated from base + level + equipment)
    public int Level { get; set; } = 1;
    public int CurrentLegend { get; set; } = 0;
    public int LegendToNextLevel { get; set; } = 100;

    // Combat Stats
    public int CurrentHitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public int Defense { get; set; }
    public int Soak { get; set; }

    // Resources
    public string ResourceType { get; set; } = "Stamina"; // "Stamina" or "Aether Pool"
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentAether { get; set; }
    public int MaxAether { get; set; }

    // Tactical Combat
    public GridPosition? Position { get; set; }
    public string CurrentStance { get; set; } = "aggressive"; // "aggressive", "defensive", "passive"
    public int MovementRange { get; set; } = 3; // Tiles per turn

    // State
    public bool IsIncapacitated { get; set; } = false; // System Crash state
    public bool IsRecruited { get; set; } = false;
    public bool IsInParty { get; set; } = false;

    // Abilities
    public List<CompanionAbility> Abilities { get; set; } = new();
    public Dictionary<string, int> AbilityCooldowns { get; set; } = new(); // Ability name -> turns remaining

    // Equipment
    public Equipment? EquippedWeapon { get; set; }
    public Equipment? EquippedArmor { get; set; }
    public Equipment? EquippedAccessory { get; set; }

    // Personal Quest
    public string? PersonalQuestTitle { get; set; }
    public bool PersonalQuestCompleted { get; set; } = false;

    // Helper methods
    public bool IsAlive => CurrentHitPoints > 0 && !IsIncapacitated;

    public bool CanUseAbility(CompanionAbility ability)
    {
        // Check resource cost
        if (ability.ResourceCost > 0)
        {
            if (ResourceType == "Stamina" && CurrentStamina < ability.ResourceCost)
                return false;
            if (ResourceType == "Aether Pool" && CurrentAether < ability.ResourceCost)
                return false;
        }

        // Check cooldown
        if (AbilityCooldowns.TryGetValue(ability.AbilityName, out int remaining) && remaining > 0)
            return false;

        return true;
    }

    public bool HasActiveBuff(string abilityName)
    {
        // Check if buff is currently active
        // Simplified for v0.34.2 - can expand with status effect system later
        return false;
    }

    public bool AbilityOnCooldown(string abilityName)
    {
        return AbilityCooldowns.TryGetValue(abilityName, out int remaining) && remaining > 0;
    }

    // v0.34.4: Direct command integration
    /// <summary>
    /// Queued action from direct command (command verb)
    /// Overrides AI selection if set
    /// </summary>
    public CompanionAction? QueuedAction { get; set; } = null;
}
