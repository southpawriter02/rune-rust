using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room puzzle collection methods.
/// </summary>
[TestFixture]
public class RoomPuzzleTests
{
    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _room = new Room("Test Room", "A test room.", Position3D.Origin);
    }

    [Test]
    public void AddPuzzle_AddsPuzzleToCollection()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test Puzzle", "Test", PuzzleType.Logic);

        // Act
        _room.AddPuzzle(puzzle);

        // Assert
        _room.Puzzles.Should().HaveCount(1);
        _room.Puzzles.Should().Contain(puzzle);
        _room.HasPuzzles.Should().BeTrue();
    }

    [Test]
    public void RemovePuzzle_RemovesPuzzleFromCollection()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test Puzzle", "Test", PuzzleType.Logic);
        _room.AddPuzzle(puzzle);

        // Act
        var removed = _room.RemovePuzzle(puzzle);

        // Assert
        removed.Should().BeTrue();
        _room.Puzzles.Should().BeEmpty();
        _room.HasPuzzles.Should().BeFalse();
    }

    [Test]
    public void GetPuzzleByKeyword_FindsMatchingPuzzle()
    {
        // Arrange
        var puzzle = Puzzle.Create("stone-altar", "Stone Altar", "Test", PuzzleType.Sequence,
            keywords: new[] { "altar", "stone" });
        _room.AddPuzzle(puzzle);

        // Act
        var found = _room.GetPuzzleByKeyword("altar");

        // Assert
        found.Should().Be(puzzle);
    }

    [Test]
    public void GetUnsolvedPuzzles_ReturnsOnlySolvable()
    {
        // Arrange
        var unsolved = Puzzle.Create("unsolved", "Unsolved", "Test", PuzzleType.Logic);
        var solved = Puzzle.Create("solved", "Solved", "Test", PuzzleType.Logic);
        solved.Solve();

        _room.AddPuzzle(unsolved);
        _room.AddPuzzle(solved);

        // Act
        var result = _room.GetUnsolvedPuzzles().ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(unsolved);
        result.Should().NotContain(solved);
    }

    [Test]
    public void HasUnsolvedPuzzles_WhenHasSolvable_ReturnsTrue()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);
        _room.AddPuzzle(puzzle);

        // Assert
        _room.HasUnsolvedPuzzles.Should().BeTrue();
    }

    [Test]
    public void HasUnsolvedPuzzles_WhenAllSolved_ReturnsFalse()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic);
        puzzle.Solve();
        _room.AddPuzzle(puzzle);

        // Assert
        _room.HasUnsolvedPuzzles.Should().BeFalse();
    }

    [Test]
    public void GetPuzzlesByType_FiltersCorrectly()
    {
        // Arrange
        var sequence = Puzzle.Create("seq", "Sequence", "Test", PuzzleType.Sequence);
        var logic = Puzzle.Create("logic", "Logic", "Test", PuzzleType.Logic);
        _room.AddPuzzle(sequence);
        _room.AddPuzzle(logic);

        // Act
        var result = _room.GetPuzzlesByType(PuzzleType.Sequence).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(sequence);
    }

    [Test]
    public void ProcessPuzzleResetTicks_ResetsExpiredPuzzles()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Logic,
            maxAttempts: 1, canReset: true, resetDelay: 1);
        puzzle.RecordFailedAttempt(); // Fails and sets TurnsUntilReset = 1
        _room.AddPuzzle(puzzle);

        // Act
        var resetCount = _room.ProcessPuzzleResetTicks();

        // Assert
        resetCount.Should().Be(1);
        puzzle.State.Should().Be(PuzzleState.Unsolved);
    }
}
