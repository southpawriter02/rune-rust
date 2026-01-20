using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for the <see cref="CraftingStationDefinition"/> entity.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Factory method validation and property assignment</description></item>
///   <item><description>ID normalization to lowercase</description></item>
///   <item><description>Invalid parameter validation</description></item>
///   <item><description>Query methods (SupportsCategory, CanCraftRecipe, GetCategoriesDisplay)</description></item>
///   <item><description>Instance creation methods</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CraftingStationDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid crafting station definition with default values for testing.
    /// </summary>
    private static CraftingStationDefinition CreateValidDefinition(
        string stationId = "anvil",
        string name = "Anvil",
        string description = "A sturdy anvil for smithing metal items.",
        IEnumerable<RecipeCategory>? supportedCategories = null,
        string craftingSkillId = "smithing",
        string? iconPath = null)
    {
        supportedCategories ??= new[] { RecipeCategory.Weapon, RecipeCategory.Tool, RecipeCategory.Material };

        return CraftingStationDefinition.Create(
            stationId,
            name,
            description,
            supportedCategories,
            craftingSkillId,
            iconPath);
    }

    /// <summary>
    /// Creates a valid recipe definition for testing CanCraftRecipe.
    /// </summary>
    private static RecipeDefinition CreateValidRecipe(
        RecipeCategory category = RecipeCategory.Weapon)
    {
        return RecipeDefinition.Create(
            recipeId: "test-item",
            name: "Test Item",
            description: "A test item for unit testing",
            category: category,
            requiredStationId: "anvil",
            ingredients: new[] { new RecipeIngredient("iron-ore", 1) },
            output: new RecipeOutput("test-item", 1),
            difficultyClass: 10);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALID CONSTRUCTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating a station definition with valid parameters sets all properties correctly.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_SetsAllProperties()
    {
        // Arrange
        var categories = new List<RecipeCategory>
        {
            RecipeCategory.Weapon,
            RecipeCategory.Tool,
            RecipeCategory.Material
        };

        // Act
        var definition = CraftingStationDefinition.Create(
            stationId: "anvil",
            name: "Anvil",
            description: "A sturdy anvil for smithing metal items.",
            supportedCategories: categories,
            craftingSkillId: "smithing",
            iconPath: "icons/stations/anvil.png");

        // Assert
        definition.Id.Should().NotBe(Guid.Empty);
        definition.StationId.Should().Be("anvil");
        definition.Name.Should().Be("Anvil");
        definition.Description.Should().Be("A sturdy anvil for smithing metal items.");
        definition.SupportedCategories.Should().HaveCount(3);
        definition.SupportedCategories.Should().Contain(RecipeCategory.Weapon);
        definition.SupportedCategories.Should().Contain(RecipeCategory.Tool);
        definition.SupportedCategories.Should().Contain(RecipeCategory.Material);
        definition.CraftingSkillId.Should().Be("smithing");
        definition.IconPath.Should().Be("icons/stations/anvil.png");
    }

    /// <summary>
    /// Verifies that station ID is normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseStationId_NormalizesToLowercase()
    {
        // Act
        var definition = CreateValidDefinition(stationId: "Alchemy-Table");

        // Assert
        definition.StationId.Should().Be("alchemy-table");
    }

    /// <summary>
    /// Verifies that crafting skill ID is normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseCraftingSkillId_NormalizesToLowercase()
    {
        // Act
        var definition = CreateValidDefinition(craftingSkillId: "SMITHING");

        // Assert
        definition.CraftingSkillId.Should().Be("smithing");
    }

    /// <summary>
    /// Verifies that creating a definition without icon path sets null.
    /// </summary>
    [Test]
    public void Create_WithoutIconPath_SetsNull()
    {
        // Act
        var definition = CreateValidDefinition();

        // Assert
        definition.IconPath.Should().BeNull();
    }

    /// <summary>
    /// Verifies that creating a definition with a single category works correctly.
    /// </summary>
    [Test]
    public void Create_WithSingleCategory_SetsCategories()
    {
        // Arrange
        var categories = new[] { RecipeCategory.Consumable };

        // Act
        var definition = CreateValidDefinition(
            stationId: "cooking-fire",
            supportedCategories: categories,
            craftingSkillId: "cooking");

        // Assert
        definition.SupportedCategories.Should().HaveCount(1);
        definition.SupportedCategories.Should().Contain(RecipeCategory.Consumable);
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID PARAMETER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating a definition with null stationId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullStationId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(stationId: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("stationId");
    }

    /// <summary>
    /// Verifies that creating a definition with empty stationId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyStationId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(stationId: "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("stationId");
    }

    /// <summary>
    /// Verifies that creating a definition with whitespace stationId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceStationId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(stationId: "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("stationId");
    }

    /// <summary>
    /// Verifies that creating a definition with null name throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(name: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    /// <summary>
    /// Verifies that creating a definition with empty name throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(name: "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    /// <summary>
    /// Verifies that creating a definition with null description throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(description: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    /// <summary>
    /// Verifies that creating a definition with null craftingSkillId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullCraftingSkillId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidDefinition(craftingSkillId: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("craftingSkillId");
    }

    /// <summary>
    /// Verifies that creating a definition with null supportedCategories throws ArgumentNullException.
    /// </summary>
    [Test]
    public void Create_WithNullSupportedCategories_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CraftingStationDefinition.Create(
            "anvil",
            "Anvil",
            "A sturdy anvil",
            null!,
            "smithing");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("supportedCategories");
    }

    /// <summary>
    /// Verifies that creating a definition with empty supportedCategories throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithEmptySupportedCategories_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => CreateValidDefinition(supportedCategories: Array.Empty<RecipeCategory>());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("supportedCategories");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that SupportsCategory returns true for a supported category.
    /// </summary>
    [Test]
    public void SupportsCategory_WithSupportedCategory_ReturnsTrue()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var result = definition.SupportsCategory(RecipeCategory.Weapon);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SupportsCategory returns false for an unsupported category.
    /// </summary>
    [Test]
    public void SupportsCategory_WithUnsupportedCategory_ReturnsFalse()
    {
        // Arrange - anvil supports Weapon, Tool, Material (not Potion)
        var definition = CreateValidDefinition();

        // Act
        var result = definition.SupportsCategory(RecipeCategory.Potion);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that SupportsCategory works for all supported categories.
    /// </summary>
    [TestCase(RecipeCategory.Weapon, true)]
    [TestCase(RecipeCategory.Tool, true)]
    [TestCase(RecipeCategory.Material, true)]
    [TestCase(RecipeCategory.Potion, false)]
    [TestCase(RecipeCategory.Armor, false)]
    [TestCase(RecipeCategory.Accessory, false)]
    [TestCase(RecipeCategory.Consumable, false)]
    public void SupportsCategory_ForAllCategories_ReturnsExpectedResult(
        RecipeCategory category, bool expected)
    {
        // Arrange - anvil supports Weapon, Tool, Material
        var definition = CreateValidDefinition();

        // Act
        var result = definition.SupportsCategory(category);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that CanCraftRecipe returns true for a recipe with a supported category.
    /// </summary>
    [Test]
    public void CanCraftRecipe_WithSupportedCategory_ReturnsTrue()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var recipe = CreateValidRecipe(RecipeCategory.Weapon);

        // Act
        var result = definition.CanCraftRecipe(recipe);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanCraftRecipe returns false for a recipe with an unsupported category.
    /// </summary>
    [Test]
    public void CanCraftRecipe_WithUnsupportedCategory_ReturnsFalse()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var recipe = CreateValidRecipe(RecipeCategory.Potion);

        // Act
        var result = definition.CanCraftRecipe(recipe);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CanCraftRecipe throws ArgumentNullException for null recipe.
    /// </summary>
    [Test]
    public void CanCraftRecipe_WithNullRecipe_ThrowsArgumentNullException()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var act = () => definition.CanCraftRecipe(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("recipe");
    }

    /// <summary>
    /// Verifies that GetCategoriesDisplay returns a comma-separated list of categories.
    /// </summary>
    [Test]
    public void GetCategoriesDisplay_ReturnsFormattedString()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var result = definition.GetCategoriesDisplay();

        // Assert
        result.Should().Be("Weapon, Tool, Material");
    }

    /// <summary>
    /// Verifies that GetCategoriesDisplay works for a single category.
    /// </summary>
    [Test]
    public void GetCategoriesDisplay_SingleCategory_ReturnsOnlyThatCategory()
    {
        // Arrange
        var definition = CreateValidDefinition(
            stationId: "cooking-fire",
            supportedCategories: new[] { RecipeCategory.Consumable },
            craftingSkillId: "cooking");

        // Act
        var result = definition.GetCategoriesDisplay();

        // Assert
        result.Should().Be("Consumable");
    }

    // ═══════════════════════════════════════════════════════════════
    // INSTANCE CREATION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateInstance returns a valid CraftingStation with correct properties.
    /// </summary>
    [Test]
    public void CreateInstance_ReturnsValidCraftingStation()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station = definition.CreateInstance();

        // Assert
        station.Should().NotBeNull();
        station.Id.Should().NotBe(Guid.Empty);
        station.DefinitionId.Should().Be("anvil");
        station.Name.Should().Be("Anvil");
        station.Description.Should().Be("A sturdy anvil for smithing metal items.");
        station.FeatureType.Should().Be(RoomFeatureType.CraftingStation);
        station.IsInteractable.Should().BeTrue();
        station.InteractionVerb.Should().Be("use");
        station.IsAvailable.Should().BeTrue();
        station.LastUsedAt.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateInstance with custom description sets the custom description.
    /// </summary>
    [Test]
    public void CreateInstance_WithCustomDescription_SetsCustomDescription()
    {
        // Arrange
        var definition = CreateValidDefinition();
        var customDescription = "An ancient anvil, its surface scarred by centuries of legendary smithing.";

        // Act
        var station = definition.CreateInstance(customDescription);

        // Assert
        station.Should().NotBeNull();
        station.DefinitionId.Should().Be("anvil");
        station.Name.Should().Be("Anvil");
        station.Description.Should().Be(customDescription);
    }

    /// <summary>
    /// Verifies that multiple CreateInstance calls create unique instances.
    /// </summary>
    [Test]
    public void CreateInstance_MultipleCalls_CreatesUniqueInstances()
    {
        // Arrange
        var definition = CreateValidDefinition();

        // Act
        var station1 = definition.CreateInstance();
        var station2 = definition.CreateInstance();

        // Assert
        station1.Id.Should().NotBe(station2.Id);
        station1.DefinitionId.Should().Be(station2.DefinitionId);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString returns the expected format.
    /// </summary>
    [Test]
    public void ToString_ReturnsNameAndId()
    {
        // Arrange
        var definition = CreateValidDefinition(stationId: "anvil", name: "Anvil");

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("Anvil (anvil)");
    }

    /// <summary>
    /// Verifies that ToString works for stations with hyphenated IDs.
    /// </summary>
    [Test]
    public void ToString_WithHyphenatedId_ReturnsCorrectFormat()
    {
        // Arrange
        var definition = CreateValidDefinition(
            stationId: "alchemy-table",
            name: "Alchemy Table",
            supportedCategories: new[] { RecipeCategory.Potion, RecipeCategory.Consumable },
            craftingSkillId: "alchemy");

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("Alchemy Table (alchemy-table)");
    }
}
