// ------------------------------------------------------------------------------
// <copyright file="DamageTypesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for damage-types.schema.json validation.
// Verifies schema structure, damage category validation, relationship enum validation,
// resistance value range validation, immunity/absorption flags, and ID pattern enforcement.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the damage-types.schema.json JSON Schema.
/// Tests ensure the schema correctly validates damage type configuration files,
/// enforces relationship enum, resistance value range, pattern validation,
/// and required fields.
/// </summary>
[TestFixture]
public class DamageTypesSchemaTests
{
    /// <summary>
    /// Path to the damage types schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/damage-types.schema.json";

    /// <summary>
    /// Path to the actual damage-types.json configuration file.
    /// </summary>
    private const string DamageTypesJsonPath = "../../../../../config/damage-types.json";

    /// <summary>
    /// Loaded JSON Schema for damage type definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the damage types schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    /// <summary>
    /// Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all required definitions are present (DamageCategory, DamageType,
    /// DamageRelationship, ResistanceTemplate, ResistanceEntry).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Damage Types Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("DamageCategory", "should define DamageCategory");
        _schema.Definitions.Should().ContainKey("DamageType", "should define DamageType");
        _schema.Definitions.Should().ContainKey("DamageRelationship", "should define DamageRelationship");
        _schema.Definitions.Should().ContainKey("ResistanceTemplate", "should define ResistanceTemplate");
        _schema.Definitions.Should().ContainKey("ResistanceEntry", "should define ResistanceEntry");
    }

    /// <summary>
    /// Verifies the existing damage-types.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// existing damage types, categories, and templates are valid.
    /// </summary>
    [Test]
    public async Task ExistingDamageTypesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual damage-types.json file
        var damageTypesJsonContent = await File.ReadAllTextAsync(DamageTypesJsonPath);

        // Act: Validate the damage-types.json content against the schema
        var errors = _schema.Validate(damageTypesJsonContent);

        // Assert: No validation errors - all existing damage types should be valid
        errors.Should().BeEmpty(
            "existing damage-types.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that relationship must be a valid enum value
    /// (Strong, Weak, Neutral, Nullifies). Invalid values should fail validation.
    /// </summary>
    [Test]
    public void Relationship_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid relationship enum value
        var invalidJson = """
        {
            "damageTypes": [
                {
                    "id": "test-damage",
                    "name": "Test Damage",
                    "description": "A test damage type with invalid relationship.",
                    "color": "white",
                    "relationships": [
                        {
                            "targetTypeId": "fire",
                            "relationship": "SuperStrong"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid relationship enum
        errors.Should().NotBeEmpty("invalid relationship 'SuperStrong' should fail validation");
    }

    /// <summary>
    /// Verifies that resistance value must be within valid range (-100 to 100).
    /// Values outside this range should fail validation.
    /// </summary>
    [Test]
    public void ResistanceValue_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with resistance value outside valid range
        var invalidJson = """
        {
            "damageTypes": [
                {
                    "id": "physical",
                    "name": "Physical",
                    "description": "Physical damage.",
                    "color": "white"
                }
            ],
            "resistanceTemplates": [
                {
                    "id": "test-template",
                    "name": "Test Template",
                    "creatureTypes": ["test-creature"],
                    "resistances": [
                        {
                            "damageTypeId": "physical",
                            "value": 150
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range resistance value
        errors.Should().NotBeEmpty("resistance value 150 should fail validation (max is 100)");
    }

    /// <summary>
    /// Verifies that isImmunity flag is validated as boolean.
    /// Valid boolean values should pass validation.
    /// </summary>
    [Test]
    public void IsImmunity_ValidBoolean_PassesValidation()
    {
        // Arrange: JSON with valid isImmunity boolean flag
        var validJson = """
        {
            "damageTypes": [
                {
                    "id": "poison",
                    "name": "Poison",
                    "description": "Toxic damage.",
                    "color": "green"
                }
            ],
            "resistanceTemplates": [
                {
                    "id": "undead-template",
                    "name": "Undead Template",
                    "creatureTypes": ["undead"],
                    "resistances": [
                        {
                            "damageTypeId": "poison",
                            "value": 100,
                            "isImmunity": true
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the valid JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: No validation errors
        errors.Should().BeEmpty("valid isImmunity boolean should pass validation");
    }

    /// <summary>
    /// Verifies that isAbsorption flag is validated as boolean.
    /// Valid boolean values should pass validation.
    /// </summary>
    [Test]
    public void IsAbsorption_ValidBoolean_PassesValidation()
    {
        // Arrange: JSON with valid isAbsorption boolean flag
        var validJson = """
        {
            "damageTypes": [
                {
                    "id": "fire",
                    "name": "Fire",
                    "description": "Burning damage.",
                    "color": "red"
                }
            ],
            "resistanceTemplates": [
                {
                    "id": "fire-elemental-template",
                    "name": "Fire Elemental Template",
                    "creatureTypes": ["fire-elemental"],
                    "resistances": [
                        {
                            "damageTypeId": "fire",
                            "value": 100,
                            "isImmunity": true,
                            "isAbsorption": true
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the valid JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: No validation errors
        errors.Should().BeEmpty("valid isAbsorption boolean should pass validation");
    }

    /// <summary>
    /// Verifies that ID fields must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    [Test]
    public void Id_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid damage type ID (uppercase, spaces)
        var invalidJson = """
        {
            "damageTypes": [
                {
                    "id": "Fire Damage",
                    "name": "Fire Damage",
                    "description": "A damage type with invalid ID pattern.",
                    "color": "red"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("damage type ID 'Fire Damage' should fail pattern validation");
    }

    /// <summary>
    /// Verifies that color field is required and non-empty.
    /// Missing color field should fail validation.
    /// </summary>
    [Test]
    public void Color_MissingOrEmpty_FailsValidation()
    {
        // Arrange: JSON with missing color field
        var invalidJson = """
        {
            "damageTypes": [
                {
                    "id": "fire",
                    "name": "Fire",
                    "description": "Fire damage without a color."
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing required color field
        errors.Should().NotBeEmpty("missing color field should fail validation");
    }
}
