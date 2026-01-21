// ------------------------------------------------------------------------------
// <copyright file="SkillSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for skills.schema.json validation.
// Verifies schema structure, attribute enum validation, category enum validation,
// and dice pool pattern validation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the skills.schema.json JSON Schema.
/// Tests ensure the schema correctly validates skill configuration files,
/// enforces attribute enums, category enums, and dice pool patterns.
/// </summary>
[TestFixture]
public class SkillSchemaTests
{
    /// <summary>
    /// Path to the skills schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/skills.schema.json";

    /// <summary>
    /// Path to the actual skills.json configuration file.
    /// </summary>
    private const string SkillsJsonPath = "../../../../../config/skills.json";

    /// <summary>
    /// Loaded JSON Schema for skill definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the skills schema.
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
        _schema.Title.Should().Be("Skills Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("SkillDefinition", "should define SkillDefinition");
    }

    /// <summary>
    /// Verifies the existing skills.json configuration validates against the schema without errors.
    /// This is the primary acceptance test ensuring existing data is compatible.
    /// </summary>
    [Test]
    public async Task ExistingSkillsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual skills.json file
        var skillsJsonContent = await File.ReadAllTextAsync(SkillsJsonPath);

        // Act: Validate the skills.json content against the schema
        var errors = _schema.Validate(skillsJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing skills.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that primaryAttribute must be a valid enum value.
    /// Invalid attribute values should produce validation errors.
    /// </summary>
    [Test]
    public void PrimaryAttribute_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid attribute value
        var invalidJson = """
        {
            "skills": [
                {
                    "id": "swimming",
                    "name": "Swimming",
                    "description": "Moving through water.",
                    "primaryAttribute": "strength",
                    "baseDicePool": "1d10",
                    "allowUntrained": true,
                    "category": "Physical",
                    "tags": ["physical"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the attribute enum
        errors.Should().NotBeEmpty(
            "attribute 'strength' should fail enum validation");
        
        // Verify the error references the primaryAttribute property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("primaryAttribute",
            "error should reference the primaryAttribute property");
    }

    /// <summary>
    /// Verifies that category must be a valid enum value.
    /// Invalid category values should produce validation errors.
    /// </summary>
    [Test]
    public void Category_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid category value
        var invalidJson = """
        {
            "skills": [
                {
                    "id": "combat",
                    "name": "Combat",
                    "description": "General fighting ability.",
                    "primaryAttribute": "might",
                    "baseDicePool": "1d10",
                    "allowUntrained": true,
                    "category": "Combat",
                    "tags": ["combat"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the category enum
        errors.Should().NotBeEmpty(
            "category 'Combat' should fail enum validation");
        
        // Verify the error references the category property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("category",
            "error should reference the category property");
    }
}
