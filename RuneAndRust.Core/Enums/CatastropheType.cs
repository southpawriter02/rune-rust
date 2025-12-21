namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the type of catastrophic consequence for crafting failures.
/// Different trades have different catastrophe effects when net successes are negative.
/// </summary>
public enum CatastropheType
{
    /// <summary>
    /// No special catastrophe effect. Standard material loss only.
    /// Used for Bodging and Field Medicine trades.
    /// </summary>
    None = 0,

    /// <summary>
    /// Explosive catastrophe. Deals physical damage to the crafter.
    /// Used for Alchemy trade - volatile compounds detonate on failure.
    /// </summary>
    Explosive = 1,

    /// <summary>
    /// Toxic catastrophe. Reserved for future implementation.
    /// Could apply poison or sickness effects.
    /// </summary>
    Toxic = 2,

    /// <summary>
    /// Corrosive catastrophe. Reserved for future implementation.
    /// Could damage equipment or environment.
    /// </summary>
    Corrosive = 3,

    /// <summary>
    /// Corruption catastrophe. Adds permanent Corruption to the crafter.
    /// Used for Runeforging trade - runic backlash taints the soul.
    /// </summary>
    Corruption = 4
}
