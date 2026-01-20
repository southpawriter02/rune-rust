using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the PuzzleAttempt value object.
/// </summary>
[TestFixture]
public class PuzzleAttemptTests
{
    [Test]
    public void Create_WithValidPuzzleId_ReturnsActiveAttempt()
    {
        // Arrange
        var puzzleId = Guid.NewGuid();

        // Act
        var attempt = PuzzleAttempt.Create(puzzleId);

        // Assert
        attempt.PuzzleId.Should().Be(puzzleId);
        attempt.IsActive.Should().BeTrue();
        attempt.Succeeded.Should().BeNull();
        attempt.CompletedSteps.Should().BeEmpty();
        attempt.CurrentInput.Should().BeEmpty();
    }

    [Test]
    public void AddStep_AddsToCompletedSteps()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());

        // Act
        attempt.AddStep("step-1");
        attempt.AddStep("step-2");

        // Assert
        attempt.CompletedSteps.Should().HaveCount(2);
        attempt.CompletedSteps.Should().ContainInOrder("step-1", "step-2");
        attempt.StepCount.Should().Be(2);
    }

    [Test]
    public void SetInput_SetsCurrentInput()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());

        // Act
        attempt.SetInput("1234");

        // Assert
        attempt.CurrentInput.Should().Be("1234");
        attempt.InputLength.Should().Be(4);
    }

    [Test]
    public void AppendInput_AppendsToCurrentInput()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());
        attempt.SetInput("12");

        // Act
        attempt.AppendInput("34");

        // Assert
        attempt.CurrentInput.Should().Be("1234");
    }

    [Test]
    public void ClearInput_ClearsCurrentInput()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());
        attempt.SetInput("1234");

        // Act
        attempt.ClearInput();

        // Assert
        attempt.CurrentInput.Should().BeEmpty();
    }

    [Test]
    public void Complete_MarksAttemptAsInactive()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());

        // Act
        attempt.Complete(succeeded: true);

        // Assert
        attempt.IsActive.Should().BeFalse();
        attempt.Succeeded.Should().BeTrue();
    }

    [Test]
    public void Complete_WithFailed_SetsSucceededFalse()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());

        // Act
        attempt.Complete(succeeded: false);

        // Assert
        attempt.IsActive.Should().BeFalse();
        attempt.Succeeded.Should().BeFalse();
    }

    [Test]
    public void Reset_ClearsAllProgress()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());
        attempt.AddStep("step-1");
        attempt.SetInput("1234");
        attempt.Complete(false);

        // Act
        attempt.Reset();

        // Assert
        attempt.IsActive.Should().BeTrue();
        attempt.Succeeded.Should().BeNull();
        attempt.CompletedSteps.Should().BeEmpty();
        attempt.CurrentInput.Should().BeEmpty();
    }

    [Test]
    public void HasProgress_WhenHasSteps_ReturnsTrue()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());
        attempt.AddStep("step-1");

        // Assert
        attempt.HasProgress.Should().BeTrue();
    }

    [Test]
    public void HasProgress_WhenHasInput_ReturnsTrue()
    {
        // Arrange
        var attempt = PuzzleAttempt.Create(Guid.NewGuid());
        attempt.SetInput("1");

        // Assert
        attempt.HasProgress.Should().BeTrue();
    }
}
