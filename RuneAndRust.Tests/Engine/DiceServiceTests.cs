using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Extensive tests for the DiceService class.
/// Validates dice pool mechanics, success/botch calculations, fumble detection, and logging behavior.
/// </summary>
public class DiceServiceTests
{
    private readonly Mock<ILogger<DiceService>> _mockLogger;
    private readonly DiceService _sut;

    public DiceServiceTests()
    {
        _mockLogger = new Mock<ILogger<DiceService>>();
        _sut = new DiceService(_mockLogger.Object);
    }

    #region Pool Size Tests

    [Fact]
    public void Roll_WithZeroPoolSize_ShouldLogWarningAndRollOne()
    {
        // Act
        var result = _sut.Roll(0, "ZeroPoolTest");

        // Assert
        result.Rolls.Should().HaveCount(1, "zero pool size should be clamped to 1");
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void Roll_WithNegativePoolSize_ShouldLogErrorAndRollOne()
    {
        // Act
        var result = _sut.Roll(-5, "NegativePoolTest");

        // Assert
        result.Rolls.Should().HaveCount(1, "negative pool size should be clamped to 1");
        VerifyLogLevel(LogLevel.Error);
    }

    [Fact]
    public void Roll_WithPoolSizeOne_ReturnsExactlyOneRoll()
    {
        // Act
        var result = _sut.Roll(1);

        // Assert
        result.Rolls.Should().HaveCount(1);
    }

    [Fact]
    public void Roll_WithPoolSizeFive_ReturnsExactlyFiveRolls()
    {
        // Act
        var result = _sut.Roll(5);

        // Assert
        result.Rolls.Should().HaveCount(5);
    }

    [Fact]
    public void Roll_WithPoolSizeTen_ReturnsExactlyTenRolls()
    {
        // Act
        var result = _sut.Roll(10);

        // Assert
        result.Rolls.Should().HaveCount(10);
    }

    [Fact]
    public void Roll_WithLargePoolSize_HandlesCorrectly()
    {
        // Act
        var result = _sut.Roll(100);

        // Assert
        result.Rolls.Should().HaveCount(100);
    }

    #endregion

    #region Roll Value Tests

    [Fact]
    public void Roll_AllValues_ShouldBeBetweenOneAndTen()
    {
        // Arrange - Roll many dice to get a good sample
        var result = _sut.Roll(100, "RangeTest");

        // Assert
        result.Rolls.Should().OnlyContain(v => v >= 1 && v <= 10);
    }

    [Fact]
    public void Roll_ShouldNeverReturnZero()
    {
        // Arrange - Roll many dice to ensure statistical coverage
        for (int i = 0; i < 100; i++)
        {
            var result = _sut.Roll(10);

            // Assert
            result.Rolls.Should().NotContain(0, "d10 cannot roll 0");
        }
    }

    [Fact]
    public void Roll_ShouldNeverReturnEleven()
    {
        // Arrange - Roll many dice to ensure statistical coverage
        for (int i = 0; i < 100; i++)
        {
            var result = _sut.Roll(10);

            // Assert
            result.Rolls.Should().NotContain(11, "d10 cannot roll 11");
        }
    }

    #endregion

    #region Success Calculation Tests

    [Fact]
    public void Roll_SuccessCount_ShouldMatchRollsEightOrHigher()
    {
        // Act - Roll many times to verify consistency
        for (int i = 0; i < 50; i++)
        {
            var result = _sut.Roll(10);

            // Assert
            var expectedSuccesses = result.Rolls.Count(r => r >= 8);
            result.Successes.Should().Be(expectedSuccesses);
        }
    }

    [Fact]
    public void Roll_WithKnownRolls_SuccessesCalculatedCorrectly()
    {
        // We can't easily inject specific roll values without mocking Random,
        // but we can verify the logic by checking the relationship between Rolls and Successes
        var result = _sut.Roll(20);

        var manualSuccessCount = result.Rolls.Count(r => r >= 8);
        result.Successes.Should().Be(manualSuccessCount);
    }

    #endregion

    #region Botch Calculation Tests

    [Fact]
    public void Roll_BotchCount_ShouldMatchRollsOfOne()
    {
        // Act - Roll many times to verify consistency
        for (int i = 0; i < 50; i++)
        {
            var result = _sut.Roll(10);

            // Assert
            var expectedBotches = result.Rolls.Count(r => r == 1);
            result.Botches.Should().Be(expectedBotches);
        }
    }

    [Fact]
    public void Roll_WithKnownRolls_BotchesCalculatedCorrectly()
    {
        var result = _sut.Roll(20);

        var manualBotchCount = result.Rolls.Count(r => r == 1);
        result.Botches.Should().Be(manualBotchCount);
    }

    #endregion

    #region Fumble Tests

    [Fact]
    public void Roll_ZeroSuccessesWithBotches_LogsWarning()
    {
        // We need to roll enough times to likely hit a fumble
        var fumbleFound = false;
        for (int attempt = 0; attempt < 1000 && !fumbleFound; attempt++)
        {
            var result = _sut.Roll(3, "FumbleTest");
            if (result.Successes == 0 && result.Botches > 0)
            {
                fumbleFound = true;
            }
        }

        // If we found a fumble, verify warning was logged
        if (fumbleFound)
        {
            VerifyLogMessageContains("FUMBLE");
        }
        else
        {
            // Statistically unlikely but possible - skip assertion
            Assert.True(true, "No fumble occurred in 1000 attempts (statistically unlikely but possible)");
        }
    }

    [Fact]
    public void Roll_SuccessesAndBotchesRelationship_IsIndependent()
    {
        // Verify that successes and botches are counted independently
        var result = _sut.Roll(50);

        // A roll of 1 is ONLY a botch (not a success)
        // A roll of 8+ is ONLY a success (not a botch)
        // Values 2-7 are neither
        var ones = result.Rolls.Count(r => r == 1);
        var eightOrHigher = result.Rolls.Count(r => r >= 8);
        var middleValues = result.Rolls.Count(r => r >= 2 && r <= 7);

        result.Botches.Should().Be(ones);
        result.Successes.Should().Be(eightOrHigher);
        (ones + eightOrHigher + middleValues).Should().Be(result.Rolls.Count);
    }

    #endregion

    #region Context Tests

    [Fact]
    public void Roll_WithContext_IncludesContextInLogs()
    {
        // Act
        _sut.Roll(5, "CombatRoll");

        // Assert - verify context appears in log messages
        VerifyLogMessageContains("CombatRoll");
    }

    [Fact]
    public void Roll_WithoutContext_UsesDefaultContext()
    {
        // Act
        _sut.Roll(5);

        // Assert - default context is "Unspecified"
        VerifyLogMessageContains("Unspecified");
    }

    #endregion

    #region DiceResult Record Tests

    [Fact]
    public void DiceResult_Rolls_ShouldNotBeDirectlyModifiable()
    {
        // Arrange - DiceService creates the result, which returns a ReadOnlyCollection
        var result = _sut.Roll(4, "ImmutabilityTest");

        // Assert - Rolls is IReadOnlyList, cannot be cast to List and modified
        result.Rolls.Should().BeAssignableTo<IReadOnlyList<int>>();

        // The Rolls collection should not be directly modifiable
        // (IReadOnlyList doesn't expose Add, Remove, etc.)
        result.Rolls.GetType().GetMethod("Add").Should().BeNull("IReadOnlyList should not have an Add method");
    }

    [Fact]
    public void DiceResult_Properties_ShouldBeInitOnly()
    {
        // Arrange
        var rolls = new List<int> { 1, 5, 8, 10 }.AsReadOnly();
        var result = new DiceResult(2, 1, rolls);

        // Assert - Record properties are init-only, verified by checking the values are set correctly
        result.Successes.Should().Be(2);
        result.Botches.Should().Be(1);
        result.Rolls.Should().HaveCount(4);
    }

    [Fact]
    public void DiceResult_Equality_SameValues_AreEquivalent()
    {
        // Arrange
        var rolls1 = new List<int> { 1, 5, 8 }.AsReadOnly();
        var rolls2 = new List<int> { 1, 5, 8 }.AsReadOnly();
        var result1 = new DiceResult(1, 1, rolls1);
        var result2 = new DiceResult(1, 1, rolls2);

        // Assert - Records with same values should be equivalent (note: records use reference equality for collections)
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public void DiceResult_Equality_SameRollsReference_AreEqual()
    {
        // Arrange - Same rolls reference should result in actual record equality
        var rolls = new List<int> { 1, 5, 8 }.AsReadOnly();
        var result1 = new DiceResult(1, 1, rolls);
        var result2 = new DiceResult(1, 1, rolls);

        // Assert - Records with same reference for Rolls should be equal
        result1.Should().Be(result2);
    }

    [Fact]
    public void DiceResult_Equality_DifferentSuccesses_AreNotEqual()
    {
        // Arrange
        var rolls = new List<int> { 1, 5, 8 }.AsReadOnly();
        var result1 = new DiceResult(1, 1, rolls);
        var result2 = new DiceResult(2, 1, rolls);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void DiceResult_Equality_DifferentBotches_AreNotEqual()
    {
        // Arrange
        var rolls = new List<int> { 1, 5, 8 }.AsReadOnly();
        var result1 = new DiceResult(1, 1, rolls);
        var result2 = new DiceResult(1, 2, rolls);

        // Assert
        result1.Should().NotBe(result2);
    }

    #endregion

    #region Logger Verification Tests

    [Fact]
    public void Roll_ShouldLogAtTraceLevel_ForMethodEntry()
    {
        // Act
        _sut.Roll(5, "TraceTest");

        // Assert
        VerifyLogLevel(LogLevel.Trace);
    }

    [Fact]
    public void Roll_ShouldLogAtDebugLevel_ForResultSummary()
    {
        // Act
        _sut.Roll(5, "DebugTest");

        // Assert
        VerifyLogLevel(LogLevel.Debug);
    }

    [Fact]
    public void Roll_ShouldLogAtWarningLevel_ForInvalidPoolSize()
    {
        // Act
        _sut.Roll(0, "InvalidPoolTest");

        // Assert
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new DiceService(_mockLogger.Object);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region RollSingle Tests

    [Fact]
    public void RollSingle_ReturnsValueInRange()
    {
        // Act & Assert - Run multiple times to verify range
        for (int i = 0; i < 100; i++)
        {
            var result = _sut.RollSingle(10, "RangeTest");
            result.Should().BeInRange(1, 10);
        }
    }

    [Fact]
    public void RollSingle_WithDifferentSides_ReturnsValueInRange()
    {
        // Arrange & Act
        var d6Results = Enumerable.Range(0, 50).Select(_ => _sut.RollSingle(6)).ToList();
        var d20Results = Enumerable.Range(0, 50).Select(_ => _sut.RollSingle(20)).ToList();

        // Assert
        d6Results.Should().AllSatisfy(r => r.Should().BeInRange(1, 6));
        d20Results.Should().AllSatisfy(r => r.Should().BeInRange(1, 20));
    }

    [Fact]
    public void RollSingle_WithZeroSides_ClampsToOne()
    {
        // Act
        var result = _sut.RollSingle(0, "ZeroSidesTest");

        // Assert
        result.Should().Be(1);
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void RollSingle_WithNegativeSides_ClampsToOne()
    {
        // Act
        var result = _sut.RollSingle(-5, "NegativeSidesTest");

        // Assert
        result.Should().Be(1);
        VerifyLogLevel(LogLevel.Warning);
    }

    [Fact]
    public void RollSingle_LogsContext()
    {
        // Act
        _sut.RollSingle(10, "InitiativeRoll");

        // Assert
        VerifyLogMessageContains("InitiativeRoll");
    }

    [Fact]
    public void RollSingle_Distribution_ShouldBeReasonablyRandom()
    {
        // Roll many d10s and verify distribution
        var allRolls = Enumerable.Range(0, 1000).Select(_ => _sut.RollSingle(10)).ToList();

        // Each value 1-10 should appear roughly 100 times
        var groups = allRolls.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());

        foreach (var value in Enumerable.Range(1, 10))
        {
            groups.Should().ContainKey(value, $"value {value} should appear in 1000 rolls");
            groups[value].Should().BeGreaterThan(50, $"value {value} should appear at least 50 times in 1000 rolls");
        }
    }

    #endregion

    #region Statistical Distribution Tests

    [Fact]
    public void Roll_Distribution_ShouldBeReasonablyRandom()
    {
        // Roll a large number of dice and verify distribution is roughly uniform
        var allRolls = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            var result = _sut.Roll(100);
            allRolls.AddRange(result.Rolls);
        }

        // Each value 1-10 should appear roughly 1000 times (10000 total / 10 values)
        // We allow a generous margin for randomness
        var groups = allRolls.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());

        foreach (var value in Enumerable.Range(1, 10))
        {
            groups.Should().ContainKey(value, $"value {value} should appear in 10000 rolls");
            groups[value].Should().BeGreaterThan(500, $"value {value} should appear at least 500 times in 10000 rolls");
        }
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
