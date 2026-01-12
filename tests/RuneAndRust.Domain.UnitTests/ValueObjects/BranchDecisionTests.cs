using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for BranchDecision value object.
/// </summary>
[TestFixture]
public class BranchDecisionTests
{
    [Test]
    public void AllMainPath_CreatesDecisionWithAllMainPaths()
    {
        // Arrange
        var position = new Position3D(0, 0, 0);
        var exits = new[] { Direction.North, Direction.East };

        // Act
        var result = BranchDecision.AllMainPath(position, exits);

        // Assert
        result.MainPaths.Should().BeEquivalentTo(exits);
        result.IsDeadEnd.Should().BeFalse();
    }

    [Test]
    public void CreateDeadEnd_CreatesEmptyDecision()
    {
        // Arrange
        var position = new Position3D(5, 5, 2);

        // Act
        var result = BranchDecision.CreateDeadEnd(position);

        // Assert
        result.IsDeadEnd.Should().BeTrue();
        result.AllExits.Should().BeEmpty();
    }

    [Test]
    public void ExitFilters_ReturnCorrectDirections()
    {
        // Arrange
        var decisions = new Dictionary<Direction, BranchType>
        {
            [Direction.North] = BranchType.MainPath,
            [Direction.East] = BranchType.SidePath,
            [Direction.South] = BranchType.DeadEnd,
            [Direction.West] = BranchType.Loop
        };
        var decision = new BranchDecision
        {
            Position = new Position3D(0, 0, 0),
            ExitDecisions = decisions,
            IsDeadEnd = false
        };

        // Assert
        decision.MainPaths.Should().ContainSingle().Which.Should().Be(Direction.North);
        decision.SidePaths.Should().ContainSingle().Which.Should().Be(Direction.East);
        decision.DeadEnds.Should().ContainSingle().Which.Should().Be(Direction.South);
        decision.Loops.Should().ContainSingle().Which.Should().Be(Direction.West);
        decision.AllExits.Should().HaveCount(4);
    }
}
