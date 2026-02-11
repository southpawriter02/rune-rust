// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrCorruptionTrigger.cs
// Identifies specific conditions under which the Berserkr's Heretical abilities
// trigger Corruption risk. Each trigger maps to a unique corruption scenario.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies specific triggers for Corruption in the Berserkr specialization.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the Myrk-gengr (which triggers Corruption from light conditions),
/// the Berserkr's Corruption is combat-focused. Corruption primarily triggers
/// when abilities are used at high Rage (80+), reflecting the dangerous
/// loss of self-control inherent to the Heretical path.
/// </para>
/// <para>
/// <strong>Tier 1 Triggers (v0.20.5a):</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Entering combat while Enraged (80+ Rage): +1 Corruption</description></item>
///   <item><description>Using Fury Strike while Enraged: +1 Corruption</description></item>
///   <item><description>Passive abilities (Blood Scent, Pain is Fuel): No Corruption risk</description></item>
/// </list>
/// </remarks>
/// <seealso cref="BerserkrAbilityId"/>
/// <seealso cref="RageLevel"/>
public enum BerserkrCorruptionTrigger
{
    /// <summary>
    /// Entering combat while at 80+ Rage. The Berserkr's fury overwhelms
    /// their discipline before battle even begins. +1 Corruption.
    /// </summary>
    EnterCombatEnraged = 1,

    /// <summary>
    /// Using any active ability while at 80+ Rage. General trigger for
    /// non-specific ability usage during extreme fury. +1 Corruption.
    /// </summary>
    AbilityWhileEnraged = 2,

    /// <summary>
    /// Specifically using Fury Strike while at 80+ Rage.
    /// The channeled fury overwhelms restraint. +1 Corruption.
    /// Implemented in v0.20.5a.
    /// </summary>
    FuryStrikeWhileEnraged = 3,

    /// <summary>
    /// Using Unstoppable while at 80+ Rage (Tier 2).
    /// Planned for v0.20.5b.
    /// </summary>
    UnstoppableWhileEnraged = 4,

    /// <summary>
    /// Using Intimidating Presence against a Coherent target (Tier 2).
    /// Planned for v0.20.5b.
    /// </summary>
    IntimidatingCoherentTarget = 5,

    /// <summary>
    /// Activating Fury of the Forlorn (Tier 3).
    /// Planned for v0.20.5c.
    /// </summary>
    FuryOfTheForlornUsage = 6,

    /// <summary>
    /// Activating Avatar of Destruction (Capstone).
    /// Always triggers +2 Corruption. Planned for v0.20.5c.
    /// </summary>
    CapstoneActivation = 7,

    /// <summary>
    /// Killing a Coherent creature while Enraged.
    /// Planned for future version.
    /// </summary>
    KillCoherentEnraged = 8,

    /// <summary>
    /// Sustaining Berserk (100 Rage) for extended periods.
    /// Planned for future version.
    /// </summary>
    SustainedBerserkRage = 9
}
