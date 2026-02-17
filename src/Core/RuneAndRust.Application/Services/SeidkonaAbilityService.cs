using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Seiðkona specialization ability execution.
/// Implements Tier 1 (Foundation) ability logic including Seiðr Bolt (damage + Resonance build),
/// Wyrd Sight (detection), and Aether Attunement (passive AP regen).
/// </summary>
/// <remarks>
/// <para>Tier 1 abilities (v0.20.8a):</para>
/// <list type="bullet">
/// <item>Seiðr Bolt (Active): 2d6 Aetheric damage, +1 Resonance, +1 Accumulated Damage, costs 1 AP.
///   Subject to probability-based Corruption check at Resonance 5+ (Heretical path).</item>
/// <item>Wyrd Sight (Active): detect invisible/magic/Corruption within 10 spaces for 3 turns, costs 2 AP.
///   Does NOT build Resonance, does NOT trigger Corruption checks.</item>
/// <item>Aether Attunement (Passive): +10% AP regeneration rate. Always active, no Resonance or Corruption.</item>
/// </list>
/// <para>Critical design principle: Corruption risk is evaluated BEFORE resources are spent.
/// For Seiðr Bolt, the evaluation uses the current Resonance (before the +1 gain from the cast).
/// This ensures the player sees the risk assessment before committing to an action.</para>
/// <para>Dice roll methods are marked <c>internal virtual</c> for unit test overriding.</para>
/// </remarks>
public class SeidkonaAbilityService : ISeidkonaAbilityService
{
    // ===== Tier 1 Constants (v0.20.8a) =====

    /// <summary>
    /// AP cost for the Seiðr Bolt ability.
    /// </summary>
    private const int SeidrBoltApCost = 1;

    /// <summary>
    /// AP cost for the Wyrd Sight ability.
    /// </summary>
    private const int WyrdSightApCost = 2;

    /// <summary>
    /// Passive AP regeneration bonus from Aether Attunement (represents +10%).
    /// </summary>
    private const int AetherAttunementBonus = 10;

    /// <summary>
    /// Amount of Resonance gained per Seiðr Bolt cast.
    /// </summary>
    private const int SeidrBoltResonanceGain = 1;

    // ===== PP Requirements =====

    /// <summary>
    /// PP threshold for Tier 2 ability unlock.
    /// </summary>
    private const int Tier2PpRequirement = 8;

    /// <summary>
    /// PP threshold for Tier 3 ability unlock.
    /// </summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>
    /// PP threshold for Capstone ability unlock.
    /// </summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>
    /// The specialization ID string for Seiðkona.
    /// </summary>
    private const string SeidkonaSpecId = "seidkona";

    private readonly ISeidkonaResonanceService _resonanceService;
    private readonly ISeidkonaCorruptionService _corruptionService;
    private readonly ILogger<SeidkonaAbilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeidkonaAbilityService"/> class.
    /// </summary>
    /// <param name="resonanceService">Service for Aether Resonance resource management.</param>
    /// <param name="corruptionService">Service for Corruption risk evaluation.</param>
    /// <param name="logger">Logger for ability execution events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SeidkonaAbilityService(
        ISeidkonaResonanceService resonanceService,
        ISeidkonaCorruptionService corruptionService,
        ILogger<SeidkonaAbilityService> logger)
    {
        _resonanceService = resonanceService ?? throw new ArgumentNullException(nameof(resonanceService));
        _corruptionService = corruptionService ?? throw new ArgumentNullException(nameof(corruptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public SeidrBoltResult? ExecuteSeidrBolt(Player player, Guid targetId)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Seiðr Bolt failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.SeidrBolt))
        {
            _logger.LogWarning(
                "Seiðr Bolt failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < SeidrBoltApCost)
        {
            _logger.LogWarning(
                "Seiðr Bolt failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, SeidrBoltApCost, player.CurrentAP);
            return null;
        }

        // Capture Resonance state BEFORE any changes
        var resonance = _resonanceService.GetResonance(player);
        var resonanceBefore = resonance?.CurrentResonance ?? 0;

        // === Evaluate Corruption risk BEFORE spending resources ===
        // Uses current Resonance (before the +1 gain from this cast)
        var corruptionResult = _corruptionService.EvaluateRisk(
            SeidkonaAbilityId.SeidrBolt,
            resonanceBefore);

        // Deduct AP
        player.CurrentAP -= SeidrBoltApCost;

        // Roll damage (2d6 Aetheric)
        var damageRoll = Roll2D6();

        // Build Resonance (+1 from Seiðr Bolt)
        _resonanceService.BuildResonance(player, SeidrBoltResonanceGain, "Seiðr Bolt cast");

        // Add to Accumulated Aetheric Damage tracker
        _resonanceService.AddAccumulatedDamage(player, damageRoll);

        // Apply Corruption if triggered
        if (corruptionResult.IsTriggered)
        {
            _corruptionService.ApplyCorruption(player.Id, corruptionResult);
        }

        // Capture Resonance state AFTER changes
        var resonanceAfter = resonance?.CurrentResonance ?? 0;

        // Build result
        var result = new SeidrBoltResult
        {
            DamageRoll = damageRoll,
            TotalDamage = damageRoll,
            ResonanceBefore = resonanceBefore,
            ResonanceAfter = resonanceAfter,
            CorruptionCheckPerformed = corruptionResult.RollResult > 0 || corruptionResult.RiskPercent > 0,
            CorruptionTriggered = corruptionResult.IsTriggered,
            CorruptionReason = corruptionResult.Reason,
            CorruptionRoll = corruptionResult.RollResult,
            CorruptionRiskPercent = corruptionResult.RiskPercent
        };

        _logger.LogInformation(
            "Seiðr Bolt executed: {Player} ({PlayerId}) vs target {TargetId}. " +
            "{DamageBreakdown}. {ResonanceChange}. " +
            "Corruption: {CorruptionStatus}",
            player.Name, player.Id, targetId,
            result.GetDamageBreakdown(),
            result.GetResonanceChange(),
            corruptionResult.IsTriggered
                ? $"+{corruptionResult.CorruptionAmount} (d100: {corruptionResult.RollResult} ≤ {corruptionResult.RiskPercent}%)"
                : corruptionResult.RollResult > 0
                    ? $"safe (d100: {corruptionResult.RollResult} > {corruptionResult.RiskPercent}%)"
                    : "safe (no check needed)");

        return result;
    }

    /// <inheritdoc />
    public WyrdSightResult? ExecuteWyrdSight(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSeidkona(player))
        {
            _logger.LogWarning(
                "Wyrd Sight failed: {Player} ({PlayerId}) is not a Seiðkona",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.WyrdSight))
        {
            _logger.LogWarning(
                "Wyrd Sight failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < WyrdSightApCost)
        {
            _logger.LogWarning(
                "Wyrd Sight failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, WyrdSightApCost, player.CurrentAP);
            return null;
        }

        // Deduct AP
        player.CurrentAP -= WyrdSightApCost;

        // Create Wyrd Sight effect — NO Resonance gain, NO Corruption check
        var wyrdSight = WyrdSightResult.Create(player.Id);
        player.SetWyrdSight(wyrdSight);

        _logger.LogInformation(
            "Wyrd Sight activated: {Player} ({PlayerId}). {Description}",
            player.Name, player.Id, wyrdSight.GetDescription());

        return wyrdSight;
    }

    /// <inheritdoc />
    public int GetAetherAttunementBonus(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsSeidkona(player))
            return 0;

        if (!player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.AetherAttunement))
            return 0;

        return AetherAttunementBonus;
    }

    /// <inheritdoc />
    public Dictionary<SeidkonaAbilityId, bool> GetAbilityReadiness(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        var readiness = new Dictionary<SeidkonaAbilityId, bool>();

        if (!IsSeidkona(player))
            return readiness;

        // Tier 1 active abilities
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.SeidrBolt))
        {
            readiness[SeidkonaAbilityId.SeidrBolt] = player.CurrentAP >= SeidrBoltApCost;
        }

        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.WyrdSight))
        {
            readiness[SeidkonaAbilityId.WyrdSight] = player.CurrentAP >= WyrdSightApCost;
        }

        // Tier 1 passive — always "ready" if unlocked
        if (player.HasSeidkonaAbilityUnlocked(SeidkonaAbilityId.AetherAttunement))
        {
            readiness[SeidkonaAbilityId.AetherAttunement] = true;
        }

        return readiness;
    }

    /// <inheritdoc />
    public bool CanUnlockTier2(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return IsSeidkona(player) && GetPPInvested(player) >= Tier2PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return IsSeidkona(player) && GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return IsSeidkona(player) && GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsSeidkona(player))
            return 0;

        return player.GetSeidkonaPPInvested();
    }

    /// <summary>
    /// Rolls 2d6 for Seiðr Bolt damage.
    /// Marked <c>internal virtual</c> for unit test overriding.
    /// </summary>
    /// <returns>The sum of two d6 rolls (2–12).</returns>
    internal virtual int Roll2D6()
    {
        return Random.Shared.Next(1, 7) + Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Checks if a player is a Seiðkona.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's specialization is "seidkona".</returns>
    private static bool IsSeidkona(Player player)
    {
        return string.Equals(player.SpecializationId, SeidkonaSpecId, StringComparison.OrdinalIgnoreCase);
    }
}
