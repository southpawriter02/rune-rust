using FluentAssertions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="WeaponCategory"/> enum.
/// </summary>
[TestFixture]
public class WeaponCategoryTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Enum Value Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all 11 weapon categories are defined.
    /// </summary>
    [Test]
    public void WeaponCategory_ContainsAllElevenCategories()
    {
        // Assert
        Enum.GetValues<WeaponCategory>().Should().HaveCount(11);
        Enum.IsDefined(WeaponCategory.Axes).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Swords).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Hammers).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Daggers).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Polearms).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Staves).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Bows).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Crossbows).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Shields).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.Firearms).Should().BeTrue();
        Enum.IsDefined(WeaponCategory.ArcaneImplements).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that each category has the expected integer value per specification.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Axes, 0)]
    [TestCase(WeaponCategory.Swords, 1)]
    [TestCase(WeaponCategory.Hammers, 2)]
    [TestCase(WeaponCategory.Daggers, 3)]
    [TestCase(WeaponCategory.Polearms, 4)]
    [TestCase(WeaponCategory.Staves, 5)]
    [TestCase(WeaponCategory.Bows, 6)]
    [TestCase(WeaponCategory.Crossbows, 7)]
    [TestCase(WeaponCategory.Shields, 8)]
    [TestCase(WeaponCategory.Firearms, 9)]
    [TestCase(WeaponCategory.ArcaneImplements, 10)]
    public void WeaponCategory_HasExpectedIntegerValue(
        WeaponCategory category, int expectedValue)
    {
        // Assert
        ((int)category).Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Parsing Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that category strings can be parsed case-insensitively.
    /// </summary>
    [Test]
    [TestCase("Axes", WeaponCategory.Axes)]
    [TestCase("axes", WeaponCategory.Axes)]
    [TestCase("AXES", WeaponCategory.Axes)]
    [TestCase("Swords", WeaponCategory.Swords)]
    [TestCase("Daggers", WeaponCategory.Daggers)]
    [TestCase("ArcaneImplements", WeaponCategory.ArcaneImplements)]
    [TestCase("arcaneimplements", WeaponCategory.ArcaneImplements)]
    public void TryParse_ValidCategoryName_ReturnsTrue(
        string input, WeaponCategory expected)
    {
        // Act
        var success = Enum.TryParse<WeaponCategory>(input, ignoreCase: true, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that invalid category names fail to parse.
    /// </summary>
    [Test]
    [TestCase("Invalid")]
    [TestCase("Weapon")]
    [TestCase("Knife")]
    [TestCase("")]
    public void TryParse_InvalidCategoryName_ReturnsFalse(string input)
    {
        // Act
        var success = Enum.TryParse<WeaponCategory>(input, ignoreCase: true, out _);

        // Assert
        success.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Casting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that integer values can be cast to weapon categories.
    /// </summary>
    [Test]
    [TestCase(0, WeaponCategory.Axes)]
    [TestCase(1, WeaponCategory.Swords)]
    [TestCase(5, WeaponCategory.Staves)]
    [TestCase(9, WeaponCategory.Firearms)]
    [TestCase(10, WeaponCategory.ArcaneImplements)]
    public void WeaponCategory_CanCastFromInteger(
        int value, WeaponCategory expected)
    {
        // Act
        var category = (WeaponCategory)value;

        // Assert
        category.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Range Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that enum values are sequential from 0 to 10.
    /// </summary>
    [Test]
    public void WeaponCategory_ValuesAreSequential()
    {
        // Arrange
        var categories = Enum.GetValues<WeaponCategory>().ToArray();

        // Assert
        categories.Should().HaveCount(11);
        for (int i = 0; i < categories.Length; i++)
        {
            ((int)categories[i]).Should().Be(i);
        }
    }
}
