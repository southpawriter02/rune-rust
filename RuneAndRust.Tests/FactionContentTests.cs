using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using System.IO;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.33.3: Test suite for faction quests and rewards integration
/// Validates quest-faction linkage and reward availability
/// </summary>
[TestFixture]
public class FactionContentTests
{
    private FactionService _factionService;
    private SaveRepository _saveRepository;
    private string _testDbDirectory;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        // Create unique test database
        _testDbDirectory = Path.Combine(Path.GetTempPath(), $"faction_content_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbDirectory);

        // Initialize database schema
        _saveRepository = new SaveRepository(_testDbDirectory);
        _connectionString = $"Data Source={Path.Combine(_testDbDirectory, "runeandrust.db")}";

        // Run v0.33.3 content seeding
        SeedFactionContent();

        _factionService = new FactionService(_connectionString);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDbDirectory))
        {
            Directory.Delete(_testDbDirectory, true);
        }
    }

    private void SeedFactionContent()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Read and execute the v0.33.3 SQL script
        var sqlScript = File.ReadAllText("/home/user/rune-rust/Data/v0.33.3_faction_content.sql");

        // Split by transaction boundaries and execute
        var commands = sqlScript.Split(new[] { "BEGIN TRANSACTION;", "COMMIT;" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var commandBlock in commands)
        {
            if (string.IsNullOrWhiteSpace(commandBlock)) continue;

            var statements = commandBlock.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var statement in statements)
            {
                var trimmed = statement.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--")) continue;

                try
                {
                    var command = connection.CreateCommand();
                    command.CommandText = trimmed;
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Skip comments and empty statements
                }
            }
        }
    }

    [Test]
    public void FactionQuests_AfterSeeding_Has25QuestsTotal()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Faction_Quests";
        var count = (long)command.ExecuteScalar()!;

        // Assert
        Assert.That(count, Is.EqualTo(25), "Expected 25 faction quests (5 per faction)");
    }

    [Test]
    public void FactionQuests_EachFaction_Has5Quests()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        for (int factionId = 1; factionId <= 5; factionId++)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Faction_Quests WHERE faction_id = @factionId";
            command.Parameters.AddWithValue("@factionId", factionId);
            var count = (long)command.ExecuteScalar()!;

            Assert.That(count, Is.EqualTo(5), $"Faction {factionId} should have 5 quests");
        }
    }

    [Test]
    public void FactionRewards_AfterSeeding_HasAtLeast15Rewards()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Faction_Rewards";
        var count = (long)command.ExecuteScalar()!;

        // Assert
        Assert.That(count, Is.GreaterThanOrEqualTo(15), "Expected at least 15 faction rewards");
    }

    [Test]
    public void FactionRewards_EachFaction_HasMultipleRewards()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        for (int factionId = 1; factionId <= 5; factionId++)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Faction_Rewards WHERE faction_id = @factionId";
            command.Parameters.AddWithValue("@factionId", factionId);
            var count = (long)command.ExecuteScalar()!;

            Assert.That(count, Is.GreaterThanOrEqualTo(2), $"Faction {factionId} should have at least 2 rewards");
        }
    }

    [Test]
    public void GetAvailableFactionQuests_WithNoReputation_ReturnsBasicQuests()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes

        // Act
        var quests = _factionService.GetAvailableFactionQuests(characterId, factionId);

        // Assert
        Assert.That(quests.Count, Is.GreaterThan(0), "Should have quests available at 0 reputation");

        // Verify these are the basic quests (required_reputation = 0)
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Faction_Quests WHERE faction_id = @factionId AND required_reputation = 0";
        command.Parameters.AddWithValue("@factionId", factionId);
        var expectedCount = (long)command.ExecuteScalar()!;

        Assert.That(quests.Count, Is.EqualTo((int)expectedCount));
    }

    [Test]
    public void GetAvailableFactionQuests_WithFriendlyReputation_ReturnsMoreQuests()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes
        var reputationService = new ReputationService(_connectionString);

        // Build reputation to Friendly tier (+30)
        reputationService.ModifyReputation(characterId, factionId, 30, "Test");

        // Act
        var quests = _factionService.GetAvailableFactionQuests(characterId, factionId);

        // Assert - Should include quests requiring 0 and 25 reputation
        Assert.That(quests.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void GetAvailableFactionRewards_WithNoReputation_ReturnsLimitedRewards()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes

        // Act
        var rewards = _factionService.GetAvailableFactionRewards(characterId, factionId);

        // Assert - With 0 reputation, should only get rewards requiring 0 or less
        Assert.That(rewards, Is.Not.Null);
        Assert.That(rewards.All(r => r.RequiredReputation <= 0), Is.True);
    }

    [Test]
    public void GetAvailableFactionRewards_WithFriendlyReputation_ReturnsMoreRewards()
    {
        // Arrange
        int characterId = 1;
        int factionId = 1; // Iron-Banes
        var reputationService = new ReputationService(_connectionString);

        // Build reputation to Friendly tier (+30)
        reputationService.ModifyReputation(characterId, factionId, 30, "Test");

        // Act
        var rewards = _factionService.GetAvailableFactionRewards(characterId, factionId);

        // Assert
        Assert.That(rewards.Count, Is.GreaterThan(0));
        Assert.That(rewards.Any(r => r.RequiredReputation == 25), Is.True);
    }

    [Test]
    public void FactionQuests_IronBanes_HasCorrectReputationGates()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT required_reputation, COUNT(*) as count
            FROM Faction_Quests
            WHERE faction_id = 1
            GROUP BY required_reputation
            ORDER BY required_reputation
        ";

        var gates = new Dictionary<int, int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            gates[reader.GetInt32(0)] = reader.GetInt32(1);
        }

        // Assert - Should have quests at reputation gates: 0, 25, 50, 75
        Assert.That(gates.ContainsKey(0), Is.True, "Should have basic quests at 0 reputation");
        Assert.That(gates.ContainsKey(25), Is.True, "Should have Friendly quests at 25 reputation");
        Assert.That(gates.ContainsKey(50), Is.True, "Should have Allied quests at 50 reputation");
        Assert.That(gates.ContainsKey(75), Is.True, "Should have Exalted quests at 75 reputation");
    }

    [Test]
    public void FactionRewards_HasDiverseRewardTypes()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT reward_type FROM Faction_Rewards";

        var rewardTypes = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rewardTypes.Add(reader.GetString(0));
        }

        // Assert - Should have multiple reward types
        Assert.That(rewardTypes.Count, Is.GreaterThanOrEqualTo(3), "Should have at least 3 different reward types");
        Assert.That(rewardTypes, Does.Contain("Equipment").Or.Contain("Ability").Or.Contain("Service"));
    }

    [Test]
    public void RepeatableQuests_AreProperlyFlagged()
    {
        // Arrange & Act
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Faction_Quests WHERE is_repeatable = 1";
        var repeatableCount = (long)command.ExecuteScalar()!;

        // Assert - Should have some repeatable quests for reputation grinding
        Assert.That(repeatableCount, Is.GreaterThan(0), "Should have some repeatable quests");
    }

    [Test]
    public void IndependentsFaction_HasLoneWolfReward()
    {
        // Arrange
        int characterId = 1;
        int independentsFactionId = 5;
        var reputationService = new ReputationService(_connectionString);

        // Build to Exalted (100 reputation)
        reputationService.ModifyReputation(characterId, independentsFactionId, 100, "Test");

        // Act
        var rewards = _factionService.GetAvailableFactionRewards(characterId, independentsFactionId);

        // Assert
        Assert.That(rewards.Any(r => r.RewardName.Contains("Lone Wolf")), Is.True,
            "Independents should have Lone Wolf trait at Exalted");
    }
}
