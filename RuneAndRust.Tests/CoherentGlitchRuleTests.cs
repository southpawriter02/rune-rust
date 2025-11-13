using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine.CoherentGlitch;
using RuneAndRust.Engine.CoherentGlitch.Rules;

namespace RuneAndRust.Tests;

/// <summary>
/// Unit tests for Coherent Glitch rule system (v0.12)
/// Tests individual rules and rule engine behavior
/// </summary>
[TestClass]
public class CoherentGlitchRuleTests
{
    #region Mandatory Rules

    [TestMethod]
    public void UnstableCeilingRubbleRule_WhenUnstableCeilingPresent_ReturnsTrue()
    {
        // Arrange
        var rule = new UnstableCeilingRubbleRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            DynamicHazards = new List<DynamicHazard>
            {
                new UnstableCeilingHazard()
            }
        };
        var context = new PopulationContext();

        // Act
        var result = rule.ShouldApply(room, context);

        // Assert
        Assert.IsTrue(result, "Rule should apply when Unstable Ceiling hazard is present");
    }

    [TestMethod]
    public void UnstableCeilingRubbleRule_Apply_AddsRubblePile()
    {
        // Arrange
        var rule = new UnstableCeilingRubbleRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            DynamicHazards = new List<DynamicHazard>
            {
                new UnstableCeilingHazard()
            },
            StaticTerrain = new List<StaticTerrain>()
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(1, room.StaticTerrain.Count, "Should add exactly one rubble pile");
        Assert.IsInstanceOfType(room.StaticTerrain[0], typeof(RubblePile));

        var rubble = (RubblePile)room.StaticTerrain[0];
        Assert.IsTrue(rubble.IsFromCeilingCollapse, "Rubble should be marked as from ceiling collapse");
    }

    [TestMethod]
    public void UnstableCeilingRubbleRule_Apply_DoesNotAddDuplicateRubble()
    {
        // Arrange
        var rule = new UnstableCeilingRubbleRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            DynamicHazards = new List<DynamicHazard>
            {
                new UnstableCeilingHazard()
            },
            StaticTerrain = new List<StaticTerrain>
            {
                new RubblePile() // Already has rubble
            }
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(1, room.StaticTerrain.Count, "Should not add duplicate rubble");
    }

    #endregion

    #region Weighted Rules

    [TestMethod]
    public void FloodedElectricalDangerRule_Apply_IncreasesElectricalHazardWeight()
    {
        // Arrange
        var rule = new FloodedElectricalDangerRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            AmbientConditions = new List<AmbientCondition>
            {
                new FloodedCondition()
            }
        };
        var context = new PopulationContext();
        context.BiomeElements.AddElement("live_power_conduit", BiomeElementType.DynamicHazard, 0.20f);

        // Act
        rule.Apply(room, context);

        // Assert
        var element = context.BiomeElements.GetElement("live_power_conduit");
        Assert.IsNotNull(element, "Element should exist");
        Assert.AreEqual(0.50f, element.Weight, 0.001f, "Weight should be multiplied by 2.5 (0.20 * 2.5 = 0.50)");
    }

    [TestMethod]
    public void FloodedElectricalDangerRule_Apply_EnhancesExistingPowerConduits()
    {
        // Arrange
        var rule = new FloodedElectricalDangerRule();
        var conduit = new LivePowerConduitHazard
        {
            DamageMultiplier = 1.0,
            RangeMultiplier = 1.0
        };
        var room = new Room
        {
            RoomId = "test_room_1",
            AmbientConditions = new List<AmbientCondition>
            {
                new FloodedCondition()
            },
            DynamicHazards = new List<DynamicHazard> { conduit }
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(2.0, conduit.DamageMultiplier, "Damage multiplier should be 2.0");
        Assert.AreEqual(1.5, conduit.RangeMultiplier, "Range multiplier should be 1.5");
        Assert.IsTrue(conduit.IsFloodedEnhanced, "Should be marked as flooded enhanced");
        Assert.IsTrue(conduit.IsEnhancedByRule, "Should be marked as enhanced by rule");
    }

    #endregion

    #region Exclusion Rules

    [TestMethod]
    public void NoSteamInFloodedRule_Apply_ExcludesSteamVents()
    {
        // Arrange
        var rule = new NoSteamInFloodedRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            AmbientConditions = new List<AmbientCondition>
            {
                new FloodedCondition()
            }
        };
        var context = new PopulationContext();
        context.BiomeElements.AddElement("steam_vent", BiomeElementType.DynamicHazard, 0.18f);

        // Act
        rule.Apply(room, context);

        // Assert
        var element = context.BiomeElements.GetElement("steam_vent");
        // Element should be excluded from random selection
        var selected = context.BiomeElements.SelectRandom(BiomeElementType.DynamicHazard, new Random(42));
        Assert.IsNull(selected, "Steam vent should not be selectable in flooded room");
    }

    [TestMethod]
    public void NoSteamInFloodedRule_Apply_RemovesExistingSteamVents()
    {
        // Arrange
        var rule = new NoSteamInFloodedRule();
        var steamVent = new SteamVentHazard();
        var room = new Room
        {
            RoomId = "test_room_1",
            AmbientConditions = new List<AmbientCondition>
            {
                new FloodedCondition()
            },
            DynamicHazards = new List<DynamicHazard> { steamVent }
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(0, room.DynamicHazards.Count, "Steam vents should be removed from flooded rooms");
    }

    [TestMethod]
    public void EntryHallSafetyRule_Apply_ReducesSpawnBudget()
    {
        // Arrange
        var rule = new EntryHallSafetyRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Archetype = RoomArchetype.EntryHall,
            IsStartRoom = true
        };
        var context = new PopulationContext
        {
            SpawnBudget = 10,
            SpawnBudgetModifier = 0
        };

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(-3, context.SpawnBudgetModifier, "Spawn budget modifier should be -3");
    }

    [TestMethod]
    public void EntryHallSafetyRule_Apply_RemovesChampions()
    {
        // Arrange
        var rule = new EntryHallSafetyRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Archetype = RoomArchetype.EntryHall,
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { ThreatLevel = ThreatLevel.Low },
                new DormantProcess { ThreatLevel = ThreatLevel.High, IsChampion = true },
                new DormantProcess { ThreatLevel = ThreatLevel.Medium }
            }
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(2, /* room.DormantProcesses */ room.Enemies.Count, "Champions should be removed");
        Assert.IsFalse(/* room.DormantProcesses */ room.Enemies.Any(e => e.ThreatLevel >= ThreatLevel.High),
            "No high-threat enemies should remain");
    }

    #endregion

    #region Contextual Rules

    [TestMethod]
    public void GeothermalSteamRule_ShouldApply_WithGeothermalKeywords()
    {
        // Arrange
        var rule = new GeothermalSteamRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Geothermal Pumping Station",
            Description = "Ancient thermal control systems line the walls."
        };
        var context = new PopulationContext();

        // Act
        var result = rule.ShouldApply(room, context);

        // Assert
        Assert.IsTrue(result, "Rule should apply to geothermal rooms");
    }

    [TestMethod]
    public void GeothermalSteamRule_Apply_IncreasesSteamVentWeight()
    {
        // Arrange
        var rule = new GeothermalSteamRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Geothermal Pumping Station"
        };
        var context = new PopulationContext();
        context.BiomeElements.AddElement("steam_vent", BiomeElementType.DynamicHazard, 0.18f);

        // Act
        rule.Apply(room, context);

        // Assert
        var element = context.BiomeElements.GetElement("steam_vent");
        Assert.AreEqual(0.54f, element.Weight, 0.001f, "Weight should be tripled (0.18 * 3 = 0.54)");
    }

    [TestMethod]
    public void GeothermalSteamRule_Apply_AddsExtremeHeatCondition()
    {
        // Arrange
        var rule = new GeothermalSteamRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Thermal Exchange Chamber",
            AmbientConditions = new List<AmbientCondition>()
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.IsTrue(room.HasAmbientCondition("[Extreme Heat]"),
            "Room should have Extreme Heat condition");
    }

    #endregion

    #region Tactical Rules

    [TestMethod]
    public void TacticalCoverPlacementRule_ShouldApply_WhenEnemiesPresent()
    {
        // Arrange
        var rule = new TacticalCoverPlacementRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { SpawnPosition = new Vector2(5, 5) }
            }
        };
        var context = new PopulationContext();

        // Act
        var result = rule.ShouldApply(room, context);

        // Assert
        Assert.IsTrue(result, "Rule should apply when enemies are present");
    }

    [TestMethod]
    public void TacticalCoverPlacementRule_Apply_AddsCoverNearEnemies()
    {
        // Arrange
        var rule = new TacticalCoverPlacementRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { SpawnPosition = new Vector2(5, 5) },
                new DormantProcess { SpawnPosition = new Vector2(8, 8) }
            },
            StaticTerrain = new List<StaticTerrain>()
        };
        var context = new PopulationContext { Rng = new Random(42) };

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.IsTrue(room.StaticTerrain.Count > 0, "Cover should be added");
        Assert.IsTrue(room.StaticTerrain.All(t => t.ProvidesTouchCover),
            "All added terrain should provide cover");
    }

    #endregion

    #region Narrative Chain Rules

    [TestMethod]
    public void BrokenMaintenanceCycleRule_ShouldApply_WithMaintenanceRoomAndHaugbui()
    {
        // Arrange
        var rule = new BrokenMaintenanceCycleRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Maintenance Hub",
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { ProcessType = "haugbui_class" }
            }
        };
        var context = new PopulationContext();

        // Act
        var result = rule.ShouldApply(room, context);

        // Assert
        Assert.IsTrue(result, "Rule should apply to maintenance rooms with Haugbui");
    }

    [TestMethod]
    public void BrokenMaintenanceCycleRule_Apply_AddsOrganizedRubblePiles()
    {
        // Arrange
        var rule = new BrokenMaintenanceCycleRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Maintenance Hub",
            // DormantProcesses = new List<DormantProcess>
            {
                new DormantProcess { ProcessType = "haugbui_class" }
            },
            StaticTerrain = new List<StaticTerrain>(),
            LootNodes = new List<LootNode>()
        };
        var context = new PopulationContext { Rng = new Random(42) };

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.IsTrue(room.StaticTerrain.Count >= 3, "Should add at least 3 organized rubble piles");
        Assert.IsTrue(room.StaticTerrain.OfType<RubblePile>().All(r => r.IsOrganized),
            "All rubble should be organized");
        Assert.IsTrue(room.LootNodes.Count > 0, "Should add loot nodes");
    }

    [TestMethod]
    public void ChasmInfrastructureRule_Apply_AddsCollapsedGantry()
    {
        // Arrange
        var rule = new ChasmInfrastructureRule();
        var chasm = new ChasmHazard { Position = new Vector2(5, 5), Width = 4.0f };
        var room = new Room
        {
            RoomId = "test_room_1",
            DynamicHazards = new List<DynamicHazard> { chasm },
            StaticTerrain = new List<StaticTerrain>(),
            LootNodes = new List<LootNode>()
        };
        var context = new PopulationContext { Rng = new Random(42) };

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.IsTrue(room.StaticTerrain.Count >= 2,
            "Should add gantry and rubble");
        Assert.IsTrue(room.StaticTerrain.Any(t => t is MachineryWreckage),
            "Should add collapsed gantry");
        Assert.IsTrue(room.Description.Contains("chasm"), "Description should mention chasm");
    }

    #endregion

    #region Balance Rules

    [TestMethod]
    public void SecretRoomRewardRule_Apply_Sets5xLootMultiplier()
    {
        // Arrange
        var rule = new SecretRoomRewardRule();
        var room = new Room
        {
            RoomId = "test_room_1",
            GeneratedNodeType = NodeType.Secret,
            LootNodes = new List<LootNode>()
        };
        var context = new PopulationContext { LootMultiplier = 1.0 };

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(5.0, context.LootMultiplier, "Secret room loot multiplier should be 5.0");
        Assert.IsTrue(room.LootNodes.Any(l => l.Quality == LootQuality.Rare),
            "Should add guaranteed Rare loot");
    }

    [TestMethod]
    public void HiddenContainerDiscoveryRule_Apply_ReducesDiscoveryDC()
    {
        // Arrange
        var rule = new HiddenContainerDiscoveryRule();
        var container = new HiddenContainer { DiscoveryDC = 15 }; // Old DC
        var room = new Room
        {
            RoomId = "test_room_1",
            LootNodes = new List<LootNode> { container }
        };
        var context = new PopulationContext();

        // Act
        rule.Apply(room, context);

        // Assert
        Assert.AreEqual(12, container.DiscoveryDC, "DC should be reduced to 12");
    }

    #endregion

    #region Rule Engine

    [TestMethod]
    public void RuleEngine_Initialize_RegistersAllRules()
    {
        // Arrange & Act
        var engine = new CoherentGlitchRuleEngine();

        // Assert
        var rules = engine.GetRules();
        Assert.IsTrue(rules.Count >= 15, $"Should register at least 15 rules (actual: {rules.Count})");
    }

    [TestMethod]
    public void RuleEngine_ApplyRules_ExecutesRulesInPriorityOrder()
    {
        // Arrange
        var engine = new CoherentGlitchRuleEngine();
        var room = new Room
        {
            RoomId = "test_room_1",
            Archetype = RoomArchetype.Chamber,
            DynamicHazards = new List<DynamicHazard>
            {
                new UnstableCeilingHazard() // Should trigger mandatory rule
            },
            StaticTerrain = new List<StaticTerrain>()
        };
        var context = new PopulationContext();

        // Act
        engine.ApplyRules(room, context);

        // Assert
        Assert.IsTrue(context.TotalRulesFired > 0, "At least one rule should fire");
        Assert.AreEqual(1, room.CoherentGlitchRulesFired, "Room should track rules fired");

        // Mandatory rule should have fired (UnstableCeiling -> Rubble)
        Assert.IsTrue(room.StaticTerrain.Any(t => t is RubblePile),
            "Mandatory rule should have added rubble pile");
    }

    #endregion
}
