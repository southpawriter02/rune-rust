using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for evaluating and applying Rust-Witch deterministic self-Corruption.
/// </summary>
/// <remarks>
/// <para>The Rust-Witch's Corruption model is <strong>deterministic</strong>, meaning every active
/// ability always inflicts a fixed amount of self-Corruption. There is no d100 roll (unlike the
/// Seiðkona) — the cost is guaranteed and represents the entropic price of weaponizing decay.</para>
///
/// <para>Self-Corruption amounts decrease by 1 at Rank 3 for Tier 1-2 abilities, but remain
/// fixed for Tier 3 and Capstone abilities. This creates a progression where mastering lower-tier
/// abilities slightly reduces their entropy cost, while high-tier abilities always carry full weight.</para>
///
/// <para>Corruption is evaluated BEFORE resource spending, consistent with the system-wide
/// pattern established by the Berserkr (v0.20.5a). The <see cref="EvaluateRisk"/> method returns
/// a result object; the actual Corruption application happens separately via <see cref="ApplyCorruption"/>.</para>
/// </remarks>
public class RustWitchCorruptionService : IRustWitchCorruptionService
{
    // Self-corruption amounts by ability (Rank 1-2 / Rank 3)
    private const int CorrosiveCurseCorruptionBase = 2;
    private const int CorrosiveCurseCorruptionRank3 = 1;
    private const int SystemShockCorruptionBase = 3;
    private const int SystemShockCorruptionRank3 = 2;
    private const int FlashRustCorruptionBase = 4;
    private const int FlashRustCorruptionRank3 = 3;
    private const int UnmakingWordCorruption = 4;       // No rank reduction
    private const int EntropicCascadeCorruption = 6;    // No rank reduction

    private readonly ILogger<RustWitchCorruptionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RustWitchCorruptionService"/> class.
    /// </summary>
    /// <param name="logger">Logger for Corruption evaluation and application events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public RustWitchCorruptionService(ILogger<RustWitchCorruptionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public RustWitchCorruptionRiskResult EvaluateRisk(RustWitchAbilityId abilityId, int rank)
    {
        var cost = GetCorruptionCost(abilityId, rank);

        // Passive abilities have no self-corruption
        if (cost == 0)
        {
            _logger.LogDebug(
                "RustWitch.Corruption: No self-Corruption for passive ability {AbilityId}",
                abilityId);

            return RustWitchCorruptionRiskResult.CreateSafe(
                $"{abilityId} is a passive ability — no self-Corruption.");
        }

        var trigger = GetTriggerForAbility(abilityId);

        _logger.LogDebug(
            "RustWitch.Corruption: Evaluated {AbilityId} at Rank {Rank} — " +
            "deterministic self-Corruption of +{Amount} (Trigger: {Trigger})",
            abilityId, rank, cost, trigger);

        return RustWitchCorruptionRiskResult.CreateTriggered(
            cost,
            trigger,
            $"{abilityId} at Rank {rank} — deterministic self-Corruption",
            rank);
    }

    /// <inheritdoc />
    public void ApplyCorruption(Guid characterId, RustWitchCorruptionRiskResult result)
    {
        if (!result.IsTriggered)
        {
            _logger.LogDebug(
                "RustWitch.Corruption: No Corruption to apply for {CharacterId} — {Reason}",
                characterId, result.Reason);
            return;
        }

        // NOTE: Actual Corruption modification on the Player entity is handled by the
        // calling service (RustWitchAbilityService), which has access to the Player object.
        // This method handles logging and event emission.
        _logger.LogInformation(
            "RustWitch.Corruption: Applied +{Amount} self-Corruption to {CharacterId} " +
            "(Trigger: {Trigger}, Rank: {Rank})",
            result.CorruptionAmount, characterId, result.Trigger, result.AbilityRank);
    }

    /// <inheritdoc />
    public string GetTriggerDescription(RustWitchCorruptionTrigger trigger)
    {
        return trigger switch
        {
            RustWitchCorruptionTrigger.CorrosiveCurseCast =>
                "Corrosive Curse — applying [Corroded] stacks corrodes the caster's essence",
            RustWitchCorruptionTrigger.SystemShockCast =>
                "System Shock — channeling entropic energy into a concentrated burst",
            RustWitchCorruptionTrigger.FlashRustCast =>
                "Flash Rust — unleashing widespread oxidation accelerates self-decay",
            RustWitchCorruptionTrigger.UnmakingWordCast =>
                "Unmaking Word — speaking words of un-creation carries profound entropic cost",
            RustWitchCorruptionTrigger.EntropicCascadeCast =>
                "Entropic Cascade — channeling the full force of entropy exacts the ultimate price",
            _ => $"Unknown Rust-Witch Corruption trigger: {trigger}"
        };
    }

    /// <inheritdoc />
    public int GetCorruptionCost(RustWitchAbilityId abilityId, int rank)
    {
        return abilityId switch
        {
            // Passive abilities — no self-corruption
            RustWitchAbilityId.PhilosopherOfDust => 0,
            RustWitchAbilityId.EntropicField => 0,
            RustWitchAbilityId.AcceleratedEntropy => 0,
            RustWitchAbilityId.CascadeReaction => 0,

            // Active abilities — deterministic self-corruption
            RustWitchAbilityId.CorrosiveCurse =>
                rank >= 3 ? CorrosiveCurseCorruptionRank3 : CorrosiveCurseCorruptionBase,
            RustWitchAbilityId.SystemShock =>
                rank >= 3 ? SystemShockCorruptionRank3 : SystemShockCorruptionBase,
            RustWitchAbilityId.FlashRust =>
                rank >= 3 ? FlashRustCorruptionRank3 : FlashRustCorruptionBase,
            RustWitchAbilityId.UnmakingWord => UnmakingWordCorruption,
            RustWitchAbilityId.EntropicCascade => EntropicCascadeCorruption,

            _ => 0
        };
    }

    /// <summary>
    /// Maps an ability ID to its corresponding Corruption trigger enum value.
    /// </summary>
    /// <param name="abilityId">The ability that was cast.</param>
    /// <returns>The trigger enum for the ability, or the first trigger as fallback.</returns>
    private static RustWitchCorruptionTrigger GetTriggerForAbility(RustWitchAbilityId abilityId)
    {
        return abilityId switch
        {
            RustWitchAbilityId.CorrosiveCurse => RustWitchCorruptionTrigger.CorrosiveCurseCast,
            RustWitchAbilityId.SystemShock => RustWitchCorruptionTrigger.SystemShockCast,
            RustWitchAbilityId.FlashRust => RustWitchCorruptionTrigger.FlashRustCast,
            RustWitchAbilityId.UnmakingWord => RustWitchCorruptionTrigger.UnmakingWordCast,
            RustWitchAbilityId.EntropicCascade => RustWitchCorruptionTrigger.EntropicCascadeCast,
            _ => RustWitchCorruptionTrigger.CorrosiveCurseCast // Fallback (shouldn't be reached)
        };
    }
}
