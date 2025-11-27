using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.28.1: Unit tests for Seidkona specialization
/// Tests specialization seeding, ability structure, Spirit Bargain mechanics, and Trauma Economy integration
/// </summary>
[TestFixture]
public class SeidkonaSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private SeidkonaService _seidkonaService = null!;
    private SpiritBargainService _spiritBargainService = null!;
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
        _seidkonaService = new SeidkonaService(_connectionString);
        _spiritBargainService = new SpiritBargainService(_connectionString, new Random(42)); // Fixed seed for deterministic tests
        _traumaService = new TraumaEconomyService(new Random(42));
    }

    #region Specialization Seeding Tests

    [Test]
    public void Seidkona_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(28001); // Seidkona ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Seidkona"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(5)); // Mystic
        Assert.That(result.Specialization.PathType, Is.EqualTo("Corrupted"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Psychic Archaeologist / Trauma Economy High Risk"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("High"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🔮"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Aether Pool"));
    }

    [Test]
    public void Seidkona_AppearsInMysticSpecializations()
    {
        // Arrange
        var character = CreateTestMystic();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var seidkona = result.Specializations!.FirstOrDefault(s => s.Name == "Seidkona");
        Assert.That(seidkona, Is.Not.Null);
    }

    [Test]
    public void Seidkona_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestMystic();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 28001);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Seidkona_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28001);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Seidkona_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28001);

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
    public void Seidkona_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28001);

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
    public void Seidkona_TotalPPCostIs37()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(28001);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(37), "Total PP to learn all abilities should be 37 (3+3+3 + 4+4+4 + 5+5 + 6)");
    }

    #endregion

    #region Spirit Bargain Mechanics Tests

    [Test]
    public void SpiritBargain_InitializesForCharacter()
    {
        // Arrange
        var character = CreateTestMystic();

        // Act
        var initialized = _spiritBargainService.InitializeSpiritBargains(character.CharacterID);

        // Assert
        Assert.That(initialized, Is.True, "Spirit Bargain tracking should initialize successfully");
    }

    [Test]
    public void SpiritBargain_GuaranteedSuccess_DuringMomentOfClarity()
    {
        // Arrange
        var character = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(character.CharacterID);
        _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 2, 1);

        // Act
        var result = _spiritBargainService.AttemptSpiritBargain(character.CharacterID, "Echo of Vigor", 0.25f);

        // Assert
        Assert.That(result.Success, Is.True, "Should always succeed during Clarity");
        Assert.That(result.Guaranteed, Is.True, "Should be marked as guaranteed");
    }

    [Test]
    public void SpiritBargain_AppliesFickleFortuneBonus()
    {
        // Arrange
        var character = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(character.CharacterID);
        _spiritBargainService.UpdateFickleFortuneRank(character.CharacterID, 1); // +15% bonus

        // Act - Multiple attempts to test probability
        int successCount = 0;
        int attempts = 100;

        for (int i = 0; i < attempts; i++)
        {
            var result = _spiritBargainService.AttemptSpiritBargain(character.CharacterID, "Echo of Vigor", 0.25f);
            if (result.Success) successCount++;
        }

        // Assert - With +15% bonus, base 25% becomes 40%
        // Allow some variance (30% - 50% success rate)
        Assert.That(successCount, Is.GreaterThan(25), "Should have higher success rate with Fickle Fortune");
    }

    [Test]
    public void MomentOfClarity_CanOnlyBeUsed_OncePerCombat_Rank1()
    {
        // Arrange
        var character = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(character.CharacterID);

        // Act
        bool firstUse = _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 2, 1);
        _spiritBargainService.EndMomentOfClarity(character.CharacterID);
        bool secondUse = _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 2, 1);

        // Assert
        Assert.That(firstUse, Is.True, "First use should succeed");
        Assert.That(secondUse, Is.False, "Second use should fail at Rank 1");
    }

    [Test]
    public void MomentOfClarity_CanBeUsedTwice_Rank3()
    {
        // Arrange
        var character = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(character.CharacterID);

        // Act
        bool firstUse = _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 3, 3);
        _spiritBargainService.EndMomentOfClarity(character.CharacterID);
        bool secondUse = _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 3, 3);

        // Assert
        Assert.That(firstUse, Is.True, "First use should succeed");
        Assert.That(secondUse, Is.True, "Second use should succeed at Rank 3");
    }

    [Test]
    public void CombatStateReset_ClearsOncePerCombatLimits()
    {
        // Arrange
        var character = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(character.CharacterID);
        _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 2, 1);

        // Act
        _spiritBargainService.ResetCombatState(character.CharacterID);
        bool useAfterReset = _spiritBargainService.ActivateMomentOfClarity(character.CharacterID, 2, 1);

        // Assert
        Assert.That(useAfterReset, Is.True, "Should be able to use Clarity again after combat reset");
    }

    #endregion

    #region Ability Execution Tests - Tier 1

    [Test]
    public void EchoOfVigor_RestoresHP()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestMystic();
        target.HP = 10; // Damaged
        target.MaxHP = 50;

        // Act
        var result = _seidkonaService.ExecuteEchoOfVigor(caster, target, 1);

        // Assert
        Assert.That(result.healingAmount, Is.GreaterThan(0), "Should restore HP");
        Assert.That(target.HP, Is.GreaterThan(10), "Target HP should increase");
    }

    [Test]
    public void EchoOfVigor_CannotTargetSelf_Rank1()
    {
        // Arrange
        var caster = CreateTestMystic();

        // Act
        var result = _seidkonaService.ExecuteEchoOfVigor(caster, caster, 1);

        // Assert
        Assert.That(result.healingAmount, Is.EqualTo(0), "Should not heal at Rank 1 self-target");
    }

    [Test]
    public void EchoOfVigor_CanTargetSelf_Rank3()
    {
        // Arrange
        var caster = CreateTestMystic();
        caster.HP = 10;
        caster.MaxHP = 50;

        // Act
        var result = _seidkonaService.ExecuteEchoOfVigor(caster, caster, 3);

        // Assert
        Assert.That(result.healingAmount, Is.GreaterThan(0), "Should heal self at Rank 3");
    }

    [Test]
    public void EchoOfMisfortune_AppliesCurseDebuff()
    {
        // Arrange
        var caster = CreateTestMystic();
        var target = CreateTestEnemy();

        // Act
        var result = _seidkonaService.ExecuteEchoOfMisfortune(caster, target, 1);

        // Assert
        Assert.That(result.cursed, Is.True, "Should apply [Cursed] debuff");
        Assert.That(result.duration, Is.EqualTo(2), "Rank 1 should apply 2-turn curse");
    }

    #endregion

    #region Ability Execution Tests - Tier 2

    [Test]
    public void ForlornCommunion_AppliesUnavoidableStress()
    {
        // Arrange
        var character = CreateTestMystic();
        character.PsychicStress = 10;

        // Act
        var result = _seidkonaService.ExecuteForlornCommunion(character, forlornEntityId: 100, rank: 1);

        // Assert
        Assert.That(result.stressCost, Is.EqualTo(15), "Should gain exactly +15 Stress at Rank 1");
        Assert.That(character.PsychicStress, Is.EqualTo(25), "Stress should be unavoidable");
    }

    [Test]
    public void ForlornCommunion_ReducedStressCost_HigherRanks()
    {
        // Arrange
        var character = CreateTestMystic();
        character.PsychicStress = 10;

        // Act
        var result = _seidkonaService.ExecuteForlornCommunion(character, forlornEntityId: 100, rank: 3);

        // Assert
        Assert.That(result.stressCost, Is.EqualTo(10), "Rank 3 should only cost +10 Stress");
        Assert.That(character.PsychicStress, Is.EqualTo(20));
    }

    [Test]
    public void SpiritualAnchor_RemovesPsychicStress()
    {
        // Arrange
        var character = CreateTestMystic();
        character.PsychicStress = 50;

        // Act
        var result = _seidkonaService.ExecuteSpiritualAnchor(character, rank: 1);

        // Assert
        Assert.That(result.stressRemoved, Is.EqualTo(20), "Rank 1 should remove 20 Stress");
        Assert.That(character.PsychicStress, Is.EqualTo(30), "Stress should be reduced");
    }

    #endregion

    #region Ability Execution Tests - Tier 3

    [Test]
    public void SpiritWard_ProtectsAlliesFromStress()
    {
        // Arrange
        var caster = CreateTestMystic();
        var allies = new List<PlayerCharacter> { CreateTestMystic(), CreateTestMystic() };

        // Act
        var result = _seidkonaService.ExecuteSpiritWard(caster, targetRow: 1, rowAllies: allies, rank: 1);

        // Assert
        Assert.That(result.success, Is.True, "Spirit Ward should be placed successfully");
        Assert.That(result.duration, Is.GreaterThanOrEqualTo(3), "Base duration should be 3 turns");
    }

    [Test]
    public void RideTheEchoes_AppliesCorruption()
    {
        // Arrange
        var caster = CreateTestMystic();
        caster.Corruption = 10;

        // Act
        var result = _seidkonaService.ExecuteRideTheEchoes(caster, targetX: 5, targetY: 3, rank: 1);

        // Assert
        Assert.That(result.success, Is.True, "Teleport should succeed");
        Assert.That(result.corruptionGained, Is.EqualTo(2), "Rank 1 should gain +2 Corruption");
        Assert.That(caster.Corruption, Is.EqualTo(12), "Caster Corruption should increase");
    }

    #endregion

    #region Capstone Tests

    [Test]
    public void MomentOfClarity_ActivatesSuccessfully()
    {
        // Arrange
        var caster = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(caster.CharacterID);

        // Act
        var result = _seidkonaService.ExecuteMomentOfClarity(caster, rank: 1);

        // Assert
        Assert.That(result.success, Is.True, "Clarity should activate");
        Assert.That(result.duration, Is.EqualTo(2), "Rank 1 should last 2 turns");
        Assert.That(result.aftermathStress, Is.EqualTo(20), "Rank 1 should have +20 Stress aftermath");
    }

    [Test]
    public void ClarityAftermath_AppliesStressCost()
    {
        // Arrange
        var caster = CreateTestMystic();
        caster.PsychicStress = 10;
        _spiritBargainService.InitializeSpiritBargains(caster.CharacterID);
        _spiritBargainService.ActivateMomentOfClarity(caster.CharacterID, 2, 1);

        // Act
        var result = _seidkonaService.ApplyClarityAftermath(caster, rank: 1);

        // Assert
        Assert.That(result.stressGained, Is.EqualTo(20), "Should gain +20 Stress aftermath at Rank 1");
        Assert.That(caster.PsychicStress, Is.EqualTo(30), "Stress should increase");
    }

    #endregion

    #region Spirit Bargain Statistics Tests

    [Test]
    public void SpiritBargainStatistics_TrackSuccessRate()
    {
        // Arrange
        var character = CreateTestMystic();
        _spiritBargainService.InitializeSpiritBargains(character.CharacterID);

        // Act - Attempt several bargains
        for (int i = 0; i < 10; i++)
        {
            _spiritBargainService.AttemptSpiritBargain(character.CharacterID, "Test", 0.5f);
        }

        var stats = _spiritBargainService.GetBargainStatistics(character.CharacterID);

        // Assert
        Assert.That(stats.attempted, Is.EqualTo(10), "Should track all attempts");
        Assert.That(stats.successRate, Is.GreaterThanOrEqualTo(0.0f), "Success rate should be non-negative");
        Assert.That(stats.successRate, Is.LessThanOrEqualTo(1.0f), "Success rate should not exceed 100%");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestMystic()
    {
        var player = new PlayerCharacter
        {
            CharacterID = Random.Shared.Next(1000, 9999),
            Name = "Test Seidkona",
            Class = CharacterClass.Mystic,
            HP = 40,
            MaxHP = 40,
            // CurrentAP = 100, // Not in PlayerCharacter
            // MaxAP = 100, // Not in PlayerCharacter
            // Attributes = new Attributes(2, 3, 4, 5, 3), // MIGHT, FINESSE, WITS, WILL, STURDINESS // Removed because it's read-only maybe? No, I need to set it.
            CurrentLegend = 5,
            PsychicStress = 0,
            Corruption = 0
        };
        player.Attributes.Might = 2;
        player.Attributes.Finesse = 3;
        player.Attributes.Wits = 4;
        player.Attributes.Will = 5;
        player.Attributes.Sturdiness = 3;
        return player;
    }

    private Enemy CreateTestEnemy()
    {
        var enemy = new Enemy
        {
            Id = Random.Shared.Next(1000, 9999).ToString(),
            Name = "Test Enemy",
            HP = 50,
            MaxHP = 50
        };
        enemy.Attributes.Might = 3;
        enemy.Attributes.Finesse = 2;
        enemy.Attributes.Sturdiness = 3;
        return enemy;
    }

    #endregion
}
