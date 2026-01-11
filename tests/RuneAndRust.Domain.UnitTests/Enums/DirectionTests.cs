using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the Direction enum.
/// </summary>
[TestFixture]
public class DirectionTests
{
    [Test]
    public void Direction_HasSixValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<Direction>();

        // Assert
        values.Should().HaveCount(6);
        values.Should().Contain(Direction.North);
        values.Should().Contain(Direction.South);
        values.Should().Contain(Direction.East);
        values.Should().Contain(Direction.West);
        values.Should().Contain(Direction.Up);
        values.Should().Contain(Direction.Down);
    }

    [Test]
    public void Direction_HasCorrectIntegerValues()
    {
        // Assert
        ((int)Direction.North).Should().Be(0);
        ((int)Direction.South).Should().Be(1);
        ((int)Direction.East).Should().Be(2);
        ((int)Direction.West).Should().Be(3);
        ((int)Direction.Up).Should().Be(4);
        ((int)Direction.Down).Should().Be(5);
    }

    [TestCase(Direction.North, Direction.South)]
    [TestCase(Direction.South, Direction.North)]
    [TestCase(Direction.East, Direction.West)]
    [TestCase(Direction.West, Direction.East)]
    [TestCase(Direction.Up, Direction.Down)]
    [TestCase(Direction.Down, Direction.Up)]
    public void GetOppositeDirection_ReturnsCorrectOpposite(Direction input, Direction expected)
    {
        // Act
        var result = Dungeon.GetOppositeDirection(input);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void GetOppositeDirection_WithInvalidDirection_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDirection = (Direction)99;

        // Act
        var act = () => Dungeon.GetOppositeDirection(invalidDirection);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
