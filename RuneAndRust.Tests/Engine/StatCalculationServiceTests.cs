using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Engine.Services;
using Xunit;

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
