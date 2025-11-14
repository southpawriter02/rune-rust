using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.20.2: Integration tests for Cover System
/// Tests full combat scenarios with cover bonuses and destruction.
/// Target: 12+ integration tests
/// </summary>
[TestFixture]
public class CoverIntegrationTests
{
    private GridInitializationService _gridService = null!;
    private CoverService _coverService = null!;
    private DiceService _diceService = null!;

    [SetUp]
    public void Setup()
    {
        _gridService = new GridInitializationService();
        _coverService = new CoverService();
        _diceService = new DiceService();
    }

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "TestPlayer",
            HP = 50,
            MaxHP = 50,
            Attributes = new Attributes { Might = 3, Finesse = 3, Wits = 3, Will = 3, Sturdiness = 3 }
        };
    }

    private Enemy CreateTestEnemy(string name = "TestEnemy")
    {
        return new Enemy
        {
            Name = name,
            Id = Guid.NewGuid().ToString(),
            HP = 30,
            MaxHP = 30,
            Attributes = new Attributes { Might = 3, Finesse = 2, Will = 2, Sturdiness = 2 },
            BaseDamageDice = 2,
            Type = EnemyType.CorruptedServitor
        };
    }

    private Room CreateTestRoom()
    {
        return new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "A test combat arena"
        };
    }

    #endregion

    #region Grid Initialization with Cover

    [Test]
    public void GridInitialization_GeneratesCover_BasedOnColumnCount()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy> { CreateTestEnemy(), CreateTestEnemy(), CreateTestEnemy() };
        var room = CreateTestRoom();

        // Act
        var grid = _gridService.InitializeGrid(player, enemies);
        _gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert
        int coverCount = 0;
        foreach (var tile in grid.Tiles.Values)
        {
            if (tile.Cover != CoverType.None)
            {
                coverCount++;
            }
        }

        // Grid has 5 columns (3 enemies + 2), so should have ~2-3 cover pieces
        Assert.That(coverCount, Is.GreaterThan(0), "Grid should have at least 1 cover piece");
        Assert.That(coverCount, Is.LessThanOrEqualTo(grid.Columns), "Cover count should not exceed columns");
    }

    [Test]
    public void GridInitialization_CoverHasProperHealth_ForPhysicalTypes()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy> { CreateTestEnemy() };
        var room = CreateTestRoom();

        // Act
        var grid = _gridService.InitializeGrid(player, enemies);
        _gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert
        bool foundPhysicalCover = false;
        foreach (var tile in grid.Tiles.Values)
        {
            if (tile.Cover == CoverType.Physical || tile.Cover == CoverType.Both)
            {
                foundPhysicalCover = true;
                Assert.That(tile.CoverHealth, Is.EqualTo(20), "Physical cover should have 20 HP");
                Assert.That(tile.CoverDescription, Is.Not.Null, "Physical cover should have description");
            }
        }

        // Note: Due to randomness, we might not always get physical cover in small grids
        // This test passes if we find physical cover OR if we don't (metaphysical only)
        if (foundPhysicalCover)
        {
            Assert.Pass("Found physical cover with proper health");
        }
        else
        {
            Assert.Pass("No physical cover generated (metaphysical only)");
        }
    }

    [Test]
    public void GridInitialization_CoverDoesNotBlockCombatants()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy> { CreateTestEnemy(), CreateTestEnemy() };
        var room = CreateTestRoom();

        // Act
        var grid = _gridService.InitializeGrid(player, enemies);
        _gridService.ApplyEnvironmentalFeatures(grid, room);

        // Assert - No cover should be placed on occupied tiles
        var playerTile = grid.GetTile(player.Position!);
        Assert.That(playerTile?.Cover, Is.EqualTo(CoverType.None), "Player tile should not have cover");

        foreach (var enemy in enemies)
        {
            var enemyTile = grid.GetTile(enemy.Position!);
            Assert.That(enemyTile?.Cover, Is.EqualTo(CoverType.None), $"{enemy.Name} tile should not have cover");
        }
    }

    #endregion

    #region Physical Cover in Combat

    [Test]
    public void Combat_PhysicalCover_GrantsDefenseBonus()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var player = CreateTestPlayer();
        var enemy = CreateTestEnemy();

        player.Position = new GridPosition(Zone.Player, Row.Back, 2);
        enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2);

        var enemyTile = grid.GetTile(enemy.Position);
        _coverService.PlaceCover(enemyTile!, CoverType.Physical, "Test Barricade");

        // Act
        var coverBonus = _coverService.CalculateCoverBonus(enemy.Position, player.Position, AttackType.Ranged, grid);

        // Assert
        Assert.That(coverBonus.DefenseBonus, Is.EqualTo(4), "Cover should grant +4 Defense");
        Assert.That(coverBonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void Combat_CoverDestruction_ReducesHealthOverMultipleHits()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var position = new GridPosition(Zone.Enemy, Row.Front, 2);
        var tile = grid.GetTile(position)!;

        _coverService.PlaceCover(tile, CoverType.Physical, "Crate", 20);

        // Act - Simulate 3 hits of 12 damage each (cover takes 25% = 3 damage per hit)
        _coverService.DamageCover(tile, 12); // Cover: 20 -> 17
        Assert.That(tile.CoverHealth, Is.EqualTo(17));

        _coverService.DamageCover(tile, 12); // Cover: 17 -> 14
        Assert.That(tile.CoverHealth, Is.EqualTo(14));

        _coverService.DamageCover(tile, 12); // Cover: 14 -> 11
        Assert.That(tile.CoverHealth, Is.EqualTo(11));
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Physical), "Cover should still exist");
    }

    [Test]
    public void Combat_CoverDestruction_DestroysAtZeroHP()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var position = new GridPosition(Zone.Enemy, Row.Front, 2);
        var tile = grid.GetTile(position)!;

        _coverService.PlaceCover(tile, CoverType.Physical, "Barrel", 10);

        // Act - Deal 60 damage (cover takes 15)
        var message = _coverService.DamageCover(tile, 60);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.None), "Cover should be destroyed");
        Assert.That(tile.CoverHealth, Is.Null, "Cover health should be null");
        Assert.That(message, Does.Contain("COVER DESTROYED"));
        Assert.That(message, Does.Contain("Barrel"));
    }

    [Test]
    public void Combat_BothCover_PreservesMetaphysical_AfterPhysicalDestruction()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var position = new GridPosition(Zone.Player, Row.Back, 2);
        var tile = grid.GetTile(position)!;

        _coverService.PlaceCover(tile, CoverType.Both, "Sanctified Altar", 10);

        // Act - Destroy physical portion
        var message = _coverService.DamageCover(tile, 60);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Metaphysical), "Metaphysical cover should remain");
        Assert.That(tile.CoverDescription, Is.EqualTo("Runic Anchor"));
        Assert.That(message, Does.Contain("COVER DESTROYED"));
    }

    #endregion

    #region Metaphysical Cover in Combat

    [Test]
    public void Combat_MetaphysicalCover_GrantsResolveBonus()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var player = CreateTestPlayer();
        var enemy = CreateTestEnemy();

        player.Position = new GridPosition(Zone.Player, Row.Back, 2);
        enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2);

        var playerTile = grid.GetTile(player.Position);
        _coverService.PlaceCover(playerTile!, CoverType.Metaphysical, "Runic Anchor");

        // Act
        var coverBonus = _coverService.CalculateCoverBonus(player.Position, enemy.Position, AttackType.Psychic, grid);

        // Assert
        Assert.That(coverBonus.ResolveBonus, Is.EqualTo(4), "Metaphysical cover should grant +4 Resolve");
        Assert.That(coverBonus.DefenseBonus, Is.EqualTo(0));
    }

    [Test]
    public void Combat_MetaphysicalCover_CannotBeDamaged()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var position = new GridPosition(Zone.Player, Row.Back, 2);
        var tile = grid.GetTile(position)!;

        _coverService.PlaceCover(tile, CoverType.Metaphysical, "Runic Anchor");

        // Act - Try to damage it
        var message = _coverService.DamageCover(tile, 100);

        // Assert
        Assert.That(message, Is.Null, "Metaphysical cover cannot be damaged");
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Metaphysical), "Cover should remain");
        Assert.That(tile.CoverDescription, Is.EqualTo("Runic Anchor"));
    }

    #endregion

    #region Cover Applicability Rules

    [Test]
    public void Combat_SameZone_CoverDoesNotApply()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var targetPos = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPos = new GridPosition(Zone.Player, Row.Front, 3); // Same zone

        var tile = grid.GetTile(targetPos)!;
        _coverService.PlaceCover(tile, CoverType.Physical, "Wall");

        // Act
        var bonus = _coverService.CalculateCoverBonus(targetPos, attackerPos, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Cover should not apply in same zone");
    }

    [Test]
    public void Combat_MeleeAttack_CoverDoesNotApply()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var targetPos = new GridPosition(Zone.Enemy, Row.Front, 2);
        var attackerPos = new GridPosition(Zone.Player, Row.Front, 2); // Melee range

        var tile = grid.GetTile(targetPos)!;
        _coverService.PlaceCover(tile, CoverType.Physical, "Barrier");

        // Act
        var bonus = _coverService.CalculateCoverBonus(targetPos, attackerPos, AttackType.Melee, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Cover should not apply to melee attacks");
    }

    [Test]
    public void Combat_PhysicalCover_DoesNotBlockPsychic()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var targetPos = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPos = new GridPosition(Zone.Enemy, Row.Front, 2);

        var tile = grid.GetTile(targetPos)!;
        _coverService.PlaceCover(tile, CoverType.Physical, "Steel Barrier");

        // Act
        var bonus = _coverService.CalculateCoverBonus(targetPos, attackerPos, AttackType.Psychic, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0), "Physical cover should not block psychic attacks");
    }

    [Test]
    public void Combat_MetaphysicalCover_DoesNotBlockRanged()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        var targetPos = new GridPosition(Zone.Enemy, Row.Back, 2);
        var attackerPos = new GridPosition(Zone.Player, Row.Front, 2);

        var tile = grid.GetTile(targetPos)!;
        _coverService.PlaceCover(tile, CoverType.Metaphysical, "Runic Anchor");

        // Act
        var bonus = _coverService.CalculateCoverBonus(targetPos, attackerPos, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Metaphysical cover should not block ranged attacks");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    #endregion

    #region Cover Distribution

    [Test]
    public void CoverGeneration_ProducesMixedTypes()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemies = new List<Enemy>
        {
            CreateTestEnemy("Enemy1"),
            CreateTestEnemy("Enemy2"),
            CreateTestEnemy("Enemy3"),
            CreateTestEnemy("Enemy4"),
            CreateTestEnemy("Enemy5")
        };
        var room = CreateTestRoom();

        // Act - Generate multiple grids to test distribution
        int physicalCount = 0;
        int metaphysicalCount = 0;
        int bothCount = 0;

        for (int iteration = 0; iteration < 20; iteration++)
        {
            var grid = _gridService.InitializeGrid(player, enemies);
            _gridService.ApplyEnvironmentalFeatures(grid, room);

            foreach (var tile in grid.Tiles.Values)
            {
                if (tile.Cover == CoverType.Physical) physicalCount++;
                else if (tile.Cover == CoverType.Metaphysical) metaphysicalCount++;
                else if (tile.Cover == CoverType.Both) bothCount++;
            }
        }

        // Assert - Expect physical to be most common (~70%)
        Assert.That(physicalCount, Is.GreaterThan(0), "Should generate some physical cover");
        Assert.That(metaphysicalCount, Is.GreaterThan(0), "Should generate some metaphysical cover");
        // Both is rare (5%), might not appear in 20 iterations, so we don't assert it
        Assert.That(physicalCount, Is.GreaterThan(metaphysicalCount), "Physical should be more common than metaphysical");
    }

    #endregion
}
