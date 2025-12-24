using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Manages status effect lifecycle: application, ticking, and removal.
/// Provides methods for effect application, turn processing, and stat modifiers.
/// </summary>
/// <remarks>See: SPEC-STATUS-001 for Status Effect System design.</remarks>
public interface IStatusEffectService
{
    /// <summary>
    /// Applies a status effect to a combatant. Handles stacking and duration refresh logic.
    /// Stackable effects (Bleeding, Poisoned, Fortified) increase stacks up to maximum.
    /// Non-stackable effects (Stunned, Vulnerable) refresh duration only.
    /// </summary>
    /// <param name="target">The combatant to apply the effect to.</param>
    /// <param name="type">The type of status effect to apply.</param>
    /// <param name="duration">Duration in turns.</param>
    /// <param name="sourceId">The ID of the combatant applying the effect.</param>
    void ApplyEffect(Combatant target, StatusEffectType type, int duration, Guid sourceId);

    /// <summary>
    /// Removes all instances of a specific effect type from a combatant.
    /// </summary>
    /// <param name="target">The combatant to remove the effect from.</param>
    /// <param name="type">The type of status effect to remove.</param>
    void RemoveEffect(Combatant target, StatusEffectType type);

    /// <summary>
    /// Removes all status effects from a combatant. Used when combat ends.
    /// </summary>
    /// <param name="target">The combatant to clear effects from.</param>
    void ClearAllEffects(Combatant target);

    /// <summary>
    /// Processes the start of a combatant's turn. Applies DoT damage.
    /// Bleeding damage ignores soak; Poisoned damage applies soak.
    /// </summary>
    /// <param name="combatant">The combatant whose turn is starting.</param>
    /// <returns>Total damage dealt from DoT effects (before any HP modification).</returns>
    int ProcessTurnStart(Combatant combatant);

    /// <summary>
    /// Checks if the combatant can act this turn.
    /// Returns false if the combatant has the Stunned effect.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>True if the combatant can act; false if stunned.</returns>
    bool CanAct(Combatant combatant);

    /// <summary>
    /// Processes the end of a combatant's turn.
    /// Decrements all effect durations and removes expired effects.
    /// </summary>
    /// <param name="combatant">The combatant whose turn is ending.</param>
    void ProcessTurnEnd(Combatant combatant);

    /// <summary>
    /// Gets the soak modifier from status effects.
    /// Fortified grants +2 soak per stack.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>Total soak bonus from status effects.</returns>
    int GetSoakModifier(Combatant combatant);

    /// <summary>
    /// Gets the damage multiplier from status effects.
    /// Vulnerable applies a 1.5x damage multiplier.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>Damage multiplier (1.0f for no modifier, 1.5f for Vulnerable).</returns>
    float GetDamageMultiplier(Combatant combatant);

    /// <summary>
    /// Gets all active status effects on a combatant for display purposes.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>Read-only list of active status effects.</returns>
    IReadOnlyList<ActiveStatusEffect> GetActiveEffects(Combatant combatant);

    /// <summary>
    /// Checks if a combatant has a specific effect type active.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <param name="type">The effect type to look for.</param>
    /// <returns>True if the effect is active; false otherwise.</returns>
    bool HasEffect(Combatant combatant, StatusEffectType type);

    /// <summary>
    /// Gets the stack count of a specific effect on a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <param name="type">The effect type to query.</param>
    /// <returns>Number of stacks, or 0 if effect is not present.</returns>
    int GetEffectStacks(Combatant combatant, StatusEffectType type);
}
