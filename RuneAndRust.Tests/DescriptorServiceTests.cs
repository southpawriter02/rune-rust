using Xunit;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38: Unit tests for the Descriptor Library Framework
/// Tests base templates, modifiers, composites, querying, and weighted selection
/// </summary>
public class DescriptorServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly DescriptorService _service;

    public DescriptorServiceTests()
    {
        // Configure Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        // Create in-memory database
        _connectionString = "Data Source=:memory:";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        // Initialize schema
        InitializeSchema();

        // Seed test data
        SeedTestData();

        // Initialize repository and service
        _repository = new DescriptorRepository(_connectionString);
        _service = new DescriptorService(_repository);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Setup

    private void InitializeSchema()
    {
        var createSql = @"
            CREATE TABLE Descriptor_Base_Templates (
                template_id INTEGER PRIMARY KEY AUTOINCREMENT,
                template_name TEXT NOT NULL UNIQUE,
                category TEXT NOT NULL,
                archetype TEXT NOT NULL,
                base_mechanics TEXT,
                name_template TEXT NOT NULL,
                description_template TEXT NOT NULL,
                tags TEXT,
                notes TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE Descriptor_Thematic_Modifiers (
                modifier_id INTEGER PRIMARY KEY AUTOINCREMENT,
                modifier_name TEXT NOT NULL UNIQUE,
                primary_biome TEXT NOT NULL,
                adjective TEXT NOT NULL,
                detail_fragment TEXT NOT NULL,
                stat_modifiers TEXT,
                status_effects TEXT,
                color_palette TEXT,
                ambient_sounds TEXT,
                particle_effects TEXT,
                notes TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE Descriptor_Composites (
                composite_id INTEGER PRIMARY KEY AUTOINCREMENT,
                base_template_id INTEGER NOT NULL,
                modifier_id INTEGER,
                final_name TEXT NOT NULL,
                final_description TEXT NOT NULL,
                final_mechanics TEXT,
                biome_restrictions TEXT,
                spawn_weight REAL DEFAULT 1.0,
                spawn_rules TEXT,
                is_active INTEGER DEFAULT 1,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP
            );
        ";

        var command = _connection.CreateCommand();
        command.CommandText = createSql;
        command.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        var insertSql = @"
            -- Base Templates
            INSERT INTO Descriptor_Base_Templates (template_name, category, archetype, base_mechanics, name_template, description_template, tags)
            VALUES
                ('Pillar_Base', 'Feature', 'Cover', '{""hp"": 50, ""soak"": 8}', '{Modifier} Support Pillar', 'A {Modifier_Adj} pillar that {Modifier_Detail}.', '[""Structure"", ""Cover""]'),
                ('Corridor_Base', 'Room', 'Passage', '{""size"": ""Small""}', '{Modifier} Corridor', 'A {Modifier_Adj} passageway.', '[""Corridor""]'),
                ('Container_Base', 'Object', 'Container', '{""hp"": 20}', '{Modifier} Container', 'A {Modifier_Adj} container.', '[""Lootable""]');

            -- Thematic Modifiers
            INSERT INTO Descriptor_Thematic_Modifiers (modifier_name, primary_biome, adjective, detail_fragment, stat_modifiers)
            VALUES
                ('Scorched', 'Muspelheim', 'scorched', 'radiates intense heat', '{""fire_resistance"": -50}'),
                ('Frozen', 'Niflheim', 'ice-covered', 'drips with meltwater', '{""ice_resistance"": -50}'),
                ('Rusted', 'The_Roots', 'corroded', 'shows extensive decay', '{""hp_multiplier"": 0.7}');

            -- Composites
            INSERT INTO Descriptor_Composites (base_template_id, modifier_id, final_name, final_description, final_mechanics, biome_restrictions, spawn_weight)
            VALUES
                (1, 1, 'Scorched Support Pillar', 'A scorched pillar that radiates intense heat.', '{""hp"": 50, ""soak"": 8, ""fire_resistance"": -50}', '[""Muspelheim""]', 1.0),
                (1, 2, 'Ice-Covered Support Pillar', 'An ice-covered pillar that drips with meltwater.', '{""hp"": 50, ""soak"": 8, ""ice_resistance"": -50}', '[""Niflheim""]', 1.2),
                (1, 3, 'Corroded Support Pillar', 'A corroded pillar that shows extensive decay.', '{""hp"": 35, ""soak"": 8}', '[""The_Roots""]', 0.8);
        ";

        var command = _connection.CreateCommand();
        command.CommandText = insertSql;
        command.ExecuteNonQuery();
    }

    #endregion

    #region Base Template Tests

    [Fact]
    public void GetBaseTemplate_ByName_ReturnsTemplate()
    {
        // Act
        var template = _service.GetBaseTemplate("Pillar_Base");

        // Assert
        Assert.NotNull(template);
        Assert.Equal("Pillar_Base", template.TemplateName);
        Assert.Equal("Feature", template.Category);
        Assert.Equal("Cover", template.Archetype);
    }

    [Fact]
    public void GetBaseTemplate_InvalidName_ReturnsNull()
    {
        // Act
        var template = _service.GetBaseTemplate("NonExistent_Base");

        // Assert
        Assert.Null(template);
    }

    [Fact]
    public void GetBaseTemplatesByCategory_ReturnsFilteredTemplates()
    {
        // Act
        var templates = _service.GetBaseTemplatesByCategory("Feature");

        // Assert
        Assert.Single(templates);
        Assert.Equal("Pillar_Base", templates[0].TemplateName);
    }

    [Fact]
    public void GetBaseTemplatesByArchetype_ReturnsFilteredTemplates()
    {
        // Act
        var templates = _service.GetBaseTemplatesByArchetype("Cover");

        // Assert
        Assert.Single(templates);
        Assert.Equal("Pillar_Base", templates[0].TemplateName);
    }

    [Fact]
    public void GetAllBaseTemplates_ReturnsAllTemplates()
    {
        // Act
        var templates = _service.GetAllBaseTemplates();

        // Assert
        Assert.Equal(3, templates.Count);
    }

    #endregion

    #region Thematic Modifier Tests

    [Fact]
    public void GetModifier_ByName_ReturnsModifier()
    {
        // Act
        var modifier = _service.GetModifier("Scorched");

        // Assert
        Assert.NotNull(modifier);
        Assert.Equal("Scorched", modifier.ModifierName);
        Assert.Equal("Muspelheim", modifier.PrimaryBiome);
        Assert.Equal("scorched", modifier.Adjective);
    }

    [Fact]
    public void GetModifier_InvalidName_ReturnsNull()
    {
        // Act
        var modifier = _service.GetModifier("NonExistent");

        // Assert
        Assert.Null(modifier);
    }

    [Fact]
    public void GetModifiersForBiome_ReturnsFilteredModifiers()
    {
        // Act
        var modifiers = _service.GetModifiersForBiome("Muspelheim");

        // Assert
        Assert.Single(modifiers);
        Assert.Equal("Scorched", modifiers[0].ModifierName);
    }

    [Fact]
    public void GetAllModifiers_ReturnsAllModifiers()
    {
        // Act
        var modifiers = _service.GetAllModifiers();

        // Assert
        Assert.Equal(3, modifiers.Count);
    }

    #endregion

    #region Composite Descriptor Tests

    [Fact]
    public void GetComposite_ByName_ReturnsComposite()
    {
        // Act
        var composite = _service.GetCompositeByName("Scorched Support Pillar");

        // Assert
        Assert.NotNull(composite);
        Assert.Equal("Scorched Support Pillar", composite.FinalName);
        Assert.NotNull(composite.BaseTemplate);
        Assert.NotNull(composite.Modifier);
        Assert.Equal("Pillar_Base", composite.BaseTemplate.TemplateName);
        Assert.Equal("Scorched", composite.Modifier.ModifierName);
    }

    [Fact]
    public void ComposeDescriptor_ExistingComposite_ReturnsFromDatabase()
    {
        // Act
        var composite = _service.ComposeDescriptor("Pillar_Base", "Scorched");

        // Assert
        Assert.NotNull(composite);
        Assert.Equal("Scorched Support Pillar", composite.FinalName);
        Assert.True(composite.CompositeId > 0);  // From database
    }

    [Fact]
    public void ComposeDescriptor_NewComposite_GeneratesOnTheFly()
    {
        // Act
        var composite = _service.ComposeDescriptor("Container_Base", "Scorched");

        // Assert
        Assert.NotNull(composite);
        Assert.Contains("Scorched", composite.FinalName);
        Assert.Contains("Container", composite.FinalName);
        Assert.Equal(0, composite.CompositeId);  // Not from database
    }

    [Fact]
    public void ComposeDescriptor_NoModifier_ReturnsUnmodifiedTemplate()
    {
        // Act
        var composite = _service.ComposeDescriptor("Container_Base", null);

        // Assert
        Assert.NotNull(composite);
        Assert.Equal("Container", composite.FinalName);
        Assert.Null(composite.Modifier);
    }

    #endregion

    #region Query System Tests

    [Fact]
    public void QueryDescriptors_ByCategory_ReturnsFilteredResults()
    {
        // Arrange
        var query = new DescriptorQuery
        {
            Category = "Feature"
        };

        // Act
        var result = _service.QueryDescriptors(query);

        // Assert
        Assert.Equal(3, result.TotalCount);  // 3 pillar variants
        Assert.All(result.Descriptors, d =>
            Assert.Contains("Pillar", d.FinalName));
    }

    [Fact]
    public void QueryDescriptors_ByBiome_ReturnsFilteredResults()
    {
        // Arrange
        var query = new DescriptorQuery
        {
            Biome = "Muspelheim"
        };

        // Act
        var result = _service.QueryDescriptors(query);

        // Assert
        Assert.Single(result.Descriptors);
        Assert.Equal("Scorched Support Pillar", result.Descriptors[0].FinalName);
    }

    [Fact]
    public void QueryDescriptors_ByModifier_ReturnsFilteredResults()
    {
        // Arrange
        var query = new DescriptorQuery
        {
            ModifierName = "Frozen"
        };

        // Act
        var result = _service.QueryDescriptors(query);

        // Assert
        Assert.Single(result.Descriptors);
        Assert.Equal("Ice-Covered Support Pillar", result.Descriptors[0].FinalName);
    }

    [Fact]
    public void QueryDescriptors_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var query = new DescriptorQuery
        {
            Category = "Feature",
            Limit = 2
        };

        // Act
        var result = _service.QueryDescriptors(query);

        // Assert
        Assert.Equal(2, result.Descriptors.Count);
    }

    #endregion

    #region Weighted Selection Tests

    [Fact]
    public void WeightedRandomSelection_WithDescriptors_ReturnsDescriptor()
    {
        // Arrange
        var composites = new List<DescriptorComposite>
        {
            new() { FinalName = "A", SpawnWeight = 0.5f },
            new() { FinalName = "B", SpawnWeight = 1.0f },
            new() { FinalName = "C", SpawnWeight = 1.5f }
        };

        var random = new Random(42);  // Fixed seed for determinism

        // Act
        var selected = _service.WeightedRandomSelection(composites, random);

        // Assert
        Assert.NotNull(selected);
        Assert.Contains(selected.FinalName, new[] { "A", "B", "C" });
    }

    [Fact]
    public void WeightedRandomSelection_EmptyList_ReturnsNull()
    {
        // Arrange
        var composites = new List<DescriptorComposite>();

        // Act
        var selected = _service.WeightedRandomSelection(composites);

        // Assert
        Assert.Null(selected);
    }

    [Fact]
    public void WeightedRandomSelection_Distribution_FavorsHigherWeights()
    {
        // Arrange
        var composites = new List<DescriptorComposite>
        {
            new() { FinalName = "Low", SpawnWeight = 0.1f },
            new() { FinalName = "High", SpawnWeight = 10.0f }
        };

        var random = new Random(42);
        var selections = new Dictionary<string, int> { { "Low", 0 }, { "High", 0 } };

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var selected = _service.WeightedRandomSelection(composites, random);
            if (selected != null)
            {
                selections[selected.FinalName]++;
            }
        }

        // Assert
        Assert.True(selections["High"] > selections["Low"],
            $"High weight should be selected more often. High: {selections["High"]}, Low: {selections["Low"]}");
    }

    #endregion

    #region Composition & Mechanics Tests

    [Fact]
    public void GenerateComposite_WithModifier_MergesMechanics()
    {
        // Arrange
        var baseTemplate = _service.GetBaseTemplate("Pillar_Base")!;
        var modifier = _service.GetModifier("Rusted")!;

        // Act
        var composite = _service.GenerateComposite(baseTemplate, modifier);

        // Assert
        Assert.NotNull(composite);
        Assert.Equal("Corroded Support Pillar", composite.FinalName);

        var mechanics = composite.GetFinalMechanics();
        Assert.NotNull(mechanics);

        // HP should be multiplied by 0.7 (from Rusted modifier)
        // Original: 50, Modified: 35
        Assert.True(mechanics.ContainsKey("hp"));
    }

    [Fact]
    public void MergeMechanics_AppliesMultipliers()
    {
        // Arrange
        var baseMechanics = "{\"hp\": 100, \"soak\": 10}";
        var statModifiers = "{\"hp_multiplier\": 0.5, \"soak\": 5}";

        // Act
        var merged = _service.MergeMechanics(baseMechanics, statModifiers);

        // Assert
        Assert.NotNull(merged);
        Assert.Contains("50", merged);  // HP should be 100 * 0.5 = 50
    }

    [Fact]
    public void GenerateComposite_WithoutModifier_UsesBaseTemplate()
    {
        // Arrange
        var baseTemplate = _service.GetBaseTemplate("Corridor_Base")!;

        // Act
        var composite = _service.GenerateComposite(baseTemplate, null);

        // Assert
        Assert.NotNull(composite);
        Assert.Equal("Corridor", composite.FinalName);
        Assert.Null(composite.Modifier);
        Assert.Contains("standard", composite.FinalDescription);
    }

    #endregion

    #region Library Stats Tests

    [Fact]
    public void GetLibraryStats_ReturnsCorrectCounts()
    {
        // Act
        var stats = _service.GetLibraryStats();

        // Assert
        Assert.Equal(3, stats.TotalBaseTemplates);
        Assert.Equal(3, stats.TotalModifiers);
        Assert.Equal(3, stats.TotalComposites);
        Assert.Equal(3, stats.ActiveComposites);
        Assert.Equal(0, stats.InactiveComposites);
    }

    [Fact]
    public void GetLibraryStats_GroupsByCategoryCorrectly()
    {
        // Act
        var stats = _service.GetLibraryStats();

        // Assert
        Assert.True(stats.BaseTemplatesByCategory.ContainsKey("Feature"));
        Assert.Equal(1, stats.BaseTemplatesByCategory["Feature"]);
        Assert.True(stats.BaseTemplatesByCategory.ContainsKey("Room"));
        Assert.Equal(1, stats.BaseTemplatesByCategory["Room"]);
    }

    #endregion

    #region Model Validation Tests

    [Fact]
    public void DescriptorBaseTemplate_IsValid_ValidatesCorrectly()
    {
        // Arrange
        var validTemplate = new DescriptorBaseTemplate
        {
            TemplateName = "Test_Base",
            Category = "Feature",
            Archetype = "Cover",
            NameTemplate = "Test",
            DescriptionTemplate = "Test description"
        };

        var invalidTemplate = new DescriptorBaseTemplate
        {
            TemplateName = "",  // Invalid
            Category = "Invalid",  // Invalid category
            Archetype = "Cover",
            NameTemplate = "Test",
            DescriptionTemplate = "Test"
        };

        // Act & Assert
        Assert.True(validTemplate.IsValid());
        Assert.False(invalidTemplate.IsValid());
    }

    [Fact]
    public void ThematicModifier_IsValid_ValidatesCorrectly()
    {
        // Arrange
        var validModifier = new ThematicModifier
        {
            ModifierName = "Test",
            PrimaryBiome = "Muspelheim",
            Adjective = "test",
            DetailFragment = "test detail"
        };

        var invalidModifier = new ThematicModifier
        {
            ModifierName = "",  // Invalid
            PrimaryBiome = "InvalidBiome",  // Invalid biome
            Adjective = "test",
            DetailFragment = "test"
        };

        // Act & Assert
        Assert.True(validModifier.IsValid());
        Assert.False(invalidModifier.IsValid());
    }

    [Fact]
    public void DescriptorComposite_CanSpawnInBiome_ChecksRestrictionsCorrectly()
    {
        // Arrange
        var composite = new DescriptorComposite
        {
            BiomeRestrictions = "[\"Muspelheim\", \"Niflheim\"]"
        };

        // Act & Assert
        Assert.True(composite.CanSpawnInBiome("Muspelheim"));
        Assert.True(composite.CanSpawnInBiome("Niflheim"));
        Assert.False(composite.CanSpawnInBiome("Alfheim"));
    }

    #endregion
}
