using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Configuration;
using RuneAndRust.Infrastructure.Providers;

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for RecipeScrollProvider (v0.11.1c).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading scroll configurations from settings</description></item>
///   <item><description>Filtering scrolls by dungeon level</description></item>
///   <item><description>Filtering scrolls by loot source</description></item>
///   <item><description>Combined level and source filtering</description></item>
///   <item><description>Drop chance retrieval</description></item>
///   <item><description>Case-insensitive lookups</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeScrollProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<RecipeScrollProvider>> _mockLogger = null!;
    private RecipeScrollSettings _settings = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<RecipeScrollProvider>>();

        // Setup test settings with recipe scroll configurations
        _settings = new RecipeScrollSettings
        {
            RecipeScrolls =
            [
                new RecipeScrollConfigDto
                {
                    RecipeId = "steel-sword",
                    DropWeight = 10,
                    MinDungeonLevel = 3,
                    MaxDungeonLevel = null,
                    LootSources = ["Chest", "Boss"],
                    BaseValue = 100
                },
                new RecipeScrollConfigDto
                {
                    RecipeId = "mithril-blade",
                    DropWeight = 2,
                    MinDungeonLevel = 8,
                    MaxDungeonLevel = null,
                    LootSources = ["Boss", "Quest"],
                    BaseValue = 500
                },
                new RecipeScrollConfigDto
                {
                    RecipeId = "fire-resistance-potion",
                    DropWeight = 8,
                    MinDungeonLevel = 5,
                    MaxDungeonLevel = 10,
                    LootSources = ["Chest", "Monster", "Boss"],
                    BaseValue = 75
                },
                new RecipeScrollConfigDto
                {
                    RecipeId = "greater-healing-potion",
                    DropWeight = 12,
                    MinDungeonLevel = 4,
                    MaxDungeonLevel = null,
                    LootSources = ["Chest", "Monster", "Boss"],
                    BaseValue = 80
                }
            ],
            ScrollDropChances = new Dictionary<string, decimal>
            {
                ["Chest"] = 0.15m,
                ["Boss"] = 0.30m,
                ["Monster"] = 0.02m,
                ["Quest"] = 0.50m
            }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeScrollProvider(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeScrollProvider(Options.Create(_settings), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetScrollsForLevel TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetScrollsForLevel returns only scrolls eligible at the specified level.
    /// </summary>
    [Test]
    public void GetScrollsForLevel_ReturnsOnlyEligibleScrolls()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var level3Scrolls = provider.GetScrollsForLevel(3);
        var level5Scrolls = provider.GetScrollsForLevel(5);
        var level8Scrolls = provider.GetScrollsForLevel(8);

        // Assert
        // Level 3: steel-sword (min 3)
        level3Scrolls.Should().HaveCount(1);
        level3Scrolls.Should().Contain(s => s.RecipeId == "steel-sword");

        // Level 5: steel-sword (min 3), fire-resistance-potion (min 5), greater-healing-potion (min 4)
        level5Scrolls.Should().HaveCount(3);
        level5Scrolls.Should().Contain(s => s.RecipeId == "steel-sword");
        level5Scrolls.Should().Contain(s => s.RecipeId == "fire-resistance-potion");
        level5Scrolls.Should().Contain(s => s.RecipeId == "greater-healing-potion");

        // Level 8: all 4 scrolls
        level8Scrolls.Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that GetScrollsForLevel respects max dungeon level.
    /// </summary>
    [Test]
    public void GetScrollsForLevel_RespectsMaxDungeonLevel()
    {
        // Arrange
        var provider = CreateProvider();

        // Act - Level 11, fire-resistance-potion has max level 10
        var level11Scrolls = provider.GetScrollsForLevel(11);

        // Assert
        level11Scrolls.Should().NotContain(s => s.RecipeId == "fire-resistance-potion");
        level11Scrolls.Should().Contain(s => s.RecipeId == "steel-sword");
        level11Scrolls.Should().Contain(s => s.RecipeId == "mithril-blade");
        level11Scrolls.Should().Contain(s => s.RecipeId == "greater-healing-potion");
    }

    /// <summary>
    /// Verifies that GetScrollsForLevel returns empty for levels below all minimums.
    /// </summary>
    [Test]
    public void GetScrollsForLevel_BelowAllMinimums_ReturnsEmptyList()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var level1Scrolls = provider.GetScrollsForLevel(1);
        var level2Scrolls = provider.GetScrollsForLevel(2);

        // Assert
        level1Scrolls.Should().BeEmpty();
        level2Scrolls.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetScrollsForSource TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetScrollsForSource returns scrolls for the specified source.
    /// </summary>
    [Test]
    public void GetScrollsForSource_ReturnsScrollsForSource()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var chestScrolls = provider.GetScrollsForSource(LootSourceType.Chest);
        var bossScrolls = provider.GetScrollsForSource(LootSourceType.Boss);
        var questScrolls = provider.GetScrollsForSource(LootSourceType.Quest);

        // Assert
        // Chest: steel-sword, fire-resistance-potion, greater-healing-potion
        chestScrolls.Should().HaveCount(3);
        chestScrolls.Should().NotContain(s => s.RecipeId == "mithril-blade");

        // Boss: all 4
        bossScrolls.Should().HaveCount(4);

        // Quest: only mithril-blade
        questScrolls.Should().HaveCount(1);
        questScrolls.Should().Contain(s => s.RecipeId == "mithril-blade");
    }

    /// <summary>
    /// Verifies that GetScrollsForSource returns empty for sources with no scrolls.
    /// </summary>
    [Test]
    public void GetScrollsForSource_UnconfiguredSource_ReturnsEmptyList()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var shopScrolls = provider.GetScrollsForSource(LootSourceType.Shop);
        var craftingScrolls = provider.GetScrollsForSource(LootSourceType.Crafting);

        // Assert
        shopScrolls.Should().BeEmpty();
        craftingScrolls.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetEligibleScrolls TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetEligibleScrolls returns scrolls matching both level and source.
    /// </summary>
    [Test]
    public void GetEligibleScrolls_ReturnsCombinedFiltering()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var level5ChestScrolls = provider.GetEligibleScrolls(5, LootSourceType.Chest);
        var level5BossScrolls = provider.GetEligibleScrolls(5, LootSourceType.Boss);
        var level8QuestScrolls = provider.GetEligibleScrolls(8, LootSourceType.Quest);

        // Assert
        // Level 5, Chest: steel-sword (3+, chest/boss), fire-resistance (5-10, chest/monster/boss), greater-healing (4+, chest/monster/boss)
        level5ChestScrolls.Should().HaveCount(3);

        // Level 5, Boss: same 3 (mithril-blade requires level 8)
        level5BossScrolls.Should().HaveCount(3);
        level5BossScrolls.Should().NotContain(s => s.RecipeId == "mithril-blade");

        // Level 8, Quest: only mithril-blade (quest source)
        level8QuestScrolls.Should().HaveCount(1);
        level8QuestScrolls.Should().Contain(s => s.RecipeId == "mithril-blade");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetDropChance TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDropChance returns the configured drop chance for each source.
    /// </summary>
    [Test]
    public void GetDropChance_ReturnsConfiguredChance()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var chestChance = provider.GetDropChance(LootSourceType.Chest);
        var bossChance = provider.GetDropChance(LootSourceType.Boss);
        var monsterChance = provider.GetDropChance(LootSourceType.Monster);
        var questChance = provider.GetDropChance(LootSourceType.Quest);

        // Assert
        chestChance.Should().Be(0.15m);
        bossChance.Should().Be(0.30m);
        monsterChance.Should().Be(0.02m);
        questChance.Should().Be(0.50m);
    }

    /// <summary>
    /// Verifies that GetDropChance returns 0 for unconfigured sources.
    /// </summary>
    [Test]
    public void GetDropChance_UnconfiguredSource_ReturnsZero()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var shopChance = provider.GetDropChance(LootSourceType.Shop);
        var craftingChance = provider.GetDropChance(LootSourceType.Crafting);

        // Assert
        shopChance.Should().Be(0m);
        craftingChance.Should().Be(0m);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetScrollConfig TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetScrollConfig returns the config for existing recipes.
    /// </summary>
    [Test]
    public void GetScrollConfig_ExistingRecipe_ReturnsConfig()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var steelSwordConfig = provider.GetScrollConfig("steel-sword");

        // Assert
        steelSwordConfig.Should().NotBeNull();
        steelSwordConfig!.RecipeId.Should().Be("steel-sword");
        steelSwordConfig.DropWeight.Should().Be(10);
        steelSwordConfig.MinDungeonLevel.Should().Be(3);
        steelSwordConfig.BaseValue.Should().Be(100);
    }

    /// <summary>
    /// Verifies that GetScrollConfig is case-insensitive.
    /// </summary>
    [Test]
    public void GetScrollConfig_CaseInsensitive()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var config1 = provider.GetScrollConfig("steel-sword");
        var config2 = provider.GetScrollConfig("STEEL-SWORD");
        var config3 = provider.GetScrollConfig("Steel-Sword");

        // Assert
        config1.Should().NotBeNull();
        config2.Should().NotBeNull();
        config3.Should().NotBeNull();
        config1!.RecipeId.Should().Be(config2!.RecipeId);
        config2.RecipeId.Should().Be(config3!.RecipeId);
    }

    /// <summary>
    /// Verifies that GetScrollConfig returns null for unknown recipes.
    /// </summary>
    [Test]
    public void GetScrollConfig_UnknownRecipe_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var config = provider.GetScrollConfig("unknown-recipe");

        // Assert
        config.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetScrollConfig returns null for null/empty recipe ID.
    /// </summary>
    [Test]
    public void GetScrollConfig_NullOrEmptyRecipeId_ReturnsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.GetScrollConfig(null!).Should().BeNull();
        provider.GetScrollConfig("").Should().BeNull();
        provider.GetScrollConfig("   ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // HasScrollConfig TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasScrollConfig returns correct values.
    /// </summary>
    [Test]
    public void HasScrollConfig_ReturnsCorrectResult()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        provider.HasScrollConfig("steel-sword").Should().BeTrue();
        provider.HasScrollConfig("STEEL-SWORD").Should().BeTrue(); // Case-insensitive
        provider.HasScrollConfig("unknown-recipe").Should().BeFalse();
        provider.HasScrollConfig(null!).Should().BeFalse();
        provider.HasScrollConfig("").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllScrollConfigs AND GetScrollCount TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllScrollConfigs returns all loaded configs.
    /// </summary>
    [Test]
    public void GetAllScrollConfigs_ReturnsAllConfigs()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var allConfigs = provider.GetAllScrollConfigs();

        // Assert
        allConfigs.Should().HaveCount(4);
        allConfigs.Should().Contain(c => c.RecipeId == "steel-sword");
        allConfigs.Should().Contain(c => c.RecipeId == "mithril-blade");
        allConfigs.Should().Contain(c => c.RecipeId == "fire-resistance-potion");
        allConfigs.Should().Contain(c => c.RecipeId == "greater-healing-potion");
    }

    /// <summary>
    /// Verifies that GetScrollCount returns the total count.
    /// </summary>
    [Test]
    public void GetScrollCount_ReturnsCorrectCount()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var count = provider.GetScrollCount();

        // Assert
        count.Should().Be(4);
    }

    // ═══════════════════════════════════════════════════════════════
    // EMPTY CONFIGURATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider handles empty configuration gracefully.
    /// </summary>
    [Test]
    public void Provider_WithEmptyConfiguration_ReturnsEmptyResults()
    {
        // Arrange
        var emptySettings = new RecipeScrollSettings
        {
            RecipeScrolls = [],
            ScrollDropChances = new Dictionary<string, decimal>()
        };
        var provider = new RecipeScrollProvider(
            Options.Create(emptySettings),
            _mockLogger.Object);

        // Act & Assert
        provider.GetScrollCount().Should().Be(0);
        provider.GetAllScrollConfigs().Should().BeEmpty();
        provider.GetScrollsForLevel(5).Should().BeEmpty();
        provider.GetScrollsForSource(LootSourceType.Boss).Should().BeEmpty();
        provider.GetDropChance(LootSourceType.Chest).Should().Be(0m);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private RecipeScrollProvider CreateProvider()
    {
        return new RecipeScrollProvider(
            Options.Create(_settings),
            _mockLogger.Object);
    }
}
