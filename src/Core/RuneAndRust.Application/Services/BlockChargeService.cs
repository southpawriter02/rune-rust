// ═══════════════════════════════════════════════════════════════════════════════
// BlockChargeService.cs
// Manages Block Charge resource operations with comprehensive logging.
// Version: 0.20.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages Block Charge resource operations for Skjaldmær specialization.
/// Handles spending, restoration, and validation with comprehensive logging.
/// </summary>
public class BlockChargeService : IBlockChargeService
{
    private readonly ILogger<BlockChargeService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="BlockChargeService"/>.
    /// </summary>
    /// <param name="logger">Logger for audit trail.</param>
    public BlockChargeService(ILogger<BlockChargeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public (bool Success, BlockChargeResource Resource) SpendCharges(
        BlockChargeResource resource,
        int amount,
        Guid characterId,
        string characterName)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        var (success, updated) = resource.TrySpend(amount);

        if (!success)
        {
            _logger.LogWarning(
                "Block Charge spend rejected for character {CharacterId} " +
                "({CharacterName}): Attempted to spend {Amount} charges but " +
                "only {Current} available (max {Max})",
                characterId, characterName, amount,
                resource.CurrentCharges, resource.MaxCharges);
            return (false, resource);
        }

        _logger.LogInformation(
            "Block Charges spent: Character {CharacterId} ({CharacterName}) " +
            "spent {Amount} charges. Status: {Current}/{Max}",
            characterId, characterName, amount,
            updated.CurrentCharges, updated.MaxCharges);

        return (true, updated);
    }

    /// <inheritdoc />
    public BlockChargeResource RestoreCharges(
        BlockChargeResource resource,
        int amount,
        Guid characterId,
        string characterName)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        var previousValue = resource.CurrentCharges;
        var updated = resource.Restore(amount);

        _logger.LogInformation(
            "Block Charges restored: Character {CharacterId} ({CharacterName}) " +
            "restored {Amount} charges. Status: {Previous}/{Max} -> {Current}/{Max}",
            characterId, characterName, amount,
            previousValue, resource.MaxCharges,
            updated.CurrentCharges, updated.MaxCharges);

        return updated;
    }

    /// <inheritdoc />
    public BlockChargeResource RestoreAllCharges(
        BlockChargeResource resource,
        Guid characterId,
        string characterName)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var updated = resource.RestoreAll();

        _logger.LogInformation(
            "Block Charges fully restored: Character {CharacterId} ({CharacterName}) " +
            "restored to full ({Max} charges)",
            characterId, characterName, updated.MaxCharges);

        return updated;
    }

    /// <inheritdoc />
    public bool CanSpend(BlockChargeResource resource, int amount)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return resource.CurrentCharges >= amount;
    }

    /// <inheritdoc />
    public int CalculateBulwarkBonus(BlockChargeResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return resource.GetBulwarkHpBonus();
    }
}
