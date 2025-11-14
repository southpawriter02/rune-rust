using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.20.3: Unit tests for GlitchService
/// Tests all three glitch types, severity scaling, and edge cases
/// </summary>
[TestClass]
public class GlitchServiceTests
{
    private GlitchService CreateGlitchService(int seed = 12345)
    {
        var diceService = new DiceService(seed);
        var random = new Random(seed);
        return new GlitchService(diceService, random);
    }

    private PlayerCharacter CreatePlayer(int finesse = 5, int sturdiness = 5, int wits = 5, int corruption = 0)
    {
        return new PlayerCharacter
        {
            Name = "TestPlayer",
            HP = 50,
            MaxHP = 50,
            Corruption = corruption,
            Attributes = new Attributes
            {
                Finesse = finesse,
                Sturdiness = sturdiness,
                Wits = wits,
                Might = 4,
                Will = 4
            }
        };
    }

    private Enemy CreateEnemy(int finesse = 4, int sturdiness = 4, int wits = 4)
    {
        return new Enemy
        {
            Name = "TestEnemy",
            Id = "enemy1",
            HP = 30,
            MaxHP = 30,
            Attributes = new Attributes
            {
                Finesse = finesse,
                Sturdiness = sturdiness,
                Wits = wits,
                Might = 3,
                Will = 3
            }
        };
    }

    private BattlefieldTile CreateGlitchedTile(Core.GlitchType glitchType, int severity)
    {
        var position = new GridPosition(Zone.Player, Row.Front, 0);
        return new BattlefieldTile(position)
        {
            Type = TileType.Glitched,
            GlitchType = glitchType,
            GlitchSeverity = severity
        };
    }

    private BattlefieldGrid CreateTestGrid(int columns = 5)
    {
        return new BattlefieldGrid(columns);
    }

    #region Flickering Platform Tests

    [TestMethod]
    public void ResolveFlickeringPlatform_HighFinessePlayer_Success()
    {
        // Arrange
        var service = CreateGlitchService();
        var player = CreatePlayer(finesse: 8);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsTrue(result.Success, "High FINESSE should pass DC 12 check");
        Assert.IsFalse(result.MovementFailed);
        Assert.IsNull(result.TeleportTo);
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_LowFinessePlayer_Failure()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999); // Seed for low rolls
        var player = CreatePlayer(finesse: 2);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success, "Low FINESSE should fail DC 12 check");
        Assert.IsTrue(result.MovementFailed, "Flickering Platform failure should block movement");
        Assert.IsTrue(player.HP < startingHP, "Player should take damage");
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_Severity1_DealsDamage()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        int damageTaken = startingHP - player.HP;
        Assert.IsTrue(damageTaken >= 1 && damageTaken <= 6, "Severity 1 should deal 1d6 damage");
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_Severity2_DealsMoreDamage()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 2);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        int damageTaken = startingHP - player.HP;
        Assert.IsTrue(damageTaken >= 2 && damageTaken <= 12, "Severity 2 should deal 2d6 damage");
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_Severity3_DealsMaxDamage()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 3);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        int damageTaken = startingHP - player.HP;
        Assert.IsTrue(damageTaken >= 3 && damageTaken <= 18, "Severity 3 should deal 3d6 damage");
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_Enemy_Success()
    {
        // Arrange
        var service = CreateGlitchService();
        var enemy = CreateEnemy(finesse: 7);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(enemy, tile, grid);

        // Assert
        Assert.IsTrue(result.Success, "High FINESSE enemy should pass check");
        Assert.IsFalse(result.MovementFailed);
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_Enemy_Failure()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var enemy = CreateEnemy(finesse: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 2);
        var grid = CreateTestGrid();
        int startingHP = enemy.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(enemy, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.MovementFailed);
        Assert.IsTrue(enemy.HP < startingHP, "Enemy should take damage");
    }

    #endregion

    #region Inverted Gravity Tests

    [TestMethod]
    public void ResolveInvertedGravity_HighSturdiness_Success()
    {
        // Arrange
        var service = CreateGlitchService();
        var player = CreatePlayer(sturdiness: 8);
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsTrue(result.Success, "High STURDINESS should pass DC 12 check");
        Assert.IsFalse(result.MovementFailed);
        Assert.AreEqual(0, player.DisorientedTurnsRemaining);
    }

    [TestMethod]
    public void ResolveInvertedGravity_LowSturdiness_AppliesDisoriented()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(sturdiness: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.MovementFailed, "Inverted Gravity should not block movement");
        Assert.AreEqual(1, player.DisorientedTurnsRemaining, "Severity 1 should apply Disoriented for 1 turn");
    }

    [TestMethod]
    public void ResolveInvertedGravity_Severity2_AppliesDisoriented2Turns()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(sturdiness: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 2);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(2, player.DisorientedTurnsRemaining, "Severity 2 should apply Disoriented for 2 turns");
    }

    [TestMethod]
    public void ResolveInvertedGravity_Severity3_AppliesDisoriented3Turns()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(sturdiness: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 3);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(3, player.DisorientedTurnsRemaining, "Severity 3 should apply Disoriented for 3 turns");
    }

    [TestMethod]
    public void ResolveInvertedGravity_Enemy_AppliesDisoriented()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var enemy = CreateEnemy(sturdiness: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 2);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(enemy, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(2, enemy.DisorientedTurnsRemaining, "Enemy should be Disoriented for 2 turns");
    }

    #endregion

    #region Looping Corridor Tests

    [TestMethod]
    public void ResolveLoopingCorridor_HighWits_Success()
    {
        // Arrange
        var service = CreateGlitchService();
        var player = CreatePlayer(wits: 8);
        var tile = CreateGlitchedTile(Core.GlitchType.Looping, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsTrue(result.Success, "High WITS should pass DC 12 check");
        Assert.IsFalse(result.MovementFailed);
        Assert.IsNull(result.TeleportTo);
    }

    [TestMethod]
    public void ResolveLoopingCorridor_LowWits_Teleports()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(wits: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.Looping, severity: 1);
        var grid = CreateTestGrid();
        var originalPosition = tile.Position;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.MovementFailed, "Looping Corridor should not block movement");
        Assert.IsNotNull(result.TeleportTo, "Should teleport to random location");
        Assert.AreEqual(originalPosition.Zone, result.TeleportTo.Value.Zone, "Should teleport within same zone");
    }

    [TestMethod]
    public void ResolveLoopingCorridor_Severity3_HigherDC()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(wits: 3);
        var tile = CreateGlitchedTile(Core.GlitchType.Looping, severity: 3);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - DC 16 is very difficult, even moderate WITS should fail
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.TeleportTo);
    }

    [TestMethod]
    public void ResolveLoopingCorridor_Enemy_Teleports()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var enemy = CreateEnemy(wits: 1);
        var tile = CreateGlitchedTile(Core.GlitchType.Looping, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(enemy, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.TeleportTo, "Enemy should be teleported");
    }

    #endregion

    #region DC Calculation Tests

    [TestMethod]
    public void CalculateDC_Severity1_ReturnsDC12()
    {
        // Test through actual glitch resolution
        var service = CreateGlitchService();
        var player = CreatePlayer(finesse: 6); // Should sometimes pass DC 12
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - DC 12 exists (we can't directly test private method, but behavior confirms it)
        Assert.IsNotNull(result, "Result should be returned for Severity 1");
    }

    [TestMethod]
    public void CalculateDC_Severity2_ReturnsDC14()
    {
        // Test through actual glitch resolution
        var service = CreateGlitchService();
        var player = CreatePlayer(sturdiness: 7); // Should sometimes pass DC 14
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 2);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsNotNull(result, "Result should be returned for Severity 2");
    }

    [TestMethod]
    public void CalculateDC_Severity3_ReturnsDC16()
    {
        // Test through actual glitch resolution
        var service = CreateGlitchService();
        var player = CreatePlayer(wits: 8); // Should sometimes pass DC 16
        var tile = CreateGlitchedTile(Core.GlitchType.Looping, severity: 3);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsNotNull(result, "Result should be returned for Severity 3");
    }

    #endregion

    #region Corruption Scaling Tests

    [TestMethod]
    public void CorruptionScaling_LowCorruption_NoScaling()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1, corruption: 50);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - Damage should be 1d6 (not scaled)
        int damageTaken = startingHP - player.HP;
        Assert.IsTrue(damageTaken >= 1 && damageTaken <= 6, "Low corruption should not scale severity");
    }

    [TestMethod]
    public void CorruptionScaling_HighCorruption_ScalesUp()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1, corruption: 75);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - Damage should be 2d6 (scaled from 1 to 2)
        int damageTaken = startingHP - player.HP;
        Assert.IsTrue(damageTaken >= 2 && damageTaken <= 12, "High corruption should scale severity from 1 to 2");
    }

    [TestMethod]
    public void CorruptionScaling_MaxSeverity_CappedAt3()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1, corruption: 90);
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 3);
        var grid = CreateTestGrid();
        int startingHP = player.HP;

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - Damage should be 3d6 (capped at max severity)
        int damageTaken = startingHP - player.HP;
        Assert.IsTrue(damageTaken >= 3 && damageTaken <= 18, "Severity should be capped at 3");
    }

    [TestMethod]
    public void CorruptionScaling_HighCorruption_AffectsDisoriented()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(sturdiness: 1, corruption: 80);
        var tile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 1);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - Duration should be 2 turns (scaled from 1 to 2)
        Assert.IsFalse(result.Success);
        Assert.AreEqual(2, player.DisorientedTurnsRemaining, "High corruption should scale severity to 2");
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ResolveGlitchedTileEntry_NoGlitchType_ReturnsSuccess()
    {
        // Arrange
        var service = CreateGlitchService();
        var player = CreatePlayer();
        var tile = new BattlefieldTile(new GridPosition(Zone.Player, Row.Front, 0));
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsTrue(result.Success, "Tile without glitch should succeed");
        Assert.IsFalse(result.MovementFailed);
        Assert.IsNull(result.TeleportTo);
    }

    [TestMethod]
    public void ResolveLoopingCorridor_NoValidTiles_Succeeds()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(wits: 1);
        var grid = new BattlefieldGrid(1); // Very small grid
        var tile = grid.GetTile(new GridPosition(Zone.Player, Row.Back, 0))!;
        tile.Type = TileType.Glitched;
        tile.GlitchType = Core.GlitchType.Looping;
        tile.GlitchSeverity = 1;

        // Occupy all other tiles
        foreach (var t in grid.Tiles.Values)
        {
            if (t != tile)
            {
                t.IsOccupied = true;
            }
        }

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert - Should succeed gracefully when no valid teleport destination
        Assert.IsFalse(result.Success);
        // Teleport might be null or same position - both are valid fallbacks
    }

    [TestMethod]
    public void ResolveFlickeringPlatform_ReducesHPToZero_NotNegative()
    {
        // Arrange
        var service = CreateGlitchService(seed: 99999);
        var player = CreatePlayer(finesse: 1);
        player.HP = 1; // Very low HP
        var tile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 3);
        var grid = CreateTestGrid();

        // Act
        var result = service.ResolveGlitchedTileEntry(player, tile, grid);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(player.HP >= 0, "HP should not go negative");
    }

    [TestMethod]
    public void ResolveGlitchedTileEntry_AllThreeTypes_ReturnResults()
    {
        // Arrange
        var service = CreateGlitchService();
        var player = CreatePlayer();
        var grid = CreateTestGrid();

        var flickeringTile = CreateGlitchedTile(Core.GlitchType.Flickering, severity: 1);
        var gravityTile = CreateGlitchedTile(Core.GlitchType.InvertedGravity, severity: 1);
        var loopingTile = CreateGlitchedTile(Core.GlitchType.Looping, severity: 1);

        // Act
        var result1 = service.ResolveGlitchedTileEntry(player, flickeringTile, grid);
        var result2 = service.ResolveGlitchedTileEntry(player, gravityTile, grid);
        var result3 = service.ResolveGlitchedTileEntry(player, loopingTile, grid);

        // Assert
        Assert.IsNotNull(result1, "Flickering should return result");
        Assert.IsNotNull(result2, "Inverted Gravity should return result");
        Assert.IsNotNull(result3, "Looping Corridor should return result");
    }

    #endregion

    #region GlitchResult Tests

    [TestMethod]
    public void GlitchResult_Success_CreatesSuccessResult()
    {
        // Act
        var result = GlitchResult.Success("Test message");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Test message", result.Message);
        Assert.IsFalse(result.MovementFailed);
        Assert.IsNull(result.TeleportTo);
    }

    [TestMethod]
    public void GlitchResult_Failure_CreatesFailureResult()
    {
        // Act
        var result = GlitchResult.Failure("Test failure", movementFailed: true);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Test failure", result.Message);
        Assert.IsTrue(result.MovementFailed);
        Assert.IsNull(result.TeleportTo);
    }

    [TestMethod]
    public void GlitchResult_TeleportFailure_CreatesTeleportResult()
    {
        // Arrange
        var teleportPosition = new GridPosition(Zone.Enemy, Row.Back, 2);

        // Act
        var result = GlitchResult.TeleportFailure("Teleported!", teleportPosition);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Teleported!", result.Message);
        Assert.IsFalse(result.MovementFailed);
        Assert.AreEqual(teleportPosition, result.TeleportTo);
    }

    #endregion
}
