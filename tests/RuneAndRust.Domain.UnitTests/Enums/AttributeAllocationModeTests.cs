// ═══════════════════════════════════════════════════════════════════════════════
// AttributeAllocationModeTests.cs
// Unit tests for the AttributeAllocationMode enum verifying the two expected
// values (Simple, Advanced) with correct explicit integer assignments for
// stable serialization and database storage.
// Version: 0.17.2b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="AttributeAllocationMode"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that the AttributeAllocationMode enum contains exactly two values
/// (Simple, Advanced) with correct explicit integer assignments for stable
/// serialization and database storage.
/// </para>
/// <para>
/// These tests ensure that:
/// </para>
/// <list type="bullet">
///   <item><description>The enum has exactly 2 values</description></item>
///   <item><description>All expected values are present</description></item>
///   <item><description>Integer assignments are stable (0-1)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="AttributeAllocationMode"/>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.AttributeAllocationState"/>
[TestFixture]
public class AttributeAllocationModeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the AttributeAllocationMode enum has exactly two values,
    /// one for each allocation approach (Simple and Advanced) during character
    /// creation Step 3.
    /// </summary>
    [Test]
    public void AttributeAllocationMode_HasTwoValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<AttributeAllocationMode>();

        // Assert
        values.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that all expected allocation mode values are defined in the enum.
    /// </summary>
    [Test]
    public void AttributeAllocationMode_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            AttributeAllocationMode.Simple,
            AttributeAllocationMode.Advanced
        };

        // Act
        var actualValues = Enum.GetValues<AttributeAllocationMode>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXPLICIT INTEGER VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each AttributeAllocationMode enum value has the correct
    /// explicit integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="mode">The AttributeAllocationMode enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Explicit integer values (0-1) ensure that:
    /// <list type="bullet">
    ///   <item><description>JSON serialization produces consistent values</description></item>
    ///   <item><description>Database storage remains stable across code changes</description></item>
    ///   <item><description>Simple mode is the default (0) for new players</description></item>
    /// </list>
    /// </remarks>
    [Test]
    [TestCase(AttributeAllocationMode.Simple, 0)]
    [TestCase(AttributeAllocationMode.Advanced, 1)]
    public void AttributeAllocationMode_HasCorrectIntegerValues(
        AttributeAllocationMode mode, int expected)
    {
        // Assert
        ((int)mode).Should().Be(expected);
    }
}
