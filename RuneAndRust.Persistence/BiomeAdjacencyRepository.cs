using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.39.2: Data repository for biome adjacency rules
/// Provides database access to biome compatibility and transition requirements
/// </summary>
public class BiomeAdjacencyRepository
{
    private static readonly ILogger _log = Log.ForContext<BiomeAdjacencyRepository>();
    private readonly string _connectionString;

    public BiomeAdjacencyRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("BiomeAdjacencyRepository initialized");
    }

    #region Read Operations

    /// <summary>
    /// Gets all adjacency rules
    /// </summary>
    public List<BiomeAdjacencyRule> GetAllRules()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                adjacency_id, biome_a, biome_b, compatibility,
                min_transition_rooms, max_transition_rooms,
                transition_theme, notes, created_at, updated_at
            FROM Biome_Adjacency
            ORDER BY biome_a, biome_b
        ";

        var rules = new List<BiomeAdjacencyRule>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rules.Add(MapAdjacencyRule(reader));
        }

        _log.Debug("Loaded {Count} adjacency rules", rules.Count);
        return rules;
    }

    /// <summary>
    /// Gets an adjacency rule by ID
    /// </summary>
    public BiomeAdjacencyRule? GetRuleById(int adjacencyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                adjacency_id, biome_a, biome_b, compatibility,
                min_transition_rooms, max_transition_rooms,
                transition_theme, notes, created_at, updated_at
            FROM Biome_Adjacency
            WHERE adjacency_id = $adjacencyId
        ";
        command.Parameters.AddWithValue("$adjacencyId", adjacencyId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAdjacencyRule(reader);
        }

        _log.Warning("Adjacency rule not found: ID {AdjacencyId}", adjacencyId);
        return null;
    }

    /// <summary>
    /// Gets the adjacency rule for two biomes (order-independent)
    /// </summary>
    public BiomeAdjacencyRule? GetRule(string biomeA, string biomeB)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                adjacency_id, biome_a, biome_b, compatibility,
                min_transition_rooms, max_transition_rooms,
                transition_theme, notes, created_at, updated_at
            FROM Biome_Adjacency
            WHERE (biome_a = $biomeA AND biome_b = $biomeB)
               OR (biome_a = $biomeB AND biome_b = $biomeA)
            LIMIT 1
        ";
        command.Parameters.AddWithValue("$biomeA", biomeA);
        command.Parameters.AddWithValue("$biomeB", biomeB);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var rule = MapAdjacencyRule(reader);
            _log.Debug("Found adjacency rule: {BiomeA} <-> {BiomeB} = {Compatibility}",
                biomeA, biomeB, rule.Compatibility);
            return rule;
        }

        _log.Warning("No adjacency rule found for: {BiomeA} <-> {BiomeB}", biomeA, biomeB);
        return null;
    }

    /// <summary>
    /// Gets all rules for a specific biome
    /// </summary>
    public List<BiomeAdjacencyRule> GetRulesForBiome(string biome)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                adjacency_id, biome_a, biome_b, compatibility,
                min_transition_rooms, max_transition_rooms,
                transition_theme, notes, created_at, updated_at
            FROM Biome_Adjacency
            WHERE biome_a = $biome OR biome_b = $biome
            ORDER BY biome_a, biome_b
        ";
        command.Parameters.AddWithValue("$biome", biome);

        var rules = new List<BiomeAdjacencyRule>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rules.Add(MapAdjacencyRule(reader));
        }

        _log.Debug("Loaded {Count} adjacency rules for biome {Biome}", rules.Count, biome);
        return rules;
    }

    /// <summary>
    /// Gets all biomes that are compatible with a specific biome
    /// </summary>
    public List<string> GetCompatibleBiomes(string biome, bool includeTransitionRequired = true)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var compatibilityFilter = includeTransitionRequired
            ? "compatibility IN ('Compatible', 'RequiresTransition')"
            : "compatibility = 'Compatible'";

        command.CommandText = $@"
            SELECT DISTINCT
                CASE
                    WHEN biome_a = $biome THEN biome_b
                    ELSE biome_a
                END AS compatible_biome
            FROM Biome_Adjacency
            WHERE (biome_a = $biome OR biome_b = $biome)
              AND {compatibilityFilter}
        ";
        command.Parameters.AddWithValue("$biome", biome);

        var compatibleBiomes = new List<string>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            compatibleBiomes.Add(reader.GetString(0));
        }

        _log.Debug("Found {Count} compatible biomes for {Biome}", compatibleBiomes.Count, biome);
        return compatibleBiomes;
    }

    #endregion

    #region Write Operations

    /// <summary>
    /// Adds or updates an adjacency rule
    /// </summary>
    public void UpsertRule(BiomeAdjacencyRule rule)
    {
        if (!rule.IsValid())
        {
            _log.Error("Attempted to upsert invalid adjacency rule: {BiomeA} <-> {BiomeB}",
                rule.BiomeA, rule.BiomeB);
            throw new ArgumentException("Invalid adjacency rule", nameof(rule));
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Biome_Adjacency (
                biome_a, biome_b, compatibility,
                min_transition_rooms, max_transition_rooms,
                transition_theme, notes, updated_at
            )
            VALUES (
                $biomeA, $biomeB, $compatibility,
                $minTransitionRooms, $maxTransitionRooms,
                $transitionTheme, $notes, CURRENT_TIMESTAMP
            )
            ON CONFLICT(biome_a, biome_b) DO UPDATE SET
                compatibility = $compatibility,
                min_transition_rooms = $minTransitionRooms,
                max_transition_rooms = $maxTransitionRooms,
                transition_theme = $transitionTheme,
                notes = $notes,
                updated_at = CURRENT_TIMESTAMP
        ";

        command.Parameters.AddWithValue("$biomeA", rule.BiomeA);
        command.Parameters.AddWithValue("$biomeB", rule.BiomeB);
        command.Parameters.AddWithValue("$compatibility", rule.Compatibility.ToString());
        command.Parameters.AddWithValue("$minTransitionRooms", rule.MinTransitionRooms);
        command.Parameters.AddWithValue("$maxTransitionRooms", rule.MaxTransitionRooms);
        command.Parameters.AddWithValue("$transitionTheme", (object?)rule.TransitionTheme ?? DBNull.Value);
        command.Parameters.AddWithValue("$notes", (object?)rule.Notes ?? DBNull.Value);

        command.ExecuteNonQuery();

        _log.Information("Upserted adjacency rule: {BiomeA} <-> {BiomeB} = {Compatibility}",
            rule.BiomeA, rule.BiomeB, rule.Compatibility);
    }

    /// <summary>
    /// Deletes an adjacency rule
    /// </summary>
    public void DeleteRule(int adjacencyId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM Biome_Adjacency
            WHERE adjacency_id = $adjacencyId
        ";
        command.Parameters.AddWithValue("$adjacencyId", adjacencyId);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _log.Information("Deleted adjacency rule: ID {AdjacencyId}", adjacencyId);
        }
        else
        {
            _log.Warning("No adjacency rule found to delete: ID {AdjacencyId}", adjacencyId);
        }
    }

    #endregion

    #region Helper Methods

    private BiomeAdjacencyRule MapAdjacencyRule(SqliteDataReader reader)
    {
        var rule = new BiomeAdjacencyRule
        {
            AdjacencyId = reader.GetInt32(0),
            BiomeA = reader.GetString(1),
            BiomeB = reader.GetString(2),
            Compatibility = Enum.Parse<BiomeCompatibility>(reader.GetString(3)),
            MinTransitionRooms = reader.GetInt32(4),
            MaxTransitionRooms = reader.GetInt32(5),
            TransitionTheme = reader.IsDBNull(6) ? null : reader.GetString(6),
            Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAt = reader.IsDBNull(8) ? DateTime.UtcNow : DateTime.Parse(reader.GetString(8)),
            UpdatedAt = reader.IsDBNull(9) ? DateTime.UtcNow : DateTime.Parse(reader.GetString(9))
        };

        return rule;
    }

    #endregion
}
