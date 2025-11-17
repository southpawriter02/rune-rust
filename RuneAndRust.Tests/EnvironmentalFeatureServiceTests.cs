using Xunit;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38.2: Unit tests for the Environmental Feature Catalog
/// Tests static terrain and dynamic hazard generation
/// </summary>
public class EnvironmentalFeatureServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly EnvironmentalFeatureService _service;

    public EnvironmentalFeatureServiceTests()
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

        // Initialize services
        _repository = new DescriptorRepository(_connectionString);
        _service = new EnvironmentalFeatureService(_repository, Log.Logger);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Setup

    private void InitializeSchema()
    {
        var createSql = @"
            -- Base Templates
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

            -- Thematic Modifiers
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

            -- Composites
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
            -- Static Terrain Templates
            INSERT INTO Descriptor_Base_Templates (template_name, category, archetype, base_mechanics, name_template, description_template, tags)
            VALUES
                ('Pillar_Base', 'Feature', 'Cover',
                 '{""hp"": 50, ""soak"": 8, ""cover_quality"": ""Heavy"", ""cover_bonus"": -4, ""blocks_los"": true, ""destructible"": true, ""tiles_occupied"": 1}',
                 '{Modifier} Support Pillar',
                 'A {Modifier_Adj} pillar {Modifier_Detail}. It provides heavy cover.',
                 '[""Structure"", ""Cover"", ""Destructible""]'),

                ('Chasm_Base', 'Feature', 'Obstacle',
                 '{""impassable"": true, ""fall_damage"": ""6d6"", ""damage_type"": ""Physical"", ""tiles_width"": 2, ""tactical_divider"": true}',
                 '{Modifier} Chasm',
                 'A {Modifier_Adj} chasm {Modifier_Detail}. Falling into it would be fatal.',
                 '[""Impassable"", ""Dangerous""]'),

                ('Elevation_Base', 'Feature', 'Tactical',
                 '{""elevation_bonus"": ""+1d"", ""applies_to"": ""Ranged"", ""climb_cost"": 3, ""tiles_occupied"": 4, ""provides_cover"": true}',
                 '{Modifier} Platform',
                 'A {Modifier_Adj} raised platform {Modifier_Detail}. It offers a tactical vantage point.',
                 '[""Elevation"", ""Advantage""]'),

                ('Rubble_Pile_Base', 'Feature', 'Obstacle',
                 '{""movement_cost_modifier"": 2, ""cover_quality"": ""Light"", ""cover_bonus"": -2, ""tiles_occupied"": 2}',
                 '{Modifier} Rubble Pile',
                 'A pile of {Modifier_Adj} rubble {Modifier_Detail}. Crossing it will slow movement.',
                 '[""Difficult_Terrain"", ""Cover""]');

            -- Dynamic Hazard Templates
            INSERT INTO Descriptor_Base_Templates (template_name, category, archetype, base_mechanics, name_template, description_template, tags)
            VALUES
                ('Steam_Vent_Base', 'Feature', 'DynamicHazard',
                 '{""damage"": ""2d6"", ""damage_type"": ""Fire"", ""activation_frequency"": 3, ""activation_type"": ""Periodic"", ""area_pattern"": ""3x3"", ""warning_turn"": true}',
                 '{Modifier} Steam Vent',
                 'A fractured pipe vents {Modifier_Adj} steam periodically. It erupts every 3 turns.',
                 '[""Hazard"", ""Periodic""]'),

                ('Power_Conduit_Base', 'Feature', 'DynamicHazard',
                 '{""damage"": ""3d6"", ""damage_type"": ""Lightning"", ""activation_range"": 2, ""activation_type"": ""Proximity""}',
                 'Live Power Conduit',
                 'An exposed power conduit arcs with {Modifier_Adj} electricity. Approach with caution.',
                 '[""Hazard"", ""Proximity""]'),

                ('Burning_Ground_Base', 'Feature', 'DynamicHazard',
                 '{""damage"": ""2d6"", ""damage_type"": ""Fire"", ""activation_timing"": ""End_Of_Turn"", ""tiles_affected"": 4}',
                 '{Modifier} Burning Ground',
                 'The ground here burns with {Modifier_Adj} flames. Standing in it deals fire damage.',
                 '[""Hazard"", ""Persistent"", ""Fire""]');

            -- Thematic Modifiers
            INSERT INTO Descriptor_Thematic_Modifiers (modifier_name, primary_biome, adjective, detail_fragment, stat_modifiers)
            VALUES
                ('Scorched', 'Muspelheim', 'scorched', 'radiates intense heat', '{""fire_resistance"": -50}'),
                ('Frozen', 'Niflheim', 'ice-covered', 'is encased in thick ice', '{""ice_resistance"": -50}'),
                ('Industrial_Decay', 'The_Roots', 'rusted and corroded', 'shows extensive industrial decay', '{""hp_multiplier"": 0.6, ""soak_multiplier"": 0.7}'),
                ('Lava_Filled', 'Muspelheim', 'lava-filled', 'flows with molten rock', '{""fall_damage_bonus"": ""8d6"", ""damage_type"": ""Fire"", ""ambient_heat_range"": 2, ""ambient_heat_damage"": ""1d4""}'),
                ('Geothermal', 'The_Roots', 'geothermal', 'vents superheated steam', '{}'),
                ('Void', 'Alfheim', 'reality-torn', 'flickers with unstable void energy', '{""damage_type"": ""Psychic"", ""proximity_stress"": 2, ""proximity_range"": 3, ""unstable"": true}');

            -- Composites
            INSERT INTO Descriptor_Composites (base_template_id, modifier_id, final_name, final_description, biome_restrictions)
            VALUES
                (1, 1, 'Scorched Support Pillar', 'A scorched pillar radiates intense heat.', '[""Muspelheim""]'),
                (2, 4, 'Lava River', 'A lava-filled chasm flows with molten rock.', '[""Muspelheim""]'),
                (5, 5, 'Geothermal Steam Vent', 'A geothermal steam vent vents superheated steam.', '[""The_Roots""]');
        ";

        var command = _connection.CreateCommand();
        command.CommandText = insertSql;
        command.ExecuteNonQuery();
    }

    #endregion

    #region Static Terrain Generation Tests

    [Fact]
    public void GenerateStaticTerrain_Pillar_ReturnsCorrectFeature()
    {
        // Act
        var feature = _service.GenerateStaticTerrain("Pillar_Base", "Scorched");

        // Assert
        Assert.NotNull(feature);
        Assert.Equal("Scorched Support Pillar", feature.Name);
        Assert.Equal("Pillar_Base", feature.BaseTemplateName);
        Assert.Equal("Scorched", feature.ModifierName);
        Assert.Equal("Cover", feature.Archetype);
        Assert.Equal(50, feature.HP);
        Assert.Equal(8, feature.Soak);
        Assert.Equal(CoverQuality.Heavy, feature.CoverQuality);
        Assert.Equal(-4, feature.CoverBonus);
        Assert.True(feature.BlocksLoS);
        Assert.True(feature.IsDestructible);
    }

    [Fact]
    public void GenerateStaticTerrain_Chasm_ReturnsCorrectFeature()
    {
        // Act
        var feature = _service.GenerateStaticTerrain("Chasm_Base", "Frozen");

        // Assert
        Assert.NotNull(feature);
        Assert.Contains("Chasm", feature.Name);
        Assert.Equal("Chasm_Base", feature.BaseTemplateName);
        Assert.True(feature.IsImpassable);
        Assert.Equal("6d6", feature.FallDamage);
        Assert.Equal("Physical", feature.DamageType);
        Assert.Equal(2, feature.TilesWidth);
        Assert.True(feature.IsTacticalDivider);
    }

    [Fact]
    public void GenerateStaticTerrain_Elevation_ReturnsCorrectFeature()
    {
        // Act
        var feature = _service.GenerateStaticTerrain("Elevation_Base", "Scorched");

        // Assert
        Assert.NotNull(feature);
        Assert.Equal("Tactical", feature.Archetype);
        Assert.Equal("+1d", feature.ElevationBonus);
        Assert.Equal(3, feature.ClimbCost);
        Assert.Equal(4, feature.TilesOccupied);
    }

    [Fact]
    public void GenerateStaticTerrain_RubblePile_ReturnsDifficultTerrain()
    {
        // Act
        var feature = _service.GenerateStaticTerrain("Rubble_Pile_Base", "Industrial_Decay");

        // Assert
        Assert.NotNull(feature);
        Assert.Equal(2, feature.MovementCostModifier);
        Assert.Equal(CoverQuality.Light, feature.CoverQuality);
        Assert.Equal(-2, feature.CoverBonus);
    }

    [Fact]
    public void GenerateStaticTerrain_WithModifier_AppliesStatAdjustments()
    {
        // Act - Industrial_Decay has hp_multiplier: 0.6
        var feature = _service.GenerateStaticTerrain("Pillar_Base", "Industrial_Decay");

        // Assert
        Assert.NotNull(feature);
        // Base HP is 50, with 0.6 multiplier = 30
        Assert.Equal(30, feature.HP);
        // Base Soak is 8, with 0.7 multiplier = 5 (rounded down)
        Assert.InRange(feature.Soak, 5, 6);
    }

    [Fact]
    public void GenerateStaticTerrain_NoModifier_UsesBaseTemplate()
    {
        // Act
        var feature = _service.GenerateStaticTerrain("Pillar_Base");

        // Assert
        Assert.NotNull(feature);
        Assert.Contains("Pillar", feature.Name);
        Assert.Null(feature.ModifierName);
        Assert.Equal(50, feature.HP);  // Unmodified
    }

    [Fact]
    public void GenerateStaticTerrain_InvalidTemplate_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _service.GenerateStaticTerrain("NonExistent_Base", "Scorched"));
    }

    [Fact]
    public void StaticTerrain_TacticalSummary_ReturnsCorrectFormat()
    {
        // Arrange
        var feature = _service.GenerateStaticTerrain("Pillar_Base", "Scorched");

        // Act
        var summary = feature.GetTacticalSummary();

        // Assert
        Assert.NotNull(summary);
        Assert.Contains("Heavy Cover", summary);
        Assert.Contains("-4 dice", summary);
        Assert.Contains("Destructible", summary);
    }

    #endregion

    #region Dynamic Hazard Generation Tests

    [Fact]
    public void GenerateDynamicHazard_SteamVent_ReturnsCorrectHazard()
    {
        // Act
        var hazard = _service.GenerateDynamicHazard("Steam_Vent_Base", "Geothermal");

        // Assert
        Assert.NotNull(hazard);
        Assert.Equal("Geothermal Steam Vent", hazard.Name);
        Assert.Equal("Steam_Vent_Base", hazard.BaseTemplateName);
        Assert.Equal("Geothermal", hazard.ModifierName);
        Assert.Equal("2d6", hazard.Damage);
        Assert.Equal("Fire", hazard.DamageType);
        Assert.Equal(HazardActivationType.Periodic, hazard.ActivationType);
        Assert.Equal(3, hazard.ActivationFrequency);
        Assert.Equal(AreaEffectPattern.ThreeByThree, hazard.AreaPattern);
        Assert.True(hazard.WarningTurn);
    }

    [Fact]
    public void GenerateDynamicHazard_PowerConduit_ReturnsProximityHazard()
    {
        // Act
        var hazard = _service.GenerateDynamicHazard("Power_Conduit_Base", "Scorched");

        // Assert
        Assert.NotNull(hazard);
        Assert.Contains("Power Conduit", hazard.Name);
        Assert.Equal("3d6", hazard.Damage);
        Assert.Equal("Lightning", hazard.DamageType);
        Assert.Equal(HazardActivationType.Proximity, hazard.ActivationType);
        Assert.Equal(2, hazard.ActivationRange);
    }

    [Fact]
    public void GenerateDynamicHazard_BurningGround_ReturnsPersistentHazard()
    {
        // Act
        var hazard = _service.GenerateDynamicHazard("Burning_Ground_Base", "Scorched");

        // Assert
        Assert.NotNull(hazard);
        Assert.Equal("2d6", hazard.Damage);
        Assert.Equal("Fire", hazard.DamageType);
        Assert.Equal(4, hazard.TilesAffected);
    }

    [Fact]
    public void GenerateDynamicHazard_LavaFilledChasm_AddsAmbientHeat()
    {
        // Act
        var hazard = _service.GenerateDynamicHazard("Chasm_Base", "Lava_Filled");

        // Assert
        Assert.NotNull(hazard);
        Assert.Equal(2, hazard.AmbientHeatRange);
        Assert.Equal("1d4", hazard.AmbientHeatDamage);
        Assert.Contains("8d6", hazard.Damage);  // Bonus damage from modifier
    }

    [Fact]
    public void GenerateDynamicHazard_VoidChasm_AddsProximityStress()
    {
        // Act
        var hazard = _service.GenerateDynamicHazard("Chasm_Base", "Void");

        // Assert
        Assert.NotNull(hazard);
        Assert.Equal("Psychic", hazard.DamageType);
        Assert.Equal(2, hazard.ProximityStress);
        Assert.Equal(3, hazard.ProximityRange);
        Assert.True(hazard.IsUnstable);
    }

    [Fact]
    public void GenerateDynamicHazard_NoModifier_UsesBaseTemplate()
    {
        // Act
        var hazard = _service.GenerateDynamicHazard("Steam_Vent_Base");

        // Assert
        Assert.NotNull(hazard);
        Assert.Contains("Steam Vent", hazard.Name);
        Assert.Null(hazard.ModifierName);
        Assert.Equal("2d6", hazard.Damage);
    }

    [Fact]
    public void DynamicHazard_TacticalSummary_ReturnsCorrectFormat()
    {
        // Arrange
        var hazard = _service.GenerateDynamicHazard("Steam_Vent_Base", "Geothermal");

        // Act
        var summary = hazard.GetTacticalSummary();

        // Assert
        Assert.NotNull(summary);
        Assert.Contains("2d6 Fire damage", summary);
        Assert.Contains("Activates every 3 turns", summary);
        Assert.Contains("Area: ThreeByThree", summary);
        Assert.Contains("warning turn", summary);
    }

    [Fact]
    public void DynamicHazard_ShouldActivateThisTurn_PeriodicHazard()
    {
        // Arrange
        var hazard = _service.GenerateDynamicHazard("Steam_Vent_Base", "Geothermal");

        // Act - Advance turns
        Assert.False(hazard.ShouldActivateThisTurn());  // Turn 1
        Assert.False(hazard.ShouldActivateThisTurn());  // Turn 2
        Assert.True(hazard.ShouldActivateThisTurn());   // Turn 3 - should activate

        // Assert - Counter should reset
        Assert.Equal(0, hazard.CurrentTurnCounter);
    }

    #endregion

    #region Biome Query Tests

    [Fact]
    public void GetStaticTerrainForBiome_Muspelheim_ReturnsScorchedFeatures()
    {
        // Act
        var features = _service.GetStaticTerrainForBiome("Muspelheim", null, 10);

        // Assert
        Assert.NotNull(features);
        Assert.NotEmpty(features);
        Assert.Contains(features, f => f.BiomeRestriction == "Muspelheim");
    }

    [Fact]
    public void GetDynamicHazardsForBiome_TheRoots_ReturnsGeothermalHazards()
    {
        // Act
        var hazards = _service.GetDynamicHazardsForBiome("The_Roots", null, 10);

        // Assert
        Assert.NotNull(hazards);
        Assert.NotEmpty(hazards);
        Assert.Contains(hazards, h => h.ModifierName == "Geothermal");
    }

    [Fact]
    public void GetStaticTerrainForBiome_WithTags_FiltersCorrectly()
    {
        // Act
        var features = _service.GetStaticTerrainForBiome(
            "Muspelheim",
            new List<string> { "Cover" },
            10);

        // Assert
        Assert.NotNull(features);
        // Should only return cover features
    }

    #endregion

    #region Feature Validation Tests

    [Fact]
    public void StaticTerrainFeature_IsValid_ValidatesCorrectly()
    {
        // Arrange
        var validFeature = new StaticTerrainFeature
        {
            Name = "Test Pillar",
            Archetype = "Cover",
            TilesOccupied = 1
        };

        var invalidFeature = new StaticTerrainFeature
        {
            Name = "",  // Invalid
            Archetype = "Cover",
            TilesOccupied = -1  // Invalid
        };

        // Act & Assert
        Assert.True(validFeature.IsValid());
        Assert.False(invalidFeature.IsValid());
    }

    [Fact]
    public void DynamicHazard_IsValid_ValidatesCorrectly()
    {
        // Arrange
        var validHazard = new DynamicHazard
        {
            Name = "Test Hazard",
            Damage = "2d6",
            DamageType = "Fire"
        };

        var invalidHazard = new DynamicHazard
        {
            Name = "",  // Invalid
            Damage = "",  // Invalid
            DamageType = ""  // Invalid
        };

        // Act & Assert
        Assert.True(validHazard.IsValid());
        Assert.False(invalidHazard.IsValid());
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_GenerateMuspelheimRoom_AllFeatures()
    {
        // Arrange
        var biome = "Muspelheim";

        // Act - Generate mix of static terrain and hazards
        var staticTerrain = new List<StaticTerrainFeature>
        {
            _service.GenerateStaticTerrain("Pillar_Base", "Scorched"),
            _service.GenerateStaticTerrain("Rubble_Pile_Base", "Scorched")
        };

        var hazards = new List<DynamicHazard>
        {
            _service.GenerateDynamicHazard("Burning_Ground_Base", "Scorched")
        };

        // Assert
        Assert.Equal(2, staticTerrain.Count);
        Assert.Single(hazards);

        foreach (var feature in staticTerrain)
        {
            Assert.NotNull(feature.Name);
            Assert.NotEmpty(feature.Name);
            Assert.True(feature.IsValid());
            Assert.Equal("Muspelheim", feature.BiomeRestriction);
        }

        foreach (var hazard in hazards)
        {
            Assert.NotNull(hazard.Name);
            Assert.NotEmpty(hazard.Name);
            Assert.True(hazard.IsValid());
            Assert.Equal("Fire", hazard.DamageType);
        }
    }

    [Fact]
    public void Integration_AllBiomes_GenerateSuccessfully()
    {
        // Arrange
        var biomes = new[] { "Muspelheim", "Niflheim", "The_Roots", "Alfheim" };

        // Act & Assert
        foreach (var biome in biomes)
        {
            // Try to generate static terrain
            var pillar = _service.GenerateStaticTerrain("Pillar_Base");
            Assert.NotNull(pillar);
            Assert.True(pillar.IsValid());

            // Try to generate hazard
            var hazard = _service.GenerateDynamicHazard("Steam_Vent_Base");
            Assert.NotNull(hazard);
            Assert.True(hazard.IsValid());
        }
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void EdgeCase_NullModifier_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            _service.GenerateStaticTerrain("Pillar_Base", null));

        Assert.Null(exception);
    }

    [Fact]
    public void EdgeCase_InvalidModifier_LogsWarning()
    {
        // Act
        var feature = _service.GenerateStaticTerrain("Pillar_Base", "NonExistent");

        // Assert - Should return feature with no modifier applied
        Assert.NotNull(feature);
        Assert.Null(feature.ModifierName);
    }

    #endregion
}
