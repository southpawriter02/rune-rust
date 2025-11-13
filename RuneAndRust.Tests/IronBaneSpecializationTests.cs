using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19.2: Unit tests for Iron-Bane specialization
/// Tests specialization seeding, ability structure, and Righteous Fervor resource management
/// </summary>
[TestFixture]
public class IronBaneSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private RighteousFervorService _fervorService = null!;
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
        _fervorService = new RighteousFervorService();
    }

    #region Specialization Seeding Tests

    [Test]
    public void IronBane_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(11); // Iron-Bane ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Iron-Bane"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(1)); // Warrior
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("WILL"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Anti-Mechanical Specialist / Controller"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Low"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🔥"));
    }

    [Test]
    public void IronBane_AppearsInWarriorSpecializations()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 3; // Meet minimum Legend requirement
        character.Attributes.Will = 3; // Meet WILL requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var ironBane = result.Specializations!.FirstOrDefault(s => s.Name == "Iron-Bane");
        Assert.That(ironBane, Is.Not.Null);
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void IronBane_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(11);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void IronBane_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(11);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var tier1 = result.Abilities!.Count(a => a.TierLevel == 1);
        var tier2 = result.Abilities!.Count(a => a.TierLevel == 2);
        var tier3 = result.Abilities!.Count(a => a.TierLevel == 3);
        var capstone = result.Abilities!.Count(a => a.TierLevel == 4);

        Assert.That(tier1, Is.EqualTo(3), "Should have 3 Tier 1 abilities");
        Assert.That(tier2, Is.EqualTo(3), "Should have 3 Tier 2 abilities");
        Assert.That(tier3, Is.EqualTo(2), "Should have 2 Tier 3 abilities");
        Assert.That(capstone, Is.EqualTo(1), "Should have 1 Capstone ability");
    }

    [Test]
    public void IronBane_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(11);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var tier1Costs = result.Abilities!.Where(a => a.TierLevel == 1).Select(a => a.PPCost);
        var tier2Costs = result.Abilities!.Where(a => a.TierLevel == 2).Select(a => a.PPCost);
        var tier3Costs = result.Abilities!.Where(a => a.TierLevel == 3).Select(a => a.PPCost);
        var capstoneCost = result.Abilities!.Where(a => a.TierLevel == 4).Select(a => a.PPCost).FirstOrDefault();

        Assert.That(tier1Costs, Is.All.EqualTo(0), "Tier 1 abilities should cost 0 PP");
        Assert.That(tier2Costs, Is.All.EqualTo(4), "Tier 2 abilities should cost 4 PP");
        Assert.That(tier3Costs, Is.All.EqualTo(5), "Tier 3 abilities should cost 5 PP");
        Assert.That(capstoneCost, Is.EqualTo(6), "Capstone should cost 6 PP");
    }

    [Test]
    public void IronBane_Tier1Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(11, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Scholar of Corruption"));
        Assert.That(abilityNames, Contains.Item("Purifying Flame"));
        Assert.That(abilityNames, Contains.Item("Weakness Exploiter"));
    }

    [Test]
    public void IronBane_Tier2Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(11, 2);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("System Shutdown"));
        Assert.That(abilityNames, Contains.Item("Critical Strike"));
        Assert.That(abilityNames, Contains.Item("Flame Ward"));
    }

    [Test]
    public void IronBane_Tier3Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(11, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Purging Flame"));
        Assert.That(abilityNames, Contains.Item("Righteous Conviction"));
    }

    [Test]
    public void IronBane_Capstone_IsDisvinePurge()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(11, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(1));
        Assert.That(result.Abilities![0].Name, Is.EqualTo("Divine Purge"));
    }

    [Test]
    public void IronBane_Capstone_RequiresBothTier3Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(11, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var capstone = result.Abilities![0];
        Assert.That(capstone.Prerequisites, Is.Not.Null);
        Assert.That(capstone.Prerequisites!.RequiredAbilityIDs, Is.Not.Null);
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs.Count, Is.EqualTo(2));
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Contains.Item(1107)); // Purging Flame
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Contains.Item(1108)); // Righteous Conviction
    }

    [Test]
    public void IronBane_AllAbilities_HaveFireDamageType()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(11);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        // Active abilities should have Fire damage type
        var activeAbilities = result.Abilities!.Where(a => a.AbilityType == "Active");
        foreach (var ability in activeAbilities)
        {
            Assert.That(ability.DamageType, Is.EqualTo("Fire"),
                $"{ability.Name} should have Fire damage type");
        }
    }

    [Test]
    public void IronBane_AllAbilities_UseWillAttribute()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(11);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        // Active abilities should use WILL attribute
        var activeAbilities = result.Abilities!.Where(a => a.AbilityType == "Active");
        foreach (var ability in activeAbilities)
        {
            Assert.That(ability.AttributeUsed, Is.EqualTo("will"),
                $"{ability.Name} should use WILL attribute");
        }
    }

    #endregion

    #region Righteous Fervor Service Tests

    [Test]
    public void RighteousFervor_GenerateFervor_IncreasesCorrectly()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 0;
        character.MaxRighteousFervor = 100;

        // Act
        _fervorService.GenerateFervor(character, 25, "Test");

        // Assert
        Assert.That(character.RighteousFervor, Is.EqualTo(25));
    }

    [Test]
    public void RighteousFervor_GenerateFervor_CapsAtMax()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 90;
        character.MaxRighteousFervor = 100;

        // Act
        _fervorService.GenerateFervor(character, 25, "Test");

        // Assert
        Assert.That(character.RighteousFervor, Is.EqualTo(100)); // Capped at max
    }

    [Test]
    public void RighteousFervor_SpendFervor_DecreasesCorrectly()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 50;

        // Act
        var success = _fervorService.SpendFervor(character, 30, "Test Ability");

        // Assert
        Assert.That(success, Is.True);
        Assert.That(character.RighteousFervor, Is.EqualTo(20));
    }

    [Test]
    public void RighteousFervor_SpendFervor_FailsWhenInsufficient()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 20;

        // Act
        var success = _fervorService.SpendFervor(character, 30, "Test Ability");

        // Assert
        Assert.That(success, Is.False);
        Assert.That(character.RighteousFervor, Is.EqualTo(20)); // Unchanged
    }

    [Test]
    public void RighteousFervor_HasEnoughFervor_ReturnsTrueWhenSufficient()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 50;

        // Act
        var hasEnough = _fervorService.HasEnoughFervor(character, 30);

        // Assert
        Assert.That(hasEnough, Is.True);
    }

    [Test]
    public void RighteousFervor_HasEnoughFervor_ReturnsFalseWhenInsufficient()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 20;

        // Act
        var hasEnough = _fervorService.HasEnoughFervor(character, 30);

        // Assert
        Assert.That(hasEnough, Is.False);
    }

    [Test]
    public void RighteousFervor_ResetFervor_SetsToZero()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 75;

        // Act
        _fervorService.ResetFervor(character);

        // Assert
        Assert.That(character.RighteousFervor, Is.EqualTo(0));
    }

    [Test]
    public void RighteousFervor_GenerateFervor_AppliesRank1Multiplier()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 0;

        // Add Righteous Conviction at Rank 1 (+50%)
        character.Abilities.Add(new Ability
        {
            Name = "Righteous Conviction",
            CurrentRank = 1
        });

        // Act
        _fervorService.GenerateFervor(character, 20, "Test");

        // Assert
        Assert.That(character.RighteousFervor, Is.EqualTo(30)); // 20 * 1.5 = 30
    }

    [Test]
    public void RighteousFervor_GenerateFervor_AppliesRank3Multiplier()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.RighteousFervor = 0;

        // Add Righteous Conviction at Rank 3 (+100%, i.e., double)
        character.Abilities.Add(new Ability
        {
            Name = "Righteous Conviction",
            CurrentRank = 3
        });

        // Act
        _fervorService.GenerateFervor(character, 25, "Test");

        // Assert
        Assert.That(character.RighteousFervor, Is.EqualTo(50)); // 25 * 2.0 = 50
    }

    [Test]
    public void RighteousFervor_RefundAbilityCosts_RefundsHalfCosts()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.Stamina = 50;
        character.MaxStamina = 100;
        character.RighteousFervor = 30;
        character.MaxRighteousFervor = 100;

        // Add Righteous Conviction at Rank 3 (enables refunds)
        character.Abilities.Add(new Ability
        {
            Name = "Righteous Conviction",
            CurrentRank = 3
        });

        // Act
        _fervorService.RefundAbilityCosts(character, 40, 20); // Refund half of 40 Stamina, 20 Fervor

        // Assert
        Assert.That(character.Stamina, Is.EqualTo(70)); // 50 + 20 = 70
        Assert.That(character.RighteousFervor, Is.EqualTo(40)); // 30 + 10 = 40
    }

    #endregion

    #region Unlock Requirement Tests

    [Test]
    public void IronBane_UnlockRequirements_RequiresLegend3()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 2; // Below requirement
        character.Attributes.Will = 3;
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 11);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend"));
    }

    [Test]
    public void IronBane_UnlockRequirements_RequiresWill3()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 3;
        character.Attributes.Will = 2; // Below requirement
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 11);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("WILL"));
    }

    [Test]
    public void IronBane_UnlockRequirements_MeetsAllRequirements_CanUnlock()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 3;
        character.Attributes.Will = 3;
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 11);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestWarrior()
    {
        return new PlayerCharacter
        {
            Name = $"TestWarrior_{Guid.NewGuid()}",
            Class = CharacterClass.Warrior,
            Specialization = Specialization.None,
            ProgressionPoints = 0,
            CurrentLegend = 1,
            Corruption = 0,
            HP = 50,
            MaxHP = 50,
            Stamina = 100,
            MaxStamina = 100,
            RighteousFervor = 0,
            MaxRighteousFervor = 100,
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
