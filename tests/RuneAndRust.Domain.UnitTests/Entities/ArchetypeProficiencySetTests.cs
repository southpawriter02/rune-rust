// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeProficiencySetTests.cs
// Unit tests for the ArchetypeProficiencySet entity.
// Version: 0.16.1c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="ArchetypeProficiencySet"/> entity.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify the archetype proficiency set functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>Factory method validation and creation</description></item>
///   <item><description>Proficiency checking (IsProficientWith, GetStartingLevel)</description></item>
///   <item><description>Derived properties (IsVersatile, IsSpecialist, IsProficientWithAllWeapons)</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArchetypeProficiencySetTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// All weapon categories for Warrior archetype (11 total).
    /// </summary>
    private static readonly WeaponCategory[] AllCategories = Enum.GetValues<WeaponCategory>();

    /// <summary>
    /// Mystic archetype categories (3 total).
    /// </summary>
    private static readonly WeaponCategory[] MysticCategories =
    [
        WeaponCategory.Daggers,
        WeaponCategory.Staves,
        WeaponCategory.ArcaneImplements
    ];

    /// <summary>
    /// Skirmisher archetype categories (5 total).
    /// </summary>
    private static readonly WeaponCategory[] SkirmisherCategories =
    [
        WeaponCategory.Daggers,
        WeaponCategory.Swords,
        WeaponCategory.Axes,
        WeaponCategory.Bows,
        WeaponCategory.Crossbows
    ];

    /// <summary>
    /// Adept archetype categories (4 total).
    /// </summary>
    private static readonly WeaponCategory[] AdeptCategories =
    [
        WeaponCategory.Daggers,
        WeaponCategory.Staves,
        WeaponCategory.Hammers,
        WeaponCategory.Crossbows
    ];

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Valid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters creates a proficiency set.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesProficiencySet()
    {
        // Arrange
        const string archetypeId = "Mystic";
        const string displayName = "Mystic";
        const string description = "Arcane practitioner who channels magical energies.";

        // Act
        var set = ArchetypeProficiencySet.Create(
            archetypeId,
            displayName,
            description,
            MysticCategories);

        // Assert
        set.Should().NotBeNull();
        set.Id.Should().NotBe(Guid.Empty);
        set.ArchetypeId.Should().Be("mystic"); // Normalized to lowercase
        set.DisplayName.Should().Be(displayName);
        set.Description.Should().Be(description);
        set.ProficientCategories.Should().HaveCount(3);
        set.ProficientCategories.Should().BeEquivalentTo(MysticCategories);
    }

    /// <summary>
    /// Verifies that archetype ID is normalized to lowercase.
    /// </summary>
    [Test]
    [TestCase("WARRIOR")]
    [TestCase("Warrior")]
    [TestCase("warrior")]
    [TestCase("WaRrIoR")]
    public void Create_NormalizesArchetypeIdToLowercase(string archetypeId)
    {
        // Arrange & Act
        var set = ArchetypeProficiencySet.Create(
            archetypeId,
            "Warrior",
            "Master of arms with all weapons.",
            AllCategories);

        // Assert
        set.ArchetypeId.Should().Be("warrior");
    }

    /// <summary>
    /// Verifies that duplicate categories are removed.
    /// </summary>
    [Test]
    public void Create_WithDuplicateCategories_RemovesDuplicates()
    {
        // Arrange
        var categoriesWithDuplicates = new[]
        {
            WeaponCategory.Daggers,
            WeaponCategory.Daggers,
            WeaponCategory.Staves,
            WeaponCategory.Staves,
            WeaponCategory.ArcaneImplements
        };

        // Act
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner with magical implements.",
            categoriesWithDuplicates);

        // Assert
        set.ProficientCategories.Should().HaveCount(3);
        set.ProficientCategories.Should().OnlyHaveUniqueItems();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Invalid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws on null archetype ID.
    /// </summary>
    [Test]
    public void Create_WithNullArchetypeId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeProficiencySet.Create(
            null!,
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws on empty archetype ID.
    /// </summary>
    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithEmptyArchetypeId_ThrowsArgumentException(string archetypeId)
    {
        // Arrange & Act
        var act = () => ArchetypeProficiencySet.Create(
            archetypeId,
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws on null display name.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeProficiencySet.Create(
            "warrior",
            null!,
            "Master of arms.",
            AllCategories);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws on null description.
    /// </summary>
    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            null!,
            AllCategories);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws on null categories collection.
    /// </summary>
    [Test]
    public void Create_WithNullCategories_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Create throws on empty categories collection.
    /// </summary>
    [Test]
    public void Create_WithEmptyCategories_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            Array.Empty<WeaponCategory>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one weapon category*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IsProficientWith Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsProficientWith returns true for proficient categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Daggers)]
    [TestCase(WeaponCategory.Staves)]
    [TestCase(WeaponCategory.ArcaneImplements)]
    public void IsProficientWith_ForProficientCategory_ReturnsTrue(WeaponCategory category)
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act
        var result = set.IsProficientWith(category);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsProficientWith returns false for non-proficient categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Axes)]
    [TestCase(WeaponCategory.Swords)]
    [TestCase(WeaponCategory.Hammers)]
    [TestCase(WeaponCategory.Polearms)]
    [TestCase(WeaponCategory.Bows)]
    [TestCase(WeaponCategory.Crossbows)]
    [TestCase(WeaponCategory.Shields)]
    [TestCase(WeaponCategory.Firearms)]
    public void IsProficientWith_ForNonProficientCategory_ReturnsFalse(WeaponCategory category)
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act
        var result = set.IsProficientWith(category);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Warrior is proficient with all categories.
    /// </summary>
    [Test]
    public void IsProficientWith_ForWarriorWithAllCategories_AllReturnTrue()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Act & Assert
        foreach (var category in AllCategories)
        {
            set.IsProficientWith(category).Should().BeTrue(
                because: $"Warrior should be proficient with {category}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetStartingLevel Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStartingLevel returns Proficient for proficient categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Daggers)]
    [TestCase(WeaponCategory.Staves)]
    [TestCase(WeaponCategory.ArcaneImplements)]
    public void GetStartingLevel_ForProficientCategory_ReturnsProficient(WeaponCategory category)
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act
        var result = set.GetStartingLevel(category);

        // Assert
        result.Should().Be(WeaponProficiencyLevel.Proficient);
    }

    /// <summary>
    /// Verifies that GetStartingLevel returns NonProficient for non-proficient categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Axes)]
    [TestCase(WeaponCategory.Swords)]
    [TestCase(WeaponCategory.Hammers)]
    [TestCase(WeaponCategory.Bows)]
    [TestCase(WeaponCategory.Crossbows)]
    [TestCase(WeaponCategory.Shields)]
    [TestCase(WeaponCategory.Firearms)]
    public void GetStartingLevel_ForNonProficientCategory_ReturnsNonProficient(WeaponCategory category)
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act
        var result = set.GetStartingLevel(category);

        // Assert
        result.Should().Be(WeaponProficiencyLevel.NonProficient);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetProficientCategoryCount Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetProficientCategoryCount returns correct count per archetype.
    /// </summary>
    [Test]
    [TestCase("warrior", 11)]
    [TestCase("skirmisher", 5)]
    [TestCase("adept", 4)]
    [TestCase("mystic", 3)]
    public void GetProficientCategoryCount_ReturnsExpectedCount(string archetypeId, int expectedCount)
    {
        // Arrange
        var categories = archetypeId switch
        {
            "warrior" => AllCategories,
            "skirmisher" => SkirmisherCategories,
            "adept" => AdeptCategories,
            "mystic" => MysticCategories,
            _ => throw new ArgumentException($"Unknown archetype: {archetypeId}")
        };

        var set = ArchetypeProficiencySet.Create(
            archetypeId,
            archetypeId,
            "Test archetype description.",
            categories);

        // Act
        var count = set.GetProficientCategoryCount();

        // Assert
        count.Should().Be(expectedCount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetNonProficientCategories Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetNonProficientCategories returns empty list for Warrior.
    /// </summary>
    [Test]
    public void GetNonProficientCategories_ForWarrior_ReturnsEmptyList()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Act
        var nonProficient = set.GetNonProficientCategories();

        // Assert
        nonProficient.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetNonProficientCategories returns correct categories for Mystic.
    /// </summary>
    [Test]
    public void GetNonProficientCategories_ForMystic_Returns8Categories()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act
        var nonProficient = set.GetNonProficientCategories();

        // Assert
        nonProficient.Should().HaveCount(8); // 11 total - 3 proficient = 8
        nonProficient.Should().NotContain(WeaponCategory.Daggers);
        nonProficient.Should().NotContain(WeaponCategory.Staves);
        nonProficient.Should().NotContain(WeaponCategory.ArcaneImplements);
        nonProficient.Should().Contain(WeaponCategory.Swords);
        nonProficient.Should().Contain(WeaponCategory.Axes);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Property Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsProficientWithAllWeapons returns true for Warrior.
    /// </summary>
    [Test]
    public void IsProficientWithAllWeapons_ForWarriorWith11Categories_ReturnsTrue()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Act & Assert
        set.IsProficientWithAllWeapons.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsProficientWithAllWeapons returns false for non-Warrior archetypes.
    /// </summary>
    [Test]
    public void IsProficientWithAllWeapons_ForMysticWith3Categories_ReturnsFalse()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act & Assert
        set.IsProficientWithAllWeapons.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsVersatile returns true for archetypes with 7+ proficiencies.
    /// </summary>
    [Test]
    public void IsVersatile_ForWarriorWith11Categories_ReturnsTrue()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Act & Assert
        set.IsVersatile.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsVersatile returns false for archetypes with fewer than 7 proficiencies.
    /// </summary>
    [Test]
    public void IsVersatile_ForSkirmisherWith5Categories_ReturnsFalse()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "skirmisher",
            "Skirmisher",
            "Light weapons specialist.",
            SkirmisherCategories);

        // Act & Assert
        set.IsVersatile.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsSpecialist returns true for archetypes with 3 or fewer proficiencies.
    /// </summary>
    [Test]
    public void IsSpecialist_ForMysticWith3Categories_ReturnsTrue()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act & Assert
        set.IsSpecialist.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsSpecialist returns false for archetypes with more than 3 proficiencies.
    /// </summary>
    [Test]
    public void IsSpecialist_ForAdeptWith4Categories_ReturnsFalse()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "adept",
            "Adept",
            "Hybrid combatant.",
            AdeptCategories);

        // Act & Assert
        set.IsSpecialist.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns expected format.
    /// </summary>
    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms.",
            AllCategories);

        // Act
        var result = set.ToString();

        // Assert
        result.Should().Be("Warrior (warrior): 11 proficiencies");
    }

    /// <summary>
    /// Verifies that ToString includes correct proficiency count for Mystic.
    /// </summary>
    [Test]
    public void ToString_ForMystic_ReturnsCorrectCount()
    {
        // Arrange
        var set = ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner.",
            MysticCategories);

        // Act
        var result = set.ToString();

        // Assert
        result.Should().Be("Mystic (mystic): 3 proficiencies");
    }
}
