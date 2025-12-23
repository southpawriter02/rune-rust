using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents an active ability that a combatant can use during combat.
/// Abilities define their costs, cooldowns, and effects through an EffectScript system.
/// </summary>
public class ActiveAbility
{
    /// <summary>
    /// Unique identifier for this ability.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name for the ability (e.g., "Power Strike", "Flame Bolt").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Flavor text description shown in UI (AAM-VOICE compliant).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Stamina cost to use this ability. 0 for free abilities.
    /// </summary>
    public int StaminaCost { get; set; }

    /// <summary>
    /// Aether cost to use this ability. Only usable by Mystic archetype.
    /// 0 for non-magical abilities.
    /// </summary>
    public int AetherCost { get; set; }

    /// <summary>
    /// Number of turns before the ability can be used again.
    /// 0 for abilities with no cooldown.
    /// </summary>
    public int CooldownTurns { get; set; }

    /// <summary>
    /// Range of the ability. 0 = Self, 1 = Melee, >1 = Ranged.
    /// </summary>
    public int Range { get; set; } = 1;

    /// <summary>
    /// Semicolon-separated effect commands that define ability behavior.
    /// Format: "COMMAND:Param1:Param2;COMMAND:Param1:Param2"
    /// Supported commands (v0.2.3b):
    /// - DAMAGE:Type:Dice (e.g., "DAMAGE:Physical:2d6")
    /// - HEAL:Amount (e.g., "HEAL:15")
    /// - STATUS:Type:Duration:Stacks (e.g., "STATUS:Bleeding:3:2")
    /// </summary>
    public string EffectScript { get; set; } = string.Empty;

    /// <summary>
    /// The character archetype that has access to this ability.
    /// Determines which characters can learn and use this ability.
    /// Null for enemy-specific abilities (v0.2.4a).
    /// </summary>
    public ArchetypeType? Archetype { get; set; }

    /// <summary>
    /// Tier level (1-3). Higher tiers unlock at higher character levels.
    /// Tier 1: Level 1+, Tier 2: Level 5+, Tier 3: Level 10+.
    /// </summary>
    public int Tier { get; set; } = 1;

    // ═══════════════════════════════════════════════════════════════════════
    // Telegraphed Ability Properties (v0.2.4c)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Number of turns required to charge before release. 0 = instant cast.
    /// During charge, the caster gains Chanting status (v0.2.4c).
    /// </summary>
    public int ChargeTurns { get; set; } = 0;

    /// <summary>
    /// AAM-VOICE compliant message displayed when charge begins.
    /// Example: "The Construct's core begins to glow violent red."
    /// If null, a default message is generated (v0.2.4c).
    /// </summary>
    public string? TelegraphMessage { get; set; }

    /// <summary>
    /// Damage threshold to interrupt the charge (percentage of MaxHP).
    /// Default 0.10 means 10% of enemy MaxHP must be dealt in one hit.
    /// Set to 1.0 for uninterruptible abilities (v0.2.4c).
    /// </summary>
    public float InterruptThreshold { get; set; } = 0.10f;
}
