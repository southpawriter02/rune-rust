// ═══════════════════════════════════════════════════════════════════════════════
// ShadowEssenceService.cs
// Application service for managing Shadow Essence resource operations.
// Handles spending, generation, and darkness-based generation with full
// structured logging.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages Shadow Essence resource operations for the Myrk-gengr specialization.
/// </summary>
/// <remarks>
/// <para>
/// This service wraps the immutable <see cref="ShadowEssenceResource"/> value
/// object with structured logging and validation. It does not persist state
/// directly — callers are responsible for tracking the returned resource.
/// </para>
/// </remarks>
/// <seealso cref="IShadowEssenceService"/>
/// <seealso cref="ShadowEssenceResource"/>
public class ShadowEssenceService(ILogger<ShadowEssenceService> logger) : IShadowEssenceService
{
    private readonly ILogger<ShadowEssenceService> _logger = logger;

    // ─────────────────────────────────────────────────────────────────────────
    // Spending
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public (bool Success, ShadowEssenceResource Resource) TrySpendEssence(
        ShadowEssenceResource resource,
        int amount,
        string sourceAbility)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAbility);

        _logger.LogDebug(
            "Attempting to spend {Amount} Shadow Essence for {Ability}. " +
            "Current: {CurrentEssence}/{MaxEssence}",
            amount, sourceAbility, resource.CurrentEssence, resource.MaxEssence);

        var (success, updated) = resource.TrySpend(amount);

        if (success)
        {
            _logger.LogInformation(
                "Spent {Amount} Shadow Essence for {Ability}. " +
                "Remaining: {CurrentEssence}/{MaxEssence}",
                amount, sourceAbility, updated.CurrentEssence, updated.MaxEssence);
        }
        else
        {
            _logger.LogWarning(
                "Insufficient Shadow Essence for {Ability}. " +
                "Required: {Required}, Available: {Available}",
                sourceAbility, amount, resource.CurrentEssence);
        }

        return (success, updated);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generation
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public ShadowEssenceResource GenerateEssence(
        ShadowEssenceResource resource,
        int amount,
        string source)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var updated = resource.Generate(amount);

        _logger.LogInformation(
            "Generated {Amount} Shadow Essence from {Source}. " +
            "Now: {CurrentEssence}/{MaxEssence}",
            amount, source, updated.CurrentEssence, updated.MaxEssence);

        return updated;
    }

    /// <inheritdoc />
    public ShadowEssenceResource GenerateFromDarkness(
        ShadowEssenceResource resource,
        LightLevelType lightLevel)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var generationAmount = GetGenerationAmountForLightLevel(lightLevel);

        if (generationAmount == 0)
        {
            _logger.LogDebug(
                "No Shadow Essence generation at light level {LightLevel}",
                lightLevel);
            return resource;
        }

        var updated = resource.GenerateFromDarkness(lightLevel);

        _logger.LogInformation(
            "Darkness generation: +{Amount} Shadow Essence at {LightLevel}. " +
            "Now: {CurrentEssence}/{MaxEssence} (generation count: {Count})",
            generationAmount, lightLevel,
            updated.CurrentEssence, updated.MaxEssence,
            updated.DarknessGenerationCount);

        return updated;
    }

    /// <inheritdoc />
    public int GetGenerationAmountForLightLevel(LightLevelType lightLevel) =>
        lightLevel switch
        {
            LightLevelType.Darkness => 5,
            LightLevelType.DimLight => 3,
            _ => 0
        };
}
