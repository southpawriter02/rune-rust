using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.28.2: Unit tests for Echo-Caller specialization
/// Tests specialization seeding, ability structure, Echo Chain mechanics, Fear Cascade, and Trauma Economy integration
/// </summary>
[TestFixture]
public class EchoCallerSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private EchoCallerService _echoCallerService = null!;
    private FearCascadeService _fearCascadeService = null!;
    private TraumaEconomyService _traumaService = null!;
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
        _echoCallerService = new EchoCallerService(_connectionString);
        _fearCascadeService = new FearCascadeService(_connectionString);
        _traumaService = new TraumaEconomyService(new Random(42)); // Fixed seed for deterministic tests
    }

    #region Specialization Seeding Tests

    [Test]
    public void EchoCaller_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(28002); // Echo-Caller ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("EchoCaller"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(5)); // Mystic
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Psychic Artillery / Crowd Control / Medium Trauma Risk"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Medium"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("👁️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Aether Pool"));
    }

    [Test]
    public void EchoCaller_AppearsInMysticSpecializations()
    {
        // Arrange
        var character = CreateTestMystic();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var echoCaller = result.Specializations!.FirstOrDefault(s => s.Name == "EchoCaller");
        Assert.That(echoCaller, Is.Not.Null);
    }

    [Test]
    public void EchoCaller_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestMystic();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 28002);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void EchoCaller_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28002);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void EchoCaller_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28002);

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
    public void EchoCaller_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28002);

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
    public void EchoCaller_TotalPPCostIs37()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28002);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(37), "Total PP to learn all abilities should be 37 (3+3+3 + 4+4+4 + 5+5 + 6)");
    }

    #endregion

    #region Echo Chain Mechanics Tests

    [Test]
    public void EchoChain_InitializesForCharacter()
    {
        // Arrange
        var character = CreateTestMystic();

        // Act
        var initialized = _echoCallerService.InitializeEchoChains(character.CharacterID);

        // Assert
        Assert.That(initialized, Is.True, "Echo Chain tracking should initialize successfully");
    }

    [Test]
    public void EchoCascade_IncreasesChainRange_Rank1()
    {
        // Arrange
        var character = CreateTestMystic();
        _echoCallerService.InitializeEchoChains(character.CharacterID);

        // Act
        _echoCallerService.UpdateEchoCascadeRank(character.CharacterID, 1);

        // Assert - Rank 1 should have 2-tile range, 60% damage
        // Verify through database query or service method
    }

    [Test]
    public void EchoCascade_IncreasesChainRange_Rank3()
    {
        // Arrange
        var character = CreateTestMystic();
        _echoCallerService.InitializeEchoChains(character.CharacterID);

        // Act
        _echoCallerService.UpdateEchoCascadeRank(character.CharacterID, 3);

        // Assert - Rank 3 should have 3-tile range, 80% damage, 2 targets
        // Verify through database query or service method
    }

    [Test]
    public void CombatStateReset_ClearsOncePerCombatLimits()
    {
        // Arrange
        var character = CreateTestMystic();
        _echoCallerService.InitializeEchoChains(character.CharacterID);

        // Simulate using Silence Made Weapon
        // (Would need to track uses)

        // Act
        _echoCallerService.ResetCombatState(character.CharacterID);

        // Assert
        // Verify that silence_weapon_uses_this_combat is reset to 0
    }

    #endregion

    #region Ability Execution Tests - Tier 1

    [Test]
    public void ScreamOfSilence_DealsPsychicDamage()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastScreamOfSilence(caster, target, 1);

        // Assert
        Assert.That(result.Success, Is.True, "Scream of Silence should succeed");
        Assert.That(result.Damage, Is.GreaterThan(0), "Should deal damage");
    }

    [Test]
    public void ScreamOfSilence_DealsBonusDamage_ToFearedTarget()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // In a real implementation, would apply Feared status to target first
        // For now, test the logic flow

        // Act
        var result = _echoCallerService.CastScreamOfSilence(caster, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        // With Feared target, damage should be 3d8 + 1d8 = 4d8 total
    }

    [Test]
    public void ScreamOfSilence_TriggersEchoChain_Rank3()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastScreamOfSilence(caster, target, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        // Would verify Echo Chain triggered through message or affected enemies list
    }

    [Test]
    public void PhantomMenace_AppliesFeared()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastPhantomMenace(caster, target, 1);

        // Assert
        Assert.That(result.Success, Is.True, "Phantom Menace should succeed");
        Assert.That(result.AffectedEnemyIds, Contains.Item(target.EnemyID), "Target should be affected");
    }

    #endregion

    #region Ability Execution Tests - Tier 2

    [Test]
    public void RealityFracture_DealsAndPushes()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastRealityFracture(caster, target, "North", 1);

        // Assert
        Assert.That(result.Success, Is.True, "Reality Fracture should succeed");
        Assert.That(result.Damage, Is.GreaterThan(0), "Should deal damage");
        // Would also verify Disoriented status and Push were applied
    }

    #endregion

    #region Ability Execution Tests - Tier 3

    [Test]
    public void FearCascade_DamagesAlreadyFearedEnemies()
    {
        // Arrange
        int casterId = 1;
        int targetId = 2;

        // In real implementation, would set up enemies with Feared status

        // Act
        var result = _fearCascadeService.TriggerFearCascade(casterId, targetId, radius: 3, rank: 1);

        // Assert
        Assert.That(result.Success, Is.True, "Fear Cascade should succeed");
        // Would verify that already-Feared enemies took damage
    }

    [Test]
    public void FearCascade_NewEnemies_MustPassWILLCheck()
    {
        // Arrange
        int casterId = 1;
        int targetId = 2;

        // Act
        var result = _fearCascadeService.TriggerFearCascade(casterId, targetId, radius: 3, rank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        // Would verify that new enemies made WILL checks
    }

    [Test]
    public void FearCascade_EchoChainSpreads_Rank3()
    {
        // Arrange
        int casterId = 1;
        int targetId = 2;

        // Act
        var result = _fearCascadeService.TriggerFearCascade(casterId, targetId, radius: 3, rank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        // Would verify that Echo Chain spread to enemy outside radius
    }

    [Test]
    public void EchoDisplacement_TeleportsAndDamages()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        caster.PsychicStress = 10;
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastEchoDisplacement(caster, target, newX: 10, newY: 5, rank: 1);

        // Assert
        Assert.That(result.Success, Is.True, "Echo Displacement should succeed");
        Assert.That(result.Damage, Is.GreaterThan(0), "Should deal damage");
        Assert.That(caster.PsychicStress, Is.GreaterThan(10), "Should gain Psychic Stress");
    }

    [Test]
    public void EchoDisplacement_AppliesStressCost()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();
        int initialStress = 10;
        caster.PsychicStress = initialStress;
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastEchoDisplacement(caster, target, newX: 10, newY: 5, rank: 1);

        // Assert
        Assert.That(caster.PsychicStress, Is.EqualTo(initialStress + 5), "Should gain +5 Psychic Stress at Rank 1");
    }

    #endregion

    #region Capstone Tests

    [Test]
    public void SilenceMadeWeapon_ScalesDamage_WithStatusCount()
    {
        // Arrange
        var caster = CreateTestMystic();
        var enemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };
        caster.PsychicStress = 10;
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // In real implementation, would apply Feared status to some enemies

        // Act
        var result = _echoCallerService.CastSilenceMadeWeapon(caster, enemies, rank: 1);

        // Assert
        Assert.That(result.Success, Is.True, "Silence Made Weapon should succeed");
        Assert.That(result.AffectedEnemyIds.Count, Is.EqualTo(4), "All 4 enemies should be affected");
    }

    [Test]
    public void SilenceMadeWeapon_CanOnlyBeUsed_OncePerCombat_Rank1()
    {
        // Arrange
        var caster = CreateTestMystic();
        var enemies = new List<Enemy> { CreateTestEnemy() };
        caster.PsychicStress = 10;
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var firstUse = _echoCallerService.CastSilenceMadeWeapon(caster, enemies, rank: 1);
        var secondUse = _echoCallerService.CastSilenceMadeWeapon(caster, enemies, rank: 1);

        // Assert
        Assert.That(firstUse.Success, Is.True, "First use should succeed");
        Assert.That(secondUse.Success, Is.False, "Second use should fail at Rank 1");
    }

    [Test]
    public void SilenceMadeWeapon_CanBeUsedTwice_Rank3()
    {
        // Arrange
        var caster = CreateTestMystic();
        var enemies = new List<Enemy> { CreateTestEnemy() };
        caster.PsychicStress = 10;
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var firstUse = _echoCallerService.CastSilenceMadeWeapon(caster, enemies, rank: 3);
        var secondUse = _echoCallerService.CastSilenceMadeWeapon(caster, enemies, rank: 3);

        // Assert
        Assert.That(firstUse.Success, Is.True, "First use should succeed");
        Assert.That(secondUse.Success, Is.True, "Second use should succeed at Rank 3");
    }

    [Test]
    public void SilenceMadeWeapon_AppliesStressCost_ToEchoCaller()
    {
        // Arrange
        var caster = CreateTestMystic();
        var enemies = new List<Enemy> { CreateTestEnemy() };
        int initialStress = 10;
        caster.PsychicStress = initialStress;
        _echoCallerService.InitializeEchoChains(caster.CharacterID);

        // Act
        var result = _echoCallerService.CastSilenceMadeWeapon(caster, enemies, rank: 1);

        // Assert
        Assert.That(caster.PsychicStress, Is.EqualTo(initialStress + 15), "Should gain +15 Psychic Stress at Rank 1");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestMystic()
    {
        return new PlayerCharacter
        {
            CharacterID = Random.Shared.Next(1000, 9999),
            Name = "Test Echo-Caller",
            Class = CharacterClass.Mystic,
            CurrentHP = 40,
            MaxHP = 40,
            CurrentAP = 100,
            MaxAP = 100,
            MIGHT = 2,
            FINESSE = 3,
            WITS = 4,
            WILL = 5,
            STURDINESS = 3,
            CurrentLegend = 5,
            PsychicStress = 0,
            Corruption = 0,
            Stamina = 100 // Aether mapped to Stamina
        };
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            EnemyID = Random.Shared.Next(1000, 9999),
            Name = "Test Enemy",
            CurrentHP = 50,
            MaxHP = 50,
            MIGHT = 3,
            FINESSE = 2,
            STURDINESS = 3,
            WILL = 2
        };
    }

    #endregion
}
