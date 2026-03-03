namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Varð-Warden specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Varð-Warden specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP threshold): SanctifiedResolve, RunicBarrier, ConsecrateGround — 3 PP each</item>
/// <item>Tier 2 (Discipline, 8+ PP threshold): RuneOfShielding, ReinforceWard, WardensVigil — 4 PP each</item>
/// <item>Tier 3 (Mastery, 16+ PP threshold): GlyphOfSanctuary, AegisOfSanctity — 5 PP each</item>
/// <item>Capstone (Pinnacle, 24+ PP threshold): IndomitableBastion — 6 PP</item>
/// </list>
///
/// <para>The Varð-Warden ("Guardian-Warden") is a Coherent Mystic specialization focused on Defensive Casting
/// and Battlefield Control. Unlike the Rust-Witch's deterministic Corruption or the Blót-Priest's self-sacrifice mechanics,
/// the Varð-Warden uses NO Corruption and instead creates barriers, zones, ally buffs, and has a once-per-expedition
/// reaction capstone that can negate fatal damage.</para>
///
/// <para><b>Core Mechanics:</b></para>
/// <list type="bullet">
/// <item>Barriers: Protective structures with scaling HP (30/40/50 at R1/R2/R3) and limited duration (2/3/4 turns).</item>
/// <item>Zones: Persistent area effects that heal/damage per turn or provide benefits to allies and detriments to enemies.</item>
/// <item>Ally Buffs: RuneOfShielding grants +Soak and Corruption resistance; GlyphOfSanctuary grants temp HP and Stress immunity.</item>
/// <item>Once-Per-Expedition Capstone: IndomitableBastion negates fatal damage, creates a barrier, and can only be used once per expedition.</item>
/// <item>Row-Wide Control: WardensVigil grants Stress resistance to all allies in a row without consuming AP.</item>
/// </list>
///
/// <para>Ability IDs use the 29010–29018 range (reassigned from original 28010–28018 to avoid collision with Echo-Caller)
/// per SPEC-MYSTIC-SPECS-001 revision for Varð-Warden coherent path definition.</para>
/// </remarks>
public enum VardWardenAbilityId
{
    /// <summary>
    /// Tier 1 — Passive ability: the Varð-Warden's sacred attunement grants enhanced resistance to
    /// pushes and pulls. Adds +1d10 WILL dice vs Push/Pull effects. No ranks, always active while conscious.
    /// No AP cost. Costs 3 PP to unlock.
    /// </summary>
    SanctifiedResolve = 29010,

    /// <summary>
    /// Tier 1 — Active ability: create a protective rune-ward barrier at a designated location.
    /// Barrier HP: R1 = 30 HP, R2 = 40 HP, R3 = 50 HP. Duration: R1 = 2 turns, R2 = 3 turns, R3 = 4 turns.
    /// Costs 3 AP. No Corruption. Costs 3 PP to unlock.
    /// </summary>
    RunicBarrier = 29011,

    /// <summary>
    /// Tier 1 — Active ability: consecrate the ground in an area, creating a persistent zone
    /// that heals allies and damages Blighted/Undying enemies each turn. Healing: R1 = 1d6, R2 = 1d6+2, R3 = 2d6 per turn.
    /// Damage to enemies: same. Duration: 3-4 turns. Costs 3 AP. No Corruption. Costs 3 PP to unlock.
    /// </summary>
    ConsecrateGround = 29012,

    /// <summary>
    /// Tier 2 — Active ability: engrave a protective rune on an ally, granting +Soak and Corruption resistance.
    /// Soak bonus: R1 = +3, R2 = +5, R3 = +7. Corruption resistance: R1 = +10%, R2 = +15%, R3 = +20%.
    /// Duration: 4 turns. Costs 3 AP. Requires 8+ PP invested. No Corruption. Costs 4 PP to unlock.
    /// </summary>
    RuneOfShielding = 29013,

    /// <summary>
    /// Tier 2 — Active ability: reinforce an existing barrier (restore HP) or boost an active zone
    /// (increase healing/damage). Barrier reinforce: restore 15/20/25 HP (R1/R2/R3). Zone boost: +50%/+75%/+100% effectiveness.
    /// Costs 2 AP. Requires 8+ PP invested. No Corruption. Costs 4 PP to unlock.
    /// </summary>
    ReinforceWard = 29014,

    /// <summary>
    /// Tier 2 — Passive ability: the Varð-Warden's sanctified presence grants row-wide Stress resistance.
    /// All allies in the same row as the Warden gain +1 Stress resistance per turn while conscious.
    /// No ranks, always active. No AP cost. Requires 8+ PP invested. No Corruption. Costs 4 PP to unlock.
    /// </summary>
    WardensVigil = 29015,

    /// <summary>
    /// Tier 3 — Active ability: inscribe a glyph of sanctuary that grants all allies in range
    /// temporary HP and Stress immunity. Temp HP: 3d10 per ally. Stress immunity: 2 turns.
    /// Duration: instant application, effects persist per description. Costs 4 AP. Requires 16+ PP invested.
    /// No Corruption. Costs 5 PP to unlock.
    /// </summary>
    GlyphOfSanctuary = 29016,

    /// <summary>
    /// Tier 3 — Passive ability: barriers created by the Varð-Warden reflect a portion of blocked damage
    /// back to attackers and automatically cleanse nearby zones of hostile effects.
    /// Barrier reflection: 25%/35%/50% of blocked damage (R1/R2/R3) returned as damage to attacker.
    /// Zone cleanse: removes 1/2/3 hostile status stacks from zone area (R1/R2/R3).
    /// No AP cost. Requires 16+ PP invested. No Corruption. Costs 5 PP to unlock.
    /// </summary>
    AegisOfSanctity = 29017,

    /// <summary>
    /// Capstone — Reaction ability: an indomitable bastion of pure sanctity that negates fatal damage
    /// to a nearby ally and creates a barrier protecting them. Once per expedition only (not per combat).
    /// Creates a 30 HP barrier on the saved ally. Must be triggered as a reaction.
    /// No AP cost (reaction ability). Requires 24+ PP invested. No Corruption. Costs 6 PP to unlock.
    /// </summary>
    IndomitableBastion = 29018
}
