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
}
