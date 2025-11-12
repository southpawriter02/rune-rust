namespace RuneAndRust.Core;

/// <summary>
/// v0.11 Ambient Condition System
/// Environmental effects that modify room behavior
/// Can interact with hazards per Coherent Glitch rules
/// </summary>
public class AmbientCondition
{
    // Identity
    public string ConditionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AmbientConditionType Type { get; set; }

    // Effects
    public int PsychicStressPerTurn { get; set; } = 0;
    public int MovementCostModifier { get; set; } = 0; // +1 for flooded
    public int AccuracyModifier { get; set; } = 0; // -1d for dim lighting
    public int WillResolveModifier { get; set; } = 0; // -1d for psychic resonance

    // Status Effect Risks
    public string? CanApplyStatusEffect { get; set; } = null; // "Poisoned", "Corroded"
    public float StatusEffectChance { get; set; } = 0f;

    // Hazard Interactions (Coherent Glitch rules)
    public List<string> EnhancesHazardTypes { get; set; } = new(); // e.g., ["LivePowerConduit"]
    public float HazardEnhancementMultiplier { get; set; } = 1.0f;

    // Equipment Degradation
    public bool CausesEquipmentDegradation { get; set; } = false;
    public int DegradationAmount { get; set; } = 0;

    // Wild Magic (for Runic Instability)
    public bool CausesWildMagic { get; set; } = false;
    public float WildMagicChance { get; set; } = 0f;

    // Spawn Weight Modifiers
    public Dictionary<string, float> SpawnWeightModifiers { get; set; } = new();
    // e.g., ["LivePowerConduit" -> 2.0] means electrical hazards spawn 2x more often
}

/// <summary>
/// Types of ambient conditions for v0.11
/// </summary>
public enum AmbientConditionType
{
    PsychicResonance,    // +2 Stress/turn, -1d WILL, common in narrative rooms
    RunicInstability,    // 20% wild magic on Aether abilities
    Flooded,             // +1 movement cost, enhances electrical hazards
    CorrodedAtmosphere,  // Equipment degradation, rust particles
    DimLighting,         // Visibility penalty, failing illumination
    ToxicAir,            // Passive poison buildup
    ExtremeHeat,         // Fire hazard enhancement
    SubZeroTemperature,  // Movement penalty, cold damage risk
    HighRadiation,       // Cumulative exposure damage
    ElectromagneticField // Electronics malfunction, compass useless
}
