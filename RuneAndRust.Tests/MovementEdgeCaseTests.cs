using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.29.5: Edge case tests for movement system integration
/// </summary>
[TestClass]
public class MovementEdgeCaseTests
{
    private PositioningService _positioningService = null!;
    private ForcedMovementService _forcedMovementService = null!;
    private BattlefieldGrid _grid = null!;
    private List<EnvironmentalObject> _environmentalObjects = null!;

    [TestInitialize]
    public void Setup()
    {
        _positioningService = new PositioningService();
        _forcedMovementService = new ForcedMovementService();
        _grid = new BattlefieldGrid(5);
        _environmentalObjects = new List<EnvironmentalObject>();
    }

    [TestMethod]
    public void IsPassable_OccupiedTileWithBurningGround_ReturnsFalse()
    {
        // Arrange: Tile has both occupant and burning ground
        var position = new GridPosition(Zone.Player, Row.Front, 0);
        var tile = _grid.GetTile(position)!;

        // Occupy the tile
        tile.IsOccupied = true;
        tile.OccupantId = "player";

        // Add burning ground (passable hazard)
        var burningGround = new EnvironmentalObject
        {
            Name = "[Burning Ground]",
            GridPosition = position.ToString(),
            BlocksMovement = false
        };

        var envObjects = new List<EnvironmentalObject> { burningGround };

        // Act
        var passable = tile.IsPassable(envObjects);

        // Assert
        Assert.IsFalse(passable, "Occupied tiles should not be passable even with passable hazards");
    }

    [TestMethod]
    public void GetBlockingFeature_MultipleObjects_ReturnsFirstBlocking()
    {
        // Arrange: Multiple environmental objects, one blocks
        var position = new GridPosition(Zone.Player, Row.Front, 0);
        var tile = _grid.GetTile(position)!;

        var envObjects = new List<EnvironmentalObject>
        {
            new EnvironmentalObject
            {
                Name = "[Burning Ground]",
                GridPosition = position.ToString(),
                BlocksMovement = false
            },
            new EnvironmentalObject
            {
                Name = "[Collapsed Structure]",
                GridPosition = position.ToString(),
                BlocksMovement = true
            }
        };

        // Act
        var feature = tile.GetBlockingFeature(envObjects);

        // Assert
        Assert.AreEqual("[Collapsed Structure]", feature);
    }

    [TestMethod]
    public void TryPushTileBased_ZeroDistance_NoMovement()
    {
        // Arrange
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("Enemy", Zone.Enemy, Row.Front, 1);
        var originalPosition = enemy.Position!.Value;

        // Act: Push with distance 0
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 0,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.TilesTraversed);
        Assert.AreEqual(originalPosition, enemy.Position);
    }

    [TestMethod]
    public void TryPushTileBased_NegativeDirection_PushesLeft()
    {
        // Arrange
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("Enemy", Zone.Enemy, Row.Front, 2);

        // Act: Push 1 tile to the left
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: -1, rowDelta: 0),
            distance: 1,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.TilesTraversed);
        Assert.AreEqual(new GridPosition(Zone.Enemy, Row.Front, 1), enemy.Position);
    }

    [TestMethod]
    public void MoveCombatant_NullEnvironmentalObjects_WorksWithoutErrors()
    {
        // Arrange: No environmental objects list provided
        var player = CreateTestPlayer("TestPlayer", Zone.Player, Row.Front, 0);
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // Act: Pass null for environmental objects
        var result = _positioningService.MoveCombatant(player, targetPosition, _grid, null);

        // Assert
        Assert.IsTrue(result.Success, "Should work with null environmental objects");
        Assert.AreEqual(targetPosition, player.Position);
    }

    [TestMethod]
    public void GetBlockingFeature_NullEnvironmentalObjects_ReturnsNull()
    {
        // Arrange
        var position = new GridPosition(Zone.Player, Row.Front, 0);
        var tile = _grid.GetTile(position)!;

        // Act
        var feature = tile.GetBlockingFeature(null);

        // Assert
        Assert.IsNull(feature);
    }

    [TestMethod]
    public void TryPushTileBased_ImmediatelyAtEdge_StopsImmediately()
    {
        // Arrange: Enemy at rightmost column
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("Enemy", Zone.Enemy, Row.Front, 4); // Rightmost column

        // Act: Try to push right (off edge)
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 1,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.TilesTraversed, "Should not move off edge");
        Assert.AreEqual(new GridPosition(Zone.Enemy, Row.Front, 4), enemy.Position);
    }

    [TestMethod]
    public void TryPushTileBased_ImmediatelyIntoLava_KillsWithZeroTraversal()
    {
        // Arrange: Enemy right next to lava
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("Enemy", Zone.Enemy, Row.Front, 0);

        // Lava at next position
        var lavaPosition = new GridPosition(Zone.Enemy, Row.Front, 1);
        var lavaRiver = new EnvironmentalObject
        {
            Name = "[Lava River]",
            GridPosition = lavaPosition.ToString(),
            BlocksMovement = true
        };
        _environmentalObjects.Add(lavaRiver);

        // Act: Push 1 tile into lava
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 1,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.WasLethal);
        Assert.AreEqual(0, result.TilesTraversed, "No traversal before hitting lava");
        Assert.AreEqual(0, enemy.CurrentHP);
    }

    [TestMethod]
    public void IsPassable_EmptyEnvironmentalObjectsList_ReturnsTrue()
    {
        // Arrange
        var position = new GridPosition(Zone.Player, Row.Front, 0);
        var tile = _grid.GetTile(position)!;

        var emptyList = new List<EnvironmentalObject>();

        // Act
        var passable = tile.IsPassable(emptyList);

        // Assert
        Assert.IsTrue(passable, "Empty environmental objects list should allow passage");
    }

    // Helper methods
    private PlayerCharacter CreateTestPlayer(string name, Zone zone, Row row, int column)
    {
        return new PlayerCharacter
        {
            Name = name,
            Position = new GridPosition(zone, row, column),
            CurrentHP = 50,
            MaxHP = 50,
            Stamina = 100,
            MaxStamina = 100,
            KineticEnergy = 0,
            MaxKineticEnergy = 100,
            Attributes = new RuneAndRust.Core.Attributes
            {
                Finesse = 3,
                Wits = 3
            }
        };
    }

    private Enemy CreateTestEnemy(string name, Zone zone, Row row, int column)
    {
        return new Enemy
        {
            Id = "enemy_" + name.ToLower(),
            Name = name,
            Position = new GridPosition(zone, row, column),
            CurrentHP = 30,
            MaxHP = 30,
            KineticEnergy = 0,
            MaxKineticEnergy = 100,
            Corruption = 0,
            Attributes = new RuneAndRust.Core.Attributes
            {
                Finesse = 2,
                Wits = 2
            }
        };
    }
}
