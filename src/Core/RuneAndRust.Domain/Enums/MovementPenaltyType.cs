namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of movement penalty types that abilities like Unstoppable can ignore.
/// Used by the Berserkr specialization's Tier 2 ability system to determine
/// which terrain and crowd-control effects are negated.
/// </summary>
/// <remarks>
/// Introduced in v0.20.5b as part of the Berserkr Tier 2 Abilities implementation.
/// Each value represents a distinct category of movement restriction that enemies
/// or terrain can impose on a character.
/// </remarks>
public enum MovementPenaltyType
{
    /// <summary>
    /// Rough terrain, debris, or obstacles that reduce movement speed (half speed or skip).
    /// Examples: rubble, collapsed hallways, dense undergrowth.
    /// </summary>
    DifficultTerrain = 1,

    /// <summary>
    /// Speed reduction effect imposed by spells, abilities, or environmental hazards.
    /// Examples: slow spell, frost aura, exhaustion.
    /// </summary>
    Slow = 2,

    /// <summary>
    /// Complete immobilization — the character cannot move at all.
    /// Examples: root spell, grapple, paralysis.
    /// </summary>
    Root = 3,

    /// <summary>
    /// Wading through water, mud, or other fluids that impede movement.
    /// Examples: flooded corridors, swamp terrain, shallow rivers.
    /// </summary>
    Water = 4,

    /// <summary>
    /// Vines, webs, chains, or other entangling effects that restrict mobility.
    /// Examples: spider webs, vine growth, chain snare.
    /// </summary>
    Entangle = 5,

    /// <summary>
    /// Being pushed, pulled, or otherwise forcibly moved against the character's will.
    /// When Unstoppable is active, the character gains advantage on saves to resist.
    /// Examples: push attack, gravitational pull, knockback.
    /// </summary>
    ForcedMovement = 6
}
