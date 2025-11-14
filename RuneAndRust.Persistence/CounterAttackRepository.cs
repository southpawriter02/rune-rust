using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.21.4: Counter-Attack Repository
/// Manages persistence and retrieval of parry statistics and bonuses
/// </summary>
public class CounterAttackRepository
{
    private static readonly ILogger _log = Log.ForContext<CounterAttackRepository>();
    private readonly string _connectionString;

    public CounterAttackRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, "runeandrust.db");
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("CounterAttackRepository initialized with database path: {DbPath}", dbPath);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _log.Debug("Initializing counter-attack tables");

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create ParryStatistics table
            var createStatsTable = connection.CreateCommand();
            createStatsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS ParryStatistics (
                    CharacterID INTEGER PRIMARY KEY,
                    TotalParryAttempts INTEGER DEFAULT 0,
                    SuccessfulParries INTEGER DEFAULT 0,
                    SuperiorParries INTEGER DEFAULT 0,
                    CriticalParries INTEGER DEFAULT 0,
                    FailedParries INTEGER DEFAULT 0,
                    RipostesLanded INTEGER DEFAULT 0,
                    RiposteKills INTEGER DEFAULT 0
                )
            ";
            createStatsTable.ExecuteNonQuery();

            // Create ParryBonuses table
            var createBonusesTable = connection.CreateCommand();
            createBonusesTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS ParryBonuses (
                    BonusID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CharacterID INTEGER NOT NULL,
                    Source TEXT NOT NULL,
                    BonusDice INTEGER DEFAULT 0,
                    AllowsSuperiorRiposte INTEGER DEFAULT 0,
                    ParriesPerRound INTEGER DEFAULT 1
                )
            ";
            createBonusesTable.ExecuteNonQuery();

            // Create ParryAttempts table (combat log)
            var createAttemptsTable = connection.CreateCommand();
            createAttemptsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS ParryAttempts (
                    AttemptID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CombatInstanceID INTEGER NOT NULL,
                    DefenderID INTEGER NOT NULL,
                    AttackerID INTEGER NOT NULL,
                    AttackAbility TEXT,
                    ParryPoolRoll INTEGER NOT NULL,
                    AttackerAccuracyRoll INTEGER NOT NULL,
                    Outcome TEXT NOT NULL,
                    RiposteTriggered INTEGER DEFAULT 0,
                    RiposteDamage INTEGER DEFAULT 0,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                )
            ";
            createAttemptsTable.ExecuteNonQuery();

            _log.Information("Counter-attack tables initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize counter-attack tables");
            throw;
        }
    }

    #region Parry Statistics

    /// <summary>
    /// Get parry statistics for a character
    /// </summary>
    public ParryStatistics GetParryStatistics(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ParryStatistics WHERE CharacterID = @characterId";
            command.Parameters.AddWithValue("@characterId", characterId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new ParryStatistics
                {
                    CharacterID = reader.GetInt32(0),
                    TotalParryAttempts = reader.GetInt32(1),
                    SuccessfulParries = reader.GetInt32(2),
                    SuperiorParries = reader.GetInt32(3),
                    CriticalParries = reader.GetInt32(4),
                    FailedParries = reader.GetInt32(5),
                    RipostesLanded = reader.GetInt32(6),
                    RiposteKills = reader.GetInt32(7)
                };
            }

            // Return empty stats if no record exists
            return new ParryStatistics { CharacterID = characterId };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get parry statistics for character {CharacterId}", characterId);
            return new ParryStatistics { CharacterID = characterId };
        }
    }

    /// <summary>
    /// Update parry statistics after a parry attempt
    /// </summary>
    public void UpdateParryStatistics(ParryResult result)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check if stats exist
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM ParryStatistics WHERE CharacterID = @characterId";
            checkCommand.Parameters.AddWithValue("@characterId", result.DefenderID);
            var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

            if (!exists)
            {
                // Create new stats record
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
                    INSERT INTO ParryStatistics (CharacterID, TotalParryAttempts, SuccessfulParries,
                        SuperiorParries, CriticalParries, FailedParries, RipostesLanded, RiposteKills)
                    VALUES (@characterId, 0, 0, 0, 0, 0, 0, 0)
                ";
                insertCommand.Parameters.AddWithValue("@characterId", result.DefenderID);
                insertCommand.ExecuteNonQuery();
            }

            // Update stats
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE ParryStatistics
                SET TotalParryAttempts = TotalParryAttempts + 1,
                    SuccessfulParries = SuccessfulParries + @success,
                    SuperiorParries = SuperiorParries + @superior,
                    CriticalParries = CriticalParries + @critical,
                    FailedParries = FailedParries + @failed,
                    RipostesLanded = RipostesLanded + @riposte,
                    RiposteKills = RiposteKills + @kill
                WHERE CharacterID = @characterId
            ";

            updateCommand.Parameters.AddWithValue("@characterId", result.DefenderID);
            updateCommand.Parameters.AddWithValue("@success", result.Success ? 1 : 0);
            updateCommand.Parameters.AddWithValue("@superior", result.Outcome == ParryOutcome.Superior ? 1 : 0);
            updateCommand.Parameters.AddWithValue("@critical", result.Outcome == ParryOutcome.Critical ? 1 : 0);
            updateCommand.Parameters.AddWithValue("@failed", !result.Success ? 1 : 0);
            updateCommand.Parameters.AddWithValue("@riposte",
                result.RiposteTriggered && result.Riposte?.Hit == true ? 1 : 0);
            updateCommand.Parameters.AddWithValue("@kill",
                result.Riposte?.KilledTarget == true ? 1 : 0);

            updateCommand.ExecuteNonQuery();

            _log.Debug("Updated parry statistics for character {CharacterId}: Outcome={Outcome}",
                result.DefenderID, result.Outcome);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update parry statistics");
        }
    }

    #endregion

    #region Parry Bonuses

    /// <summary>
    /// Get all parry bonuses for a character
    /// </summary>
    public List<ParryBonus> GetParryBonuses(int characterId)
    {
        var bonuses = new List<ParryBonus>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ParryBonuses WHERE CharacterID = @characterId";
            command.Parameters.AddWithValue("@characterId", characterId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bonuses.Add(new ParryBonus
                {
                    BonusID = reader.GetInt32(0),
                    CharacterID = reader.GetInt32(1),
                    Source = reader.GetString(2),
                    BonusDice = reader.GetInt32(3),
                    AllowsSuperiorRiposte = reader.GetInt32(4) == 1,
                    ParriesPerRound = reader.GetInt32(5)
                });
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get parry bonuses for character {CharacterId}", characterId);
        }

        return bonuses;
    }

    /// <summary>
    /// Add a parry bonus for a character
    /// </summary>
    public int AddParryBonus(ParryBonus bonus)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ParryBonuses (CharacterID, Source, BonusDice, AllowsSuperiorRiposte, ParriesPerRound)
                VALUES (@characterId, @source, @bonusDice, @superiorRiposte, @parriesPerRound)
            ";

            command.Parameters.AddWithValue("@characterId", bonus.CharacterID);
            command.Parameters.AddWithValue("@source", bonus.Source);
            command.Parameters.AddWithValue("@bonusDice", bonus.BonusDice);
            command.Parameters.AddWithValue("@superiorRiposte", bonus.AllowsSuperiorRiposte ? 1 : 0);
            command.Parameters.AddWithValue("@parriesPerRound", bonus.ParriesPerRound);

            command.ExecuteNonQuery();

            // Get last inserted ID
            var lastIdCommand = connection.CreateCommand();
            lastIdCommand.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(lastIdCommand.ExecuteScalar());
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to add parry bonus for character {CharacterId}", bonus.CharacterID);
            return -1;
        }
    }

    /// <summary>
    /// Remove a parry bonus by ID
    /// </summary>
    public void RemoveParryBonus(int bonusId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ParryBonuses WHERE BonusID = @bonusId";
            command.Parameters.AddWithValue("@bonusId", bonusId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to remove parry bonus {BonusId}", bonusId);
        }
    }

    /// <summary>
    /// Remove all parry bonuses for a character
    /// </summary>
    public void RemoveAllParryBonuses(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ParryBonuses WHERE CharacterID = @characterId";
            command.Parameters.AddWithValue("@characterId", characterId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to remove all parry bonuses for character {CharacterId}", characterId);
        }
    }

    #endregion

    #region Combat Log

    /// <summary>
    /// Record a parry attempt in the combat log
    /// </summary>
    public int RecordParryAttempt(ParryAttempt attempt)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ParryAttempts
                (CombatInstanceID, DefenderID, AttackerID, AttackAbility, ParryPoolRoll,
                 AttackerAccuracyRoll, Outcome, RiposteTriggered, RiposteDamage, Timestamp)
                VALUES (@combatId, @defenderId, @attackerId, @ability, @parryRoll,
                        @accuracyRoll, @outcome, @riposte, @damage, @timestamp)
            ";

            command.Parameters.AddWithValue("@combatId", attempt.CombatInstanceID);
            command.Parameters.AddWithValue("@defenderId", attempt.DefenderID);
            command.Parameters.AddWithValue("@attackerId", attempt.AttackerID);
            command.Parameters.AddWithValue("@ability", (object?)attempt.AttackAbility ?? DBNull.Value);
            command.Parameters.AddWithValue("@parryRoll", attempt.ParryPoolRoll);
            command.Parameters.AddWithValue("@accuracyRoll", attempt.AttackerAccuracyRoll);
            command.Parameters.AddWithValue("@outcome", attempt.Outcome.ToString());
            command.Parameters.AddWithValue("@riposte", attempt.RiposteTriggered ? 1 : 0);
            command.Parameters.AddWithValue("@damage", attempt.RiposteDamage);
            command.Parameters.AddWithValue("@timestamp", attempt.Timestamp);

            command.ExecuteNonQuery();

            // Get last inserted ID
            var lastIdCommand = connection.CreateCommand();
            lastIdCommand.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(lastIdCommand.ExecuteScalar());
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to record parry attempt");
            return -1;
        }
    }

    /// <summary>
    /// Get all parry attempts for a combat instance
    /// </summary>
    public List<ParryAttempt> GetParryAttempts(int combatInstanceId)
    {
        var attempts = new List<ParryAttempt>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM ParryAttempts
                WHERE CombatInstanceID = @combatId
                ORDER BY Timestamp ASC
            ";
            command.Parameters.AddWithValue("@combatId", combatInstanceId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                attempts.Add(new ParryAttempt
                {
                    AttemptID = reader.GetInt32(0),
                    CombatInstanceID = reader.GetInt32(1),
                    DefenderID = reader.GetInt32(2),
                    AttackerID = reader.GetInt32(3),
                    AttackAbility = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ParryPoolRoll = reader.GetInt32(5),
                    AttackerAccuracyRoll = reader.GetInt32(6),
                    Outcome = Enum.Parse<ParryOutcome>(reader.GetString(7)),
                    RiposteTriggered = reader.GetInt32(8) == 1,
                    RiposteDamage = reader.GetInt32(9),
                    Timestamp = reader.GetDateTime(10)
                });
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get parry attempts for combat {CombatId}", combatInstanceId);
        }

        return attempts;
    }

    #endregion
}
