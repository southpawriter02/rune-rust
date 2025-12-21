namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the types of damage in combat (v0.3.3a).
/// Physical damage is reduced by armor soak; elemental damage bypasses soak.
/// </summary>
public enum DamageType
{
    /// <summary>
    /// Physical damage from weapons and impacts. Reduced by armor soak.
    /// </summary>
    Physical = 0,

    /// <summary>
    /// Fire/heat damage. Bypasses armor soak.
    /// </summary>
    Fire = 1,

    /// <summary>
    /// Cold/ice damage. Bypasses armor soak.
    /// </summary>
    Ice = 2,

    /// <summary>
    /// Lightning/electrical damage. Bypasses armor soak.
    /// </summary>
    Lightning = 3,

    /// <summary>
    /// Poison/toxin damage. Bypasses armor soak.
    /// </summary>
    Poison = 4,

    /// <summary>
    /// Acid/corrosive damage. Bypasses armor soak.
    /// </summary>
    Acid = 5,

    /// <summary>
    /// Psychic/mental damage. Bypasses armor soak.
    /// </summary>
    Psychic = 6,

    /// <summary>
    /// Blight/corruption damage from Runic Blight. Bypasses armor soak.
    /// </summary>
    Blight = 7
}
