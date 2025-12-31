namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the valid targeting options for a spell.
/// Determines what entities can be selected when casting.
/// </summary>
/// <remarks>
/// Target Types (v0.4.3b - The Grimoire):
/// - Self: Caster only
/// - SingleEnemy: One hostile target
/// - SingleAlly: One friendly target
/// - AllEnemies: All hostile targets
/// - AllAllies: All friendly targets
/// - SingleAny: Any single target regardless of allegiance
/// - Area: Affects a zone or area of effect
/// </remarks>
public enum SpellTargetType
{
    /// <summary>
    /// Spell can only target the caster themselves.
    /// Used for self-buffs, personal shields, and self-healing.
    /// </summary>
    Self = 0,

    /// <summary>
    /// Spell targets a single hostile entity.
    /// Standard offensive targeting for damage spells.
    /// </summary>
    SingleEnemy = 1,

    /// <summary>
    /// Spell targets a single friendly entity.
    /// Used for targeted healing, buffs, and protection.
    /// </summary>
    SingleAlly = 2,

    /// <summary>
    /// Spell affects all hostile entities in combat.
    /// Mass damage or debuff effects.
    /// </summary>
    AllEnemies = 3,

    /// <summary>
    /// Spell affects all friendly entities in combat.
    /// Mass healing or buff effects.
    /// </summary>
    AllAllies = 4,

    /// <summary>
    /// Spell can target any single entity regardless of allegiance.
    /// Flexible targeting for utility spells.
    /// </summary>
    SingleAny = 5,

    /// <summary>
    /// Spell affects a zone or area of effect.
    /// May hit multiple targets based on positioning.
    /// </summary>
    Area = 6
}
