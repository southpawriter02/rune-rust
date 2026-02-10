// ═══════════════════════════════════════════════════════════════════════════════
// RuneChargeService.cs
// Application service implementing Rune Charge management for the Rúnasmiðr
// specialization. Provides structured logging for all resource operations.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages Rune Charge resource operations for the Rúnasmiðr specialization.
/// </summary>
/// <remarks>
/// <para>
/// This service wraps the immutable <see cref="RuneChargeResource"/> value
/// object, adding structured logging for all state transitions. Each operation
/// logs the character identity, previous/new charge counts, and operation type.
/// </para>
/// <para>
/// Logging follows the established Skjaldmær pattern:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Information:</b> Successful spend, generation, and restoration events
///   </description></item>
///   <item><description>
///     <b>Warning:</b> Rejected operations (insufficient charges, already full)
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="IRuneChargeService"/>
/// <seealso cref="RuneChargeResource"/>
public class RuneChargeService : IRuneChargeService
{
    private readonly ILogger<RuneChargeService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RuneChargeService"/>.
    /// </summary>
    /// <param name="logger">Logger for structured event output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public RuneChargeService(ILogger<RuneChargeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public (bool Success, RuneChargeResource Resource) SpendCharges(
        RuneChargeResource resource,
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
                "Rune Charge spend REJECTED for {CharacterName} ({CharacterId}): " +
                "requested {RequestedAmount} but only {CurrentCharges}/{MaxCharges} available",
                characterName,
                characterId,
                amount,
                resource.CurrentCharges,
                resource.MaxCharges);

            return (false, resource);
        }

        _logger.LogInformation(
            "Rune Charges spent by {CharacterName} ({CharacterId}): " +
            "{AmountSpent} charges consumed, {PreviousCharges} → {NewCharges}/{MaxCharges}",
            characterName,
            characterId,
            amount,
            resource.CurrentCharges,
            updated.CurrentCharges,
            updated.MaxCharges);

        return (true, updated);
    }

    /// <inheritdoc />
    public RuneChargeResource GenerateFromCraft(
        RuneChargeResource resource,
        bool isComplexCraft,
        Guid characterId,
        string characterName)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var craftType = isComplexCraft ? "complex" : "standard";

        if (resource.IsFullyCharged)
        {
            _logger.LogWarning(
                "Rune Charge generation skipped for {CharacterName} ({CharacterId}): " +
                "already at maximum capacity {CurrentCharges}/{MaxCharges} " +
                "(craft type: {CraftType})",
                characterName,
                characterId,
                resource.CurrentCharges,
                resource.MaxCharges,
                craftType);

            return resource;
        }

        var updated = resource.GenerateFromCraft(isComplexCraft);

        _logger.LogInformation(
            "Rune Charges generated for {CharacterName} ({CharacterId}): " +
            "{CraftType} craft, {PreviousCharges} → {NewCharges}/{MaxCharges}",
            characterName,
            characterId,
            craftType,
            resource.CurrentCharges,
            updated.CurrentCharges,
            updated.MaxCharges);

        return updated;
    }

    /// <inheritdoc />
    public RuneChargeResource RestoreCharges(
        RuneChargeResource resource,
        int amount,
        Guid characterId,
        string characterName)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        var updated = resource.Restore(amount);

        _logger.LogInformation(
            "Rune Charges restored for {CharacterName} ({CharacterId}): " +
            "{AmountRestored} charges restored, {PreviousCharges} → {NewCharges}/{MaxCharges}",
            characterName,
            characterId,
            amount,
            resource.CurrentCharges,
            updated.CurrentCharges,
            updated.MaxCharges);

        return updated;
    }

    /// <inheritdoc />
    public RuneChargeResource RestoreAllCharges(
        RuneChargeResource resource,
        Guid characterId,
        string characterName)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var updated = resource.RestoreAll();

        _logger.LogInformation(
            "Rune Charges fully restored for {CharacterName} ({CharacterId}): " +
            "{PreviousCharges} → {NewCharges}/{MaxCharges} (long rest)",
            characterName,
            characterId,
            resource.CurrentCharges,
            updated.CurrentCharges,
            updated.MaxCharges);

        return updated;
    }

    /// <inheritdoc />
    public bool CanSpend(RuneChargeResource resource, int amount)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        return resource.CurrentCharges >= amount;
    }
}
