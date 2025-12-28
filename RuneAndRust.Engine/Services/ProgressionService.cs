using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Engine-layer service for managing character attribute progression.
/// Handles spending Progression Points (PP) to upgrade character attributes.
/// </summary>
/// <remarks>See: v0.4.0b (The Growth) for system design.</remarks>
public class ProgressionService : IProgressionService
{
    private readonly IStatCalculationService _statCalc;
    private readonly ILogger<ProgressionService> _logger;

    /// <summary>
    /// Maximum value an attribute can reach.
    /// </summary>
    private const int AttributeCap = 10;

    /// <summary>
    /// Flat cost in PP to upgrade any attribute by 1 point.
    /// </summary>
    private const int UpgradeCost = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionService"/> class.
    /// </summary>
    /// <param name="statCalc">The stat calculation service for recalculating derived stats.</param>
    /// <param name="logger">The logger instance.</param>
    public ProgressionService(
        IStatCalculationService statCalc,
        ILogger<ProgressionService> logger)
    {
        _statCalc = statCalc;
        _logger = logger;
    }

    /// <inheritdoc/>
    public AttributeUpgradeResult UpgradeAttribute(Character character, CharacterAttribute attribute)
    {
        _logger.LogTrace(
            "[Progression] UpgradeAttribute called for {Name}: {Attribute}",
            character.Name, attribute);

        // Validate: Check if at cap
        int currentValue = character.GetAttribute(attribute);
        if (currentValue >= AttributeCap)
        {
            _logger.LogWarning(
                "[Progression] Upgrade failed: {Attribute} is at cap ({Cap}) for {Name}",
                attribute, AttributeCap, character.Name);

            return AttributeUpgradeResult.Failure(
                $"{attribute} is already at maximum ({AttributeCap}).");
        }

        // Validate: Check if character has enough PP
        int cost = UpgradeCost;
        if (character.ProgressionPoints < cost)
        {
            _logger.LogWarning(
                "[Progression] Upgrade failed for {Name}: Insufficient PP ({Have}/{Need})",
                character.Name, character.ProgressionPoints, cost);

            return AttributeUpgradeResult.Failure(
                $"Insufficient Progression Points. Need {cost}, have {character.ProgressionPoints}.");
        }

        // Execute transaction
        int oldValue = currentValue;
        int newValue = currentValue + 1;

        character.ProgressionPoints -= cost;
        character.SetAttribute(attribute, newValue);

        // Recalculate derived stats (MaxHP, MaxStamina may change)
        _statCalc.RecalculateDerivedStats(character);

        _logger.LogInformation(
            "[Progression] {Name} upgraded {Attribute} from {OldVal} to {NewVal} (Cost: {Cost} PP, Remaining: {Remaining} PP)",
            character.Name, attribute, oldValue, newValue, cost, character.ProgressionPoints);

        return AttributeUpgradeResult.Ok(
            $"{attribute} upgraded to {newValue}.",
            attribute,
            oldValue,
            newValue,
            cost);
    }

    /// <inheritdoc/>
    public int GetUpgradeCost(Character character, CharacterAttribute attribute)
    {
        int currentValue = character.GetAttribute(attribute);

        // Return MaxValue if at cap (cannot upgrade)
        if (currentValue >= AttributeCap)
        {
            return int.MaxValue;
        }

        return UpgradeCost;
    }

    /// <inheritdoc/>
    public bool CanUpgrade(Character character, CharacterAttribute attribute)
    {
        int currentValue = character.GetAttribute(attribute);

        // Cannot upgrade if at cap
        if (currentValue >= AttributeCap)
        {
            return false;
        }

        // Cannot upgrade if insufficient PP
        int cost = GetUpgradeCost(character, attribute);
        return character.ProgressionPoints >= cost;
    }
}
