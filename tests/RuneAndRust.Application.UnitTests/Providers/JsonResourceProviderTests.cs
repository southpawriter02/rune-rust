using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Providers;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for JsonResourceProvider (v0.11.0a).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading resource definitions from JSON configuration</description></item>
///   <item><description>Resource lookup by ID (case-insensitive)</description></item>
///   <item><description>Filtering by category and quality</description></item>
///   <item><description>Existence checks and ID enumeration</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class JsonResourceProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<JsonResourceProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JsonResourceProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "gatherable-resources.json");
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR AND LOADING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the provider loads all resources from the config file.
    /// </summary>
    [Test]
    public void Constructor_LoadsResourcesFromConfiguration()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange & Act
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Assert
        provider.Count.Should().BeGreaterOrEqualTo(16); // We defined 16 resources in config
    }

    /// <summary>
    /// Verifies that constructor throws when config path is null.
    /// </summary>
    [Test]
    public void Constructor_NullConfigPath_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new JsonResourceProvider(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configPath");
    }

    /// <summary>
    /// Verifies that constructor throws when logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new JsonResourceProvider(_testConfigPath, null!);

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
        var act = () => new JsonResourceProvider("/nonexistent/path/resources.json", _mockLogger.Object);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetResource TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetResource returns the correct resource by ID.
    /// </summary>
    [Test]
    public void GetResource_WithExistingId_ReturnsDefinition()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var resource = provider.GetResource("iron-ore");

        // Assert
        resource.Should().NotBeNull();
        resource!.Name.Should().Be("Iron Ore");
        resource.Category.Should().Be(ResourceCategory.Ore);
        resource.Quality.Should().Be(ResourceQuality.Common);
        resource.BaseValue.Should().Be(5);
        resource.StackSize.Should().Be(20);
    }

    /// <summary>
    /// Verifies that GetResource returns null for unknown resource IDs.
    /// </summary>
    [Test]
    public void GetResource_WithNonExistingId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var resource = provider.GetResource("nonexistent-resource");

        // Assert
        resource.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetResource performs case-insensitive lookup.
    /// </summary>
    [Test]
    public void GetResource_CaseInsensitive_ReturnsResource()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var resource = provider.GetResource("IRON-ORE");

        // Assert
        resource.Should().NotBeNull();
        resource!.ResourceId.Should().Be("iron-ore");
    }

    /// <summary>
    /// Verifies that GetResource returns null for null or empty ID.
    /// </summary>
    [Test]
    public void GetResource_WithNullOrEmptyId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.GetResource(null!).Should().BeNull();
        provider.GetResource("").Should().BeNull();
        provider.GetResource("   ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllResources TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllResources returns all loaded resources.
    /// </summary>
    [Test]
    public void GetAllResources_ReturnsAllLoadedResources()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var resources = provider.GetAllResources();

        // Assert
        resources.Should().NotBeEmpty();
        resources.Should().HaveCountGreaterOrEqualTo(16);
        resources.Should().Contain(r => r.ResourceId == "iron-ore");
        resources.Should().Contain(r => r.ResourceId == "gold-ore");
        resources.Should().Contain(r => r.ResourceId == "healing-herb");
        resources.Should().Contain(r => r.ResourceId == "dragon-scale");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetResourcesByCategory TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetResourcesByCategory returns filtered resources.
    /// </summary>
    [Test]
    public void GetResourcesByCategory_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ores = provider.GetResourcesByCategory(ResourceCategory.Ore);
        var herbs = provider.GetResourcesByCategory(ResourceCategory.Herb);
        var gems = provider.GetResourcesByCategory(ResourceCategory.Gem);

        // Assert
        ores.Should().HaveCountGreaterOrEqualTo(4); // iron, copper, gold, mithril
        ores.Should().OnlyContain(r => r.Category == ResourceCategory.Ore);

        herbs.Should().HaveCountGreaterOrEqualTo(3); // healing-herb, mana-leaf, firebloom
        herbs.Should().OnlyContain(r => r.Category == ResourceCategory.Herb);

        gems.Should().HaveCountGreaterOrEqualTo(2); // ruby, sapphire
        gems.Should().OnlyContain(r => r.Category == ResourceCategory.Gem);
    }

    /// <summary>
    /// Verifies all categories are represented in the loaded resources.
    /// </summary>
    [Test]
    public void GetResourcesByCategory_AllCategoriesRepresented()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert - Each category should have at least one resource
        provider.GetResourcesByCategory(ResourceCategory.Ore).Should().NotBeEmpty();
        provider.GetResourcesByCategory(ResourceCategory.Herb).Should().NotBeEmpty();
        provider.GetResourcesByCategory(ResourceCategory.Leather).Should().NotBeEmpty();
        provider.GetResourcesByCategory(ResourceCategory.Gem).Should().NotBeEmpty();
        provider.GetResourcesByCategory(ResourceCategory.Wood).Should().NotBeEmpty();
        provider.GetResourcesByCategory(ResourceCategory.Cloth).Should().NotBeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetResourcesByQuality TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetResourcesByQuality returns filtered resources.
    /// </summary>
    [Test]
    public void GetResourcesByQuality_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var common = provider.GetResourcesByQuality(ResourceQuality.Common);
        var fine = provider.GetResourcesByQuality(ResourceQuality.Fine);
        var rare = provider.GetResourcesByQuality(ResourceQuality.Rare);

        // Assert
        common.Should().NotBeEmpty();
        common.Should().OnlyContain(r => r.Quality == ResourceQuality.Common);

        fine.Should().NotBeEmpty();
        fine.Should().OnlyContain(r => r.Quality == ResourceQuality.Fine);

        rare.Should().NotBeEmpty();
        rare.Should().OnlyContain(r => r.Quality == ResourceQuality.Rare);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetResources (Combined Filter) TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetResources filters by category and minimum quality.
    /// </summary>
    [Test]
    public void GetResources_FiltersByCategoryAndMinimumQuality()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act - Get Fine or better Ores
        var fineOres = provider.GetResources(ResourceCategory.Ore, ResourceQuality.Fine);

        // Assert
        fineOres.Should().NotBeEmpty();
        fineOres.Should().OnlyContain(r => r.Category == ResourceCategory.Ore);
        fineOres.Should().OnlyContain(r => r.Quality >= ResourceQuality.Fine);
        fineOres.Should().Contain(r => r.ResourceId == "gold-ore"); // Fine quality
        fineOres.Should().Contain(r => r.ResourceId == "mithril-ore"); // Rare quality
        fineOres.Should().NotContain(r => r.ResourceId == "iron-ore"); // Common quality
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
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.Exists("iron-ore").Should().BeTrue();
        provider.Exists("IRON-ORE").Should().BeTrue(); // Case-insensitive
        provider.Exists("gold-ore").Should().BeTrue();
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
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.Exists("nonexistent-resource").Should().BeFalse();
        provider.Exists(null!).Should().BeFalse();
        provider.Exists("").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetResourceIds TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetResourceIds returns all resource IDs.
    /// </summary>
    [Test]
    public void GetResourceIds_ReturnsAllIds()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ids = provider.GetResourceIds();

        // Assert
        ids.Should().HaveCountGreaterOrEqualTo(16);
        ids.Should().Contain("iron-ore");
        ids.Should().Contain("gold-ore");
        ids.Should().Contain("healing-herb");
        ids.Should().Contain("dragon-scale");
        ids.Should().Contain("ruby");
        ids.Should().Contain("oak-wood");
        ids.Should().Contain("linen");
    }

    // ═══════════════════════════════════════════════════════════════
    // VALUE CALCULATION INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that loaded resources calculate values correctly.
    /// </summary>
    [Test]
    public void GetResource_LoadedResourceCalculatesValuesCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ironOre = provider.GetResource("iron-ore"); // Common, BaseValue 5
        var goldOre = provider.GetResource("gold-ore"); // Fine, BaseValue 25
        var mithrilOre = provider.GetResource("mithril-ore"); // Rare, BaseValue 100

        // Assert
        ironOre.Should().NotBeNull();
        ironOre!.GetActualValue().Should().Be(5); // 5 * 1.0 = 5
        ironOre.GetCraftingBonus().Should().Be(0); // Common = 0

        goldOre.Should().NotBeNull();
        goldOre!.GetActualValue().Should().Be(37); // 25 * 1.5 = 37.5 -> 37
        goldOre.GetCraftingBonus().Should().Be(1); // Fine = 1

        mithrilOre.Should().NotBeNull();
        mithrilOre!.GetActualValue().Should().Be(300); // 100 * 3.0 = 300
        mithrilOre.GetCraftingBonus().Should().Be(2); // Rare = 2
    }

    /// <summary>
    /// Verifies that loaded resources include icon paths when specified.
    /// </summary>
    [Test]
    public void GetResource_LoadsIconPathWhenSpecified()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ironOre = provider.GetResource("iron-ore");

        // Assert
        ironOre.Should().NotBeNull();
        ironOre!.IconPath.Should().Be("icons/resources/iron_ore.png");
    }

    // ═══════════════════════════════════════════════════════════════
    // STACKING INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that resources from the same definition can stack.
    /// </summary>
    [Test]
    public void GetResource_SameResourceCanStack()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new JsonResourceProvider(_testConfigPath, _mockLogger.Object);
        var ironOre1 = provider.GetResource("iron-ore");
        var ironOre2 = provider.GetResource("iron-ore");
        var copperOre = provider.GetResource("copper-ore");

        // Assert
        ironOre1.Should().NotBeNull();
        ironOre2.Should().NotBeNull();
        copperOre.Should().NotBeNull();

        // Same resource type can stack
        ironOre1!.CanStackWith(ironOre2!).Should().BeTrue();

        // Different resource types cannot stack
        ironOre1.CanStackWith(copperOre!).Should().BeFalse();
    }
}
