// ═══════════════════════════════════════════════════════════════════════════════
// CoreAttributeTests.cs
// Unit tests for the CoreAttribute enum verifying all five expected values
// with correct explicit integer assignments for stable serialization.
// Version: 0.17.2a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="CoreAttribute"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that the CoreAttribute enum contains all five expected values
/// (Might, Finesse, Wits, Will, Sturdiness) with correct explicit integer
/// assignments for stable serialization and database storage.
/// </para>
/// <para>
/// These tests ensure that:
/// </para>
/// <list type="bullet">
///   <item><description>The enum has exactly 5 values</description></item>
///   <item><description>All expected values are present</description></item>
///   <item><description>Integer assignments are stable (0-4)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CoreAttribute"/>
[TestFixture]
public class CoreAttributeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the CoreAttribute enum has exactly five values,
    /// one for each core character attribute in Aethelgard.
    /// </summary>
    [Test]
    public void CoreAttribute_HasFiveValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CoreAttribute>();

        // Assert
        values.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that all expected core attribute values are defined in the enum.
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
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXPLICIT INTEGER VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each CoreAttribute enum value has the correct explicit
    /// integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="attribute">The CoreAttribute enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Explicit integer values (0-4) ensure that:
    /// <list type="bullet">
    ///   <item><description>JSON serialization produces consistent values</description></item>
    ///   <item><description>Database storage remains stable across code changes</description></item>
    ///   <item><description>Array/dictionary indexing by cast is reliable</description></item>
    /// </list>
    /// </remarks>
    [Test]
    [TestCase(CoreAttribute.Might, 0)]
    [TestCase(CoreAttribute.Finesse, 1)]
    [TestCase(CoreAttribute.Wits, 2)]
    [TestCase(CoreAttribute.Will, 3)]
    [TestCase(CoreAttribute.Sturdiness, 4)]
    public void CoreAttribute_HasCorrectIntegerValues(CoreAttribute attribute, int expected)
    {
        // Assert
        ((int)attribute).Should().Be(expected);
    }
}
