using RuneAndRust.Core.Attributes;

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
    [GameDocument(
        "Bleeding",
        "A physical affliction causing damage over time. The wound seeps steadily, ignoring protective garments. Multiple wounds compound the blood loss. Field medics recommend bandages or alchemical coagulants.")]
    Bleeding = 0,

    /// <summary>
    /// Poison damage-over-time effect. Deals 1d6 × Stacks damage per turn.
    /// Applies armor soak to damage. Stacks up to 5 times with duration refresh.
    /// </summary>
    [GameDocument(
        "Poisoned",
        "A toxic contamination spreading through the body. Unlike open wounds, the damage manifests internally and armor offers some protection. Antidotes or time are the only remedies. Multiple exposures intensify the suffering.")]
    Poisoned = 1,

    /// <summary>
    /// Hard crowd control effect. Target loses their turn entirely.
    /// Does not stack; reapplication refreshes duration only.
    /// </summary>
    [GameDocument(
        "Stunned",
        "Complete incapacitation of motor function. The afflicted stands motionless, unable to act or defend themselves. A temporary state, but one that invites opportunistic strikes from nearby threats.")]
    Stunned = 2,

    /// <summary>
    /// Damage amplification debuff. Target takes +50% damage from all sources.
    /// Does not stack; reapplication refreshes duration only.
    /// </summary>
    [GameDocument(
        "Vulnerable",
        "A compromised defensive posture. The afflicted's guard is broken, their weak points exposed. All incoming harm lands with greater severity until they recover their stance.")]
    Vulnerable = 3,

    /// <summary>
    /// Mental impairment from near-breaking point. -1 to all dice pools.
    /// Does not stack; reapplication refreshes duration only.
    /// Acquired when stabilizing from a Breaking Point (v0.3.0c).
    /// </summary>
    [GameDocument(
        "Disoriented",
        "A fog of the mind following psychological trauma. The afflicted's thoughts scatter, making all tasks more difficult. Often observed after narrowly escaping a breaking point.")]
    Disoriented = 4,

    /// <summary>
    /// Out-of-combat debuff from resting without supplies.
    /// Halves all recovery from rest. Prevents running.
    /// Cured by resting at a Sanctuary or consuming supplies (v0.3.2a).
    /// </summary>
    [GameDocument(
        "Exhausted",
        "Bone-deep weariness from insufficient rest or sustenance. Recovery is halved and rapid movement becomes impossible. Only proper rest with supplies or sanctuary shelter can restore vitality.")]
    Exhausted = 5,

    /// <summary>
    /// Intel effect. Target's planned action is revealed to the player.
    /// Reveals enemy intent regardless of WITS check result.
    /// Does not stack; reapplication refreshes duration only (v0.3.6c).
    /// </summary>
    [GameDocument(
        "Analyzed",
        "The target's behavioral patterns have been catalogued. Their next action becomes predictable, revealing intent regardless of other factors. A tactical advantage for observant combatants.")]
    Analyzed = 6,

    /// <summary>
    /// Locked-in casting state. Enemy is charging a powerful ability.
    /// Cannot change actions while chanting. Vulnerable to interruption.
    /// Does not stack; reapplication refreshes duration only (v0.2.4c).
    /// </summary>
    [GameDocument(
        "Chanting",
        "A preparatory state for powerful abilities. The entity focuses entirely on channeling, unable to change course. This concentration makes them susceptible to interruption through sufficient harm.")]
    Chanting = 7,

    /// <summary>
    /// Magical silence effect. Target cannot cast spells (v0.4.3c).
    /// Does not stack; reapplication refreshes duration only.
    /// </summary>
    [GameDocument(
        "Silenced",
        "A suppression of magical ability. The afflicted's connection to the Aether is dampened, preventing spell casting. Physical abilities remain unaffected. Often caused by counter-magic or runic seals.")]
    Silenced = 8,

    /// <summary>
    /// Active concentration state. Caster is maintaining a concentration spell (v0.4.3c).
    /// Cannot cast other concentration spells while active.
    /// Does not stack; ends when concentration is broken or spell expires.
    /// </summary>
    [GameDocument(
        "Concentrating",
        "A state of focused magical maintenance. The caster sustains an ongoing spell effect, unable to channel another similar working. Taking damage or casting another concentration spell breaks this focus.")]
    Concentrating = 9,

    // ═══════════════════════════════════════════════════════════════════════
    // BUFFS (100+)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Defensive buff. Grants +2 armor soak per stack.
    /// Stacks up to 5 times with duration refresh.
    /// </summary>
    [GameDocument(
        "Fortified",
        "Enhanced defensive resilience. The beneficiary's armor or natural toughness is temporarily augmented, absorbing more punishment. Multiple applications compound this protection.")]
    Fortified = 100,

    /// <summary>
    /// Speed buff. Grants an additional action per turn.
    /// Does not stack. Reserved for future implementation.
    /// </summary>
    [GameDocument(
        "Hasted",
        "Accelerated combat reflexes. The beneficiary moves with supernatural speed, gaining additional actions within each combat round. A rare and powerful advantage.")]
    Hasted = 101,

    /// <summary>
    /// Offensive buff. Grants +1 bonus die to damage rolls.
    /// Does not stack. Reserved for future implementation.
    /// </summary>
    [GameDocument(
        "Inspired",
        "Combat fervor and heightened aggression. The beneficiary strikes with greater force, their attacks carrying additional weight. Morale effects and battle cries often produce this state.")]
    Inspired = 102
}
