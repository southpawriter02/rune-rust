using NUnit.Framework;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using RuneAndRust.Core;
using Microsoft.Data.Sqlite;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.34.3: Tests for RecruitmentService
/// Validates faction-gated recruitment, party management, and personal quest unlocking
/// </summary>
[TestFixture]
public class RecruitmentServiceTests
{
    private string _testDbPath = null!;
    private string _connectionString = null!;
    private SaveRepository _saveRepo = null!;
    private RecruitmentService _recruitmentService = null!;
    private ReputationService _reputationService = null!;

    [SetUp]
    public void Setup()
    {
        // Create unique test database for each test
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_recruitment_{Guid.NewGuid()}.db");
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
        _recruitmentService = new RecruitmentService(_connectionString);
        _reputationService = new ReputationService(_connectionString);
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
    /// Test 1: Faction reputation requirements block recruitment
    /// </summary>
    [Test]
    public void CanRecruitCompanion_InsufficientFactionReputation_ReturnsFalse()
    {
        // Arrange
        int characterId = 1;
        int companionId = 1; // Kara Ironbreaker (requires Jarnheim Resistance 20)

        // Verify faction requirement exists
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var verifyCommand = connection.CreateCommand();
        verifyCommand.CommandText = @"
            SELECT required_faction, required_reputation_value
            FROM Companions
            WHERE companion_id = @companionId
        ";
        verifyCommand.Parameters.AddWithValue("@companionId", companionId);
        using var reader = verifyCommand.ExecuteReader();
        reader.Read();
        var requiredFaction = reader.GetString(0);
        var requiredValue = reader.GetInt32(1);

        Assert.That(requiredFaction, Is.Not.Null.And.Not.Empty, "Kara should have faction requirement");
        Assert.That(requiredValue, Is.GreaterThan(0), "Kara should have reputation threshold");

        // Act: Attempt recruitment without sufficient reputation
        var canRecruit = _recruitmentService.CanRecruitCompanion(characterId, companionId, out string failureReason);

        // Assert
        Assert.That(canRecruit, Is.False, "Should not allow recruitment without sufficient faction reputation");
        Assert.That(failureReason, Does.Contain("reputation"), "Failure reason should mention reputation");
        Assert.That(failureReason, Does.Contain(requiredFaction), "Failure reason should mention faction name");
    }

    /// <summary>
    /// Test 2: Successful recruitment with sufficient faction reputation
    /// </summary>
    [Test]
    public void RecruitCompanion_WithSufficientReputation_Succeeds()
    {
        // Arrange
        int characterId = 1;
        int companionId = 1; // Kara Ironbreaker (requires Jarnheim Resistance 20)

        // Get Jarnheim Resistance faction ID
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var factionCommand = connection.CreateCommand();
        factionCommand.CommandText = "SELECT faction_id FROM Factions WHERE faction_name = 'Jarnheim Resistance'";
        var factionId = (int)(long)factionCommand.ExecuteScalar()!;

        // Grant sufficient reputation
        _reputationService.ModifyReputation(characterId, factionId, 25); // Above threshold of 20

        // Act: Recruit companion
        var success = _recruitmentService.RecruitCompanion(characterId, companionId);

        // Assert
        Assert.That(success, Is.True, "Recruitment should succeed with sufficient reputation");

        // Verify companion was added to Characters_Companions
        var verifyCommand = connection.CreateCommand();
        verifyCommand.CommandText = @"
            SELECT is_recruited, is_in_party
            FROM Characters_Companions
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        verifyCommand.Parameters.AddWithValue("@charId", characterId);
        verifyCommand.Parameters.AddWithValue("@companionId", companionId);
        using var reader = verifyCommand.ExecuteReader();

        Assert.That(reader.Read(), Is.True, "Companion should exist in Characters_Companions");
        Assert.That(reader.GetInt32(0), Is.EqualTo(1), "is_recruited should be 1");
        Assert.That(reader.GetInt32(1), Is.EqualTo(1), "is_in_party should be 1 (auto-added to party)");
    }

    /// <summary>
    /// Test 3: Party size limit enforcement (max 3 companions)
    /// </summary>
    [Test]
    public void RecruitCompanion_PartyFull_ReturnsFailure()
    {
        // Arrange
        int characterId = 1;

        // Grant high reputation to bypass faction checks
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var factionCommand = connection.CreateCommand();
        factionCommand.CommandText = "SELECT faction_id FROM Factions";
        using var factionReader = factionCommand.ExecuteReader();
        var factionIds = new List<int>();
        while (factionReader.Read())
        {
            factionIds.Add(factionReader.GetInt32(0));
        }
        factionReader.Close();

        foreach (var factionId in factionIds)
        {
            _reputationService.ModifyReputation(characterId, factionId, 100); // Max reputation
        }

        // Recruit 3 companions (filling party)
        _recruitmentService.RecruitCompanion(characterId, 1); // Kara
        _recruitmentService.RecruitCompanion(characterId, 2); // Finnr
        _recruitmentService.RecruitCompanion(characterId, 3); // Bjorn

        // Act: Attempt to recruit 4th companion
        var canRecruit = _recruitmentService.CanRecruitCompanion(characterId, 4, out string failureReason);

        // Assert
        Assert.That(canRecruit, Is.False, "Should not allow recruitment when party is full");
        Assert.That(failureReason, Does.Contain("full"), "Failure reason should mention party full");
        Assert.That(failureReason, Does.Contain("3/3"), "Failure reason should show party size limit");
    }

    /// <summary>
    /// Test 4: Party management - AddToParty and RemoveFromParty
    /// </summary>
    [Test]
    public void PartyManagement_AddAndRemove_WorksCorrectly()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr (no faction requirement)

        // Recruit companion
        _recruitmentService.RecruitCompanion(characterId, companionId);

        // Act: Remove from party
        var removed = _recruitmentService.RemoveFromParty(characterId, companionId);
        Assert.That(removed, Is.True, "RemoveFromParty should succeed");

        // Verify not in party
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var verifyCommand = connection.CreateCommand();
        verifyCommand.CommandText = @"
            SELECT is_in_party FROM Characters_Companions
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        verifyCommand.Parameters.AddWithValue("@charId", characterId);
        verifyCommand.Parameters.AddWithValue("@companionId", companionId);
        var isInParty = (int)(long)verifyCommand.ExecuteScalar()!;
        Assert.That(isInParty, Is.EqualTo(0), "Companion should not be in party after removal");

        // Act: Add back to party
        var added = _recruitmentService.AddToParty(characterId, companionId);
        Assert.That(added, Is.True, "AddToParty should succeed");

        // Verify back in party
        verifyCommand.Parameters.Clear();
        verifyCommand.Parameters.AddWithValue("@charId", characterId);
        verifyCommand.Parameters.AddWithValue("@companionId", companionId);
        isInParty = (int)(long)verifyCommand.ExecuteScalar()!;
        Assert.That(isInParty, Is.EqualTo(1), "Companion should be in party after add");
    }

    /// <summary>
    /// Test 5: Personal quest unlocking on recruitment
    /// </summary>
    [Test]
    public void RecruitCompanion_WithPersonalQuest_UnlocksQuest()
    {
        // Arrange
        int characterId = 1;
        int companionId = 1; // Kara Ironbreaker (has personal quest)

        // Grant sufficient reputation
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var factionCommand = connection.CreateCommand();
        factionCommand.CommandText = "SELECT faction_id FROM Factions WHERE faction_name = 'Jarnheim Resistance'";
        var factionId = (int)(long)factionCommand.ExecuteScalar()!;
        _reputationService.ModifyReputation(characterId, factionId, 25);

        // Verify companion has personal quest
        var questCheckCommand = connection.CreateCommand();
        questCheckCommand.CommandText = @"
            SELECT personal_quest_id, personal_quest_title
            FROM Companions
            WHERE companion_id = @companionId
        ";
        questCheckCommand.Parameters.AddWithValue("@companionId", companionId);
        using var questReader = questCheckCommand.ExecuteReader();
        Assert.That(questReader.Read(), Is.True, "Companion should exist");
        var questId = questReader.IsDBNull(0) ? (int?)null : questReader.GetInt32(0);
        var questTitle = questReader.IsDBNull(1) ? null : questReader.GetString(1);
        questReader.Close();

        // Skip test if companion has no personal quest
        if (!questId.HasValue)
        {
            Assert.Inconclusive("Companion has no personal quest defined");
            return;
        }

        // Act: Recruit companion
        var success = _recruitmentService.RecruitCompanion(characterId, companionId);
        Assert.That(success, Is.True, "Recruitment should succeed");

        // Assert: Verify personal quest was unlocked in Companion_Quests table
        var questUnlockedCommand = connection.CreateCommand();
        questUnlockedCommand.CommandText = @"
            SELECT is_unlocked, quest_id
            FROM Companion_Quests
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        questUnlockedCommand.Parameters.AddWithValue("@charId", characterId);
        questUnlockedCommand.Parameters.AddWithValue("@companionId", companionId);

        using var unlockReader = questUnlockedCommand.ExecuteReader();
        Assert.That(unlockReader.Read(), Is.True, "Personal quest should be unlocked");
        var isUnlocked = unlockReader.GetInt32(0);
        var unlockedQuestId = unlockReader.GetInt32(1);

        Assert.That(isUnlocked, Is.EqualTo(1), "Personal quest should be marked as unlocked");
        Assert.That(unlockedQuestId, Is.EqualTo(questId.Value), "Unlocked quest ID should match companion's personal quest");
    }

    /// <summary>
    /// Test 6: GetRecruitableCompanions filters by location and recruitment status
    /// </summary>
    [Test]
    public void GetRecruitableCompanions_FiltersCorrectly()
    {
        // Arrange
        int characterId = 1;

        // Act: Get all recruitable companions
        var allCompanions = _recruitmentService.GetRecruitableCompanions(characterId);

        // Assert: Should return all 6 companions initially
        Assert.That(allCompanions.Count, Is.GreaterThan(0), "Should have recruitable companions");

        // Recruit one companion
        _recruitmentService.RecruitCompanion(characterId, 2); // Finnr (no faction requirement)

        // Act: Get recruitable companions again
        var remainingCompanions = _recruitmentService.GetRecruitableCompanions(characterId);

        // Assert: Should have one fewer companion
        Assert.That(remainingCompanions.Count, Is.EqualTo(allCompanions.Count - 1),
            "Recruited companion should not appear in recruitable list");
        Assert.That(remainingCompanions.Any(c => c.CompanionID == 2), Is.False,
            "Finnr should not be in recruitable list after recruitment");
    }

    /// <summary>
    /// Test 7: DismissCompanion removes companion permanently
    /// </summary>
    [Test]
    public void DismissCompanion_RemovesFromRecruitedList()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr

        _recruitmentService.RecruitCompanion(characterId, companionId);

        // Act: Dismiss companion
        var dismissed = _recruitmentService.DismissCompanion(characterId, companionId);

        // Assert
        Assert.That(dismissed, Is.True, "Dismiss should succeed");

        // Verify companion removed from Characters_Companions
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var verifyCommand = connection.CreateCommand();
        verifyCommand.CommandText = @"
            SELECT COUNT(*) FROM Characters_Companions
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        verifyCommand.Parameters.AddWithValue("@charId", characterId);
        verifyCommand.Parameters.AddWithValue("@companionId", companionId);
        var count = (long)verifyCommand.ExecuteScalar()!;

        Assert.That(count, Is.EqualTo(0), "Companion should be removed from Characters_Companions");

        // Verify can recruit again
        var canRecruit = _recruitmentService.CanRecruitCompanion(characterId, companionId, out _);
        Assert.That(canRecruit, Is.True, "Should be able to recruit again after dismissal");
    }

    /// <summary>
    /// Test 8: Companion progression entry created on recruitment
    /// </summary>
    [Test]
    public void RecruitCompanion_CreatesProgressionEntry()
    {
        // Arrange
        int characterId = 1;
        int companionId = 2; // Finnr

        // Act: Recruit companion
        _recruitmentService.RecruitCompanion(characterId, companionId);

        // Assert: Verify Companion_Progression entry created
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var verifyCommand = connection.CreateCommand();
        verifyCommand.CommandText = @"
            SELECT current_level, current_legend, legend_to_next_level, unlocked_abilities
            FROM Companion_Progression
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        verifyCommand.Parameters.AddWithValue("@charId", characterId);
        verifyCommand.Parameters.AddWithValue("@companionId", companionId);

        using var reader = verifyCommand.ExecuteReader();
        Assert.That(reader.Read(), Is.True, "Companion_Progression entry should exist");

        var level = reader.GetInt32(0);
        var legend = reader.GetInt32(1);
        var legendToNext = reader.GetInt32(2);
        var abilities = reader.GetString(3);

        Assert.That(level, Is.EqualTo(1), "Starting level should be 1");
        Assert.That(legend, Is.EqualTo(0), "Starting legend should be 0");
        Assert.That(legendToNext, Is.EqualTo(100), "Legend to next level should be 100");
        Assert.That(abilities, Is.Not.Null.And.Not.Empty, "Should have starting abilities");
    }
}
