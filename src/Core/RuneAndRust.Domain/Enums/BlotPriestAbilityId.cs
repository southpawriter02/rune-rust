namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Unique numeric identifiers for all Blót-Priest specialization abilities.
/// Range: 30010–30018 (assigned in SPEC-MYSTIC-SPECS-001).
/// </summary>
/// <remarks>
/// <para>The Blót-Priest ("Sacrifice-Priest") is the Heretical Sacrificial Healer path
/// within the Mystic archetype. Its core identity is HP-based casting, life siphoning,
/// and Corruption transference — the most Corruption-intensive specialization in the system.</para>
///
/// <para>Ability tree structure (9 abilities across 4 tiers):</para>
/// <list type="table">
///   <listheader><term>Tier</term><description>PP Cost / Abilities</description></listheader>
///   <item><term>Tier 1 (Foundation)</term><description>3 PP each — Sanguine Pact, Blood Siphon, Gift of Vitae</description></item>
///   <item><term>Tier 2 (Discipline)</term><description>4 PP each — Blood Ward, Exsanguinate, Crimson Vigor</description></item>
///   <item><term>Tier 3 (Mastery)</term><description>5 PP each — Hemorrhaging Curse, Martyr's Resolve</description></item>
///   <item><term>Capstone (Ultimate)</term><description>6 PP — Heartstopper</description></item>
/// </list>
///
/// <para>Key mechanic: Sacrificial Casting converts HP to AP at a ratio that improves with rank
/// (2:1 → 1.5:1 → 1:1). Every HP-cast adds +1 Corruption. Healing allies via Gift of Vitae
/// transfers the Blót-Priest's own Corruption to the target.</para>
/// </remarks>
public enum BlotPriestAbilityId
{
    // ===== Tier 1: Foundation (3 PP each, starting Rank 1) =====

    /// <summary>
    /// <b>Sanguine Pact</b> — Tier 1 Passive (ID: 30010)
    /// </summary>
    /// <remarks>
    /// <para>Unlocks Sacrificial Casting: spend HP instead of AP to cast abilities.</para>
    /// <para>Conversion rate: R1 = 2 HP per 1 AP, R2 = 1.5 HP per 1 AP, R3 = 1 HP per 1 AP.</para>
    /// <para>Every HP-cast adds +1 Corruption (R3: +0.5, rounded up).</para>
    /// <para>Cannot reduce HP below 1.</para>
    /// </remarks>
    SanguinePact = 30010,

    /// <summary>
    /// <b>Blood Siphon</b> — Tier 1 Active (ID: 30011)
    /// </summary>
    /// <remarks>
    /// <para>Offensive attack that deals 3d6→5d6 damage and heals the caster for a percentage
    /// of damage dealt (Life Siphon mechanic).</para>
    /// <para>AP Cost: 2 AP. Siphon percentage: R1 = 25%, R2 = 35%, R3 = 50%.</para>
    /// <para>Self-Corruption: +1 per cast (from consuming Blighted life force).</para>
    /// </remarks>
    BloodSiphon = 30011,

    /// <summary>
    /// <b>Gift of Vitae</b> — Tier 1 Active (ID: 30012)
    /// </summary>
    /// <remarks>
    /// <para>Heals a single ally for 4d10→8d10 HP, but transfers a portion of the
    /// Blót-Priest's accumulated Corruption to the target (Blight Transference).</para>
    /// <para>AP Cost: 3 AP. Corruption transferred: R1 = 2, R2 = 1, R3 = 1 (reduced penalty).</para>
    /// <para>Self-Corruption: +1 per cast.</para>
    /// </remarks>
    GiftOfVitae = 30012,

    // ===== Tier 2: Discipline (4 PP each, requires 8 PP invested) =====

    /// <summary>
    /// <b>Blood Ward</b> — Tier 2 Active (ID: 30013)
    /// </summary>
    /// <remarks>
    /// <para>Sacrifices HP to create a temporary shield on self or ally.
    /// Shield value: 2.5–3.5× HP sacrificed (scaling by rank).</para>
    /// <para>AP Cost: 2 AP. Self-Corruption: +1 per cast.</para>
    /// <para>R1 = 2.5× multiplier, R2 = 3×, R3 = 3.5×. Stress attackers on hit.</para>
    /// </remarks>
    BloodWard = 30013,

    /// <summary>
    /// <b>Exsanguinate</b> — Tier 2 Active (ID: 30014)
    /// </summary>
    /// <remarks>
    /// <para>Applies a DoT curse that drains enemy HP each turn, healing the caster via lifesteal.</para>
    /// <para>AP Cost: 3 AP. Duration: 3 turns.</para>
    /// <para>Damage: R1 = 2d6/tick, R2 = 3d6/tick, R3 = 4d6/tick.</para>
    /// <para>Lifesteal: 25% of DoT damage. Self-Corruption: +1 per tick (total +3 per cast).</para>
    /// </remarks>
    Exsanguinate = 30014,

    /// <summary>
    /// <b>Crimson Vigor</b> — Tier 2 Passive (ID: 30015)
    /// </summary>
    /// <remarks>
    /// <para>Grants [Bloodied] bonuses when below 50% HP.</para>
    /// <para>R1: +50% healing potency, +25% siphon efficiency.</para>
    /// <para>R2: +75% healing potency, +40% siphon efficiency.</para>
    /// <para>R3: +100% healing potency, +60% siphon efficiency.</para>
    /// <para>No AP cost. No self-Corruption.</para>
    /// </remarks>
    CrimsonVigor = 30015,

    // ===== Tier 3: Mastery (5 PP each, requires 16 PP invested) =====

    /// <summary>
    /// <b>Hemorrhaging Curse</b> — Tier 3 Active (ID: 30016)
    /// </summary>
    /// <remarks>
    /// <para>Powerful DoT that also applies [Bleeding] and an anti-healing debuff (−50% healing received).</para>
    /// <para>AP Cost: 4 AP. Duration: 4 turns. Damage: 3d8/tick + lifesteal 30%.</para>
    /// <para>Self-Corruption: +2 per cast (fixed, no rank reduction).</para>
    /// </remarks>
    HemorrhagingCurse = 30016,

    /// <summary>
    /// <b>Martyr's Resolve</b> — Tier 3 Passive (ID: 30017)
    /// </summary>
    /// <remarks>
    /// <para>When [Bloodied] (below 50% HP): +5 Soak, +2d Resolve saves.</para>
    /// <para>At Rank 2 (via Capstone): +8 Soak, +3d Resolve.</para>
    /// <para>At Rank 3 (via Capstone): +10 Soak, +4d Resolve.</para>
    /// <para>No AP cost. No self-Corruption.</para>
    /// </remarks>
    MartyrsResolve = 30017,

    // ===== Capstone: Ultimate (6 PP, requires 24 PP invested) =====

    /// <summary>
    /// <b>Heartstopper</b> — Capstone Active (ID: 30018)
    /// </summary>
    /// <remarks>
    /// <para>Dual-mode capstone, once per combat:</para>
    /// <para><b>Crimson Deluge</b> (AoE Heal): Heals all allies for 8d10 HP but transfers
    /// +5 Corruption to each healed ally. Self-Corruption: +10.</para>
    /// <para><b>Final Anathema</b> (Execute): Deals massive damage to a single target.
    /// If target dies, transfers all target's remaining Corruption to the Blót-Priest.
    /// Self-Corruption: +15 (after transfer).</para>
    /// <para>AP Cost: 5 AP. Once per combat.</para>
    /// </remarks>
    Heartstopper = 30018
}
