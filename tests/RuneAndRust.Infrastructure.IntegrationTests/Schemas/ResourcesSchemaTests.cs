// ------------------------------------------------------------------------------
// <copyright file="ResourcesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for resources.schema.json validation.
// Verifies schema structure, attribute enum validation, abbreviation length,
// hex color validation, default max range, regen/decay range, build-on trigger
// validation, and backward compatibility with the existing resources.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the resources.schema.json JSON Schema.
/// Tests ensure the schema correctly validates resource configuration files,
/// enforces attribute enums, abbreviation length, hex color patterns, and
/// range constraints for defaultMax, regenPerTurn, decayPerTurn, and build-on triggers.
/// </summary>
/// <remarks>
/// <para>
/// The resource schema validates configurations including:
/// <list type="bullet">
/// <item><description>Resource IDs (kebab-case pattern)</description></item>
/// <item><description>Abbreviation length (1-4 characters)</description></item>
/// <item><description>Color hex codes (#RRGGBB format)</description></item>
/// <item><description>Default max (>= 1)</description></item>
/// <item><description>Regeneration and decay (>= 0)</description></item>
/// <item><description>Build-on triggers (>= 1 if present)</description></item>
/// <item><description>Attribute enum (might, fortitude, will, wits, finesse)</description></item>
/// <item><description>Required fields (id, displayName, abbreviation, description, color, defaultMax, sortOrder)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class ResourcesSchemaTests
{
    /// <summary>
    /// Path to the resources schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/resources.schema.json";

    /// <summary>
    /// Path to the actual resources.json configuration file.
    /// </summary>
    private const string ResourcesJsonPath = "../../../../../config/resources.json";

    /// <summary>
    /// Loaded JSON Schema for resource definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the resources schema.
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
    /// Validates that all required definitions are present (4 total).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 4 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Resource Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (4 total)
        _schema.Definitions.Should().ContainKey("ResourceDefinition", "should define ResourceDefinition");
        _schema.Definitions.Should().ContainKey("MaxCalculation", "should define MaxCalculation");
        _schema.Definitions.Should().ContainKey("AttributeScaling", "should define AttributeScaling");
        _schema.Definitions.Should().ContainKey("DepletionEffect", "should define DepletionEffect");
    }

    /// <summary>
    /// Verifies the existing resources.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all 6 existing resources are valid.
    /// </summary>
    /// <remarks>
    /// The existing resources.json contains 6 resources:
    /// health, mana, rage, energy, faith, focus.
    /// </remarks>
    [Test]
    public async Task ResourcesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual resources.json file
        var jsonContent = await File.ReadAllTextAsync(ResourcesJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing resources.json should be valid
        errors.Should().BeEmpty(
            "existing resources.json with 6 resources should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that abbreviation must be 1-4 characters.
    /// Abbreviations longer than 4 characters should fail validation.
    /// </summary>
    /// <remarks>
    /// Abbreviation length rules:
    /// <list type="bullet">
    /// <item><description>Valid: HP, MP, RG, EN, FTH, FOC</description></item>
    /// <item><description>Invalid: HEALTH (6 chars > 4)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Abbreviation_TooLong_FailsValidation()
    {
        // Arrange: JSON with abbreviation longer than maximum (MANAPOWER = 9 > 4)
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "mana",
                    "displayName": "Mana",
                    "abbreviation": "MANAPOWER",
                    "description": "Magic resource.",
                    "color": "#0066FF",
                    "defaultMax": 100,
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to abbreviation too long
        errors.Should().NotBeEmpty("abbreviation 'MANAPOWER' (9 chars) should fail validation (max is 4)");
    }

    /// <summary>
    /// Verifies that color must be a valid hex color code (#RRGGBB format).
    /// Invalid hex patterns should fail validation.
    /// </summary>
    /// <remarks>
    /// Hex color pattern: ^#[0-9A-Fa-f]{6}$
    /// <list type="bullet">
    /// <item><description>Valid: #FF0000, #0066FF, #00CCAA</description></item>
    /// <item><description>Invalid: red, blue, #FFF (3-digit), FF0000 (no #)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Color_InvalidHexPattern_FailsValidation()
    {
        // Arrange: JSON with invalid color (named color instead of hex)
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "health",
                    "displayName": "Health",
                    "abbreviation": "HP",
                    "description": "Your life force.",
                    "color": "red",
                    "defaultMax": 100,
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid hex color pattern
        errors.Should().NotBeEmpty("color 'red' should fail validation (must be hex #RRGGBB)");
    }

    /// <summary>
    /// Verifies that defaultMax must be at least 1.
    /// Values less than 1 (including 0 and negative) should fail validation.
    /// </summary>
    /// <remarks>
    /// Default maximum must be positive to allow meaningful resource usage.
    /// </remarks>
    [Test]
    public void DefaultMax_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with defaultMax less than minimum (0 < 1)
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "health",
                    "displayName": "Health",
                    "abbreviation": "HP",
                    "description": "Your life force.",
                    "color": "#FF0000",
                    "defaultMax": 0,
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to defaultMax less than minimum
        errors.Should().NotBeEmpty("defaultMax 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that attribute in AttributeScaling must be one of the valid enum values.
    /// Invalid attribute values should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid attributes: might, fortitude, will, wits, finesse.
    /// </remarks>
    [Test]
    public void AttributeScaling_InvalidAttribute_FailsValidation()
    {
        // Arrange: JSON with invalid attribute "strength"
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "health",
                    "displayName": "Health",
                    "abbreviation": "HP",
                    "description": "Your life force.",
                    "color": "#FF0000",
                    "defaultMax": 100,
                    "sortOrder": 0,
                    "maxCalculation": {
                        "baseValue": 50,
                        "attributeScaling": {
                            "attribute": "strength",
                            "multiplier": 2.0
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid attribute enum value
        errors.Should().NotBeEmpty("attribute 'strength' should fail validation (valid: might, fortitude, will, wits, finesse)");
    }

    /// <summary>
    /// Verifies that buildOnDamageDealt must be at least 1 when specified.
    /// Values less than 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Build-on triggers must be meaningful (at least 1 resource gained).
    /// </remarks>
    [Test]
    public void BuildOnTrigger_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with buildOnDamageDealt less than minimum (0 < 1)
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "rage",
                    "displayName": "Rage",
                    "abbreviation": "RG",
                    "description": "Battle fury.",
                    "color": "#FF6600",
                    "defaultMax": 100,
                    "sortOrder": 0,
                    "buildOnDamageDealt": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to buildOnDamageDealt less than minimum
        errors.Should().NotBeEmpty("buildOnDamageDealt 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that resource ID must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// <item><description>Valid: health, mana, battle-rage</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void ResourceId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid resource ID (uppercase)
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "Health Points",
                    "displayName": "Health",
                    "abbreviation": "HP",
                    "description": "Your life force.",
                    "color": "#FF0000",
                    "defaultMax": 100,
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("resource ID 'Health Points' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that required fields must be present.
    /// Missing required fields should fail validation.
    /// </summary>
    /// <remarks>
    /// Required fields: id, displayName, abbreviation, description, color, defaultMax, sortOrder.
    /// </remarks>
    [Test]
    public void RequiredFields_Missing_FailsValidation()
    {
        // Arrange: JSON missing required field 'description'
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "health",
                    "displayName": "Health",
                    "abbreviation": "HP",
                    "color": "#FF0000",
                    "defaultMax": 100,
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing required field
        errors.Should().NotBeEmpty("missing required field 'description' should fail validation");
    }

    /// <summary>
    /// Verifies that regenPerTurn cannot be negative.
    /// Negative regeneration values should fail validation.
    /// </summary>
    /// <remarks>
    /// Regeneration must be non-negative. Use decayPerTurn for resource loss.
    /// </remarks>
    [Test]
    public void RegenPerTurn_Negative_FailsValidation()
    {
        // Arrange: JSON with negative regenPerTurn
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "mana",
                    "displayName": "Mana",
                    "abbreviation": "MP",
                    "description": "Magic resource.",
                    "color": "#0066FF",
                    "defaultMax": 100,
                    "sortOrder": 0,
                    "regenPerTurn": -5
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative regeneration
        errors.Should().NotBeEmpty("regenPerTurn -5 should fail validation (minimum is 0)");
    }

    /// <summary>
    /// Verifies that maxCalculation.baseValue must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Base value in maxCalculation must be positive.
    /// </remarks>
    [Test]
    public void MaxCalculationBaseValue_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with maxCalculation.baseValue less than minimum (0 < 1)
        var invalidJson = """
        {
            "resourceTypes": [
                {
                    "id": "mana",
                    "displayName": "Mana",
                    "abbreviation": "MP",
                    "description": "Magic resource.",
                    "color": "#0066FF",
                    "defaultMax": 100,
                    "sortOrder": 0,
                    "maxCalculation": {
                        "baseValue": 0
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to baseValue less than minimum
        errors.Should().NotBeEmpty("maxCalculation.baseValue 0 should fail validation (minimum is 1)");
    }
}
