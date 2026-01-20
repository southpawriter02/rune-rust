using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for StyleDescriptors value object.
/// </summary>
[TestFixture]
public class StyleDescriptorsTests
{
    [Test]
    public void RoughHewn_HasAllCategories()
    {
        // Act
        var desc = StyleDescriptors.RoughHewn;

        // Assert
        desc.Walls.Should().NotBeEmpty();
        desc.Floors.Should().NotBeEmpty();
        desc.Ceilings.Should().NotBeEmpty();
        desc.Passages.Should().NotBeEmpty();
        desc.Decorations.Should().NotBeEmpty();
    }

    [Test]
    public void GetRandom_ValidCategory_ReturnsDescriptor()
    {
        // Arrange
        var desc = StyleDescriptors.RoughHewn;
        var random = new Random(12345);

        // Act
        var result = desc.GetRandom("walls", random);

        // Assert
        result.Should().NotBeNullOrEmpty();
        desc.Walls.Should().Contain(result!);
    }

    [Test]
    public void GetRandom_InvalidCategory_ReturnsNull()
    {
        // Arrange
        var desc = StyleDescriptors.RoughHewn;
        var random = new Random(12345);

        // Act
        var result = desc.GetRandom("invalid", random);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Empty_HasNoPools()
    {
        // Act
        var desc = StyleDescriptors.Empty;

        // Assert
        desc.Walls.Should().BeEmpty();
        desc.Floors.Should().BeEmpty();
    }
}
