using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="EquipmentClassAffinity"/> enum.
/// </summary>
[TestFixture]
public class EquipmentClassAffinityTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Enum Value Count Test
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the enum has exactly 5 values with correct integer assignments.
    /// </summary>
    /// <remarks>
    /// Integer assignments are explicitly defined for stable serialization.
    /// </remarks>
    [Test]
    public void EquipmentClassAffinity_HasFiveValues_WithCorrectIntegerAssignments()
    {
        // Arrange & Act
        var affinities = Enum.GetValues<EquipmentClassAffinity>();

        // Assert - Verify count
        affinities.Should().HaveCount(5, "the enum should have exactly 5 values per specification");

        // Assert - Verify integer assignments (required for stable serialization)
        ((int)EquipmentClassAffinity.Warrior).Should().Be(0, "Warrior is the first affinity (0)");
        ((int)EquipmentClassAffinity.Skirmisher).Should().Be(1, "Skirmisher is the second affinity (1)");
        ((int)EquipmentClassAffinity.Mystic).Should().Be(2, "Mystic is the third affinity (2)");
        ((int)EquipmentClassAffinity.Adept).Should().Be(3, "Adept is the fourth affinity (3)");
        ((int)EquipmentClassAffinity.Universal).Should().Be(4, "Universal is the fifth affinity (4)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Parsing Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that affinity strings can be parsed case-insensitively.
    /// </summary>
    [Test]
    [TestCase("Warrior", EquipmentClassAffinity.Warrior)]
    [TestCase("warrior", EquipmentClassAffinity.Warrior)]
    [TestCase("WARRIOR", EquipmentClassAffinity.Warrior)]
    [TestCase("Skirmisher", EquipmentClassAffinity.Skirmisher)]
    [TestCase("Mystic", EquipmentClassAffinity.Mystic)]
    [TestCase("Adept", EquipmentClassAffinity.Adept)]
    [TestCase("Universal", EquipmentClassAffinity.Universal)]
    public void TryParse_ValidAffinityName_ReturnsTrue(
        string input, EquipmentClassAffinity expected)
    {
        // Act
        var success = Enum.TryParse<EquipmentClassAffinity>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that invalid affinity names fail to parse.
    /// </summary>
    [Test]
    [TestCase("Invalid")]
    [TestCase("Fighter")]
    [TestCase("Mage")]
    [TestCase("")]
    public void TryParse_InvalidAffinityName_ReturnsFalse(string input)
    {
        // Act
        var success = Enum.TryParse<EquipmentClassAffinity>(input, ignoreCase: true, out _);

        // Assert
        success.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Casting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that integer values can be cast to affinities.
    /// </summary>
    [Test]
    [TestCase(0, EquipmentClassAffinity.Warrior)]
    [TestCase(1, EquipmentClassAffinity.Skirmisher)]
    [TestCase(2, EquipmentClassAffinity.Mystic)]
    [TestCase(3, EquipmentClassAffinity.Adept)]
    [TestCase(4, EquipmentClassAffinity.Universal)]
    public void EquipmentClassAffinity_CanCastFromInteger(
        int value, EquipmentClassAffinity expected)
    {
        // Act
        var affinity = (EquipmentClassAffinity)value;

        // Assert
        affinity.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Range Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that enum values are sequential from 0 to 4.
    /// </summary>
    [Test]
    public void EquipmentClassAffinity_ValuesAreSequential()
    {
        // Arrange
        var affinities = Enum.GetValues<EquipmentClassAffinity>().ToArray();

        // Assert
        affinities.Should().HaveCount(5);
        for (int i = 0; i < affinities.Length; i++)
        {
            ((int)affinities[i]).Should().Be(i, 
                $"affinity at index {i} should have integer value {i}");
        }
    }
}
