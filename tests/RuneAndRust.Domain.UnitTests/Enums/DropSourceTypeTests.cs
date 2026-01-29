using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="DropSourceType"/> enum.
/// </summary>
[TestFixture]
public class DropSourceTypeTests
{
    /// <summary>
    /// Verifies that all expected drop source types are defined.
    /// </summary>
    [Test]
    public void DropSourceType_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            DropSourceType.Monster,
            DropSourceType.Container,
            DropSourceType.Boss,
            DropSourceType.Vendor,
            DropSourceType.Quest
        };

        // Act
        var actualValues = Enum.GetValues<DropSourceType>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
        actualValues.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that enum values are explicitly assigned for stable serialization.
    /// </summary>
    [Test]
    public void DropSourceType_HasExplicitIntValues()
    {
        // Assert
        ((int)DropSourceType.Monster).Should().Be(0);
        ((int)DropSourceType.Container).Should().Be(1);
        ((int)DropSourceType.Boss).Should().Be(2);
        ((int)DropSourceType.Vendor).Should().Be(3);
        ((int)DropSourceType.Quest).Should().Be(4);
    }
}
