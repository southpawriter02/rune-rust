using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Extensive tests for the StatCalculationService class.
/// Validates modifier application, attribute clamping, and logging behavior.
/// </summary>
public class StatCalculationServiceTests
{
    private readonly Mock<ILogger<StatCalculationService>> _mockLogger;
    private readonly StatCalculationService _sut;

    public StatCalculationServiceTests()
    {
        _mockLogger = new Mock<ILogger<StatCalculationService>>();
        _sut = new StatCalculationService(_mockLogger.Object);
    }

    #region ApplyModifier Tests

    [Fact]
    public void ApplyModifier_PositiveModifier_AddsToBase()
    {
        // Arrange
        int baseValue = 5;
        int modifier = 3;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(8);
    }

    [Fact]
    public void ApplyModifier_NegativeModifier_SubtractsFromBase()
    {
        // Arrange
        int baseValue = 5;
        int modifier = -2;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void ApplyModifier_ZeroModifier_ReturnsBase()
    {
        // Arrange
        int baseValue = 7;
        int modifier = 0;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    public void ApplyModifier_LargePositiveModifier_HandlesCorrectly()
    {
        // Arrange
        int baseValue = 5;
        int modifier = 100;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(105);
    }

    [Fact]
    public void ApplyModifier_LargeNegativeModifier_HandlesCorrectly()
    {
        // Arrange
        int baseValue = 5;
        int modifier = -100;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(-95);
    }

    [Fact]
    public void ApplyModifier_ResultsInNegative_ReturnsNegativeValue()
    {
        // Arrange
        int baseValue = 3;
        int modifier = -5;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(-2);
    }

    [Fact]
    public void ApplyModifier_ZeroBase_AppliesModifierCorrectly()
    {
        // Arrange
        int baseValue = 0;
        int modifier = 5;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void ApplyModifier_NegativeBase_AppliesModifierCorrectly()
    {
        // Arrange
        int baseValue = -3;
        int modifier = 5;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region ClampAttribute Tests

    [Fact]
    public void ClampAttribute_ValueBelowMin_ReturnsMin()
    {
        // Arrange
        int value = 0;

        // Act
        var result = _sut.ClampAttribute(value);

        // Assert
        result.Should().Be(1, "default min is 1");
    }

    [Fact]
    public void ClampAttribute_ValueAboveMax_ReturnsMax()
    {
        // Arrange
        int value = 15;

        // Act
        var result = _sut.ClampAttribute(value);

        // Assert
        result.Should().Be(10, "default max is 10");
    }

    [Fact]
    public void ClampAttribute_ValueAtMin_ReturnsMin()
    {
        // Arrange
        int value = 1;

        // Act
        var result = _sut.ClampAttribute(value);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void ClampAttribute_ValueAtMax_ReturnsMax()
    {
        // Arrange
        int value = 10;

        // Act
        var result = _sut.ClampAttribute(value);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void ClampAttribute_ValueInRange_ReturnsUnchanged()
    {
        // Arrange
        int value = 5;

        // Act
        var result = _sut.ClampAttribute(value);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void ClampAttribute_DefaultRange_IsOneToTen()
    {
        // Act & Assert
        _sut.ClampAttribute(0).Should().Be(1);
        _sut.ClampAttribute(11).Should().Be(10);
        _sut.ClampAttribute(5).Should().Be(5);
    }

    [Fact]
    public void ClampAttribute_CustomMin_UsesCustomMin()
    {
        // Arrange
        int value = 0;
        int customMin = 3;

        // Act
        var result = _sut.ClampAttribute(value, min: customMin);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void ClampAttribute_CustomMax_UsesCustomMax()
    {
        // Arrange
        int value = 20;
        int customMax = 15;

        // Act
        var result = _sut.ClampAttribute(value, max: customMax);

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public void ClampAttribute_CustomRange_ClampsCorrectly()
    {
        // Arrange
        int customMin = 5;
        int customMax = 20;

        // Act & Assert
        _sut.ClampAttribute(3, customMin, customMax).Should().Be(5);
        _sut.ClampAttribute(25, customMin, customMax).Should().Be(20);
        _sut.ClampAttribute(10, customMin, customMax).Should().Be(10);
    }

    [Fact]
    public void ClampAttribute_NegativeValue_ClampsToMin()
    {
        // Arrange
        int value = -5;

        // Act
        var result = _sut.ClampAttribute(value);

        // Assert
        result.Should().Be(1);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(5, 5)]
    [InlineData(9, 9)]
    [InlineData(10, 10)]
    public void ClampAttribute_AllValidValues_ReturnUnchanged(int input, int expected)
    {
        // Act
        var result = _sut.ClampAttribute(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Logger Verification Tests

    [Fact]
    public void ApplyModifier_ShouldLogAtTraceLevel_OnEntry()
    {
        // Act
        _sut.ApplyModifier(5, 3);

        // Assert
        VerifyLogLevel(LogLevel.Trace);
    }

    [Fact]
    public void ApplyModifier_ShouldLogAtDebugLevel_OnResult()
    {
        // Act
        _sut.ApplyModifier(5, 3);

        // Assert
        VerifyLogLevel(LogLevel.Debug);
    }

    [Fact]
    public void ClampAttribute_BelowMin_ShouldLogWarning()
    {
        // Act
        _sut.ClampAttribute(0);

        // Assert
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void ClampAttribute_AboveMax_ShouldLogWarning()
    {
        // Act
        _sut.ClampAttribute(15);

        // Assert
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void ClampAttribute_InRange_ShouldNotLogWarning()
    {
        // Arrange
        var warningLogger = new Mock<ILogger<StatCalculationService>>();
        var service = new StatCalculationService(warningLogger.Object);

        // Act
        service.ClampAttribute(5);

        // Assert - verify Warning was NOT called
        warningLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void ApplyModifier_ShouldLogModifierValue()
    {
        // Act
        _sut.ApplyModifier(5, 3);

        // Assert
        VerifyLogMessageContains("3");
    }

    [Fact]
    public void ApplyModifier_ShouldLogBaseValue()
    {
        // Act
        _sut.ApplyModifier(5, 3);

        // Assert
        VerifyLogMessageContains("5");
    }

    [Fact]
    public void ClampAttribute_ShouldLogRange()
    {
        // Act
        _sut.ClampAttribute(5, 1, 10);

        // Assert
        VerifyLogMessageContains("1");
        VerifyLogMessageContains("10");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new StatCalculationService(_mockLogger.Object);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ApplyModifier_IntMaxValue_HandlesOverflow()
    {
        // Arrange
        int baseValue = int.MaxValue;
        int modifier = 1;

        // Act
        var result = _sut.ApplyModifier(baseValue, modifier);

        // Assert - C# integer overflow wraps around
        result.Should().Be(int.MinValue);
    }

    [Fact]
    public void ClampAttribute_MinEqualsMax_ReturnsMinMax()
    {
        // Arrange
        int min = 5;
        int max = 5;

        // Act
        var result = _sut.ClampAttribute(10, min, max);

        // Assert
        result.Should().Be(5);
    }

    #endregion

    #region CalculateMaxHP Tests

    [Fact]
    public void CalculateMaxHP_BaseSturdinessOne_ReturnsSixty()
    {
        // Formula: 50 + (Sturdiness * 10)
        var result = _sut.CalculateMaxHP(1);
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateMaxHP_BaseSturdinesssFive_ReturnsOneHundred()
    {
        var result = _sut.CalculateMaxHP(5);
        result.Should().Be(100);
    }

    [Fact]
    public void CalculateMaxHP_BaseSturdinesssTen_ReturnsOneHundredFifty()
    {
        var result = _sut.CalculateMaxHP(10);
        result.Should().Be(150);
    }

    [Fact]
    public void CalculateMaxHP_ZeroSturdiness_ReturnsFifty()
    {
        var result = _sut.CalculateMaxHP(0);
        result.Should().Be(50);
    }

    #endregion

    #region CalculateMaxStamina Tests

    [Fact]
    public void CalculateMaxStamina_BaseValues_CalculatesCorrectly()
    {
        // Formula: 20 + (Finesse * 5) + (Sturdiness * 3)
        // Finesse 1, Sturdiness 1 = 20 + 5 + 3 = 28
        var result = _sut.CalculateMaxStamina(1, 1);
        result.Should().Be(28);
    }

    [Fact]
    public void CalculateMaxStamina_MidValues_CalculatesCorrectly()
    {
        // Finesse 5, Sturdiness 5 = 20 + 25 + 15 = 60
        var result = _sut.CalculateMaxStamina(5, 5);
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateMaxStamina_MaxValues_CalculatesCorrectly()
    {
        // Finesse 10, Sturdiness 10 = 20 + 50 + 30 = 100
        var result = _sut.CalculateMaxStamina(10, 10);
        result.Should().Be(100);
    }

    [Fact]
    public void CalculateMaxStamina_ZeroValues_ReturnsBase()
    {
        var result = _sut.CalculateMaxStamina(0, 0);
        result.Should().Be(20);
    }

    #endregion

    #region CalculateActionPoints Tests

    [Fact]
    public void CalculateActionPoints_WitsOne_ReturnsTwo()
    {
        // Formula: 2 + (Wits / 4) = 2 + 0 = 2
        var result = _sut.CalculateActionPoints(1);
        result.Should().Be(2);
    }

    [Fact]
    public void CalculateActionPoints_WitsFour_ReturnsThree()
    {
        // 2 + (4 / 4) = 3
        var result = _sut.CalculateActionPoints(4);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateActionPoints_WitsEight_ReturnsFour()
    {
        // 2 + (8 / 4) = 4
        var result = _sut.CalculateActionPoints(8);
        result.Should().Be(4);
    }

    [Fact]
    public void CalculateActionPoints_WitsTen_ReturnsFour()
    {
        // 2 + (10 / 4) = 2 + 2 = 4 (integer division)
        var result = _sut.CalculateActionPoints(10);
        result.Should().Be(4);
    }

    #endregion

    #region RecalculateDerivedStats Tests

    [Fact]
    public void RecalculateDerivedStats_UsesEffectiveAttributes()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 5,
            Finesse = 5,
            Wits = 8
        };
        character.EquipmentBonuses[CharacterAttribute.Sturdiness] = 2; // Effective = 7

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        // MaxHP = 50 + (7 * 10) = 120
        character.MaxHP.Should().Be(120);
    }

    [Fact]
    public void RecalculateDerivedStats_SetsMaxHP()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 5,
            Finesse = 3,
            Wits = 4
        };

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        character.MaxHP.Should().Be(100); // 50 + (5 * 10)
    }

    [Fact]
    public void RecalculateDerivedStats_SetsMaxStamina()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 5,
            Finesse = 5,
            Wits = 4
        };

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        character.MaxStamina.Should().Be(60); // 20 + (5 * 5) + (5 * 3)
    }

    [Fact]
    public void RecalculateDerivedStats_SetsActionPoints()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 5,
            Finesse = 5,
            Wits = 8
        };

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        character.ActionPoints.Should().Be(4); // 2 + (8 / 4)
    }

    [Fact]
    public void RecalculateDerivedStats_PreservesHPRatio()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 5,
            Finesse = 5,
            Wits = 4,
            MaxHP = 100,
            CurrentHP = 50 // 50% HP
        };

        // Add equipment bonus to increase Sturdiness
        character.EquipmentBonuses[CharacterAttribute.Sturdiness] = 2;

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        // New MaxHP = 50 + (7 * 10) = 120
        character.MaxHP.Should().Be(120);
        // CurrentHP should be ~50% of new max = 60
        character.CurrentHP.Should().Be(60);
    }

    [Fact]
    public void RecalculateDerivedStats_PreservesStaminaRatio()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 5,
            Finesse = 5,
            Wits = 4,
            MaxStamina = 60,
            CurrentStamina = 30 // 50% Stamina
        };

        // Add equipment bonus to increase Finesse
        character.EquipmentBonuses[CharacterAttribute.Finesse] = 2;

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        // New MaxStamina = 20 + (7 * 5) + (5 * 3) = 20 + 35 + 15 = 70
        character.MaxStamina.Should().Be(70);
        // CurrentStamina should be ~50% of new max = 35
        character.CurrentStamina.Should().Be(35);
    }

    [Fact]
    public void RecalculateDerivedStats_CurrentHPNeverBelowOne()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Hero",
            Sturdiness = 10,
            Finesse = 5,
            Wits = 4,
            MaxHP = 150,
            CurrentHP = 1 // Very low HP
        };

        // Remove equipment bonus (decrease stats)
        character.EquipmentBonuses[CharacterAttribute.Sturdiness] = -5;

        // Act
        _sut.RecalculateDerivedStats(character);

        // Assert
        character.CurrentHP.Should().BeGreaterThanOrEqualTo(1);
    }

    #endregion

    #region GetArchetypeBonuses Tests

    [Fact]
    public void GetArchetypeBonuses_Warrior_ReturnsSturdinessAndMight()
    {
        // Act
        var bonuses = _sut.GetArchetypeBonuses(ArchetypeType.Warrior);

        // Assert
        bonuses.Should().ContainKey(CharacterAttribute.Sturdiness);
        bonuses[CharacterAttribute.Sturdiness].Should().Be(2);
        bonuses.Should().ContainKey(CharacterAttribute.Might);
        bonuses[CharacterAttribute.Might].Should().Be(1);
    }

    [Fact]
    public void GetArchetypeBonuses_Skirmisher_ReturnsFinesseAndWits()
    {
        // Act
        var bonuses = _sut.GetArchetypeBonuses(ArchetypeType.Skirmisher);

        // Assert
        bonuses.Should().ContainKey(CharacterAttribute.Finesse);
        bonuses[CharacterAttribute.Finesse].Should().Be(2);
        bonuses.Should().ContainKey(CharacterAttribute.Wits);
        bonuses[CharacterAttribute.Wits].Should().Be(1);
    }

    [Fact]
    public void GetArchetypeBonuses_Adept_ReturnsWitsAndWill()
    {
        // Act
        var bonuses = _sut.GetArchetypeBonuses(ArchetypeType.Adept);

        // Assert
        bonuses.Should().ContainKey(CharacterAttribute.Wits);
        bonuses[CharacterAttribute.Wits].Should().Be(2);
        bonuses.Should().ContainKey(CharacterAttribute.Will);
        bonuses[CharacterAttribute.Will].Should().Be(1);
    }

    [Fact]
    public void GetArchetypeBonuses_Mystic_ReturnsWillAndSturdiness()
    {
        // Act
        var bonuses = _sut.GetArchetypeBonuses(ArchetypeType.Mystic);

        // Assert
        bonuses.Should().ContainKey(CharacterAttribute.Will);
        bonuses[CharacterAttribute.Will].Should().Be(2);
        bonuses.Should().ContainKey(CharacterAttribute.Sturdiness);
        bonuses[CharacterAttribute.Sturdiness].Should().Be(1);
    }

    #endregion

    #region GetLineageBonuses Tests

    [Fact]
    public void GetLineageBonuses_Human_ReturnsAllAttributesPlus1()
    {
        // Act
        var bonuses = _sut.GetLineageBonuses(LineageType.Human);

        // Assert
        bonuses.Should().HaveCount(5);
        bonuses.Values.Should().OnlyContain(v => v == 1);
    }

    [Fact]
    public void GetLineageBonuses_RuneMarked_ReturnsWitsAndWillWithSturdinesssPenalty()
    {
        // Act
        var bonuses = _sut.GetLineageBonuses(LineageType.RuneMarked);

        // Assert
        bonuses[CharacterAttribute.Wits].Should().Be(2);
        bonuses[CharacterAttribute.Will].Should().Be(2);
        bonuses[CharacterAttribute.Sturdiness].Should().Be(-1);
    }

    [Fact]
    public void GetLineageBonuses_IronBlooded_ReturnsSturdinessAndMightWithWitsPenalty()
    {
        // Act
        var bonuses = _sut.GetLineageBonuses(LineageType.IronBlooded);

        // Assert
        bonuses[CharacterAttribute.Sturdiness].Should().Be(2);
        bonuses[CharacterAttribute.Might].Should().Be(2);
        bonuses[CharacterAttribute.Wits].Should().Be(-1);
    }

    [Fact]
    public void GetLineageBonuses_VargrKin_ReturnsFinesseAndWitsWithWillPenalty()
    {
        // Act
        var bonuses = _sut.GetLineageBonuses(LineageType.VargrKin);

        // Assert
        bonuses[CharacterAttribute.Finesse].Should().Be(2);
        bonuses[CharacterAttribute.Wits].Should().Be(2);
        bonuses[CharacterAttribute.Will].Should().Be(-1);
    }

    #endregion

    #region Helper Methods

    private void VerifyLogLevel(LogLevel level)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogMessageContains(string substring)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(substring)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
