// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrAbilityService.cs
// Application service implementing Myrk-gengr Tier 1 abilities:
// Shadow Step, Cloak of Night, and Dark-Adapted.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Myrk-gengr Tier 1 abilities with Shadow Essence management,
/// Corruption risk evaluation, and PP cost tracking.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Supported Abilities (v0.20.4a):</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Shadow Step:</b> Teleport to a shadow within 6 spaces (2 AP, 10 Essence).
///     Generates +5 Essence on arrival in Darkness. Corruption risk in bright light.
///   </description></item>
///   <item><description>
///     <b>Cloak of Night:</b> Shadow concealment stance (1 AP, 5 Essence/turn).
///     +3 Stealth in shadow, -1 and Corruption risk in bright light.
///   </description></item>
///   <item><description>
///     <b>Dark-Adapted:</b> Passive. Removes dim light penalties.
///     +2 Essence/turn in Darkness.
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="IMyrkgengrAbilityService"/>
/// <seealso cref="IShadowCorruptionService"/>
public class MyrkgengrAbilityService : IMyrkgengrAbilityService
{
    private readonly ILogger<MyrkgengrAbilityService> _logger;
    private readonly IShadowCorruptionService _corruptionService;

    // ─────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Shadow Step essence cost.</summary>
    private const int ShadowStepEssenceCost = 10;

    /// <summary>Shadow Step maximum range in grid spaces.</summary>
    private const int ShadowStepRange = 6;

    /// <summary>Shadow Step darkness arrival bonus.</summary>
    private const int ShadowStepDarknessBonus = 5;

    /// <summary>Cloak of Night per-turn maintenance cost.</summary>
    private const int CloakMaintenanceCost = 5;

    /// <summary>Dark-Adapted passive essence generation in Darkness.</summary>
    private const int DarkAdaptedDarknessGeneration = 2;

    /// <summary>PP required to unlock Tier 2 abilities.</summary>
    private const int Tier2PPThreshold = 8;

    // ─────────────────────────────────────────────────────────────────────────
    // PP Cost Table
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps each ability to its PP cost. Tier 1 abilities are free.
    /// </summary>
    private static readonly Dictionary<MyrkgengrAbilityId, int> PPCosts = new()
    {
        // Tier 1: Free
        { MyrkgengrAbilityId.ShadowStep, 0 },
        { MyrkgengrAbilityId.CloakOfNight, 0 },
        { MyrkgengrAbilityId.DarkAdapted, 0 },

        // Tier 2: 4 PP each
        { MyrkgengrAbilityId.UmbralStrike, 4 },
        { MyrkgengrAbilityId.ShadowClone, 4 },
        { MyrkgengrAbilityId.VoidTouched, 4 },

        // Tier 3: 5 PP each
        { MyrkgengrAbilityId.MergeWithDarkness, 5 },
        { MyrkgengrAbilityId.ShadowSnare, 5 },

        // Capstone: 6 PP
        { MyrkgengrAbilityId.Eclipse, 6 }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new instance of the <see cref="MyrkgengrAbilityService"/> class.
    /// </summary>
    /// <param name="logger">Structured logger for ability execution tracing.</param>
    /// <param name="corruptionService">Service for evaluating Corruption risk.</param>
    public MyrkgengrAbilityService(
        ILogger<MyrkgengrAbilityService> logger,
        IShadowCorruptionService corruptionService)
    {
        _logger = logger;
        _corruptionService = corruptionService;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Shadow Step
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool CanExecuteShadowStep(
        ShadowEssenceResource resource,
        ShadowPosition origin,
        ShadowPosition target)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(target);

        return resource.CanSpend(ShadowStepEssenceCost)
            && target.IsValidShadowTarget()
            && target.IsWithinRange(origin, ShadowStepRange);
    }

    /// <inheritdoc />
    public (bool Success, ShadowEssenceResource Resource, CorruptionRiskResult? CorruptionResult)
        ExecuteShadowStep(
            ShadowEssenceResource resource,
            ShadowPosition origin,
            ShadowPosition target)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(target);

        _logger.LogInformation(
            "Executing Shadow Step from ({OriginX}, {OriginY}) to ({TargetX}, {TargetY}). " +
            "Origin light: {OriginLight}, Target light: {TargetLight}",
            origin.X, origin.Y, target.X, target.Y,
            origin.LightLevel, target.LightLevel);

        // ── Validate target ──────────────────────────────────────────────
        if (!target.IsValidShadowTarget())
        {
            _logger.LogWarning(
                "Shadow Step failed: invalid target at ({X}, {Y}). " +
                "Light={Light}, Occupied={Occupied}",
                target.X, target.Y, target.LightLevel, target.IsOccupied);
            return (false, resource, null);
        }

        if (!target.IsWithinRange(origin, ShadowStepRange))
        {
            _logger.LogWarning(
                "Shadow Step failed: target out of range. " +
                "Distance={Distance:F1}, MaxRange={MaxRange}",
                target.DistanceTo(origin), ShadowStepRange);
            return (false, resource, null);
        }

        // ── Spend essence ────────────────────────────────────────────────
        var (spendSuccess, updatedResource) = resource.TrySpend(ShadowStepEssenceCost);
        if (!spendSuccess)
        {
            _logger.LogWarning(
                "Shadow Step failed: insufficient Shadow Essence. " +
                "Required={Required}, Available={Available}",
                ShadowStepEssenceCost, resource.CurrentEssence);
            return (false, resource, null);
        }

        // ── Darkness arrival bonus ───────────────────────────────────────
        if (target.LightLevel == LightLevelType.Darkness)
        {
            updatedResource = updatedResource.Generate(ShadowStepDarknessBonus);
            _logger.LogInformation(
                "Shadow Step: +{Bonus} Shadow Essence from arrival in Darkness. " +
                "Now: {CurrentEssence}/{MaxEssence}",
                ShadowStepDarknessBonus,
                updatedResource.CurrentEssence, updatedResource.MaxEssence);
        }

        // ── Evaluate corruption risk ─────────────────────────────────────
        var corruptionResult = _corruptionService.EvaluateRisk(
            MyrkgengrAbilityId.ShadowStep,
            origin.LightLevel);

        if (corruptionResult.RiskTriggered)
        {
            _logger.LogWarning(
                "Shadow Step triggered Corruption: +{Amount}. {Reason}",
                corruptionResult.CorruptionGained, corruptionResult.Reason);
        }

        _logger.LogInformation(
            "Shadow Step successful: teleported to ({X}, {Y}). " +
            "Essence: {CurrentEssence}/{MaxEssence}",
            target.X, target.Y,
            updatedResource.CurrentEssence, updatedResource.MaxEssence);

        return (true, updatedResource, corruptionResult);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cloak of Night
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public (bool Success, ShadowEssenceResource Resource) ActivateCloakOfNight(
        ShadowEssenceResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _logger.LogInformation(
            "Activating Cloak of Night. Current Essence: {CurrentEssence}/{MaxEssence}",
            resource.CurrentEssence, resource.MaxEssence);

        // Cloak of Night requires essence for at least one turn of maintenance
        if (!resource.CanSpend(CloakMaintenanceCost))
        {
            _logger.LogWarning(
                "Cloak of Night activation failed: insufficient Shadow Essence. " +
                "Required for maintenance: {Required}, Available: {Available}",
                CloakMaintenanceCost, resource.CurrentEssence);
            return (false, resource);
        }

        _logger.LogInformation("Cloak of Night activated successfully");
        return (true, resource);
    }

    /// <inheritdoc />
    public (bool StanceActive, ShadowEssenceResource Resource, CorruptionRiskResult? CorruptionResult)
        MaintainCloakOfNight(
            ShadowEssenceResource resource,
            LightLevelType currentLightLevel)
    {
        ArgumentNullException.ThrowIfNull(resource);

        _logger.LogDebug(
            "Maintaining Cloak of Night. Light: {LightLevel}, " +
            "Essence: {CurrentEssence}/{MaxEssence}",
            currentLightLevel, resource.CurrentEssence, resource.MaxEssence);

        // ── Spend maintenance cost ───────────────────────────────────────
        var (spendSuccess, updatedResource) = resource.TrySpend(CloakMaintenanceCost);
        if (!spendSuccess)
        {
            _logger.LogInformation(
                "Cloak of Night ended: Shadow Essence depleted. " +
                "Available: {Available}, Required: {Required}",
                resource.CurrentEssence, CloakMaintenanceCost);
            return (false, resource, null);
        }

        // ── Evaluate corruption risk ─────────────────────────────────────
        CorruptionRiskResult? corruptionResult = null;
        if (currentLightLevel >= LightLevelType.BrightLight)
        {
            corruptionResult = _corruptionService.EvaluateRisk(
                MyrkgengrAbilityId.CloakOfNight,
                currentLightLevel);

            if (corruptionResult.RiskTriggered)
            {
                _logger.LogWarning(
                    "Cloak of Night in {LightLevel}: Corruption triggered. " +
                    "+{Amount} corruption",
                    currentLightLevel, corruptionResult.CorruptionGained);
            }
        }

        _logger.LogDebug(
            "Cloak of Night maintained. Essence: {CurrentEssence}/{MaxEssence}",
            updatedResource.CurrentEssence, updatedResource.MaxEssence);

        return (true, updatedResource, corruptionResult);
    }

    /// <inheritdoc />
    public int GetCloakOfNightStealthModifier(LightLevelType lightLevel) =>
        lightLevel switch
        {
            LightLevelType.Darkness or LightLevelType.DimLight => 3,
            LightLevelType.NormalLight => 1,
            _ => -1  // BrightLight and Sunlight
        };

    /// <inheritdoc />
    public bool GrantsSilentMovement(LightLevelType lightLevel) =>
        lightLevel <= LightLevelType.DimLight;

    // ═══════════════════════════════════════════════════════════════════════════
    // Dark-Adapted
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int GetDarkAdaptedGeneration(LightLevelType lightLevel) =>
        lightLevel == LightLevelType.Darkness ? DarkAdaptedDarknessGeneration : 0;

    /// <inheritdoc />
    public bool RemovesDimLightPenalties(LightLevelType lightLevel) =>
        lightLevel == LightLevelType.DimLight;

    // ═══════════════════════════════════════════════════════════════════════════
    // PP Cost Management
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int GetAbilityPPCost(MyrkgengrAbilityId abilityId) =>
        PPCosts.TryGetValue(abilityId, out var cost) ? cost : 0;

    /// <inheritdoc />
    public bool CanUnlockTier2(int ppInvested) => ppInvested >= Tier2PPThreshold;

    /// <inheritdoc />
    public int CalculatePPInvested(IReadOnlyList<MyrkgengrAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);
        return unlockedAbilities.Sum(GetAbilityPPCost);
    }
}
