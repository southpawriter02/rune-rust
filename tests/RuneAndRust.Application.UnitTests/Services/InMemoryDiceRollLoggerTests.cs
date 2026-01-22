using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for InMemoryDiceRollLogger.
/// </summary>
/// <remarks>
/// v0.15.0e: Tests roll logging, history retrieval, context filtering,
/// fumble/critical flag storage, and history trimming.
/// </remarks>
[TestFixture]
public class InMemoryDiceRollLoggerTests
{
    private Mock<ILogger<InMemoryDiceRollLogger>> _mockLogger = null!;
    private InMemoryDiceRollLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<InMemoryDiceRollLogger>>();
        _logger = new InMemoryDiceRollLogger(_mockLogger.Object);
    }

    #region LogRoll Tests

    [Test]
    public void LogRoll_StoresRollWithCorrectData()
    {
        // Arrange
        var pool = new DicePool(5, DiceType.D10);
        var rolls = new[] { 8, 9, 1, 4, 3 };
        var diceResult = new DiceRollResult(pool, rolls);
        var actorId = Guid.NewGuid();
        var rollLog = DiceRollLog.FromRollResult(
            diceResult,
            seed: 12345,
            context: RollContexts.CombatAttack,
            actorId: actorId);

        // Act
        _logger.LogRoll(rollLog);

        // Assert
        _logger.HistoryCount.Should().Be(1);

        var history = _logger.GetRollHistory(1);
        history.Should().HaveCount(1);
        history[0].Context.Should().Be(RollContexts.CombatAttack);
        history[0].Seed.Should().Be(12345);
        history[0].PoolSize.Should().Be(5);
        history[0].NetSuccesses.Should().Be(diceResult.NetSuccesses);
        history[0].ActorId.Should().Be(actorId);
    }

    [Test]
    public void LogRoll_NullLog_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _logger.LogRoll(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetRollHistory Tests

    [Test]
    public void GetRollHistory_ReturnsRequestedCount()
    {
        // Arrange: Log 10 rolls
        for (int i = 0; i < 10; i++)
        {
            var pool = new DicePool(3, DiceType.D10);
            var rolls = new[] { 5, 6, 7 };
            var diceResult = new DiceRollResult(pool, rolls);
            var rollLog = DiceRollLog.FromRollResult(diceResult, seed: i, context: $"Test:{i}");
            _logger.LogRoll(rollLog);
        }

        // Act
        var history = _logger.GetRollHistory(5);

        // Assert
        history.Should().HaveCount(5);
        // Should be the 5 most recent (oldest to newest: 5, 6, 7, 8, 9)
        history[0].Seed.Should().Be(5);
        history[4].Seed.Should().Be(9);
    }

    [Test]
    public void GetRollHistory_ZeroOrNegativeCount_ReturnsEmpty()
    {
        // Arrange
        LogSampleRoll();

        // Act & Assert
        _logger.GetRollHistory(0).Should().BeEmpty();
        _logger.GetRollHistory(-1).Should().BeEmpty();
    }

    [Test]
    public void GetRollHistory_CountExceedsHistory_ReturnsAll()
    {
        // Arrange: Log 3 rolls
        for (int i = 0; i < 3; i++)
        {
            LogSampleRoll(seed: i);
        }

        // Act
        var history = _logger.GetRollHistory(100);

        // Assert
        history.Should().HaveCount(3);
    }

    #endregion

    #region GetRollsByContext Tests

    [Test]
    public void GetRollsByContext_FiltersCorrectly()
    {
        // Arrange
        var pool = new DicePool(3, DiceType.D10);
        var rolls = new[] { 8, 5, 3 };

        // Log rolls with different contexts
        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(pool, rolls), 1, RollContexts.CombatAttack));
        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(pool, rolls), 2, RollContexts.CombatDamage));
        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(pool, rolls), 3, RollContexts.Skill("Stealth")));
        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(pool, rolls), 4, RollContexts.CombatAttack));

        // Act
        var combatRolls = _logger.GetRollsByContext(RollContexts.CombatPrefix);
        var attackRolls = _logger.GetRollsByContext(RollContexts.CombatAttack);
        var skillRolls = _logger.GetRollsByContext(RollContexts.SkillPrefix);

        // Assert
        combatRolls.Should().HaveCount(3);  // Attack, Damage, Attack
        attackRolls.Should().HaveCount(2);  // Just attacks
        skillRolls.Should().HaveCount(1);   // Just Stealth
    }

    [Test]
    public void GetRollsByContext_EmptyPrefix_ReturnsAllRolls()
    {
        // Arrange
        LogSampleRoll(context: RollContexts.CombatAttack);
        LogSampleRoll(context: RollContexts.Skill("Stealth"));

        // Act
        var allRolls = _logger.GetRollsByContext("");

        // Assert
        allRolls.Should().HaveCount(2);
    }

    #endregion

    #region Fumble and Critical Flag Tests

    [Test]
    public void LogRoll_StoresFumbleAndCriticalFlagsCorrectly()
    {
        // Arrange: Fumble roll (0 successes, 1+ botch)
        var pool = new DicePool(3, DiceType.D10);
        var fumbleRolls = new[] { 1, 4, 2 };  // 0S, 1B = fumble
        var fumbleResult = new DiceRollResult(pool, fumbleRolls);

        // Critical roll (5+ net successes)
        var criticalRolls = new[] { 8, 9, 10, 8, 9, 10 };  // 6S, 0B = critical
        var criticalPool = new DicePool(6, DiceType.D10);
        var criticalResult = new DiceRollResult(criticalPool, criticalRolls);

        // Act
        _logger.LogRoll(DiceRollLog.FromRollResult(fumbleResult, 1, "Test:Fumble"));
        _logger.LogRoll(DiceRollLog.FromRollResult(criticalResult, 2, "Test:Critical"));

        // Assert
        var fumbles = _logger.GetFumbles();
        var criticals = _logger.GetCriticalSuccesses();

        fumbles.Should().HaveCount(1);
        fumbles[0].IsFumble.Should().BeTrue();
        fumbles[0].IsCriticalSuccess.Should().BeFalse();

        criticals.Should().HaveCount(1);
        criticals[0].IsCriticalSuccess.Should().BeTrue();
        criticals[0].IsFumble.Should().BeFalse();
    }

    #endregion

    #region History Trimming Tests

    [Test]
    public void LogRoll_TrimsOldestWhenOverCapacity()
    {
        // Arrange: Create logger with small capacity
        var smallLogger = new InMemoryDiceRollLogger(_mockLogger.Object, maxHistorySize: 5);
        var pool = new DicePool(3, DiceType.D10);
        var rolls = new[] { 5, 6, 7 };

        // Act - Log 10 rolls (5 over capacity)
        for (int i = 0; i < 10; i++)
        {
            var diceResult = new DiceRollResult(pool, rolls);
            var rollLog = DiceRollLog.FromRollResult(diceResult, seed: i, context: $"Test:{i}");
            smallLogger.LogRoll(rollLog);
        }

        // Assert
        smallLogger.HistoryCount.Should().Be(5);
        smallLogger.MaxHistorySize.Should().Be(5);

        var history = smallLogger.GetRollHistory(10);
        history.Should().HaveCount(5);
        // Should have rolls 5-9 (oldest 0-4 were trimmed)
        history[0].Seed.Should().Be(5);
        history[4].Seed.Should().Be(9);
    }

    #endregion

    #region ClearHistory Tests

    [Test]
    public void ClearHistory_RemovesAllRolls()
    {
        // Arrange
        LogSampleRoll();
        LogSampleRoll();
        LogSampleRoll();
        _logger.HistoryCount.Should().Be(3);

        // Act
        _logger.ClearHistory();

        // Assert
        _logger.HistoryCount.Should().Be(0);
        _logger.GetRollHistory(100).Should().BeEmpty();
    }

    #endregion

    #region GetStatistics Tests

    [Test]
    public void GetStatistics_ReturnsCorrectAggregates()
    {
        // Arrange - Log a fumble and a critical
        var pool = new DicePool(3, DiceType.D10);
        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(pool, new[] { 1, 4, 2 }), 1, "Test")); // Fumble

        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(new DicePool(6, DiceType.D10),
                new[] { 8, 9, 10, 8, 9, 10 }), 2, "Test")); // Critical (6 net)

        _logger.LogRoll(DiceRollLog.FromRollResult(
            new DiceRollResult(pool, new[] { 8, 5, 4 }), 3, "Test")); // 1 net

        // Act
        var stats = _logger.GetStatistics();

        // Assert
        stats.TotalRolls.Should().Be(3);
        stats.TotalFumbles.Should().Be(1);
        stats.TotalCriticals.Should().Be(1);
        stats.FumbleRate.Should().BeApproximately(1.0 / 3.0, 0.01);
        stats.CriticalRate.Should().BeApproximately(1.0 / 3.0, 0.01);
        stats.AverageNetSuccesses.Should().BeApproximately((0 + 6 + 1) / 3.0, 0.01);
    }

    [Test]
    public void GetStatistics_EmptyHistory_ReturnsZeros()
    {
        // Act
        var stats = _logger.GetStatistics();

        // Assert
        stats.TotalRolls.Should().Be(0);
        stats.TotalFumbles.Should().Be(0);
        stats.TotalCriticals.Should().Be(0);
        stats.AverageNetSuccesses.Should().Be(0);
        stats.FumbleRate.Should().Be(0);
        stats.CriticalRate.Should().Be(0);
    }

    #endregion

    #region GetRollsByActor Tests

    [Test]
    public void GetRollsByActor_FiltersCorrectly()
    {
        // Arrange
        var actor1 = Guid.NewGuid();
        var actor2 = Guid.NewGuid();

        LogSampleRoll(actorId: actor1);
        LogSampleRoll(actorId: actor1);
        LogSampleRoll(actorId: actor2);

        // Act
        var actor1Rolls = _logger.GetRollsByActor(actor1);
        var actor2Rolls = _logger.GetRollsByActor(actor2);

        // Assert
        actor1Rolls.Should().HaveCount(2);
        actor2Rolls.Should().HaveCount(1);
    }

    #endregion

    #region Helper Methods

    private void LogSampleRoll(
        int seed = 1,
        string context = RollContexts.Default,
        Guid? actorId = null)
    {
        var pool = new DicePool(3, DiceType.D10);
        var rolls = new[] { 8, 5, 3 };
        var diceResult = new DiceRollResult(pool, rolls);
        var rollLog = DiceRollLog.FromRollResult(diceResult, seed, context, actorId);
        _logger.LogRoll(rollLog);
    }

    #endregion
}
