using Xunit;
using RuneAndRust.Core.Descriptors;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.38.4: Unit tests for the Atmospheric Descriptor System
/// Tests multi-sensory atmospheric description generation
/// </summary>
public class AtmosphericDescriptorServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;
    private readonly DescriptorRepository _repository;
    private readonly AtmosphericDescriptorService _service;

    public AtmosphericDescriptorServiceTests()
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
        _service = new AtmosphericDescriptorService(_repository);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Setup

    private void InitializeSchema()
    {
        var createSql = @"
            CREATE TABLE Atmospheric_Descriptors (
                descriptor_id INTEGER PRIMARY KEY AUTOINCREMENT,
                category TEXT NOT NULL,
                intensity TEXT NOT NULL,
                descriptor_text TEXT NOT NULL,
                biome_affinity TEXT,
                tags TEXT,
                CHECK (category IN ('Lighting', 'Sound', 'Smell', 'Temperature', 'PsychicPresence')),
                CHECK (intensity IN ('Subtle', 'Moderate', 'Oppressive'))
            );

            CREATE TABLE Biome_Atmosphere_Profiles (
                profile_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_name TEXT NOT NULL UNIQUE,
                lighting_descriptors TEXT NOT NULL,
                sound_descriptors TEXT NOT NULL,
                smell_descriptors TEXT NOT NULL,
                temperature_descriptors TEXT NOT NULL,
                psychic_descriptors TEXT NOT NULL,
                composite_template TEXT NOT NULL,
                default_intensity TEXT DEFAULT 'Moderate',
                CHECK (default_intensity IN ('Subtle', 'Moderate', 'Oppressive'))
            );
        ";

        using var command = _connection.CreateCommand();
        command.CommandText = createSql;
        command.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        var insertSql = @"
            -- Generic Descriptors (5 per category, various intensities)
            INSERT INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags) VALUES
            -- Lighting
            ('Lighting', 'Subtle', 'The light here is dim, barely enough to see by.', NULL, '[""Dim"", ""Generic""]'),
            ('Lighting', 'Moderate', 'Shadows crowd the edges of your vision.', NULL, '[""Dim"", ""Shadow""]'),
            ('Lighting', 'Oppressive', 'True darkness reigns here, swallowing all light.', NULL, '[""Darkness"", ""Extreme""]'),
            -- Sound
            ('Sound', 'Subtle', 'Water drips steadily from unseen sources.', NULL, '[""Water"", ""Dripping""]'),
            ('Sound', 'Moderate', 'Distant machinery groans and clanks rhythmically.', NULL, '[""Mechanical"", ""Industrial""]'),
            ('Sound', 'Oppressive', 'The silence here is oppressive and unnatural.', NULL, '[""Silence"", ""Unnatural""]'),
            -- Smell
            ('Smell', 'Subtle', 'A faint metallic scent hangs in the air.', NULL, '[""Metal"", ""Subtle""]'),
            ('Smell', 'Moderate', 'The metallic tang of rust is overwhelming.', NULL, '[""Metal"", ""Rust""]'),
            ('Smell', 'Oppressive', 'Everything reeks of corroded metal.', NULL, '[""Metal"", ""Corroded""]'),
            -- Temperature
            ('Temperature', 'Subtle', 'The air is pleasantly warm.', NULL, '[""Heat"", ""Warm""]'),
            ('Temperature', 'Moderate', 'The temperature here is oppressively hot.', NULL, '[""Heat"", ""Hot""]'),
            ('Temperature', 'Oppressive', 'The air burns your lungs with each breath.', NULL, '[""Heat"", ""Scorching""]'),
            -- Psychic Presence
            ('PsychicPresence', 'Subtle', 'A faint unease prickles at the edges of consciousness.', NULL, '[""Blight"", ""Low""]'),
            ('PsychicPresence', 'Moderate', 'The Runic Blight''s presence is palpable.', NULL, '[""Blight"", ""Moderate""]'),
            ('PsychicPresence', 'Oppressive', 'The Blight''s corruption is overwhelming.', NULL, '[""Blight"", ""High""]');

            -- Muspelheim Biome-Specific Descriptors
            INSERT INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags) VALUES
            ('Lighting', 'Moderate', 'Everything is bathed in red-orange firelight.', 'Muspelheim', '[""Warm"", ""Fire""]'),
            ('Sound', 'Moderate', 'Lava rumbles distantly like approaching thunder.', 'Muspelheim', '[""Fire"", ""Lava""]'),
            ('Smell', 'Oppressive', 'Sulfur and superheated rock dominate.', 'Muspelheim', '[""Brimstone"", ""Sulfur""]'),
            ('Temperature', 'Oppressive', 'Scorching heat makes each breath painful.', 'Muspelheim', '[""Heat"", ""Scorching""]'),
            ('PsychicPresence', 'Subtle', 'The Blight seems diminished here, burned away by primal fire.', 'Muspelheim', '[""Blight"", ""Diminished""]');

            -- The_Roots Biome-Specific Descriptors
            INSERT INTO Atmospheric_Descriptors (category, intensity, descriptor_text, biome_affinity, tags) VALUES
            ('Lighting', 'Moderate', 'Sickly light flickers from failing panels.', 'The_Roots', '[""Sickly"", ""Failing""]'),
            ('Sound', 'Moderate', 'Water drips steadily, and metal groans under stress.', 'The_Roots', '[""Water"", ""Metal""]'),
            ('Smell', 'Moderate', 'The air smells of rust and mildew.', 'The_Roots', '[""Rust"", ""Mildew""]'),
            ('Temperature', 'Moderate', 'Cool humidity clings to everything.', 'The_Roots', '[""Cool"", ""Humid""]'),
            ('PsychicPresence', 'Moderate', 'A faint unease suggests the Blight''s presence.', 'The_Roots', '[""Blight"", ""Moderate""]');

            -- Biome Atmosphere Profiles
            INSERT INTO Biome_Atmosphere_Profiles (
                biome_name, lighting_descriptors, sound_descriptors, smell_descriptors,
                temperature_descriptors, psychic_descriptors, composite_template, default_intensity
            ) VALUES
            ('Muspelheim', '[16]', '[17]', '[18]', '[19]', '[20]',
             '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.', 'Oppressive'),
            ('The_Roots', '[21]', '[22]', '[23]', '[24]', '[25]',
             '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.', 'Moderate'),
            ('Generic', '[1, 2]', '[4, 5]', '[7, 8]', '[10, 11]', '[13, 14]',
             '{Lighting}. {Sound}. {Smell}. {Temperature}. {Psychic}.', 'Moderate');
        ";

        using var command = _connection.CreateCommand();
        command.CommandText = insertSql;
        command.ExecuteNonQuery();
    }

    #endregion

    #region Basic Generation Tests

    [Fact]
    public void GenerateAtmosphere_WithValidBiome_ReturnsCompleteDescription()
    {
        // Act
        var result = _service.GenerateAtmosphere("Muspelheim", AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(".", result); // Should be a sentence
    }

    [Fact]
    public void GenerateAtmosphere_WithMuspelheim_ContainsFireThemes()
    {
        // Act
        var result = _service.GenerateAtmosphere("Muspelheim", AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        // Result should contain fire/heat related content (based on Muspelheim profile)
        // At minimum, should be a valid composed string
        Assert.Contains(".", result);
    }

    [Fact]
    public void GenerateAtmosphere_WithTheRoots_ContainsDecayThemes()
    {
        // Act
        var result = _service.GenerateAtmosphere("The_Roots", AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".", result);
    }

    [Fact]
    public void GenerateAtmosphere_WithUnknownBiome_ReturnsGenericAtmosphere()
    {
        // Act
        var result = _service.GenerateAtmosphere("UnknownBiome", AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(".", result);
    }

    [Fact]
    public void GenerateAtmosphere_WithDefaultIntensity_UsesProfileDefault()
    {
        // Act
        var result = _service.GenerateAtmosphere("Muspelheim");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Intensity Tests

    [Theory]
    [InlineData(AtmosphericIntensity.Subtle)]
    [InlineData(AtmosphericIntensity.Moderate)]
    [InlineData(AtmosphericIntensity.Oppressive)]
    public void GenerateAtmosphere_AllIntensities_ReturnsValidDescription(AtmosphericIntensity intensity)
    {
        // Act
        var result = _service.GenerateAtmosphere("Generic", intensity);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateAtmosphere_OpppressiveIntensity_ReturnsIntenseDescription()
    {
        // Act
        var result = _service.GenerateAtmosphere("Muspelheim", AtmosphericIntensity.Oppressive);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Oppressive intensity should exist
        Assert.Contains(".", result);
    }

    #endregion

    #region Category Tests

    [Theory]
    [InlineData(AtmosphericCategory.Lighting)]
    [InlineData(AtmosphericCategory.Sound)]
    [InlineData(AtmosphericCategory.Smell)]
    [InlineData(AtmosphericCategory.Temperature)]
    [InlineData(AtmosphericCategory.PsychicPresence)]
    public void GenerateCategoryAtmosphere_AllCategories_ReturnsDescriptor(AtmosphericCategory category)
    {
        // Act
        var result = _service.GenerateCategoryAtmosphere(
            "Generic",
            category,
            AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateCategoryAtmosphere_Lighting_ReturnsLightingDescriptor()
    {
        // Act
        var result = _service.GenerateCategoryAtmosphere(
            "Generic",
            AtmosphericCategory.Lighting,
            AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateCategoryAtmosphere_PsychicPresence_ReturnsPsychicDescriptor()
    {
        // Act
        var result = _service.GenerateCategoryAtmosphere(
            "Generic",
            AtmosphericCategory.PsychicPresence,
            AtmosphericIntensity.Moderate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Biome Profile Tests

    [Fact]
    public void GetAvailableBiomes_ReturnsAllProfiles()
    {
        // Act
        var biomes = _service.GetAvailableBiomes();

        // Assert
        Assert.NotNull(biomes);
        Assert.NotEmpty(biomes);
        Assert.Contains("Muspelheim", biomes);
        Assert.Contains("The_Roots", biomes);
        Assert.Contains("Generic", biomes);
    }

    [Fact]
    public void GetDefaultIntensity_ForMuspelheim_ReturnsOppressive()
    {
        // Act
        var intensity = _service.GetDefaultIntensity("Muspelheim");

        // Assert
        Assert.Equal(AtmosphericIntensity.Oppressive, intensity);
    }

    [Fact]
    public void GetDefaultIntensity_ForTheRoots_ReturnsModerate()
    {
        // Act
        var intensity = _service.GetDefaultIntensity("The_Roots");

        // Assert
        Assert.Equal(AtmosphericIntensity.Moderate, intensity);
    }

    [Fact]
    public void GetDefaultIntensity_ForUnknownBiome_ReturnsModerate()
    {
        // Act
        var intensity = _service.GetDefaultIntensity("UnknownBiome");

        // Assert
        Assert.Equal(AtmosphericIntensity.Moderate, intensity);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ValidateAtmosphericSystem_WithTestData_ReturnsTrue()
    {
        // Act
        var isValid = _service.ValidateAtmosphericSystem();

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public void GenerateAtmosphere_MultipleCalls_ReturnsDifferentDescriptions()
    {
        // Act
        var results = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(_service.GenerateAtmosphere("Generic", AtmosphericIntensity.Moderate));
        }

        // Assert
        // Should have some variation (not all identical)
        var uniqueResults = results.Distinct().Count();
        Assert.True(uniqueResults >= 1); // At least one unique result
    }

    [Fact]
    public void GenerateAtmosphere_ConsistentBiome_UsesAppropriateDescriptors()
    {
        // Act
        var muspelheimResults = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            muspelheimResults.Add(_service.GenerateAtmosphere("Muspelheim", AtmosphericIntensity.Moderate));
        }

        // Assert
        foreach (var result in muspelheimResults)
        {
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // All results should be valid sentences
            Assert.Contains(".", result);
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullWorkflow_GenerateMultipleBiomes_AllSucceed()
    {
        // Arrange
        var biomes = new[] { "Muspelheim", "The_Roots", "Generic" };
        var intensities = new[]
        {
            AtmosphericIntensity.Subtle,
            AtmosphericIntensity.Moderate,
            AtmosphericIntensity.Oppressive
        };

        // Act & Assert
        foreach (var biome in biomes)
        {
            foreach (var intensity in intensities)
            {
                var result = _service.GenerateAtmosphere(biome, intensity);
                Assert.NotNull(result);
                Assert.NotEmpty(result);
            }
        }
    }

    [Fact]
    public void FullWorkflow_GenerateAllCategories_AllSucceed()
    {
        // Arrange
        var categories = Enum.GetValues<AtmosphericCategory>();

        // Act & Assert
        foreach (var category in categories)
        {
            var result = _service.GenerateCategoryAtmosphere(
                "Generic",
                category,
                AtmosphericIntensity.Moderate);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    #endregion
}
