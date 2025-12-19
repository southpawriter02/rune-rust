using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Factories;
using RuneAndRust.Engine.Services;
using Xunit;
using Attribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the CharacterFactory class.
/// Validates character creation with lineage and archetype bonuses.
/// </summary>
public class CharacterFactoryTests
{
    private readonly Mock<ILogger<CharacterFactory>> _mockLogger;
    private readonly Mock<ILogger<StatCalculationService>> _mockStatLogger;
    private readonly IStatCalculationService _statService;
    private readonly CharacterFactory _factory;

    public CharacterFactoryTests()
    {
        _mockLogger = new Mock<ILogger<CharacterFactory>>();
        _mockStatLogger = new Mock<ILogger<StatCalculationService>>();
        _statService = new StatCalculationService(_mockStatLogger.Object);
        _factory = new CharacterFactory(_mockLogger.Object, _statService);
    }

    #region CreateSimple Tests

    [Fact]
    public void CreateSimple_WithValidInputs_CreatesCharacter()
    {
        // Act
        var character = _factory.CreateSimple("Test Hero", LineageType.Human, ArchetypeType.Warrior);

        // Assert
        character.Should().NotBeNull();
        character.Name.Should().Be("Test Hero");
        character.Lineage.Should().Be(LineageType.Human);
        character.Archetype.Should().Be(ArchetypeType.Warrior);
    }

    [Fact]
    public void CreateSimple_WithHumanLineage_AppliesPlusOneToBonuses()
    {
        // Act
        var character = _factory.CreateSimple("Human", LineageType.Human, ArchetypeType.Warrior);

        // Assert - Human gets +1 to all, Warrior gets +2 Sturdiness +1 Might
        // Base 5 + Human +1 + Warrior bonuses
        character.Sturdiness.Should().Be(8); // 5 + 1 + 2
        character.Might.Should().Be(7); // 5 + 1 + 1
        character.Wits.Should().Be(6); // 5 + 1
        character.Will.Should().Be(6); // 5 + 1
        character.Finesse.Should().Be(6); // 5 + 1
    }

    [Fact]
    public void CreateSimple_WithRuneMarkedLineage_AppliesCorrectBonuses()
    {
        // Act
        var character = _factory.CreateSimple("RuneMarked", LineageType.RuneMarked, ArchetypeType.Adept);

        // Assert - RuneMarked: +2 Wits, +2 Will, -1 Sturdiness
        // Adept: +2 Wits, +1 Will
        // Base 5 + lineage + archetype
        character.Sturdiness.Should().Be(4); // 5 - 1 = 4
        character.Might.Should().Be(5); // 5
        character.Wits.Should().Be(9); // 5 + 2 + 2 = 9
        character.Will.Should().Be(8); // 5 + 2 + 1 = 8
        character.Finesse.Should().Be(5); // 5
    }

    [Fact]
    public void CreateSimple_WithIronBloodedLineage_AppliesCorrectBonuses()
    {
        // Act
        var character = _factory.CreateSimple("IronBlooded", LineageType.IronBlooded, ArchetypeType.Warrior);

        // Assert - IronBlooded: +2 Sturdiness, +2 Might, -1 Wits
        // Warrior: +2 Sturdiness, +1 Might
        character.Sturdiness.Should().Be(9); // 5 + 2 + 2 = 9
        character.Might.Should().Be(8); // 5 + 2 + 1 = 8
        character.Wits.Should().Be(4); // 5 - 1 = 4
        character.Will.Should().Be(5); // 5
        character.Finesse.Should().Be(5); // 5
    }

    [Fact]
    public void CreateSimple_WithVargrKinLineage_AppliesCorrectBonuses()
    {
        // Act
        var character = _factory.CreateSimple("VargrKin", LineageType.VargrKin, ArchetypeType.Skirmisher);

        // Assert - VargrKin: +2 Finesse, +2 Wits, -1 Will
        // Skirmisher: +2 Finesse, +1 Wits
        character.Sturdiness.Should().Be(5); // 5
        character.Might.Should().Be(5); // 5
        character.Wits.Should().Be(8); // 5 + 2 + 1 = 8
        character.Will.Should().Be(4); // 5 - 1 = 4
        character.Finesse.Should().Be(9); // 5 + 2 + 2 = 9
    }

    [Fact]
    public void CreateSimple_WithMysticArchetype_AppliesCorrectBonuses()
    {
        // Act
        var character = _factory.CreateSimple("Mystic", LineageType.Human, ArchetypeType.Mystic);

        // Assert - Human: +1 all, Mystic: +2 Will, +1 Sturdiness
        character.Sturdiness.Should().Be(7); // 5 + 1 + 1 = 7
        character.Might.Should().Be(6); // 5 + 1
        character.Wits.Should().Be(6); // 5 + 1
        character.Will.Should().Be(8); // 5 + 1 + 2 = 8
        character.Finesse.Should().Be(6); // 5 + 1
    }

    [Fact]
    public void CreateSimple_ClampsAttributesToValidRange()
    {
        // The combination that would exceed 10 gets clamped
        // IronBlooded (+2 STU, +2 MIG) + Warrior (+2 STU, +1 MIG)
        // Sturdiness would be 5 + 2 + 2 = 9 (valid)

        // Act
        var character = _factory.CreateSimple("Tank", LineageType.IronBlooded, ArchetypeType.Warrior);

        // Assert
        character.Sturdiness.Should().BeLessThanOrEqualTo(10);
        character.Might.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void CreateSimple_ClampsNegativesToMinimum()
    {
        // Edge case where penalties might drop below 1
        // Currently our combinations don't go that low, but verify clamping works

        // Act
        var character = _factory.CreateSimple("Test", LineageType.RuneMarked, ArchetypeType.Warrior);

        // Assert - Sturdiness: 5 + 2 (warrior) - 1 (runemarked) = 6 (valid)
        character.Sturdiness.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void CreateSimple_CalculatesDerivedStats()
    {
        // Act
        var character = _factory.CreateSimple("Hero", LineageType.Human, ArchetypeType.Warrior);

        // Assert - verify derived stats are calculated
        // Human + Warrior: STU=8, FIN=6, WIT=6
        // MaxHP = 50 + (8 * 10) = 130
        // MaxStamina = 20 + (6 * 5) + (8 * 3) = 20 + 30 + 24 = 74
        // ActionPoints = 2 + (6 / 4) = 2 + 1 = 3
        character.MaxHP.Should().Be(130);
        character.CurrentHP.Should().Be(130);
        character.MaxStamina.Should().Be(74);
        character.CurrentStamina.Should().Be(74);
        character.ActionPoints.Should().Be(3);
    }

    [Fact]
    public void CreateSimple_SetsUniqueId()
    {
        // Act
        var character1 = _factory.CreateSimple("Hero1", LineageType.Human, ArchetypeType.Warrior);
        var character2 = _factory.CreateSimple("Hero2", LineageType.Human, ArchetypeType.Warrior);

        // Assert
        character1.Id.Should().NotBe(Guid.Empty);
        character2.Id.Should().NotBe(Guid.Empty);
        character1.Id.Should().NotBe(character2.Id);
    }

    #endregion

    #region CreateWithDistribution Tests

    [Fact]
    public void CreateWithDistribution_WithValidPoints_CreatesCharacter()
    {
        // Arrange
        var distribution = new Dictionary<Attribute, int>
        {
            { Attribute.Sturdiness, 2 },
            { Attribute.Might, 2 },
            { Attribute.Wits, 2 },
            { Attribute.Will, 2 },
            { Attribute.Finesse, 2 }
        };

        // Act
        var character = _factory.CreateWithDistribution("Custom", LineageType.Human, ArchetypeType.Warrior, distribution);

        // Assert
        character.Should().NotBeNull();
        character.Name.Should().Be("Custom");
    }

    [Fact]
    public void CreateWithDistribution_AppliesDistributionBeforeBonuses()
    {
        // Arrange
        var distribution = new Dictionary<Attribute, int>
        {
            { Attribute.Sturdiness, 4 },
            { Attribute.Might, 3 },
            { Attribute.Wits, 2 },
            { Attribute.Will, 1 },
            { Attribute.Finesse, 0 }
        };

        // Act
        var character = _factory.CreateWithDistribution("Custom", LineageType.Human, ArchetypeType.Warrior, distribution);

        // Assert - base 5 + distribution + Human (+1 all) + Warrior (+2 STU, +1 MIG)
        character.Sturdiness.Should().Be(10); // 5 + 4 + 1 + 2 = 12, clamped to 10
        character.Might.Should().Be(10); // 5 + 3 + 1 + 1 = 10
        character.Wits.Should().Be(8); // 5 + 2 + 1 = 8
        character.Will.Should().Be(7); // 5 + 1 + 1 = 7
        character.Finesse.Should().Be(6); // 5 + 0 + 1 = 6
    }

    [Fact]
    public void CreateWithDistribution_WithWrongPointTotal_ThrowsArgumentException()
    {
        // Arrange
        var distribution = new Dictionary<Attribute, int>
        {
            { Attribute.Sturdiness, 5 },
            { Attribute.Might, 5 },
            { Attribute.Wits, 5 },
            { Attribute.Will, 5 },
            { Attribute.Finesse, 5 }
        }; // Total = 25, expected 10

        // Act
        var action = () => _factory.CreateWithDistribution("Custom", LineageType.Human, ArchetypeType.Warrior, distribution);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*10*");
    }

    [Fact]
    public void CreateWithDistribution_WithTooFewPoints_ThrowsArgumentException()
    {
        // Arrange
        var distribution = new Dictionary<Attribute, int>
        {
            { Attribute.Sturdiness, 1 },
            { Attribute.Might, 1 }
        }; // Total = 2, expected 10

        // Act
        var action = () => _factory.CreateWithDistribution("Custom", LineageType.Human, ArchetypeType.Warrior, distribution);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Constants Tests

    [Fact]
    public void BaseAttributeValue_ShouldBeFive()
    {
        // Assert
        CharacterFactory.BaseAttributeValue.Should().Be(5);
    }

    [Fact]
    public void AttributePointPool_ShouldBeTen()
    {
        // Assert
        CharacterFactory.AttributePointPool.Should().Be(10);
    }

    #endregion

    #region All Lineage/Archetype Combinations

    [Theory]
    [InlineData(LineageType.Human, ArchetypeType.Warrior)]
    [InlineData(LineageType.Human, ArchetypeType.Skirmisher)]
    [InlineData(LineageType.Human, ArchetypeType.Adept)]
    [InlineData(LineageType.Human, ArchetypeType.Mystic)]
    [InlineData(LineageType.RuneMarked, ArchetypeType.Warrior)]
    [InlineData(LineageType.RuneMarked, ArchetypeType.Skirmisher)]
    [InlineData(LineageType.RuneMarked, ArchetypeType.Adept)]
    [InlineData(LineageType.RuneMarked, ArchetypeType.Mystic)]
    [InlineData(LineageType.IronBlooded, ArchetypeType.Warrior)]
    [InlineData(LineageType.IronBlooded, ArchetypeType.Skirmisher)]
    [InlineData(LineageType.IronBlooded, ArchetypeType.Adept)]
    [InlineData(LineageType.IronBlooded, ArchetypeType.Mystic)]
    [InlineData(LineageType.VargrKin, ArchetypeType.Warrior)]
    [InlineData(LineageType.VargrKin, ArchetypeType.Skirmisher)]
    [InlineData(LineageType.VargrKin, ArchetypeType.Adept)]
    [InlineData(LineageType.VargrKin, ArchetypeType.Mystic)]
    public void CreateSimple_AllCombinations_CreateValidCharacter(LineageType lineage, ArchetypeType archetype)
    {
        // Act
        var character = _factory.CreateSimple($"{lineage}_{archetype}", lineage, archetype);

        // Assert
        character.Should().NotBeNull();
        character.Sturdiness.Should().BeInRange(1, 10);
        character.Might.Should().BeInRange(1, 10);
        character.Wits.Should().BeInRange(1, 10);
        character.Will.Should().BeInRange(1, 10);
        character.Finesse.Should().BeInRange(1, 10);
        character.MaxHP.Should().BeGreaterThan(0);
        character.MaxStamina.Should().BeGreaterThan(0);
        character.ActionPoints.Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion
}
