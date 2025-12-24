using RuneAndRust.Core.Attributes;

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
    [GameDocument(
        "Armored",
        "Thickened hide or protective plating significantly increases damage absorption. These creatures shrug off blows that would fell lesser beasts, though their bulk may impede swift movement.")]
    Armored = 10,

    /// <summary>
    /// +50% HP, immune to Stunned status effect.
    /// </summary>
    [GameDocument(
        "Relentless",
        "Exceptional vitality and unstoppable determination. These creatures possess enhanced endurance and cannot be stunned by conventional means. Retreat may be wiser than confrontation.")]
    Relentless = 11,

    /// <summary>
    /// +25% damage dealt, +25% damage received.
    /// </summary>
    [GameDocument(
        "Berserker",
        "Frenzied aggression trading defense for offense. These creatures strike with devastating force but expose themselves to counter-attack. Exploit the opening their recklessness creates.")]
    Berserker = 12,

    // ═══════════════════════════════════════════════════════════════
    // ON-HIT EFFECTS (20-29)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Heals for 25% of damage dealt on hit.
    /// </summary>
    [GameDocument(
        "Vampiric",
        "Life-draining properties that restore vitality with each successful strike. Prolonged combat favors these creatures as they recover while inflicting harm. Swift elimination is essential.")]
    Vampiric = 20,

    /// <summary>
    /// Applies 1 stack of Vulnerable on hit.
    /// </summary>
    [GameDocument(
        "Corrosive",
        "Secretions or energies that weaken defensive capabilities. Each strike from these creatures leaves the victim more susceptible to subsequent harm. Avoid extended exchanges.")]
    Corrosive = 21,

    // ═══════════════════════════════════════════════════════════════
    // REACTIVE/PASSIVE (30-39)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Deals 15 AoE damage to all combatants on death.
    /// </summary>
    [GameDocument(
        "Explosive",
        "Volatile internal chemistry or unstable Blight-energy that detonates upon death. All nearby combatants suffer harm when the creature falls. Maintain distance before delivering the killing blow.")]
    Explosive = 30,

    /// <summary>
    /// Regenerates 10% MaxHP at the start of each turn.
    /// </summary>
    [GameDocument(
        "Regenerating",
        "Rapid tissue restoration that heals wounds between strikes. Unless damage is dealt continuously, these creatures recover steadily. Sustained pressure prevents recovery.")]
    Regenerating = 31,

    /// <summary>
    /// Reflects 25% of damage received back to attacker.
    /// </summary>
    [GameDocument(
        "Thorns",
        "Defensive spines or reactive energies that harm attackers. A portion of all damage dealt returns to the striker. Ranged attacks or high single strikes minimize retaliation.")]
    Thorns = 32
}
