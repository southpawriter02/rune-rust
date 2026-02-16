using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Bone-Setter specialization ability execution.
/// Implements Tier 1 (Foundation) abilities: Field Dressing, Diagnose, and Steady Hands.
/// </summary>
/// <remarks>
/// <para>The Bone-Setter is the first dedicated healing specialization and the first Coherent
/// (no Corruption) path in the v0.20.x series. This service does NOT depend on a corruption
/// service, unlike <see cref="BerserkrAbilityService"/>.</para>
/// <para>Key design decisions:</para>
/// <list type="bullet">
/// <item>No Corruption evaluation step — all Bone-Setter abilities follow the Coherent path</item>
/// <item>Immutable supply operations — spending supplies creates new resource instances</item>
/// <item>Guard-clause chain: null → spec → ability unlocked → AP → supply → execute</item>
/// <item>Target data passed as method parameters (not loaded from repository)</item>
/// </list>
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding.
/// Requires <c>InternalsVisibleTo</c> in the project file to be accessible from test assemblies.</para>
/// </remarks>
public class BoneSetterAbilityService : IBoneSetterAbilityService
{
    // ===== Tier 1 Cost Constants =====

    /// <summary>
    /// AP cost for the Field Dressing healing ability.
    /// </summary>
    private const int FieldDressingApCost = 2;

    /// <summary>
    /// Number of Medical Supplies consumed by Field Dressing.
    /// </summary>
    private const int FieldDressingSupplyCost = 1;

    /// <summary>
    /// AP cost for the Diagnose information ability.
    /// </summary>
    private const int DiagnoseApCost = 1;

    /// <summary>
    /// Healing bonus granted by the Steady Hands passive ability.
    /// </summary>
    private const int SteadyHandsHealingBonus = 2;

    // ===== Tier Unlock PP Requirements =====

    /// <summary>
    /// Minimum PP invested to unlock Tier 2 abilities.
    /// </summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>
    /// Minimum PP invested to unlock Tier 3 abilities.
    /// </summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>
    /// Minimum PP invested to unlock the Capstone ability.
    /// </summary>
    private const int CapstonePpRequirement = 24;

    // ===== Wound Severity Thresholds =====

    /// <summary>
    /// HP percentage threshold for Minor wound severity (90–100%).
    /// </summary>
    private const float MinorThreshold = 0.90f;

    /// <summary>
    /// HP percentage threshold for Light wound severity (70–89%).
    /// </summary>
    private const float LightThreshold = 0.70f;

    /// <summary>
    /// HP percentage threshold for Moderate wound severity (40–69%).
    /// </summary>
    private const float ModerateThreshold = 0.40f;

    /// <summary>
    /// HP percentage threshold for Serious wound severity (15–39%).
    /// </summary>
    private const float SeriousThreshold = 0.15f;

    /// <summary>
    /// The specialization ID string for Bone-Setter.
    /// </summary>
    private const string BoneSetterSpecId = "bone-setter";

    private readonly IBoneSetterMedicalSuppliesService _suppliesService;
    private readonly ILogger<BoneSetterAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoneSetterAbilityService"/> class.
    /// </summary>
    /// <param name="suppliesService">Service for Medical Supplies resource management.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public BoneSetterAbilityService(
        IBoneSetterMedicalSuppliesService suppliesService,
        ILogger<BoneSetterAbilityService> logger)
    {
        _suppliesService = suppliesService ?? throw new ArgumentNullException(nameof(suppliesService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1 Ability Methods (v0.20.6a) =====

    /// <inheritdoc />
    public FieldDressingResult? ExecuteFieldDressing(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Field Dressing failed: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.FieldDressing))
        {
            _logger.LogWarning(
                "Field Dressing failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < FieldDressingApCost)
        {
            _logger.LogWarning(
                "Field Dressing failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, FieldDressingApCost, player.CurrentAP);
            return null;
        }

        // Validate Medical Supply availability
        if (!_suppliesService.ValidateSupplyAvailability(player))
        {
            _logger.LogWarning(
                "Field Dressing failed: {Player} ({PlayerId}) has no Medical Supplies available",
                player.Name, player.Id);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= FieldDressingApCost;

        // Spend 1 Medical Supply (lowest quality first)
        var spentSupply = _suppliesService.SpendSupply(player);
        if (spentSupply == null)
        {
            // This shouldn't happen after validation, but guard defensively
            _logger.LogWarning(
                "Field Dressing failed: {Player} ({PlayerId}) supply spend returned null " +
                "after validation passed — restoring AP",
                player.Name, player.Id);
            player.CurrentAP += FieldDressingApCost;
            return null;
        }

        // Roll healing dice (2d6)
        var healingRoll = Roll2D6();

        // Calculate quality bonus from consumed supply
        var qualityBonus = _suppliesService.CalculateQualityBonus(spentSupply);

        // Check Steady Hands passive bonus
        var steadyHandsBonus = player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.SteadyHands)
            ? SteadyHandsHealingBonus
            : 0;

        // Calculate total healing, capped at target's max HP
        var totalHealing = healingRoll + qualityBonus + steadyHandsBonus;
        var hpAfter = Math.Min(targetCurrentHp + totalHealing, targetMaxHp);

        // Get remaining supplies count for result
        var suppliesRemaining = player.MedicalSupplies?.GetTotalSupplyCount() ?? 0;

        // Build result
        var result = new FieldDressingResult
        {
            TargetId = targetId,
            TargetName = targetName,
            HpBefore = targetCurrentHp,
            HealingRoll = healingRoll,
            QualityBonus = qualityBonus,
            SteadyHandsBonus = steadyHandsBonus,
            HpAfter = hpAfter,
            SuppliesRemaining = suppliesRemaining,
            SupplyTypeUsed = spentSupply.SupplyType.ToString()
        };

        _logger.LogInformation(
            "Field Dressing executed: {Player} ({PlayerId}) healed {Target} ({TargetId}). " +
            "{HealingBreakdown}. HP: {HpBefore} → {HpAfter}/{MaxHp}. " +
            "Supply used: {SupplyType} (Quality: {Quality}). " +
            "Supplies remaining: {Remaining}. AP remaining: {RemainingAP}",
            player.Name, player.Id, targetName, targetId,
            result.GetHealingBreakdown(),
            targetCurrentHp, hpAfter, targetMaxHp,
            spentSupply.SupplyType, spentSupply.Quality,
            suppliesRemaining, player.CurrentAP);

        return result;
    }

    /// <inheritdoc />
    public DiagnoseResult? ExecuteDiagnose(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp,
        IEnumerable<string> statusEffects,
        IEnumerable<string> vulnerabilities,
        IEnumerable<string> resistances)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Diagnose failed: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.Diagnose))
        {
            _logger.LogWarning(
                "Diagnose failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < DiagnoseApCost)
        {
            _logger.LogWarning(
                "Diagnose failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, DiagnoseApCost, player.CurrentAP);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===
        // === No Medical Supply cost — Diagnose is free ===

        // Deduct AP
        player.CurrentAP -= DiagnoseApCost;

        // Classify wound severity from HP percentage
        var woundSeverity = ClassifyWoundSeverity(targetCurrentHp, targetMaxHp);

        // Materialize collections to avoid multiple enumeration
        var effectsList = statusEffects?.ToList() ?? [];
        var vulnerabilitiesList = vulnerabilities?.ToList() ?? [];
        var resistancesList = resistances?.ToList() ?? [];

        // Build result
        var result = new DiagnoseResult
        {
            TargetId = targetId,
            TargetName = targetName,
            CurrentHp = targetCurrentHp,
            MaxHp = targetMaxHp,
            WoundSeverity = woundSeverity,
            StatusEffects = effectsList.AsReadOnly(),
            Vulnerabilities = vulnerabilitiesList.AsReadOnly(),
            Resistances = resistancesList.AsReadOnly()
        };

        _logger.LogInformation(
            "Diagnose executed: {Player} ({PlayerId}) analyzed {Target} ({TargetId}). " +
            "HP: {CurrentHp}/{MaxHp} ({Percentage:P0}). Severity: {Severity}. " +
            "Bloodied: {IsBloodied}. Effects: {Effects}. " +
            "Vulnerabilities: {Vulnerabilities}. Resistances: {Resistances}. " +
            "AP remaining: {RemainingAP}",
            player.Name, player.Id, targetName, targetId,
            targetCurrentHp, targetMaxHp, result.HpPercentage,
            woundSeverity, result.IsBloodied,
            result.GetStatusSummary(),
            vulnerabilitiesList.Count > 0 ? string.Join(", ", vulnerabilitiesList) : "None",
            resistancesList.Count > 0 ? string.Join(", ", resistancesList) : "None",
            player.CurrentAP);

        return result;
    }

    // ===== Utility Methods =====

    /// <inheritdoc />
    public Dictionary<BoneSetterAbilityId, bool> GetAbilityReadiness(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var readiness = new Dictionary<BoneSetterAbilityId, bool>();

        if (!IsBoneSetter(player))
            return readiness;

        var hasSupplies = _suppliesService.ValidateSupplyAvailability(player);

        // Check each unlocked ability's readiness
        foreach (var abilityId in player.UnlockedBoneSetterAbilities)
        {
            var isReady = abilityId switch
            {
                BoneSetterAbilityId.FieldDressing =>
                    player.CurrentAP >= FieldDressingApCost && hasSupplies,

                BoneSetterAbilityId.Diagnose =>
                    player.CurrentAP >= DiagnoseApCost,

                // Passive abilities are always "ready" (they apply automatically)
                BoneSetterAbilityId.SteadyHands => true,

                _ => false
            };

            readiness[abilityId] = isReady;
        }

        return readiness;
    }

    /// <inheritdoc />
    public bool CanUnlockTier2(Player player)
    {
        if (player == null)
            return false;

        if (!IsBoneSetter(player))
            return false;

        return GetPPInvested(player) >= Tier2PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        if (player == null)
            return false;

        if (!IsBoneSetter(player))
            return false;

        return GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        if (player == null)
            return false;

        if (!IsBoneSetter(player))
            return false;

        return GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.GetBoneSetterPPInvested();
    }

    // ===== Dice Roll Methods (internal virtual for testing) =====

    /// <summary>
    /// Rolls 2d6 for Field Dressing base healing.
    /// </summary>
    /// <returns>Sum of two d6 rolls (range: 2–12).</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// </remarks>
    internal virtual int Roll2D6()
    {
        return Random.Shared.Next(1, 7)
             + Random.Shared.Next(1, 7);
    }

    // ===== Private Helper Methods =====

    /// <summary>
    /// Classifies wound severity based on current HP as a percentage of max HP.
    /// </summary>
    /// <param name="currentHp">The target's current HP.</param>
    /// <param name="maxHp">The target's maximum HP.</param>
    /// <returns>The appropriate <see cref="WoundSeverity"/> classification.</returns>
    /// <remarks>
    /// Thresholds: Minor (90–100%), Light (70–89%), Moderate (40–69%),
    /// Serious (15–39%), Critical (1–14%), Unconscious (0%).
    /// </remarks>
    private static WoundSeverity ClassifyWoundSeverity(int currentHp, int maxHp)
    {
        // Edge case: zero max HP or zero current HP
        if (maxHp <= 0 || currentHp <= 0)
            return WoundSeverity.Unconscious;

        var percentage = (float)currentHp / maxHp;

        return percentage switch
        {
            >= MinorThreshold => WoundSeverity.Minor,
            >= LightThreshold => WoundSeverity.Light,
            >= ModerateThreshold => WoundSeverity.Moderate,
            >= SeriousThreshold => WoundSeverity.Serious,
            > 0 => WoundSeverity.Critical,
            _ => WoundSeverity.Unconscious
        };
    }

    /// <summary>
    /// Checks if a player is a Bone-Setter.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's specialization is "bone-setter".</returns>
    private static bool IsBoneSetter(Player player)
    {
        return string.Equals(player.SpecializationId, BoneSetterSpecId, StringComparison.OrdinalIgnoreCase);
    }
}
