using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Puzzle type-specific data properties and setters (v0.4.2b).
/// </summary>
[TestFixture]
public class PuzzleTypeDataTests
{
    [Test]
    public void SetSequenceData_OnSequencePuzzle_SetsData()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Sequence);
        var sequenceData = SequencePuzzle.Create(["fire", "water"]);

        // Act
        puzzle.SetSequenceData(sequenceData);

        // Assert
        puzzle.SequenceData.Should().Be(sequenceData);
    }

    [Test]
    public void SetSequenceData_OnNonSequencePuzzle_ThrowsException()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Combination);
        var sequenceData = SequencePuzzle.Create(["fire"]);

        // Act
        var act = () => puzzle.SetSequenceData(sequenceData);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void SetCombinationData_OnCombinationPuzzle_SetsData()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Combination);
        var combinationData = CombinationPuzzle.Create("1234");

        // Act
        puzzle.SetCombinationData(combinationData);

        // Assert
        puzzle.CombinationData.Should().Be(combinationData);
    }

    [Test]
    public void SetCombinationData_OnNonCombinationPuzzle_ThrowsException()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Pattern);
        var combinationData = CombinationPuzzle.Create("1234");

        // Act
        var act = () => puzzle.SetCombinationData(combinationData);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void SetPatternData_OnPatternPuzzle_SetsData()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Pattern);
        var patternData = PatternPuzzle.Create("XOXXOX", 3, 2);

        // Act
        puzzle.SetPatternData(patternData);

        // Assert
        puzzle.PatternData.Should().Be(patternData);
    }

    [Test]
    public void SetPatternData_OnNonPatternPuzzle_ThrowsException()
    {
        // Arrange
        var puzzle = Puzzle.Create("test", "Test", "Test", PuzzleType.Sequence);
        var patternData = PatternPuzzle.Create("XOXXOX", 3, 2);

        // Act
        var act = () => puzzle.SetPatternData(patternData);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
