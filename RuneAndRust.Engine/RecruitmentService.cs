using RuneAndRust.Core;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.34.3: Companion Recruitment Service
/// Handles faction-gated recruitment, party management, and personal quest unlocking
/// </summary>
public class RecruitmentService
{
    private static readonly ILogger _log = Log.ForContext<RecruitmentService>();
    private readonly string _connectionString;
    private readonly ReputationService _reputationService;

    private const int MAX_PARTY_SIZE = 3; // 1 player + 3 companions max

    public RecruitmentService(string connectionString)
    {
        _connectionString = connectionString;
        _reputationService = new ReputationService(connectionString);
    }

    /// <summary>
    /// Checks if a companion can be recruited by the character
    /// Validates faction requirements, party size, and quest completion
    /// </summary>
    public bool CanRecruitCompanion(int characterId, int companionId, out string failureReason)
    {
        _log.Debug("CanRecruitCompanion: CharacterId={CharacterId}, CompanionId={CompanionId}",
            characterId, companionId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 1. Check if companion exists
        var companion = GetCompanion(companionId, connection);
        if (companion == null)
        {
            failureReason = "Companion not found";
            _log.Warning("Companion not found: CompanionId={CompanionId}", companionId);
            return false;
        }

        // 2. Check if already recruited
        if (IsCompanionRecruited(characterId, companionId, connection))
        {
            failureReason = $"{companion.DisplayName} is already recruited";
            _log.Debug("Companion already recruited: CompanionId={CompanionId}", companionId);
            return false;
        }

        // 3. Check party size limit
        var currentPartySize = GetPartySize(characterId, connection);
        if (currentPartySize >= MAX_PARTY_SIZE)
        {
            failureReason = $"Party is full ({currentPartySize}/{MAX_PARTY_SIZE} companions)";
            _log.Debug("Party size limit reached: CharacterId={CharacterId}, Size={Size}",
                characterId, currentPartySize);
            return false;
        }

        // 4. Check faction reputation requirement
        if (!string.IsNullOrEmpty(companion.RequiredFaction) && companion.RequiredReputationValue.HasValue)
        {
            var factionId = GetFactionId(companion.RequiredFaction, connection);
            if (factionId.HasValue)
            {
                var reputation = _reputationService.GetFactionReputation(characterId, factionId.Value);
                var currentRepValue = reputation?.ReputationValue ?? 0;

                if (currentRepValue < companion.RequiredReputationValue.Value)
                {
                    failureReason = $"Requires {companion.RequiredFaction} reputation {companion.RequiredReputationValue.Value} (current: {currentRepValue})";
                    _log.Debug("Insufficient faction reputation: CharacterId={CharacterId}, Faction={Faction}, Required={Required}, Current={Current}",
                        characterId, companion.RequiredFaction, companion.RequiredReputationValue.Value, currentRepValue);
                    return false;
                }
            }
        }

        // 5. Check recruitment quest requirement (if specified)
        // Simplified: Assumes quest completion is tracked elsewhere
        // Future enhancement: Check quest_id completion in Quests system

        failureReason = string.Empty;
        _log.Information("Companion can be recruited: CharacterId={CharacterId}, CompanionId={CompanionId}, DisplayName={DisplayName}",
            characterId, companionId, companion.DisplayName);
        return true;
    }

    /// <summary>
    /// Recruits a companion and adds them to the character's party
    /// Unlocks personal quest if available
    /// </summary>
    public bool RecruitCompanion(int characterId, int companionId)
    {
        if (!CanRecruitCompanion(characterId, companionId, out string failureReason))
        {
            _log.Warning("Cannot recruit companion: {Reason}", failureReason);
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var companion = GetCompanion(companionId, connection);
        if (companion == null)
        {
            return false;
        }

        // Insert into Characters_Companions
        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO Characters_Companions
            (character_id, companion_id, is_recruited, recruited_at, is_in_party, current_hp, current_resource, current_stance)
            VALUES (@charId, @companionId, 1, @recruitedAt, 1, @maxHp, @maxResource, @stance)
        ";
        insertCommand.Parameters.AddWithValue("@charId", characterId);
        insertCommand.Parameters.AddWithValue("@companionId", companionId);
        insertCommand.Parameters.AddWithValue("@recruitedAt", DateTime.Now.ToString("o"));
        insertCommand.Parameters.AddWithValue("@maxHp", companion.BaseMaxHP);
        insertCommand.Parameters.AddWithValue("@maxResource", companion.BaseMaxResource);
        insertCommand.Parameters.AddWithValue("@stance", companion.DefaultStance);
        insertCommand.ExecuteNonQuery();

        // Insert into Companion_Progression (initial level 1)
        var progressionCommand = connection.CreateCommand();
        progressionCommand.CommandText = @"
            INSERT INTO Companion_Progression
            (character_id, companion_id, current_level, current_legend, legend_to_next_level, unlocked_abilities)
            VALUES (@charId, @companionId, 1, 0, 100, @startingAbilities)
        ";
        progressionCommand.Parameters.AddWithValue("@charId", characterId);
        progressionCommand.Parameters.AddWithValue("@companionId", companionId);
        progressionCommand.Parameters.AddWithValue("@startingAbilities", companion.StartingAbilities);
        progressionCommand.ExecuteNonQuery();

        // Unlock personal quest (if available)
        if (!string.IsNullOrEmpty(companion.PersonalQuestTitle))
        {
            UnlockPersonalQuest(characterId, companionId, connection);
        }

        _log.Information("Companion recruited: CharacterId={CharacterId}, CompanionId={CompanionId}, DisplayName={DisplayName}",
            characterId, companionId, companion.DisplayName);

        return true;
    }

    /// <summary>
    /// Dismisses a companion permanently (removes from recruited companions)
    /// </summary>
    public bool DismissCompanion(int characterId, int companionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM Characters_Companions
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Companion dismissed: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
            return true;
        }

        _log.Warning("Failed to dismiss companion (not recruited): CharacterId={CharacterId}, CompanionId={CompanionId}",
            characterId, companionId);
        return false;
    }

    /// <summary>
    /// Adds a recruited companion to the active party
    /// </summary>
    public bool AddToParty(int characterId, int companionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check party size
        var currentPartySize = GetPartySize(characterId, connection);
        if (currentPartySize >= MAX_PARTY_SIZE)
        {
            _log.Warning("Cannot add to party - party full: CharacterId={CharacterId}, Size={Size}",
                characterId, currentPartySize);
            return false;
        }

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_Companions
            SET is_in_party = 1, updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId AND is_recruited = 1
        ";
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Companion added to party: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
            return true;
        }

        _log.Warning("Failed to add companion to party: CharacterId={CharacterId}, CompanionId={CompanionId}",
            characterId, companionId);
        return false;
    }

    /// <summary>
    /// Removes a companion from active party (but keeps them recruited)
    /// </summary>
    public bool RemoveFromParty(int characterId, int companionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Characters_Companions
            SET is_in_party = 0, updated_at = @updatedAt
            WHERE character_id = @charId AND companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Companion removed from party: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all recruitable companions filtered by location and eligibility
    /// </summary>
    public List<Companion> GetRecruitableCompanions(int characterId, string? currentLocation = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT c.companion_id, c.companion_name, c.display_name, c.archetype, c.faction_affiliation,
                   c.background_summary, c.personality_traits, c.recruitment_location,
                   c.required_faction, c.required_reputation_value, c.combat_role
            FROM Companions c
            LEFT JOIN Characters_Companions cc ON c.companion_id = cc.companion_id AND cc.character_id = @charId
            WHERE cc.companion_id IS NULL
        ";

        if (!string.IsNullOrEmpty(currentLocation))
        {
            query += " AND c.recruitment_location LIKE @location";
        }

        var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddWithValue("@charId", characterId);

        if (!string.IsNullOrEmpty(currentLocation))
        {
            command.Parameters.AddWithValue("@location", $"%{currentLocation}%");
        }

        var companions = new List<Companion>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var companion = new Companion
            {
                CompanionID = reader.GetInt32(0),
                CompanionName = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Archetype = reader.GetString(3),
                FactionAffiliation = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                BackgroundSummary = reader.GetString(5),
                PersonalityTraits = reader.GetString(6),
                RecruitmentLocation = reader.GetString(7),
                RequiredFaction = reader.IsDBNull(8) ? null : reader.GetString(8),
                RequiredReputationValue = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                CombatRole = reader.GetString(10)
            };

            companions.Add(companion);
        }

        _log.Debug("Found {Count} recruitable companions for CharacterId={CharacterId}, Location={Location}",
            companions.Count, characterId, currentLocation ?? "Any");

        return companions;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private CompanionData? GetCompanion(int companionId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT companion_id, companion_name, display_name, archetype, faction_affiliation,
                   required_faction, required_reputation_value, recruitment_quest_id,
                   base_max_hp, base_defense, base_soak, resource_type, base_max_resource,
                   default_stance, starting_abilities, personal_quest_title
            FROM Companions
            WHERE companion_id = @companionId
        ";
        command.Parameters.AddWithValue("@companionId", companionId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new CompanionData
            {
                CompanionID = reader.GetInt32(0),
                CompanionName = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Archetype = reader.GetString(3),
                FactionAffiliation = reader.IsDBNull(4) ? null : reader.GetString(4),
                RequiredFaction = reader.IsDBNull(5) ? null : reader.GetString(5),
                RequiredReputationValue = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                RecruitmentQuestID = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                BaseMaxHP = reader.GetInt32(8),
                BaseDefense = reader.GetInt32(9),
                BaseSoak = reader.GetInt32(10),
                ResourceType = reader.GetString(11),
                BaseMaxResource = reader.GetInt32(12),
                DefaultStance = reader.GetString(13),
                StartingAbilities = reader.GetString(14),
                PersonalQuestTitle = reader.IsDBNull(15) ? null : reader.GetString(15)
            };
        }

        return null;
    }

    private bool IsCompanionRecruited(int characterId, int companionId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM Characters_Companions
            WHERE character_id = @charId AND companion_id = @companionId AND is_recruited = 1
        ";
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);

        var count = (long)command.ExecuteScalar()!;
        return count > 0;
    }

    private int GetPartySize(int characterId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM Characters_Companions
            WHERE character_id = @charId AND is_in_party = 1
        ";
        command.Parameters.AddWithValue("@charId", characterId);

        return (int)(long)command.ExecuteScalar()!;
    }

    private int? GetFactionId(string factionName, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT faction_id
            FROM Factions
            WHERE faction_name = @factionName
        ";
        command.Parameters.AddWithValue("@factionName", factionName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }

        return null;
    }

    private void UnlockPersonalQuest(int characterId, int companionId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Companion_Quests (character_id, companion_id, quest_id, is_unlocked, unlocked_at)
            SELECT @charId, @companionId, c.personal_quest_id, 1, @unlockedAt
            FROM Companions c
            WHERE c.companion_id = @companionId AND c.personal_quest_id IS NOT NULL
        ";
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@companionId", companionId);
        command.Parameters.AddWithValue("@unlockedAt", DateTime.Now.ToString("o"));

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Personal quest unlocked: CharacterId={CharacterId}, CompanionId={CompanionId}",
                characterId, companionId);
        }
    }
}

/// <summary>
/// Internal DTO for companion data from database
/// </summary>
internal class CompanionData
{
    public int CompanionID { get; set; }
    public string CompanionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Archetype { get; set; } = string.Empty;
    public string? FactionAffiliation { get; set; }
    public string? RequiredFaction { get; set; }
    public int? RequiredReputationValue { get; set; }
    public int? RecruitmentQuestID { get; set; }
    public int BaseMaxHP { get; set; }
    public int BaseDefense { get; set; }
    public int BaseSoak { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public int BaseMaxResource { get; set; }
    public string DefaultStance { get; set; } = string.Empty;
    public string StartingAbilities { get; set; } = string.Empty;
    public string? PersonalQuestTitle { get; set; }
}
