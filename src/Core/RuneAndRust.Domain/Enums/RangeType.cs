namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of attack range for weapons and abilities.
/// </summary>
public enum RangeType
{
    /// <summary>Melee range - must be adjacent (distance 1).</summary>
    Melee = 0,

    /// <summary>Reach range - can attack at distance 1 or 2.</summary>
    Reach = 1,

    /// <summary>Ranged - can attack at configurable distance.</summary>
    Ranged = 2
}
