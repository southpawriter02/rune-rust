using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="FallbackReason"/> enum.
/// </summary>
/// <remarks>
/// Tests verify that all expected enum values are defined with stable
/// integer assignments for serialization compatibility.
/// </remarks>
[TestFixture]
public class FallbackReasonTests
{
    /// <summary>
    /// Verifies that all expected fallback reasons are defined.
    /// </summary>
    [Test]
    public void FallbackReason_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            FallbackReason.None,
            FallbackReason.PoolExhausted,
            FallbackReason.NoMatchingSource,
            FallbackReason.DropChanceFailed,
            FallbackReason.TierTooLow
        };

        // Act
        var actualValues = Enum.GetValues<FallbackReason>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
        actualValues.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that enum values are explicitly assigned for stable serialization.
    /// </summary>
    [Test]
    public void FallbackReason_HasExplicitIntValues()
    {
        // Assert
        ((int)FallbackReason.None).Should().Be(0);
        ((int)FallbackReason.PoolExhausted).Should().Be(1);
        ((int)FallbackReason.NoMatchingSource).Should().Be(2);
        ((int)FallbackReason.DropChanceFailed).Should().Be(3);
        ((int)FallbackReason.TierTooLow).Should().Be(4);
    }
}
