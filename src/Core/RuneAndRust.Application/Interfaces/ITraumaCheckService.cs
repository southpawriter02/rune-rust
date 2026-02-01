// ═══════════════════════════════════════════════════════════════════════════════
// ITraumaCheckService.cs
// Defines the Application Layer contract for the Trauma Check system.
// Trauma checks determine when characters must roll to avoid acquiring new traumas.
// Version: 0.18.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;

/// <summary>
/// Service for performing trauma checks and managing check mechanics.
/// </summary>
/// <remarks>
/// <para>
/// The ITraumaCheckService handles the mechanics of trauma checks,
/// including difficulty determination, dice pool calculation, and
/// random trauma selection.
/// </para>
/// <para>
/// <strong>Responsibilities:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Determine check difficulty for each trigger type</description></item>
///   <item><description>Calculate effective dice pool (RESOLVE - corruption penalty)</description></item>
///   <item><description>Prevent duplicate checks (one check per event per character)</description></item>
///   <item><description>Select random trauma from appropriate category on failure</description></item>
///   <item><description>Track check history and modifiers</description></item>
/// </list>
/// <para>
/// <strong>Trauma Check Formula:</strong>
/// <code>
/// Effective Dice Pool = base_resolve - corruption_penalty
/// where corruption_penalty = floor(corruption_value / 20)
/// Successes = Count all dice >= 4 in roll
/// Result = Successes >= SuccessesNeeded for Trigger
/// </code>
/// </para>
/// <para>
/// <strong>Difficulty Table:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>CriticalFailure: 1 success</description></item>
///   <item><description>AllyDeath, NearDeathExperience, ProlongedExposure: 2 successes</description></item>
///   <item><description>StressThreshold100, WitnessingHorror: 3 successes</description></item>
///   <item><description>CorruptionThreshold100, RuinMadnessEscape: 4 successes</description></item>
/// </list>
/// <para>
/// <strong>Implementation Note:</strong>
/// The full implementation is provided in v0.18.3e (TraumaCheckService).
/// </para>
/// </remarks>
/// <seealso cref="TraumaCheckTrigger"/>
/// <seealso cref="TraumaCheckResult"/>
/// <seealso cref="TraumaType"/>
public interface ITraumaCheckService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CHECK EXECUTION METHODS — Perform Trauma Checks
    // ═══════════════════════════════════════════════════════════════════════════

    #region Check Execution Methods

    /// <summary>
    /// Performs a trauma check for the given character and trigger.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Full trauma check flow:
    /// </para>
    /// <list type="number">
    ///   <item><description>Check if this character has already had this check type (ShouldTriggerCheck)</description></item>
    ///   <item><description>Get base RESOLVE pool</description></item>
    ///   <item><description>Apply corruption penalty (floor(corruption/20))</description></item>
    ///   <item><description>Get modifiers applicable to this character</description></item>
    ///   <item><description>Roll dice using IDiceService</description></item>
    ///   <item><description>Count successes (die >= 4)</description></item>
    ///   <item><description>If failed, select random trauma</description></item>
    ///   <item><description>Return TraumaCheckResult</description></item>
    /// </list>
    /// <para>
    /// <strong>Corruption Penalty Table:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>0-19: 0 dice penalty</description></item>
    ///   <item><description>20-39: 1 die penalty</description></item>
    ///   <item><description>40-59: 2 dice penalty</description></item>
    ///   <item><description>60-79: 3 dice penalty</description></item>
    ///   <item><description>80-99: 4 dice penalty</description></item>
    ///   <item><description>100: 5 dice penalty</description></item>
    /// </list>
    /// </remarks>
    /// <param name="characterId">Character making the check.</param>
    /// <param name="trigger">Type of trauma-inducing event.</param>
    /// <returns>Complete check result with outcome.</returns>
    /// <exception cref="ArgumentException">If character not found.</exception>
    /// <example>
    /// <code>
    /// var result = await traumaCheckService.PerformTraumaCheckAsync(
    ///     characterId,
    ///     TraumaCheckTrigger.AllyDeath
    /// );
    /// if (!result.Passed)
    /// {
    ///     DisplayTraumaAcquired(result.TraumaAcquired);
    /// }
    /// </code>
    /// </example>
    Task<TraumaCheckResult> PerformTraumaCheckAsync(
        Guid characterId,
        TraumaCheckTrigger trigger);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DIFFICULTY CALCULATION METHODS — Determine Check Requirements
    // ═══════════════════════════════════════════════════════════════════════════

    #region Difficulty Calculation Methods

    /// <summary>
    /// Gets the difficulty (successes needed) for the given trigger.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Difficulties are fixed per trigger type:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>CriticalFailure: 1 success</description></item>
    ///   <item><description>AllyDeath, NearDeathExperience, ProlongedExposure: 2 successes</description></item>
    ///   <item><description>StressThreshold100, WitnessingHorror: 3 successes</description></item>
    ///   <item><description>CorruptionThreshold100, RuinMadnessEscape: 4 successes</description></item>
    /// </list>
    /// </remarks>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>Number of successes needed (1-4).</returns>
    /// <example>
    /// <code>
    /// var difficulty = traumaCheckService.GetTraumaCheckDifficulty(TraumaCheckTrigger.AllyDeath);
    /// // difficulty == 2
    /// </code>
    /// </example>
    int GetTraumaCheckDifficulty(TraumaCheckTrigger trigger);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // MODIFIER METHODS — Query Active Modifiers
    // ═══════════════════════════════════════════════════════════════════════════

    #region Modifier Methods

    /// <summary>
    /// Gets modifiers applicable to the character's trauma checks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifiers can come from:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Existing traumas (some make checks harder/easier)</description></item>
    ///   <item><description>Character traits or abilities</description></item>
    ///   <item><description>Current status effects</description></item>
    ///   <item><description>Campaign-specific rules</description></item>
    /// </list>
    /// </remarks>
    /// <param name="characterId">Character to check.</param>
    /// <returns>List of applicable modifiers.</returns>
    /// <example>
    /// <code>
    /// var modifiers = await traumaCheckService.GetCurrentModifiersAsync(characterId);
    /// // modifiers might contain: ["Strong Will", "Corrupted Mind"]
    /// </code>
    /// </example>
    Task<IReadOnlyList<string>> GetCurrentModifiersAsync(Guid characterId);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION METHODS — Determine If Check Should Trigger
    // ═══════════════════════════════════════════════════════════════════════════

    #region Validation Methods

    /// <summary>
    /// Determines whether a trauma check should trigger.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns false if:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Character already had this check type recently (e.g., only one AllyDeath check per session)</description></item>
    ///   <item><description>Character is not in valid state for this check</description></item>
    ///   <item><description>Check has been explicitly disabled</description></item>
    /// </list>
    /// <para>
    /// This prevents duplicate checks from stacking on a single event.
    /// </para>
    /// </remarks>
    /// <param name="characterId">Character to check.</param>
    /// <param name="trigger">Trigger type.</param>
    /// <returns>True if check should proceed, false to skip.</returns>
    /// <example>
    /// <code>
    /// if (await traumaCheckService.ShouldTriggerCheckAsync(characterId, TraumaCheckTrigger.AllyDeath))
    /// {
    ///     var result = await traumaCheckService.PerformTraumaCheckAsync(characterId, TraumaCheckTrigger.AllyDeath);
    /// }
    /// </code>
    /// </example>
    Task<bool> ShouldTriggerCheckAsync(Guid characterId, TraumaCheckTrigger trigger);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // TRAUMA SELECTION METHODS — Random Trauma from Category
    // ═══════════════════════════════════════════════════════════════════════════

    #region Trauma Selection Methods

    /// <summary>
    /// Gets a random trauma from the given category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when trauma check fails. Selects from the category determined
    /// by the trigger type. Selection is unweighted (random).
    /// </para>
    /// <para>
    /// <strong>Trigger → Category Mapping:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>StressThreshold100 → Cognitive or Emotional</description></item>
    ///   <item><description>CorruptionThreshold100 → Corruption</description></item>
    ///   <item><description>AllyDeath → Emotional</description></item>
    ///   <item><description>NearDeathExperience → Physical or Emotional</description></item>
    ///   <item><description>CriticalFailure → Context-dependent</description></item>
    ///   <item><description>ProlongedExposure → Existential</description></item>
    ///   <item><description>WitnessingHorror → Cognitive</description></item>
    ///   <item><description>RuinMadnessEscape → Cognitive or Existential</description></item>
    /// </list>
    /// </remarks>
    /// <param name="category">Trauma category to select from.</param>
    /// <returns>Trauma ID, or null if no suitable trauma exists.</returns>
    /// <example>
    /// <code>
    /// var traumaId = await traumaCheckService.GetRandomTraumaAsync(TraumaType.Emotional);
    /// // traumaId might be "survivors-guilt" or "night-terrors"
    /// </code>
    /// </example>
    Task<string?> GetRandomTraumaAsync(TraumaType category);

    #endregion
}
