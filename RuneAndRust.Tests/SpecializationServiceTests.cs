using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19: Unit tests for SpecializationService
/// Tests unlock validation, requirements checking, and specialization management
/// </summary>
[TestFixture]
public class SpecializationServiceTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _service = null!;
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

        _service = new SpecializationService(_connectionString);
    }

    #region Get Specializations Tests

    [Test]
    public void GetAvailableSpecializations_AdeptCharacter_ReturnsAdeptSpecs()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);

        // Act
        var result = _service.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);
        Assert.That(result.Specializations!.Count, Is.EqualTo(3)); // BoneSetter, JotunReader, Skald
    }

    [Test]
    public void GetAvailableSpecializations_WarriorCharacter_ReturnsEmptyList()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Warrior);

        // Act
        var result = _service.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);
        Assert.That(result.Specializations!.Count, Is.EqualTo(0)); // No Warrior specs seeded yet
    }

    [Test]
    public void GetSpecialization_ValidId_ReturnsSpecialization()
    {
        // Arrange
        int boneSetterId = 1;

        // Act
        var result = _service.GetSpecialization(boneSetterId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Bone-Setter"));
    }

    [Test]
    public void GetSpecialization_InvalidId_ReturnsFailure()
    {
        // Arrange
        int invalidId = 9999;

        // Act
        var result = _service.GetSpecialization(invalidId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Specialization, Is.Null);
    }

    #endregion

    #region Unlock Specialization Tests

    [Test]
    public void UnlockSpecialization_ValidAdeptWithPP_UnlocksSuccessfully()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10; // Enough PP
        int boneSetterId = 1;

        // Act
        var result = _service.UnlockSpecialization(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(character.ProgressionPoints, Is.EqualTo(7)); // 10 - 3 = 7
        Assert.That(result.Message, Does.Contain("unlocked"));
    }

    [Test]
    public void UnlockSpecialization_InsufficientPP_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 1; // Not enough PP (needs 3)
        int boneSetterId = 1;

        // Act
        var result = _service.UnlockSpecialization(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Requires 3 PP"));
    }

    [Test]
    public void UnlockSpecialization_WrongArchetype_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Warrior);
        character.ProgressionPoints = 10;
        int boneSetterId = 1; // Adept specialization

        // Act
        var result = _service.UnlockSpecialization(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("archetype"));
    }

    [Test]
    public void UnlockSpecialization_AlreadyUnlocked_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;
        int boneSetterId = 1;

        // Unlock first time
        _service.UnlockSpecialization(character, boneSetterId);

        // Give more PP
        character.ProgressionPoints = 10;

        // Act - try to unlock again
        var result = _service.UnlockSpecialization(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("already"));
    }

    [Test]
    public void UnlockSpecialization_InvalidId_Fails()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;
        int invalidId = 9999;

        // Act
        var result = _service.UnlockSpecialization(character, invalidId);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    #endregion

    #region Validation Tests

    [Test]
    public void CanUnlock_ValidCharacter_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;
        int boneSetterId = 1;

        // Act
        var result = _service.CanUnlock(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Can unlock"));
    }

    [Test]
    public void CanUnlock_InsufficientPP_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 0;
        int boneSetterId = 1;

        // Act
        var result = _service.CanUnlock(character, boneSetterId);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    #endregion

    #region Query Tests

    [Test]
    public void HasUnlocked_UnlockedSpecialization_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;
        int boneSetterId = 1;

        _service.UnlockSpecialization(character, boneSetterId);

        // Act
        var hasUnlocked = _service.HasUnlocked(character, boneSetterId);

        // Assert
        Assert.That(hasUnlocked, Is.True);
    }

    [Test]
    public void HasUnlocked_NotUnlocked_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        int boneSetterId = 1;

        // Act
        var hasUnlocked = _service.HasUnlocked(character, boneSetterId);

        // Assert
        Assert.That(hasUnlocked, Is.False);
    }

    [Test]
    public void GetPPSpentInTree_NoAbilitiesLearned_ReturnsZero()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 10;
        int boneSetterId = 1;

        _service.UnlockSpecialization(character, boneSetterId);

        // Act
        var ppSpent = _service.GetPPSpentInTree(character, boneSetterId);

        // Assert
        Assert.That(ppSpent, Is.EqualTo(0));
    }

    [Test]
    public void GetUnlockedSpecializations_MultipleUnlocked_ReturnsAll()
    {
        // Arrange
        var character = CreateTestCharacter(CharacterClass.Adept);
        character.ProgressionPoints = 20;

        _service.UnlockSpecialization(character, 1); // BoneSetter
        _service.UnlockSpecialization(character, 2); // JotunReader

        // Act
        var unlocked = _service.GetUnlockedSpecializations(character);

        // Assert
        Assert.That(unlocked.Count, Is.EqualTo(2));
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
