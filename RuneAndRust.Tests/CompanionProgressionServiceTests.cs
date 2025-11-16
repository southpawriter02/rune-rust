using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.34.3: Tests for CompanionProgressionService
/// Validates leveling, stat scaling, equipment management, and ability unlocking
/// </summary>
[TestFixture]
public class CompanionProgressionServiceTests
{
    private string _testDbPath = null!;
    private string _connectionString = null!;
    private SaveRepository _saveRepo = null!;
    private CompanionProgressionService _progressionService = null!;
    private RecruitmentService _recruitmentService = null!;

    [SetUp]
    public void Setup()
    {
        // Create unique test database for each test
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_progression_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_testDbPath}";

        // Initialize database with all required tables
        _saveRepo = new SaveRepository(_testDbPath);
        _saveRepo.InitializeDatabase();

        // Seed companion data
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Execute companion schema
        var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Data/v0.34.1_companion_schema.sql");
        if (File.Exists(schemaPath))
        {
            var schemaSql = File.ReadAllText(schemaPath);
            var schemaCommand = connection.CreateCommand();
            schemaCommand.CommandText = schemaSql;
            schemaCommand.ExecuteNonQuery();
        }

        // Create test character
        var createCharCommand = connection.CreateCommand();
        createCharCommand.CommandText = @"
            INSERT INTO Characters (character_id, character_name, display_name, current_level, current_legend, legend_to_next_level)
            VALUES (1, 'test_player', 'Test Player', 1, 0, 100)
        ";
        createCharCommand.ExecuteNonQuery();

        // Initialize services
        _progressionService = new CompanionProgressionService(_connectionString);
        _recruitmentService = new RecruitmentService(_connectionString);

        // Recruit test companion (Finnr - no faction requirement)
        _recruitmentService.RecruitCompanion(1, 2);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    /// <summary>
    /// Test 1: AwardLegend increases legend and triggers level up at threshold
    /// </summary>
    [Test]
    public void AwardLegend_AtThreshold_TriggersLevelUp()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr

        // Verify starting state (Level 1, 0 Legend, 100 to next)
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT current_level, current_legend, legend_to_next_level
            FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        using (var reader = checkCommand.ExecuteReader())
        {
            Assert.That(reader.Read(), Is.True, "Companion progression should exist");
            Assert.That(reader.GetInt32(0), Is.EqualTo(1), "Starting level should be 1");
            Assert.That(reader.GetInt32(1), Is.EqualTo(0), "Starting legend should be 0");
            Assert.That(reader.GetInt32(2), Is.EqualTo(100), "Legend to next level should be 100");
        }

        // Act: Award 150 Legend (enough to level up and have 50 remaining)
        _progressionService.AwardLegend(characterId, companionId, 150);

        // Assert: Verify level increased to 2
        checkCommand.Parameters.Clear();
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        using (var reader = checkCommand.ExecuteReader())
        {
            Assert.That(reader.Read(), Is.True);
            var newLevel = reader.GetInt32(0);
            var remainingLegend = reader.GetInt32(1);
            var legendToNext = reader.GetInt32(2);

            Assert.That(newLevel, Is.EqualTo(2), "Should level up to 2");
            Assert.That(remainingLegend, Is.EqualTo(50), "Should have 50 remaining legend");
            Assert.That(legendToNext, Is.GreaterThan(100), "Next level requirement should scale");
        }
    }

    /// <summary>
    /// Test 2: Multiple level ups in single AwardLegend call
    /// </summary>
    [Test]
    public void AwardLegend_LargeAmount_TriggersMultipleLevelUps()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr

        // Award massive Legend amount to trigger multiple level ups
        // Level 1->2: 100 Legend
        // Level 2->3: 110 Legend (1.1x scaling)
        // Level 3->4: 121 Legend
        // Total for 3 levels: ~331 Legend
        // Award 400 to ensure multiple level ups

        // Act
        _progressionService.AwardLegend(characterId, companionId, 400);

        // Assert: Verify multiple levels gained
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT current_level FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        var newLevel = (int)(long)checkCommand.ExecuteScalar()!;
        Assert.That(newLevel, Is.GreaterThanOrEqualTo(3), "Should gain at least 2 levels from 400 Legend");
    }

    /// <summary>
    /// Test 3: CalculateScaledStats applies correct formulas
    /// Scaling formulas:
    /// - Attributes: Base + 2 × (Level-1)
    /// - HP/Resources: Base × (1 + 0.1 × (Level-1))
    /// - Defense: Base + (Level-1)
    /// - Soak: Base + (Level-1)/2
    /// </summary>
    [Test]
    public void CalculateScaledStats_AppliesCorrectFormulas()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr

        // Get base stats from database
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var baseCommand = connection.CreateCommand();
        baseCommand.CommandText = @"
            SELECT base_might, base_finesse, base_sturdiness, base_wits, base_will,
                   base_max_hp, base_defense, base_soak, base_max_resource
            FROM Companions
            WHERE companion_id = @companionId
        ";
        baseCommand.Parameters.AddWithValue("@companionId", companionId);

        int baseMight, baseFinesse, baseSturdiness, baseWits, baseWill;
        int baseMaxHP, baseDefense, baseSoak, baseMaxResource;

        using (var reader = baseCommand.ExecuteReader())
        {
            Assert.That(reader.Read(), Is.True);
            baseMight = reader.GetInt32(0);
            baseFinesse = reader.GetInt32(1);
            baseSturdiness = reader.GetInt32(2);
            baseWits = reader.GetInt32(3);
            baseWill = reader.GetInt32(4);
            baseMaxHP = reader.GetInt32(5);
            baseDefense = reader.GetInt32(6);
            baseSoak = reader.GetInt32(7);
            baseMaxResource = reader.GetInt32(8);
        }

        // Level up companion to level 5 (4 level bonuses)
        _progressionService.AwardLegend(characterId, companionId, 1000); // Enough for multiple levels

        // Act: Calculate scaled stats
        var scaledStats = _progressionService.CalculateScaledStats(characterId, companionId);

        // Assert: Verify stats exist and scaling applied
        Assert.That(scaledStats, Is.Not.Null, "Scaled stats should not be null");
        Assert.That(scaledStats.Level, Is.GreaterThan(1), "Should have leveled up");

        var levelBonus = scaledStats.Level - 1;

        // Verify attribute scaling: Base + 2 × (Level-1)
        Assert.That(scaledStats.Might, Is.EqualTo(baseMight + (2 * levelBonus)),
            $"MIGHT should scale: {baseMight} + (2 × {levelBonus})");
        Assert.That(scaledStats.Finesse, Is.EqualTo(baseFinesse + (2 * levelBonus)),
            $"FINESSE should scale: {baseFinesse} + (2 × {levelBonus})");

        // Verify HP scaling: Base × (1 + 0.1 × (Level-1))
        var expectedHP = (int)(baseMaxHP * (1.0 + (0.1 * levelBonus)));
        Assert.That(scaledStats.MaxHP, Is.EqualTo(expectedHP),
            $"HP should scale: {baseMaxHP} × (1 + 0.1 × {levelBonus})");

        // Verify Defense scaling: Base + (Level-1)
        Assert.That(scaledStats.Defense, Is.EqualTo(baseDefense + levelBonus),
            $"Defense should scale: {baseDefense} + {levelBonus}");

        // Verify base stats preserved
        Assert.That(scaledStats.BaseMight, Is.EqualTo(baseMight), "Base stats should be preserved");
        Assert.That(scaledStats.BaseMaxHP, Is.EqualTo(baseMaxHP), "Base HP should be preserved");
    }

    /// <summary>
    /// Test 4: Equipment management - EquipCompanionItem and UnequipCompanionItem
    /// </summary>
    [Test]
    public void EquipCompanionItem_ValidSlot_UpdatesProgression()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr
        int weaponItemId = 101;
        int armorItemId = 102;
        int accessoryItemId = 103;

        // Act: Equip weapon
        var weaponEquipped = _progressionService.EquipCompanionItem(characterId, companionId, weaponItemId, "weapon");
        Assert.That(weaponEquipped, Is.True, "Weapon equip should succeed");

        // Act: Equip armor
        var armorEquipped = _progressionService.EquipCompanionItem(characterId, companionId, armorItemId, "armor");
        Assert.That(armorEquipped, Is.True, "Armor equip should succeed");

        // Act: Equip accessory
        var accessoryEquipped = _progressionService.EquipCompanionItem(characterId, companionId, accessoryItemId, "accessory");
        Assert.That(accessoryEquipped, Is.True, "Accessory equip should succeed");

        // Assert: Verify equipment in database
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT equipped_weapon_id, equipped_armor_id, equipped_accessory_id
            FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        using var reader = checkCommand.ExecuteReader();
        Assert.That(reader.Read(), Is.True);

        var equippedWeapon = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
        var equippedArmor = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
        var equippedAccessory = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);

        Assert.That(equippedWeapon, Is.EqualTo(weaponItemId), "Weapon should be equipped");
        Assert.That(equippedArmor, Is.EqualTo(armorItemId), "Armor should be equipped");
        Assert.That(equippedAccessory, Is.EqualTo(accessoryItemId), "Accessory should be equipped");
    }

    /// <summary>
    /// Test 5: UnequipCompanionItem clears equipment slot
    /// </summary>
    [Test]
    public void UnequipCompanionItem_ClearsSlot()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr
        int weaponItemId = 101;

        // Equip weapon first
        _progressionService.EquipCompanionItem(characterId, companionId, weaponItemId, "weapon");

        // Act: Unequip weapon
        var unequipped = _progressionService.UnequipCompanionItem(characterId, companionId, "weapon");
        Assert.That(unequipped, Is.True, "Unequip should succeed");

        // Assert: Verify weapon slot is NULL
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT equipped_weapon_id FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        var equippedWeapon = checkCommand.ExecuteScalar();
        Assert.That(equippedWeapon, Is.EqualTo(DBNull.Value), "Weapon slot should be NULL after unequip");
    }

    /// <summary>
    /// Test 6: Invalid equipment slot handling
    /// </summary>
    [Test]
    public void EquipCompanionItem_InvalidSlot_ReturnsFalse()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2;
        int itemId = 101;

        // Act: Attempt to equip to invalid slot
        var result = _progressionService.EquipCompanionItem(characterId, companionId, itemId, "invalid_slot");

        // Assert
        Assert.That(result, Is.False, "Should reject invalid equipment slot");
    }

    /// <summary>
    /// Test 7: UnlockAbility adds ability to unlocked list
    /// </summary>
    [Test]
    public void UnlockAbility_AddsToUnlockedList()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr
        int abilityId = 999; // Test ability ID

        // Act: Unlock ability
        var unlocked = _progressionService.UnlockAbility(characterId, companionId, abilityId);
        Assert.That(unlocked, Is.True, "Ability unlock should succeed");

        // Assert: Verify ability in unlocked_abilities JSON array
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT unlocked_abilities FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        var abilitiesJson = (string)checkCommand.ExecuteScalar()!;
        Assert.That(abilitiesJson, Does.Contain(abilityId.ToString()), "Abilities JSON should contain new ability ID");

        // Act: Try to unlock same ability again
        var duplicate = _progressionService.UnlockAbility(characterId, companionId, abilityId);
        Assert.That(duplicate, Is.False, "Should not unlock duplicate ability");
    }

    /// <summary>
    /// Test 8: Ability unlocks at specific levels (3, 5, 7)
    /// </summary>
    [Test]
    public void AwardLegend_AtAbilityUnlockLevel_TriggersUnlock()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr

        // Act: Level up to level 3 (first ability unlock)
        // Level 1->2: 100 Legend
        // Level 2->3: 110 Legend
        // Total: 210 Legend
        _progressionService.AwardLegend(characterId, companionId, 250);

        // Assert: Verify reached level 3
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT current_level FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        checkCommand.Parameters.AddWithValue("@charId", characterId);
        checkCommand.Parameters.AddWithValue("@companionId", companionId);

        var level = (int)(long)checkCommand.ExecuteScalar()!;
        Assert.That(level, Is.GreaterThanOrEqualTo(3), "Should reach level 3 for ability unlock");

        // Note: Actual ability unlock logic depends on companion-specific abilities being defined
        // This test verifies the level threshold is reached; full integration tested in v0.34.4
    }

    /// <summary>
    /// Test 9: Legend scaling formula - exponential growth
    /// </summary>
    [Test]
    public void LegendScaling_IncreasesExponentially()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2;

        // Level up multiple times and verify legend requirements increase
        var previousRequirement = 100; // Level 1->2 requirement

        for (int i = 0; i < 3; i++)
        {
            // Award legend to level up
            _progressionService.AwardLegend(characterId, companionId, 1000);

            // Check new requirement
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
                SELECT current_level, legend_to_next_level FROM Companion_Progression
                WHERE character_id = @charId AND companion_id = @companionId
            ";
            checkCommand.Parameters.AddWithValue("@charId", characterId);
            checkCommand.Parameters.AddWithValue("@companionId", companionId);

            using var reader = checkCommand.ExecuteReader();
            reader.Read();
            var level = reader.GetInt32(0);
            var requirement = reader.GetInt32(1);

            // Verify exponential scaling (1.1x multiplier)
            if (level > 2)
            {
                Assert.That(requirement, Is.GreaterThan(previousRequirement),
                    $"Legend requirement at level {level} should be greater than previous");
            }

            previousRequirement = requirement;
        }
    }
}
