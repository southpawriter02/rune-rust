namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Damage type multipliers for a specific biome.
/// </summary>
public class BiomeDamageModifier
{
    /// <summary>
    /// Gets the biome ID this modifier applies to.
    /// </summary>
    public string BiomeId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the damage type multipliers (e.g., "fire" -> 0.5 for resistance).
    /// </summary>
    public IReadOnlyDictionary<string, float> Modifiers { get; init; } = new Dictionary<string, float>();

    /// <summary>
    /// Default modifier (no changes).
    /// </summary>
    public static BiomeDamageModifier Default => new();

    /// <summary>
    /// Gets the multiplier for a damage type.
    /// </summary>
    public float GetModifier(string damageType)
    {
        return Modifiers.TryGetValue(damageType, out var modifier) ? modifier : 1.0f;
    }

    /// <summary>
    /// Applies the modifier to base damage.
    /// </summary>
    public int ApplyModifier(string damageType, int baseDamage)
    {
        var modifier = GetModifier(damageType);
        return (int)(baseDamage * modifier);
    }

    /// <summary>
    /// Creates a volcanic biome modifier (fire resistant, ice vulnerable).
    /// </summary>
    public static BiomeDamageModifier Volcanic => new()
    {
        BiomeId = "volcanic-caverns",
        Modifiers = new Dictionary<string, float>
        {
            ["fire"] = 0.5f,
            ["ice"] = 1.5f
        }
    };

    /// <summary>
    /// Creates a flooded biome modifier (ice resistant, lightning vulnerable).
    /// </summary>
    public static BiomeDamageModifier Flooded => new()
    {
        BiomeId = "flooded-depths",
        Modifiers = new Dictionary<string, float>
        {
            ["ice"] = 0.75f,
            ["lightning"] = 1.5f
        }
    };

    /// <summary>
    /// Creates a fungal biome modifier (poison resistant).
    /// </summary>
    public static BiomeDamageModifier Fungal => new()
    {
        BiomeId = "fungal-caverns",
        Modifiers = new Dictionary<string, float>
        {
            ["poison"] = 0.5f
        }
    };
}
