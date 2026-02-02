namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the sources of Rage generation.
/// </summary>
/// <remarks>
/// Each source determines how rage is gained and may have
/// different multipliers or flat values.
/// </remarks>
public enum RageSource
{
    /// <summary>
    /// Rage gained from taking damage.
    /// </summary>
    /// <remarks>
    /// Formula: floor(damage / 5)
    /// Encourages standing firm in combat.
    /// </remarks>
    TakingDamage = 0,

    /// <summary>
    /// Rage gained from dealing damage to enemies.
    /// </summary>
    /// <remarks>
    /// Formula: floor(damage / 10)
    /// Encourages offensive gameplay.
    /// </remarks>
    DealingDamage = 1,

    /// <summary>
    /// Rage gained when allies take damage.
    /// </summary>
    /// <remarks>
    /// Flat: 5 rage per ally damaged
    /// Encourages protecting allies.
    /// </remarks>
    AllyDamaged = 2,

    /// <summary>
    /// Bonus rage from defeating enemies.
    /// </summary>
    /// <remarks>
    /// Flat: 15 rage per kill
    /// Encourages finishing enemies.
    /// </remarks>
    EnemyKill = 3,

    /// <summary>
    /// Passive rage maintenance at FrenzyBeyondReason threshold.
    /// </summary>
    /// <remarks>
    /// Flat: 5 rage per turn
    /// Keeps berserker in highest state.
    /// </remarks>
    RageMaintenance = 4
}
