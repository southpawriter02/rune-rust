using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Bone-Setter specialization ability execution.
/// Implements Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) abilities.
/// </summary>
/// <remarks>
/// <para>The Bone-Setter is the first dedicated healing specialization and the first Coherent
/// (no Corruption) path in the v0.20.x series. This service does NOT depend on a corruption
/// service, unlike <see cref="BerserkrAbilityService"/>.</para>
/// <para>Tier 1 abilities: Field Dressing, Diagnose, Steady Hands (v0.20.6a).</para>
/// <para>Tier 2 abilities: Emergency Surgery, Antidote Craft, Triage (v0.20.6b).</para>
/// <para>Tier 3 abilities: Resuscitate, Preventive Care (v0.20.6c).</para>
/// <para>Capstone ability: Miracle Worker (v0.20.6c).</para>
/// <para>Key design decisions:</para>
/// <list type="bullet">
/// <item>No Corruption evaluation step — all Bone-Setter abilities follow the Coherent path</item>
/// <item>Immutable supply operations — spending supplies creates new resource instances</item>
/// <item>Guard-clause chain: null → spec → ability unlocked → AP → supply → execute</item>
/// <item>Target data passed as method parameters (not loaded from repository)</item>
/// <item>Emergency Surgery uses highest-quality supply (opposite of Field Dressing)</item>
/// <item>Antidote Craft always succeeds — 100% success rate, no DC check</item>
/// <item>Triage is evaluated by other abilities to apply bonus healing inline</item>
/// <item>Resuscitate consumes 2 supplies sequentially (lowest quality first)</item>
/// <item>Miracle Worker uses long-rest cooldown (not per-combat)</item>
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

    // ===== Tier 2 Cost Constants (v0.20.6b) =====

    /// <summary>
    /// AP cost for the Emergency Surgery high-impact healing ability.
    /// </summary>
    private const int EmergencySurgeryApCost = 3;

    /// <summary>
    /// Number of Medical Supplies consumed by Emergency Surgery.
    /// </summary>
    private const int EmergencySurgerySupplyCost = 1;

    /// <summary>
    /// AP cost for the Antidote Craft ability.
    /// </summary>
    private const int AntidoteCraftApCost = 2;

    /// <summary>
    /// Number of Herbs supplies consumed by Antidote Craft.
    /// </summary>
    private const int AntidoteCraftHerbsCost = 1;

    /// <summary>
    /// Number of Plant Fiber materials required for Antidote Craft.
    /// </summary>
    private const int AntidoteCraftPlantFiberCost = 2;

    /// <summary>
    /// Number of Mineral Powder materials required for Antidote Craft.
    /// </summary>
    private const int AntidoteCraftMineralPowderCost = 1;

    /// <summary>
    /// Maximum quality rating for crafted items.
    /// </summary>
    private const int MaxCraftedQuality = 5;

    /// <summary>
    /// Minimum material quality for the high-quality material bonus.
    /// All materials must be at or above this threshold for +1 quality bonus.
    /// </summary>
    private const int HighQualityMaterialThreshold = 3;

    /// <summary>
    /// Radius in spaces for the Triage passive ability evaluation.
    /// </summary>
    private const int TriageRadius = 5;

    /// <summary>
    /// Healing bonus multiplier for the Triage passive.
    /// Most wounded ally receives BaseHealing * this value as bonus.
    /// </summary>
    private const float TriageBonusMultiplier = 0.5f;

    /// <summary>
    /// Recovery bonus for targets in the Recovering condition (+3).
    /// </summary>
    private const int RecoveryBonusRecovering = 3;

    /// <summary>
    /// Recovery bonus for targets in the Incapacitated condition (+1).
    /// </summary>
    private const int RecoveryBonusIncapacitated = 1;

    /// <summary>
    /// Recovery bonus for targets in the Dying condition (+4, maximum).
    /// </summary>
    private const int RecoveryBonusDying = 4;

    /// <summary>
    /// Recipe name for the Basic Antidote crafting recipe.
    /// </summary>
    private const string BasicAntidoteRecipeName = "Basic Antidote";

    // ===== Tier 3 Cost Constants (v0.20.6c) =====

    /// <summary>
    /// AP cost for the Resuscitate revival ability.
    /// </summary>
    private const int ResuscitateApCost = 4;

    /// <summary>
    /// Number of Medical Supplies consumed by Resuscitate.
    /// Two supplies are spent sequentially (lowest quality first).
    /// </summary>
    private const int ResuscitateSupplyCost = 2;

    /// <summary>
    /// Radius in spaces for the Preventive Care passive aura.
    /// All allies within this distance receive saving throw bonuses.
    /// </summary>
    private const int PreventiveCareRadius = 5;

    /// <summary>
    /// Saving throw bonus granted by the Preventive Care aura
    /// against poison and disease effects.
    /// </summary>
    private const int PreventiveCareSaveBonus = 1;

    // ===== Capstone Cost Constants (v0.20.6c) =====

    /// <summary>
    /// AP cost for the Miracle Worker capstone ability.
    /// </summary>
    private const int MiracleWorkerApCost = 5;

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

    // ===== Tier 2 Ability Methods (v0.20.6b) =====

    /// <inheritdoc />
    public EmergencySurgeryResult? ExecuteEmergencySurgery(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp,
        RecoveryCondition targetCondition)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Emergency Surgery failed: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.EmergencySurgery))
        {
            _logger.LogWarning(
                "Emergency Surgery failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < EmergencySurgeryApCost)
        {
            _logger.LogWarning(
                "Emergency Surgery failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, EmergencySurgeryApCost, player.CurrentAP);
            return null;
        }

        // Validate Medical Supply availability
        if (!_suppliesService.ValidateSupplyAvailability(player))
        {
            _logger.LogWarning(
                "Emergency Surgery failed: {Player} ({PlayerId}) has no Medical Supplies available",
                player.Name, player.Id);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= EmergencySurgeryApCost;

        // Spend 1 Medical Supply (highest quality for emergency use)
        var highestSupply = _suppliesService.GetHighestQualitySupply(player);
        if (highestSupply == null)
        {
            // Defensive guard — shouldn't happen after validation
            _logger.LogWarning(
                "Emergency Surgery failed: {Player} ({PlayerId}) highest quality supply lookup " +
                "returned null after validation passed — restoring AP",
                player.Name, player.Id);
            player.CurrentAP += EmergencySurgeryApCost;
            return null;
        }

        var spentSupply = _suppliesService.SpendSupply(player, highestSupply.SupplyType);
        if (spentSupply == null)
        {
            // Defensive guard — shouldn't happen after validation
            _logger.LogWarning(
                "Emergency Surgery failed: {Player} ({PlayerId}) supply spend returned null " +
                "after validation passed — restoring AP",
                player.Name, player.Id);
            player.CurrentAP += EmergencySurgeryApCost;
            return null;
        }

        // Roll healing dice (4d6)
        var healingRoll = Roll4D6();

        // Calculate quality bonus from consumed supply
        var qualityBonus = _suppliesService.CalculateQualityBonus(spentSupply);

        // Check Steady Hands passive bonus
        var steadyHandsBonus = player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.SteadyHands)
            ? SteadyHandsHealingBonus
            : 0;

        // Calculate recovery condition bonus
        var recoveryBonus = CalculateRecoveryBonus(targetCondition);
        var bonusTriggered = recoveryBonus > 0;

        // Calculate total healing, capped at target's max HP
        var totalHealing = healingRoll + qualityBonus + steadyHandsBonus + recoveryBonus;
        var hpAfter = Math.Min(targetCurrentHp + totalHealing, targetMaxHp);

        // Get remaining supplies count for result
        var suppliesRemaining = player.MedicalSupplies?.GetTotalSupplyCount() ?? 0;

        // Build result
        var result = new EmergencySurgeryResult
        {
            TargetId = targetId,
            TargetName = targetName,
            HpBefore = targetCurrentHp,
            HealingRoll = healingRoll,
            QualityBonus = qualityBonus,
            SteadyHandsBonus = steadyHandsBonus,
            RecoveryBonus = recoveryBonus,
            HpAfter = hpAfter,
            SuppliesUsed = EmergencySurgerySupplyCost,
            SuppliesRemaining = suppliesRemaining,
            SupplyTypeUsed = spentSupply.SupplyType.ToString(),
            BonusTriggered = bonusTriggered,
            TargetCondition = bonusTriggered ? targetCondition : null
        };

        _logger.LogInformation(
            "Emergency Surgery executed: {Player} ({PlayerId}) healed {Target} ({TargetId}). " +
            "{HealingBreakdown}. HP: {HpBefore} → {HpAfter}/{MaxHp}. " +
            "Recovery condition: {Condition} (bonus: +{RecoveryBonus}). " +
            "Supply used: {SupplyType} (Quality: {Quality}). " +
            "Supplies remaining: {Remaining}. AP remaining: {RemainingAP}",
            player.Name, player.Id, targetName, targetId,
            result.GetHealingBreakdown(),
            targetCurrentHp, hpAfter, targetMaxHp,
            targetCondition, recoveryBonus,
            spentSupply.SupplyType, spentSupply.Quality,
            suppliesRemaining, player.CurrentAP);

        return result;
    }

    /// <inheritdoc />
    public AntidoteCraftResult? ExecuteAntidoteCraft(
        Player player,
        CraftingMaterial[] availableMaterials)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.AntidoteCraft))
        {
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < AntidoteCraftApCost)
        {
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, AntidoteCraftApCost, player.CurrentAP);
            return null;
        }

        // Validate Herbs supply availability
        if (!_suppliesService.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs))
        {
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) has no Herbs supplies available",
                player.Name, player.Id);
            return null;
        }

        // Get remaining supplies count for result (before crafting)
        var suppliesRemaining = player.MedicalSupplies?.GetTotalSupplyCount() ?? 0;

        // Validate crafting materials
        var materialsArray = availableMaterials ?? [];
        var plantFiber = materialsArray
            .FirstOrDefault(m => m.Type == CraftingMaterialType.PlantFiber);
        var mineralPowder = materialsArray
            .FirstOrDefault(m => m.Type == CraftingMaterialType.MineralPowder);

        if (plantFiber == null || plantFiber.Quantity < AntidoteCraftPlantFiberCost)
        {
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) has insufficient Plant Fiber " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, AntidoteCraftPlantFiberCost,
                plantFiber?.Quantity ?? 0);
            return AntidoteCraftResult.CreateFailure(
                BasicAntidoteRecipeName,
                $"Insufficient Plant Fiber (need {AntidoteCraftPlantFiberCost}, " +
                $"have {plantFiber?.Quantity ?? 0})",
                suppliesRemaining);
        }

        if (mineralPowder == null || mineralPowder.Quantity < AntidoteCraftMineralPowderCost)
        {
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) has insufficient Mineral Powder " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, AntidoteCraftMineralPowderCost,
                mineralPowder?.Quantity ?? 0);
            return AntidoteCraftResult.CreateFailure(
                BasicAntidoteRecipeName,
                $"Insufficient Mineral Powder (need {AntidoteCraftMineralPowderCost}, " +
                $"have {mineralPowder?.Quantity ?? 0})",
                suppliesRemaining);
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= AntidoteCraftApCost;

        // Spend 1 Herbs supply
        var spentHerbs = _suppliesService.SpendSupply(player, MedicalSupplyType.Herbs);
        if (spentHerbs == null)
        {
            // Defensive guard — shouldn't happen after validation
            _logger.LogWarning(
                "Antidote Craft failed: {Player} ({PlayerId}) Herbs spend returned null " +
                "after validation passed — restoring AP",
                player.Name, player.Id);
            player.CurrentAP += AntidoteCraftApCost;
            return null;
        }

        // Calculate output quality: Min(Herbs quality + material bonus, MaxCraftedQuality)
        var materialBonus = CalculateMaterialBonus(materialsArray);
        var craftedQuality = Math.Min(spentHerbs.Quality + materialBonus, MaxCraftedQuality);

        // Create the Antidote supply item
        var antidote = MedicalSupplyItem.Create(
            MedicalSupplyType.Antidote,
            $"Crafted Antidote (Q{craftedQuality})",
            "An antidote crafted from herbs and salvage materials by a skilled Bone-Setter.",
            craftedQuality,
            "craft");

        // Add Antidote to player's Medical Supplies inventory
        var addSuccess = _suppliesService.AddSupply(player, antidote);
        if (!addSuccess)
        {
            _logger.LogWarning(
                "Antidote Craft: {Player} ({PlayerId}) crafted Antidote but inventory is full — " +
                "Antidote could not be added. Herbs were already consumed.",
                player.Name, player.Id);
        }

        // Get updated supplies count for result
        suppliesRemaining = player.MedicalSupplies?.GetTotalSupplyCount() ?? 0;

        // Build materials consumed summary
        var materialsConsumed = new Dictionary<string, int>
        {
            ["Herbs"] = AntidoteCraftHerbsCost,
            ["Plant Fiber"] = AntidoteCraftPlantFiberCost,
            ["Mineral Powder"] = AntidoteCraftMineralPowderCost
        };

        // Build result
        var result = AntidoteCraftResult.CreateSuccess(
            BasicAntidoteRecipeName,
            craftedQuality,
            materialsConsumed,
            antidote,
            suppliesRemaining);

        _logger.LogInformation(
            "Antidote Craft executed: {Player} ({PlayerId}) crafted {Recipe} " +
            "(Quality: {Quality}). Material bonus: +{MaterialBonus}. " +
            "Herbs quality: {HerbsQuality}. Materials consumed: {Materials}. " +
            "Antidote added to inventory: {AddSuccess}. " +
            "Supplies remaining: {Remaining}. AP remaining: {RemainingAP}",
            player.Name, player.Id, BasicAntidoteRecipeName,
            craftedQuality, materialBonus,
            spentHerbs.Quality, result.GetMaterialSummary(),
            addSuccess, suppliesRemaining, player.CurrentAP);

        return result;
    }

    /// <inheritdoc />
    public TriageResult? EvaluateTriage(
        Player player,
        TriageTarget[] alliesInRadius,
        int baseHealing)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Triage evaluation skipped: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.Triage))
        {
            _logger.LogWarning(
                "Triage evaluation skipped: {Player} ({PlayerId}) has not unlocked the Triage passive",
                player.Name, player.Id);
            return null;
        }

        // Validate allies in radius
        if (alliesInRadius == null || alliesInRadius.Length == 0)
        {
            _logger.LogWarning(
                "Triage evaluation skipped: no allies within {Radius}-space radius " +
                "for {Player} ({PlayerId})",
                TriageRadius, player.Name, player.Id);
            return null;
        }

        // Find the most wounded ally (lowest HP percentage)
        var mostWounded = alliesInRadius
            .OrderBy(a => a.HpPercentage)
            .First();

        // Calculate bonus healing: base × multiplier, rounded down via integer cast
        var bonusHealing = (int)(baseHealing * TriageBonusMultiplier);

        // Build result
        var result = new TriageResult
        {
            MostWoundedTargetId = mostWounded.TargetId,
            MostWoundedTargetName = mostWounded.TargetName,
            MostWoundedHpPercentage = mostWounded.HpPercentage,
            TargetsInRadius = alliesInRadius.Length,
            BaseHealing = baseHealing
        };

        _logger.LogInformation(
            "Triage evaluated: {Player} ({PlayerId}) identified {Target} ({TargetId}) " +
            "as most wounded ({HpPercent:P0} HP). {BonusSummary}. " +
            "Allies in radius: {AlliesCount}",
            player.Name, player.Id,
            mostWounded.TargetName, mostWounded.TargetId,
            mostWounded.HpPercentage, result.GetBonusSummary(),
            alliesInRadius.Length);

        return result;
    }

    // ===== Tier 3 Ability Methods (v0.20.6c) =====

    /// <inheritdoc />
    public ResuscitateResult? ExecuteResuscitate(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Resuscitate failed: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.Resuscitate))
        {
            _logger.LogWarning(
                "Resuscitate failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < ResuscitateApCost)
        {
            _logger.LogWarning(
                "Resuscitate failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, ResuscitateApCost, player.CurrentAP);
            return null;
        }

        // Validate target is unconscious (0 HP)
        if (targetCurrentHp != 0)
        {
            _logger.LogWarning(
                "Resuscitate failed: target {Target} ({TargetId}) is not unconscious " +
                "(current HP: {CurrentHp}, must be 0)",
                targetName, targetId, targetCurrentHp);
            return null;
        }

        // Validate sufficient Medical Supplies (need 2, not just 1)
        var totalSupplies = player.MedicalSupplies?.GetTotalSupplyCount() ?? 0;
        if (totalSupplies < ResuscitateSupplyCost)
        {
            _logger.LogWarning(
                "Resuscitate failed: {Player} ({PlayerId}) has insufficient Medical Supplies " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, ResuscitateSupplyCost, totalSupplies);
            return null;
        }

        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= ResuscitateApCost;

        // Spend first Medical Supply (lowest quality first)
        var firstSupply = _suppliesService.SpendSupply(player);
        if (firstSupply == null)
        {
            // Defensive guard — shouldn't happen after validation
            _logger.LogWarning(
                "Resuscitate failed: {Player} ({PlayerId}) first supply spend returned null " +
                "after validation passed — restoring AP",
                player.Name, player.Id);
            player.CurrentAP += ResuscitateApCost;
            return null;
        }

        // Spend second Medical Supply (lowest quality first)
        var secondSupply = _suppliesService.SpendSupply(player);
        if (secondSupply == null)
        {
            // Defensive guard — first supply already consumed, partial spend
            _logger.LogWarning(
                "Resuscitate failed: {Player} ({PlayerId}) second supply spend returned null " +
                "after first supply was already consumed — restoring AP. " +
                "Warning: first supply ({SupplyType}, Q{Quality}) was already spent.",
                player.Name, player.Id, firstSupply.SupplyType, firstSupply.Quality);
            player.CurrentAP += ResuscitateApCost;
            return null;
        }

        // Get remaining supplies count for result
        var suppliesRemaining = player.MedicalSupplies?.GetTotalSupplyCount() ?? 0;

        // Build result — target revived to 1 HP
        var result = new ResuscitateResult
        {
            TargetId = targetId,
            TargetName = targetName,
            HpBefore = targetCurrentHp,
            SuppliesRemaining = suppliesRemaining,
            Method = ResurrectionMethod.SkillBasedResuscitation,
            ResurrectionMessage = $"{player.Name} performs emergency resuscitation on {targetName}, " +
                                  "pulling them back from the brink of death."
        };

        _logger.LogInformation(
            "Resuscitate executed: {Player} ({PlayerId}) revived {Target} ({TargetId}). " +
            "HP: {HpBefore} → {HpAfter}. Method: {Method}. " +
            "Supplies used: {SuppliesUsed} (1st: {FirstType} Q{FirstQuality}, " +
            "2nd: {SecondType} Q{SecondQuality}). " +
            "Supplies remaining: {Remaining}. AP remaining: {RemainingAP}",
            player.Name, player.Id, targetName, targetId,
            targetCurrentHp, result.HpAfter, result.Method,
            ResuscitateSupplyCost,
            firstSupply.SupplyType, firstSupply.Quality,
            secondSupply.SupplyType, secondSupply.Quality,
            suppliesRemaining, player.CurrentAP);

        return result;
    }

    /// <inheritdoc />
    public PreventiveCareAura? EvaluatePreventiveCare(
        Player player,
        Guid[] allyIdsInRadius)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Preventive Care evaluation skipped: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.PreventiveCare))
        {
            _logger.LogWarning(
                "Preventive Care evaluation skipped: {Player} ({PlayerId}) has not unlocked " +
                "the Preventive Care passive",
                player.Name, player.Id);
            return null;
        }

        // Validate allies in radius
        if (allyIdsInRadius == null || allyIdsInRadius.Length == 0)
        {
            _logger.LogWarning(
                "Preventive Care evaluation skipped: no allies within {Radius}-space radius " +
                "for {Player} ({PlayerId})",
                PreventiveCareRadius, player.Name, player.Id);
            return null;
        }

        // === No AP cost — passive ability ===
        // === No supply cost — passive ability ===
        // === No Corruption evaluation — Coherent path ===

        // Build result
        var result = new PreventiveCareAura
        {
            BoneSetterId = player.Id,
            AffectedAllyIds = allyIdsInRadius.ToList().AsReadOnly()
        };

        _logger.LogInformation(
            "Preventive Care evaluated: {Player} ({PlayerId}) aura active. " +
            "Radius: {Radius} spaces. Bonus: +{PoisonBonus} poison saves, " +
            "+{DiseaseBonus} disease saves. Allies affected: {AllyCount}. " +
            "{AuraSummary}",
            player.Name, player.Id,
            result.AuraRadius, result.PoisonSaveBonus,
            result.DiseaseSaveBonus, allyIdsInRadius.Length,
            result.GetAuraSummary());

        return result;
    }

    // ===== Capstone Ability Methods (v0.20.6c) =====

    /// <inheritdoc />
    public MiracleWorkerResult? ExecuteMiracleWorker(
        Player player,
        Guid targetId,
        string targetName,
        int targetCurrentHp,
        int targetMaxHp,
        IEnumerable<string> activeConditions)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsBoneSetter(player))
        {
            _logger.LogWarning(
                "Miracle Worker failed: {Player} ({PlayerId}) is not a Bone-Setter",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasBoneSetterAbilityUnlocked(BoneSetterAbilityId.MiracleWorker))
        {
            _logger.LogWarning(
                "Miracle Worker failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate long-rest cooldown
        if (player.HasUsedMiracleWorkerThisRestCycle)
        {
            _logger.LogWarning(
                "Miracle Worker failed: {Player} ({PlayerId}) has already used Miracle Worker " +
                "this rest cycle. Next available after long rest.",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < MiracleWorkerApCost)
        {
            _logger.LogWarning(
                "Miracle Worker failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, MiracleWorkerApCost, player.CurrentAP);
            return null;
        }

        // === No supply cost — Miracle Worker transcends material requirements ===
        // === No Corruption evaluation — Coherent path ===

        // Deduct AP
        player.CurrentAP -= MiracleWorkerApCost;

        // Set long-rest cooldown
        player.HasUsedMiracleWorkerThisRestCycle = true;

        // Materialize conditions list
        var conditionsList = activeConditions?.ToList() ?? [];

        // Build result — full HP restoration + condition clearing
        var result = new MiracleWorkerResult
        {
            TargetId = targetId,
            TargetName = targetName,
            HpBefore = targetCurrentHp,
            MaxHp = targetMaxHp,
            ClearedConditions = conditionsList.AsReadOnly(),
            MiracleMessage = $"{player.Name} performs a miraculous act of healing on {targetName}, " +
                             "restoring them to perfect health and cleansing all ailments."
        };

        _logger.LogInformation(
            "Miracle Worker executed: {Player} ({PlayerId}) performed miracle on " +
            "{Target} ({TargetId}). HP: {HpBefore} → {HpAfter}/{MaxHp} " +
            "(+{TotalHealing} healing). Conditions cleared: {ConditionsCleared} " +
            "({ClearedList}). No supply cost. Cooldown set — next available after long rest. " +
            "AP remaining: {RemainingAP}",
            player.Name, player.Id, targetName, targetId,
            targetCurrentHp, result.HpAfter, targetMaxHp,
            result.TotalHealing, result.ConditionsCleared,
            conditionsList.Count > 0 ? string.Join(", ", conditionsList) : "None",
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
                // Tier 1 abilities
                BoneSetterAbilityId.FieldDressing =>
                    player.CurrentAP >= FieldDressingApCost && hasSupplies,

                BoneSetterAbilityId.Diagnose =>
                    player.CurrentAP >= DiagnoseApCost,

                // Tier 1 passive — always ready when unlocked
                BoneSetterAbilityId.SteadyHands => true,

                // Tier 2 abilities
                BoneSetterAbilityId.EmergencySurgery =>
                    player.CurrentAP >= EmergencySurgeryApCost && hasSupplies,

                BoneSetterAbilityId.AntidoteCraft =>
                    player.CurrentAP >= AntidoteCraftApCost &&
                    _suppliesService.ValidateSupplyAvailability(player, MedicalSupplyType.Herbs),

                // Tier 2 passive — always ready when unlocked
                BoneSetterAbilityId.Triage => true,

                // Tier 3 abilities
                BoneSetterAbilityId.Resuscitate =>
                    player.CurrentAP >= ResuscitateApCost &&
                    (player.MedicalSupplies?.GetTotalSupplyCount() ?? 0) >= ResuscitateSupplyCost,

                // Tier 3 passive — always ready when unlocked
                BoneSetterAbilityId.PreventiveCare => true,

                // Capstone — requires AP and long-rest cooldown not spent
                BoneSetterAbilityId.MiracleWorker =>
                    !player.HasUsedMiracleWorkerThisRestCycle &&
                    player.CurrentAP >= MiracleWorkerApCost,

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

    /// <summary>
    /// Rolls 4d6 for Emergency Surgery base healing.
    /// </summary>
    /// <returns>Sum of four d6 rolls (range: 4–24).</returns>
    /// <remarks>
    /// Marked <c>internal virtual</c> to allow test subclasses to provide deterministic values.
    /// Double the dice pool of Field Dressing's 2d6, reflecting the higher-impact Tier 2 healing.
    /// </remarks>
    internal virtual int Roll4D6()
    {
        return Random.Shared.Next(1, 7)
             + Random.Shared.Next(1, 7)
             + Random.Shared.Next(1, 7)
             + Random.Shared.Next(1, 7);
    }

    // ===== Private Helper Methods =====

    /// <summary>
    /// Calculates the recovery condition bonus for Emergency Surgery.
    /// More critical conditions yield higher bonuses to reward triage prioritization.
    /// </summary>
    /// <param name="condition">The target's current recovery condition.</param>
    /// <returns>
    /// The recovery bonus value: 0 (Active), +1 (Incapacitated), +3 (Recovering),
    /// +4 (Dying), 0 (Dead or unknown).
    /// </returns>
    private static int CalculateRecoveryBonus(RecoveryCondition condition)
    {
        return condition switch
        {
            RecoveryCondition.Active => 0,
            RecoveryCondition.Incapacitated => RecoveryBonusIncapacitated,
            RecoveryCondition.Recovering => RecoveryBonusRecovering,
            RecoveryCondition.Dying => RecoveryBonusDying,
            RecoveryCondition.Dead => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Calculates the material quality bonus for Antidote Craft.
    /// If all crafting materials are at or above the high-quality threshold, grants +1 bonus.
    /// </summary>
    /// <param name="materials">Array of crafting materials used in the recipe.</param>
    /// <returns>+1 if all materials are Quality 3+, otherwise 0.</returns>
    private static int CalculateMaterialBonus(CraftingMaterial[] materials)
    {
        if (materials == null || materials.Length == 0)
            return 0;

        // All materials must be at or above the threshold for the bonus
        var allHighQuality = materials
            .All(m => m.Quality >= HighQualityMaterialThreshold);

        return allHighQuality ? 1 : 0;
    }

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
