// ------------------------------------------------------------------------------
// <copyright file="BackgroundSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for backgrounds.schema.json validation.
// Verifies schema structure, category enum validation, attribute bonus range,
// and required fields.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the backgrounds.schema.json JSON Schema.
/// Tests ensure the schema correctly validates background configuration files,
/// enforces category enums, attribute bonus ranges, and required fields.
/// </summary>
[TestFixture]
public class BackgroundSchemaTests
{
    /// <summary>
    /// Path to the backgrounds schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/backgrounds.schema.json";

    /// <summary>
    /// Path to the actual backgrounds.json configuration file.
    /// </summary>
    private const string BackgroundsJsonPath = "../../../../../config/backgrounds.json";

    /// <summary>
    /// Loaded JSON Schema for background definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the backgrounds schema.
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
        _schema.Title.Should().Be("Background Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("BackgroundDefinition", "should define BackgroundDefinition");
        _schema.Definitions.Should().ContainKey("AttributeBonuses", "should define AttributeBonuses");
    }

    /// <summary>
    /// Verifies the existing backgrounds.json configuration validates against the schema without errors.
    /// This is the primary acceptance test ensuring existing data is compatible.
    /// </summary>
    [Test]
    public async Task ExistingBackgroundsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual backgrounds.json file
        var backgroundsJsonContent = await File.ReadAllTextAsync(BackgroundsJsonPath);

        // Act: Validate the backgrounds.json content against the schema
        var errors = _schema.Validate(backgroundsJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing backgrounds.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that category must be a valid enum value (Profession or Lineage).
    /// Invalid category values should produce validation errors.
    /// </summary>
    [Test]
    public void Category_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid category value
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "id": "noble",
                    "name": "Noble",
                    "category": "Occupation",
                    "description": "Born into wealth and privilege.",
                    "attributeBonuses": { "will": 1 },
                    "starterAbilityId": "command",
                    "starterAbilityName": "Command",
                    "starterAbilityDescription": "Issue orders to allies.",
                    "startingItems": ["signet_ring"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the category enum
        errors.Should().NotBeEmpty(
            "category 'Occupation' should fail enum validation");
        
        // Verify the error references the category property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("category",
            "error should reference the category property");
    }

    /// <summary>
    /// Verifies that attribute bonuses must be within 0 to 2 range.
    /// Negative values or values above 2 should produce validation errors.
    /// </summary>
    [Test]
    public void AttributeBonuses_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with out-of-range attribute bonus (might: 5)
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "id": "berserker",
                    "name": "Berserker",
                    "category": "Profession",
                    "description": "A warrior consumed by battle fury.",
                    "attributeBonuses": { "might": 5 },
                    "starterAbilityId": "rage",
                    "starterAbilityName": "Rage",
                    "starterAbilityDescription": "Enter a furious battle state.",
                    "startingItems": ["greataxe"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the attribute range
        errors.Should().NotBeEmpty(
            "attribute bonus 'might: 5' should fail range validation (max is 2)");
        
        // Verify the error references the might property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("might",
            "error should reference the might property");
    }

    /// <summary>
    /// Verifies that negative attribute bonuses are not allowed (minimum: 0).
    /// Backgrounds provide bonuses only, not penalties.
    /// </summary>
    [Test]
    public void AttributeBonuses_NegativeValue_FailsValidation()
    {
        // Arrange: JSON with negative attribute bonus
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "id": "clumsy",
                    "name": "Clumsy",
                    "category": "Lineage",
                    "description": "Born with two left feet.",
                    "attributeBonuses": { "finesse": -1 },
                    "starterAbilityId": "trip",
                    "starterAbilityName": "Trip",
                    "starterAbilityDescription": "Fall over at inopportune times.",
                    "startingItems": ["bandages"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the negative value
        errors.Should().NotBeEmpty(
            "attribute bonus 'finesse: -1' should fail minimum validation (min is 0)");
        
        // Verify the error references the finesse property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("finesse",
            "error should reference the finesse property");
    }
}
