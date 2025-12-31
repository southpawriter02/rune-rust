using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for managing spell casting during combat.
/// Mirrors the AbilityService pattern but adds Flux integration via IAetherService.
/// </summary>
/// <remarks>
/// See: v0.4.3c (The Incantation) for implementation details.
///
/// Validation Pipeline (7 checks in order):
/// 1. Magic eligibility (Archetype: Adept or Mystic)
/// 2. AP cost affordability
/// 3. Target type compatibility
/// 4. Range validation
/// 5. Target alive check
/// 6. Silenced status check
/// 7. Concentration conflict check
///
/// Flux Integration:
/// - On successful cast: AddFlux(spell.FluxCost)
/// - Publishes SpellCastEvent for UI/audio feedback
/// </remarks>
public interface IMagicService
{
    /// <summary>
    /// Checks whether a caster can cast a specific spell on a target.
    /// </summary>
    /// <param name="caster">The combatant attempting to cast.</param>
    /// <param name="spell">The spell to cast.</param>
    /// <param name="target">The intended target (can be caster for self-targeted spells).</param>
    /// <returns>True if the spell can be cast; false otherwise.</returns>
    bool CanCast(Combatant caster, Spell spell, Combatant? target);

    /// <summary>
    /// Casts a spell from the caster to the target.
    /// Deducts AP, applies effects, generates flux, and publishes SpellCastEvent.
    /// </summary>
    /// <param name="caster">The combatant casting the spell.</param>
    /// <param name="spell">The spell to cast.</param>
    /// <param name="target">The target of the spell (can be caster for self-targeted spells).</param>
    /// <returns>A MagicResult describing the outcome.</returns>
    MagicResult CastSpell(Combatant caster, Spell spell, Combatant target);

    /// <summary>
    /// Initiates a charged spell cast, applying Chanting status and storing the spell.
    /// The spell will be released after ChargeTurns via ReleaseCharge.
    /// </summary>
    /// <param name="caster">The combatant initiating the charge.</param>
    /// <param name="spell">The charged spell to prepare.</param>
    /// <returns>A MagicResult with the telegraph message.</returns>
    MagicResult InitiateCharge(Combatant caster, Spell spell);

    /// <summary>
    /// Releases a charged spell that has finished charging.
    /// Called when the Chanting status expires and the spell is ready.
    /// </summary>
    /// <param name="caster">The combatant releasing the charge.</param>
    /// <param name="spell">The spell being released.</param>
    /// <param name="target">The target of the spell.</param>
    /// <returns>A MagicResult describing the spell's effect.</returns>
    MagicResult ReleaseCharge(Combatant caster, Spell spell, Combatant target);

    /// <summary>
    /// Validates whether a target is valid for a specific spell.
    /// Checks target type compatibility and range.
    /// </summary>
    /// <param name="caster">The combatant attempting to cast.</param>
    /// <param name="spell">The spell to validate targeting for.</param>
    /// <param name="target">The intended target.</param>
    /// <returns>True if the target is valid; false otherwise.</returns>
    bool ValidateTarget(Combatant caster, Spell spell, Combatant target);

    /// <summary>
    /// Gets the reason why a spell cannot be cast.
    /// </summary>
    /// <param name="caster">The combatant attempting to cast.</param>
    /// <param name="spell">The spell that cannot be cast.</param>
    /// <param name="target">The intended target.</param>
    /// <returns>The CastFailureReason explaining why the cast is invalid.</returns>
    CastFailureReason GetFailureReason(Combatant caster, Spell spell, Combatant? target);
}
