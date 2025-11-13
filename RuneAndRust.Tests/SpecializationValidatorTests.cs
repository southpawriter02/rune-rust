using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19: Unit tests for SpecializationValidator
/// Tests validation rules and error detection
/// </summary>
[TestFixture]
public class SpecializationValidatorTests
{
    private string _connectionString = string.Empty;
    private SpecializationValidator _validator = null!;
    private SpecializationRepository _specializationRepo = null!;
    private AbilityRepository _abilityRepo = null!;

    [SetUp]
    public void Setup()
    {
        // Create in-memory database for testing
        _connectionString = "Data Source=:memory:";

        // Initialize database schema
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var saveRepo = new SaveRepository(":memory:");

        _validator = new SpecializationValidator(_connectionString);
        _specializationRepo = new SpecializationRepository(_connectionString);
        _abilityRepo = new AbilityRepository(_connectionString);
    }

    #region Metadata Validation Tests

    [Test]
    public void ValidateMetadata_EmptyName_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        spec.Name = "";
        _specializationRepo.Insert(spec);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("name is empty"));
    }

    [Test]
    public void ValidateMetadata_InvalidPathType_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        spec.PathType = "Invalid";
        _specializationRepo.Insert(spec);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Invalid path type"));
    }

    [Test]
    public void ValidateMetadata_InvalidTraumaRisk_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        spec.TraumaRisk = "SuperHigh";
        _specializationRepo.Insert(spec);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Invalid trauma risk"));
    }

    [Test]
    public void ValidateMetadata_ValidSpecialization_PassesMetadataCheck()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);
        SeedValidAbilities(spec.SpecializationID);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        // Should have no metadata errors (may have other errors if abilities aren't perfect)
        Assert.That(result.Errors, Has.None.Contains("name"));
        Assert.That(result.Errors, Has.None.Contains("path type"));
    }

    #endregion

    #region Ability Count Validation Tests

    [Test]
    public void ValidateAbilityCount_TooFewAbilities_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        // Seed only 5 abilities instead of 9
        for (int i = 1; i <= 5; i++)
        {
            var ability = CreateValidAbility(spec.SpecializationID, i, 1);
            _abilityRepo.Insert(ability);
        }

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Invalid ability count: 5"));
    }

    [Test]
    public void ValidateAbilityCount_TooManyAbilities_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        // Seed 12 abilities instead of 9
        for (int i = 1; i <= 12; i++)
        {
            var ability = CreateValidAbility(spec.SpecializationID, i, 1);
            _abilityRepo.Insert(ability);
        }

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Invalid ability count: 12"));
    }

    [Test]
    public void ValidateAbilityCount_ExactlyNineAbilities_Passes()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);
        SeedValidAbilities(spec.SpecializationID);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.Errors, Has.None.Contains("ability count"));
    }

    #endregion

    #region Tier Structure Validation Tests

    [Test]
    public void ValidateTierStructure_WrongTier1Count_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        // Seed 2 Tier 1 instead of 3
        SeedAbilities(spec.SpecializationID, tier1Count: 2, tier2Count: 3, tier3Count: 2, capstoneCount: 1);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Tier 1 has 2 abilities"));
    }

    [Test]
    public void ValidateTierStructure_WrongTier2Count_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        // Seed 4 Tier 2 instead of 3
        SeedAbilities(spec.SpecializationID, tier1Count: 3, tier2Count: 4, tier3Count: 2, capstoneCount: 1);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Tier 2 has 4 abilities"));
    }

    [Test]
    public void ValidateTierStructure_Correct3_3_2_1Pattern_Passes()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);
        SeedValidAbilities(spec.SpecializationID);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.Errors, Has.None.Contains("Tier 1 has"));
        Assert.That(result.Errors, Has.None.Contains("Tier 2 has"));
        Assert.That(result.Errors, Has.None.Contains("Tier 3 has"));
        Assert.That(result.Errors, Has.None.Contains("Capstone tier has"));
    }

    #endregion

    #region PP Cost Validation Tests

    [Test]
    public void ValidatePPCosts_Tier1NonZeroCost_ReturnsWarning()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        SeedValidAbilities(spec.SpecializationID);

        // Change one Tier 1 ability to have cost
        var ability = _abilityRepo.GetBySpecializationAndTier(spec.SpecializationID, 1).First();
        // Can't modify directly, so this test documents expected behavior

        // For this test, we'll create a new spec with wrong costs
        var testSpec = CreateValidSpecialization();
        testSpec.SpecializationID = 999;
        _specializationRepo.Insert(testSpec);

        var wrongAbility = CreateValidAbility(999, 1, 1);
        wrongAbility.PPCost = 3; // Should be 0
        _abilityRepo.Insert(wrongAbility);

        // Act
        var result = _validator.ValidateSpecialization(999);

        // Assert
        Assert.That(result.Warnings, Has.Some.Contains("costs 3 PP (convention: 0 PP"));
    }

    [Test]
    public void ValidatePPCosts_StandardCosts_NoWarnings()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);
        SeedValidAbilities(spec.SpecializationID);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.Warnings, Has.None.Contains("costs"));
    }

    #endregion

    #region Prerequisite Validation Tests

    [Test]
    public void ValidatePrerequisites_NonExistentPrerequisite_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);
        SeedValidAbilities(spec.SpecializationID);

        // Add an ability with invalid prerequisite
        var badAbility = CreateValidAbility(spec.SpecializationID, 999, 2);
        badAbility.Prerequisites = new AbilityPrerequisites
        {
            RequiredAbilityIDs = new List<int> { 9999 }, // Non-existent
            RequiredPPInTree = 8
        };
        _abilityRepo.Insert(badAbility);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("non-existent prerequisite"));
    }

    [Test]
    public void ValidatePrerequisites_CapstoneWithoutPrerequisites_ReturnsWarning()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        spec.SpecializationID = 888;
        _specializationRepo.Insert(spec);

        // Seed abilities with capstone that has no prerequisites
        SeedAbilities(888, 3, 3, 2, 1);

        var capstone = _abilityRepo.GetBySpecializationAndTier(888, 4).First();
        // Capstone already has empty prerequisites from SeedAbilities

        // Act
        var result = _validator.ValidateSpecialization(888);

        // Assert
        Assert.That(result.Warnings, Has.Some.Contains("Capstone ability"));
        Assert.That(result.Warnings, Has.Some.Contains("no prerequisite abilities"));
    }

    #endregion

    #region Total PP Cost Validation Tests

    [Test]
    public void ValidateTotalPPCost_TooLow_ReturnsWarning()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        spec.SpecializationID = 777;
        _specializationRepo.Insert(spec);

        // Seed abilities with very low costs
        for (int i = 1; i <= 9; i++)
        {
            var ability = CreateValidAbility(777, i, 1);
            ability.PPCost = 1; // All cost 1 PP = 9 total (too low)
            _abilityRepo.Insert(ability);
        }

        // Act
        var result = _validator.ValidateSpecialization(777);

        // Assert
        Assert.That(result.Warnings, Has.Some.Contains("Total PP cost is 9"));
    }

    [Test]
    public void ValidateTotalPPCost_TooHigh_ReturnsWarning()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        spec.SpecializationID = 666;
        _specializationRepo.Insert(spec);

        // Seed abilities with very high costs
        for (int i = 1; i <= 9; i++)
        {
            var ability = CreateValidAbility(666, i, 1);
            ability.PPCost = 5; // All cost 5 PP = 45 total (too high)
            _abilityRepo.Insert(ability);
        }

        // Act
        var result = _validator.ValidateSpecialization(666);

        // Assert
        Assert.That(result.Warnings, Has.Some.Contains("Total PP cost is 45"));
    }

    [Test]
    public void ValidateTotalPPCost_StandardCost_NoWarnings()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);
        SeedValidAbilities(spec.SpecializationID);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        // Standard: 0 (3×Tier1) + 12 (3×4 Tier2) + 10 (2×5 Tier3) + 6 (Capstone) = 28
        Assert.That(result.Warnings, Has.None.Contains("Total PP cost"));
    }

    #endregion

    #region Ability Metadata Validation Tests

    [Test]
    public void ValidateAbilityMetadata_InvalidAbilityType_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        var ability = CreateValidAbility(spec.SpecializationID, 1, 1);
        ability.AbilityType = "SuperActive"; // Invalid
        _abilityRepo.Insert(ability);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("invalid type"));
    }

    [Test]
    public void ValidateAbilityMetadata_PassiveWithStaminaCost_ReturnsWarning()
    {
        // Arrange
        var spec = CreateValidSpecialization();
        _specializationRepo.Insert(spec);

        var ability = CreateValidAbility(spec.SpecializationID, 1, 1);
        ability.AbilityType = "Passive";
        ability.ResourceCost = new AbilityResourceCost { Stamina = 20 };
        _abilityRepo.Insert(ability);

        // Act
        var result = _validator.ValidateSpecialization(spec.SpecializationID);

        // Assert
        Assert.That(result.Warnings, Has.Some.Contains("Passive ability"));
        Assert.That(result.Warnings, Has.Some.Contains("stamina cost"));
    }

    #endregion

    #region Helper Methods

    private SpecializationData CreateValidSpecialization()
    {
        return new SpecializationData
        {
            SpecializationID = 100,
            Name = "Test Specialization",
            ArchetypeID = 2, // Adept
            PathType = "Coherent",
            MechanicalRole = "Test Role",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "WILL",
            Description = "Test description",
            Tagline = "Test tagline",
            UnlockRequirements = new UnlockRequirements(),
            ResourceSystem = "Stamina",
            TraumaRisk = "Low",
            IconEmoji = "🧪",
            PPCostToUnlock = 3,
            IsActive = true
        };
    }

    private AbilityData CreateValidAbility(int specializationId, int abilityId, int tierLevel)
    {
        return new AbilityData
        {
            AbilityID = abilityId,
            SpecializationID = specializationId,
            Name = $"Test Ability {abilityId}",
            Description = "Test description",
            TierLevel = tierLevel,
            PPCost = tierLevel switch { 1 => 0, 2 => 4, 3 => 5, 4 => 6, _ => 0 },
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = tierLevel switch { 1 => 0, 2 => 8, 3 => 16, 4 => 24, _ => 0 }
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 10 },
            MechanicalSummary = "Test summary",
            AttributeUsed = "wits",
            BonusDice = 1,
            SuccessThreshold = 2,
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        };
    }

    private void SeedValidAbilities(int specializationId)
    {
        SeedAbilities(specializationId, 3, 3, 2, 1);
    }

    private void SeedAbilities(int specializationId, int tier1Count, int tier2Count, int tier3Count, int capstoneCount)
    {
        int abilityId = 1;

        // Tier 1
        for (int i = 0; i < tier1Count; i++)
        {
            _abilityRepo.Insert(CreateValidAbility(specializationId, abilityId++, 1));
        }

        // Tier 2
        for (int i = 0; i < tier2Count; i++)
        {
            _abilityRepo.Insert(CreateValidAbility(specializationId, abilityId++, 2));
        }

        // Tier 3
        for (int i = 0; i < tier3Count; i++)
        {
            _abilityRepo.Insert(CreateValidAbility(specializationId, abilityId++, 3));
        }

        // Capstone
        for (int i = 0; i < capstoneCount; i++)
        {
            _abilityRepo.Insert(CreateValidAbility(specializationId, abilityId++, 4));
        }
    }

    #endregion
}
