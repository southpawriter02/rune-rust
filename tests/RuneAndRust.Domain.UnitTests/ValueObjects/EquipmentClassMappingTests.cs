using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="EquipmentClassMapping"/> value object.
/// </summary>
[TestFixture]
public class EquipmentClassMappingTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // IsAppropriateFor - Archetype Affinity Matching Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a Warrior-affinity category returns true for Warrior archetype.
    /// </summary>
    [Test]
    public void IsAppropriateFor_WarriorAffinityWithWarriorArchetype_ReturnsTrue()
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            categoryId: "axes",
            displayName: "Axes",
            affinity: EquipmentClassAffinity.Warrior,
            primaryAttribute: "MIGHT",
            equipmentSlot: "Weapon",
            exampleItems: new[] { "Hand Axe", "Greataxe" });

        // Act
        var result = mapping.IsAppropriateFor("warrior");

        // Assert
        result.Should().BeTrue("Warrior affinity should match Warrior archetype");
    }

    /// <summary>
    /// Verifies that a Warrior-affinity category returns false for non-Warrior archetypes.
    /// </summary>
    [Test]
    [TestCase("skirmisher")]
    [TestCase("mystic")]
    [TestCase("adept")]
    public void IsAppropriateFor_WarriorAffinityWithOtherArchetypes_ReturnsFalse(string archetypeId)
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            categoryId: "axes",
            displayName: "Axes",
            affinity: EquipmentClassAffinity.Warrior,
            primaryAttribute: "MIGHT",
            equipmentSlot: "Weapon",
            exampleItems: new[] { "Hand Axe" });

        // Act
        var result = mapping.IsAppropriateFor(archetypeId);

        // Assert
        result.Should().BeFalse($"Warrior affinity should not match {archetypeId}");
    }

    /// <summary>
    /// Verifies that Universal affinity returns true for all archetypes.
    /// </summary>
    [Test]
    [TestCase("warrior")]
    [TestCase("skirmisher")]
    [TestCase("mystic")]
    [TestCase("adept")]
    public void IsAppropriateFor_UniversalAffinity_ReturnsTrueForAllArchetypes(string archetypeId)
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            categoryId: "light-armor",
            displayName: "Light Armor",
            affinity: EquipmentClassAffinity.Universal,
            primaryAttribute: "",
            equipmentSlot: "Armor",
            exampleItems: new[] { "Leather Armor" });

        // Act
        var result = mapping.IsAppropriateFor(archetypeId);

        // Assert
        result.Should().BeTrue($"Universal affinity should be appropriate for {archetypeId}");
    }

    /// <summary>
    /// Verifies that archetype matching is case-insensitive.
    /// </summary>
    [Test]
    [TestCase("Warrior")]
    [TestCase("WARRIOR")]
    [TestCase("warrior")]
    [TestCase("WaRrIoR")]
    public void IsAppropriateFor_CaseInsensitiveArchetypeMatching(string archetypeId)
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            categoryId: "hammers",
            displayName: "Hammers",
            affinity: EquipmentClassAffinity.Warrior,
            primaryAttribute: "MIGHT",
            equipmentSlot: "Weapon",
            exampleItems: new[] { "Mace" });

        // Act
        var result = mapping.IsAppropriateFor(archetypeId);

        // Assert
        result.Should().BeTrue("archetype matching should be case-insensitive");
    }

    /// <summary>
    /// Verifies that unknown archetypes return false (except for Universal).
    /// </summary>
    [Test]
    public void IsAppropriateFor_UnknownArchetype_ReturnsFalse()
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            categoryId: "daggers",
            displayName: "Daggers",
            affinity: EquipmentClassAffinity.Skirmisher,
            primaryAttribute: "FINESSE",
            equipmentSlot: "Weapon",
            exampleItems: new[] { "Knife" });

        // Act
        var result = mapping.IsAppropriateFor("unknown-archetype");

        // Assert
        result.Should().BeFalse("unknown archetypes should not match non-Universal affinities");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IsAppropriateFor - Each Affinity Matching Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies each affinity correctly matches its corresponding archetype.
    /// </summary>
    [Test]
    [TestCase(EquipmentClassAffinity.Warrior, "warrior", true)]
    [TestCase(EquipmentClassAffinity.Skirmisher, "skirmisher", true)]
    [TestCase(EquipmentClassAffinity.Mystic, "mystic", true)]
    [TestCase(EquipmentClassAffinity.Adept, "adept", true)]
    [TestCase(EquipmentClassAffinity.Universal, "warrior", true)]
    [TestCase(EquipmentClassAffinity.Warrior, "mystic", false)]
    [TestCase(EquipmentClassAffinity.Mystic, "warrior", false)]
    public void IsAppropriateFor_AffinityArchetypeMatrix(
        EquipmentClassAffinity affinity, string archetypeId, bool expectedResult)
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            categoryId: "test-category",
            displayName: "Test Category",
            affinity: affinity,
            primaryAttribute: "TEST",
            equipmentSlot: "Weapon",
            exampleItems: new[] { "Test Item" });

        // Act
        var result = mapping.IsAppropriateFor(archetypeId);

        // Assert
        result.Should().Be(expectedResult,
            $"{affinity} affinity with {archetypeId} archetype should be {expectedResult}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies IsUniversal computed property.
    /// </summary>
    [Test]
    [TestCase(EquipmentClassAffinity.Universal, true)]
    [TestCase(EquipmentClassAffinity.Warrior, false)]
    [TestCase(EquipmentClassAffinity.Skirmisher, false)]
    [TestCase(EquipmentClassAffinity.Mystic, false)]
    [TestCase(EquipmentClassAffinity.Adept, false)]
    public void IsUniversal_ReturnsCorrectValue(EquipmentClassAffinity affinity, bool expected)
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            "test", "Test", affinity, "", "Weapon", new[] { "Item" });

        // Act & Assert
        mapping.IsUniversal.Should().Be(expected);
    }

    /// <summary>
    /// Verifies slot type computed properties.
    /// </summary>
    [Test]
    [TestCase("Weapon", true, false, false)]
    [TestCase("Armor", false, true, false)]
    [TestCase("Accessory", false, false, true)]
    [TestCase("weapon", true, false, false)]  // Case-insensitive
    [TestCase("ARMOR", false, true, false)]   // Case-insensitive
    public void SlotTypeProperties_ReturnCorrectValues(
        string equipmentSlot, bool expectedWeapon, bool expectedArmor, bool expectedAccessory)
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            "test", "Test", EquipmentClassAffinity.Universal, "", equipmentSlot, new[] { "Item" });

        // Act & Assert
        mapping.IsWeapon.Should().Be(expectedWeapon);
        mapping.IsArmor.Should().Be(expectedArmor);
        mapping.IsAccessory.Should().Be(expectedAccessory);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Validation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create normalizes category ID to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesCategoryIdToLowercase()
    {
        // Act
        var mapping = EquipmentClassMapping.Create(
            "AXES", "Axes", EquipmentClassAffinity.Warrior, "MIGHT", "Weapon", new[] { "Axe" });

        // Assert
        mapping.CategoryId.Should().Be("axes", "category ID should be normalized to lowercase");
    }

    /// <summary>
    /// Verifies that Create handles null primaryAttribute.
    /// </summary>
    [Test]
    public void Create_WithNullPrimaryAttribute_UsesEmptyString()
    {
        // Act
        var mapping = EquipmentClassMapping.Create(
            "accessories", "Accessories", EquipmentClassAffinity.Universal, 
            null!, "Accessory", new[] { "Ring" });

        // Assert
        mapping.PrimaryAttribute.Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies argument validation for required parameters.
    /// </summary>
    [Test]
    public void Create_WithNullCategoryId_ThrowsArgumentException()
    {
        // Act
        var act = () => EquipmentClassMapping.Create(
            null!, "Test", EquipmentClassAffinity.Universal, "", "Weapon", new[] { "Item" });

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithNullExampleItems_ThrowsArgumentNullException()
    {
        // Act
        var act = () => EquipmentClassMapping.Create(
            "test", "Test", EquipmentClassAffinity.Universal, "", "Weapon", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Utility Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies FormatExamples returns comma-separated list.
    /// </summary>
    [Test]
    public void FormatExamples_ReturnsCommaSeparatedList()
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            "axes", "Axes", EquipmentClassAffinity.Warrior, "MIGHT", "Weapon",
            new[] { "Hand Axe", "Bearded Axe", "Greataxe" });

        // Act
        var result = mapping.FormatExamples();

        // Assert
        result.Should().Be("Hand Axe, Bearded Axe, Greataxe");
    }

    /// <summary>
    /// Verifies ToString returns formatted display string.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var mapping = EquipmentClassMapping.Create(
            "axes", "Axes", EquipmentClassAffinity.Warrior, "MIGHT", "Weapon", new[] { "Axe" });

        // Act
        var result = mapping.ToString();

        // Assert
        result.Should().Be("Axes (axes): Warrior [MIGHT]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Empty Static Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies Empty static property returns valid default.
    /// </summary>
    [Test]
    public void Empty_ReturnsUniversalAffinity()
    {
        // Act
        var empty = EquipmentClassMapping.Empty;

        // Assert
        empty.CategoryId.Should().BeEmpty();
        empty.DisplayName.Should().Be("Unknown");
        empty.Affinity.Should().Be(EquipmentClassAffinity.Universal);
        empty.IsUniversal.Should().BeTrue();
        empty.ExampleItems.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies Empty is appropriate for all archetypes.
    /// </summary>
    [Test]
    public void Empty_IsAppropriateForAllArchetypes()
    {
        // Arrange
        var empty = EquipmentClassMapping.Empty;

        // Assert
        empty.IsAppropriateFor("warrior").Should().BeTrue();
        empty.IsAppropriateFor("skirmisher").Should().BeTrue();
        empty.IsAppropriateFor("mystic").Should().BeTrue();
        empty.IsAppropriateFor("adept").Should().BeTrue();
    }
}
