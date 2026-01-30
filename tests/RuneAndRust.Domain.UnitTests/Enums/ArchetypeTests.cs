// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeTests.cs
// Unit tests for the Archetype enum verifying all four expected values
// with correct explicit integer assignments for stable serialization.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="Archetype"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that the Archetype enum contains all four expected values
/// (Warrior, Skirmisher, Mystic, Adept) with correct explicit integer
/// assignments for stable serialization and database storage.
/// </para>
/// <para>
/// These tests ensure that:
/// </para>
/// <list type="bullet">
///   <item><description>The enum has exactly 4 values</description></item>
///   <item><description>All expected values are present</description></item>
///   <item><description>Integer assignments are stable (0-3)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="Archetype"/>
[TestFixture]
public class ArchetypeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the Archetype enum has exactly four values,
    /// one for each combat role archetype in Aethelgard.
    /// </summary>
    [Test]
    public void Archetype_HasFourValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<Archetype>();

        // Assert
        values.Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that all expected archetype values are defined in the enum.
    /// </summary>
    [Test]
    public void Archetype_ContainsAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            Archetype.Warrior,
            Archetype.Skirmisher,
            Archetype.Mystic,
            Archetype.Adept
        };

        // Act
        var actualValues = Enum.GetValues<Archetype>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXPLICIT INTEGER VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each Archetype enum value has the correct explicit
    /// integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="archetype">The Archetype enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Explicit integer values (0-3) ensure that:
    /// <list type="bullet">
    ///   <item><description>JSON serialization produces consistent values</description></item>
    ///   <item><description>Database storage remains stable across code changes</description></item>
    ///   <item><description>Array/dictionary indexing by cast is reliable</description></item>
    /// </list>
    /// </remarks>
    [Test]
    [TestCase(Archetype.Warrior, 0)]
    [TestCase(Archetype.Skirmisher, 1)]
    [TestCase(Archetype.Mystic, 2)]
    [TestCase(Archetype.Adept, 3)]
    public void Archetype_HasCorrectIntegerValues(Archetype archetype, int expected)
    {
        // Assert
        ((int)archetype).Should().Be(expected);
    }
}
