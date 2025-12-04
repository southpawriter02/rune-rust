namespace RuneAndRust.Core.Traits;

/// <summary>
/// Configuration for a creature trait instance, allowing parameterization of trait effects.
/// Each enemy can have multiple TraitConfigurations with different values.
/// </summary>
public class TraitConfiguration
{
    /// <summary>
    /// The trait this configuration applies to.
    /// </summary>
    public CreatureTrait Trait { get; set; }

    /// <summary>
    /// Primary numeric value for the trait effect.
    /// Interpretation depends on trait type:
    /// - Regeneration: HP healed per turn
    /// - ArmoredPlating: Soak bonus
    /// - TemporalPrescience: Evasion bonus
    /// - ChronoDistortion: Stress inflicted
    /// - DamageThreshold: Minimum damage to register
    /// - ShieldGenerator: Temporary HP amount
    /// </summary>
    public int PrimaryValue { get; set; }

    /// <summary>
    /// Secondary numeric value for traits that need two parameters.
    /// Common uses:
    /// - Range/radius for aura effects (ChronoDistortion, BlightAura, ForlornAura)
    /// - Duration for timed effects
    /// - Tile count for movement bonuses
    /// </summary>
    public int SecondaryValue { get; set; }

    /// <summary>
    /// Percentage value for probability-based or multiplier traits.
    /// Examples:
    /// - Reflective: 0.20 = 20% damage reflection
    /// - CausalEcho: 0.20 = 20% chance for double hit
    /// - Vampiric: 0.50 = 50% lifesteal
    /// </summary>
    public float Percentage { get; set; }

    /// <summary>
    /// Trait balance point cost for creature budget calculations.
    /// Set by TraitDefaults or overridden for specific configurations.
    /// </summary>
    public int PointCost { get; set; }

    /// <summary>
    /// Additional metadata for complex trait configurations.
    /// Used for traits that need non-numeric parameters like:
    /// - DamageType for ElementalAbsorption
    /// - SpawnEnemyType for SplitOnDeath
    /// - LinkedEnemyId for SymbioticLink
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Runtime state tracking for traits with per-combat usage limits.
    /// Examples:
    /// - TimeLoop: Has resurrection been used?
    /// - Rewind: Has undo been used?
    /// - ModularConstruction: Has first-hit reduction been applied?
    /// </summary>
    public bool HasBeenUsed { get; set; }

    /// <summary>
    /// Counter for traits that track numeric state.
    /// Examples:
    /// - AdaptiveArmor: Which damage types have been adapted to
    /// - LastStand: Turns remaining for immortality
    /// </summary>
    public int StateCounter { get; set; }

    /// <summary>
    /// Gets the category this trait belongs to.
    /// </summary>
    public TraitCategory Category => Trait.GetCategory();

    /// <summary>
    /// Creates a configuration with default values for the given trait.
    /// </summary>
    public static TraitConfiguration Create(CreatureTrait trait)
    {
        return new TraitConfiguration { Trait = trait };
    }

    /// <summary>
    /// Creates a configuration with a primary value.
    /// </summary>
    public static TraitConfiguration Create(CreatureTrait trait, int primaryValue)
    {
        return new TraitConfiguration
        {
            Trait = trait,
            PrimaryValue = primaryValue
        };
    }

    /// <summary>
    /// Creates a configuration with primary and secondary values.
    /// </summary>
    public static TraitConfiguration Create(CreatureTrait trait, int primaryValue, int secondaryValue)
    {
        return new TraitConfiguration
        {
            Trait = trait,
            PrimaryValue = primaryValue,
            SecondaryValue = secondaryValue
        };
    }

    /// <summary>
    /// Gets a metadata value with type safety.
    /// </summary>
    public T? GetMetadata<T>(string key)
    {
        if (Metadata == null || !Metadata.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        return default;
    }

    /// <summary>
    /// Sets a metadata value.
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }

    /// <summary>
    /// Resets per-combat state (call at combat start).
    /// </summary>
    public void ResetCombatState()
    {
        HasBeenUsed = false;
        StateCounter = 0;
    }

    public override string ToString()
    {
        var parts = new List<string> { Trait.ToString() };

        if (PrimaryValue != 0)
            parts.Add($"Primary={PrimaryValue}");
        if (SecondaryValue != 0)
            parts.Add($"Secondary={SecondaryValue}");
        if (Percentage != 0)
            parts.Add($"Pct={Percentage:P0}");

        return string.Join(", ", parts);
    }
}
