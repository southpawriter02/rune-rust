namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of Berserkr-specific Corruption triggers.
/// Each trigger represents a combat action or state that may accumulate Corruption
/// through the Heretical Path mechanics.
/// </summary>
/// <remarks>
/// <para>Corruption triggers are evaluated at specific moments during combat:</para>
/// <list type="bullet">
/// <item>Combat entry: <see cref="EnterCombatEnraged"/> — starting combat at 80+ Rage</item>
/// <item>Ability usage: <see cref="FuryStrikeWhileEnraged"/>, <see cref="RecklessAssaultEnraged"/>,
///   <see cref="UnstoppableWhileEnraged"/>, <see cref="IntimidatingCoherentTarget"/></item>
/// <item>General: <see cref="AbilityWhileEnraged"/> — any ability at 80+ Rage</item>
/// <item>Kill events: <see cref="KillCoherentEnraged"/> — killing Coherent-aligned target while Enraged</item>
/// <item>Sustained state: <see cref="SustainedBerserkRage"/> — maintaining 100 Rage for extended periods</item>
/// <item>Capstone: <see cref="CapstoneActivation"/> — activating Avatar of Destruction</item>
/// <item>Tier 3: <see cref="FuryOfTheForlornUsage"/> — Fury of the Forlorn passive triggering</item>
/// </list>
/// <para>Corruption amounts vary by trigger severity: passive abilities are always safe,
/// while high-Rage active abilities typically generate +1 Corruption, and capstone
/// abilities generate +2 Corruption.</para>
/// </remarks>
public enum BerserkrCorruptionTrigger
{
    /// <summary>Entering combat while Rage is at 80+ (Enraged threshold). Corruption: +1.</summary>
    EnterCombatEnraged = 1,

    /// <summary>Using any ability while Rage is at 80+ (generic fallback). Corruption: +1.</summary>
    AbilityWhileEnraged = 2,

    /// <summary>Using Fury Strike while Rage is at 80+. Corruption: +1 (v0.20.5a).</summary>
    FuryStrikeWhileEnraged = 3,

    /// <summary>Using Reckless Assault while Rage is at 80+ (per-turn). Corruption: +1/turn (v0.20.5b).</summary>
    RecklessAssaultEnraged = 4,

    /// <summary>Using Unstoppable while Rage is at 80+. Corruption: +1 (v0.20.5b).</summary>
    UnstoppableWhileEnraged = 5,

    /// <summary>Using Intimidating Presence against a Coherent-aligned target. Corruption: +1 (v0.20.5b).</summary>
    IntimidatingCoherentTarget = 6,

    /// <summary>Fury of the Forlorn passive triggering. Corruption: +1 per trigger (v0.20.5c).</summary>
    FuryOfTheForlornUsage = 7,

    /// <summary>Activating Avatar of Destruction capstone. Corruption: +2 (v0.20.5c).</summary>
    CapstoneActivation = 8,

    /// <summary>Killing a Coherent-aligned target while Enraged. Corruption: +1.</summary>
    KillCoherentEnraged = 9,

    /// <summary>Maintaining maximum Rage (100) for 3+ consecutive turns. Corruption: +1/turn.</summary>
    SustainedBerserkRage = 10
}
