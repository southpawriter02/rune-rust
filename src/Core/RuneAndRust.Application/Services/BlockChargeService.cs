using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing the Skjaldmær's Block Charge resource.
/// Provides logging-wrapped operations on <see cref="Domain.ValueObjects.BlockChargeResource"/>.
/// </summary>
/// <remarks>
/// Block Charges are a finite resource (max 3) that fuel defensive reactions:
/// <list type="bullet">
/// <item>Intercept (1 charge)</item>
/// <item>Guardian's Sacrifice (2 charges)</item>
/// </list>
/// </remarks>
public class BlockChargeService : IBlockChargeService
{
    private readonly ILogger<BlockChargeService> _logger;

    public BlockChargeService(ILogger<BlockChargeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool SpendCharges(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (player.BlockCharges == null)
        {
            _logger.LogWarning(
                "Block Charge spend failed: {Player} ({PlayerId}) has no BlockChargeResource",
                player.Name, player.Id);
            return false;
        }

        var result = player.BlockCharges.Spend(amount);

        if (result)
        {
            _logger.LogInformation(
                "Block Charges spent: {Player} ({PlayerId}) spent {Amount} charges. " +
                "Remaining: {Remaining}/{Max}",
                player.Name, player.Id, amount,
                player.BlockCharges.CurrentCharges, player.BlockCharges.MaxCharges);
        }
        else
        {
            _logger.LogDebug(
                "Block Charge spend failed: {Player} ({PlayerId}) needs {Amount} but has {Current}",
                player.Name, player.Id, amount, player.BlockCharges.CurrentCharges);
        }

        return result;
    }

    /// <inheritdoc />
    public void RestoreCharges(Player player, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (player.BlockCharges == null)
        {
            _logger.LogWarning(
                "Block Charge restore failed: {Player} ({PlayerId}) has no BlockChargeResource",
                player.Name, player.Id);
            return;
        }

        player.BlockCharges.Restore(amount);

        _logger.LogInformation(
            "Block Charges restored: {Player} ({PlayerId}) restored {Amount} charges. " +
            "Now: {Current}/{Max}",
            player.Name, player.Id, amount,
            player.BlockCharges.CurrentCharges, player.BlockCharges.MaxCharges);
    }

    /// <inheritdoc />
    public void RestoreAllCharges(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.BlockCharges == null)
        {
            _logger.LogWarning(
                "Block Charge restore-all failed: {Player} ({PlayerId}) has no BlockChargeResource",
                player.Name, player.Id);
            return;
        }

        player.BlockCharges.RestoreAll();

        _logger.LogInformation(
            "Block Charges fully restored: {Player} ({PlayerId}). Now: {Current}/{Max}",
            player.Name, player.Id,
            player.BlockCharges.CurrentCharges, player.BlockCharges.MaxCharges);
    }

    /// <inheritdoc />
    public bool CanSpend(Player player, int amount)
    {
        if (player?.BlockCharges == null)
            return false;

        return player.BlockCharges.CurrentCharges >= amount;
    }

    /// <inheritdoc />
    public int GetCurrentValue(Player player)
    {
        return player?.BlockCharges?.CurrentCharges ?? 0;
    }

    /// <inheritdoc />
    public int GetMaxValue(Player player)
    {
        return player?.BlockCharges?.MaxCharges ?? 0;
    }
}
