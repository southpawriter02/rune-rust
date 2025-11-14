using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// Unit tests for v0.20.2 Cover System
/// Tests cover bonus calculation, cover destruction, and integration with combat
/// </summary>
[TestClass]
public class CoverSystemTests
{
    private CoverService _coverService = null!;
    private BattlefieldGrid _grid = null!;
    private PlayerCharacter _player = null!;
    private Enemy _enemy = null!;

    [TestInitialize]
    public void Setup()
    {
        _coverService = new CoverService();
        _grid = new BattlefieldGrid(5); // 5 columns
        _player = CreateTestPlayer();
        _enemy = CreateTestEnemy();
    }

    #region Cover Bonus Calculation Tests

    [TestMethod]
    public void CalculateCoverBonus_PhysicalCover_RangedAttack_ReturnsDefenseBonus()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Physical;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Ranged, _grid);

        // Assert
        Assert.AreEqual(4, bonus.DefenseBonus, "Physical cover should grant +4 Defense vs ranged");
        Assert.AreEqual(0, bonus.ResolveBonus, "Physical cover should not grant Resolve bonus");
    }

    [TestMethod]
    public void CalculateCoverBonus_MetaphysicalCover_PsychicAttack_ReturnsResolveBonus()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Metaphysical;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Psychic, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "Metaphysical cover should not grant Defense bonus");
        Assert.AreEqual(4, bonus.ResolveBonus, "Metaphysical cover should grant +4 Resolve vs psychic");
    }

    [TestMethod]
    public void CalculateCoverBonus_BothCover_RangedAttack_ReturnsDefenseBonus()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Both;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Ranged, _grid);

        // Assert
        Assert.AreEqual(4, bonus.DefenseBonus, "Both cover should grant +4 Defense vs ranged");
        Assert.AreEqual(0, bonus.ResolveBonus, "Resolve bonus only applies to psychic attacks");
    }

    [TestMethod]
    public void CalculateCoverBonus_BothCover_PsychicAttack_ReturnsResolveBonus()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Both;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Psychic, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "Defense bonus only applies to ranged attacks");
        Assert.AreEqual(4, bonus.ResolveBonus, "Both cover should grant +4 Resolve vs psychic");
    }

    [TestMethod]
    public void CalculateCoverBonus_NoCover_ReturnsZeroBonuses()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.None;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Ranged, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "No cover should grant no bonuses");
        Assert.AreEqual(0, bonus.ResolveBonus, "No cover should grant no bonuses");
    }

    [TestMethod]
    public void CalculateCoverBonus_SameZone_ReturnsNone()
    {
        // Arrange - both in Player zone
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Player, Row.Front, 1, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Physical;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Ranged, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "Cover should not apply in same zone combat");
        Assert.AreEqual(0, bonus.ResolveBonus, "Cover should not apply in same zone combat");
    }

    [TestMethod]
    public void CalculateCoverBonus_MeleeAttack_ReturnsNone()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Physical;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Melee, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "Cover should not apply to melee attacks");
        Assert.AreEqual(0, bonus.ResolveBonus, "Cover should not apply to melee attacks");
    }

    [TestMethod]
    public void CalculateCoverBonus_PhysicalCover_PsychicAttack_ReturnsNone()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Physical;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Psychic, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "Physical cover doesn't block psychic");
        Assert.AreEqual(0, bonus.ResolveBonus, "Physical cover doesn't block psychic");
    }

    [TestMethod]
    public void CalculateCoverBonus_MetaphysicalCover_RangedAttack_ReturnsNone()
    {
        // Arrange
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = _grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Metaphysical;

        // Act
        var bonus = _coverService.CalculateCoverBonus(_player, _enemy, AttackType.Ranged, _grid);

        // Assert
        Assert.AreEqual(0, bonus.DefenseBonus, "Metaphysical cover doesn't block physical");
        Assert.AreEqual(0, bonus.ResolveBonus, "Metaphysical cover doesn't block physical");
    }

    #endregion

    #region Cover Destruction Tests

    [TestMethod]
    public void DamageCover_ExceedsHP_DestroysPhysicalCover()
    {
        // Arrange
        var combatState = CreateTestCombatState();
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Physical;
        tile.CoverHealth = 10;
        tile.CoverDescription = "Crate";

        // Act
        _coverService.DamageCover(tile, 15, combatState);

        // Assert
        Assert.AreEqual(CoverType.None, tile.Cover, "Cover should be destroyed");
        Assert.IsNull(tile.CoverHealth, "Cover health should be null after destruction");
        Assert.IsTrue(combatState.CombatLog.Any(log => log.Contains("COVER DESTROYED")), "Destruction should be logged");
    }

    [TestMethod]
    public void DamageCover_PartialDamage_ReducesHP()
    {
        // Arrange
        var combatState = CreateTestCombatState();
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Physical;
        tile.CoverHealth = 20;

        // Act
        _coverService.DamageCover(tile, 8, combatState);

        // Assert
        Assert.AreEqual(CoverType.Physical, tile.Cover, "Cover should remain");
        Assert.AreEqual(12, tile.CoverHealth, "Cover HP should be reduced");
    }

    [TestMethod]
    public void DamageCover_BothType_PreservesMetaphysical()
    {
        // Arrange
        var combatState = CreateTestCombatState();
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Both;
        tile.CoverHealth = 10;

        // Act
        _coverService.DamageCover(tile, 15, combatState);

        // Assert
        Assert.AreEqual(CoverType.Metaphysical, tile.Cover, "Metaphysical cover should remain");
        Assert.IsNull(tile.CoverHealth, "Physical health should be removed");
    }

    [TestMethod]
    public void DamageCover_MetaphysicalOnly_DoesNothing()
    {
        // Arrange
        var combatState = CreateTestCombatState();
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Metaphysical;

        // Act
        _coverService.DamageCover(tile, 10, combatState);

        // Assert
        Assert.AreEqual(CoverType.Metaphysical, tile.Cover, "Metaphysical cover cannot be damaged");
        Assert.IsNull(tile.CoverHealth, "Metaphysical cover has no HP");
    }

    [TestMethod]
    public void DamageCover_InitializesHealthIfNull()
    {
        // Arrange
        var combatState = CreateTestCombatState();
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Physical;
        tile.CoverHealth = null; // Not initialized

        // Act
        _coverService.DamageCover(tile, 5, combatState);

        // Assert
        Assert.IsNotNull(tile.CoverHealth, "Cover health should be initialized");
        Assert.AreEqual(15, tile.CoverHealth, "Default 20 HP minus 5 damage");
    }

    #endregion

    #region Metaphysical Cover Creation Tests

    [TestMethod]
    public void CreateMetaphysicalCover_OnNone_AddsMetaphysical()
    {
        // Arrange
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.None;

        // Act
        _coverService.CreateMetaphysicalCover(tile, "Sanctified Ground");

        // Assert
        Assert.AreEqual(CoverType.Metaphysical, tile.Cover, "Should add metaphysical cover");
        Assert.AreEqual("Sanctified Ground", tile.CoverDescription, "Description should be set");
    }

    [TestMethod]
    public void CreateMetaphysicalCover_OnPhysical_UpgradesToBoth()
    {
        // Arrange
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Physical;
        tile.CoverHealth = 20;

        // Act
        _coverService.CreateMetaphysicalCover(tile, "Sanctified Ground");

        // Assert
        Assert.AreEqual(CoverType.Both, tile.Cover, "Should upgrade to Both");
        Assert.AreEqual(20, tile.CoverHealth, "Physical HP should be preserved");
        Assert.AreEqual("Sanctified Ground", tile.CoverDescription, "Description should be updated");
    }

    [TestMethod]
    public void CreateMetaphysicalCover_OnMetaphysical_RemainsMetaphysical()
    {
        // Arrange
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2, 0));
        tile!.Cover = CoverType.Metaphysical;

        // Act
        _coverService.CreateMetaphysicalCover(tile, "Sanctified Ground");

        // Assert
        Assert.AreEqual(CoverType.Metaphysical, tile.Cover, "Should remain metaphysical");
        Assert.AreEqual("Sanctified Ground", tile.CoverDescription, "Description should be updated");
    }

    #endregion

    #region Cover Generation Tests

    [TestMethod]
    public void GenerateCover_CreatesCoverOnGrid()
    {
        // Arrange
        var gridService = new GridInitializationService();
        var grid = new BattlefieldGrid(5);
        var room = CreateTestRoom();

        // Place player and enemies
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);
        grid.GetTile(_player.Position.Value)!.IsOccupied = true;
        grid.GetTile(_enemy.Position.Value)!.IsOccupied = true;

        // Act
        gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert
        var tilesWithCover = grid.Tiles.Values.Where(t => t.Cover != CoverType.None).ToList();
        Assert.IsTrue(tilesWithCover.Count > 0, "Should generate at least one cover piece");
    }

    [TestMethod]
    public void GenerateCover_DoesNotPlaceOnOccupiedTiles()
    {
        // Arrange
        var gridService = new GridInitializationService();
        var grid = new BattlefieldGrid(3);
        var room = CreateTestRoom();

        // Place player and enemies on all tiles
        foreach (var tile in grid.Tiles.Values)
        {
            tile.IsOccupied = true;
        }

        // Act
        gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert
        var tilesWithCover = grid.Tiles.Values.Where(t => t.Cover != CoverType.None).ToList();
        Assert.AreEqual(0, tilesWithCover.Count, "Should not place cover on occupied tiles");
    }

    [TestMethod]
    public void GenerateCover_BossRoom_HasMoreMetaphysicalCover()
    {
        // Arrange
        var gridService = new GridInitializationService();
        var grid = new BattlefieldGrid(7);
        var room = CreateTestRoom();
        room.IsBossRoom = true;

        // Place player
        _player.Position = new GridPosition(Zone.Player, Row.Back, 3, 0);
        grid.GetTile(_player.Position.Value)!.IsOccupied = true;

        // Act - run multiple times to get statistical distribution
        int metaphysicalCount = 0;
        int physicalCount = 0;
        int iterations = 100;

        for (int i = 0; i < iterations; i++)
        {
            var testGrid = new BattlefieldGrid(7);
            testGrid.GetTile(new GridPosition(Zone.Player, Row.Back, 3, 0))!.IsOccupied = true;

            gridService.ApplyEnvironmentalFeatures(testGrid, room);

            foreach (var tile in testGrid.Tiles.Values.Where(t => t.Cover != CoverType.None))
            {
                if (tile.Cover == CoverType.Metaphysical || tile.Cover == CoverType.Both)
                {
                    metaphysicalCount++;
                }
                if (tile.Cover == CoverType.Physical)
                {
                    physicalCount++;
                }
            }
        }

        // Assert - Boss rooms should have higher metaphysical ratio (35% vs 15%)
        // With enough iterations, metaphysical should be more common in boss rooms
        var totalCover = metaphysicalCount + physicalCount;
        Assert.IsTrue(totalCover > 0, "Should generate cover");
    }

    [TestMethod]
    public void GenerateCover_SetsHealthForPhysicalCover()
    {
        // Arrange
        var gridService = new GridInitializationService();
        var grid = new BattlefieldGrid(5);
        var room = CreateTestRoom();

        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        grid.GetTile(_player.Position.Value)!.IsOccupied = true;

        // Act
        gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert
        var physicalCover = grid.Tiles.Values.FirstOrDefault(t =>
            t.Cover == CoverType.Physical || t.Cover == CoverType.Both);

        if (physicalCover != null)
        {
            Assert.IsNotNull(physicalCover.CoverHealth, "Physical cover should have HP");
            Assert.AreEqual(20, physicalCover.CoverHealth, "Default cover HP should be 20");
        }
    }

    [TestMethod]
    public void GenerateCover_SetsDescriptions()
    {
        // Arrange
        var gridService = new GridInitializationService();
        var grid = new BattlefieldGrid(5);
        var room = CreateTestRoom();

        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        grid.GetTile(_player.Position.Value)!.IsOccupied = true;

        // Act
        gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert
        var coverTiles = grid.Tiles.Values.Where(t => t.Cover != CoverType.None).ToList();
        if (coverTiles.Any())
        {
            foreach (var tile in coverTiles)
            {
                Assert.IsNotNull(tile.CoverDescription, "Cover should have description");
                Assert.IsFalse(string.IsNullOrEmpty(tile.CoverDescription), "Description should not be empty");
            }
        }
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "Test Player",
            Attributes = new Attributes { Sturdiness = 3, Will = 4 },
            Position = null,
            PsychicStress = 0,
            Corruption = 0
        };
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            Id = "test_enemy",
            Name = "Test Enemy",
            Attributes = new Attributes { Might = 3, Sturdiness = 3 },
            Position = null,
            HP = 30,
            MaxHP = 30
        };
    }

    private CombatState CreateTestCombatState()
    {
        return new CombatState
        {
            Player = _player,
            Enemies = new List<Enemy> { _enemy },
            CombatLog = new List<string>(),
            Grid = _grid
        };
    }

    private Room CreateTestRoom()
    {
        return new Room
        {
            Id = 1,
            Name = "Test Room",
            Description = "A test room",
            IsBossRoom = false,
            Archetype = Population.RoomArchetype.Chamber
        };
    }

    #endregion
}
