using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.32.4: Tests for Jötun Corpse Terrain Service
/// Tests fallen giant terrain mechanics:
/// - Corpse layout generation (Hull Section, Limb Bridge, Interior Cavity)
/// - Elevation and verticality
/// - Psychic Stress application (+2 per turn)
/// - Terrain type identification
/// - Procedural corpse type selection
/// </summary>
[TestClass]
public class JotunCorpseTerrainServiceTests
{
    private JotunCorpseTerrainService _service = null!;
    private DiceService _diceService = null!;
    private TraumaEconomyService _traumaEconomyService = null!;

    [TestInitialize]
    public void Setup()
    {
        _diceService = new DiceService();
        _traumaEconomyService = new TraumaEconomyService();
        _service = new JotunCorpseTerrainService(_diceService, _traumaEconomyService);
    }

    #region Layout Generation Tests

    [TestMethod]
    public void GenerateCorpseLayout_HullSection_Creates4x6Platform()
    {
        // Arrange
        var grid = new GridState(20, 20);

        // Act
        _service.GenerateCorpseLayout(grid, forcedCorpseType: "Hull Section");

        // Assert
        var corpseTiles = grid.Tiles.Where(t => t.HasTerrain("Jotun Corpse Terrain")).ToList();

        Assert.IsTrue(corpseTiles.Count > 0, "Should create corpse terrain tiles");
        Assert.IsTrue(corpseTiles.Count <= 24, "Hull Section should be at most 4x6 = 24 tiles");

        // Verify elevation
        foreach (var tile in corpseTiles)
        {
            Assert.AreEqual(2, tile.GetMetadata<int>("Elevation"),
                "Hull Section should be at elevation +2");
            Assert.AreEqual("Hull", tile.GetMetadata<string>("CorpseFeature"),
                "Should be marked as Hull feature");
        }
    }

    [TestMethod]
    public void GenerateCorpseLayout_LimbBridge_CreatesElevatedBridge()
    {
        // Arrange
        var grid = new GridState(20, 20);

        // Act
        _service.GenerateCorpseLayout(grid, forcedCorpseType: "Limb Bridge");

        // Assert
        var corpseTiles = grid.Tiles.Where(t => t.HasTerrain("Jotun Corpse Terrain")).ToList();

        Assert.IsTrue(corpseTiles.Count > 0, "Should create corpse terrain tiles");

        // Verify elevation
        foreach (var tile in corpseTiles)
        {
            Assert.AreEqual(3, tile.GetMetadata<int>("Elevation"),
                "Limb Bridge should be at elevation +3");
            Assert.AreEqual("Limb", tile.GetMetadata<string>("CorpseFeature"),
                "Should be marked as Limb feature");
        }

        // Verify bridge shape (should span across room)
        var minX = corpseTiles.Min(t => t.Position.X);
        var maxX = corpseTiles.Max(t => t.Position.X);
        var bridgeWidth = maxX - minX;

        Assert.IsTrue(bridgeWidth > 5, "Bridge should span across room");
    }

    [TestMethod]
    public void GenerateCorpseLayout_InteriorCavity_CreatesEnclosedArea()
    {
        // Arrange
        var grid = new GridState(20, 20);

        // Act
        _service.GenerateCorpseLayout(grid, forcedCorpseType: "Interior Cavity");

        // Assert
        var corpseTiles = grid.Tiles.Where(t => t.HasTerrain("Jotun Corpse Terrain")).ToList();

        Assert.IsTrue(corpseTiles.Count > 0, "Should create corpse terrain tiles");
        Assert.IsTrue(corpseTiles.Count <= 12, "Interior Cavity should be at most 3x4 = 12 tiles");

        // Verify ground level
        foreach (var tile in corpseTiles)
        {
            Assert.AreEqual(0, tile.GetMetadata<int>("Elevation"),
                "Interior Cavity should be at ground level (elevation 0)");
            Assert.AreEqual("Interior", tile.GetMetadata<string>("CorpseFeature"),
                "Should be marked as Interior feature");
            Assert.IsTrue(tile.GetMetadata<bool>("IsEnclosed"),
                "Interior tiles should be marked as enclosed");
        }
    }

    [TestMethod]
    public void GenerateCorpseLayout_RandomType_CreatesValidTerrain()
    {
        // Arrange
        var grid = new GridState(20, 20);

        // Act (no forced type - random selection)
        _service.GenerateCorpseLayout(grid);

        // Assert
        var corpseTiles = grid.Tiles.Where(t => t.HasTerrain("Jotun Corpse Terrain")).ToList();

        Assert.IsTrue(corpseTiles.Count > 0, "Should create corpse terrain tiles");

        // Verify all tiles have required metadata
        foreach (var tile in corpseTiles)
        {
            Assert.IsTrue(tile.GetMetadata<bool>("AppliesProximityStress"),
                "All corpse tiles should apply proximity stress");
            Assert.AreEqual(2, tile.GetMetadata<int>("StressPerTurn"),
                "Should apply +2 Stress per turn");
            Assert.IsNotNull(tile.GetMetadata<string>("CorpseType"),
                "Should have CorpseType metadata");
        }
    }

    #endregion

    #region Stress Application Tests

    [TestMethod]
    public void ApplyCorpseProximityStress_CharacterOnCorpse_AppliesStress()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        var character = CreateTestPlayer("StressedChar", 100);
        var initialStress = character.PsychicStress;

        // Act
        _service.ApplyCorpseProximityStress(character, corpseTile);

        // Assert
        Assert.IsTrue(character.PsychicStress > initialStress,
            "Character on corpse terrain should gain Psychic Stress");
        Assert.AreEqual(initialStress + 2, character.PsychicStress,
            "Should apply +2 Psychic Stress");
    }

    [TestMethod]
    public void ApplyCorpseProximityStress_CharacterNotOnCorpse_NoStress()
    {
        // Arrange
        var normalTile = new GridTile { Position = new GridPosition(5, 5) };
        // NOT corpse terrain

        var character = CreateTestPlayer("SafeChar", 100);
        var initialStress = character.PsychicStress;

        // Act
        _service.ApplyCorpseProximityStress(character, normalTile);

        // Assert
        Assert.AreEqual(initialStress, character.PsychicStress,
            "Character NOT on corpse terrain should not gain Stress");
    }

    [TestMethod]
    public void ApplyCorpseProximityStress_MultipleTurns_StressAccumulates()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));
        var character = CreateTestPlayer("AccumulatingChar", 100);
        var initialStress = character.PsychicStress;

        // Act: Apply stress 5 times (simulating 5 turns)
        for (int i = 0; i < 5; i++)
        {
            _service.ApplyCorpseProximityStress(character, corpseTile);
        }

        // Assert
        Assert.AreEqual(initialStress + 10, character.PsychicStress,
            "Stress should accumulate over multiple turns (+2 × 5 = +10)");
    }

    #endregion

    #region Utility Tests

    [TestMethod]
    public void GetCorpseTerrainType_HullSection_ReturnsCorrectType()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        // Act
        var corpseType = _service.GetCorpseTerrainType(corpseTile);

        // Assert
        Assert.AreEqual("Hull Section", corpseType,
            "Should return correct corpse type");
    }

    [TestMethod]
    public void GetCorpseTerrainType_NonCorpseTile_ReturnsNull()
    {
        // Arrange
        var normalTile = new GridTile { Position = new GridPosition(5, 5) };

        // Act
        var corpseType = _service.GetCorpseTerrainType(normalTile);

        // Assert
        Assert.IsNull(corpseType, "Non-corpse tile should return null");
    }

    [TestMethod]
    public void GetCorpseTerrainElevation_HullSection_Returns2()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        // Act
        var elevation = _service.GetCorpseTerrainElevation(corpseTile);

        // Assert
        Assert.AreEqual(2, elevation, "Hull Section should be at elevation +2");
    }

    [TestMethod]
    public void GetCorpseTerrainElevation_LimbBridge_Returns3()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Limb Bridge");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        // Act
        var elevation = _service.GetCorpseTerrainElevation(corpseTile);

        // Assert
        Assert.AreEqual(3, elevation, "Limb Bridge should be at elevation +3");
    }

    [TestMethod]
    public void GetCorpseTerrainElevation_InteriorCavity_Returns0()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Interior Cavity");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        // Act
        var elevation = _service.GetCorpseTerrainElevation(corpseTile);

        // Assert
        Assert.AreEqual(0, elevation, "Interior Cavity should be at ground level (0)");
    }

    [TestMethod]
    public void IsInsideCorpse_InteriorCavity_ReturnsTrue()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Interior Cavity");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        // Act
        var isInside = _service.IsInsideCorpse(corpseTile);

        // Assert
        Assert.IsTrue(isInside, "Interior Cavity tiles should be marked as inside corpse");
    }

    [TestMethod]
    public void IsInsideCorpse_HullSection_ReturnsFalse()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        var corpseTile = grid.Tiles.First(t => t.HasTerrain("Jotun Corpse Terrain"));

        // Act
        var isInside = _service.IsInsideCorpse(corpseTile);

        // Assert
        Assert.IsFalse(isInside, "Hull Section tiles should NOT be inside corpse");
    }

    [TestMethod]
    public void GetCorpseTerrainCount_AfterGeneration_ReturnsCorrectCount()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        // Act
        var count = _service.GetCorpseTerrainCount(grid);

        // Assert
        Assert.IsTrue(count > 0, "Should have corpse terrain tiles");
        Assert.IsTrue(count <= 24, "Hull Section should create at most 24 tiles");
    }

    [TestMethod]
    public void GetCorpseTerrainCount_NoCorpse_Returns0()
    {
        // Arrange
        var grid = new GridState(20, 20);
        // No corpse terrain generated

        // Act
        var count = _service.GetCorpseTerrainCount(grid);

        // Assert
        Assert.AreEqual(0, count, "Grid without corpse terrain should return 0");
    }

    [TestMethod]
    public void GenerateTerrainReport_AfterGeneration_ReturnsCompleteReport()
    {
        // Arrange
        var grid = new GridState(20, 20);
        _service.GenerateCorpseLayout(grid, "Hull Section");

        // Act
        var report = _service.GenerateTerrainReport(grid);

        // Assert
        Assert.IsNotNull(report, "Should return terrain report");
        Assert.IsTrue(report.HasCorpseTerrain, "Should indicate corpse terrain presence");
        Assert.IsTrue(report.TotalCorpseTiles > 0, "Should count total tiles");
        Assert.IsTrue(report.HullSectionTiles > 0, "Should count Hull Section tiles");
        Assert.AreEqual(2.0, report.AverageElevation, "Hull Section average elevation should be 2");
    }

    [TestMethod]
    public void GenerateTerrainReport_NoCorpse_ReturnsEmptyReport()
    {
        // Arrange
        var grid = new GridState(20, 20);
        // No corpse terrain

        // Act
        var report = _service.GenerateTerrainReport(grid);

        // Assert
        Assert.IsNotNull(report, "Should return terrain report");
        Assert.IsFalse(report.HasCorpseTerrain, "Should indicate no corpse terrain");
        Assert.AreEqual(0, report.TotalCorpseTiles, "Should have 0 tiles");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void GenerateMultipleCorpseLayouts_AllTypes_CreateValidTerrain()
    {
        // Arrange & Act
        var grids = new List<GridState>();
        var types = new[] { "Hull Section", "Limb Bridge", "Interior Cavity" };

        foreach (var type in types)
        {
            var grid = new GridState(20, 20);
            _service.GenerateCorpseLayout(grid, type);
            grids.Add(grid);
        }

        // Assert
        foreach (var grid in grids)
        {
            var corpseTiles = grid.Tiles.Where(t => t.HasTerrain("Jotun Corpse Terrain")).ToList();
            Assert.IsTrue(corpseTiles.Count > 0, "Each type should create corpse terrain");

            foreach (var tile in corpseTiles)
            {
                Assert.IsTrue(tile.GetMetadata<bool>("AppliesProximityStress"),
                    "All tiles should apply Stress");
            }
        }
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer(string name, int hp)
    {
        return new PlayerCharacter
        {
            Name = name,
            HP = hp,
            MaxHP = hp,
            PsychicStress = 0,
            Position = new GridPosition(0, 0),
            Attributes = new CharacterAttributes
            {
                Might = 10,
                Finesse = 10,
                Sturdiness = 10,
                Wits = 10,
                Will = 10
            }
        };
    }

    #endregion
}
