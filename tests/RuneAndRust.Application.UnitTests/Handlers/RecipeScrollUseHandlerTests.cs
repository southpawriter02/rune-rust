using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Handlers;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Handlers;

/// <summary>
/// Unit tests for RecipeScrollUseHandler (v0.11.1c).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Successful recipe learning from scrolls</description></item>
///   <item><description>Preserving scrolls when recipe is already known</description></item>
///   <item><description>Failure when recipe doesn't exist</description></item>
///   <item><description>Invalid item handling</description></item>
///   <item><description>Event logging on successful learn</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class RecipeScrollUseHandlerTests
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private Mock<IRecipeService> _mockRecipeService = null!;
    private Mock<IRecipeProvider> _mockRecipeProvider = null!;
    private Mock<IGameEventLogger> _mockEventLogger = null!;
    private Mock<ILogger<RecipeScrollUseHandler>> _mockLogger = null!;
    private RecipeScrollUseHandler _handler = null!;

    private RecipeDefinition _steelSwordRecipe = null!;
    private RecipeDefinition _mithrilBladeRecipe = null!;

    // ═══════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════

    [SetUp]
    public void SetUp()
    {
        _mockRecipeService = new Mock<IRecipeService>();
        _mockRecipeProvider = new Mock<IRecipeProvider>();
        _mockEventLogger = new Mock<IGameEventLogger>();
        _mockLogger = new Mock<ILogger<RecipeScrollUseHandler>>();

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
        _mockRecipeProvider.Setup(p => p.GetRecipe("unknown-recipe")).Returns((RecipeDefinition?)null);

        _handler = new RecipeScrollUseHandler(
            _mockRecipeService.Object,
            _mockRecipeProvider.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullRecipeService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeScrollUseHandler(
            null!,
            _mockRecipeProvider.Object,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("recipeService");
    }

    [Test]
    public void Constructor_WithNullRecipeProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeScrollUseHandler(
            _mockRecipeService.Object,
            null!,
            _mockEventLogger.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("recipeProvider");
    }

    [Test]
    public void Constructor_WithNullEventLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RecipeScrollUseHandler(
            _mockRecipeService.Object,
            _mockRecipeProvider.Object,
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
        var act = () => new RecipeScrollUseHandler(
            _mockRecipeService.Object,
            _mockRecipeProvider.Object,
            _mockEventLogger.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    // ═══════════════════════════════════════════════════════════════
    // HandledEffect TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HandledEffect_ReturnsLearnRecipe()
    {
        // Act & Assert
        _handler.HandledEffect.Should().Be(ItemEffect.LearnRecipe);
    }

    // ═══════════════════════════════════════════════════════════════
    // Handle SUCCESS TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that using a valid scroll for an unknown recipe learns the recipe and consumes the scroll.
    /// </summary>
    [Test]
    public void Handle_WithValidScroll_LearnsRecipeAndReturnsConsumed()
    {
        // Arrange
        var player = CreateTestPlayer();
        var scroll = Item.CreateRecipeScroll("steel-sword", "Steel Sword", 150);

        _mockRecipeService.Setup(s => s.IsRecipeKnown(player, "steel-sword")).Returns(false);
        _mockRecipeService.Setup(s => s.LearnRecipe(player, "steel-sword"))
            .Returns(LearnRecipeResult.Success("steel-sword", "Steel Sword"));

        // Act
        var result = _handler.Handle(player, scroll);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.WasConsumed.Should().BeTrue();
        result.ResultType.Should().Be(ItemUseResultType.Consumed);
        result.Message.Should().Contain("Steel Sword");
        result.Message.Should().Contain("learn");

        // Verify learn was called
        _mockRecipeService.Verify(s => s.LearnRecipe(player, "steel-sword"), Times.Once);

        // Verify event was logged
        _mockEventLogger.Verify(
            e => e.LogInventory(
                "RecipeDiscovered",
                It.Is<string>(s => s.Contains("Steel Sword")),
                player.Id,
                It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that using a scroll for an already-known recipe preserves the scroll.
    /// </summary>
    [Test]
    public void Handle_WhenRecipeAlreadyKnown_PreservesScrollAndReturnsPreserved()
    {
        // Arrange
        var player = CreateTestPlayer();
        var scroll = Item.CreateRecipeScroll("steel-sword", "Steel Sword", 150);

        _mockRecipeService.Setup(s => s.IsRecipeKnown(player, "steel-sword")).Returns(true);

        // Act
        var result = _handler.Handle(player, scroll);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.WasConsumed.Should().BeFalse();
        result.ResultType.Should().Be(ItemUseResultType.Preserved);
        result.Message.Should().Contain("already know");
        result.Message.Should().Contain("preserved");

        // Verify learn was NOT called
        _mockRecipeService.Verify(s => s.LearnRecipe(It.IsAny<Player>(), It.IsAny<string>()), Times.Never);

        // Verify event was NOT logged
        _mockEventLogger.Verify(
            e => e.LogInventory(
                "RecipeDiscovered",
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Handle FAILURE TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that using a scroll for a non-existent recipe returns failure.
    /// </summary>
    [Test]
    public void Handle_WhenRecipeNotFound_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer();
        var scroll = Item.CreateRecipeScroll("unknown-recipe", "Unknown Recipe", 100);

        // Act
        var result = _handler.Handle(player, scroll);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.WasConsumed.Should().BeFalse();
        result.ResultType.Should().Be(ItemUseResultType.Failed);
        result.Message.Should().Contain("faded");

        // Verify learn was NOT called
        _mockRecipeService.Verify(s => s.LearnRecipe(It.IsAny<Player>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that using an item with wrong effect type returns failure.
    /// </summary>
    [Test]
    public void Handle_WithWrongEffectType_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer();
        var potion = Item.CreateHealthPotion(); // Has Heal effect, not LearnRecipe

        // Act
        var result = _handler.Handle(player, potion);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.WasConsumed.Should().BeFalse();
        result.ResultType.Should().Be(ItemUseResultType.Failed);
        result.Message.Should().Contain("cannot be used");
    }

    /// <summary>
    /// Verifies that using an item without a recipe ID returns failure.
    /// </summary>
    [Test]
    public void Handle_WithMissingRecipeId_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer();
        // Create an item with LearnRecipe effect but no RecipeId
        var invalidScroll = new Item(
            name: "Blank Scroll",
            description: "A blank scroll",
            type: ItemType.Consumable,
            effect: ItemEffect.LearnRecipe,
            recipeId: null);

        // Act
        var result = _handler.Handle(player, invalidScroll);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.WasConsumed.Should().BeFalse();
        result.ResultType.Should().Be(ItemUseResultType.Failed);
        result.Message.Should().Contain("blank");
    }

    // ═══════════════════════════════════════════════════════════════
    // Handle ARGUMENT VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Handle_WithNullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var scroll = Item.CreateRecipeScroll("steel-sword", "Steel Sword");

        // Act
        var act = () => _handler.Handle(null!, scroll);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Handle_WithNullItem_ThrowsArgumentNullException()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var act = () => _handler.Handle(player, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Handle CASE INSENSITIVITY TESTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that recipe ID lookup is case-insensitive.
    /// </summary>
    [Test]
    public void Handle_RecipeIdIsCaseInsensitive()
    {
        // Arrange
        var player = CreateTestPlayer();
        // Recipe ID is normalized to lowercase in CreateRecipeScroll
        var scroll = Item.CreateRecipeScroll("STEEL-SWORD", "Steel Sword");

        _mockRecipeService.Setup(s => s.IsRecipeKnown(player, "steel-sword")).Returns(false);
        _mockRecipeService.Setup(s => s.LearnRecipe(player, "steel-sword"))
            .Returns(LearnRecipeResult.Success("steel-sword", "Steel Sword"));

        // Act
        var result = _handler.Handle(player, scroll);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.WasConsumed.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer");
    }
}
