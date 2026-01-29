using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="UniqueItem"/> entity.
/// </summary>
[TestFixture]
public class UniqueItemTests
{
    /// <summary>
    /// Creates a standard test drop source for use in tests.
    /// </summary>
    private static DropSource CreateTestDropSource() =>
        DropSource.Create(DropSourceType.Boss, "test-boss", 5.0m);

    /// <summary>
    /// Creates a standard test stats object for use in tests.
    /// </summary>
    private static ItemStats CreateTestStats() =>
        ItemStats.Create(might: 5, agility: 2, bonusDamage: 10);

    /// <summary>
    /// Verifies that Create with valid data creates a UniqueItem.
    /// </summary>
    [Test]
    public void Create_WithValidData_CreatesUniqueItem()
    {
        // Arrange
        var stats = CreateTestStats();
        var dropSources = new[] { CreateTestDropSource() };

        // Act
        var item = UniqueItem.Create(
            itemId: "shadowfang-blade",
            name: "Shadowfang Blade",
            description: "A blade forged in darkness.",
            flavorText: "The shadows remember.",
            category: EquipmentCategory.Weapon,
            stats: stats,
            dropSources: dropSources,
            requiredLevel: 10);

        // Assert
        item.ItemId.Should().Be("shadowfang-blade");
        item.Name.Should().Be("Shadowfang Blade");
        item.Description.Should().Be("A blade forged in darkness.");
        item.FlavorText.Should().Be("The shadows remember.");
        item.Category.Should().Be(EquipmentCategory.Weapon);
        item.Stats.Should().Be(stats);
        item.RequiredLevel.Should().Be(10);
        item.Id.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that Create always sets QualityTier to MythForged (Legendary).
    /// </summary>
    [Test]
    public void Create_AlwaysSetsLegendaryQuality()
    {
        // Arrange & Act
        var item = UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item.",
            flavorText: "",
            category: EquipmentCategory.Accessory,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource() });

        // Assert
        item.QualityTier.Should().Be(QualityTier.MythForged);
    }

    /// <summary>
    /// Verifies that Create throws when drop sources list is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyDropSources_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: Array.Empty<DropSource>());

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*drop source*");
    }

    /// <summary>
    /// Verifies that HasAffinityFor returns true when no class restrictions exist.
    /// </summary>
    [Test]
    public void HasAffinityFor_WithNoRestrictions_ReturnsTrue()
    {
        // Arrange
        var item = UniqueItem.Create(
            itemId: "universal-item",
            name: "Universal Item",
            description: "An item for all classes.",
            flavorText: "",
            category: EquipmentCategory.Accessory,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource() },
            classAffinities: null);

        // Act & Assert
        item.HasAffinityFor("warrior").Should().BeTrue();
        item.HasAffinityFor("mage").Should().BeTrue();
        item.HasAffinityFor("rogue").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasAffinityFor correctly checks restricted classes.
    /// </summary>
    [Test]
    public void HasAffinityFor_WithRestrictions_ChecksCorrectly()
    {
        // Arrange
        var item = UniqueItem.Create(
            itemId: "warrior-blade",
            name: "Warrior's Blade",
            description: "A blade for warriors.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: CreateTestStats(),
            dropSources: new[] { CreateTestDropSource() },
            classAffinities: new[] { "warrior", "paladin" });

        // Act & Assert
        item.HasAffinityFor("warrior").Should().BeTrue();
        item.HasAffinityFor("paladin").Should().BeTrue();
        item.HasAffinityFor("mage").Should().BeFalse();
        item.HasAffinityFor("rogue").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CanDropFrom correctly identifies matching sources.
    /// </summary>
    [Test]
    public void CanDropFrom_WithMatchingSource_ReturnsTrue()
    {
        // Arrange
        var dropSources = new[]
        {
            DropSource.Create(DropSourceType.Boss, "shadow-lord", 5.0m),
            DropSource.Create(DropSourceType.Container, "legendary-chest", 0.5m)
        };
        var item = UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: dropSources);

        // Act & Assert
        item.CanDropFrom(DropSourceType.Boss, "shadow-lord").Should().BeTrue();
        item.CanDropFrom(DropSourceType.Container, "legendary-chest").Should().BeTrue();
        item.CanDropFrom(DropSourceType.Boss, "goblin-king").Should().BeFalse();
        item.CanDropFrom(DropSourceType.Monster, "shadow-lord").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create normalizes all IDs to lowercase.
    /// </summary>
    [Test]
    public void Create_NormalizesAllIds()
    {
        // Arrange & Act
        var item = UniqueItem.Create(
            itemId: "SHADOWFANG-Blade",
            name: "Shadowfang Blade",
            description: "A blade.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource() },
            classAffinities: new[] { "WARRIOR", "Rogue" },
            specialEffectIds: new[] { "LIFE-Drain" },
            setId: "Shadow-SET");

        // Assert
        item.ItemId.Should().Be("shadowfang-blade");
        item.ClassAffinities.Should().BeEquivalentTo(new[] { "warrior", "rogue" });
        item.SpecialEffectIds.Should().BeEquivalentTo(new[] { "life-drain" });
        item.SetId.Should().Be("shadow-set");
    }

    /// <summary>
    /// Verifies that GetDropSourcesByType filters correctly.
    /// </summary>
    [Test]
    public void GetDropSourcesByType_ReturnsMatchingSourcesOnly()
    {
        // Arrange
        var dropSources = new[]
        {
            DropSource.Create(DropSourceType.Boss, "boss-1", 5.0m),
            DropSource.Create(DropSourceType.Boss, "boss-2", 3.0m),
            DropSource.Create(DropSourceType.Container, "chest-1", 0.5m)
        };
        var item = UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: dropSources);

        // Act
        var bossSources = item.GetDropSourcesByType(DropSourceType.Boss).ToList();
        var containerSources = item.GetDropSourcesByType(DropSourceType.Container).ToList();
        var questSources = item.GetDropSourcesByType(DropSourceType.Quest).ToList();

        // Assert
        bossSources.Should().HaveCount(2);
        containerSources.Should().HaveCount(1);
        questSources.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies helper properties return correct values.
    /// </summary>
    [Test]
    public void HelperProperties_ReturnCorrectValues()
    {
        // Arrange
        var item = UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test item.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource(), CreateTestDropSource() },
            specialEffectIds: new[] { "effect-1", "effect-2" },
            setId: "test-set");

        // Assert
        item.DropSourceCount.Should().Be(2);
        item.HasSpecialEffects.Should().BeTrue();
        item.IsPartOfSet.Should().BeTrue();
    }

    /// <summary>
    /// Verifies helper properties when item has no optional data.
    /// </summary>
    [Test]
    public void HelperProperties_WithNoOptionalData_ReturnCorrectValues()
    {
        // Arrange
        var item = UniqueItem.Create(
            itemId: "basic-item",
            name: "Basic Item",
            description: "A basic item.",
            flavorText: "",
            category: EquipmentCategory.Accessory,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource() });

        // Assert
        item.DropSourceCount.Should().Be(1);
        item.HasSpecialEffects.Should().BeFalse();
        item.IsPartOfSet.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create throws when itemId is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyItemId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => UniqueItem.Create(
            itemId: "",
            name: "Test Item",
            description: "A test.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource() });

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create throws when requiredLevel is less than 1.
    /// </summary>
    [Test]
    public void Create_WithRequiredLevelZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => UniqueItem.Create(
            itemId: "test-item",
            name: "Test Item",
            description: "A test.",
            flavorText: "",
            category: EquipmentCategory.Weapon,
            stats: ItemStats.Empty,
            dropSources: new[] { CreateTestDropSource() },
            requiredLevel: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
