using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for the Puzzle entity.
/// </summary>
[TestFixture]
public class PuzzleTests
{
    [Test]
    public void Create_WithValidParameters_CreatesPuzzle()
    {
        // Act
        var puzzle = Puzzle.Create(
            "test-puzzle",
            "Test Puzzle",
            "A test puzzle.",
            PuzzleType.Sequence);

        // Assert
        puzzle.Id.Should().NotBeEmpty();
        puzzle.DefinitionId.Should().Be("test-puzzle");
        puzzle.Name.Should().Be("Test Puzzle");
        puzzle.Type.Should().Be(PuzzleType.Sequence);
        puzzle.State.Should().Be(PuzzleState.Unsolved);
        puzzle.IsSolvable.Should().BeTrue();
    }

    [Test]
    public void Create_NormalizesDefinitionId()
    {
        // Act
        var puzzle = Puzzle.Create("TEST-PUZZLE", "Test", "Test", PuzzleType.Logic);

        // Assert
        puzzle.DefinitionId.Should().Be("test-puzzle");
    }

    [Test]
    public void Create_ClampsDifficulty()
    {
        // Act
        var lowDifficulty = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic, difficulty: 0);
        var highDifficulty = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic, difficulty: 10);

        // Assert
        lowDifficulty.Difficulty.Should().Be(1);
        highDifficulty.Difficulty.Should().Be(5);
    }

    [Test]
    public void BeginAttempt_WhenUnsolved_TransitionsToInProgress()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);

        // Act
        var result = puzzle.BeginAttempt();

        // Assert
        result.Should().BeTrue();
        puzzle.State.Should().Be(PuzzleState.InProgress);
    }

    [Test]
    public void BeginAttempt_WhenSolved_ReturnsFalse()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);
        puzzle.Solve();

        // Act
        var result = puzzle.BeginAttempt();

        // Assert
        result.Should().BeFalse();
        puzzle.State.Should().Be(PuzzleState.Solved);
    }

    [Test]
    public void RecordFailedAttempt_IncrementsAttemptCount()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);

        // Act
        puzzle.RecordFailedAttempt();

        // Assert
        puzzle.AttemptCount.Should().Be(1);
    }

    [Test]
    public void RecordFailedAttempt_WhenMaxReached_TransitionsToFailed()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic, maxAttempts: 2);
        puzzle.RecordFailedAttempt();

        // Act
        puzzle.RecordFailedAttempt();

        // Assert
        puzzle.State.Should().Be(PuzzleState.Failed);
        puzzle.IsFailed.Should().BeTrue();
    }

    [Test]
    public void RecordFailedAttempt_WithResetDelay_SetsTurnsUntilReset()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic,
            maxAttempts: 1, canReset: true, resetDelay: 5);

        // Act
        puzzle.RecordFailedAttempt();

        // Assert
        puzzle.TurnsUntilReset.Should().Be(5);
    }

    [Test]
    public void Solve_TransitionsToSolved()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);

        // Act
        puzzle.Solve();

        // Assert
        puzzle.State.Should().Be(PuzzleState.Solved);
        puzzle.IsSolved.Should().BeTrue();
        puzzle.IsSolvable.Should().BeFalse();
    }

    [Test]
    public void Reset_WhenCanReset_TransitionsToUnsolved()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic, maxAttempts: 1);
        puzzle.RecordFailedAttempt();

        // Act
        var result = puzzle.Reset();

        // Assert
        result.Should().BeTrue();
        puzzle.State.Should().Be(PuzzleState.Unsolved);
        puzzle.AttemptCount.Should().Be(0);
    }

    [Test]
    public void Reset_WhenSolved_ReturnsFalse()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);
        puzzle.Solve();

        // Act
        var result = puzzle.Reset();

        // Assert
        result.Should().BeFalse();
        puzzle.State.Should().Be(PuzzleState.Solved);
    }

    [Test]
    public void TickReset_DecrementsAndResetsWhenZero()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic,
            maxAttempts: 1, canReset: true, resetDelay: 2);
        puzzle.RecordFailedAttempt();

        // Act - First tick
        puzzle.TickReset();
        var afterFirstTick = puzzle.TurnsUntilReset;

        // Second tick - should reset
        var resetOccurred = puzzle.TickReset();

        // Assert
        afterFirstTick.Should().Be(1);
        resetOccurred.Should().BeTrue();
        puzzle.State.Should().Be(PuzzleState.Unsolved);
    }

    [Test]
    public void Lock_WhenUnsolved_TransitionsToLocked()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);

        // Act
        puzzle.Lock();

        // Assert
        puzzle.State.Should().Be(PuzzleState.Locked);
        puzzle.IsLocked.Should().BeTrue();
    }

    [Test]
    public void Unlock_WhenLocked_TransitionsToUnsolved()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);
        puzzle.Lock();

        // Act
        puzzle.Unlock();

        // Assert
        puzzle.State.Should().Be(PuzzleState.Unsolved);
    }

    [Test]
    public void MatchesKeyword_FindsByExplicitKeyword()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test Puzzle", "Test", PuzzleType.Logic,
            keywords: new[] { "altar", "stone" });

        // Act & Assert
        puzzle.MatchesKeyword("altar").Should().BeTrue();
        puzzle.MatchesKeyword("stone").Should().BeTrue();
        puzzle.MatchesKeyword("fire").Should().BeFalse();
    }

    [Test]
    public void MatchesKeyword_FindsByName()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Stone Altar", "Test", PuzzleType.Logic);

        // Act & Assert
        puzzle.MatchesKeyword("Stone").Should().BeTrue();
        puzzle.MatchesKeyword("Altar").Should().BeTrue();
    }

    [Test]
    public void MatchesKeyword_IsCaseInsensitive()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test Puzzle", "Test", PuzzleType.Logic,
            keywords: new[] { "altar" });

        // Act & Assert
        puzzle.MatchesKeyword("ALTAR").Should().BeTrue();
        puzzle.MatchesKeyword("altar").Should().BeTrue();
        puzzle.MatchesKeyword("Altar").Should().BeTrue();
    }

    [Test]
    public void HasAttemptsRemaining_WithUnlimited_ReturnsTrue()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic, maxAttempts: -1);
        puzzle.RecordFailedAttempt();
        puzzle.RecordFailedAttempt();

        // Assert
        puzzle.HasAttemptsRemaining.Should().BeTrue();
    }

    [Test]
    public void HasAttemptsRemaining_WhenExhausted_ReturnsFalse()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic, maxAttempts: 2);
        puzzle.RecordFailedAttempt();
        puzzle.RecordFailedAttempt();

        // Assert
        puzzle.HasAttemptsRemaining.Should().BeFalse();
    }
}
