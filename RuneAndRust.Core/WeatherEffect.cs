namespace RuneAndRust.Core;

/// <summary>
/// v0.22: Environmental Combat System - Weather Effects
/// Represents temporary or persistent weather conditions affecting combat.
/// Weather effects are more dynamic than ambient conditions - they can move, intensify, or dissipate.
///
/// v2.0 Canonical Weather Types:
/// - Reality Storms: Corrupted Aether manifestations
/// - Static Discharge: Electromagnetic chaos from decaying systems
/// - Corrosion Cloud: Chemical decay atmospheres
/// - Psychic Resonance Storm: Trauma echoes made manifest
///
/// v5.0 Compliance: Weather is system failure, not natural phenomena
/// </summary>
public class WeatherEffect
{
    // Identity
    public int WeatherId { get; set; }
    public int? SectorId { get; set; } // If sector-wide (future expansion)
    public int? RoomId { get; set; } // If room-specific

    // Weather properties
    public WeatherType WeatherType { get; set; }
    public WeatherIntensity Intensity { get; set; } = WeatherIntensity.Moderate;
    public int? DurationTurns { get; set; } // Null if permanent
    public int TurnsRemaining { get; set; }

    // Display
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🌪️";

    // Combat effects (modifiers applied to all combatants)
    public int AccuracyModifier { get; set; } = 0; // -2 in reality storm
    public int DamageModifier { get; set; } = 0; // +damage for fire weather, etc.
    public int MovementCostModifier { get; set; } = 0; // +1 in heavy weather
    public int StressPerTurn { get; set; } = 0; // Psychic pressure from reality storms

    // Damage over time
    public string? DamageFormula { get; set; } // "2d6 Psychic" for severe effects
    public string? DamageType { get; set; } // "Psychic", "Fire", "Physical"

    // Status effects
    public string? StatusEffectApplied { get; set; } // "[Disoriented]", "[Corroded]"
    public float StatusEffectChance { get; set; } = 0f; // 0.0 - 1.0

    // Hazard interaction
    public bool AmplifiesHazards { get; set; } = false;
    public float HazardAmplificationMultiplier { get; set; } = 1.0f; // 1.5x = 50% more damage
    public List<DynamicHazardType> AffectedHazardTypes { get; set; } = new(); // Specific hazard types affected

    // Environmental object interaction
    public bool AcceleratesDegradation { get; set; } = false; // Corrosion cloud damages cover faster
    public int DegradationMultiplier { get; set; } = 1; // 2x degradation rate

    // State
    public bool IsActive { get; set; } = true;
    public bool IsSuppressed { get; set; } = false; // Can be temporarily suppressed by abilities

    /// <summary>
    /// Advances weather state by one turn
    /// </summary>
    /// <returns>True if weather is still active, false if it has dissipated</returns>
    public bool AdvanceTurn()
    {
        if (DurationTurns.HasValue)
        {
            TurnsRemaining--;
            if (TurnsRemaining <= 0)
            {
                IsActive = false;
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the combat modifier description for this weather effect
    /// </summary>
    public string GetModifierDescription()
    {
        var modifiers = new List<string>();

        if (AccuracyModifier != 0)
            modifiers.Add($"{(AccuracyModifier > 0 ? "+" : "")}{AccuracyModifier}d Accuracy");
        if (MovementCostModifier != 0)
            modifiers.Add($"+{MovementCostModifier} Movement Cost");
        if (StressPerTurn > 0)
            modifiers.Add($"{StressPerTurn} Stress/turn");
        if (DamageFormula != null)
            modifiers.Add($"{DamageFormula} {DamageType}/turn");
        if (AmplifiesHazards)
            modifiers.Add($"Hazards: {HazardAmplificationMultiplier:P0} damage");

        return modifiers.Count > 0 ? string.Join(", ", modifiers) : "No direct effects";
    }
}

/// <summary>
/// Type of weather effect (v2.0 canonical, v5.0 compliant)
/// All weather is technological/system failure, not natural phenomena
/// </summary>
public enum WeatherType
{
    None,                       // No weather effects
    RealityStorm,               // Corrupted Aether manifestation (accuracy penalty, stress)
    StaticDischarge,            // Electromagnetic chaos (lightning hazards amplified)
    CorrosionCloud,             // Chemical decay atmosphere (equipment degradation)
    PsychicResonanceStorm,      // Trauma echoes (psychic damage, stress buildup)
    ToxicFog,                   // Industrial chemical dispersal (poison damage)
    RadiationPulse,             // Nuclear system leakage (radiation damage)
    SystemGlitch,               // Reality instability (random teleports, glitch effects)
    VoidIncursion,              // Blight manifestation (corruption buildup)
    DataStorm,                  // Information cascade (psychic overload for Readers)
    TemporalDistortion          // Chronological malfunction (movement/action disruption)
}

/// <summary>
/// Intensity level of weather effect
/// </summary>
public enum WeatherIntensity
{
    Low,                // Minor effects, easily manageable
    Moderate,           // Standard effects, noticeable impact
    High,               // Severe effects, significant danger
    Extreme             // Catastrophic effects, combat-defining
}

/// <summary>
/// Extended ambient condition data for v0.22 compatibility
/// Adds additional fields to existing v0.11 AmbientCondition
/// </summary>
public class AmbientConditionExtended
{
    // v0.11 baseline properties (reference existing AmbientCondition)
    public AmbientCondition BaseCondition { get; set; } = new();

    // v0.22 extensions
    public int? DurationTurns { get; set; } // Null if permanent
    public int TurnsRemaining { get; set; }
    public bool IsSuppressible { get; set; } = false;
    public string? SuppressionAbility { get; set; } // "Consecrate Ground", "Purify Air"
    public int? SuppressionDuration { get; set; } // How long suppression lasts
    public bool IsCurrentlySuppressed { get; set; } = false;

    // Resolve check for resisting effects
    public int? ResolveCheckDC { get; set; } // DC to resist effects
    public string? ResolveCheckStat { get; set; } // "STURDINESS", "WILL"
    public string? StatusOnFumble { get; set; } // "[Poisoned]" if fumbled check

    // Intensity tracking
    public string Intensity { get; set; } = "Moderate"; // "Low", "Moderate", "High", "Extreme"
    public int CorruptionPerTurn { get; set; } = 0; // [Metaphysical Corruption]

    /// <summary>
    /// Advances ambient condition state by one turn
    /// </summary>
    /// <returns>True if condition is still active</returns>
    public bool AdvanceTurn()
    {
        if (DurationTurns.HasValue)
        {
            TurnsRemaining--;
            return TurnsRemaining > 0;
        }
        return true; // Permanent conditions always active
    }
}
