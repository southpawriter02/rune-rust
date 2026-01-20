using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="TerrainService"/> class (v0.5.2a).
/// </summary>
[TestFixture]
public class TerrainServiceTests
{
    private Mock<ICombatGridService> _mockGridService = null!;
    private Mock<IDiceService> _mockDiceService = null!;
    private Mock<IGameConfigurationProvider> _mockConfig = null!;
    private Mock<ILogger<TerrainService>> _mockLogger = null!;
    private CombatGrid _grid = null!;
    private TerrainService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockGridService = new Mock<ICombatGridService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockConfig = new Mock<IGameConfigurationProvider>();
        _mockLogger = new Mock<ILogger<TerrainService>>();

        // Setup default terrain definitions
        var terrainDefinitions = new List<TerrainDefinition>
        {
            TerrainDefinition.Create("normal-floor", "Stone Floor", TerrainType.Normal),
            TerrainDefinition.Create("rubble", "Rubble", TerrainType.Difficult, movementCostMultiplier: 2.0f),
            TerrainDefinition.Create("wall", "Wall", TerrainType.Impassable, blocksLOS: true),
            TerrainDefinition.Create("fire", "Fire", TerrainType.Hazardous, damageOnEntry: "1d6", damageType: "fire")
        };
        _mockConfig.Setup(c => c.GetTerrainDefinitions()).Returns(terrainDefinitions);

        // Create a 5x5 grid
        _grid = CombatGrid.Create(5, 5);
        _mockGridService.Setup(g => g.GetActiveGrid()).Returns(_grid);

        _service = new TerrainService(
            _mockGridService.Object,
            _mockDiceService.Object,
            _mockConfig.Object,
            _mockLogger.Object);
    }

    // ===== GetMovementCostMultiplier Tests =====

    [Test]
    public void GetMovementCostMultiplier_NormalTerrain_Returns1()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("normal-floor");

        // Act
        var multiplier = _service.GetMovementCostMultiplier(position);

        // Assert
        multiplier.Should().Be(1.0f);
    }

    [Test]
    public void GetMovementCostMultiplier_DifficultTerrain_Returns2()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("rubble");

        // Act
        var multiplier = _service.GetMovementCostMultiplier(position);

        // Assert
        multiplier.Should().Be(2.0f);
    }

    [Test]
    public void GetMovementCostMultiplier_BaseTerrain_ReturnsCorrectValue()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrain(TerrainType.Difficult);

        // Act
        var multiplier = _service.GetMovementCostMultiplier(position);

        // Assert
        multiplier.Should().Be(2.0f);
    }

    [Test]
    public void GetMovementCostMultiplier_InvalidPosition_Returns1()
    {
        // Arrange
        var position = new GridPosition(100, 100); // Out of bounds

        // Act
        var multiplier = _service.GetMovementCostMultiplier(position);

        // Assert
        multiplier.Should().Be(1.0f);
    }

    // ===== GetMovementCost Tests =====

    [Test]
    public void GetMovementCost_NormalTerrain_CardinalDirection_Returns2()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("normal-floor");

        // Act
        var cost = _service.GetMovementCost(position, MovementDirection.North);

        // Assert
        cost.Should().Be(2); // Base cardinal cost is 2
    }

    [Test]
    public void GetMovementCost_DifficultTerrain_CardinalDirection_Returns4()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("rubble");

        // Act
        var cost = _service.GetMovementCost(position, MovementDirection.North);

        // Assert
        cost.Should().Be(4); // Base 2 x 2.0 multiplier = 4
    }

    [Test]
    public void GetMovementCost_DifficultTerrain_DiagonalDirection_Returns6()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("rubble");

        // Act
        var cost = _service.GetMovementCost(position, MovementDirection.NorthEast);

        // Assert
        cost.Should().Be(6); // Base 3 x 2.0 multiplier = 6
    }

    // ===== IsPassable Tests =====

    [Test]
    public void IsPassable_NormalTerrain_ReturnsTrue()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("normal-floor");

        // Act
        var isPassable = _service.IsPassable(position);

        // Assert
        isPassable.Should().BeTrue();
    }

    [Test]
    public void IsPassable_ImpassableTerrain_ReturnsFalse()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("wall");

        // Act
        var isPassable = _service.IsPassable(position);

        // Assert
        isPassable.Should().BeFalse();
    }

    [Test]
    public void IsPassable_InvalidPosition_ReturnsFalse()
    {
        // Arrange
        var position = new GridPosition(100, 100);

        // Act
        var isPassable = _service.IsPassable(position);

        // Assert
        isPassable.Should().BeFalse();
    }

    // ===== DealsDamage Tests =====

    [Test]
    public void DealsDamage_HazardousTerrain_ReturnsTrue()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("fire");

        // Act
        var dealsDamage = _service.DealsDamage(position);

        // Assert
        dealsDamage.Should().BeTrue();
    }

    [Test]
    public void DealsDamage_NormalTerrain_ReturnsFalse()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("normal-floor");

        // Act
        var dealsDamage = _service.DealsDamage(position);

        // Assert
        dealsDamage.Should().BeFalse();
    }

    [Test]
    public void DealsDamage_NoTerrainDefinition_ReturnsFalse()
    {
        // Arrange
        var position = new GridPosition(2, 2);

        // Act
        var dealsDamage = _service.DealsDamage(position);

        // Assert
        dealsDamage.Should().BeFalse();
    }

    // ===== GetTerrainDamage Tests =====

    [Test]
    public void GetTerrainDamage_HazardousTerrain_ReturnsDamageResult()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("fire");

        var pool = new DicePool(1, DiceType.D6, 0, false, 10);
        var mockResult = new DiceRollResult(
            pool: pool,
            rolls: new[] { 4 },
            total: 4,
            advantageType: AdvantageType.Normal,
            explosionRolls: Array.Empty<int>(),
            allRollTotals: new[] { 4 },
            selectedRollIndex: 0);
        _mockDiceService
            .Setup(d => d.Roll(It.IsAny<string>(), It.IsAny<AdvantageType>()))
            .Returns(mockResult);

        // Act
        var result = _service.GetTerrainDamage(position);

        // Assert
        result.Should().NotBeNull();
        result!.Value.DamageDealt.Should().BeTrue();
        result.Value.Damage.Should().Be(4);
        result.Value.DamageType.Should().Be("fire");
        result.Value.TerrainName.Should().Be("Fire");
        result.Value.Message.Should().Contain("4");
    }

    [Test]
    public void GetTerrainDamage_NormalTerrain_ReturnsNull()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("normal-floor");

        // Act
        var result = _service.GetTerrainDamage(position);

        // Assert
        result.Should().BeNull();
    }

    // ===== GetTerrainType Tests =====

    [Test]
    public void GetTerrainType_WithDefinition_ReturnsDefinitionType()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrainDefinition("rubble");

        // Act
        var type = _service.GetTerrainType(position);

        // Assert
        type.Should().Be(TerrainType.Difficult);
    }

    [Test]
    public void GetTerrainType_WithBaseTerrain_ReturnsCellType()
    {
        // Arrange
        var position = new GridPosition(2, 2);
        _grid.GetCell(position)!.SetTerrain(TerrainType.Hazardous);

        // Act
        var type = _service.GetTerrainType(position);

        // Assert
        type.Should().Be(TerrainType.Hazardous);
    }

    // ===== SetTerrain Tests =====

    [Test]
    public void SetTerrain_ByType_UpdatesCell()
    {
        // Arrange
        var position = new GridPosition(2, 2);

        // Act
        _service.SetTerrain(position, TerrainType.Difficult);

        // Assert
        _grid.GetCell(position)!.TerrainType.Should().Be(TerrainType.Difficult);
    }

    [Test]
    public void SetTerrain_ByDefinitionId_UpdatesCell()
    {
        // Arrange
        var position = new GridPosition(2, 2);

        // Act
        _service.SetTerrain(position, "fire");

        // Assert
        _grid.GetCell(position)!.TerrainDefinitionId.Should().Be("fire");
    }

    // ===== GetAllTerrainDefinitions Tests =====

    [Test]
    public void GetAllTerrainDefinitions_ReturnsLoadedDefinitions()
    {
        // Act
        var definitions = _service.GetAllTerrainDefinitions().ToList();

        // Assert
        definitions.Should().HaveCount(4);
        definitions.Should().Contain(d => d.Id == "normal-floor");
        definitions.Should().Contain(d => d.Id == "rubble");
        definitions.Should().Contain(d => d.Id == "wall");
        definitions.Should().Contain(d => d.Id == "fire");
    }
}
