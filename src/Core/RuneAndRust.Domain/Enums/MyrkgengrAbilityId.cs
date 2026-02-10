// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrAbilityId.cs
// Strongly-typed identifiers for all Myrk-gengr (Shadow-Walker) specialization
// abilities, organized by tier.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies all abilities available to the Myrk-gengr specialization.
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
/// The Myrk-gengr is a Heretical Skirmisher specialization that manipulates
/// shadows and darkness. All abilities interact with the <see cref="LightLevelType"/>
/// system, and many carry Corruption risk when used outside of shadow.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="LightLevelType"/>
/// <seealso cref="ShadowAbilityType"/>
public enum MyrkgengrAbilityId
{
    // ═══════ Tier 1: Shadow Foundation (Free, Rank 1) ═══════

    /// <summary>
    /// Active: teleport to a shadow location within 6 spaces.
    /// Costs 2 AP and 10 Shadow Essence. Does not require line of sight.
    /// Generates +5 Shadow Essence on arrival in Darkness.
    /// Triggers Corruption risk if used from bright light.
    /// Implemented in v0.20.4a.
    /// </summary>
    ShadowStep = 1,

    /// <summary>
    /// Stance: enter a shadow concealment stance.
    /// Costs 1 AP to enter, 5 Shadow Essence per turn to maintain.
    /// Grants +3 Stealth in shadows, silent movement in shadows.
    /// Triggers Corruption risk if maintained in bright light.
    /// Automatically ends if Shadow Essence is depleted.
    /// Implemented in v0.20.4a.
    /// </summary>
    CloakOfNight = 2,

    /// <summary>
    /// Passive: removes all penalties associated with dim light.
    /// Affects Perception, Attack, Skill checks, and Movement.
    /// Does NOT grant darkvision or remove penalties for true Darkness.
    /// Passively generates +2 Shadow Essence per turn in Darkness.
    /// Implemented in v0.20.4a.
    /// </summary>
    DarkAdapted = 3,

    // ═══════ Tier 2: Shadow Mastery (4 PP each, Rank 2) ═══════

    /// <summary>
    /// Active: shadow-infused melee strike with bonus damage from darkness.
    /// Planned for v0.20.4b.
    /// </summary>
    UmbralStrike = 4,

    /// <summary>
    /// Active: create a shadow duplicate that acts as a decoy.
    /// Planned for v0.20.4b.
    /// </summary>
    ShadowClone = 5,

    /// <summary>
    /// Passive: gain resistance to shadow/void damage and enhanced
    /// perception in darkness.
    /// Planned for v0.20.4b.
    /// </summary>
    VoidTouched = 6,

    // ═══════ Tier 3: Shadow Dominion (5 PP each, Rank 3) ═══════

    /// <summary>
    /// Active transformation: merge with shadows to become incorporeal.
    /// Costs 3 AP and 25 Shadow Essence. Duration: 1 turn (extendable).
    /// Grants phasing through objects and immunity to physical attacks.
    /// Vulnerable to magical light (2d6 damage). Corruption risk on extension.
    /// Implemented in v0.20.4c.
    /// </summary>
    MergeWithDarkness = 7,

    /// <summary>
    /// Active control: ensnare a target in shadow tendrils.
    /// Costs 2 AP and 20 Shadow Essence. Duration: 2 turns.
    /// Roots target in place with DC 14 save to escape.
    /// Corruption risk if used on Coherent-aligned or innocent targets.
    /// Implemented in v0.20.4c.
    /// </summary>
    ShadowSnare = 8,

    // ═══════ Capstone: Ultimate (6 PP, Rank 4) ═══════

    /// <summary>
    /// Ultimate: create a massive zone of shadow that extinguishes all light.
    /// Costs 5 AP and 40 Shadow Essence. Duration: 3 turns.
    /// 8-space radius darkness zone. Caster gains 50% concealment and
    /// regenerates 10 Essence per turn. Enemies are blinded and suffer penalties.
    /// Always applies +2 Corruption. Usable once per combat.
    /// Implemented in v0.20.4c.
    /// </summary>
    Eclipse = 9
}
