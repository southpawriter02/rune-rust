namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="CorruptionSource"/> domain enum.
/// Tests verify explicit integer assignments for stable serialization
/// and enum count to prevent accidental additions.
/// </summary>
[TestFixture]
public class CorruptionSourceTests
{
    // -------------------------------------------------------------------------
    // Enum Values — Explicit Integer Assignments
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that all CorruptionSource enum values have the expected explicit
    /// integer assignments for stable database serialization.
    /// </summary>
    [Test]
    [TestCase(CorruptionSource.MysticMagic, 0)]
    [TestCase(CorruptionSource.HereticalAbility, 1)]
    [TestCase(CorruptionSource.Artifact, 2)]
    [TestCase(CorruptionSource.Environmental, 3)]
    [TestCase(CorruptionSource.Consumable, 4)]
    [TestCase(CorruptionSource.Ritual, 5)]
    [TestCase(CorruptionSource.ForlornContact, 6)]
    [TestCase(CorruptionSource.BlightTransfer, 7)]
    public void CorruptionSource_HasExpectedValues(CorruptionSource source, int expectedValue)
    {
        // Assert
        ((int)source).Should().Be(expectedValue,
            because: $"{source} must have a stable integer value for database persistence");
    }

    // -------------------------------------------------------------------------
    // Enum Count — Guard Against Accidental Changes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that exactly 8 CorruptionSource enum values exist.
    /// Adding or removing values requires updating this test, serialization
    /// mappings, and all switch expressions that handle this enum.
    /// </summary>
    [Test]
    public void CorruptionSource_HasCorrectCount()
    {
        // Arrange
        var values = Enum.GetValues<CorruptionSource>();

        // Assert
        values.Should().HaveCount(8,
            because: "there should be exactly 8 corruption source categories");
    }
}
