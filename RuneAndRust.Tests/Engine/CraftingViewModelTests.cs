using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Data;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the CraftingService.BuildViewModel method (v0.3.7b).
/// Validates ViewModel construction, recipe filtering, and ingredient availability mapping.
/// </summary>
public class CraftingViewModelTests
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

    public CraftingViewModelTests()
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
    }

    #region BuildViewModel - Trade Filtering Tests

    [Fact]
    public void BuildViewModel_FiltersRecipesByTrade()
    {
        // Arrange
        // RecipeRegistry has pre-seeded recipes

        // Act
        var bodgingVm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging);
        var alchemyVm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Alchemy);

        // Assert
        bodgingVm.Recipes.Should().AllSatisfy(r => r.Trade.Should().Be(CraftingTrade.Bodging));
        alchemyVm.Recipes.Should().AllSatisfy(r => r.Trade.Should().Be(CraftingTrade.Alchemy));
    }

    [Fact]
    public void BuildViewModel_SetsCorrectCharacterName()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging);

        // Assert
        vm.CharacterName.Should().Be("Test Crafter");
    }

    [Fact]
    public void BuildViewModel_SetsCrafterWits()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging);

        // Assert
        vm.CrafterWits.Should().Be(5);
    }

    [Fact]
    public void BuildViewModel_SetsSelectedTrade()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Runeforging);

        // Assert
        vm.SelectedTrade.Should().Be(CraftingTrade.Runeforging);
    }

    #endregion

    #region BuildViewModel - Recipe Index Tests

    [Fact]
    public void BuildViewModel_AssignsOneBasedIndices()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging);

        // Assert
        if (vm.Recipes.Count > 0)
        {
            vm.Recipes[0].Index.Should().Be(1);
            if (vm.Recipes.Count > 1)
            {
                vm.Recipes[1].Index.Should().Be(2);
            }
        }
    }

    [Fact]
    public void BuildViewModel_SetsSelectedRecipeIndex()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: 2);

        // Assert
        vm.SelectedRecipeIndex.Should().Be(2);
    }

    #endregion

    #region BuildViewModel - Ingredient Availability Tests

    [Fact]
    public void BuildViewModel_SetsCanCraft_WhenIngredientsAvailable()
    {
        // Arrange - Give the character enough ingredients for a Bodging Torch recipe
        _testCharacter.Inventory.Add(new InventoryItem
        {
            Item = _scrapWood,
            Quantity = 10
        });
        _testCharacter.Inventory.Add(new InventoryItem
        {
            Item = _oilyRag,
            Quantity = 10
        });

        // Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging);

        // Assert - At least one recipe should be craftable with these materials
        vm.Recipes.Should().Contain(r => r.CanCraft);
    }

    [Fact]
    public void BuildViewModel_SetsCanCraft_False_WhenIngredientsMissing()
    {
        // Arrange - Empty inventory
        _testCharacter.Inventory.Clear();

        // Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging);

        // Assert - No recipes should be craftable with empty inventory
        vm.Recipes.Should().AllSatisfy(r => r.CanCraft.Should().BeFalse());
    }

    #endregion

    #region BuildViewModel - Details Panel Tests

    [Fact]
    public void BuildViewModel_BuildsDetailsForSelectedRecipe()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: 0);

        // Assert
        if (vm.Recipes.Count > 0)
        {
            vm.SelectedRecipeDetails.Should().NotBeNull();
            vm.SelectedRecipeDetails!.RecipeId.Should().Be(vm.Recipes[0].RecipeId);
        }
    }

    [Fact]
    public void BuildViewModel_NullDetails_WhenIndexOutOfRange()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: 999);

        // Assert
        vm.SelectedRecipeDetails.Should().BeNull();
    }

    [Fact]
    public void BuildViewModel_NullDetails_WhenNoRecipes()
    {
        // Arrange - Use a trade that might have no recipes (or negative index)
        // Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: -1);

        // Assert
        vm.SelectedRecipeDetails.Should().BeNull();
    }

    [Fact]
    public void BuildViewModel_DetailsIncludeIngredients()
    {
        // Arrange & Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: 0);

        // Assert
        if (vm.SelectedRecipeDetails != null)
        {
            vm.SelectedRecipeDetails.Ingredients.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void BuildViewModel_IngredientView_IsSatisfied_WhenAvailableGteRequired()
    {
        // Arrange - Give character enough of one ingredient
        _testCharacter.Inventory.Add(new InventoryItem
        {
            Item = _scrapWood,
            Quantity = 100
        });

        // Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: 0);

        // Assert
        if (vm.SelectedRecipeDetails != null)
        {
            var scrapWoodIngredient = vm.SelectedRecipeDetails.Ingredients
                .FirstOrDefault(i => i.ItemName.Equals("scrap_wood", StringComparison.OrdinalIgnoreCase));

            if (scrapWoodIngredient != null)
            {
                scrapWoodIngredient.AvailableQuantity.Should().Be(100);
                scrapWoodIngredient.IsSatisfied.Should().BeTrue();
            }
        }
    }

    [Fact]
    public void BuildViewModel_IngredientView_IsSatisfied_False_WhenAvailableLtRequired()
    {
        // Arrange - Empty inventory
        _testCharacter.Inventory.Clear();

        // Act
        var vm = _sut.BuildViewModel(_testCharacter, CraftingTrade.Bodging, selectedIndex: 0);

        // Assert
        if (vm.SelectedRecipeDetails != null && vm.SelectedRecipeDetails.Ingredients.Count > 0)
        {
            vm.SelectedRecipeDetails.Ingredients.Should().AllSatisfy(i =>
            {
                i.AvailableQuantity.Should().Be(0);
                i.IsSatisfied.Should().BeFalse();
            });
        }
    }

    #endregion
}
