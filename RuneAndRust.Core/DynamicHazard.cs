namespace RuneAndRust.Core;

/// <summary>
/// v0.11 Dynamic Hazard System
/// Environmental dangers that activate, damage, or hinder
/// Result of 800 years of decay per v5.0 compliance
/// </summary>
public class DynamicHazard
{
    // Identity
    public string HazardId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DynamicHazardType Type { get; set; }

    // Activation
    public float ActivationChance { get; set; } = 1.0f; // 1.0 = always, 0.4 = 40% per turn
    public HazardTrigger Trigger { get; set; } = HazardTrigger.Automatic;

    // Damage
    public int DamageDice { get; set; } = 0; // Number of d6s
    public int DamageDieSize { get; set; } = 6;
    public string DamageType { get; set; } = "Physical"; // Physical, Fire, Lightning, Poison

    // Area of Effect
    public int AreaSize { get; set; } = 1; // 1 = single target, 3 = 3x3 area
    public bool AffectsAllCombatants { get; set; } = false;

    // Special Properties
    public bool IsOneTime { get; set; } = false; // Destroyed after activation
    public bool RequiresProximity { get; set; } = false; // Activated by getting close
    public int ProximityRange { get; set; } = 2; // Tiles

    // Status Effects
    public string? AppliesStatusEffect { get; set; } = null; // "Poisoned", "Corroded", etc.
    public float StatusEffectChance { get; set; } = 0f;

    // Interaction with Ambient Conditions (Coherent Glitch rules)
    public string? EnhancedByCondition { get; set; } = null; // e.g., "Flooded" enhances electrical
    public float EnhancementMultiplier { get; set; } = 2.0f;

    // State
    public bool IsActive { get; set; } = true;
    public bool HasActivatedThisTurn { get; set; } = false;
}

/// <summary>
/// Types of dynamic hazards for v0.11
/// </summary>
public enum DynamicHazardType
{
    SteamVent,           // Superheated steam from geothermal systems
    LivePowerConduit,    // Exposed electrical wiring
    UnstableCeiling,     // Collapsing infrastructure (one-time)
    ToxicSporeCloud,     // Fungal contamination
    CorrodedGrating,     // Fragile floor that breaks
    LeakingCoolant,      // Slippery chemical spill
    RadiationLeak,       // Cumulative exposure
    PressurizedPipe,     // Explosive decompression

    // v0.16 new hazards (Content Expansion)
    SporeCloud,          // Symbiotic Plate spore dispersion
    AutomatedTurret,     // Pre-Glitch active security
    CollapsingCeiling,   // Structural collapse hazard
    DataStream,          // Jötun-Reader psychic overflow
    FungalGrowth,        // Symbiotic Plate terrain barrier
    UnstableGrating,     // Corroded floor trap
    PsychicEcho,         // Trauma site resonance
    RadiationSource      // Nuclear system leak
}

/// <summary>
/// How a hazard is triggered
/// </summary>
public enum HazardTrigger
{
    Automatic,      // Activates every turn or at set chance
    OnProximity,    // Activates when someone gets close
    OnLoudAction,   // Triggered by explosions, heavy attacks
    OnMovement,     // Triggered by crossing/stepping on
    Manual          // Must be deliberately activated
}
