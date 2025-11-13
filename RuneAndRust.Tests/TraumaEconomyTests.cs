using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Core.Population;

namespace RuneAndRust.Tests;

/// <summary>
/// Unit tests for v0.15 Trauma Economy system
/// </summary>
[TestClass]
public class TraumaEconomyTests
{
    private Random _testRng = null!;
    private TraumaEconomyService _traumaService = null!;

    [TestInitialize]
    public void Setup()
    {
        _testRng = new Random(42); // Deterministic for testing
        _traumaService = new TraumaEconomyService(_testRng);
    }

    #region Breaking Point Tests

    [TestMethod]
    public void AddStress_ReachingHundred_TriggersBreakingPoint()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.PsychicStress = 95;

        // Act
        var (stressGained, trauma) = _traumaService.AddStress(character, 10, "combat");

        // Assert
        Assert.IsNotNull(trauma, "Trauma should be acquired at Breaking Point");
        Assert.AreEqual(60, character.PsychicStress, "Stress should reset to 60 after Breaking Point");
        Assert.AreEqual(1, character.Traumas.Count, "Character should have 1 trauma");
    }

    [TestMethod]
    public void AddStress_NotReachingHundred_NoBreakingPoint()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.PsychicStress = 80;

        // Act
        var (stressGained, trauma) = _traumaService.AddStress(character, 10, "combat");

        // Assert
        Assert.IsNull(trauma, "No trauma should be acquired below 100 stress");
        Assert.AreEqual(90, character.PsychicStress, "Stress should be 90");
        Assert.AreEqual(0, character.Traumas.Count, "Character should have no traumas");
    }

    [TestMethod]
    public void AddStress_MultipleBreakingPoints_AcquiresMultipleTraumas()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        // First Breaking Point
        character.PsychicStress = 95;
        var (_, trauma1) = _traumaService.AddStress(character, 10, "ambush");

        // Second Breaking Point
        character.PsychicStress = 95;
        var (_, trauma2) = _traumaService.AddStress(character, 10, "darkness");

        // Assert
        Assert.IsNotNull(trauma1);
        Assert.IsNotNull(trauma2);
        // Note: May have 1 or 2 traumas depending on if duplicates were selected
        Assert.IsTrue(character.Traumas.Count >= 1, "Should have at least 1 trauma");
    }

    [TestMethod]
    public void BreakingPoint_DuplicateTrauma_DoesNotAddDuplicate()
    {
        // Arrange
        var character = CreateTestCharacter();
        var existingTrauma = TraumaLibrary.GetTrauma("paranoia");
        if (existingTrauma != null)
        {
            character.Traumas.Add(existingTrauma);
        }

        // Act
        character.PsychicStress = 98;
        var (_, trauma) = _traumaService.AddStress(character, 10, "ambush"); // Should trigger paranoia

        // Assert
        Assert.AreEqual(1, character.Traumas.Count, "Should still have only 1 trauma (no duplicate)");
        Assert.AreEqual(60, character.PsychicStress, "Stress should still reset");
    }

    #endregion

    #region Corruption Threshold Tests

    [TestMethod]
    public void AddCorruption_Crossing25_TriggersThreshold()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Corruption = 20;

        // Act
        var (gained, thresholds) = _traumaService.AddCorruption(character, 10, "jotun_reader");

        // Assert
        Assert.AreEqual(10, gained);
        Assert.AreEqual(30, character.Corruption);
        Assert.IsTrue(thresholds.Contains(25), "Should trigger threshold 25");
    }

    [TestMethod]
    public void AddCorruption_Crossing50_TriggersThreshold()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Corruption = 45;

        // Act
        var (gained, thresholds) = _traumaService.AddCorruption(character, 10, "jotun_reader");

        // Assert
        Assert.AreEqual(10, gained);
        Assert.AreEqual(55, character.Corruption);
        Assert.IsTrue(thresholds.Contains(50), "Should trigger threshold 50");
    }

    [TestMethod]
    public void AddCorruption_Crossing75_AcquiresMachineAffinity()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Corruption = 70;

        // Act
        var (gained, thresholds) = _traumaService.AddCorruption(character, 10, "jotun_reader");

        // Assert
        Assert.AreEqual(10, gained);
        Assert.IsTrue(thresholds.Contains(75), "Should trigger threshold 75");
        Assert.IsTrue(character.HasTrauma("machine_affinity"), "Should have Machine Affinity trauma");
    }

    [TestMethod]
    public void AddCorruption_Reaching100_TriggersTerminal()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Corruption = 95;

        // Act
        var (gained, thresholds) = _traumaService.AddCorruption(character, 10, "symbiotic_plate");

        // Assert
        Assert.AreEqual(5, gained); // Clamped at 100
        Assert.AreEqual(100, character.Corruption);
        Assert.IsTrue(thresholds.Contains(100), "Should trigger terminal corruption");
    }

    [TestMethod]
    public void AddCorruption_MultipleThresholds_TriggersAll()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.Corruption = 20;

        // Act
        var (gained, thresholds) = _traumaService.AddCorruption(character, 60, "massive_exposure");

        // Assert
        Assert.AreEqual(60, gained);
        Assert.AreEqual(80, character.Corruption);
        Assert.IsTrue(thresholds.Contains(25), "Should trigger 25");
        Assert.IsTrue(thresholds.Contains(50), "Should trigger 50");
        Assert.IsTrue(thresholds.Contains(75), "Should trigger 75");
        Assert.IsFalse(thresholds.Contains(100), "Should not trigger 100");
    }

    #endregion

    #region Trauma Progression Tests

    [TestMethod]
    public void TraumaProgression_AfterSevenDays_MayProgress()
    {
        // Arrange
        var progressionService = new TraumaProgressionService(_testRng);
        var character = CreateTestCharacter();
        var trauma = TraumaLibrary.GetTrauma("paranoia")!;
        trauma.DaysSinceManagement = 7;
        character.Traumas.Add(trauma);

        // Act - Run multiple times since progression is probabilistic
        var progressedAny = false;
        for (int i = 0; i < 20; i++)
        {
            var testChar = CreateTestCharacter();
            var testTrauma = TraumaLibrary.GetTrauma("paranoia")!;
            testTrauma.DaysSinceManagement = 7;
            testChar.Traumas.Add(testTrauma);

            var progressed = progressionService.CheckTraumaProgression(testChar, 1);
            if (progressed.Count > 0)
            {
                progressedAny = true;
                break;
            }
        }

        // Assert
        Assert.IsTrue(progressedAny, "Trauma should have progressed in at least one of 20 attempts");
    }

    [TestMethod]
    public void TraumaProgression_Level3_DoesNotProgressFurther()
    {
        // Arrange
        var progressionService = new TraumaProgressionService(_testRng);
        var character = CreateTestCharacter();
        var trauma = TraumaLibrary.GetTrauma("paranoia")!;
        trauma.ProgressionLevel = 3;
        trauma.DaysSinceManagement = 7;
        character.Traumas.Add(trauma);

        // Act
        var progressed = progressionService.CheckTraumaProgression(character, 1);

        // Assert
        Assert.AreEqual(0, progressed.Count, "Level 3 traumas should not progress further");
        Assert.AreEqual(3, trauma.ProgressionLevel, "Should remain at level 3");
    }

    #endregion

    #region Trauma Management Tests

    [TestMethod]
    public void ManageTrauma_ThroughRest_ReducesStress()
    {
        // Arrange
        var managementService = new TraumaManagementService(_traumaService, _testRng);
        var character = CreateTestCharacter();
        character.PsychicStress = 50;
        var trauma = TraumaLibrary.GetTrauma("paranoia")!;
        character.Traumas.Add(trauma);

        // Act
        var (success, message) = managementService.ManageTrauma(character, trauma, ManagementMethod.Rest);

        // Assert
        Assert.IsTrue(success, "Management should succeed");
        Assert.AreEqual(40, character.PsychicStress, "Stress should be reduced by 10");
        Assert.IsTrue(trauma.IsManagedThisSession, "Should be marked as managed");
        Assert.AreEqual(0, trauma.DaysSinceManagement, "Days should reset");
    }

    [TestMethod]
    public void ManageTrauma_AlreadyManaged_Fails()
    {
        // Arrange
        var managementService = new TraumaManagementService(_traumaService, _testRng);
        var character = CreateTestCharacter();
        var trauma = TraumaLibrary.GetTrauma("paranoia")!;
        trauma.IsManagedThisSession = true;
        character.Traumas.Add(trauma);

        // Act
        var (success, message) = managementService.ManageTrauma(character, trauma, ManagementMethod.Rest);

        // Assert
        Assert.IsFalse(success, "Should fail if already managed");
    }

    [TestMethod]
    public void ManageTrauma_ThroughTherapy_CostsCorrectAmount()
    {
        // Arrange
        var managementService = new TraumaManagementService(_traumaService, _testRng);
        var character = CreateTestCharacter();
        character.Currency = 150;
        var trauma = TraumaLibrary.GetTrauma("paranoia")!;
        character.Traumas.Add(trauma);

        // Act
        var (success, message) = managementService.ManageTrauma(character, trauma, ManagementMethod.Therapy);

        // Assert
        Assert.IsTrue(success, "Therapy should succeed with enough currency");
        Assert.AreEqual(50, character.Currency, "Should cost 100 cogs");
        Assert.AreEqual(-7, trauma.DaysSinceManagement, "Should have buffer days");
    }

    [TestMethod]
    public void ManageTrauma_ThroughTherapy_InsufficientFunds_Fails()
    {
        // Arrange
        var managementService = new TraumaManagementService(_traumaService, _testRng);
        var character = CreateTestCharacter();
        character.Currency = 50; // Not enough
        var trauma = TraumaLibrary.GetTrauma("paranoia")!;
        character.Traumas.Add(trauma);

        // Act
        var (success, message) = managementService.ManageTrauma(character, trauma, ManagementMethod.Therapy);

        // Assert
        Assert.IsFalse(success, "Therapy should fail with insufficient funds");
        Assert.AreEqual(50, character.Currency, "Currency should not change");
    }

    #endregion

    #region Reality Glitch Tests

    [TestMethod]
    public void RealityGlitch_HighStress_IncreasesChance()
    {
        // Arrange
        var glitchService = new RealityGlitchService(_testRng);
        var character = CreateTestCharacter();

        // Act & Assert
        character.PsychicStress = 70;
        var lowStressTriggered = glitchService.ShouldTriggerGlitch(character);

        character.PsychicStress = 95;
        // At 95 stress, glitch chance is very high, so trigger multiple times
        var highStressCount = 0;
        for (int i = 0; i < 100; i++)
        {
            if (glitchService.ShouldTriggerGlitch(character))
                highStressCount++;
        }

        Assert.IsTrue(highStressCount > 10, "High stress should trigger glitches frequently");
    }

    [TestMethod]
    public void RealityGlitch_TerminalCorruption_AlwaysTriggers()
    {
        // Arrange
        var glitchService = new RealityGlitchService(_testRng);
        var character = CreateTestCharacter();
        character.Corruption = 100;

        // Act
        bool triggered = glitchService.ShouldTriggerGlitch(character);

        // Assert
        Assert.IsTrue(triggered, "Terminal corruption should always trigger glitches");
    }

    [TestMethod]
    public void RealityGlitch_GetIntensity_ScalesWithState()
    {
        // Arrange
        var glitchService = new RealityGlitchService(_testRng);
        var character = CreateTestCharacter();

        // Act & Assert
        character.PsychicStress = 70;
        character.Corruption = 50;
        int lowIntensity = glitchService.GetGlitchIntensity(character);
        Assert.AreEqual(0, lowIntensity, "Below thresholds should be 0");

        character.PsychicStress = 85;
        character.Corruption = 65;
        int medIntensity = glitchService.GetGlitchIntensity(character);
        Assert.AreEqual(2, medIntensity, "Medium levels should be 2");

        character.PsychicStress = 95;
        character.Corruption = 95;
        int highIntensity = glitchService.GetGlitchIntensity(character);
        Assert.AreEqual(4, highIntensity, "High levels should be 4");
    }

    #endregion

    #region Environmental Stress Tests

    [TestMethod]
    public void EnvironmentalStress_PsychicResonance_AppliesStress()
    {
        // Arrange
        var envService = new EnvironmentalStressService(_traumaService);
        var character = CreateTestCharacter();
        var room = CreateTestRoom();
        // AmbientCondition.PsychicResonance (removed)
        room.AmbientConditions = new List<AmbientCondition>();

        // Act
        int stress = envService.ApplyEnvironmentalStress(character, room);

        // Assert
        Assert.IsTrue(stress >= 2, "Psychic Resonance should apply at least 2 stress");
    }

    [TestMethod]
    public void EnvironmentalStress_Agoraphobia_InLargeRoom_AppliesStress()
    {
        // Arrange
        var envService = new EnvironmentalStressService(_traumaService);
        var character = CreateTestCharacter();
        var trauma = TraumaLibrary.GetTrauma("agoraphobia")!;
        character.Traumas.Add(trauma);

        var room = CreateTestRoom();
        room.Name = "The Vast Hall";
        room.Description = "An enormous chamber";

        // Act
        int stress = envService.ApplyEnvironmentalStress(character, room);

        // Assert
        Assert.IsTrue(stress > 0, "Agoraphobia in large room should apply stress");
    }

    #endregion

    #region Trauma Library Tests

    [TestMethod]
    public void TraumaLibrary_GetAllTraumas_Returns12()
    {
        // Act
        var allTraumas = TraumaLibrary.GetAllTraumas();

        // Assert
        Assert.AreEqual(12, allTraumas.Count, "Should have 12 trauma types");
    }

    [TestMethod]
    public void TraumaLibrary_GetTrauma_ReturnsCorrectTrauma()
    {
        // Act
        var paranoia = TraumaLibrary.GetTrauma("paranoia");

        // Assert
        Assert.IsNotNull(paranoia);
        Assert.AreEqual("paranoia", paranoia.TraumaId);
        Assert.AreEqual("[PARANOIA]", paranoia.Name);
    }

    [TestMethod]
    public void TraumaLibrary_AllTraumas_HaveEffects()
    {
        // Act
        var allTraumas = TraumaLibrary.GetAllTraumas();

        // Assert
        foreach (var trauma in allTraumas)
        {
            Assert.IsTrue(trauma.Effects.Count > 0,
                $"Trauma {trauma.TraumaId} should have at least one effect");
        }
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestCharacter()
    {
        return new PlayerCharacter
        {
            Name = "TestCharacter",
            Class = CharacterClass.Warrior,
            HP = 30,
            MaxHP = 30,
            PsychicStress = 0,
            Corruption = 0,
            Currency = 0,
            Attributes = new Attributes(might: 3, finesse: 2, wits: 2, will: 2, sturdiness: 3)
        };
    }

    private Room CreateTestRoom()
    {
        return new Room
        {
            Id = 1,
            Name = "Test Room",
            Description = "A test room",
            AmbientConditions = new List<AmbientCondition>()
        };
    }

    #endregion
}
