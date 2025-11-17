namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.2: Dynamic hazard model
/// Represents active environmental dangers (steam vents, electrical hazards, toxic zones)
/// Generated from Descriptor_Base_Templates + Thematic_Modifiers
/// </summary>
public class DynamicHazard
{
    /// <summary>
    /// Unique identifier for this hazard instance
    /// </summary>
    public int HazardId { get; set; }

    /// <summary>
    /// Hazard name (e.g., "Geothermal Steam Vent")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive text for the hazard
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base template name (e.g., "Steam_Vent_Base")
    /// </summary>
    public string? BaseTemplateName { get; set; }

    /// <summary>
    /// Modifier name (e.g., "Geothermal")
    /// </summary>
    public string? ModifierName { get; set; }

    /// <summary>
    /// Hazard archetype (DynamicHazard)
    /// </summary>
    public string Archetype { get; set; } = "DynamicHazard";

    // Damage Properties
    /// <summary>
    /// Damage dice (e.g., "2d6", "3d6+2")
    /// </summary>
    public string Damage { get; set; } = string.Empty;

    /// <summary>
    /// Damage type (Fire, Lightning, Poison, Physical, Psychic)
    /// </summary>
    public string DamageType { get; set; } = string.Empty;

    // Activation Properties
    /// <summary>
    /// Activation type (Periodic, Proximity, Triggered, Persistent, Movement)
    /// </summary>
    public HazardActivationType ActivationType { get; set; }

    /// <summary>
    /// Activation frequency (for Periodic hazards, every N turns)
    /// </summary>
    public int ActivationFrequency { get; set; }

    /// <summary>
    /// Activation range (for Proximity hazards, range in tiles)
    /// </summary>
    public int ActivationRange { get; set; }

    /// <summary>
    /// Activation timing (for Persistent hazards, StartOfTurn or EndOfTurn)
    /// </summary>
    public HazardActivationTiming? ActivationTiming { get; set; }

    /// <summary>
    /// Whether hazard provides warning turn before activation
    /// </summary>
    public bool WarningTurn { get; set; }

    // Area Properties
    /// <summary>
    /// Area effect pattern
    /// </summary>
    public AreaEffectPattern AreaPattern { get; set; }

    /// <summary>
    /// Number of tiles affected
    /// </summary>
    public int TilesAffected { get; set; }

    // Status Effect Properties
    /// <summary>
    /// Status effect applied (e.g., ["Burning", 2] for Burning with 2 turn duration)
    /// </summary>
    public List<object>? StatusEffect { get; set; }

    /// <summary>
    /// Chance to apply status effect (0.0-1.0)
    /// </summary>
    public float StatusEffectChance { get; set; }

    // Special Properties
    /// <summary>
    /// Whether this hazard activates only once then is destroyed
    /// </summary>
    public bool IsOneTime { get; set; }

    /// <summary>
    /// Terrain created when hazard activates (e.g., "Rubble_Pile")
    /// </summary>
    public string? CreatesTerrain { get; set; }

    /// <summary>
    /// Trigger conditions (for Triggered hazards)
    /// </summary>
    public List<string> Triggers { get; set; } = new();

    /// <summary>
    /// Ambient conditions that enhance this hazard
    /// </summary>
    public List<string> EnhancedBy { get; set; } = new();

    // Persistent Hazard Properties
    /// <summary>
    /// Chance to spread to adjacent tiles (0.0-1.0)
    /// </summary>
    public float SpreadChance { get; set; }

    /// <summary>
    /// Accuracy penalty applied while in hazard area
    /// </summary>
    public int AccuracyPenalty { get; set; }

    /// <summary>
    /// Whether status effect stacks with duration
    /// </summary>
    public bool Stacks { get; set; }

    // Special Modifiers (from Lava_Filled, Void, etc.)
    /// <summary>
    /// Ambient heat range (for Lava_Filled modifier)
    /// </summary>
    public int AmbientHeatRange { get; set; }

    /// <summary>
    /// Ambient heat damage (for Lava_Filled modifier)
    /// </summary>
    public string? AmbientHeatDamage { get; set; }

    /// <summary>
    /// Proximity stress per turn (for Void modifier)
    /// </summary>
    public int ProximityStress { get; set; }

    /// <summary>
    /// Proximity range for stress (for Void modifier)
    /// </summary>
    public int ProximityRange { get; set; }

    /// <summary>
    /// Whether hazard may shift position between turns (for Void modifier)
    /// </summary>
    public bool IsUnstable { get; set; }

    // Runtime State
    /// <summary>
    /// Current turn counter (for Periodic hazards)
    /// </summary>
    public int CurrentTurnCounter { get; set; }

    /// <summary>
    /// Whether hazard has been activated (for OneTime hazards)
    /// </summary>
    public bool HasActivated { get; set; }

    /// <summary>
    /// Biome restriction (if any)
    /// </summary>
    public string? BiomeRestriction { get; set; }

    /// <summary>
    /// Tags for filtering and classification
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Validates that this hazard has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Name)) return false;
        if (string.IsNullOrEmpty(Damage)) return false;
        if (string.IsNullOrEmpty(DamageType)) return false;

        return true;
    }

    /// <summary>
    /// Gets a tactical summary of this hazard
    /// </summary>
    public string GetTacticalSummary()
    {
        var summary = new List<string>();

        summary.Add($"{Damage} {DamageType} damage");

        summary.Add(ActivationType switch
        {
            HazardActivationType.Periodic => $"Activates every {ActivationFrequency} turns",
            HazardActivationType.Proximity => $"Activates within {ActivationRange} tiles",
            HazardActivationType.Triggered => $"Triggers on: {string.Join(", ", Triggers)}",
            HazardActivationType.Persistent => $"Activates at {ActivationTiming}",
            HazardActivationType.Movement => "Activates on movement",
            _ => "Unknown activation"
        });

        if (StatusEffect != null && StatusEffect.Count > 0)
        {
            var effectName = StatusEffect[0]?.ToString() ?? "Unknown";
            var chance = StatusEffectChance > 0 ? $" ({StatusEffectChance * 100}% chance)" : "";
            summary.Add($"Status: {effectName}{chance}");
        }

        if (AreaPattern != AreaEffectPattern.Single)
        {
            summary.Add($"Area: {AreaPattern}");
        }

        if (IsOneTime)
        {
            summary.Add("One-time activation");
        }

        if (WarningTurn)
        {
            summary.Add("Provides warning turn");
        }

        return string.Join(" | ", summary);
    }

    /// <summary>
    /// Checks if this hazard should activate this turn
    /// </summary>
    public bool ShouldActivateThisTurn()
    {
        if (IsOneTime && HasActivated)
            return false;

        if (ActivationType == HazardActivationType.Periodic)
        {
            CurrentTurnCounter++;
            if (CurrentTurnCounter >= ActivationFrequency)
            {
                CurrentTurnCounter = 0;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Marks this hazard as activated (for OneTime hazards)
    /// </summary>
    public void Activate()
    {
        HasActivated = true;
    }
}
