using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="CombatGridService"/> (v0.5.0a).
/// </summary>
[TestFixture]
public class CombatGridServiceTests
{
    private CombatGridService _service = null!;
    private Mock<IGameConfigurationProvider> _configMock = null!;
    private Mock<ILogger<CombatGridService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IGameConfigurationProvider>();
        _configMock.Setup(c => c.GetGridSettings()).Returns(new GridSettings());

        _loggerMock = new Mock<ILogger<CombatGridService>>();

        _service = new CombatGridService(_configMock.Object, _loggerMock.Object);
    }

    private static Room CreateTestRoom(string name = "Test Room") =>
        new Room(name, "A test room.", Position3D.Origin);

    private static Player CreateTestPlayer() => new Player("TestHero");

    private static Monster CreateTestMonster(string name = "Goblin") =>
        new Monster(name, "A test monster", 1, Stats.Default);

    // ===== CreateGrid Tests =====

    [Test]
    public void CreateGrid_WithRoom_CreatesGridWithRoomId()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var grid = _service.CreateGrid(room);

        // Assert
        grid.Should().NotBeNull();
        grid.RoomId.Should().Be(room.Id);
        grid.Width.Should().Be(8); // Default from GridSettings
        grid.Height.Should().Be(8);
    }

    [Test]
    public void CreateGrid_UsesConfiguredDefaults()
    {
        // Arrange
        _configMock.Setup(c => c.GetGridSettings()).Returns(new GridSettings
        {
            DefaultWidth = 12,
            DefaultHeight = 10
        });
        _service = new CombatGridService(_configMock.Object, _loggerMock.Object);
        var room = CreateTestRoom();

        // Act
        var grid = _service.CreateGrid(room);

        // Assert
        grid.Width.Should().Be(12);
        grid.Height.Should().Be(10);
    }

    [Test]
    public void CreateGrid_WithCustomDimensions_OverridesDefaults()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act
        var grid = _service.CreateGrid(room, width: 6, height: 5);

        // Assert
        grid.Width.Should().Be(6);
        grid.Height.Should().Be(5);
    }

    // ===== InitializePositions Tests =====

    [Test]
    public void InitializePositions_PlacesPlayerAtCenterSouth()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var player = CreateTestPlayer();

        // Act
        var result = _service.InitializePositions(grid, player, []);

        // Assert
        result.Success.Should().BeTrue();
        // Center-south = (Width/2, Height-2) = (4, 6) for an 8x8 grid
        result.PlayerPosition.X.Should().Be(4);
        result.PlayerPosition.Y.Should().Be(6);
        grid.GetEntityPosition(player.Id).Should().Be(result.PlayerPosition);
    }

    [Test]
    public void InitializePositions_PlacesMonstersInNorthZone()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var player = CreateTestPlayer();
        var monsters = new[]
        {
            CreateTestMonster("Goblin"),
            CreateTestMonster("Orc")
        };

        // Act
        var result = _service.InitializePositions(grid, player, monsters);

        // Assert
        result.Success.Should().BeTrue();
        result.MonsterPositions.Should().HaveCount(2);

        foreach (var (monsterId, pos) in result.MonsterPositions)
        {
            pos.Y.Should().Be(1); // North zone = row 1
        }
    }

    [Test]
    public void InitializePositions_ReturnsFailure_WhenPlayerCannotBePlaced()
    {
        // Arrange - Create a 3x3 grid and fill center-south position
        var grid = CombatGrid.Create(3, 3);
        var blockingEntity = Guid.NewGuid();
        grid.PlaceEntity(blockingEntity, new GridPosition(1, 1), isPlayer: false);

        var player = CreateTestPlayer();

        // Act
        var result = _service.InitializePositions(grid, player, []);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to place player");
    }

    // ===== Active Grid Management Tests =====

    [Test]
    public void GetActiveGrid_AfterSetActiveGrid_ReturnsGrid()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act
        _service.SetActiveGrid(grid);
        var result = _service.GetActiveGrid();

        // Assert
        result.Should().BeSameAs(grid);
    }

    [Test]
    public void GetActiveGrid_Initially_ReturnsNull()
    {
        // Act
        var result = _service.GetActiveGrid();

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ClearGrid_AfterSet_ReturnsNull()
    {
        // Arrange
        _service.SetActiveGrid(CombatGrid.CreateDefault());

        // Act
        _service.ClearGrid();
        var result = _service.GetActiveGrid();

        // Assert
        result.Should().BeNull();
    }

    // ===== Query Delegation Tests =====

    [Test]
    public void GetEntityPosition_DelegatesToActiveGrid()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entityId = Guid.NewGuid();
        var position = new GridPosition(3, 3);
        grid.PlaceEntity(entityId, position, isPlayer: true);
        _service.SetActiveGrid(grid);

        // Act
        var result = _service.GetEntityPosition(entityId);

        // Assert
        result.Should().Be(position);
    }

    [Test]
    public void GetDistance_DelegatesToActiveGrid()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(0, 0), isPlayer: true);
        grid.PlaceEntity(entity2, new GridPosition(3, 4), isPlayer: false);
        _service.SetActiveGrid(grid);

        // Act
        var result = _service.GetDistance(entity1, entity2);

        // Assert
        result.Should().Be(4); // max(3, 4) = 4
    }

    [Test]
    public void AreAdjacent_DelegatesToActiveGrid()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        grid.PlaceEntity(entity1, new GridPosition(3, 3), isPlayer: true);
        grid.PlaceEntity(entity2, new GridPosition(4, 4), isPlayer: false);
        _service.SetActiveGrid(grid);

        // Act
        var result = _service.AreAdjacent(entity1, entity2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void AreAdjacent_NoActiveGrid_ReturnsFalse()
    {
        // Act
        var result = _service.AreAdjacent(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
