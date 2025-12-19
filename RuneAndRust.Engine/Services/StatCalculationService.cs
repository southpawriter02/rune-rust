using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Provides stat calculation operations including modifier application,
/// value clamping, and derived stat calculations.
/// </summary>
public class StatCalculationService : IStatCalculationService
{
    private readonly ILogger<StatCalculationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatCalculationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public StatCalculationService(ILogger<StatCalculationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public int ApplyModifier(int baseValue, int modifier)
    {
        _logger.LogTrace("Applying modifier {Modifier} to base value {BaseValue}", modifier, baseValue);

        var result = baseValue + modifier;

        _logger.LogDebug("Modifier applied: {BaseValue} + {Modifier} = {Result}", baseValue, modifier, result);

        return result;
    }

    /// <inheritdoc/>
    public int ClampAttribute(int value, int min = 1, int max = 10)
    {
        _logger.LogTrace("Clamping value {Value} to range [{Min}, {Max}]", value, min, max);

        if (value < min)
        {
            _logger.LogWarning("Value {Value} below minimum {Min}, clamped to {Min}", value, min, min);
            return min;
        }

        if (value > max)
        {
            _logger.LogWarning("Value {Value} above maximum {Max}, clamped to {Max}", value, max, max);
            return max;
        }

        _logger.LogTrace("Value {Value} within range [{Min}, {Max}], no clamping needed", value, min, max);
        return value;
    }

    /// <inheritdoc/>
    public int CalculateMaxHP(int sturdiness)
    {
        _logger.LogTrace("Calculating MaxHP for Sturdiness {Sturdiness}", sturdiness);

        var result = 50 + (sturdiness * 10);

        _logger.LogDebug("MaxHP calculated: 50 + ({Sturdiness} * 10) = {Result}", sturdiness, result);

        return result;
    }

    /// <inheritdoc/>
    public int CalculateMaxStamina(int finesse, int sturdiness)
    {
        _logger.LogTrace("Calculating MaxStamina for Finesse {Finesse}, Sturdiness {Sturdiness}", finesse, sturdiness);

        var result = 20 + (finesse * 5) + (sturdiness * 3);

        _logger.LogDebug("MaxStamina calculated: 20 + ({Finesse} * 5) + ({Sturdiness} * 3) = {Result}", finesse, sturdiness, result);

        return result;
    }

    /// <inheritdoc/>
    public int CalculateActionPoints(int wits)
    {
        _logger.LogTrace("Calculating ActionPoints for Wits {Wits}", wits);

        var result = 2 + (wits / 4);

        _logger.LogDebug("ActionPoints calculated: 2 + ({Wits} / 4) = {Result}", wits, result);

        return result;
    }

    /// <inheritdoc/>
    public void RecalculateDerivedStats(Character character)
    {
        _logger.LogInformation("Recalculating derived stats for character {CharacterName}", character.Name);

        // Use effective attributes (base + equipment bonuses)
        var effectiveSturdiness = character.GetEffectiveAttribute(CharacterAttribute.Sturdiness);
        var effectiveFinesse = character.GetEffectiveAttribute(CharacterAttribute.Finesse);
        var effectiveWits = character.GetEffectiveAttribute(CharacterAttribute.Wits);

        _logger.LogDebug("Effective attributes - Sturdiness: {Sturdiness}, Finesse: {Finesse}, Wits: {Wits}",
            effectiveSturdiness, effectiveFinesse, effectiveWits);

        var previousMaxHP = character.MaxHP;
        var previousMaxStamina = character.MaxStamina;

        character.MaxHP = CalculateMaxHP(effectiveSturdiness);
        character.MaxStamina = CalculateMaxStamina(effectiveFinesse, effectiveSturdiness);
        character.ActionPoints = CalculateActionPoints(effectiveWits);

        // Adjust current HP/Stamina proportionally if max changed (preserve ratio)
        // Or initialize to max if this is the first calculation
        if (previousMaxHP == 0)
        {
            // First time initialization - set current to max
            character.CurrentHP = character.MaxHP;
        }
        else if (character.MaxHP != previousMaxHP)
        {
            // Preserve ratio when max changes
            var hpRatio = (double)character.CurrentHP / previousMaxHP;
            character.CurrentHP = Math.Max(1, (int)(character.MaxHP * hpRatio));
        }

        if (previousMaxStamina == 0)
        {
            // First time initialization - set current to max
            character.CurrentStamina = character.MaxStamina;
        }
        else if (character.MaxStamina != previousMaxStamina)
        {
            // Preserve ratio when max changes
            var staminaRatio = (double)character.CurrentStamina / previousMaxStamina;
            character.CurrentStamina = Math.Max(0, (int)(character.MaxStamina * staminaRatio));
        }

        _logger.LogInformation(
            "Derived stats calculated - MaxHP: {MaxHP}, MaxStamina: {MaxStamina}, ActionPoints: {ActionPoints}",
            character.MaxHP, character.MaxStamina, character.ActionPoints);
    }

    /// <inheritdoc/>
    public Dictionary<CharacterAttribute, int> GetArchetypeBonuses(ArchetypeType archetype)
    {
        _logger.LogTrace("Getting attribute bonuses for archetype {Archetype}", archetype);

        var bonuses = archetype switch
        {
            ArchetypeType.Warrior => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 2 },
                { CharacterAttribute.Might, 1 }
            },
            ArchetypeType.Skirmisher => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 2 },
                { CharacterAttribute.Wits, 1 }
            },
            ArchetypeType.Adept => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Wits, 2 },
                { CharacterAttribute.Will, 1 }
            },
            ArchetypeType.Mystic => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Will, 2 },
                { CharacterAttribute.Sturdiness, 1 }
            },
            _ => new Dictionary<CharacterAttribute, int>()
        };

        _logger.LogDebug("Archetype {Archetype} provides {BonusCount} bonuses", archetype, bonuses.Count);

        return bonuses;
    }

    /// <inheritdoc/>
    public Dictionary<CharacterAttribute, int> GetLineageBonuses(LineageType lineage)
    {
        _logger.LogTrace("Getting attribute bonuses for lineage {Lineage}", lineage);

        var bonuses = lineage switch
        {
            LineageType.Human => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 1 },
                { CharacterAttribute.Might, 1 },
                { CharacterAttribute.Wits, 1 },
                { CharacterAttribute.Will, 1 },
                { CharacterAttribute.Finesse, 1 }
            },
            LineageType.RuneMarked => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Wits, 2 },
                { CharacterAttribute.Will, 2 },
                { CharacterAttribute.Sturdiness, -1 }
            },
            LineageType.IronBlooded => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 2 },
                { CharacterAttribute.Might, 2 },
                { CharacterAttribute.Wits, -1 }
            },
            LineageType.VargrKin => new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 2 },
                { CharacterAttribute.Wits, 2 },
                { CharacterAttribute.Will, -1 }
            },
            _ => new Dictionary<CharacterAttribute, int>()
        };

        _logger.LogDebug("Lineage {Lineage} provides {BonusCount} bonuses", lineage, bonuses.Count);

        return bonuses;
    }
}
