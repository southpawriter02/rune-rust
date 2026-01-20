using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for LootService recipe scroll generation (v0.11.1c).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Recipe scroll generation with eligible scrolls</description></item>
///   <item><description>Drop chance mechanics</description></item>
///   <item><description>Known recipe exclusion</description></item>
///   <item><description>Weighted selection</description></item>
///   <item><description>Service graceful degradation without scroll provider</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class LootServiceScrollTests
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private Mock<IGameConfigurationProvider> _mockConfigProvider = null!;
    private Mock<ILogger<LootService>> _mockLogger = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<IRecipeScrollProvider> _mockScrollProvider = null!;
    private Mock<IRecipeProvider> _mockRecipeProvider = null!;
    private Mock<IRecipeService> _mockRecipeService = null!;

    private RecipeScrollConfig _steelSwordScrollConfig = null!;
    private RecipeScrollConfig _mithrilBladeScrollConfig = null!;
    private RecipeDefinition _steelSwordRecipe = null!;
    private RecipeDefinition _mithrilBladeRecipe = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _mockConfigProvider = new Mock<IGameConfigurationProvider>();
        _mockLogger = new Mock<ILogger<LootService>>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockScrollProvider = new Mock<IRecipeScrollProvider>();
        _mockRecipeProvider = new Mock<IRecipeProvider>();
        _mockRecipeService = new Mock<IRecipeService>();

        // Setup test scroll configs
        _steelSwordScrollConfig = RecipeScrollConfig.Create(
            recipeId: "steel-sword",
            dropWeight: 10,
            minDungeonLevel: 3,
            maxDungeonLevel: null,
            lootSources: new[] { LootSourceType.Chest, LootSourceType.Boss },
            baseValue: 100);

        _mithrilBladeScrollConfig = RecipeScrollConfig.Create(
            recipeId: "mithril-blade",
            dropWeight: 2,
            minDungeonLevel: 8,
            maxDungeonLevel: null,
            lootSources: new[] { LootSourceType.Boss, LootSourceType.Quest },
            baseValue: 500);

        // Setup test recipes
        _steelSwordRecipe = RecipeDefinition.Create(
            recipeId: "steel-sword",
            name: "Steel Sword",
            description: "A sharp steel blade",
            category: RecipeCategory.Weapon,
            requiredStationId: "anvil",
            ingredients: new[] { new RecipeIngredient("iron-ore", 10) },
            output: new RecipeOutput("steel-sword", 1),
            difficultyClass: 14,
            isDefault: false);

        _mithrilBladeRecipe = RecipeDefinition.Create(
            recipeId: "mithril-blade",
            name: "Mithril Blade",
            description: "A legendary blade",
            category: RecipeCategory.Weapon,
            requiredStationId: "anvil",
            ingredients: new[] { new RecipeIngredient("mithril-ore", 5) },
            output: new RecipeOutput("mithril-blade", 1),
            difficultyClass: 18,
            isDefault: false);

        // Setup recipe provider mock
        _mockRecipeProvider.Setup(p => p.GetRecipe("steel-sword")).Returns(_steelSwordRecipe);
        _mockRecipeProvider.Setup(p => p.GetRecipe("mithril-blade")).Returns(_mithrilBladeRecipe);
    }

    // ═══════════════════════════════════════════════════════════════
    // TryGenerateRecipeScroll SUCCESS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TryGenerateRecipeScroll returns a scroll when eligible scrolls exist
    /// and the drop check passes.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_WithEligibleScrolls_ReturnsScroll()
    {
        // Arrange
        // Use a seeded random that will always succeed the drop check
        var deterministicRandom = new DeterministicRandom(0.1); // Below 0.15 chest drop chance

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Chest)).Returns(0.15m);
        _mockScrollProvider.Setup(p => p.GetEligibleScrolls(5, LootSourceType.Chest))
            .Returns(new List<RecipeScrollConfig> { _steelSwordScrollConfig });

        var service = CreateServiceWithRandom(deterministicRandom);
        var context = LootContext.Chest(5);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert
        scroll.Should().NotBeNull();
        scroll!.Name.Should().Contain("Steel Sword");
        scroll.RecipeId.Should().Be("steel-sword");
        scroll.Effect.Should().Be(ItemEffect.LearnRecipe);
        scroll.IsRecipeScroll.Should().BeTrue();
        scroll.Value.Should().Be(100);
    }

    /// <summary>
    /// Verifies weighted selection works correctly with multiple scrolls.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_WithMultipleScrolls_UsesWeightedSelection()
    {
        // Arrange
        // Seeded random: 0.1 for drop check (passes), then selection
        var deterministicRandom = new DeterministicRandom(0.1, 5); // Roll of 5 out of 12 total weight

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Boss)).Returns(0.30m);
        _mockScrollProvider.Setup(p => p.GetEligibleScrolls(8, LootSourceType.Boss))
            .Returns(new List<RecipeScrollConfig>
            {
                _steelSwordScrollConfig, // Weight 10
                _mithrilBladeScrollConfig // Weight 2
            });

        var service = CreateServiceWithRandom(deterministicRandom);
        var context = LootContext.Boss(8);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert - Roll of 5 should select steel-sword (cumulative 0-9 for weight 10)
        scroll.Should().NotBeNull();
        scroll!.RecipeId.Should().Be("steel-sword");
    }

    // ═══════════════════════════════════════════════════════════════
    // TryGenerateRecipeScroll FAILURE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that TryGenerateRecipeScroll returns null when drop check fails.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_DropCheckFails_ReturnsNull()
    {
        // Arrange
        var deterministicRandom = new DeterministicRandom(0.9); // Above 0.15 chest drop chance

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Chest)).Returns(0.15m);
        _mockScrollProvider.Setup(p => p.GetEligibleScrolls(5, LootSourceType.Chest))
            .Returns(new List<RecipeScrollConfig> { _steelSwordScrollConfig });

        var service = CreateServiceWithRandom(deterministicRandom);
        var context = LootContext.Chest(5);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert
        scroll.Should().BeNull();
    }

    /// <summary>
    /// Verifies that TryGenerateRecipeScroll returns null when no eligible scrolls exist.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_NoEligibleScrolls_ReturnsNull()
    {
        // Arrange
        var deterministicRandom = new DeterministicRandom(0.1); // Would pass drop check

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Chest)).Returns(0.15m);
        _mockScrollProvider.Setup(p => p.GetEligibleScrolls(1, LootSourceType.Chest))
            .Returns(new List<RecipeScrollConfig>()); // No eligible scrolls at level 1

        var service = CreateServiceWithRandom(deterministicRandom);
        var context = LootContext.Chest(1);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert
        scroll.Should().BeNull();
    }

    /// <summary>
    /// Verifies that TryGenerateRecipeScroll returns null when no scroll provider is configured.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_NoScrollProvider_ReturnsNull()
    {
        // Arrange - Service without scroll provider
        var service = new LootService(
            _mockConfigProvider.Object,
            _mockLogger.Object,
            _mockEventLogger.Object,
            scrollProvider: null,
            recipeProvider: null,
            recipeService: null);

        var context = LootContext.Chest(5);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert
        scroll.Should().BeNull();
    }

    /// <summary>
    /// Verifies that TryGenerateRecipeScroll returns null when no recipe provider is configured.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_NoRecipeProvider_ReturnsNull()
    {
        // Arrange - Service with scroll provider but no recipe provider
        var deterministicRandom = new DeterministicRandom(0.1);

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Chest)).Returns(0.15m);

        var service = new LootService(
            _mockConfigProvider.Object,
            _mockLogger.Object,
            _mockEventLogger.Object,
            deterministicRandom,
            _mockScrollProvider.Object,
            recipeProvider: null,
            recipeService: null);

        var context = LootContext.Chest(5);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert
        scroll.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // TryGenerateRecipeScroll WITH KNOWN RECIPE EXCLUSION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that known recipes are excluded when ExcludeKnownRecipes is true.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_WithExcludeKnownRecipes_FiltersKnownRecipes()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var deterministicRandom = new DeterministicRandom(0.1, 0); // Pass drop check, select first

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Boss)).Returns(0.30m);
        _mockScrollProvider.Setup(p => p.GetEligibleScrolls(8, LootSourceType.Boss))
            .Returns(new List<RecipeScrollConfig>
            {
                _steelSwordScrollConfig,
                _mithrilBladeScrollConfig
            });

        // Player knows steel-sword
        _mockRecipeService.Setup(s => s.IsRecipeKnown(player, "steel-sword")).Returns(true);
        _mockRecipeService.Setup(s => s.IsRecipeKnown(player, "mithril-blade")).Returns(false);

        var service = CreateServiceWithRandom(deterministicRandom);
        var context = LootContext.Boss(8, player, excludeKnown: true);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert - Should get mithril-blade since steel-sword is filtered out
        scroll.Should().NotBeNull();
        scroll!.RecipeId.Should().Be("mithril-blade");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetScrollDropChance TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetScrollDropChance returns configured drop chance.
    /// </summary>
    [Test]
    public void GetScrollDropChance_ReturnsConfiguredChance()
    {
        // Arrange
        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Chest)).Returns(0.15m);
        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Boss)).Returns(0.30m);

        var service = CreateService();

        // Act
        var chestChance = service.GetScrollDropChance(LootSourceType.Chest);
        var bossChance = service.GetScrollDropChance(LootSourceType.Boss);

        // Assert
        chestChance.Should().Be(0.15m);
        bossChance.Should().Be(0.30m);
    }

    /// <summary>
    /// Verifies that GetScrollDropChance returns 0 without scroll provider.
    /// </summary>
    [Test]
    public void GetScrollDropChance_NoScrollProvider_ReturnsZero()
    {
        // Arrange
        var service = new LootService(
            _mockConfigProvider.Object,
            _mockLogger.Object,
            _mockEventLogger.Object,
            scrollProvider: null,
            recipeProvider: null,
            recipeService: null);

        // Act
        var chance = service.GetScrollDropChance(LootSourceType.Boss);

        // Assert
        chance.Should().Be(0m);
    }

    // ═══════════════════════════════════════════════════════════════
    // EVENT LOGGING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that scroll generation logs an event.
    /// </summary>
    [Test]
    public void TryGenerateRecipeScroll_Success_LogsEvent()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var deterministicRandom = new DeterministicRandom(0.1);

        _mockScrollProvider.Setup(p => p.GetDropChance(LootSourceType.Chest)).Returns(0.15m);
        _mockScrollProvider.Setup(p => p.GetEligibleScrolls(5, LootSourceType.Chest))
            .Returns(new List<RecipeScrollConfig> { _steelSwordScrollConfig });

        var service = CreateServiceWithRandom(deterministicRandom);
        var context = LootContext.Chest(5, player);

        // Act
        var scroll = service.TryGenerateRecipeScroll(context);

        // Assert
        scroll.Should().NotBeNull();
        _mockEventLogger.Verify(
            e => e.LogInventory(
                "RecipeScrollGenerated",
                It.Is<string>(s => s.Contains("Steel Sword")),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private LootService CreateService()
    {
        return new LootService(
            _mockConfigProvider.Object,
            _mockLogger.Object,
            _mockEventLogger.Object,
            _mockScrollProvider.Object,
            _mockRecipeProvider.Object,
            _mockRecipeService.Object);
    }

    private LootService CreateServiceWithRandom(Random random)
    {
        return new LootService(
            _mockConfigProvider.Object,
            _mockLogger.Object,
            _mockEventLogger.Object,
            random,
            _mockScrollProvider.Object,
            _mockRecipeProvider.Object,
            _mockRecipeService.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // DETERMINISTIC RANDOM FOR TESTING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// A deterministic random implementation for testing that returns predetermined values.
    /// </summary>
    private sealed class DeterministicRandom : Random
    {
        private readonly double _doubleValue;
        private readonly int _intValue;
        private bool _doubleReturned;

        public DeterministicRandom(double doubleValue, int intValue = 0)
        {
            _doubleValue = doubleValue;
            _intValue = intValue;
            _doubleReturned = false;
        }

        public override double NextDouble()
        {
            _doubleReturned = true;
            return _doubleValue;
        }

        public override int Next(int maxValue)
        {
            return _intValue % maxValue;
        }
    }
}
