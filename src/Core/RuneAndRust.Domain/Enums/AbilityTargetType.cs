namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines valid target types for abilities.
/// </summary>
public enum AbilityTargetType
{
    /// <summary>
    /// Targets the caster themselves.
    /// </summary>
    Self = 0,

    /// <summary>
    /// Targets a single enemy.
    /// </summary>
    SingleEnemy = 1,

    /// <summary>
    /// Targets all enemies in combat.
    /// </summary>
    AllEnemies = 2,

    /// <summary>
    /// Targets a single ally (future feature).
    /// </summary>
    SingleAlly = 3,

    /// <summary>
    /// Targets all allies (future feature).
    /// </summary>
    AllAllies = 4,

    /// <summary>
    /// Targets an area (future feature).
    /// </summary>
    Area = 5,

    /// <summary>
    /// No target required (passive or toggle).
    /// </summary>
    None = 6
}
