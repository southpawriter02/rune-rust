using NUnit.Framework;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.33.1: Test suite for Faction System database schema
/// Validates table creation, seeding, constraints, and foreign keys
/// </summary>
[TestFixture]
public class FactionDatabaseTests
{
    private SaveRepository _repository;
    private string _testDbDirectory;
    private string _testDbPath;

    [SetUp]
    public void Setup()
    {
        // Create a unique test database for each test
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"faction_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);
        _testDbPath = Path.Combine(_testDbDirectory, "runeandrust.db");

        // Initialize repository (this will create tables and seed data)
        _repository = new SaveRepository(_testDbDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test database and directory
        if (Directory.Exists(_testDbDirectory))
        {
            Directory.Delete(_testDbDirectory, true);
        }
    }

    [Test]
    public void FactionTables_AfterInitialization_AllTablesExist()
    {
        // Arrange & Act (tables created in Setup)
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT name FROM sqlite_master
            WHERE type='table' AND name LIKE '%Faction%'
            ORDER BY name
        ";

        var tables = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        // Assert
        Assert.That(tables, Has.Count.EqualTo(4), "Expected 4 faction-related tables");
        Assert.That(tables, Does.Contain("Factions"));
        Assert.That(tables, Does.Contain("Characters_FactionReputations"));
        Assert.That(tables, Does.Contain("Faction_Quests"));
        Assert.That(tables, Does.Contain("Faction_Rewards"));
    }

    [Test]
    public void Factions_AfterSeeding_HasFiveFactions()
    {
        // Arrange & Act (data seeded in Setup)
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Factions";
        var count = (long)command.ExecuteScalar()!;

        // Assert
        Assert.That(count, Is.EqualTo(5), "Expected 5 factions to be seeded");
    }

    [Test]
    public void Factions_AfterSeeding_HasCorrectFactionNames()
    {
        // Arrange & Act (data seeded in Setup)
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT faction_name FROM Factions ORDER BY faction_id";

        var factionNames = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            factionNames.Add(reader.GetString(0));
        }

        // Assert
        Assert.That(factionNames, Has.Count.EqualTo(5));
        Assert.That(factionNames[0], Is.EqualTo("IronBanes"));
        Assert.That(factionNames[1], Is.EqualTo("GodSleeperCultists"));
        Assert.That(factionNames[2], Is.EqualTo("JotunReaders"));
        Assert.That(factionNames[3], Is.EqualTo("RustClans"));
        Assert.That(factionNames[4], Is.EqualTo("Independents"));
    }

    [Test]
    public void Factions_IronBanes_HasCorrectPhilosophy()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT philosophy FROM Factions WHERE faction_name = 'IronBanes'";
        var philosophy = (string)command.ExecuteScalar()!;

        // Assert - Should reference purification protocols, not religious language
        Assert.That(philosophy, Does.Contain("purification protocols"));
        Assert.That(philosophy, Does.Contain("corrupted processes"));
        Assert.That(philosophy, Does.Not.Contain("crusade").IgnoreCase);
        Assert.That(philosophy, Does.Not.Contain("holy").IgnoreCase);
    }

    [Test]
    public void FactionReputations_ReputationConstraint_EnforcesValidRange()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        // Create a test character first
        var createCharCommand = connection.CreateCommand();
        createCharCommand.CommandText = @"
            INSERT INTO saves (character_name, class, current_milestone, current_legend, progression_points,
                might, finesse, wits, will, sturdiness, current_hp, max_hp, current_stamina, max_stamina,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated, last_saved)
            VALUES ('TestChar', 'Warrior', 1, 0, 0, 3, 3, 2, 2, 3, 30, 30, 10, 10, 0, '[]', 0, 0, datetime('now'))
        ";
        createCharCommand.ExecuteNonQuery();

        var getCharIdCommand = connection.CreateCommand();
        getCharIdCommand.CommandText = "SELECT id FROM saves WHERE character_name = 'TestChar'";
        var characterId = (long)getCharIdCommand.ExecuteScalar()!;

        // Act & Assert - Valid reputation (should succeed)
        var validCommand = connection.CreateCommand();
        validCommand.CommandText = @"
            INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
            VALUES (@charId, 1, 50, 'Allied')
        ";
        validCommand.Parameters.AddWithValue("@charId", characterId);
        Assert.DoesNotThrow(() => validCommand.ExecuteNonQuery());

        // Act & Assert - Invalid reputation (should fail)
        var invalidCommand = connection.CreateCommand();
        invalidCommand.CommandText = @"
            INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
            VALUES (@charId, 2, 150, 'Exalted')
        ";
        invalidCommand.Parameters.AddWithValue("@charId", characterId);

        var ex = Assert.Throws<SqliteException>(() => invalidCommand.ExecuteNonQuery());
        Assert.That(ex!.Message, Does.Contain("CHECK constraint failed"));
    }

    [Test]
    public void FactionReputations_UniqueConstraint_PreventsDuplicateEntries()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        // Create a test character
        var createCharCommand = connection.CreateCommand();
        createCharCommand.CommandText = @"
            INSERT INTO saves (character_name, class, current_milestone, current_legend, progression_points,
                might, finesse, wits, will, sturdiness, current_hp, max_hp, current_stamina, max_stamina,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated, last_saved)
            VALUES ('UniqueTestChar', 'Warrior', 1, 0, 0, 3, 3, 2, 2, 3, 30, 30, 10, 10, 0, '[]', 0, 0, datetime('now'))
        ";
        createCharCommand.ExecuteNonQuery();

        var getCharIdCommand = connection.CreateCommand();
        getCharIdCommand.CommandText = "SELECT id FROM saves WHERE character_name = 'UniqueTestChar'";
        var characterId = (long)getCharIdCommand.ExecuteScalar()!;

        // Act - Insert first reputation entry (should succeed)
        var firstInsert = connection.CreateCommand();
        firstInsert.CommandText = @"
            INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
            VALUES (@charId, 1, 25, 'Friendly')
        ";
        firstInsert.Parameters.AddWithValue("@charId", characterId);
        Assert.DoesNotThrow(() => firstInsert.ExecuteNonQuery());

        // Assert - Second insert for same character/faction pair should fail
        var secondInsert = connection.CreateCommand();
        secondInsert.CommandText = @"
            INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
            VALUES (@charId, 1, 50, 'Allied')
        ";
        secondInsert.Parameters.AddWithValue("@charId", characterId);

        var ex = Assert.Throws<SqliteException>(() => secondInsert.ExecuteNonQuery());
        Assert.That(ex!.Message, Does.Contain("UNIQUE constraint failed"));
    }

    [Test]
    public void FactionIndexes_AfterCreation_AllIndexesExist()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT name FROM sqlite_master
            WHERE type='index' AND name LIKE '%faction%'
            ORDER BY name
        ";

        var indexes = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            indexes.Add(reader.GetString(0));
        }

        // Assert - Should have at least 10 faction-related indexes
        Assert.That(indexes, Has.Count.GreaterThanOrEqualTo(10),
            "Expected at least 10 faction-related indexes");
        Assert.That(indexes, Does.Contain("idx_factions_name"));
        Assert.That(indexes, Does.Contain("idx_char_faction_rep_character"));
        Assert.That(indexes, Does.Contain("idx_char_faction_rep_faction"));
        Assert.That(indexes, Does.Contain("idx_faction_quests_faction"));
        Assert.That(indexes, Does.Contain("idx_faction_rewards_faction"));
    }

    [Test]
    public void FactionQuests_ForeignKey_EnforcesFactionsTable()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();
        connection.Execute("PRAGMA foreign_keys = ON");

        // Act & Assert - Valid faction_id (should succeed)
        var validCommand = connection.CreateCommand();
        validCommand.CommandText = @"
            INSERT INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward)
            VALUES ('test_quest', 1, 0, 10)
        ";
        Assert.DoesNotThrow(() => validCommand.ExecuteNonQuery());

        // Invalid faction_id (should fail with foreign key constraint)
        var invalidCommand = connection.CreateCommand();
        invalidCommand.CommandText = @"
            INSERT INTO Faction_Quests (quest_id, faction_id, required_reputation, reputation_reward)
            VALUES ('invalid_quest', 999, 0, 10)
        ";

        var ex = Assert.Throws<SqliteException>(() => invalidCommand.ExecuteNonQuery());
        Assert.That(ex!.Message, Does.Contain("FOREIGN KEY constraint failed"));
    }

    [Test]
    public void FactionRewards_RewardTypeConstraint_EnforcesValidTypes()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        // Act & Assert - Valid reward type (should succeed)
        var validCommand = connection.CreateCommand();
        validCommand.CommandText = @"
            INSERT INTO Faction_Rewards (faction_id, reward_type, reward_name, required_reputation)
            VALUES (1, 'Equipment', 'Purification Sigil', 25)
        ";
        Assert.DoesNotThrow(() => validCommand.ExecuteNonQuery());

        // Invalid reward type (should fail)
        var invalidCommand = connection.CreateCommand();
        invalidCommand.CommandText = @"
            INSERT INTO Faction_Rewards (faction_id, reward_type, reward_name, required_reputation)
            VALUES (1, 'InvalidType', 'Test Item', 25)
        ";

        var ex = Assert.Throws<SqliteException>(() => invalidCommand.ExecuteNonQuery());
        Assert.That(ex!.Message, Does.Contain("CHECK constraint failed"));
    }

    [Test]
    public void FactionReputations_CascadeDelete_DeletesWithCharacter()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        // Create test character
        var createCharCommand = connection.CreateCommand();
        createCharCommand.CommandText = @"
            INSERT INTO saves (character_name, class, current_milestone, current_legend, progression_points,
                might, finesse, wits, will, sturdiness, current_hp, max_hp, current_stamina, max_stamina,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated, last_saved)
            VALUES ('DeleteTestChar', 'Warrior', 1, 0, 0, 3, 3, 2, 2, 3, 30, 30, 10, 10, 0, '[]', 0, 0, datetime('now'))
        ";
        createCharCommand.ExecuteNonQuery();

        var getCharIdCommand = connection.CreateCommand();
        getCharIdCommand.CommandText = "SELECT id FROM saves WHERE character_name = 'DeleteTestChar'";
        var characterId = (long)getCharIdCommand.ExecuteScalar()!;

        // Add reputation entries
        var insertRepCommand = connection.CreateCommand();
        insertRepCommand.CommandText = @"
            INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier)
            VALUES (@charId, 1, 50, 'Allied')
        ";
        insertRepCommand.Parameters.AddWithValue("@charId", characterId);
        insertRepCommand.ExecuteNonQuery();

        // Verify reputation exists
        var countBeforeCommand = connection.CreateCommand();
        countBeforeCommand.CommandText = "SELECT COUNT(*) FROM Characters_FactionReputations WHERE character_id = @charId";
        countBeforeCommand.Parameters.AddWithValue("@charId", characterId);
        var countBefore = (long)countBeforeCommand.ExecuteScalar()!;
        Assert.That(countBefore, Is.EqualTo(1));

        // Act - Delete character
        var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM saves WHERE id = @charId";
        deleteCommand.Parameters.AddWithValue("@charId", characterId);
        deleteCommand.ExecuteNonQuery();

        // Assert - Reputation entries should be cascade deleted
        var countAfterCommand = connection.CreateCommand();
        countAfterCommand.CommandText = "SELECT COUNT(*) FROM Characters_FactionReputations WHERE character_id = @charId";
        countAfterCommand.Parameters.AddWithValue("@charId", characterId);
        var countAfter = (long)countAfterCommand.ExecuteScalar()!;
        Assert.That(countAfter, Is.EqualTo(0), "Reputation entries should be cascade deleted with character");
    }
}

/// <summary>
/// Extension methods for SqliteConnection to simplify test code
/// </summary>
internal static class SqliteConnectionExtensions
{
    public static void Execute(this SqliteConnection connection, string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
