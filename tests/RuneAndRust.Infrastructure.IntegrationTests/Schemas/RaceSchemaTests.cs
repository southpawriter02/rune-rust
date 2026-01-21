// ------------------------------------------------------------------------------
// <copyright file="RaceSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for races.schema.json validation.
// Verifies schema structure, pattern validation, range validation, and required fields.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the races.schema.json JSON Schema.
/// Tests ensure the schema correctly validates race configuration files,
/// enforces ID patterns, attribute modifier ranges, and required fields.
/// </summary>
[TestFixture]
public class RaceSchemaTests
{
    /// <summary>
    /// Path to the races schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/races.schema.json";

    /// <summary>
    /// Path to the actual races.json configuration file.
    /// </summary>
    private const string RacesJsonPath = "../../../../../config/races.json";

    /// <summary>
    /// Loaded JSON Schema for race definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the races schema.
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
        _schema.Title.Should().Be("Race Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("RaceDefinition", "should define RaceDefinition");
        _schema.Definitions.Should().ContainKey("AttributeModifiers", "should define AttributeModifiers");
    }

    /// <summary>
    /// Verifies the existing races.json configuration validates against the schema without errors.
    /// This is the primary acceptance test ensuring existing data is compatible.
    /// </summary>
    [Test]
    public async Task ExistingRacesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual races.json file
        var racesJsonContent = await File.ReadAllTextAsync(RacesJsonPath);

        // Act: Validate the races.json content against the schema
        var errors = _schema.Validate(racesJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing races.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that race IDs must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid patterns should produce validation errors.
    /// </summary>
    [Test]
    public void RaceId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid race ID (uppercase, spaces)
        var invalidJson = """
        {
            "races": [
                {
                    "id": "High Elf",
                    "name": "High Elf",
                    "description": "Noble elves from the ancient kingdoms.",
                    "attributeModifiers": {
                        "might": 0,
                        "fortitude": 0,
                        "will": 1,
                        "wits": 1,
                        "finesse": 1
                    },
                    "traitId": "ancient_wisdom",
                    "traitName": "Ancient Wisdom",
                    "traitDescription": "Bonus to knowledge checks."
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the ID pattern
        errors.Should().NotBeEmpty(
            "race ID 'High Elf' should fail pattern validation");
        
        // Verify the error description contains the pattern mismatch info
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("id", 
            "error should reference the id property");
    }

    /// <summary>
    /// Verifies that attribute modifiers must be within the -3 to +3 range.
    /// Values outside this range should produce validation errors.
    /// </summary>
    [Test]
    public void AttributeModifiers_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with out-of-range attribute modifier (might: 5)
        var invalidJson = """
        {
            "races": [
                {
                    "id": "overpowered-race",
                    "name": "Overpowered Race",
                    "description": "A race with overpowered attributes.",
                    "attributeModifiers": {
                        "might": 5,
                        "fortitude": 0,
                        "will": 0,
                        "wits": 0,
                        "finesse": 0
                    },
                    "traitId": "power_overwhelming",
                    "traitName": "Power Overwhelming",
                    "traitDescription": "Too strong."
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the attribute range
        errors.Should().NotBeEmpty(
            "attribute modifier 'might: 5' should fail range validation (max is 3)");
        
        // Verify the error description contains the might property info
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("might", 
            "error should reference the might property");
    }

    /// <summary>
    /// Verifies that all required fields must be present for a valid race definition.
    /// Missing required fields should produce validation errors.
    /// </summary>
    [Test]
    public void Race_MissingRequiredFields_FailsValidation()
    {
        // Arrange: JSON missing required fields (name, attributeModifiers)
        var invalidJson = """
        {
            "races": [
                {
                    "id": "incomplete-race",
                    "description": "A race missing required fields."
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for missing required fields
        errors.Should().NotBeEmpty(
            "race definition missing required fields should fail validation");
        
        // The error count should reflect multiple missing required fields
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("races[0]",
            "error should reference the race at index 0");
    }
}


