// ═══════════════════════════════════════════════════════════════════════════════
// AbilityTypeTests.cs
// Unit tests for the AbilityType enum verifying all expected values
// with correct explicit integer assignments for stable serialization.
// Version: 0.17.3c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="AbilityType"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that the AbilityType enum contains all expected values
/// (Active, Passive, Stance) with correct explicit integer assignments
/// for stable serialization and database storage.
/// </para>
/// <para>
/// These tests ensure that:
/// </para>
/// <list type="bullet">
///   <item><description>The enum has exactly 3 values</description></item>
///   <item><description>All expected values are present (Active, Passive, Stance)</description></item>
///   <item><description>Integer assignments are stable (0-2)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="AbilityType"/>
[TestFixture]
public class AbilityTypeTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the AbilityType enum has exactly three values,
    /// one for each ability activation classification.
    /// </summary>
    /// <remarks>
    /// The three ability types are:
    /// <list type="bullet">
    ///   <item><description>Active — Player-activated, costs resources, has cooldown</description></item>
    ///   <item><description>Passive — Always-on, no activation required</description></item>
    ///   <item><description>Stance — Toggleable mode, modifies behavior</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void AbilityType_HasThreeValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<AbilityType>();

        // Assert
        values.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies that the AbilityType enum contains all expected values:
    /// Active, Passive, and Stance.
    /// </summary>
    /// <remarks>
    /// Each archetype grants a mix of these ability types:
    /// <list type="bullet">
    ///   <item><description>Warrior: 1 Active, 1 Stance, 1 Passive</description></item>
    ///   <item><description>Skirmisher: 2 Active, 1 Passive</description></item>
    ///   <item><description>Mystic: 2 Active, 1 Passive</description></item>
    ///   <item><description>Adept: 2 Active, 1 Passive</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void AbilityType_ContainsAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<AbilityType>();

        // Assert
        values.Should().Contain(AbilityType.Active);
        values.Should().Contain(AbilityType.Passive);
        values.Should().Contain(AbilityType.Stance);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXPLICIT INTEGER VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that each AbilityType enum value has the correct explicit
    /// integer assignment for stable serialization and database storage.
    /// </summary>
    /// <param name="abilityType">The AbilityType enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    /// <remarks>
    /// Active (0) requires player activation and costs resources.
    /// Passive (1) is always active with no activation required.
    /// Stance (2) is a toggleable mode that modifies behavior.
    /// </remarks>
    [Test]
    [TestCase(AbilityType.Active, 0)]
    [TestCase(AbilityType.Passive, 1)]
    [TestCase(AbilityType.Stance, 2)]
    public void AbilityType_HasCorrectIntegerValues(AbilityType abilityType, int expected)
    {
        // Assert
        ((int)abilityType).Should().Be(expected);
    }
}
