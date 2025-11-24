namespace RuneAndRust.Core.AI;

/// <summary>
/// Defines the AI behavior archetype for enemy combatants.
/// Each archetype has distinct decision-making priorities and tactics.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public enum AIArchetype
{
    /// <summary>
    /// Aggressive: Prioritizes high-damage targets, uses offensive abilities liberally,
    /// ignores defensive positioning, rushes toward enemies.
    /// Examples: Skar-Horde Berserkers, Magma Elementals
    /// </summary>
    Aggressive = 1,

    /// <summary>
    /// Defensive: Protects nearby allies, uses defensive abilities to shield others,
    /// positions between threats and allies, prioritizes intercepting attacks.
    /// Examples: Rusted Wardens, Iron-Bane Crusaders
    /// </summary>
    Defensive = 2,

    /// <summary>
    /// Cautious: Retreats when HP drops below 50%, uses cover extensively,
    /// prioritizes self-preservation, avoids risky engagements.
    /// Examples: Scavengers, Undying Scouts
    /// </summary>
    Cautious = 3,

    /// <summary>
    /// Reckless: Charges into melee regardless of danger, uses high-risk/high-reward abilities,
    /// ignores HP thresholds, no retreat logic.
    /// Examples: Corrupted Thralls, Blighted Hounds
    /// </summary>
    Reckless = 4,

    /// <summary>
    /// Tactical: Coordinates with allies, uses positioning to maximize advantage,
    /// prioritizes high-value targets, adapts to player strategy.
    /// Examples: Forge-Masters, Jötun Commanders
    /// </summary>
    Tactical = 5,

    /// <summary>
    /// Support: Prioritizes healing injured allies, stays at range,
    /// uses buffs on strongest allies, retreats when threatened.
    /// Examples: Bone-Menders, Reality Shapers
    /// </summary>
    Support = 6,

    /// <summary>
    /// Control: Uses status effects liberally, targets clustered enemies,
    /// prioritizes disabling high-threat targets, maintains distance.
    /// Examples: Frost-Weavers, Psychic Leeches
    /// </summary>
    Control = 7,

    /// <summary>
    /// Ambusher: Waits for optimal moment to strike, targets isolated enemies,
    /// uses positioning for flanking, retreats after burst damage.
    /// Examples: Rust-Stalkers, Shadow-Forms
    /// </summary>
    Ambusher = 8
}
