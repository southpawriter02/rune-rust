using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.26.2: Unit tests for GorgeMawAscetic specialization
/// Tests specialization seeding, ability structure, Tremorsense mechanics, and seismic control
/// </summary>
[TestFixture]
public class GorgeMawAsceticSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private GorgeMawAsceticService _asceticService = null!;
    private TremorsenseService _tremorsenseService = null!;
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
        _asceticService = new GorgeMawAsceticService(_connectionString);
        _tremorsenseService = new TremorsenseService(_connectionString);
    }

    #region Specialization Seeding Tests

    [Test]
    public void GorgeMawAscetic_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(26002); // GorgeMawAscetic ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("GorgeMawAscetic"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(1)); // Warrior
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("MIGHT"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Control Fighter / Seismic Monk"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("None"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("⛰️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina"));
    }

    [Test]
    public void GorgeMawAscetic_AppearsInWarriorSpecializations()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var ascetic = result.Specializations!.FirstOrDefault(s => s.Name == "GorgeMawAscetic");
        Assert.That(ascetic, Is.Not.Null);
    }

    [Test]
    public void GorgeMawAscetic_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 26002);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void GorgeMawAscetic_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26002);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void GorgeMawAscetic_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26002);

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
    public void GorgeMawAscetic_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26002);

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
    public void GorgeMawAscetic_TotalPPCostIs33()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26002);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(33), "Total PP to learn all abilities should be 33");
    }

    #endregion

    #region Tremorsense Tests

    [Test]
    public void Tremorsense_DetectsAllGroundEnemies_IgnoresFlying()
    {
        // Arrange
        int characterId = 1;
        var enemies = new List<Enemy>
        {
            new Enemy { EnemyID = 101, Name = "Ground Beast 1", IsFlying = false },
            new Enemy { EnemyID = 102, Name = "Ground Beast 2", IsFlying = false },
            new Enemy { EnemyID = 103, Name = "Flying Harpy", IsFlying = true },
            new Enemy { EnemyID = 104, Name = "Ground Beast 3", IsFlying = false }
        };

        // Act
        var result = _tremorsenseService.DetectGroundEnemies(characterId, enemies);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.GroundEnemiesDetected.Count, Is.EqualTo(3), "Should detect 3 ground enemies");
        Assert.That(result.FlyingEnemiesCount, Is.EqualTo(1), "Should count 1 flying enemy");
        Assert.That(result.GroundEnemiesDetected.Contains(103), Is.False, "Should not detect flying enemy");
    }

    [Test]
    public void Tremorsense_Applies50PercentMissChance_VsFlyingEnemies()
    {
        // Arrange
        int characterId = 1;
        var flyingEnemy = new Enemy { EnemyID = 103, Name = "Flying Harpy", IsFlying = true };

        // Act
        var modifiers = _tremorsenseService.ApplyFlyingPenalty(characterId, flyingEnemy);

        // Assert
        Assert.That(modifiers.MissChance, Is.EqualTo(0.5f), "Should apply 50% miss chance vs flying enemies");
    }

    [Test]
    public void Tremorsense_NoMissChance_VsGroundEnemies()
    {
        // Arrange
        int characterId = 1;
        var groundEnemy = new Enemy { EnemyID = 101, Name = "Ground Beast", IsFlying = false };

        // Act
        var modifiers = _tremorsenseService.ApplyFlyingPenalty(characterId, groundEnemy);

        // Assert
        Assert.That(modifiers.MissChance, Is.EqualTo(0f), "Should have no miss chance vs ground enemies");
    }

    [Test]
    public void Tremorsense_IsImmuneToVisionImpairment()
    {
        // Arrange
        int characterId = 1;

        // Act
        var isImmune = _tremorsenseService.IsImmuneToVisionImpairment(characterId);

        // Assert
        Assert.That(isImmune, Is.True, "Should be immune to vision impairment effects");
    }

    [Test]
    public void Tremorsense_AutoDetects_StealthedGroundEnemies()
    {
        // Arrange
        int characterId = 1;
        var enemies = new List<Enemy>
        {
            new Enemy { EnemyID = 101, Name = "Visible Beast", IsFlying = false, IsHidden = false },
            new Enemy { EnemyID = 102, Name = "Stealthed Assassin", IsFlying = false, IsStealth = true },
            new Enemy { EnemyID = 103, Name = "Flying Stealth", IsFlying = true, IsStealth = true }
        };

        // Act
        var stealthedGround = _tremorsenseService.DetectStealthedGroundEnemies(characterId, enemies);

        // Assert
        Assert.That(stealthedGround.Count, Is.EqualTo(1), "Should detect 1 stealthed ground enemy");
        Assert.That(stealthedGround.Contains(102), Is.True, "Should detect stealthed assassin");
        Assert.That(stealthedGround.Contains(103), Is.False, "Should not detect flying stealth enemy");
    }

    [Test]
    public void Tremorsense_ZeroDefense_VsFlyingAttacks()
    {
        // Arrange
        int characterId = 1;

        // Act
        var defense = _tremorsenseService.GetDefenseVsFlyingAttack(characterId);

        // Assert
        Assert.That(defense, Is.EqualTo(0), "Should have 0 defense vs flying attacks");
    }

    #endregion

    #region Tier 1 Ability Tests

    [Test]
    public void StoneFist_Rank1_DealsBaseDamage()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var target = CreateTestEnemy();

        // Act
        var result = _asceticService.ExecuteStoneFist(ascetic, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(ascetic.Stamina, Is.EqualTo(70), "Should cost 30 Stamina");
    }

    [Test]
    public void StoneFist_Rank3_HasStaggerChance()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;

        // Run multiple times to check for stagger proc
        int staggerCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var target = CreateTestEnemy();
            ascetic.Stamina = 100;
            var result = _asceticService.ExecuteStoneFist(ascetic, target, 3);

            if (result.StatusEffectsApplied.Contains("Staggered"))
            {
                staggerCount++;
            }
        }

        // Assert - Should proc roughly 10% of the time (allow 3-17% range for variance)
        Assert.That(staggerCount, Is.InRange(3, 17), "Stagger should proc approximately 10% of the time");
    }

    [Test]
    public void StoneFist_InsufficientStamina_Fails()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 20; // Not enough
        var target = CreateTestEnemy();

        // Act
        var result = _asceticService.ExecuteStoneFist(ascetic, target, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Stamina"));
    }

    [Test]
    public void ConcussivePulse_PushesEnemiesAndDealsDamage()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var frontRowEnemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act
        var result = _asceticService.ExecuteConcussivePulse(ascetic, frontRowEnemies, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.EnemiesAffected, Is.EqualTo(3), "Should affect 3 enemies");
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(ascetic.Stamina, Is.EqualTo(65), "Should cost 35 Stamina");
    }

    [Test]
    public void ConcussivePulse_IgnoresFlyingEnemies()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var frontRowEnemies = new List<Enemy>
        {
            new Enemy { EnemyID = 101, Name = "Ground Beast", HP = 50, MaxHP = 50, IsFlying = false },
            new Enemy { EnemyID = 102, Name = "Flying Harpy", HP = 50, MaxHP = 50, IsFlying = true },
            new Enemy { EnemyID = 103, Name = "Ground Beast 2", HP = 50, MaxHP = 50, IsFlying = false }
        };

        // Act
        var result = _asceticService.ExecuteConcussivePulse(ascetic, frontRowEnemies, 1);

        // Assert
        Assert.That(result.EnemiesAffected, Is.EqualTo(2), "Should only affect 2 ground enemies");
    }

    #endregion

    #region Tier 2 Ability Tests

    [Test]
    public void SensoryDiscipline_ProvidesCorrectBonuses()
    {
        // Test Rank 1
        var rank1Bonus = _asceticService.GetSensoryDisciplineBonus(1);
        Assert.That(rank1Bonus, Is.EqualTo(2), "Rank 1 should provide +2 dice");

        // Test Rank 2
        var rank2Bonus = _asceticService.GetSensoryDisciplineBonus(2);
        Assert.That(rank2Bonus, Is.EqualTo(3), "Rank 2 should provide +3 dice");

        // Test Rank 3
        var rank3Bonus = _asceticService.GetSensoryDisciplineBonus(3);
        Assert.That(rank3Bonus, Is.EqualTo(4), "Rank 3 should provide +4 dice");
    }

    [Test]
    public void ShatteringWave_Rank1_60PercentStunChance()
    {
        // Arrange
        var ascetic = CreateTestWarrior();

        // Run multiple times to verify probabilistic behavior
        int stunCount = 0;
        int staggerCount = 0;
        for (int i = 0; i < 100; i++)
        {
            ascetic.Stamina = 100;
            var target = CreateTestEnemy();
            var result = _asceticService.ExecuteShatteringWave(ascetic, target, 1);

            if (result.StatusEffectsApplied.Contains("Stunned"))
                stunCount++;
            else if (result.StatusEffectsApplied.Contains("Staggered"))
                staggerCount++;
        }

        // Assert
        Assert.That(stunCount, Is.InRange(45, 75), "Stun should proc approximately 60% of the time");
        Assert.That(stunCount + staggerCount, Is.EqualTo(100), "Should always apply either Stun or Stagger");
    }

    [Test]
    public void ShatteringWave_CannotAffectFlyingEnemies()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var flyingTarget = new Enemy { EnemyID = 101, Name = "Flying Harpy", HP = 50, MaxHP = 50, IsFlying = true };

        // Act
        var result = _asceticService.ExecuteShatteringWave(ascetic, flyingTarget, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("cannot affect flying enemy"));
    }

    [Test]
    public void ShatteringWave_Rank3_HigherStunChanceAndDuration()
    {
        // Arrange
        var ascetic = CreateTestWarrior();

        // Run multiple times
        int stunCount = 0;
        for (int i = 0; i < 100; i++)
        {
            ascetic.Stamina = 100;
            var target = CreateTestEnemy();
            var result = _asceticService.ExecuteShatteringWave(ascetic, target, 3);

            if (result.StatusEffectsApplied.Contains("Stunned"))
                stunCount++;
        }

        // Assert - Rank 3 has 85% stun chance
        Assert.That(stunCount, Is.InRange(75, 95), "Rank 3 should stun approximately 85% of the time");
    }

    #endregion

    #region Tier 3 Ability Tests

    [Test]
    public void InnerStillness_ProvidesCorrectImmunities()
    {
        // Test Rank 1
        var rank1Immunities = _asceticService.GetInnerStillnessImmunities(1);
        Assert.That(rank1Immunities.Count, Is.EqualTo(1));
        Assert.That(rank1Immunities.Contains("Fear"), Is.True);

        // Test Rank 2
        var rank2Immunities = _asceticService.GetInnerStillnessImmunities(2);
        Assert.That(rank2Immunities.Count, Is.EqualTo(2));
        Assert.That(rank2Immunities.Contains("Fear"), Is.True);
        Assert.That(rank2Immunities.Contains("Disoriented"), Is.True);

        // Test Rank 3
        var rank3Immunities = _asceticService.GetInnerStillnessImmunities(3);
        Assert.That(rank3Immunities.Count, Is.EqualTo(3));
        Assert.That(rank3Immunities.Contains("Fear"), Is.True);
        Assert.That(rank3Immunities.Contains("Disoriented"), Is.True);
        Assert.That(rank3Immunities.Contains("Charmed"), Is.True);
    }

    [Test]
    public void InnerStillness_ProvidesAuraBonus()
    {
        // Test Rank 1-2
        var rank1Aura = _asceticService.GetInnerStillnessAuraBonus(1);
        Assert.That(rank1Aura, Is.EqualTo(1), "Rank 1-2 should provide +1 die aura");

        var rank2Aura = _asceticService.GetInnerStillnessAuraBonus(2);
        Assert.That(rank2Aura, Is.EqualTo(1), "Rank 1-2 should provide +1 die aura");

        // Test Rank 3
        var rank3Aura = _asceticService.GetInnerStillnessAuraBonus(3);
        Assert.That(rank3Aura, Is.EqualTo(2), "Rank 3 should provide +2 dice aura");
    }

    #endregion

    #region Capstone Ability Tests

    [Test]
    public void Earthshaker_DealsHighDamageToAllGroundEnemies()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var allEnemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            new Enemy { EnemyID = 103, Name = "Flying Harpy", HP = 50, MaxHP = 50, IsFlying = true },
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act
        var result = _asceticService.ExecuteEarthshaker(ascetic, allEnemies, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.EnemiesAffected, Is.EqualTo(4), "Should affect 4 ground enemies (not the flying one)");
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(result.StatusEffectsApplied.Contains("KnockedDown"), Is.True);
        Assert.That(ascetic.Stamina, Is.EqualTo(40), "Should cost 60 Stamina");
    }

    [Test]
    public void Earthshaker_AltersTerrain()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var allEnemies = new List<Enemy> { CreateTestEnemy() };

        // Act - Test different ranks
        var rank1Result = _asceticService.ExecuteEarthshaker(ascetic, allEnemies, 1);
        Assert.That(rank1Result.TerrainAltered, Is.True);
        Assert.That(rank1Result.TerrainSize, Is.EqualTo(3), "Rank 1 should create 3x3 terrain");

        ascetic.Stamina = 100;
        var rank2Result = _asceticService.ExecuteEarthshaker(ascetic, allEnemies, 2);
        Assert.That(rank2Result.TerrainSize, Is.EqualTo(4), "Rank 2 should create 4x4 terrain");

        ascetic.Stamina = 100;
        var rank3Result = _asceticService.ExecuteEarthshaker(ascetic, allEnemies, 3);
        Assert.That(rank3Result.TerrainSize, Is.EqualTo(5), "Rank 3 should create 5x5 terrain");
    }

    [Test]
    public void Earthshaker_Rank3_AppliesVulnerable()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 100;
        var allEnemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        var result = _asceticService.ExecuteEarthshaker(ascetic, allEnemies, 3);

        // Assert
        Assert.That(result.StatusEffectsApplied.Contains("Vulnerable"), Is.True, "Rank 3 should apply Vulnerable");
        Assert.That(result.StatusEffectsApplied.Contains("KnockedDown"), Is.True, "Should still apply Knocked Down");
    }

    [Test]
    public void Earthshaker_InsufficientStamina_Fails()
    {
        // Arrange
        var ascetic = CreateTestWarrior();
        ascetic.Stamina = 50; // Not enough
        var allEnemies = new List<Enemy> { CreateTestEnemy() };

        // Act
        var result = _asceticService.ExecuteEarthshaker(ascetic, allEnemies, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Stamina"));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestWarrior()
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Ascetic",
            Class = CharacterClass.Warrior,
            CurrentLegend = 5,
            ProgressionPoints = 50,
            HP = 120,
            MaxHP = 120,
            Stamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Corruption = 0,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 2,
                Wits = 2,
                Will = 3,
                Sturdiness = 4
            },
            Abilities = new List<Ability>(),
            StatusEffects = new List<StatusEffect>(),
            CompletedQuests = new List<Quest>()
        };
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            EnemyID = new Random().Next(1000, 9999),
            Name = "Test Ground Beast",
            HP = 50,
            MaxHP = 50,
            IsFlying = false,
            IsHidden = false,
            IsStealth = false,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
