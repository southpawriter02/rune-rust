// ------------------------------------------------------------------------------
// <copyright file="ClassSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for classes.schema.json validation.
// Verifies schema structure, pattern validation, resource type enum, growth rate validation,
// and required fields.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the classes.schema.json JSON Schema.
/// Tests ensure the schema correctly validates class configuration files,
/// enforces ID patterns, resource type enums, growth rate constraints, and required fields.
/// </summary>
[TestFixture]
public class ClassSchemaTests
{
    /// <summary>
    /// Path to the classes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/classes.schema.json";

    /// <summary>
    /// Path to the actual classes.json configuration file.
    /// </summary>
    private const string ClassesJsonPath = "../../../../../config/classes.json";

    /// <summary>
    /// Loaded JSON Schema for class definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the classes schema.
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
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Class Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("ClassDefinition", "should define ClassDefinition");
        _schema.Definitions.Should().ContainKey("StatModifiers", "should define StatModifiers");
        _schema.Definitions.Should().ContainKey("GrowthRates", "should define GrowthRates");
    }

    /// <summary>
    /// Verifies the existing classes.json configuration validates against the schema without errors.
    /// This is the primary acceptance test ensuring existing data is compatible.
    /// </summary>
    [Test]
    public async Task ExistingClassesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual classes.json file
        var classesJsonContent = await File.ReadAllTextAsync(ClassesJsonPath);

        // Act: Validate the classes.json content against the schema
        var errors = _schema.Validate(classesJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing classes.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that class IDs must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid patterns should produce validation errors.
    /// </summary>
    [Test]
    public void ClassId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid class ID (uppercase, spaces)
        var invalidJson = """
        {
            "classes": [
                {
                    "id": "Shadow Walker",
                    "name": "Shadow Walker",
                    "description": "A class with an invalid ID.",
                    "archetypeId": "skirmisher",
                    "statModifiers": {
                        "maxHealth": 0, "attack": 0, "defense": 0,
                        "might": 0, "fortitude": 0, "will": 0, "wits": 0, "finesse": 0
                    },
                    "growthRates": {
                        "maxHealth": 5, "attack": 2, "defense": 1,
                        "might": 1, "fortitude": 1, "will": 1, "wits": 1, "finesse": 1
                    },
                    "primaryResourceId": "energy",
                    "startingAbilityIds": []
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the ID pattern
        errors.Should().NotBeEmpty(
            "class ID 'Shadow Walker' should fail pattern validation");
        
        // Verify the error references the id property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("id",
            "error should reference the id property");
    }

    /// <summary>
    /// Verifies that primaryResourceId must be one of the valid enum values.
    /// Invalid resource types should produce validation errors.
    /// </summary>
    [Test]
    public void PrimaryResourceId_InvalidValue_FailsValidation()
    {
        // Arrange: JSON with invalid resource type "ki"
        var invalidJson = """
        {
            "classes": [
                {
                    "id": "monk",
                    "name": "Monk",
                    "description": "A class using an invalid resource type.",
                    "archetypeId": "adept",
                    "statModifiers": {
                        "maxHealth": 0, "attack": 0, "defense": 0,
                        "might": 0, "fortitude": 0, "will": 0, "wits": 0, "finesse": 0
                    },
                    "growthRates": {
                        "maxHealth": 5, "attack": 2, "defense": 1,
                        "might": 1, "fortitude": 1, "will": 1, "wits": 1, "finesse": 1
                    },
                    "primaryResourceId": "ki",
                    "startingAbilityIds": []
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the invalid enum value
        errors.Should().NotBeEmpty(
            "resource type 'ki' should fail enum validation");
        
        // Verify the error references the resource type
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("primaryResourceId",
            "error should reference the primaryResourceId property");
    }

    /// <summary>
    /// Verifies that growth rates must be non-negative values (minimum: 0).
    /// Negative growth rates should produce validation errors.
    /// </summary>
    [Test]
    public void GrowthRates_NegativeValue_FailsValidation()
    {
        // Arrange: JSON with negative growth rate
        var invalidJson = """
        {
            "classes": [
                {
                    "id": "cursed-class",
                    "name": "Cursed Class",
                    "description": "A class with negative growth rates.",
                    "archetypeId": "mystic",
                    "statModifiers": {
                        "maxHealth": 0, "attack": 0, "defense": 0,
                        "might": 0, "fortitude": 0, "will": 0, "wits": 0, "finesse": 0
                    },
                    "growthRates": {
                        "maxHealth": -5,
                        "attack": 2, "defense": 1,
                        "might": 1, "fortitude": 1, "will": 1, "wits": 1, "finesse": 1
                    },
                    "primaryResourceId": "mana",
                    "startingAbilityIds": []
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for negative growth rate
        errors.Should().NotBeEmpty(
            "growthRates.maxHealth: -5 should fail minimum validation (min is 0)");
        
        // Verify the error references the growth rate property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("maxHealth",
            "error should reference the maxHealth property");
    }
}
