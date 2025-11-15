namespace RuneAndRust.Core;

/// <summary>
/// v0.22: Environmental Combat System - Environmental Objects
/// Represents destructible terrain, interactive objects, cover, and hazards on the battlefield.
/// Part of the v0.22 parent specification for environmental combat framework.
///
/// Design Philosophy (v2.0 Canonical):
/// - Environment as active participant in combat
/// - Telegraphed dangers, never sudden deaths
/// - Player agency: weaponize, avoid, or tank through
/// - Specialization synergy (Analysts detect, Controllers manipulate)
/// </summary>
public class EnvironmentalObject
{
    // Identity
    public int ObjectId { get; set; }
    public int RoomId { get; set; }
    public string? GridPosition { get; set; } // "Front_Left_Column_2" (JSON for multi-tile objects)
    public EnvironmentalObjectType ObjectType { get; set; } // Cover, Hazard, Interactive, Obstacle

    // Display
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🔶"; // Default icon

    // Structural properties (for destructible objects)
    public bool IsDestructible { get; set; } = false;
    public int? CurrentDurability { get; set; } // HP for destructible objects
    public int? MaxDurability { get; set; }
    public int SoakValue { get; set; } = 0; // Damage reduction before durability loss

    // Hazard properties
    public bool IsHazard { get; set; } = false;
    public HazardTrigger HazardTrigger { get; set; } = HazardTrigger.Manual;
    public string? DamageFormula { get; set; } // "6d10 Fire", "4d6 Poison"
    public string? DamageType { get; set; } // "Fire", "Poison", "Physical", "Psychic"
    public string? StatusEffect { get; set; } // "[Burning]", "[Poisoned]", "[Corroded]"
    public bool IgnoresSoak { get; set; } = false;

    // Movement and visibility blocking (v0.29.5 - Muspelheim tile integration)
    public bool BlocksMovement { get; set; } = false; // Prevents movement through this tile (lava rivers, chasms)
    public bool BlocksLineOfSight { get; set; } = false; // Blocks line of sight for targeting

    // Cover properties (integrates with v0.20.2 CoverService)
    public bool ProvidesCover { get; set; } = false;
    public CoverQuality CoverQuality { get; set; } = CoverQuality.None;
    public string? CoverArc { get; set; } // Direction cover blocks from (JSON array)

    // Interactive properties
    public bool IsInteractive { get; set; } = false;
    public InteractionType InteractionType { get; set; } = InteractionType.None;
    public int InteractionCost { get; set; } = 0; // Stamina cost
    public string? InteractionSkillCheck { get; set; } // "System Bypass DC 14"

    // State tracking
    public EnvironmentalObjectState State { get; set; } = EnvironmentalObjectState.Active;
    public int TriggersRemaining { get; set; } = 1; // For one-time hazards
    public int CooldownRemaining { get; set; } = 0; // Turns until re-armed
    public int CooldownDuration { get; set; } = 0; // Base cooldown in turns

    // Destruction aftermath (v0.22.1)
    public string? CreatesTerrainOnDestroy { get; set; } // "Difficult", "Hazardous", NULL
    public int? TerrainDuration { get; set; } // Turns (NULL = permanent)

    // Chain reaction properties (v0.22.1)
    public int ExplosionRadius { get; set; } = 0; // Tiles affected (0 = self only)
    public bool CanTriggerAdjacents { get; set; } = false;

    /// <summary>
    /// Applies damage to this environmental object (if destructible)
    /// </summary>
    /// <param name="damage">Incoming damage</param>
    /// <returns>True if object was destroyed</returns>
    public bool ApplyDamage(int damage)
    {
        if (!IsDestructible || CurrentDurability == null)
        {
            return false;
        }

        // Apply soak (damage reduction)
        int actualDamage = Math.Max(0, damage - SoakValue);
        CurrentDurability = Math.Max(0, CurrentDurability.Value - actualDamage);

        if (CurrentDurability <= 0)
        {
            State = EnvironmentalObjectState.Destroyed;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if object can be triggered/interacted with
    /// </summary>
    public bool CanTrigger()
    {
        return State == EnvironmentalObjectState.Active && TriggersRemaining > 0;
    }

    /// <summary>
    /// Triggers the object (hazard activation, interaction, etc.)
    /// </summary>
    public void Trigger()
    {
        if (CanTrigger())
        {
            TriggersRemaining--;
            if (TriggersRemaining <= 0)
            {
                State = EnvironmentalObjectState.Triggered;
            }
        }
    }
}

/// <summary>
/// Type of environmental object
/// </summary>
public enum EnvironmentalObjectType
{
    Cover,          // Physical or metaphysical cover (integrates with v0.20.2)
    Hazard,         // Environmental danger (fire, toxic pools, energy fields)
    Interactive,    // Can be triggered/activated (explosive barrels, steam vents)
    Obstacle,       // Deployable barriers, caltrops, traps
    Terrain         // Static terrain features (unstable ceilings, crumbling floors)
}

/// <summary>
/// Quality/strength of cover provided
/// </summary>
public enum CoverQuality
{
    None,           // No cover
    Light,          // Minimal protection (+2 Defense)
    Heavy,          // Significant protection (+4 Defense) - standard v0.20.2
    Total           // Complete protection (+6 Defense, blocks line of sight)
}

/// <summary>
/// Type of interaction possible with object
/// </summary>
public enum InteractionType
{
    None,           // Cannot be interacted with
    Trigger,        // Trigger an effect (explosive barrel detonation)
    Activate,       // Activate/deactivate (steam vent control)
    Loot,           // Contains items
    Bypass          // Can be disabled/bypassed (security system)
}

/// <summary>
/// Current state of environmental object
/// </summary>
public enum EnvironmentalObjectState
{
    Active,         // Functional and can be used/triggered
    Damaged,        // Partially damaged (below 50% durability) - v0.22.1
    Destroyed,      // Destroyed and non-functional
    Triggered,      // Has been triggered (one-time effects)
    Depleted,       // No longer has resources (loot taken, etc.)
    Disabled        // Temporarily disabled/suppressed
}
