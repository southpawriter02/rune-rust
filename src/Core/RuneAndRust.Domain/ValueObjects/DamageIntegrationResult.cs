// ═══════════════════════════════════════════════════════════════════════════════
// DamageIntegrationResult.cs
// Immutable value object representing the complete outcome of unified damage
// processing across all trauma economy systems.
// Version: 0.18.5b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Immutable value object representing the complete outcome of unified damage processing.
/// </summary>
/// <remarks>
/// <para>
/// Captures all effects triggered by a damage event across all trauma economy systems:
/// stress generation, specialization resource changes, trauma triggers, and threshold crossings.
/// </para>
/// <para>
/// <strong>Usage:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Logging damage events with full context</description></item>
///   <item><description>Applying all effects to character state atomically</description></item>
///   <item><description>Generating combat messages and UI feedback</description></item>
///   <item><description>Creating save/replay data for persistence</description></item>
/// </list>
/// <para>
/// <strong>Stress Calculation:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Base stress = floor(damage after soak / 10)</description></item>
///   <item><description>Critical hit bonus = +5 stress</description></item>
///   <item><description>Near-death bonus = +10 stress (HP &lt; 25% max)</description></item>
/// </list>
/// <para>
/// <strong>Specialization Effects:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Berserker: Gains rage proportional to damage taken</description></item>
///   <item><description>Storm Blade: Loses momentum on critical hits received</description></item>
///   <item><description>Arcanist: Loses coherence if interrupted while casting</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var result = handler.ProcessDamage(characterId, 25, context);
/// logger.LogInformation("Damage result: {StressGained} stress, {TraumaCheckTriggered}",
///     result.StressGained, result.TraumaCheckTriggered);
///
/// if (result.TraumaCheckTriggered)
/// {
///     traumaService.PerformTraumaCheck(characterId);
/// }
/// </code>
/// </example>
/// <seealso cref="RuneAndRust.Domain.Entities.TraumaEconomyState"/>
/// <seealso cref="StressSource"/>
public readonly record struct DamageIntegrationResult(
    int DamageDealt,
    int DamageAfterSoak,
    int SoakApplied,
    int StressGained,
    StressSource? StressSource,
    int? CorruptionGained,
    CorruptionSource? CorruptionSource,
    int? RageGained,
    int? MomentumLost,
    int? CoherenceLost,
    bool TraumaCheckTriggered,
    IReadOnlyList<string> ThresholdsCrossed,
    IReadOnlyList<string> TransitionMessages)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this damage represents a critical hit.
    /// </summary>
    /// <remarks>
    /// Inferred by presence of "critical" in transition messages.
    /// </remarks>
    public bool IsCriticalHit =>
        TransitionMessages.Any(m => m.Contains("critical", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets whether this damage triggered a near-death event.
    /// </summary>
    /// <remarks>
    /// Inferred by presence of "near-death" in transition messages.
    /// </remarks>
    public bool IsNearDeathEvent =>
        TransitionMessages.Any(m => m.Contains("near-death", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the total specialization resource losses (momentum + coherence).
    /// </summary>
    /// <remarks>
    /// Rage gains are not included as they are positive changes, not losses.
    /// </remarks>
    public int TotalResourceLoss =>
        (MomentumLost ?? 0) + (CoherenceLost ?? 0);

    /// <summary>
    /// Gets whether any specialization effects were triggered.
    /// </summary>
    public bool HasSpecializationEffects =>
        RageGained.HasValue || MomentumLost.HasValue || CoherenceLost.HasValue;

    /// <summary>
    /// Gets whether this result indicates significant psychological impact.
    /// </summary>
    /// <remarks>
    /// True if stress gained is 10 or more, or if trauma check was triggered.
    /// </remarks>
    public bool IsHighImpact =>
        StressGained >= 10 || TraumaCheckTriggered;

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
    ///   <item><description>Damage values are non-negative</description></item>
    ///   <item><description>Soak does not exceed damage dealt</description></item>
    ///   <item><description>Damage after soak is at least 1 when damage exceeds soak</description></item>
    ///   <item><description>Stress gain is non-negative</description></item>
    ///   <item><description>Optional resource changes are non-negative if present</description></item>
    ///   <item><description>Collections are not null</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = handler.ProcessDamage(characterId, damage, context);
    /// if (!result.IsValid())
    /// {
    ///     logger.LogWarning("Invalid damage result detected: {@Result}", result);
    /// }
    /// </code>
    /// </example>
    public bool IsValid()
    {
        // Damage values should be non-negative
        if (DamageDealt < 0 || DamageAfterSoak < 0 || SoakApplied < 0)
        {
            return false;
        }

        // Soak can't exceed damage dealt
        if (SoakApplied > DamageDealt)
        {
            return false;
        }

        // Damage after soak should be at least 1 when damage exceeds soak
        var expectedDamageAfterSoak = Math.Max(1, DamageDealt - SoakApplied);
        if (DamageDealt > 0 && DamageAfterSoak != expectedDamageAfterSoak)
        {
            return false;
        }

        // Stress gain should be non-negative
        if (StressGained < 0)
        {
            return false;
        }

        // Optional gains/losses should be non-negative if present
        if (RageGained.HasValue && RageGained < 0)
        {
            return false;
        }

        if (MomentumLost.HasValue && MomentumLost < 0)
        {
            return false;
        }

        if (CoherenceLost.HasValue && CoherenceLost < 0)
        {
            return false;
        }

        if (CorruptionGained.HasValue && CorruptionGained < 0)
        {
            return false;
        }

        // Collections must not be null
        if (ThresholdsCrossed == null || TransitionMessages == null)
        {
            return false;
        }

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new DamageIntegrationResult from components.
    /// </summary>
    /// <param name="damageDealt">Raw damage before soak.</param>
    /// <param name="damageAfterSoak">Damage after soak applied.</param>
    /// <param name="soakApplied">Total soak value absorbed.</param>
    /// <param name="stressGained">Stress generated from damage.</param>
    /// <param name="stressSource">Optional source of stress (defaults to PhysicalDamage).</param>
    /// <param name="corruptionGained">Optional corruption gained.</param>
    /// <param name="corruptionSource">Optional source of corruption.</param>
    /// <param name="rageGained">Optional rage gained (Berserker).</param>
    /// <param name="momentumLost">Optional momentum lost (Storm Blade).</param>
    /// <param name="coherenceLost">Optional coherence lost (Arcanist).</param>
    /// <param name="traumaCheckTriggered">Whether trauma check was triggered.</param>
    /// <param name="thresholdsCrossed">List of thresholds crossed.</param>
    /// <param name="transitionMessages">List of narrative messages.</param>
    /// <returns>A new immutable DamageIntegrationResult.</returns>
    /// <example>
    /// <code>
    /// var result = DamageIntegrationResult.Create(
    ///     damageDealt: 50,
    ///     damageAfterSoak: 40,
    ///     soakApplied: 10,
    ///     stressGained: 4,
    ///     stressSource: StressSource.PhysicalDamage,
    ///     rageGained: 8);
    /// </code>
    /// </example>
    public static DamageIntegrationResult Create(
        int damageDealt,
        int damageAfterSoak,
        int soakApplied,
        int stressGained,
        StressSource? stressSource = null,
        int? corruptionGained = null,
        CorruptionSource? corruptionSource = null,
        int? rageGained = null,
        int? momentumLost = null,
        int? coherenceLost = null,
        bool traumaCheckTriggered = false,
        IReadOnlyList<string>? thresholdsCrossed = null,
        IReadOnlyList<string>? transitionMessages = null)
    {
        return new DamageIntegrationResult(
            DamageDealt: damageDealt,
            DamageAfterSoak: damageAfterSoak,
            SoakApplied: soakApplied,
            StressGained: stressGained,
            StressSource: stressSource ?? Enums.StressSource.Combat,
            CorruptionGained: corruptionGained,
            CorruptionSource: corruptionSource,
            RageGained: rageGained,
            MomentumLost: momentumLost,
            CoherenceLost: coherenceLost,
            TraumaCheckTriggered: traumaCheckTriggered,
            ThresholdsCrossed: thresholdsCrossed ?? new List<string>().AsReadOnly(),
            TransitionMessages: transitionMessages ?? new List<string>().AsReadOnly()
        );
    }

    /// <summary>
    /// Creates an empty result representing zero damage (fully absorbed or missed).
    /// </summary>
    /// <returns>A result with zero damage and no effects.</returns>
    public static DamageIntegrationResult Empty => Create(
        damageDealt: 0,
        damageAfterSoak: 1, // Minimum damage is 1
        soakApplied: 0,
        stressGained: 0);

    // ═══════════════════════════════════════════════════════════════════════════
    // Display
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of the damage result for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing key damage values.</returns>
    /// <example>
    /// <code>
    /// var result = handler.ProcessDamage(characterId, 50, context);
    /// Console.WriteLine(result.ToString());
    /// // Output: "DamageResult: 50 → 40 (Soak 10), +4 stress"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"DamageResult: {DamageDealt} → {DamageAfterSoak} " +
        $"(Soak {SoakApplied}), +{StressGained} stress" +
        (TraumaCheckTriggered ? " [TRAUMA CHECK]" : "");
}
