// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrAbilityId.cs
// Strongly-typed identifiers for all Berserkr (Fury Warrior) specialization
// abilities, organized by tier.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies all abilities available to the Berserkr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Abilities are organized across four tiers, each requiring increasing
/// Proficiency Point (PP) investment to unlock:
/// </para>
/// <list type="bullet">
///   <item><description><b>Tier 1 (1–3):</b> Free, unlocked immediately</description></item>
///   <item><description><b>Tier 2 (4–6):</b> 4 PP each, requires 8 PP invested</description></item>
///   <item><description><b>Tier 3 (7–8):</b> 5 PP each, requires 16 PP invested</description></item>
///   <item><description><b>Capstone (9):</b> 6 PP, requires 24 PP invested</description></item>
/// </list>
/// <para>
/// The Berserkr is a Heretical Warrior specialization that channels Rage
/// to fuel devastating combat abilities. High Rage levels (80+) trigger
/// Corruption risk, reflecting the Heretical nature of the path.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="RageLevel"/>
/// <seealso cref="BerserkrCorruptionTrigger"/>
public enum BerserkrAbilityId
{
    // ═══════ Tier 1: Fury Foundation (Free, Rank 1) ═══════

    /// <summary>
    /// Active: channel Rage into a devastating melee strike.
    /// Costs 2 AP and 20 Rage. Deals weapon damage + 3d6 fury damage.
    /// On critical hit (natural 20): +1d6 bonus fury damage.
    /// Triggers +1 Corruption if used at 80+ Rage (Enraged/Berserk).
    /// Implemented in v0.20.5a.
    /// </summary>
    FuryStrike = 1,

    /// <summary>
    /// Passive: gain +10 Rage when any enemy becomes bloodied (≤50% HP).
    /// Grants +1 Attack bonus against bloodied targets.
    /// Does NOT trigger Corruption risk.
    /// Implemented in v0.20.5a.
    /// </summary>
    BloodScent = 2,

    /// <summary>
    /// Passive: gain +5 Rage when taking damage from any source.
    /// Fuels the combat escalation loop — taking hits generates Rage
    /// for more powerful abilities.
    /// Does NOT trigger Corruption risk.
    /// Implemented in v0.20.5a.
    /// </summary>
    PainIsFuel = 3,

    // ═══════ Tier 2: Battle Fury (4 PP each, Rank 2) ═══════

    /// <summary>
    /// Active: reckless all-out attack with increased damage but reduced defense.
    /// Planned for v0.20.5b.
    /// </summary>
    RecklessAssault = 4,

    /// <summary>
    /// Passive: resist effects that would stop or slow movement while enraged.
    /// Planned for v0.20.5b.
    /// </summary>
    Unstoppable = 5,

    /// <summary>
    /// Active: intimidate enemies, reducing their attack and defense.
    /// Planned for v0.20.5b.
    /// </summary>
    IntimidatingPresence = 6,

    // ═══════ Tier 3: Fury Mastery (5 PP each, Rank 3) ═══════

    /// <summary>
    /// Active: unleash accumulated fury in an area-of-effect attack.
    /// Planned for v0.20.5c.
    /// </summary>
    FuryOfTheForlorn = 7,

    /// <summary>
    /// Passive: when reduced to 0 HP while enraged, survive with 1 HP once per combat.
    /// Planned for v0.20.5c.
    /// </summary>
    DeathDefiance = 8,

    // ═══════ Capstone: Ultimate (6 PP, Rank 4) ═══════

    /// <summary>
    /// Ultimate: transform into an avatar of destruction for 3 turns.
    /// Massive stat bonuses, immunity to crowd control, automatic Rage generation.
    /// Always applies +2 Corruption. Usable once per combat.
    /// Planned for v0.20.5c.
    /// </summary>
    AvatarOfDestruction = 9
}
