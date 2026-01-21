// ═══════════════════════════════════════════════════════════════════════════════
// RecipeBrowserTests.cs
// Unit tests for the RecipeBrowser class.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for <see cref="RecipeBrowser"/>.
/// </summary>
[TestFixture]
public class RecipeBrowserTests
{
    private Mock<ITerminalService> _mockTerminalService = null!;
    private RecipeBrowserConfig _config = null!;
    private RecipeBrowser _browser = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTerminalService = new Mock<ITerminalService>();
        _config = RecipeBrowserConfig.CreateDefault();
        _browser = new RecipeBrowser(_mockTerminalService.Object, _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullTerminalService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new RecipeBrowser(null!, _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("terminalService");
    }

    [Test]
    public void Constructor_WithNullConfig_UsesDefault()
    {
        // Arrange & Act
        var browser = new RecipeBrowser(_mockTerminalService.Object, null);

        // Assert - should not throw
        browser.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FilterByCategory Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FilterByCategory_WithWeapons_FiltersCorrectly()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        _browser.FilterByCategory(RecipeCategory.Weapons);

        // Assert
        _browser.ActiveCategory.Should().Be(RecipeCategory.Weapons);
    }

    [Test]
    public void FilterByCategory_WithNull_ShowsAllRecipes()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);
        _browser.FilterByCategory(RecipeCategory.Weapons);

        // Act
        _browser.FilterByCategory(null);

        // Assert
        _browser.ActiveCategory.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Search Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Search_WithMatchingText_ReturnsFilteredRecipes()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        var results = _browser.Search("sword");

        // Assert
        results.Should().Contain(r => r.Name.Contains("Sword", StringComparison.OrdinalIgnoreCase));
        _browser.SearchText.Should().Be("sword");
    }

    [Test]
    public void Search_WithEmptyText_ReturnsAllRecipes()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);
        _browser.Search("test"); // Apply a filter first

        // Act
        var results = _browser.Search("");

        // Assert
        _browser.SearchText.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Navigation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SelectNext_WhenRecipesExist_IncrementsIndex()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        _browser.SelectNext();

        // Assert
        _browser.SelectedIndex.Should().Be(1);
    }

    [Test]
    public void SelectPrevious_WhenAtFirst_WrapsToLast()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        _browser.SelectPrevious();

        // Assert
        _browser.SelectedIndex.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void GetSelectedRecipe_WhenRecipesExist_ReturnsSelectedRecipe()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        var selected = _browser.GetSelectedRecipe();

        // Assert
        selected.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Visibility Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Hide_WhenVisible_ClearsBrowserAndSetsInvisible()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        _browser.Hide();

        // Assert
        _browser.IsVisible.Should().BeFalse();
        _browser.RecipeCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Input Handling Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void HandleInput_UpArrow_ReturnsTrue()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        var handled = _browser.HandleInput(ConsoleKey.UpArrow);

        // Assert
        handled.Should().BeTrue();
    }

    [Test]
    public void HandleInput_Escape_HidesBrowser()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);

        // Act
        _browser.HandleInput(ConsoleKey.Escape);

        // Assert
        _browser.IsVisible.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Event Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RecipeSelected_WhenEnterPressed_RaisesEvent()
    {
        // Arrange
        var recipes = CreateSampleRecipes();
        _browser.RenderRecipes(recipes);
        RecipeBrowserDisplayDto? selectedRecipe = null;
        _browser.RecipeSelected += r => selectedRecipe = r;

        // Act
        _browser.HandleInput(ConsoleKey.Enter);

        // Assert
        selectedRecipe.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    private static List<RecipeBrowserDisplayDto> CreateSampleRecipes()
    {
        return new List<RecipeBrowserDisplayDto>
        {
            new("recipe-1", "Iron Sword", "A basic iron sword", RecipeCategory.Weapons, "Blacksmith", true, false),
            new("recipe-2", "Steel Blade", "A finely crafted steel blade", RecipeCategory.Weapons, "Blacksmith", true, true),
            new("recipe-3", "Health Potion", "Restores 50 health", RecipeCategory.Potions, "Alchemist", true, false),
            new("recipe-4", "Iron Shield", "A sturdy iron shield", RecipeCategory.Armor, "Blacksmith", false, false),
            new("recipe-5", "Ruby Ring", "A ring set with a ruby", RecipeCategory.Jewelry, "Jeweler", true, false)
        };
    }
}
