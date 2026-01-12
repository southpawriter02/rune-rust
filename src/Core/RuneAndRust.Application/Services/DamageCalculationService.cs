using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of damage calculation with resistance modifiers.
/// </summary>
public class DamageCalculationService : IDamageCalculationService
{
    private readonly ILogger<DamageCalculationService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    /// <summary>
    /// Creates a new damage calculation service.
    /// </summary>
    public DamageCalculationService(
        ILogger<DamageCalculationService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
    }

    /// <inheritdoc/>
    public DamageInstance CalculateDamage(int baseDamage, string? damageTypeId, DamageResistances targetResistances)
    {
        // Normalize damage type
        var effectiveDamageType = string.IsNullOrWhiteSpace(damageTypeId)
            ? IDamageCalculationService.DefaultDamageType
            : damageTypeId.ToLowerInvariant();

        // Ensure non-negative base damage
        baseDamage = Math.Max(0, baseDamage);

        // Get resistance for this damage type
        var resistance = targetResistances.GetResistance(effectiveDamageType);

        // Check for immunity first
        if (targetResistances.IsImmune(effectiveDamageType))
        {
            _logger.LogDebug(
                "Damage calculation: {BaseDamage} {DamageType} -> IMMUNE (100% resistance)",
                baseDamage, effectiveDamageType);
            return DamageInstance.Immune(baseDamage, effectiveDamageType);
        }

        // Calculate multiplier and final damage
        var multiplier = targetResistances.GetMultiplier(effectiveDamageType);
        var finalDamage = (int)Math.Round(baseDamage * multiplier);
        finalDamage = Math.Max(0, finalDamage); // Ensure non-negative

        // Determine resistance effect flags
        var wasResisted = resistance > 0;
        var wasVulnerable = resistance < 0;

        _logger.LogDebug(
            "Damage calculation: {BaseDamage} {DamageType} x {Multiplier:F2} = {FinalDamage} (resistance: {Resistance}%)",
            baseDamage, effectiveDamageType, multiplier, finalDamage, resistance);

        _eventLogger?.LogCombat("DamageCalculated", $"{baseDamage} {effectiveDamageType} → {finalDamage}",
            data: new Dictionary<string, object>
            {
                ["baseDamage"] = baseDamage,
                ["damageType"] = effectiveDamageType,
                ["finalDamage"] = finalDamage,
                ["resistance"] = resistance,
                ["wasResisted"] = wasResisted,
                ["wasVulnerable"] = wasVulnerable
            });

        return new DamageInstance(
            baseDamage,
            effectiveDamageType,
            finalDamage,
            resistance,
            wasResisted,
            wasVulnerable,
            false);
    }

    /// <inheritdoc/>
    public string GetResistanceDescription(string damageTypeId, DamageResistances resistances)
    {
        if (string.IsNullOrWhiteSpace(damageTypeId))
            damageTypeId = IDamageCalculationService.DefaultDamageType;

        var resistance = resistances.GetResistance(damageTypeId);
        var label = GetResistanceLabel(resistance);

        if (resistance == 0)
            return label;

        var sign = resistance > 0 ? "+" : "";
        return $"{label} ({sign}{resistance}%)";
    }

    /// <inheritdoc/>
    public string GetResistanceLabel(int resistance)
    {
        return resistance switch
        {
            >= 100 => "Immune",
            >= 75 => "Highly Resistant",
            >= 50 => "Resistant",
            >= 25 => "Slightly Resistant",
            > 0 => "Minor Resistance",
            0 => "Normal",
            > -25 => "Minor Vulnerability",
            > -50 => "Slightly Vulnerable",
            > -75 => "Vulnerable",
            > -100 => "Highly Vulnerable",
            _ => "Extremely Vulnerable"
        };
    }
}
