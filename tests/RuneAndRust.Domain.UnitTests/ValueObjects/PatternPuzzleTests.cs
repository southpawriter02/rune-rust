using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the PatternPuzzle value object.
/// </summary>
[TestFixture]
public class PatternPuzzleTests
{
    [Test]
    public void Create_WithValidPattern_CreatesInstance()
    {
        // Act
        var puzzle = PatternPuzzle.Create("XOXXOX", 3, 2, ["X", "O"]);

        // Assert
        puzzle.TargetPattern.Should().Be("XOXXOX");
        puzzle.GridWidth.Should().Be(3);
        puzzle.GridHeight.Should().Be(2);
        puzzle.TotalCells.Should().Be(6);
    }

    [Test]
    public void Validate_WhenExactMatch_ReturnsTrue()
    {
        // Arrange
        var puzzle = PatternPuzzle.Create("XOXXOX", 3, 2);

        // Act & Assert
        puzzle.Validate("XOXXOX").Should().BeTrue();
    }

    [Test]
    public void Validate_WhenWrong_ReturnsFalse()
    {
        // Arrange
        var puzzle = PatternPuzzle.Create("XOXXOX", 3, 2);

        // Act & Assert
        puzzle.Validate("OOOOOO").Should().BeFalse();
    }

    [Test]
    public void Validate_WithRotationsEnabled_Accepts90DegreeRotation()
    {
        // Arrange - 2x2 grid: AB / CD rotated 90° = CA / DB
        var puzzle = PatternPuzzle.Create("ABCD", 2, 2, acceptRotations: true);

        // Act & Assert
        puzzle.Validate("CADB").Should().BeTrue();
    }

    [Test]
    public void Validate_WithRotationsDisabled_RejectsRotation()
    {
        // Arrange
        var puzzle = PatternPuzzle.Create("ABCD", 2, 2, acceptRotations: false);

        // Act & Assert
        puzzle.Validate("CADB").Should().BeFalse();
    }

    [Test]
    public void Validate_WithReflectionsEnabled_AcceptsHorizontalReflection()
    {
        // Arrange - 2x2 grid: AB / CD horizontal reflection = BA / DC
        var puzzle = PatternPuzzle.Create("ABCD", 2, 2, acceptReflections: true);

        // Act & Assert
        puzzle.Validate("BADC").Should().BeTrue();
    }

    [Test]
    public void IsValidInput_WithValidElements_ReturnsTrue()
    {
        // Arrange
        var puzzle = PatternPuzzle.Create("XOXXOX", 3, 2, ["X", "O"]);

        // Act & Assert
        puzzle.IsValidInput("XOXO").Should().BeTrue();
    }

    [Test]
    public void IsValidInput_WithInvalidElements_ReturnsFalse()
    {
        // Arrange
        var puzzle = PatternPuzzle.Create("XOXXOX", 3, 2, ["X", "O"]);

        // Act & Assert
        puzzle.IsValidInput("XOAB").Should().BeFalse();
    }

    [Test]
    public void IsValidInput_WithNoRestrictions_AcceptsAny()
    {
        // Arrange - No pattern elements specified
        var puzzle = PatternPuzzle.Create("ABCD", 2, 2);

        // Act & Assert
        puzzle.IsValidInput("ZZZZ").Should().BeTrue();
    }

    [Test]
    public void RenderGrid_ProducesValidOutput()
    {
        // Arrange
        var puzzle = PatternPuzzle.Create("ABCD", 2, 2);

        // Act
        var grid = puzzle.RenderGrid();

        // Assert
        grid.Should().Contain("A");
        grid.Should().Contain("B");
        grid.Should().Contain("C");
        grid.Should().Contain("D");
    }
}
