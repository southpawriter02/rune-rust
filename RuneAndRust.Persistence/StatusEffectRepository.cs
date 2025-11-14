using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.21.3: Advanced Status Effect Repository
/// Manages persistence and retrieval of status effects and interaction rules
/// </summary>
public class StatusEffectRepository
{
    private static readonly ILogger _log = Log.ForContext<StatusEffectRepository>();
    private readonly string _connectionString;

    public StatusEffectRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, "runeandrust.db");
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("StatusEffectRepository initialized with database path: {DbPath}", dbPath);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _log.Debug("Initializing status effect tables");

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create ActiveStatusEffects table
            var createActiveEffectsTable = connection.CreateCommand();
            createActiveEffectsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS ActiveStatusEffects (
                    EffectInstanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                    TargetID INTEGER NOT NULL,
                    EffectType TEXT NOT NULL,
                    StackCount INTEGER DEFAULT 1,
                    DurationRemaining INTEGER NOT NULL,
                    AppliedBy INTEGER,
                    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Category TEXT NOT NULL,
                    CanStack INTEGER DEFAULT 0,
                    MaxStacks INTEGER DEFAULT 1,
                    IgnoresSoak INTEGER DEFAULT 0,
                    DamageBase TEXT,
                    Metadata TEXT
                )
            ";
            createActiveEffectsTable.ExecuteNonQuery();

            // Create StatusInteractions table
            var createInteractionsTable = connection.CreateCommand();
            createInteractionsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS StatusInteractions (
                    InteractionID INTEGER PRIMARY KEY AUTOINCREMENT,
                    InteractionType TEXT NOT NULL,
                    PrimaryEffect TEXT NOT NULL,
                    SecondaryEffect TEXT,
                    RequiredApplications INTEGER DEFAULT 2,
                    ResultEffect TEXT,
                    ResultDuration INTEGER DEFAULT 1,
                    Multiplier REAL DEFAULT 1.0,
                    Resolution TEXT,
                    Description TEXT
                )
            ";
            createInteractionsTable.ExecuteNonQuery();

            // Seed canonical interactions
            SeedCanonicalInteractions(connection);

            _log.Information("Status effect tables initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize status effect tables");
            throw;
        }
    }

    private void SeedCanonicalInteractions(SqliteConnection connection)
    {
        // Check if interactions already seeded
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM StatusInteractions";
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count > 0)
        {
            _log.Debug("Status interactions already seeded, skipping");
            return;
        }

        _log.Information("Seeding canonical status interactions");

        var interactions = new[]
        {
            // Conversion: Disoriented → Stunned
            new StatusInteraction
            {
                InteractionType = StatusInteractionType.Conversion,
                PrimaryEffect = "Disoriented",
                RequiredApplications = 2,
                ResultEffect = "Stunned",
                ResultDuration = 1,
                Description = "Two applications of Disoriented convert to Stunned for 1 turn"
            },
            // Conversion: Slowed → Rooted
            new StatusInteraction
            {
                InteractionType = StatusInteractionType.Conversion,
                PrimaryEffect = "Slowed",
                RequiredApplications = 3,
                ResultEffect = "Rooted",
                ResultDuration = 1,
                Description = "Three applications of Slowed convert to Rooted for 1 turn"
            },
            // Amplification: Bleeding + Corroded
            new StatusInteraction
            {
                InteractionType = StatusInteractionType.Amplification,
                PrimaryEffect = "Bleeding",
                SecondaryEffect = "Corroded",
                Multiplier = 1.5f,
                Description = "Bleeding damage increased 50% if target is Corroded"
            },
            // Amplification: Poisoned + Bleeding
            new StatusInteraction
            {
                InteractionType = StatusInteractionType.Amplification,
                PrimaryEffect = "Poisoned",
                SecondaryEffect = "Bleeding",
                Multiplier = 1.3f,
                Description = "Poisoned and Bleeding stack multiplicatively for 1.3× total DoT"
            },
            // Suppression: Slowed + Hasted
            new StatusInteraction
            {
                InteractionType = StatusInteractionType.Suppression,
                PrimaryEffect = "Slowed",
                SecondaryEffect = "Hasted",
                Resolution = "Cancel",
                Description = "Slowed and Hasted cancel each other"
            }
        };

        foreach (var interaction in interactions)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO StatusInteractions
                (InteractionType, PrimaryEffect, SecondaryEffect, RequiredApplications,
                 ResultEffect, ResultDuration, Multiplier, Resolution, Description)
                VALUES (@type, @primary, @secondary, @required, @result, @duration, @multiplier, @resolution, @description)
            ";

            insertCommand.Parameters.AddWithValue("@type", interaction.InteractionType.ToString());
            insertCommand.Parameters.AddWithValue("@primary", interaction.PrimaryEffect);
            insertCommand.Parameters.AddWithValue("@secondary", (object?)interaction.SecondaryEffect ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@required", interaction.RequiredApplications);
            insertCommand.Parameters.AddWithValue("@result", (object?)interaction.ResultEffect ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@duration", interaction.ResultDuration);
            insertCommand.Parameters.AddWithValue("@multiplier", interaction.Multiplier);
            insertCommand.Parameters.AddWithValue("@resolution", (object?)interaction.Resolution ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@description", interaction.Description);

            insertCommand.ExecuteNonQuery();
        }

        _log.Information("Seeded {Count} canonical status interactions", interactions.Length);
    }

    /// <summary>
    /// Get all active effects for a target
    /// </summary>
    public List<StatusEffect> GetActiveEffects(int targetId)
    {
        var effects = new List<StatusEffect>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM ActiveStatusEffects
                WHERE TargetID = @targetId AND DurationRemaining > 0
                ORDER BY AppliedAt ASC
            ";
            command.Parameters.AddWithValue("@targetId", targetId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                effects.Add(new StatusEffect
                {
                    EffectInstanceID = reader.GetInt32(0),
                    TargetID = reader.GetInt32(1),
                    EffectType = reader.GetString(2),
                    StackCount = reader.GetInt32(3),
                    DurationRemaining = reader.GetInt32(4),
                    AppliedBy = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    AppliedAt = reader.GetDateTime(6),
                    Category = Enum.Parse<StatusEffectCategory>(reader.GetString(7)),
                    CanStack = reader.GetInt32(8) == 1,
                    MaxStacks = reader.GetInt32(9),
                    IgnoresSoak = reader.GetInt32(10) == 1,
                    DamageBase = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Metadata = reader.IsDBNull(12) ? null : reader.GetString(12)
                });
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get active effects for target {TargetId}", targetId);
        }

        return effects;
    }

    /// <summary>
    /// Get a specific effect type for a target
    /// </summary>
    public StatusEffect? GetActiveEffect(int targetId, string effectType)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM ActiveStatusEffects
                WHERE TargetID = @targetId AND EffectType = @effectType AND DurationRemaining > 0
                LIMIT 1
            ";
            command.Parameters.AddWithValue("@targetId", targetId);
            command.Parameters.AddWithValue("@effectType", effectType);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new StatusEffect
                {
                    EffectInstanceID = reader.GetInt32(0),
                    TargetID = reader.GetInt32(1),
                    EffectType = reader.GetString(2),
                    StackCount = reader.GetInt32(3),
                    DurationRemaining = reader.GetInt32(4),
                    AppliedBy = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    AppliedAt = reader.GetDateTime(6),
                    Category = Enum.Parse<StatusEffectCategory>(reader.GetString(7)),
                    CanStack = reader.GetInt32(8) == 1,
                    MaxStacks = reader.GetInt32(9),
                    IgnoresSoak = reader.GetInt32(10) == 1,
                    DamageBase = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Metadata = reader.IsDBNull(12) ? null : reader.GetString(12)
                };
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get active effect {EffectType} for target {TargetId}", effectType, targetId);
        }

        return null;
    }

    /// <summary>
    /// Add or update a status effect
    /// </summary>
    public int ApplyEffect(StatusEffect effect)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ActiveStatusEffects
                (TargetID, EffectType, StackCount, DurationRemaining, AppliedBy, AppliedAt,
                 Category, CanStack, MaxStacks, IgnoresSoak, DamageBase, Metadata)
                VALUES (@targetId, @effectType, @stackCount, @duration, @appliedBy, @appliedAt,
                        @category, @canStack, @maxStacks, @ignoresSoak, @damageBase, @metadata)
            ";

            command.Parameters.AddWithValue("@targetId", effect.TargetID);
            command.Parameters.AddWithValue("@effectType", effect.EffectType);
            command.Parameters.AddWithValue("@stackCount", effect.StackCount);
            command.Parameters.AddWithValue("@duration", effect.DurationRemaining);
            command.Parameters.AddWithValue("@appliedBy", effect.AppliedBy);
            command.Parameters.AddWithValue("@appliedAt", effect.AppliedAt);
            command.Parameters.AddWithValue("@category", effect.Category.ToString());
            command.Parameters.AddWithValue("@canStack", effect.CanStack ? 1 : 0);
            command.Parameters.AddWithValue("@maxStacks", effect.MaxStacks);
            command.Parameters.AddWithValue("@ignoresSoak", effect.IgnoresSoak ? 1 : 0);
            command.Parameters.AddWithValue("@damageBase", (object?)effect.DamageBase ?? DBNull.Value);
            command.Parameters.AddWithValue("@metadata", (object?)effect.Metadata ?? DBNull.Value);

            command.ExecuteNonQuery();

            // Get the last inserted ID
            var lastIdCommand = connection.CreateCommand();
            lastIdCommand.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(lastIdCommand.ExecuteScalar());
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply effect {EffectType} to target {TargetId}", effect.EffectType, effect.TargetID);
            return -1;
        }
    }

    /// <summary>
    /// Update effect stacks
    /// </summary>
    public void UpdateEffectStacks(int effectInstanceId, int newStackCount)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE ActiveStatusEffects
                SET StackCount = @stackCount
                WHERE EffectInstanceID = @id
            ";

            command.Parameters.AddWithValue("@stackCount", newStackCount);
            command.Parameters.AddWithValue("@id", effectInstanceId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update effect stacks for instance {InstanceId}", effectInstanceId);
        }
    }

    /// <summary>
    /// Remove a specific effect
    /// </summary>
    public void RemoveEffect(int targetId, string effectType)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM ActiveStatusEffects
                WHERE TargetID = @targetId AND EffectType = @effectType
            ";

            command.Parameters.AddWithValue("@targetId", targetId);
            command.Parameters.AddWithValue("@effectType", effectType);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to remove effect {EffectType} from target {TargetId}", effectType, targetId);
        }
    }

    /// <summary>
    /// Remove all effects from a target
    /// </summary>
    public void RemoveAllEffects(int targetId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ActiveStatusEffects WHERE TargetID = @targetId";
            command.Parameters.AddWithValue("@targetId", targetId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to remove all effects from target {TargetId}", targetId);
        }
    }

    /// <summary>
    /// Decrement duration for all effects
    /// </summary>
    public void DecrementDurations(int targetId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE ActiveStatusEffects
                SET DurationRemaining = DurationRemaining - 1
                WHERE TargetID = @targetId AND DurationRemaining > 0
            ";

            command.Parameters.AddWithValue("@targetId", targetId);

            command.ExecuteNonQuery();

            // Remove expired effects
            var removeCommand = connection.CreateCommand();
            removeCommand.CommandText = @"
                DELETE FROM ActiveStatusEffects
                WHERE TargetID = @targetId AND DurationRemaining <= 0
            ";
            removeCommand.Parameters.AddWithValue("@targetId", targetId);
            removeCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to decrement durations for target {TargetId}", targetId);
        }
    }

    /// <summary>
    /// Get all status interactions
    /// </summary>
    public List<StatusInteraction> GetAllInteractions()
    {
        var interactions = new List<StatusInteraction>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM StatusInteractions";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                interactions.Add(new StatusInteraction
                {
                    InteractionID = reader.GetInt32(0),
                    InteractionType = Enum.Parse<StatusInteractionType>(reader.GetString(1)),
                    PrimaryEffect = reader.GetString(2),
                    SecondaryEffect = reader.IsDBNull(3) ? null : reader.GetString(3),
                    RequiredApplications = reader.GetInt32(4),
                    ResultEffect = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ResultDuration = reader.GetInt32(6),
                    Multiplier = reader.GetFloat(7),
                    Resolution = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    Description = reader.GetString(9)
                });
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get status interactions");
        }

        return interactions;
    }

    /// <summary>
    /// Get interactions involving a specific effect
    /// </summary>
    public List<StatusInteraction> GetInteractionsForEffect(string effectType)
    {
        var interactions = new List<StatusInteraction>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM StatusInteractions
                WHERE PrimaryEffect = @effect OR SecondaryEffect = @effect
            ";
            command.Parameters.AddWithValue("@effect", effectType);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                interactions.Add(new StatusInteraction
                {
                    InteractionID = reader.GetInt32(0),
                    InteractionType = Enum.Parse<StatusInteractionType>(reader.GetString(1)),
                    PrimaryEffect = reader.GetString(2),
                    SecondaryEffect = reader.IsDBNull(3) ? null : reader.GetString(3),
                    RequiredApplications = reader.GetInt32(4),
                    ResultEffect = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ResultDuration = reader.GetInt32(6),
                    Multiplier = reader.GetFloat(7),
                    Resolution = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    Description = reader.GetString(9)
                });
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get interactions for effect {EffectType}", effectType);
        }

        return interactions;
    }
}
