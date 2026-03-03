namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Rust-Witch specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Rust-Witch specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP threshold): PhilosopherOfDust, CorrosiveCurse, EntropicField — 3 PP each</item>
/// <item>Tier 2 (Discipline, 8+ PP threshold): SystemShock, FlashRust, AcceleratedEntropy — 4 PP each</item>
/// <item>Tier 3 (Mastery, 16+ PP threshold): UnmakingWord, CascadeReaction — 5 PP each</item>
/// <item>Capstone (Pinnacle, 24+ PP threshold): EntropicCascade — 6 PP</item>
/// </list>
///
/// <para>The Rust-Witch is a Heretical Mystic specialization focused on the [Corroded] status effect.
/// Unlike the Seiðkona's probability-based Corruption (d100 vs percentage), the Rust-Witch uses
/// <strong>deterministic self-Corruption</strong>: every active ability inflicts a fixed amount of
/// Corruption on the caster. The amounts decrease slightly at Rank 3 for Tier 1-2 abilities but
/// remain fixed for Tier 3 and Capstone abilities.</para>
///
/// <para><b>[Corroded] Mechanic:</b> A stacking DoT status effect, max 5 stacks per target.
/// Base damage: 1d4/stack/turn. With Accelerated Entropy (25006): 2d6/stack/turn.
/// Each stack also imposes -1 Armor penalty. Stacks persist until cleansed.</para>
///
/// <para>Ability IDs use the 25001–25009 range per SPEC-RUST-WITCH-25001.</para>
/// </remarks>
public enum RustWitchAbilityId
{
    /// <summary>
    /// Tier 1 — Passive ability: the Rust-Witch's attunement to entropy grants enhanced
    /// analytical perception against corrupted entities. Adds bonus dice to analysis checks
    /// targeting enemies with [Corroded] stacks or Corruption above 0. Costs 3 PP to unlock.
    /// No self-Corruption (passive ability). No AP cost.
    /// </summary>
    PhilosopherOfDust = 25001,

    /// <summary>
    /// Tier 1 — Active ability: curse a target with corrosive entropy, applying [Corroded]
    /// stacks. Rank 1: 1 stack. Rank 2: 2 stacks. Rank 3: 3 stacks. Costs 2 AP.
    /// Self-Corruption: +2 (Rank 3: +1). Subject to [Corroded] max stack cap of 5/target.
    /// Costs 3 PP to unlock.
    /// </summary>
    CorrosiveCurse = 25002,

    /// <summary>
    /// Tier 1 — Passive ability: emanate a persistent aura of entropy that weakens nearby
    /// enemies' armor. Enemies within the aura suffer -1 Armor per [Corroded] stack they carry.
    /// Always active while conscious. No self-Corruption (passive). No AP cost. Costs 3 PP.
    /// </summary>
    EntropicField = 25003,

    /// <summary>
    /// Tier 2 — Active ability: deliver a concentrated burst of entropic energy that
    /// applies [Corroded] and [Stunned] (Mechanical enemies only). Costs 3 AP.
    /// Self-Corruption: +3 (Rank 3: +2). Requires 8+ PP invested. Costs 4 PP to unlock.
    /// </summary>
    SystemShock = 25004,

    /// <summary>
    /// Tier 2 — Active ability: unleash a wave of accelerated oxidation across all enemies
    /// in an area, applying [Corroded] stacks to every target. Costs 4 AP.
    /// Self-Corruption: +4 (Rank 3: +3). Requires 8+ PP invested. Costs 4 PP to unlock.
    /// </summary>
    FlashRust = 25005,

    /// <summary>
    /// Tier 2 — Passive ability: enhances [Corroded] DoT from base 1d4/stack/turn to
    /// 2d6/stack/turn. This is the primary damage amplifier for the Rust-Witch's DoT strategy.
    /// No self-Corruption (passive). No AP cost. Requires 8+ PP invested. Costs 4 PP to unlock.
    /// </summary>
    AcceleratedEntropy = 25006,

    /// <summary>
    /// Tier 3 — Active ability: speak a word of unmade creation that doubles the [Corroded]
    /// stacks on a target (still capped at 5). Costs 4 AP. Self-Corruption: +4 (all ranks).
    /// Requires 16+ PP invested. Costs 5 PP to unlock.
    /// </summary>
    UnmakingWord = 25007,

    /// <summary>
    /// Tier 3 — Passive ability: when a [Corroded] enemy dies, all remaining [Corroded]
    /// stacks spread to adjacent enemies within 1 tile. This is the primary chain-kill enabler.
    /// No self-Corruption (passive). No AP cost. Requires 16+ PP invested. Costs 5 PP to unlock.
    /// </summary>
    CascadeReaction = 25008,

    /// <summary>
    /// Capstone — Active ability: channel the full force of entropy against a single target.
    /// If the target has &gt;50 Corruption OR 5 [Corroded] stacks, instant kill (execute).
    /// Otherwise, deals 6d6 Arcane damage. Costs 5 AP. Self-Corruption: +6 (all ranks).
    /// Requires 24+ PP invested. Costs 6 PP to unlock.
    /// </summary>
    EntropicCascade = 25009
}
