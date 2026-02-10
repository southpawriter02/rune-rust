// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrTier3AbilityService.cs
// Application service implementing Myrk-gengr Tier 3 abilities
// (Merge with Darkness, Shadow Snare) and the Capstone ultimate (Eclipse).
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Myrk-gengr Tier 3 abilities (Merge with Darkness, Shadow Snare)
/// and the Capstone ultimate (Eclipse).
/// </summary>
/// <remarks>
/// <para>
/// This service handles Tier 3 and Capstone ability operations. It operates
/// on immutable value objects and returns new instances for all state transitions.
/// </para>
/// <para><strong>Supported Abilities (v0.20.4c):</strong></para>
/// <list type="bullet">
///   <item><description>
///     <b>Merge with Darkness:</b> Active transformation (3 AP, 25 Essence).
///     Become incorporeal — phase through objects, immune to physical attacks.
///     Vulnerable to magical light. Duration: 1 turn, extendable.
///   </description></item>
///   <item><description>
///     <b>Shadow Snare:</b> Active control (2 AP, 20 Essence).
///     Root target in shadow tendrils (DC 14 save, 2-turn duration).
///     Corruption risk against Coherent targets.
///   </description></item>
///   <item><description>
///     <b>Eclipse:</b> Capstone ultimate (5 AP, 40 Essence).
///     8-space radius darkness zone for 3 turns. Extinguishes lights,
///     grants 50% concealment, +10 Essence/turn. Always +2 Corruption.
///     Once per combat.
///   </description></item>
/// </list>
/// <para>
/// <b>Tier 3 requires 16 PP invested in the Myrk-gengr ability tree.</b>
/// Each Tier 3 ability costs 5 PP. The Capstone requires 24 PP and costs 6 PP.
/// </para>
/// </remarks>
/// <seealso cref="MyrkgengrTier2AbilityService"/>
/// <seealso cref="IShadowCorruptionService"/>
public class MyrkgengrTier3AbilityService
{
    private readonly ILogger<MyrkgengrTier3AbilityService> _logger;
    private readonly IShadowCorruptionService _corruptionService;
    private readonly Random _random;

    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Shadow Essence cost for Merge with Darkness.</summary>
    public const int MergeWithDarknessEssenceCost = 25;

    /// <summary>Shadow Essence cost for Shadow Snare.</summary>
    public const int ShadowSnareEssenceCost = 20;

    /// <summary>Shadow Essence cost for Eclipse.</summary>
    public const int EclipseEssenceCost = 40;

    /// <summary>PP threshold required to unlock Tier 3 abilities.</summary>
    public const int Tier3PPThreshold = 16;

    /// <summary>PP threshold required to unlock the Capstone ability.</summary>
    public const int CapstonePPThreshold = 24;

    /// <summary>Corruption gain per incorporeal extension.</summary>
    public const int ExtensionCorruptionGain = 1;

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new instance of <see cref="MyrkgengrTier3AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    /// <param name="corruptionService">Service for evaluating Corruption risk.</param>
    /// <param name="random">Optional random instance for testability. Defaults to shared instance.</param>
    public MyrkgengrTier3AbilityService(
        ILogger<MyrkgengrTier3AbilityService> logger,
        IShadowCorruptionService corruptionService,
        Random? random = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _random = random ?? Random.Shared;
    }

    // ═══════ Merge with Darkness ═══════

    /// <summary>
    /// Checks whether Merge with Darkness can be executed under current conditions.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="lightLevel">Light level at the caster's position.</param>
    /// <returns>True if the caster has sufficient essence and is not in bright light.</returns>
    public bool CanExecuteMergeWithDarkness(
        ShadowEssenceResource resource,
        LightLevelType lightLevel)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return resource.CurrentEssence >= MergeWithDarknessEssenceCost
               && lightLevel != LightLevelType.BrightLight;
    }

    /// <summary>
    /// Executes Merge with Darkness, becoming incorporeal.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="lightLevel">Light level at the caster's position.</param>
    /// <returns>
    /// A tuple of (incorporealState, updatedResource, corruptionResult, errorMessage).
    /// On failure, incorporealState is null and errorMessage describes the reason.
    /// </returns>
    public (IncorporealState? State, ShadowEssenceResource Resource, CorruptionRiskResult? Corruption, string? Error)
        ExecuteMergeWithDarkness(
            ShadowEssenceResource resource,
            LightLevelType lightLevel)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _logger.LogInformation(
            "Attempting Merge with Darkness at light level {LightLevel} with {CurrentEssence}/{MaxEssence} Essence",
            lightLevel, resource.CurrentEssence, resource.MaxEssence);

        // Validate light level
        if (lightLevel == LightLevelType.BrightLight)
        {
            _logger.LogWarning("Merge with Darkness failed: cannot activate in bright light");
            return (null, resource, null, "Cannot merge with darkness in bright light.");
        }

        // Attempt to spend essence
        var (success, updatedResource) = resource.TrySpend(MergeWithDarknessEssenceCost);
        if (!success)
        {
            _logger.LogWarning(
                "Merge with Darkness failed: insufficient Essence ({Current}/{Required})",
                resource.CurrentEssence, MergeWithDarknessEssenceCost);
            return (null, resource, null,
                $"Insufficient Shadow Essence. Need {MergeWithDarknessEssenceCost}, have {resource.CurrentEssence}.");
        }

        // Create incorporeal state
        var state = IncorporealState.Create();

        // Evaluate corruption risk
        var corruptionResult = _corruptionService.EvaluateRisk(
            MyrkgengrAbilityId.MergeWithDarkness,
            lightLevel);

        _logger.LogInformation(
            "Merge with Darkness activated. Incorporeal for {Duration} turn(s). Essence: {Current}/{Max}. Corruption: {Triggered}",
            state.RemainingTurns, updatedResource.CurrentEssence, updatedResource.MaxEssence,
            corruptionResult.RiskTriggered);

        return (state, updatedResource, corruptionResult, null);
    }

    /// <summary>
    /// Extends the incorporeal state by one additional turn.
    /// </summary>
    /// <param name="state">Current incorporeal state.</param>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <returns>
    /// A tuple of (updatedState, updatedResource, corruptionGain).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// When the state cannot be extended (inactive or max extensions reached).
    /// </exception>
    public (IncorporealState State, ShadowEssenceResource Resource, int CorruptionGain)
        ExtendIncorporealState(
            IncorporealState state,
            ShadowEssenceResource resource)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resource);

        _logger.LogInformation(
            "Attempting to extend incorporeal state. Extensions: {Count}/{Max}. Essence: {Current}",
            state.ExtensionCount, IncorporealState.MaxExtensions, resource.CurrentEssence);

        if (!state.CanExtend)
        {
            throw new InvalidOperationException(
                $"Cannot extend incorporeal state. Active={state.IsActive}, Extensions={state.ExtensionCount}/{IncorporealState.MaxExtensions}");
        }

        var (success, updatedResource) = resource.TrySpend(IncorporealState.ExtensionEssenceCost);
        if (!success)
        {
            throw new InvalidOperationException(
                $"Insufficient Essence for extension. Need {IncorporealState.ExtensionEssenceCost}, have {resource.CurrentEssence}.");
        }

        var updatedState = state.Extend(ExtensionCorruptionGain);

        _logger.LogInformation(
            "Incorporeal state extended. Turns: {Turns}, Extensions: {Count}/{Max}, Corruption: +{Gain}",
            updatedState.RemainingTurns, updatedState.ExtensionCount,
            IncorporealState.MaxExtensions, ExtensionCorruptionGain);

        return (updatedState, updatedResource, ExtensionCorruptionGain);
    }

    // ═══════ Shadow Snare ═══════

    /// <summary>
    /// Checks whether Shadow Snare can be executed under current conditions.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <returns>True if the caster has sufficient essence.</returns>
    public bool CanExecuteShadowSnare(ShadowEssenceResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return resource.CurrentEssence >= ShadowSnareEssenceCost;
    }

    /// <summary>
    /// Executes Shadow Snare, rooting a target in shadow tendrils.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="casterId">ID of the caster.</param>
    /// <param name="targetId">ID of the target to snare.</param>
    /// <param name="lightLevel">Light level at the caster's position.</param>
    /// <param name="targetIsCoherent">Whether the target is Coherent-aligned.</param>
    /// <returns>
    /// A tuple of (snareEffect, updatedResource, corruptionResult, errorMessage).
    /// On failure, snareEffect is null and errorMessage describes the reason.
    /// </returns>
    public (ShadowSnareEffect? Snare, ShadowEssenceResource Resource, CorruptionRiskResult? Corruption, string? Error)
        ExecuteShadowSnare(
            ShadowEssenceResource resource,
            Guid casterId,
            Guid targetId,
            LightLevelType lightLevel,
            bool targetIsCoherent = false)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _logger.LogInformation(
            "Attempting Shadow Snare on target {TargetId} at light level {LightLevel}. Coherent: {IsCoherent}. Essence: {Current}/{Max}",
            targetId, lightLevel, targetIsCoherent, resource.CurrentEssence, resource.MaxEssence);

        // Attempt to spend essence
        var (success, updatedResource) = resource.TrySpend(ShadowSnareEssenceCost);
        if (!success)
        {
            _logger.LogWarning(
                "Shadow Snare failed: insufficient Essence ({Current}/{Required})",
                resource.CurrentEssence, ShadowSnareEssenceCost);
            return (null, resource, null,
                $"Insufficient Shadow Essence. Need {ShadowSnareEssenceCost}, have {resource.CurrentEssence}.");
        }

        // Create snare effect
        var snare = ShadowSnareEffect.Create(casterId, targetId);

        // Evaluate corruption risk (higher if target is Coherent)
        var corruptionResult = _corruptionService.EvaluateRisk(
            MyrkgengrAbilityId.ShadowSnare,
            lightLevel,
            targetIsCoherent);

        _logger.LogInformation(
            "Shadow Snare applied to {TargetId}. Duration: {Duration} turns, DC: {SaveDC}. Corruption: {Triggered}",
            targetId, snare.RemainingTurns, snare.SaveDC, corruptionResult.RiskTriggered);

        return (snare, updatedResource, corruptionResult, null);
    }

    /// <summary>
    /// Processes a target's attempt to escape a Shadow Snare.
    /// </summary>
    /// <param name="snare">Current snare effect.</param>
    /// <param name="saveRoll">The target's save roll (including modifiers).</param>
    /// <returns>
    /// A tuple of (updatedSnare, whether the target escaped).
    /// </returns>
    public (ShadowSnareEffect Snare, bool Escaped) AttemptSnareEscape(
        ShadowSnareEffect snare,
        int saveRoll)
    {
        ArgumentNullException.ThrowIfNull(snare);

        _logger.LogInformation(
            "Target {TargetId} attempting to escape Shadow Snare. Roll: {Roll}, DC: {DC}",
            snare.TargetId, saveRoll, snare.SaveDC);

        var (updatedSnare, escaped) = snare.AttemptEscape(saveRoll);

        if (escaped)
        {
            _logger.LogInformation(
                "Target {TargetId} escaped Shadow Snare (roll {Roll} >= DC {DC})",
                snare.TargetId, saveRoll, snare.SaveDC);
        }
        else
        {
            _logger.LogInformation(
                "Target {TargetId} failed to escape Shadow Snare (roll {Roll} < DC {DC}). Attempts: {Attempts}",
                snare.TargetId, saveRoll, snare.SaveDC, updatedSnare.EscapeAttempts);
        }

        return (updatedSnare, escaped);
    }

    // ═══════ Eclipse (Capstone) ═══════

    /// <summary>
    /// Checks whether Eclipse can be executed under current conditions.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="hasUsedEclipseThisCombat">Whether Eclipse has already been used this combat.</param>
    /// <returns>True if the caster has sufficient essence and hasn't used Eclipse this combat.</returns>
    public bool CanExecuteEclipse(
        ShadowEssenceResource resource,
        bool hasUsedEclipseThisCombat)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return resource.CurrentEssence >= EclipseEssenceCost
               && !hasUsedEclipseThisCombat;
    }

    /// <summary>
    /// Executes Eclipse, creating a zone of total darkness.
    /// </summary>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <param name="casterId">ID of the caster.</param>
    /// <param name="centerX">X coordinate for zone center.</param>
    /// <param name="centerY">Y coordinate for zone center.</param>
    /// <returns>
    /// A tuple of (eclipseZone, updatedResource, corruptionResult, errorMessage).
    /// On failure, eclipseZone is null and errorMessage describes the reason.
    /// </returns>
    public (EclipseZone? Zone, ShadowEssenceResource Resource, CorruptionRiskResult? Corruption, string? Error)
        ExecuteEclipse(
            ShadowEssenceResource resource,
            Guid casterId,
            int centerX,
            int centerY)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _logger.LogInformation(
            "Attempting Eclipse at position ({X},{Y}). Essence: {Current}/{Max}",
            centerX, centerY, resource.CurrentEssence, resource.MaxEssence);

        // Attempt to spend essence
        var (success, updatedResource) = resource.TrySpend(EclipseEssenceCost);
        if (!success)
        {
            _logger.LogWarning(
                "Eclipse failed: insufficient Essence ({Current}/{Required})",
                resource.CurrentEssence, EclipseEssenceCost);
            return (null, resource, null,
                $"Insufficient Shadow Essence. Need {EclipseEssenceCost}, have {resource.CurrentEssence}.");
        }

        // Create eclipse zone
        var zone = EclipseZone.Create(casterId, centerX, centerY);

        // Eclipse ALWAYS triggers corruption (+2 mandatory)
        var corruptionResult = _corruptionService.EvaluateRisk(
            MyrkgengrAbilityId.Eclipse,
            LightLevelType.Darkness);

        _logger.LogInformation(
            "Eclipse activated at ({X},{Y}). Radius: {Radius}, Duration: {Duration} turns. " +
            "Mandatory corruption: +{Corruption}. Essence: {Current}/{Max}",
            centerX, centerY, zone.Radius, zone.RemainingTurns,
            EclipseZone.MandatoryCorruption, updatedResource.CurrentEssence, updatedResource.MaxEssence);

        return (zone, updatedResource, corruptionResult, null);
    }

    /// <summary>
    /// Processes a turn tick for an active Eclipse zone, regenerating essence.
    /// </summary>
    /// <param name="zone">Current Eclipse zone.</param>
    /// <param name="resource">Current Shadow Essence resource.</param>
    /// <returns>
    /// A tuple of (updatedZone, updatedResource) reflecting the turn tick
    /// and essence regeneration.
    /// </returns>
    public (EclipseZone Zone, ShadowEssenceResource Resource) TickEclipseZone(
        EclipseZone zone,
        ShadowEssenceResource resource)
    {
        ArgumentNullException.ThrowIfNull(zone);
        ArgumentNullException.ThrowIfNull(resource);

        if (!zone.IsActive)
        {
            _logger.LogDebug("Eclipse zone tick skipped: zone is inactive");
            return (zone, resource);
        }

        // Regenerate essence
        var (_, regenAmount) = zone.GetCasterBenefits();
        var updatedResource = resource.Generate(regenAmount);

        _logger.LogInformation(
            "Eclipse zone tick. Regenerated {Regen} Essence. Resource: {Current}/{Max}. Turns remaining: {Turns}",
            regenAmount, updatedResource.CurrentEssence, updatedResource.MaxEssence,
            zone.RemainingTurns);

        // Tick down zone duration
        var updatedZone = zone.TickDown();

        if (!updatedZone.IsActive)
        {
            _logger.LogInformation("Eclipse zone expired after {Duration} turns", EclipseZone.BaseDuration);
        }

        return (updatedZone, updatedResource);
    }

    // ═══════ PP / Unlock Checks ═══════

    /// <summary>
    /// Checks whether a character has enough PP invested to unlock Tier 3 abilities.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Myrk-gengr tree.</param>
    /// <returns>True if the character meets the Tier 3 PP threshold.</returns>
    public bool CanUnlockTier3(int ppInvested)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ppInvested);
        return ppInvested >= Tier3PPThreshold;
    }

    /// <summary>
    /// Checks whether a character has enough PP invested to unlock the Capstone ability.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Myrk-gengr tree.</param>
    /// <returns>True if the character meets the Capstone PP threshold.</returns>
    public bool CanUnlockCapstone(int ppInvested)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ppInvested);
        return ppInvested >= CapstonePPThreshold;
    }
}
