namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of entries in the combat log.
/// </summary>
public enum CombatLogType
{
    /// <summary>Combat has started.</summary>
    CombatStart,

    /// <summary>A new round has begun.</summary>
    RoundStart,

    /// <summary>A combatant's turn has started.</summary>
    TurnStart,

    /// <summary>An attack was made.</summary>
    Attack,

    /// <summary>Damage was dealt.</summary>
    Damage,

    /// <summary>Healing was done.</summary>
    Heal,

    /// <summary>A combatant defended.</summary>
    Defend,

    /// <summary>A flee attempt was made.</summary>
    Flee,

    /// <summary>A combatant was defeated.</summary>
    Defeat,

    /// <summary>Combat has ended.</summary>
    CombatEnd,

    /// <summary>A status effect was applied (for v0.0.6c/d).</summary>
    StatusApplied,

    /// <summary>A status effect expired (for v0.0.6c/d).</summary>
    StatusExpired,

    /// <summary>AI decision made (for debugging/display).</summary>
    AIDecision
}
