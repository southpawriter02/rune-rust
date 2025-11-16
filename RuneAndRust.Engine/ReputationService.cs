using RuneAndRust.Core.Factions;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.33.2: Core reputation calculation and management service
/// Handles reputation changes, tier calculations, and world reaction modifiers
/// </summary>
public class ReputationService
{
    private static readonly ILogger _log = Log.ForContext<ReputationService>();
    private readonly string _connectionString;

    public ReputationService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Modifies a character's reputation with a faction
    /// </summary>
    public void ModifyReputation(int characterId, int factionId, int change, string reason)
    {
        if (change == 0)
        {
            _log.Debug("Reputation change is 0, skipping modification");
            return;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Get current reputation or create new entry
        var currentRep = GetFactionReputation(characterId, factionId, connection);
        var oldValue = currentRep?.ReputationValue ?? 0;
        var oldTier = currentRep?.ReputationTier ?? FactionReputationTier.Neutral;

        // Calculate new value and clamp
        var newValue = Math.Clamp(oldValue + change, -100, 100);
        var newTier = GetReputationTier(newValue);

        if (currentRep == null)
        {
            // Create new reputation entry
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Characters_FactionReputations (character_id, faction_id, reputation_value, reputation_tier, last_modified)
                VALUES (@charId, @factionId, @value, @tier, @modified)
            ";
            insertCommand.Parameters.AddWithValue("@charId", characterId);
            insertCommand.Parameters.AddWithValue("@factionId", factionId);
            insertCommand.Parameters.AddWithValue("@value", newValue);
            insertCommand.Parameters.AddWithValue("@tier", newTier.ToString());
            insertCommand.Parameters.AddWithValue("@modified", DateTime.Now.ToString("o"));
            insertCommand.ExecuteNonQuery();

            _log.Information("Reputation created: CharacterId={CharacterId}, FactionId={FactionId}, Value={Value}, Tier={Tier}, Reason={Reason}",
                characterId, factionId, newValue, newTier, reason);
        }
        else
        {
            // Update existing reputation entry
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Characters_FactionReputations
                SET reputation_value = @value, reputation_tier = @tier, last_modified = @modified
                WHERE character_id = @charId AND faction_id = @factionId
            ";
            updateCommand.Parameters.AddWithValue("@value", newValue);
            updateCommand.Parameters.AddWithValue("@tier", newTier.ToString());
            updateCommand.Parameters.AddWithValue("@modified", DateTime.Now.ToString("o"));
            updateCommand.Parameters.AddWithValue("@charId", characterId);
            updateCommand.Parameters.AddWithValue("@factionId", factionId);
            updateCommand.ExecuteNonQuery();

            _log.Information("Reputation modified: CharacterId={CharacterId}, FactionId={FactionId}, Change={Change}, OldValue={OldValue}, NewValue={NewValue}, OldTier={OldTier}, NewTier={NewTier}, Reason={Reason}",
                characterId, factionId, change, oldValue, newValue, oldTier, newTier, reason);
        }

        // Log tier transitions
        if (oldTier != newTier)
        {
            _log.Information("Reputation tier transition: CharacterId={CharacterId}, FactionId={FactionId}, OldTier={OldTier}, NewTier={NewTier}",
                characterId, factionId, oldTier, newTier);
        }
    }

    /// <summary>
    /// Gets a character's reputation with a faction
    /// </summary>
    public FactionReputation? GetFactionReputation(int characterId, int factionId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return GetFactionReputation(characterId, factionId, connection);
    }

    private FactionReputation? GetFactionReputation(int characterId, int factionId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT reputation_id, character_id, faction_id, reputation_value, reputation_tier, last_modified
            FROM Characters_FactionReputations
            WHERE character_id = @charId AND faction_id = @factionId
        ";
        command.Parameters.AddWithValue("@charId", characterId);
        command.Parameters.AddWithValue("@factionId", factionId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new FactionReputation
            {
                ReputationId = reader.GetInt32(0),
                CharacterId = reader.GetInt32(1),
                FactionId = reader.GetInt32(2),
                ReputationValue = reader.GetInt32(3),
                ReputationTier = Enum.Parse<FactionReputationTier>(reader.GetString(4)),
                LastModified = DateTime.Parse(reader.GetString(5))
            };
        }

        return null;
    }

    /// <summary>
    /// Converts reputation value to tier
    /// </summary>
    public FactionReputationTier GetReputationTier(int reputationValue)
    {
        if (reputationValue >= 75) return FactionReputationTier.Exalted;
        if (reputationValue >= 50) return FactionReputationTier.Allied;
        if (reputationValue >= 25) return FactionReputationTier.Friendly;
        if (reputationValue >= -25) return FactionReputationTier.Neutral;
        if (reputationValue >= -75) return FactionReputationTier.Hostile;
        return FactionReputationTier.Hated;
    }

    /// <summary>
    /// Gets price modifier based on reputation tier
    /// </summary>
    public float GetPriceModifier(FactionReputationTier tier)
    {
        return tier switch
        {
            FactionReputationTier.Exalted => 0.70f,   // -30% discount
            FactionReputationTier.Allied => 0.80f,    // -20% discount
            FactionReputationTier.Friendly => 0.90f,  // -10% discount
            FactionReputationTier.Neutral => 1.0f,    // Normal price
            FactionReputationTier.Hostile => 1.25f,   // +25% markup
            FactionReputationTier.Hated => 1.50f,     // +50% markup
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets random encounter frequency modifier based on reputation tier
    /// </summary>
    public float GetEncounterFrequencyModifier(FactionReputationTier tier)
    {
        return tier switch
        {
            FactionReputationTier.Exalted => 0.0f,    // No hostile encounters
            FactionReputationTier.Allied => 0.25f,    // 75% reduction in hostile encounters
            FactionReputationTier.Friendly => 0.5f,   // 50% reduction in hostile encounters
            FactionReputationTier.Neutral => 1.0f,    // Normal encounter rate
            FactionReputationTier.Hostile => 2.0f,    // 2x hostile encounter rate
            FactionReputationTier.Hated => 3.0f,      // 3x hostile encounter rate
            _ => 1.0f
        };
    }

    /// <summary>
    /// Checks if a faction should attack a character on sight
    /// </summary>
    public bool IsFactionHostile(int characterId, int factionId)
    {
        var reputation = GetFactionReputation(characterId, factionId);
        if (reputation == null)
            return false; // Neutral by default

        return reputation.ReputationTier == FactionReputationTier.Hated ||
               reputation.ReputationTier == FactionReputationTier.Hostile;
    }

    /// <summary>
    /// Gets all faction reputations for a character
    /// </summary>
    public List<FactionReputation> GetAllReputations(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT reputation_id, character_id, faction_id, reputation_value, reputation_tier, last_modified
            FROM Characters_FactionReputations
            WHERE character_id = @charId
            ORDER BY faction_id
        ";
        command.Parameters.AddWithValue("@charId", characterId);

        var reputations = new List<FactionReputation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            reputations.Add(new FactionReputation
            {
                ReputationId = reader.GetInt32(0),
                CharacterId = reader.GetInt32(1),
                FactionId = reader.GetInt32(2),
                ReputationValue = reader.GetInt32(3),
                ReputationTier = Enum.Parse<FactionReputationTier>(reader.GetString(4)),
                LastModified = DateTime.Parse(reader.GetString(5))
            });
        }

        return reputations;
    }

    /// <summary>
    /// Calculates reputation change based on action type and context
    /// </summary>
    public int CalculateReputationChange(string actionType, int observerFactionId, int? targetFactionId, Faction? observerFaction = null)
    {
        // Base reputation changes by action type
        var change = actionType switch
        {
            WitnessedActionTypes.KillUndying => CalculateUndyingKillReputation(observerFactionId),
            WitnessedActionTypes.KillJotunForged => CalculateJotunKillReputation(observerFactionId),
            WitnessedActionTypes.SpareEnemy => 5,
            WitnessedActionTypes.CompleteQuest => 0, // Quest-specific rewards handled elsewhere
            WitnessedActionTypes.AttackFactionMember => -30,
            WitnessedActionTypes.KillFactionMember => -50,
            WitnessedActionTypes.TradeWithMerchant => 5,
            WitnessedActionTypes.DonateResources => 15,
            WitnessedActionTypes.RecoverData => CalculateDataRecoveryReputation(observerFactionId),
            WitnessedActionTypes.DestroyData => CalculateDataDestructionReputation(observerFactionId),
            WitnessedActionTypes.ShareKnowledge => CalculateKnowledgeSharingReputation(observerFactionId),
            WitnessedActionTypes.HoardKnowledge => CalculateKnowledgeHoardingReputation(observerFactionId),
            _ => 0
        };

        // Apply ally/enemy modifiers if we have faction relationship data
        if (observerFaction != null && targetFactionId.HasValue)
        {
            // Check if target faction is allied or enemy with observer
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var targetFaction = GetFactionById(targetFactionId.Value, connection);

            if (targetFaction != null)
            {
                if (observerFaction.IsAlly(targetFaction.FactionName))
                {
                    // Hurting an ally's enemy is good, helping them is good
                    if (actionType == WitnessedActionTypes.KillFactionMember ||
                        actionType == WitnessedActionTypes.AttackFactionMember)
                    {
                        change = Math.Abs(change); // Make negative into positive
                    }
                }
                else if (observerFaction.IsEnemy(targetFaction.FactionName))
                {
                    // Hurting an enemy is good
                    if (actionType == WitnessedActionTypes.KillFactionMember)
                    {
                        change = 20; // Bonus for killing enemies
                    }
                }
            }
        }

        return change;
    }

    private int CalculateUndyingKillReputation(int factionId)
    {
        // Faction 1: Iron-Banes love killing Undying
        if (factionId == 1) return 10;
        // Faction 2: God-Sleeper Cultists dislike killing Undying
        if (factionId == 2) return -20;
        // Others are neutral
        return 0;
    }

    private int CalculateJotunKillReputation(int factionId)
    {
        // Faction 1: Iron-Banes love purging corrupted Jötun
        if (factionId == 1) return 30;
        // Faction 2: God-Sleeper Cultists HATE killing their "gods"
        if (factionId == 2) return -60;
        // Faction 3: Jötun-Readers slightly negative (lost knowledge)
        if (factionId == 3) return -10;
        return 0;
    }

    private int CalculateDataRecoveryReputation(int factionId)
    {
        // Faction 3: Jötun-Readers love data recovery
        if (factionId == 3) return 10;
        return 2; // Everyone appreciates knowledge to some degree
    }

    private int CalculateDataDestructionReputation(int factionId)
    {
        // Faction 3: Jötun-Readers hate data destruction
        if (factionId == 3) return -20;
        return -5; // Most factions dislike waste
    }

    private int CalculateKnowledgeSharingReputation(int factionId)
    {
        // Faction 3: Jötun-Readers love knowledge sharing
        if (factionId == 3) return 20;
        // Faction 4: Rust-Clans appreciate practical knowledge
        if (factionId == 4) return 10;
        return 5;
    }

    private int CalculateKnowledgeHoardingReputation(int factionId)
    {
        // Faction 3: Jötun-Readers hate knowledge hoarding
        if (factionId == 3) return -30;
        return -5;
    }

    private Faction? GetFactionById(int factionId, SqliteConnection connection)
    {
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
}
