using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for psychic stress and corruption management.
/// Handles stress infliction with WILL-based resolve checks, corruption accumulation,
/// and breaking point/terminal error triggers.
/// </summary>
/// <remarks>See: SPEC-TRAUMA-001 for Trauma & Stress System design.</remarks>
public interface ITraumaService
{
    /// <summary>
    /// Inflicts psychic stress on a target, rolling a WILL-based resolve check.
    /// Each success on the resolve check reduces incoming stress by 1.
    /// </summary>
    /// <param name="target">The combatant receiving stress.</param>
    /// <param name="amount">The base stress amount to inflict.</param>
    /// <param name="source">Description of the stress source for logging.</param>
    /// <returns>A StressResult containing mitigation details and new stress total.</returns>
    StressResult InflictStress(Combatant target, int amount, string source);

    /// <summary>
    /// Reduces stress on a target (for rest, recovery abilities, etc.).
    /// Does not require a resolve check.
    /// </summary>
    /// <param name="target">The combatant recovering stress.</param>
    /// <param name="amount">The stress amount to remove.</param>
    /// <param name="source">Description of the recovery source for logging.</param>
    /// <returns>A StressResult containing the new stress total.</returns>
    StressResult RecoverStress(Combatant target, int amount, string source);

    /// <summary>
    /// Gets the stress status tier for a given stress value.
    /// </summary>
    /// <param name="stressValue">The current stress value (0-100).</param>
    /// <returns>The corresponding StressStatus enum value.</returns>
    StressStatus GetStressStatus(int stressValue);

    /// <summary>
    /// Calculates the defense penalty from stress.
    /// Formula: stress / 20, rounded down (max 5 at 100 stress).
    /// </summary>
    /// <param name="stressValue">The current stress value.</param>
    /// <returns>The defense penalty to apply (0-5).</returns>
    int GetDefensePenalty(int stressValue);

    /// <summary>
    /// Handles the breaking point event when stress reaches 100.
    /// Rolls a WILL-based resolve check to determine outcome.
    /// </summary>
    /// <param name="target">The combatant who reached 100 stress.</param>
    void HandleBreakingPoint(Combatant target);

    #region Breaking Point Resolution (v0.3.0c)

    /// <summary>
    /// Resolves a Breaking Point event by rolling a WILL-based resolve check.
    /// Difficulty 3: Success stabilizes, failure causes trauma, fumble causes catastrophe.
    /// </summary>
    /// <param name="character">The character at the breaking point.</param>
    /// <param name="source">Description of what caused the breaking point.</param>
    /// <returns>A BreakingPointResult with outcome, trauma (if any), and new stress level.</returns>
    BreakingPointResult ResolveBreakingPoint(Character character, string source);

    /// <summary>
    /// Applies permanent attribute penalties from a trauma to a character.
    /// Called when a trauma is first acquired.
    /// </summary>
    /// <param name="character">The character acquiring the trauma.</param>
    /// <param name="trauma">The trauma being applied.</param>
    void ApplyTraumaPenalties(Character character, Trauma trauma);

    /// <summary>
    /// Gets the total attribute penalty from all active traumas.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="attribute">The attribute to get penalties for.</param>
    /// <returns>The total penalty (negative value) from traumas.</returns>
    int GetTraumaAttributePenalty(Character character, Enums.Attribute attribute);

    #endregion

    #region Corruption Methods (v0.3.0b)

    /// <summary>
    /// Adds corruption to a character. Unlike stress, corruption is NOT mitigated by WILL.
    /// Corruption is permanent and accumulates directly.
    /// </summary>
    /// <param name="character">The character receiving corruption.</param>
    /// <param name="amount">The corruption amount to add.</param>
    /// <param name="source">Description of the corruption source for logging.</param>
    /// <returns>A CorruptionResult containing tier change details and new total.</returns>
    CorruptionResult AddCorruption(Character character, int amount, string source);

    /// <summary>
    /// Adds corruption to a combatant during combat.
    /// Updates both the combatant and underlying character source.
    /// </summary>
    /// <param name="combatant">The combatant receiving corruption.</param>
    /// <param name="amount">The corruption amount to add.</param>
    /// <param name="source">Description of the corruption source for logging.</param>
    /// <returns>A CorruptionResult containing tier change details and new total.</returns>
    CorruptionResult AddCorruption(Combatant combatant, int amount, string source);

    /// <summary>
    /// Purges corruption from a character (rare cleansing rituals, artifacts).
    /// </summary>
    /// <param name="character">The character being cleansed.</param>
    /// <param name="amount">The corruption amount to remove.</param>
    /// <param name="source">Description of the purge source for logging.</param>
    /// <returns>A CorruptionResult containing tier change details and new total.</returns>
    CorruptionResult PurgeCorruption(Character character, int amount, string source);

    /// <summary>
    /// Gets the corruption state for a given corruption value.
    /// </summary>
    /// <param name="corruptionValue">The current corruption value (0-100).</param>
    /// <returns>The CorruptionState with tier and penalty information.</returns>
    CorruptionState GetCorruptionState(int corruptionValue);

    /// <summary>
    /// Handles the terminal error event when corruption reaches 100.
    /// Character becomes a Forlorn (unplayable).
    /// </summary>
    /// <param name="character">The character who reached terminal corruption.</param>
    void HandleTerminalError(Character character);

    #endregion
}
