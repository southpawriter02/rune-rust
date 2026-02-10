using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Skjaldmær specialization ability operations.
/// Implements Tier 3 (Mastery) and Capstone ability logic.
/// </summary>
/// <remarks>
/// <para>Abilities managed by this service:</para>
/// <list type="bullet">
/// <item>Unbreakable (Passive): -3 damage reduction, minimum 1</item>
/// <item>Guardian's Sacrifice (Reaction): absorb 100% ally damage for 2 Block Charges</item>
/// <item>The Wall Lives (Ultimate): invulnerability (HP ≥ 1) for 3 turns, once per combat</item>
/// </list>
/// </remarks>
public class SkjaldmaerAbilityService : ISkjaldmaerAbilityService
{
    /// <summary>
    /// Flat damage reduction from the Unbreakable passive ability.
    /// </summary>
    private const int UnbreakableReduction = 3;

    /// <summary>
    /// Block Charges required for Guardian's Sacrifice.
    /// </summary>
    private const int GuardiansSacrificeChargeCost = 2;

    /// <summary>
    /// AP cost to activate The Wall Lives.
    /// </summary>
    private const int TheWallLivesApCost = 4;

    /// <summary>
    /// PP threshold for Tier 3 ability unlock.
    /// </summary>
    private const int Tier3PpRequirement = 16;

    /// <summary>
    /// PP threshold for Capstone ability unlock.
    /// </summary>
    private const int CapstonePpRequirement = 24;

    /// <summary>
    /// The specialization ID string for Skjaldmær.
    /// </summary>
    private const string SkjaldmaerSpecId = "skjaldmaer";

    private readonly IBlockChargeService _blockChargeService;
    private readonly ILogger<SkjaldmaerAbilityService> _logger;

    public SkjaldmaerAbilityService(
        IBlockChargeService blockChargeService,
        ILogger<SkjaldmaerAbilityService> logger)
    {
        _blockChargeService = blockChargeService ?? throw new ArgumentNullException(nameof(blockChargeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public int GetDamageReduction(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsSkjaldmaer(player))
            return 0;

        if (!player.HasAbilityUnlocked(SkjaldmaerAbilityId.Unbreakable))
            return 0;

        return UnbreakableReduction;
    }

    /// <inheritdoc />
    public bool TryGuardiansSacrifice(Player skjaldmaer, Player defendedAlly, int incomingDamage)
    {
        ArgumentNullException.ThrowIfNull(skjaldmaer);
        ArgumentNullException.ThrowIfNull(defendedAlly);

        // Validate prerequisites
        if (!IsSkjaldmaer(skjaldmaer))
        {
            _logger.LogWarning(
                "Guardian's Sacrifice failed: {Player} ({PlayerId}) is not a Skjaldmær",
                skjaldmaer.Name, skjaldmaer.Id);
            return false;
        }

        if (!skjaldmaer.HasAbilityUnlocked(SkjaldmaerAbilityId.GuardiansSacrifice))
        {
            _logger.LogWarning(
                "Guardian's Sacrifice failed: {Player} ({PlayerId}) has not unlocked the ability",
                skjaldmaer.Name, skjaldmaer.Id);
            return false;
        }

        // Spend charges (atomic — fails if insufficient)
        if (!_blockChargeService.SpendCharges(skjaldmaer, GuardiansSacrificeChargeCost))
        {
            _logger.LogWarning(
                "Guardian's Sacrifice failed: {Player} ({PlayerId}) has insufficient Block Charges " +
                "(need {Required}, have {Available})",
                skjaldmaer.Name, skjaldmaer.Id,
                GuardiansSacrificeChargeCost,
                _blockChargeService.GetCurrentValue(skjaldmaer));
            return false;
        }

        // Transfer damage: Skjaldmær absorbs all damage, ally takes none
        skjaldmaer.TakeDamage(incomingDamage);

        _logger.LogInformation(
            "Guardian's Sacrifice executed: {Skjaldmaer} ({SkjaldmaerId}) absorbed {Damage} damage " +
            "meant for {Ally} ({AllyId}). Charges remaining: {Charges}",
            skjaldmaer.Name, skjaldmaer.Id, incomingDamage,
            defendedAlly.Name, defendedAlly.Id,
            _blockChargeService.GetCurrentValue(skjaldmaer));

        return true;
    }

    /// <inheritdoc />
    public bool ActivateTheWallLives(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsSkjaldmaer(player))
        {
            _logger.LogWarning(
                "The Wall Lives activation failed: {Player} ({PlayerId}) is not a Skjaldmær",
                player.Name, player.Id);
            return false;
        }

        // Validate ability unlock
        if (!player.HasAbilityUnlocked(SkjaldmaerAbilityId.TheWallLives))
        {
            _logger.LogWarning(
                "The Wall Lives activation failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return false;
        }

        // Validate once-per-combat
        if (player.HasUsedCapstoneThisCombat)
        {
            _logger.LogWarning(
                "The Wall Lives activation failed: {Player} ({PlayerId}) already used capstone this combat",
                player.Name, player.Id);
            return false;
        }

        // Validate AP cost
        if (player.CurrentAP < TheWallLivesApCost)
        {
            _logger.LogWarning(
                "The Wall Lives activation failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, TheWallLivesApCost, player.CurrentAP);
            return false;
        }

        // Deduct AP
        player.CurrentAP -= TheWallLivesApCost;

        // Create and activate state
        player.TheWallLivesState = new TheWallLivesState();
        player.TheWallLivesState.Activate();

        // Mark capstone as used this combat
        player.HasUsedCapstoneThisCombat = true;

        _logger.LogInformation(
            "The Wall Lives activated: {Player} ({PlayerId}) cannot drop below 1 HP for {Duration} turns. " +
            "AP remaining: {RemainingAP}",
            player.Name, player.Id,
            TheWallLivesState.DefaultDuration,
            player.CurrentAP);

        return true;
    }

    /// <inheritdoc />
    public bool CanUseCapstone(Player player)
    {
        if (player == null)
            return false;

        if (!IsSkjaldmaer(player))
            return false;

        return !player.HasUsedCapstoneThisCombat;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        if (player == null)
            return false;

        if (!IsSkjaldmaer(player))
            return false;

        return GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        if (player == null)
            return false;

        if (!IsSkjaldmaer(player))
            return false;

        return GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.GetPPInvested();
    }

    /// <summary>
    /// Checks if a player is a Skjaldmær.
    /// </summary>
    private static bool IsSkjaldmaer(Player player)
    {
        return string.Equals(player.SpecializationId, SkjaldmaerSpecId, StringComparison.OrdinalIgnoreCase);
    }
}
