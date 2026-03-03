using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Blót-Priest specialization ability execution.
/// Implements Tier 1 (Foundation), Tier 2 (Discipline), Tier 3 (Mastery),
/// and Capstone (Ultimate) ability logic for the Heretical Sacrificial Healer path.
/// </summary>
/// <remarks>
/// <para>The Blót-Priest is the most Corruption-intensive specialization in the system.
/// Core mechanics include Sacrificial Casting (HP→AP), Life Siphon, Blight Transference
/// (healing allies transfers YOUR Corruption to THEM), and [Bloodied] bonuses.</para>
///
/// <para>Tier 1 abilities:</para>
/// <list type="bullet">
/// <item>Sanguine Pact (Passive): Unlocks Sacrificial Casting (HP→AP). No execute method.</item>
/// <item>Blood Siphon (Active): 3d6→5d6 damage + lifesteal (25-50%). 2 AP, +1 Corruption.</item>
/// <item>Gift of Vitae (Active): Heal ally 4d10→8d10, transfers 1-2 Corruption. 3 AP, +1 self-Corruption.</item>
/// </list>
///
/// <para>Tier 2 abilities:</para>
/// <list type="bullet">
/// <item>Blood Ward (Active): HP→Shield (2.5-3.5× value). 2 AP, +1 Corruption.</item>
/// <item>Exsanguinate (Active): 3-turn DoT + 25% lifesteal. 3 AP, +1 Corruption/tick.</item>
/// <item>Crimson Vigor (Passive): [Bloodied] bonuses to healing/siphon. No execute method.</item>
/// </list>
///
/// <para>Tier 3 abilities:</para>
/// <list type="bullet">
/// <item>Hemorrhaging Curse (Active): DoT + [Bleeding] + anti-heal. 4 AP, +2 Corruption.</item>
/// <item>Martyr's Resolve (Passive): +Soak/Resolve when [Bloodied]. No execute method.</item>
/// </list>
///
/// <para>Capstone ability:</para>
/// <list type="bullet">
/// <item>Heartstopper (Active): Crimson Deluge (AoE heal +10 self/+5 ally Corruption)
///   OR Final Anathema (execute, +15 Corruption). 5 AP, once per combat.</item>
/// </list>
///
/// <para>Critical design principle: Corruption evaluation happens BEFORE resource spending.
/// Self-Corruption is deterministic — no dice roll, fixed amounts per ability and trigger.</para>
///
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding via
/// <c>TestBlotPriestAbilityService</c>.</para>
/// </remarks>
public class BlotPriestAbilityService : IBlotPriestAbilityService
{
    // ===== AP Cost Constants =====

    /// <summary>AP cost for Blood Siphon.</summary>
    private const int BloodSiphonApCost = 2;

    /// <summary>AP cost for Gift of Vitae.</summary>
    private const int GiftOfVitaeApCost = 3;

    /// <summary>AP cost for Blood Ward.</summary>
    private const int BloodWardApCost = 2;

    /// <summary>AP cost for Exsanguinate.</summary>
    private const int ExsanguinateApCost = 3;

    /// <summary>AP cost for Hemorrhaging Curse.</summary>
    private const int HemorrhagingCurseApCost = 4;

    /// <summary>AP cost for Heartstopper.</summary>
    private const int HeartstopperApCost = 5;

    // ===== Siphon/Heal Constants =====

    /// <summary>Siphon percentage at Rank 1 (25%).</summary>
    private const int SiphonPercentRank1 = 25;

    /// <summary>Siphon percentage at Rank 2 (35%).</summary>
    private const int SiphonPercentRank2 = 35;

    /// <summary>Siphon percentage at Rank 3 (50%).</summary>
    private const int SiphonPercentRank3 = 50;

    /// <summary>Blood Ward multiplier at Rank 1 (2.5×).</summary>
    private const double BloodWardMultiplierRank1 = 2.5;

    /// <summary>Blood Ward multiplier at Rank 2 (3.0×).</summary>
    private const double BloodWardMultiplierRank2 = 3.0;

    /// <summary>Blood Ward multiplier at Rank 3 (3.5×).</summary>
    private const double BloodWardMultiplierRank3 = 3.5;

    /// <summary>Exsanguinate duration in turns.</summary>
    private const int ExsanguinateDuration = 3;

    /// <summary>Exsanguinate lifesteal percentage.</summary>
    private const int ExsanguinateLifestealPercent = 25;

    /// <summary>Hemorrhaging Curse duration in turns.</summary>
    private const int HemorrhagingCurseDuration = 4;

    /// <summary>Hemorrhaging Curse healing reduction percentage.</summary>
    private const int HemorrhagingCurseHealReduction = 50;

    /// <summary>Hemorrhaging Curse lifesteal percentage.</summary>
    private const int HemorrhagingCurseLifestealPercent = 30;

    /// <summary>Crimson Deluge Corruption per healed ally.</summary>
    private const int CrimsonDelugeCorruptionPerAlly = 5;

    /// <summary>Crimson Deluge self-Corruption.</summary>
    private const int CrimsonDelugeSelfCorruption = 10;

    /// <summary>Final Anathema self-Corruption.</summary>
    private const int FinalAnathemaSelfCorruption = 15;

    // ===== Bloodied Constants =====

    /// <summary>Crimson Vigor healing bonus at Rank 1 (+50%).</summary>
    private const int CrimsonVigorHealBonusR1 = 50;

    /// <summary>Crimson Vigor healing bonus at Rank 2 (+75%).</summary>
    private const int CrimsonVigorHealBonusR2 = 75;

    /// <summary>Crimson Vigor healing bonus at Rank 3 (+100%).</summary>
    private const int CrimsonVigorHealBonusR3 = 100;

    /// <summary>Crimson Vigor siphon bonus at Rank 1 (+25%).</summary>
    private const int CrimsonVigorSiphonBonusR1 = 25;

    /// <summary>Crimson Vigor siphon bonus at Rank 2 (+40%).</summary>
    private const int CrimsonVigorSiphonBonusR2 = 40;

    /// <summary>Crimson Vigor siphon bonus at Rank 3 (+60%).</summary>
    private const int CrimsonVigorSiphonBonusR3 = 60;

    // ===== PP Requirements =====

    /// <summary>PP threshold for Tier 2 ability unlock.</summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>PP threshold for Tier 3 ability unlock.</summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>PP threshold for Capstone ability unlock.</summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>The specialization ID string for Blót-Priest.</summary>
    private const string BlotPriestSpecId = "blot-priest";

    private readonly IBlotPriestCorruptionService _corruptionService;
    private readonly ILogger<BlotPriestAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlotPriestAbilityService"/> class.
    /// </summary>
    /// <param name="corruptionService">Service for deterministic Corruption evaluation.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public BlotPriestAbilityService(
        IBlotPriestCorruptionService corruptionService,
        ILogger<BlotPriestAbilityService> logger)
    {
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Tier 1: Sacrificial Cast (Sanguine Pact) =====

    /// <inheritdoc />
    public SacrificialCastResult? EvaluateSacrificialCast(Player player, int hpToSpend, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Sacrificial Cast")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.SanguinePact, "Sacrificial Cast")) return null;

        // Conversion ratio by rank: R1 = 2:1, R2 = 1.5:1, R3 = 1:1
        var conversionRatio = rank switch
        {
            1 => 2.0,
            2 => 1.5,
            >= 3 => 1.0,
            _ => 2.0
        };

        // Cannot spend more HP than available (must keep at least 1 HP)
        var maxHpAvailable = player.Health - 1;
        if (maxHpAvailable <= 0)
        {
            _logger.LogWarning(
                "BlotPriest.SacrificialCast: {Player} ({PlayerId}) has only {HP} HP — cannot sacrifice",
                player.Name, player.Id, player.Health);
            return null;
        }

        var actualHpSpent = Math.Min(hpToSpend, maxHpAvailable);
        var apGained = (int)Math.Floor(actualHpSpent / conversionRatio);

        if (apGained <= 0)
        {
            _logger.LogWarning(
                "BlotPriest.SacrificialCast: {Player} ({PlayerId}) would gain 0 AP from {HP} HP " +
                "(ratio {Ratio}:1)",
                player.Name, player.Id, actualHpSpent, conversionRatio);
            return null;
        }

        // Corruption: +1 per sacrificial cast
        var corruptionCost = 1;

        _logger.LogInformation(
            "BlotPriest.SacrificialCast: {Player} ({PlayerId}) sacrifices {HP} HP for {AP} AP " +
            "(ratio {Ratio}:1). Corruption +{Corruption}. Remaining HP: {Remaining}",
            player.Name, player.Id, actualHpSpent, apGained, conversionRatio,
            corruptionCost, player.Health - actualHpSpent);

        return new SacrificialCastResult
        {
            IsSuccess = true,
            Description = $"Sacrificial Cast: {actualHpSpent} HP → {apGained} AP",
            HpSpent = actualHpSpent,
            ApGained = apGained,
            ConversionRatio = conversionRatio,
            CorruptionGained = corruptionCost,
            RemainingHp = player.Health - actualHpSpent,
            Trigger = BlotPriestCorruptionTrigger.SacrificialCast,
            AbilityRank = rank
        };
    }

    // ===== Tier 1: Blood Siphon (30011) =====

    /// <inheritdoc />
    public BloodSiphonResult? ExecuteBloodSiphon(
        Player player,
        Guid targetId,
        bool isBloodied = false,
        bool hasCrimsonVigor = false,
        int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Blood Siphon")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.BloodSiphon, "Blood Siphon")) return null;
        if (!ValidateAP(player, BloodSiphonApCost, "Blood Siphon")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(BlotPriestAbilityId.BloodSiphon, rank);

        player.CurrentAP -= BloodSiphonApCost;

        // Roll damage: R1=3d6, R2=4d6, R3=5d6
        var diceCount = rank switch
        {
            1 => 3,
            2 => 4,
            >= 3 => 5,
            _ => 3
        };

        var damage = 0;
        for (var i = 0; i < diceCount; i++)
        {
            damage += RollD6();
        }

        // Siphon percentage by rank
        var siphonPercent = rank switch
        {
            1 => SiphonPercentRank1,
            2 => SiphonPercentRank2,
            >= 3 => SiphonPercentRank3,
            _ => SiphonPercentRank1
        };

        // Apply Crimson Vigor [Bloodied] bonus to siphon if applicable
        if (isBloodied && hasCrimsonVigor)
        {
            var crimsonVigorRank = GetCrimsonVigorRankFromSiphonBonus(rank);
            siphonPercent += crimsonVigorRank;
        }

        var healAmount = (int)Math.Floor(damage * siphonPercent / 100.0);

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "BlotPriest.BloodSiphon: {Player} ({PlayerId}) deals {Damage} to {Target}, " +
            "heals {Heal} HP ({Siphon}% siphon). Bloodied={Bloodied}. Corruption +{Corruption}",
            player.Name, player.Id, damage, targetId, healAmount, siphonPercent,
            isBloodied, corruptionResult.CorruptionAmount);

        return new BloodSiphonResult
        {
            IsSuccess = true,
            Description = $"Blood Siphon deals {damage} damage, heals {healAmount} HP",
            AetherSpent = BloodSiphonApCost,
            DamageDealt = damage,
            HealAmount = healAmount,
            SiphonPercent = siphonPercent,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Tier 1: Gift of Vitae (30012) =====

    /// <inheritdoc />
    public GiftOfVitaeResult? ExecuteGiftOfVitae(
        Player player,
        Guid allyId,
        bool isBloodied = false,
        bool hasCrimsonVigor = false,
        int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Gift of Vitae")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.GiftOfVitae, "Gift of Vitae")) return null;
        if (!ValidateAP(player, GiftOfVitaeApCost, "Gift of Vitae")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(BlotPriestAbilityId.GiftOfVitae, rank);

        player.CurrentAP -= GiftOfVitaeApCost;

        // Roll healing: R1=4d10, R2=6d10, R3=8d10
        var diceCount = rank switch
        {
            1 => 4,
            2 => 6,
            >= 3 => 8,
            _ => 4
        };

        var healAmount = 0;
        for (var i = 0; i < diceCount; i++)
        {
            healAmount += RollD10();
        }

        // Apply Crimson Vigor [Bloodied] bonus to healing if applicable
        if (isBloodied && hasCrimsonVigor)
        {
            var healBonus = rank switch
            {
                1 => CrimsonVigorHealBonusR1,
                2 => CrimsonVigorHealBonusR2,
                >= 3 => CrimsonVigorHealBonusR3,
                _ => CrimsonVigorHealBonusR1
            };
            healAmount = (int)Math.Floor(healAmount * (100 + healBonus) / 100.0);
        }

        // Corruption transferred to ally (Blight Transference)
        var corruptionTransferred = corruptionResult.CorruptionTransferred;

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "BlotPriest.GiftOfVitae: {Player} ({PlayerId}) heals {Ally} for {Heal} HP. " +
            "Corruption transferred: +{Transfer}. Self-Corruption: +{Self}. Bloodied={Bloodied}",
            player.Name, player.Id, allyId, healAmount, corruptionTransferred,
            corruptionResult.CorruptionAmount, isBloodied);

        return new GiftOfVitaeResult
        {
            IsSuccess = true,
            Description = $"Gift of Vitae heals for {healAmount} HP",
            AetherSpent = GiftOfVitaeApCost,
            HealAmount = healAmount,
            CorruptionTransferred = corruptionTransferred,
            AllyName = allyId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: Blood Ward (30013) =====

    /// <inheritdoc />
    public BloodWardResult? ExecuteBloodWard(Player player, Guid targetId, int hpToSacrifice, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Blood Ward")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.BloodWard, "Blood Ward")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "Blood Ward", 2)) return null;
        if (!ValidateAP(player, BloodWardApCost, "Blood Ward")) return null;

        // Must keep at least 1 HP
        var maxHpAvailable = player.Health - 1;
        if (maxHpAvailable <= 0 || hpToSacrifice <= 0)
        {
            _logger.LogWarning(
                "BlotPriest.BloodWard: {Player} ({PlayerId}) cannot sacrifice HP (current: {HP})",
                player.Name, player.Id, player.Health);
            return null;
        }

        var actualHpSacrificed = Math.Min(hpToSacrifice, maxHpAvailable);

        var corruptionResult = _corruptionService.EvaluateRisk(BlotPriestAbilityId.BloodWard, rank);

        player.CurrentAP -= BloodWardApCost;

        // Shield multiplier by rank
        var multiplier = rank switch
        {
            1 => BloodWardMultiplierRank1,
            2 => BloodWardMultiplierRank2,
            >= 3 => BloodWardMultiplierRank3,
            _ => BloodWardMultiplierRank1
        };

        var shieldValue = (int)Math.Floor(actualHpSacrificed * multiplier);

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "BlotPriest.BloodWard: {Player} ({PlayerId}) sacrifices {HP} HP → {Shield} shield " +
            "({Multiplier:F1}×) on {Target}. Self-Corruption: +{Corruption}",
            player.Name, player.Id, actualHpSacrificed, shieldValue, multiplier, targetId,
            corruptionResult.CorruptionAmount);

        return new BloodWardResult
        {
            IsSuccess = true,
            Description = $"Blood Ward: {actualHpSacrificed} HP → {shieldValue} shield",
            AetherSpent = BloodWardApCost,
            HpSacrificed = actualHpSacrificed,
            ShieldValue = shieldValue,
            Multiplier = multiplier,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Tier 2: Exsanguinate (30014) =====

    /// <inheritdoc />
    public ExsanguinateResult? ExecuteExsanguinate(Player player, Guid targetId, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Exsanguinate")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.Exsanguinate, "Exsanguinate")) return null;
        if (!ValidateTierRequirement(player, Tier2PpRequirement, "Exsanguinate", 2)) return null;
        if (!ValidateAP(player, ExsanguinateApCost, "Exsanguinate")) return null;

        // Note: Exsanguinate corruption is per-tick, not per-cast.
        // The initial cast does not generate corruption — each tick does.
        player.CurrentAP -= ExsanguinateApCost;

        // Roll initial tick damage: R1=2d6, R2=3d6, R3=4d6
        var diceCount = rank switch
        {
            1 => 2,
            2 => 3,
            >= 3 => 4,
            _ => 2
        };

        var damagePerTick = 0;
        for (var i = 0; i < diceCount; i++)
        {
            damagePerTick += RollD6();
        }

        _logger.LogInformation(
            "BlotPriest.Exsanguinate: {Player} ({PlayerId}) applies DoT to {Target}: " +
            "{Damage}/tick for {Duration} turns, {Lifesteal}% lifesteal. +{TotalCorruption} total Corruption",
            player.Name, player.Id, targetId, damagePerTick, ExsanguinateDuration,
            ExsanguinateLifestealPercent, ExsanguinateDuration);

        return new ExsanguinateResult
        {
            IsSuccess = true,
            Description = $"Exsanguinate: {damagePerTick}/tick for {ExsanguinateDuration} turns",
            AetherSpent = ExsanguinateApCost,
            DamagePerTick = damagePerTick,
            Duration = ExsanguinateDuration,
            LifestealPercent = ExsanguinateLifestealPercent,
            TargetName = targetId.ToString(),
            CorruptionGained = 0, // Corruption comes from ticks, not the initial cast
            TotalCorruptionOverDuration = ExsanguinateDuration, // +1 per tick
            Trigger = BlotPriestCorruptionTrigger.ExsanguinateTick,
            AbilityRank = rank
        };
    }

    // ===== Tier 3: Hemorrhaging Curse (30016) =====

    /// <inheritdoc />
    public HemorrhagingCurseResult? ExecuteHemorrhagingCurse(Player player, Guid targetId, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Hemorrhaging Curse")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.HemorrhagingCurse, "Hemorrhaging Curse")) return null;
        if (!ValidateTierRequirement(player, Tier3PpRequirement, "Hemorrhaging Curse", 3)) return null;
        if (!ValidateAP(player, HemorrhagingCurseApCost, "Hemorrhaging Curse")) return null;

        var corruptionResult = _corruptionService.EvaluateRisk(BlotPriestAbilityId.HemorrhagingCurse, rank);

        player.CurrentAP -= HemorrhagingCurseApCost;

        // Damage per tick: 3d8
        var damagePerTick = RollD8() + RollD8() + RollD8();

        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        _logger.LogInformation(
            "BlotPriest.HemorrhagingCurse: {Player} ({PlayerId}) curses {Target}: " +
            "{Damage}/tick for {Duration} turns, [Bleeding], -{HealReduction}% healing. Corruption +{Corruption}",
            player.Name, player.Id, targetId, damagePerTick, HemorrhagingCurseDuration,
            HemorrhagingCurseHealReduction, corruptionResult.CorruptionAmount);

        return new HemorrhagingCurseResult
        {
            IsSuccess = true,
            Description = $"Hemorrhaging Curse: {damagePerTick}/tick for {HemorrhagingCurseDuration} turns",
            AetherSpent = HemorrhagingCurseApCost,
            DamagePerTick = damagePerTick,
            Duration = HemorrhagingCurseDuration,
            HealingReductionPercent = HemorrhagingCurseHealReduction,
            LifestealPercent = HemorrhagingCurseLifestealPercent,
            BleedingApplied = true,
            TargetName = targetId.ToString(),
            CorruptionGained = corruptionResult.CorruptionAmount,
            Trigger = corruptionResult.Trigger,
            AbilityRank = rank
        };
    }

    // ===== Capstone: Heartstopper — Crimson Deluge Mode =====

    /// <inheritdoc />
    public HeartstopperResult? ExecuteCrimsonDeluge(Player player, IReadOnlyList<Guid> allyIds, int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(allyIds);

        if (!ValidateSpecialization(player, "Heartstopper: Crimson Deluge")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.Heartstopper, "Heartstopper")) return null;
        if (!ValidateTierRequirement(player, CapstonePpRequirement, "Heartstopper", 4)) return null;
        if (!ValidateAP(player, HeartstopperApCost, "Heartstopper")) return null;

        if (player.HasUsedHeartstopperThisCombat)
        {
            _logger.LogWarning(
                "BlotPriest.Heartstopper: {Player} ({PlayerId}) has already used Heartstopper this combat",
                player.Name, player.Id);
            return null;
        }

        player.CurrentAP -= HeartstopperApCost;
        player.HasUsedHeartstopperThisCombat = true;

        // Heal all allies: 8d10 each
        var healPerAlly = 0;
        for (var i = 0; i < 8; i++)
        {
            healPerAlly += RollD10();
        }

        _logger.LogInformation(
            "BlotPriest.Heartstopper.CrimsonDeluge: {Player} ({PlayerId}) heals {AllyCount} allies " +
            "for {Heal} HP each, transfers +{Transfer} Corruption each. Self-Corruption: +{Self}",
            player.Name, player.Id, allyIds.Count, healPerAlly,
            CrimsonDelugeCorruptionPerAlly, CrimsonDelugeSelfCorruption);

        return new HeartstopperResult
        {
            IsSuccess = true,
            Description = $"Crimson Deluge: heals {allyIds.Count} allies for {healPerAlly} HP each",
            AetherSpent = HeartstopperApCost,
            Mode = "CrimsonDeluge",
            IsCrimsonDeluge = true,
            IsFinalAnathema = false,
            HealPerAlly = healPerAlly,
            AlliesHealed = allyIds.Count,
            CorruptionPerAlly = CrimsonDelugeCorruptionPerAlly,
            CorruptionGained = CrimsonDelugeSelfCorruption,
            Trigger = BlotPriestCorruptionTrigger.CrimsonDelugeCast,
            AbilityRank = rank
        };
    }

    // ===== Capstone: Heartstopper — Final Anathema Mode =====

    /// <inheritdoc />
    public HeartstopperResult? ExecuteFinalAnathema(
        Player player,
        Guid targetId,
        int targetCurrentHp,
        int targetCorruption,
        int rank = 1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!ValidateSpecialization(player, "Heartstopper: Final Anathema")) return null;
        if (!ValidateAbilityUnlocked(player, BlotPriestAbilityId.Heartstopper, "Heartstopper")) return null;
        if (!ValidateTierRequirement(player, CapstonePpRequirement, "Heartstopper", 4)) return null;
        if (!ValidateAP(player, HeartstopperApCost, "Heartstopper")) return null;

        if (player.HasUsedHeartstopperThisCombat)
        {
            _logger.LogWarning(
                "BlotPriest.Heartstopper: {Player} ({PlayerId}) has already used Heartstopper this combat",
                player.Name, player.Id);
            return null;
        }

        player.CurrentAP -= HeartstopperApCost;
        player.HasUsedHeartstopperThisCombat = true;

        // Massive damage: 10d10
        var damage = 0;
        for (var i = 0; i < 10; i++)
        {
            damage += RollD10();
        }

        // Check if target killed (damage >= current HP)
        var targetKilled = damage >= targetCurrentHp;
        var corruptionAbsorbed = targetKilled ? targetCorruption : 0;

        _logger.LogInformation(
            "BlotPriest.Heartstopper.FinalAnathema: {Player} ({PlayerId}) deals {Damage} to {Target}. " +
            "Killed={Killed}, absorbed {Absorbed} Corruption. Self-Corruption: +{Self}",
            player.Name, player.Id, damage, targetId, targetKilled,
            corruptionAbsorbed, FinalAnathemaSelfCorruption);

        return new HeartstopperResult
        {
            IsSuccess = true,
            Description = targetKilled
                ? $"Final Anathema KILLS target! Absorbed {corruptionAbsorbed} Corruption"
                : $"Final Anathema deals {damage} damage",
            AetherSpent = HeartstopperApCost,
            Mode = "FinalAnathema",
            IsCrimsonDeluge = false,
            IsFinalAnathema = true,
            DamageDealt = damage,
            TargetKilled = targetKilled,
            CorruptionAbsorbed = corruptionAbsorbed,
            TargetName = targetId.ToString(),
            CorruptionGained = FinalAnathemaSelfCorruption,
            Trigger = BlotPriestCorruptionTrigger.FinalAnathemaCast,
            AbilityRank = rank
        };
    }

    // ===== Utility Methods =====

    /// <inheritdoc />
    public ExsanguinateTickResult ProcessExsanguinateTick(
        int tickDamage,
        Guid targetId,
        string targetName,
        int lifestealPercent,
        int remainingTicks)
    {
        var lifestealHeal = (int)Math.Floor(tickDamage * lifestealPercent / 100.0);

        _logger.LogDebug(
            "BlotPriest.ExsanguinateTick: {Target} takes {Damage}, heals {Heal} ({Lifesteal}%). " +
            "{Remaining} ticks remaining. +1 Corruption",
            targetName, tickDamage, lifestealHeal, lifestealPercent, remainingTicks);

        return new ExsanguinateTickResult
        {
            IsSuccess = true,
            Description = $"Exsanguinate tick: {tickDamage} damage, +{lifestealHeal} HP",
            TargetId = targetId,
            TargetName = targetName,
            DamageDealt = tickDamage,
            LifestealHeal = lifestealHeal,
            RemainingTicks = remainingTicks,
            CorruptionGained = 1 // +1 Corruption per tick
        };
    }

    /// <inheritdoc />
    public string GetAbilityReadiness(Player player, BlotPriestAbilityId abilityId)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsBlotPriest(player))
            return "Requires Blót-Priest specialization";

        if (!player.HasBlotPriestAbilityUnlocked(abilityId))
            return $"{abilityId} is not unlocked";

        if (abilityId == BlotPriestAbilityId.Heartstopper && player.HasUsedHeartstopperThisCombat)
            return "Heartstopper already used this combat";

        var apCost = GetApCostForAbility(abilityId);
        if (apCost > 0 && player.CurrentAP < apCost)
            return $"Insufficient AP ({player.CurrentAP}/{apCost})";

        return "Ready";
    }

    // ===== Validation Helpers =====

    /// <summary>
    /// Validates that the player has the Blót-Priest specialization.
    /// </summary>
    private bool ValidateSpecialization(Player player, string abilityName)
    {
        if (IsBlotPriest(player)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) is not a Blót-Priest (has: {Spec})",
            abilityName, player.Name, player.Id, player.SpecializationId ?? "none");
        return false;
    }

    /// <summary>
    /// Validates that the player has unlocked the specified ability.
    /// </summary>
    private bool ValidateAbilityUnlocked(Player player, BlotPriestAbilityId abilityId, string abilityName)
    {
        if (player.HasBlotPriestAbilityUnlocked(abilityId)) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) has not unlocked {AbilityId}",
            abilityName, player.Name, player.Id, abilityId);
        return false;
    }

    /// <summary>
    /// Validates that the player meets the PP investment requirement for a tier.
    /// </summary>
    private bool ValidateTierRequirement(Player player, int requiredPP, string abilityName, int tier)
    {
        var invested = player.GetBlotPriestPPInvested();
        if (invested >= requiredPP) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) needs {Required} PP for Tier {Tier}, has {Invested}",
            abilityName, player.Name, player.Id, requiredPP, tier, invested);
        return false;
    }

    /// <summary>
    /// Validates that the player has enough AP for the ability.
    /// </summary>
    private bool ValidateAP(Player player, int requiredAP, string abilityName)
    {
        if (player.CurrentAP >= requiredAP) return true;

        _logger.LogWarning(
            "{Ability} failed: {Player} ({PlayerId}) has insufficient AP (need {Required}, have {Available})",
            abilityName, player.Name, player.Id, requiredAP, player.CurrentAP);
        return false;
    }

    /// <summary>
    /// Checks if the player is a Blót-Priest (case-insensitive).
    /// </summary>
    private static bool IsBlotPriest(Player player)
    {
        return string.Equals(player.SpecializationId, BlotPriestSpecId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the AP cost for a given ability.
    /// </summary>
    private static int GetApCostForAbility(BlotPriestAbilityId abilityId)
    {
        return abilityId switch
        {
            BlotPriestAbilityId.BloodSiphon => BloodSiphonApCost,
            BlotPriestAbilityId.GiftOfVitae => GiftOfVitaeApCost,
            BlotPriestAbilityId.BloodWard => BloodWardApCost,
            BlotPriestAbilityId.Exsanguinate => ExsanguinateApCost,
            BlotPriestAbilityId.HemorrhagingCurse => HemorrhagingCurseApCost,
            BlotPriestAbilityId.Heartstopper => HeartstopperApCost,
            _ => 0 // Passive abilities have no AP cost
        };
    }

    /// <summary>
    /// Gets the Crimson Vigor siphon bonus for a given rank.
    /// </summary>
    private static int GetCrimsonVigorRankFromSiphonBonus(int rank)
    {
        return rank switch
        {
            1 => CrimsonVigorSiphonBonusR1,
            2 => CrimsonVigorSiphonBonusR2,
            >= 3 => CrimsonVigorSiphonBonusR3,
            _ => CrimsonVigorSiphonBonusR1
        };
    }

    // ===== Dice Roll Methods (internal virtual for test overriding) =====

    /// <summary>
    /// Rolls a single d6 (1-6). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD6()
    {
        return Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Rolls a single d8 (1-8). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD8()
    {
        return Random.Shared.Next(1, 9);
    }

    /// <summary>
    /// Rolls a single d10 (1-10). Override in test subclass for deterministic results.
    /// </summary>
    internal virtual int RollD10()
    {
        return Random.Shared.Next(1, 11);
    }
}
