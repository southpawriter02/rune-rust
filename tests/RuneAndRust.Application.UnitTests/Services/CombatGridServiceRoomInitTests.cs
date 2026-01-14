using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Mocks;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for CombatGridService room initialization methods.
/// </summary>
[TestFixture]
public class CombatGridServiceRoomInitTests
{
    private MockConfigurationProvider _mockConfig = null!;
    private CombatGridService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockConfig = new MockConfigurationProvider()
            .WithDefaultTerrainDefinitions()
            .WithDefaultCoverDefinitions();

        _service = new CombatGridService(
            _mockConfig,
            NullLogger<CombatGridService>.Instance);
    }

    // ===== InitializeFromRoom Tests =====

    [Test]
    public void InitializeFromRoom_CreatesGridWithCorrectSize()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);

        // Act
        var grid = _service.InitializeFromRoom(room);

        // Assert
        grid.Should().NotBeNull();
        grid.Width.Should().Be(8); // Default
        grid.Height.Should().Be(8);
    }

    [Test]
    public void InitializeFromRoom_SetsActiveGrid()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);

        // Act
        _service.InitializeFromRoom(room);

        // Assert
        _service.GetActiveGrid().Should().NotBeNull();
    }

    [Test]
    public void InitializeFromRoom_WithTerrainLayout_AppliesTerrain()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        var terrainLayout = new[]
        {
            new TerrainLayoutEntry { Positions = ["A1", "B1"], TerrainId = "rubble" }
        };

        // Act
        var grid = _service.InitializeFromRoom(room, terrainLayout);

        // Assert
        var cellA1 = grid.GetCell(new GridPosition(0, 0));
        var cellB1 = grid.GetCell(new GridPosition(1, 0));
        cellA1!.TerrainType.Should().Be(TerrainType.Difficult);
        cellB1!.TerrainType.Should().Be(TerrainType.Difficult);
    }

    [Test]
    public void InitializeFromRoom_WithCoverLayout_AddsCover()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        var coverLayout = new[]
        {
            new CoverLayoutEntry { Position = "C3", CoverId = "wooden-crate" }
        };

        // Act
        var grid = _service.InitializeFromRoom(room, null, coverLayout);

        // Assert
        var pos = new GridPosition(2, 2);
        grid.HasCover(pos).Should().BeTrue();
        grid.GetCover(pos)!.Name.Should().Be("Wooden Crate");
    }

    // ===== ApplyRoomTerrain Tests =====

    [Test]
    public void ApplyRoomTerrain_AppliesTerrainToPositions()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        _service.InitializeFromRoom(room);
        var terrainLayout = new[]
        {
            new TerrainLayoutEntry { Positions = ["D4"], TerrainId = "fire" }
        };

        // Act
        _service.ApplyRoomTerrain(terrainLayout);

        // Assert
        var grid = _service.GetActiveGrid()!;
        var cell = grid.GetCell(new GridPosition(3, 3));
        cell!.TerrainType.Should().Be(TerrainType.Hazardous);
    }

    [Test]
    public void ApplyRoomTerrain_WhenNoActiveGrid_DoesNotThrow()
    {
        // Arrange
        _service.ClearGrid();
        var terrainLayout = new[]
        {
            new TerrainLayoutEntry { Positions = ["A1"], TerrainId = "rubble" }
        };

        // Act
        var act = () => _service.ApplyRoomTerrain(terrainLayout);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void ApplyRoomTerrain_WithUnknownTerrain_SkipsEntry()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        _service.InitializeFromRoom(room);
        var terrainLayout = new[]
        {
            new TerrainLayoutEntry { Positions = ["A1"], TerrainId = "unknown-terrain" }
        };

        // Act
        _service.ApplyRoomTerrain(terrainLayout);

        // Assert - should not throw, cell remains normal
        var grid = _service.GetActiveGrid()!;
        var cell = grid.GetCell(new GridPosition(0, 0));
        cell!.TerrainType.Should().Be(TerrainType.Normal);
    }

    // ===== ApplyRoomCover Tests =====

    [Test]
    public void ApplyRoomCover_AddsCoverToPositions()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        _service.InitializeFromRoom(room);
        var coverLayout = new[]
        {
            new CoverLayoutEntry { Position = "E5", CoverId = "stone-pillar" }
        };

        // Act
        _service.ApplyRoomCover(coverLayout);

        // Assert
        var grid = _service.GetActiveGrid()!;
        var pos = new GridPosition(4, 4);
        grid.HasCover(pos).Should().BeTrue();
    }

    [Test]
    public void ApplyRoomCover_WithMultiplePositions_AddsCoverToAll()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        _service.InitializeFromRoom(room);
        var coverLayout = new[]
        {
            new CoverLayoutEntry { Positions = ["A1", "B2", "C3"], CoverId = "barrel" }
        };

        // Act
        _service.ApplyRoomCover(coverLayout);

        // Assert
        var grid = _service.GetActiveGrid()!;
        grid.HasCover(new GridPosition(0, 0)).Should().BeTrue();
        grid.HasCover(new GridPosition(1, 1)).Should().BeTrue();
        grid.HasCover(new GridPosition(2, 2)).Should().BeTrue();
    }

    [Test]
    public void ApplyRoomCover_WithUnknownCover_SkipsEntry()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        _service.InitializeFromRoom(room);
        var coverLayout = new[]
        {
            new CoverLayoutEntry { Position = "A1", CoverId = "unknown-cover" }
        };

        // Act
        _service.ApplyRoomCover(coverLayout);

        // Assert - should not throw, no cover added
        var grid = _service.GetActiveGrid()!;
        grid.HasCover(new GridPosition(0, 0)).Should().BeFalse();
    }

    [Test]
    public void ApplyRoomCover_WithOutOfBoundsPosition_SkipsPosition()
    {
        // Arrange
        var room = new Room("test-room", "Test Room", Position3D.Origin);
        _service.InitializeFromRoom(room);
        var coverLayout = new[]
        {
            new CoverLayoutEntry { Position = "Z99", CoverId = "wooden-crate" }
        };

        // Act
        var act = () => _service.ApplyRoomCover(coverLayout);

        // Assert - should not throw
        act.Should().NotThrow();
    }
}
