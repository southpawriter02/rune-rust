using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Factories;

/// <summary>
/// Factory for creating and configuring Character entities.
/// Handles attribute allocation, lineage/archetype bonuses, and derived stat calculation.
/// </summary>
public class CharacterFactory
{
    private readonly ILogger<CharacterFactory> _logger;
    private readonly IStatCalculationService _statService;

    /// <summary>
    /// Base attribute value for all characters before bonuses.
    /// </summary>
    public const int BaseAttributeValue = 5;

    /// <summary>
    /// Number of attribute points available for distribution during creation.
    /// </summary>
    public const int AttributePointPool = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="statService">The stat calculation service.</param>
    public CharacterFactory(
        ILogger<CharacterFactory> logger,
        IStatCalculationService statService)
    {
        _logger = logger;
        _statService = statService;
    }

    /// <summary>
    /// Creates a simple character with default attributes and the specified lineage/archetype.
    /// Useful for quick starts or testing.
    /// </summary>
    /// <param name="name">The character's name.</param>
    /// <param name="lineage">The character's lineage.</param>
    /// <param name="archetype">The character's archetype.</param>
    /// <returns>A fully configured character with all bonuses and derived stats applied.</returns>
    public Character CreateSimple(string name, LineageType lineage, ArchetypeType archetype)
    {
        _logger.LogInformation(
            "Creating simple character: {Name}, Lineage: {Lineage}, Archetype: {Archetype}",
            name, lineage, archetype);

        var character = new Character
        {
            Name = name,
            Lineage = lineage,
            Archetype = archetype,
            Sturdiness = BaseAttributeValue,
            Might = BaseAttributeValue,
            Wits = BaseAttributeValue,
            Will = BaseAttributeValue,
            Finesse = BaseAttributeValue
        };

        // Apply lineage bonuses
        ApplyBonuses(character, _statService.GetLineageBonuses(lineage));

        // Apply archetype bonuses
        ApplyBonuses(character, _statService.GetArchetypeBonuses(archetype));

        // Clamp all attributes to valid range
        ClampAllAttributes(character);

        // Calculate derived stats
        _statService.RecalculateDerivedStats(character);

        _logger.LogInformation(
            "Character {Name} created successfully with final stats - STU:{Sturdiness} MIG:{Might} WIT:{Wits} WIL:{Will} FIN:{Finesse}",
            character.Name, character.Sturdiness, character.Might, character.Wits, character.Will, character.Finesse);

        return character;
    }

    /// <summary>
    /// Creates a character with custom attribute distribution.
    /// </summary>
    /// <param name="name">The character's name.</param>
    /// <param name="lineage">The character's lineage.</param>
    /// <param name="archetype">The character's archetype.</param>
    /// <param name="attributeDistribution">Dictionary mapping attributes to their allocated points.</param>
    /// <returns>A fully configured character with all bonuses and derived stats applied.</returns>
    public Character CreateWithDistribution(
        string name,
        LineageType lineage,
        ArchetypeType archetype,
        Dictionary<CharacterAttribute, int> attributeDistribution)
    {
        _logger.LogInformation(
            "Creating character with custom distribution: {Name}, Lineage: {Lineage}, Archetype: {Archetype}",
            name, lineage, archetype);

        // Validate total points distributed
        var totalPoints = attributeDistribution.Values.Sum();
        if (totalPoints != AttributePointPool)
        {
            _logger.LogWarning(
                "Invalid point distribution: {TotalPoints} points used, expected {Expected}",
                totalPoints, AttributePointPool);
            throw new ArgumentException(
                $"Must distribute exactly {AttributePointPool} points. Got {totalPoints}.",
                nameof(attributeDistribution));
        }

        var character = new Character
        {
            Name = name,
            Lineage = lineage,
            Archetype = archetype,
            Sturdiness = BaseAttributeValue,
            Might = BaseAttributeValue,
            Wits = BaseAttributeValue,
            Will = BaseAttributeValue,
            Finesse = BaseAttributeValue
        };

        // Apply custom distribution
        ApplyBonuses(character, attributeDistribution);

        // Apply lineage bonuses
        ApplyBonuses(character, _statService.GetLineageBonuses(lineage));

        // Apply archetype bonuses
        ApplyBonuses(character, _statService.GetArchetypeBonuses(archetype));

        // Clamp all attributes to valid range
        ClampAllAttributes(character);

        // Calculate derived stats
        _statService.RecalculateDerivedStats(character);

        _logger.LogInformation(
            "Character {Name} created with custom distribution - STU:{Sturdiness} MIG:{Might} WIT:{Wits} WIL:{Will} FIN:{Finesse}",
            character.Name, character.Sturdiness, character.Might, character.Wits, character.Will, character.Finesse);

        return character;
    }

    /// <summary>
    /// Applies a set of attribute bonuses to a character.
    /// </summary>
    /// <param name="character">The character to modify.</param>
    /// <param name="bonuses">The bonuses to apply.</param>
    private void ApplyBonuses(Character character, Dictionary<CharacterAttribute, int> bonuses)
    {
        foreach (var (attribute, bonus) in bonuses)
        {
            var currentValue = character.GetAttribute(attribute);
            var newValue = currentValue + bonus;
            character.SetAttribute(attribute, newValue);

            _logger.LogTrace(
                "Applied {Bonus:+0;-0} to {Attribute}: {OldValue} -> {NewValue}",
                bonus, attribute, currentValue, newValue);
        }
    }

    /// <summary>
    /// Clamps all character attributes to the valid range [1, 10].
    /// </summary>
    /// <param name="character">The character to clamp.</param>
    private void ClampAllAttributes(Character character)
    {
        character.Sturdiness = _statService.ClampAttribute(character.Sturdiness);
        character.Might = _statService.ClampAttribute(character.Might);
        character.Wits = _statService.ClampAttribute(character.Wits);
        character.Will = _statService.ClampAttribute(character.Will);
        character.Finesse = _statService.ClampAttribute(character.Finesse);
    }
}
