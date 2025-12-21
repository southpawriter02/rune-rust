using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Data;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.Models.Crafting;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the CraftingService class.
/// Validates crafting mechanics including ingredient validation, WITS-based rolls,
/// and outcome determination (Failure, Success, Masterwork, Catastrophe).
/// </summary>
public class CraftingServiceTests
{
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ITraumaService> _mockTraumaService;
    private readonly Mock<ILogger<CraftingService>> _mockLogger;
    private readonly CraftingService _sut;

    private readonly Character _testCharacter;
    private readonly Item _scrapWood;
    private readonly Item _oilyRag;
    private readonly Item _torch;

    public CraftingServiceTests()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockItemRepository = new Mock<IItemRepository>();
        _mockTraumaService = new Mock<ITraumaService>();
        _mockLogger = new Mock<ILogger<CraftingService>>();

        _sut = new CraftingService(
            _mockDiceService.Object,
            _mockInventoryService.Object,
            _mockItemRepository.Object,
            _mockTraumaService.Object,
            _mockLogger.Object);

        // Set up test character with WITS 5
        _testCharacter = new Character
        {
            Name = "Test Crafter",
            Wits = 5
        };

        // Set up test items
        _scrapWood = new Item
        {
            Id = Guid.NewGuid(),
            Name = "scrap_wood",
            ItemType = ItemType.Material,
            IsStackable = true
        };

        _oilyRag = new Item
        {
            Id = Guid.NewGuid(),
            Name = "oily_rag",
            ItemType = ItemType.Material,
            IsStackable = true
        };

        _torch = new Item
        {
            Id = Guid.NewGuid(),
            Name = "torch",
            ItemType = ItemType.Consumable
        };
    }

    #region CraftItemAsync - Ingredient Validation Tests

    [Fact]
    public async Task CraftItem_WithUnknownRecipe_ReturnsFailure()
    {
        // Arrange
        var unknownRecipeId = "RCP_UNKNOWN";

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, unknownRecipeId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.Message.Should().Contain("not found");
        result.RecipeId.Should().Be(unknownRecipeId);
    }

    [Fact]
    public async Task CraftItem_WithInsufficientIngredients_ReturnsFailure()
    {
        // Arrange - Character has no items in inventory
        _testCharacter.Inventory = new List<InventoryItem>();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.Message.Should().Contain("Missing ingredients");
    }

    [Fact]
    public async Task CraftItem_WithValidIngredients_ConsumesIngredients()
    {
        // Arrange - Set up inventory with required ingredients
        SetupCharacterWithTorchIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 3); // DC is 2, so 3 is success
        SetupItemRepository();

        _mockInventoryService
            .Setup(s => s.RemoveItemAsync(It.IsAny<Character>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new InventoryResult(true, "Removed"));
        _mockInventoryService
            .Setup(s => s.AddItemAsync(It.IsAny<Character>(), It.IsAny<Item>(), It.IsAny<int>()))
            .ReturnsAsync(new InventoryResult(true, "Added"));

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        _mockInventoryService.Verify(
            s => s.RemoveItemAsync(_testCharacter, "scrap_wood", 1),
            Times.Once);
        _mockInventoryService.Verify(
            s => s.RemoveItemAsync(_testCharacter, "oily_rag", 1),
            Times.Once);
    }

    #endregion

    #region CraftItemAsync - Outcome Tests

    [Fact]
    public async Task CraftItem_RollMeetsDc_ReturnsSuccess()
    {
        // Arrange
        SetupCharacterWithTorchIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 2); // DC is exactly 2
        SetupItemRepository();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(CraftingOutcome.Success);
        result.OutputQuality.Should().Be(QualityTier.ClanForged);
        result.Message.Should().Contain("Success");
    }

    [Fact]
    public async Task CraftItem_RollExceedsDcBy5_ReturnsMasterwork()
    {
        // Arrange
        SetupCharacterWithTorchIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 7); // DC 2 + 5 = 7 for masterwork
        SetupItemRepository();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(CraftingOutcome.Masterwork);
        result.OutputQuality.Should().Be(QualityTier.MythForged);
        result.Message.Should().Contain("MASTERWORK");
    }

    [Fact]
    public async Task CraftItem_RollBelowDc_ReturnsFailure()
    {
        // Arrange
        SetupCharacterWithTorchIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 1); // DC is 2, so 1 is failure
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Failure);
        result.OutputItemId.Should().BeNull();
        result.Message.Should().Contain("Failure");
    }

    [Fact]
    public async Task CraftItem_NetNegativeSuccesses_ReturnsCatastrophe()
    {
        // Arrange
        SetupCharacterWithTorchIngredients();
        SetupCatastropheCraftRoll(); // Net successes < 0
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(CraftingOutcome.Catastrophe);
        result.OutputItemId.Should().BeNull();
        result.Message.Should().Contain("CATASTROPHE");
    }

    [Fact]
    public async Task CraftItem_RollExactlyAtMasterworkThreshold_ReturnsMasterwork()
    {
        // Arrange - Masterwork threshold is DC + 5, so for DC 2 we need 7 successes
        SetupCharacterWithTorchIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 7);
        SetupItemRepository();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.Outcome.Should().Be(CraftingOutcome.Masterwork);
    }

    #endregion

    #region HasIngredients Tests

    [Fact]
    public void HasIngredients_WithExactQuantity_ReturnsTrue()
    {
        // Arrange
        SetupCharacterWithTorchIngredients();
        var recipe = RecipeRegistry.GetById("RCP_BOD_TORCH")!;

        // Act
        var result = _sut.HasIngredients(_testCharacter, recipe);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasIngredients_WithExcessQuantity_ReturnsTrue()
    {
        // Arrange - Give character 5 of each ingredient when only 1 is needed
        SetupCharacterWithExcessIngredients();
        var recipe = RecipeRegistry.GetById("RCP_BOD_TORCH")!;

        // Act
        var result = _sut.HasIngredients(_testCharacter, recipe);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasIngredients_WithMissingItem_ReturnsFalse()
    {
        // Arrange - Only give scrap_wood, missing oily_rag
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem
            {
                Item = _scrapWood,
                Quantity = 1
            }
        };
        var recipe = RecipeRegistry.GetById("RCP_BOD_TORCH")!;

        // Act
        var result = _sut.HasIngredients(_testCharacter, recipe);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasIngredients_WithInsufficientQuantity_ReturnsFalse()
    {
        // Arrange - RCP_BOD_ROPE needs 3 fiber_bundle, only have 2
        var fiberBundle = new Item { Name = "fiber_bundle", ItemType = ItemType.Material };
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = fiberBundle, Quantity = 2 }
        };
        var recipe = RecipeRegistry.GetById("RCP_BOD_ROPE")!;

        // Act
        var result = _sut.HasIngredients(_testCharacter, recipe);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Recipe Query Tests

    [Fact]
    public void GetAvailableRecipes_FiltersToAffordable()
    {
        // Arrange - Only have ingredients for torch
        SetupCharacterWithTorchIngredients();

        // Act
        var result = _sut.GetAvailableRecipes(_testCharacter);

        // Assert
        result.Should().Contain(r => r.RecipeId == "RCP_BOD_TORCH");
        result.Should().NotContain(r => r.RecipeId == "RCP_ALC_STIM"); // Don't have these ingredients
    }

    [Fact]
    public void GetRecipesByTrade_ReturnsCorrectTrade()
    {
        // Act
        var bodgingRecipes = _sut.GetRecipesByTrade(CraftingTrade.Bodging);
        var alchemyRecipes = _sut.GetRecipesByTrade(CraftingTrade.Alchemy);

        // Assert
        bodgingRecipes.Should().HaveCount(3);
        bodgingRecipes.Should().OnlyContain(r => r.Trade == CraftingTrade.Bodging);

        alchemyRecipes.Should().HaveCount(3);
        alchemyRecipes.Should().OnlyContain(r => r.Trade == CraftingTrade.Alchemy);
    }

    [Fact]
    public void GetRecipe_WithValidId_ReturnsRecipe()
    {
        // Act
        var result = _sut.GetRecipe("RCP_BOD_TORCH");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Improvised Torch");
        result.Trade.Should().Be(CraftingTrade.Bodging);
        result.BaseDc.Should().Be(2);
    }

    [Fact]
    public void GetRecipe_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = _sut.GetRecipe("INVALID_RECIPE_ID");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllRecipes_Returns12Recipes()
    {
        // Act
        var result = _sut.GetAllRecipes();

        // Assert
        result.Should().HaveCount(12);
    }

    #endregion

    #region Trade-Specific Catastrophe Tests (v0.3.1c)

    [Fact]
    public async Task CraftItem_AlchemyCatastrophe_DealsDamage()
    {
        // Arrange - Set up character with alchemy ingredients
        _testCharacter.CurrentHP = 50;
        _testCharacter.MaxHP = 50;
        SetupCharacterWithAlchemyIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_ALC_STIM");

        // Assert
        result.Outcome.Should().Be(CraftingOutcome.Catastrophe);
        result.DamageDealt.Should().Be(5); // RCP_ALC_STIM has CatastropheDamage = 5
        _testCharacter.CurrentHP.Should().Be(45); // 50 - 5 damage
    }

    [Fact]
    public async Task CraftItem_AlchemyCatastrophe_SetsResultDamage()
    {
        // Arrange
        _testCharacter.CurrentHP = 100;
        _testCharacter.MaxHP = 100;
        SetupCharacterWithAlchemyIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_ALC_STIM");

        // Assert
        result.DamageDealt.Should().NotBeNull();
        result.DamageDealt.Should().BeGreaterThan(0);
        result.Message.Should().Contain("damage");
    }

    [Fact]
    public async Task CraftItem_RuneforgingCatastrophe_AddsCorruption()
    {
        // Arrange - Set up character with runeforging ingredients
        _testCharacter.Corruption = 0;
        SetupCharacterWithRuneforgingIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();

        // Setup TraumaService to track AddCorruption calls
        _mockTraumaService
            .Setup(s => s.AddCorruption(It.IsAny<Character>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new CorruptionResult(3, 3, 3, CorruptionTier.Pristine, CorruptionTier.Pristine, false, false, "Runic Backlash"));

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_RUN_GLOW");

        // Assert
        result.Outcome.Should().Be(CraftingOutcome.Catastrophe);
        _mockTraumaService.Verify(
            s => s.AddCorruption(_testCharacter, 3, "Runic Backlash"),
            Times.Once);
    }

    [Fact]
    public async Task CraftItem_RuneforgingCatastrophe_SetsResultCorruption()
    {
        // Arrange
        _testCharacter.Corruption = 0;
        SetupCharacterWithRuneforgingIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();

        _mockTraumaService
            .Setup(s => s.AddCorruption(It.IsAny<Character>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new CorruptionResult(3, 3, 3, CorruptionTier.Pristine, CorruptionTier.Pristine, false, false, "Runic Backlash"));

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_RUN_GLOW");

        // Assert
        result.CorruptionAdded.Should().NotBeNull();
        result.CorruptionAdded.Should().Be(3); // RCP_RUN_GLOW has CatastropheCorruption = 3
        result.Message.Should().Contain("Corruption");
    }

    [Fact]
    public async Task CraftItem_BodgingCatastrophe_NoDamageOrCorruption()
    {
        // Arrange - Bodging has CatastropheType.None
        SetupCharacterWithTorchIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();
        var originalHp = _testCharacter.CurrentHP;
        var originalCorruption = _testCharacter.Corruption;

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_BOD_TORCH");

        // Assert
        result.Outcome.Should().Be(CraftingOutcome.Catastrophe);
        result.DamageDealt.Should().BeNull();
        result.CorruptionAdded.Should().BeNull();
        _testCharacter.CurrentHP.Should().Be(originalHp);
        _testCharacter.Corruption.Should().Be(originalCorruption);
    }

    [Fact]
    public async Task CraftItem_AlchemySuccess_CreatesPotionWithNoDamage()
    {
        // Arrange
        _testCharacter.CurrentHP = 50;
        SetupCharacterWithAlchemyIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 4); // DC 3, so success
        SetupItemRepository();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_ALC_STIM");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(CraftingOutcome.Success);
        result.DamageDealt.Should().BeNull();
        _testCharacter.CurrentHP.Should().Be(50); // No damage on success
    }

    [Fact]
    public async Task CraftItem_RuneforgingSuccess_NoCorruptionAdded()
    {
        // Arrange
        _testCharacter.Corruption = 0;
        SetupCharacterWithRuneforgingIngredients();
        SetupSuccessfulCraftRoll(netSuccesses: 3); // DC 2, so success
        SetupItemRepository();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_RUN_GLOW");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.CorruptionAdded.Should().BeNull();
        _mockTraumaService.Verify(
            s => s.AddCorruption(It.IsAny<Character>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Recipe_CatastropheType_DefaultsToNone()
    {
        // Arrange
        var recipe = new Recipe
        {
            RecipeId = "TEST_RECIPE",
            Name = "Test Recipe",
            Trade = CraftingTrade.Bodging
        };

        // Assert
        recipe.CatastropheType.Should().Be(CatastropheType.None);
        recipe.CatastropheDamage.Should().Be(0);
        recipe.CatastropheCorruption.Should().Be(0);
    }

    [Fact]
    public void Recipe_AlchemyRecipes_HaveExplosiveCatastrophe()
    {
        // Arrange
        var alchemyRecipes = RecipeRegistry.GetByTrade(CraftingTrade.Alchemy);

        // Assert
        alchemyRecipes.Should().AllSatisfy(r =>
        {
            r.CatastropheType.Should().Be(CatastropheType.Explosive);
            r.CatastropheDamage.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void Recipe_RuneforgingRecipes_HaveCorruptionCatastrophe()
    {
        // Arrange
        var runeforgingRecipes = RecipeRegistry.GetByTrade(CraftingTrade.Runeforging);

        // Assert
        runeforgingRecipes.Should().AllSatisfy(r =>
        {
            r.CatastropheType.Should().Be(CatastropheType.Corruption);
            r.CatastropheCorruption.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task CraftItem_AlchemyCatastrophe_DoesNotReduceHpBelowZero()
    {
        // Arrange - Character with very low HP
        _testCharacter.CurrentHP = 3;
        _testCharacter.MaxHP = 50;
        SetupCharacterWithAlchemyIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();

        // Act - RCP_ALC_STIM deals 5 damage on catastrophe
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_ALC_STIM");

        // Assert
        _testCharacter.CurrentHP.Should().Be(0); // Clamped to 0, not negative
    }

    [Fact]
    public async Task CraftItem_FirebombCatastrophe_DealsHighDamage()
    {
        // Arrange - Firebomb has the highest catastrophe damage (15)
        _testCharacter.CurrentHP = 100;
        SetupCharacterWithFirebombIngredients();
        SetupCatastropheCraftRoll();
        SetupInventoryServiceSuccess();

        // Act
        var result = await _sut.CraftItemAsync(_testCharacter, "RCP_ALC_FIREBOMB");

        // Assert
        result.DamageDealt.Should().Be(15);
        _testCharacter.CurrentHP.Should().Be(85);
    }

    #endregion

    #region Helper Methods

    private void SetupCharacterWithTorchIngredients()
    {
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem
            {
                Item = _scrapWood,
                Quantity = 1
            },
            new InventoryItem
            {
                Item = _oilyRag,
                Quantity = 1
            }
        };
    }

    private void SetupCharacterWithExcessIngredients()
    {
        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem
            {
                Item = _scrapWood,
                Quantity = 5
            },
            new InventoryItem
            {
                Item = _oilyRag,
                Quantity = 5
            }
        };
    }

    private void SetupSuccessfulCraftRoll(int netSuccesses)
    {
        // For net successes, we need successes >= netSuccesses and botches = 0
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(netSuccesses, 0, new List<int> { 8, 8, 8, 8, 8 }));
    }

    private void SetupCatastropheCraftRoll()
    {
        // Net negative: more botches than successes
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(1, 3, new List<int> { 8, 1, 1, 1, 4 }));
    }

    private void SetupItemRepository()
    {
        _mockItemRepository
            .Setup(r => r.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(_torch);
    }

    private void SetupInventoryServiceSuccess()
    {
        _mockInventoryService
            .Setup(s => s.RemoveItemAsync(It.IsAny<Character>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new InventoryResult(true, "Removed"));
        _mockInventoryService
            .Setup(s => s.AddItemAsync(It.IsAny<Character>(), It.IsAny<Item>(), It.IsAny<int>()))
            .ReturnsAsync(new InventoryResult(true, "Added"));
    }

    private void SetupCharacterWithAlchemyIngredients()
    {
        // RCP_ALC_STIM requires: blight_moss x1, clean_water x1
        var blightMoss = new Item { Name = "blight_moss", ItemType = ItemType.Material, IsStackable = true };
        var cleanWater = new Item { Name = "clean_water", ItemType = ItemType.Material, IsStackable = true };

        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = blightMoss, Quantity = 1 },
            new InventoryItem { Item = cleanWater, Quantity = 1 }
        };
    }

    private void SetupCharacterWithRuneforgingIngredients()
    {
        // RCP_RUN_GLOW requires: quartz_shard x1, aether_dust x1
        var quartzShard = new Item { Name = "quartz_shard", ItemType = ItemType.Material, IsStackable = true };
        var aetherDust = new Item { Name = "aether_dust", ItemType = ItemType.Material, IsStackable = true };

        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = quartzShard, Quantity = 1 },
            new InventoryItem { Item = aetherDust, Quantity = 1 }
        };
    }

    private void SetupCharacterWithFirebombIngredients()
    {
        // RCP_ALC_FIREBOMB requires: oily_rag x2, sulfur x1, glass_vial x1
        var oilyRag = new Item { Name = "oily_rag", ItemType = ItemType.Material, IsStackable = true };
        var sulfur = new Item { Name = "sulfur", ItemType = ItemType.Material, IsStackable = true };
        var glassVial = new Item { Name = "glass_vial", ItemType = ItemType.Material, IsStackable = true };

        _testCharacter.Inventory = new List<InventoryItem>
        {
            new InventoryItem { Item = oilyRag, Quantity = 2 },
            new InventoryItem { Item = sulfur, Quantity = 1 },
            new InventoryItem { Item = glassVial, Quantity = 1 }
        };
    }

    #endregion
}
