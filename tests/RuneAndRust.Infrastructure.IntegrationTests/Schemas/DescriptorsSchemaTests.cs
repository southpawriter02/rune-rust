// ------------------------------------------------------------------------------
// <copyright file="DescriptorsSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for descriptors.schema.json validation.
// Verifies schema structure, pool validation, descriptor configuration,
// weight constraints, percentage ranges, ID patterns, and backward compatibility
// with existing descriptor configuration files (combat-hits.json, environmental.json).
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the descriptors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates descriptor configuration files,
/// enforces ID patterns, weight constraints, percentage ranges, and tag arrays.
/// </summary>
/// <remarks>
/// <para>
/// The descriptors schema validates configurations including:
/// <list type="bullet">
/// <item><description>Category as lowercase kebab-case (e.g., combat-hits, environmental)</description></item>
/// <item><description>Pools with additionalProperties for flexible naming</description></item>
/// <item><description>Pool structure with id, name, and descriptors array (minItems: 1)</description></item>
/// <item><description>Descriptor structure with id, text, optional weight/tags/themes</description></item>
/// <item><description>Weight as positive integer (minimum: 1)</description></item>
/// <item><description>Damage/health percentage ranges (0-1)</description></item>
/// <item><description>Variable substitution documentation via variables array</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class DescriptorsSchemaTests
{
    /// <summary>
    /// Path to the descriptors schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/descriptors.schema.json";

    /// <summary>
    /// Path to the combat-hits.json descriptor configuration file.
    /// </summary>
    private const string CombatHitsJsonPath = "../../../../../config/descriptors/combat-hits.json";

    /// <summary>
    /// Path to the environmental.json descriptor configuration file.
    /// </summary>
    private const string EnvironmentalJsonPath = "../../../../../config/descriptors/environmental.json";

    /// <summary>
    /// Loaded JSON Schema for descriptor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the descriptors schema.
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

    #region DSC-001: Schema Structure Validation

    /// <summary>
    /// DSC-001: Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all required definitions are present (3 total: DescriptorPool, Descriptor, VariablePattern).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 3 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Descriptor Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (3 total)
        _schema.Definitions.Should().ContainKey("DescriptorPool", "should define DescriptorPool");
        _schema.Definitions.Should().ContainKey("Descriptor", "should define Descriptor");
        _schema.Definitions.Should().ContainKey("VariablePattern", "should define VariablePattern");
    }

    #endregion

    #region DSC-002: Pool Structure Validation

    /// <summary>
    /// DSC-002: Verifies that pools require id, name, and non-empty descriptors array.
    /// Empty descriptors array should fail validation (minItems: 1).
    /// </summary>
    /// <remarks>
    /// Every pool must have at least one descriptor. This test ensures the schema
    /// rejects pools with empty descriptor arrays.
    /// </remarks>
    [Test]
    public void Pool_EmptyDescriptorsArray_FailsValidation()
    {
        // Arrange: JSON with empty descriptors array
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit_sword": {
                    "id": "hit_sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": []
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty descriptors array
        errors.Should().NotBeEmpty("empty descriptors array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// DSC-002: Verifies that pools must not have additional unknown properties.
    /// Unknown fields in pool objects should fail validation.
    /// </summary>
    /// <remarks>
    /// The schema uses additionalProperties: false to prevent unknown fields.
    /// </remarks>
    [Test]
    public void Pool_UnknownProperty_FailsValidation()
    {
        // Arrange: JSON with unknown property in pool
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit_sword": {
                    "id": "hit_sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": [
                        { "id": "test", "text": "test" }
                    ],
                    "unknownField": "value"
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown property
        errors.Should().NotBeEmpty("unknown property 'unknownField' in pool should fail validation");
    }

    /// <summary>
    /// DSC-002: Verifies that pools object must have at least one pool.
    /// Empty pools object should fail validation (minProperties: 1).
    /// </summary>
    /// <remarks>
    /// A descriptor configuration must define at least one pool.
    /// </remarks>
    [Test]
    public void Pools_Empty_FailsValidation()
    {
        // Arrange: JSON with empty pools object
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {}
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty pools
        errors.Should().NotBeEmpty("empty pools object should fail validation (minProperties: 1)");
    }

    #endregion

    #region DSC-003: Descriptor Structure Validation

    /// <summary>
    /// DSC-003: Verifies minimal descriptor with only required fields validates successfully.
    /// Only id and text are required; all other properties are optional.
    /// </summary>
    /// <remarks>
    /// This tests the minimum valid descriptor configuration.
    /// </remarks>
    [Test]
    public void Descriptor_MinimalConfiguration_PassesValidation()
    {
        // Arrange: JSON with minimal descriptor
        var validJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text"
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimal valid descriptor
        errors.Should().BeEmpty("minimal descriptor with id and text should validate successfully");
    }

    /// <summary>
    /// DSC-003: Verifies that descriptor missing 'id' field fails validation.
    /// id is a required property on Descriptor.
    /// </summary>
    /// <remarks>
    /// Every descriptor must have a unique identifier.
    /// </remarks>
    [Test]
    public void Descriptor_MissingId_FailsValidation()
    {
        // Arrange: JSON with descriptor missing id
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "text": "test text"
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing id
        errors.Should().NotBeEmpty("descriptor missing 'id' should fail validation");
    }

    /// <summary>
    /// DSC-003: Verifies that descriptor missing 'text' field fails validation.
    /// text is a required property on Descriptor.
    /// </summary>
    /// <remarks>
    /// Every descriptor must have text content.
    /// </remarks>
    [Test]
    public void Descriptor_MissingText_FailsValidation()
    {
        // Arrange: JSON with descriptor missing text
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor"
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing text
        errors.Should().NotBeEmpty("descriptor missing 'text' should fail validation");
    }

    #endregion

    #region DSC-004: Weight Validation

    /// <summary>
    /// DSC-004: Verifies that weight must be at least 1.
    /// Weight of 0 should fail validation.
    /// </summary>
    /// <remarks>
    /// Weight: minimum 1, default 100. Zero weight would prevent selection.
    /// </remarks>
    [Test]
    public void Weight_Zero_FailsValidation()
    {
        // Arrange: JSON with weight of 0
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text",
                            "weight": 0
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to weight below minimum
        errors.Should().NotBeEmpty("weight 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// DSC-004: Verifies that negative weight fails validation.
    /// </summary>
    /// <remarks>
    /// Weight must be a positive integer.
    /// </remarks>
    [Test]
    public void Weight_Negative_FailsValidation()
    {
        // Arrange: JSON with negative weight
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text",
                            "weight": -5
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative weight
        errors.Should().NotBeEmpty("weight -5 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// DSC-004: Verifies that fractional weight fails validation.
    /// Weight must be an integer.
    /// </summary>
    /// <remarks>
    /// Weight type is integer, not number. Fractional values are invalid.
    /// </remarks>
    [Test]
    public void Weight_Fractional_FailsValidation()
    {
        // Arrange: JSON with fractional weight
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text",
                            "weight": 25.5
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to fractional weight
        errors.Should().NotBeEmpty("weight 25.5 should fail validation (must be integer)");
    }

    #endregion

    #region DSC-005: Percentage Range Validation

    /// <summary>
    /// DSC-005: Verifies that minDamagePercent must be within 0-1 range.
    /// Value above 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Damage percentages are normalized (0 = 0%, 1 = 100%).
    /// </remarks>
    [Test]
    public void MinDamagePercent_AboveMaximum_FailsValidation()
    {
        // Arrange: JSON with minDamagePercent above 1
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text",
                            "minDamagePercent": 1.5
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to minDamagePercent out of range
        errors.Should().NotBeEmpty("minDamagePercent 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// DSC-005: Verifies that maxDamagePercent must be within 0-1 range.
    /// Negative value should fail validation.
    /// </summary>
    /// <remarks>
    /// Damage percentages are normalized and cannot be negative.
    /// </remarks>
    [Test]
    public void MaxDamagePercent_BelowMinimum_FailsValidation()
    {
        // Arrange: JSON with maxDamagePercent below 0
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text",
                            "maxDamagePercent": -0.1
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to maxDamagePercent out of range
        errors.Should().NotBeEmpty("maxDamagePercent -0.1 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// DSC-005: Verifies that maxHealthPercent must be within 0-1 range.
    /// Value above 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Health percentages are normalized (0 = 0%, 1 = 100%).
    /// </remarks>
    [Test]
    public void MaxHealthPercent_AboveMaximum_FailsValidation()
    {
        // Arrange: JSON with maxHealthPercent above 1
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "test_descriptor",
                            "text": "test text",
                            "maxHealthPercent": 2.0
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to maxHealthPercent out of range
        errors.Should().NotBeEmpty("maxHealthPercent 2.0 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// DSC-005: Verifies valid percentage values within range pass validation.
    /// </summary>
    /// <remarks>
    /// All four percentage fields (minDamagePercent, maxDamagePercent, minHealthPercent, maxHealthPercent)
    /// should accept values from 0 to 1 inclusive.
    /// </remarks>
    [Test]
    public void PercentageValues_WithinRange_PassValidation()
    {
        // Arrange: JSON with valid percentage values
        var validJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        {
                            "id": "heavy_hit",
                            "text": "cleaves through",
                            "minDamagePercent": 0.5,
                            "maxDamagePercent": 1.0
                        },
                        {
                            "id": "near_death_warning",
                            "text": "barely standing",
                            "minHealthPercent": 0,
                            "maxHealthPercent": 0.25
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid percentage values
        errors.Should().BeEmpty("valid percentage values within 0-1 range should pass validation");
    }

    #endregion

    #region DSC-006: ID Pattern Validation

    /// <summary>
    /// DSC-006: Verifies that category must follow kebab-case pattern.
    /// Underscores in category should fail validation.
    /// </summary>
    /// <remarks>
    /// Category pattern: ^[a-z][a-z0-9-]*$ (lowercase kebab-case, no underscores).
    /// </remarks>
    [Test]
    public void Category_WithUnderscore_FailsValidation()
    {
        // Arrange: JSON with underscore in category
        var invalidJson = """
        {
            "category": "combat_hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        { "id": "test", "text": "test" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to underscore in category
        errors.Should().NotBeEmpty("category 'combat_hits' should fail pattern validation (must be kebab-case)");
    }

    /// <summary>
    /// DSC-006: Verifies that category must be lowercase.
    /// Uppercase letters in category should fail validation.
    /// </summary>
    /// <remarks>
    /// Category pattern: ^[a-z][a-z0-9-]*$ (must start with lowercase, all lowercase).
    /// </remarks>
    [Test]
    public void Category_WithUppercase_FailsValidation()
    {
        // Arrange: JSON with uppercase in category
        var invalidJson = """
        {
            "category": "Combat-Hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        { "id": "test", "text": "test" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to uppercase in category
        errors.Should().NotBeEmpty("category 'Combat-Hits' should fail pattern validation (must be lowercase)");
    }

    /// <summary>
    /// DSC-006: Verifies that pool ID must follow snake_case pattern.
    /// Hyphens in pool ID should fail validation.
    /// </summary>
    /// <remarks>
    /// Pool ID pattern: ^[a-z][a-z0-9_]*$ (lowercase with underscores, no hyphens).
    /// </remarks>
    [Test]
    public void PoolId_WithHyphen_FailsValidation()
    {
        // Arrange: JSON with hyphen in pool id
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit-sword": {
                    "id": "hit-sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": [
                        { "id": "test", "text": "test" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to hyphen in pool id
        errors.Should().NotBeEmpty("pool id 'hit-sword' should fail pattern validation (must use underscores)");
    }

    /// <summary>
    /// DSC-006: Verifies that descriptor ID must be lowercase.
    /// Uppercase letters in descriptor ID should fail validation.
    /// </summary>
    /// <remarks>
    /// Descriptor ID pattern: ^[a-z][a-z0-9_]*$ (lowercase with underscores).
    /// </remarks>
    [Test]
    public void DescriptorId_WithUppercase_FailsValidation()
    {
        // Arrange: JSON with uppercase in descriptor id
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        { "id": "Slashes_Across", "text": "slashes across" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to uppercase in descriptor id
        errors.Should().NotBeEmpty("descriptor id 'Slashes_Across' should fail pattern validation (must be lowercase)");
    }

    #endregion

    #region Backward Compatibility Tests

    /// <summary>
    /// Verifies the existing combat-hits.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test for combat descriptors.
    /// </summary>
    /// <remarks>
    /// The combat-hits.json contains 11 pools with weapon-specific hit descriptions,
    /// critical hit descriptions, miss descriptions, and fumble descriptions.
    /// </remarks>
    [Test]
    public async Task CombatHitsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual combat-hits.json file
        var jsonContent = await File.ReadAllTextAsync(CombatHitsJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing combat-hits.json should be valid
        errors.Should().BeEmpty(
            "existing combat-hits.json with 11 pools should validate against schema without errors");
    }

    /// <summary>
    /// Verifies the existing environmental.json configuration validates against the schema
    /// without errors. This tests environmental descriptors with tags and themes.
    /// </summary>
    /// <remarks>
    /// The environmental.json contains 4 pools (lighting, sounds, smells, temperature)
    /// with context tags and theme filters.
    /// </remarks>
    [Test]
    public async Task EnvironmentalJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual environmental.json file
        var jsonContent = await File.ReadAllTextAsync(EnvironmentalJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing environmental.json should be valid
        errors.Should().BeEmpty(
            "existing environmental.json with 4 pools and tag/theme filters should validate against schema without errors");
    }

    #endregion

    #region Complete Configuration Tests

    /// <summary>
    /// Verifies a complete descriptor configuration with all optional fields validates successfully.
    /// This tests the full feature set of the schema.
    /// </summary>
    /// <remarks>
    /// Tests weight, tags, themes, damage/health percentages, variables, and fallbackId together.
    /// </remarks>
    [Test]
    public void CompleteConfiguration_AllOptionalFields_PassesValidation()
    {
        // Arrange: JSON with all optional fields populated
        var validJson = """
        {
            "$schema": "./schemas/descriptors.schema.json",
            "version": "1.0.0",
            "category": "combat-deaths",
            "pools": {
                "player_death": {
                    "id": "player_death",
                    "name": "Player Death Descriptions",
                    "description": "Descriptions for when the player character dies in combat",
                    "descriptors": [
                        {
                            "id": "falls_battle",
                            "text": "{player} has fallen in battle against the {killer}...",
                            "weight": 25,
                            "tags": ["combat", "death"],
                            "themes": ["dark_fantasy", "horror"],
                            "minHealthPercent": 0,
                            "maxHealthPercent": 0,
                            "variables": ["player", "killer"]
                        },
                        {
                            "id": "overwhelmed",
                            "text": "{player} was overwhelmed by {killer}'s relentless assault...",
                            "weight": 20,
                            "tags": ["combat", "death"],
                            "themes": ["dark_fantasy"],
                            "variables": ["player", "killer"],
                            "fallbackId": "falls_battle"
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for complete configuration
        errors.Should().BeEmpty("complete configuration with all optional fields should validate successfully");
    }

    #endregion
}
