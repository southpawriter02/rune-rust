// ═══════════════════════════════════════════════════════════════════════════════
// IStatusEffectService.cs
// Application Layer interface for applying and managing status effects on
// characters. Status effects include conditions like Stunned, Prone, Unconscious,
// Blinded, etc. that modify gameplay mechanics.
// Version: 0.18.2d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for managing character status effects.
/// </summary>
/// <remarks>
/// <para>
/// Status effects are temporary conditions that modify character capabilities.
/// This service provides methods for applying, querying, and removing effects.
/// </para>
/// <para>
/// <strong>Common Status Effects:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Stunned: Cannot take actions (from Panic Table, combat)</description></item>
///   <item><description>Prone: -2 Defense, disadvantage on attacks</description></item>
///   <item><description>Unconscious: Cannot act, auto-fail Agility saves</description></item>
///   <item><description>Blinded: Disadvantage on attack, advantage for attackers</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ICpsService"/>
public interface IStatusEffectService
{
    /// <summary>
    /// Applies a status effect to a character for a specified duration.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="effectId">The status effect identifier (e.g., "Stunned", "Prone").</param>
    /// <param name="durationTurns">The duration of the effect in turns.</param>
    /// <remarks>
    /// <para>
    /// If the character already has this effect, the duration is extended
    /// by the specified amount (stacking durations).
    /// </para>
    /// </remarks>
    void ApplyEffect(Guid characterId, string effectId, int durationTurns);

    /// <summary>
    /// Checks whether a character currently has a specific status effect.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="effectId">The status effect identifier to check for.</param>
    /// <returns>
    /// <c>true</c> if the character has the specified effect; <c>false</c> otherwise.
    /// </returns>
    bool HasEffect(Guid characterId, string effectId);

    /// <summary>
    /// Removes a specific status effect from a character.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="effectId">The status effect identifier to remove.</param>
    void RemoveEffect(Guid characterId, string effectId);

    /// <summary>
    /// Gets all active status effects on a character.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// A read-only collection of active status effect identifiers.
    /// </returns>
    IReadOnlyList<string> GetActiveEffects(Guid characterId);

    /// <summary>
    /// Advances the turn timer for all status effects on a character,
    /// removing any effects that have expired.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <returns>
    /// A read-only collection of effect identifiers that expired this turn.
    /// </returns>
    IReadOnlyList<string> TickEffects(Guid characterId);
}
