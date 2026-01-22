// ------------------------------------------------------------------------------
// <copyright file="SoundEffectsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for sound-effects.schema.json validation.
// Verifies schema structure, category validation, sound effect configuration,
// volume/pitch ranges, global settings, and backward compatibility with
// the existing sound-effects.json configuration file.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the sound-effects.schema.json JSON Schema.
/// Tests ensure the schema correctly validates sound effect configuration files,
/// enforces volume/pitch ranges, priority constraints, and file array requirements.
/// </summary>
/// <remarks>
/// <para>
/// The sound effects schema validates configurations including:
/// <list type="bullet">
/// <item><description>Categories with additionalProperties for flexible naming</description></item>
/// <item><description>Sound effects with files array (minItems: 1)</description></item>
/// <item><description>Volume range (0-1)</description></item>
/// <item><description>Pitch range (0.5-2.0)</description></item>
/// <item><description>Priority range (0-10)</description></item>
/// <item><description>Global settings (maxSimultaneous, defaultVolume, masterMute, fallbackEnabled)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class SoundEffectsSchemaTests
{
    /// <summary>
    /// Path to the sound effects schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/sound-effects.schema.json";

    /// <summary>
    /// Path to the actual sound-effects.json configuration file.
    /// </summary>
    private const string SoundEffectsJsonPath = "../../../../../config/sound-effects.json";

    /// <summary>
    /// Loaded JSON Schema for sound effect definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the sound effects schema.
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
        _schema.Title.Should().Be("Sound Effects Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (5 total)
        _schema.Definitions.Should().ContainKey("SoundCategory", "should define SoundCategory");
        _schema.Definitions.Should().ContainKey("SoundEffect", "should define SoundEffect");
        _schema.Definitions.Should().ContainKey("VolumeRange", "should define VolumeRange");
        _schema.Definitions.Should().ContainKey("PitchRange", "should define PitchRange");
        _schema.Definitions.Should().ContainKey("GlobalSettings", "should define GlobalSettings");
    }

    /// <summary>
    /// Verifies the existing sound-effects.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all 6 categories with 19 effects are valid.
    /// </summary>
    /// <remarks>
    /// The sound-effects.json contains 6 categories: combat (6 effects), ability (3 effects),
    /// ui (4 effects), items (3 effects), puzzle (3 effects), movement (3 effects).
    /// </remarks>
    [Test]
    public async Task SoundEffectsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual sound-effects.json file
        var jsonContent = await File.ReadAllTextAsync(SoundEffectsJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing sound-effects.json should be valid
        errors.Should().BeEmpty(
            "existing sound-effects.json with 6 categories and 19 effects should validate against schema without errors");
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
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "volume": 1.5
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to volume out of range
        errors.Should().NotBeEmpty("volume 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that pitch must be between 0.5 and 2.0.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Pitch: 0.5 = half speed, 1.0 = normal, 2.0 = double speed.
    /// </remarks>
    [Test]
    public void Pitch_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with pitch less than 0.5 (too slow)
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "pitch": 0.2
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to pitch out of range
        errors.Should().NotBeEmpty("pitch 0.2 should fail validation (must be 0.5-2.0)");
    }

    /// <summary>
    /// Verifies that files array must have at least 1 item.
    /// Empty files array should fail validation.
    /// </summary>
    /// <remarks>
    /// Every sound effect must have at least one audio file.
    /// </remarks>
    [Test]
    public void Files_EmptyArray_FailsValidation()
    {
        // Arrange: JSON with empty files array
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": []
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty files array
        errors.Should().NotBeEmpty("empty files array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that priority must be between 0 and 10.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Priority: 0 = lowest, 10 = highest. Higher priority sounds preempt lower.
    /// </remarks>
    [Test]
    public void Priority_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with priority greater than 10
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "priority": 15
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to priority out of range
        errors.Should().NotBeEmpty("priority 15 should fail validation (must be 0-10)");
    }

    /// <summary>
    /// Verifies that categories object must have at least 1 property.
    /// Empty categories should fail validation.
    /// </summary>
    /// <remarks>
    /// A sound effects configuration must define at least one category.
    /// </remarks>
    [Test]
    public void Categories_Empty_FailsValidation()
    {
        // Arrange: JSON with empty categories object
        var invalidJson = """
        {
            "categories": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty categories
        errors.Should().NotBeEmpty("empty categories object should fail validation (minProperties: 1)");
    }

    /// <summary>
    /// Verifies that volumeVariance.min must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Volume variance is a multiplier range for dynamic volume.
    /// </remarks>
    [Test]
    public void VolumeVarianceMin_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with volumeVariance.min greater than 1
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "volumeVariance": {
                                "min": 1.5,
                                "max": 2.0
                            }
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to volumeVariance.min out of range
        errors.Should().NotBeEmpty("volumeVariance.min 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that pitchVariance.max must be between 0.5 and 2.0.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Pitch variance is a multiplier range for dynamic pitch.
    /// </remarks>
    [Test]
    public void PitchVarianceMax_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with pitchVariance.max greater than 2.0
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "pitchVariance": {
                                "min": 0.9,
                                "max": 3.0
                            }
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to pitchVariance.max out of range
        errors.Should().NotBeEmpty("pitchVariance.max 3.0 should fail validation (must be 0.5-2.0)");
    }

    /// <summary>
    /// Verifies that maxSimultaneous must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// At least one sound must be able to play at a time.
    /// </remarks>
    [Test]
    public void MaxSimultaneous_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with maxSimultaneous of 0
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"]
                        }
                    }
                }
            },
            "settings": {
                "maxSimultaneous": 0
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to maxSimultaneous less than 1
        errors.Should().NotBeEmpty("maxSimultaneous 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that defaultVolume must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Default volume is normalized like individual effect volume.
    /// </remarks>
    [Test]
    public void DefaultVolume_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with defaultVolume greater than 1
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"]
                        }
                    }
                }
            },
            "settings": {
                "defaultVolume": 1.5
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to defaultVolume out of range
        errors.Should().NotBeEmpty("defaultVolume 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that fallbackEffectId must follow the kebab-case pattern.
    /// Invalid ID patterns should fail validation.
    /// </summary>
    /// <remarks>
    /// Fallback effect IDs reference other effects in the same configuration.
    /// </remarks>
    [Test]
    public void FallbackEffectId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid fallbackEffectId (uppercase)
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "fallbackEffectId": "Attack-Hit"
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid fallbackEffectId pattern
        errors.Should().NotBeEmpty("fallbackEffectId 'Attack-Hit' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that cooldown must be at least 0.
    /// Negative values should fail validation.
    /// </summary>
    /// <remarks>
    /// Cooldown is minimum seconds between plays. 0 = no cooldown.
    /// </remarks>
    [Test]
    public void Cooldown_Negative_FailsValidation()
    {
        // Arrange: JSON with negative cooldown
        var invalidJson = """
        {
            "categories": {
                "test": {
                    "effects": {
                        "test-sound": {
                            "files": ["audio/test.ogg"],
                            "cooldown": -1.0
                        }
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative cooldown
        errors.Should().NotBeEmpty("cooldown -1.0 should fail validation (minimum is 0)");
    }
}
