using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using RuneAndRust.Core.Quests;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.27.1: Unit tests for Skald specialization
/// Tests specialization seeding, ability structure, performance mechanics, and Trauma Economy integration
/// </summary>
[TestFixture]
public class SkaldSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private SkaldService _skaldService = null!;
    private PerformanceStateService _performanceService = null!;
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
        _skaldService = new SkaldService(_connectionString);
        _performanceService = new PerformanceStateService(_connectionString);

        // Create performance tracking table
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Characters_Performances (
                    character_id INTEGER PRIMARY KEY,
                    is_performing BOOLEAN NOT NULL DEFAULT 0,
                    current_performance_ability_id INTEGER,
                    performance_duration_remaining INTEGER,
                    performance_rank INTEGER DEFAULT 1,
                    can_move BOOLEAN NOT NULL DEFAULT 1,
                    can_use_items BOOLEAN NOT NULL DEFAULT 1,
                    can_use_standard_action BOOLEAN NOT NULL DEFAULT 0,
                    interrupted_this_combat BOOLEAN NOT NULL DEFAULT 0,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Skald_Einherjar_Usage (
                    character_id INTEGER,
                    combat_id INTEGER,
                    used_at DATETIME,
                    PRIMARY KEY (character_id, combat_id)
                );

                CREATE TABLE IF NOT EXISTS Skald_Einherjar_Affected (
                    skald_character_id INTEGER PRIMARY KEY,
                    affected_ally_ids TEXT,
                    stress_cost INTEGER
                );";
            cmd.ExecuteNonQuery();
        }
    }

    #region Specialization Seeding Tests

    [Test]
    public void Skald_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(27001); // Skald ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Skald"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(2)); // Adept
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Performance Buffer / Trauma Economy Support"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Low"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("📜"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina"));
    }

    [Test]
    public void Skald_AppearsInAdeptSpecializations()
    {
        // Arrange
        var character = CreateTestAdept();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var skald = result.Specializations!.FirstOrDefault(s => s.Name == "Skald");
        Assert.That(skald, Is.Not.Null);
    }

    [Test]
    public void Skald_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestAdept();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 27001);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Skald_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(27001);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Skald_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(27001);

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
    public void Skald_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(27001);

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
    public void Skald_TotalPPCostIs37()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(27001);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(37), "Total PP to learn all abilities should be 37 (3+3+3 + 4+4+4 + 5+5 + 6)");
    }

    #endregion

    #region Performance State Tests

    [Test]
    public void StartPerformance_CreatesActivePerformance()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed

        // Act
        bool started = _performanceService.StartPerformance(skald.CharacterID, 27002, 1, skald.Attributes.Will, 0);

        // Assert
        Assert.That(started, Is.True, "Performance should start successfully");
        Assert.That(_performanceService.IsPerforming(skald.CharacterID), Is.True, "Character should be performing");
    }

    [Test]
    public void StartPerformance_FailsIfAlreadyPerforming()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        _performanceService.StartPerformance(skald.CharacterID, 27002, 1, skald.Attributes.Will, 0);

        // Act
        bool secondStart = _performanceService.StartPerformance(skald.CharacterID, 27003, 1, skald.Attributes.Will, 0);

        // Assert
        Assert.That(secondStart, Is.False, "Cannot start second performance while already performing");
    }

    [Test]
    public void InterruptPerformance_EndsActivePerformance()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        _performanceService.StartPerformance(skald.CharacterID, 27002, 1, skald.Attributes.Will, 0);

        // Act
        bool interrupted = _performanceService.InterruptPerformance(skald.CharacterID, "[Stunned]");

        // Assert
        Assert.That(interrupted, Is.True, "Performance should be interrupted");
        Assert.That(_performanceService.IsPerforming(skald.CharacterID), Is.False, "Character should no longer be performing");
    }

    [Test]
    public void PerformanceDuration_CalculatedFromWILL()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed

        // Act
        _performanceService.StartPerformance(skald.CharacterID, 27002, 1, skald.Attributes.Will, 0);
        var performance = _performanceService.GetCurrentPerformance(skald.CharacterID);

        // Assert
        Assert.That(performance, Is.Not.Null);
        Assert.That(performance!.Value.durationRemaining, Is.EqualTo(5), "Duration should equal WILL score");
    }

    [Test]
    public void EnduringPerformance_IncreasesPerformanceDuration()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        int enduringRank = 2; // +3 rounds

        // Act
        _performanceService.StartPerformance(skald.CharacterID, 27002, 1, skald.Attributes.Will, enduringRank);
        var performance = _performanceService.GetCurrentPerformance(skald.CharacterID);

        // Assert
        Assert.That(performance, Is.Not.Null);
        Assert.That(performance!.Value.durationRemaining, Is.EqualTo(8), "Duration should be WILL (5) + Enduring bonus (3)");
    }

    #endregion

    #region Tier 1 Ability Tests

    [Test]
    public void SagaOfCourage_GrantsFearImmunityAndStressResistance()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var allies = new List<PlayerCharacter>
        {
            CreateTestAdept(),
            CreateTestAdept(),
            CreateTestAdept()
        };

        // Act - Test Rank 1
        var rank1Result = _skaldService.ExecuteSagaOfCourage(skald, allies, 1, 0);

        // Assert
        Assert.That(rank1Result.success, Is.True);
        Assert.That(rank1Result.duration, Is.EqualTo(5), "Duration should equal WILL");
        Assert.That(rank1Result.message, Does.Contain("+1 dice vs Psychic Stress"), "Should grant +1 die at Rank 1");
        Assert.That(_performanceService.IsPerforming(skald.CharacterID), Is.True, "Skald should be performing");
    }

    [Test]
    public void DirgeOfDefeat_DebuffsIntelligentEnemies()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var enemies = new List<Enemy>
        {
            CreateTestEnemy(),
            CreateTestEnemy()
        };

        // Act - Test Rank 2
        var rank2Result = _skaldService.ExecuteDirgeOfDefeat(skald, enemies, 2, 0);

        // Assert
        Assert.That(rank2Result.success, Is.True);
        Assert.That(rank2Result.duration, Is.EqualTo(5), "Duration should equal WILL");
        Assert.That(rank2Result.message, Does.Contain("-2 dice"), "Should apply -2 dice penalty at Rank 2");
    }

    #endregion

    #region Tier 2 Ability Tests

    [Test]
    public void RousingVerse_RestoresStamina()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var ally = CreateTestAdept();
        ally.Stamina = 50;
        ally.MaxStamina = 100;

        // Act - Test Rank 1
        var rank1Result = _skaldService.ExecuteRousingVerse(skald, ally, 1);

        // Assert
        int expectedRestore = 15 + (5 * 2); // 15 + (WILL × 2) = 25
        Assert.That(rank1Result.staminaRestored, Is.EqualTo(expectedRestore), "Should restore 25 Stamina at Rank 1");
        Assert.That(ally.Stamina, Is.EqualTo(75), "Ally Stamina should increase");
    }

    [Test]
    public void RousingVerse_Rank3_RemovesExhausted()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var ally = CreateTestAdept();

        // Act - Test Rank 3
        var rank3Result = _skaldService.ExecuteRousingVerse(skald, ally, 3);

        // Assert
        Assert.That(rank3Result.exhaustedRemoved, Is.True, "Rank 3 should remove Exhausted status");
    }

    [Test]
    public void SongOfSilence_AppliesSilencedStatus()
    {
        // Arrange
        var skald = CreateTestAdept();
        var enemy = CreateTestEnemy();

        // Act - Test Rank 1
        var rank1Result = _skaldService.ExecuteSongOfSilence(skald, enemy, 1);

        // Assert
        Assert.That(rank1Result.success, Is.True);
        Assert.That(rank1Result.duration, Is.EqualTo(2), "Rank 1 should silence for 2 rounds");
        Assert.That(rank1Result.damage, Is.EqualTo(0), "Rank 1 should not deal damage");
    }

    [Test]
    public void SongOfSilence_Rank3_DealsPsychicDamage()
    {
        // Arrange
        var skald = CreateTestAdept();
        var enemy = CreateTestEnemy();

        // Act - Test Rank 3
        var rank3Result = _skaldService.ExecuteSongOfSilence(skald, enemy, 3);

        // Assert
        Assert.That(rank3Result.duration, Is.EqualTo(3), "Rank 3 should silence for 3 rounds");
        Assert.That(rank3Result.damage, Is.GreaterThan(0), "Rank 3 should deal 2d6 Psychic damage");
    }

    #endregion

    #region Tier 3 Ability Tests

    [Test]
    public void LayOfTheIronWall_GrantsSoakToFrontRow()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var frontRowAllies = new List<PlayerCharacter>
        {
            CreateTestAdept(),
            CreateTestAdept()
        };

        // Act - Test Rank 2
        var rank2Result = _skaldService.ExecuteLayOfTheIronWall(skald, frontRowAllies, 2, 0);

        // Assert
        Assert.That(rank2Result.success, Is.True);
        Assert.That(rank2Result.soakBonus, Is.EqualTo(3), "Rank 2 should grant +3 Soak");
        Assert.That(rank2Result.duration, Is.EqualTo(5), "Duration should equal WILL");
    }

    [Test]
    public void HeartOfClan_ProvidesBonusDice()
    {
        // Test Rank 1
        var rank1Bonus = _skaldService.GetHeartOfClanBonus(1);
        Assert.That(rank1Bonus, Is.EqualTo(1), "Rank 1 should provide +1 die");

        // Test Rank 2
        var rank2Bonus = _skaldService.GetHeartOfClanBonus(2);
        Assert.That(rank2Bonus, Is.EqualTo(2), "Rank 2 should provide +2 dice");

        // Test Rank 3
        var rank3Bonus = _skaldService.GetHeartOfClanBonus(3);
        Assert.That(rank3Bonus, Is.EqualTo(2), "Rank 3 should provide +2 dice");
    }

    #endregion

    #region Capstone Ability Tests

    [Test]
    public void SagaOfEinherjar_GrantsMassiveBuffs()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var allies = new List<PlayerCharacter>
        {
            CreateTestAdept(),
            CreateTestAdept(),
            CreateTestAdept()
        };

        // Act - Test Rank 1
        var rank1Result = _skaldService.ExecuteSagaOfEinherjar(skald, allies, 1, 0);

        // Assert
        Assert.That(rank1Result.success, Is.True);
        Assert.That(rank1Result.damageBonus, Is.EqualTo(3), "Rank 1 should grant +3 damage dice");
        Assert.That(rank1Result.tempHP, Is.EqualTo(20), "Rank 1 should grant 20 temp HP");
        Assert.That(rank1Result.stressCost, Is.EqualTo(10), "Rank 1 should cost 10 Stress at end");
    }

    [Test]
    public void SagaOfEinherjar_CanOnlyBeUsedOncePerCombat()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var allies = new List<PlayerCharacter> { CreateTestAdept() };

        // Act
        var firstResult = _skaldService.ExecuteSagaOfEinherjar(skald, allies, 1, 0);

        // End first performance to allow second attempt
        _performanceService.EndPerformance(skald.CharacterID, "test");

        var secondResult = _skaldService.ExecuteSagaOfEinherjar(skald, allies, 1, 0);

        // Assert
        Assert.That(firstResult.success, Is.True, "First use should succeed");
        Assert.That(secondResult.success, Is.False, "Second use should fail (once per combat)");
        Assert.That(secondResult.message, Does.Contain("already used"), "Should indicate already used");
    }

    [Test]
    public void SagaOfEinherjar_Rank3_GrantsFearStunImmunity()
    {
        // Arrange
        var skald = CreateTestAdept();
        // skald.WILL = 5; // Removed
        var allies = new List<PlayerCharacter> { CreateTestAdept() };

        // Act - Test Rank 3
        var rank3Result = _skaldService.ExecuteSagaOfEinherjar(skald, allies, 3, 0);

        // Assert
        Assert.That(rank3Result.success, Is.True);
        Assert.That(rank3Result.damageBonus, Is.EqualTo(5), "Rank 3 should grant +5 damage dice");
        Assert.That(rank3Result.tempHP, Is.EqualTo(40), "Rank 3 should grant 40 temp HP");
        Assert.That(rank3Result.stressCost, Is.EqualTo(6), "Rank 3 should only cost 6 Stress (reduced)");
        Assert.That(rank3Result.message, Does.Contain("Fear/Stun immunity"), "Should grant Fear/Stun immunity at Rank 3");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestAdept()
    {
        var player = new PlayerCharacter
        {
            CharacterID = new Random().Next(1000, 9999),
            Name = "Test Skald",
            Class = CharacterClass.Adept,
            CurrentLegend = 5,
            ProgressionPoints = 50,
            HP = 80,
            MaxHP = 80,
            Stamina = 120,
            MaxStamina = 120,
            PsychicStress = 0,
            Corruption = 0,
            Abilities = new List<Ability>(),
            StatusEffects = new List<StatusEffect>(),
            CompletedQuests = new List<Quest>()
        };

        player.Attributes.Might = 2;
        player.Attributes.Finesse = 2;
        player.Attributes.Wits = 4;
        player.Attributes.Will = 5;
        player.Attributes.Sturdiness = 2;

        return player;
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            Id = new Random().Next(1000, 9999).ToString(),
            Name = "Test Enemy",
            HP = 50,
            MaxHP = 50,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
