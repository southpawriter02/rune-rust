// ------------------------------------------------------------------------------
// <copyright file="MusicThemesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for music-themes.schema.json validation.
// Verifies schema structure, theme category enum, transition type enum,
// intensity layer validation, stinger validation, loop points, and
// backward compatibility with the existing music-themes.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the music-themes.schema.json JSON Schema.
/// Tests ensure the schema correctly validates music theme configuration files,
/// enforces theme enums, transition types, volume ranges, and intensity layer constraints.
/// </summary>
/// <remarks>
/// <para>
/// The music themes schema validates configurations including:
/// <list type="bullet">
/// <item><description>Theme categories (8 enum values)</description></item>
/// <item><description>Transition types (3 enum values)</description></item>
/// <item><description>Tracks array (minItems: 1)</description></item>
/// <item><description>Volume range (0-1)</description></item>
/// <item><description>Intensity layer ranges (0-1)</description></item>
/// <item><description>Stinger name pattern (kebab-case)</description></item>
/// <item><description>Loop points (samples or seconds)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class MusicThemesSchemaTests
{
    /// <summary>
    /// Path to the music themes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/music-themes.schema.json";

    /// <summary>
    /// Path to the actual music-themes.json configuration file.
    /// </summary>
    private const string MusicThemesJsonPath = "../../../../../config/music-themes.json";

    /// <summary>
    /// Loaded JSON Schema for music theme definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the music themes schema.
    /// </summary>
    /// <remarks>
    /// Loads the schema from the file system before each test execution.
    /// The schema is validated during loading to ensure it is valid JSON Schema Draft-07.
    /// </remarks>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    /// <summary>
    /// Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all required definitions are present (5 total).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 5 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Music Theme Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (5 total)
        _schema.Definitions.Should().ContainKey("ThemeDefinition", "should define ThemeDefinition");
        _schema.Definitions.Should().ContainKey("IntensityLayer", "should define IntensityLayer");
        _schema.Definitions.Should().ContainKey("LoopPoints", "should define LoopPoints");
        _schema.Definitions.Should().ContainKey("Stinger", "should define Stinger");
        _schema.Definitions.Should().ContainKey("Transitions", "should define Transitions");
    }

    /// <summary>
    /// Verifies the existing music-themes.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all 5 themes, 4 stingers, and transitions are valid.
    /// </summary>
    /// <remarks>
    /// The music-themes.json contains 5 themes (MainMenu, Exploration, Combat, BossCombat, SafeArea),
    /// 4 stingers (victory, defeat, level-up, quest-complete), and transition settings.
    /// </remarks>
    [Test]
    public async Task MusicThemesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual music-themes.json file
        var jsonContent = await File.ReadAllTextAsync(MusicThemesJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing music-themes.json should be valid
        errors.Should().BeEmpty(
            "existing music-themes.json with 5 themes, 4 stingers, and transitions should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that theme must be one of the 8 valid enum values.
    /// Invalid theme categories should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid themes: MainMenu, Exploration, Combat, BossCombat, SafeArea, Puzzle, Cinematic, Credits.
    /// Theme values are case-sensitive.
    /// </remarks>
    [Test]
    public void Theme_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid theme "Battle" (not in enum)
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Battle",
                    "tracks": ["audio/music/battle.ogg"]
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid theme enum value
        errors.Should().NotBeEmpty("theme 'Battle' should fail validation (valid: MainMenu, Exploration, Combat, etc.)");
    }

    /// <summary>
    /// Verifies that transitionType must be one of the 3 valid enum values.
    /// Invalid transition types should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid transition types: Crossfade, Immediate, FadeOutFadeIn.
    /// </remarks>
    [Test]
    public void TransitionType_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid transition type "Blend"
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"]
                }
            ],
            "transitions": {
                "transitionType": "Blend"
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid transition type enum value
        errors.Should().NotBeEmpty("transitionType 'Blend' should fail validation (valid: Crossfade, Immediate, FadeOutFadeIn)");
    }

    /// <summary>
    /// Verifies that tracks array must have at least 1 item.
    /// Empty tracks array should fail validation.
    /// </summary>
    /// <remarks>
    /// Every theme must have at least one music track.
    /// </remarks>
    [Test]
    public void Tracks_EmptyArray_FailsValidation()
    {
        // Arrange: JSON with empty tracks array
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": []
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty tracks array
        errors.Should().NotBeEmpty("empty tracks array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that volume must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Volume is normalized (0 = silent, 1 = full volume).
    /// </remarks>
    [Test]
    public void Volume_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with volume greater than 1 (150%)
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"],
                    "volume": 1.5
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to volume out of range
        errors.Should().NotBeEmpty("volume 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that themes array must have at least 1 item.
    /// Empty themes array should fail validation.
    /// </summary>
    /// <remarks>
    /// A music configuration must define at least one theme.
    /// </remarks>
    [Test]
    public void Themes_EmptyArray_FailsValidation()
    {
        // Arrange: JSON with empty themes array
        var invalidJson = """
        {
            "themes": [],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty themes array
        errors.Should().NotBeEmpty("empty themes array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that stinger name must follow the kebab-case pattern.
    /// Invalid name patterns should fail validation.
    /// </summary>
    /// <remarks>
    /// Stinger names use pattern: ^[a-z][a-z0-9-]*$
    /// </remarks>
    [Test]
    public void StingerName_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid stinger name (uppercase)
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"]
                }
            ],
            "stingers": [
                {
                    "name": "Victory",
                    "track": "audio/music/victory.ogg"
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid stinger name pattern
        errors.Should().NotBeEmpty("stinger name 'Victory' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that intensity layer minIntensity must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Intensity values represent game state (0 = calm, 1 = maximum intensity).
    /// </remarks>
    [Test]
    public void IntensityLayerMinIntensity_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with minIntensity greater than 1
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"],
                    "intensityLayers": [
                        {
                            "track": "audio/music/layer.ogg",
                            "minIntensity": 1.5,
                            "maxIntensity": 2.0
                        }
                    ]
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to minIntensity out of range
        errors.Should().NotBeEmpty("minIntensity 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that duckAmount must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// duckAmount: 0 = music silent during stinger, 1 = no ducking, 0.3 = default.
    /// </remarks>
    [Test]
    public void DuckAmount_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with duckAmount greater than 1
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"]
                }
            ],
            "stingers": [
                {
                    "name": "victory",
                    "track": "audio/music/victory.ogg",
                    "duckAmount": 1.5
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to duckAmount out of range
        errors.Should().NotBeEmpty("duckAmount 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that crossfade durations must be at least 0.
    /// Negative values should fail validation.
    /// </summary>
    /// <remarks>
    /// Crossfade durations are in seconds. 0 = instant cut.
    /// </remarks>
    [Test]
    public void CrossfadeDuration_Negative_FailsValidation()
    {
        // Arrange: JSON with negative crossfade duration
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"]
                }
            ],
            "transitions": {
                "defaultCrossfade": -1.0
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative crossfade
        errors.Should().NotBeEmpty("defaultCrossfade -1.0 should fail validation (minimum is 0)");
    }

    /// <summary>
    /// Verifies that loopStartSamples must be at least 0.
    /// Negative values should fail validation.
    /// </summary>
    /// <remarks>
    /// Sample positions are non-negative integers.
    /// </remarks>
    [Test]
    public void LoopStartSamples_Negative_FailsValidation()
    {
        // Arrange: JSON with negative loopStartSamples
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"],
                    "loopPoints": {
                        "loopStartSamples": -1000
                    }
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative sample position
        errors.Should().NotBeEmpty("loopStartSamples -1000 should fail validation (minimum is 0)");
    }

    /// <summary>
    /// Verifies that volumeMultiplier must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// volumeMultiplier is the maximum volume for an intensity layer.
    /// </remarks>
    [Test]
    public void VolumeMultiplier_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with volumeMultiplier greater than 1
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "Combat",
                    "tracks": ["audio/music/combat.ogg"],
                    "intensityLayers": [
                        {
                            "track": "audio/music/layer.ogg",
                            "minIntensity": 0.0,
                            "maxIntensity": 1.0,
                            "volumeMultiplier": 2.0
                        }
                    ]
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to volumeMultiplier out of range
        errors.Should().NotBeEmpty("volumeMultiplier 2.0 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that theme category is case-sensitive.
    /// Lowercase theme values should fail validation.
    /// </summary>
    /// <remarks>
    /// Theme enum values are PascalCase: Combat, not combat.
    /// </remarks>
    [Test]
    public void Theme_CaseSensitive_FailsValidation()
    {
        // Arrange: JSON with lowercase theme (case-sensitive enum)
        var invalidJson = """
        {
            "themes": [
                {
                    "theme": "combat",
                    "tracks": ["audio/music/combat.ogg"]
                }
            ],
            "transitions": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to case-sensitive theme enum
        errors.Should().NotBeEmpty("theme 'combat' should fail validation (case-sensitive, must be 'Combat')");
    }
}
