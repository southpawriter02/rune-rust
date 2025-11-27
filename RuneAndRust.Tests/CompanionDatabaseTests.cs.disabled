using NUnit.Framework;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.34.1: Test suite for Companion System database schema
/// Validates table creation, seeding, constraints, and foreign keys
/// </summary>
[TestFixture]
public class CompanionDatabaseTests
{
    private SaveRepository _repository;
    private string _testDbDirectory;
    private string _testDbPath;

    [SetUp]
    public void Setup()
    {
        // Create a unique test database for each test
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"companion_test_{Guid.NewGuid()}");
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
    public void CompanionTables_AfterInitialization_AllTablesExist()
    {
        // Arrange & Act (tables created in Setup)
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT name FROM sqlite_master
            WHERE type='table' AND name LIKE '%Companion%'
            ORDER BY name
        ";

        var tables = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        // Assert
        Assert.That(tables, Has.Count.EqualTo(5), "Expected 5 companion-related tables");
        Assert.That(tables, Does.Contain("Companions"));
        Assert.That(tables, Does.Contain("Characters_Companions"));
        Assert.That(tables, Does.Contain("Companion_Progression"));
        Assert.That(tables, Does.Contain("Companion_Quests"));
        Assert.That(tables, Does.Contain("Companion_Abilities"));
    }

    [Test]
    public void Companions_AfterSeeding_HasSixCompanions()
    {
        // Arrange & Act (data seeded in Setup)
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Companions";
        var count = (long)command.ExecuteScalar()!;

        // Assert
        Assert.That(count, Is.EqualTo(6), "Expected 6 recruitable companions");
    }

    [Test]
    public void CompanionAbilities_AfterSeeding_Has18Abilities()
    {
        // Arrange & Act (data seeded in Setup)
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Companion_Abilities";
        var count = (long)command.ExecuteScalar()!;

        // Assert
        Assert.That(count, Is.EqualTo(18), "Expected 18 companion abilities (3 per companion)");
    }

    [Test]
    public void Companions_SeededData_ContainsKaraIronbreaker()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT companion_id, companion_name, display_name, archetype, combat_role
            FROM Companions
            WHERE companion_id = 34001
        ";

        using var reader = command.ExecuteReader();
        Assert.That(reader.Read(), Is.True, "Kára Ironbreaker should exist");

        // Assert
        Assert.That(reader.GetInt32(0), Is.EqualTo(34001));
        Assert.That(reader.GetString(1), Is.EqualTo("Kara_Ironbreaker"));
        Assert.That(reader.GetString(2), Is.EqualTo("Kára Ironbreaker"));
        Assert.That(reader.GetString(3), Is.EqualTo("Warrior"));
        Assert.That(reader.GetString(4), Is.EqualTo("Tank"));
    }

    [Test]
    public void Companions_WithFactionRequirements_HasCorrectData()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT companion_name, required_faction, required_reputation_value
            FROM Companions
            WHERE required_faction IS NOT NULL
            ORDER BY companion_name
        ";

        var factionRequirements = new List<(string name, string faction, long rep)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            factionRequirements.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt64(2)
            ));
        }

        // Assert
        Assert.That(factionRequirements, Has.Count.EqualTo(4), "4 companions have faction requirements");
        Assert.That(factionRequirements, Has.Some.Matches<(string, string, long)>(
            x => x.faction == "Iron-Bane" && x.rep == 25));
        Assert.That(factionRequirements, Has.Some.Matches<(string, string, long)>(
            x => x.faction == "Jotun-Reader" && x.rep == 25));
        Assert.That(factionRequirements, Has.Some.Matches<(string, string, long)>(
            x => x.faction == "Rust-Clan" && x.rep == 0));
        Assert.That(factionRequirements, Has.Some.Matches<(string, string, long)>(
            x => x.faction == "God-Sleeper" && x.rep == 25));
    }

    [Test]
    public void CompanionAbilities_ForKara_HasThreeAbilities()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ability_id, ability_name, resource_cost
            FROM Companion_Abilities
            WHERE owner = 'Kara_Ironbreaker'
            ORDER BY ability_id
        ";

        var abilities = new List<(long id, string name, long cost)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            abilities.Add((
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetInt64(2)
            ));
        }

        // Assert
        Assert.That(abilities, Has.Count.EqualTo(3));
        Assert.That(abilities[0].name, Is.EqualTo("Shield Bash"));
        Assert.That(abilities[1].name, Is.EqualTo("Taunt"));
        Assert.That(abilities[2].name, Is.EqualTo("Purification Strike"));
    }

    [Test]
    public void Companions_Archetypes_CorrectDistribution()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT archetype, COUNT(*) as count
            FROM Companions
            GROUP BY archetype
            ORDER BY archetype
        ";

        var archetypeCounts = new Dictionary<string, long>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            archetypeCounts[reader.GetString(0)] = reader.GetInt64(1);
        }

        // Assert
        Assert.That(archetypeCounts["Warrior"], Is.EqualTo(3), "Expected 3 Warriors");
        Assert.That(archetypeCounts["Adept"], Is.EqualTo(1), "Expected 1 Adept");
        Assert.That(archetypeCounts["Mystic"], Is.EqualTo(2), "Expected 2 Mystics");
    }

    [Test]
    public void CompanionAbilities_PassiveAbilities_HaveZeroCost()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ability_name, resource_cost
            FROM Companion_Abilities
            WHERE range_type = 'passive'
        ";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var abilityName = reader.GetString(0);
            var cost = reader.GetInt64(1);

            // Assert
            Assert.That(cost, Is.EqualTo(0), $"Passive ability {abilityName} should have 0 cost");
        }
    }

    [Test]
    public void Companions_Indices_AreCreated()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT name FROM sqlite_master
            WHERE type='index' AND name LIKE 'idx_companion%'
            ORDER BY name
        ";

        var indices = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            indices.Add(reader.GetString(0));
        }

        // Assert
        Assert.That(indices, Has.Count.GreaterThanOrEqualTo(5), "Expected at least 5 companion indices");
        Assert.That(indices, Does.Contain("idx_companions_faction"));
        Assert.That(indices, Does.Contain("idx_companions_location"));
        Assert.That(indices, Does.Contain("idx_companion_abilities_owner"));
    }

    [Test]
    public void CharactersCompanions_ForeignKeyConstraints_AreEnforced()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        // Enable foreign keys
        var pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
        pragmaCommand.ExecuteNonQuery();

        // Act & Assert - Try to insert with invalid companion_id
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Characters_Companions
            (character_id, companion_id, current_hp, current_resource)
            VALUES (999, 99999, 100, 100)
        ";

        // Should throw foreign key constraint violation
        Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());
    }

    [Test]
    public void CompanionProgression_DefaultValues_AreCorrect()
    {
        // Arrange
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        // Act - Insert a progression record with minimal data
        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO Companion_Progression (character_id, companion_id)
            VALUES (1, 34001)
        ";
        insertCommand.ExecuteNonQuery();

        // Verify defaults
        var selectCommand = connection.CreateCommand();
        selectCommand.CommandText = @"
            SELECT current_level, current_legend, legend_to_next_level, unlocked_abilities
            FROM Companion_Progression
            WHERE character_id = 1 AND companion_id = 34001
        ";

        using var reader = selectCommand.ExecuteReader();
        Assert.That(reader.Read(), Is.True);

        // Assert
        Assert.That(reader.GetInt64(0), Is.EqualTo(1), "Default level should be 1");
        Assert.That(reader.GetInt64(1), Is.EqualTo(0), "Default legend should be 0");
        Assert.That(reader.GetInt64(2), Is.EqualTo(100), "Default legend_to_next_level should be 100");
        Assert.That(reader.GetString(3), Is.EqualTo("[]"), "Default unlocked_abilities should be empty array");
    }

    [Test]
    public void Companions_StartingAbilities_JsonArrayIsValid()
    {
        // Arrange & Act
        using var connection = new SqliteConnection($"Data Source={_testDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT companion_name, starting_abilities
            FROM Companions
        ";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var companionName = reader.GetString(0);
            var startingAbilities = reader.GetString(1);

            // Assert - Should be valid JSON array
            Assert.That(startingAbilities, Does.StartWith("["));
            Assert.That(startingAbilities, Does.EndWith("]"));

            // Parse to verify it's valid JSON
            var abilities = System.Text.Json.JsonSerializer.Deserialize<int[]>(startingAbilities);
            Assert.That(abilities, Has.Length.EqualTo(3), $"{companionName} should have 3 starting abilities");
        }
    }
}
