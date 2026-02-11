// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrAbilityService.cs
// Application service implementing Berserkr Tier 1 abilities:
// Fury Strike (active), Blood Scent (passive), Pain is Fuel (passive).
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Berserkr Tier 1 abilities with Rage management and
/// Corruption risk integration.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Fury Strike (Active):</strong> 2 AP, 20 Rage.
/// Weapon damage + 3d6 fury. Critical (nat 20): +1d6.
/// Corruption: +1 if used at 80+ Rage.
/// </para>
/// <para>
/// <strong>Blood Scent (Passive):</strong>
/// +10 Rage when enemy becomes bloodied (≤50% HP).
/// +1 Attack vs bloodied targets. No Corruption.
/// </para>
/// <para>
/// <strong>Pain is Fuel (Passive):</strong>
/// +5 Rage when taking damage. No Corruption.
/// </para>
/// </remarks>
/// <seealso cref="IBerserkrAbilityService"/>
/// <seealso cref="IBerserkrRageService"/>
/// <seealso cref="IBerserkrCorruptionService"/>
public class BerserkrAbilityService(
    IBerserkrRageService rageService,
    IBerserkrCorruptionService corruptionService,
    ILogger<BerserkrAbilityService> logger)
    : IBerserkrAbilityService
{
    private readonly IBerserkrRageService _rageService = rageService;
    private readonly IBerserkrCorruptionService _corruptionService = corruptionService;
    private readonly ILogger<BerserkrAbilityService> _logger = logger;

    /// <summary>
    /// Random instance for dice rolling. Accessible for testing.
    /// </summary>
    private readonly Random _random = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Active Abilities
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public FuryStrikeResult UseFuryStrike(Guid characterId, Guid targetId)
    {
        _logger.LogDebug(
            "Fury Strike initiated by {CharacterId} against {TargetId}",
            characterId, targetId);

        var rage = _rageService.GetRage(characterId);
        if (rage is null)
        {
            _logger.LogWarning(
                "Fury Strike failed: no Rage resource for character {CharacterId}",
                characterId);

            return FuryStrikeResult.Create(
                attackRoll: 0, baseDamage: 0, furyDamage: 0);
        }

        // Check if sufficient Rage
        if (rage.CurrentRage < RageResource.FuryStrikeCost)
        {
            _logger.LogDebug(
                "Fury Strike failed: insufficient Rage ({CurrentRage}/{RequiredRage}) " +
                "for character {CharacterId}",
                rage.CurrentRage, RageResource.FuryStrikeCost, characterId);

            return FuryStrikeResult.Create(
                attackRoll: 0, baseDamage: 0, furyDamage: 0);
        }

        // Evaluate Corruption risk before spending Rage
        var corruptionRisk = _corruptionService.EvaluateRisk(
            BerserkrAbilityId.FuryStrike,
            rage.CurrentRage);

        // Spend Rage
        var spent = _rageService.SpendRage(characterId, RageResource.FuryStrikeCost);
        if (!spent)
        {
            _logger.LogWarning(
                "Fury Strike failed: SpendRage returned false for character {CharacterId}",
                characterId);

            return FuryStrikeResult.Create(
                attackRoll: 0, baseDamage: 0, furyDamage: 0);
        }

        // Roll dice
        var attackRoll = RollD20();
        var wasCritical = attackRoll == 20;
        var baseDamage = RollWeaponDamage();
        var furyDamage = Roll3D6();
        var criticalBonus = wasCritical ? RollD6() : 0;

        // Apply Corruption if triggered
        if (corruptionRisk.IsTriggered)
        {
            _corruptionService.ApplyCorruption(characterId, corruptionRisk);

            _logger.LogWarning(
                "Fury Strike triggered Corruption for {CharacterId}: " +
                "+{CorruptionAmount} ({Reason})",
                characterId, corruptionRisk.CorruptionAmount, corruptionRisk.Reason);
        }

        var result = FuryStrikeResult.Create(
            attackRoll: attackRoll,
            baseDamage: baseDamage,
            furyDamage: furyDamage,
            criticalBonusDamage: criticalBonus,
            rageSpent: RageResource.FuryStrikeCost,
            wasCritical: wasCritical,
            corruptionTriggered: corruptionRisk.IsTriggered,
            corruptionReason: corruptionRisk.IsTriggered ? corruptionRisk.Reason : null);

        _logger.LogInformation(
            "Fury Strike by {CharacterId} against {TargetId}: {Result}",
            characterId, targetId, result);

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Passive Triggers
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool CheckBloodScent(
        Guid characterId,
        Guid targetId,
        int previousHp,
        int currentHp,
        int maxHp,
        string targetName)
    {
        _logger.LogDebug(
            "Blood Scent check for {CharacterId}: target {TargetName} " +
            "HP {PreviousHp} → {CurrentHp}/{MaxHp}",
            characterId, targetName, previousHp, currentHp, maxHp);

        var bloodiedState = BloodiedState.Create(targetId, targetName, currentHp, maxHp);

        if (!bloodiedState.WasJustBloodied(previousHp))
        {
            _logger.LogDebug(
                "Blood Scent not triggered: {TargetName} was not just bloodied",
                targetName);
            return false;
        }

        _rageService.AddRage(characterId, RageResource.BloodScentGain, "Blood Scent");

        _logger.LogInformation(
            "Blood Scent triggered for {CharacterId}: {TargetName} bloodied, " +
            "+{RageGained} Rage",
            characterId, targetName, RageResource.BloodScentGain);

        return true;
    }

    /// <inheritdoc />
    public int CheckPainIsFuel(Guid characterId, int damageReceived)
    {
        _logger.LogDebug(
            "Pain is Fuel check for {CharacterId}: {DamageReceived} damage received",
            characterId, damageReceived);

        if (damageReceived <= 0)
        {
            _logger.LogDebug(
                "Pain is Fuel not triggered: no damage received for {CharacterId}",
                characterId);
            return 0;
        }

        _rageService.AddRage(characterId, RageResource.PainIsFuelGain, "Pain is Fuel");

        _logger.LogInformation(
            "Pain is Fuel triggered for {CharacterId}: +{RageGained} Rage " +
            "from {DamageReceived} damage received",
            characterId, RageResource.PainIsFuelGain, damageReceived);

        return RageResource.PainIsFuelGain;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Readiness
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public bool GetAbilityReadiness(Guid characterId, BerserkrAbilityId abilityId)
    {
        // Passive abilities are always "ready"
        if (abilityId is BerserkrAbilityId.BloodScent or BerserkrAbilityId.PainIsFuel)
            return true;

        // Active abilities require Rage
        if (abilityId == BerserkrAbilityId.FuryStrike)
        {
            var rage = _rageService.GetRage(characterId);
            return rage is not null && rage.CurrentRage >= RageResource.FuryStrikeCost;
        }

        _logger.LogDebug(
            "Ability readiness check for {AbilityId}: not implemented in Tier 1",
            abilityId);

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Dice Rolling (internal for testing override)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Rolls a d20.</summary>
    internal virtual int RollD20() => _random.Next(1, 21);

    /// <summary>Rolls a d6.</summary>
    internal virtual int RollD6() => _random.Next(1, 7);

    /// <summary>Rolls 3d6 for fury damage.</summary>
    internal virtual int Roll3D6() => RollD6() + RollD6() + RollD6();

    /// <summary>Rolls weapon damage (simplified as 1d8 for base implementation).</summary>
    internal virtual int RollWeaponDamage() => _random.Next(1, 9);
}
