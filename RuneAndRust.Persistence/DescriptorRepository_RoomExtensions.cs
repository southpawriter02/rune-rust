using Microsoft.Data.Sqlite;
using RuneAndRust.Core.Descriptors;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38.1: Room-specific extensions to DescriptorRepository
/// Handles descriptor fragments and room function variants
/// </summary>
public partial class DescriptorRepository
{
    #region Descriptor Fragments

    /// <summary>
    /// Gets descriptor fragments by category and optional tags
    /// </summary>
    public List<DescriptorFragment> GetDescriptorFragments(
        string category,
        List<string>? tags = null,
        bool onlyActive = true)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                fragment_id, category, subcategory, fragment_text,
                tags, weight, is_active, created_at, updated_at
            FROM Descriptor_Fragments
            WHERE category = $category
        ";

        if (onlyActive)
        {
            command.CommandText += " AND is_active = 1";
        }

        command.CommandText += " ORDER BY weight DESC, fragment_text";

        command.Parameters.AddWithValue("$category", category);

        var fragments = new List<DescriptorFragment>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            fragments.Add(MapDescriptorFragment(reader));
        }

        // Post-process: Filter by tags if specified
        if (tags != null && tags.Count > 0)
        {
            fragments = fragments.Where(f => f.MatchesTags(tags)).ToList();
        }

        _log.Debug("Loaded {Count} descriptor fragments for category {Category}",
            fragments.Count, category);

        return fragments;
    }

    /// <summary>
    /// Gets descriptor fragments by subcategory
    /// </summary>
    public List<DescriptorFragment> GetDescriptorFragmentsBySubcategory(
        string category,
        string subcategory,
        List<string>? tags = null,
        bool onlyActive = true)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                fragment_id, category, subcategory, fragment_text,
                tags, weight, is_active, created_at, updated_at
            FROM Descriptor_Fragments
            WHERE category = $category AND subcategory = $subcategory
        ";

        if (onlyActive)
        {
            command.CommandText += " AND is_active = 1";
        }

        command.CommandText += " ORDER BY weight DESC, fragment_text";

        command.Parameters.AddWithValue("$category", category);
        command.Parameters.AddWithValue("$subcategory", subcategory);

        var fragments = new List<DescriptorFragment>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            fragments.Add(MapDescriptorFragment(reader));
        }

        // Post-process: Filter by tags if specified
        if (tags != null && tags.Count > 0)
        {
            fragments = fragments.Where(f => f.MatchesTags(tags)).ToList();
        }

        _log.Debug("Loaded {Count} descriptor fragments for {Category}/{Subcategory}",
            fragments.Count, category, subcategory);

        return fragments;
    }

    private DescriptorFragment MapDescriptorFragment(SqliteDataReader reader)
    {
        return new DescriptorFragment
        {
            FragmentId = reader.GetInt32(0),
            Category = reader.GetString(1),
            Subcategory = reader.IsDBNull(2) ? null : reader.GetString(2),
            FragmentText = reader.GetString(3),
            Tags = reader.IsDBNull(4) ? null : reader.GetString(4),
            Weight = (float)reader.GetDouble(5),
            IsActive = reader.GetInt32(6) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(7)),
            UpdatedAt = DateTime.Parse(reader.GetString(8))
        };
    }

    #endregion

    #region Room Function Variants

    /// <summary>
    /// Gets room function variants by archetype and optional biome
    /// </summary>
    public List<RoomFunctionVariant> GetRoomFunctionVariants(
        string? archetype = null,
        string? biome = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                function_id, function_name, function_detail,
                biome_affinity, archetype, tags, created_at
            FROM Room_Function_Variants
            WHERE 1=1
        ";

        if (!string.IsNullOrEmpty(archetype))
        {
            command.CommandText += " AND archetype = $archetype";
            command.Parameters.AddWithValue("$archetype", archetype);
        }

        if (!string.IsNullOrEmpty(biome))
        {
            command.CommandText += " AND (biome_affinity LIKE $biome OR biome_affinity IS NULL)";
            command.Parameters.AddWithValue("$biome", $"%{biome}%");
        }

        command.CommandText += " ORDER BY function_name";

        var functions = new List<RoomFunctionVariant>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            functions.Add(MapRoomFunctionVariant(reader));
        }

        _log.Debug("Loaded {Count} room function variants (archetype: {Archetype}, biome: {Biome})",
            functions.Count, archetype, biome);

        return functions;
    }

    /// <summary>
    /// Gets a random room function variant matching criteria
    /// </summary>
    public RoomFunctionVariant? GetRandomFunctionVariant(
        string? archetype = null,
        string? biome = null,
        Random? random = null)
    {
        var functions = GetRoomFunctionVariants(archetype, biome);

        if (functions.Count == 0)
            return null;

        var rng = random ?? new Random();
        return functions[rng.Next(functions.Count)];
    }

    private RoomFunctionVariant MapRoomFunctionVariant(SqliteDataReader reader)
    {
        return new RoomFunctionVariant
        {
            FunctionId = reader.GetInt32(0),
            FunctionName = reader.GetString(1),
            FunctionDetail = reader.GetString(2),
            BiomeAffinity = reader.IsDBNull(3) ? null : reader.GetString(3),
            Archetype = reader.GetString(4),
            Tags = reader.IsDBNull(5) ? null : reader.GetString(5),
            CreatedAt = DateTime.Parse(reader.GetString(6))
        };
    }

    #endregion
}
