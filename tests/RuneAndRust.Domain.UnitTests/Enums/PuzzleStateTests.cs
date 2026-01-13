using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the PuzzleState enum.
/// </summary>
[TestFixture]
public class PuzzleStateTests
{
    [Test]
    public void PuzzleState_HasAllExpectedValues()
    {
        // Act
        var values = Enum.GetValues<PuzzleState>();

        // Assert
        values.Should().HaveCount(5);
        values.Should().Contain(PuzzleState.Unsolved);
        values.Should().Contain(PuzzleState.InProgress);
        values.Should().Contain(PuzzleState.Solved);
        values.Should().Contain(PuzzleState.Failed);
        values.Should().Contain(PuzzleState.Locked);
    }

    [Test]
    public void PuzzleState_HasCorrectIntegerValues()
    {
        // Assert
        ((int)PuzzleState.Unsolved).Should().Be(0);
        ((int)PuzzleState.InProgress).Should().Be(1);
        ((int)PuzzleState.Solved).Should().Be(2);
        ((int)PuzzleState.Failed).Should().Be(3);
        ((int)PuzzleState.Locked).Should().Be(4);
    }
}
