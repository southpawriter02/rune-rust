using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.24.1: Unit tests for Veiðimaðr (Hunter) specialization
/// Tests specialization seeding, ability structure, corruption tracking, and marking mechanics
/// </summary>
[TestFixture]
public class VeidimadurSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private VeidimadurService _veidimadurService = null!;
    private MarkingService _markingService = null!;
    private CorruptionTrackingService _corruptionService = null!;
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
        _veidimadurService = new VeidimadurService(_connectionString);
        _markingService = new MarkingService(_connectionString);
        _corruptionService = new CorruptionTrackingService(_connectionString);
    }

    #region Specialization Seeding Tests

    [Test]
    public void Veiðimaðr_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(24001); // Veiðimaðr ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Veiðimaðr"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(4)); // Skirmisher
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("FINESSE"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Ranged DPS / Corruption Tracker"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("Medium"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🏹"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina + Focus"));
    }

    [Test]
    public void Veiðimaðr_AppearsInSkirmisherSpecializations()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 5; // Meet minimum Legend requirement

        // Act
        var result = _specializationService.GetAvailableSpecializations(character);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specializations, Is.Not.Null);

        var veidimadur = result.Specializations!.FirstOrDefault(s => s.Name == "Veiðimaðr");
        Assert.That(veidimadur, Is.Not.Null);
    }

    [Test]
    public void Veiðimaðr_RequiresMinimumLegend5()
    {
        // Arrange
        var character = CreateTestSkirmisher();
        character.CurrentLegend = 4; // Below minimum

        // Act
        var result = _specializationService.CanUnlock(character, 24001);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Legend 5"));
    }

    #endregion

    #region Ability Structure Tests

    [Test]
    public void Veiðimaðr_Has9Abilities()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24001);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Abilities, Is.Not.Null);
        Assert.That(result.Abilities!.Count, Is.EqualTo(9));
    }

    [Test]
    public void Veiðimaðr_HasCorrectTierDistribution()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24001);

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
    public void Veiðimaðr_HasCorrectPPCosts()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24001);

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
    public void Veiðimaðr_TotalPPCostIs33()
    {
        // Act
        var result = _abilityService.GetAbilitiesForSpecialization(24001);

        // Assert
        var totalPP = result.Abilities!.Sum(a => a.PPCost);
        Assert.That(totalPP, Is.EqualTo(33), "Total PP to learn all abilities should be 33");
    }

    #endregion

    #region Tier 1 Ability Tests

    [Test]
    public void WildernessAcclimation_ProvidesCorrectBonuses()
    {
        // Test Rank 1
        var rank1Bonus = _veidimadurService.GetWildernessAcklimationBonus(1);
        Assert.That(rank1Bonus, Is.EqualTo(1), "Rank 1 should provide +1d10");

        // Test Rank 2
        var rank2Bonus = _veidimadurService.GetWildernessAcklimationBonus(2);
        Assert.That(rank2Bonus, Is.EqualTo(2), "Rank 2 should provide +2d10");

        // Test Rank 3
        var rank3Bonus = _veidimadurService.GetWildernessAcklimationBonus(3);
        Assert.That(rank3Bonus, Is.EqualTo(3), "Rank 3 should provide +3d10");
    }

    [Test]
    public void AimedShot_Rank1_DealsWeaponDamage()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        hunter.EquippedWeapon = new Weapon { Name = "Longbow", Damage = 15 };
        var target = CreateTestEnemy();

        // Act
        var result = _veidimadurService.ExecuteAimedShot(hunter, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.DamageDealt, Is.GreaterThan(0));
        Assert.That(hunter.Stamina, Is.EqualTo(60), "Should cost 40 Stamina at Rank 1");
    }

    [Test]
    public void AimedShot_Rank2_ReducedStaminaCost()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        hunter.EquippedWeapon = new Weapon { Name = "Longbow", Damage = 15 };
        var target = CreateTestEnemy();

        // Act
        var result = _veidimadurService.ExecuteAimedShot(hunter, target, 2);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(hunter.Stamina, Is.EqualTo(65), "Should cost 35 Stamina at Rank 2");
    }

    [Test]
    public void AimedShot_InsufficientStamina_Fails()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 30; // Not enough for Rank 1 (40 cost)
        var target = CreateTestEnemy();

        // Act
        var result = _veidimadurService.ExecuteAimedShot(hunter, target, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient Stamina"));
    }

    [Test]
    public void SetSnare_CreatesValidSnare()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;

        // Act
        var result = _veidimadurService.SetSnare(hunter, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(hunter.Stamina, Is.EqualTo(65), "Should cost 35 Stamina");
    }

    #endregion

    #region Tier 2 Ability Tests

    [Test]
    public void MarkForDeath_Rank1_AppliesMarkAndInflictsStress()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        hunter.PsychicStress = 0;
        var target = CreateTestEnemy();

        // Act
        var result = _veidimadurService.ExecuteMarkForDeath(hunter, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.BonusDamage, Is.EqualTo(8), "Rank 1 should provide +8 damage");
        Assert.That(result.Duration, Is.EqualTo(3), "Rank 1 duration should be 3 turns");
        Assert.That(result.StressCost, Is.EqualTo(5), "Rank 1 should cost 5 Psychic Stress");
        Assert.That(hunter.PsychicStress, Is.EqualTo(5), "Hunter should have 5 Psychic Stress");
        Assert.That(hunter.Stamina, Is.EqualTo(70), "Should cost 30 Stamina");
    }

    [Test]
    public void MarkForDeath_Rank3_ProvidesAllyBonus()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        var target = CreateTestEnemy();

        // Act
        var result = _veidimadurService.ExecuteMarkForDeath(hunter, target, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.BonusDamage, Is.EqualTo(15), "Rank 3 should provide +15 damage to hunter");
        Assert.That(result.AllyBonusDamage, Is.EqualTo(5), "Rank 3 should provide +5 damage to allies");
        Assert.That(result.StressCost, Is.EqualTo(2), "Rank 3 should cost only 2 Psychic Stress");
    }

    [Test]
    public void BlightTippedArrow_AppliesBlightedToxin()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        var target = CreateTestEnemy();
        target.Corruption = 0; // Not enough for Glitch

        // Act
        var result = _veidimadurService.ExecuteBlightTippedArrow(hunter, target, 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.StatusEffectsApplied, Contains.Item("Blighted Toxin"));
        Assert.That(hunter.Stamina, Is.EqualTo(55), "Should cost 45 Stamina");
    }

    [Test]
    public void BlightTippedArrow_Rank1_40PercentGlitchVsHighCorruption()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        var target = CreateTestEnemy();
        target.Corruption = 50; // High enough for Glitch proc

        // Act - Run multiple times to verify probabilistic behavior
        int glitchCount = 0;
        for (int i = 0; i < 100; i++)
        {
            hunter.Stamina = 100; // Reset stamina
            target.StatusEffects.Clear(); // Reset status effects
            var result = _veidimadurService.ExecuteBlightTippedArrow(hunter, target, 1);
            if (result.StatusEffectsApplied.Contains("Glitch"))
            {
                glitchCount++;
            }
        }

        // Assert - Should proc roughly 40% of the time (allow 25-55% range for variance)
        Assert.That(glitchCount, Is.InRange(25, 55), "Glitch should proc approximately 40% of the time");
    }

    [Test]
    public void PredatorsFocus_ProvidesResolveBonus_OnlyInBackRow()
    {
        // Test in back row
        var backRowBonus = _veidimadurService.GetPredatorsFocusBonus(1, true);
        Assert.That(backRowBonus, Is.EqualTo(1), "Should provide +1d10 in back row");

        // Test not in back row
        var frontRowBonus = _veidimadurService.GetPredatorsFocusBonus(1, false);
        Assert.That(frontRowBonus, Is.EqualTo(0), "Should provide no bonus in front row");
    }

    #endregion

    #region Tier 3 Ability Tests

    [Test]
    public void ExploitCorruption_Rank1_ProvidesCritBonus()
    {
        // Test Low Corruption
        var lowBonus = _corruptionService.GetExploitCorruptionCritBonus(1, CorruptionTrackingService.CorruptionLevel.Low);
        Assert.That(lowBonus, Is.EqualTo(5), "Low corruption should provide +5% crit");

        // Test Medium Corruption
        var medBonus = _corruptionService.GetExploitCorruptionCritBonus(1, CorruptionTrackingService.CorruptionLevel.Medium);
        Assert.That(medBonus, Is.EqualTo(10), "Medium corruption should provide +10% crit");

        // Test High Corruption
        var highBonus = _corruptionService.GetExploitCorruptionCritBonus(1, CorruptionTrackingService.CorruptionLevel.High);
        Assert.That(highBonus, Is.EqualTo(15), "High corruption should provide +15% crit");

        // Test Extreme Corruption
        var extremeBonus = _corruptionService.GetExploitCorruptionCritBonus(1, CorruptionTrackingService.CorruptionLevel.Extreme);
        Assert.That(extremeBonus, Is.EqualTo(20), "Extreme corruption should provide +20% crit");
    }

    [Test]
    public void ExploitCorruption_Rank2_DoublesCritBonus()
    {
        // Test High Corruption at Rank 2
        var highBonus = _corruptionService.GetExploitCorruptionCritBonus(2, CorruptionTrackingService.CorruptionLevel.High);
        Assert.That(highBonus, Is.EqualTo(30), "Rank 2 should double bonus (15% * 2 = 30%)");

        // Test Extreme Corruption at Rank 2
        var extremeBonus = _corruptionService.GetExploitCorruptionCritBonus(2, CorruptionTrackingService.CorruptionLevel.Extreme);
        Assert.That(extremeBonus, Is.EqualTo(40), "Rank 2 should double bonus (20% * 2 = 40%)");
    }

    [Test]
    public void HeartseekerShot_RequiresCharging()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        var target = CreateTestEnemy();

        // Act - Try to release without charging
        var result = _veidimadurService.ReleaseHeartseekerShot(hunter, target, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Must charge"));
    }

    [Test]
    public void HeartseekerShot_ChargeAndRelease_DealsHighDamage()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        hunter.EquippedWeapon = new Weapon { Name = "Longbow", Damage = 15 };
        var target = CreateTestEnemy();

        // Act - Charge
        var chargeResult = _veidimadurService.ChargeHeartseekerShot(hunter);
        Assert.That(chargeResult.Success, Is.True);

        // Act - Release
        var releaseResult = _veidimadurService.ReleaseHeartseekerShot(hunter, target, 1);

        // Assert
        Assert.That(releaseResult.Success, Is.True);
        Assert.That(releaseResult.DamageDealt, Is.GreaterThan(0));
        Assert.That(hunter.Stamina, Is.EqualTo(40), "Should cost 60 Stamina");
    }

    [Test]
    public void HeartseekerShot_Rank3_PurgesCorruptionFromMarkedTarget()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();
        hunter.Stamina = 100;
        hunter.EquippedWeapon = new Weapon { Name = "Longbow", Damage = 15 };
        var target = CreateTestEnemy();
        target.Corruption = 50;

        // Mark the target first
        _veidimadurService.ExecuteMarkForDeath(hunter, target, 1);

        // Charge
        _veidimadurService.ChargeHeartseekerShot(hunter);

        // Act - Release with Rank 3
        var result = _veidimadurService.ReleaseHeartseekerShot(hunter, target, 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CorruptionPurged, Is.GreaterThan(0), "Should purge corruption from marked target");
        Assert.That(result.CorruptionPurged, Is.LessThanOrEqualTo(20), "Rank 3 should purge max 20 corruption");
    }

    #endregion

    #region Capstone Ability Tests

    [Test]
    public void StalkerOfTheUnseen_TogglesStanceOnOff()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();

        // Act - Activate
        var activateResult = _veidimadurService.ActivateStalkerStance(hunter, 1);
        Assert.That(activateResult.Success, Is.True);
        Assert.That(activateResult.Message, Does.Contain("activated"));

        // Act - Deactivate
        var deactivateResult = _veidimadurService.ActivateStalkerStance(hunter, 1);
        Assert.That(deactivateResult.Success, Is.True);
        Assert.That(deactivateResult.Message, Does.Contain("deactivated"));
        Assert.That(hunter.PsychicStress, Is.EqualTo(10), "Should inflict 10 Psychic Stress on deactivation at Rank 1");
    }

    [Test]
    public void StalkerOfTheUnseen_Rank3_ReducedStressPenalty()
    {
        // Arrange
        var hunter = CreateTestSkirmisher();

        // Act - Activate and deactivate
        _veidimadurService.ActivateStalkerStance(hunter, 3);
        _veidimadurService.ActivateStalkerStance(hunter, 3);

        // Assert
        Assert.That(hunter.PsychicStress, Is.EqualTo(5), "Rank 3 should only inflict 5 Psychic Stress on deactivation");
    }

    #endregion

    #region Corruption Tracking Tests

    [Test]
    public void CorruptionLevel_CategorizesCorrectly()
    {
        Assert.That(_corruptionService.GetCorruptionLevel(0), Is.EqualTo(CorruptionTrackingService.CorruptionLevel.None));
        Assert.That(_corruptionService.GetCorruptionLevel(15), Is.EqualTo(CorruptionTrackingService.CorruptionLevel.Low));
        Assert.That(_corruptionService.GetCorruptionLevel(45), Is.EqualTo(CorruptionTrackingService.CorruptionLevel.Medium));
        Assert.That(_corruptionService.GetCorruptionLevel(75), Is.EqualTo(CorruptionTrackingService.CorruptionLevel.High));
        Assert.That(_corruptionService.GetCorruptionLevel(95), Is.EqualTo(CorruptionTrackingService.CorruptionLevel.Extreme));
    }

    [Test]
    public void PurgeCorruption_RemovesCorrectAmount()
    {
        // Arrange
        var enemy = CreateTestEnemy();
        enemy.Corruption = 50;

        // Act
        var purged = _corruptionService.PurgeCorruption(enemy, 20);

        // Assert
        Assert.That(purged, Is.EqualTo(20), "Should purge 20 corruption");
        Assert.That(enemy.Corruption, Is.EqualTo(30), "Should have 30 corruption remaining");
    }

    [Test]
    public void PurgeCorruption_CannotPurgeMoreThanAvailable()
    {
        // Arrange
        var enemy = CreateTestEnemy();
        enemy.Corruption = 10;

        // Act
        var purged = _corruptionService.PurgeCorruption(enemy, 20);

        // Assert
        Assert.That(purged, Is.EqualTo(10), "Should only purge available corruption");
        Assert.That(enemy.Corruption, Is.EqualTo(0), "Should have 0 corruption remaining");
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestSkirmisher()
    {
        return new PlayerCharacter
        {
            Name = "Test Hunter",
            Class = CharacterClass.Skirmisher,
            CurrentLegend = 5,
            ProgressionPoints = 50,
            HP = 100,
            MaxHP = 100,
            Stamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Corruption = 0,
            Attributes = new Attributes
            {
                Might = 2,
                Finesse = 4,
                Wits = 3,
                Will = 2,
                Sturdiness = 3
            },
            Abilities = new List<Ability>(),
            StatusEffects = new List<StatusEffect>(),
            CompletedQuests = new List<Quest>()
        };
    }

    private Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            Name = "Test Corrupted Beast",
            HP = 100,
            MaxHP = 100,
            Corruption = 0,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
