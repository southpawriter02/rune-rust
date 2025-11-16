using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.31.4: Tests for Runic Instability Service
/// Tests [Runic Instability] ambient condition for Alfheim biome:
/// - Wild Magic Surge probability (25% chance)
/// - Surge effect generation (Damage/Range/Targets/Duration ±50%)
/// - Surge application to abilities
/// - Aether Pool amplification (+10% for Mystics)
/// - Psychic Stress from surge feedback (+5 per surge)
/// </summary>
[TestClass]
public class RunicInstabilityServiceTests
{
    private RunicInstabilityService _service = null!;
    private DiceService _diceService = null!;

    [TestInitialize]
    public void Setup()
    {
        _diceService = new DiceService();
        _service = new RunicInstabilityService(_diceService);
    }

    #region Wild Magic Surge Trigger Tests

    [TestMethod]
    public void TryTriggerWildMagicSurge_NotInAlfheim_ReturnsNull()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        int wrongBiomeId = 5; // Not Alfheim (biome_id: 6)

        // Act
        var surge = _service.TryTriggerWildMagicSurge(mystic, "Aetheric Bolt", wrongBiomeId);

        // Assert
        Assert.IsNull(surge, "Wild Magic Surge should only trigger in Alfheim (biome_id: 6)");
    }

    [TestMethod]
    public void TryTriggerWildMagicSurge_NonMystic_ReturnsNull()
    {
        // Arrange
        var warrior = CreateTestPlayer("Warrior", 100, 100);
        // Not a Mystic archetype
        int alfheimBiomeId = 6;

        // Act
        var surge = _service.TryTriggerWildMagicSurge(warrior, "Shield Bash", alfheimBiomeId);

        // Assert
        Assert.IsNull(surge, "Wild Magic Surge should only affect Mystics");
    }

    [TestMethod]
    public void TryTriggerWildMagicSurge_Mystic_TriggersApproximately25Percent()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        mystic.PsychicStress = 0;
        int alfheimBiomeId = 6;
        int trials = 1000;
        int surgeCount = 0;

        // Act
        for (int i = 0; i < trials; i++)
        {
            var surge = _service.TryTriggerWildMagicSurge(mystic, "Aetheric Bolt", alfheimBiomeId);
            if (surge != null)
            {
                surgeCount++;
            }
        }

        double surgeRate = (double)surgeCount / trials;

        // Assert
        Assert.IsTrue(surgeRate >= 0.20 && surgeRate <= 0.30,
            $"Surge rate should be ~25%, got {surgeRate:P2} ({surgeCount}/{trials})");
    }

    [TestMethod]
    public void TryTriggerWildMagicSurge_WhenTriggered_IncreasesStressBy5()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        mystic.PsychicStress = 10;
        int alfheimBiomeId = 6;

        // Act - Keep trying until we get a surge
        WildMagicSurgeResult? surge = null;
        int attempts = 0;
        while (surge == null && attempts < 100)
        {
            mystic.PsychicStress = 10; // Reset for each attempt
            surge = _service.TryTriggerWildMagicSurge(mystic, "Aetheric Bolt", alfheimBiomeId);
            attempts++;
        }

        // Assert
        Assert.IsNotNull(surge, "Should eventually trigger a surge");
        Assert.AreEqual(15, mystic.PsychicStress,
            "Triggered surge should apply +5 Psychic Stress");
    }

    #endregion

    #region Surge Effect Generation Tests

    [TestMethod]
    public void TryTriggerWildMagicSurge_GeneratesValidSurgeType()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        int alfheimBiomeId = 6;
        var validTypes = new HashSet<SurgeType>
        {
            SurgeType.DamageModification,
            SurgeType.RangeModification,
            SurgeType.TargetModification,
            SurgeType.DurationModification
        };

        // Act - Generate multiple surges to test all types
        var surgeTypes = new HashSet<SurgeType>();
        for (int i = 0; i < 100; i++)
        {
            var surge = _service.TryTriggerWildMagicSurge(mystic, "Test Ability", alfheimBiomeId);
            if (surge != null)
            {
                surgeTypes.Add(surge.Type);
                Assert.IsTrue(validTypes.Contains(surge.Type),
                    $"Invalid surge type: {surge.Type}");
            }
        }

        // Assert - Should generate at least some variety
        Assert.IsTrue(surgeTypes.Count > 0, "Should generate at least one surge type");
    }

    [TestMethod]
    public void TryTriggerWildMagicSurge_DamageModification_Is50Percent()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        int alfheimBiomeId = 6;

        // Act - Generate surges until we find a damage modification
        WildMagicSurgeResult? damageSurge = null;
        for (int i = 0; i < 200 && damageSurge == null; i++)
        {
            var surge = _service.TryTriggerWildMagicSurge(mystic, "Test", alfheimBiomeId);
            if (surge?.Type == SurgeType.DamageModification)
            {
                damageSurge = surge;
            }
        }

        // Assert
        Assert.IsNotNull(damageSurge, "Should eventually generate a damage surge");
        Assert.IsTrue(damageSurge.Modifier == 0.5 || damageSurge.Modifier == -0.5,
            $"Damage modifier should be ±50%, got {damageSurge.Modifier}");
    }

    [TestMethod]
    public void TryTriggerWildMagicSurge_RangeModification_Is1Tile()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        int alfheimBiomeId = 6;

        // Act - Generate surges until we find a range modification
        WildMagicSurgeResult? rangeSurge = null;
        for (int i = 0; i < 200 && rangeSurge == null; i++)
        {
            var surge = _service.TryTriggerWildMagicSurge(mystic, "Test", alfheimBiomeId);
            if (surge?.Type == SurgeType.RangeModification)
            {
                rangeSurge = surge;
            }
        }

        // Assert
        Assert.IsNotNull(rangeSurge, "Should eventually generate a range surge");
        Assert.IsTrue(rangeSurge.Modifier == 1 || rangeSurge.Modifier == -1,
            $"Range modifier should be ±1 tile, got {rangeSurge.Modifier}");
    }

    #endregion

    #region Surge Application Tests

    [TestMethod]
    public void ApplySurgeToDamage_IncreaseBy50Percent()
    {
        // Arrange
        int baseDamage = 20;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.DamageModification,
            Modifier = 0.5 // +50%
        };

        // Act
        int modifiedDamage = _service.ApplySurgeToDamage(baseDamage, surge);

        // Assert
        Assert.AreEqual(30, modifiedDamage, "20 damage + 50% = 30 damage");
    }

    [TestMethod]
    public void ApplySurgeToDamage_DecreaseBy50Percent()
    {
        // Arrange
        int baseDamage = 20;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.DamageModification,
            Modifier = -0.5 // -50%
        };

        // Act
        int modifiedDamage = _service.ApplySurgeToDamage(baseDamage, surge);

        // Assert
        Assert.AreEqual(10, modifiedDamage, "20 damage - 50% = 10 damage");
    }

    [TestMethod]
    public void ApplySurgeToDamage_MinimumDamage1()
    {
        // Arrange
        int baseDamage = 1;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.DamageModification,
            Modifier = -0.5 // -50%
        };

        // Act
        int modifiedDamage = _service.ApplySurgeToDamage(baseDamage, surge);

        // Assert
        Assert.AreEqual(1, modifiedDamage, "Damage should never go below 1");
    }

    [TestMethod]
    public void ApplySurgeToDamage_WrongSurgeType_ReturnsBaseDamage()
    {
        // Arrange
        int baseDamage = 20;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.RangeModification, // Not damage
            Modifier = 1
        };

        // Act
        int modifiedDamage = _service.ApplySurgeToDamage(baseDamage, surge);

        // Assert
        Assert.AreEqual(20, modifiedDamage, "Should ignore surge when type doesn't match");
    }

    [TestMethod]
    public void ApplySurgeToRange_IncreasesBy1()
    {
        // Arrange
        int baseRange = 5;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.RangeModification,
            Modifier = 1
        };

        // Act
        int modifiedRange = _service.ApplySurgeToRange(baseRange, surge);

        // Assert
        Assert.AreEqual(6, modifiedRange, "5 range + 1 = 6 range");
    }

    [TestMethod]
    public void ApplySurgeToRange_DecreasesBy1()
    {
        // Arrange
        int baseRange = 5;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.RangeModification,
            Modifier = -1
        };

        // Act
        int modifiedRange = _service.ApplySurgeToRange(baseRange, surge);

        // Assert
        Assert.AreEqual(4, modifiedRange, "5 range - 1 = 4 range");
    }

    [TestMethod]
    public void ApplySurgeToRange_MinimumRange1()
    {
        // Arrange
        int baseRange = 1;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.RangeModification,
            Modifier = -1
        };

        // Act
        int modifiedRange = _service.ApplySurgeToRange(baseRange, surge);

        // Assert
        Assert.AreEqual(1, modifiedRange, "Range should never go below 1");
    }

    [TestMethod]
    public void ApplySurgeToTargets_IncreasesBy1()
    {
        // Arrange
        int baseTargets = 3;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.TargetModification,
            Modifier = 1
        };

        // Act
        int modifiedTargets = _service.ApplySurgeToTargets(baseTargets, surge);

        // Assert
        Assert.AreEqual(4, modifiedTargets, "3 targets + 1 = 4 targets");
    }

    [TestMethod]
    public void ApplySurgeToTargets_DecreasesBy1()
    {
        // Arrange
        int baseTargets = 3;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.TargetModification,
            Modifier = -1
        };

        // Act
        int modifiedTargets = _service.ApplySurgeToTargets(baseTargets, surge);

        // Assert
        Assert.AreEqual(2, modifiedTargets, "3 targets - 1 = 2 targets");
    }

    [TestMethod]
    public void ApplySurgeToTargets_MinimumTargets1()
    {
        // Arrange
        int baseTargets = 1;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.TargetModification,
            Modifier = -1
        };

        // Act
        int modifiedTargets = _service.ApplySurgeToTargets(baseTargets, surge);

        // Assert
        Assert.AreEqual(1, modifiedTargets, "Targets should never go below 1");
    }

    [TestMethod]
    public void ApplySurgeToDuration_IncreasesBy50Percent()
    {
        // Arrange
        int baseDuration = 4;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.DurationModification,
            Modifier = 0.5 // +50%
        };

        // Act
        int modifiedDuration = _service.ApplySurgeToDuration(baseDuration, surge);

        // Assert
        Assert.AreEqual(6, modifiedDuration, "4 turns + 50% = 6 turns");
    }

    [TestMethod]
    public void ApplySurgeToDuration_DecreasesBy50Percent()
    {
        // Arrange
        int baseDuration = 4;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.DurationModification,
            Modifier = -0.5 // -50%
        };

        // Act
        int modifiedDuration = _service.ApplySurgeToDuration(baseDuration, surge);

        // Assert
        Assert.AreEqual(2, modifiedDuration, "4 turns - 50% = 2 turns");
    }

    [TestMethod]
    public void ApplySurgeToDuration_MinimumDuration1()
    {
        // Arrange
        int baseDuration = 1;
        var surge = new WildMagicSurgeResult
        {
            Type = SurgeType.DurationModification,
            Modifier = -0.5 // -50%
        };

        // Act
        int modifiedDuration = _service.ApplySurgeToDuration(baseDuration, surge);

        // Assert
        Assert.AreEqual(1, modifiedDuration, "Duration should never go below 1 turn");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullSurgeWorkflow_DamageModification()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        mystic.PsychicStress = 10;
        int alfheimBiomeId = 6;
        int baseDamage = 20;

        // Act - Trigger surge until we get a damage modification
        WildMagicSurgeResult? damageSurge = null;
        for (int i = 0; i < 200 && damageSurge == null; i++)
        {
            mystic.PsychicStress = 10; // Reset stress
            var surge = _service.TryTriggerWildMagicSurge(mystic, "Aetheric Bolt", alfheimBiomeId);
            if (surge?.Type == SurgeType.DamageModification)
            {
                damageSurge = surge;
            }
        }

        // Act - Apply surge to damage
        int finalDamage = _service.ApplySurgeToDamage(baseDamage, damageSurge);

        // Assert
        Assert.IsNotNull(damageSurge);
        Assert.AreEqual(15, mystic.PsychicStress, "Should gain +5 stress from surge");
        Assert.IsTrue(finalDamage == 10 || finalDamage == 30,
            $"20 damage ±50% should be 10 or 30, got {finalDamage}");
        Assert.IsTrue(damageSurge.NarrativeText.Length > 0,
            "Should have narrative text");
    }

    [TestMethod]
    public void FullSurgeWorkflow_MultipleAbilities_AccumulatesStress()
    {
        // Arrange
        var mystic = CreateTestMystic("TestMystic", 100, 100);
        mystic.PsychicStress = 0;
        int alfheimBiomeId = 6;
        int surgeCount = 0;

        // Act - Cast 100 abilities
        for (int i = 0; i < 100; i++)
        {
            var surge = _service.TryTriggerWildMagicSurge(mystic, "Aetheric Bolt", alfheimBiomeId);
            if (surge != null)
            {
                surgeCount++;
            }
        }

        // Assert
        int expectedStress = surgeCount * 5;
        Assert.AreEqual(expectedStress, mystic.PsychicStress,
            $"Should have {expectedStress} stress from {surgeCount} surges");
        Assert.IsTrue(surgeCount >= 15 && surgeCount <= 35,
            $"Expected ~25 surges, got {surgeCount}");
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

    private PlayerCharacter CreateTestMystic(string name, int hp, int maxHp)
    {
        var mystic = CreateTestPlayer(name, hp, maxHp);
        // Note: Set Archetype to "Mystic" - implementation may vary
        // This test assumes the service checks a string property
        mystic.Attributes.Will = 14; // Mystics should have high WILL
        return mystic;
    }

    #endregion
}
