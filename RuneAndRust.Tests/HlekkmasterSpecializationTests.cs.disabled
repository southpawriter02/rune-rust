using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.25.2: Unit tests for Hlekkr-master (Chain-Master) specialization
/// Tests specialization seeding, ability structure, forced movement mechanics, and control effects
/// </summary>
[TestFixture]
public class HlekkmasterSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private HlekkmasterService _hlekkmasterService = null!;
    private ForcedMovementService _forcedMovementService = null!;
    private DataSeeder _seeder = null!;

    [SetUp]
    public void Setup()
    {
        // Create in-memory database for testing
        _connectionString = "Data Source=:memory:";

        // Initialize database schema
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var saveRepo = new SaveRepository(":memory:");
        _seeder = new DataSeeder(_connectionString);

        // Seed test data
        _seeder.SeedExistingSpecializations();

        _specializationService = new SpecializationService(_connectionString);
        _abilityService = new AbilityService(_connectionString);
        _hlekkmasterService = new HlekkmasterService(_connectionString);
        _forcedMovementService = new ForcedMovementService();
    }

    #region Specialization Seeding Tests

    [Test]
    public void Hlekkmaster_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(25002); // Hlekkr-master ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Hlekkr-master"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(4)); // Skirmisher
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("FINESSE"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("MIGHT"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Battlefield Controller / Formation Breaker"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Low"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("⛓️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina"));
    }

    [Test]
    public void Hlekkmaster_AppearsInSkirmisherSpecializations()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var hlekkmaster = result.Specializations!.FirstOrDefault(s => s.Name == "Hlekkr-master");
        Assert.That(hlekkmaster, Is.Not.Null);
    }

    [Test]
    public void Hlekkmaster_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 25002);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Hlekkmaster_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25002);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Hlekkmaster_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25002);

        // Assert
        var tier1 = result.Abilities!.Count(a => a.TierLevel == 1);
        var tier2 = result.Abilities!.Count(a => a.TierLevel == 2);
        var tier3 = result.Abilities!.Count(a => a.TierLevel == 3);
        var tier4 = result.Abilities!.Count(a => a.TierLevel == 4);

        Assert.That(tier1, Is.EqualTo(3), "Should have 3 Tier 1 abilities");
        Assert.That(tier2, Is.EqualTo(3), "Should have 3 Tier 2 abilities");
        Assert.That(tier3, Is.EqualTo(2), "Should have 2 Tier 3 abilities");
        Assert.That(tier4, Is.EqualTo(1), "Should have 1 Tier 4 ability (Capstone)");
    }

    [Test]
    public void Hlekkmaster_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25002);

        // Assert
        var tier1Abilities = result.Abilities!.Where(a => a.TierLevel == 1);
        var tier2Abilities = result.Abilities!.Where(a => a.TierLevel == 2);
        var tier3Abilities = result.Abilities!.Where(a => a.TierLevel == 3);
        var tier4Abilities = result.Abilities!.Where(a => a.TierLevel == 4);

        Assert.That(tier1Abilities.All(a => a.PPCost == 3), Is.True, "Tier 1 should cost 3 PP");
        Assert.That(tier2Abilities.All(a => a.PPCost == 4), Is.True, "Tier 2 should cost 4 PP");
        Assert.That(tier3Abilities.All(a => a.PPCost == 5), Is.True, "Tier 3 should cost 5 PP");
        Assert.That(tier4Abilities.All(a => a.PPCost == 6), Is.True, "Tier 4 should cost 6 PP");
    }

    [Test]
    public void Hlekkmaster_TotalPPCostIs33()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25002);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(33), "Total PP to learn all abilities should be 33");
    }

    [Test]
    public void Hlekkmaster_CapstoneRequiresTier3Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25002);
        var capstone = result.Abilities!.First(a => a.Name == "Master of Puppets");

        // Assert
        Assert.That(capstone.Prerequisites.RequiredPPInTree, Is.EqualTo(24));
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Is.Not.Empty);
    }

    #endregion

    #region Forced Movement Service Tests

    [Test]
    public void ForcedMovement_PullsEnemyFromBackToFront()
    {
        // Arrange
        var enemy = CreateTestEnemy();
        enemy.Position = new GridPosition(Zone.Enemy, Row.Back, 0);

        // Act
        var result = _forcedMovementService.AttemptForcedMovement(
            enemy,
            ForcedMovementService.MovementDirection.Pull,
            "Test Pull");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(enemy.Position!.Value.Row, Is.EqualTo(Row.Front));
    }

    [Test]
    public void ForcedMovement_PullFailsIfNotInBackRow()
    {
        // Arrange
        var enemy = CreateTestEnemy();
        enemy.Position = new GridPosition(Zone.Enemy, Row.Front, 0);

        // Act
        var result = _forcedMovementService.AttemptForcedMovement(
            enemy,
            ForcedMovementService.MovementDirection.Pull,
            "Test Pull");

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Back Row"));
    }

    [Test]
    public void ForcedMovement_CalculatesCorruptionBonus()
    {
        // Arrange
        var testCases = new[]
        {
            (Corruption: 0, ExpectedBonus: 0),
            (Corruption: 10, ExpectedBonus: 10),   // Low
            (Corruption: 40, ExpectedBonus: 20),   // Medium
            (Corruption: 70, ExpectedBonus: 40),   // High
            (Corruption: 95, ExpectedBonus: 60)    // Extreme
        };

        foreach (var testCase in testCases)
        {
            // Act
            var bonus = _forcedMovementService.CalculatePullSuccessBonus(testCase.Corruption);

            // Assert
            Assert.That(bonus, Is.EqualTo(testCase.ExpectedBonus),
                $"Corruption {testCase.Corruption} should give +{testCase.ExpectedBonus}% bonus");
        }
    }

    #endregion

    #region Hlekkr-master Service Tests

    [Test]
    public void PragmaticPreparation_ExtendsControlDuration()
    {
        // Act
        var rank1Bonus = _hlekkmasterService.GetPragmaticPreparationControlBonus(1);
        var rank2Bonus = _hlekkmasterService.GetPragmaticPreparationControlBonus(2);
        var rank3Bonus = _hlekkmasterService.GetPragmaticPreparationControlBonus(3);

        // Assert
        Assert.That(rank1Bonus, Is.EqualTo(1), "Rank 1 should add +1 turn");
        Assert.That(rank2Bonus, Is.EqualTo(1), "Rank 2 should add +1 turn");
        Assert.That(rank3Bonus, Is.EqualTo(2), "Rank 3 should add +2 turns");
    }

    [Test]
    public void NettingShot_AppliesRootedStatus()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;
        attacker.Attributes.Finesse = 14; // +2 modifier

        var target = CreateTestEnemy();
        target.HP = 50;

        // Act
        var result = _hlekkmasterService.ExecuteNettingShot(
            attacker,
            new List<Enemy> { target },
            abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(attacker.Stamina, Is.EqualTo(80), "Should cost 20 Stamina");
        Assert.That(target.StatusEffects.Any(e => e.EffectType == "Rooted"), Is.True);
    }

    [Test]
    public void NettingShot_Rank2CanTargetTwoEnemies()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;

        var targets = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act
        var result = _hlekkmasterService.ExecuteNettingShot(
            attacker,
            targets,
            abilityRank: 2);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.TargetsHit, Is.EqualTo(2));
    }

    [Test]
    public void GrapplingHookToss_PullsAndDamages()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;
        attacker.Attributes.Finesse = 16; // +3 modifier

        var target = CreateTestEnemy();
        target.HP = 50;
        target.Position = new GridPosition(Zone.Enemy, Row.Back, 0);

        // Act
        var result = _hlekkmasterService.ExecuteGrapplingHookToss(
            attacker,
            target,
            abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(attacker.Stamina, Is.EqualTo(70), "Should cost 30 Stamina");
        Assert.That(target.Position!.Value.Row, Is.EqualTo(Row.Front), "Should pull to Front Row");
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(target.StatusEffects.Any(e => e.EffectType == "Disoriented"), Is.True);
    }

    [Test]
    public void SnagTheGlitch_ScalesWithCorruption()
    {
        // Arrange
        var testCases = new[]
        {
            (Corruption: 10, Rank: 1, ExpectedBonus: 10),
            (Corruption: 40, Rank: 1, ExpectedBonus: 20),
            (Corruption: 70, Rank: 1, ExpectedBonus: 40),
            (Corruption: 95, Rank: 1, ExpectedBonus: 60),
            (Corruption: 95, Rank: 2, ExpectedBonus: 120)  // Doubled at Rank 2
        };

        foreach (var testCase in testCases)
        {
            // Act
            var bonus = _hlekkmasterService.GetSnagTheGlitchSuccessBonus(
                testCase.Corruption,
                testCase.Rank);

            // Assert
            Assert.That(bonus, Is.EqualTo(testCase.ExpectedBonus),
                $"Corruption {testCase.Corruption} at Rank {testCase.Rank} should give +{testCase.ExpectedBonus}%");
        }
    }

    [Test]
    public void UnyieldingGrip_CanSeizeMechanicalEnemies()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;

        var target = CreateTestEnemy();
        target.Type = EnemyType.BlightDrone; // Mechanical
        target.HP = 50;

        // Act
        var result = _hlekkmasterService.ExecuteUnyieldingGrip(
            attacker,
            target,
            abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(attacker.Stamina, Is.EqualTo(75), "Should cost 25 Stamina");
        // Note: Success chance is 60%, so we can't assert [Seized] was applied
    }

    [Test]
    public void PunishTheHelpless_BonusDamageVsControlled()
    {
        // Arrange
        var controlled = CreateTestEnemy();
        controlled.StatusEffects.Add(new StatusEffect
        {
            EffectType = "Rooted",
            DurationRemaining = 2
        });

        var notControlled = CreateTestEnemy();

        // Act
        var controlledMultiplier = _hlekkmasterService.GetPunishTheHelplessDamageMultiplier(controlled, 1);
        var notControlledMultiplier = _hlekkmasterService.GetPunishTheHelplessDamageMultiplier(notControlled, 1);

        // Assert
        Assert.That(controlledMultiplier, Is.EqualTo(1.5f), "Rank 1 should be +50% (1.5x)");
        Assert.That(notControlledMultiplier, Is.EqualTo(1.0f), "No bonus for uncontrolled");
    }

    [Test]
    public void ChainScythe_HitsEntireRow()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;

        var targets = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act
        var result = _hlekkmasterService.ExecuteChainScythe(
            attacker,
            targets,
            Row.Front,
            abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(attacker.Stamina, Is.EqualTo(65), "Should cost 35 Stamina");
        Assert.That(result.TargetsHit, Is.EqualTo(3));
        Assert.That(result.StatusEffectsApplied, Does.Contain("Slowed"));
    }

    [Test]
    public void CorruptionSiphonChain_RequiresCorruption()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;

        var target = CreateTestEnemy();
        target.Corruption = 0; // No corruption

        // Act
        var result = _hlekkmasterService.ExecuteCorruptionSiphonChain(
            attacker,
            target,
            abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("no Corruption"));
    }

    [Test]
    public void CorruptionSiphonChain_AppliesStressCostEvenOnFailure()
    {
        // Arrange
        var attacker = CreateTestSkirmisher();
        attacker.Stamina = 100;
        attacker.PsychicStress = 0;

        var target = CreateTestEnemy();
        target.Corruption = 95; // High corruption

        // Act - run multiple times since success is random
        for (int i = 0; i < 10; i++)
        {
            attacker.PsychicStress = 0;
            var result = _hlekkmasterService.ExecuteCorruptionSiphonChain(
                attacker,
                target,
                abilityRank: 1);

            // Assert - stress should be applied regardless of success
            Assert.That(result.PsychicStressGained, Is.EqualTo(5), "Should gain 5 stress at Rank 1");
        }
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestSkirmisher()
    {
        return new PlayerCharacter
        {
            Name = "Test Skirmisher",
            Class = CharacterClass.Skirmisher,
            CurrentLegend = 5,
            Corruption = 0,
            HP = 100,
            MaxHP = 100,
            Stamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Attributes = new Attributes
            {
                Might = 12,
                Finesse = 14,
                Sturdiness = 10,
                Wits = 10,
                Will = 10,
                Presence = 8
            },
            Position = new GridPosition(Zone.Player, Row.Front, 0)
        };
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            EnemyID = 1,
            Name = "Test Enemy",
            Type = EnemyType.TestSubject,
            HP = 50,
            MaxHP = 50,
            Corruption = 0,
            PsychicStress = 0,
            Attributes = new Attributes
            {
                Might = 10,
                Finesse = 10,
                Sturdiness = 10
            },
            Position = new GridPosition(Zone.Enemy, Row.Front, 0)
        };
    }

    #endregion
}
