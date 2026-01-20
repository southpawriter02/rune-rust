using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the MultiPartPuzzle value object.
/// </summary>
[TestFixture]
public class MultiPartPuzzleTests
{
    [Test]
    public void Create_WithValidParameters_CreatesInstance()
    {
        // Act
        var puzzle = MultiPartPuzzle.Create("master", ["comp1", "comp2", "comp3"]);

        // Assert
        puzzle.MasterPuzzleId.Should().Be("master");
        puzzle.TotalComponents.Should().Be(3);
        puzzle.SolvedCount.Should().Be(0);
        puzzle.IsComplete.Should().BeFalse();
    }

    [Test]
    public void RecordComponentSolved_ValidComponent_ReturnsTrue()
    {
        // Arrange
        var puzzle = MultiPartPuzzle.Create("master", ["comp1", "comp2"]);

        // Act
        var result = puzzle.RecordComponentSolved("comp1");

        // Assert
        result.Should().BeTrue();
        puzzle.SolvedCount.Should().Be(1);
    }

    [Test]
    public void RecordComponentSolved_InvalidComponent_ReturnsFalse()
    {
        // Arrange
        var puzzle = MultiPartPuzzle.Create("master", ["comp1", "comp2"]);

        // Act
        var result = puzzle.RecordComponentSolved("invalid");

        // Assert
        result.Should().BeFalse();
        puzzle.SolvedCount.Should().Be(0);
    }

    [Test]
    public void RecordComponentSolved_WithOrder_EnforcesSequence()
    {
        // Arrange
        var puzzle = MultiPartPuzzle.Create("master", ["comp1", "comp2"],
            requiresOrder: true, requiredOrder: ["comp1", "comp2"]);

        // Act - try to solve out of order
        var result = puzzle.RecordComponentSolved("comp2");

        // Assert
        result.Should().BeFalse();
        puzzle.GetNextExpectedComponent().Should().Be("comp1");
    }

    [Test]
    public void IsComplete_WhenAllSolved_ReturnsTrue()
    {
        // Arrange
        var puzzle = MultiPartPuzzle.Create("master", ["comp1", "comp2"]);
        puzzle.RecordComponentSolved("comp1");
        puzzle.RecordComponentSolved("comp2");

        // Act & Assert
        puzzle.IsComplete.Should().BeTrue();
        puzzle.CompletionPercent.Should().Be(100);
    }

    [Test]
    public void GetRemainingComponents_ReturnsUnsolved()
    {
        // Arrange
        var puzzle = MultiPartPuzzle.Create("master", ["comp1", "comp2", "comp3"]);
        puzzle.RecordComponentSolved("comp2");

        // Act
        var remaining = puzzle.GetRemainingComponents().ToList();

        // Assert
        remaining.Should().Contain("comp1");
        remaining.Should().Contain("comp3");
        remaining.Should().NotContain("comp2");
    }
}
