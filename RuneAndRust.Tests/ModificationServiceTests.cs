using NUnit.Framework;
using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using RuneAndRust.Engine.Crafting;
using RuneAndRust.Persistence;
using System.Text.Json;

namespace RuneAndRust.Tests;

[TestFixture]
public class ModificationServiceTests
{
    private SqliteConnection _connection = null!;
    private CraftingRepository _repository = null!;
    private ModificationService _modificationService = null!;

    [SetUp]
    public void Setup()
    {
        // Create in-memory database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Create schema
        CreateTestSchema();

        // Seed test data
        SeedTestData();

        // Initialize services
        _repository = new CraftingRepository(_connection.ConnectionString);
        _modificationService = new ModificationService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    #region Test Data Setup

    private void CreateTestSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE Items (
                item_id INTEGER PRIMARY KEY,
                item_name TEXT NOT NULL,
                item_type TEXT NOT NULL,
                quality_tier INTEGER DEFAULT 1
            );

            CREATE TABLE Character_Inventory (
                inventory_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                item_id INTEGER NOT NULL,
                quantity INTEGER DEFAULT 0,
                quality_tier INTEGER DEFAULT 1,
                UNIQUE(character_id, item_id, quality_tier)
            );

            CREATE TABLE Runic_Inscriptions (
                inscription_id INTEGER PRIMARY KEY,
                inscription_name TEXT NOT NULL,
                inscription_tier INTEGER NOT NULL,
                target_equipment_type TEXT NOT NULL,
                effect_type TEXT NOT NULL,
                effect_value TEXT NOT NULL,
                is_temporary INTEGER NOT NULL,
                uses_if_temporary INTEGER DEFAULT 0,
                component_requirements TEXT NOT NULL,
                crafting_cost_credits INTEGER DEFAULT 0,
                inscription_description TEXT DEFAULT ''
            );

            CREATE TABLE Equipment_Modifications (
                modification_id INTEGER PRIMARY KEY AUTOINCREMENT,
                equipment_item_id INTEGER NOT NULL,
                modification_type TEXT NOT NULL,
                modification_name TEXT NOT NULL,
                modification_value TEXT NOT NULL,
                is_permanent INTEGER NOT NULL,
                remaining_uses INTEGER NULL,
                applied_at TEXT NOT NULL,
                applied_by_recipe_id INTEGER NULL,
                FOREIGN KEY (equipment_item_id) REFERENCES Character_Inventory(inventory_id)
            );

            CREATE TABLE Characters (
                character_id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                credits INTEGER DEFAULT 0
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            -- Items
            INSERT INTO Items (item_id, item_name, item_type, quality_tier) VALUES
            (2001, 'Plasma Rifle', 'Weapon', 3),
            (3001, 'Reinforced Chest Plate', 'Armor', 3),
            (9001, 'Minor Aetheric Shard', 'Component', 3),
            (9020, 'Stabilizing Compound', 'Component', 2);

            -- Characters
            INSERT INTO Characters (character_id, name, credits) VALUES
            (1, 'Test Character', 500),
            (2, 'Poor Character', 50);

            -- Character Inventory (Equipment)
            INSERT INTO Character_Inventory (inventory_id, character_id, item_id, quantity, quality_tier) VALUES
            (100, 1, 2001, 1, 3),  -- Plasma Rifle for character 1
            (101, 1, 3001, 1, 3),  -- Chest Plate for character 1
            (102, 2, 2001, 1, 3);  -- Plasma Rifle for character 2

            -- Character Inventory (Components)
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier) VALUES
            (1, 9001, 3, 3),  -- 3x Minor Aetheric Shard (Quality 3)
            (1, 9020, 5, 2),  -- 5x Stabilizing Compound (Quality 2)
            (2, 9001, 0, 3);  -- Character 2 has no components

            -- Runic Inscriptions
            INSERT INTO Runic_Inscriptions VALUES
            (8001, 'Rune of Flame', 3, 'Weapon', 'Elemental',
             '{""element"": ""Fire"", ""bonus_damage"": 5, ""burn_chance"": 0.15}',
             1, 10, '[{""ItemId"": 9001, ""Quantity"": 1, ""MinQuality"": 3}, {""ItemId"": 9020, ""Quantity"": 2, ""MinQuality"": 2}]',
             150, 'Adds fire damage and burn chance'),
            (8002, 'Rune of Sharpness', 4, 'Weapon', 'Stat_Boost',
             '{""stat"": ""damage"", ""value"": 10}',
             0, 0, '[{""ItemId"": 9001, ""Quantity"": 1, ""MinQuality"": 3}]',
             500, 'Permanent damage boost'),
            (8003, 'Rune of Fire Resistance', 3, 'Armor', 'Resistance',
             '{""resistance_type"": ""Fire"", ""value"": 15}',
             1, 10, '[{""ItemId"": 9001, ""Quantity"": 1, ""MinQuality"": 3}]',
             150, 'Adds fire resistance'),
            (8004, 'Rune of Bleeding', 3, 'Weapon', 'Status',
             '{""status"": ""Bleed"", ""application_chance"": 0.25, ""duration"": 3}',
             1, 10, '[{""ItemId"": 9001, ""Quantity"": 1, ""MinQuality"": 3}]',
             150, 'Applies bleeding status'),
            (8005, 'Rune of Regeneration', 4, 'Both', 'Special',
             '{""effect"": ""regeneration"", ""hp_per_turn"": 3}',
             0, 0, '[{""ItemId"": 9001, ""Quantity"": 2, ""MinQuality"": 3}]',
             300, 'Regenerates HP each turn');
        ";
        cmd.ExecuteNonQuery();
    }

    #endregion

    #region Successful Application Tests

    [Test]
    public void ApplyModification_ValidInscription_Success()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 100; // Plasma Rifle
        int inscriptionId = 8001; // Rune of Flame

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ModificationId, Is.Not.Null);
        Assert.That(result.ModificationName, Is.EqualTo("Rune of Flame"));
        Assert.That(result.RemainingUses, Is.EqualTo(10)); // Temporary with 10 uses
        Assert.That(result.Message, Does.Contain("10 uses"));
    }

    [Test]
    public void ApplyModification_PermanentInscription_Success()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 100; // Plasma Rifle
        int inscriptionId = 8002; // Rune of Sharpness (permanent)

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ModificationName, Is.EqualTo("Rune of Sharpness"));
        Assert.That(result.RemainingUses, Is.Null); // Permanent
        Assert.That(result.Message, Does.Contain("permanent"));
    }

    [Test]
    public void ApplyModification_ArmorInscription_Success()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 101; // Chest Plate (armor)
        int inscriptionId = 8003; // Rune of Fire Resistance (armor)

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.ModificationName, Is.EqualTo("Rune of Fire Resistance"));
    }

    [Test]
    public void ApplyModification_BothTypeInscription_WorksOnAnyEquipment()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 100; // Plasma Rifle (weapon)
        int inscriptionId = 8005; // Rune of Regeneration (Both type)

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    #endregion

    #region Failure Tests

    [Test]
    public void ApplyModification_InvalidInscriptionId_Fails()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 100;
        int inscriptionId = 9999; // Non-existent

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Inscription not found"));
    }

    [Test]
    public void ApplyModification_InvalidEquipmentId_Fails()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 9999; // Non-existent
        int inscriptionId = 8001;

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Equipment not found"));
    }

    [Test]
    public void ApplyModification_WrongEquipmentType_Fails()
    {
        // Arrange
        int characterId = 1;
        int equipmentId = 101; // Chest Plate (armor)
        int inscriptionId = 8001; // Rune of Flame (weapon only)

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("can only be applied to"));
    }

    [Test]
    public void ApplyModification_MaxSlotsFull_Fails()
    {
        // Arrange: Apply 3 modifications first
        int characterId = 1;
        int equipmentId = 100;

        _modificationService.ApplyModification(characterId, equipmentId, 8001);
        _modificationService.ApplyModification(characterId, equipmentId, 8002);
        _modificationService.ApplyModification(characterId, equipmentId, 8004);

        // Act: Try to apply 4th modification
        var result = _modificationService.ApplyModification(characterId, equipmentId, 8005);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("maximum 3"));
    }

    [Test]
    public void ApplyModification_InsufficientComponents_Fails()
    {
        // Arrange
        int characterId = 2; // Character with no components
        int equipmentId = 102;
        int inscriptionId = 8001;

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient components"));
    }

    [Test]
    public void ApplyModification_InsufficientCredits_Fails()
    {
        // Arrange
        int characterId = 2; // Character with only 50 credits
        int equipmentId = 102;
        int inscriptionId = 8002; // Costs 500 credits

        // Give character the components but not enough credits
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier)
            VALUES (2, 9001, 5, 3)
        ";
        cmd.ExecuteNonQuery();

        // Act
        var result = _modificationService.ApplyModification(characterId, equipmentId, inscriptionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient credits"));
    }

    #endregion

    #region Modification Management Tests

    [Test]
    public void RemoveModification_ValidOwnership_Success()
    {
        // Arrange: Apply a modification first
        int characterId = 1;
        int equipmentId = 100;
        var applyResult = _modificationService.ApplyModification(characterId, equipmentId, 8001);
        int modificationId = applyResult.ModificationId!.Value;

        // Act
        var result = _modificationService.RemoveModification(characterId, modificationId);

        // Assert
        Assert.That(result, Is.True);

        // Verify modification is gone
        var mods = _modificationService.GetActiveModifications(equipmentId);
        Assert.That(mods, Is.Empty);
    }

    [Test]
    public void RemoveModification_NotOwned_Fails()
    {
        // Arrange: Character 1 applies modification
        int characterId1 = 1;
        int equipmentId = 100;
        var applyResult = _modificationService.ApplyModification(characterId1, equipmentId, 8001);
        int modificationId = applyResult.ModificationId!.Value;

        // Act: Character 2 tries to remove it
        var result = _modificationService.RemoveModification(2, modificationId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetActiveModifications_ReturnsOnlyActive()
    {
        // Arrange: Apply 2 permanent and 1 temporary modification
        int characterId = 1;
        int equipmentId = 100;

        _modificationService.ApplyModification(characterId, equipmentId, 8001); // Temporary
        _modificationService.ApplyModification(characterId, equipmentId, 8002); // Permanent

        // Act
        var mods = _modificationService.GetActiveModifications(equipmentId);

        // Assert
        Assert.That(mods, Has.Count.EqualTo(2));
        Assert.That(mods.Any(m => m.ModificationName == "Rune of Flame"), Is.True);
        Assert.That(mods.Any(m => m.ModificationName == "Rune of Sharpness"), Is.True);
    }

    #endregion

    #region Temporary Modification Tests

    [Test]
    public void DecrementUses_TemporaryMod_DecrementsCorrectly()
    {
        // Arrange: Apply temporary modification with 10 uses
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8001);

        // Act
        _modificationService.DecrementTemporaryModificationUses(equipmentId);

        // Assert: Verify uses decreased
        var mods = _modificationService.GetActiveModifications(equipmentId);
        Assert.That(mods, Has.Count.EqualTo(1));
        Assert.That(mods[0].RemainingUses, Is.EqualTo(9));
    }

    [Test]
    public void DecrementUses_ReachesZero_RemovesMod()
    {
        // Arrange: Apply temporary modification and decrement to 1 use
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8001);

        // Decrement 9 times to reach 1 use
        for (int i = 0; i < 9; i++)
        {
            _modificationService.DecrementTemporaryModificationUses(equipmentId);
        }

        // Verify 1 use remaining
        var modsBefore = _modificationService.GetActiveModifications(equipmentId);
        Assert.That(modsBefore[0].RemainingUses, Is.EqualTo(1));

        // Act: Decrement final use
        _modificationService.DecrementTemporaryModificationUses(equipmentId);

        // Assert: Modification should be removed
        var modsAfter = _modificationService.GetActiveModifications(equipmentId);
        Assert.That(modsAfter, Is.Empty);
    }

    [Test]
    public void DecrementUses_PermanentMod_NotAffected()
    {
        // Arrange: Apply permanent modification
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8002); // Permanent

        // Act: Decrement uses (should have no effect on permanent mod)
        _modificationService.DecrementTemporaryModificationUses(equipmentId);

        // Assert: Permanent modification still exists
        var mods = _modificationService.GetActiveModifications(equipmentId);
        Assert.That(mods, Has.Count.EqualTo(1));
        Assert.That(mods[0].IsPermanent, Is.True);
    }

    #endregion

    #region Stat Calculation Tests

    [Test]
    public void CalculateStats_StatBoost_Calculated()
    {
        // Arrange: Apply stat boost modification
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8002); // +10 damage

        // Act
        var stats = _modificationService.CalculateModificationStats(equipmentId);

        // Assert
        Assert.That(stats.BonusDamage, Is.EqualTo(10));
    }

    [Test]
    public void CalculateStats_Resistance_Calculated()
    {
        // Arrange: Apply resistance modification to armor
        int characterId = 1;
        int equipmentId = 101; // Chest Plate
        _modificationService.ApplyModification(characterId, equipmentId, 8003); // +15% Fire resistance

        // Act
        var stats = _modificationService.CalculateModificationStats(equipmentId);

        // Assert
        Assert.That(stats.Resistances.ContainsKey("Fire"), Is.True);
        Assert.That(stats.Resistances["Fire"], Is.EqualTo(15));
    }

    [Test]
    public void CalculateStats_ElementalEffect_Calculated()
    {
        // Arrange: Apply elemental modification
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8001); // +5 fire damage, 15% burn

        // Act
        var stats = _modificationService.CalculateModificationStats(equipmentId);

        // Assert
        Assert.That(stats.ElementalDamage, Has.Count.EqualTo(1));
        Assert.That(stats.ElementalDamage[0].Element, Is.EqualTo("Fire"));
        Assert.That(stats.ElementalDamage[0].BonusDamage, Is.EqualTo(5));
        Assert.That(stats.ElementalDamage[0].ApplicationChance, Is.EqualTo(0.15));
    }

    [Test]
    public void CalculateStats_StatusEffect_Calculated()
    {
        // Arrange: Apply status effect modification
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8004); // Bleed 25% chance, 3 turns

        // Act
        var stats = _modificationService.CalculateModificationStats(equipmentId);

        // Assert
        Assert.That(stats.StatusEffects, Has.Count.EqualTo(1));
        Assert.That(stats.StatusEffects[0].StatusName, Is.EqualTo("Bleed"));
        Assert.That(stats.StatusEffects[0].ApplicationChance, Is.EqualTo(0.25));
        Assert.That(stats.StatusEffects[0].Duration, Is.EqualTo(3));
    }

    [Test]
    public void CalculateStats_SpecialEffect_Calculated()
    {
        // Arrange: Apply special effect modification
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8005); // Regeneration 3 HP/turn

        // Act
        var stats = _modificationService.CalculateModificationStats(equipmentId);

        // Assert
        Assert.That(stats.RegenerationPerTurn, Is.EqualTo(3));
    }

    [Test]
    public void CalculateStats_MultipleMods_CombinesCorrectly()
    {
        // Arrange: Apply multiple modifications
        int characterId = 1;
        int equipmentId = 100;
        _modificationService.ApplyModification(characterId, equipmentId, 8001); // +5 fire damage
        _modificationService.ApplyModification(characterId, equipmentId, 8002); // +10 damage
        _modificationService.ApplyModification(characterId, equipmentId, 8004); // Bleed status

        // Act
        var stats = _modificationService.CalculateModificationStats(equipmentId);

        // Assert
        Assert.That(stats.BonusDamage, Is.EqualTo(10));
        Assert.That(stats.ElementalDamage, Has.Count.EqualTo(1));
        Assert.That(stats.StatusEffects, Has.Count.EqualTo(1));
    }

    #endregion
}
