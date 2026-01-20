using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Shared.Adapters;
using RuneAndRust.Presentation.Shared.DTOs;

namespace RuneAndRust.Presentation.Shared.UnitTests.Adapters;

/// <summary>
/// Unit tests for GridRenderer terrain and cover display.
/// </summary>
[TestFixture]
public class GridRendererDisplayTests
{
    private GridRenderer _renderer = null!;
    private CombatGrid _grid = null!;

    [SetUp]
    public void Setup()
    {
        _renderer = new GridRenderer(NullLogger<GridRenderer>.Instance);
        _grid = CombatGrid.Create(8, 8);
    }

    // ===== RenderCell Tests =====

    [Test]
    public void RenderCell_WhenPlayerPresent_ReturnsPlayerChar()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: true);
        var options = GridRenderOptions.Default;

        // Act
        var result = _renderer.RenderCell(cell, null, options);

        // Assert
        result.Should().Be('@');
    }

    [Test]
    public void RenderCell_WhenMonsterPresent_ReturnsMonsterChar()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: false);
        var options = GridRenderOptions.Default;

        // Act
        var result = _renderer.RenderCell(cell, null, options);

        // Assert
        result.Should().Be('M');
    }

    [Test]
    public void RenderCell_WhenCoverPresent_ReturnsCoverChar()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        var coverDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, displayChar: '□');
        var cover = CoverObject.Create(coverDef, new GridPosition(0, 0));
        var options = GridRenderOptions.Default;

        // Act
        var result = _renderer.RenderCell(cell, cover, options);

        // Assert
        result.Should().Be('□');
    }

    [Test]
    public void RenderCell_WhenShowCoverFalse_IgnoresCover()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        var coverDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, displayChar: '□');
        var cover = CoverObject.Create(coverDef, new GridPosition(0, 0));
        var options = new GridRenderOptions { ShowCover = false };

        // Act
        var result = _renderer.RenderCell(cell, cover, options);

        // Assert
        result.Should().Be('.'); // Falls through to terrain
    }

    [Test]
    public void RenderCell_EntityTakesPriorityOverCover()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.PlaceEntity(Guid.NewGuid(), isPlayer: true);
        var coverDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, displayChar: '□');
        var cover = CoverObject.Create(coverDef, new GridPosition(0, 0));
        var options = GridRenderOptions.Default;

        // Act
        var result = _renderer.RenderCell(cell, cover, options);

        // Assert
        result.Should().Be('@'); // Player takes priority
    }

    // ===== GetTerrainChar Tests =====

    [Test]
    public void GetTerrainChar_ForNormalTerrain_ReturnsDot()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetTerrain(TerrainType.Normal);

        // Act
        var result = _renderer.GetTerrainChar(cell);

        // Assert
        result.Should().Be('.');
    }

    [Test]
    public void GetTerrainChar_ForDifficultTerrain_ReturnsTilde()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetTerrain(TerrainType.Difficult);

        // Act
        var result = _renderer.GetTerrainChar(cell);

        // Assert
        result.Should().Be('~');
    }

    [Test]
    public void GetTerrainChar_ForImpassableTerrain_ReturnsHash()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetTerrain(TerrainType.Impassable);

        // Act
        var result = _renderer.GetTerrainChar(cell);

        // Assert
        result.Should().Be('#');
    }

    [Test]
    public void GetTerrainChar_ForHazardousTerrain_ReturnsTriangle()
    {
        // Arrange
        var cell = GridCell.Create(0, 0);
        cell.SetTerrain(TerrainType.Hazardous);

        // Act
        var result = _renderer.GetTerrainChar(cell);

        // Assert
        result.Should().Be('▲');
    }

    // ===== GetUniqueCoverTypes Tests =====

    [Test]
    public void GetUniqueCoverTypes_ReturnsDistinctCoverObjects()
    {
        // Arrange
        var crateDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial);
        var pillarDef = CoverDefinition.Create("pillar", "Pillar", CoverType.Full);

        _grid.AddCover(CoverObject.Create(crateDef, new GridPosition(1, 1)));
        _grid.AddCover(CoverObject.Create(crateDef, new GridPosition(2, 2)));
        _grid.AddCover(CoverObject.Create(pillarDef, new GridPosition(3, 3)));

        // Act
        var result = _renderer.GetUniqueCoverTypes(_grid).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.DefinitionId == "crate");
        result.Should().Contain(c => c.DefinitionId == "pillar");
    }

    [Test]
    public void GetUniqueCoverTypes_ExcludesDestroyedCover()
    {
        // Arrange
        var crateDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, isDestructible: true, maxHitPoints: 10);
        var cover = CoverObject.Create(crateDef, new GridPosition(1, 1));
        cover.TakeDamage(20); // Destroy it
        _grid.AddCover(cover);

        // Act
        var result = _renderer.GetUniqueCoverTypes(_grid).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    // ===== RenderLegend Tests =====

    [Test]
    public void RenderLegend_IncludesBasicSymbols()
    {
        // Act
        var result = _renderer.RenderLegend(_grid, GridRenderOptions.Default);

        // Assert
        result.Should().Contain("@ = You");
        result.Should().Contain("M = Monster");
    }

    [Test]
    public void RenderLegend_IncludesCoverInfo()
    {
        // Arrange
        var crateDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, defenseBonus: 2, displayChar: '□');
        _grid.AddCover(CoverObject.Create(crateDef, new GridPosition(1, 1)));

        // Act
        var result = _renderer.RenderLegend(_grid, GridRenderOptions.Default);

        // Assert
        result.Should().Contain("□ = Crate");
        result.Should().Contain("+2 def");
    }

    [Test]
    public void RenderLegend_ShowsFullCoverInfo()
    {
        // Arrange
        var pillarDef = CoverDefinition.Create("pillar", "Pillar", CoverType.Full, displayChar: '█');
        _grid.AddCover(CoverObject.Create(pillarDef, new GridPosition(1, 1)));

        // Act
        var result = _renderer.RenderLegend(_grid, GridRenderOptions.Default);

        // Assert
        result.Should().Contain("█ = Pillar");
        result.Should().Contain("full cover");
    }

    // ===== RenderGrid with Cover Tests =====

    [Test]
    public void RenderGrid_DisplaysCoverSymbols()
    {
        // Arrange
        var crateDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, displayChar: '□');
        _grid.AddCover(CoverObject.Create(crateDef, new GridPosition(2, 2)));

        // Act
        var result = _renderer.RenderGrid(_grid);

        // Assert
        result.Should().Contain("□");
    }

    [Test]
    public void RenderGrid_WithShowCoverFalse_HidesCover()
    {
        // Arrange
        var crateDef = CoverDefinition.Create("crate", "Crate", CoverType.Partial, displayChar: '□');
        _grid.AddCover(CoverObject.Create(crateDef, new GridPosition(2, 2)));
        var options = new GridRenderOptions { ShowCover = false };

        // Act
        var result = _renderer.RenderGrid(_grid, options);

        // Assert
        result.Should().NotContain("□");
    }
}
