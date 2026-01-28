using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="WeaponCategoryDefinition"/> value object.
/// </summary>
[TestFixture]
public class WeaponCategoryDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create factory method creates valid definitions.
    /// </summary>
    [Test]
    public void Create_ValidParameters_CreatesDefinition()
    {
        // Arrange
        var exampleWeapons = new[] { "Longsword", "Shortsword", "Greatsword" };

        // Act
        var definition = WeaponCategoryDefinition.Create(
            WeaponCategory.Swords,
            "Swords",
            "Versatile bladed weapons balanced for both offense and defense.",
            exampleWeapons,
            AttributeType.Might,
            requiresSpecialTraining: false);

        // Assert
        definition.Category.Should().Be(WeaponCategory.Swords);
        definition.DisplayName.Should().Be("Swords");
        definition.Description.Should().Contain("Versatile");
        definition.ExampleWeapons.Should().HaveCount(3);
        definition.PrimaryAttribute.Should().Be(AttributeType.Might);
        definition.RequiresSpecialTraining.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create throws for null display name.
    /// </summary>
    [Test]
    public void Create_NullDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => WeaponCategoryDefinition.Create(
            WeaponCategory.Axes,
            null!,
            "Valid description",
            new[] { "Battleaxe" },
            AttributeType.Might,
            false);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws for empty description.
    /// </summary>
    [Test]
    public void Create_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => WeaponCategoryDefinition.Create(
            WeaponCategory.Axes,
            "Axes",
            "",
            new[] { "Battleaxe" },
            AttributeType.Might,
            false);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws for null example weapons.
    /// </summary>
    [Test]
    public void Create_NullExampleWeapons_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => WeaponCategoryDefinition.Create(
            WeaponCategory.Axes,
            "Axes",
            "Heavy chopping weapons.",
            null!,
            AttributeType.Might,
            false);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies CategoryValue returns correct integer.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Axes, 0)]
    [TestCase(WeaponCategory.Swords, 1)]
    [TestCase(WeaponCategory.ArcaneImplements, 10)]
    public void CategoryValue_ReturnsCorrectInteger(
        WeaponCategory category, int expectedValue)
    {
        // Arrange
        var definition = CreateMinimalDefinition(category, AttributeType.Might);

        // Assert
        definition.CategoryValue.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies IsMagical returns true only for WILL-based categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Staves, true)]
    [TestCase(WeaponCategory.ArcaneImplements, true)]
    [TestCase(WeaponCategory.Swords, false)]
    [TestCase(WeaponCategory.Firearms, false)]
    public void IsMagical_ReturnsCorrectValue(
        WeaponCategory category, bool expectedMagical)
    {
        // Arrange
        var attribute = expectedMagical ? AttributeType.Will : AttributeType.Might;
        var definition = CreateMinimalDefinition(category, attribute);

        // Assert
        definition.IsMagical.Should().Be(expectedMagical);
    }

    /// <summary>
    /// Verifies IsRanged returns true for Bows, Crossbows, and Firearms.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Bows, true)]
    [TestCase(WeaponCategory.Crossbows, true)]
    [TestCase(WeaponCategory.Firearms, true)]
    [TestCase(WeaponCategory.Swords, false)]
    [TestCase(WeaponCategory.Staves, false)]
    [TestCase(WeaponCategory.ArcaneImplements, false)]
    public void IsRanged_ReturnsCorrectValue(
        WeaponCategory category, bool expectedRanged)
    {
        // Arrange
        var definition = CreateMinimalDefinition(category, AttributeType.Might);

        // Assert
        definition.IsRanged.Should().Be(expectedRanged);
    }

    /// <summary>
    /// Verifies IsMelee returns true for close-combat weapons.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Axes, true)]
    [TestCase(WeaponCategory.Swords, true)]
    [TestCase(WeaponCategory.Daggers, true)]
    [TestCase(WeaponCategory.Shields, true)]
    [TestCase(WeaponCategory.Staves, true)]
    [TestCase(WeaponCategory.Bows, false)]
    [TestCase(WeaponCategory.Firearms, false)]
    [TestCase(WeaponCategory.ArcaneImplements, false)]
    public void IsMelee_ReturnsCorrectValue(
        WeaponCategory category, bool expectedMelee)
    {
        // Arrange
        var definition = CreateMinimalDefinition(category, AttributeType.Might);

        // Assert
        definition.IsMelee.Should().Be(expectedMelee);
    }

    /// <summary>
    /// Verifies IsDefensive returns true only for Shields.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Shields, true)]
    [TestCase(WeaponCategory.Swords, false)]
    [TestCase(WeaponCategory.Axes, false)]
    public void IsDefensive_ReturnsCorrectValue(
        WeaponCategory category, bool expectedDefensive)
    {
        // Arrange
        var definition = CreateMinimalDefinition(category, AttributeType.Might);

        // Assert
        definition.IsDefensive.Should().Be(expectedDefensive);
    }

    /// <summary>
    /// Verifies attribute helper properties return correct values.
    /// </summary>
    [Test]
    public void AttributeHelperProperties_ReturnCorrectValues()
    {
        // Arrange
        var mightDef = CreateMinimalDefinition(WeaponCategory.Axes, AttributeType.Might);
        var finesseDef = CreateMinimalDefinition(WeaponCategory.Daggers, AttributeType.Finesse);
        var willDef = CreateMinimalDefinition(WeaponCategory.Staves, AttributeType.Will);
        var witsDef = CreateMinimalDefinition(WeaponCategory.Crossbows, AttributeType.Wits);

        // Assert
        mightDef.UsesMight.Should().BeTrue();
        mightDef.UsesFinesse.Should().BeFalse();

        finesseDef.UsesFinesse.Should().BeTrue();
        finesseDef.UsesMight.Should().BeFalse();

        willDef.UsesWill.Should().BeTrue();
        willDef.UsesWits.Should().BeFalse();

        witsDef.UsesWits.Should().BeTrue();
        witsDef.UsesWill.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies FormatExamples returns comma-separated list.
    /// </summary>
    [Test]
    public void FormatExamples_ReturnsCommaSeparatedList()
    {
        // Arrange
        var definition = WeaponCategoryDefinition.Create(
            WeaponCategory.Swords,
            "Swords",
            "Versatile bladed weapons.",
            new[] { "Longsword", "Shortsword", "Greatsword" },
            AttributeType.Might,
            false);

        // Act
        var result = definition.FormatExamples();

        // Assert
        result.Should().Be("Longsword, Shortsword, Greatsword");
    }

    /// <summary>
    /// Verifies FormatPrimaryAttribute returns uppercase string.
    /// </summary>
    [Test]
    [TestCase(AttributeType.Might, "MIGHT")]
    [TestCase(AttributeType.Finesse, "FINESSE")]
    [TestCase(AttributeType.Will, "WILL")]
    [TestCase(AttributeType.Wits, "WITS")]
    public void FormatPrimaryAttribute_ReturnsUppercaseString(
        AttributeType attribute, string expected)
    {
        // Arrange
        var definition = CreateMinimalDefinition(WeaponCategory.Axes, attribute);

        // Act
        var result = definition.FormatPrimaryAttribute();

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies ToString returns formatted string with attribute.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var swords = WeaponCategoryDefinition.Create(
            WeaponCategory.Swords,
            "Swords",
            "Versatile bladed weapons.",
            new[] { "Longsword" },
            AttributeType.Might,
            false);

        // Act
        var result = swords.ToString();

        // Assert
        result.Should().Be("Swords (MIGHT)");
    }

    /// <summary>
    /// Verifies ToString includes special training indicator when required.
    /// </summary>
    [Test]
    public void ToString_SpecialTraining_IncludesIndicator()
    {
        // Arrange
        var firearms = WeaponCategoryDefinition.Create(
            WeaponCategory.Firearms,
            "Firearms",
            "Gunpowder-based weapons.",
            new[] { "Pistol" },
            AttributeType.Wits,
            requiresSpecialTraining: true);

        // Act
        var result = firearms.ToString();

        // Assert
        result.Should().Be("Firearms (WITS) [Special Training]");
    }

    /// <summary>
    /// Verifies ToDebugString returns detailed debug information.
    /// </summary>
    [Test]
    public void ToDebugString_ReturnsDetailedInformation()
    {
        // Arrange
        var axes = WeaponCategoryDefinition.Create(
            WeaponCategory.Axes,
            "Axes",
            "Heavy chopping weapons.",
            new[] { "Battleaxe", "Handaxe", "Greataxe" },
            AttributeType.Might,
            false);

        // Act
        var result = axes.ToDebugString();

        // Assert
        result.Should().Contain("Category: Axes (0)");
        result.Should().Contain("Attr: MIGHT");
        result.Should().Contain("Special: False");
        result.Should().Contain("Examples: 3");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Record Equality Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that two definitions with same values are equal.
    /// </summary>
    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var examples = new[] { "Longsword" };
        var def1 = WeaponCategoryDefinition.Create(
            WeaponCategory.Swords, "Swords", "Description", examples, AttributeType.Might, false);
        var def2 = WeaponCategoryDefinition.Create(
            WeaponCategory.Swords, "Swords", "Description", examples, AttributeType.Might, false);

        // Assert
        def1.Should().Be(def2);
    }

    /// <summary>
    /// Verifies that two definitions with different values are not equal.
    /// </summary>
    [Test]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var examples = new[] { "Longsword" };
        var swords = WeaponCategoryDefinition.Create(
            WeaponCategory.Swords, "Swords", "Description", examples, AttributeType.Might, false);
        var axes = WeaponCategoryDefinition.Create(
            WeaponCategory.Axes, "Axes", "Description", examples, AttributeType.Might, false);

        // Assert
        swords.Should().NotBe(axes);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a minimal definition for testing derived properties.
    /// </summary>
    private static WeaponCategoryDefinition CreateMinimalDefinition(
        WeaponCategory category, AttributeType attribute)
    {
        return WeaponCategoryDefinition.Create(
            category,
            category.ToString(),
            "Test description for validation.",
            new[] { "Test Weapon" },
            attribute,
            requiresSpecialTraining: false);
    }
}
