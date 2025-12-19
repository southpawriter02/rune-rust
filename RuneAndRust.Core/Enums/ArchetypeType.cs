namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the character archetypes available in Rune &amp; Rust.
/// Archetypes determine the character's combat style and stat bonuses.
/// </summary>
public enum ArchetypeType
{
    /// <summary>
    /// Frontline combatant specializing in durability and melee damage.
    /// Bonuses: +2 Sturdiness, +1 Might.
    /// </summary>
    Warrior = 0,

    /// <summary>
    /// Cunning fighter favoring speed and precision over raw power.
    /// Bonuses: +2 Finesse, +1 Wits.
    /// </summary>
    Skirmisher = 1,

    /// <summary>
    /// Runic practitioner channeling ancient power through inscribed formulas.
    /// Bonuses: +2 Wits, +1 Will.
    /// </summary>
    Adept = 2,

    /// <summary>
    /// Wielder of primal forces, drawing power from the world's corruption.
    /// Bonuses: +2 Will, +1 Sturdiness.
    /// </summary>
    Mystic = 3
}
