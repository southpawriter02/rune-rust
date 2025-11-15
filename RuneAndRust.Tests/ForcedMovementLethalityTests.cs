using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.29.5: Tests for forced movement with environmental kills
/// </summary>
[TestClass]
public class ForcedMovementLethalityTests
{
    private ForcedMovementService _forcedMovementService = null!;
    private BattlefieldGrid _grid = null!;
    private List<EnvironmentalObject> _environmentalObjects = null!;

    [TestInitialize]
    public void Setup()
    {
        _forcedMovementService = new ForcedMovementService();
        _grid = new BattlefieldGrid(5); // 5 columns
        _environmentalObjects = new List<EnvironmentalObject>();
    }

    [TestMethod]
    public void TryPushTileBased_IntoLavaRiver_CausesInstantDeath()
    {
        // Arrange
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 1);

        var lavaPosition = new GridPosition(Zone.Enemy, Row.Front, 2);
        var lavaRiver = new EnvironmentalObject
        {
            Name = "[Chasm/Lava River]",
            GridPosition = lavaPosition.ToString(),
            BlocksMovement = true,
            IsHazard = true
        };
        _environmentalObjects.Add(lavaRiver);

        // Act: Push enemy 1 tile to the right (into lava)
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 1,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success, "Push should execute");
        Assert.IsTrue(result.WasLethal, "Push into lava should be lethal");
        Assert.AreEqual("[Chasm/Lava River]", result.KillingHazard);
        Assert.AreEqual(0, enemy.CurrentHP, "Enemy should be dead (0 HP)");
        Assert.AreEqual(lavaPosition, enemy.Position, "Enemy should be at lava position");
    }

    [TestMethod]
    public void TryPushTileBased_ThreeTilesWithLavaAtTwo_StopsAtLavaWithDeath()
    {
        // Arrange: Push 3 tiles, but lava is at tile 2
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 0);

        // Lava at position 2 (Enemy zone, Front row, column 2)
        var lavaPosition = new GridPosition(Zone.Enemy, Row.Front, 2);
        var lavaRiver = new EnvironmentalObject
        {
            Name = "[Chasm/Lava River]",
            GridPosition = lavaPosition.ToString(),
            BlocksMovement = true
        };
        _environmentalObjects.Add(lavaRiver);

        // Act: Push 3 tiles to the right
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 3,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.WasLethal, "Should be lethal");
        Assert.AreEqual(1, result.TilesTraversed, "Should traverse 1 tile before hitting lava at tile 2");
        Assert.AreEqual(0, enemy.CurrentHP, "Enemy should be dead");
        Assert.AreEqual(lavaPosition, enemy.Position, "Enemy should be at lava position");
    }

    [TestMethod]
    public void TryPushTileBased_IntoCollapsedStructure_StopsWithoutDeath()
    {
        // Arrange: Non-lethal blocking (collapsed structure)
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 1);

        var wallPosition = new GridPosition(Zone.Enemy, Row.Front, 2);
        var wall = new EnvironmentalObject
        {
            Name = "[Collapsed Structure]",
            GridPosition = wallPosition.ToString(),
            BlocksMovement = true
        };
        _environmentalObjects.Add(wall);

        // Act: Push 1 tile into wall
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 1,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success, "Push should execute");
        Assert.IsFalse(result.WasLethal, "Wall should not be lethal");
        Assert.AreEqual(0, result.TilesTraversed, "Should not traverse into wall");
        Assert.IsTrue(enemy.CurrentHP > 0, "Enemy should be alive");
        // Enemy should still be at original position (not pushed into wall)
        Assert.AreEqual(new GridPosition(Zone.Enemy, Row.Front, 1), enemy.Position);
    }

    [TestMethod]
    public void TryPushTileBased_OffBattlefieldEdge_StopsAtEdge()
    {
        // Arrange: Push toward edge, tile 5 doesn't exist (grid has columns 0-4)
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 3);

        // Act: Push 3 tiles to the right (would go off edge)
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 3,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success, "Push should execute");
        Assert.IsFalse(result.WasLethal, "Should not be lethal");
        Assert.AreEqual(1, result.TilesTraversed, "Should stop at column 4 (last valid column)");
        Assert.AreEqual(new GridPosition(Zone.Enemy, Row.Front, 4), enemy.Position);
    }

    [TestMethod]
    public void TryPushTileBased_ThroughPassableTiles_SuccessfulPush()
    {
        // Arrange: Push through clear tiles
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 0);

        // Act: Push 2 tiles to the right (all clear)
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 2,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.WasLethal);
        Assert.AreEqual(2, result.TilesTraversed);
        Assert.AreEqual(new GridPosition(Zone.Enemy, Row.Front, 2), enemy.Position);
    }

    [TestMethod]
    public void TryPushTileBased_PushIntoChasmVariation_CausesInstantDeath()
    {
        // Arrange: Test alternate hazard name "[Chasm]"
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 1);

        var chasmPosition = new GridPosition(Zone.Enemy, Row.Front, 2);
        var chasm = new EnvironmentalObject
        {
            Name = "[Chasm]",
            GridPosition = chasmPosition.ToString(),
            BlocksMovement = true,
            IsHazard = true
        };
        _environmentalObjects.Add(chasm);

        // Act
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 1,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.WasLethal, "Chasm should be lethal");
        Assert.AreEqual("[Chasm]", result.KillingHazard);
        Assert.AreEqual(0, enemy.CurrentHP);
    }

    [TestMethod]
    public void TryPushTileBased_PushWithBurningGround_PassesThroughAndSurvives()
    {
        // Arrange: Push through damaging but passable hazard
        var warrior = CreateTestPlayer("Warrior", Zone.Player, Row.Front, 0);
        var enemy = CreateTestEnemy("TestEnemy", Zone.Enemy, Row.Front, 0);

        var burningPosition = new GridPosition(Zone.Enemy, Row.Front, 1);
        var burningGround = new EnvironmentalObject
        {
            Name = "[Burning Ground]",
            GridPosition = burningPosition.ToString(),
            BlocksMovement = false, // Passable
            IsHazard = true
        };
        _environmentalObjects.Add(burningGround);

        // Act: Push through burning ground
        var result = _forcedMovementService.TryPushTileBased(
            warrior,
            enemy,
            direction: (columnDelta: 1, rowDelta: 0),
            distance: 2,
            _grid,
            _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.WasLethal, "Burning ground not lethal for forced movement");
        Assert.AreEqual(2, result.TilesTraversed, "Should pass through burning ground");
        Assert.AreEqual(new GridPosition(Zone.Enemy, Row.Front, 2), enemy.Position);
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
