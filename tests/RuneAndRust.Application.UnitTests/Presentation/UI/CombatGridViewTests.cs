using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="CombatGridView"/>.
/// </summary>
[TestFixture]
public class CombatGridViewTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private ScreenLayout _layout = null!;
    private CombatGridView _gridView = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        var mockLogger = new Mock<ILogger<ScreenLayout>>();
        _layout = new ScreenLayout(_mockTerminal.Object, mockLogger.Object);
        
        _gridView = new CombatGridView(_mockTerminal.Object, _layout);
    }
    
    [TearDown]
    public void TearDown()
    {
        _layout?.Dispose();
    }

    #region RenderGrid Tests

    [Test]
    public void RenderGrid_WithEmptyGrid_ReturnsLines()
    {
        // Arrange
        var grid = new string[8, 8];
        for (var x = 0; x < 8; x++)
        for (var y = 0; y < 8; y++)
            grid[x, y] = "open";
        
        var entities = new List<(string, int, int, bool, bool)>();

        // Act
        var result = _gridView.RenderGrid(grid, entities);

        // Assert
        result.Should().NotBeEmpty();
        result.Any(line => line.Contains('A')).Should().BeTrue(); // Column label
        result.Any(line => line.Contains('.')).Should().BeTrue(); // Open terrain
    }

    [Test]
    public void RenderGrid_WithPlayer_ShowsPlayerSymbol()
    {
        // Arrange
        var grid = new string[8, 8];
        for (var x = 0; x < 8; x++)
        for (var y = 0; y < 8; y++)
            grid[x, y] = "open";
        
        var entities = new List<(string, int, int, bool, bool)>
        {
            ("Hero", 3, 3, true, false)
        };

        // Act
        var result = _gridView.RenderGrid(grid, entities);

        // Assert
        result.Any(line => line.Contains('@')).Should().BeTrue();
    }

    [Test]
    public void RenderGrid_WithMonster_ShowsMonsterSymbol()
    {
        // Arrange
        var grid = new string[8, 8];
        for (var x = 0; x < 8; x++)
        for (var y = 0; y < 8; y++)
            grid[x, y] = "open";
        
        var entities = new List<(string, int, int, bool, bool)>
        {
            ("Goblin", 5, 5, false, false)
        };

        // Act
        var result = _gridView.RenderGrid(grid, entities);

        // Assert
        result.Any(line => line.Contains('M')).Should().BeTrue();
    }

    [Test]
    public void RenderGrid_WithTerrainTypes_ShowsTerrainSymbols()
    {
        // Arrange
        var grid = new string[8, 8];
        for (var x = 0; x < 8; x++)
        for (var y = 0; y < 8; y++)
            grid[x, y] = "open";
        grid[2, 2] = "water";
        grid[3, 3] = "wall";
        
        var entities = new List<(string, int, int, bool, bool)>();

        // Act
        var result = _gridView.RenderGrid(grid, entities);

        // Assert
        result.Any(line => line.Contains('~')).Should().BeTrue(); // Water
        result.Any(line => line.Contains('#')).Should().BeTrue(); // Wall
    }

    #endregion

    #region Highlight Tests

    [Test]
    public void HighlightCells_AddsCellsToSet()
    {
        // Arrange
        var cells = new List<(int, int)> { (1, 1), (2, 2), (3, 3) };

        // Act
        _gridView.HighlightCells(cells, HighlightType.Movement);

        // Assert
        _gridView.HighlightedCells.Should().HaveCount(3);
        _gridView.CurrentHighlightType.Should().Be(HighlightType.Movement);
    }

    [Test]
    public void ClearHighlights_RemovesAllHighlights()
    {
        // Arrange
        _gridView.HighlightCells(new[] { (1, 1), (2, 2) }, HighlightType.Attack);
        _gridView.SetSelectedCell((3, 3));

        // Act
        _gridView.ClearHighlights();

        // Assert
        _gridView.HighlightedCells.Should().BeEmpty();
        _gridView.SelectedCell.Should().BeNull();
    }

    [Test]
    public void SetSelectedCell_SetsSelection()
    {
        // Act
        _gridView.SetSelectedCell((4, 4));

        // Assert
        _gridView.SelectedCell.Should().Be((4, 4));
    }

    #endregion

    #region GetHighlightColor Tests

    [Test]
    public void GetHighlightColor_ReturnsCorrectColors()
    {
        CombatGridView.GetHighlightColor(HighlightType.Movement).Should().Be(ConsoleColor.DarkBlue);
        CombatGridView.GetHighlightColor(HighlightType.Attack).Should().Be(ConsoleColor.DarkRed);
        CombatGridView.GetHighlightColor(HighlightType.Ability).Should().Be(ConsoleColor.DarkMagenta);
        CombatGridView.GetHighlightColor(HighlightType.Selected).Should().Be(ConsoleColor.Yellow);
    }

    #endregion
}
