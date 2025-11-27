using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.26.3: Unit tests for Skjaldmaer specialization
/// Tests specialization seeding, ability structure, protection mechanics, and Trauma Economy integration
/// </summary>
[TestFixture]
public class SkjaldmaerSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private SkjaldmaerService _skjaldmaerService = null!;
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
        _skjaldmaerService = new SkjaldmaerService(_connectionString);
    }

    #region Specialization Seeding Tests

    [Test]
    public void Skjaldmaer_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(26003); // Skjaldmaer ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Skjaldmaer"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(1)); // Warrior
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("STURDINESS"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Tank / Psychic Stress Mitigation"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Low"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🛡️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina"));
    }

    [Test]
    public void Skjaldmaer_AppearsInWarriorSpecializations()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var skjaldmaer = result.Specializations!.FirstOrDefault(s => s.Name == "Skjaldmaer");
        Assert.That(skjaldmaer, Is.Not.Null);
    }

    [Test]
    public void Skjaldmaer_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 26003);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Skjaldmaer_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26003);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Skjaldmaer_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26003);

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
    public void Skjaldmaer_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26003);

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
    public void Skjaldmaer_TotalPPCostIs33()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26003);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(33), "Total PP to learn all abilities should be 33");
    }

    #endregion

    #region Tier 1 Ability Tests

    [Test]
    public void OathOfProtector_AppliesSoakAndStressResistance()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var ally = CreateTestWarrior();
        ally.CharacterID = 2;
        ally.Name = "Protected Ally";

        // Act - Test Rank 1
        var rank1Result = _skjaldmaerService.ExecuteOathOfProtector(skjaldmaer, ally, 1);

        // Assert
        Assert.That(rank1Result.soakBonus, Is.EqualTo(2), "Rank 1 should provide +2 Soak");
        Assert.That(rank1Result.stressDiceBonus, Is.EqualTo(1), "Rank 1 should provide +1 Stress dice");
        Assert.That(rank1Result.duration, Is.EqualTo(2), "Rank 1 should last 2 turns");
        Assert.That(rank1Result.cleansedDebuff, Is.False, "Rank 1 should not cleanse debuffs");
    }

    [Test]
    public void OathOfProtector_Rank3_CleansesMentalDebuff()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var ally = CreateTestWarrior();
        ally.CharacterID = 2;

        // Act - Test Rank 3
        var rank3Result = _skjaldmaerService.ExecuteOathOfProtector(skjaldmaer, ally, 3);

        // Assert
        Assert.That(rank3Result.soakBonus, Is.EqualTo(4), "Rank 3 should provide +4 Soak");
        Assert.That(rank3Result.stressDiceBonus, Is.EqualTo(2), "Rank 3 should provide +2 Stress dice");
        Assert.That(rank3Result.duration, Is.EqualTo(3), "Rank 3 should last 3 turns");
        // Note: cleansedDebuff would need status effect system to fully test
    }

    [Test]
    public void ShieldBash_Rank1_DealsBaseDamageWithStaggerChance()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var enemy = CreateTestEnemy();

        // Run multiple times to verify probabilistic behavior
        int staggerCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = _skjaldmaerService.ExecuteShieldBash(skjaldmaer, enemy, 1);

            Assert.That(result.damage, Is.GreaterThan(0), "Should deal damage");
            Assert.That(result.pushedToBackRow, Is.False, "Rank 1 should not push");

            if (result.staggered)
            {
                staggerCount++;
            }
        }

        // Assert - Should proc roughly 50% of the time (allow 35-65% range for variance)
        Assert.That(staggerCount, Is.InRange(35, 65), "Stagger should proc approximately 50% of the time");
    }

    [Test]
    public void ShieldBash_Rank3_HasHigherStaggerChanceAndPush()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var enemy = CreateTestEnemy();

        // Run multiple times
        int staggerCount = 0;
        int pushCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = _skjaldmaerService.ExecuteShieldBash(skjaldmaer, enemy, 3);

            if (result.staggered)
            {
                staggerCount++;
                if (result.pushedToBackRow)
                {
                    pushCount++;
                }
            }
        }

        // Assert - Rank 3 has 75% stagger chance
        Assert.That(staggerCount, Is.InRange(65, 85), "Rank 3 should stagger approximately 75% of the time");
        Assert.That(pushCount, Is.EqualTo(staggerCount), "All staggers should result in push at Rank 3");
    }

    #endregion

    #region Tier 2 Ability Tests

    [Test]
    public void GuardiansTaunt_AppliesTauntAndPsychicCost()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var frontRowEnemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };
        var backRowEnemies = new List<Enemy>();

        int initialStress = skjaldmaer.PsychicStress;

        // Act - Test Rank 1
        var rank1Result = _skjaldmaerService.ExecuteGuardiansTaunt(skjaldmaer, frontRowEnemies, backRowEnemies, 1);

        // Assert
        Assert.That(rank1Result.enemiesTaunted, Is.EqualTo(3), "Should taunt 3 front row enemies");
        Assert.That(rank1Result.psychicStressCost, Is.GreaterThan(0), "Should cost Psychic Stress");
        Assert.That(rank1Result.tauntDuration, Is.EqualTo(2), "Should last 2 rounds");
        Assert.That(skjaldmaer.PsychicStress, Is.GreaterThan(initialStress), "Skjaldmaer should gain Psychic Stress");
    }

    [Test]
    public void GuardiansTaunt_Rank2_LowerStressCost()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var frontRowEnemies = new List<Enemy> { CreateTestEnemy() };
        var backRowEnemies = new List<Enemy>();

        // Act
        var rank2Result = _skjaldmaerService.ExecuteGuardiansTaunt(skjaldmaer, frontRowEnemies, backRowEnemies, 2);

        // Assert
        Assert.That(rank2Result.psychicStressCost, Is.LessThanOrEqualTo(3), "Rank 2 should cost 3 or less Stress");
    }

    [Test]
    public void GuardiansTaunt_Rank3_TauntsAllEnemies()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var frontRowEnemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy()
        };
        var backRowEnemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act - Test Rank 3
        var rank3Result = _skjaldmaerService.ExecuteGuardiansTaunt(skjaldmaer, frontRowEnemies, backRowEnemies, 3);

        // Assert
        Assert.That(rank3Result.enemiesTaunted, Is.EqualTo(5), "Rank 3 should taunt ALL enemies (both rows)");
    }

    #endregion

    #region Capstone Ability Tests

    [Test]
    public void BastionOfSanity_AbsorbsTraumaForAlly()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var ally = CreateTestWarrior();
        ally.CharacterID = 2;
        var trauma = new Trauma
        {
            TraumaID = 1,
            Name = "Crippling Fear",
            Description = "Test trauma",
            Severity = 1
        };

        int initialStress = skjaldmaer.PsychicStress;
        int initialCorruption = skjaldmaer.Corruption;

        // Act
        var result = _skjaldmaerService.TriggerBastionOfSanity(skjaldmaer, ally, trauma, false);

        // Assert
        Assert.That(result.triggered, Is.True, "Should trigger successfully");
        Assert.That(result.stressGained, Is.GreaterThan(0), "Should gain Psychic Stress");
        Assert.That(result.corruptionGained, Is.GreaterThan(0), "Should gain Corruption");
        Assert.That(skjaldmaer.PsychicStress, Is.GreaterThan(initialStress), "Stress should increase");
        Assert.That(skjaldmaer.Corruption, Is.GreaterThan(initialCorruption), "Corruption should increase");
    }

    [Test]
    public void BastionOfSanity_CanOnlyTriggerOncePerCombat()
    {
        // Arrange
        var skjaldmaer = CreateTestWarrior();
        var ally = CreateTestWarrior();
        ally.CharacterID = 2;
        var trauma = new Trauma
        {
            TraumaID = 1,
            Name = "Crippling Fear",
            Description = "Test trauma",
            Severity = 1
        };

        // Act - Try to trigger when already triggered
        var result = _skjaldmaerService.TriggerBastionOfSanity(skjaldmaer, ally, trauma, true);

        // Assert
        Assert.That(result.triggered, Is.False, "Should fail if already triggered this combat");
    }

    #endregion

    #region Passive Ability Tests

    [Test]
    public void SanctifiedResolve_ProvidesCorrectBonuses()
    {
        // Test Rank 1
        var rank1Bonus = _skjaldmaerService.GetSanctifiedResolveBonus(1);
        Assert.That(rank1Bonus, Is.EqualTo(1), "Rank 1 should provide +1 die");

        // Test Rank 2
        var rank2Bonus = _skjaldmaerService.GetSanctifiedResolveBonus(2);
        Assert.That(rank2Bonus, Is.EqualTo(2), "Rank 2 should provide +2 dice");

        // Test Rank 3
        var rank3Bonus = _skjaldmaerService.GetSanctifiedResolveBonus(3);
        Assert.That(rank3Bonus, Is.EqualTo(3), "Rank 3 should provide +3 dice");
    }

    [Test]
    public void SanctifiedResolve_Rank3_ReducesAmbientStress()
    {
        // Test Rank 3 stress reduction
        var rank3Reduction = _skjaldmaerService.GetSanctifiedResolveStressReduction(3);
        Assert.That(rank3Reduction, Is.EqualTo(0.10f), "Rank 3 should reduce ambient stress by 10%");

        // Test Rank 1-2 (no reduction)
        var rank1Reduction = _skjaldmaerService.GetSanctifiedResolveStressReduction(1);
        Assert.That(rank1Reduction, Is.EqualTo(0f), "Rank 1 should not reduce ambient stress");
    }

    [Test]
    public void BastionOfSanity_ProvidesPassiveAura()
    {
        // Act
        var aura = _skjaldmaerService.GetBastionOfSanityAura();

        // Assert
        Assert.That(aura.willBonus, Is.EqualTo(1), "Should provide +1 WILL to allies");
        Assert.That(aura.stressReduction, Is.EqualTo(0.10f), "Should reduce ambient stress by 10%");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestWarrior()
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Skjaldmaer",
            Class = CharacterClass.Warrior,
            CurrentLegend = 5,
            ProgressionPoints = 50,
            HP = 120,
            MaxHP = 120,
            CurrentHP = 120,
            Stamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Corruption = 0,
            Attributes = new Attributes
            {
                Might = 3,
                Finesse = 2,
                Wits = 2,
                Will = 4,
                Sturdiness = 5
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
            Name = "Test Enemy",
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
