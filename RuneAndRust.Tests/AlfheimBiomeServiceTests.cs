using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.31.4: Tests for Alfheim Biome Service
/// Tests orchestration of Alfheim biome mechanics:
/// - [Runic Instability] ambient condition application
/// - [Psychic Resonance] high-intensity stress (ground zero)
/// - Wild Magic Surge triggering
/// - Reality Tear integration
/// - Enemy generation with weighted spawns
/// - Party preparedness checks
/// </summary>
[TestClass]
public class AlfheimBiomeServiceTests
{
    private AlfheimBiomeService _service = null!;
    private RunicInstabilityService _runicInstabilityService = null!;
    private RealityTearService _realityTearService = null!;
    private DiceService _diceService = null!;

    [TestInitialize]
    public void Setup()
    {
        _diceService = new DiceService();
        _runicInstabilityService = new RunicInstabilityService(_diceService);
        _realityTearService = new RealityTearService(_diceService, null);

        _service = new AlfheimBiomeService(
            _runicInstabilityService,
            _realityTearService,
            _diceService
        );
    }

    #region Combat Initialization Tests

    [TestMethod]
    public void InitializeCombat_AppliesPsychicResonance()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.PsychicStress = 0;
        var party = new List<PlayerCharacter> { character };

        // Act
        _service.InitializeCombat(party);

        // Assert
        Assert.AreEqual(10, character.PsychicStress,
            "Should apply +10 Psychic Stress from [Psychic Resonance] at combat start");
    }

    [TestMethod]
    public void InitializeCombat_MultipleCharacters_AllGainStress()
    {
        // Arrange
        var character1 = CreateTestPlayer("Character1", 100, 100);
        var character2 = CreateTestPlayer("Character2", 100, 100);
        character1.PsychicStress = 5;
        character2.PsychicStress = 20;
        var party = new List<PlayerCharacter> { character1, character2 };

        // Act
        _service.InitializeCombat(party);

        // Assert
        Assert.AreEqual(15, character1.PsychicStress, "Character1 should gain +10 stress");
        Assert.AreEqual(30, character2.PsychicStress, "Character2 should gain +10 stress");
    }

    #endregion

    #region Psychic Resonance Tests

    [TestMethod]
    public void ApplyPsychicResonanceEncounter_Gains10Stress()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.PsychicStress = 10;
        var party = new List<PlayerCharacter> { character };

        // Act
        var results = _service.ApplyPsychicResonanceEncounter(party);

        // Assert
        Assert.AreEqual(1, results.Count, "Should return one result");
        Assert.AreEqual(20, character.PsychicStress, "Character should gain +10 Psychic Stress");
        Assert.AreEqual(10, results[0].PreviousStress);
        Assert.AreEqual(10, results[0].StressGained);
        Assert.AreEqual(20, results[0].CurrentStress);
        Assert.IsTrue(results[0].Message.Contains("Ground zero"),
            "Message should reference ground zero of Great Silence");
    }

    [TestMethod]
    public void ProcessTurnStress_Gains2StressPerTurn()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.PsychicStress = 10;
        var party = new List<PlayerCharacter> { character };

        // Act
        var results = _service.ProcessTurnStress(party, 3);

        // Assert
        Assert.AreEqual(1, results.Count, "Should return one result");
        Assert.AreEqual(12, character.PsychicStress, "Character should gain +2 Psychic Stress per turn");
        Assert.AreEqual(10, results[0].PreviousStress);
        Assert.AreEqual(2, results[0].StressGained);
        Assert.AreEqual(12, results[0].CurrentStress);
    }

    [TestMethod]
    public void ProcessTurnStress_MultipleCharacters_AllGain2Stress()
    {
        // Arrange
        var character1 = CreateTestPlayer("Character1", 100, 100);
        var character2 = CreateTestPlayer("Character2", 100, 100);
        character1.PsychicStress = 0;
        character2.PsychicStress = 25;
        var party = new List<PlayerCharacter> { character1, character2 };

        // Act
        var results = _service.ProcessTurnStress(party, 1);

        // Assert
        Assert.AreEqual(2, results.Count, "Should return two results");
        Assert.AreEqual(2, character1.PsychicStress, "Character1 should gain +2 stress");
        Assert.AreEqual(27, character2.PsychicStress, "Character2 should gain +2 stress");
    }

    #endregion

    #region Wild Magic Surge Tests

    [TestMethod]
    public void CheckWildMagicSurge_NonMystic_ReturnsNull()
    {
        // Arrange
        var character = CreateTestPlayer("Warrior", 100, 100);
        // Character is not Mystic archetype

        // Act
        var surge = _service.CheckWildMagicSurge(character, "Shield Bash");

        // Assert
        Assert.IsNull(surge, "Wild Magic Surge should not trigger for non-Mystics");
    }

    [TestMethod]
    public void ApplySurgeToDamage_NullSurge_ReturnsBaseDamage()
    {
        // Arrange
        int baseDamage = 20;

        // Act
        int modifiedDamage = _service.ApplySurgeToDamage(baseDamage, null);

        // Assert
        Assert.AreEqual(20, modifiedDamage, "Should return base damage when surge is null");
    }

    [TestMethod]
    public void ApplySurgeToRange_NullSurge_ReturnsBaseRange()
    {
        // Arrange
        int baseRange = 5;

        // Act
        int modifiedRange = _service.ApplySurgeToRange(baseRange, null);

        // Assert
        Assert.AreEqual(5, modifiedRange, "Should return base range when surge is null");
    }

    [TestMethod]
    public void ApplySurgeToTargets_NullSurge_ReturnsBaseTargets()
    {
        // Arrange
        int baseTargets = 3;

        // Act
        int modifiedTargets = _service.ApplySurgeToTargets(baseTargets, null);

        // Assert
        Assert.AreEqual(3, modifiedTargets, "Should return base targets when surge is null");
    }

    [TestMethod]
    public void ApplySurgeToDuration_NullSurge_ReturnsBaseDuration()
    {
        // Arrange
        int baseDuration = 4;

        // Act
        int modifiedDuration = _service.ApplySurgeToDuration(baseDuration, null);

        // Assert
        Assert.AreEqual(4, modifiedDuration, "Should return base duration when surge is null");
    }

    #endregion

    #region Enemy Generation Tests

    [TestMethod]
    public void GenerateAlfheimEnemyGroup_Difficulty1_GeneratesCorrectCount()
    {
        // Arrange
        int difficulty = 1; // Easy: 2-3 enemies, no elite
        var random = new Random(42); // Fixed seed for deterministic testing

        // Act
        var enemies = _service.GenerateAlfheimEnemyGroup(difficulty, random);

        // Assert
        Assert.IsTrue(enemies.Count >= 2 && enemies.Count <= 3,
            $"Difficulty 1 should generate 2-3 enemies, got {enemies.Count}");
        Assert.IsFalse(enemies.Contains("Forlorn Echo"),
            "Difficulty 1 should not allow elite enemies");
    }

    [TestMethod]
    public void GenerateAlfheimEnemyGroup_Difficulty2_GeneratesCorrectCount()
    {
        // Arrange
        int difficulty = 2; // Normal: 3-4 enemies, max 1 elite
        var random = new Random(42);

        // Act
        var enemies = _service.GenerateAlfheimEnemyGroup(difficulty, random);

        // Assert
        Assert.IsTrue(enemies.Count >= 3 && enemies.Count <= 4,
            $"Difficulty 2 should generate 3-4 enemies, got {enemies.Count}");
        int eliteCount = enemies.Count(e => e == "Forlorn Echo");
        Assert.IsTrue(eliteCount <= 1,
            $"Difficulty 2 should allow max 1 elite, got {eliteCount}");
    }

    [TestMethod]
    public void GenerateAlfheimEnemyGroup_Difficulty3_GeneratesCorrectCount()
    {
        // Arrange
        int difficulty = 3; // Hard: 4-5 enemies, max 2 elite
        var random = new Random(42);

        // Act
        var enemies = _service.GenerateAlfheimEnemyGroup(difficulty, random);

        // Assert
        Assert.IsTrue(enemies.Count >= 4 && enemies.Count <= 5,
            $"Difficulty 3 should generate 4-5 enemies, got {enemies.Count}");
        int eliteCount = enemies.Count(e => e == "Forlorn Echo");
        Assert.IsTrue(eliteCount <= 2,
            $"Difficulty 3 should allow max 2 elite, got {eliteCount}");
    }

    [TestMethod]
    public void GenerateAlfheimEnemyGroup_Difficulty4_GeneratesCorrectCount()
    {
        // Arrange
        int difficulty = 4; // Deadly: 5-6 enemies, max 3 elite
        var random = new Random(42);

        // Act
        var enemies = _service.GenerateAlfheimEnemyGroup(difficulty, random);

        // Assert
        Assert.IsTrue(enemies.Count >= 5 && enemies.Count <= 6,
            $"Difficulty 4 should generate 5-6 enemies, got {enemies.Count}");
        int eliteCount = enemies.Count(e => e == "Forlorn Echo");
        Assert.IsTrue(eliteCount <= 3,
            $"Difficulty 4 should allow max 3 elite, got {eliteCount}");
    }

    [TestMethod]
    public void GenerateAlfheimEnemyGroup_OnlyGeneratesValidEnemyTypes()
    {
        // Arrange
        int difficulty = 3;
        var random = new Random();
        var validTypes = new HashSet<string>
        {
            "Aether-Vulture",
            "Crystalline Construct",
            "Energy Elemental",
            "Forlorn Echo"
        };

        // Act
        var enemies = _service.GenerateAlfheimEnemyGroup(difficulty, random);

        // Assert
        foreach (var enemy in enemies)
        {
            Assert.IsTrue(validTypes.Contains(enemy),
                $"Generated invalid enemy type: {enemy}");
        }
    }

    #endregion

    #region Difficulty Calculation Tests

    [TestMethod]
    public void CalculateRecommendedDifficulty_LowLevelParty_Returns1()
    {
        // Arrange
        var character = CreateTestPlayer("LowLevel", 100, 100);
        character.Level = 7;
        character.Attributes.Will = 12;
        var party = new List<PlayerCharacter> { character };

        // Act
        int difficulty = _service.CalculateRecommendedDifficulty(party);

        // Assert
        Assert.AreEqual(1, difficulty, "Level 7 party should get difficulty 1 (Easy)");
    }

    [TestMethod]
    public void CalculateRecommendedDifficulty_MidLevelParty_Returns2()
    {
        // Arrange
        var character = CreateTestPlayer("MidLevel", 100, 100);
        character.Level = 8;
        character.Attributes.Will = 12;
        var party = new List<PlayerCharacter> { character };

        // Act
        int difficulty = _service.CalculateRecommendedDifficulty(party);

        // Assert
        Assert.AreEqual(2, difficulty, "Level 8 party should get difficulty 2 (Normal)");
    }

    [TestMethod]
    public void CalculateRecommendedDifficulty_HighLevelParty_Returns3()
    {
        // Arrange
        var character = CreateTestPlayer("HighLevel", 100, 100);
        character.Level = 10;
        character.Attributes.Will = 12;
        var party = new List<PlayerCharacter> { character };

        // Act
        int difficulty = _service.CalculateRecommendedDifficulty(party);

        // Assert
        Assert.AreEqual(3, difficulty, "Level 10 party should get difficulty 3 (Hard)");
    }

    [TestMethod]
    public void CalculateRecommendedDifficulty_VeryHighLevelParty_Returns4()
    {
        // Arrange
        var character = CreateTestPlayer("VeryHighLevel", 100, 100);
        character.Level = 12;
        character.Attributes.Will = 14;
        var party = new List<PlayerCharacter> { character };

        // Act
        int difficulty = _service.CalculateRecommendedDifficulty(party);

        // Assert
        Assert.AreEqual(4, difficulty, "Level 12 party should get difficulty 4 (Deadly)");
    }

    [TestMethod]
    public void CalculateRecommendedDifficulty_LowWill_ReducesDifficulty()
    {
        // Arrange
        var character = CreateTestPlayer("LowWill", 100, 100);
        character.Level = 12;
        character.Attributes.Will = 8; // Low WILL
        var party = new List<PlayerCharacter> { character };

        // Act
        int difficulty = _service.CalculateRecommendedDifficulty(party);

        // Assert
        Assert.IsTrue(difficulty < 4,
            "Low WILL should reduce difficulty recommendation");
    }

    #endregion

    #region Party Preparedness Tests

    [TestMethod]
    public void CheckPartyPreparedness_HighWill_ReturnsAdequate()
    {
        // Arrange
        var character = CreateTestPlayer("HighWill", 100, 100);
        character.Attributes.Will = 14;
        var party = new List<PlayerCharacter> { character };

        // Act
        var report = _service.CheckPartyPreparedness(party);

        // Assert
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.Characters.Count);
        Assert.AreEqual("Adequate", report.Characters[0].WarningLevel);
        Assert.IsTrue(report.PartyIsAdequatelyPrepared);
    }

    [TestMethod]
    public void CheckPartyPreparedness_LowWill_ReturnsWarning()
    {
        // Arrange
        var character = CreateTestPlayer("LowWill", 100, 100);
        character.Attributes.Will = 13;
        var party = new List<PlayerCharacter> { character };

        // Act
        var report = _service.CheckPartyPreparedness(party);

        // Assert
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.Characters.Count);
        Assert.AreEqual("Warning", report.Characters[0].WarningLevel);
        Assert.IsTrue(report.Characters[0].WarningMessage.Contains("moderate WILL"));
    }

    [TestMethod]
    public void CheckPartyPreparedness_CriticalWill_ReturnsCritical()
    {
        // Arrange
        var character = CreateTestPlayer("CriticalWill", 100, 100);
        character.Attributes.Will = 10;
        var party = new List<PlayerCharacter> { character };

        // Act
        var report = _service.CheckPartyPreparedness(party);

        // Assert
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.Characters.Count);
        Assert.AreEqual("Critical", report.Characters[0].WarningLevel);
        Assert.IsTrue(report.Characters[0].WarningMessage.Contains("CRITICAL"));
        Assert.IsFalse(report.PartyIsAdequatelyPrepared);
    }

    [TestMethod]
    public void CheckPartyPreparedness_AverageWill_CalculatedCorrectly()
    {
        // Arrange
        var character1 = CreateTestPlayer("Character1", 100, 100);
        var character2 = CreateTestPlayer("Character2", 100, 100);
        character1.Attributes.Will = 10;
        character2.Attributes.Will = 14;
        var party = new List<PlayerCharacter> { character1, character2 };

        // Act
        var report = _service.CheckPartyPreparedness(party);

        // Assert
        Assert.AreEqual(12.0, report.AverageWill);
        Assert.IsTrue(report.PartyIsAdequatelyPrepared,
            "Average WILL of 12 should be adequate");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullCombatWorkflow_InitializeAndProcessTurns()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.PsychicStress = 0;
        var party = new List<PlayerCharacter> { character };

        // Act - Initialize combat
        _service.InitializeCombat(party);
        int stressAfterInit = character.PsychicStress;

        // Act - Process 3 turns
        _service.ProcessTurnStress(party, 1);
        _service.ProcessTurnStress(party, 2);
        _service.ProcessTurnStress(party, 3);

        // Assert
        Assert.AreEqual(10, stressAfterInit,
            "Should gain +10 stress at combat start");
        Assert.AreEqual(16, character.PsychicStress,
            "Should gain +2 stress per turn (10 + 2 + 2 + 2 = 16)");
    }

    [TestMethod]
    public void FullCombatWorkflow_DifficultyAndGeneration()
    {
        // Arrange
        var character1 = CreateTestPlayer("Character1", 100, 100);
        var character2 = CreateTestPlayer("Character2", 100, 100);
        character1.Level = 10;
        character2.Level = 10;
        character1.Attributes.Will = 14;
        character2.Attributes.Will = 12;
        var party = new List<PlayerCharacter> { character1, character2 };

        // Act - Check preparedness
        var preparedness = _service.CheckPartyPreparedness(party);

        // Act - Calculate difficulty
        int difficulty = _service.CalculateRecommendedDifficulty(party);

        // Act - Generate enemies
        var enemies = _service.GenerateAlfheimEnemyGroup(difficulty);

        // Assert
        Assert.IsTrue(preparedness.PartyIsAdequatelyPrepared,
            "Party with WILL 14/12 should be prepared");
        Assert.AreEqual(3, difficulty,
            "Level 10 party should get difficulty 3");
        Assert.IsTrue(enemies.Count >= 4 && enemies.Count <= 5,
            "Difficulty 3 should generate 4-5 enemies");
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
            Level = 10,
            PsychicStress = 0,
            Corruption = 0,
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

    #endregion
}
