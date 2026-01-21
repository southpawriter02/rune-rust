// ═══════════════════════════════════════════════════════════════════════════════
// CraftingStationMenuTests.cs
// Unit tests for the CraftingStationMenu class.
// Version: 0.13.3b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.UnitTests.UI;

/// <summary>
/// Unit tests for CraftingStationMenu.
/// </summary>
[TestFixture]
public class CraftingStationMenuTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private MaterialAvailabilityDisplay _materialDisplay = null!;
    private CraftingProgressRenderer _progressRenderer = null!;
    private CraftingStationConfig _config = null!;
    private CraftingStationMenu _menu = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _config = new CraftingStationConfig();

        // Create real instances for testing
        var resourceRenderer = new ResourceStackRenderer();
        _materialDisplay = new MaterialAvailabilityDisplay(
            resourceRenderer, _mockTerminal.Object, _config);
        _progressRenderer = new CraftingProgressRenderer(_config);

        _menu = new CraftingStationMenu(
            _materialDisplay,
            _progressRenderer,
            _mockTerminal.Object,
            _config);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Navigation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SelectNext_WithRecipes_AdvancesSelection()
    {
        // Arrange
        SetupMenuWithRecipes(3);

        // Act
        _menu.SelectNext();

        // Assert
        _menu.SelectedIndex.Should().Be(1);
    }

    [Test]
    public void SelectNext_AtLastRecipe_WrapsToFirst()
    {
        // Arrange
        SetupMenuWithRecipes(3);
        _menu.SelectRecipe(2); // Move to last

        // Act
        _menu.SelectNext();

        // Assert
        _menu.SelectedIndex.Should().Be(0);
    }

    [Test]
    public void SelectPrevious_AtFirstRecipe_WrapsToLast()
    {
        // Arrange
        SetupMenuWithRecipes(3);
        // Selection starts at 0

        // Act
        _menu.SelectPrevious();

        // Assert
        _menu.SelectedIndex.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Input Handling Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void HandleInput_EscapeKey_HidesMenu()
    {
        // Arrange
        SetupMenuWithRecipes(3);
        _menu.IsVisible.Should().BeTrue();

        // Act
        var handled = _menu.HandleInput(ConsoleKey.Escape);

        // Assert
        handled.Should().BeTrue();
        _menu.IsVisible.Should().BeFalse();
    }

    [Test]
    [TestCase(ConsoleKey.UpArrow)]
    [TestCase(ConsoleKey.W)]
    public void HandleInput_UpArrowOrW_SelectsPrevious(ConsoleKey key)
    {
        // Arrange
        SetupMenuWithRecipes(3);
        _menu.SelectRecipe(1);

        // Act
        var handled = _menu.HandleInput(key);

        // Assert
        handled.Should().BeTrue();
        _menu.SelectedIndex.Should().Be(0);
    }

    [Test]
    [TestCase(ConsoleKey.DownArrow)]
    [TestCase(ConsoleKey.S)]
    public void HandleInput_DownArrowOrS_SelectsNext(ConsoleKey key)
    {
        // Arrange
        SetupMenuWithRecipes(3);

        // Act
        var handled = _menu.HandleInput(key);

        // Assert
        handled.Should().BeTrue();
        _menu.SelectedIndex.Should().Be(1);
    }

    [Test]
    public void HandleInput_UnhandledKey_ReturnsFalse()
    {
        // Arrange
        SetupMenuWithRecipes(3);

        // Act
        var handled = _menu.HandleInput(ConsoleKey.F1);

        // Assert
        handled.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Craft Action Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void HandleInput_CraftKey_WithCraftableRecipe_RaisesCraftRequested()
    {
        // Arrange
        var recipes = new[]
        {
            CreateRecipe("sword-1", "Iron Sword", isCraftable: true)
        };
        SetupMenuWithRecipes(recipes);

        string? requestedRecipeId = null;
        _menu.CraftRequested += id => requestedRecipeId = id;

        // Act
        _menu.HandleInput(ConsoleKey.C);

        // Assert
        requestedRecipeId.Should().Be("sword-1");
    }

    [Test]
    public void HandleInput_CraftKey_WithNonCraftableRecipe_DoesNotRaiseCraftRequested()
    {
        // Arrange
        var recipes = new[]
        {
            CreateRecipe("sword-1", "Iron Sword", isCraftable: false)
        };
        SetupMenuWithRecipes(recipes);

        bool craftRequested = false;
        _menu.CraftRequested += _ => craftRequested = true;

        // Act
        _menu.HandleInput(ConsoleKey.C);

        // Assert
        craftRequested.Should().BeFalse("should not request craft for non-craftable recipe");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Menu State Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void RenderStation_SetsMenuVisibility()
    {
        // Arrange
        var station = CreateStation("forge-1", "Test Forge");

        // Act
        _menu.RenderStation(station, Array.Empty<StationRecipeDisplayDto>(), Array.Empty<ResourceStackDisplayDto>());

        // Assert
        _menu.IsVisible.Should().BeTrue();
        _menu.CurrentStation.Should().Be(station);
    }

    [Test]
    public void Hide_ClearsMenuState()
    {
        // Arrange
        SetupMenuWithRecipes(3);

        // Act
        _menu.Hide();

        // Assert
        _menu.IsVisible.Should().BeFalse();
        _menu.CurrentStation.Should().BeNull();
        _menu.RecipeCount.Should().Be(0);
        _menu.SelectedIndex.Should().Be(0);
    }

    [Test]
    public void Hide_RaisesMenuClosedEvent()
    {
        // Arrange
        SetupMenuWithRecipes(3);
        bool menuClosed = false;
        _menu.MenuClosed += () => menuClosed = true;

        // Act
        _menu.Hide();

        // Assert
        menuClosed.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithNullMaterialDisplay_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new CraftingStationMenu(
            materialDisplay: null!,
            _progressRenderer,
            _mockTerminal.Object,
            _config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("materialDisplay");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Test Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    private void SetupMenuWithRecipes(int count)
    {
        var recipes = Enumerable.Range(0, count)
            .Select(i => CreateRecipe($"recipe-{i}", $"Recipe {i}", isCraftable: true))
            .ToArray();
        SetupMenuWithRecipes(recipes);
    }

    private void SetupMenuWithRecipes(IEnumerable<StationRecipeDisplayDto> recipes)
    {
        var station = CreateStation("test-station", "Test Station");
        _menu.SetPosition(0, 0);
        _menu.RenderStation(station, recipes, Array.Empty<ResourceStackDisplayDto>());
    }

    private static CraftingStationDisplayDto CreateStation(string id, string name)
    {
        return new CraftingStationDisplayDto(
            StationId: id,
            Name: name,
            Description: "Test description",
            StationType: "Forge",
            SkillRequired: null);
    }

    private static StationRecipeDisplayDto CreateRecipe(string id, string name, bool isCraftable)
    {
        return new StationRecipeDisplayDto(
            RecipeId: id,
            Name: name,
            Description: "A test recipe",
            Materials: Array.Empty<MaterialRequirementDto>(),
            OutputItemId: $"{id}-item",
            OutputItemName: name,
            OutputQuantity: 1,
            IsCraftable: isCraftable);
    }
}
