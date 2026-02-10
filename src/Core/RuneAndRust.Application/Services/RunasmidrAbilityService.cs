using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Rúnasmiðr specialization ability operations.
/// Implements Tier 1 ability logic for inscription, identification, and ward creation.
/// </summary>
/// <remarks>
/// <para>Abilities managed by this service:</para>
/// <list type="bullet">
/// <item>Inscribe Rune (Active): enhance weapon (+2 damage) or armor (+1 Defense) for 10 turns</item>
/// <item>Read the Marks (Passive): auto-identify Jötun technology (integration point)</item>
/// <item>Runestone Ward (Active): create ward absorbing up to 10 damage</item>
/// </list>
/// <para>Follows the same service pattern as <see cref="SkjaldmaerAbilityService"/>
/// for consistency across specialization implementations.</para>
/// </remarks>
public class RunasmidrAbilityService : IRunasmidrAbilityService
{
    /// <summary>
    /// AP cost for the Inscribe Rune ability.
    /// </summary>
    private const int InscribeRuneApCost = 3;

    /// <summary>
    /// Rune Charge cost for the Inscribe Rune ability.
    /// </summary>
    private const int InscribeRuneChargeCost = 1;

    /// <summary>
    /// AP cost for the Runestone Ward ability.
    /// </summary>
    private const int RunestoneWardApCost = 2;

    /// <summary>
    /// Rune Charge cost for the Runestone Ward ability.
    /// </summary>
    private const int RunestoneWardChargeCost = 1;

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
    /// The specialization ID string for Rúnasmiðr.
    /// </summary>
    private const string RunasmidrSpecId = "runasmidr";

    private readonly IRuneChargeService _runeChargeService;
    private readonly ILogger<RunasmidrAbilityService> _logger;

    public RunasmidrAbilityService(
        IRuneChargeService runeChargeService,
        ILogger<RunasmidrAbilityService> logger)
    {
        _runeChargeService = runeChargeService ?? throw new ArgumentNullException(nameof(runeChargeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public InscribedRune? ExecuteInscribeRune(Player player, Guid targetItemId, bool isWeapon)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsRunasmidr(player))
        {
            _logger.LogWarning(
                "Inscribe Rune failed: {Player} ({PlayerId}) is not a Rúnasmiðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasRunasmidrAbilityUnlocked(RunasmidrAbilityId.InscribeRune))
        {
            _logger.LogWarning(
                "Inscribe Rune failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < InscribeRuneApCost)
        {
            _logger.LogWarning(
                "Inscribe Rune failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, InscribeRuneApCost, player.CurrentAP);
            return null;
        }

        // Validate Rune Charge cost
        if (!_runeChargeService.CanSpend(player, InscribeRuneChargeCost))
        {
            _logger.LogWarning(
                "Inscribe Rune failed: {Player} ({PlayerId}) has insufficient Rune Charges " +
                "(need {Required}, have {Available})",
                player.Name, player.Id,
                InscribeRuneChargeCost,
                _runeChargeService.GetCurrentValue(player));
            return null;
        }

        // Deduct AP
        player.CurrentAP -= InscribeRuneApCost;

        // Spend Rune Charge (atomic)
        _runeChargeService.SpendCharges(player, InscribeRuneChargeCost);

        // Create the rune based on target type
        var rune = isWeapon
            ? InscribedRune.CreateWeaponEnhancement(targetItemId)
            : InscribedRune.CreateArmorProtection(targetItemId);

        // Remove any existing rune on the same item, then add the new one
        player.RemoveInscribedRune(targetItemId);
        player.AddInscribedRune(rune);

        var runeTypeName = isWeapon ? "Enhancement" : "Protection";
        _logger.LogInformation(
            "Inscribe Rune executed: {Player} ({PlayerId}) inscribed {RuneType} rune on item {ItemId}. " +
            "Bonus: +{Bonus}, Duration: {Duration} turns. AP remaining: {RemainingAP}, " +
            "Charges remaining: {Charges}",
            player.Name, player.Id, runeTypeName, targetItemId,
            rune.EnhancementBonus, rune.Duration, player.CurrentAP,
            _runeChargeService.GetCurrentValue(player));

        return rune;
    }

    /// <inheritdoc />
    public RunestoneWard? ExecuteRunestoneWard(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // Validate specialization
        if (!IsRunasmidr(player))
        {
            _logger.LogWarning(
                "Runestone Ward failed: {Player} ({PlayerId}) is not a Rúnasmiðr",
                player.Name, player.Id);
            return null;
        }

        // Validate ability unlock
        if (!player.HasRunasmidrAbilityUnlocked(RunasmidrAbilityId.RunestoneWard))
        {
            _logger.LogWarning(
                "Runestone Ward failed: {Player} ({PlayerId}) has not unlocked the ability",
                player.Name, player.Id);
            return null;
        }

        // Validate AP cost
        if (player.CurrentAP < RunestoneWardApCost)
        {
            _logger.LogWarning(
                "Runestone Ward failed: {Player} ({PlayerId}) has insufficient AP " +
                "(need {Required}, have {Available})",
                player.Name, player.Id, RunestoneWardApCost, player.CurrentAP);
            return null;
        }

        // Validate Rune Charge cost
        if (!_runeChargeService.CanSpend(player, RunestoneWardChargeCost))
        {
            _logger.LogWarning(
                "Runestone Ward failed: {Player} ({PlayerId}) has insufficient Rune Charges " +
                "(need {Required}, have {Available})",
                player.Name, player.Id,
                RunestoneWardChargeCost,
                _runeChargeService.GetCurrentValue(player));
            return null;
        }

        // Deduct AP
        player.CurrentAP -= RunestoneWardApCost;

        // Spend Rune Charge (atomic)
        _runeChargeService.SpendCharges(player, RunestoneWardChargeCost);

        // Create the ward (replaces any existing ward)
        var ward = RunestoneWard.Create(player.Id);
        player.SetActiveWard(ward);

        _logger.LogInformation(
            "Runestone Ward created: {Player} ({PlayerId}) created ward with {Absorption} absorption. " +
            "AP remaining: {RemainingAP}, Charges remaining: {Charges}",
            player.Name, player.Id, ward.DamageAbsorption, player.CurrentAP,
            _runeChargeService.GetCurrentValue(player));

        return ward;
    }

    /// <inheritdoc />
    public IReadOnlyList<InscribedRune> GetActiveRunes(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.ActiveRunes;
    }

    /// <inheritdoc />
    public RunestoneWard? GetActiveWard(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.ActiveWard;
    }

    /// <inheritdoc />
    public bool CanUnlockTier2(Player player)
    {
        if (player == null)
            return false;

        if (!IsRunasmidr(player))
            return false;

        return GetPPInvested(player) >= Tier2PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockTier3(Player player)
    {
        if (player == null)
            return false;

        if (!IsRunasmidr(player))
            return false;

        return GetPPInvested(player) >= Tier3PpRequirement;
    }

    /// <inheritdoc />
    public bool CanUnlockCapstone(Player player)
    {
        if (player == null)
            return false;

        if (!IsRunasmidr(player))
            return false;

        return GetPPInvested(player) >= CapstonePpRequirement;
    }

    /// <inheritdoc />
    public int GetPPInvested(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.GetRunasmidrPPInvested();
    }

    /// <summary>
    /// Checks if a player is a Rúnasmiðr.
    /// </summary>
    private static bool IsRunasmidr(Player player)
    {
        return string.Equals(player.SpecializationId, RunasmidrSpecId, StringComparison.OrdinalIgnoreCase);
    }
}
