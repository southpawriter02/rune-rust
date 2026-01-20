using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the RecipeService.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Learning recipes (success, already known, not found)</description></item>
///   <item><description>Getting known recipes</description></item>
///   <item><description>Initializing default recipes</description></item>
///   <item><description>Craft validation (recipe exists, known, station, ingredients)</description></item>
///   <item><description>Event publishing on recipe learn</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private Mock<IRecipeProvider> _mockRecipeProvider = null!;
    private Mock<IResourceProvider> _mockResourceProvider = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<RecipeService>> _mockLogger = null!;
    private RecipeService _service = null!;

    private List<RecipeDefinition> _testRecipes = null!;
    private List<ResourceDefinition> _testResources = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _mockRecipeProvider = new Mock<IRecipeProvider>();
        _mockResourceProvider = new Mock<IResourceProvider>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<RecipeService>>();

        // Setup test resources
        _testResources =
        [
            ResourceDefinition.Create("iron-ore", "Iron Ore", "Raw iron", ResourceCategory.Ore, ResourceQuality.Common, 5, 20),
            ResourceDefinition.Create("leather", "Leather", "Tanned hide", ResourceCategory.Leather, ResourceQuality.Common, 3, 20),
            ResourceDefinition.Create("herb", "Herb", "Healing herb", ResourceCategory.Herb, ResourceQuality.Common, 2, 30)
        ];

        // Setup test recipes
        _testRecipes =
        [
            RecipeDefinition.Create(
                recipeId: "iron-sword",
                name: "Iron Sword",
                description: "A basic iron sword",
                category: RecipeCategory.Weapon,
                requiredStationId: "anvil",
                ingredients: new[] { new RecipeIngredient("iron-ore", 5), new RecipeIngredient("leather", 2) },
                output: new RecipeOutput("iron-sword", 1),
                difficultyClass: 12,
                isDefault: true),
            RecipeDefinition.Create(
                recipeId: "healing-potion",
                name: "Healing Potion",
                description: "A restorative potion",
                category: RecipeCategory.Potion,
                requiredStationId: "alchemy-table",
                ingredients: new[] { new RecipeIngredient("herb", 3) },
                output: new RecipeOutput("healing-potion", 1),
                difficultyClass: 10,
                isDefault: true),
            RecipeDefinition.Create(
                recipeId: "steel-sword",
                name: "Steel Sword",
                description: "An advanced steel sword",
                category: RecipeCategory.Weapon,
                requiredStationId: "anvil",
                ingredients: new[] { new RecipeIngredient("iron-ore", 10), new RecipeIngredient("leather", 3) },
                output: new RecipeOutput("steel-sword", 1),
                difficultyClass: 15,
                isDefault: false)
        ];

        // Setup mocks
        _mockRecipeProvider.Setup(p => p.GetRecipe("iron-sword")).Returns(_testRecipes[0]);
        _mockRecipeProvider.Setup(p => p.GetRecipe("healing-potion")).Returns(_testRecipes[1]);
        _mockRecipeProvider.Setup(p => p.GetRecipe("steel-sword")).Returns(_testRecipes[2]);
        _mockRecipeProvider.Setup(p => p.GetRecipe("unknown")).Returns((RecipeDefinition?)null);
        _mockRecipeProvider.Setup(p => p.Exists("iron-sword")).Returns(true);
        _mockRecipeProvider.Setup(p => p.Exists("healing-potion")).Returns(true);
        _mockRecipeProvider.Setup(p => p.Exists("steel-sword")).Returns(true);
        _mockRecipeProvider.Setup(p => p.Exists("unknown")).Returns(false);
        _mockRecipeProvider.Setup(p => p.GetDefaultRecipes()).Returns(_testRecipes.Where(r => r.IsDefault).ToList());
        _mockRecipeProvider.Setup(p => p.GetRecipeCount()).Returns(3);
        _mockRecipeProvider.Setup(p => p.GetAllRecipes()).Returns(_testRecipes);

        _mockResourceProvider.Setup(p => p.GetResource("iron-ore")).Returns(_testResources[0]);
        _mockResourceProvider.Setup(p => p.GetResource("leather")).Returns(_testResources[1]);
        _mockResourceProvider.Setup(p => p.GetResource("herb")).Returns(_testResources[2]);
        _mockResourceProvider.Setup(p => p.Count).Returns(3);

        _service = new RecipeService(
            _mockRecipeProvider.Object,
            _mockResourceProvider.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRecipeProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeService(
            null!,
            _mockResourceProvider.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("recipeProvider");
    }

    [Test]
    public void Constructor_WithNullResourceProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeService(
            _mockRecipeProvider.Object,
            null!,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("resourceProvider");
    }

    [Test]
    public void Constructor_WithNullEventLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeService(
            _mockRecipeProvider.Object,
            _mockResourceProvider.Object,
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("eventLogger");
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeService(
            _mockRecipeProvider.Object,
            _mockResourceProvider.Object,
            _mockEventLogger.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // LEARN RECIPE TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void LearnRecipe_ValidRecipe_ReturnsSuccessAndPublishesEvent()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.LearnRecipe(player, "iron-sword");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ResultType.Should().Be(LearnResultType.Success);
        result.RecipeId.Should().Be("iron-sword");
        result.RecipeName.Should().Be("Iron Sword");
        result.FailureReason.Should().BeNull();

        // Verify event was published
        _mockEventLogger.Verify(
            e => e.LogCharacter(
                "RecipeLearned",
                It.Is<string>(s => s.Contains("Iron Sword")),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Test]
    public void LearnRecipe_AlreadyKnown_ReturnsAlreadyKnown()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");

        // Act
        var result = _service.LearnRecipe(player, "iron-sword");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(LearnResultType.AlreadyKnown);
        result.RecipeId.Should().Be("iron-sword");
        result.RecipeName.Should().Be("Iron Sword");
        result.FailureReason.Should().Be("You already know this recipe.");
    }

    [Test]
    public void LearnRecipe_RecipeNotFound_ReturnsNotFound()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.LearnRecipe(player, "unknown");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResultType.Should().Be(LearnResultType.NotFound);
        result.RecipeId.Should().Be("unknown");
        result.RecipeName.Should().BeNull();
        result.FailureReason.Should().Be("Recipe not found.");
    }

    [Test]
    public void LearnRecipe_WithNullPlayer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.LearnRecipe(null!, "iron-sword");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void LearnRecipe_WithNullRecipeId_ThrowsArgumentException()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var act = () => _service.LearnRecipe(player, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET KNOWN RECIPES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetKnownRecipes_ReturnsOnlyKnownRecipes()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.RecipeBook.Learn("healing-potion");

        // Act
        var recipes = _service.GetKnownRecipes(player);

        // Assert
        recipes.Should().HaveCount(2);
        recipes.Select(r => r.RecipeId).Should().Contain("iron-sword");
        recipes.Select(r => r.RecipeId).Should().Contain("healing-potion");
        recipes.Select(r => r.RecipeId).Should().NotContain("steel-sword");
    }

    [Test]
    public void GetKnownRecipes_EmptyBook_ReturnsEmptyList()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var recipes = _service.GetKnownRecipes(player);

        // Assert
        recipes.Should().BeEmpty();
    }

    [Test]
    public void GetKnownRecipes_FiltersOutNonExistentRecipes()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.RecipeBook.Learn("deleted-recipe"); // Recipe no longer in provider

        // Act
        var recipes = _service.GetKnownRecipes(player);

        // Assert
        recipes.Should().HaveCount(1);
        recipes.Single().RecipeId.Should().Be("iron-sword");
    }

    // ═══════════════════════════════════════════════════════════════
    // IS RECIPE KNOWN TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsRecipeKnown_KnownRecipe_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");

        // Act
        var result = _service.IsRecipeKnown(player, "iron-sword");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsRecipeKnown_UnknownRecipe_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.IsRecipeKnown(player, "iron-sword");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsRecipeKnown_NonExistentRecipe_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.IsRecipeKnown(player, "nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // INITIALIZE DEFAULT RECIPES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void InitializeDefaultRecipes_AddsDefaultRecipes()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        _service.InitializeDefaultRecipes(player);

        // Assert
        player.RecipeBook.KnownCount.Should().Be(2); // iron-sword and healing-potion are default
        player.RecipeBook.IsKnown("iron-sword").Should().BeTrue();
        player.RecipeBook.IsKnown("healing-potion").Should().BeTrue();
        player.RecipeBook.IsKnown("steel-sword").Should().BeFalse();
    }

    [Test]
    public void InitializeDefaultRecipes_DoesNotDuplicateExisting()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");

        // Act
        _service.InitializeDefaultRecipes(player);

        // Assert
        player.RecipeBook.KnownCount.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // CAN CRAFT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CanCraft_AllConditionsMet_ReturnsSuccess()
    {
        // Arrange
        var player = CreateTestPlayerWithResources();
        player.RecipeBook.Learn("iron-sword");

        // Act
        var result = _service.CanCraft(player, "iron-sword", "anvil");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Recipe.Should().NotBeNull();
        result.Recipe!.RecipeId.Should().Be("iron-sword");
        result.FailureReason.Should().BeNull();
        result.MissingIngredients.Should().BeNull();
    }

    [Test]
    public void CanCraft_RecipeNotFound_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.CanCraft(player, "nonexistent", "anvil");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Be("Recipe not found.");
    }

    [Test]
    public void CanCraft_RecipeNotKnown_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _service.CanCraft(player, "iron-sword", "anvil");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Be("You don't know this recipe.");
        result.Recipe.Should().NotBeNull();
    }

    [Test]
    public void CanCraft_WrongStation_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");

        // Act
        var result = _service.CanCraft(player, "iron-sword", "alchemy-table");

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("Anvil");
        result.Recipe.Should().NotBeNull();
    }

    [Test]
    public void CanCraft_MissingIngredients_ReturnsInsufficientResources()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        // Player has no resources

        // Act
        var result = _service.CanCraft(player, "iron-sword", "anvil");

        // Assert
        result.IsValid.Should().BeFalse();
        result.IsMissingIngredients.Should().BeTrue();
        result.MissingIngredients.Should().NotBeNull();
        result.MissingIngredients.Should().HaveCount(2); // iron-ore and leather
        result.MissingIngredients!.Any(m => m.ResourceId == "iron-ore").Should().BeTrue();
        result.MissingIngredients!.Any(m => m.ResourceId == "leather").Should().BeTrue();
    }

    [Test]
    public void CanCraft_PartialIngredients_ReportsOnlyMissing()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.InitializeResource("iron-ore", 100);
        player.GetResource("iron-ore")!.SetCurrent(5); // Has enough iron-ore
        // No leather

        // Act
        var result = _service.CanCraft(player, "iron-sword", "anvil");

        // Assert
        result.IsValid.Should().BeFalse();
        result.IsMissingIngredients.Should().BeTrue();
        result.MissingIngredients.Should().HaveCount(1);
        result.MissingIngredients!.Single().ResourceId.Should().Be("leather");
        result.MissingIngredients!.Single().QuantityNeeded.Should().Be(2);
    }

    [Test]
    public void CanCraft_InsufficientQuantity_ReportsNeededAmount()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.InitializeResource("iron-ore", 100);
        player.GetResource("iron-ore")!.SetCurrent(3); // Has 3, needs 5
        player.InitializeResource("leather", 100);
        player.GetResource("leather")!.SetCurrent(2); // Has enough

        // Act
        var result = _service.CanCraft(player, "iron-sword", "anvil");

        // Assert
        result.IsValid.Should().BeFalse();
        result.IsMissingIngredients.Should().BeTrue();
        result.MissingIngredients.Should().HaveCount(1);
        result.MissingIngredients!.Single().ResourceId.Should().Be("iron-ore");
        result.MissingIngredients!.Single().QuantityNeeded.Should().Be(2); // Need 2 more
    }

    [Test]
    public void CanCraft_NullStation_SkipsStationCheck()
    {
        // Arrange
        var player = CreateTestPlayerWithResources();
        player.RecipeBook.Learn("iron-sword");

        // Act
        var result = _service.CanCraft(player, "iron-sword", null);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // COUNT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetKnownRecipeCount_ReturnsCorrectCount()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.RecipeBook.Learn("healing-potion");

        // Act
        var count = _service.GetKnownRecipeCount(player);

        // Assert
        count.Should().Be(2);
    }

    [Test]
    public void GetTotalRecipeCount_ReturnsProviderCount()
    {
        // Act
        var count = _service.GetTotalRecipeCount();

        // Assert
        count.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // CATEGORY AND STATION FILTER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetKnownRecipesByCategory_FiltersCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.RecipeBook.Learn("healing-potion");

        // Act
        var weapons = _service.GetKnownRecipesByCategory(player, RecipeCategory.Weapon);
        var potions = _service.GetKnownRecipesByCategory(player, RecipeCategory.Potion);

        // Assert
        weapons.Should().HaveCount(1);
        weapons.Single().RecipeId.Should().Be("iron-sword");
        potions.Should().HaveCount(1);
        potions.Single().RecipeId.Should().Be("healing-potion");
    }

    [Test]
    public void GetCraftableRecipes_FiltersCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");
        player.RecipeBook.Learn("healing-potion");

        // Act
        var anvilRecipes = _service.GetCraftableRecipes(player, "anvil");
        var alchemyRecipes = _service.GetCraftableRecipes(player, "alchemy-table");

        // Assert
        anvilRecipes.Should().HaveCount(1);
        anvilRecipes.Single().RecipeId.Should().Be("iron-sword");
        alchemyRecipes.Should().HaveCount(1);
        alchemyRecipes.Single().RecipeId.Should().Be("healing-potion");
    }

    [Test]
    public void GetCraftableRecipes_IsCaseInsensitive()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.RecipeBook.Learn("iron-sword");

        // Act
        var result1 = _service.GetCraftableRecipes(player, "anvil");
        var result2 = _service.GetCraftableRecipes(player, "ANVIL");
        var result3 = _service.GetCraftableRecipes(player, "Anvil");

        // Assert
        result1.Should().HaveCount(1);
        result2.Should().HaveCount(1);
        result3.Should().HaveCount(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer");
    }

    private static Player CreateTestPlayerWithResources()
    {
        var player = new Player("TestPlayer");

        // Add resources for iron-sword recipe: 5 iron-ore, 2 leather
        player.InitializeResource("iron-ore", 100);
        player.GetResource("iron-ore")!.SetCurrent(10);

        player.InitializeResource("leather", 100);
        player.GetResource("leather")!.SetCurrent(5);

        return player;
    }
}
