// ═══════════════════════════════════════════════════════════════════════════════
// ResourceTypeTests.cs
// Unit tests for the ResourceType enum verifying both expected values
// with correct explicit integer assignments for stable serialization.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="ResourceType"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that the ResourceType enum contains both expected values
/// (Stamina, AetherPool) with correct explicit integer assignments
/// for stable serialization and database storage.
/// </para>
/// <para>
/// These tests ensure that:
/// </para>
/// <list type="bullet">
///   <item><description>The enum has exactly 2 values</description></item>
///   <item><description>Integer assignments are stable (0-1)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ResourceType"/>
[TestFixture]
public class ResourceTypeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the ResourceType enum has exactly two values,
    /// one for each primary resource pool type.
    /// </summary>
    [Test]
    public void ResourceType_HasTwoValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ResourceType>();

        // Assert
        values.Should().HaveCount(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXPLICIT INTEGER VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each ResourceType enum value has the correct explicit
    /// integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="resourceType">The ResourceType enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Stamina (0) is the physical resource used by Warrior, Skirmisher, and Adept.
    /// AetherPool (1) is the magical resource used exclusively by Mystic.
    /// </remarks>
    [Test]
    [TestCase(ResourceType.Stamina, 0)]
    [TestCase(ResourceType.AetherPool, 1)]
    public void ResourceType_HasCorrectIntegerValues(ResourceType resourceType, int expected)
    {
        // Assert
        ((int)resourceType).Should().Be(expected);
    }
}
