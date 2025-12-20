namespace RuneAndRust.Core.Enums;

/// <summary>
/// Types of creature traits that can be applied to Elite/Champion enemies.
/// Traits provide stat modifiers and runtime behavior triggers.
/// </summary>
public enum CreatureTraitType
{
    // ═══════════════════════════════════════════════════════════════
    // STAT MODIFIERS (10-19)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// +3 ArmorSoak, reduced mobility (future).
    /// </summary>
    Armored = 10,

    /// <summary>
    /// +50% HP, immune to Stunned status effect.
    /// </summary>
    Relentless = 11,

    /// <summary>
    /// +25% damage dealt, +25% damage received.
    /// </summary>
    Berserker = 12,

    // ═══════════════════════════════════════════════════════════════
    // ON-HIT EFFECTS (20-29)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Heals for 25% of damage dealt on hit.
    /// </summary>
    Vampiric = 20,

    /// <summary>
    /// Applies 1 stack of Vulnerable on hit.
    /// </summary>
    Corrosive = 21,

    // ═══════════════════════════════════════════════════════════════
    // REACTIVE/PASSIVE (30-39)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Deals 15 AoE damage to all combatants on death.
    /// </summary>
    Explosive = 30,

    /// <summary>
    /// Regenerates 10% MaxHP at the start of each turn.
    /// </summary>
    Regenerating = 31,

    /// <summary>
    /// Reflects 25% of damage received back to attacker.
    /// </summary>
    Thorns = 32
}
