using RuneAndRust.Core;

namespace RuneAndRust.Core.Population;

/// <summary>
/// Environmental hazards in procedurally generated Sectors (v0.11)
/// Dynamic = Can be modified by Coherent Glitch rules (v0.12)
/// </summary>
public abstract class DynamicHazard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string HazardId { get; set; } = string.Empty; // Alias for Core compatibility
    public string HazardName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DynamicHazardType Type { get; set; } // Hazard type classification

    // Mechanical Properties
    public int DamageDice { get; set; } = 0; // Number of d6s
    public int DamageFlat { get; set; } = 0; // Flat damage
    public int StressPerTurn { get; set; } = 0; // Psychic Stress

    // Positional Data
    public Vector2? Position { get; set; } = null;
    public float Range { get; set; } = 0f; // Affected area radius

    // Coherent Glitch Modifiers (v0.12)
    public double DamageMultiplier { get; set; } = 1.0;
    public double RangeMultiplier { get; set; } = 1.0;
    public bool IsEnhancedByRule { get; set; } = false; // Flagged by rule system

    // v0.37.1: Investigation properties
    public string FlavorText => Description; // Alias for command system
    public bool IsActive { get; set; } = true;
    public bool CanBeDisabled { get; set; } = false;
    public bool HasBeenInvestigated { get; set; } = false;
    public int InvestigationDC { get; set; } = 3;
    public string? DisableHint { get; set; } = null;

    // Additional backward compatibility properties
    public bool IsHidden { get; set; } = false;
    public bool HasBeenDiscovered { get; set; } = false;
}

/// <summary>
/// [Steam Vent] - Geothermal venting hazard (v0.11)
/// </summary>
public class SteamVentHazard : DynamicHazard
{
    public SteamVentHazard()
    {
        HazardName = "Steam Vent";
        Description = "Superheated steam vents unpredictably from corroded pipes.";
        DamageDice = 2; // 2d6 damage
        Range = 2.0f;
        Type = DynamicHazardType.SteamVent;
    }

    public bool IsIntermittent { get; set; } = true; // Vents every 2-3 turns
    public int TurnsUntilNextVent { get; set; } = 2;
}

/// <summary>
/// [Live Power Conduit] - Electrical hazard (v0.11)
/// </summary>
public class LivePowerConduitHazard : DynamicHazard
{
    public LivePowerConduitHazard()
    {
        HazardName = "Live Power Conduit";
        Description = "Exposed electrical wiring crackles with residual charge.";
        DamageDice = 3; // 3d6 damage
        Range = 1.5f;
        Type = DynamicHazardType.LivePowerConduit;
    }

    public bool IsContactBased { get; set; } = true; // Only damages on touch
    public bool IsFloodedEnhanced { get; set; } = false; // Enhanced by [Flooded] condition
}

/// <summary>
/// [Unstable Ceiling] - Structural collapse hazard (v0.11)
/// </summary>
public class UnstableCeilingHazard : DynamicHazard
{
    public UnstableCeilingHazard()
    {
        HazardName = "Unstable Ceiling";
        Description = "Corroded support beams groan ominously overhead.";
        DamageDice = 4; // 4d6 damage on collapse
        Range = 3.0f; // Large affected area
        Type = DynamicHazardType.UnstableCeiling;
    }

    public int CollapseThreshold { get; set; } = 10; // Cumulative damage triggers collapse
    public int AccumulatedDamage { get; set; } = 0;
}

/// <summary>
/// [Toxic Spore Cloud] - Biological hazard (v0.11)
/// </summary>
public class ToxicSporeCloudHazard : DynamicHazard
{
    public ToxicSporeCloudHazard()
    {
        HazardName = "Toxic Spore Cloud";
        Description = "Bioluminescent fungal spores drift through the air.";
        DamageDice = 1; // 1d6 damage
        StressPerTurn = 1; // +1 Psychic Stress per turn
        Range = 4.0f; // Wide area
        Type = DynamicHazardType.SporeCloud;
    }

    public bool IsMoving { get; set; } = false; // Drifts over time
}

/// <summary>
/// [Chasm] - Traversal hazard (v0.11)
/// </summary>
public class ChasmHazard : DynamicHazard
{
    public ChasmHazard()
    {
        HazardName = "Chasm";
        Description = "A deep fissure splits the floor, its bottom lost in darkness.";
        DamageDice = 6; // 6d6 fall damage
        Range = 0f; // No area effect
        Type = DynamicHazardType.Chasm;
    }

    public float Width { get; set; } = 3.0f; // Meters
    public bool IsTraversable { get; set; } = false; // Requires skill check
}

/// <summary>
/// [Corroded Grating] - Fragile floor hazard (v0.11)
/// </summary>
public class CorrodedGratingHazard : DynamicHazard
{
    public CorrodedGratingHazard()
    {
        HazardName = "Corroded Grating";
        Description = "The floor grating is severely weakened. It may break under weight.";
        DamageDice = 2; // 2d6 damage
        Range = 1.0f;
        Type = DynamicHazardType.CorrodedGrating;
    }

    public float ActivationChance { get; set; } = 0.3f;
}

/// <summary>
/// [Leaking Coolant] - Slippery surface hazard (v0.11)
/// </summary>
public class LeakingCoolantHazard : DynamicHazard
{
    public LeakingCoolantHazard()
    {
        HazardName = "Leaking Coolant";
        Description = "Slippery chemical coolant pools across the floor, making movement treacherous.";
        DamageDice = 1; // 1d6 damage
        Range = 2.0f;
        Type = DynamicHazardType.LeakingCoolant;
    }

    public float ActivationChance { get; set; } = 0.5f;
}
