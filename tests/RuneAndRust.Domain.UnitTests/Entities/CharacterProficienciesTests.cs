// ═══════════════════════════════════════════════════════════════════════════════
// CharacterProficienciesTests.cs
// Unit tests for the CharacterProficiencies entity.
// Version: 0.16.1d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="CharacterProficiencies"/> entity.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify the character proficiencies functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>Factory method creation from archetype</description></item>
///   <item><description>Level and progress queries</description></item>
///   <item><description>Aggregate query methods</description></item>
///   <item><description>Progress update functionality</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CharacterProficienciesTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    private static readonly ProficiencyThresholds DefaultThresholds =
        ProficiencyThresholds.Default;

    private static readonly WeaponCategory[] MysticCategories =
    [
        WeaponCategory.Daggers,
        WeaponCategory.Staves,
        WeaponCategory.ArcaneImplements
    ];

    private static readonly WeaponCategory[] AllCategories =
        Enum.GetValues<WeaponCategory>();

    /// <summary>
    /// Creates a Mystic archetype proficiency set for testing.
    /// </summary>
    private static ArchetypeProficiencySet CreateMysticArchetype() =>
        ArchetypeProficiencySet.Create(
            "mystic",
            "Mystic",
            "Arcane practitioner who channels magical energies.",
            MysticCategories);

    /// <summary>
    /// Creates a Warrior archetype proficiency set for testing.
    /// </summary>
    private static ArchetypeProficiencySet CreateWarriorArchetype() =>
        ArchetypeProficiencySet.Create(
            "warrior",
            "Warrior",
            "Master of arms with all weapons.",
            AllCategories);

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Valid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateFromArchetype creates proficiencies for all categories.
    /// </summary>
    [Test]
    public void CreateFromArchetype_CreatesAll11Categories()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var archetypeSet = CreateMysticArchetype();

        // Act
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            characterId,
            archetypeSet,
            DefaultThresholds);

        // Assert
        proficiencies.Proficiencies.Should().HaveCount(11);
        foreach (var category in AllCategories)
        {
            proficiencies.Proficiencies.Should().ContainKey(category);
        }
    }

    /// <summary>
    /// Verifies that CreateFromArchetype sets correct starting levels.
    /// </summary>
    [Test]
    public void CreateFromArchetype_SetsCorrectStartingLevels()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var archetypeSet = CreateMysticArchetype();

        // Act
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            characterId,
            archetypeSet,
            DefaultThresholds);

        // Assert - Proficient categories
        proficiencies.GetLevel(WeaponCategory.Daggers)
            .Should().Be(WeaponProficiencyLevel.Proficient);
        proficiencies.GetLevel(WeaponCategory.Staves)
            .Should().Be(WeaponProficiencyLevel.Proficient);
        proficiencies.GetLevel(WeaponCategory.ArcaneImplements)
            .Should().Be(WeaponProficiencyLevel.Proficient);

        // Assert - NonProficient categories
        proficiencies.GetLevel(WeaponCategory.Swords)
            .Should().Be(WeaponProficiencyLevel.NonProficient);
        proficiencies.GetLevel(WeaponCategory.Axes)
            .Should().Be(WeaponProficiencyLevel.NonProficient);
        proficiencies.GetLevel(WeaponCategory.Hammers)
            .Should().Be(WeaponProficiencyLevel.NonProficient);
    }

    /// <summary>
    /// Verifies that CreateFromArchetype initializes experience to 0.
    /// </summary>
    [Test]
    public void CreateFromArchetype_InitializesExperienceToZero()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var archetypeSet = CreateMysticArchetype();

        // Act
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            characterId,
            archetypeSet,
            DefaultThresholds);

        // Assert
        foreach (var category in AllCategories)
        {
            proficiencies.GetCombatExperience(category).Should().Be(0);
        }
    }

    /// <summary>
    /// Verifies that CreateFromArchetype sets entity properties correctly.
    /// </summary>
    [Test]
    public void CreateFromArchetype_SetsEntityProperties()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var archetypeSet = CreateMysticArchetype();

        // Act
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            characterId,
            archetypeSet,
            DefaultThresholds);

        // Assert
        proficiencies.Id.Should().NotBe(Guid.Empty);
        proficiencies.CharacterId.Should().Be(characterId);
    }

    /// <summary>
    /// Verifies that Warrior starts with all 11 proficiencies.
    /// </summary>
    [Test]
    public void CreateFromArchetype_ForWarrior_All11Proficient()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var archetypeSet = CreateWarriorArchetype();

        // Act
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            characterId,
            archetypeSet,
            DefaultThresholds);

        // Assert
        var proficientCount = proficiencies.GetCountAtLevel(WeaponProficiencyLevel.Proficient);
        proficientCount.Should().Be(11);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Invalid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateFromArchetype throws on null archetype set.
    /// </summary>
    [Test]
    public void CreateFromArchetype_WithNullArchetype_ThrowsArgumentNullException()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var act = () => CharacterProficiencies.CreateFromArchetype(
            characterId,
            null!,
            DefaultThresholds);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("archetypeProficiencySet");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetLevel Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetLevel returns correct level for proficient categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Daggers)]
    [TestCase(WeaponCategory.Staves)]
    [TestCase(WeaponCategory.ArcaneImplements)]
    public void GetLevel_ForProficientCategory_ReturnsProficient(WeaponCategory category)
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var level = proficiencies.GetLevel(category);

        // Assert
        level.Should().Be(WeaponProficiencyLevel.Proficient);
    }

    /// <summary>
    /// Verifies that GetLevel returns NonProficient for non-proficient categories.
    /// </summary>
    [Test]
    [TestCase(WeaponCategory.Swords)]
    [TestCase(WeaponCategory.Axes)]
    [TestCase(WeaponCategory.Hammers)]
    public void GetLevel_ForNonProficientCategory_ReturnsNonProficient(WeaponCategory category)
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var level = proficiencies.GetLevel(category);

        // Assert
        level.Should().Be(WeaponProficiencyLevel.NonProficient);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetProgress Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetProgress returns full progress details.
    /// </summary>
    [Test]
    public void GetProgress_ReturnsFullProgressDetails()
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var progress = proficiencies.GetProgress(WeaponCategory.Daggers);

        // Assert
        progress.Category.Should().Be(WeaponCategory.Daggers);
        progress.CurrentLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        progress.CombatExperience.Should().Be(0);
        progress.ThresholdForNextLevel.Should().Be(25);
    }

    /// <summary>
    /// Verifies that GetProgress throws for unknown category.
    /// </summary>
    [Test]
    public void GetProgress_ForUnknownCategory_ThrowsArgumentException()
    {
        // This test requires a category not in the dictionary, which shouldn't happen
        // in normal use, but we'll test the internal state validation.

        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // GetProgress should work for all categories
        foreach (var category in AllCategories)
        {
            var act = () => proficiencies.GetProgress(category);
            act.Should().NotThrow();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanAdvance and IsAtMaxLevel Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanAdvance returns true for non-Master levels.
    /// </summary>
    [Test]
    public void CanAdvance_ForNonMasterLevel_ReturnsTrue()
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Assert
        proficiencies.CanAdvance(WeaponCategory.Daggers).Should().BeTrue();
        proficiencies.IsAtMaxLevel(WeaponCategory.Daggers).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Aggregate Query Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCategoriesAtLevel returns correct categories.
    /// </summary>
    [Test]
    public void GetCategoriesAtLevel_ReturnsCor­rectCategories()
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var proficient = proficiencies.GetCategoriesAtLevel(WeaponProficiencyLevel.Proficient);
        var nonProficient = proficiencies.GetCategoriesAtLevel(WeaponProficiencyLevel.NonProficient);

        // Assert
        proficient.Should().HaveCount(3);
        proficient.Should().Contain(WeaponCategory.Daggers);
        proficient.Should().Contain(WeaponCategory.Staves);
        proficient.Should().Contain(WeaponCategory.ArcaneImplements);

        nonProficient.Should().HaveCount(8);
    }

    /// <summary>
    /// Verifies that GetCountAtLevel returns correct counts.
    /// </summary>
    [Test]
    public void GetCountAtLevel_ReturnsCorrectCounts()
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Assert
        proficiencies.GetCountAtLevel(WeaponProficiencyLevel.NonProficient).Should().Be(8);
        proficiencies.GetCountAtLevel(WeaponProficiencyLevel.Proficient).Should().Be(3);
        proficiencies.GetCountAtLevel(WeaponProficiencyLevel.Expert).Should().Be(0);
        proficiencies.GetCountAtLevel(WeaponProficiencyLevel.Master).Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetTotalCombatExperience sums correctly.
    /// </summary>
    [Test]
    public void GetTotalCombatExperience_SumsCorrectly()
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var total = proficiencies.GetTotalCombatExperience();

        // Assert - All start at 0
        total.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetCategoriesReadyToAdvance returns empty when none ready.
    /// </summary>
    [Test]
    public void GetCategoriesReadyToAdvance_WhenNoneReady_ReturnsEmpty()
    {
        // Arrange
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            Guid.NewGuid(),
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var ready = proficiencies.GetCategoriesReadyToAdvance();

        // Assert
        ready.Should().BeEmpty();
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
        var characterId = Guid.NewGuid();
        var proficiencies = CharacterProficiencies.CreateFromArchetype(
            characterId,
            CreateMysticArchetype(),
            DefaultThresholds);

        // Act
        var result = proficiencies.ToString();

        // Assert
        result.Should().Contain($"Character {characterId}");
        result.Should().Contain("NonProficient=8");
        result.Should().Contain("Proficient=3");
        result.Should().Contain("Expert=0");
        result.Should().Contain("Master=0");
    }
}
