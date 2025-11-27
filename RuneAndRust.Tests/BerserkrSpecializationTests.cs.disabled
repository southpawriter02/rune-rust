using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.26.1: Unit tests for Berserkr specialization
/// Tests specialization seeding, ability structure, Fury resource mechanics, and high-risk gameplay
/// </summary>
[TestFixture]
public class BerserkrSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private BerserkrService _berserkrService = null!;
    private FuryService _furyService = null!;
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
        _berserkrService = new BerserkrService(_connectionString);
        _furyService = new FuryService(_connectionString);
    }

    #region Specialization Seeding Tests

    [Test]
    public void Berserkr_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(26001); // Berserkr ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Berserkr"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(1)); // Warrior
        Assert.That(result.Specialization.PathType, Is.EqualTo("Heretical"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("MIGHT"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("STURDINESS"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Melee Damage Dealer / Fury Fighter"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("High"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🔥"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina + Fury (0-100)"));
        Assert.That(result.Specialization.PPCostToUnlock, Is.EqualTo(10));
    }

    [Test]
    public void Berserkr_AppearsInWarriorSpecializations()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var berserkr = result.Specializations!.FirstOrDefault(s => s.Name == "Berserkr");
        Assert.That(berserkr, Is.Not.Null);
    }

    [Test]
    public void Berserkr_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 26001);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    [Test]
    public void Berserkr_HasHighTraumaRisk()
    {
        // Act
        var result = _specializationService.GetSpecialization(26001);

        // Assert
        Assert.That(result.Specialization!.TraumaRisk, Is.EqualTo("High"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Berserkr_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26001);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Berserkr_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26001);

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
    public void Berserkr_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26001);

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
    public void Berserkr_AllAbilitiesHaveCorrectSpecializationID()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(26001);

        // Assert
        Assert.That(result.Abilities!.All(a => a.SpecializationID == 26001), Is.True);
    }

    #endregion

    #region Fury Resource Mechanic Tests

    [Test]
    public void FuryService_InitializesFuryTracking()
    {
        // Arrange
        var character = CreateTestWarrior();
        int characterId = 1;

        // Act
        var result = _furyService.InitializeFury(characterId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CurrentFury, Is.EqualTo(0));
    }

    [Test]
    public void FuryService_GeneratesFuryFromDamageTaken_BaseRate()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);

        // Act
        var result = _furyService.GenerateFuryFromDamageTaken(characterId, 20, hasBloodFueled: false);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.FuryChange, Is.EqualTo(20), "Base Fury generation should be 1:1 with HP damage");
        Assert.That(result.CurrentFury, Is.EqualTo(20));
    }

    [Test]
    public void FuryService_GeneratesFuryFromDamageTaken_WithBloodFueled()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);

        // Act - Rank 1 Blood-Fueled (2x multiplier)
        var result = _furyService.GenerateFuryFromDamageTaken(characterId, 20, hasBloodFueled: true, bloodFueledRank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.FuryChange, Is.EqualTo(40), "Blood-Fueled Rank 1 should double Fury generation");
        Assert.That(result.CurrentFury, Is.EqualTo(40));
    }

    [Test]
    public void FuryService_FuryCapsAt100()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);

        // Act
        var result = _furyService.GenerateFuryFromDamageTaken(characterId, 200, hasBloodFueled: false);

        // Assert
        Assert.That(result.CurrentFury, Is.EqualTo(100), "Fury should cap at 100");
        Assert.That(result.FuryChange, Is.EqualTo(100), "Only 100 Fury should be gained");
    }

    [Test]
    public void FuryService_SpendsFurySuccessfully()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 50);

        // Act
        var result = _furyService.SpendFury(characterId, 30);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CurrentFury, Is.EqualTo(20), "Should have 20 Fury remaining after spending 30");
    }

    [Test]
    public void FuryService_FailsToSpendInsufficientFury()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 20);

        // Act
        var result = _furyService.SpendFury(characterId, 30);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Fury"));
    }

    [Test]
    public void FuryService_ResetsFuryAfterCombat()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 50);

        // Act
        var result = _furyService.ResetFury(characterId, "Combat ended");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CurrentFury, Is.EqualTo(0));
    }

    [Test]
    public void FuryService_TriggersUnstoppableFury()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);

        // Act
        var result = _furyService.TriggerUnstoppableFury(characterId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CurrentFury, Is.EqualTo(100), "Unstoppable Fury should grant 100 Fury");
        Assert.That(result.Message, Does.Contain("UNSTOPPABLE FURY"));
    }

    [Test]
    public void FuryService_UnstoppableFury_OnlyTriggersOncePerCombat()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.TriggerUnstoppableFury(characterId);

        // Act
        var result = _furyService.TriggerUnstoppableFury(characterId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("already used"));
    }

    #endregion

    #region Primal Vigor Tests

    [Test]
    public void PrimalVigor_Provides2StaminaRegenPer25Fury_Rank1()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 50); // 50 Fury

        // Act
        int bonus = _berserkrService.CalculatePrimalVigorBonus(characterId, rank: 1);

        // Assert
        Assert.That(bonus, Is.EqualTo(4), "50 Fury / 25 = 2 breakpoints * 2 = +4 Stamina regen");
    }

    [Test]
    public void PrimalVigor_Provides8StaminaRegenAt100Fury_Rank1()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 100); // 100 Fury

        // Act
        int bonus = _berserkrService.CalculatePrimalVigorBonus(characterId, rank: 1);

        // Assert
        Assert.That(bonus, Is.EqualTo(8), "100 Fury / 25 = 4 breakpoints * 2 = +8 Stamina regen");
    }

    [Test]
    public void PrimalVigor_ScalesWithRank()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 100); // 100 Fury

        // Act
        int rank1Bonus = _berserkrService.CalculatePrimalVigorBonus(characterId, rank: 1);
        int rank2Bonus = _berserkrService.CalculatePrimalVigorBonus(characterId, rank: 2);
        int rank3Bonus = _berserkrService.CalculatePrimalVigorBonus(characterId, rank: 3);

        // Assert
        Assert.That(rank1Bonus, Is.EqualTo(8), "Rank 1: 4 breakpoints * 2 = +8");
        Assert.That(rank2Bonus, Is.EqualTo(12), "Rank 2: 4 breakpoints * 3 = +12");
        Assert.That(rank3Bonus, Is.EqualTo(16), "Rank 3: 4 breakpoints * 4 = +16");
    }

    #endregion

    #region Ability Execution Tests

    [Test]
    public void WildSwing_GeneratesFuryPerEnemyHit()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CharacterID = 1;
        _furyService.InitializeFury(character.CharacterID);

        // Act
        var (totalDamage, furyGenerated, message) = _berserkrService.ExecuteWildSwing(character, targetCount: 3, rank: 1);

        // Assert
        Assert.That(furyGenerated, Is.GreaterThan(0), "Should generate Fury from hitting 3 enemies");
        Assert.That(totalDamage, Is.GreaterThan(0), "Should deal damage");
        Assert.That(message, Does.Contain("Wild Swing"));
    }

    [Test]
    public void RecklessAssault_GeneratesHighFury()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CharacterID = 1;
        _furyService.InitializeFury(character.CharacterID);

        // Act
        var (damage, furyGenerated, appliesVulnerable, message) = _berserkrService.ExecuteRecklessAssault(character, rank: 1);

        // Assert
        Assert.That(furyGenerated, Is.GreaterThan(10), "Should generate significant Fury");
        Assert.That(appliesVulnerable, Is.True, "Should apply Vulnerable status");
        Assert.That(damage, Is.GreaterThan(0), "Should deal damage");
    }

    [Test]
    public void HemorrhagingStrike_AppliesBleedingEffect()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CharacterID = 1;
        _furyService.InitializeFury(character.CharacterID);

        // Act
        var (immediateDamage, bleedDiceCount, bleedDuration, message) = _berserkrService.ExecuteHemorrhagingStrike(character, rank: 1);

        // Assert
        Assert.That(immediateDamage, Is.GreaterThan(0), "Should deal immediate damage");
        Assert.That(bleedDiceCount, Is.EqualTo(3), "Rank 1 should apply 3d6 bleed");
        Assert.That(bleedDuration, Is.EqualTo(3), "Rank 1 bleed lasts 3 turns");
        Assert.That(message, Does.Contain("Bleeding"));
    }

    [Test]
    public void WhirlwindOfDestruction_HitsAllEnemies()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CharacterID = 1;
        _furyService.InitializeFury(character.CharacterID);

        // Act
        var (totalDamage, furyRefundPerKill, message) = _berserkrService.ExecuteWhirlwindOfDestruction(character, targetCount: 5, rank: 1);

        // Assert
        Assert.That(totalDamage, Is.GreaterThan(0), "Should deal damage to all enemies");
        Assert.That(message, Does.Contain("ALL"));
    }

    #endregion

    #region Trauma Economy Integration Tests

    [Test]
    public void Berserkr_SuffersWillPenalty_WhileHoldingFury()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);
        _furyService.GenerateFuryFromDamageTaken(characterId, 50); // 50 Fury

        // Act
        int penalty = _berserkrService.GetWillPenalty(characterId);

        // Assert
        Assert.That(penalty, Is.EqualTo(-2), "Should suffer -2 dice penalty to WILL while holding any Fury");
    }

    [Test]
    public void Berserkr_NoWillPenalty_WithZeroFury()
    {
        // Arrange
        int characterId = 1;
        _furyService.InitializeFury(characterId);

        // Act
        int penalty = _berserkrService.GetWillPenalty(characterId);

        // Assert
        Assert.That(penalty, Is.EqualTo(0), "Should have no penalty with 0 Fury");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestWarrior()
    {
        return new PlayerCharacter
        {
            CharacterID = 1,
            CharacterName = "Test Berserkr",
            ClassName = "Warrior",
            MIGHT = 5,
            FINESSE = 3,
            WITS = 3,
            WILL = 2,
            STURDINESS = 4,
            CurrentHP = 100,
            MaxHP = 100,
            CurrentStamina = 100,
            MaxStamina = 100,
            CurrentLegend = 5,
            ProgressionPoints = 20
        };
    }

    #endregion
}
