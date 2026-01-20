using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the RoomType enum.
/// </summary>
[TestFixture]
public class RoomTypeTests
{
    [Test]
    public void RoomType_HasSixValues()
    {
        // Arrange
        var values = Enum.GetValues<RoomType>();

        // Assert
        values.Should().HaveCount(6);
    }

    [Test]
    [TestCase(RoomType.Standard, 0)]
    [TestCase(RoomType.Treasure, 1)]
    [TestCase(RoomType.Trap, 2)]
    [TestCase(RoomType.Boss, 3)]
    [TestCase(RoomType.Safe, 4)]
    [TestCase(RoomType.Shrine, 5)]
    public void RoomType_HasCorrectIntegerValue(RoomType roomType, int expectedValue)
    {
        // Assert
        ((int)roomType).Should().Be(expectedValue);
    }
}
