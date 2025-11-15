using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.24.2: Unit tests for Myrk-gengr (Shadow-Walker) specialization
/// Tests specialization seeding, ability structure, stealth mechanics, and terror strikes
/// </summary>
[TestFixture]
public class MyrkgengrSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private MyrkgengrService _myrkgengrService = null!;
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
        _myrkgengrService = new MyrkgengrService(_connectionString);
    }

    #region Specialization Seeding Tests

    [Test]
    public void Myrkgengr_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(24002); // Myrk-gengr ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Myrk-gengr"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(4)); // Skirmisher
        Assert.That(result.Specialization.PathType, Is.EqualTo("Heretical"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("FINESSE"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Stealth Assassin / Alpha Strike"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("High"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🌑"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina + Focus"));
    }

    [Test]
    public void Myrkgengr_AppearsInSkirmisherSpecializations()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var myrkgengr = result.Specializations!.FirstOrDefault(s => s.Name == "Myrk-gengr");
        Assert.That(myrkgengr, Is.Not.Null);
    }

    [Test]
    public void Myrkgengr_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 24002);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    [Test]
    public void Myrkgengr_IsHeretical_AcceptsAnyCorruptionLevel()
    {
        // Arrange
        var lowCorruption = CreateTestSkirmisher();
        lowCorruption.CurrentLegend = 5;
        lowCorruption.Corruption = 10;

        var highCorruption = CreateTestSkirmisher();
        highCorruption.CurrentLegend = 5;
        highCorruption.Corruption = 85;

        // Act
        var lowResult = _specializationService.CanUnlock(lowCorruption, 24002);
        var highResult = _specializationService.CanUnlock(highCorruption, 24002);

        // Assert
        Assert.That(lowResult.Success, Is.True, "Should accept low Corruption");
        Assert.That(highResult.Success, Is.True, "Should accept high Corruption (Heretical path)");
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Myrkgengr_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24002);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Myrkgengr_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24002);

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
    public void Myrkgengr_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24002);

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
    public void Myrkgengr_TotalPPCostIs33()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24002);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(33), "Total PP to learn all abilities should be 33");
    }

    #endregion

    #region Tier 1 Ability Tests

    [Test]
    public void OneWithTheStatic_ProvidesCorrectStealthBonuses()
    {
        // Test Rank 1 (no Psychic Resonance)
        var rank1Bonus = _myrkgengrService.GetStealthBonus(1, false);
        Assert.That(rank1Bonus, Is.EqualTo(1), "Rank 1 should provide +1d10");

        // Test Rank 1 (with Psychic Resonance)
        var rank1BonusResonance = _myrkgengrService.GetStealthBonus(1, true);
        Assert.That(rank1BonusResonance, Is.EqualTo(3), "Rank 1 should provide +3d10 in Resonance zones");

        // Test Rank 2
        var rank2Bonus = _myrkgengrService.GetStealthBonus(2, false);
        Assert.That(rank2Bonus, Is.EqualTo(2), "Rank 2 should provide +2d10");

        // Test Rank 3
        var rank3Bonus = _myrkgengrService.GetStealthBonus(3, false);
        Assert.That(rank3Bonus, Is.EqualTo(3), "Rank 3 should provide +3d10");

        // Test Rank 3 (with Psychic Resonance)
        var rank3BonusResonance = _myrkgengrService.GetStealthBonus(3, true);
        Assert.That(rank3BonusResonance, Is.EqualTo(5), "Rank 3 should provide +5d10 in Resonance zones");
    }

    [Test]
    public void EnterTheVoid_Rank1_Costs40Stamina()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;

        // Act
        var result = _myrkgengrService.EnterTheVoid(shadowWalker, 1, false);

        // Assert - Should deduct 40 Stamina regardless of success
        Assert.That(shadowWalker.Stamina, Is.LessThanOrEqualTo(60));
    }

    [Test]
    public void EnterTheVoid_Rank2_ReducedStaminaCost()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;

        // Act
        var result = _myrkgengrService.EnterTheVoid(shadowWalker, 2, false);

        // Assert - Rank 2 costs 35 Stamina
        Assert.That(shadowWalker.Stamina, Is.LessThanOrEqualTo(65));
    }

    [Test]
    public void EnterTheVoid_FailsWithInsufficientStamina()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 30; // Below 40 needed

        // Act
        var result = _myrkgengrService.EnterTheVoid(shadowWalker, 1, false);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Stamina"));
    }

    [Test]
    public void ShadowStrike_RequiresHiddenState()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        var target = CreateTestEnemy();

        // Act - Try to use Shadow Strike without being Hidden
        var result = _myrkgengrService.ExecuteShadowStrike(shadowWalker, target, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Hidden"));
    }

    [Test]
    public void ShadowStrike_Rank1_DealsDoubledDamage()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };

        // Apply Hidden status
        shadowWalker.StatusEffects.Add(new StatusEffect
        {
            EffectType = "Hidden",
            Duration = -1
        });

        var target = CreateTestEnemy();
        var initialHP = target.HP;

        // Act
        var result = _myrkgengrService.ExecuteShadowStrike(shadowWalker, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.IsCritical, Is.True);
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(target.HP, Is.LessThan(initialHP), "Target should take damage");
    }

    [Test]
    public void ShadowStrike_Rank2_RefundsStaminaOnKill()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.MaxStamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 50 };

        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var target = CreateTestEnemy();
        target.HP = 10; // Low HP to ensure kill

        // Act
        var result = _myrkgengrService.ExecuteShadowStrike(shadowWalker, target, 2);

        // Assert
        if (result.IsKill)
        {
            Assert.That(result.StaminaRefunded, Is.EqualTo(20), "Should refund 20 Stamina on kill");
        }
    }

    [Test]
    public void ShadowStrike_Rank3_AppliesBleeding()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };

        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var target = CreateTestEnemy();

        // Act
        var result = _myrkgengrService.ExecuteShadowStrike(shadowWalker, target, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.StatusEffectsApplied, Does.Contain("Bleeding"));
        Assert.That(target.StatusEffects.Any(e => e.EffectType == "Bleeding"), Is.True);
    }

    #endregion

    #region Tier 2 Ability Tests

    [Test]
    public void ThroatCutter_DealsBaseDamage()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };
        var target = CreateTestEnemy();

        // Act
        var result = _myrkgengrService.ExecuteThroatCutter(shadowWalker, target, 1, false);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(shadowWalker.Stamina, Is.EqualTo(55), "Should cost 45 Stamina");
    }

    [Test]
    public void ThroatCutter_FromFlankOrHidden_AppliesSilenced()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };
        var target = CreateTestEnemy();

        // Act
        var result = _myrkgengrService.ExecuteThroatCutter(shadowWalker, target, 1, isFlankingOrHidden: true);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.StatusEffectsApplied, Does.Contain("Silenced"));
        Assert.That(target.StatusEffects.Any(e => e.EffectType == "Silenced"), Is.True);
    }

    [Test]
    public void ThroatCutter_Rank3_AppliesBleedingToFearedTarget()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };
        var target = CreateTestEnemy();

        // Apply Feared status first
        target.StatusEffects.Add(new StatusEffect { EffectType = "Feared", Duration = 2 });

        // Act
        var result = _myrkgengrService.ExecuteThroatCutter(shadowWalker, target, 3, true);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.StatusEffectsApplied, Does.Contain("Bleeding"));
    }

    [Test]
    public void MindOfStillness_RegeneratesWhileHidden()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.PsychicStress = 50;
        shadowWalker.Stamina = 50;
        shadowWalker.MaxStamina = 100;

        // Apply Hidden status
        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var initialStress = shadowWalker.PsychicStress;
        var initialStamina = shadowWalker.Stamina;

        // Act
        _myrkgengrService.ApplyMindOfStillness(shadowWalker, 1);

        // Assert
        Assert.That(shadowWalker.PsychicStress, Is.LessThan(initialStress), "Should reduce Psychic Stress");
        Assert.That(shadowWalker.Stamina, Is.GreaterThan(initialStamina), "Should regenerate Stamina");
    }

    [Test]
    public void MindOfStillness_DoesNothingWhenNotHidden()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.PsychicStress = 50;
        shadowWalker.Stamina = 50;

        var initialStress = shadowWalker.PsychicStress;
        var initialStamina = shadowWalker.Stamina;

        // Act - No Hidden status
        _myrkgengrService.ApplyMindOfStillness(shadowWalker, 1);

        // Assert
        Assert.That(shadowWalker.PsychicStress, Is.EqualTo(initialStress), "Should not reduce stress when not Hidden");
        Assert.That(shadowWalker.Stamina, Is.EqualTo(initialStamina), "Should not regen stamina when not Hidden");
    }

    #endregion

    #region Tier 3 Ability Tests

    [Test]
    public void TerrorFromTheVoid_InflictsPsychicStress()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };
        shadowWalker.CombatFlags = new Dictionary<string, object>();

        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var target = CreateTestEnemy();
        target.PsychicStress = 0;

        // Act - First Shadow Strike with Terror from the Void
        var result = _myrkgengrService.ExecuteShadowStrike(shadowWalker, target, 1, terrorFromVoidRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.PsychicStressInflicted, Is.EqualTo(12), "Should inflict 12 Stress at Rank 1");
        Assert.That(target.PsychicStress, Is.GreaterThan(0));
    }

    [Test]
    public void GhostlyForm_ProvidesDefenseBonusWhileHidden()
    {
        // Test Rank 1
        var rank1Bonus = _myrkgengrService.GetGhostlyFormDefenseBonus(1, isHidden: true);
        Assert.That(rank1Bonus, Is.EqualTo(2), "Rank 1 should provide +2d10 Defense while Hidden");

        // Test when not Hidden
        var noBonus = _myrkgengrService.GetGhostlyFormDefenseBonus(1, isHidden: false);
        Assert.That(noBonus, Is.EqualTo(0), "Should provide no bonus when not Hidden");

        // Test Rank 3
        var rank3Bonus = _myrkgengrService.GetGhostlyFormDefenseBonus(3, isHidden: true);
        Assert.That(rank3Bonus, Is.EqualTo(4), "Rank 3 should provide +4d10 Defense while Hidden");
    }

    #endregion

    #region Capstone Ability Tests

    [Test]
    public void LivingGlitch_RequiresHiddenState()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        var target = CreateTestEnemy();

        // Act - Try without Hidden
        var result = _myrkgengrService.ExecuteLivingGlitch(shadowWalker, target, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Hidden"));
    }

    [Test]
    public void LivingGlitch_Rank1_DealsMassiveDamage()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.Corruption = 0;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };

        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var target = CreateTestEnemy();
        var initialHP = target.HP;
        var initialStress = target.PsychicStress;

        // Act
        var result = _myrkgengrService.ExecuteLivingGlitch(shadowWalker, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.DamageDealt, Is.GreaterThan(20), "Should deal massive damage");
        Assert.That(result.PsychicStressInflicted, Is.EqualTo(25), "Should inflict 25 Stress at Rank 1");
        Assert.That(result.CorruptionGained, Is.EqualTo(18), "Should gain 18 Corruption at Rank 1");
        Assert.That(shadowWalker.Corruption, Is.EqualTo(18));
        Assert.That(target.HP, Is.LessThan(initialHP));
        Assert.That(target.PsychicStress, Is.GreaterThan(initialStress));
    }

    [Test]
    public void LivingGlitch_Rank3_ReducesSelfCorruption()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.Corruption = 0;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 10 };

        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var target = CreateTestEnemy();

        // Act
        var result = _myrkgengrService.ExecuteLivingGlitch(shadowWalker, target, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CorruptionGained, Is.EqualTo(12), "Should gain only 12 Corruption at Rank 3");
        Assert.That(result.PsychicStressInflicted, Is.EqualTo(35), "Should inflict 35 Stress at Rank 3");
    }

    [Test]
    public void LivingGlitch_Rank3_MaintainsStealthOnKill()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 100;
        shadowWalker.Corruption = 0;
        shadowWalker.EquippedWeapon = new Weapon { Name = "Dagger", Damage = 100 };

        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });

        var target = CreateTestEnemy();
        target.HP = 10; // Low HP to ensure kill

        // Act
        var result = _myrkgengrService.ExecuteLivingGlitch(shadowWalker, target, 3);

        // Assert
        if (result.IsKill)
        {
            Assert.That(result.StealthMaintained, Is.True, "Should maintain stealth on kill at Rank 3");
        }
    }

    [Test]
    public void LivingGlitch_FailsWithInsufficientStamina()
    {
        // Arrange
        var shadowWalker = CreateTestSkirmisher();
        shadowWalker.Stamina = 50; // Below 60 required
        shadowWalker.StatusEffects.Add(new StatusEffect { EffectType = "Hidden", Duration = -1 });
        var target = CreateTestEnemy();

        // Act
        var result = _myrkgengrService.ExecuteLivingGlitch(shadowWalker, target, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Stamina"));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestSkirmisher()
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Shadow-Walker",
            ArchetypeID = 4, // Skirmisher
            CurrentLegend = 5,
            Stamina = 100,
            MaxStamina = 100,
            HP = 100,
            MaxHP = 100,
            PsychicStress = 0,
            Corruption = 0,
            StatusEffects = new List<StatusEffect>(),
            CombatFlags = new Dictionary<string, object>(),
            Attributes = new Dictionary<string, int>
            {
                { "FINESSE", 16 }, // +3 modifier
                { "WILL", 14 }     // +2 modifier
            }
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
            PsychicStress = 0,
            Corruption = 0,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
