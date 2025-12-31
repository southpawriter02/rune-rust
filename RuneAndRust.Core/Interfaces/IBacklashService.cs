using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.Models.Magic;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for magical backlash mechanics.
/// Handles risk calculation, effect application, and corruption tracking.
/// </summary>
/// <remarks>
/// See: v0.4.3d (The Backlash) for implementation details.
/// The Backlash system transforms spellcasting from a simple resource exchange
/// into a calculated risk when Flux exceeds the Critical threshold (50).
/// </remarks>
public interface IBacklashService
{
    /// <summary>
    /// Checks if backlash occurs and applies effects if triggered.
    /// Should be called BEFORE executing a spell when Flux > Critical.
    /// </summary>
    /// <param name="caster">The spellcaster to check.</param>
    /// <param name="spellName">Optional: Name of spell being attempted (for logging).</param>
    /// <returns>Result indicating if backlash occurred and its effects.</returns>
    BacklashResult CheckBacklash(Combatant caster, string? spellName = null);

    /// <summary>
    /// Gets the current backlash risk percentage based on Flux level.
    /// Risk = CurrentFlux - CriticalThreshold (50). Returns 0 if below critical.
    /// </summary>
    /// <returns>Risk percentage (0-50).</returns>
    int GetCurrentRisk();

    /// <summary>
    /// Determines if there is any backlash risk at current Flux level.
    /// </summary>
    /// <returns>True if Flux > CriticalThreshold (50).</returns>
    bool IsAtRisk();

    /// <summary>
    /// Gets the corruption level for a character based on their corruption value.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>The corruption level tier (Untouched through Lost).</returns>
    CorruptionLevel GetCorruptionLevel(Character character);

    /// <summary>
    /// Gets the corruption level for a combatant (via CharacterSource).
    /// Returns Untouched if combatant has no CharacterSource.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>The corruption level tier.</returns>
    CorruptionLevel GetCorruptionLevel(Combatant combatant);

    /// <summary>
    /// Gets all penalties associated with a character's corruption level.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>Penalties record with all modifiers and restrictions.</returns>
    CorruptionPenalties GetCorruptionPenalties(Character character);

    /// <summary>
    /// Gets all penalties for a combatant (via CharacterSource).
    /// Returns Untouched penalties if combatant has no CharacterSource.
    /// </summary>
    /// <param name="combatant">The combatant to check.</param>
    /// <returns>Penalties record with all modifiers and restrictions.</returns>
    CorruptionPenalties GetCorruptionPenalties(Combatant combatant);

    /// <summary>
    /// Adds corruption to a character. Publishes CorruptionChangedEvent if tier changes.
    /// Corruption is clamped to 0-100 range.
    /// </summary>
    /// <param name="character">The character to corrupt.</param>
    /// <param name="amount">Amount of corruption to add (typically 1 for Catastrophic backlash).</param>
    /// <param name="source">Source of the corruption (for logging/events).</param>
    void AddCorruption(Character character, int amount, string source);

    /// <summary>
    /// Attempts to reduce corruption (rare, requires special items/rituals).
    /// Corruption never heals naturally - this requires explicit purification.
    /// </summary>
    /// <param name="character">The character to purify.</param>
    /// <param name="amount">Amount of corruption to remove.</param>
    /// <param name="source">Source of purification (for logging/events).</param>
    /// <returns>True if corruption was reduced, false if character had no corruption.</returns>
    bool PurgeCorruption(Character character, int amount, string source);

    /// <summary>
    /// Checks if a character can cast spells based on corruption level.
    /// Characters at "Lost" level (75+ corruption) cannot cast.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <returns>True if not "Lost", false if soul is consumed.</returns>
    bool CanCastSpells(Character character);

    /// <summary>
    /// Gets a formatted risk warning message for UI display.
    /// Returns empty string if no risk.
    /// </summary>
    /// <returns>Warning message about current backlash risk.</returns>
    string GetRiskWarning();
}
