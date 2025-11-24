namespace RuneAndRust.Core.AI;

/// <summary>
/// Factors considered when assessing the threat level of a target.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public enum ThreatFactor
{
    /// <summary>
    /// How much damage the target deals per turn.
    /// Higher damage output = higher threat.
    /// </summary>
    DamageOutput,

    /// <summary>
    /// Current HP of the target (inverse - low HP = easier kill = higher priority).
    /// Lower HP = higher threat score for finishing off wounded targets.
    /// </summary>
    CurrentHP,

    /// <summary>
    /// Tactical positioning advantage/disadvantage.
    /// Considers: elevation, cover, isolation, flanking opportunities.
    /// </summary>
    Positioning,

    /// <summary>
    /// Threat from the target's available abilities.
    /// Considers: ability damage, CC potential, AOE capabilities, cooldown status.
    /// </summary>
    Abilities,

    /// <summary>
    /// Active status effects on the target.
    /// Buffs increase threat, debuffs decrease threat.
    /// </summary>
    StatusEffects,

    /// <summary>
    /// Distance from the AI actor.
    /// Closer targets may be higher priority for melee, lower for ranged.
    /// </summary>
    Proximity
}
