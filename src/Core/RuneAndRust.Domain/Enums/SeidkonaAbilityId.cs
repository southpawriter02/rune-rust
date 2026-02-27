namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Seiðkona (Seeress) specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Seiðkona specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP): SeidrBolt, WyrdSight, AetherAttunement — Introduced in v0.20.8a</item>
/// <item>Tier 2 (Discipline, 8+ PP): FatesThread, WeaveDisruption, ResonanceCascade — Introduced in v0.20.8b</item>
/// <item>Tier 3 (Mastery, 16+ PP): VolvasVision, AetherStorm — Introduced in v0.20.8c</item>
/// <item>Capstone (Pinnacle, 24+ PP): Unraveling — Introduced in v0.20.8c</item>
/// </list>
/// <para>The Seiðkona is the only Mystic archetype specialization in the v0.20.x series and
/// represents the final Heretical path. Unlike the Berserkr's deterministic Corruption
/// (always triggers at 80+ Rage), the Seiðkona employs probability-based Corruption checks
/// (d100 vs percentage threshold) that scale with Aether Resonance level:
/// 0% at Resonance 0–4, 5% at 5–7, 15% at 8–9, 25% at 10.</para>
/// <para>The Seiðkona's unique Aether Resonance resource builds through casting and is NOT
/// consumed (except by the Unraveling capstone). This creates an escalating risk-reward
/// tension — more casting means more power but higher Corruption probability.</para>
/// <para>Ability IDs use the 800–808 range to avoid collision with other specializations
/// (Berserkr 1–9, Bone-Setter 600–608, Veiðimaðr 700–708).</para>
/// </remarks>
public enum SeidkonaAbilityId
{
    /// <summary>
    /// Tier 1 — Active ability: launch a bolt of raw Aetheric energy at a target.
    /// Deals 2d6 Aetheric damage, grants +1 Aether Resonance, and accumulates +1
    /// Aetheric damage for the Unraveling capstone tracker. Costs 1 AP. Subject to
    /// probability-based Corruption check if Resonance is 5+ (Heretical path).
    /// Introduced in v0.20.8a.
    /// </summary>
    SeidrBolt = 800,

    /// <summary>
    /// Tier 1 — Active ability: open the Wyrd Sight to perceive hidden truths.
    /// Detects invisible creatures, magic auras, and Corruption sources within a
    /// 10-space radius for 3 turns. Costs 2 AP. Does NOT grant Resonance and does
    /// NOT trigger a Corruption check — pure detection has no Aetheric cost.
    /// Introduced in v0.20.8a.
    /// </summary>
    WyrdSight = 801,

    /// <summary>
    /// Tier 1 — Passive ability: attunement to the Aether enhances natural recovery.
    /// Grants +10% AP regeneration rate. Always active while conscious. Does NOT
    /// grant Resonance (passive abilities do not channel Aether). No Corruption risk.
    /// Introduced in v0.20.8a.
    /// </summary>
    AetherAttunement = 802,

    /// <summary>
    /// Tier 2 — Active ability: weave the threads of fate to manipulate probability.
    /// 4 PP to unlock, requires 8 PP invested. Subject to Corruption check at current
    /// Resonance level (Heretical path). Introduced in v0.20.8b.
    /// </summary>
    FatesThread = 803,

    /// <summary>
    /// Tier 2 — Active ability: disrupt an enemy's magical weave, dealing damage
    /// and applying debuffs. 4 PP to unlock, requires 8 PP invested. Subject to
    /// Corruption check at current Resonance level (Heretical path). Introduced in v0.20.8b.
    /// </summary>
    WeaveDisruption = 804,

    /// <summary>
    /// Tier 2 — Active ability: channel accumulated Resonance into a cascading burst
    /// of Aetheric energy. 4 PP to unlock, requires 8 PP invested. Subject to
    /// Corruption check at current Resonance level (Heretical path). Introduced in v0.20.8b.
    /// </summary>
    ResonanceCascade = 805,

    /// <summary>
    /// Tier 3 — Active ability: invoke the Völva's prophetic vision to perceive
    /// future events and enemy intentions. 5 PP to unlock, requires 16 PP invested.
    /// Subject to Corruption check at current Resonance level (Heretical path).
    /// Introduced in v0.20.8c.
    /// </summary>
    VolvasVision = 806,

    /// <summary>
    /// Tier 3 — Active ability: unleash a devastating storm of raw Aetheric energy
    /// across a wide area. 5 PP to unlock, requires 16 PP invested. Subject to
    /// Corruption check at current Resonance level (Heretical path). Introduced in v0.20.8c.
    /// </summary>
    AetherStorm = 807,

    /// <summary>
    /// Capstone — Ultimate ability: release all accumulated Aetheric damage in a
    /// single cataclysmic detonation. Resets Aether Resonance and Accumulated Aetheric
    /// Damage to zero. The only ability that consumes Resonance. 6 PP to unlock,
    /// requires 24 PP invested. Guaranteed Corruption check at 20% (Heretical path).
    /// Introduced in v0.20.8c.
    /// </summary>
    Unraveling = 808
}
