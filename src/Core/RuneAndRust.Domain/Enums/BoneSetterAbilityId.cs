namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Bone-Setter specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Bone-Setter specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP): FieldDressing, Diagnose, SteadyHands — Introduced in v0.20.6a</item>
/// <item>Tier 2 (Discipline, 8+ PP): EmergencySurgery, AntidoteCraft, Triage — Introduced in v0.20.6b</item>
/// <item>Tier 3 (Mastery, 16+ PP): Resuscitate, PreventiveCare — Introduced in v0.20.6c</item>
/// <item>Capstone (Miracle, 24+ PP): MiracleWorker — Introduced in v0.20.6c</item>
/// </list>
/// <para>The Bone-Setter is the first dedicated healing specialization in the v0.20.x series.
/// As a Coherent path specialization under the Adept archetype, all abilities carry zero
/// Corruption risk regardless of context. The Bone-Setter relies on consumable Medical Supplies
/// and practical skill rather than dark power.</para>
/// <para>Ability IDs use the 600–608 range to avoid collision with other specializations.</para>
/// </remarks>
public enum BoneSetterAbilityId
{
    /// <summary>Tier 1 — Active ability: heal 2d6 + quality bonus + Steady Hands bonus, costs 2 AP + 1 Medical Supply (v0.20.6a).</summary>
    FieldDressing = 600,

    /// <summary>Tier 1 — Active ability: reveal target HP, status effects, vulnerabilities, and resistances, costs 1 AP (v0.20.6a).</summary>
    Diagnose = 601,

    /// <summary>Tier 1 — Passive ability: +2 bonus to all medical ability healing, always active (v0.20.6a).</summary>
    SteadyHands = 602,

    /// <summary>Tier 2 — Active ability: heal 4d6 HP, target gains Recovering condition, costs 4 AP + 2 Medical Supplies (v0.20.6b).</summary>
    EmergencySurgery = 603,

    /// <summary>Tier 2 — Active ability: craft Antidote from Herbs + salvage, DC 12 crafting check, costs 3 AP (v0.20.6b).</summary>
    AntidoteCraft = 604,

    /// <summary>Tier 2 — Passive ability: +50% healing to most wounded ally in area heals, +1 healing when any ally below 25% HP (v0.20.6b).</summary>
    Triage = 605,

    /// <summary>Tier 3 — Active ability: revive unconscious ally to 1 HP, requires Stimulant or DC 16 check, costs 4 AP (v0.20.6c).</summary>
    Resuscitate = 606,

    /// <summary>Tier 3 — Passive ability: allies within 6 spaces gain +1 to saves vs poison, disease, and Blight (v0.20.6c).</summary>
    PreventiveCare = 607,

    /// <summary>Capstone — Ultimate ability: full heal one ally + remove all negative conditions, costs 6 AP + 3 Medical Supplies, once per long rest (v0.20.6c).</summary>
    MiracleWorker = 608
}
