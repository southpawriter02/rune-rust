using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for ambient condition processing (v0.3.3b).
/// Handles passive stat modifiers and turn-based tick effects.
/// </summary>
/// <remarks>See: SPEC-COND-001 for Ambient Condition System design.</remarks>
public interface IConditionService
{
    /// <summary>
    /// Gets the ambient condition for a room, if any.
    /// </summary>
    /// <param name="roomId">The room identifier.</param>
    /// <returns>The ambient condition if present; null otherwise.</returns>
    Task<AmbientCondition?> GetRoomConditionAsync(Guid roomId);

    /// <summary>
    /// Gets the passive stat modifiers for a condition type.
    /// </summary>
    /// <param name="type">The condition type.</param>
    /// <returns>Dictionary mapping attributes to their penalty values (negative).</returns>
    Dictionary<CharacterAttribute, int> GetStatModifiers(ConditionType type);

    /// <summary>
    /// Applies passive stat modifiers to a combatant based on room condition.
    /// Called at combat start when building Combatant from Character.
    /// </summary>
    /// <param name="combatant">The combatant to apply modifiers to.</param>
    /// <param name="conditionType">The condition type, or null if no condition.</param>
    void ApplyPassiveModifiers(Combatant combatant, ConditionType? conditionType);

    /// <summary>
    /// Processes turn-start tick effect for the active combatant.
    /// Handles damage, stress, and corruption effects based on condition's TickScript.
    /// </summary>
    /// <param name="combatant">The combatant whose turn is starting.</param>
    /// <param name="condition">The ambient condition to process.</param>
    /// <returns>Result describing what effects were applied.</returns>
    Task<ConditionTickResult> ProcessTurnTickAsync(Combatant combatant, AmbientCondition condition);
}
