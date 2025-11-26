using Microsoft.Data.Sqlite;
using RuneAndRust.Core.Descriptors;
using Serilog;
using System.IO;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.38: Data repository for the Descriptor Library Framework
/// Provides database access to base templates, thematic modifiers, and composite descriptors
/// v0.38.1: Extended with room descriptor fragments and function variants (see DescriptorRepository_RoomExtensions.cs)
/// </summary>
public partial class DescriptorRepository
{
    private static readonly ILogger _log = Log.ForContext<DescriptorRepository>();
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new DescriptorRepository with automatic database path resolution.
    /// Uses the runeandrust.db file in the specified directory or current directory.
    /// </summary>
    /// <param name="dataDirectory">Optional data directory. Defaults to current directory.</param>
    public DescriptorRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, "runeandrust.db");
        _connectionString = $"Data Source={dbPath}";
        _log.Debug("DescriptorRepository initialized with database path: {DbPath}", dbPath);
    }

    #region Base Templates

    /// <summary>
    /// Gets all base templates
    /// </summary>
    public List<DescriptorBaseTemplate> GetAllBaseTemplates()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id, template_name, category, archetype,
                base_mechanics, name_template, description_template,
                tags, notes, created_at, updated_at
            FROM Descriptor_Base_Templates
            ORDER BY category, template_name
        ";

        var templates = new List<DescriptorBaseTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(MapBaseTemplate(reader));
        }

        _log.Debug("Loaded {Count} base templates", templates.Count);
        return templates;
    }

    /// <summary>
    /// Gets a base template by ID
    /// </summary>
    public DescriptorBaseTemplate? GetBaseTemplateById(int templateId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id, template_name, category, archetype,
                base_mechanics, name_template, description_template,
                tags, notes, created_at, updated_at
            FROM Descriptor_Base_Templates
            WHERE template_id = $templateId
        ";
        command.Parameters.AddWithValue("$templateId", templateId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapBaseTemplate(reader);
        }

        _log.Warning("Base template not found: ID {TemplateId}", templateId);
        return null;
    }

    /// <summary>
    /// Gets a base template by name
    /// </summary>
    public DescriptorBaseTemplate? GetBaseTemplateByName(string templateName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id, template_name, category, archetype,
                base_mechanics, name_template, description_template,
                tags, notes, created_at, updated_at
            FROM Descriptor_Base_Templates
            WHERE template_name = $templateName
        ";
        command.Parameters.AddWithValue("$templateName", templateName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapBaseTemplate(reader);
        }

        _log.Warning("Base template not found: {TemplateName}", templateName);
        return null;
    }

    /// <summary>
    /// Gets base templates by category
    /// </summary>
    public List<DescriptorBaseTemplate> GetBaseTemplatesByCategory(string category)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id, template_name, category, archetype,
                base_mechanics, name_template, description_template,
                tags, notes, created_at, updated_at
            FROM Descriptor_Base_Templates
            WHERE category = $category
            ORDER BY template_name
        ";
        command.Parameters.AddWithValue("$category", category);

        var templates = new List<DescriptorBaseTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(MapBaseTemplate(reader));
        }

        _log.Debug("Loaded {Count} base templates for category {Category}", templates.Count, category);
        return templates;
    }

    /// <summary>
    /// Gets base templates by archetype
    /// </summary>
    public List<DescriptorBaseTemplate> GetBaseTemplatesByArchetype(string archetype)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                template_id, template_name, category, archetype,
                base_mechanics, name_template, description_template,
                tags, notes, created_at, updated_at
            FROM Descriptor_Base_Templates
            WHERE archetype = $archetype
            ORDER BY template_name
        ";
        command.Parameters.AddWithValue("$archetype", archetype);

        var templates = new List<DescriptorBaseTemplate>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            templates.Add(MapBaseTemplate(reader));
        }

        _log.Debug("Loaded {Count} base templates for archetype {Archetype}", templates.Count, archetype);
        return templates;
    }

    private DescriptorBaseTemplate MapBaseTemplate(SqliteDataReader reader)
    {
        return new DescriptorBaseTemplate
        {
            TemplateId = reader.GetInt32(0),
            TemplateName = reader.GetString(1),
            Category = reader.GetString(2),
            Archetype = reader.GetString(3),
            BaseMechanics = reader.IsDBNull(4) ? null : reader.GetString(4),
            NameTemplate = reader.GetString(5),
            DescriptionTemplate = reader.GetString(6),
            Tags = reader.IsDBNull(7) ? null : reader.GetString(7),
            Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
            CreatedAt = DateTime.Parse(reader.GetString(9)),
            UpdatedAt = DateTime.Parse(reader.GetString(10))
        };
    }

    #endregion

    #region Thematic Modifiers

    /// <summary>
    /// Gets all thematic modifiers
    /// </summary>
    public List<ThematicModifier> GetAllModifiers()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                modifier_id, modifier_name, primary_biome,
                adjective, detail_fragment,
                stat_modifiers, status_effects,
                color_palette, ambient_sounds, particle_effects,
                notes, created_at, updated_at
            FROM Descriptor_Thematic_Modifiers
            ORDER BY primary_biome, modifier_name
        ";

        var modifiers = new List<ThematicModifier>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapThematicModifier(reader));
        }

        _log.Debug("Loaded {Count} thematic modifiers", modifiers.Count);
        return modifiers;
    }

    /// <summary>
    /// Gets a thematic modifier by ID
    /// </summary>
    public ThematicModifier? GetModifierById(int modifierId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                modifier_id, modifier_name, primary_biome,
                adjective, detail_fragment,
                stat_modifiers, status_effects,
                color_palette, ambient_sounds, particle_effects,
                notes, created_at, updated_at
            FROM Descriptor_Thematic_Modifiers
            WHERE modifier_id = $modifierId
        ";
        command.Parameters.AddWithValue("$modifierId", modifierId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapThematicModifier(reader);
        }

        _log.Warning("Thematic modifier not found: ID {ModifierId}", modifierId);
        return null;
    }

    /// <summary>
    /// Gets a thematic modifier by name
    /// </summary>
    public ThematicModifier? GetModifierByName(string modifierName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                modifier_id, modifier_name, primary_biome,
                adjective, detail_fragment,
                stat_modifiers, status_effects,
                color_palette, ambient_sounds, particle_effects,
                notes, created_at, updated_at
            FROM Descriptor_Thematic_Modifiers
            WHERE modifier_name = $modifierName
        ";
        command.Parameters.AddWithValue("$modifierName", modifierName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapThematicModifier(reader);
        }

        _log.Warning("Thematic modifier not found: {ModifierName}", modifierName);
        return null;
    }

    /// <summary>
    /// Gets modifiers for a specific biome
    /// </summary>
    public List<ThematicModifier> GetModifiersForBiome(string biome)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                modifier_id, modifier_name, primary_biome,
                adjective, detail_fragment,
                stat_modifiers, status_effects,
                color_palette, ambient_sounds, particle_effects,
                notes, created_at, updated_at
            FROM Descriptor_Thematic_Modifiers
            WHERE primary_biome = $biome
            ORDER BY modifier_name
        ";
        command.Parameters.AddWithValue("$biome", biome);

        var modifiers = new List<ThematicModifier>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            modifiers.Add(MapThematicModifier(reader));
        }

        _log.Debug("Loaded {Count} modifiers for biome {Biome}", modifiers.Count, biome);
        return modifiers;
    }

    private ThematicModifier MapThematicModifier(SqliteDataReader reader)
    {
        return new ThematicModifier
        {
            ModifierId = reader.GetInt32(0),
            ModifierName = reader.GetString(1),
            PrimaryBiome = reader.GetString(2),
            Adjective = reader.GetString(3),
            DetailFragment = reader.GetString(4),
            StatModifiers = reader.IsDBNull(5) ? null : reader.GetString(5),
            StatusEffects = reader.IsDBNull(6) ? null : reader.GetString(6),
            ColorPalette = reader.IsDBNull(7) ? null : reader.GetString(7),
            AmbientSounds = reader.IsDBNull(8) ? null : reader.GetString(8),
            ParticleEffects = reader.IsDBNull(9) ? null : reader.GetString(9),
            Notes = reader.IsDBNull(10) ? null : reader.GetString(10),
            CreatedAt = DateTime.Parse(reader.GetString(11)),
            UpdatedAt = DateTime.Parse(reader.GetString(12))
        };
    }

    #endregion

    #region Composite Descriptors

    /// <summary>
    /// Gets all composite descriptors
    /// </summary>
    public List<DescriptorComposite> GetAllComposites(bool onlyActive = true)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                composite_id, base_template_id, modifier_id,
                final_name, final_description, final_mechanics,
                biome_restrictions, spawn_weight, spawn_rules,
                is_active, created_at, updated_at
            FROM Descriptor_Composites
        ";

        if (onlyActive)
        {
            command.CommandText += " WHERE is_active = 1";
        }

        command.CommandText += " ORDER BY final_name";

        var composites = new List<DescriptorComposite>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            composites.Add(MapComposite(reader));
        }

        _log.Debug("Loaded {Count} composite descriptors (onlyActive: {OnlyActive})",
            composites.Count, onlyActive);
        return composites;
    }

    /// <summary>
    /// Gets a composite descriptor by ID
    /// </summary>
    public DescriptorComposite? GetCompositeById(int compositeId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                composite_id, base_template_id, modifier_id,
                final_name, final_description, final_mechanics,
                biome_restrictions, spawn_weight, spawn_rules,
                is_active, created_at, updated_at
            FROM Descriptor_Composites
            WHERE composite_id = $compositeId
        ";
        command.Parameters.AddWithValue("$compositeId", compositeId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapComposite(reader);
        }

        _log.Warning("Composite descriptor not found: ID {CompositeId}", compositeId);
        return null;
    }

    /// <summary>
    /// Gets a composite descriptor by final name
    /// </summary>
    public DescriptorComposite? GetCompositeByName(string finalName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                composite_id, base_template_id, modifier_id,
                final_name, final_description, final_mechanics,
                biome_restrictions, spawn_weight, spawn_rules,
                is_active, created_at, updated_at
            FROM Descriptor_Composites
            WHERE final_name = $finalName
        ";
        command.Parameters.AddWithValue("$finalName", finalName);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapComposite(reader);
        }

        _log.Debug("Composite descriptor not found: {FinalName}", finalName);
        return null;
    }

    /// <summary>
    /// Gets composites for a specific base template
    /// </summary>
    public List<DescriptorComposite> GetCompositesForBaseTemplate(int baseTemplateId, bool onlyActive = true)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                composite_id, base_template_id, modifier_id,
                final_name, final_description, final_mechanics,
                biome_restrictions, spawn_weight, spawn_rules,
                is_active, created_at, updated_at
            FROM Descriptor_Composites
            WHERE base_template_id = $baseTemplateId
        ";

        if (onlyActive)
        {
            command.CommandText += " AND is_active = 1";
        }

        command.CommandText += " ORDER BY final_name";

        command.Parameters.AddWithValue("$baseTemplateId", baseTemplateId);

        var composites = new List<DescriptorComposite>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            composites.Add(MapComposite(reader));
        }

        _log.Debug("Loaded {Count} composites for base template ID {BaseTemplateId}",
            composites.Count, baseTemplateId);
        return composites;
    }

    /// <summary>
    /// Gets composites for a specific modifier
    /// </summary>
    public List<DescriptorComposite> GetCompositesForModifier(int modifierId, bool onlyActive = true)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                composite_id, base_template_id, modifier_id,
                final_name, final_description, final_mechanics,
                biome_restrictions, spawn_weight, spawn_rules,
                is_active, created_at, updated_at
            FROM Descriptor_Composites
            WHERE modifier_id = $modifierId
        ";

        if (onlyActive)
        {
            command.CommandText += " AND is_active = 1";
        }

        command.CommandText += " ORDER BY final_name";

        command.Parameters.AddWithValue("$modifierId", modifierId);

        var composites = new List<DescriptorComposite>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            composites.Add(MapComposite(reader));
        }

        _log.Debug("Loaded {Count} composites for modifier ID {ModifierId}",
            composites.Count, modifierId);
        return composites;
    }

    private DescriptorComposite MapComposite(SqliteDataReader reader)
    {
        return new DescriptorComposite
        {
            CompositeId = reader.GetInt32(0),
            BaseTemplateId = reader.GetInt32(1),
            ModifierId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            FinalName = reader.GetString(3),
            FinalDescription = reader.GetString(4),
            FinalMechanics = reader.IsDBNull(5) ? null : reader.GetString(5),
            BiomeRestrictions = reader.IsDBNull(6) ? null : reader.GetString(6),
            SpawnWeight = (float)reader.GetDouble(7),
            SpawnRules = reader.IsDBNull(8) ? null : reader.GetString(8),
            IsActive = reader.GetInt32(9) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(10)),
            UpdatedAt = DateTime.Parse(reader.GetString(11))
        };
    }

    #endregion

    #region Query System

    /// <summary>
    /// Queries composites based on filter criteria
    /// </summary>
    public List<DescriptorComposite> QueryComposites(DescriptorQuery query)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();

        // Build dynamic query
        var sql = @"
            SELECT
                dc.composite_id, dc.base_template_id, dc.modifier_id,
                dc.final_name, dc.final_description, dc.final_mechanics,
                dc.biome_restrictions, dc.spawn_weight, dc.spawn_rules,
                dc.is_active, dc.created_at, dc.updated_at
            FROM Descriptor_Composites dc
            INNER JOIN Descriptor_Base_Templates bt ON dc.base_template_id = bt.template_id
            LEFT JOIN Descriptor_Thematic_Modifiers tm ON dc.modifier_id = tm.modifier_id
            WHERE 1=1
        ";

        if (query.OnlyActive)
        {
            sql += " AND dc.is_active = 1";
        }

        if (!string.IsNullOrEmpty(query.Category))
        {
            sql += " AND bt.category = $category";
            command.Parameters.AddWithValue("$category", query.Category);
        }

        if (!string.IsNullOrEmpty(query.Archetype))
        {
            sql += " AND bt.archetype = $archetype";
            command.Parameters.AddWithValue("$archetype", query.Archetype);
        }

        if (!string.IsNullOrEmpty(query.Biome))
        {
            sql += " AND (dc.biome_restrictions LIKE $biome OR dc.biome_restrictions IS NULL)";
            command.Parameters.AddWithValue("$biome", $"%{query.Biome}%");
        }

        if (!string.IsNullOrEmpty(query.ModifierName))
        {
            sql += " AND tm.modifier_name = $modifierName";
            command.Parameters.AddWithValue("$modifierName", query.ModifierName);
        }

        if (!string.IsNullOrEmpty(query.BaseTemplateName))
        {
            sql += " AND bt.template_name = $baseTemplateName";
            command.Parameters.AddWithValue("$baseTemplateName", query.BaseTemplateName);
        }

        sql += " ORDER BY dc.final_name";

        if (query.Limit.HasValue)
        {
            sql += " LIMIT $limit";
            command.Parameters.AddWithValue("$limit", query.Limit.Value);
        }

        command.CommandText = sql;

        var composites = new List<DescriptorComposite>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            composites.Add(MapComposite(reader));
        }

        _log.Debug("Query returned {Count} composites (Category: {Category}, Archetype: {Archetype}, Biome: {Biome})",
            composites.Count, query.Category, query.Archetype, query.Biome);

        return composites;
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets statistics about the descriptor library
    /// </summary>
    public DescriptorLibraryStats GetLibraryStats()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stats = new DescriptorLibraryStats();

        // Total counts
        stats.TotalBaseTemplates = GetCount(connection, "Descriptor_Base_Templates");
        stats.TotalModifiers = GetCount(connection, "Descriptor_Thematic_Modifiers");
        stats.TotalComposites = GetCount(connection, "Descriptor_Composites");

        // Active/inactive composites
        stats.ActiveComposites = GetCount(connection, "Descriptor_Composites", "is_active = 1");
        stats.InactiveComposites = GetCount(connection, "Descriptor_Composites", "is_active = 0");

        // Base templates by category
        stats.BaseTemplatesByCategory = GetGroupCount(connection,
            "Descriptor_Base_Templates", "category");

        // Modifiers by biome
        stats.ModifiersByBiome = GetGroupCount(connection,
            "Descriptor_Thematic_Modifiers", "primary_biome");

        _log.Information("Descriptor library stats: {BaseTemplates} base templates, {Modifiers} modifiers, {Composites} composites",
            stats.TotalBaseTemplates, stats.TotalModifiers, stats.TotalComposites);

        return stats;
    }

    private int GetCount(SqliteConnection connection, string tableName, string? whereClause = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";

        if (!string.IsNullOrEmpty(whereClause))
        {
            command.CommandText += $" WHERE {whereClause}";
        }

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private Dictionary<string, int> GetGroupCount(SqliteConnection connection, string tableName, string groupColumn)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"SELECT {groupColumn}, COUNT(*) FROM {tableName} GROUP BY {groupColumn}";

        var results = new Dictionary<string, int>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var key = reader.GetString(0);
            var count = reader.GetInt32(1);
            results[key] = count;
        }

        return results;
    }

    #endregion
}
