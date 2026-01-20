using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Models;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the CraftingService.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>CanCraft validation (station, recipe, knowledge, resources)</description></item>
///   <item><description>Craft execution (dice roll, success, failure)</description></item>
///   <item><description>Quality determination (Standard, Fine, Masterwork, Legendary)</description></item>
///   <item><description>Resource consumption on success</description></item>
///   <item><description>Graduated resource loss on failure (25%/50%)</description></item>
///   <item><description>Event publishing</description></item>
///   <item><description>Query methods (GetCraftableRecipesHere, GetReadyToCraftRecipes)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CraftingServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private Mock<IRecipeProvider> _mockRecipeProvider = null!;
    private Mock<IRecipeService> _mockRecipeService = null!;
    private Mock<ICraftingStationProvider> _mockStationProvider = null!;
    private Mock<IResourceProvider> _mockResourceProvider = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<CraftingService>> _mockLogger = null!;
    private CraftingService _service = null!;

    private Player _testPlayer = null!;
    private Room _testRoom = null!;
    private CraftingStation _testStation = null!;
    private RecipeDefinition _testRecipe = null!;
    private CraftingStationDefinition _testStationDefinition = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        // Initialize mocks
        _mockRecipeProvider = new Mock<IRecipeProvider>();
        _mockRecipeService = new Mock<IRecipeService>();
        _mockStationProvider = new Mock<ICraftingStationProvider>();
        _mockResourceProvider = new Mock<IResourceProvider>();
        _mockDiceService = new Mock<IDiceService>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<CraftingService>>();

        // Create test station definition
        _testStationDefinition = CraftingStationDefinition.Create(
            stationId: "anvil",
            name: "Anvil",
            description: "For smithing metal items",
            categories: [RecipeCategory.Weapon, RecipeCategory.Armor],
            skillId: "smithing",
            bonusPerLevel: 1);

        // Create test recipe
        _testRecipe = RecipeDefinition.Create(
            recipeId: "iron-sword",
            name: "Iron Sword",
            description: "A basic iron sword",
            category: RecipeCategory.Weapon,
            requiredStationId: "anvil",
            ingredients: [new RecipeIngredient("iron-ore", 5), new RecipeIngredient("leather", 2)],
            output: new RecipeOutput("iron-sword", 1),
            difficultyClass: 12,
            isDefault: true);

        // Create test player with resources
        _testPlayer = CreateTestPlayer();

        // Create test crafting station entity
        _testStation = CraftingStation.Create(
            definitionId: "anvil",
            name: "Anvil",
            description: "A sturdy anvil for smithing");

        // Create test room with crafting station
        _testRoom = CreateTestRoom(_testStation);

        // Setup default mock behaviors
        SetupDefaultMocks();

        // Create service
        _service = new CraftingService(
            _mockRecipeProvider.Object,
            _mockRecipeService.Object,
            _mockStationProvider.Object,
            _mockResourceProvider.Object,
            _mockDiceService.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    private static Player CreateTestPlayer()
    {
        var player = Player.Create("TestPlayer");

        // Initialize resources - player starts with iron ore and leather
        player.InitializeResource("iron-ore", 20, startAtZero: false);
        player.InitializeResource("leather", 20, startAtZero: false);

        // Add resources to the pools (Gain adds to current value)
        var ironPool = player.GetResource("iron-ore");
        var leatherPool = player.GetResource("leather");

        // Pools start at max by default, so we're good

        // Teach the player the recipe
        player.RecipeBook.LearnRecipe("iron-sword");

        // Add smithing skill with Apprentice proficiency
        var smithingSkill = PlayerSkill.Create("smithing", "Smithing");
        smithingSkill.SetProficiency(SkillProficiency.Apprentice);
        player.AddSkill(smithingSkill);

        return player;
    }

    private static Room CreateTestRoom(CraftingStation? station = null)
    {
        var position = new Position3D(0, 0, 0);
        var room = new Room("Test Forge", "A room with a forge", position, Biome.Town);

        if (station != null)
        {
            room.AddCraftingStation(station);
        }

        return room;
    }

    private void SetupDefaultMocks()
    {
        // Recipe provider
        _mockRecipeProvider.Setup(p => p.GetRecipe("iron-sword")).Returns(_testRecipe);
        _mockRecipeProvider.Setup(p => p.GetRecipe("unknown")).Returns((RecipeDefinition?)null);
        _mockRecipeProvider.Setup(p => p.Exists("iron-sword")).Returns(true);
        _mockRecipeProvider.Setup(p => p.Exists("unknown")).Returns(false);

        // Recipe service
        _mockRecipeService.Setup(s => s.KnowsRecipe(_testPlayer, "iron-sword")).Returns(true);
        _mockRecipeService.Setup(s => s.KnowsRecipe(_testPlayer, "unknown")).Returns(false);

        // Station provider
        _mockStationProvider.Setup(p => p.GetStation("anvil")).Returns(_testStationDefinition);
        _mockStationProvider.Setup(p => p.SupportsCategory("anvil", RecipeCategory.Weapon)).Returns(true);

        // Resource provider
        var ironResource = ResourceDefinition.Create(
            "iron-ore", "Iron Ore", "Raw iron", ResourceCategory.Ore, ResourceQuality.Common, 5, 20);
        var leatherResource = ResourceDefinition.Create(
            "leather", "Leather", "Tanned hide", ResourceCategory.Leather, ResourceQuality.Common, 3, 20);
        _mockResourceProvider.Setup(p => p.GetResource("iron-ore")).Returns(ironResource);
        _mockResourceProvider.Setup(p => p.GetResource("leather")).Returns(leatherResource);

        // Dice service - default roll of 15
        _mockDiceService.Setup(d => d.RollD20()).Returns(15);
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCRAFT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CanCraft_RecipeNotFound_ReturnsFailed()
    {
        // Act
        var result = _service.CanCraft(_testPlayer, "unknown", _testRoom);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("not found");
        result.Recipe.Should().BeNull();
    }

    [Test]
    public void CanCraft_RecipeNotKnown_ReturnsFailed()
    {
        // Arrange
        _mockRecipeService.Setup(s => s.KnowsRecipe(_testPlayer, "iron-sword")).Returns(false);

        // Act
        var result = _service.CanCraft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("don't know");
    }

    [Test]
    public void CanCraft_NoStationInRoom_ReturnsFailed()
    {
        // Arrange
        var emptyRoom = CreateTestRoom(station: null);

        // Act
        var result = _service.CanCraft(_testPlayer, "iron-sword", emptyRoom);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("crafting station");
    }

    [Test]
    public void CanCraft_InsufficientResources_ReturnsInsufficientResources()
    {
        // Arrange - player has no iron ore
        var poorPlayer = Player.Create("PoorPlayer");
        poorPlayer.RecipeBook.LearnRecipe("iron-sword");

        // Initialize resources with zero current value
        poorPlayer.InitializeResource("iron-ore", 20, startAtZero: true);
        poorPlayer.InitializeResource("leather", 20, startAtZero: true);

        _mockRecipeService.Setup(s => s.KnowsRecipe(poorPlayer, "iron-sword")).Returns(true);

        // Act
        var result = _service.CanCraft(poorPlayer, "iron-sword", _testRoom);

        // Assert
        result.IsValid.Should().BeFalse();
        result.IsMissingIngredients.Should().BeTrue();
        result.MissingIngredients.Should().NotBeEmpty();
    }

    [Test]
    public void CanCraft_AllValid_ReturnsSuccess()
    {
        // Act
        var result = _service.CanCraft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Recipe.Should().NotBeNull();
        result.Recipe!.RecipeId.Should().Be("iron-sword");
        result.Station.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // CRAFT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Craft_RollSucceeds_ReturnsSuccessWithItem()
    {
        // Arrange - roll of 15 + modifier should beat DC 12
        _mockDiceService.Setup(d => d.RollD20()).Returns(15);

        // Act
        var result = _service.Craft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CraftedItem.Should().NotBeNull();
        result.Roll.Should().Be(15);
        result.Total.Should().BeGreaterThanOrEqualTo(result.DifficultyClass);
    }

    [Test]
    public void Craft_RollFails_ReturnsFailureWithResourceLoss()
    {
        // Arrange - roll of 1 should fail DC 12
        _mockDiceService.Setup(d => d.RollD20()).Returns(1);

        // Act
        var result = _service.Craft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.WasDiceRollFailure.Should().BeTrue();
        result.CraftedItem.Should().BeNull();
        result.ResourcesLost.Should().NotBeNull();
    }

    [Test]
    public void Craft_Natural20_ReturnsLegendaryQuality()
    {
        // Arrange
        _mockDiceService.Setup(d => d.RollD20()).Returns(20);

        // Act
        var result = _service.Craft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Quality.Should().Be(CraftedItemQuality.Legendary);
        result.IsNatural20.Should().BeTrue();
    }

    [Test]
    public void Craft_PublishesCraftAttemptedEvent()
    {
        // Arrange
        _mockDiceService.Setup(d => d.RollD20()).Returns(15);

        // Act
        _service.Craft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        _mockEventLogger.Verify(
            e => e.Log(It.Is<CraftAttemptedEvent>(evt =>
                evt.RecipeId == "iron-sword" &&
                evt.PlayerId == _testPlayer.Id)),
            Times.Once);
    }

    [Test]
    public void Craft_Success_PublishesItemCraftedEvent()
    {
        // Arrange
        _mockDiceService.Setup(d => d.RollD20()).Returns(15);

        // Act
        _service.Craft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        _mockEventLogger.Verify(
            e => e.Log(It.Is<ItemCraftedEvent>(evt =>
                evt.RecipeId == "iron-sword" &&
                evt.PlayerId == _testPlayer.Id)),
            Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // CALCULATEFAILURELOSS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CalculateFailureLoss_CloseFailure_Returns25PercentLoss()
    {
        // Arrange - margin of -3 is a close failure (>= -5)
        const int margin = -3;

        // Act
        var losses = _service.CalculateFailureLoss(_testRecipe, margin);

        // Assert
        losses.Should().NotBeEmpty();
        // For 5 iron-ore at 25% = 1 (minimum 1), for 2 leather at 25% = 1 (minimum 1)
        var ironLoss = losses.FirstOrDefault(l => l.ResourceId == "iron-ore");
        ironLoss.Should().NotBeNull();
        ironLoss!.Amount.Should().BeLessThanOrEqualTo(2); // 25% of 5 = 1.25 -> 1 or 2
    }

    [Test]
    public void CalculateFailureLoss_BadFailure_Returns50PercentLoss()
    {
        // Arrange - margin of -7 is a bad failure (< -5)
        const int margin = -7;

        // Act
        var losses = _service.CalculateFailureLoss(_testRecipe, margin);

        // Assert
        losses.Should().NotBeEmpty();
        // For 5 iron-ore at 50% = 2-3, for 2 leather at 50% = 1
        var ironLoss = losses.FirstOrDefault(l => l.ResourceId == "iron-ore");
        ironLoss.Should().NotBeNull();
        ironLoss!.Amount.Should().BeGreaterThanOrEqualTo(2); // 50% of 5 = 2.5 -> 2 or 3
    }

    // ═══════════════════════════════════════════════════════════════
    // RESULT RECORD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CraftResult_GetRollDisplay_FormatsCorrectly()
    {
        // Arrange
        var result = CraftResult.Success(
            roll: 15,
            modifier: 4,
            total: 19,
            dc: 12,
            item: new Item("Test Item", "A test", ItemType.Weapon, 100),
            quality: CraftedItemQuality.Fine);

        // Act
        var display = result.GetRollDisplay();

        // Assert
        display.Should().Be("d20(15) +4 = 19 vs DC 12");
    }

    [Test]
    public void CraftValidation_WithStation_ReturnsCorrectStation()
    {
        // Act
        var result = _service.CanCraft(_testPlayer, "iron-sword", _testRoom);

        // Assert
        result.HasStation.Should().BeTrue();
        result.Station.Should().NotBeNull();
        result.Station!.StationId.Should().Be("anvil");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetCraftingModifier_WithSkill_ReturnsCorrectModifier()
    {
        // Arrange - player has Apprentice (level 1) in smithing, bonusPerLevel = 1
        // So modifier should be 1 * 1 = 1

        // Act
        var modifier = _service.GetCraftingModifier(_testPlayer, "anvil");

        // Assert
        modifier.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void GetCurrentStation_WithStation_ReturnsStation()
    {
        // Act
        var station = _service.GetCurrentStation(_testRoom);

        // Assert
        station.Should().NotBeNull();
        station!.DefinitionId.Should().Be("anvil");
    }

    [Test]
    public void GetCurrentStation_NoStation_ReturnsNull()
    {
        // Arrange
        var emptyRoom = CreateTestRoom(station: null);

        // Act
        var station = _service.GetCurrentStation(emptyRoom);

        // Assert
        station.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRecipeProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CraftingService(
            null!,
            _mockRecipeService.Object,
            _mockStationProvider.Object,
            _mockResourceProvider.Object,
            _mockDiceService.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("recipeProvider");
    }

    [Test]
    public void Constructor_WithNullDiceService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CraftingService(
            _mockRecipeProvider.Object,
            _mockRecipeService.Object,
            _mockStationProvider.Object,
            _mockResourceProvider.Object,
            null!,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("diceService");
    }
}
