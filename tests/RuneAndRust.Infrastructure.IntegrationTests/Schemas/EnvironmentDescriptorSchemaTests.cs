// ------------------------------------------------------------------------------
// <copyright file="EnvironmentDescriptorSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for environment-descriptors.schema.json validation.
// Verifies schema structure, environment category validation, sensory/weather/ambient
// pool types, temporal filtering (timeOfDay, season), and location tag validation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the environment-descriptors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates environment descriptor configuration files,
/// enforces environment-specific category restrictions, sensory/weather/ambient pool type
/// enumerations, and environment-specific descriptor properties (timeOfDay, season).
/// </summary>
/// <remarks>
/// <para>
/// The environment descriptors schema provides:
/// <list type="bullet">
/// <item><description>EnvironmentCategory enum restricting category to environmental, weather, ambient, objects</description></item>
/// <item><description>SensoryCategory enum for sensory pools (4 values: lighting, sounds, smells, temperature)</description></item>
/// <item><description>WeatherPoolTypes enum for weather pools (10 values: rain/storm/snow/fog/wind indoor/outdoor)</description></item>
/// <item><description>AmbientPoolTypes enum for ambient pools (15 values: sound/visual/creature/environmental by biome)</description></item>
/// <item><description>EnvironmentDescriptor with timeOfDay and season temporal filtering arrays</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class EnvironmentDescriptorSchemaTests
{
    /// <summary>
    /// Path to the environment descriptors schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/environment-descriptors.schema.json";

    /// <summary>
    /// Path to the environmental.json descriptor configuration file.
    /// </summary>
    private const string EnvironmentalJsonPath = "../../../../../config/descriptors/environmental.json";

    /// <summary>
    /// Path to the weather.json descriptor configuration file.
    /// </summary>
    private const string WeatherJsonPath = "../../../../../config/descriptors/weather.json";

    /// <summary>
    /// Path to the ambient-events.json descriptor configuration file.
    /// </summary>
    private const string AmbientEventsJsonPath = "../../../../../config/descriptors/ambient-events.json";

    /// <summary>
    /// Loaded JSON Schema for environment descriptor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the environment descriptors schema.
    /// </summary>
    /// <remarks>
    /// Loads the schema from the file system before each test execution.
    /// Uses FromFileAsync with full path to properly resolve $ref references.
    /// The schema is validated during loading to ensure it is valid JSON Schema Draft-07.
    /// </remarks>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system with full path for reference resolution
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region ESC-001: Schema Loading

    /// <summary>
    /// ESC-001: Verifies the environment-descriptors.schema.json loads successfully
    /// and is a valid JSON Schema with expected structure and definitions.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All required definitions are present (10 total)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void EnvironmentDescriptorSchema_LoadsSuccessfully_ReturnsValidSchema()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Environment Descriptor Configuration Schema", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (10 environment-specific definitions)
        _schema.Definitions.Should().ContainKey("EnvironmentCategory", "should define EnvironmentCategory enum");
        _schema.Definitions.Should().ContainKey("SensoryCategory", "should define SensoryCategory enum");
        _schema.Definitions.Should().ContainKey("WeatherPoolTypes", "should define WeatherPoolTypes enum");
        _schema.Definitions.Should().ContainKey("AmbientPoolTypes", "should define AmbientPoolTypes enum");
        _schema.Definitions.Should().ContainKey("EnvironmentDescriptorPool", "should define EnvironmentDescriptorPool");
        _schema.Definitions.Should().ContainKey("EnvironmentDescriptor", "should define EnvironmentDescriptor");
        _schema.Definitions.Should().ContainKey("TimeOfDayValue", "should define TimeOfDayValue enum");
        _schema.Definitions.Should().ContainKey("SeasonValue", "should define SeasonValue enum");
        _schema.Definitions.Should().ContainKey("VariablePattern", "should define VariablePattern documentation");
    }

    #endregion

    #region ESC-002: Category Validation

    /// <summary>
    /// ESC-002: Verifies that non-environment categories (e.g., 'combat-hits') fail validation
    /// against the environment-descriptors schema.
    /// </summary>
    /// <remarks>
    /// The EnvironmentCategory enum only allows: environmental, weather, ambient, objects.
    /// Other category values should be rejected.
    /// </remarks>
    [Test]
    public void EnvironmentDescriptorSchema_InvalidCategory_FailsValidation()
    {
        // Arrange: JSON with non-environment category
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit_sword": {
                    "id": "hit_sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": [
                        { "id": "test_hit", "text": "test hit" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to non-environment category
        errors.Should().NotBeEmpty(
            "category 'combat-hits' should fail validation against environment schema (must be environmental, weather, ambient, or objects)");
    }

    /// <summary>
    /// ESC-002: Verifies that valid environment categories pass validation successfully.
    /// Tests all four valid values: environmental, weather, ambient, objects.
    /// </summary>
    /// <remarks>
    /// Each test case validates a different allowed category value.
    /// </remarks>
    [Test]
    [TestCase("environmental")]
    [TestCase("weather")]
    [TestCase("ambient")]
    [TestCase("objects")]
    public void EnvironmentDescriptorSchema_ValidCategory_PassesValidation(string category)
    {
        // Arrange: JSON with valid environment category
        var validJson = $$"""
        {
            "category": "{{category}}",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        { "id": "test_desc", "text": "test text" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid environment category
        errors.Should().BeEmpty($"category '{category}' should be a valid environment category");
    }

    #endregion

    #region ESC-003: Environmental Validation

    /// <summary>
    /// ESC-003: Verifies the existing environmental.json configuration validates
    /// against the environment-descriptors schema without errors.
    /// </summary>
    /// <remarks>
    /// The environmental.json contains 4 sensory pools (lighting, sounds, smells, temperature)
    /// with approximately 23 descriptors. All use location tags and theme filters.
    /// </remarks>
    [Test]
    public async Task EnvironmentalJson_ValidatesAgainstSchema_Succeeds()
    {
        // Arrange: Load the actual environmental.json file
        var jsonContent = await File.ReadAllTextAsync(EnvironmentalJsonPath);

        // Act: Validate the JSON content against the environment schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing environmental.json should be valid
        errors.Should().BeEmpty(
            "existing environmental.json with 4 sensory pools should validate against environment schema without errors");
    }

    #endregion

    #region ESC-004: Weather Validation

    /// <summary>
    /// ESC-004: Verifies the existing weather.json configuration validates
    /// against the environment-descriptors schema without errors.
    /// </summary>
    /// <remarks>
    /// The weather.json contains 10 weather pools (rain/storm/snow/fog/wind indoor/outdoor)
    /// with approximately 40 descriptors.
    /// </remarks>
    [Test]
    public async Task WeatherJson_ValidatesAgainstSchema_Succeeds()
    {
        // Arrange: Load the actual weather.json file
        var jsonContent = await File.ReadAllTextAsync(WeatherJsonPath);

        // Act: Validate the JSON content against the environment schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing weather.json should be valid
        errors.Should().BeEmpty(
            "existing weather.json with 10 weather pools should validate against environment schema without errors");
    }

    #endregion

    #region ESC-005: Ambient Events Validation

    /// <summary>
    /// ESC-005: Verifies the existing ambient-events.json configuration validates
    /// against the environment-descriptors schema without errors.
    /// </summary>
    /// <remarks>
    /// The ambient-events.json contains 15 ambient pools (sound/visual/creature/environmental
    /// by biome) with approximately 81 descriptors.
    /// </remarks>
    [Test]
    public async Task AmbientEventsJson_ValidatesAgainstSchema_Succeeds()
    {
        // Arrange: Load the actual ambient-events.json file
        var jsonContent = await File.ReadAllTextAsync(AmbientEventsJsonPath);

        // Act: Validate the JSON content against the environment schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing ambient-events.json should be valid
        errors.Should().BeEmpty(
            "existing ambient-events.json with 15 ambient pools should validate against environment schema without errors");
    }

    #endregion

    #region ESC-006: Tags and Themes Validation

    /// <summary>
    /// ESC-006: Verifies that location tags and theme tags are validated as string arrays
    /// and descriptors with these arrays validate successfully.
    /// </summary>
    /// <remarks>
    /// Location tags include: indoor, outdoor, cave, dungeon, forest, danger, safe, etc.
    /// Theme tags include: dark_fantasy, high_fantasy, horror.
    /// Both should be arrays of strings with unique items.
    /// </remarks>
    [Test]
    public void EnvironmentDescriptor_TagsAndThemes_ValidatesAsStringArrays()
    {
        // Arrange: JSON with tags and themes arrays
        var validJson = """
        {
            "category": "environmental",
            "pools": {
                "lighting": {
                    "id": "lighting",
                    "name": "Lighting Descriptions",
                    "descriptors": [
                        {
                            "id": "dim_torch",
                            "text": "dimly lit by flickering torches",
                            "weight": 20,
                            "tags": ["indoor", "dungeon", "danger"],
                            "themes": ["dark_fantasy", "horror"]
                        },
                        {
                            "id": "bright_sunlight",
                            "text": "bathed in bright sunlight",
                            "weight": 25,
                            "tags": ["outdoor", "safe"],
                            "themes": ["high_fantasy"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for descriptors with tags and themes
        errors.Should().BeEmpty(
            "descriptors with tags and themes as string arrays should validate successfully");
    }

    #endregion

    #region Temporal Filtering Validation

    /// <summary>
    /// Verifies that timeOfDay and season temporal filtering properties
    /// validate successfully when provided on descriptors.
    /// </summary>
    /// <remarks>
    /// timeOfDay accepts: dawn, day, dusk, night
    /// season accepts: spring, summer, autumn, winter
    /// These enable time-based and seasonal atmosphere variation.
    /// </remarks>
    [Test]
    public void EnvironmentDescriptor_TemporalFiltering_PassesValidation()
    {
        // Arrange: JSON with temporal filtering properties
        var validJson = """
        {
            "category": "environmental",
            "pools": {
                "lighting": {
                    "id": "lighting",
                    "name": "Lighting Descriptions",
                    "descriptors": [
                        {
                            "id": "morning_mist",
                            "text": "Morning mist clings to the ground, slowly burning off",
                            "weight": 20,
                            "tags": ["outdoor", "forest"],
                            "timeOfDay": ["dawn"]
                        },
                        {
                            "id": "midday_sun",
                            "text": "The sun beats down from directly overhead",
                            "weight": 25,
                            "tags": ["outdoor"],
                            "timeOfDay": ["day"],
                            "season": ["summer"]
                        },
                        {
                            "id": "winter_twilight",
                            "text": "Long shadows stretch across the snow as the sun sets",
                            "weight": 15,
                            "tags": ["outdoor", "frozen"],
                            "timeOfDay": ["dusk"],
                            "season": ["winter"]
                        },
                        {
                            "id": "starry_night",
                            "text": "Stars glitter in the clear night sky",
                            "weight": 20,
                            "tags": ["outdoor"],
                            "timeOfDay": ["night"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for descriptors with temporal filtering
        errors.Should().BeEmpty(
            "descriptors with timeOfDay and season temporal filtering should validate successfully");
    }

    /// <summary>
    /// Verifies that invalid timeOfDay values fail validation.
    /// Only dawn, day, dusk, night are valid values.
    /// </summary>
    [Test]
    public void EnvironmentDescriptor_InvalidTimeOfDay_FailsValidation()
    {
        // Arrange: JSON with invalid timeOfDay value
        var invalidJson = """
        {
            "category": "environmental",
            "pools": {
                "lighting": {
                    "id": "lighting",
                    "name": "Lighting Descriptions",
                    "descriptors": [
                        {
                            "id": "morning_light",
                            "text": "Morning light filters through",
                            "timeOfDay": ["morning"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid timeOfDay value
        errors.Should().NotBeEmpty(
            "timeOfDay 'morning' should fail validation (valid values: dawn, day, dusk, night)");
    }

    /// <summary>
    /// Verifies that invalid season values fail validation.
    /// Only spring, summer, autumn, winter are valid values.
    /// </summary>
    [Test]
    public void EnvironmentDescriptor_InvalidSeason_FailsValidation()
    {
        // Arrange: JSON with invalid season value
        var invalidJson = """
        {
            "category": "environmental",
            "pools": {
                "lighting": {
                    "id": "lighting",
                    "name": "Lighting Descriptions",
                    "descriptors": [
                        {
                            "id": "rainy_season",
                            "text": "Rain pours down",
                            "season": ["monsoon"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid season value
        errors.Should().NotBeEmpty(
            "season 'monsoon' should fail validation (valid values: spring, summer, autumn, winter)");
    }

    #endregion

    #region Weather Pool Types Validation

    /// <summary>
    /// Verifies that weather pools with valid indoor/outdoor naming pattern
    /// validate successfully.
    /// </summary>
    [Test]
    public void WeatherDescriptor_IndoorOutdoorPools_PassesValidation()
    {
        // Arrange: JSON with weather pools following indoor/outdoor pattern
        var validJson = """
        {
            "category": "weather",
            "pools": {
                "weather_rain_indoor": {
                    "id": "weather_rain_indoor",
                    "name": "Rain (Indoor)",
                    "descriptors": [
                        {
                            "id": "rain_roof",
                            "text": "Rain patters steadily on the roof above",
                            "weight": 25
                        }
                    ]
                },
                "weather_rain_outdoor": {
                    "id": "weather_rain_outdoor",
                    "name": "Rain (Outdoor)",
                    "descriptors": [
                        {
                            "id": "rain_exposed",
                            "text": "Steady rain patters against you",
                            "weight": 25
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for weather pools with indoor/outdoor pattern
        errors.Should().BeEmpty(
            "weather pools with valid indoor/outdoor naming should validate successfully");
    }

    #endregion

    #region Ambient Pool Types Validation

    /// <summary>
    /// Verifies that ambient pools with biome-specific naming pattern
    /// validate successfully.
    /// </summary>
    [Test]
    public void AmbientDescriptor_BiomeSpecificPools_PassesValidation()
    {
        // Arrange: JSON with ambient pools following biome-specific pattern
        var validJson = """
        {
            "category": "ambient",
            "pools": {
                "sound_cave": {
                    "id": "sound_cave",
                    "name": "Sound Events (Cave)",
                    "descriptors": [
                        {
                            "id": "water_drip",
                            "text": "A single drop of water falls, its echo multiplying into a chorus",
                            "weight": 25,
                            "tags": ["cave", "underground"]
                        }
                    ]
                },
                "creature_forest": {
                    "id": "creature_forest",
                    "name": "Creature Events (Forest)",
                    "descriptors": [
                        {
                            "id": "deer_flee",
                            "text": "A deer freezes at your approach, then bounds away",
                            "weight": 20,
                            "tags": ["forest", "outdoor"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for ambient pools with biome-specific naming
        errors.Should().BeEmpty(
            "ambient pools with valid biome-specific naming should validate successfully");
    }

    #endregion

    #region Unknown Properties Validation

    /// <summary>
    /// Verifies that unknown properties on environment descriptors fail validation.
    /// Schema enforces additionalProperties: false.
    /// </summary>
    [Test]
    public void EnvironmentDescriptor_UnknownProperty_FailsValidation()
    {
        // Arrange: JSON with unknown property on descriptor
        var invalidJson = """
        {
            "category": "environmental",
            "pools": {
                "lighting": {
                    "id": "lighting",
                    "name": "Lighting Descriptions",
                    "descriptors": [
                        {
                            "id": "test_light",
                            "text": "test lighting text",
                            "unknownProperty": "invalid"
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown property
        errors.Should().NotBeEmpty(
            "unknown property 'unknownProperty' on descriptor should fail validation");
    }

    #endregion
}
