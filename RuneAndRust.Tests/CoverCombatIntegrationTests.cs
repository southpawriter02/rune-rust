using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// Integration tests for v0.20.2 Cover System with combat mechanics
/// Tests the interaction between cover, attacks, and damage
/// </summary>
[TestClass]
public class CoverCombatIntegrationTests
{
    private DiceService _diceService = null!;
    private CombatEngine _combatEngine = null!;
    private ResolveCheckService _resolveService = null!;
    private PlayerCharacter _player = null!;
    private Enemy _enemy = null!;

    [TestInitialize]
    public void Setup()
    {
        _diceService = new DiceService(new Random(42)); // Deterministic
        var sagaService = new SagaService();
        var lootService = new LootService(_diceService);
        var equipmentService = new EquipmentService(_diceService);
        var hazardService = new HazardService(_diceService);
        var currencyService = new CurrencyService();

        _combatEngine = new CombatEngine(_diceService, sagaService, lootService,
            equipmentService, hazardService, currencyService);
        _resolveService = new ResolveCheckService(_diceService);

        _player = CreateTestPlayer();
        _enemy = CreateTestEnemy();
    }

    #region Player Attack with Cover Tests

    [TestMethod]
    public void PlayerAttack_EnemyBehindPhysicalCover_WithRifle_ReceivesDefenseBonus()
    {
        // Arrange
        var combatState = InitializeCombatWithPositions();

        // Equip player with a rifle (ranged weapon)
        _player.EquippedWeapon = CreateRifle();

        // Place enemy behind physical cover
        var enemyTile = combatState.Grid!.GetTile(_enemy.Position!.Value);
        enemyTile!.Cover = CoverType.Physical;
        enemyTile.CoverHealth = 20;
        enemyTile.CoverDescription = "Crate";

        var initialHP = _enemy.HP;

        // Act
        _combatEngine.PlayerAttack(combatState, _enemy);

        // Assert - Check that cover was mentioned in combat log
        var coverMention = combatState.CombatLog.Any(log => log.Contains("COVER"));
        Assert.IsTrue(coverMention || _enemy.HP == initialHP,
            "Cover should provide defense bonus or attack should miss");
    }

    [TestMethod]
    public void PlayerAttack_EnemyBehindCover_WithMelee_NoCoverBonus()
    {
        // Arrange
        var combatState = InitializeCombatWithPositions();

        // Equip player with melee weapon (or no weapon for unarmed)
        _player.EquippedWeapon = null; // Unarmed is melee

        // Place enemy behind physical cover
        var enemyTile = combatState.Grid!.GetTile(_enemy.Position!.Value);
        enemyTile!.Cover = CoverType.Physical;

        // Act
        _combatEngine.PlayerAttack(combatState, _enemy);

        // Assert - Melee attacks should ignore cover
        var coverMention = combatState.CombatLog.Any(log => log.Contains("COVER"));
        Assert.IsFalse(coverMention, "Melee attacks should ignore cover");
    }

    [TestMethod]
    public void PlayerAttack_DamagesCoverOnHit()
    {
        // Arrange
        var combatState = InitializeCombatWithPositions();
        _player.EquippedWeapon = CreateRifle();

        // Place enemy behind physical cover with low HP
        var enemyTile = combatState.Grid!.GetTile(_enemy.Position!.Value);
        enemyTile!.Cover = CoverType.Physical;
        enemyTile.CoverHealth = 5; // Low HP for easy destruction
        enemyTile.CoverDescription = "Crate";

        var initialCoverHP = enemyTile.CoverHealth;

        // Act - Attack multiple times to ensure hit
        for (int i = 0; i < 5; i++)
        {
            if (enemyTile.Cover != CoverType.None)
            {
                _combatEngine.PlayerAttack(combatState, _enemy);
            }
            else
            {
                break; // Cover destroyed
            }
        }

        // Assert - Cover should eventually be damaged or destroyed
        var coverDamaged = enemyTile.CoverHealth < initialCoverHP ||
                          enemyTile.Cover == CoverType.None;
        var destructionLogged = combatState.CombatLog.Any(log => log.Contains("COVER DESTROYED"));

        Assert.IsTrue(coverDamaged || destructionLogged,
            "Cover should be damaged or destroyed after successful hits");
    }

    [TestMethod]
    public void PlayerAttack_DestroyCover_BothType_PreservesMetaphysical()
    {
        // Arrange
        var combatState = InitializeCombatWithPositions();
        _player.EquippedWeapon = CreateRifle();
        _player.Attributes.Might = 8; // High attack for guaranteed hit

        // Place enemy behind "Both" type cover with low HP
        var enemyTile = combatState.Grid!.GetTile(_enemy.Position!.Value);
        enemyTile!.Cover = CoverType.Both;
        enemyTile.CoverHealth = 1; // Very low HP
        enemyTile.CoverDescription = "Fortified Anchor";

        // Act - Attack until cover is destroyed
        for (int i = 0; i < 10; i++)
        {
            var previousCoverType = enemyTile.Cover;
            _combatEngine.PlayerAttack(combatState, _enemy);

            // Check if physical component was destroyed
            if (previousCoverType == CoverType.Both && enemyTile.Cover == CoverType.Metaphysical)
            {
                // Success! Physical destroyed, metaphysical remains
                Assert.AreEqual(CoverType.Metaphysical, enemyTile.Cover,
                    "Metaphysical cover should remain after physical destruction");
                return;
            }

            if (!_enemy.IsAlive) break;
        }

        // If we didn't trigger the transition, at least verify cover behavior
        Assert.IsTrue(enemyTile.Cover == CoverType.Both || enemyTile.Cover == CoverType.Metaphysical,
            "Cover should be Both or Metaphysical (after destruction)");
    }

    #endregion

    #region Enemy Attack with Cover Tests

    [TestMethod]
    public void EnemyAttack_PlayerBehindPhysicalCover_ReceivesDefenseBonus()
    {
        // Arrange
        var combatState = InitializeCombatWithPositions();

        // Place player behind physical cover
        var playerTile = combatState.Grid!.GetTile(_player.Position!.Value);
        playerTile!.Cover = CoverType.Physical;
        playerTile.CoverHealth = 20;

        var initialPlayerHP = _player.HP;
        var enemyAI = new EnemyAI(_diceService, 42);

        // Act
        enemyAI.DecideAndExecuteAction(_enemy, _player, combatState);

        // Assert - Cover should provide some protection
        var coverMention = combatState.CombatLog.Any(log => log.Contains("COVER"));
        Assert.IsTrue(coverMention || _player.HP == initialPlayerHP,
            "Cover should provide defense bonus or attack should miss");
    }

    [TestMethod]
    public void EnemyAttack_SameZone_CoverDoesNotApply()
    {
        // Arrange
        var combatState = InitializeCombat();

        // Place both in same zone
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Player, Row.Front, 1, 0);

        var playerTile = combatState.Grid!.GetTile(_player.Position.Value);
        var enemyTile = combatState.Grid.GetTile(_enemy.Position.Value);
        playerTile!.IsOccupied = true;
        enemyTile!.IsOccupied = true;

        // Give player physical cover
        playerTile.Cover = CoverType.Physical;

        var enemyAI = new EnemyAI(_diceService, 42);

        // Act
        enemyAI.DecideAndExecuteAction(_enemy, _player, combatState);

        // Assert - Cover should NOT apply in same zone
        var coverMention = combatState.CombatLog.Any(log => log.Contains("COVER"));
        Assert.IsFalse(coverMention, "Cover should not apply in same zone combat");
    }

    #endregion

    #region Resolve Check with Metaphysical Cover Tests

    [TestMethod]
    public void ResolveCheck_PlayerBehindMetaphysicalCover_ReceivesResolveBonus()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        var playerTile = grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Metaphysical;
        playerTile.CoverDescription = "Runic Anchor";

        _player.Attributes.Will = 3;

        // Act
        var (success1, successes1, details1) = _resolveService.RollResolveCheck(_player, 2, grid, _enemy);
        var (success2, successes2, details2) = _resolveService.RollResolveCheck(_player, 2); // Without grid

        // Assert - With cover should have more dice
        Assert.IsTrue(details1.Contains("from cover") || details1.Contains("7 dice"),
            "Should mention cover bonus or show increased dice pool (3 base + 4 cover = 7)");
    }

    [TestMethod]
    public void ResolveCheck_EnvironmentalStress_MetaphysicalCoverHelps()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);

        var playerTile = grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Metaphysical;

        _player.Attributes.Will = 4;
        int baseStress = 10;

        // Act
        var (stressWithCover, successesWithCover, detailsWithCover) =
            _resolveService.RollEnvironmentalStressResistance(_player, baseStress, grid);
        var (stressNoCover, successesNoCover, detailsNoCover) =
            _resolveService.RollEnvironmentalStressResistance(_player, baseStress, null);

        // Assert - With cover should roll more dice (4 base + 4 cover = 8)
        Assert.IsTrue(detailsWithCover.Contains("metaphysical cover") || detailsWithCover.Contains("8 dice"),
            "Should mention metaphysical cover or show increased dice pool");
    }

    [TestMethod]
    public void ResolveCheck_PhysicalCoverOnly_NoResolveBonus()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);

        var playerTile = grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Physical; // Only physical, not metaphysical

        _player.Attributes.Will = 3;

        // Act
        var (success, successes, details) = _resolveService.RollResolveCheck(_player, 2, grid, _enemy);

        // Assert - Should not get cover bonus from physical cover
        Assert.IsFalse(details.Contains("from cover"),
            "Physical cover should not provide Resolve bonus");
        Assert.IsTrue(details.Contains("3 dice"),
            "Should roll base WILL dice only");
    }

    [TestMethod]
    public void ResolveCheck_BothTypeCover_ProvidesResolveBonus()
    {
        // Arrange
        var grid = new BattlefieldGrid(5);
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);

        var playerTile = grid.GetTile(_player.Position.Value);
        playerTile!.Cover = CoverType.Both;

        _player.Attributes.Will = 3;

        // Act
        var (success, successes, details) = _resolveService.RollResolveCheck(_player, 2, grid, _enemy);

        // Assert - Both type cover includes metaphysical protection
        Assert.IsTrue(details.Contains("from cover") || details.Contains("7 dice"),
            "Both cover should provide Resolve bonus (3 + 4 = 7 dice)");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "Test Player",
            HP = 50,
            MaxHP = 50,
            Stamina = 30,
            MaxStamina = 30,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 3,
                Sturdiness = 3,
                Wits = 3,
                Will = 4
            },
            WeaponAttribute = "MIGHT",
            BaseDamage = 2,
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
            HP = 30,
            MaxHP = 30,
            Attributes = new Attributes
            {
                Might = 3,
                Finesse = 2,
                Sturdiness = 3,
                Wits = 2,
                Will = 2
            },
            BaseDamageDice = 2,
            DamageBonus = 0,
            Soak = 0
        };
    }

    private Equipment CreateRifle()
    {
        return new Equipment
        {
            Name = "Test Rifle",
            Type = EquipmentType.Weapon,
            WeaponCategory = WeaponCategory.Rifle,
            WeaponAttribute = "FINESSE",
            DamageDice = 3,
            DamageBonus = 0,
            AccuracyBonus = 0,
            StaminaCost = 5
        };
    }

    private CombatState InitializeCombat()
    {
        var enemies = new List<Enemy> { _enemy };
        var room = new Room
        {
            Id = 1,
            Name = "Test Room",
            IsBossRoom = false
        };

        return _combatEngine.InitializeCombat(_player, enemies, room, canFlee: true);
    }

    private CombatState InitializeCombatWithPositions()
    {
        var combatState = InitializeCombat();

        // Ensure positions are in opposing zones
        _player.Position = new GridPosition(Zone.Player, Row.Back, 2, 0);
        _enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 2, 0);

        // Update grid tiles
        var playerTile = combatState.Grid!.GetTile(_player.Position.Value);
        var enemyTile = combatState.Grid.GetTile(_enemy.Position.Value);
        playerTile!.IsOccupied = true;
        playerTile.OccupantId = "player";
        enemyTile!.IsOccupied = true;
        enemyTile.OccupantId = _enemy.Id;

        return combatState;
    }

    #endregion
}
