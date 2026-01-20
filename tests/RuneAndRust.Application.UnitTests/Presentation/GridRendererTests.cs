using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Shared.Adapters;
using RuneAndRust.Presentation.Shared.DTOs;

namespace RuneAndRust.Application.UnitTests.Presentation;

/// <summary>
/// Unit tests for <see cref="GridRenderer"/> (v0.5.0c).
/// </summary>
[TestFixture]
public class GridRendererTests
{
    private GridRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new GridRenderer();
    }

    private static CombatGrid CreateGridWithPlayer()
    {
        var grid = CombatGrid.CreateDefault();
        var playerId = Guid.NewGuid();
        grid.PlaceEntity(playerId, new GridPosition(3, 3), isPlayer: true);
        return grid;
    }

    private static CombatGrid CreateGridWithPlayerAndMonsters()
    {
        var grid = CombatGrid.CreateDefault();
        var playerId = Guid.NewGuid();
        var monster1Id = Guid.NewGuid();
        var monster2Id = Guid.NewGuid();
        
        grid.PlaceEntity(playerId, new GridPosition(3, 6), isPlayer: true);
        grid.PlaceEntity(monster1Id, new GridPosition(2, 1), isPlayer: false);
        grid.PlaceEntity(monster2Id, new GridPosition(5, 1), isPlayer: false);
        
        return grid;
    }

    // ===== RenderGrid Tests =====

    [Test]
    public void RenderGrid_WithDefaultOptions_ContainsHeader()
    {
        // Arrange
        var grid = CreateGridWithPlayer();

        // Act
        var result = _renderer.RenderGrid(grid);

        // Assert
        result.Should().Contain("Combat Grid");
    }

    [Test]
    public void RenderGrid_WithTurnNumber_ShowsTurn()
    {
        // Arrange
        var grid = CreateGridWithPlayer();
        var options = new GridRenderOptions { TurnNumber = 3 };

        // Act
        var result = _renderer.RenderGrid(grid, options);

        // Assert
        result.Should().Contain("[Turn 3]");
    }

    [Test]
    public void RenderGrid_ContainsColumnHeaders()
    {
        // Arrange
        var grid = CreateGridWithPlayer();

        // Act
        var result = _renderer.RenderGrid(grid);

        // Assert
        result.Should().Contain("A");
        result.Should().Contain("H");
    }

    [Test]
    public void RenderGrid_ContainsPlayerSymbol()
    {
        // Arrange
        var grid = CreateGridWithPlayer();

        // Act
        var result = _renderer.RenderGrid(grid);

        // Assert
        result.Should().Contain("@");
    }

    [Test]
    public void RenderGrid_ContainsMonsterSymbols()
    {
        // Arrange
        var grid = CreateGridWithPlayerAndMonsters();

        // Act
        var result = _renderer.RenderGrid(grid);

        // Assert
        result.Should().Contain("M");
    }

    [Test]
    public void RenderGrid_WithLegend_ContainsLegend()
    {
        // Arrange
        var grid = CreateGridWithPlayer();
        var options = new GridRenderOptions { ShowLegend = true };

        // Act
        var result = _renderer.RenderGrid(grid, options);

        // Assert
        result.Should().Contain("Legend:");
        result.Should().Contain("@ = You");
    }

    [Test]
    public void RenderGrid_ContainsBoxDrawing()
    {
        // Arrange
        var grid = CreateGridWithPlayer();

        // Act
        var result = _renderer.RenderGrid(grid);

        // Assert
        result.Should().Contain("+---+");
        result.Should().Contain("|");
    }

    // ===== RenderCompactGrid Tests =====

    [Test]
    public void RenderCompactGrid_NoBoxDrawing()
    {
        // Arrange
        var grid = CreateGridWithPlayer();
        var options = GridRenderOptions.CompactDefault;

        // Act
        var result = _renderer.RenderCompactGrid(grid, options);

        // Assert
        result.Should().NotContain("+---+");
    }

    [Test]
    public void RenderCompactGrid_ContainsCoordinates()
    {
        // Arrange
        var grid = CreateGridWithPlayer();

        // Act
        var result = _renderer.RenderCompactGrid(grid);

        // Assert
        result.Should().Contain("A");
        result.Should().Contain("1");
    }

    [Test]
    public void RenderCompactGrid_SideLegend()
    {
        // Arrange
        var grid = CreateGridWithPlayer();
        var options = new GridRenderOptions { Compact = true, ShowLegend = true };

        // Act
        var result = _renderer.RenderCompactGrid(grid, options);

        // Assert
        result.Should().Contain("Legend:");
        result.Should().Contain("@ = You");
    }

    // ===== RenderLegend Tests =====

    [Test]
    public void RenderLegend_ContainsAllSymbols()
    {
        // Arrange
        var grid = CreateGridWithPlayer();

        // Act
        var result = _renderer.RenderLegend(grid);

        // Assert
        result.Should().Contain("@ = You");
        result.Should().Contain("M = Monster");
        result.Should().Contain(". = Empty");
        result.Should().Contain("# = Wall");
    }

    [Test]
    public void RenderLegend_IncludesPlayerPosition()
    {
        // Arrange
        var grid = CreateGridWithPlayer(); // Player at D4 (3, 3)

        // Act
        var result = _renderer.RenderLegend(grid);

        // Assert
        result.Should().Contain("(D4)");
    }

    // ===== RenderCombatantList Tests =====

    [Test]
    public void RenderCombatantList_IncludesPlayer()
    {
        // Arrange
        var grid = CreateGridWithPlayer();
        var player = new Player("TestHero");
        player.SetCombatGridPosition(new GridPosition(3, 3));
        player.ResetMovementPoints();

        // Act
        var result = _renderer.RenderCombatantList(grid, player, []);

        // Assert
        result.Should().Contain("You");
        result.Should().Contain("(D4)");
        result.Should().Contain("HP:");
    }

    [Test]
    public void RenderCombatantList_IncludesMonsters()
    {
        // Arrange
        var grid = CreateGridWithPlayerAndMonsters();
        var player = new Player("TestHero");
        var monster = new Monster("Goblin", "A goblin", 30, Stats.Default);
        monster.SetCombatGridPosition(new GridPosition(2, 1));

        // Act
        var result = _renderer.RenderCombatantList(grid, player, [monster]);

        // Assert
        result.Should().Contain("Goblin");
        result.Should().Contain("(C2)");
    }

    [Test]
    public void RenderCombatantList_ExcludesDeadMonsters()
    {
        // Arrange
        var grid = CreateGridWithPlayer();
        var player = new Player("TestHero");
        // Create a valid monster then kill it with damage
        var monster = new Monster("DeadGoblin", "A dead goblin", 10, new Stats(10, 5, 0));
        monster.TakeDamage(15); // Deal enough damage to kill it

        // Act
        var result = _renderer.RenderCombatantList(grid, player, [monster]);

        // Assert
        result.Should().NotContain("DeadGoblin");
    }
}
