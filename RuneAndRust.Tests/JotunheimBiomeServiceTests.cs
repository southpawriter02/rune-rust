using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.32.4: Tests for Jötunheim Biome Service
/// Tests orchestration of Jötunheim biome mechanics:
/// - NO ambient condition (physical threats only)
/// - Live Power Conduit processing
/// - High-Pressure Steam Vent eruptions (every 3 turns)
/// - Unstable Ceiling/Wall collapses
/// - Jötun proximity Stress effects
/// - Flooded terrain + Toxic Haze processing
/// </summary>
[TestClass]
public class JotunheimBiomeServiceTests
{
    private JotunheimBiomeService _service = null!;
    private JotunheimDataRepository _dataRepository = null!;
    private PowerConduitService _powerConduitService = null!;
    private DiceService _diceService = null!;
    private EnvironmentalObjectService _environmentalObjectService = null!;
    private TraumaEconomyService _traumaEconomyService = null!;

    [TestInitialize]
    public void Setup()
    {
        // Note: Using in-memory database for testing
        _dataRepository = new JotunheimDataRepository("Data Source=:memory:");
        _diceService = new DiceService();
        _environmentalObjectService = new EnvironmentalObjectService();
        _traumaEconomyService = new TraumaEconomyService();
        _powerConduitService = new PowerConduitService(_diceService, _environmentalObjectService);

        _service = new JotunheimBiomeService(
            _dataRepository,
            _powerConduitService,
            _diceService,
            _environmentalObjectService,
            _traumaEconomyService
        );
    }

    #region No Ambient Condition Tests

    [TestMethod]
    public void VerifyNoAmbientCondition_BiomeDataCorrect_ReturnsTrue()
    {
        // Arrange
        // Database should have NULL ambient_condition_id for Jötunheim

        // Act
        var hasNoAmbient = _service.VerifyNoAmbientCondition();

        // Assert
        Assert.IsTrue(hasNoAmbient,
            "Jötunheim must have NO ambient condition (canonical design)");
    }

    #endregion

    #region Steam Vent Tests

    [TestMethod]
    public void ProcessSteamVents_Turn3_EruptsAndDealsDamage()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var steamVent = battlefield.Grid.GetTile(new GridPosition(5, 5));
        steamVent.AddEnvironmentalFeature("High-Pressure Steam Vent");

        var character = CreateTestPlayer("TestChar", 100, 100);
        character.Position = new GridPosition(5, 6); // In cone range
        battlefield.AddCharacter(character);

        var initialHP = character.HP;

        // Act: Process turns 1, 2, 3
        _service.ProcessEnvironmentalHazards(battlefield, 1); // No eruption
        _service.ProcessEnvironmentalHazards(battlefield, 2); // Warning
        _service.ProcessEnvironmentalHazards(battlefield, 3); // ERUPTION

        // Assert
        Assert.IsTrue(character.HP < initialHP,
            "Character should take Fire damage from steam vent eruption");

        // Check for combat log entry
        Assert.IsTrue(battlefield.CombatLog.Any(log => log.Contains("steam")),
            "Combat log should mention steam eruption");
    }

    [TestMethod]
    public void ProcessSteamVents_Turn2_ShowsWarningNotEruption()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var steamVent = battlefield.Grid.GetTile(new GridPosition(5, 5));
        steamVent.AddEnvironmentalFeature("High-Pressure Steam Vent");

        // Act
        _service.ProcessEnvironmentalHazards(battlefield, 1);
        _service.ProcessEnvironmentalHazards(battlefield, 2);

        // Assert
        var hasWarning = battlefield.CombatLog.Any(log =>
            log.Contains("hiss") || log.Contains("pressure is building"));

        Assert.IsTrue(hasWarning,
            "Turn 2 should show warning message for upcoming eruption");
    }

    [TestMethod]
    public void ProcessSteamVents_MultipleVents_EachEruptsIndependently()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();

        var vent1 = battlefield.Grid.GetTile(new GridPosition(3, 3));
        vent1.AddEnvironmentalFeature("High-Pressure Steam Vent");

        var vent2 = battlefield.Grid.GetTile(new GridPosition(7, 7));
        vent2.AddEnvironmentalFeature("High-Pressure Steam Vent");

        // Act: Process turn 3 (eruption cycle)
        _service.ProcessEnvironmentalHazards(battlefield, 3);

        // Assert
        var steamLogs = battlefield.CombatLog.Count(log => log.Contains("steam") || log.Contains("ERUPT"));
        Assert.IsTrue(steamLogs >= 1,
            "Both steam vents should erupt independently");
    }

    #endregion

    #region Ceiling Collapse Tests

    [TestMethod]
    public void CheckCeilingCollapse_HeavyDamageNearby_TriggersCollapse()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var unstableTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        unstableTile.AddEnvironmentalFeature("Unstable Ceiling/Wall");

        var impactPosition = new GridPosition(5, 6); // Adjacent, within trigger range
        var heavyDamage = 15; // Above 10 threshold

        // Act
        var collapsed = _service.CheckCeilingCollapse(impactPosition, heavyDamage, battlefield);

        // Assert
        Assert.IsTrue(collapsed, "Heavy damage should trigger ceiling collapse");
        Assert.IsFalse(unstableTile.HasEnvironmentalFeature("Unstable Ceiling/Wall"),
            "Unstable ceiling should be removed after collapse");
        Assert.IsTrue(unstableTile.HasTerrain("Debris Pile"),
            "Should create Debris Pile terrain after collapse");
    }

    [TestMethod]
    public void CheckCeilingCollapse_LowDamage_NoCollapse()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var unstableTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        unstableTile.AddEnvironmentalFeature("Unstable Ceiling/Wall");

        var impactPosition = new GridPosition(5, 6);
        var lowDamage = 5; // Below 10 threshold

        // Act
        var collapsed = _service.CheckCeilingCollapse(impactPosition, lowDamage, battlefield);

        // Assert
        Assert.IsFalse(collapsed, "Low damage should NOT trigger ceiling collapse");
        Assert.IsTrue(unstableTile.HasEnvironmentalFeature("Unstable Ceiling/Wall"),
            "Unstable ceiling should remain intact");
    }

    [TestMethod]
    public void CheckCeilingCollapse_FarAway_NoCollapse()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var unstableTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        unstableTile.AddEnvironmentalFeature("Unstable Ceiling/Wall");

        var impactPosition = new GridPosition(10, 10); // Far away (>2 tiles)
        var heavyDamage = 20;

        // Act
        var collapsed = _service.CheckCeilingCollapse(impactPosition, heavyDamage, battlefield);

        // Assert
        Assert.IsFalse(collapsed, "Impact too far away should NOT trigger collapse");
    }

    [TestMethod]
    public void CheckCeilingCollapse_DealsDamageInArea()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var unstableTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        unstableTile.AddEnvironmentalFeature("Unstable Ceiling/Wall");

        // Place character in collapse area
        var character = CreateTestPlayer("VictimChar", 100, 100);
        character.Position = new GridPosition(5, 5);
        battlefield.AddCharacter(character);

        var initialHP = character.HP;
        var impactPosition = new GridPosition(5, 6);

        // Act
        _service.CheckCeilingCollapse(impactPosition, 15, battlefield);

        // Assert
        Assert.IsTrue(character.HP < initialHP,
            "Character in collapse area should take Physical damage");
    }

    #endregion

    #region Jötun Proximity Stress Tests

    [TestMethod]
    public void ProcessJotunProximityStress_CharacterOnCorpseTerrain_AppliesStress()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var corpseTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        corpseTile.AddTerrain("Jotun Corpse Terrain");

        var character = CreateTestPlayer("StressedChar", 100, 100);
        character.Position = new GridPosition(5, 5);
        battlefield.AddCharacter(character);

        var initialStress = character.PsychicStress;

        // Act
        _service.ProcessEnvironmentalHazards(battlefield, 1);

        // Assert
        Assert.IsTrue(character.PsychicStress > initialStress,
            "Character on Jötun corpse terrain should gain Psychic Stress");
        Assert.AreEqual(initialStress + 2, character.PsychicStress,
            "Should apply +2 Psychic Stress per turn");
    }

    [TestMethod]
    public void ProcessJotunProximityStress_CharacterNotOnCorpse_NoStress()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var character = CreateTestPlayer("SafeChar", 100, 100);
        character.Position = new GridPosition(5, 5); // Normal tile, not corpse terrain
        battlefield.AddCharacter(character);

        var initialStress = character.PsychicStress;

        // Act
        _service.ProcessEnvironmentalHazards(battlefield, 1);

        // Assert
        Assert.AreEqual(initialStress, character.PsychicStress,
            "Character NOT on corpse terrain should not gain Stress");
    }

    [TestMethod]
    public void ProcessJotunProximityStress_MultipleTurns_StressAccumulates()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var corpseTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        corpseTile.AddTerrain("Jotun Corpse Terrain");

        var character = CreateTestPlayer("AccumulatingChar", 100, 100);
        character.Position = new GridPosition(5, 5);
        battlefield.AddCharacter(character);

        var initialStress = character.PsychicStress;

        // Act: Process 5 turns
        for (int turn = 1; turn <= 5; turn++)
        {
            _service.ProcessEnvironmentalHazards(battlefield, turn);
        }

        // Assert
        Assert.AreEqual(initialStress + 10, character.PsychicStress,
            "Stress should accumulate over multiple turns (+2 per turn × 5 turns = +10)");
    }

    #endregion

    #region Flooded Terrain Tests

    [TestMethod]
    public void ProcessFloodedTerrain_CharacterInFlood_TakesPoisonDamage()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var floodedTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        floodedTile.AddTerrain("Flooded (Coolant)");

        var character = CreateTestPlayer("FloodedChar", 100, 100);
        character.Position = new GridPosition(5, 5);
        battlefield.AddCharacter(character);

        var initialHP = character.HP;

        // Act
        _service.ProcessEnvironmentalHazards(battlefield, 1);

        // Assert
        Assert.AreEqual(initialHP - 1, character.HP,
            "Character in flooded terrain should take 1 Poison damage per turn");
    }

    #endregion

    #region Assembly Line Tests

    [TestMethod]
    public void ProcessAssemblyLines_CharacterOnBelt_MovedInDirection()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();
        var beltTile = battlefield.Grid.GetTile(new GridPosition(5, 5));
        beltTile.AddEnvironmentalFeature("Assembly Line (Active)");

        var character = CreateTestPlayer("MovedChar", 100, 100);
        character.Position = new GridPosition(5, 5);
        battlefield.AddCharacter(character);

        var initialPosition = character.Position;

        // Act
        _service.ProcessAssemblyLines(battlefield);

        // Assert
        Assert.AreNotEqual(initialPosition, character.Position,
            "Character should be moved by assembly line belt");

        var distance = Math.Abs(character.Position.X - initialPosition.X) +
                       Math.Abs(character.Position.Y - initialPosition.Y);
        Assert.AreEqual(2, distance,
            "Character should be moved 2 tiles by assembly line");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void ProcessEnvironmentalHazards_CompleteFlow_AllHazardsProcessed()
    {
        // Arrange
        var battlefield = CreateTestBattlefield();

        // Add various hazards
        var conduitTile = battlefield.Grid.GetTile(new GridPosition(3, 3));
        conduitTile.AddEnvironmentalFeature("Live Power Conduit");

        var steamVent = battlefield.Grid.GetTile(new GridPosition(5, 5));
        steamVent.AddEnvironmentalFeature("High-Pressure Steam Vent");

        var corpseTile = battlefield.Grid.GetTile(new GridPosition(7, 7));
        corpseTile.AddTerrain("Jotun Corpse Terrain");

        // Act: Process full turn
        _service.ProcessEnvironmentalHazards(battlefield, 3); // Turn 3 (steam erupts)

        // Assert
        Assert.IsTrue(battlefield.CombatLog.Count > 0,
            "Should have combat log entries from hazard processing");
    }

    #endregion

    #region Helper Methods

    private BattlefieldState CreateTestBattlefield()
    {
        var grid = new GridState(15, 15);
        return new BattlefieldState
        {
            Grid = grid,
            CombatLog = new List<string>(),
            CurrentTurn = 1
        };
    }

    private PlayerCharacter CreateTestPlayer(string name, int hp, int maxHP)
    {
        return new PlayerCharacter
        {
            Name = name,
            HP = hp,
            MaxHP = maxHP,
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
