using RuneAndRust.Core.Quests;

namespace RuneAndRust.Core;

public class PlayerCharacter
{
    public string Name { get; set; } = "Survivor";
    public CharacterClass Class { get; set; }
    public Specialization Specialization { get; set; } = Specialization.None; // v0.7: Unlocked with 10 PP
    public Attributes Attributes { get; set; } = new();

    // v0.7.1: Archetype System
    public Archetype? Archetype { get; set; } // Formal archetype reference

    // v0.7.1: Stance System (Warriors)
    public Stance ActiveStance { get; set; } = Stance.CreateBalancedStance();

    // Progression (Aethelgard Saga System)
    public int CurrentMilestone { get; set; } = 0;
    public int CurrentLegend { get; set; } = 0;
    public int LegendToNextMilestone { get; set; } = 100;
    public int ProgressionPoints { get; set; } = 2; // Start with 2 PP

    // Resources
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
    public int AP { get; set; }

    // Economy (v0.9)
    public int Currency { get; set; } = 0; // Dvergr Cogs (⚙)

    // Trauma Economy (v0.5)
    public int PsychicStress { get; set; } = 0; // 0-100, recoverable at Sanctuary
    public int Corruption { get; set; } = 0; // 0-100, permanent
    public int RoomsExploredSinceRest { get; set; } = 0; // For rest cooldown tracking

    // Trauma Economy (v0.15) - Permanent psychological wounds
    public List<Trauma> Traumas { get; set; } = new(); // Permanent Traumas from Breaking Points

    // Combat (v0.1 - deprecated in favor of equipment system)
    public string WeaponName { get; set; } = string.Empty;
    public string WeaponAttribute { get; set; } = string.Empty; // Which attribute the weapon uses
    public int BaseDamage { get; set; } = 1; // Number of d6s

    // Equipment System (v0.3)
    public Equipment? EquippedWeapon { get; set; }
    public Equipment? EquippedArmor { get; set; }
    public List<Equipment> Inventory { get; set; } = new(); // Up to 5 items
    public int MaxInventorySize { get; set; } = 5;

    // Consumables & Crafting (v0.7)
    public List<Consumable> Consumables { get; set; } = new(); // Potions, medicines, etc.
    public Dictionary<ComponentType, int> CraftingComponents { get; set; } = new(); // Crafting materials
    public int MaxConsumables { get; set; } = 10; // Max consumable items

    // Faction & Quests (v0.8)
    public FactionReputationSystem FactionReputations { get; set; } = new();
    public List<Quest> ActiveQuests { get; set; } = new();
    public List<Quest> CompletedQuests { get; set; } = new();

    // Abilities
    public List<Ability> Abilities { get; set; } = new();

    // Defense
    public int DefenseBonus { get; set; } = 0; // Temporary defense from Defend action
    public int DefenseTurnsRemaining { get; set; } = 0;

    // Status Effects (v0.2)
    public int BattleRageTurnsRemaining { get; set; } = 0; // Warrior Lv5 ability
    public int ShieldAbsorptionRemaining { get; set; } = 0; // Mystic Lv3 ability

    // v0.7: Adept Specialization Status Effects
    public int VulnerableTurnsRemaining { get; set; } = 0; // Bone-Setter: Anatomical Insight - Take +25% damage
    public int AnalyzedTurnsRemaining { get; set; } = 0; // Jötun-Reader: Exploit Design Flaw - Allies get +2 Accuracy
    public int SeizedTurnsRemaining { get; set; } = 0; // Jötun-Reader: Architect of the Silence - Cannot take actions
    public bool IsPerforming { get; set; } = false; // Skald: Performance channeling active
    public int PerformingTurnsRemaining { get; set; } = 0; // Skald: Performance duration
    public string? CurrentPerformance { get; set; } = null; // Skald: Which performance is active
    public int InspiredTurnsRemaining { get; set; } = 0; // Skald: Saga of the Einherjar - +3 damage dice
    public int SilencedTurnsRemaining { get; set; } = 0; // Skald: Song of Silence - Cannot cast/perform
    public int TempHP { get; set; } = 0; // Temporary HP from Saga of the Einherjar (damaged first)

    public bool IsAlive => HP > 0;

    public int GetAttributeValue(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "might" => Attributes.Might,
            "finesse" => Attributes.Finesse,
            "wits" => Attributes.Wits,
            "will" => Attributes.Will,
            "sturdiness" => Attributes.Sturdiness,
            _ => 0
        };
    }

    // v0.15: Trauma Economy Helper Methods

    /// <summary>
    /// Checks if the character has a specific trauma by ID
    /// </summary>
    public bool HasTrauma(string traumaId)
    {
        return Traumas.Any(t => t.TraumaId.Equals(traumaId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a trauma by ID, or null if not present
    /// </summary>
    public Trauma? GetTrauma(string traumaId)
    {
        return Traumas.FirstOrDefault(t => t.TraumaId.Equals(traumaId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the total attribute penalty from all traumas for a given attribute
    /// </summary>
    public int GetTraumaAttributePenalty(string attributeName, string? condition = null)
    {
        int totalPenalty = 0;
        foreach (var trauma in Traumas)
        {
            totalPenalty += trauma.GetAttributePenalty(attributeName, condition);
        }
        return totalPenalty;
    }

    /// <summary>
    /// Gets the total stress multiplier from all traumas for a given condition
    /// </summary>
    public float GetTraumaStressMultiplier(string condition)
    {
        float totalMultiplier = 1.0f;
        foreach (var trauma in Traumas)
        {
            totalMultiplier *= trauma.GetStressMultiplier(condition);
        }
        return totalMultiplier;
    }

    /// <summary>
    /// Gets the total passive stress per turn from all traumas
    /// </summary>
    public int GetTraumaPassiveStress(string condition)
    {
        int totalStress = 0;
        foreach (var trauma in Traumas)
        {
            totalStress += trauma.GetPassiveStress(condition);
        }
        return totalStress;
    }

    /// <summary>
    /// Checks if any traumas block rest in current location
    /// </summary>
    public bool IsRestBlockedByTraumas(Room? room)
    {
        return Traumas.Any(t => t.BlocksRest(room));
    }

    /// <summary>
    /// Gets the rest effectiveness multiplier from all traumas
    /// </summary>
    public float GetRestEffectivenessMultiplier()
    {
        float multiplier = 1.0f;
        foreach (var trauma in Traumas)
        {
            multiplier *= trauma.GetRestEffectivenessMultiplier();
        }
        return multiplier;
    }
}
