using Microsoft.Data.Sqlite;
using RuneAndRust.Core.AI;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// Repository for AI configuration data (threat weights, archetype assignments, etc.).
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public class AIConfigurationRepository : RuneAndRust.Engine.AI.IAIConfigurationRepository
{
    private static readonly ILogger _log = Log.ForContext<AIConfigurationRepository>();
    private readonly string _connectionString;

    // Cache for threat weights (loaded once, reused)
    private Dictionary<AIArchetype, AIThreatWeights>? _weightsCache;

    public AIConfigurationRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, "runeandrust.db");
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("AIConfigurationRepository initialized with database path: {DbPath}", dbPath);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _log.Debug("Initializing AI configuration tables");

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create AIThreatWeights table
            var createWeightsTable = connection.CreateCommand();
            createWeightsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS AIThreatWeights (
                    ArchetypeId INTEGER PRIMARY KEY,
                    ArchetypeName TEXT NOT NULL,
                    DamageWeight REAL NOT NULL CHECK(DamageWeight >= 0.0 AND DamageWeight <= 1.0),
                    HPWeight REAL NOT NULL CHECK(HPWeight >= 0.0 AND HPWeight <= 1.0),
                    PositionWeight REAL NOT NULL CHECK(PositionWeight >= 0.0 AND PositionWeight <= 1.0),
                    AbilityWeight REAL NOT NULL CHECK(AbilityWeight >= 0.0 AND AbilityWeight <= 1.0),
                    StatusWeight REAL NOT NULL CHECK(StatusWeight >= 0.0 AND StatusWeight <= 1.0),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )
            ";
            createWeightsTable.ExecuteNonQuery();

            // Seed default threat weights
            SeedDefaultThreatWeights(connection);

            _log.Information("AI configuration tables initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize AI configuration tables");
            throw;
        }
    }

    private void SeedDefaultThreatWeights(SqliteConnection connection)
    {
        // Check if weights already seeded
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM AIThreatWeights";
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count > 0)
        {
            _log.Debug("AI threat weights already seeded, skipping");
            return;
        }

        _log.Information("Seeding default AI threat weights for 8 archetypes");

        var seedCommand = connection.CreateCommand();
        seedCommand.CommandText = @"
            INSERT INTO AIThreatWeights (ArchetypeId, ArchetypeName, DamageWeight, HPWeight, PositionWeight, AbilityWeight, StatusWeight)
            VALUES
                (1, 'Aggressive', 0.50, 0.10, 0.10, 0.20, 0.10),
                (2, 'Defensive', 0.20, 0.30, 0.30, 0.10, 0.10),
                (3, 'Cautious', 0.30, 0.20, 0.30, 0.10, 0.10),
                (4, 'Reckless', 0.40, 0.00, 0.00, 0.40, 0.20),
                (5, 'Tactical', 0.30, 0.25, 0.25, 0.15, 0.05),
                (6, 'Support', 0.20, 0.40, 0.20, 0.10, 0.10),
                (7, 'Control', 0.25, 0.15, 0.20, 0.30, 0.10),
                (8, 'Ambusher', 0.30, 0.30, 0.30, 0.05, 0.05)
        ";
        seedCommand.ExecuteNonQuery();

        _log.Information("Successfully seeded 8 AI threat weight configurations");
    }

    /// <inheritdoc/>
    public async Task<AIThreatWeights> GetThreatWeightsAsync(AIArchetype archetype)
    {
        // Use cache if available
        var allWeights = await GetAllThreatWeightsAsync();

        if (allWeights.TryGetValue(archetype, out var weights))
        {
            return weights;
        }

        _log.Warning("No threat weights found for archetype {Archetype}, returning default Tactical weights", archetype);

        // Fallback to Tactical
        return allWeights.GetValueOrDefault(AIArchetype.Tactical) ?? CreateFallbackWeights(archetype);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<AIArchetype, AIThreatWeights>> GetAllThreatWeightsAsync()
    {
        // Return cached weights if available
        if (_weightsCache != null)
        {
            return _weightsCache;
        }

        var weights = new Dictionary<AIArchetype, AIThreatWeights>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM AIThreatWeights ORDER BY ArchetypeId";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var archetypeId = reader.GetInt32(0);
                var weight = new AIThreatWeights
                {
                    ArchetypeId = archetypeId,
                    ArchetypeName = reader.GetString(1),
                    Archetype = (AIArchetype)archetypeId,
                    DamageWeight = (decimal)reader.GetDouble(2),
                    HPWeight = (decimal)reader.GetDouble(3),
                    PositionWeight = (decimal)reader.GetDouble(4),
                    AbilityWeight = (decimal)reader.GetDouble(5),
                    StatusWeight = (decimal)reader.GetDouble(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.GetDateTime(8)
                };

                weights[weight.Archetype] = weight;
            }

            _log.Debug("Loaded {Count} AI threat weight configurations", weights.Count);

            // Cache the results
            _weightsCache = weights;

            return weights;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load AI threat weights from database");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateThreatWeightsAsync(AIThreatWeights weights)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE AIThreatWeights
                SET DamageWeight = $damage,
                    HPWeight = $hp,
                    PositionWeight = $position,
                    AbilityWeight = $ability,
                    StatusWeight = $status,
                    UpdatedAt = CURRENT_TIMESTAMP
                WHERE ArchetypeId = $id
            ";

            command.Parameters.AddWithValue("$id", weights.ArchetypeId);
            command.Parameters.AddWithValue("$damage", (double)weights.DamageWeight);
            command.Parameters.AddWithValue("$hp", (double)weights.HPWeight);
            command.Parameters.AddWithValue("$position", (double)weights.PositionWeight);
            command.Parameters.AddWithValue("$ability", (double)weights.AbilityWeight);
            command.Parameters.AddWithValue("$status", (double)weights.StatusWeight);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                _log.Information("Updated threat weights for archetype {Archetype}", weights.Archetype);

                // Invalidate cache
                _weightsCache = null;
            }
            else
            {
                _log.Warning("No rows updated for archetype {Archetype}", weights.Archetype);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update AI threat weights for {Archetype}", weights.Archetype);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SeedDefaultThreatWeightsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        SeedDefaultThreatWeights(connection);
    }

    private AIThreatWeights CreateFallbackWeights(AIArchetype archetype)
    {
        // Emergency fallback if database is corrupted
        return new AIThreatWeights
        {
            ArchetypeId = (int)archetype,
            ArchetypeName = archetype.ToString(),
            Archetype = archetype,
            DamageWeight = 0.30m,
            HPWeight = 0.25m,
            PositionWeight = 0.25m,
            AbilityWeight = 0.15m,
            StatusWeight = 0.05m
        };
    }

    // v0.42.2: Archetype Configuration Methods

    private Dictionary<AIArchetype, AIArchetypeConfiguration>? _archetypeConfigCache;

    /// <inheritdoc/>
    public async Task<AIArchetypeConfiguration> GetArchetypeConfigurationAsync(AIArchetype archetype)
    {
        var allConfigs = await GetAllArchetypeConfigurationsAsync();

        if (allConfigs.TryGetValue(archetype, out var config))
        {
            return config;
        }

        _log.Warning("No configuration found for archetype {Archetype}, returning default", archetype);
        return CreateFallbackArchetypeConfiguration(archetype);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<AIArchetype, AIArchetypeConfiguration>> GetAllArchetypeConfigurationsAsync()
    {
        if (_archetypeConfigCache != null)
        {
            return _archetypeConfigCache;
        }

        var configs = new Dictionary<AIArchetype, AIArchetypeConfiguration>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Create table if it doesn't exist
            var createTable = connection.CreateCommand();
            createTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS AIArchetypeConfiguration (
                    ArchetypeId INTEGER PRIMARY KEY,
                    ArchetypeName TEXT NOT NULL,
                    Description TEXT,
                    DamageAbilityModifier REAL NOT NULL CHECK(DamageAbilityModifier >= 0.0 AND DamageAbilityModifier <= 2.0),
                    UtilityAbilityModifier REAL NOT NULL CHECK(UtilityAbilityModifier >= 0.0 AND UtilityAbilityModifier <= 2.0),
                    DefensiveAbilityModifier REAL NOT NULL CHECK(DefensiveAbilityModifier >= 0.0 AND DefensiveAbilityModifier <= 2.0),
                    AggressionLevel INTEGER NOT NULL CHECK(AggressionLevel >= 1 AND AggressionLevel <= 5),
                    RetreatThresholdHP REAL,
                    PreferredRange TEXT NOT NULL,
                    UsesCoordination INTEGER NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )
            ";
            createTable.ExecuteNonQuery();

            // Seed if empty
            SeedDefaultArchetypeConfigurations(connection);

            // Load all configurations
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM AIArchetypeConfiguration ORDER BY ArchetypeId";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var archetypeId = reader.GetInt32(0);
                var config = new AIArchetypeConfiguration
                {
                    ArchetypeId = archetypeId,
                    ArchetypeName = reader.GetString(1),
                    Archetype = (AIArchetype)archetypeId,
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    DamageAbilityModifier = (decimal)reader.GetDouble(3),
                    UtilityAbilityModifier = (decimal)reader.GetDouble(4),
                    DefensiveAbilityModifier = (decimal)reader.GetDouble(5),
                    AggressionLevel = reader.GetInt32(6),
                    RetreatThresholdHP = reader.IsDBNull(7) ? null : (decimal?)reader.GetDouble(7),
                    PreferredRange = reader.GetString(8),
                    UsesCoordination = reader.GetInt32(9) == 1,
                    CreatedAt = reader.GetDateTime(10)
                };

                configs[config.Archetype] = config;
            }

            _log.Debug("Loaded {Count} AI archetype configurations", configs.Count);

            _archetypeConfigCache = configs;

            return configs;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load AI archetype configurations");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateArchetypeConfigurationAsync(AIArchetypeConfiguration config)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE AIArchetypeConfiguration
                SET DamageAbilityModifier = $damage,
                    UtilityAbilityModifier = $utility,
                    DefensiveAbilityModifier = $defensive,
                    AggressionLevel = $aggression,
                    RetreatThresholdHP = $retreat,
                    PreferredRange = $range,
                    UsesCoordination = $coordination
                WHERE ArchetypeId = $id
            ";

            command.Parameters.AddWithValue("$id", config.ArchetypeId);
            command.Parameters.AddWithValue("$damage", (double)config.DamageAbilityModifier);
            command.Parameters.AddWithValue("$utility", (double)config.UtilityAbilityModifier);
            command.Parameters.AddWithValue("$defensive", (double)config.DefensiveAbilityModifier);
            command.Parameters.AddWithValue("$aggression", config.AggressionLevel);
            command.Parameters.AddWithValue("$retreat", config.RetreatThresholdHP.HasValue ? (double)config.RetreatThresholdHP.Value : DBNull.Value);
            command.Parameters.AddWithValue("$range", config.PreferredRange);
            command.Parameters.AddWithValue("$coordination", config.UsesCoordination ? 1 : 0);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                _log.Information("Updated archetype configuration for {Archetype}", config.Archetype);
                _archetypeConfigCache = null; // Invalidate cache
            }
            else
            {
                _log.Warning("No rows updated for archetype {Archetype}", config.Archetype);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update archetype configuration for {Archetype}", config.Archetype);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SeedDefaultArchetypeConfigurationsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        SeedDefaultArchetypeConfigurations(connection);
    }

    private void SeedDefaultArchetypeConfigurations(SqliteConnection connection)
    {
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM AIArchetypeConfiguration";
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count > 0)
        {
            _log.Debug("AI archetype configurations already seeded, skipping");
            return;
        }

        _log.Information("Seeding default AI archetype configurations for 8 archetypes");

        var seedCommand = connection.CreateCommand();
        seedCommand.CommandText = @"
            INSERT INTO AIArchetypeConfiguration
                (ArchetypeId, ArchetypeName, Description, DamageAbilityModifier, UtilityAbilityModifier, DefensiveAbilityModifier,
                 AggressionLevel, RetreatThresholdHP, PreferredRange, UsesCoordination)
            VALUES
                (1, 'Aggressive', 'Rushes enemies, prioritizes damage', 1.40, 0.50, 0.30, 5, NULL, 'Melee', 0),
                (2, 'Defensive', 'Protects allies, uses defensive abilities', 0.70, 1.00, 1.50, 2, 0.30, 'Medium', 1),
                (3, 'Cautious', 'Retreats when low, uses cover', 1.00, 1.00, 1.20, 3, 0.50, 'Long', 0),
                (4, 'Reckless', 'Charges in, ignores danger', 1.60, 0.40, 0.20, 5, NULL, 'Melee', 0),
                (5, 'Tactical', 'Balanced, coordinates with allies', 1.00, 1.10, 1.00, 3, 0.25, 'Medium', 1),
                (6, 'Support', 'Heals allies, stays at range', 0.30, 2.00, 1.50, 1, 0.40, 'Long', 1),
                (7, 'Control', 'Uses CC, disables threats', 0.60, 1.80, 0.80, 3, 0.35, 'Long', 1),
                (8, 'Ambusher', 'Waits for opportunity, burst damage', 1.50, 0.70, 0.60, 4, 0.40, 'Medium', 0)
        ";
        seedCommand.ExecuteNonQuery();

        _log.Information("Successfully seeded 8 AI archetype configurations");
    }

    private AIArchetypeConfiguration CreateFallbackArchetypeConfiguration(AIArchetype archetype)
    {
        return new AIArchetypeConfiguration
        {
            ArchetypeId = (int)archetype,
            ArchetypeName = archetype.ToString(),
            Archetype = archetype,
            Description = "Fallback configuration",
            DamageAbilityModifier = 1.0m,
            UtilityAbilityModifier = 1.0m,
            DefensiveAbilityModifier = 1.0m,
            AggressionLevel = 3,
            PreferredRange = "Medium",
            UsesCoordination = false
        };
    }
}
