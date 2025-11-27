using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.30.2: Tests for Frigid Cold Service
/// Tests [Frigid Cold] ambient condition for Niflheim biome
/// - Ice Vulnerability (+50% Ice damage)
/// - Critical Hit Slow (2 turns)
/// - Psychic Stress (+5 per combat)
/// </summary>
[TestClass]
public class FrigidColdServiceTests
{
    private FrigidColdService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new FrigidColdService();
    }

    #region Ice Vulnerability Tests

    [TestMethod]
    public void ApplyIceVulnerability_BaseDamage10_Returns15()
    {
        // Arrange
        int baseDamage = 10;

        // Act
        int amplifiedDamage = _service.ApplyIceVulnerability(baseDamage);

        // Assert
        Assert.AreEqual(15, amplifiedDamage, "Ice damage should be amplified by +50%");
    }

    [TestMethod]
    public void ApplyIceVulnerability_BaseDamage20_Returns30()
    {
        // Arrange
        int baseDamage = 20;

        // Act
        int amplifiedDamage = _service.ApplyIceVulnerability(baseDamage);

        // Assert
        Assert.AreEqual(30, amplifiedDamage, "Ice damage should be amplified by +50%");
    }

    [TestMethod]
    public void ApplyIceVulnerability_BaseDamage0_Returns0()
    {
        // Arrange
        int baseDamage = 0;

        // Act
        int amplifiedDamage = _service.ApplyIceVulnerability(baseDamage);

        // Assert
        Assert.AreEqual(0, amplifiedDamage, "Zero damage should remain zero");
    }

    [TestMethod]
    public void ApplyIceVulnerability_OddBaseDamage_RoundsCorrectly()
    {
        // Arrange
        int baseDamage = 7; // 7 * 1.5 = 10.5 → should round to 10

        // Act
        int amplifiedDamage = _service.ApplyIceVulnerability(baseDamage);

        // Assert
        Assert.AreEqual(10, amplifiedDamage, "Odd damage should round correctly");
    }

    #endregion

    #region Critical Hit Slow Tests

    [TestMethod]
    public void ProcessCriticalHitSlow_PlayerTarget_AppliesSlowedFor2Turns()
    {
        // Arrange
        var player = CreateTestPlayer("TestPlayer", 100, 100);
        string attackerName = "Frost-Rimed Undying";

        // Act
        var result = _service.ProcessCriticalHitSlow(player, attackerName);

        // Assert
        Assert.IsTrue(result.SlowedApplied, "Slowed should be applied on critical hit");
        Assert.AreEqual(2, result.SlowedDuration, "Slowed duration should be 2 turns");
        Assert.AreEqual(player.Name, result.TargetName);
        Assert.AreEqual(attackerName, result.AttackerName);
        Assert.IsTrue(result.Message.Contains("[Slowed]"), "Message should mention [Slowed]");
    }

    [TestMethod]
    public void ProcessCriticalHitSlow_EnemyTarget_AppliesSlowedFor2Turns()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);
        string attackerName = "Player Character";

        // Act
        var result = _service.ProcessCriticalHitSlow(enemy, attackerName);

        // Assert
        Assert.IsTrue(result.SlowedApplied, "Slowed should be applied on critical hit");
        Assert.AreEqual(2, result.SlowedDuration, "Slowed duration should be 2 turns");
        Assert.AreEqual(enemy.Name, result.TargetName);
        Assert.AreEqual(attackerName, result.AttackerName);
    }

    #endregion

    #region Psychic Stress Tests

    [TestMethod]
    public void ApplyEnvironmentalStress_SingleCharacter_Gains5Stress()
    {
        // Arrange
        var character = CreateTestPlayer("TestPlayer", 100, 100);
        character.PsychicStress = 10;
        var characters = new List<PlayerCharacter> { character };

        // Act
        var results = _service.ApplyEnvironmentalStress(characters);

        // Assert
        Assert.AreEqual(1, results.Count, "Should return one result");
        Assert.AreEqual(15, character.PsychicStress, "Character should gain +5 Psychic Stress");
        Assert.AreEqual(10, results[0].PreviousStress);
        Assert.AreEqual(5, results[0].StressGained);
        Assert.AreEqual(15, results[0].CurrentStress);
    }

    [TestMethod]
    public void ApplyEnvironmentalStress_MultipleCharacters_AllGain5Stress()
    {
        // Arrange
        var character1 = CreateTestPlayer("Character1", 100, 100);
        var character2 = CreateTestPlayer("Character2", 100, 100);
        character1.PsychicStress = 0;
        character2.PsychicStress = 20;
        var characters = new List<PlayerCharacter> { character1, character2 };

        // Act
        var results = _service.ApplyEnvironmentalStress(characters);

        // Assert
        Assert.AreEqual(2, results.Count, "Should return two results");
        Assert.AreEqual(5, character1.PsychicStress, "Character1 should gain +5 stress");
        Assert.AreEqual(25, character2.PsychicStress, "Character2 should gain +5 stress");
        Assert.AreEqual(5, results[0].StressGained);
        Assert.AreEqual(5, results[1].StressGained);
    }

    [TestMethod]
    public void ApplyEnvironmentalStress_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        var characters = new List<PlayerCharacter>();

        // Act
        var results = _service.ApplyEnvironmentalStress(characters);

        // Assert
        Assert.AreEqual(0, results.Count, "Should return empty results for empty input");
    }

    #endregion

    #region Status Message Tests

    [TestMethod]
    public void GetFrigidColdStatusMessage_ContainsKeyInformation()
    {
        // Act
        string message = _service.GetFrigidColdStatusMessage();

        // Assert
        Assert.IsTrue(message.Contains("[Frigid Cold]"), "Message should mention [Frigid Cold]");
        Assert.IsTrue(message.Contains("Ice"), "Message should mention Ice damage");
        Assert.IsTrue(message.Contains("50%"), "Message should mention 50% vulnerability");
        Assert.IsTrue(message.Contains("[Slowed]"), "Message should mention [Slowed]");
        Assert.IsTrue(message.Contains("Psychic Stress"), "Message should mention Psychic Stress");
    }

    [TestMethod]
    public void GetWarningMessage_ContainsWarningInformation()
    {
        // Act
        string message = _service.GetWarningMessage();

        // Assert
        Assert.IsTrue(message.Contains("⚠️"), "Message should contain warning emoji");
        Assert.IsTrue(message.Contains("[Frigid Cold]"), "Message should mention [Frigid Cold]");
        Assert.IsTrue(message.Contains("Ice Resistance"), "Message should recommend Ice Resistance");
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
            PsychicStress = 0,
            Attributes = new CharacterAttributes
            {
                Vigor = 10,
                Finesse = 10,
                Wits = 10,
                Resolve = 10,
                Sturdiness = 10
            }
        };
    }

    private Enemy CreateTestEnemy(string name, int hp, int maxHp)
    {
        return new Enemy
        {
            EnemyID = 1,
            Name = name,
            HP = hp,
            MaxHP = maxHp,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
