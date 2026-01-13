using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for the VisionType enum.
/// </summary>
[TestFixture]
public class VisionTypeTests
{
    [Test]
    public void VisionType_HasThreeValues()
    {
        // Assert
        var values = Enum.GetValues<VisionType>();
        values.Should().HaveCount(3);
    }

    [Test]
    public void VisionType_NormalIsZero()
    {
        // Assert
        ((int)VisionType.Normal).Should().Be(0);
    }

    [Test]
    public void VisionType_TrueSightIsHighest()
    {
        // Assert
        ((int)VisionType.TrueSight).Should().Be(2);
    }
}
