using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="CoreAttribute"/> enum.
/// </summary>
/// <remarks>
/// Verifies that the CoreAttribute enum contains all five expected values
/// with correct explicit integer assignments for stable serialization.
/// </remarks>
[TestFixture]
public class CoreAttributeTests
{
    /// <summary>
    /// Verifies that all expected core attribute values are defined.
    /// </summary>
    [Test]
    public void CoreAttribute_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            CoreAttribute.Might,
            CoreAttribute.Finesse,
            CoreAttribute.Wits,
            CoreAttribute.Will,
            CoreAttribute.Sturdiness
        };

        // Act
        var actualValues = Enum.GetValues<CoreAttribute>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
        actualValues.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that enum values are explicitly assigned for stable serialization.
    /// </summary>
    [Test]
    public void CoreAttribute_HasExplicitIntValues()
    {
        // Assert
        ((int)CoreAttribute.Might).Should().Be(0);
        ((int)CoreAttribute.Finesse).Should().Be(1);
        ((int)CoreAttribute.Wits).Should().Be(2);
        ((int)CoreAttribute.Will).Should().Be(3);
        ((int)CoreAttribute.Sturdiness).Should().Be(4);
    }
}
