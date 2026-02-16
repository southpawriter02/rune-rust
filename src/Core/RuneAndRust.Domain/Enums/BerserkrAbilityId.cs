namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Berserkr specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Berserkr specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP): FuryStrike, BloodScent, PainIsFuel</item>
/// <item>Tier 2 (Discipline, 8+ PP): RecklessAssault, Unstoppable, IntimidatingPresence — Introduced in v0.20.5b</item>
/// <item>Tier 3 (Mastery, 16+ PP): FuryOfTheForlorn, DeathDefiance — Introduced in v0.20.5c</item>
/// <item>Capstone (Avatar, 24+ PP): AvatarOfDestruction — Introduced in v0.20.5c</item>
/// </list>
/// <para>The Berserkr archetype focuses on offensive aggression fueled by Rage,
/// trading defensive capability for escalating damage output. Higher Rage levels
/// unlock more powerful effects but risk Corruption accumulation.</para>
/// </remarks>
public enum BerserkrAbilityId
{
    /// <summary>Tier 1 — Active ability: weapon attack + 3d6 bonus damage, costs 20 Rage, nat 20 adds +1d6 (v0.20.5a).</summary>
    FuryStrike = 1,

    /// <summary>Tier 1 — Passive ability: +10 Rage when enemy becomes bloodied, +1 Attack vs bloodied targets (v0.20.5a).</summary>
    BloodScent = 2,

    /// <summary>Tier 1 — Passive ability: +5 Rage whenever the Berserkr takes damage (v0.20.5a).</summary>
    PainIsFuel = 3,

    /// <summary>Tier 2 — Stance ability: +4 Attack (scaling +1 per 20 Rage) at -2 Defense, 1 AP (v0.20.5b).</summary>
    RecklessAssault = 4,

    /// <summary>Tier 2 — Active ability: ignore all movement penalties for 2 turns, 1 AP + 15 Rage (v0.20.5b).</summary>
    Unstoppable = 5,

    /// <summary>Tier 2 — Active ability: force Will save or -2 Attack for 3 turns, 2 AP + 10 Rage (v0.20.5b).</summary>
    IntimidatingPresence = 6,

    /// <summary>Tier 3 — Passive ability: +2 Attack and +1d6 damage when no allies within 6 spaces (v0.20.5c).</summary>
    FuryOfTheForlorn = 7,

    /// <summary>Tier 3 — Reaction ability: survive lethal damage at 1 HP once per long rest (v0.20.5c).</summary>
    DeathDefiance = 8,

    /// <summary>Capstone — Ultimate ability: maximize Rage, double all Rage bonuses for 3 turns, once per combat (v0.20.5c).</summary>
    AvatarOfDestruction = 9
}
