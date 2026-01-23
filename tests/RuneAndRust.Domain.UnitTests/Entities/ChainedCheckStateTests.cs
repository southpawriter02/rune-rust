using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="ChainedCheckState"/> entity.
/// </summary>
[TestFixture]
public class ChainedCheckStateTests
{
    #region Factory Method Tests

    [Test]
    public void Create_WithValidParameters_CreatesStateWithCorrectProperties()
    {
        // Arrange
        var steps = new[]
        {
            ChainedCheckStep.Create("step-1", "Access", "system-bypass", 3),
            ChainedCheckStep.Create("step-2", "Authentication", "system-bypass", 4, retries: 1)
        };

        // Act
        var state = ChainedCheckState.Create(
            "test-chain-001",
            "player-001",
            "Terminal Hacking",
            steps,
            "terminal-001");

        // Assert
        state.CheckId.Should().Be("test-chain-001");
        state.CharacterId.Should().Be("player-001");
        state.ChainName.Should().Be("Terminal Hacking");
        state.Steps.Should().HaveCount(2);
        state.CurrentStepIndex.Should().Be(0);
        state.Status.Should().Be(ChainedCheckStatus.NotStarted);
        state.TargetId.Should().Be("terminal-001");
        state.IsComplete.Should().BeFalse();
    }

    [Test]
    public void Create_WithEmptyCheckId_ThrowsArgumentException()
    {
        // Arrange
        var steps = new[] { ChainedCheckStep.Create("step-1", "Test", "skill", 3) };

        // Act
        var act = () => ChainedCheckState.Create("", "player", "Chain", steps);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("checkId");
    }

    [Test]
    public void Create_WithEmptySteps_ThrowsArgumentException()
    {
        // Arrange
        var steps = Array.Empty<ChainedCheckStep>();

        // Act
        var act = () => ChainedCheckState.Create("check-1", "player", "Chain", steps);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("steps");
    }

    #endregion

    #region Step Processing Tests

    [Test]
    public void RecordStepResult_OnSuccess_AdvancesToNextStep()
    {
        // Arrange
        var steps = new[]
        {
            ChainedCheckStep.Create("step-1", "First", "skill", 3),
            ChainedCheckStep.Create("step-2", "Second", "skill", 3)
        };
        var state = ChainedCheckState.Create("check-1", "player", "Chain", steps);

        var successResult = CreateSuccessfulResult();

        // Act
        state.RecordStepResult(successResult, wasRetry: false);

        // Assert
        state.CurrentStepIndex.Should().Be(1);
        state.Status.Should().Be(ChainedCheckStatus.InProgress);
        state.StepResults.Should().HaveCount(1);
        state.SuccessfulSteps.Should().Be(1);
    }

    [Test]
    public void RecordStepResult_OnFinalStepSuccess_MarksChainSucceeded()
    {
        // Arrange
        var steps = new[] { ChainedCheckStep.Create("step-1", "Only Step", "skill", 3) };
        var state = ChainedCheckState.Create("check-1", "player", "Chain", steps);

        var successResult = CreateSuccessfulResult();

        // Act
        state.RecordStepResult(successResult, wasRetry: false);

        // Assert
        state.CurrentStepIndex.Should().Be(1);
        state.Status.Should().Be(ChainedCheckStatus.Succeeded);
        state.IsComplete.Should().BeTrue();
        state.CompletedAt.Should().NotBeNull();
    }

    [Test]
    public void RecordStepResult_OnFailureWithRetries_SetsAwaitingRetry()
    {
        // Arrange
        var steps = new[] { ChainedCheckStep.Create("step-1", "Retryable", "skill", 3, retries: 2) };
        var state = ChainedCheckState.Create("check-1", "player", "Chain", steps);

        var failureResult = CreateFailureResult();

        // Act
        state.RecordStepResult(failureResult, wasRetry: false);

        // Assert
        state.Status.Should().Be(ChainedCheckStatus.AwaitingRetry);
        state.RetriesRemaining[0].Should().Be(1); // Started with 2, now 1
        state.CanRetry().Should().BeTrue();
    }

    [Test]
    public void RecordStepResult_OnFailureNoRetries_MarksChainFailed()
    {
        // Arrange
        var steps = new[] { ChainedCheckStep.Create("step-1", "No Retry", "skill", 3, retries: 0) };
        var state = ChainedCheckState.Create("check-1", "player", "Chain", steps);

        var failureResult = CreateFailureResult();

        // Act
        state.RecordStepResult(failureResult, wasRetry: false);

        // Assert
        state.Status.Should().Be(ChainedCheckStatus.Failed);
        state.IsComplete.Should().BeTrue();
        state.CompletedAt.Should().NotBeNull();
    }

    #endregion

    #region Abandon Tests

    [Test]
    public void Abandon_WhenInProgress_MarksChainFailed()
    {
        // Arrange
        var steps = new[]
        {
            ChainedCheckStep.Create("step-1", "First", "skill", 3),
            ChainedCheckStep.Create("step-2", "Second", "skill", 3)
        };
        var state = ChainedCheckState.Create("check-1", "player", "Chain", steps);
        state.RecordStepResult(CreateSuccessfulResult(), wasRetry: false);

        // Act
        state.Abandon();

        // Assert
        state.Status.Should().Be(ChainedCheckStatus.Failed);
        state.IsComplete.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static SkillCheckResult CreateSuccessfulResult()
    {
        // Create a dice result with 4 successes, 0 botches for a clear success
        var pool = new DicePool(5, DiceType.D10);
        var diceResult = new DiceRollResult(pool, new[] { 8, 9, 10, 8, 5 });

        return new SkillCheckResult(
            skillId: "test-skill",
            skillName: "Test Skill",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 3,
            difficultyName: "Moderate");
    }

    private static SkillCheckResult CreateFailureResult()
    {
        // Create a dice result with 1 success, 1 botch for a failure (0 net vs DC 3)
        var pool = new DicePool(3, DiceType.D10);
        var diceResult = new DiceRollResult(pool, new[] { 8, 1, 5 });

        return new SkillCheckResult(
            skillId: "test-skill",
            skillName: "Test Skill",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 3,
            difficultyName: "Moderate");
    }

    #endregion
}
