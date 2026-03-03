using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Deterministic Corruption evaluation service for the Blót-Priest specialization.
/// </summary>
/// <remarks>
/// <para>The Blót-Priest generates more Corruption than any other specialization.
/// Self-Corruption is deterministic (fixed amounts per trigger), and several abilities
/// also transfer Corruption to allies through Blight Transference.</para>
///
/// <para>Self-Corruption by trigger:</para>
/// <list type="table">
///   <listheader><term>Trigger</term><description>Self / Transfer</description></listheader>
///   <item><term>Sacrificial Cast</term><description>+1 / 0</description></item>
///   <item><term>Blood Siphon</term><description>+1 / 0</description></item>
///   <item><term>Gift of Vitae</term><description>+1 / R1:2, R2:1, R3:1</description></item>
///   <item><term>Blood Ward</term><description>+1 / 0</description></item>
///   <item><term>Exsanguinate Tick</term><description>+1 / 0</description></item>
///   <item><term>Hemorrhaging Curse</term><description>+2 / 0</description></item>
///   <item><term>Crimson Deluge</term><description>+10 / 5 per ally</description></item>
///   <item><term>Final Anathema</term><description>+15 / 0</description></item>
/// </list>
/// </remarks>
public class BlotPriestCorruptionService : IBlotPriestCorruptionService
{
    // ===== Self-Corruption Constants =====

    /// <summary>Standard Corruption for most abilities (+1 per action).</summary>
    private const int StandardCorruption = 1;

    /// <summary>Hemorrhaging Curse Corruption (fixed +2, no rank reduction).</summary>
    private const int HemorrhagingCurseCorruption = 2;

    /// <summary>Crimson Deluge self-Corruption (+10).</summary>
    private const int CrimsonDelugeSelfCorruption = 10;

    /// <summary>Final Anathema self-Corruption (+15).</summary>
    private const int FinalAnathemaSelfCorruption = 15;

    // ===== Corruption Transfer Constants =====

    /// <summary>Gift of Vitae transfer at Rank 1 (+2 to ally).</summary>
    private const int GiftOfVitaeTransferRank1 = 2;

    /// <summary>Gift of Vitae transfer at Rank 2+ (+1 to ally).</summary>
    private const int GiftOfVitaeTransferRank2Plus = 1;

    /// <summary>Crimson Deluge transfer per ally (+5).</summary>
    private const int CrimsonDelugeTransferPerAlly = 5;

    private readonly ILogger<BlotPriestCorruptionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlotPriestCorruptionService"/> class.
    /// </summary>
    /// <param name="logger">Logger for Corruption events.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public BlotPriestCorruptionService(ILogger<BlotPriestCorruptionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public BlotPriestCorruptionRiskResult EvaluateRisk(BlotPriestAbilityId abilityId, int rank)
    {
        // Passive abilities generate no Corruption
        if (abilityId is BlotPriestAbilityId.SanguinePact
            or BlotPriestAbilityId.CrimsonVigor
            or BlotPriestAbilityId.MartyrsResolve)
        {
            _logger.LogDebug(
                "BlotPriest.EvaluateRisk: {Ability} is passive — no self-Corruption",
                abilityId);

            return BlotPriestCorruptionRiskResult.CreateSafe(
                $"{abilityId} is a passive ability — no self-Corruption risk");
        }

        var selfCorruption = GetCorruptionCost(abilityId, rank);
        var transfer = GetTransferAmount(abilityId, rank);
        var trigger = GetTriggerForAbility(abilityId);

        _logger.LogDebug(
            "BlotPriest.EvaluateRisk: {Ability} R{Rank} → self-Corruption +{Self}, transfer +{Transfer} " +
            "(Trigger: {Trigger})",
            abilityId, rank, selfCorruption, transfer, trigger);

        return BlotPriestCorruptionRiskResult.CreateTriggered(
            selfCorruption,
            transfer,
            trigger,
            $"{abilityId} at Rank {rank}: +{selfCorruption} self-Corruption" +
            (transfer > 0 ? $", +{transfer} transferred to ally" : string.Empty),
            rank);
    }

    /// <inheritdoc />
    public void ApplyCorruption(Guid characterId, BlotPriestCorruptionRiskResult result)
    {
        if (!result.IsTriggered)
        {
            _logger.LogDebug(
                "BlotPriest.ApplyCorruption: No Corruption to apply for {CharacterId}",
                characterId);
            return;
        }

        _logger.LogInformation(
            "BlotPriest.ApplyCorruption: {CharacterId} receives +{Self} self-Corruption " +
            "(Trigger: {Trigger}, Rank: {Rank})" +
            (result.CorruptionTransferred > 0
                ? $", ally receives +{result.CorruptionTransferred} via Blight Transfer"
                : string.Empty),
            characterId, result.CorruptionAmount, result.Trigger, result.AbilityRank);

        // NOTE: Actual Player.Corruption modification is delegated to the game session layer.
        // This service evaluates and logs — it does NOT directly mutate Player state.
    }

    /// <inheritdoc />
    public string GetTriggerDescription(BlotPriestCorruptionTrigger trigger)
    {
        return trigger switch
        {
            BlotPriestCorruptionTrigger.SacrificialCast =>
                "Sacrificial Casting: HP consumed as biological casting medium, contaminating the caster's essence",
            BlotPriestCorruptionTrigger.BloodSiphonCast =>
                "Blood Siphon: Consuming Blighted life force extracted from dying neural tissue",
            BlotPriestCorruptionTrigger.GiftOfVitaeCast =>
                "Gift of Vitae: Filtering corrupted life force through the caster's body to heal an ally",
            BlotPriestCorruptionTrigger.BloodWardCast =>
                "Blood Ward: Crystallizing sacrificed blood into a protective barrier",
            BlotPriestCorruptionTrigger.ExsanguinateTick =>
                "Exsanguinate: Sustained bio-Aetheric drain feeding corrupted energy back to the caster",
            BlotPriestCorruptionTrigger.HemorrhagingCurseCast =>
                "Hemorrhaging Curse: Inducing cascading cellular breakdown via Aetheric resonance frequencies",
            BlotPriestCorruptionTrigger.CrimsonDelugeCast =>
                "Crimson Deluge: Massive bio-Aetheric transduction healing all allies at catastrophic personal cost",
            BlotPriestCorruptionTrigger.FinalAnathemaCast =>
                "Final Anathema: Channeling lethal sacrifice energy to annihilate a single target",
            _ => $"Unknown Blót-Priest Corruption trigger: {trigger}"
        };
    }

    /// <inheritdoc />
    public int GetCorruptionCost(BlotPriestAbilityId abilityId, int rank)
    {
        return abilityId switch
        {
            // Passive abilities: no Corruption
            BlotPriestAbilityId.SanguinePact => 0,
            BlotPriestAbilityId.CrimsonVigor => 0,
            BlotPriestAbilityId.MartyrsResolve => 0,

            // Standard +1 abilities
            BlotPriestAbilityId.BloodSiphon => StandardCorruption,
            BlotPriestAbilityId.GiftOfVitae => StandardCorruption,
            BlotPriestAbilityId.BloodWard => StandardCorruption,

            // Exsanguinate: +1 per tick (not per cast), tracked separately
            BlotPriestAbilityId.Exsanguinate => StandardCorruption,

            // Hemorrhaging Curse: +2 (fixed, no rank reduction)
            BlotPriestAbilityId.HemorrhagingCurse => HemorrhagingCurseCorruption,

            // Heartstopper: mode-dependent (we use the higher value — actual mode
            // selection happens in the ability service, which passes the correct trigger)
            BlotPriestAbilityId.Heartstopper => CrimsonDelugeSelfCorruption,

            _ => 0
        };
    }

    /// <inheritdoc />
    public int GetTransferAmount(BlotPriestAbilityId abilityId, int rank)
    {
        return abilityId switch
        {
            // Gift of Vitae: R1 transfers 2, R2+ transfers 1
            BlotPriestAbilityId.GiftOfVitae => rank >= 2
                ? GiftOfVitaeTransferRank2Plus
                : GiftOfVitaeTransferRank1,

            // Heartstopper (Crimson Deluge mode): +5 per ally
            BlotPriestAbilityId.Heartstopper => CrimsonDelugeTransferPerAlly,

            // All other abilities: no transfer
            _ => 0
        };
    }

    /// <summary>
    /// Maps an ability ID to its corresponding Corruption trigger.
    /// </summary>
    private static BlotPriestCorruptionTrigger GetTriggerForAbility(BlotPriestAbilityId abilityId)
    {
        return abilityId switch
        {
            BlotPriestAbilityId.BloodSiphon => BlotPriestCorruptionTrigger.BloodSiphonCast,
            BlotPriestAbilityId.GiftOfVitae => BlotPriestCorruptionTrigger.GiftOfVitaeCast,
            BlotPriestAbilityId.BloodWard => BlotPriestCorruptionTrigger.BloodWardCast,
            BlotPriestAbilityId.Exsanguinate => BlotPriestCorruptionTrigger.ExsanguinateTick,
            BlotPriestAbilityId.HemorrhagingCurse => BlotPriestCorruptionTrigger.HemorrhagingCurseCast,
            BlotPriestAbilityId.Heartstopper => BlotPriestCorruptionTrigger.CrimsonDelugeCast,
            _ => BlotPriestCorruptionTrigger.SacrificialCast
        };
    }
}
