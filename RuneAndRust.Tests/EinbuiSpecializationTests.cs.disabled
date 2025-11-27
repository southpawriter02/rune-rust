using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.27.2: Unit tests for Einbui specialization
/// Tests specialization seeding, ability structure, field crafting mechanics, and survival systems
/// </summary>
[TestFixture]
public class EinbuiSpecializationTests
{
    private string _connectionString = string.Empty;
    private SpecializationService _specializationService = null!;
    private AbilityService _abilityService = null!;
    private EinbuiService _einbuiService = null!;
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
        _einbuiService = new EinbuiService(_connectionString);

        // Create field crafting tracking table
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Characters_FieldCrafting (
                    character_id INTEGER PRIMARY KEY,
                    held_item_1 TEXT,
                    held_item_2 TEXT,
                    held_item_3 TEXT,
                    total_items_crafted INTEGER DEFAULT 0,
                    total_traps_placed INTEGER DEFAULT 0,
                    blight_haven_used_this_expedition BOOLEAN NOT NULL DEFAULT 0,
                    blight_haven_room_id INTEGER,
                    resourceful_eye_rooms_this_expedition TEXT DEFAULT '[]',
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            cmd.ExecuteNonQuery();
        }
    }

    #region Specialization Seeding Tests

    [Test]
    public void Einbui_IsSeeded_WithCorrectProperties()
    {
        // Act
        var result = _specializationService.GetSpecialization(27002); // Einbui ID

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Specialization, Is.Not.Null);
        Assert.That(result.Specialization!.Name, Is.EqualTo("Einbui"));
        Assert.That(result.Specialization.ArchetypeID, Is.EqualTo(2)); // Adept
        Assert.That(result.Specialization.PathType, Is.EqualTo("Coherent"));
        Assert.That(result.Specialization.PrimaryAttribute, Is.EqualTo("WITS"));
        Assert.That(result.Specialization.SecondaryAttribute, Is.EqualTo("FINESSE"));
        Assert.That(result.Specialization.MechanicalRole, Is.EqualTo("Survival Specialist / Resource Generation / Exploration Support"));
        Assert.That(result.Specialization.TraumaRisk, Is.EqualTo("None"));
        Assert.That(result.Specialization.IconEmoji, Is.EqualTo("🏕️"));
        Assert.That(result.Specialization.ResourceSystem, Is.EqualTo("Stamina"));
    }

    [Test]
    public void Einbui_Has9Abilities_WithCorrectTierDistribution()
    {
        // Act
        var allAbilities = _abilityService.GetAbilitiesForSpecialization(27002);

        // Assert
        Assert.That(allAbilities.Count, Is.EqualTo(9), "Einbui should have exactly 9 abilities");

        var tier1 = allAbilities.Where(a => a.TierLevel == 1).ToList();
        var tier2 = allAbilities.Where(a => a.TierLevel == 2).ToList();
        var tier3 = allAbilities.Where(a => a.TierLevel == 3).ToList();
        var capstone = allAbilities.Where(a => a.TierLevel == 4).ToList();

        Assert.That(tier1.Count, Is.EqualTo(3), "Should have 3 Tier 1 abilities");
        Assert.That(tier2.Count, Is.EqualTo(3), "Should have 3 Tier 2 abilities");
        Assert.That(tier3.Count, Is.EqualTo(2), "Should have 2 Tier 3 abilities");
        Assert.That(capstone.Count, Is.EqualTo(1), "Should have 1 Capstone ability");

        // Verify PP costs
        Assert.That(tier1.All(a => a.PPCost == 3), Is.True, "Tier 1 abilities should cost 3 PP");
        Assert.That(tier2.All(a => a.PPCost == 4), Is.True, "Tier 2 abilities should cost 4 PP");
        Assert.That(tier3.All(a => a.PPCost == 5), Is.True, "Tier 3 abilities should cost 5 PP");
        Assert.That(capstone.All(a => a.PPCost == 6), Is.True, "Capstone should cost 6 PP");
    }

    [Test]
    public void Einbui_AbilityIDs_AreInCorrectRange()
    {
        // Act
        var allAbilities = _abilityService.GetAbilitiesForSpecialization(27002);

        // Assert
        Assert.That(allAbilities.All(a => a.AbilityID >= 27010 && a.AbilityID <= 27018), Is.True,
            "All Einbui ability IDs should be in range 27010-27018");
    }

    #endregion

    #region Tier 1: Field Crafting Tests

    [Test]
    public void CraftBasicConcoction_CraftsCrudePoultice_AtRank1()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.CraftBasicConcoction(
            characterId,
            EinbuiService.ConcoctionType.Poultice,
            rank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ItemName, Is.EqualTo("Crude Poultice"));
        Assert.That(result.ItemEffect, Is.EqualTo("Restore 2d6 HP"));
        Assert.That(result.Message, Contains.Substring("Crude Poultice"));
    }

    [Test]
    public void CraftBasicConcoction_CraftsWeakStimulant_AtRank1()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.CraftBasicConcoction(
            characterId,
            EinbuiService.ConcoctionType.Stimulant,
            rank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ItemName, Is.EqualTo("Weak Stimulant"));
        Assert.That(result.ItemEffect, Is.EqualTo("Restore 15 Stamina"));
    }

    [Test]
    public void CraftBasicConcoction_CraftsSuperiorPoultice_AtRank3()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.CraftBasicConcoction(
            characterId,
            EinbuiService.ConcoctionType.Poultice,
            rank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ItemName, Is.EqualTo("Superior Poultice"));
        Assert.That(result.ItemEffect, Is.EqualTo("Restore 6d6 HP + remove [Bleeding]"));
        Assert.That(result.ItemEffect, Contains.Substring("Bleeding"));
    }

    [Test]
    public void CraftBasicConcoction_CannotExceed3HeldItems()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Craft 3 items (fill up slots)
        _einbuiService.CraftBasicConcoction(characterId, EinbuiService.ConcoctionType.Poultice, 1);
        _einbuiService.CraftBasicConcoction(characterId, EinbuiService.ConcoctionType.Stimulant, 1);
        _einbuiService.CraftBasicConcoction(characterId, EinbuiService.ConcoctionType.Poultice, 1);

        // Act - Try to craft 4th item
        var result = _einbuiService.CraftBasicConcoction(characterId, EinbuiService.ConcoctionType.Poultice, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Contains.Substring("Cannot hold more than 3"));
    }

    [Test]
    public void UseHeldItem_ConsumesItem_Successfully()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);
        _einbuiService.CraftBasicConcoction(characterId, EinbuiService.ConcoctionType.Poultice, 1);

        // Act
        var result = _einbuiService.UseHeldItem(characterId, "Crude Poultice");

        // Assert
        Assert.That(result.success, Is.True);
        Assert.That(result.effect, Is.EqualTo("Restore 2d6 HP"));
        Assert.That(result.message, Contains.Substring("Crude Poultice"));
    }

    [Test]
    public void MasterImproviser_UpgradesConcoction_ToRank3Automatically()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act - Rank 1 concoction with Master Improviser Rank 1
        var result = _einbuiService.CraftBasicConcoction(
            characterId,
            EinbuiService.ConcoctionType.Poultice,
            rank: 1,
            masterImproviserRank: 1);

        // Assert - Should get Rank 3 effects
        Assert.That(result.Success, Is.True);
        Assert.That(result.ItemName, Is.EqualTo("Superior Poultice"));
        Assert.That(result.ItemEffect, Contains.Substring("6d6 HP"));
        Assert.That(result.ItemEffect, Contains.Substring("Bleeding"));
    }

    #endregion

    #region Tier 1: Improvised Trap Tests

    [Test]
    public void PlaceImprovisedTrap_PlacesTrap_AtRank1()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.PlaceImprovisedTrap(
            characterId,
            tileX: 5,
            tileY: 3,
            rank: 1);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Contains.Substring("(5,3)"));
        Assert.That(result.Message, Contains.Substring("[Rooted] for 1 turn"));
    }

    [Test]
    public void PlaceImprovisedTrap_AppliesBleedingEffect_AtRank3()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.PlaceImprovisedTrap(
            characterId,
            tileX: 5,
            tileY: 3,
            rank: 3);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Contains.Substring("[Rooted] for 2 turns"));
        Assert.That(result.Message, Contains.Substring("[Bleeding]"));
        Assert.That(result.Message, Contains.Substring("1d4 per turn for 3 turns"));
    }

    [Test]
    public void MasterImproviser_UpgradesTrap_ToRank3Automatically()
    {
        // Arrange
        int characterId = 1;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act - Rank 1 trap with Master Improviser Rank 1
        var result = _einbuiService.PlaceImprovisedTrap(
            characterId,
            tileX: 5,
            tileY: 3,
            rank: 1,
            masterImproviserRank: 1);

        // Assert - Should get Rank 3 effects
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Contains.Substring("[Rooted] for 2 turns"));
        Assert.That(result.Message, Contains.Substring("[Bleeding]"));
    }

    #endregion

    #region Tier 2: Resourceful Eye Tests

    [Test]
    public void ResourcefulEye_FindsHiddenResources_Successfully()
    {
        // Arrange
        int characterId = 1;
        int roomId = 100;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.ExecuteResourcefulEye(characterId, roomId, rank: 1);

        // Assert
        Assert.That(result.success, Is.True);
        Assert.That(result.resourcesFound, Is.GreaterThan(0));
        Assert.That(result.message, Contains.Substring("hidden resource"));
    }

    [Test]
    public void ResourcefulEye_CanOnlyBeUsed_OncePerRoom()
    {
        // Arrange
        int characterId = 1;
        int roomId = 100;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act - First use
        var firstUse = _einbuiService.ExecuteResourcefulEye(characterId, roomId, rank: 1);

        // Act - Second use in same room
        var secondUse = _einbuiService.ExecuteResourcefulEye(characterId, roomId, rank: 1);

        // Assert
        Assert.That(firstUse.success, Is.True);
        Assert.That(secondUse.success, Is.False);
        Assert.That(secondUse.message, Contains.Substring("already"));
    }

    [Test]
    public void ResourcefulEye_Rank3_AlsoRevealsSecretsAndTraps()
    {
        // Arrange
        int characterId = 1;
        int roomId = 100;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.ExecuteResourcefulEye(characterId, roomId, rank: 3);

        // Assert
        Assert.That(result.success, Is.True);
        Assert.That(result.foundSecrets, Is.True);
        Assert.That(result.message, Contains.Substring("hidden passages"));
    }

    #endregion

    #region Capstone: Blight Haven Tests

    [Test]
    public void DesignateBlightHaven_CreatesHaven_Successfully()
    {
        // Arrange
        int characterId = 1;
        int roomId = 100;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.DesignateBlightHaven(characterId, roomId, rank: 1);

        // Assert
        Assert.That(result.success, Is.True);
        Assert.That(result.message, Contains.Substring("Blight Haven"));
        Assert.That(result.message, Contains.Substring("0% Ambush"));
        Assert.That(result.message, Contains.Substring("+10 recovery"));
    }

    [Test]
    public void DesignateBlightHaven_CanOnlyBeUsed_OncePerExpedition()
    {
        // Arrange
        int characterId = 1;
        int roomId1 = 100;
        int roomId2 = 101;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act - First use
        var firstUse = _einbuiService.DesignateBlightHaven(characterId, roomId1, rank: 1);

        // Act - Second use (same expedition)
        var secondUse = _einbuiService.DesignateBlightHaven(characterId, roomId2, rank: 1);

        // Assert
        Assert.That(firstUse.success, Is.True);
        Assert.That(secondUse.success, Is.False);
        Assert.That(secondUse.message, Contains.Substring("already"));
        Assert.That(secondUse.message, Contains.Substring("One per expedition"));
    }

    [Test]
    public void DesignateBlightHaven_Rank3_AllowsAdvancedCrafting()
    {
        // Arrange
        int characterId = 1;
        int roomId = 100;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act
        var result = _einbuiService.DesignateBlightHaven(characterId, roomId, rank: 3);

        // Assert
        Assert.That(result.success, Is.True);
        Assert.That(result.message, Contains.Substring("+30 recovery"));
        Assert.That(result.message, Contains.Substring("advanced crafting"));
    }

    [Test]
    public void ResetBlightHaven_AllowsNewUse_InNewExpedition()
    {
        // Arrange
        int characterId = 1;
        int roomId1 = 100;
        int roomId2 = 101;
        _einbuiService.InitializeFieldCraftingData(characterId);

        // Act - Use once
        _einbuiService.DesignateBlightHaven(characterId, roomId1, rank: 1);

        // Reset for new expedition
        _einbuiService.ResetBlightHavenForNewExpedition(characterId);

        // Try again in new expedition
        var result = _einbuiService.DesignateBlightHaven(characterId, roomId2, rank: 1);

        // Assert
        Assert.That(result.success, Is.True, "Should be able to use Blight Haven again after expedition reset");
    }

    #endregion

    #region Ability Detail Tests

    [Test]
    public void RadicalSelfReliance1_IsPassive_WithCorrectBonuses()
    {
        // Act
        var ability = _abilityService.GetAbility(27010);

        // Assert
        Assert.That(ability, Is.Not.Null);
        Assert.That(ability!.Name, Is.EqualTo("Radical Self-Reliance I"));
        Assert.That(ability.AbilityType, Is.EqualTo("Passive"));
        Assert.That(ability.TierLevel, Is.EqualTo(1));
        Assert.That(ability.BonusDice, Is.EqualTo(1));
        Assert.That(ability.Notes, Contains.Substring("Wasteland Survival"));
        Assert.That(ability.Notes, Contains.Substring("Tracking"));
        Assert.That(ability.Notes, Contains.Substring("Foraging"));
    }

    [Test]
    public void TheUltimateSurvivor_IsCapstone_WithCorrectProperties()
    {
        // Act
        var ability = _abilityService.GetAbility(27018);

        // Assert
        Assert.That(ability, Is.Not.Null);
        Assert.That(ability!.Name, Is.EqualTo("The Ultimate Survivor"));
        Assert.That(ability.AbilityType, Is.EqualTo("Passive+Active"));
        Assert.That(ability.TierLevel, Is.EqualTo(4));
        Assert.That(ability.PPCost, Is.EqualTo(6));
        Assert.That(ability.CooldownType, Is.EqualTo("Once Per Expedition (Blight Haven only)"));
        Assert.That(ability.Notes, Contains.Substring("Universal Competence"));
        Assert.That(ability.Notes, Contains.Substring("Blight Haven"));
        Assert.That(ability.Notes, Contains.Substring("0% Ambush"));
    }

    [Test]
    public void LiveOffTheLand_ReducesRationConsumption()
    {
        // Act
        var ability = _abilityService.GetAbility(27017);

        // Assert
        Assert.That(ability, Is.Not.Null);
        Assert.That(ability!.Name, Is.EqualTo("Live off the Land"));
        Assert.That(ability.AbilityType, Is.EqualTo("Passive"));
        Assert.That(ability.TierLevel, Is.EqualTo(3));
        Assert.That(ability.Notes, Contains.Substring("25%"));
        Assert.That(ability.Notes, Contains.Substring("40%"));
        Assert.That(ability.Notes, Contains.Substring("50%"));
        Assert.That(ability.Notes, Contains.Substring("Common Herbs"));
    }

    #endregion
}
