using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19: Integration tests for full specialization workflow
/// Tests complete user flows: unlock → learn → rank up → validate
/// </summary>
[TestFixture]
public class SpecializationIntegrationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private SpecializationValidator _validator = null!;
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
        _validator = new SpecializationValidator(_connectionString);
    }

    #region Full Workflow Tests

    [Test]
    public void FullWorkflow_UnlockSpecialization_LearnAllAbilities_Success()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100; // Plenty of PP

        int boneSetterId = 1;

        // Act & Assert - Step 1: Unlock Specialization
        var unlockResult = _specializationService.UnlockSpecialization(character, boneSetterId);
        Assert.That(unlockResult.Success, Is.True, "Should unlock specialization");
        Assert.That(character.ProgressionPoints, Is.EqualTo(97)); // 100 - 3 = 97

        // Act & Assert - Step 2: Learn all Tier 1 abilities (0 PP each)
        var tier1Abilities = _abilityService.GetAbilitiesByTier(boneSetterId, 1).Abilities!;
        foreach (var ability in tier1Abilities)
        {
            var learnResult = _abilityService.LearnAbility(character, ability.AbilityID);
            Assert.That(learnResult.Success, Is.True, $"Should learn {ability.Name}");
        }

        // PP should still be 97 (Tier 1 is free)
        Assert.That(character.ProgressionPoints, Is.EqualTo(97));

        // Act & Assert - Step 3: Learn Tier 2 abilities (4 PP each, requires 8 PP in tree)
        // First, spend 8 PP in tree to unlock Tier 2
        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), boneSetterId, 8);

        character.ProgressionPoints = 100; // Reset for clarity

        var tier2Abilities = _abilityService.GetAbilitiesByTier(boneSetterId, 2).Abilities!;
        foreach (var ability in tier2Abilities)
        {
            var learnResult = _abilityService.LearnAbility(character, ability.AbilityID);
            Assert.That(learnResult.Success, Is.True, $"Should learn {ability.Name}");
        }

        // Should have spent 12 PP (3 × 4)
        Assert.That(character.ProgressionPoints, Is.EqualTo(88)); // 100 - 12 = 88

        // Act & Assert - Step 4: Learn Tier 3 abilities (5 PP each, requires 16 PP in tree)
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), boneSetterId, 8); // Add 8 more (total 20)

        var tier3Abilities = _abilityService.GetAbilitiesByTier(boneSetterId, 3).Abilities!;
        foreach (var ability in tier3Abilities)
        {
            var learnResult = _abilityService.LearnAbility(character, ability.AbilityID);
            Assert.That(learnResult.Success, Is.True, $"Should learn {ability.Name}");
        }

        // Should have spent 10 PP (2 × 5)
        Assert.That(character.ProgressionPoints, Is.EqualTo(78)); // 88 - 10 = 78

        // Act & Assert - Step 5: Learn Capstone (6 PP, requires Tier 3 prerequisites)
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), boneSetterId, 6); // Add 6 more (total 26)

        var capstoneAbility = _abilityService.GetAbilitiesByTier(boneSetterId, 4).Abilities!.First();
        var capstoneResult = _abilityService.LearnAbility(character, capstoneAbility.AbilityID);
        Assert.That(capstoneResult.Success, Is.True, "Should learn capstone");

        // Should have spent 6 PP
        Assert.That(character.ProgressionPoints, Is.EqualTo(72)); // 78 - 6 = 72

        // Verify all abilities learned
        var learnedAbilities = _abilityService.GetLearnedAbilities(character);
        Assert.That(learnedAbilities.Count, Is.EqualTo(9), "Should have learned all 9 abilities");
    }

    [Test]
    public void FullWorkflow_RankProgression_FromRank1To3_Success()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100;

        int boneSetterId = 1;
        int mendWoundId = 102; // Mend Wound - Tier 1 ability

        // Unlock and learn ability
        _specializationService.UnlockSpecialization(character, boneSetterId);
        _abilityService.LearnAbility(character, mendWoundId);

        // Get ability data to check rank costs
        var ability = _abilityService.GetAbility(mendWoundId).Ability!;

        // Act & Assert - Rank 1 → 2
        var initialRank = _abilityService.GetCurrentRank(character, mendWoundId);
        Assert.That(initialRank, Is.EqualTo(1));

        character.ProgressionPoints = 20; // Ensure enough PP

        var rankUp1Result = _abilityService.RankUpAbility(character, mendWoundId);
        Assert.That(rankUp1Result.Success, Is.True, "Should rank up to Rank 2");

        var rank2 = _abilityService.GetCurrentRank(character, mendWoundId);
        Assert.That(rank2, Is.EqualTo(2));
        Assert.That(character.ProgressionPoints, Is.EqualTo(15)); // 20 - 5 = 15

        // Act & Assert - Rank 2 → 3 (if available)
        if (ability.CostToRank3 > 0)
        {
            character.ProgressionPoints = 20;

            var rankUp2Result = _abilityService.RankUpAbility(character, mendWoundId);
            Assert.That(rankUp2Result.Success, Is.True, "Should rank up to Rank 3");

            var rank3 = _abilityService.GetCurrentRank(character, mendWoundId);
            Assert.That(rank3, Is.EqualTo(3));
        }
        else
        {
            // Rank 3 not available (CostToRank3 = 0)
            var rankUp2Result = _abilityService.RankUpAbility(character, mendWoundId);
            Assert.That(rankUp2Result.Success, Is.False, "Should not rank up (Rank 3 not available)");
        }
    }

    [Test]
    public void FullWorkflow_MultipleSpecializations_CanUnlockBoth_Success()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100;

        int boneSetterId = 1;
        int jotunReaderId = 2;

        // Act - Unlock first specialization
        var unlock1Result = _specializationService.UnlockSpecialization(character, boneSetterId);
        Assert.That(unlock1Result.Success, Is.True);

        // Act - Unlock second specialization
        var unlock2Result = _specializationService.UnlockSpecialization(character, jotunReaderId);
        Assert.That(unlock2Result.Success, Is.True);

        // Assert - Both are unlocked
        Assert.That(_specializationService.HasUnlocked(character, boneSetterId), Is.True);
        Assert.That(_specializationService.HasUnlocked(character, jotunReaderId), Is.True);

        // Assert - PP spent correctly
        Assert.That(character.ProgressionPoints, Is.EqualTo(94)); // 100 - 3 - 3 = 94
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Validation_AllSeededSpecializations_PassValidation()
    {
        // Act
        var validationResult = _validator.ValidateAllSpecializations();

        // Assert
        Assert.That(validationResult.IsValid, Is.True,
            $"Validation failed with errors:\n{validationResult.GetSummary()}");

        // Log any warnings
        if (validationResult.Warnings.Count > 0)
        {
            TestContext.WriteLine($"Validation warnings:\n{validationResult.GetSummary()}");
        }
    }

    [Test]
    public void Validation_BoneSetter_PassesAllRules()
    {
        // Act
        var result = _validator.ValidateSpecialization(1); // BoneSetter

        // Assert
        Assert.That(result.IsValid, Is.True,
            $"BoneSetter validation failed:\n{result.GetSummary()}");
    }

    [Test]
    public void Validation_JotunReader_PassesAllRules()
    {
        // Act
        var result = _validator.ValidateSpecialization(2); // JotunReader

        // Assert
        Assert.That(result.IsValid, Is.True,
            $"JotunReader validation failed:\n{result.GetSummary()}");
    }

    [Test]
    public void Validation_Skald_PassesAllRules()
    {
        // Act
        var result = _validator.ValidateSpecialization(3); // Skald

        // Assert
        Assert.That(result.IsValid, Is.True,
            $"Skald validation failed:\n{result.GetSummary()}");
    }

    [Test]
    public void Validation_GenerateReport_ContainsAllSpecializations()
    {
        // Act
        var report = _validator.GenerateValidationReport();

        // Assert
        Assert.That(report, Does.Contain("Total Specializations: 3"));
        Assert.That(report, Does.Contain("Bone-Setter"));
        Assert.That(report, Does.Contain("Jötun-Reader"));
        Assert.That(report, Does.Contain("Skald"));

        TestContext.WriteLine(report);
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void EdgeCase_LearnCapstoneWithoutPrerequisites_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100;

        int boneSetterId = 1;
        int miracleWorkerId = 109; // Capstone ability

        // Unlock specialization and manually set PP in tree
        _specializationService.UnlockSpecialization(character, boneSetterId);

        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), boneSetterId, 24); // Meets PP requirement

        character.ProgressionPoints = 50;

        // Act - Try to learn capstone without learning Tier 3 prerequisites
        var result = _abilityService.LearnAbility(character, miracleWorkerId);

        // Assert
        Assert.That(result.Success, Is.False, "Should not allow learning capstone without prerequisites");
        Assert.That(result.Message, Does.Contain("Prerequisites not met"));
    }

    [Test]
    public void EdgeCase_LearnAbilityFromUnlockedSpecialization_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100;

        int mendWoundId = 102; // BoneSetter ability

        // Act - Try to learn without unlocking specialization
        var result = _abilityService.LearnAbility(character, mendWoundId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("unlock"));
    }

    [Test]
    public void EdgeCase_RankUpAtMaxRank_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100;

        int boneSetterId = 1;
        int fieldMedicId = 101; // Passive with MaxRank = 1

        // Unlock and learn
        _specializationService.UnlockSpecialization(character, boneSetterId);
        _abilityService.LearnAbility(character, fieldMedicId);

        // Act - Try to rank up (already at max rank)
        var result = _abilityService.RankUpAbility(character, fieldMedicId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("maximum rank"));
    }

    [Test]
    public void EdgeCase_UnlockWrongArchetypeSpecialization_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Warrior);
        character.ProgressionPoints = 100;

        int boneSetterId = 1; // Adept specialization

        // Act
        var result = _specializationService.UnlockSpecialization(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("archetype"));
    }

    [Test]
    public void EdgeCase_PPSpentInTreeTracking_AccurateAfterMultipleLearns()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 100;

        int boneSetterId = 1;

        // Unlock specialization
        _specializationService.UnlockSpecialization(character, boneSetterId);

        // Learn several abilities with different costs
        var repo = new SpecializationRepository(_connectionString);
        repo.UpdatePPSpentInTree(character.Name.GetHashCode(), boneSetterId, 8); // Unlock Tier 2

        character.ProgressionPoints = 50;

        var anatomicalInsightId = 104; // Tier 2, 4 PP
        var administerAntidoteId = 105; // Tier 2, 4 PP

        _abilityService.LearnAbility(character, anatomicalInsightId);
        _abilityService.LearnAbility(character, administerAntidoteId);

        // Act
        var ppSpent = _specializationService.GetPPSpentInTree(character, boneSetterId);

        // Assert
        Assert.That(ppSpent, Is.EqualTo(16)); // 8 (initial) + 4 + 4 = 16
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
