using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Providers;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for JsonHarvestableFeatureProvider (v0.11.0b).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading feature definitions from JSON configuration</description></item>
///   <item><description>Feature lookup by ID (case-insensitive)</description></item>
///   <item><description>Filtering by resource, tool, and difficulty</description></item>
///   <item><description>Creating feature instances from definitions</description></item>
///   <item><description>Resource ID validation during loading</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class JsonHarvestableFeatureProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<JsonHarvestableFeatureProvider>> _mockLogger = null!;
    private Mock<IResourceProvider> _mockResourceProvider = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JsonHarvestableFeatureProvider>>();

        // Mock resource provider to validate resource references
        _mockResourceProvider = new Mock<IResourceProvider>();
        _mockResourceProvider.Setup(r => r.Exists("iron-ore")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("copper-ore")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("gold-ore")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("healing-herb")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("mana-leaf")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("leather")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("ruby")).Returns(true);
        _mockResourceProvider.Setup(r => r.Exists("oak-wood")).Returns(true);

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "harvestable-features.json");
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR AND LOADING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the provider loads all features from the config file.
    /// </summary>
    [Test]
    public void Constructor_LoadsFeaturesFromConfiguration()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange & Act
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Assert
        provider.Count.Should().Be(8); // We defined 8 features in config
    }

    /// <summary>
    /// Verifies that constructor throws when config path is null.
    /// </summary>
    [Test]
    public void Constructor_NullConfigPath_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new JsonHarvestableFeatureProvider(
            null!,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configPath");
    }

    /// <summary>
    /// Verifies that constructor throws when resource provider is null.
    /// </summary>
    [Test]
    public void Constructor_NullResourceProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new JsonHarvestableFeatureProvider(
            _testConfigPath,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("resourceProvider");
    }

    /// <summary>
    /// Verifies that constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that constructor throws when config file doesn't exist.
    /// </summary>
    [Test]
    public void Constructor_NonexistentFile_ThrowsFileNotFoundException()
    {
        // Arrange & Act
        var act = () => new JsonHarvestableFeatureProvider(
            "/nonexistent/path/harvestable-features.json",
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetFeature TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetFeature returns the correct feature by ID.
    /// </summary>
    [Test]
    public void GetFeature_WithExistingId_ReturnsDefinition()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var feature = provider.GetFeature("iron-ore-vein");

        // Assert
        feature.Should().NotBeNull();
        feature!.Name.Should().Be("Iron Ore Vein");
        feature.ResourceId.Should().Be("iron-ore");
        feature.MinQuantity.Should().Be(1);
        feature.MaxQuantity.Should().Be(5);
        feature.DifficultyClass.Should().Be(12);
        feature.RequiredToolId.Should().BeNull();
        feature.ReplenishTurns.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetFeature returns null for unknown feature IDs.
    /// </summary>
    [Test]
    public void GetFeature_WithNonExistingId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var feature = provider.GetFeature("nonexistent-feature");

        // Assert
        feature.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetFeature performs case-insensitive lookup.
    /// </summary>
    [Test]
    public void GetFeature_CaseInsensitive_ReturnsFeature()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var feature = provider.GetFeature("IRON-ORE-VEIN");

        // Assert
        feature.Should().NotBeNull();
        feature!.FeatureId.Should().Be("iron-ore-vein");
    }

    /// <summary>
    /// Verifies that GetFeature returns null for null or empty ID.
    /// </summary>
    [Test]
    public void GetFeature_WithNullOrEmptyId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act & Assert
        provider.GetFeature(null!).Should().BeNull();
        provider.GetFeature("").Should().BeNull();
        provider.GetFeature("   ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllFeatures TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllFeatures returns all loaded features.
    /// </summary>
    [Test]
    public void GetAllFeatures_ReturnsAllLoadedFeatures()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var features = provider.GetAllFeatures();

        // Assert
        features.Should().HaveCount(8);
        features.Should().Contain(f => f.FeatureId == "iron-ore-vein");
        features.Should().Contain(f => f.FeatureId == "copper-ore-vein");
        features.Should().Contain(f => f.FeatureId == "gold-ore-vein");
        features.Should().Contain(f => f.FeatureId == "herb-patch");
        features.Should().Contain(f => f.FeatureId == "mana-leaf-cluster");
        features.Should().Contain(f => f.FeatureId == "leather-source");
        features.Should().Contain(f => f.FeatureId == "gem-cluster");
        features.Should().Contain(f => f.FeatureId == "fallen-tree");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetFeaturesByResource TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetFeaturesByResource returns filtered features.
    /// </summary>
    [Test]
    public void GetFeaturesByResource_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var ironFeatures = provider.GetFeaturesByResource("iron-ore");

        // Assert
        ironFeatures.Should().HaveCount(1);
        ironFeatures.Should().OnlyContain(f => f.ResourceId == "iron-ore");
        ironFeatures[0].FeatureId.Should().Be("iron-ore-vein");
    }

    /// <summary>
    /// Verifies case-insensitive resource ID filtering.
    /// </summary>
    [Test]
    public void GetFeaturesByResource_CaseInsensitive_ReturnsFeatures()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var features = provider.GetFeaturesByResource("IRON-ORE");

        // Assert
        features.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that null or empty resource ID returns empty list.
    /// </summary>
    [Test]
    public void GetFeaturesByResource_WithNullOrEmptyId_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act & Assert
        provider.GetFeaturesByResource(null!).Should().BeEmpty();
        provider.GetFeaturesByResource("").Should().BeEmpty();
        provider.GetFeaturesByResource("   ").Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetFeaturesByTool TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetFeaturesByTool returns filtered features.
    /// </summary>
    [Test]
    public void GetFeaturesByTool_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var pickaxeFeatures = provider.GetFeaturesByTool("pickaxe");

        // Assert
        pickaxeFeatures.Should().HaveCount(2); // gold-ore-vein and gem-cluster
        pickaxeFeatures.Should().OnlyContain(f => f.RequiredToolId == "pickaxe");
    }

    /// <summary>
    /// Verifies that null or empty tool ID returns empty list.
    /// </summary>
    [Test]
    public void GetFeaturesByTool_WithNullOrEmptyId_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act & Assert
        provider.GetFeaturesByTool(null!).Should().BeEmpty();
        provider.GetFeaturesByTool("").Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetFeaturesByDifficulty TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetFeaturesByDifficulty returns filtered features.
    /// </summary>
    [Test]
    public void GetFeaturesByDifficulty_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act - Get easy features (DC 10)
        var easyFeatures = provider.GetFeaturesByDifficulty(10, 10);

        // Assert - copper-ore-vein, herb-patch, fallen-tree all have DC 10
        easyFeatures.Should().HaveCount(3);
        easyFeatures.Should().OnlyContain(f => f.DifficultyClass == 10);
    }

    /// <summary>
    /// Verifies range filtering for difficulty.
    /// </summary>
    [Test]
    public void GetFeaturesByDifficulty_WithRange_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act - Get features with DC 10-12
        var features = provider.GetFeaturesByDifficulty(10, 12);

        // Assert - Should include DC 10, 11, and 12 features
        features.Should().NotBeEmpty();
        features.Should().OnlyContain(f => f.DifficultyClass >= 10 && f.DifficultyClass <= 12);
    }

    // ═══════════════════════════════════════════════════════════════
    // Exists TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Exists returns correct results.
    /// </summary>
    [Test]
    public void Exists_WithExistingId_ReturnsTrue()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act & Assert
        provider.Exists("iron-ore-vein").Should().BeTrue();
        provider.Exists("IRON-ORE-VEIN").Should().BeTrue(); // Case-insensitive
        provider.Exists("herb-patch").Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Exists returns false for non-existing IDs.
    /// </summary>
    [Test]
    public void Exists_WithNonExistingId_ReturnsFalse()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act & Assert
        provider.Exists("nonexistent-feature").Should().BeFalse();
        provider.Exists(null!).Should().BeFalse();
        provider.Exists("").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetFeatureIds TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetFeatureIds returns all feature IDs.
    /// </summary>
    [Test]
    public void GetFeatureIds_ReturnsAllIds()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var ids = provider.GetFeatureIds();

        // Assert
        ids.Should().HaveCount(8);
        ids.Should().Contain("iron-ore-vein");
        ids.Should().Contain("copper-ore-vein");
        ids.Should().Contain("gold-ore-vein");
        ids.Should().Contain("herb-patch");
        ids.Should().Contain("mana-leaf-cluster");
        ids.Should().Contain("leather-source");
        ids.Should().Contain("gem-cluster");
        ids.Should().Contain("fallen-tree");
    }

    // ═══════════════════════════════════════════════════════════════
    // CreateFeatureInstance TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateFeatureInstance creates a feature from definition.
    /// </summary>
    [Test]
    public void CreateFeatureInstance_WithExistingId_ReturnsInstance()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var instance = provider.CreateFeatureInstance("iron-ore-vein");

        // Assert
        instance.Should().NotBeNull();
        instance!.DefinitionId.Should().Be("iron-ore-vein");
        instance.Name.Should().Be("Iron Ore Vein");
        instance.RemainingQuantity.Should().BeInRange(1, 5); // Random quantity in range
        instance.IsInteractable.Should().BeTrue();
        instance.InteractionVerb.Should().Be("gather");
    }

    /// <summary>
    /// Verifies that CreateFeatureInstance returns null for unknown ID.
    /// </summary>
    [Test]
    public void CreateFeatureInstance_WithNonExistingId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var instance = provider.CreateFeatureInstance("nonexistent-feature");

        // Assert
        instance.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateFeatureInstance uses provided random generator.
    /// </summary>
    [Test]
    public void CreateFeatureInstance_WithSeededRandom_ProducesReproducibleResults()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Create two instances with same seed
        var random1 = new Random(42);
        var random2 = new Random(42);

        // Act
        var instance1 = provider.CreateFeatureInstance("iron-ore-vein", random1);
        var instance2 = provider.CreateFeatureInstance("iron-ore-vein", random2);

        // Assert
        instance1.Should().NotBeNull();
        instance2.Should().NotBeNull();
        instance1!.RemainingQuantity.Should().Be(instance2!.RemainingQuantity);
    }

    // ═══════════════════════════════════════════════════════════════
    // FEATURE PROPERTIES INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that features with required tools load correctly.
    /// </summary>
    [Test]
    public void GetFeature_WithRequiredTool_LoadsToolRequirement()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var goldVein = provider.GetFeature("gold-ore-vein");
        var gemCluster = provider.GetFeature("gem-cluster");
        var leatherSource = provider.GetFeature("leather-source");
        var fallenTree = provider.GetFeature("fallen-tree");

        // Assert
        goldVein.Should().NotBeNull();
        goldVein!.RequiredToolId.Should().Be("pickaxe");
        goldVein.RequiresTool.Should().BeTrue();

        gemCluster.Should().NotBeNull();
        gemCluster!.RequiredToolId.Should().Be("pickaxe");

        leatherSource.Should().NotBeNull();
        leatherSource!.RequiredToolId.Should().Be("skinning-knife");

        fallenTree.Should().NotBeNull();
        fallenTree!.RequiredToolId.Should().Be("axe");
    }

    /// <summary>
    /// Verifies that replenishing features load correctly.
    /// </summary>
    [Test]
    public void GetFeature_WithReplenishment_LoadsReplenishTurns()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var herbPatch = provider.GetFeature("herb-patch");
        var manaLeaf = provider.GetFeature("mana-leaf-cluster");
        var ironVein = provider.GetFeature("iron-ore-vein");

        // Assert
        herbPatch.Should().NotBeNull();
        herbPatch!.ReplenishTurns.Should().Be(100);
        herbPatch.Replenishes.Should().BeTrue();

        manaLeaf.Should().NotBeNull();
        manaLeaf!.ReplenishTurns.Should().Be(150);
        manaLeaf.Replenishes.Should().BeTrue();

        ironVein.Should().NotBeNull();
        ironVein!.ReplenishTurns.Should().Be(0);
        ironVein.Replenishes.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that features with icons load the icon path.
    /// </summary>
    [Test]
    public void GetFeature_WithIcon_LoadsIconPath()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonHarvestableFeatureProvider(
            _testConfigPath,
            _mockResourceProvider.Object,
            _mockLogger.Object);

        // Act
        var ironVein = provider.GetFeature("iron-ore-vein");
        var herbPatch = provider.GetFeature("herb-patch");

        // Assert
        ironVein.Should().NotBeNull();
        ironVein!.IconPath.Should().Be("icons/features/ore_vein.png");

        herbPatch.Should().NotBeNull();
        herbPatch!.IconPath.Should().Be("icons/features/herb_patch.png");
    }
}
