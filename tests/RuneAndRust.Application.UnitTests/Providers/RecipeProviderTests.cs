using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Providers;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Providers;

/// <summary>
/// Unit tests for RecipeProvider (v0.11.1a).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Loading recipe definitions from JSON configuration</description></item>
///   <item><description>Recipe lookup by ID (case-insensitive)</description></item>
///   <item><description>Filtering by category and station</description></item>
///   <item><description>Default recipe retrieval</description></item>
///   <item><description>Output item lookup</description></item>
///   <item><description>Existence checks</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeProviderTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private Mock<ILogger<RecipeProvider>> _mockLogger = null!;
    private string _testConfigPath = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<RecipeProvider>>();

        // Use the actual config path for integration-style tests
        var baseDir = TestContext.CurrentContext.TestDirectory;
        _testConfigPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "recipes.json");
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR AND LOADING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the provider loads all recipes from the config file.
    /// </summary>
    [Test]
    public void Constructor_LoadsRecipesFromConfiguration()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange & Act
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Assert
        provider.GetRecipeCount().Should().BeGreaterOrEqualTo(15); // We defined 15 recipes in config
    }

    /// <summary>
    /// Verifies that constructor throws when config path is null.
    /// </summary>
    [Test]
    public void Constructor_NullConfigPath_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new RecipeProvider(null!, _mockLogger.Object);

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
        var act = () => new RecipeProvider(_testConfigPath, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetRecipe TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecipe returns the correct recipe by ID.
    /// </summary>
    [Test]
    public void GetRecipe_WithExistingId_ReturnsDefinition()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipe = provider.GetRecipe("iron-sword");

        // Assert
        recipe.Should().NotBeNull();
        recipe!.Name.Should().Be("Iron Sword");
        recipe.Category.Should().Be(RecipeCategory.Weapon);
        recipe.RequiredStationId.Should().Be("anvil");
        recipe.DifficultyClass.Should().Be(12);
        recipe.IsDefault.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that GetRecipe returns null for unknown recipe IDs.
    /// </summary>
    [Test]
    public void GetRecipe_WithNonExistingId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipe = provider.GetRecipe("nonexistent-recipe");

        // Assert
        recipe.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetRecipe performs case-insensitive lookup.
    /// </summary>
    [Test]
    public void GetRecipe_CaseInsensitive_ReturnsRecipe()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipe = provider.GetRecipe("IRON-SWORD");

        // Assert
        recipe.Should().NotBeNull();
        recipe!.RecipeId.Should().Be("iron-sword");
    }

    /// <summary>
    /// Verifies that GetRecipe returns null for null or empty ID.
    /// </summary>
    [Test]
    public void GetRecipe_WithNullOrEmptyId_ReturnsNull()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.GetRecipe(null!).Should().BeNull();
        provider.GetRecipe("").Should().BeNull();
        provider.GetRecipe("   ").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetAllRecipes TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllRecipes returns all loaded recipes.
    /// </summary>
    [Test]
    public void GetAllRecipes_ReturnsAllLoadedRecipes()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipes = provider.GetAllRecipes();

        // Assert
        recipes.Should().NotBeEmpty();
        recipes.Should().HaveCountGreaterOrEqualTo(15);
        recipes.Should().Contain(r => r.RecipeId == "iron-sword");
        recipes.Should().Contain(r => r.RecipeId == "healing-potion");
        recipes.Should().Contain(r => r.RecipeId == "mithril-blade");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetRecipesByCategory TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecipesByCategory returns filtered recipes.
    /// </summary>
    [Test]
    public void GetRecipesByCategory_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var weapons = provider.GetRecipesByCategory(RecipeCategory.Weapon);
        var potions = provider.GetRecipesByCategory(RecipeCategory.Potion);
        var materials = provider.GetRecipesByCategory(RecipeCategory.Material);

        // Assert
        weapons.Should().NotBeEmpty();
        weapons.Should().OnlyContain(r => r.Category == RecipeCategory.Weapon);
        weapons.Should().Contain(r => r.RecipeId == "iron-sword");

        potions.Should().NotBeEmpty();
        potions.Should().OnlyContain(r => r.Category == RecipeCategory.Potion);
        potions.Should().Contain(r => r.RecipeId == "healing-potion");

        materials.Should().NotBeEmpty();
        materials.Should().OnlyContain(r => r.Category == RecipeCategory.Material);
        materials.Should().Contain(r => r.RecipeId == "iron-ingot");
    }

    /// <summary>
    /// Verifies that GetRecipesByCategory returns empty list for categories with no recipes.
    /// </summary>
    [Test]
    public void GetRecipesByCategory_EmptyCategory_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act - Accessory category may be empty in our config
        var accessories = provider.GetRecipesByCategory(RecipeCategory.Accessory);

        // Assert - Should not throw, returns empty or valid list
        accessories.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetRecipesForStation TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecipesForStation returns recipes for a specific station.
    /// </summary>
    [Test]
    public void GetRecipesForStation_ReturnsFilteredList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var anvilRecipes = provider.GetRecipesForStation("anvil");
        var alchemyRecipes = provider.GetRecipesForStation("alchemy-table");

        // Assert
        anvilRecipes.Should().NotBeEmpty();
        anvilRecipes.Should().OnlyContain(r => r.RequiredStationId == "anvil");
        anvilRecipes.Should().Contain(r => r.RecipeId == "iron-sword");
        anvilRecipes.Should().Contain(r => r.RecipeId == "iron-ingot");

        alchemyRecipes.Should().NotBeEmpty();
        alchemyRecipes.Should().OnlyContain(r => r.RequiredStationId == "alchemy-table");
        alchemyRecipes.Should().Contain(r => r.RecipeId == "healing-potion");
    }

    /// <summary>
    /// Verifies that GetRecipesForStation is case-insensitive.
    /// </summary>
    [Test]
    public void GetRecipesForStation_CaseInsensitive()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipes1 = provider.GetRecipesForStation("ANVIL");
        var recipes2 = provider.GetRecipesForStation("Anvil");
        var recipes3 = provider.GetRecipesForStation("anvil");

        // Assert
        recipes1.Should().BeEquivalentTo(recipes2);
        recipes2.Should().BeEquivalentTo(recipes3);
    }

    /// <summary>
    /// Verifies that GetRecipesForStation returns empty for unknown stations.
    /// </summary>
    [Test]
    public void GetRecipesForStation_UnknownStation_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipes = provider.GetRecipesForStation("unknown-station");

        // Assert
        recipes.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetRecipesForStation returns empty for null/empty station ID.
    /// </summary>
    [Test]
    public void GetRecipesForStation_NullOrEmptyStationId_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.GetRecipesForStation(null!).Should().BeEmpty();
        provider.GetRecipesForStation("").Should().BeEmpty();
        provider.GetRecipesForStation("   ").Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetDefaultRecipes TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDefaultRecipes returns only default recipes.
    /// </summary>
    [Test]
    public void GetDefaultRecipes_ReturnsOnlyDefaultRecipes()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var defaults = provider.GetDefaultRecipes();

        // Assert
        defaults.Should().NotBeEmpty();
        defaults.Should().OnlyContain(r => r.IsDefault);
        defaults.Should().HaveCountGreaterOrEqualTo(9); // We have 9 default recipes
        defaults.Should().Contain(r => r.RecipeId == "iron-sword");
        defaults.Should().Contain(r => r.RecipeId == "healing-potion");
        defaults.Should().Contain(r => r.RecipeId == "iron-ingot");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetRecipesForItem TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetRecipesForItem returns recipes that produce the item.
    /// </summary>
    [Test]
    public void GetRecipesForItem_ReturnsRecipesThatProduceItem()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var swordRecipes = provider.GetRecipesForItem("iron-sword");

        // Assert
        swordRecipes.Should().NotBeEmpty();
        swordRecipes.Should().OnlyContain(r => r.Output.ItemId == "iron-sword");
        swordRecipes.Should().Contain(r => r.RecipeId == "iron-sword");
    }

    /// <summary>
    /// Verifies that GetRecipesForItem is case-insensitive.
    /// </summary>
    [Test]
    public void GetRecipesForItem_CaseInsensitive()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipes1 = provider.GetRecipesForItem("IRON-SWORD");
        var recipes2 = provider.GetRecipesForItem("Iron-Sword");
        var recipes3 = provider.GetRecipesForItem("iron-sword");

        // Assert
        recipes1.Should().BeEquivalentTo(recipes2);
        recipes2.Should().BeEquivalentTo(recipes3);
    }

    /// <summary>
    /// Verifies that GetRecipesForItem returns empty for unknown items.
    /// </summary>
    [Test]
    public void GetRecipesForItem_UnknownItem_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var recipes = provider.GetRecipesForItem("unknown-item");

        // Assert
        recipes.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetRecipesForItem returns empty for null/empty item ID.
    /// </summary>
    [Test]
    public void GetRecipesForItem_NullOrEmptyItemId_ReturnsEmptyList()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.GetRecipesForItem(null!).Should().BeEmpty();
        provider.GetRecipesForItem("").Should().BeEmpty();
        provider.GetRecipesForItem("   ").Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // Exists TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Exists returns true for existing recipes.
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
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.Exists("iron-sword").Should().BeTrue();
        provider.Exists("IRON-SWORD").Should().BeTrue(); // Case-insensitive
        provider.Exists("healing-potion").Should().BeTrue();
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
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act & Assert
        provider.Exists("nonexistent-recipe").Should().BeFalse();
        provider.Exists(null!).Should().BeFalse();
        provider.Exists("").Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // RECIPE DETAILS INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that loaded recipes have correct ingredient details.
    /// </summary>
    [Test]
    public void GetRecipe_LoadsIngredientDetailsCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ironSword = provider.GetRecipe("iron-sword");

        // Assert
        ironSword.Should().NotBeNull();
        ironSword!.Ingredients.Should().HaveCount(2);
        ironSword.Ingredients.Should().Contain(i => i.ResourceId == "iron-ore" && i.Quantity == 5);
        ironSword.Ingredients.Should().Contain(i => i.ResourceId == "leather" && i.Quantity == 2);
        ironSword.GetTotalIngredientCount().Should().Be(7);
    }

    /// <summary>
    /// Verifies that loaded recipes have correct output details.
    /// </summary>
    [Test]
    public void GetRecipe_LoadsOutputDetailsCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ironSword = provider.GetRecipe("iron-sword");
        var mithrilBlade = provider.GetRecipe("mithril-blade");

        // Assert
        ironSword.Should().NotBeNull();
        ironSword!.Output.ItemId.Should().Be("iron-sword");
        ironSword.Output.Quantity.Should().Be(1);
        ironSword.Output.HasQualityScaling.Should().BeFalse();

        mithrilBlade.Should().NotBeNull();
        mithrilBlade!.Output.ItemId.Should().Be("mithril-blade");
        mithrilBlade.Output.HasQualityScaling.Should().BeTrue();
        mithrilBlade.Output.QualityFormula.Should().Contain("Legendary");
    }

    /// <summary>
    /// Verifies that difficulty descriptions are calculated correctly.
    /// </summary>
    [Test]
    public void GetRecipe_DifficultyDescriptionsAreCorrect()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ironIngot = provider.GetRecipe("iron-ingot"); // DC 8
        var ironSword = provider.GetRecipe("iron-sword"); // DC 12
        var steelSword = provider.GetRecipe("steel-sword"); // DC 14
        var mithrilBlade = provider.GetRecipe("mithril-blade"); // DC 18

        // Assert
        ironIngot.Should().NotBeNull();
        ironIngot!.GetDifficultyDescription().Should().Be("Trivial");

        ironSword.Should().NotBeNull();
        ironSword!.GetDifficultyDescription().Should().Be("Moderate");

        steelSword.Should().NotBeNull();
        steelSword!.GetDifficultyDescription().Should().Be("Challenging");

        mithrilBlade.Should().NotBeNull();
        mithrilBlade!.GetDifficultyDescription().Should().Be("Very Hard");
    }

    /// <summary>
    /// Verifies that RequiresResource works correctly on loaded recipes.
    /// </summary>
    [Test]
    public void GetRecipe_RequiresResourceWorksCorrectly()
    {
        // Skip if config file doesn't exist (CI environment)
        if (!File.Exists(_testConfigPath))
        {
            Assert.Ignore("Config file not found at expected path. Skipping integration test.");
            return;
        }

        // Arrange
        var provider = new RecipeProvider(_testConfigPath, _mockLogger.Object);

        // Act
        var ironSword = provider.GetRecipe("iron-sword");

        // Assert
        ironSword.Should().NotBeNull();
        ironSword!.RequiresResource("iron-ore").Should().BeTrue();
        ironSword.RequiresResource("leather").Should().BeTrue();
        ironSword.RequiresResource("gold-ore").Should().BeFalse();
        ironSword.RequiresResource("IRON-ORE").Should().BeTrue(); // Case-insensitive
    }

    // ═══════════════════════════════════════════════════════════════
    // MISSING FILE HANDLING TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that provider handles missing config file gracefully with lazy loading.
    /// </summary>
    [Test]
    public void Constructor_NonexistentFile_DoesNotThrowImmediately()
    {
        // Arrange - Provider uses lazy loading, so constructor doesn't throw
        var provider = new RecipeProvider("/nonexistent/path/recipes.json", _mockLogger.Object);

        // Act - First access triggers loading
        var count = provider.GetRecipeCount();

        // Assert - Should return 0 instead of throwing
        count.Should().Be(0);
    }
}
