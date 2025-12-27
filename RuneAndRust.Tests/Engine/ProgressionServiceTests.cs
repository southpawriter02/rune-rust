using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the ProgressionService class.
/// Validates attribute upgrade transactions, PP spending, and cap enforcement.
/// </summary>
/// <remarks>See: v0.4.0b (The Growth) for system design.</remarks>
public class ProgressionServiceTests
{
    private readonly Mock<IStatCalculationService> _mockStatCalc;
    private readonly Mock<ILogger<ProgressionService>> _mockLogger;
    private readonly ProgressionService _sut;

    public ProgressionServiceTests()
    {
        _mockStatCalc = new Mock<IStatCalculationService>();
        _mockLogger = new Mock<ILogger<ProgressionService>>();
        _sut = new ProgressionService(_mockStatCalc.Object, _mockLogger.Object);
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test character with default attributes (all 5) and specified PP.
    /// </summary>
    private static Character CreateTestCharacter(int pp = 5, int attributeValue = 5)
    {
        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestHero",
            ProgressionPoints = pp,
            MaxHP = 100,
            CurrentHP = 100,
            MaxStamina = 50,
            CurrentStamina = 50
        };

        // Set all attributes to the specified value
        character.SetAttribute(CharacterAttribute.Might, attributeValue);
        character.SetAttribute(CharacterAttribute.Finesse, attributeValue);
        character.SetAttribute(CharacterAttribute.Wits, attributeValue);
        character.SetAttribute(CharacterAttribute.Will, attributeValue);
        character.SetAttribute(CharacterAttribute.Sturdiness, attributeValue);

        return character;
    }

    #endregion

    #region UpgradeAttribute - Success Tests

    [Fact]
    public void UpgradeAttribute_DeductsPP_FromCharacter()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5);
        int initialPP = character.ProgressionPoints;

        // Act
        _sut.UpgradeAttribute(character, CharacterAttribute.Might);

        // Assert
        character.ProgressionPoints.Should().Be(initialPP - 1);
    }

    [Fact]
    public void UpgradeAttribute_IncreasesAttribute_ByOne()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 5);
        int initialValue = character.GetAttribute(CharacterAttribute.Might);

        // Act
        _sut.UpgradeAttribute(character, CharacterAttribute.Might);

        // Assert
        character.GetAttribute(CharacterAttribute.Might).Should().Be(initialValue + 1);
    }

    [Fact]
    public void UpgradeAttribute_CallsRecalculateDerivedStats()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5);

        // Act
        _sut.UpgradeAttribute(character, CharacterAttribute.Sturdiness);

        // Assert
        _mockStatCalc.Verify(s => s.RecalculateDerivedStats(character), Times.Once);
    }

    [Fact]
    public void UpgradeAttribute_ReturnsSuccess_WithCorrectDetails()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 5);

        // Act
        var result = _sut.UpgradeAttribute(character, CharacterAttribute.Finesse);

        // Assert
        result.Success.Should().BeTrue();
        result.Attribute.Should().Be(CharacterAttribute.Finesse);
        result.OldValue.Should().Be(5);
        result.NewValue.Should().Be(6);
        result.PpSpent.Should().Be(1);
        result.Message.Should().Contain("Finesse");
    }

    #endregion

    #region UpgradeAttribute - Failure Tests

    [Fact]
    public void UpgradeAttribute_Fails_WhenInsufficientPP()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 0);
        int initialAttribute = character.GetAttribute(CharacterAttribute.Might);

        // Act
        var result = _sut.UpgradeAttribute(character, CharacterAttribute.Might);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient");
        character.ProgressionPoints.Should().Be(0, "PP should not change on failure");
        character.GetAttribute(CharacterAttribute.Might).Should().Be(initialAttribute, "Attribute should not change on failure");
    }

    [Fact]
    public void UpgradeAttribute_Fails_WhenAttributeAtCap()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 10);
        int initialPP = character.ProgressionPoints;

        // Act
        var result = _sut.UpgradeAttribute(character, CharacterAttribute.Wits);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("maximum");
        character.ProgressionPoints.Should().Be(initialPP, "PP should not be deducted on failure");
        character.GetAttribute(CharacterAttribute.Wits).Should().Be(10, "Attribute should remain at cap");
    }

    [Fact]
    public void UpgradeAttribute_AtCap_DoesNotCallRecalculate()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 10);

        // Act
        _sut.UpgradeAttribute(character, CharacterAttribute.Will);

        // Assert
        _mockStatCalc.Verify(s => s.RecalculateDerivedStats(It.IsAny<Character>()), Times.Never);
    }

    #endregion

    #region GetUpgradeCost Tests

    [Fact]
    public void GetUpgradeCost_Returns1_ForNormalAttributeValue()
    {
        // Arrange
        var character = CreateTestCharacter(attributeValue: 5);

        // Act
        var cost = _sut.GetUpgradeCost(character, CharacterAttribute.Might);

        // Assert
        cost.Should().Be(1);
    }

    [Fact]
    public void GetUpgradeCost_ReturnsMaxValue_WhenAttributeAtCap()
    {
        // Arrange
        var character = CreateTestCharacter(attributeValue: 10);

        // Act
        var cost = _sut.GetUpgradeCost(character, CharacterAttribute.Might);

        // Assert
        cost.Should().Be(int.MaxValue);
    }

    #endregion

    #region CanUpgrade Tests

    [Fact]
    public void CanUpgrade_ReturnsTrue_WhenAffordableAndNotCapped()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 5);

        // Act
        var canUpgrade = _sut.CanUpgrade(character, CharacterAttribute.Finesse);

        // Assert
        canUpgrade.Should().BeTrue();
    }

    [Fact]
    public void CanUpgrade_ReturnsFalse_WhenAttributeAtCap()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 10);

        // Act
        var canUpgrade = _sut.CanUpgrade(character, CharacterAttribute.Sturdiness);

        // Assert
        canUpgrade.Should().BeFalse();
    }

    [Fact]
    public void CanUpgrade_ReturnsFalse_WhenInsufficientPP()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 0, attributeValue: 5);

        // Act
        var canUpgrade = _sut.CanUpgrade(character, CharacterAttribute.Wits);

        // Assert
        canUpgrade.Should().BeFalse();
    }

    #endregion

    #region All Attributes Tests

    [Theory]
    [InlineData(CharacterAttribute.Might)]
    [InlineData(CharacterAttribute.Finesse)]
    [InlineData(CharacterAttribute.Wits)]
    [InlineData(CharacterAttribute.Will)]
    [InlineData(CharacterAttribute.Sturdiness)]
    public void UpgradeAttribute_WorksForAllAttributes(CharacterAttribute attribute)
    {
        // Arrange
        var character = CreateTestCharacter(pp: 5, attributeValue: 5);

        // Act
        var result = _sut.UpgradeAttribute(character, attribute);

        // Assert
        result.Success.Should().BeTrue();
        character.GetAttribute(attribute).Should().Be(6);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void UpgradeAttribute_CanUpgradeFromMinToMax_Incrementally()
    {
        // Arrange - Start at minimum (1) with enough PP
        var character = CreateTestCharacter(pp: 10, attributeValue: 1);

        // Act - Upgrade 9 times to reach cap (10)
        for (int i = 0; i < 9; i++)
        {
            var result = _sut.UpgradeAttribute(character, CharacterAttribute.Might);
            result.Success.Should().BeTrue($"Upgrade {i + 1} should succeed");
        }

        // Assert
        character.GetAttribute(CharacterAttribute.Might).Should().Be(10);
        character.ProgressionPoints.Should().Be(1, "Should have 1 PP remaining");
    }

    [Fact]
    public void UpgradeAttribute_MultipleAttributes_IndependentlyTracked()
    {
        // Arrange
        var character = CreateTestCharacter(pp: 3, attributeValue: 5);

        // Act - Upgrade three different attributes
        _sut.UpgradeAttribute(character, CharacterAttribute.Might);
        _sut.UpgradeAttribute(character, CharacterAttribute.Finesse);
        _sut.UpgradeAttribute(character, CharacterAttribute.Wits);

        // Assert
        character.GetAttribute(CharacterAttribute.Might).Should().Be(6);
        character.GetAttribute(CharacterAttribute.Finesse).Should().Be(6);
        character.GetAttribute(CharacterAttribute.Wits).Should().Be(6);
        character.GetAttribute(CharacterAttribute.Will).Should().Be(5, "Untouched attribute should remain at 5");
        character.GetAttribute(CharacterAttribute.Sturdiness).Should().Be(5, "Untouched attribute should remain at 5");
        character.ProgressionPoints.Should().Be(0);
    }

    #endregion
}
