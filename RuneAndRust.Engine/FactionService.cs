using RuneAndRust.Core;
using RuneAndRust.Core.Factions;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.33.2: High-level faction management service
/// Orchestrates faction operations, witness system, and world reactions
/// </summary>
public class FactionService
{
    private static readonly ILogger _log = Log.ForContext<FactionService>();
    private readonly ReputationService _reputationService;
    private readonly string _connectionString;

    public FactionService(string connectionString)
    {
        _connectionString = connectionString;
        _reputationService = new ReputationService(connectionString);
    }

    /// <summary>
    /// Process a witnessed action and apply reputation changes to all observing factions
    /// </summary>
    public void ProcessWitnessedAction(int characterId, string actionType, int? targetFactionId = null, string? location = null)
    {
        _log.Debug("Processing witnessed action: CharacterId={CharacterId}, ActionType={ActionType}, TargetFactionId={TargetFactionId}, Location={Location}",
            characterId, actionType, targetFactionId, location);

        // Get all factions that could potentially witness this action
        var witnessableFactions = GetWitnessingFactions(location);

        foreach (var faction in witnessableFactions)
        {
            var reputationChange = _reputationService.CalculateReputationChange(
                actionType,
                faction.FactionId,
                targetFactionId,
                faction
            );

            if (reputationChange != 0)
            {
                _reputationService.ModifyReputation(
                    characterId,
                    faction.FactionId,
                    reputationChange,
                    $"Witnessed: {actionType}"
                );

                _log.Information("Witnessed action applied: CharacterId={CharacterId}, FactionId={FactionId}, ActionType={ActionType}, Change={Change}",
                    characterId, faction.FactionId, actionType, reputationChange);
            }
        }
    }

    /// <summary>
    /// Gets factions that could witness actions in a specific location
    /// </summary>
    private List<Faction> GetWitnessingFactions(string? location)
    {
        if (string.IsNullOrEmpty(location))
        {
            // If no location specified, all factions can potentially witness
            return GetAllFactions();
        }

        // Filter factions by primary location
        var allFactions = GetAllFactions();
        var witnessingFactions = new List<Faction>();

        foreach (var faction in allFactions)
        {
            // Check if faction is present in this location
            if (faction.PrimaryLocation.Contains(location, StringComparison.OrdinalIgnoreCase) ||
                faction.PrimaryLocation.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                witnessingFactions.Add(faction);
            }
        }

        // If no specific factions found for location, allow all to witness
        // (represents travelers, scouts, etc.)
        if (witnessingFactions.Count == 0)
        {
            witnessingFactions = allFactions;
        }

        return witnessingFactions;
    }

    /// <summary>
    /// Gets all factions from the database
    /// </summary>
    public List<Faction> GetAllFactions()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT faction_id, faction_name, display_name, philosophy, description,
                   primary_location, allied_factions, enemy_factions
            FROM Factions
            ORDER BY faction_id
        ";

        var factions = new List<Faction>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            factions.Add(new Faction
            {
                FactionId = reader.GetInt32(0),
                FactionName = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Philosophy = reader.GetString(3),
                Description = reader.GetString(4),
                PrimaryLocation = reader.GetString(5),
                AlliedFactions = reader.GetString(6),
                EnemyFactions = reader.GetString(7)
            });
        }

        return factions;
    }

    /// <summary>
    /// Gets a specific faction by ID
    /// </summary>
    public Faction? GetFactionById(int factionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT faction_id, faction_name, display_name, philosophy, description,
                   primary_location, allied_factions, enemy_factions
            FROM Factions
            WHERE faction_id = @factionId
        ";
        command.Parameters.AddWithValue("@factionId", factionId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Faction
            {
                FactionId = reader.GetInt32(0),
                FactionName = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Philosophy = reader.GetString(3),
                Description = reader.GetString(4),
                PrimaryLocation = reader.GetString(5),
                AlliedFactions = reader.GetString(6),
                EnemyFactions = reader.GetString(7)
            };
        }

        return null;
    }

    /// <summary>
    /// Gets a specific faction by name
    /// </summary>
    public Faction? GetFactionByName(string factionName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT faction_id, faction_name, display_name, philosophy, description,
                   primary_location, allied_factions, enemy_factions
            FROM Factions
            WHERE faction_name = @factionName
        ";
        command.Parameters.AddWithValue("@factionName", factionName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Faction
            {
                FactionId = reader.GetInt32(0),
                FactionName = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Philosophy = reader.GetString(3),
                Description = reader.GetString(4),
                PrimaryLocation = reader.GetString(5),
                AlliedFactions = reader.GetString(6),
                EnemyFactions = reader.GetString(7)
            };
        }

        return null;
    }

    /// <summary>
    /// Gets a character's reputation with a specific faction
    /// </summary>
    public FactionReputation? GetCharacterReputation(int characterId, int factionId)
    {
        return _reputationService.GetFactionReputation(characterId, factionId);
    }

    /// <summary>
    /// Gets all faction reputations for a character
    /// </summary>
    public List<FactionReputation> GetAllCharacterReputations(int characterId)
    {
        return _reputationService.GetAllReputations(characterId);
    }

    /// <summary>
    /// Checks if any faction is hostile to the character
    /// </summary>
    public List<Faction> GetHostileFactions(int characterId)
    {
        var allFactions = GetAllFactions();
        var hostileFactions = new List<Faction>();

        foreach (var faction in allFactions)
        {
            if (_reputationService.IsFactionHostile(characterId, faction.FactionId))
            {
                hostileFactions.Add(faction);
            }
        }

        return hostileFactions;
    }

    /// <summary>
    /// Gets available faction quests based on character reputation
    /// </summary>
    public List<string> GetAvailableFactionQuests(int characterId, int factionId)
    {
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        var reputationValue = reputation?.ReputationValue ?? 0;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT quest_id
            FROM Faction_Quests
            WHERE faction_id = @factionId AND required_reputation <= @repValue
            ORDER BY required_reputation DESC
        ";
        command.Parameters.AddWithValue("@factionId", factionId);
        command.Parameters.AddWithValue("@repValue", reputationValue);

        var questIds = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            questIds.Add(reader.GetString(0));
        }

        _log.Debug("Available faction quests: CharacterId={CharacterId}, FactionId={FactionId}, ReputationValue={RepValue}, QuestCount={Count}",
            characterId, factionId, reputationValue, questIds.Count);

        return questIds;
    }

    /// <summary>
    /// Gets available faction rewards based on character reputation
    /// </summary>
    public List<FactionReward> GetAvailableFactionRewards(int characterId, int factionId)
    {
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        var reputationValue = reputation?.ReputationValue ?? 0;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT reward_id, faction_id, reward_type, reward_name, reward_description, required_reputation, reward_data
            FROM Faction_Rewards
            WHERE faction_id = @factionId AND required_reputation <= @repValue
            ORDER BY required_reputation DESC
        ";
        command.Parameters.AddWithValue("@factionId", factionId);
        command.Parameters.AddWithValue("@repValue", reputationValue);

        var rewards = new List<FactionReward>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rewards.Add(new FactionReward
            {
                RewardId = reader.GetInt32(0),
                FactionId = reader.GetInt32(1),
                RewardType = reader.GetString(2),
                RewardName = reader.GetString(3),
                RewardDescription = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                RequiredReputation = reader.GetInt32(5),
                RewardData = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            });
        }

        _log.Debug("Available faction rewards: CharacterId={CharacterId}, FactionId={FactionId}, ReputationValue={RepValue}, RewardCount={Count}",
            characterId, factionId, reputationValue, rewards.Count);

        return rewards;
    }

    /// <summary>
    /// Gets price modifier for a character with a specific faction
    /// </summary>
    public float GetPriceModifier(int characterId, int factionId)
    {
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        var tier = reputation?.ReputationTier ?? FactionReputationTier.Neutral;
        return _reputationService.GetPriceModifier(tier);
    }

    /// <summary>
    /// Gets encounter frequency modifier for a character with a specific faction
    /// </summary>
    public float GetEncounterModifier(int characterId, int factionId)
    {
        var reputation = _reputationService.GetFactionReputation(characterId, factionId);
        var tier = reputation?.ReputationTier ?? FactionReputationTier.Neutral;
        return _reputationService.GetEncounterFrequencyModifier(tier);
    }
}

/// <summary>
/// v0.33.2: Represents a faction reward
/// </summary>
public class FactionReward
{
    public int RewardId { get; set; }
    public int FactionId { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public string RewardName { get; set; } = string.Empty;
    public string RewardDescription { get; set; } = string.Empty;
    public int RequiredReputation { get; set; }
    public string RewardData { get; set; } = string.Empty;
}
