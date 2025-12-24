using RuneAndRust.Core.Attributes;

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
    [GameDocument(
        "Light Attack",
        "Quick strikes that conserve stamina while sacrificing power. Light attacks connect more reliably but deal modest damage. Useful for finishing weakened foes or testing defenses.")]
    Light = 0,

    /// <summary>
    /// Balanced attack with moderate stamina cost.
    /// Cost: 25 stamina. Uses d6 for weapon damage. No modifiers.
    /// </summary>
    [GameDocument(
        "Standard Attack",
        "Balanced strikes representing the default combat approach. Standard attacks offer no special modifiers but provide consistent damage at reasonable stamina cost. The reliable choice.")]
    Standard = 1,

    /// <summary>
    /// Powerful attack with high stamina cost.
    /// Cost: 40 stamina. Uses d8 for weapon damage. -1 to hit.
    /// </summary>
    [GameDocument(
        "Heavy Attack",
        "Devastating blows that sacrifice accuracy for raw power. Heavy attacks drain significant stamina and are harder to land, but deal tremendous damage on connection. Best against slow or stunned foes.")]
    Heavy = 2
}
