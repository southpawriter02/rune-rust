using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.19.3: Unit tests for Atgeir-wielder specialization
/// Tests specialization seeding, ability structure, and formation control mechanics
/// </summary>
[TestFixture]
public class AtgeirWielderSpecializationTests
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
    public void AtgeirWielder_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(12); // Atgeir-wielder ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Atgeir-wielder"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(1)); // Warrior
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("MIGHT"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Battlefield Controller / Formation Anchor"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("None"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("⚔️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina"));
    }

    [Test]
    public void AtgeirWielder_AppearsInWarriorSpecializations()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 3; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var atgeirWielder = result.Specializations!.FirstOrDefault(s => s.Name == "Atgeir-wielder");
        Assert.That(atgeirWielder, Is.Not.Null);
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void AtgeirWielder_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void AtgeirWielder_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

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
    public void AtgeirWielder_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

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
    public void AtgeirWielder_TotalPPCost_Is30()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(30), "Total PP cost should be 30 (3+3+3+4+4+4+5+5+6)");
    }

    [Test]
    public void AtgeirWielder_Tier1Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(12, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Formal Training"));
        Assert.That(abilityNames, Contains.Item("Skewer"));
        Assert.That(abilityNames, Contains.Item("Disciplined Stance"));
    }

    [Test]
    public void AtgeirWielder_Tier2Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(12, 2);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Hook and Drag"));
        Assert.That(abilityNames, Contains.Item("Line Breaker"));
        Assert.That(abilityNames, Contains.Item("Guarding Presence"));
    }

    [Test]
    public void AtgeirWielder_Tier3Abilities_HaveCorrectNames()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(12, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityNames = result.Abilities!.Select(a => a.Name).ToList();

        Assert.That(abilityNames, Contains.Item("Brace for Charge"));
        Assert.That(abilityNames, Contains.Item("Unstoppable Phalanx"));
    }

    [Test]
    public void AtgeirWielder_Capstone_IsLivingFortress()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(12, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(1));
        Assert.That(result.Abilities![0].Name, Is.EqualTo("Living Fortress"));
    }

    [Test]
    public void AtgeirWielder_Capstone_RequiresBothTier3Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(12, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var capstone = result.Abilities![0];
        Assert.That(capstone.Prerequisites, Is.Not.Null);
        Assert.That(capstone.Prerequisites!.RequiredAbilityIDs, Is.Not.Null);
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs.Count, Is.EqualTo(2));
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Contains.Item(1207)); // Brace for Charge
        Assert.That(capstone.Prerequisites.RequiredAbilityIDs, Contains.Item(1208)); // Unstoppable Phalanx
    }

    [Test]
    public void AtgeirWielder_Capstone_Requires24PPInTree()
    {
        // Act
        var result = _abilityService.GetAbilitiesByTier(12, 4);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var capstone = result.Abilities![0];
        Assert.That(capstone.Prerequisites, Is.Not.Null);
        Assert.That(capstone.Prerequisites!.RequiredPPInTree, Is.EqualTo(24));
    }

    [Test]
    public void AtgeirWielder_ActiveAbilities_UsePhysicalDamage()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        // Active damage-dealing abilities should have Physical damage type
        var damageAbilities = result.Abilities!
            .Where(a => a.AbilityType == "Active" && !string.IsNullOrEmpty(a.DamageType));

        foreach (var ability in damageAbilities)
        {
            Assert.That(ability.DamageType, Is.EqualTo("Physical"),
                $"{ability.Name} should have Physical damage type");
        }
    }

    [Test]
    public void AtgeirWielder_ActiveAbilities_UseMightAttribute()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        // Active abilities should use MIGHT attribute
        var activeAttackAbilities = result.Abilities!
            .Where(a => a.AbilityType == "Active" && !string.IsNullOrEmpty(a.AttributeUsed));

        foreach (var ability in activeAttackAbilities)
        {
            Assert.That(ability.AttributeUsed, Is.EqualTo("might"),
                $"{ability.Name} should use MIGHT attribute");
        }
    }

    #endregion

    #region Ability Mechanics Tests

    [Test]
    public void Skewer_HasCorrectProperties()
    {
        // Act
        var result = _abilityService.GetAbility(1202); // Skewer

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Name, Is.EqualTo("Skewer"));
        Assert.That(result.Ability.AbilityType, Is.EqualTo("Active"));
        Assert.That(result.Ability.ActionType, Is.EqualTo("Standard Action"));
        Assert.That(result.Ability.AttributeUsed, Is.EqualTo("might"));
        Assert.That(result.Ability.DamageType, Is.EqualTo("Physical"));
        Assert.That(result.Ability.ResourceCost, Is.Not.Null);
        Assert.That(result.Ability.ResourceCost!.Stamina, Is.EqualTo(40));
    }

    [Test]
    public void DisciplinedStance_IsDefensiveBonus()
    {
        // Act
        var result = _abilityService.GetAbility(1203); // Disciplined Stance

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Name, Is.EqualTo("Disciplined Stance"));
        Assert.That(result.Ability.AbilityType, Is.EqualTo("Active"));
        Assert.That(result.Ability.ActionType, Is.EqualTo("Bonus Action"));
        Assert.That(result.Ability.TargetType, Is.EqualTo("Self"));
        Assert.That(result.Ability.CooldownTurns, Is.EqualTo(3));
    }

    [Test]
    public void HookAndDrag_RequiresTier2()
    {
        // Act
        var result = _abilityService.GetAbility(1204); // Hook and Drag

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Prerequisites, Is.Not.Null);
        Assert.That(result.Ability.Prerequisites!.RequiredPPInTree, Is.EqualTo(8));
        Assert.That(result.Ability.CooldownTurns, Is.EqualTo(4));
    }

    [Test]
    public void LineBreaker_TargetsAllFrontRow()
    {
        // Act
        var result = _abilityService.GetAbility(1205); // Line Breaker

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Name, Is.EqualTo("Line Breaker"));
        Assert.That(result.Ability.TargetType, Is.EqualTo("All Enemies (front row)"));
        Assert.That(result.Ability.CooldownTurns, Is.EqualTo(5));
        Assert.That(result.Ability.ResourceCost, Is.Not.Null);
        Assert.That(result.Ability.ResourceCost!.Stamina, Is.EqualTo(50));
    }

    [Test]
    public void GuardingPresence_IsPassiveAura()
    {
        // Act
        var result = _abilityService.GetAbility(1206); // Guarding Presence

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Name, Is.EqualTo("Guarding Presence"));
        Assert.That(result.Ability.AbilityType, Is.EqualTo("Passive"));
        Assert.That(result.Ability.TargetType, Is.EqualTo("Adjacent Front-Row Allies"));
    }

    [Test]
    public void BraceForCharge_IsOncePerCombat()
    {
        // Act
        var result = _abilityService.GetAbility(1207); // Brace for Charge

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Name, Is.EqualTo("Brace for Charge"));
        Assert.That(result.Ability.CooldownType, Is.EqualTo("Once Per Combat"));
        Assert.That(result.Ability.CooldownTurns, Is.EqualTo(999));
    }

    [Test]
    public void UnstoppablePhalanx_RequiresTier3()
    {
        // Act
        var result = _abilityService.GetAbility(1208); // Unstoppable Phalanx

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Ability, Is.Not.Null);
        Assert.That(result.Ability!.Prerequisites, Is.Not.Null);
        Assert.That(result.Ability.Prerequisites!.RequiredPPInTree, Is.EqualTo(16));
        Assert.That(result.Ability.TargetType, Is.EqualTo("Single Enemy + Enemy Behind"));
    }

    #endregion

    #region Unlock Requirement Tests

    [Test]
    public void AtgeirWielder_UnlockRequirements_RequiresLegend3()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 2; // Below requirement
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 12);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend"));
    }

    [Test]
    public void AtgeirWielder_UnlockRequirements_MeetsAllRequirements_CanUnlock()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 3;
        character.ProgressionPoints = 10;

        // Act
        var result = _specializationService.CanUnlock(character, 12);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    #endregion

    #region Ability ID Range Tests

    [Test]
    public void AtgeirWielder_AbilityIDs_InCorrectRange()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        foreach (var ability in result.Abilities!)
        {
            Assert.That(ability.AbilityID, Is.InRange(1201, 1209),
                $"{ability.Name} should have AbilityID in range 1201-1209");
        }
    }

    [Test]
    public void AtgeirWielder_AllAbilityIDsAreUnique()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(12);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);

        var abilityIds = result.Abilities!.Select(a => a.AbilityID).ToList();
        var uniqueIds = abilityIds.Distinct().ToList();

        Assert.That(abilityIds.Count, Is.EqualTo(uniqueIds.Count),
            "All AbilityIDs should be unique");
    }

    #endregion

    #region Integration Tests

    [Test]
    public void AtgeirWielder_CanBeUnlockedAndUsed()
    {
        // Arrange
        var character = CreateTestWarrior();
        character.CurrentLegend = 3;
        character.ProgressionPoints = 30;

        // Act - Check unlock
        var canUnlock = _specializationService.CanUnlock(character, 12);
        Assert.That(canUnlock.Success, Is.True);

        // Act - Unlock specialization
        var unlockResult = _specializationService.UnlockSpecialization(character, 12);
        Assert.That(unlockResult.Success, Is.True);

        // Act - Get available abilities
        var abilitiesResult = _abilityService.GetAbilitiesForSpecialization(12);
        Assert.That(abilitiesResult.Success, Is.True);
        Assert.That(abilitiesResult.Abilities, Is.Not.Null);
        Assert.That(abilitiesResult.Abilities!.Count, Is.EqualTo(9));
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
