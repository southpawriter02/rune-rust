namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Gameplay rules and modifiers for a biome.
/// </summary>
public class BiomeRules
{
    /// <summary>
    /// Movement speed modifier (1.0 = normal, 0.5 = half speed, 1.5 = faster).
    /// </summary>
    public float MovementModifier { get; init; } = 1.0f;

    /// <summary>
    /// Visibility range modifier (1.0 = normal, lower = reduced visibility).
    /// </summary>
    public float VisibilityModifier { get; init; } = 1.0f;

    /// <summary>
    /// Combat effectiveness modifier (1.0 = normal).
    /// </summary>
    public float CombatModifier { get; init; } = 1.0f;

    /// <summary>
    /// Ambient damage per turn (0 = no ambient damage).
    /// </summary>
    public int AmbientDamage { get; init; } = 0;

    /// <summary>
    /// Damage type for ambient damage (e.g., "fire", "cold", "poison").
    /// </summary>
    public string? AmbientDamageType { get; init; }

    /// <summary>
    /// Creates default biome rules (no modifiers).
    /// </summary>
    public static BiomeRules Default => new();

    /// <summary>
    /// Applies movement modifier to a base value.
    /// </summary>
    public int ApplyMovementModifier(int baseMovement) =>
        (int)(baseMovement * MovementModifier);

    /// <summary>
    /// Applies visibility modifier to a base value.
    /// </summary>
    public int ApplyVisibilityModifier(int baseVisibility) =>
        (int)(baseVisibility * VisibilityModifier);

    /// <summary>
    /// Applies combat modifier to a damage value.
    /// </summary>
    public int ApplyCombatModifier(int baseDamage) =>
        (int)(baseDamage * CombatModifier);
}
