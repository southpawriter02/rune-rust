namespace RuneAndRust.Core.Enums;

/// <summary>
/// Types of status effects that can be applied to combatants.
/// Debuffs are assigned values 0-99, buffs are assigned values 100+.
/// </summary>
public enum StatusEffectType
{
    // ═══════════════════════════════════════════════════════════════════════
    // DEBUFFS (0-99)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Physical damage-over-time effect. Deals 1d6 × Stacks damage per turn.
    /// Ignores armor soak. Stacks up to 5 times with duration refresh.
    /// </summary>
    Bleeding = 0,

    /// <summary>
    /// Poison damage-over-time effect. Deals 1d6 × Stacks damage per turn.
    /// Applies armor soak to damage. Stacks up to 5 times with duration refresh.
    /// </summary>
    Poisoned = 1,

    /// <summary>
    /// Hard crowd control effect. Target loses their turn entirely.
    /// Does not stack; reapplication refreshes duration only.
    /// </summary>
    Stunned = 2,

    /// <summary>
    /// Damage amplification debuff. Target takes +50% damage from all sources.
    /// Does not stack; reapplication refreshes duration only.
    /// </summary>
    Vulnerable = 3,

    /// <summary>
    /// Mental impairment from near-breaking point. -1 to all dice pools.
    /// Does not stack; reapplication refreshes duration only.
    /// Acquired when stabilizing from a Breaking Point (v0.3.0c).
    /// </summary>
    Disoriented = 4,

    /// <summary>
    /// Out-of-combat debuff from resting without supplies.
    /// Halves all recovery from rest. Prevents running.
    /// Cured by resting at a Sanctuary or consuming supplies (v0.3.2a).
    /// </summary>
    Exhausted = 5,

    // ═══════════════════════════════════════════════════════════════════════
    // BUFFS (100+)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Defensive buff. Grants +2 armor soak per stack.
    /// Stacks up to 5 times with duration refresh.
    /// </summary>
    Fortified = 100,

    /// <summary>
    /// Speed buff. Grants an additional action per turn.
    /// Does not stack. Reserved for future implementation.
    /// </summary>
    Hasted = 101,

    /// <summary>
    /// Offensive buff. Grants +1 bonus die to damage rolls.
    /// Does not stack. Reserved for future implementation.
    /// </summary>
    Inspired = 102
}
