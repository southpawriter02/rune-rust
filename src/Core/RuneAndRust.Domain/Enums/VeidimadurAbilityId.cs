namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Veiðimaðr (Hunter) specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Veiðimaðr specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP): MarkQuarry, KeenSenses, ReadTheSigns — Introduced in v0.20.7a</item>
/// <item>Tier 2 (Discipline, 8+ PP): HuntersEye, TrapMastery, PredatorsPatience — Introduced in v0.20.7b</item>
/// <item>Tier 3 (Mastery, 16+ PP): ApexPredator, CripplingShot — Introduced in v0.20.7c</item>
/// <item>Capstone (Pinnacle, 24+ PP): ThePerfectHunt — Introduced in v0.20.7c</item>
/// </list>
/// <para>The Veiðimaðr is the first Coherent path Skirmisher specialization in the v0.20.x series,
/// providing a deliberate contrast to the Heretical Myrk-gengr (v0.20.4). As a Coherent path
/// specialization under the Skirmisher archetype, all abilities carry zero Corruption risk
/// regardless of context. The Veiðimaðr relies on Quarry Marks (target tracking) and
/// practical hunting skill rather than dark power.</para>
/// <para>Ability IDs use the 700–708 range to avoid collision with other specializations
/// (Berserkr 1–9, Bone-Setter 600–608).</para>
/// </remarks>
public enum VeidimadurAbilityId
{
    /// <summary>
    /// Tier 1 — Active ability: designate a visible enemy within 12 spaces as quarry,
    /// gaining +2 to all attack rolls against that target. Costs 1 AP. Mark persists until
    /// target is defeated or encounter ends. Maximum 3 marks; oldest replaced via FIFO when exceeded.
    /// No Corruption risk (Coherent path). Introduced in v0.20.7a.
    /// </summary>
    MarkQuarry = 700,

    /// <summary>
    /// Tier 1 — Passive ability: permanently grants +1 to all Perception checks and
    /// +1 to all Investigation checks. Always active while conscious. Stacks with other
    /// perception bonuses including Read the Signs. No Corruption risk (Coherent path).
    /// Introduced in v0.20.7a.
    /// </summary>
    KeenSenses = 701,

    /// <summary>
    /// Tier 1 — Active ability: investigate creature tracks, remains, or signs within touch range.
    /// Makes a skill check (1d20 + skill modifier + Keen Senses +1 + ability bonus +4) against a DC
    /// determined by track freshness. Success reveals creature type, count, direction, condition,
    /// and time passed. Costs 1 AP. No Corruption risk (Coherent path). Introduced in v0.20.7a.
    /// </summary>
    ReadTheSigns = 702,

    /// <summary>
    /// Tier 2 — Passive ability: ranged attacks ignore partial cover (+2 AC penalty negated).
    /// No AP cost (evaluated per attack). 4 PP to unlock, requires 8 PP invested.
    /// No Corruption risk (Coherent path). Introduced in v0.20.7b.
    /// </summary>
    HuntersEye = 703,

    /// <summary>
    /// Tier 2 — Active ability: place hunting traps (2 AP, 1d8 damage + immobilize) or
    /// detect traps within scan range (2 AP, +3 bonus + Keen Senses). Maximum 2 active traps.
    /// Costs 4 PP to unlock, requires 8 PP invested.
    /// No Corruption risk (Coherent path). Introduced in v0.20.7b.
    /// </summary>
    TrapMastery = 704,

    /// <summary>
    /// Tier 2 — Stance ability: while in stance and stationary, gain +3 to hit on all attacks.
    /// Costs 1 AP to enter, 0 AP to exit. Movement of any kind breaks the stance.
    /// Stacks with Quarry Mark (+2) for +5 total. Costs 4 PP to unlock, requires 8 PP invested.
    /// No Corruption risk (Coherent path). Introduced in v0.20.7b.
    /// </summary>
    PredatorsPatience = 705,

    /// <summary>
    /// Tier 3 — Passive ability: marked Quarry cannot benefit from concealment against the hunter.
    /// Applies to light obscurement, invisibility, magical camouflage, and stealth. Does not affect cover.
    /// Costs 5 PP to unlock, requires 16 PP invested.
    /// No Corruption risk (Coherent path). Introduced in v0.20.7c.
    /// </summary>
    ApexPredator = 706,

    /// <summary>
    /// Tier 3 — Attack ability: consumes 1 Quarry Mark to halve the target's movement speed
    /// for 2 turns. Guaranteed effect (no attack roll). Costs 1 AP, 5 PP to unlock,
    /// requires 16 PP invested. No Corruption risk (Coherent path). Introduced in v0.20.7c.
    /// </summary>
    CripplingShot = 707,

    /// <summary>
    /// Capstone — Ultimate ability: declare a Perfect Hunt against one marked Quarry.
    /// Automatic critical hit doubling all base damage. Consumes 1 Quarry Mark.
    /// Costs 3 AP, usable once per long rest. 6 PP to unlock, requires 24 PP invested.
    /// No Corruption risk (Coherent path). Introduced in v0.20.7c.
    /// </summary>
    ThePerfectHunt = 708
}
