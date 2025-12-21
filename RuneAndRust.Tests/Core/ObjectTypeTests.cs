using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the ObjectType enum.
/// Validates the six object categories: Furniture, Container, Device, Inscription, Corpse, Hazard.
/// </summary>
public class ObjectTypeTests
{
    [Fact]
    public void ObjectType_ShouldHaveExactlySixValues()
    {
        // Arrange
        var values = Enum.GetValues<ObjectType>();

        // Assert - Updated to 6 in v0.3.3a with Hazard addition
        values.Should().HaveCount(6, "Rune & Rust has exactly six object types (including Hazard)");
    }

    [Fact]
    public void ObjectType_ShouldContain_Furniture()
    {
        // Assert
        Enum.IsDefined(typeof(ObjectType), ObjectType.Furniture).Should().BeTrue();
    }

    [Fact]
    public void ObjectType_ShouldContain_Container()
    {
        // Assert
        Enum.IsDefined(typeof(ObjectType), ObjectType.Container).Should().BeTrue();
    }

    [Fact]
    public void ObjectType_ShouldContain_Device()
    {
        // Assert
        Enum.IsDefined(typeof(ObjectType), ObjectType.Device).Should().BeTrue();
    }

    [Fact]
    public void ObjectType_ShouldContain_Inscription()
    {
        // Assert
        Enum.IsDefined(typeof(ObjectType), ObjectType.Inscription).Should().BeTrue();
    }

    [Fact]
    public void ObjectType_ShouldContain_Corpse()
    {
        // Assert
        Enum.IsDefined(typeof(ObjectType), ObjectType.Corpse).Should().BeTrue();
    }

    [Fact]
    public void ObjectType_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)ObjectType.Furniture).Should().Be(0);
        ((int)ObjectType.Container).Should().Be(1);
        ((int)ObjectType.Device).Should().Be(2);
        ((int)ObjectType.Inscription).Should().Be(3);
        ((int)ObjectType.Corpse).Should().Be(4);
    }

    [Theory]
    [InlineData(ObjectType.Furniture, "Furniture")]
    [InlineData(ObjectType.Container, "Container")]
    [InlineData(ObjectType.Device, "Device")]
    [InlineData(ObjectType.Inscription, "Inscription")]
    [InlineData(ObjectType.Corpse, "Corpse")]
    public void ObjectType_ToString_ReturnsExpectedName(ObjectType objectType, string expectedName)
    {
        // Assert
        objectType.ToString().Should().Be(expectedName);
    }
}
