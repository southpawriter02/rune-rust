namespace RuneAndRust.Core.Population;

/// <summary>
/// Environmental hazards in procedurally generated Sectors (v0.11)
/// Dynamic = Can be modified by Coherent Glitch rules (v0.12)
/// </summary>
public abstract class DynamicHazard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string HazardName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

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
    }

    public float Width { get; set; } = 3.0f; // Meters
    public bool IsTraversable { get; set; } = false; // Requires skill check
}
