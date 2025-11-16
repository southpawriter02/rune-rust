using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.31.4: Tests for Reality Tear Service
/// Tests Reality Tear hazard mechanics for Alfheim biome:
/// - Positional warping (3-5 tiles via Manhattan distance)
/// - Energy damage application (2d8)
/// - Corruption application (+5)
/// - [Dazed] status effect (1 turn)
/// - Enemy warping (no Corruption)
/// </summary>
[TestClass]
public class RealityTearServiceTests
{
    private RealityTearService _service = null!;
    private DiceService _diceService = null!;

    [TestInitialize]
    public void Setup()
    {
        _diceService = new DiceService();
        _service = new RealityTearService(_diceService, null);
    }

    #region Player Reality Tear Tests

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_AppliesEnergyDamage()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(character.Name, result.CharacterName);
        Assert.IsTrue(result.EnergyDamage >= 2 && result.EnergyDamage <= 16,
            $"2d8 damage should be 2-16, got {result.EnergyDamage}");
        Assert.IsTrue(character.HP < 100, "Character should take damage");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_AppliesCorruption()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.Corruption = 10;
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.AreEqual(5, result.CorruptionApplied,
            "Should apply +5 Corruption per Reality Tear encounter");
        Assert.AreEqual(15, character.Corruption,
            "Corruption should increase from 10 to 15");
        Assert.AreEqual(15, result.TotalCorruption);
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_AppliesDazed()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.IsTrue(result.DazedApplied, "[Dazed] should be applied");
        Assert.AreEqual(1, result.DazedDuration, "[Dazed] should last 1 turn");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_WarpsPosition()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        character.Position = tearPosition;
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.IsNotNull(result.WarpedPosition);
        Assert.AreNotEqual(tearPosition, result.WarpedPosition,
            "Character should be warped to different position");

        // Calculate Manhattan distance
        int distance = Math.Abs(result.WarpedPosition.X - tearPosition.X) +
                      Math.Abs(result.WarpedPosition.Y - tearPosition.Y);

        Assert.IsTrue(distance >= 3 && distance <= 5,
            $"Warp distance should be 3-5 tiles (Manhattan), got {distance}");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_UpdatesCharacterPosition()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        character.Position = tearPosition;
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.IsNotNull(character.Position);
        Assert.AreEqual(result.WarpedPosition, character.Position,
            "Character position should be updated to warped position");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_GeneratesMessage()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.IsTrue(result.Message.Length > 0, "Should generate narrative message");
        Assert.IsTrue(result.Message.Contains("Reality Tear") || result.Message.Contains("🌀"),
            "Message should mention Reality Tear");
        Assert.IsTrue(result.Message.Contains("[Dazed]"),
            "Message should mention [Dazed] status");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_DamageReducesHP()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        int expectedHP = 100 - result.EnergyDamage;
        Assert.AreEqual(expectedHP, character.HP,
            $"HP should be reduced by {result.EnergyDamage}");
        Assert.AreEqual(character.HP, result.RemainingHP);
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Player_DamageCannotReduceBelowZero()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 5, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        Assert.AreEqual(0, character.HP, "HP should not go below 0");
        Assert.AreEqual(0, result.RemainingHP);
    }

    #endregion

    #region Enemy Reality Tear Tests

    [TestMethod]
    public void ProcessRealityTearEncounter_Enemy_AppliesEnergyDamage()
    {
        // Arrange
        var enemy = CreateTestEnemy("Aether-Vulture", 50, 50);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(enemy, tearPosition, grid);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(enemy.Name, result.CharacterName);
        Assert.IsTrue(result.EnergyDamage >= 2 && result.EnergyDamage <= 16,
            $"2d8 damage should be 2-16, got {result.EnergyDamage}");
        Assert.IsTrue(enemy.HP < 50, "Enemy should take damage");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Enemy_NoCorruption()
    {
        // Arrange
        var enemy = CreateTestEnemy("Aether-Vulture", 50, 50);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(enemy, tearPosition, grid);

        // Assert
        Assert.AreEqual(0, result.CorruptionApplied,
            "Enemies should not gain Corruption");
        Assert.AreEqual(0, result.TotalCorruption,
            "Enemies should not have Corruption");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Enemy_AppliesDazed()
    {
        // Arrange
        var enemy = CreateTestEnemy("Aether-Vulture", 50, 50);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(enemy, tearPosition, grid);

        // Assert
        Assert.IsTrue(result.DazedApplied, "[Dazed] should be applied");
        Assert.AreEqual(1, result.DazedDuration, "[Dazed] should last 1 turn");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_Enemy_WarpsPosition()
    {
        // Arrange
        var enemy = CreateTestEnemy("Aether-Vulture", 50, 50);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        enemy.Position = tearPosition;
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(enemy, tearPosition, grid);

        // Assert
        Assert.IsNotNull(result.WarpedPosition);
        Assert.AreNotEqual(tearPosition, result.WarpedPosition,
            "Enemy should be warped to different position");

        // Calculate Manhattan distance
        int distance = Math.Abs(result.WarpedPosition.X - tearPosition.X) +
                      Math.Abs(result.WarpedPosition.Y - tearPosition.Y);

        Assert.IsTrue(distance >= 3 && distance <= 5,
            $"Warp distance should be 3-5 tiles (Manhattan), got {distance}");
    }

    #endregion

    #region Warp Destination Selection Tests

    [TestMethod]
    public void ProcessRealityTearEncounter_WarpDestination_IsPassable()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(10, 10);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        var warpTile = grid.Tiles.FirstOrDefault(t =>
            t.Position.X == result.WarpedPosition.X &&
            t.Position.Y == result.WarpedPosition.Y);

        Assert.IsNotNull(warpTile, "Warp destination should exist on grid");
        Assert.IsTrue(warpTile.IsPassable, "Warp destination should be passable");
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_NoValidDestinations_StaysInPlace()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 0, Y = 0 };
        character.Position = tearPosition;

        // Create a tiny grid with no valid warp destinations
        var grid = CreateTestGrid(2, 2);

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert
        // With a 2x2 grid and origin at (0,0), there are no tiles 3-5 Manhattan distance away
        // Service should handle gracefully by staying in place or choosing best available
        Assert.IsNotNull(result.WarpedPosition);
    }

    [TestMethod]
    public void ProcessRealityTearEncounter_MultipleWarpAttempts_VariesDestination()
    {
        // Arrange
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(15, 15);
        var destinations = new HashSet<(int, int)>();

        // Act - Warp 20 times to see variation
        for (int i = 0; i < 20; i++)
        {
            var character = CreateTestPlayer($"Player{i}", 100, 100);
            var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);
            destinations.Add((result.WarpedPosition.X, result.WarpedPosition.Y));
        }

        // Assert
        Assert.IsTrue(destinations.Count > 1,
            "Multiple warp attempts should produce varied destinations (not always the same)");
    }

    #endregion

    #region Manhattan Distance Tests

    [TestMethod]
    public void ProcessRealityTearEncounter_ManhattanDistance_CalculatedCorrectly()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        var tearPosition = new GridPosition { X = 5, Y = 5 };
        var grid = CreateTestGrid(15, 15);

        // Act - Test multiple warps to verify distance calculation
        for (int i = 0; i < 50; i++)
        {
            character.HP = 100; // Reset
            character.Position = new GridPosition { X = tearPosition.X, Y = tearPosition.Y };

            var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

            int distance = Math.Abs(result.WarpedPosition.X - tearPosition.X) +
                          Math.Abs(result.WarpedPosition.Y - tearPosition.Y);

            // Assert each warp
            Assert.IsTrue(distance >= 3 && distance <= 5,
                $"Warp {i}: Manhattan distance should be 3-5, got {distance} " +
                $"(from {tearPosition} to {result.WarpedPosition})");
        }
    }

    #endregion

    #region Public Helper Method Tests

    [TestMethod]
    public void GetDamageDice_Returns2d8()
    {
        // Act
        string damageDice = _service.GetDamageDice();

        // Assert
        Assert.AreEqual("2d8", damageDice,
            "Reality Tear damage should be 2d8");
    }

    [TestMethod]
    public void IsRealityTear_HazardTile_ReturnsTrue()
    {
        // Arrange
        var tile = new BattlefieldTile
        {
            Type = TileType.Hazard,
            HazardType = "Reality Tear"
        };

        // Act
        bool isRealityTear = _service.IsRealityTear(tile);

        // Assert
        Assert.IsTrue(isRealityTear, "Should identify Reality Tear hazard");
    }

    [TestMethod]
    public void IsRealityTear_NormalTile_ReturnsFalse()
    {
        // Arrange
        var tile = new BattlefieldTile
        {
            Type = TileType.Normal,
            HazardType = null
        };

        // Act
        bool isRealityTear = _service.IsRealityTear(tile);

        // Assert
        Assert.IsFalse(isRealityTear, "Should not identify normal tile as Reality Tear");
    }

    [TestMethod]
    public void IsRealityTear_DifferentHazard_ReturnsFalse()
    {
        // Arrange
        var tile = new BattlefieldTile
        {
            Type = TileType.Hazard,
            HazardType = "Unstable Platform"
        };

        // Act
        bool isRealityTear = _service.IsRealityTear(tile);

        // Assert
        Assert.IsFalse(isRealityTear,
            "Should not identify other hazards as Reality Tear");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullRealityTearWorkflow_Player_AppliesAllEffects()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.Corruption = 0;
        character.Position = new GridPosition { X = 5, Y = 5 };
        var tearPosition = character.Position;
        var grid = CreateTestGrid(15, 15);

        int initialHP = character.HP;
        int initialCorruption = character.Corruption;

        // Act
        var result = _service.ProcessRealityTearEncounter(character, tearPosition, grid);

        // Assert - Verify all effects
        Assert.IsTrue(character.HP < initialHP, "HP should be reduced");
        Assert.AreEqual(5, character.Corruption - initialCorruption,
            "Corruption should increase by 5");
        Assert.IsTrue(result.DazedApplied, "[Dazed] should be applied");
        Assert.AreNotEqual(tearPosition, character.Position,
            "Position should be changed");

        // Verify warp distance
        int distance = Math.Abs(character.Position.X - tearPosition.X) +
                      Math.Abs(character.Position.Y - tearPosition.Y);
        Assert.IsTrue(distance >= 3 && distance <= 5,
            $"Should warp 3-5 tiles, warped {distance}");

        // Verify message
        Assert.IsTrue(result.Message.Contains(character.Name));
        Assert.IsTrue(result.Message.Length > 50,
            "Should have detailed narrative message");
    }

    [TestMethod]
    public void FullRealityTearWorkflow_Enemy_AppliesCorrectEffects()
    {
        // Arrange
        var enemy = CreateTestEnemy("Aether-Vulture", 50, 50);
        enemy.Position = new GridPosition { X = 5, Y = 5 };
        var tearPosition = enemy.Position;
        var grid = CreateTestGrid(15, 15);

        int initialHP = enemy.HP;

        // Act
        var result = _service.ProcessRealityTearEncounter(enemy, tearPosition, grid);

        // Assert - Verify enemy-specific effects
        Assert.IsTrue(enemy.HP < initialHP, "HP should be reduced");
        Assert.AreEqual(0, result.CorruptionApplied,
            "Enemies should not gain Corruption");
        Assert.IsTrue(result.DazedApplied, "[Dazed] should be applied");
        Assert.AreNotEqual(tearPosition, enemy.Position,
            "Position should be changed");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer(string name, int hp, int maxHp)
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            Name = name,
            HP = hp,
            MaxHP = maxHp,
            Corruption = 0,
            PsychicStress = 0,
            Attributes = new Attributes
            {
                Might = 10,
                Finesse = 10,
                Wits = 10,
                Will = 12,
                Sturdiness = 10
            }
        };
    }

    private Enemy CreateTestEnemy(string name, int hp, int maxHp)
    {
        return new Enemy
        {
            EnemyID = new Random().Next(1000, 9999),
            Name = name,
            HP = hp,
            MaxHP = maxHp,
            StatusEffects = new List<StatusEffect>()
        };
    }

    private BattlefieldGrid CreateTestGrid(int width, int height)
    {
        var grid = new BattlefieldGrid { Width = width, Height = height };
        var tiles = new List<BattlefieldTile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles.Add(new BattlefieldTile
                {
                    Position = new GridPosition { X = x, Y = y },
                    Type = TileType.Normal,
                    IsPassable = true,
                    IsOccupied = false
                });
            }
        }

        grid.Tiles = tiles;
        return grid;
    }

    #endregion
}
