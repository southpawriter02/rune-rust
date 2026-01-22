using FluentAssertions;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for ExtendedCheckState entity.
/// </summary>
/// <remarks>
/// v0.15.0d: Tests extended check mechanics including success accumulation,
/// fumble penalties, catastrophic failure, and state transitions.
/// </remarks>
[TestFixture]
public class ExtendedCheckStateTests
{
    #region Factory Method Tests

    [Test]
    public void Create_WithValidParameters_CreatesInProgressState()
    {
        // Arrange & Act
        var state = ExtendedCheckState.Create(
            "test-123",
            "player-1",
            "lockpick",
            targetSuccesses: 6,
            maxRounds: 5);

        // Assert
        state.CheckId.Should().Be("test-123");
        state.CharacterId.Should().Be("player-1");
        state.SkillId.Should().Be("lockpick");
        state.TargetSuccesses.Should().Be(6);
        state.MaxRounds.Should().Be(5);
        state.RoundsRemaining.Should().Be(5);
        state.AccumulatedSuccesses.Should().Be(0);
        state.ConsecutiveFumbles.Should().Be(0);
        state.Status.Should().Be(ExtendedCheckStatus.InProgress);
        state.IsActive.Should().BeTrue();
        state.IsComplete.Should().BeFalse();
    }

    [Test]
    public void Create_WithInvalidTarget_ThrowsException()
    {
        // Act
        var act = () => ExtendedCheckState.Create(
            "test", "player", "skill",
            targetSuccesses: 0, maxRounds: 5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetSuccesses");
    }

    [Test]
    public void Create_WithInvalidMaxRounds_ThrowsException()
    {
        // Act
        var act = () => ExtendedCheckState.Create(
            "test", "player", "skill",
            targetSuccesses: 5, maxRounds: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxRounds");
    }

    #endregion

    #region ProcessRound Success Tests

    [Test]
    public void ProcessRound_AccumulatesNetSuccesses()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 6, maxRounds: 5);
        var roll1 = CreateRollResult(netSuccesses: 2, isFumble: false);
        var roll2 = CreateRollResult(netSuccesses: 3, isFumble: false);

        // Act
        state.ProcessRound(roll1);
        state.ProcessRound(roll2);

        // Assert
        state.AccumulatedSuccesses.Should().Be(5); // 2 + 3
        state.RoundsRemaining.Should().Be(3); // 5 - 2
        state.RoundsCompleted.Should().Be(2);
        state.Status.Should().Be(ExtendedCheckStatus.InProgress);
    }

    [Test]
    public void ProcessRound_SucceedsWhenTargetReached()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 4, maxRounds: 5);
        var roll1 = CreateRollResult(netSuccesses: 2, isFumble: false);
        var roll2 = CreateRollResult(netSuccesses: 3, isFumble: false); // Total: 5 >= 4

        // Act
        state.ProcessRound(roll1);
        state.ProcessRound(roll2);

        // Assert
        state.Status.Should().Be(ExtendedCheckStatus.Succeeded);
        state.AccumulatedSuccesses.Should().Be(5);
        state.IsComplete.Should().BeTrue();
        state.IsActive.Should().BeFalse();
        state.CompletedAt.Should().NotBeNull();
    }

    [Test]
    public void ProcessRound_FailsWhenRoundsExhausted()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 10, maxRounds: 2);
        var roll = CreateRollResult(netSuccesses: 1, isFumble: false);

        // Act
        state.ProcessRound(roll);
        state.ProcessRound(roll);

        // Assert
        state.Status.Should().Be(ExtendedCheckStatus.Failed);
        state.AccumulatedSuccesses.Should().Be(2); // 1 + 1; didn't reach 10
        state.RoundsRemaining.Should().Be(0);
    }

    #endregion

    #region ProcessRound Fumble Tests

    [Test]
    public void ProcessRound_FumbleReducesAccumulated()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 6, maxRounds: 5);
        var roll1 = CreateRollResult(netSuccesses: 3, isFumble: false);
        var fumble = CreateRollResult(netSuccesses: 0, isFumble: true);

        // Act
        state.ProcessRound(roll1); // Accumulated: 3
        state.ProcessRound(fumble); // Accumulated: 3 - 2 = 1

        // Assert
        state.AccumulatedSuccesses.Should().Be(1);
        state.ConsecutiveFumbles.Should().Be(1);
        state.TotalFumbles.Should().Be(1);
        state.Status.Should().Be(ExtendedCheckStatus.InProgress);
    }

    [Test]
    public void ProcessRound_FumblePenaltyFlooredAtZero()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 6, maxRounds: 5);
        var roll1 = CreateRollResult(netSuccesses: 1, isFumble: false);
        var fumble = CreateRollResult(netSuccesses: 0, isFumble: true);

        // Act
        state.ProcessRound(roll1); // Accumulated: 1
        state.ProcessRound(fumble); // Accumulated: max(0, 1 - 2) = 0

        // Assert
        state.AccumulatedSuccesses.Should().Be(0);
    }

    [Test]
    public void ProcessRound_ThreeConsecutiveFumbles_CausesCatastrophicFailure()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 6, maxRounds: 10);
        var fumble = CreateRollResult(netSuccesses: 0, isFumble: true);

        // Act
        state.ProcessRound(fumble); // ConsecutiveFumbles: 1
        state.ProcessRound(fumble); // ConsecutiveFumbles: 2
        state.ProcessRound(fumble); // ConsecutiveFumbles: 3 -> Catastrophic!

        // Assert
        state.Status.Should().Be(ExtendedCheckStatus.CatastrophicFailure);
        state.ConsecutiveFumbles.Should().Be(3);
        state.TotalFumbles.Should().Be(3);
        state.IsComplete.Should().BeTrue();
    }

    [Test]
    public void ProcessRound_NonFumbleResetsConsecutiveFumbles()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 10, maxRounds: 10);
        var fumble = CreateRollResult(netSuccesses: 0, isFumble: true);
        var success = CreateRollResult(netSuccesses: 2, isFumble: false);

        // Act
        state.ProcessRound(fumble); // ConsecutiveFumbles: 1
        state.ProcessRound(fumble); // ConsecutiveFumbles: 2
        state.ConsecutiveFumbles.Should().Be(2);

        state.ProcessRound(success); // ConsecutiveFumbles: 0

        // Assert
        state.ConsecutiveFumbles.Should().Be(0);
        state.TotalFumbles.Should().Be(2); // Total doesn't reset
        state.Status.Should().Be(ExtendedCheckStatus.InProgress);
    }

    [Test]
    public void IsAtRisk_TrueWhenTwoConsecutiveFumbles()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 10, maxRounds: 10);
        var fumble = CreateRollResult(netSuccesses: 0, isFumble: true);

        // Act
        state.ProcessRound(fumble);
        state.ProcessRound(fumble);

        // Assert
        state.IsAtRisk.Should().BeTrue();
        state.ConsecutiveFumbles.Should().Be(ExtendedCheckConstants.CatastrophicFumbleThreshold - 1);
    }

    #endregion

    #region Abandon Tests

    [Test]
    public void Abandon_SetsAbandonedStatus()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 6, maxRounds: 5);
        var roll = CreateRollResult(netSuccesses: 2, isFumble: false);
        state.ProcessRound(roll);

        // Act
        state.Abandon();

        // Assert
        state.Status.Should().Be(ExtendedCheckStatus.Abandoned);
        state.IsComplete.Should().BeTrue();
        state.CompletedAt.Should().NotBeNull();
        state.AccumulatedSuccesses.Should().Be(2); // Preserved
    }

    [Test]
    public void Abandon_WhenNotInProgress_ThrowsException()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 2, maxRounds: 5);
        state.ProcessRound(CreateRollResult(netSuccesses: 3, isFumble: false));
        state.Status.Should().Be(ExtendedCheckStatus.Succeeded);

        // Act
        var act = () => state.Abandon();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Progress Tests

    [Test]
    public void Progress_CalculatesCorrectly()
    {
        // Arrange
        var state = ExtendedCheckState.Create("test", "player", "lockpick",
            targetSuccesses: 10, maxRounds: 5);
        var roll = CreateRollResult(netSuccesses: 3, isFumble: false);

        // Act
        state.ProcessRound(roll);

        // Assert
        state.Progress.Should().BeApproximately(0.3, 0.001); // 3/10 = 0.3
        state.SuccessesNeeded.Should().Be(7); // 10 - 3 = 7
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var state = ExtendedCheckState.Create("abc123", "player", "lockpick",
            targetSuccesses: 6, maxRounds: 5);
        var roll = CreateRollResult(netSuccesses: 2, isFumble: false);
        state.ProcessRound(roll);

        // Act
        var str = state.ToString();

        // Assert
        str.Should().Contain("abc123");
        str.Should().Contain("2/6");
        str.Should().Contain("4/5");
        str.Should().Contain("InProgress");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock DiceRollResult for testing.
    /// </summary>
    private static DiceRollResult CreateRollResult(int netSuccesses, bool isFumble)
    {
        // Create dice values that produce the desired result
        int[] values;
        if (isFumble)
        {
            // Fumble: 0 successes, at least 1 botch
            values = new[] { 1 }; // Single 1 = fumble
        }
        else if (netSuccesses == 0)
        {
            // 0 net but not fumble
            values = new[] { 5 }; // No success, no botch
        }
        else
        {
            // Create dice showing successes (8+)
            values = Enumerable.Range(0, netSuccesses).Select(_ => 8).ToArray();
        }

        var pool = new DicePool(values.Length, DiceType.D10);
        return new DiceRollResult(pool, values);
    }

    #endregion
}
