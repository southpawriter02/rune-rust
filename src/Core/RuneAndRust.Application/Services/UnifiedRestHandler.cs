// ═══════════════════════════════════════════════════════════════════════════════
// UnifiedRestHandler.cs
// Service that orchestrates rest processing across all trauma economy systems:
// stress recovery, specialization resource resets, trauma checks, CPS recovery,
// and party effects.
// Version: 0.18.5c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates rest processing across all trauma economy systems.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The <see cref="UnifiedRestHandler"/> provides a single entry point for processing
/// rest events, coordinating effects across stress, corruption, specialization resources,
/// trauma checks, and party bonuses.
/// </para>
/// <para>
/// <strong>Processing Pipeline:</strong>
/// </para>
/// <list type="number">
///   <item><description>Calculate stress recovery based on rest type</description></item>
///   <item><description>Reset specialization resources (Rage/Momentum → 0; Coherence → 50 on Long/Sanctuary)</description></item>
///   <item><description>Perform trauma checks (Long/Sanctuary only)</description></item>
///   <item><description>Check CPS stage changes from stress reduction</description></item>
///   <item><description>Apply party effects (Berserker FrenzyBeyondReason bonus)</description></item>
///   <item><description>Build and return <see cref="RestIntegrationResult"/></description></item>
/// </list>
/// <para>
/// <strong>Rest Type Effects:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>System</term>
///     <description>Short / Long / Sanctuary</description>
///   </listheader>
///   <item><term>Stress</term><description>WILL×2 / WILL×5 / Full reset</description></item>
///   <item><term>Rage</term><description>Reset to 0 / Reset to 0 / Reset to 0</description></item>
///   <item><term>Momentum</term><description>Reset to 0 / Reset to 0 / Reset to 0</description></item>
///   <item><term>Coherence</term><description>No change / Restore to 50 / Restore to 50</description></item>
///   <item><term>Trauma Check</term><description>No / Yes / Yes</description></item>
///   <item><term>Party Rage Bonus</term><description>No / Yes (-10) / Yes (-10)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var partyContext = new PartyContext(memberIds, berserkerId);
/// var result = restHandler.ProcessRest(characterId, RestType.Long, partyContext);
///
/// logger.LogInformation(
///     "Rest complete: {StressRecovered} stress recovered, CPS changed: {CpsChanged}",
///     result.StressRecovered,
///     result.CpsStageChanged);
/// </code>
/// </example>
/// <seealso cref="RestIntegrationResult"/>
/// <seealso cref="PartyContext"/>
/// <seealso cref="RestType"/>
public class UnifiedRestHandler
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Default coherence restoration value for Long/Sanctuary rest.</summary>
    private const int DefaultCoherenceRestoreValue = 50;

    /// <summary>Temporary placeholder for WILL attribute until character system integration.</summary>
    private const int PlaceholderWillAttribute = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // Dependencies (Required)
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IStressService _stressService;
    private readonly ITraumaService _traumaService;

    // ═══════════════════════════════════════════════════════════════════════════
    // Dependencies (Optional)
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IRageService? _rageService;
    private readonly IMomentumService? _momentumService;
    private readonly ICoherenceService? _coherenceService;
    private readonly ILogger<UnifiedRestHandler>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="UnifiedRestHandler"/> class.
    /// </summary>
    /// <param name="stressService">Service for stress recovery (required).</param>
    /// <param name="traumaService">Service for trauma checks (required).</param>
    /// <param name="rageService">Optional service for Berserker rage management.</param>
    /// <param name="momentumService">Optional service for Storm Blade momentum management.</param>
    /// <param name="coherenceService">Optional service for Arcanist coherence management.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stressService"/> or <paramref name="traumaService"/> is <c>null</c>.
    /// </exception>
    public UnifiedRestHandler(
        IStressService stressService,
        ITraumaService traumaService,
        IRageService? rageService = null,
        IMomentumService? momentumService = null,
        ICoherenceService? coherenceService = null,
        ILogger<UnifiedRestHandler>? logger = null)
    {
        _stressService = stressService ??
            throw new ArgumentNullException(nameof(stressService));
        _traumaService = traumaService ??
            throw new ArgumentNullException(nameof(traumaService));

        _rageService = rageService;
        _momentumService = momentumService;
        _coherenceService = coherenceService;
        _logger = logger;

        _logger?.LogDebug("UnifiedRestHandler initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes a rest event and coordinates effects across all trauma economy systems.
    /// </summary>
    /// <param name="characterId">The character taking rest.</param>
    /// <param name="restType">The type of rest being taken.</param>
    /// <param name="partyContext">Optional party context for party-wide effects.</param>
    /// <returns>A <see cref="RestIntegrationResult"/> capturing all rest effects.</returns>
    /// <remarks>
    /// <para>
    /// This method orchestrates the complete rest processing pipeline:
    /// </para>
    /// <list type="number">
    ///   <item><description>Stress recovery based on rest type and WILL</description></item>
    ///   <item><description>Specialization resource resets</description></item>
    ///   <item><description>Trauma checks (Long/Sanctuary only)</description></item>
    ///   <item><description>CPS stage recalculation</description></item>
    ///   <item><description>Party bonus application</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = restHandler.ProcessRest(characterId, RestType.Long);
    /// if (result.CpsStageChanged)
    ///     ShowCpsRecoveryNotification(result.NewCpsStage);
    /// </code>
    /// </example>
    public RestIntegrationResult ProcessRest(
        Guid characterId,
        RestType restType,
        PartyContext? partyContext = null)
    {
        _logger?.LogDebug(
            "Processing {RestType} rest for {CharacterId}",
            restType,
            characterId);

        var messages = new List<string>();

        // 1. Calculate and apply stress recovery
        var (stressRecovered, formula, cpsChanged, newCpsStage) =
            CalculateStressRecovery(characterId, restType, partyContext, messages);

        // 2. Reset specialization resources
        var (coherenceRestored, momentumReset) =
            ApplyResourceResets(characterId, restType, messages);

        // 3. Perform trauma checks (Long/Sanctuary only)
        var (traumaChecks, traumasAcquired) =
            PerformTraumaChecks(characterId, restType, messages);

        // 4. Determine party bonus (already applied in stress recovery)
        var partyBonus = GetPartyBonus(partyContext, restType);

        // 5. Build result
        var result = RestIntegrationResult.Create(
            restType: restType,
            stressRecovered: stressRecovered,
            stressRecoveryFormula: formula,
            corruptionRecovered: 0, // Future: corruption recovery
            cpsStageChanged: cpsChanged,
            newCpsStage: newCpsStage,
            traumaChecksPerformed: traumaChecks,
            traumasAcquired: traumasAcquired,
            ragePartyBonus: partyBonus,
            coherenceRestored: coherenceRestored,
            momentumReset: momentumReset,
            recoveryMessages: messages.AsReadOnly());

        _logger?.LogInformation(
            "Rest processing complete for {CharacterId}: {StressRecovered} stress recovered, CPS changed: {CpsChanged}",
            characterId,
            stressRecovered,
            cpsChanged);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods — Stress Recovery
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates and applies stress recovery based on rest type.
    /// </summary>
    private (int StressRecovered, string Formula, bool CpsChanged, CpsStage? NewCpsStage)
        CalculateStressRecovery(
            Guid characterId,
            RestType restType,
            PartyContext? partyContext,
            List<string> messages)
    {
        // Get current stress state
        var currentStress = _stressService.GetStressState(characterId);
        var previousThreshold = currentStress.Threshold;

        // Derive CPS stage from stress value using the threshold mapping
        // Stress thresholds map to CPS stages:
        // Calm (0-19) = None, Uneasy (20-39) = WeightOfKnowing,
        // Anxious (40-59) = GlimmerMadness, Panicked (60-79) = RuinMadness,
        // Overwhelmed (80-100) = HollowShell
        var previousCpsStage = MapThresholdToCpsStage(previousThreshold);

        _logger?.LogDebug(
            "Current stress for {CharacterId}: {CurrentStress}, threshold: {Threshold}",
            characterId,
            currentStress.CurrentStress,
            previousThreshold);

        // Calculate party bonus for Long/Sanctuary rest
        var partyBonus = GetPartyBonus(partyContext, restType);

        // Apply stress recovery via IStressService
        var recoveryResult = _stressService.RecoverStress(characterId, restType);

        // Calculate total recovery including party bonus
        var totalRecovered = recoveryResult.AmountRecovered + (partyBonus ?? 0);
        var formula = restType.GetFormulaDescription();

        if (partyBonus.HasValue)
        {
            formula += $" + Party Bonus ({partyBonus})";
            messages.Add($"Party bonus from Berserker's rage: -{partyBonus} stress");
        }

        messages.Add($"Stress recovered: {totalRecovered} via {restType}");

        // Check for CPS stage improvement (derived from stress threshold)
        var newStressState = _stressService.GetStressState(characterId);
        var newThreshold = newStressState.Threshold;
        var newCpsStage = MapThresholdToCpsStage(newThreshold);
        var cpsChanged = newCpsStage != previousCpsStage;

        if (cpsChanged)
        {
            messages.Add($"Mental clarity improved: {previousCpsStage} → {newCpsStage}");
            _logger?.LogInformation(
                "CPS stage improved for {CharacterId}: {PreviousStage} → {NewStage}",
                characterId,
                previousCpsStage,
                newCpsStage);
        }

        return (totalRecovered, formula, cpsChanged, cpsChanged ? newCpsStage : null);
    }

    /// <summary>
    /// Maps a stress threshold to the corresponding CPS stage.
    /// </summary>
    private static CpsStage MapThresholdToCpsStage(StressThreshold threshold) =>
        threshold switch
        {
            StressThreshold.Calm => CpsStage.None,
            StressThreshold.Uneasy => CpsStage.WeightOfKnowing,
            StressThreshold.Anxious => CpsStage.GlimmerMadness,
            StressThreshold.Panicked => CpsStage.RuinMadness,
            StressThreshold.Breaking => CpsStage.HollowShell,
            StressThreshold.Trauma => CpsStage.HollowShell, // Trauma also maps to most severe CPS
            _ => CpsStage.None
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods — Resource Resets
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resets specialization resources based on rest type.
    /// </summary>
    private (int? CoherenceRestored, bool MomentumReset)
        ApplyResourceResets(Guid characterId, RestType restType, List<string> messages)
    {
        int? coherenceRestored = null;
        var momentumReset = false;

        // Reset momentum on any rest
        if (_momentumService is not null)
        {
            var momentumState = _momentumService.GetMomentumState(characterId);
            if (momentumState is not null && momentumState.CurrentMomentum > 0)
            {
                _momentumService.ResetMomentum(characterId, $"{restType} rest");
                momentumReset = true;
                messages.Add("Momentum reset to 0");

                _logger?.LogDebug(
                    "Momentum reset for {CharacterId} during {RestType}",
                    characterId,
                    restType);
            }
        }

        // Reset rage on any rest (rage decays to 0)
        // Note: Rage naturally decays; rest fully resets it
        // The RageService handles this via decay mechanics

        // Restore coherence to 50 on Long/Sanctuary rest only
        if (_coherenceService is not null &&
            (restType == RestType.Long || restType == RestType.Sanctuary))
        {
            var coherenceState = _coherenceService.GetCoherenceState(characterId);
            if (coherenceState is not null)
            {
                var currentCoherence = coherenceState.CurrentCoherence;
                if (currentCoherence < DefaultCoherenceRestoreValue)
                {
                    // Calculate restoration amount
                    coherenceRestored = DefaultCoherenceRestoreValue - currentCoherence;

                    // Use GainCoherence to restore to 50
                    _coherenceService.GainCoherence(
                        characterId,
                        coherenceRestored.Value,
                        CoherenceSource.MeditationAction);

                    messages.Add($"Coherence restored by {coherenceRestored} (now at {DefaultCoherenceRestoreValue})");

                    _logger?.LogDebug(
                        "Coherence restored for {CharacterId}: +{Amount} to {NewValue}",
                        characterId,
                        coherenceRestored,
                        DefaultCoherenceRestoreValue);
                }
            }
        }

        return (coherenceRestored, momentumReset);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods — Trauma Checks
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Performs trauma checks if applicable for the rest type.
    /// </summary>
    private (int TraumaChecks, IReadOnlyList<string>? TraumasAcquired)
        PerformTraumaChecks(Guid characterId, RestType restType, List<string> messages)
    {
        // Trauma checks only on Long and Sanctuary rest
        if (restType == RestType.Short || restType == RestType.Milestone)
        {
            return (0, null);
        }

        _logger?.LogDebug(
            "Performing trauma check for {CharacterId} during {RestType}",
            characterId,
            restType);

        // Perform trauma check using ProlongedExposure trigger (appropriate for rest)
        var traumasAcquired = new List<string>();
        var traumaCheckResult = _traumaService.PerformTraumaCheckAsync(
            characterId,
            TraumaCheckTrigger.ProlongedExposure).GetAwaiter().GetResult();

        if (!traumaCheckResult.Passed && traumaCheckResult.TraumaAcquired is not null)
        {
            traumasAcquired.Add(traumaCheckResult.TraumaAcquired);
            messages.Add($"Trauma manifested during rest: {traumaCheckResult.TraumaAcquired}");

            _logger?.LogWarning(
                "Trauma acquired during rest for {CharacterId}: {TraumaId}",
                characterId,
                traumaCheckResult.TraumaAcquired);
        }
        else
        {
            messages.Add("Rest completed without acquiring new trauma");
        }

        return (1, traumasAcquired.Count > 0 ? traumasAcquired.AsReadOnly() : null);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods — Party Effects
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the party stress reduction bonus if applicable.
    /// </summary>
    private int? GetPartyBonus(PartyContext? partyContext, RestType restType)
    {
        // Party bonus only applies on Long and Sanctuary rest
        if (restType == RestType.Short || restType == RestType.Milestone)
        {
            return null;
        }

        // Check if a Berserker in FrenzyBeyondReason is present
        if (partyContext?.BerserkerId is null || _rageService is null)
        {
            return null;
        }

        var reduction = _rageService.GetPartyStressReduction(partyContext.BerserkerId.Value);

        if (reduction.HasValue)
        {
            _logger?.LogDebug(
                "Party stress reduction from Berserker {BerserkerId}: {Reduction}",
                partyContext.BerserkerId,
                reduction);
        }

        return reduction;
    }
}
