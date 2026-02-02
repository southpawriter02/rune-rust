// ═══════════════════════════════════════════════════════════════════════════════
// RestIntegrationResult.cs
// Immutable value object representing the complete outcome of unified rest
// processing across all trauma economy systems.
// Version: 0.18.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Immutable value object representing the complete outcome of unified rest processing.
/// </summary>
/// <remarks>
/// <para>
/// Captures all effects triggered by a rest event across all trauma economy systems:
/// stress recovery, specialization resource resets, trauma checks, CPS recovery。
/// </para>
/// <para>
/// <strong>Rest Type Effects:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>System</term>
///     <description>Short / Long / Sanctuary</description>
///   </listheader>
///   <item>
///     <term>Stress</term>
///     <description>WILL×2 / WILL×5 / Full reset</description>
///   </item>
///   <item>
///     <term>Rage</term>
///     <description>Reset to 0 / Reset to 0 / Reset to 0</description>
///   </item>
///   <item>
///     <term>Momentum</term>
///     <description>Reset to 0 / Reset to 0 / Reset to 0</description>
///   </item>
///   <item>
///     <term>Coherence</term>
///     <description>No change / Restore to 50 / Restore to 50</description>
///   </item>
///   <item>
///     <term>Trauma Check</term>
///     <description>No / Yes / Yes</description>
///   </item>
///   <item>
///     <term>Party Rage Bonus</term>
///     <description>No / Yes (-10 stress) / Yes (-10 stress)</description>
///   </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var result = restHandler.ProcessRest(characterId, RestType.Long, partyContext);
/// logger.LogInformation("Rest result: {StressRecovered} stress recovered", result.StressRecovered);
///
/// if (result.AcquiredNewTraumas)
/// {
///     foreach (var trauma in result.TraumasAcquired!)
///         ShowTraumaNotification(trauma);
/// }
/// </code>
/// </example>
/// <seealso cref="RestType"/>
/// <seealso cref="CpsStage"/>
public readonly record struct RestIntegrationResult(
    RestType RestType,
    int StressRecovered,
    string StressRecoveryFormula,
    int CorruptionRecovered,
    bool CpsStageChanged,
    CpsStage? NewCpsStage,
    int TraumaChecksPerformed,
    IReadOnlyList<string>? TraumasAcquired,
    int? RagePartyBonus,
    int? CoherenceRestored,
    bool MomentumReset,
    IReadOnlyList<string> RecoveryMessages)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any new traumas were acquired during rest.
    /// </summary>
    /// <remarks>
    /// Trauma checks can occur during Long and Sanctuary rests.
    /// </remarks>
    public bool AcquiredNewTraumas =>
        TraumasAcquired is not null && TraumasAcquired.Count > 0;

    /// <summary>
    /// Gets whether this rest triggered any party-wide bonuses.
    /// </summary>
    /// <remarks>
    /// Currently only the Berserker FrenzyBeyondReason bonus applies.
    /// </remarks>
    public bool HasPartyBonus =>
        RagePartyBonus.HasValue && RagePartyBonus.Value > 0;

    /// <summary>
    /// Gets whether any specialization resources were affected.
    /// </summary>
    public bool HasSpecializationEffects =>
        MomentumReset || CoherenceRestored.HasValue || RagePartyBonus.HasValue;

    /// <summary>
    /// Gets whether this rest represents significant recovery.
    /// </summary>
    /// <remarks>
    /// True if stress recovery exceeds 25 or CPS stage improved.
    /// </remarks>
    public bool IsSignificantRecovery =>
        StressRecovered >= 25 || CpsStageChanged;

    /// <summary>
    /// Gets whether this was a full stress reset (Sanctuary rest).
    /// </summary>
    public bool IsFullRecovery =>
        RestType == RestType.Sanctuary;

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates result for internal consistency.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all values are consistent; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Checks performed:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Stress recovered is non-negative</description></item>
    ///   <item><description>Corruption recovered is non-negative</description></item>
    ///   <item><description>Trauma checks performed is non-negative</description></item>
    ///   <item><description>Optional values are non-negative if present</description></item>
    ///   <item><description>Collections are not null</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = restHandler.ProcessRest(characterId, RestType.Long);
    /// if (!result.IsValid())
    /// {
    ///     logger.LogWarning("Invalid rest result detected: {@Result}", result);
    /// }
    /// </code>
    /// </example>
    public bool IsValid()
    {
        // Recovery values should be non-negative
        if (StressRecovered < 0 || CorruptionRecovered < 0)
        {
            return false;
        }

        // Trauma checks should be non-negative
        if (TraumaChecksPerformed < 0)
        {
            return false;
        }

        // Optional values should be non-negative if present
        if (RagePartyBonus.HasValue && RagePartyBonus < 0)
        {
            return false;
        }

        if (CoherenceRestored.HasValue && CoherenceRestored < 0)
        {
            return false;
        }

        // Recovery messages must not be null
        if (RecoveryMessages == null)
        {
            return false;
        }

        // Formula must not be null or empty
        if (string.IsNullOrWhiteSpace(StressRecoveryFormula))
        {
            return false;
        }

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new RestIntegrationResult with sensible defaults.
    /// </summary>
    /// <param name="restType">The type of rest taken.</param>
    /// <param name="stressRecovered">Amount of stress recovered.</param>
    /// <param name="stressRecoveryFormula">Formula used for recovery calculation.</param>
    /// <param name="corruptionRecovered">Optional corruption recovered.</param>
    /// <param name="cpsStageChanged">Whether CPS stage improved.</param>
    /// <param name="newCpsStage">The new CPS stage if changed.</param>
    /// <param name="traumaChecksPerformed">Number of trauma checks made.</param>
    /// <param name="traumasAcquired">List of trauma IDs acquired.</param>
    /// <param name="ragePartyBonus">Party stress reduction from Berserker FrenzyBeyondReason.</param>
    /// <param name="coherenceRestored">Amount of coherence restored (Arcanist).</param>
    /// <param name="momentumReset">Whether momentum was reset to 0.</param>
    /// <param name="recoveryMessages">Narrative messages for UI display.</param>
    /// <returns>A new immutable RestIntegrationResult.</returns>
    /// <example>
    /// <code>
    /// var result = RestIntegrationResult.Create(
    ///     restType: RestType.Long,
    ///     stressRecovered: 25,
    ///     stressRecoveryFormula: "WILL × 5",
    ///     traumaChecksPerformed: 1);
    /// </code>
    /// </example>
    public static RestIntegrationResult Create(
        RestType restType,
        int stressRecovered,
        string stressRecoveryFormula,
        int corruptionRecovered = 0,
        bool cpsStageChanged = false,
        CpsStage? newCpsStage = null,
        int traumaChecksPerformed = 0,
        IReadOnlyList<string>? traumasAcquired = null,
        int? ragePartyBonus = null,
        int? coherenceRestored = null,
        bool momentumReset = true,
        IReadOnlyList<string>? recoveryMessages = null)
    {
        return new RestIntegrationResult(
            RestType: restType,
            StressRecovered: stressRecovered,
            StressRecoveryFormula: stressRecoveryFormula,
            CorruptionRecovered: corruptionRecovered,
            CpsStageChanged: cpsStageChanged,
            NewCpsStage: newCpsStage,
            TraumaChecksPerformed: traumaChecksPerformed,
            TraumasAcquired: traumasAcquired ?? new List<string>().AsReadOnly(),
            RagePartyBonus: ragePartyBonus,
            CoherenceRestored: coherenceRestored,
            MomentumReset: momentumReset,
            RecoveryMessages: recoveryMessages ?? new List<string>().AsReadOnly()
        );
    }

    /// <summary>
    /// Creates an empty result representing no recovery (e.g., rest interrupted).
    /// </summary>
    /// <param name="restType">The type of rest attempted.</param>
    /// <returns>A result with zero recovery and no effects.</returns>
    public static RestIntegrationResult Empty(RestType restType) => Create(
        restType: restType,
        stressRecovered: 0,
        stressRecoveryFormula: "Interrupted",
        momentumReset: false);

    // ═══════════════════════════════════════════════════════════════════════════
    // Display
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the rest result for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing key recovery values.</returns>
    /// <example>
    /// <code>
    /// var result = restHandler.ProcessRest(characterId, RestType.Long);
    /// Console.WriteLine(result.ToString());
    /// // Output: "RestResult: Long Rest, -25 stress via WILL × 5"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"RestResult: {RestType}, -{StressRecovered} stress via {StressRecoveryFormula}" +
        (AcquiredNewTraumas ? $" [TRAUMA: {TraumasAcquired!.Count}]" : "") +
        (HasPartyBonus ? $" [PARTY BONUS: -{RagePartyBonus}]" : "");
}
