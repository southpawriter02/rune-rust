using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the SagaService class.
/// Validates Legend (XP) accumulation, Level thresholds, and Progression Point awards.
/// </summary>
/// <remarks>See: v0.4.0a (The Legend) for system design.</remarks>
public class SagaServiceTests
{
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<IStatCalculationService> _mockStatCalc;
    private readonly Mock<ILogger<SagaService>> _mockLogger;
    private readonly SagaService _sut;

    public SagaServiceTests()
    {
        _mockEventBus = new Mock<IEventBus>();
        _mockStatCalc = new Mock<IStatCalculationService>();
        _mockLogger = new Mock<ILogger<SagaService>>();
        _sut = new SagaService(_mockEventBus.Object, _mockStatCalc.Object, _mockLogger.Object);
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test character with default values.
    /// </summary>
    private static Character CreateTestCharacter(int legend = 0, int level = 1, int pp = 0)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestHero",
            Legend = legend,
            Level = level,
            ProgressionPoints = pp,
            MaxHP = 100,
            CurrentHP = 50,
            MaxStamina = 50,
            CurrentStamina = 25
        };
    }

    #endregion

    #region AddLegend - Basic Accumulation Tests

    [Fact]
    public void AddLegend_PositiveAmount_IncreasesTotal()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0);

        // Act
        _sut.AddLegend(character, 50, "Test reward");

        // Assert
        character.Legend.Should().Be(50);
    }

    [Fact]
    public void AddLegend_MultipleAdds_AccumulatesCorrectly()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0);

        // Act
        _sut.AddLegend(character, 30, "First reward");
        _sut.AddLegend(character, 20, "Second reward");

        // Assert
        character.Legend.Should().Be(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void AddLegend_ZeroOrNegativeAmount_NoChange(int invalidAmount)
    {
        // Arrange
        var character = CreateTestCharacter(legend: 50);

        // Act
        _sut.AddLegend(character, invalidAmount, "Invalid reward");

        // Assert
        character.Legend.Should().Be(50, "Legend should remain unchanged for non-positive amounts");
    }

    #endregion

    #region AddLegend - Level Up Tests

    [Fact]
    public void AddLegend_ReachesThreshold_TriggersLevelUp()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);

        // Act - Add exactly 100 Legend to reach Level 2 threshold
        _sut.AddLegend(character, 100, "Defeated boss");

        // Assert
        character.Level.Should().Be(2);
    }

    [Fact]
    public void AddLegend_ExceedsThreshold_TriggersLevelUp()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);

        // Act - Add more than needed for Level 2
        _sut.AddLegend(character, 150, "Defeated elite");

        // Assert
        character.Level.Should().Be(2);
        character.Legend.Should().Be(150);
    }

    [Fact]
    public void AddLegend_BelowThreshold_NoLevelUp()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);

        // Act - Add less than Level 2 threshold (100)
        _sut.AddLegend(character, 50, "Minor reward");

        // Assert
        character.Level.Should().Be(1);
    }

    #endregion

    #region AddLegend - Progression Points Tests

    [Fact]
    public void AddLegend_AwardsPP_OnLevelUp()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1, pp: 0);

        // Act - Level up to Level 2 (awards 1 PP)
        _sut.AddLegend(character, 100, "Level up reward");

        // Assert
        character.ProgressionPoints.Should().Be(1);
    }

    [Fact]
    public void AddLegend_AwardsCorrectPP_ForHigherLevels()
    {
        // Arrange - Character at level 3 (300 Legend), needs 600 for Level 4 (awards 2 PP)
        var character = CreateTestCharacter(legend: 300, level: 3, pp: 2);

        // Act - Add 300 more to reach 600 (Level 4 threshold)
        _sut.AddLegend(character, 300, "Major quest");

        // Assert
        character.Level.Should().Be(4);
        character.ProgressionPoints.Should().Be(4); // 2 original + 2 awarded
    }

    #endregion

    #region AddLegend - Multi-Level Jump Tests

    [Fact]
    public void AddLegend_MultiLevelJump_HandlesRecursively()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1, pp: 0);

        // Act - Add 700 Legend: enough for L2(100), L3(300), L4(600)
        _sut.AddLegend(character, 700, "Massive reward");

        // Assert
        character.Level.Should().Be(4);
        // PP: L2=1, L3=1, L4=2 = 4 total
        character.ProgressionPoints.Should().Be(4);
    }

    [Fact]
    public void AddLegend_MultiLevelJump_AccumulatesAllPP()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1, pp: 0);

        // Act - Add 1000 Legend: L2(100)=1PP, L3(300)=1PP, L4(600)=2PP, L5(1000)=2PP
        _sut.AddLegend(character, 1000, "Epic reward");

        // Assert
        character.Level.Should().Be(5);
        character.ProgressionPoints.Should().Be(6); // 1+1+2+2
    }

    #endregion

    #region AddLegend - HP/Stamina Restoration Tests

    [Fact]
    public void AddLegend_RestoresHpStamina_OnLevelUp()
    {
        // Arrange - Character with low HP/Stamina
        var character = CreateTestCharacter(legend: 0, level: 1);
        character.MaxHP = 100;
        character.CurrentHP = 10;
        character.MaxStamina = 50;
        character.CurrentStamina = 5;

        // Act
        _sut.AddLegend(character, 100, "Level up");

        // Assert - Full heal on level-up
        character.CurrentHP.Should().Be(character.MaxHP);
        character.CurrentStamina.Should().Be(character.MaxStamina);
    }

    [Fact]
    public void AddLegend_CallsRecalculateDerivedStats_OnLevelUp()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);

        // Act
        _sut.AddLegend(character, 100, "Level up");

        // Assert
        _mockStatCalc.Verify(s => s.RecalculateDerivedStats(character), Times.Once);
    }

    #endregion

    #region AddLegend - Event Publishing Tests

    [Fact]
    public void AddLegend_PublishesLevelUpEvent()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);
        LevelUpEvent? capturedEvent = null;
        _mockEventBus.Setup(e => e.Publish(It.IsAny<LevelUpEvent>()))
            .Callback<LevelUpEvent>(e => capturedEvent = e);

        // Act
        _sut.AddLegend(character, 100, "Level up");

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.IsAny<LevelUpEvent>()), Times.Once);
        capturedEvent.Should().NotBeNull();
        capturedEvent!.CharacterId.Should().Be(character.Id);
        capturedEvent.CharacterName.Should().Be("TestHero");
        capturedEvent.NewLevel.Should().Be(2);
        capturedEvent.ProgressionPointsAwarded.Should().Be(1);
    }

    [Fact]
    public void AddLegend_MultiLevelJump_PublishesMultipleEvents()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);

        // Act - Jump from L1 to L4
        _sut.AddLegend(character, 700, "Multi-level");

        // Assert - Should publish 3 events (L2, L3, L4)
        _mockEventBus.Verify(e => e.Publish(It.IsAny<LevelUpEvent>()), Times.Exactly(3));
    }

    [Fact]
    public void AddLegend_NoLevelUp_DoesNotPublishEvent()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 0, level: 1);

        // Act - Add less than threshold
        _sut.AddLegend(character, 50, "Minor reward");

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.IsAny<LevelUpEvent>()), Times.Never);
    }

    #endregion

    #region AddLegend - Max Level Tests

    [Fact]
    public void AddLegend_AtMaxLevel_NoFurtherLevelUp()
    {
        // Arrange - Character at max level (10)
        var character = CreateTestCharacter(legend: 4500, level: 10, pp: 22);

        // Act
        _sut.AddLegend(character, 1000, "Post-max reward");

        // Assert
        character.Level.Should().Be(10, "Level should not exceed max");
        character.Legend.Should().Be(5500, "Legend should still accumulate");
        character.ProgressionPoints.Should().Be(22, "No additional PP at max level");
    }

    [Fact]
    public void AddLegend_AtMaxLevel_DoesNotPublishEvent()
    {
        // Arrange
        var character = CreateTestCharacter(legend: 4500, level: 10);

        // Act
        _sut.AddLegend(character, 1000, "Post-max reward");

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.IsAny<LevelUpEvent>()), Times.Never);
    }

    #endregion

    #region GetLegendForNextLevel Tests

    [Theory]
    [InlineData(1, 100)]   // L1 -> L2 requires 100
    [InlineData(2, 300)]   // L2 -> L3 requires 300
    [InlineData(3, 600)]   // L3 -> L4 requires 600
    [InlineData(4, 1000)]  // L4 -> L5 requires 1000
    [InlineData(5, 1500)]  // L5 -> L6 requires 1500
    [InlineData(9, 4500)]  // L9 -> L10 requires 4500
    public void GetLegendForNextLevel_ReturnsCorrectThreshold(int currentLevel, int expectedLegend)
    {
        // Act
        var result = _sut.GetLegendForNextLevel(currentLevel);

        // Assert
        result.Should().Be(expectedLegend);
    }

    [Fact]
    public void GetLegendForNextLevel_AtMaxLevel_ReturnsNegativeOne()
    {
        // Act
        var result = _sut.GetLegendForNextLevel(10);

        // Assert
        result.Should().Be(-1);
    }

    #endregion

    #region GetPpAward Tests

    [Theory]
    [InlineData(1, 0)]   // Level 1 awards 0 PP (starting level)
    [InlineData(2, 1)]   // Level 2 awards 1 PP
    [InlineData(3, 1)]   // Level 3 awards 1 PP
    [InlineData(4, 2)]   // Level 4 awards 2 PP
    [InlineData(7, 3)]   // Level 7 awards 3 PP
    [InlineData(10, 5)]  // Level 10 awards 5 PP
    public void GetPpAward_ReturnsCorrectAmount(int level, int expectedPp)
    {
        // Act
        var result = _sut.GetPpAward(level);

        // Assert
        result.Should().Be(expectedPp);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(100)]
    public void GetPpAward_InvalidLevel_ReturnsZero(int invalidLevel)
    {
        // Act
        var result = _sut.GetPpAward(invalidLevel);

        // Assert
        result.Should().Be(0);
    }

    #endregion
}
