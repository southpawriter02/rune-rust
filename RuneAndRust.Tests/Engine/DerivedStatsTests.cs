using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;
using Xunit;
using Attribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for derived stat calculations in StatCalculationService.
/// Validates HP, Stamina, ActionPoints, and lineage/archetype bonus formulas.
/// </summary>
public class DerivedStatsTests
{
    private readonly Mock<ILogger<StatCalculationService>> _mockLogger;
    private readonly StatCalculationService _service;

    public DerivedStatsTests()
    {
        _mockLogger = new Mock<ILogger<StatCalculationService>>();
        _service = new StatCalculationService(_mockLogger.Object);
    }

    #region CalculateMaxHP Tests

    [Fact]
    public void CalculateMaxHP_WithMinimumSturdiness_Returns60()
    {
        // Formula: 50 + (Sturdiness * 10)
        // Act
        var result = _service.CalculateMaxHP(1);

        // Assert
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateMaxHP_WithDefaultSturdiness_Returns100()
    {
        // Act
        var result = _service.CalculateMaxHP(5);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculateMaxHP_WithMaximumSturdiness_Returns150()
    {
        // Act
        var result = _service.CalculateMaxHP(10);

        // Assert
        result.Should().Be(150);
    }

    [Theory]
    [InlineData(1, 60)]
    [InlineData(2, 70)]
    [InlineData(3, 80)]
    [InlineData(4, 90)]
    [InlineData(5, 100)]
    [InlineData(6, 110)]
    [InlineData(7, 120)]
    [InlineData(8, 130)]
    [InlineData(9, 140)]
    [InlineData(10, 150)]
    public void CalculateMaxHP_AllValidSturdiness_ReturnsCorrectHP(int sturdiness, int expectedHP)
    {
        // Act
        var result = _service.CalculateMaxHP(sturdiness);

        // Assert
        result.Should().Be(expectedHP);
    }

    #endregion

    #region CalculateMaxStamina Tests

    [Fact]
    public void CalculateMaxStamina_WithMinimumStats_Returns28()
    {
        // Formula: 20 + (Finesse * 5) + (Sturdiness * 3)
        // Act
        var result = _service.CalculateMaxStamina(1, 1);

        // Assert
        result.Should().Be(28); // 20 + 5 + 3
    }

    [Fact]
    public void CalculateMaxStamina_WithDefaultStats_Returns60()
    {
        // Act
        var result = _service.CalculateMaxStamina(5, 5);

        // Assert
        result.Should().Be(60); // 20 + 25 + 15
    }

    [Fact]
    public void CalculateMaxStamina_WithMaximumStats_Returns100()
    {
        // Act
        var result = _service.CalculateMaxStamina(10, 10);

        // Assert
        result.Should().Be(100); // 20 + 50 + 30
    }

    [Theory]
    [InlineData(1, 1, 28)]  // 20 + 5 + 3
    [InlineData(5, 5, 60)]  // 20 + 25 + 15
    [InlineData(10, 1, 73)] // 20 + 50 + 3
    [InlineData(1, 10, 55)] // 20 + 5 + 30
    [InlineData(8, 6, 78)]  // 20 + 40 + 18
    public void CalculateMaxStamina_VariousCombinations_ReturnsCorrectStamina(int finesse, int sturdiness, int expected)
    {
        // Act
        var result = _service.CalculateMaxStamina(finesse, sturdiness);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region CalculateActionPoints Tests

    [Fact]
    public void CalculateActionPoints_WithMinimumWits_Returns2()
    {
        // Formula: 2 + (Wits / 4)
        // Act
        var result = _service.CalculateActionPoints(1);

        // Assert
        result.Should().Be(2); // 2 + 0
    }

    [Fact]
    public void CalculateActionPoints_WithDefaultWits_Returns3()
    {
        // Act
        var result = _service.CalculateActionPoints(5);

        // Assert
        result.Should().Be(3); // 2 + 1
    }

    [Fact]
    public void CalculateActionPoints_WithMaximumWits_Returns4()
    {
        // Act
        var result = _service.CalculateActionPoints(10);

        // Assert
        result.Should().Be(4); // 2 + 2
    }

    [Theory]
    [InlineData(1, 2)]  // 2 + 0
    [InlineData(2, 2)]  // 2 + 0
    [InlineData(3, 2)]  // 2 + 0
    [InlineData(4, 3)]  // 2 + 1
    [InlineData(5, 3)]  // 2 + 1
    [InlineData(6, 3)]  // 2 + 1
    [InlineData(7, 3)]  // 2 + 1
    [InlineData(8, 4)]  // 2 + 2
    [InlineData(9, 4)]  // 2 + 2
    [InlineData(10, 4)] // 2 + 2
    public void CalculateActionPoints_AllValidWits_ReturnsCorrectAP(int wits, int expectedAP)
    {
        // Act
        var result = _service.CalculateActionPoints(wits);

        // Assert
        result.Should().Be(expectedAP);
    }

    #endregion

    #region RecalculateDerivedStats Tests

    [Fact]
    public void RecalculateDerivedStats_UpdatesAllDerivedStats()
    {
        // Arrange
        var character = new Character
        {
            Sturdiness = 7,
            Finesse = 6,
            Wits = 8
        };

        // Act
        _service.RecalculateDerivedStats(character);

        // Assert
        character.MaxHP.Should().Be(120); // 50 + 70
        character.CurrentHP.Should().Be(120);
        character.MaxStamina.Should().Be(71); // 20 + 30 + 21
        character.CurrentStamina.Should().Be(71);
        character.ActionPoints.Should().Be(4); // 2 + 2
    }

    [Fact]
    public void RecalculateDerivedStats_PreservesCurrentWhenMaxUnchanged()
    {
        // Arrange
        var character = new Character
        {
            Sturdiness = 5,
            Finesse = 5,
            Wits = 5,
            CurrentHP = 50, // Below max
            CurrentStamina = 30 // Below max
        };
        // Default MaxHP is 100, and with Sturdiness 5: 50 + 50 = 100 (unchanged)

        // Act
        _service.RecalculateDerivedStats(character);

        // Assert - Current values preserved when max doesn't change
        character.CurrentHP.Should().Be(50);
        character.CurrentStamina.Should().Be(30);
    }

    #endregion

    #region GetArchetypeBonuses Tests

    [Fact]
    public void GetArchetypeBonuses_Warrior_ReturnsSturdinessAndMight()
    {
        // Act
        var bonuses = _service.GetArchetypeBonuses(ArchetypeType.Warrior);

        // Assert
        bonuses.Should().HaveCount(2);
        bonuses[Attribute.Sturdiness].Should().Be(2);
        bonuses[Attribute.Might].Should().Be(1);
    }

    [Fact]
    public void GetArchetypeBonuses_Skirmisher_ReturnsFinesseAndWits()
    {
        // Act
        var bonuses = _service.GetArchetypeBonuses(ArchetypeType.Skirmisher);

        // Assert
        bonuses.Should().HaveCount(2);
        bonuses[Attribute.Finesse].Should().Be(2);
        bonuses[Attribute.Wits].Should().Be(1);
    }

    [Fact]
    public void GetArchetypeBonuses_Adept_ReturnsWitsAndWill()
    {
        // Act
        var bonuses = _service.GetArchetypeBonuses(ArchetypeType.Adept);

        // Assert
        bonuses.Should().HaveCount(2);
        bonuses[Attribute.Wits].Should().Be(2);
        bonuses[Attribute.Will].Should().Be(1);
    }

    [Fact]
    public void GetArchetypeBonuses_Mystic_ReturnsWillAndSturdiness()
    {
        // Act
        var bonuses = _service.GetArchetypeBonuses(ArchetypeType.Mystic);

        // Assert
        bonuses.Should().HaveCount(2);
        bonuses[Attribute.Will].Should().Be(2);
        bonuses[Attribute.Sturdiness].Should().Be(1);
    }

    #endregion

    #region GetLineageBonuses Tests

    [Fact]
    public void GetLineageBonuses_Human_ReturnsPlusOneToAll()
    {
        // Act
        var bonuses = _service.GetLineageBonuses(LineageType.Human);

        // Assert
        bonuses.Should().HaveCount(5);
        bonuses[Attribute.Sturdiness].Should().Be(1);
        bonuses[Attribute.Might].Should().Be(1);
        bonuses[Attribute.Wits].Should().Be(1);
        bonuses[Attribute.Will].Should().Be(1);
        bonuses[Attribute.Finesse].Should().Be(1);
    }

    [Fact]
    public void GetLineageBonuses_RuneMarked_ReturnsCorrectBonuses()
    {
        // Act
        var bonuses = _service.GetLineageBonuses(LineageType.RuneMarked);

        // Assert
        bonuses.Should().HaveCount(3);
        bonuses[Attribute.Wits].Should().Be(2);
        bonuses[Attribute.Will].Should().Be(2);
        bonuses[Attribute.Sturdiness].Should().Be(-1);
    }

    [Fact]
    public void GetLineageBonuses_IronBlooded_ReturnsCorrectBonuses()
    {
        // Act
        var bonuses = _service.GetLineageBonuses(LineageType.IronBlooded);

        // Assert
        bonuses.Should().HaveCount(3);
        bonuses[Attribute.Sturdiness].Should().Be(2);
        bonuses[Attribute.Might].Should().Be(2);
        bonuses[Attribute.Wits].Should().Be(-1);
    }

    [Fact]
    public void GetLineageBonuses_VargrKin_ReturnsCorrectBonuses()
    {
        // Act
        var bonuses = _service.GetLineageBonuses(LineageType.VargrKin);

        // Assert
        bonuses.Should().HaveCount(3);
        bonuses[Attribute.Finesse].Should().Be(2);
        bonuses[Attribute.Wits].Should().Be(2);
        bonuses[Attribute.Will].Should().Be(-1);
    }

    #endregion
}
