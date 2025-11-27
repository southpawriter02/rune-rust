using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.25.1: Unit tests for Strandhogg (Glitch-Raider) specialization
/// Tests specialization seeding, ability structure, Momentum mechanics, and hit-and-run tactics
/// </summary>
[TestFixture]
public class StrandhoggSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private StrandhoggService _strandhoggService = null!;
    private MomentumService _momentumService = null!;
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
        _strandhoggService = new StrandhoggService(_connectionString);
        _momentumService = new MomentumService();
    }

    #region Specialization Seeding Tests

    [Test]
    public void Strandhogg_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(25001); // Strandhogg ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Strandhogg"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(4)); // Skirmisher
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("FINESSE"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("MIGHT"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Mobile Burst DPS / Momentum Fighter"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Low"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("⚔️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina + Momentum"));
    }

    [Test]
    public void Strandhogg_AppearsInSkirmisherSpecializations()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var strandhogg = result.Specializations!.FirstOrDefault(s => s.Name == "Strandhogg");
        Assert.That(strandhogg, Is.Not.Null);
    }

    [Test]
    public void Strandhogg_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 25001);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Strandhogg_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25001);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Strandhogg_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25001);

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
    public void Strandhogg_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25001);

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
    public void Strandhogg_TotalPPCostIs33()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(25001);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(33), "Total PP to learn all abilities should be 33");
    }

    #endregion

    #region Momentum Service Tests

    [Test]
    public void MomentumService_GeneratesMomentum_CapsAt100()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 90;

        // Act
        _momentumService.GenerateMomentum(character, 20, "Test");

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(100), "Momentum should cap at 100");
    }

    [Test]
    public void MomentumService_SpendsMomentum_Successfully()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 50;

        // Act
        var result = _momentumService.SpendMomentum(character, 30, "Test Ability");

        // Assert
        Assert.That(result, Is.True);
        Assert.That(character.Momentum, Is.EqualTo(20));
    }

    [Test]
    public void MomentumService_SpendsMomentum_FailsWhenInsufficient()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 20;

        // Act
        var result = _momentumService.SpendMomentum(character, 30, "Test Ability");

        // Assert
        Assert.That(result, Is.False);
        Assert.That(character.Momentum, Is.EqualTo(20), "Momentum should not change when spend fails");
    }

    [Test]
    public void MomentumService_AppliesDecay_OutOfCombat()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 30;

        // Act
        _momentumService.ApplyMomentumDecay(character, isInCombat: false);

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(25), "Should decay 5 Momentum out of combat");
    }

    [Test]
    public void MomentumService_NoDecay_InCombat()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 30;

        // Act
        _momentumService.ApplyMomentumDecay(character, isInCombat: true);

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(30), "Should not decay Momentum in combat");
    }

    #endregion

    #region Tier 1 Ability Tests

    [Test]
    public void HarriersAlacrity_InitializesCombatMomentum_Rank1()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 0;

        // Act
        _strandhoggService.InitializeHarriersAlacrity(character, abilityRank: 1);

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(20), "Rank 1 should start with 20 Momentum");
    }

    [Test]
    public void HarriersAlacrity_InitializesCombatMomentum_Rank3()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 0;

        // Act
        _strandhoggService.InitializeHarriersAlacrity(character, abilityRank: 3);

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(30), "Rank 3 should start with 30 Momentum");
    }

    [Test]
    public void HarriersAlacrity_ProvidesVigilanceBonus()
    {
        // Test Rank 1
        var rank1Bonus = _strandhoggService.GetHarriersAlacrityVigilanceBonus(1);
        Assert.That(rank1Bonus, Is.EqualTo(2), "Rank 1 should provide +2 Vigilance");

        // Test Rank 2
        var rank2Bonus = _strandhoggService.GetHarriersAlacrityVigilanceBonus(2);
        Assert.That(rank2Bonus, Is.EqualTo(3), "Rank 2+ should provide +3 Vigilance");
    }

    [Test]
    public void ReaversStrike_GeneratesMomentum_Rank1()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 0;
        character.EquippedWeapon = new Weapon { Name = "Sword", Damage = 15 };
        var target = CreateTestEnemy();

        // Act
        var result = _strandhoggService.ExecuteReaversStrike(character, target, abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.MomentumGenerated, Is.EqualTo(15), "Should generate 15 Momentum");
        Assert.That(character.Stamina, Is.EqualTo(65), "Should cost 35 Stamina at Rank 1");
    }

    [Test]
    public void ReaversStrike_BonusMomentumVsDebuffed_Rank3()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 0;
        character.EquippedWeapon = new Weapon { Name = "Sword", Damage = 15 };
        var target = CreateTestEnemy();

        // Add debuff to target
        target.StatusEffects.Add(new StatusEffect
        {
            TargetID = target.EnemyID,
            EffectType = "Disoriented",
            DurationRemaining = 2
        });

        // Act
        var result = _strandhoggService.ExecuteReaversStrike(character, target, abilityRank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.MomentumGenerated, Is.EqualTo(25), "Should generate 25 Momentum vs debuffed (15+10)");
    }

    [Test]
    public void DreadCharge_AppliesDisoriented_AndGeneratesMomentum()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 0;
        var target = CreateTestEnemy();

        // Act
        var result = _strandhoggService.ExecuteDreadCharge(character, target, abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(result.StatusEffectsApplied, Contains.Item("Disoriented"));
        Assert.That(result.MomentumGenerated, Is.EqualTo(15), "Should generate 5 (move) + 10 (hit) = 15 Momentum");
        Assert.That(character.Stamina, Is.EqualTo(60), "Should cost 40 Stamina");
    }

    [Test]
    public void DreadCharge_IncreasedDuration_Rank2()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 0;
        var target = CreateTestEnemy();

        // Act
        var result = _strandhoggService.ExecuteDreadCharge(character, target, abilityRank: 2);

        // Assert
        Assert.That(result.Success, Is.True);

        var disorientedEffect = target.StatusEffects.FirstOrDefault(se => se.EffectType == "Disoriented");
        Assert.That(disorientedEffect, Is.Not.Null);
        Assert.That(disorientedEffect!.DurationRemaining, Is.EqualTo(2), "Rank 2 should apply 2 turn duration");
    }

    #endregion

    #region Tier 2 Ability Tests

    [Test]
    public void TidalRush_ProvidesBonusMomentum_VsDebuffedEnemy()
    {
        // Arrange
        var target = CreateTestEnemy();
        target.StatusEffects.Add(new StatusEffect
        {
            TargetID = target.EnemyID,
            EffectType = "Stunned",
            DurationRemaining = 1
        });

        // Act - Rank 1
        var rank1Bonus = _strandhoggService.GetTidalRushBonus(target, abilityRank: 1);
        Assert.That(rank1Bonus, Is.EqualTo(10), "Rank 1 should provide +10 bonus");

        // Act - Rank 2
        var rank2Bonus = _strandhoggService.GetTidalRushBonus(target, abilityRank: 2);
        Assert.That(rank2Bonus, Is.EqualTo(15), "Rank 2 should provide +15 bonus");
    }

    [Test]
    public void HarriersWhirlwind_HitAndRun_GeneratesMomentum()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 50;
        var target = CreateTestEnemy();

        // Act
        var result = _strandhoggService.ExecuteHarriersWhirlwind(character, target, abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.MomentumSpent, Is.EqualTo(30));
        Assert.That(result.MomentumGenerated, Is.EqualTo(5), "Free move should generate 5 Momentum");
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
    }

    [Test]
    public void HarriersWhirlwind_DoubleMomentumGeneration_Rank3()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 50;
        var target = CreateTestEnemy();

        // Act
        var result = _strandhoggService.ExecuteHarriersWhirlwind(character, target, abilityRank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.MomentumGenerated, Is.EqualTo(10), "Rank 3 should generate 10 Momentum from free move");
    }

    [Test]
    public void ViciousFlank_BonusDamage_VsDebuffedTarget()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 50;
        character.Attributes = new Attributes { Might = 16 }; // +3 modifier

        var target = CreateTestEnemy();
        target.HP = 100;
        target.StatusEffects.Add(new StatusEffect
        {
            TargetID = target.EnemyID,
            EffectType = "Bleeding",
            DurationRemaining = 2
        });

        var targetWithoutDebuff = CreateTestEnemy();
        targetWithoutDebuff.HP = 100;

        // Act - With debuff
        var resultDebuffed = _strandhoggService.ExecuteViciousFlank(character, target, abilityRank: 1);

        // Reset character resources
        character.Stamina = 100;
        character.Momentum = 50;

        // Act - Without debuff
        var resultNormal = _strandhoggService.ExecuteViciousFlank(character, targetWithoutDebuff, abilityRank: 1);

        // Assert
        Assert.That(resultDebuffed.DamageDealt, Is.GreaterThan(resultNormal.DamageDealt),
            "Damage vs debuffed should be +50% higher");
    }

    [Test]
    public void ViciousFlank_RefundsMomentum_OnKill_Rank3()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 50;
        var target = CreateTestEnemy();
        target.HP = 1; // Easy kill

        // Act
        var result = _strandhoggService.ExecuteViciousFlank(character, target, abilityRank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.IsKill, Is.True);
        Assert.That(result.MomentumRefunded, Is.EqualTo(10), "Rank 3 should refund 10 Momentum on kill");
    }

    #endregion

    #region Tier 3 Ability Tests

    [Test]
    public void NoQuarter_GeneratesMomentum_OnKill()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 0;

        // Act
        _strandhoggService.TriggerNoQuarter(character, abilityRank: 1);

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(5), "Should generate 5 Momentum from free move");
    }

    [Test]
    public void NoQuarter_GrantsTempHP_Rank3()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Momentum = 0;
        character.TempHP = 0;

        // Act
        _strandhoggService.TriggerNoQuarter(character, abilityRank: 3);

        // Assert
        Assert.That(character.Momentum, Is.EqualTo(10), "Rank 3 should generate 10 Momentum");
        Assert.That(character.TempHP, Is.EqualTo(15), "Rank 3 should grant 15 temp HP");
    }

    [Test]
    public void SavageHarvest_RefundsResources_OnKill()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 60;
        var target = CreateTestEnemy();
        target.HP = 1; // Easy kill

        // Act
        var result = _strandhoggService.ExecuteSavageHarvest(character, target, abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.IsKill, Is.True);
        Assert.That(result.StaminaRefunded, Is.EqualTo(20));
        Assert.That(result.MomentumRefunded, Is.EqualTo(20));
        Assert.That(character.Stamina, Is.EqualTo(70), "Should refund 20 Stamina (50 spent, 20 refunded)");
    }

    [Test]
    public void SavageHarvest_IncreasedDamage_Rank2()
    {
        // Arrange
        var character1 = CreateTestSkirmisher();
        character1.Stamina = 100;
        character1.Momentum = 60;
        var target1 = CreateTestEnemy();
        target1.HP = 100;

        var character2 = CreateTestSkirmisher();
        character2.Stamina = 100;
        character2.Momentum = 60;
        var target2 = CreateTestEnemy();
        target2.HP = 100;

        // Act
        var resultRank1 = _strandhoggService.ExecuteSavageHarvest(character1, target1, abilityRank: 1);
        var resultRank2 = _strandhoggService.ExecuteSavageHarvest(character2, target2, abilityRank: 2);

        // Assert
        Assert.That(resultRank2.DamageDealt, Is.GreaterThan(resultRank1.DamageDealt),
            "Rank 2 should deal more damage (10d10 vs 8d10)");
    }

    #endregion

    #region Capstone Ability Tests

    [Test]
    public void RiptideOfCarnage_ExecutesMultipleAttacks_Rank1()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 80;

        var targets = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act
        var result = _strandhoggService.ExecuteRiptideOfCarnage(character, targets, abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.MomentumSpent, Is.EqualTo(75));
        Assert.That(result.PsychicStressGained, Is.EqualTo(15), "Should gain 15 Psychic Stress");
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
    }

    [Test]
    public void RiptideOfCarnage_4Attacks_Rank2()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 80;

        var targets = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        foreach (var target in targets)
        {
            target.HP = 1; // Easy kills
        }

        // Act
        var result = _strandhoggService.ExecuteRiptideOfCarnage(character, targets, abilityRank: 2);

        // Assert
        Assert.That(result.Success, Is.True);
        // Rank 2 should execute 4 attacks
        var deadTargets = targets.Count(t => t.HP == 0);
        Assert.That(deadTargets, Is.EqualTo(4), "Should kill all 4 targets at Rank 2");
    }

    [Test]
    public void RiptideOfCarnage_RefundsMomentum_OnKills_Rank3()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 80;

        var targets = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        foreach (var target in targets)
        {
            target.HP = 1; // Easy kills
        }

        // Act
        var result = _strandhoggService.ExecuteRiptideOfCarnage(character, targets, abilityRank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.MomentumRefunded, Is.EqualTo(30), "Should refund 10 Momentum per kill (3 kills)");
        Assert.That(result.PsychicStressGained, Is.EqualTo(10), "Rank 3 should reduce Stress to 10");
    }

    [Test]
    public void RiptideOfCarnage_FailsWhenInsufficientMomentum()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.Stamina = 100;
        character.Momentum = 50; // Not enough

        var targets = new List<Enemy> { CreateTestEnemy() };

        // Act
        var result = _strandhoggService.ExecuteRiptideOfCarnage(character, targets, abilityRank: 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Momentum"));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestSkirmisher()
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Strandhogg",
            Archetype = new Archetype { ArchetypeID = 4, Name = "Skirmisher" },
            Attributes = new Attributes
            {
                Might = 14,  // +2 modifier
                Finesse = 16, // +3 modifier
                Wits = 12,   // +1 modifier
                Will = 10,   // +0 modifier
                Sturdiness = 12 // +1 modifier
            },
            HP = 50,
            MaxHP = 50,
            Stamina = 100,
            MaxStamina = 100,
            Momentum = 0,
            MaxMomentum = 100,
            CurrentLegend = 5,
            ProgressionPoints = 30,
            PsychicStress = 0,
            Corruption = 0
        };
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            EnemyID = 1,
            Name = "Test Enemy",
            HP = 50,
            MaxHP = 50,
            Armor = 0,
            PsychicStress = 0,
            Corruption = 0,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
