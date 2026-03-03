namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Echo-Caller specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Echo-Caller specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP threshold): EchoAttunement, ScreamOfSilence, PhantomMenace — 3 PP each</item>
/// <item>Tier 2 (Discipline, 8+ PP threshold): EchoCascade, RealityFracture, TerrorFeedback — 4 PP each</item>
/// <item>Tier 3 (Mastery, 16+ PP threshold): FearCascade, EchoDisplacement — 5 PP each</item>
/// <item>Capstone (Pinnacle, 24+ PP threshold): SilenceMadeWeapon — 6 PP</item>
/// </list>
///
/// <para>The Echo-Caller is a Coherent Mystic specialization focused on Psychic Artillery and Crowd Control,
/// utilizing the [Echo] chain system and [Feared] status manipulation. Unlike the Rust-Witch (deterministic Corruption)
/// or Blót-Priest (Corruption-driven), the Echo-Caller is a Coherent path with no Corruption mechanics.
/// Instead, abilities leverage echo chains that propagate damage through adjacent targets and fear mechanics
/// that scale damage and enable crowd control.</para>
///
/// <para><b>[Echo] Chain Mechanic:</b> A damage propagation system where abilities trigger on primary targets
/// and bounce to adjacent enemies. Base range: 1 tile (50% damage to 1 target). Enhanced by EchoCascade (Rank 2: 70%/2 tiles, Rank 3: 80%/3 tiles/2 targets).
/// Echoes do not trigger [Feared] or additional effects — they are pure damage bounces.</para>
///
/// <para><b>[Feared] Status Effect:</b> An applied crowd control status that increases damage taken from Echo-Caller abilities.
/// Enemies with [Feared] take +1d8 bonus damage from Psychic attacks (Rank 2: +2d8). TerrorFeedback passive returns +15 Aether
/// whenever a fear is applied via PhantomMenace.</para>
///
/// <para>Ability IDs use the 28010–28018 range per SPEC-MYSTIC-SPECS-001.</para>
/// </remarks>
public enum EchoCallerAbilityId
{
    /// <summary>
    /// Tier 1 — Passive ability: the Echo-Caller's attunement to psychic resonance grants
    /// enhanced Aether efficiency and psychic resistance. Reduces Aether costs by 1 (minimum 1) and
    /// adds +2 Psychic Resistance. Always active while conscious. No AP cost. No PP unlock cost (passive ability).
    /// </summary>
    EchoAttunement = 28010,

    /// <summary>
    /// Tier 1 — Active ability: unleash a scream of psychic silence that damages a target
    /// and bounces to nearby allies via [Echo] chains. Base Damage: 2d6 Psychic. If target is [Feared],
    /// adds +1d8 bonus damage (Rank 2: +2d8). [Echo] chain applies to 1 adjacent target (base 50% damage).
    /// Costs 2 AP. Costs 3 PP to unlock.
    /// </summary>
    ScreamOfSilence = 28011,

    /// <summary>
    /// Tier 1 — Active ability: invoke a phantom menace to strike terror in the target.
    /// Applies [Feared] status effect (duration: 3 turns base, 4 turns Rank 2+). Triggers [Echo] chain
    /// to 1 adjacent target (50% damage, no fear applied to echoes). Costs 2 AP.
    /// If TerrorFeedback passive is unlocked, grants +15 Aether when fear is applied.
    /// Costs 3 PP to unlock.
    /// </summary>
    PhantomMenace = 28012,

    /// <summary>
    /// Tier 2 — Passive ability: enhance the Echo-Caller's ability to chain psychic damage
    /// through resonance networks. Increases [Echo] chain range and damage percentage scaling.
    /// Rank 1: 1 tile range, 50% damage, 1 target (base).
    /// Rank 2: 2 tiles range, 70% damage, 2 targets.
    /// Rank 3: 3 tiles range, 80% damage, 2 targets.
    /// Requires 8+ PP invested. No AP cost. Costs 4 PP to unlock.
    /// </summary>
    EchoCascade = 28013,

    /// <summary>
    /// Tier 2 — Active ability: fracture reality with psychic force, dealing damage and applying
    /// crowd control debuffs to a target. Damage: 3d6 Psychic. Applies [Disoriented] (−1 Accuracy, −1 Dodge).
    /// Pushes target 1 tile away. Triggers [Echo] chain to 1-2 adjacent targets (base 50%, enhanced by EchoCascade).
    /// Costs 3 AP. Requires 8+ PP invested. Costs 4 PP to unlock.
    /// </summary>
    RealityFracture = 28014,

    /// <summary>
    /// Tier 2 — Passive ability: when the Echo-Caller applies [Feared] status via PhantomMenace,
    /// restore +15 Aether to the caster. Triggers on successful fear application only (not on echo chains).
    /// No AP cost. No self-resource cost. Requires 8+ PP invested. Costs 4 PP to unlock.
    /// </summary>
    TerrorFeedback = 28015,

    /// <summary>
    /// Tier 3 — Active ability: unleash a cascade of fear through an area, applying [Feared]
    /// to all enemies in range. Base radius: 3 tiles. Duration: 3 turns (4 turns Rank 2+).
    /// Triggers [Echo] chains independently for each target, with TerrorFeedback restoring Aether per fear applied.
    /// Costs 4 AP. Requires 16+ PP invested. Costs 5 PP to unlock.
    /// </summary>
    FearCascade = 28016,

    /// <summary>
    /// Tier 3 — Active ability: force a target to be displaced to a new location via psychic force.
    /// The target is teleported 2 tiles away (or to the furthest accessible position). Triggers [Echo] chains
    /// based on original position to adjacent targets. Applies [Disoriented] on arrival.
    /// Costs 4 AP. Requires 16+ PP invested. Costs 5 PP to unlock.
    /// </summary>
    EchoDisplacement = 28017,

    /// <summary>
    /// Capstone — Ultimate ability: weaponize silence itself, dealing massive psychic damage
    /// scaling with the number of [Feared] enemies in the area. Base damage: 4d10. Bonus damage: +2d10 per [Feared] target.
    /// Example: 2 feared enemies = 4d10 + 4d10 total. Triggers [Echo] chains to all adjacent targets of the primary target.
    /// Costs 5 AP. Once per combat. Requires 24+ PP invested. Costs 6 PP to unlock.
    /// </summary>
    SilenceMadeWeapon = 28018
}
