using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19: Unit tests for AbilityService
/// Tests ability learning, ranking, prerequisite validation
/// </summary>
[TestFixture]
public class AbilityServiceTests
{
    private string _connectionString = string.Empty;
    private AbilityService _abilityService = null!;
    private SpecializationService _specializationService = null!;
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

        _abilityService = new AbilityService(_connectionString);
        _specializationService = new SpecializationService(_connectionString);
    }

    #region Get Abilities Tests

    [Test]
    public void GetAbilitiesForSpecialization_BoneSetter_ReturnsNineAbilities()
    {
        // Arrange
        int boneSetterId = 1;

        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(boneSetterId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9)); // 3 Tier 1, 3 Tier 2, 2 Tier 3, 1 Capstone
    }

    [Test]
    public void GetAbilitiesByTier_Tier1_ReturnsThreeAbilities()
    {
        // Arrange
        int boneSetterId = 1;
        int tier1 = 1;

        // Act
        var result = _abilityService.GetAbilitiesByTier(boneSetterId, tier1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetAbilitiesByTier_Tier4Capstone_ReturnsOneAbility()
    {
        // Arrange
        int boneSetterId = 1;
        int tier4 = 4;

        // Act
        var result = _abilityService.GetAbilitiesByTier(boneSetterId, tier4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(1)); // Miracle Worker
    }

    [Test]
    public void GetAbility_ValidId_ReturnsAbility()
    {
        // Arrange
        int fieldMedicId = 101; // Field Medic I

        // Act
        var result = _abilityService.GetAbility(fieldMedicId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Name, Is.EqualTo("Field Medic I"));
    }

    #endregion

    #region Learn Ability Tests

    [Test]
    public void LearnAbility_Tier1WithSpecUnlocked_LearnsSuccessfully()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;

        // Unlock specialization first
        _specializationService.UnlockSpecialization(character, 1); // BoneSetter

        // Give more PP for abilities (Tier 1 abilities cost 0 PP in seed data)
        character.ProgressionPoints = 10;

        int mendWoundId = 102; // Tier 1 ability

        // Act
        var result = _abilityService.LearnAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Does.Contain("learned"));
    }

    [Test]
    public void LearnAbility_WithoutSpecializationUnlocked_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;

        int mendWoundId = 102;

        // Act
        var result = _abilityService.LearnAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("unlock"));
    }

    [Test]
    public void LearnAbility_Tier2WithoutEnoughPPInTree_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        // Unlock specialization
        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 10;

        int anatomicalInsightId = 104; // Tier 2 ability (requires 8 PP in tree)

        // Act
        var result = _abilityService.LearnAbility(character, anatomicalInsightId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Prerequisites not met"));
    }

    [Test]
    public void LearnAbility_InsufficientPP_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;

        // Unlock specialization
        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 2; // Not enough for Tier 2 ability (costs 4)

        // Manually update PP in tree to bypass prerequisite check
        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), 1, 8);

        int anatomicalInsightId = 104; // Tier 2 ability

        // Act
        var result = _abilityService.LearnAbility(character, anatomicalInsightId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Requires 4 PP"));
    }

    [Test]
    public void LearnAbility_AlreadyLearned_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 10;

        int mendWoundId = 102;

        // Learn first time
        _abilityService.LearnAbility(character, mendWoundId);

        // Act - try to learn again
        var result = _abilityService.LearnAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("already"));
    }

    [Test]
    public void LearnAbility_Capstone_RequiresTier3Prerequisites()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 50;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 50;

        // Manually set PP in tree to bypass tier requirements
        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), 1, 24);

        int miracleWorkerId = 109; // Capstone (requires abilities 107 and 108)

        // Act
        var result = _abilityService.LearnAbility(character, miracleWorkerId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Prerequisites not met"));
    }

    #endregion

    #region Rank Up Tests

    [Test]
    public void RankUpAbility_Rank1To2_RanksSuccessfully()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 20;

        int mendWoundId = 102;

        // Learn ability first
        _abilityService.LearnAbility(character, mendWoundId);

        // Give more PP for rank up
        character.ProgressionPoints = 10;

        // Act
        var result = _abilityService.RankUpAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Does.Contain("Rank 2"));
        Assert.That(_abilityService.GetCurrentRank(character, mendWoundId), Is.EqualTo(2));
    }

    [Test]
    public void RankUpAbility_NotLearnedYet_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 10;

        int mendWoundId = 102;

        // Act - try to rank up without learning
        var result = _abilityService.RankUpAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("not learned"));
    }

    [Test]
    public void RankUpAbility_InsufficientPP_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 20;

        int mendWoundId = 102;

        // Learn ability
        _abilityService.LearnAbility(character, mendWoundId);

        // Set low PP
        character.ProgressionPoints = 2; // Needs 5

        // Act
        var result = _abilityService.RankUpAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Requires 5 PP"));
    }

    [Test]
    public void RankUpAbility_Rank2To3_WorksIfAvailable()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 30;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 30;

        int mendWoundId = 102;

        // Learn and rank to 2
        _abilityService.LearnAbility(character, mendWoundId);
        _abilityService.RankUpAbility(character, mendWoundId);

        // Give more PP
        character.ProgressionPoints = 10;

        // Act
        var result = _abilityService.RankUpAbility(character, mendWoundId);

        // Assert
        // Rank 3 has CostToRank3 = 0 in seed data, so it should fail
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("not available"));
    }

    #endregion

    #region Validation Tests

    [Test]
    public void CanLearn_ValidConditions_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 10;

        int mendWoundId = 102;

        // Act
        var result = _abilityService.CanLearn(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void CanLearn_InsufficientPP_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 2; // Not enough for Tier 2

        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), 1, 8);

        int anatomicalInsightId = 104;

        // Act
        var result = _abilityService.CanLearn(character, anatomicalInsightId);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void CanAfford_SufficientResources_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.Stamina = 100;
        character.PsychicStress = 0;

        int mendWoundId = 102; // Costs 5 Stamina

        // Act
        var result = _abilityService.CanAfford(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void CanAfford_InsufficientStamina_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.Stamina = 3; // Not enough for 5 Stamina cost

        int mendWoundId = 102;

        // Act
        var result = _abilityService.CanAfford(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Stamina"));
    }

    #endregion

    #region Query Tests

    [Test]
    public void HasLearned_LearnedAbility_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 10;

        int mendWoundId = 102;

        _abilityService.LearnAbility(character, mendWoundId);

        // Act
        var hasLearned = _abilityService.HasLearned(character, mendWoundId);

        // Assert
        Assert.That(hasLearned, Is.True);
    }

    [Test]
    public void HasLearned_NotLearned_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        int mendWoundId = 102;

        // Act
        var hasLearned = _abilityService.HasLearned(character, mendWoundId);

        // Assert
        Assert.That(hasLearned, Is.False);
    }

    [Test]
    public void GetLearnedAbilities_MultipleAbilities_ReturnsAll()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 30;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 20;

        // Learn multiple abilities
        _abilityService.LearnAbility(character, 102); // Mend Wound
        _abilityService.LearnAbility(character, 103); // Apply Tourniquet

        // Act
        var learned = _abilityService.GetLearnedAbilities(character);

        // Assert
        Assert.That(learned.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetCurrentRank_NewlyLearned_ReturnsRank1()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _specializationService.UnlockSpecialization(character, 1);

        character.ProgressionPoints = 10;

        int mendWoundId = 102;

        _abilityService.LearnAbility(character, mendWoundId);

        // Act
        var rank = _abilityService.GetCurrentRank(character, mendWoundId);

        // Assert
        Assert.That(rank, Is.EqualTo(1));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestCharacter(CharacterClass characterClass)
    {
        return new PlayerCharacter
        {
            Name = $"TestCharacter_{Guid.NewGuid()}",
            Class = characterClass,
            Specialization = Specialization.None,
            ProgressionPoints = 0,
            CurrentLegend = 1,
            Corruption = 0,
            HP = 50,
            MaxHP = 50,
            Stamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Attributes = new Attributes
            {
                Might = 3,
                Finesse = 3,
                Wits = 3,
                Will = 3,
                Sturdiness = 3
            }
        };
    }

    #endregion
}
