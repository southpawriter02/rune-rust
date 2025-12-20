namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the type of attack being performed.
/// Determines stamina cost and damage modifiers.
/// </summary>
public enum AttackType
{
    /// <summary>
    /// Quick, low-stamina attack.
    /// Cost: 15 stamina. Uses d4 for weapon damage. +1 to hit.
    /// </summary>
    Light = 0,

    /// <summary>
    /// Balanced attack with moderate stamina cost.
    /// Cost: 25 stamina. Uses d6 for weapon damage. No modifiers.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Powerful attack with high stamina cost.
    /// Cost: 40 stamina. Uses d8 for weapon damage. -1 to hit.
    /// </summary>
    Heavy = 2
}
