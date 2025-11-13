using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19.4: Unit tests for Bone-Setter specialization
/// Tests specialization seeding, ability structure, and healing/support mechanics
/// </summary>
[TestFixture]
public class BoneSetterSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
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
    }

    #region Specialization Seeding Tests

    [Test]
    public void BoneSetter_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(1); // Bone-Setter ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Bone-Setter"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(2)); // Adept
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("FINESSE"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Healer / Sanity Anchor"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("None"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("⚕️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina + Consumable Items"));
    }

    [Test]
    public void BoneSetter_AppearsInAdeptSpecializations()
    {
        // Arrange
        var character = CreateTestAdept();
        character.CurrentLegend = 3; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var boneSetter = result.Specializations!.FirstOrDefault(s => s.Name == "Bone-Setter");
        Assert.That(boneSetter, Is.Not.Null);
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void BoneSetter_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void BoneSetter_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

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
    public void BoneSetter_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var tier1Costs = result.Abilities!.Where(a => a.TierLevel == 1).Select(a => a.PPCost);
        var tier2Costs = result.Abilities!.Where(a => a.TierLevel == 2).Select(a => a.PPCost);
        var tier3Costs = result.Abilities!.Where(a => a.TierLevel == 3).Select(a => a.PPCost);
        var capstoneCost = result.Abilities!.Where(a => a.TierLevel == 4).Select(a => a.PPCost).FirstOrDefault();

        Assert.That(tier1Costs, Is.All.EqualTo(3), "Tier 1 abilities should cost 3 PP");
        Assert.That(tier2Costs, Is.All.EqualTo(4), "Tier 2 abilities should cost 4 PP");
        Assert.That(tier3Costs, Is.All.EqualTo(5), "Tier 3 abilities should cost 5 PP");
        Assert.That(capstoneCost, Is.EqualTo(6), "Capstone should cost 6 PP");
    }

    [Test]
    public void BoneSetter_TotalPPCost_Is30()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(30), "Total PP cost should be 30 (3+3+3+4+4+4+5+5+6)");
    }

    [Test]
    public void BoneSetter_Tier1Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(1, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Field Medic"));
        Assert.That(abilityNames, Contains.Item("Mend Wound"));
        Assert.That(abilityNames, Contains.Item("Apply Tourniquet"));
    }

    [Test]
    public void BoneSetter_Tier2Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(1, 2);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Anatomical Insight"));
        Assert.That(abilityNames, Contains.Item("Administer Antidote"));
        Assert.That(abilityNames, Contains.Item("Triage"));
    }

    [Test]
    public void BoneSetter_Tier3Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(1, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Cognitive Realignment"));
        Assert.That(abilityNames, Contains.Item("Defensive Focus"));
    }

    [Test]
    public void BoneSetter_Capstone_IsMiracleWorker()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(1, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(1));
        Assert.That(result.Abilities![0].Name, Is.EqualTo("Miracle Worker"));
    }

    [Test]
    public void BoneSetter_Capstone_RequiresBothTier3Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(1, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var capstone = result.Abilities![0];
        Assert.That(capstone.Prerequisites, Is.Not.Null);
        Assert.That(capstone.Prerequisites!.RequiredAbilityIDs, Is.Not.Null);
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs.Count, Is.EqualTo(2));
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Contains.Item(107)); // Cognitive Realignment
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Contains.Item(108)); // Defensive Focus
    }

    [Test]
    public void BoneSetter_Capstone_Requires24PPInTree()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(1, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var capstone = result.Abilities![0];
        Assert.That(capstone.Prerequisites, Is.Not.Null);
        Assert.That(capstone.Prerequisites!.RequiredPPInTree, Is.EqualTo(24));
    }

    #endregion

    #region Ability Rank Tests

    [Test]
    public void BoneSetter_AllAbilities_Have3RanksExceptPassives()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        // All abilities should have MaxRank = 3 in v0.19.4
        foreach (var ability in result.Abilities!)
        {
            Assert.That(ability.MaxRank, Is.EqualTo(3),
                $"{ability.Name} should have MaxRank = 3");
        }
    }

    [Test]
    public void BoneSetter_FieldMedic_HasCorrectRankProgression()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Field Medic");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.MaxRank, Is.EqualTo(3));
        Assert.That(result.Ability.CostToRank2, Is.EqualTo(20));
        Assert.That(result.Ability.Notes, Does.Contain("Masterwork"));
        Assert.That(result.Ability.Notes, Does.Contain("Miracle Tinctures"));
    }

    [Test]
    public void BoneSetter_MendWound_HasCorrectHealingProgression()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Mend Wound");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.HealingDice, Is.EqualTo(3)); // Rank 1: 3d8
        Assert.That(result.Ability.Notes, Does.Contain("4d8")); // Rank 2
        Assert.That(result.Ability.Notes, Does.Contain("5d8")); // Rank 3
    }

    [Test]
    public void BoneSetter_Triage_HasCorrectBonusProgression()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Triage");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Notes, Does.Contain("25%")); // Rank 1
        Assert.That(result.Ability.Notes, Does.Contain("35%")); // Rank 2
        Assert.That(result.Ability.Notes, Does.Contain("50%")); // Rank 3
        Assert.That(result.Ability.Notes, Does.Contain("Revitalized"));
    }

    [Test]
    public void BoneSetter_CognitiveRealignment_HasCorrectStressHealing()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Cognitive Realignment");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Notes, Does.Contain("15 Psychic Stress")); // Rank 1
        Assert.That(result.Ability!.Notes, Does.Contain("25 Stress")); // Rank 2
        Assert.That(result.Ability!.Notes, Does.Contain("40 Stress")); // Rank 3
        Assert.That(result.Ability.Notes, Does.Contain("Focused"));
    }

    [Test]
    public void BoneSetter_MiracleWorker_HasCorrectCapstoneHealing()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Miracle Worker");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.HealingDice, Is.EqualTo(8)); // Rank 1: 8d10
        Assert.That(result.Ability.Notes, Does.Contain("10d10")); // Rank 2
        Assert.That(result.Ability.Notes, Does.Contain("12d10")); // Rank 3
        Assert.That(result.Ability.Notes, Does.Contain("cannot drop below 1 HP"));
        Assert.That(result.Ability.CooldownType, Is.EqualTo("Per Expedition"));
    }

    #endregion

    #region Consumable System Tests

    [Test]
    public void BoneSetter_MendWound_RequiresHealingPoultice()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Mend Wound");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Description, Does.Contain("poultice"));
        Assert.That(result.Ability.Notes, Does.Contain("Consumes one [Healing Poultice]"));
    }

    [Test]
    public void BoneSetter_AdministerAntidote_RequiresAntidote()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Administer Antidote");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Description, Does.Contain("antidote"));
        Assert.That(result.Ability.Notes, Does.Contain("Consumes one [Common Antidote]"));
    }

    [Test]
    public void BoneSetter_CognitiveRealignment_RequiresStabilizingDraught()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Cognitive Realignment");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Notes, Does.Contain("Consumes [Stabilizing Draught]"));
    }

    [Test]
    public void BoneSetter_MiracleWorker_RequiresMiracleTincture()
    {
        // Act
        var result = _abilityService.GetAbilitiesByName(1, "Miracle Worker");

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Notes, Does.Contain("Consumes 1 [Miracle Tincture]"));
    }

    #endregion

    #region Unlock Requirement Tests

    [Test]
    public void BoneSetter_UnlockRequirements_RequiresLegend3()
    {
        // Arrange
        var character = CreateTestAdept();
        character.CurrentLegend = 2; // Below requirement
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend"));
    }

    [Test]
    public void BoneSetter_UnlockRequirements_MeetsAllRequirements_CanUnlock()
    {
        // Arrange
        var character = CreateTestAdept();
        character.CurrentLegend = 3;
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 1);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    #endregion

    #region Ability ID Range Tests

    [Test]
    public void BoneSetter_AbilityIDs_InCorrectRange()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        foreach (var ability in result.Abilities!)
        {
            Assert.That(ability.AbilityID, Is.InRange(101, 109),
                $"{ability.Name} should have AbilityID in range 101-109");
        }
    }

    [Test]
    public void BoneSetter_AllAbilityIDsAreUnique()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityIds = result.Abilities!.Select(a => a.AbilityID).ToList();
        var uniqueIds = abilityIds.Distinct().ToList();

        Assert.That(abilityIds.Count, Is.EqualTo(uniqueIds.Count),
            "All AbilityIDs should be unique");
    }

    #endregion

    #region Coherent Path Tests

    [Test]
    public void BoneSetter_IsCoherentPath()
    {
        // Act
        var result = _specializationService.GetSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("None"));
    }

    [Test]
    public void BoneSetter_NoCorruptionRequirement()
    {
        // Act
        var result = _specializationService.GetSpecialization(1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.UnlockRequirements, Is.Not.Null);
        Assert.That(result.Specialization.UnlockRequirements!.MaxCorruption, Is.EqualTo(100));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void BoneSetter_CanBeUnlockedAndUsed()
    {
        // Arrange
        var character = CreateTestAdept();
        character.CurrentLegend = 3;
        character.ProgressionPoints = 30;

        // Act - Check unlock
        var canUnlock = _specializationService.CanUnlock(character, 1);
        Assert.That(canUnlock.Success, Is.True);

        // Act - Unlock specialization
        var unlockResult = _specializationService.UnlockSpecialization(character, 1);
        Assert.That(unlockResult.Success, Is.True);

        // Act - Get available abilities
        var abilitiesResult = _abilityService.GetAbilitiesForSpecialization(1);
        Assert.That(abilitiesResult.Success, Is.True);
        Assert.That(abilitiesResult.Abilities, Is.Not.Null);
        Assert.That(abilitiesResult.Abilities!.Count, Is.EqualTo(9));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestAdept()
    {
        return new PlayerCharacter
        {
            Name = $"TestAdept_{Guid.NewGuid()}",
            Class = CharacterClass.Adept,
            Specialization = Specialization.None,
            ProgressionPoints = 0,
            CurrentLegend = 1,
            Corruption = 0,
            HP = 40,
            MaxHP = 40,
            Stamina = 80,
            MaxStamina = 80,
            Attributes = new Attributes
            {
                Might = 2,
                Finesse = 3,
                Wits = 4,
                Will = 3,
                Sturdiness = 2
            }
        };
    }

    #endregion
}
