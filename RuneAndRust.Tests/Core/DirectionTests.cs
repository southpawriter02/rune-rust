using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the Direction enum.
/// Validates enum values, cardinal directions, and value semantics.
/// </summary>
public class DirectionTests
{
    #region Enum Value Tests

    [Fact]
    public void Direction_ShouldHaveExactlySixValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<Direction>();

        // Assert
        values.Should().HaveCount(6);
    }

    [Fact]
    public void Direction_ShouldContain_North()
    {
        // Assert
        Enum.IsDefined(typeof(Direction), Direction.North).Should().BeTrue();
    }

    [Fact]
    public void Direction_ShouldContain_South()
    {
        // Assert
        Enum.IsDefined(typeof(Direction), Direction.South).Should().BeTrue();
    }

    [Fact]
    public void Direction_ShouldContain_East()
    {
        // Assert
        Enum.IsDefined(typeof(Direction), Direction.East).Should().BeTrue();
    }

    [Fact]
    public void Direction_ShouldContain_West()
    {
        // Assert
        Enum.IsDefined(typeof(Direction), Direction.West).Should().BeTrue();
    }

    [Fact]
    public void Direction_ShouldContain_Up()
    {
        // Assert
        Enum.IsDefined(typeof(Direction), Direction.Up).Should().BeTrue();
    }

    [Fact]
    public void Direction_ShouldContain_Down()
    {
        // Assert
        Enum.IsDefined(typeof(Direction), Direction.Down).Should().BeTrue();
    }

    #endregion

    #region Numeric Value Tests

    [Fact]
    public void Direction_North_ShouldBeZero()
    {
        // Assert
        ((int)Direction.North).Should().Be(0);
    }

    [Fact]
    public void Direction_South_ShouldBeOne()
    {
        // Assert
        ((int)Direction.South).Should().Be(1);
    }

    [Fact]
    public void Direction_East_ShouldBeTwo()
    {
        // Assert
        ((int)Direction.East).Should().Be(2);
    }

    [Fact]
    public void Direction_West_ShouldBeThree()
    {
        // Assert
        ((int)Direction.West).Should().Be(3);
    }

    [Fact]
    public void Direction_Up_ShouldBeFour()
    {
        // Assert
        ((int)Direction.Up).Should().Be(4);
    }

    [Fact]
    public void Direction_Down_ShouldBeFive()
    {
        // Assert
        ((int)Direction.Down).Should().Be(5);
    }

    [Fact]
    public void Direction_EnumValues_ShouldBeSequential()
    {
        // Arrange
        var values = Enum.GetValues<Direction>()
            .Select(d => (int)d)
            .OrderBy(v => v)
            .ToList();

        // Assert
        values.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4, 5 });
    }

    #endregion

    #region ToString Tests

    [Theory]
    [InlineData(Direction.North, "North")]
    [InlineData(Direction.South, "South")]
    [InlineData(Direction.East, "East")]
    [InlineData(Direction.West, "West")]
    [InlineData(Direction.Up, "Up")]
    [InlineData(Direction.Down, "Down")]
    public void Direction_ToString_ReturnsExpectedName(Direction direction, string expectedName)
    {
        // Act
        var result = direction.ToString();

        // Assert
        result.Should().Be(expectedName);
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("North", Direction.North)]
    [InlineData("South", Direction.South)]
    [InlineData("East", Direction.East)]
    [InlineData("West", Direction.West)]
    [InlineData("Up", Direction.Up)]
    [InlineData("Down", Direction.Down)]
    public void Direction_Parse_ReturnsCorrectDirection(string input, Direction expected)
    {
        // Act
        var result = Enum.Parse<Direction>(input, ignoreCase: true);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("north")]
    [InlineData("NORTH")]
    [InlineData("NoRtH")]
    public void Direction_Parse_IsCaseInsensitive(string input)
    {
        // Act
        var result = Enum.Parse<Direction>(input, ignoreCase: true);

        // Assert
        result.Should().Be(Direction.North);
    }

    [Fact]
    public void Direction_TryParse_InvalidValue_ReturnsFalse()
    {
        // Act
        var success = Enum.TryParse<Direction>("Invalid", out _);

        // Assert
        success.Should().BeFalse();
    }

    #endregion

    #region Cardinal Direction Grouping Tests

    [Fact]
    public void Direction_HorizontalDirections_AreNorthSouthEastWest()
    {
        // Arrange
        var horizontalDirections = new[] { Direction.North, Direction.South, Direction.East, Direction.West };

        // Assert
        horizontalDirections.Should().HaveCount(4);
        horizontalDirections.Should().NotContain(Direction.Up);
        horizontalDirections.Should().NotContain(Direction.Down);
    }

    [Fact]
    public void Direction_VerticalDirections_AreUpDown()
    {
        // Arrange
        var verticalDirections = new[] { Direction.Up, Direction.Down };

        // Assert
        verticalDirections.Should().HaveCount(2);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Direction_DefaultValue_ShouldBeNorth()
    {
        // Arrange & Act
        Direction defaultDirection = default;

        // Assert
        defaultDirection.Should().Be(Direction.North);
    }

    #endregion
}
