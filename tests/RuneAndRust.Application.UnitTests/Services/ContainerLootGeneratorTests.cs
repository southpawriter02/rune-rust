// =============================================================================
// ContainerLootGeneratorTests.cs
// Unit tests for ContainerLootGenerator.
// Version: 0.16.4d
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="ContainerLootGenerator"/>.
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item>Content generation with biome modifiers applied</item>
///   <item>Handling of already-looted containers</item>
///   <item>Handling of already-generated contents</item>
///   <item>Item count, tier, and currency calculations</item>
/// </list>
/// </remarks>
[TestFixture]
public class ContainerLootGeneratorTests
{
    // =========================================================================
    // Fields
    // =========================================================================

    /// <summary>
    /// Mock smart loot service.
    /// </summary>
    private Mock<ISmartLootService> _mockSmartLootService = null!;

    /// <summary>
    /// Mock logger for diagnostic output.
    /// </summary>
    private Mock<ILogger<ContainerLootGenerator>> _mockLogger = null!;

    /// <summary>
    /// Container type definitions for testing.
    /// </summary>
    private Dictionary<ContainerType, ContainerTypeDefinition> _containerDefinitions = null!;

    /// <summary>
    /// Biome modifiers for testing.
    /// </summary>
    private Dictionary<string, BiomeLootModifiers> _biomeModifiers = null!;

    /// <summary>
    /// Available items by tier for testing.
    /// </summary>
    private Dictionary<QualityTier, IReadOnlyList<LootEntry>> _availableItems = null!;

    /// <summary>
    /// The service under test.
    /// </summary>
    private ContainerLootGenerator _generator = null!;

    // =========================================================================
    // Setup
    // =========================================================================

    /// <summary>
    /// Sets up test fixtures before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockSmartLootService = new Mock<ISmartLootService>();
        _mockLogger = new Mock<ILogger<ContainerLootGenerator>>();

        // Create test container definitions
        _containerDefinitions = new Dictionary<ContainerType, ContainerTypeDefinition>
        {
            [ContainerType.SmallChest] = ContainerTypeDefinition.Create(
                type: ContainerType.SmallChest,
                minItems: 1,
                maxItems: 2,
                minTier: 0,
                maxTier: 1,
                minCurrency: 10,
                maxCurrency: 50),
            [ContainerType.LargeChest] = ContainerTypeDefinition.Create(
                type: ContainerType.LargeChest,
                minItems: 2,
                maxItems: 4,
                minTier: 1,
                maxTier: 2,
                minCurrency: 50,
                maxCurrency: 150)
        };


        // Create test biome modifiers
        _biomeModifiers = new Dictionary<string, BiomeLootModifiers>
        {
            ["the-roots"] = BiomeLootModifiers.Default,
            ["alfheim"] = BiomeLootModifiers.Create(
                biomeId: "alfheim",
                goldMultiplier: 1.5m,
                dropRateMultiplier: 0.7m,
                qualityBonus: 1,
                rareChanceBonusPercent: 15)
        };

        // Create test loot entries
        var tier0Items = new List<LootEntry>
        {
            LootEntry.Create("rusty-sword", "swords"),
            LootEntry.Create("worn-shield", "shields")
        };

        var tier1Items = new List<LootEntry>
        {
            LootEntry.Create("iron-sword", "swords"),
            LootEntry.Create("iron-shield", "shields")
        };

        _availableItems = new Dictionary<QualityTier, IReadOnlyList<LootEntry>>
        {
            [QualityTier.JuryRigged] = tier0Items,
            [QualityTier.Scavenged] = tier1Items,
            [QualityTier.ClanForged] = tier1Items
        };

        // Configure smart loot service mock
        _mockSmartLootService
            .Setup(s => s.SelectItem(It.IsAny<SmartLootContext>()))
            .Returns((SmartLootContext ctx) =>
                ctx.HasAvailableItems
                    ? SmartLootResult.CreateRandomOnly(ctx.AvailableItems[0], ctx.AvailableItemCount)
                    : SmartLootResult.Empty);

        // Create generator with seeded random for deterministic tests
        _generator = new ContainerLootGenerator(
            _mockSmartLootService.Object,
            _mockLogger.Object,
            _containerDefinitions,
            _biomeModifiers,
            _availableItems,
            new Random(42)); // Seeded for determinism
    }

    // =========================================================================
    // GenerateContents Tests
    // =========================================================================

    /// <summary>
    /// Verifies that generating contents for a new container creates items and currency.
    /// </summary>
    [Test]
    public void GenerateContents_ForNewContainer_GeneratesItemsAndCurrency()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        var playerId = Guid.NewGuid();

        // Act
        var contents = _generator.GenerateContents(container, playerId, "the-roots");

        // Assert
        contents.HasContents.Should().BeTrue();
        contents.ItemCount.Should().BeInRange(1, 2);
        contents.CurrencyAmount.Should().BeInRange(10, 50);
        contents.AppliedTier.Should().BeInRange(0, 1);
    }

    /// <summary>
    /// Verifies that biome modifiers are applied to gold calculations.
    /// </summary>
    [Test]
    public void GenerateContents_WithBiomeModifiers_AppliesGoldMultiplier()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        var playerId = Guid.NewGuid();

        // Act - Use alfheim biome with 1.5x gold multiplier
        var contents = _generator.GenerateContents(container, playerId, "alfheim");

        // Assert - With 1.5x multiplier, min should be floor(10 * 1.5) = 15
        // Note: Due to seeded random, we verify the modifier was applied
        contents.CurrencyAmount.Should().BeGreaterThanOrEqualTo(15);
    }

    /// <summary>
    /// Verifies that looted containers return empty contents.
    /// </summary>
    [Test]
    public void GenerateContents_ForLootedContainer_ReturnsEmpty()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        var playerId = Guid.NewGuid();

        // First, generate and loot the contents
        _ = _generator.GenerateContents(container, playerId, "the-roots");
        _ = container.Loot();

        // Act
        var contents = _generator.GenerateContents(container, playerId, "the-roots");

        // Assert
        contents.Should().Be(ContainerContents.Empty);
        contents.HasContents.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that already-generated contents are returned without regeneration.
    /// </summary>
    [Test]
    public void GenerateContents_WithExistingContents_ReturnsSameContents()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        var playerId = Guid.NewGuid();

        // Act - Generate twice
        var firstContents = _generator.GenerateContents(container, playerId, "the-roots");
        var secondContents = _generator.GenerateContents(container, playerId, "the-roots");

        // Assert
        secondContents.Should().Be(firstContents);
    }

    // =========================================================================
    // GetContainerDefinition Tests
    // =========================================================================

    /// <summary>
    /// Verifies that GetContainerDefinition returns the correct definition.
    /// </summary>
    [Test]
    public void GetContainerDefinition_WithValidType_ReturnsDefinition()
    {
        // Act
        var definition = _generator.GetContainerDefinition(ContainerType.SmallChest);

        // Assert
        definition.Type.Should().Be(ContainerType.SmallChest);
        definition.MinItems.Should().Be(1);
        definition.MaxItems.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GetContainerDefinition throws for unknown container types.
    /// </summary>
    [Test]
    public void GetContainerDefinition_WithUnknownType_ThrowsKeyNotFoundException()
    {
        // Act
        var act = () => _generator.GetContainerDefinition(ContainerType.BossChest);

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    // =========================================================================
    // GetBiomeModifiers Tests
    // =========================================================================

    /// <summary>
    /// Verifies that GetBiomeModifiers returns the correct modifiers.
    /// </summary>
    [Test]
    public void GetBiomeModifiers_WithKnownBiome_ReturnsModifiers()
    {
        // Act
        var modifiers = _generator.GetBiomeModifiers("alfheim");

        // Assert
        modifiers.GoldMultiplier.Should().Be(1.5m);
        modifiers.QualityBonus.Should().Be(1);
    }

    /// <summary>
    /// Verifies that GetBiomeModifiers returns defaults for unknown biomes.
    /// </summary>
    [Test]
    public void GetBiomeModifiers_WithUnknownBiome_ReturnsDefault()
    {
        // Act
        var modifiers = _generator.GetBiomeModifiers("unknown-biome");

        // Assert
        modifiers.GoldMultiplier.Should().Be(1.0m);
        modifiers.QualityBonus.Should().Be(0);
    }

    // =========================================================================
    // HasGeneratedContents Tests
    // =========================================================================

    /// <summary>
    /// Verifies HasGeneratedContents returns false for new containers.
    /// </summary>
    [Test]
    public void HasGeneratedContents_ForNewContainer_ReturnsFalse()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);

        // Act
        var hasContents = _generator.HasGeneratedContents(container);

        // Assert
        hasContents.Should().BeFalse();
    }

    /// <summary>
    /// Verifies HasGeneratedContents returns true after generation.
    /// </summary>
    [Test]
    public void HasGeneratedContents_AfterGeneration_ReturnsTrue()
    {
        // Arrange
        var container = ContainerLootTable.Create(ContainerType.SmallChest);
        container.Open();
        _ = _generator.GenerateContents(container, Guid.NewGuid(), "the-roots");

        // Act
        var hasContents = _generator.HasGeneratedContents(container);

        // Assert
        hasContents.Should().BeTrue();
    }
}
