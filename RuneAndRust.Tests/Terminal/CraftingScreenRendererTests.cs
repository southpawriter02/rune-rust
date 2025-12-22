using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using RuneAndRust.Terminal.Services;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the CraftingScreenRenderer class (v0.3.7b).
/// Note: These tests focus on renderer construction and edge cases.
/// Full visual rendering tests are done via manual integration testing.
/// </summary>
public class CraftingScreenRendererTests
{
    private readonly Mock<ILogger<CraftingScreenRenderer>> _mockLogger;
    private readonly CraftingScreenRenderer _sut;

    public CraftingScreenRendererTests()
    {
        _mockLogger = new Mock<ILogger<CraftingScreenRenderer>>();
        _sut = new CraftingScreenRenderer(_mockLogger.Object);
    }

    #region Render Tests

    [Fact]
    public void Render_WithEmptyRecipeList_DoesNotThrow()
    {
        // Arrange
        var vm = CreateEmptyViewModel();

        // Act
        var action = () => _sut.Render(vm);

        // Assert - Should not throw even with empty recipe list
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithRecipes_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithRecipes();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithSelectedRecipe_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithRecipes() with { SelectedRecipeIndex = 1 };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithRecipeDetails_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithDetails();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithUncraftableRecipe_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithUncraftableRecipe();

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNullDetails_DoesNotThrow()
    {
        // Arrange
        var vm = CreateViewModelWithRecipes() with { SelectedRecipeDetails = null };

        // Act
        var action = () => _sut.Render(vm);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region CraftingViewHelper Tests

    [Theory]
    [InlineData(CraftingTrade.Bodging, "orange1")]
    [InlineData(CraftingTrade.Alchemy, "green")]
    [InlineData(CraftingTrade.Runeforging, "magenta1")]
    [InlineData(CraftingTrade.FieldMedicine, "cyan")]
    public void GetTradeColor_ReturnsCorrectColor(CraftingTrade trade, string expectedColor)
    {
        // Act
        var color = CraftingViewHelper.GetTradeColor(trade);

        // Assert
        color.Should().Be(expectedColor);
    }

    [Theory]
    [InlineData(CraftingTrade.Bodging, "\u2692")]
    [InlineData(CraftingTrade.Alchemy, "\u2697")]
    [InlineData(CraftingTrade.Runeforging, "\u2726")]
    [InlineData(CraftingTrade.FieldMedicine, "\u2695")]
    public void GetTradeIcon_ReturnsCorrectIcon(CraftingTrade trade, string expectedIcon)
    {
        // Act
        var icon = CraftingViewHelper.GetTradeIcon(trade);

        // Assert
        icon.Should().Be(expectedIcon);
    }

    [Fact]
    public void GetAvailabilityIndicator_ReturnsCheckmark_WhenCanCraft()
    {
        // Act
        var indicator = CraftingViewHelper.GetAvailabilityIndicator(true);

        // Assert
        indicator.Should().Contain("\u2713"); // Checkmark
        indicator.Should().Contain("green");
    }

    [Fact]
    public void GetAvailabilityIndicator_ReturnsCross_WhenCannotCraft()
    {
        // Act
        var indicator = CraftingViewHelper.GetAvailabilityIndicator(false);

        // Assert
        indicator.Should().Contain("\u2717"); // Cross
        indicator.Should().Contain("red");
    }

    [Fact]
    public void FormatDifficultyWithChance_ReturnsFormattedString()
    {
        // Act
        var result = CraftingViewHelper.FormatDifficultyWithChance(3, 5);

        // Assert
        result.Should().StartWith("DC 3");
        result.Should().Contain("%");
    }

    [Fact]
    public void FormatDifficultyWithChance_WithZeroWits_ReturnsZeroPercent()
    {
        // Act
        var result = CraftingViewHelper.FormatDifficultyWithChance(3, 0);

        // Assert
        result.Should().Be("DC 3 (0%)");
    }

    [Fact]
    public void GetDifficultyColor_ReturnsGreen_ForEasyDC()
    {
        // Act - High WITS vs low DC
        var color = CraftingViewHelper.GetDifficultyColor(1, 10);

        // Assert
        color.Should().Be("green");
    }

    [Fact]
    public void GetDifficultyColor_ReturnsRed_ForHardDC()
    {
        // Act - Low WITS vs high DC
        var color = CraftingViewHelper.GetDifficultyColor(10, 1);

        // Assert
        color.Should().Be("red");
    }

    #endregion

    #region Helper Methods

    private static CraftingViewModel CreateEmptyViewModel()
    {
        return new CraftingViewModel(
            CharacterName: "Test Crafter",
            CrafterWits: 5,
            SelectedTrade: CraftingTrade.Bodging,
            Recipes: new List<RecipeView>(),
            SelectedRecipeIndex: 0,
            SelectedRecipeDetails: null
        );
    }

    private static CraftingViewModel CreateViewModelWithRecipes()
    {
        var recipes = new List<RecipeView>
        {
            new RecipeView(
                Index: 1,
                RecipeId: "RCP_BOD_TORCH",
                Name: "Makeshift Torch",
                Trade: CraftingTrade.Bodging,
                DifficultyClass: 2,
                CanCraft: true
            ),
            new RecipeView(
                Index: 2,
                RecipeId: "RCP_BOD_REPAIR",
                Name: "Jury-Rig Repair",
                Trade: CraftingTrade.Bodging,
                DifficultyClass: 3,
                CanCraft: false
            )
        };

        return new CraftingViewModel(
            CharacterName: "Test Crafter",
            CrafterWits: 5,
            SelectedTrade: CraftingTrade.Bodging,
            Recipes: recipes,
            SelectedRecipeIndex: 0,
            SelectedRecipeDetails: null
        );
    }

    private static CraftingViewModel CreateViewModelWithDetails()
    {
        var recipes = new List<RecipeView>
        {
            new RecipeView(
                Index: 1,
                RecipeId: "RCP_BOD_TORCH",
                Name: "Makeshift Torch",
                Trade: CraftingTrade.Bodging,
                DifficultyClass: 2,
                CanCraft: true
            )
        };

        var ingredients = new List<IngredientView>
        {
            new IngredientView(
                ItemName: "scrap_wood",
                RequiredQuantity: 2,
                AvailableQuantity: 5,
                IsSatisfied: true
            ),
            new IngredientView(
                ItemName: "oily_rag",
                RequiredQuantity: 1,
                AvailableQuantity: 3,
                IsSatisfied: true
            )
        };

        var details = new RecipeDetailsView(
            RecipeId: "RCP_BOD_TORCH",
            Name: "Makeshift Torch",
            Description: "A simple torch fashioned from scrap wood and cloth.",
            Trade: CraftingTrade.Bodging,
            DifficultyClass: 2,
            CrafterWits: 5,
            Ingredients: ingredients,
            OutputItemName: "torch",
            OutputQuantity: 1,
            CanCraft: true
        );

        return new CraftingViewModel(
            CharacterName: "Test Crafter",
            CrafterWits: 5,
            SelectedTrade: CraftingTrade.Bodging,
            Recipes: recipes,
            SelectedRecipeIndex: 0,
            SelectedRecipeDetails: details
        );
    }

    private static CraftingViewModel CreateViewModelWithUncraftableRecipe()
    {
        var recipes = new List<RecipeView>
        {
            new RecipeView(
                Index: 1,
                RecipeId: "RCP_ALC_STIM",
                Name: "Stimulant Pack",
                Trade: CraftingTrade.Alchemy,
                DifficultyClass: 4,
                CanCraft: false
            )
        };

        var ingredients = new List<IngredientView>
        {
            new IngredientView(
                ItemName: "chemical_base",
                RequiredQuantity: 2,
                AvailableQuantity: 0,
                IsSatisfied: false
            ),
            new IngredientView(
                ItemName: "stimulant_extract",
                RequiredQuantity: 1,
                AvailableQuantity: 0,
                IsSatisfied: false
            )
        };

        var details = new RecipeDetailsView(
            RecipeId: "RCP_ALC_STIM",
            Name: "Stimulant Pack",
            Description: "A powerful stimulant that temporarily boosts stamina.",
            Trade: CraftingTrade.Alchemy,
            DifficultyClass: 4,
            CrafterWits: 5,
            Ingredients: ingredients,
            OutputItemName: "stimulant",
            OutputQuantity: 1,
            CanCraft: false
        );

        return new CraftingViewModel(
            CharacterName: "Test Crafter",
            CrafterWits: 5,
            SelectedTrade: CraftingTrade.Alchemy,
            Recipes: recipes,
            SelectedRecipeIndex: 0,
            SelectedRecipeDetails: details
        );
    }

    #endregion
}
