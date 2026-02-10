using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing the Rúnasmiðr's Rune Charge resource.
/// Provides logging-wrapped operations on <see cref="Domain.ValueObjects.RuneChargeResource"/>.
/// </summary>
/// <remarks>
/// <para>Rune Charges are a renewable resource (max 5) that fuel inscription abilities:</para>
/// <list type="bullet">
/// <item>Inscribe Rune (1 charge)</item>
/// <item>Runestone Ward (1 charge)</item>
/// <item>Empowered Inscription (2 charges, Tier 2)</item>
/// <item>Runic Trap (2 charges, Tier 2)</item>
/// </list>
/// <para>Follows the same service pattern as <see cref="BlockChargeService"/>
/// for consistency across specialization resource systems.</para>
/// </remarks>
public class RuneChargeService : IRuneChargeService
{
    private readonly ILogger<RuneChargeService> _logger;

    public RuneChargeService(ILogger<RuneChargeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool SpendCharges(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (player.RuneCharges == null)
        {
            _logger.LogWarning(
                "Rune Charge spend failed: {Player} ({PlayerId}) has no RuneChargeResource",
                player.Name, player.Id);
            return false;
        }

        var result = player.RuneCharges.Spend(amount);

        if (result)
        {
            _logger.LogInformation(
                "Rune Charges spent: {Player} ({PlayerId}) spent {Amount} charges. " +
                "Remaining: {Remaining}/{Max}",
                player.Name, player.Id, amount,
                player.RuneCharges.CurrentCharges, player.RuneCharges.MaxCharges);
        }
        else
        {
            _logger.LogDebug(
                "Rune Charge spend failed: {Player} ({PlayerId}) needs {Amount} but has {Current}",
                player.Name, player.Id, amount, player.RuneCharges.CurrentCharges);
        }

        return result;
    }

    /// <inheritdoc />
    public int GenerateFromCraft(Player player, bool isComplexCraft)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.RuneCharges == null)
        {
            _logger.LogWarning(
                "Rune Charge generation failed: {Player} ({PlayerId}) has no RuneChargeResource",
                player.Name, player.Id);
            return 0;
        }

        var generated = player.RuneCharges.GenerateFromCrafting(isComplexCraft);
        var craftType = isComplexCraft ? "complex" : "standard";

        _logger.LogInformation(
            "Rune Charges generated: {Player} ({PlayerId}) gained {Generated} charges from {CraftType} craft. " +
            "Now: {Current}/{Max}",
            player.Name, player.Id, generated, craftType,
            player.RuneCharges.CurrentCharges, player.RuneCharges.MaxCharges);

        return generated;
    }

    /// <inheritdoc />
    public void RestoreCharges(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (player.RuneCharges == null)
        {
            _logger.LogWarning(
                "Rune Charge restore failed: {Player} ({PlayerId}) has no RuneChargeResource",
                player.Name, player.Id);
            return;
        }

        player.RuneCharges.Generate(amount);

        _logger.LogInformation(
            "Rune Charges restored: {Player} ({PlayerId}) restored {Amount} charges. " +
            "Now: {Current}/{Max}",
            player.Name, player.Id, amount,
            player.RuneCharges.CurrentCharges, player.RuneCharges.MaxCharges);
    }

    /// <inheritdoc />
    public void RestoreAllCharges(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.RuneCharges == null)
        {
            _logger.LogWarning(
                "Rune Charge restore-all failed: {Player} ({PlayerId}) has no RuneChargeResource",
                player.Name, player.Id);
            return;
        }

        player.RuneCharges.RestoreAll();

        _logger.LogInformation(
            "Rune Charges fully restored: {Player} ({PlayerId}). Now: {Current}/{Max}",
            player.Name, player.Id,
            player.RuneCharges.CurrentCharges, player.RuneCharges.MaxCharges);
    }

    /// <inheritdoc />
    public bool CanSpend(Player player, int amount)
    {
        if (player?.RuneCharges == null)
            return false;

        return player.RuneCharges.CanAfford(amount);
    }

    /// <inheritdoc />
    public int GetCurrentValue(Player player)
    {
        return player?.RuneCharges?.CurrentCharges ?? 0;
    }

    /// <inheritdoc />
    public int GetMaxValue(Player player)
    {
        return player?.RuneCharges?.MaxCharges ?? 0;
    }
}
