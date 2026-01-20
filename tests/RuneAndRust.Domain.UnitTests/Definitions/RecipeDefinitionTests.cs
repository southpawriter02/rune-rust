using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for the <see cref="RecipeDefinition"/> entity.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Factory method validation and property assignment</description></item>
///   <item><description>ID normalization to lowercase</description></item>
///   <item><description>Invalid parameter validation</description></item>
///   <item><description>Query methods (GetTotalIngredientCount, GetIngredient, RequiresResource)</description></item>
///   <item><description>GetDifficultyDescription for all DC ranges</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid recipe with default values for testing.
    /// </summary>
    private static RecipeDefinition CreateValidRecipe(
        string recipeId = "iron-sword",
        string name = "Iron Sword",
        string description = "A basic iron sword",
        RecipeCategory category = RecipeCategory.Weapon,
        string requiredStationId = "anvil",
        IEnumerable<RecipeIngredient>? ingredients = null,
        RecipeOutput? output = null,
        int difficultyClass = 12,
        bool isDefault = false,
        int craftingTimeSeconds = 30,
        string? iconPath = null)
    {
        ingredients ??= new[] { new RecipeIngredient("iron-ore", 5) };
        output ??= new RecipeOutput("iron-sword", 1);

        return RecipeDefinition.Create(
            recipeId,
            name,
            description,
            category,
            requiredStationId,
            ingredients,
            output,
            difficultyClass,
            isDefault,
            craftingTimeSeconds,
            iconPath);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALID CONSTRUCTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating a recipe with valid parameters sets all properties correctly.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_SetsAllProperties()
    {
        // Arrange
        var ingredients = new List<RecipeIngredient>
        {
            new("iron-ore", 5),
            new("leather", 2)
        };
        var output = new RecipeOutput("iron-sword", 1);

        // Act
        var recipe = RecipeDefinition.Create(
            recipeId: "iron-sword",
            name: "Iron Sword",
            description: "A basic iron sword",
            category: RecipeCategory.Weapon,
            requiredStationId: "anvil",
            ingredients: ingredients,
            output: output,
            difficultyClass: 12,
            isDefault: true,
            craftingTimeSeconds: 30,
            iconPath: "icons/recipes/iron_sword.png");

        // Assert
        recipe.Id.Should().NotBe(Guid.Empty);
        recipe.RecipeId.Should().Be("iron-sword");
        recipe.Name.Should().Be("Iron Sword");
        recipe.Description.Should().Be("A basic iron sword");
        recipe.Category.Should().Be(RecipeCategory.Weapon);
        recipe.RequiredStationId.Should().Be("anvil");
        recipe.Ingredients.Should().HaveCount(2);
        recipe.Output.Should().Be(output);
        recipe.DifficultyClass.Should().Be(12);
        recipe.IsDefault.Should().BeTrue();
        recipe.CraftingTimeSeconds.Should().Be(30);
        recipe.IconPath.Should().Be("icons/recipes/iron_sword.png");
    }

    /// <summary>
    /// Verifies that recipe ID is normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseRecipeId_NormalizesToLowercase()
    {
        // Act
        var recipe = CreateValidRecipe(recipeId: "Iron-Sword");

        // Assert
        recipe.RecipeId.Should().Be("iron-sword");
    }

    /// <summary>
    /// Verifies that station ID is normalized to lowercase.
    /// </summary>
    [Test]
    public void Create_WithMixedCaseStationId_NormalizesToLowercase()
    {
        // Act
        var recipe = CreateValidRecipe(requiredStationId: "Alchemy-Table");

        // Assert
        recipe.RequiredStationId.Should().Be("alchemy-table");
    }

    /// <summary>
    /// Verifies that creating a recipe with default IsDefault (false) works correctly.
    /// </summary>
    [Test]
    public void Create_WithDefaultIsDefault_SetsFalse()
    {
        // Act
        var recipe = CreateValidRecipe();

        // Assert
        recipe.IsDefault.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that creating a recipe with default crafting time (30) works correctly.
    /// </summary>
    [Test]
    public void Create_WithDefaultCraftingTime_SetsThirty()
    {
        // Act
        var recipe = CreateValidRecipe();

        // Assert
        recipe.CraftingTimeSeconds.Should().Be(30);
    }

    /// <summary>
    /// Verifies that creating a recipe with zero crafting time works correctly.
    /// </summary>
    [Test]
    public void Create_WithZeroCraftingTime_Succeeds()
    {
        // Act
        var recipe = CreateValidRecipe(craftingTimeSeconds: 0);

        // Assert
        recipe.CraftingTimeSeconds.Should().Be(0);
    }

    /// <summary>
    /// Verifies that creating a recipe without icon path sets null.
    /// </summary>
    [Test]
    public void Create_WithoutIconPath_SetsNull()
    {
        // Act
        var recipe = CreateValidRecipe();

        // Assert
        recipe.IconPath.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // INVALID PARAMETER TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that creating a recipe with null recipeId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullRecipeId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidRecipe(recipeId: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("recipeId");
    }

    /// <summary>
    /// Verifies that creating a recipe with empty recipeId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyRecipeId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidRecipe(recipeId: "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("recipeId");
    }

    /// <summary>
    /// Verifies that creating a recipe with null name throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidRecipe(name: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    /// <summary>
    /// Verifies that creating a recipe with null requiredStationId throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullRequiredStationId_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidRecipe(requiredStationId: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("requiredStationId");
    }

    /// <summary>
    /// Verifies that creating a recipe with null output throws ArgumentNullException.
    /// </summary>
    [Test]
    public void Create_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var ingredients = new[] { new RecipeIngredient("iron-ore", 5) };

        // Act
        var act = () => RecipeDefinition.Create(
            "iron-sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapon,
            "anvil",
            ingredients,
            null!,
            12);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    /// <summary>
    /// Verifies that creating a recipe with empty ingredients throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithEmptyIngredients_ThrowsArgumentException()
    {
        // Act
        var act = () => CreateValidRecipe(ingredients: Array.Empty<RecipeIngredient>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("ingredients")
            .WithMessage("*at least one ingredient*");
    }

    /// <summary>
    /// Verifies that creating a recipe with null ingredients (treated as empty) throws ArgumentException.
    /// </summary>
    [Test]
    public void Create_WithNullIngredients_ThrowsArgumentException()
    {
        // Arrange
        var output = new RecipeOutput("iron-sword", 1);

        // Act
        var act = () => RecipeDefinition.Create(
            "iron-sword",
            "Iron Sword",
            "A basic iron sword",
            RecipeCategory.Weapon,
            "anvil",
            null!,
            output,
            12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("ingredients");
    }

    /// <summary>
    /// Verifies that creating a recipe with zero difficulty class throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithZeroDifficultyClass_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => CreateValidRecipe(difficultyClass: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("difficultyClass");
    }

    /// <summary>
    /// Verifies that creating a recipe with negative difficulty class throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeDifficultyClass_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => CreateValidRecipe(difficultyClass: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("difficultyClass");
    }

    /// <summary>
    /// Verifies that creating a recipe with negative crafting time throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void Create_WithNegativeCraftingTime_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => CreateValidRecipe(craftingTimeSeconds: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("craftingTimeSeconds");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetTotalIngredientCount returns the sum of all ingredient quantities.
    /// </summary>
    [Test]
    public void GetTotalIngredientCount_ReturnsSum()
    {
        // Arrange
        var ingredients = new List<RecipeIngredient>
        {
            new("iron-ore", 5),
            new("leather", 2),
            new("coal", 3)
        };
        var recipe = CreateValidRecipe(ingredients: ingredients);

        // Act
        var total = recipe.GetTotalIngredientCount();

        // Assert
        total.Should().Be(10);
    }

    /// <summary>
    /// Verifies that GetTotalIngredientCount returns correct value for single ingredient.
    /// </summary>
    [Test]
    public void GetTotalIngredientCount_SingleIngredient_ReturnsQuantity()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var total = recipe.GetTotalIngredientCount();

        // Assert
        total.Should().Be(5); // Default ingredient is iron-ore x5
    }

    /// <summary>
    /// Verifies that GetIngredient returns the ingredient when found.
    /// </summary>
    [Test]
    public void GetIngredient_ExistingId_ReturnsIngredient()
    {
        // Arrange
        var ingredients = new List<RecipeIngredient>
        {
            new("iron-ore", 5),
            new("leather", 2)
        };
        var recipe = CreateValidRecipe(ingredients: ingredients);

        // Act
        var ingredient = recipe.GetIngredient("iron-ore");

        // Assert
        ingredient.Should().NotBeNull();
        ingredient!.ResourceId.Should().Be("iron-ore");
        ingredient.Quantity.Should().Be(5);
    }

    /// <summary>
    /// Verifies that GetIngredient is case-insensitive.
    /// </summary>
    [Test]
    public void GetIngredient_MixedCaseId_ReturnsIngredient()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var ingredient = recipe.GetIngredient("Iron-ORE");

        // Assert
        ingredient.Should().NotBeNull();
        ingredient!.ResourceId.Should().Be("iron-ore");
    }

    /// <summary>
    /// Verifies that GetIngredient returns null for non-existing ID.
    /// </summary>
    [Test]
    public void GetIngredient_NonExistingId_ReturnsNull()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var ingredient = recipe.GetIngredient("copper-ore");

        // Assert
        ingredient.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetIngredient returns null for null ID.
    /// </summary>
    [Test]
    public void GetIngredient_NullId_ReturnsNull()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var ingredient = recipe.GetIngredient(null!);

        // Assert
        ingredient.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetIngredient returns null for empty ID.
    /// </summary>
    [Test]
    public void GetIngredient_EmptyId_ReturnsNull()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var ingredient = recipe.GetIngredient("");

        // Assert
        ingredient.Should().BeNull();
    }

    /// <summary>
    /// Verifies that RequiresResource returns true for existing resource.
    /// </summary>
    [Test]
    public void RequiresResource_ExistingResource_ReturnsTrue()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var requires = recipe.RequiresResource("iron-ore");

        // Assert
        requires.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that RequiresResource returns false for non-existing resource.
    /// </summary>
    [Test]
    public void RequiresResource_NonExistingResource_ReturnsFalse()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var requires = recipe.RequiresResource("copper-ore");

        // Assert
        requires.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that RequiresResource is case-insensitive.
    /// </summary>
    [Test]
    public void RequiresResource_MixedCaseId_ReturnsTrue()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var requires = recipe.RequiresResource("IRON-ORE");

        // Assert
        requires.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // DIFFICULTY DESCRIPTION TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Trivial" for DC 5-8.
    /// </summary>
    [TestCase(5, "Trivial")]
    [TestCase(8, "Trivial")]
    public void GetDifficultyDescription_TrivialDC_ReturnsTrivial(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Easy" for DC 9-10.
    /// </summary>
    [TestCase(9, "Easy")]
    [TestCase(10, "Easy")]
    public void GetDifficultyDescription_EasyDC_ReturnsEasy(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Moderate" for DC 11-12.
    /// </summary>
    [TestCase(11, "Moderate")]
    [TestCase(12, "Moderate")]
    public void GetDifficultyDescription_ModerateDC_ReturnsModerate(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Challenging" for DC 13-14.
    /// </summary>
    [TestCase(13, "Challenging")]
    [TestCase(14, "Challenging")]
    public void GetDifficultyDescription_ChallengingDC_ReturnsChallenging(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Hard" for DC 15-16.
    /// </summary>
    [TestCase(15, "Hard")]
    [TestCase(16, "Hard")]
    public void GetDifficultyDescription_HardDC_ReturnsHard(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Very Hard" for DC 17-18.
    /// </summary>
    [TestCase(17, "Very Hard")]
    [TestCase(18, "Very Hard")]
    public void GetDifficultyDescription_VeryHardDC_ReturnsVeryHard(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetDifficultyDescription returns "Legendary" for DC 19+.
    /// </summary>
    [TestCase(19, "Legendary")]
    [TestCase(20, "Legendary")]
    [TestCase(25, "Legendary")]
    public void GetDifficultyDescription_LegendaryDC_ReturnsLegendary(int dc, string expected)
    {
        // Arrange
        var recipe = CreateValidRecipe(difficultyClass: dc);

        // Act
        var description = recipe.GetDifficultyDescription();

        // Assert
        description.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetIngredientsDisplay returns formatted ingredient list.
    /// </summary>
    [Test]
    public void GetIngredientsDisplay_MultipleIngredients_ReturnsFormattedList()
    {
        // Arrange
        var ingredients = new List<RecipeIngredient>
        {
            new("iron-ore", 5),
            new("leather", 2)
        };
        var recipe = CreateValidRecipe(ingredients: ingredients);

        // Act
        var display = recipe.GetIngredientsDisplay();

        // Assert
        display.Should().Be("iron-ore x5, leather x2");
    }

    /// <summary>
    /// Verifies that GetIngredientsDisplay works for single ingredient.
    /// </summary>
    [Test]
    public void GetIngredientsDisplay_SingleIngredient_ReturnsFormattedString()
    {
        // Arrange
        var recipe = CreateValidRecipe();

        // Act
        var display = recipe.GetIngredientsDisplay();

        // Assert
        display.Should().Be("iron-ore x5");
    }

    /// <summary>
    /// Verifies that ToString returns the expected format.
    /// </summary>
    [Test]
    public void ToString_ReturnsNameAndId()
    {
        // Arrange
        var recipe = CreateValidRecipe(recipeId: "iron-sword", name: "Iron Sword");

        // Act
        var result = recipe.ToString();

        // Assert
        result.Should().Be("Iron Sword (iron-sword)");
    }

    // ═══════════════════════════════════════════════════════════════
    // CATEGORY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all recipe categories can be assigned.
    /// </summary>
    [TestCase(RecipeCategory.Weapon)]
    [TestCase(RecipeCategory.Armor)]
    [TestCase(RecipeCategory.Potion)]
    [TestCase(RecipeCategory.Consumable)]
    [TestCase(RecipeCategory.Accessory)]
    [TestCase(RecipeCategory.Tool)]
    [TestCase(RecipeCategory.Material)]
    public void Create_WithAnyCategory_SetsCategory(RecipeCategory category)
    {
        // Act
        var recipe = CreateValidRecipe(category: category);

        // Assert
        recipe.Category.Should().Be(category);
    }
}
