// ═══════════════════════════════════════════════════════════════════════════════
// ITraumaService.cs
// Defines the Application Layer contract for all trauma management operations.
// This interface is the central entry point for trauma system functionality.
// Version: 0.18.3d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for managing character traumas and trauma mechanics.
/// </summary>
/// <remarks>
/// <para>
/// The ITraumaService coordinates all trauma system operations, including:
/// </para>
/// <list type="bullet">
///   <item><description>Definition lookups (trauma blueprints from configuration)</description></item>
///   <item><description>Per-character trauma tracking</description></item>
///   <item><description>Trauma acquisition and stacking</description></item>
///   <item><description>Retirement condition evaluation</description></item>
///   <item><description>Trauma effect management</description></item>
///   <item><description>Trauma check mechanics (coordination with ITraumaCheckService)</description></item>
/// </list>
/// <para>
/// All trauma operations should go through this service. It coordinates
/// between domain entities, repositories, and check systems.
/// </para>
/// <para>
/// <strong>Service Dependencies:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>ITraumaCheckService: For performing trauma check rolls</description></item>
///   <item><description>IDiceService: For random trauma selection</description></item>
///   <item><description>Repository layer: For persistence of CharacterTrauma entities</description></item>
///   <item><description>Configuration provider: For TraumaDefinition loading from traumas.json</description></item>
/// </list>
/// <para>
/// <strong>Implementation Note:</strong>
/// The full implementation is provided in v0.18.3e (TraumaService).
/// </para>
/// </remarks>
/// <seealso cref="CharacterTrauma"/>
/// <seealso cref="TraumaDefinition"/>
/// <seealso cref="TraumaCheckTrigger"/>
/// <seealso cref="TraumaCheckResult"/>
/// <seealso cref="TraumaAcquisitionResult"/>
/// <seealso cref="RetirementCheckResult"/>
public interface ITraumaService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // QUERY METHODS — Retrieve Trauma Information
    // ═══════════════════════════════════════════════════════════════════════════

    #region Query Methods

    /// <summary>
    /// Gets all traumas currently affecting a character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns all CharacterTrauma instances for the given character,
    /// including inactive/managed traumas. To get only active traumas,
    /// filter by IsActive property.
    /// </para>
    /// <para>
    /// Traumas are ordered by AcquiredAt (earliest first).
    /// </para>
    /// </remarks>
    /// <param name="characterId">The character to query.</param>
    /// <returns>List of all traumas (empty if none).</returns>
    /// <exception cref="ArgumentException">If characterId is empty Guid.</exception>
    /// <example>
    /// <code>
    /// var traumas = await _traumaService.GetTraumasAsync(characterId);
    /// var activeTraumas = traumas.Where(t => t.IsActive).ToList();
    /// </code>
    /// </example>
    Task<IReadOnlyList<CharacterTrauma>> GetTraumasAsync(Guid characterId);

    /// <summary>
    /// Gets the definition of a specific trauma by ID.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TraumaDefinition contains all mechanical properties of the trauma,
    /// including effects, triggers, and retirement conditions.
    /// Cached after initial load from configuration.
    /// </para>
    /// </remarks>
    /// <param name="traumaId">The trauma ID (e.g., "survivors-guilt").</param>
    /// <returns>The TraumaDefinition.</returns>
    /// <exception cref="ArgumentException">If traumaId is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">If trauma not found in definitions.</exception>
    /// <example>
    /// <code>
    /// var definition = await _traumaService.GetTraumaDefinitionAsync("survivors-guilt");
    /// var effects = definition.Effects;
    /// var isRetirement = definition.IsRetirementTrauma;
    /// </code>
    /// </example>
    Task<TraumaDefinition> GetTraumaDefinitionAsync(string traumaId);

    /// <summary>
    /// Checks whether a character has a specific trauma.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Checks only for active traumas by default. Ignores managed/suppressed traumas.
    /// </para>
    /// </remarks>
    /// <param name="characterId">Character to check.</param>
    /// <param name="traumaId">Trauma ID to look for (e.g., "machine-affinity").</param>
    /// <returns>True if character has this trauma (and it's active), false otherwise.</returns>
    /// <exception cref="ArgumentException">If parameters are invalid.</exception>
    /// <example>
    /// <code>
    /// if (await _traumaService.HasTraumaAsync(characterId, "machine-affinity"))
    /// {
    ///     // Apply special mechanics
    /// }
    /// </code>
    /// </example>
    Task<bool> HasTraumaAsync(Guid characterId, string traumaId);

    /// <summary>
    /// Gets the total number of active traumas a character has.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Counts only active traumas. Managed/suppressed traumas are not counted.
    /// </para>
    /// </remarks>
    /// <param name="characterId">Character to check.</param>
    /// <returns>Count of active traumas (0 if none).</returns>
    /// <exception cref="ArgumentException">If characterId is invalid.</exception>
    /// <example>
    /// <code>
    /// int traumaCount = await _traumaService.GetTraumaCountAsync(characterId);
    /// if (traumaCount >= 3)
    /// {
    ///     // Apply "Heavily Traumatized" debuff
    /// }
    /// </code>
    /// </example>
    Task<int> GetTraumaCountAsync(Guid characterId);

    /// <summary>
    /// Gets the stack count for a specific trauma on a character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For stackable traumas: returns how many times acquired.
    /// For non-stackable traumas: returns 0 or 1.
    /// </para>
    /// </remarks>
    /// <param name="characterId">Character to check.</param>
    /// <param name="traumaId">Trauma ID (e.g., "reality-doubt").</param>
    /// <returns>Stack count (0 if character doesn't have this trauma).</returns>
    /// <exception cref="ArgumentException">If parameters are invalid.</exception>
    /// <example>
    /// <code>
    /// int realityDoubtStack = await _traumaService.GetTraumaStackCountAsync(
    ///     characterId, "reality-doubt"
    /// );
    /// if (realityDoubtStack >= 5)
    /// {
    ///     // Trigger retirement check
    /// }
    /// </code>
    /// </example>
    Task<int> GetTraumaStackCountAsync(Guid characterId, string traumaId);

    /// <summary>
    /// Gets all active mechanical effects from the character's traumas.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns flattened list of all TraumaEffect objects from all active traumas.
    /// Does not include effects from managed/suppressed traumas.
    /// </para>
    /// <para>
    /// Application layer uses this to apply modifiers to checks, rolls, etc.
    /// </para>
    /// </remarks>
    /// <param name="characterId">Character to query.</param>
    /// <returns>List of active effects (empty if none).</returns>
    /// <exception cref="ArgumentException">If characterId is invalid.</exception>
    /// <example>
    /// <code>
    /// var traumaEffects = await _traumaService.GetActiveEffectsAsync(characterId);
    /// var socialPenalties = traumaEffects
    ///     .Where(e => e.Target == "social-skills" &amp;&amp; e.EffectType == "Penalty")
    ///     .Sum(e => e.Value ?? 0);
    /// </code>
    /// </example>
    Task<IReadOnlyList<TraumaEffect>> GetActiveEffectsAsync(Guid characterId);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // MUTATION METHODS — Modify Character Trauma State
    // ═══════════════════════════════════════════════════════════════════════════

    #region Mutation Methods

    /// <summary>
    /// Acquires a trauma for the character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Handles both new acquisitions and stacking:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>If character doesn't have this trauma: create new instance</description></item>
    ///   <item><description>If stackable and character has it: increment StackCount</description></item>
    ///   <item><description>If non-stackable and character has it: return failure</description></item>
    /// </list>
    /// <para>
    /// <strong>Process:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>Load TraumaDefinition</description></item>
    ///   <item><description>Check if character already has this trauma</description></item>
    ///   <item><description>Create new or increment existing</description></item>
    ///   <item><description>Trigger retirement check if applicable</description></item>
    ///   <item><description>Return result with message</description></item>
    /// </list>
    /// </remarks>
    /// <param name="characterId">Character acquiring trauma.</param>
    /// <param name="traumaId">Trauma to acquire (e.g., "survivors-guilt").</param>
    /// <param name="source">What caused this trauma (e.g., "AllyDeath").</param>
    /// <returns>Result of acquisition attempt.</returns>
    /// <exception cref="ArgumentException">If any parameter is invalid.</exception>
    /// <exception cref="KeyNotFoundException">If trauma or character not found.</exception>
    /// <example>
    /// <code>
    /// var result = await _traumaService.AcquireTraumaAsync(
    ///     characterId: characterId,
    ///     traumaId: "survivors-guilt",
    ///     source: "AllyDeath"
    /// );
    /// 
    /// if (result.Success)
    /// {
    ///     _logger.LogInformation(result.Message);
    ///     if (result.TriggersRetirementCheck)
    ///     {
    ///         var retirementResult = await _traumaService.CheckRetirementConditionAsync(characterId);
    ///         // Handle retirement
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<TraumaAcquisitionResult> AcquireTraumaAsync(
        Guid characterId,
        string traumaId,
        string source);

    /// <summary>
    /// Evaluates whether a character must retire due to trauma.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Checks all retirement conditions:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Immediate traumas (Survivor's Guilt, Machine Affinity, Death Wish)</description></item>
    ///   <item><description>Stacking traumas (Reality Doubt 5+, Hollow Resonance 3+)</description></item>
    ///   <item><description>Accumulation (3+ different moderate traumas)</description></item>
    /// </list>
    /// <para>
    /// Returns RetirementCheckResult with:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>MustRetire: true if forced to retire</description></item>
    ///   <item><description>CanContinueWithPermission: true if optional retirement</description></item>
    ///   <item><description>TraumasCausingRetirement: which traumas triggered it</description></item>
    /// </list>
    /// </remarks>
    /// <param name="characterId">Character to evaluate.</param>
    /// <returns>Complete retirement evaluation result.</returns>
    /// <exception cref="ArgumentException">If characterId is invalid.</exception>
    /// <example>
    /// <code>
    /// var retirementCheck = await _traumaService.CheckRetirementConditionAsync(characterId);
    /// 
    /// if (retirementCheck.MustRetire)
    /// {
    ///     _logger.LogError($"Character must retire: {retirementCheck.RetirementReason}");
    ///     // Initiate character loss procedures
    /// }
    /// else if (retirementCheck.CanContinueWithPermission)
    /// {
    ///     // Offer player optional retirement
    /// }
    /// </code>
    /// </example>
    Task<RetirementCheckResult> CheckRetirementConditionAsync(Guid characterId);

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // CHECK METHODS — Perform Trauma Checks
    // ═══════════════════════════════════════════════════════════════════════════

    #region Check Methods

    /// <summary>
    /// Performs a trauma check and acquires trauma on failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Full trauma check flow:
    /// </para>
    /// <list type="number">
    ///   <item><description>Check if character should have this check type</description></item>
    ///   <item><description>Get base RESOLVE dice pool</description></item>
    ///   <item><description>Apply corruption penalty</description></item>
    ///   <item><description>Roll dice using IDiceService</description></item>
    ///   <item><description>Count successes</description></item>
    ///   <item><description>If failed: acquire random trauma from category</description></item>
    ///   <item><description>Return complete result</description></item>
    /// </list>
    /// <para>
    /// This method combines ITraumaCheckService logic with trauma acquisition.
    /// </para>
    /// </remarks>
    /// <param name="characterId">Character making the check.</param>
    /// <param name="trigger">Type of trauma-inducing event.</param>
    /// <returns>Check result with outcome.</returns>
    /// <exception cref="ArgumentException">If parameters are invalid.</exception>
    /// <example>
    /// <code>
    /// var checkResult = await _traumaService.PerformTraumaCheckAsync(
    ///     characterId: character.Id,
    ///     trigger: TraumaCheckTrigger.AllyDeath
    /// );
    /// 
    /// if (!checkResult.Passed)
    /// {
    ///     _logger.LogWarning(
    ///         $"Character acquired {checkResult.TraumaAcquired}: " +
    ///         $"{checkResult.SuccessesAchieved}/{checkResult.SuccessesNeeded} successes"
    ///     );
    /// }
    /// </code>
    /// </example>
    Task<TraumaCheckResult> PerformTraumaCheckAsync(
        Guid characterId,
        TraumaCheckTrigger trigger);

    #endregion
}
