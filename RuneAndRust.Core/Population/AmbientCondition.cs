namespace RuneAndRust.Core.Population;

/// <summary>
/// Ambient environmental conditions affecting entire rooms (v0.11)
/// Modified by Coherent Glitch rules (v0.12)
/// </summary>
public abstract class AmbientCondition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConditionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Game Effects
    public int AccuracyModifier { get; set; } = 0;
    public int DefenseModifier { get; set; } = 0;
    public int MovementModifier { get; set; } = 0; // % of normal movement
    public int StressPerTurn { get; set; } = 0;
}

/// <summary>
/// [Flooded] - Water-logged chamber (v0.11)
/// </summary>
public class FloodedCondition : AmbientCondition
{
    public FloodedCondition()
    {
        ConditionName = "[Flooded]";
        Description = "Stagnant water covers the floor, ankle-deep in places.";
        MovementModifier = -20; // -20% movement speed
    }

    public int WaterDepth { get; set; } = 1; // 1 = ankle-deep, 2 = knee-deep, 3 = waist-deep
}

/// <summary>
/// [Darkness] - Poorly lit room (v0.11)
/// </summary>
public class DarknessCondition : AmbientCondition
{
    public DarknessCondition()
    {
        ConditionName = "[Darkness]";
        Description = "Emergency lighting flickers fitfully, casting deep shadows.";
        AccuracyModifier = -2; // -2 to hit
        StressPerTurn = 1; // +1 Psychic Stress per turn
    }
}

/// <summary>
/// [Psychic Resonance] - Forlorn entity influence (v0.11)
/// </summary>
public class PsychicResonanceCondition : AmbientCondition
{
    public PsychicResonanceCondition()
    {
        ConditionName = "[Psychic Resonance]";
        Description = "The air thrums with psychic residue, oppressive and disorienting.";
        StressPerTurn = 2; // +2 Psychic Stress per turn
    }

    public int Intensity { get; set; } = 1; // 1-5 scale
}

/// <summary>
/// [Extreme Heat] - Geothermal influence (v0.11)
/// </summary>
public class ExtremeHeatCondition : AmbientCondition
{
    public ExtremeHeatCondition()
    {
        ConditionName = "[Extreme Heat]";
        Description = "Oppressive heat radiates from corroded thermal conduits.";
        StressPerTurn = 1; // +1 Psychic Stress from exhaustion
    }

    public int DamagePerTurn { get; set; } = 1; // Flat damage per turn (optional)
}
