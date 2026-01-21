// ------------------------------------------------------------------------------
// <copyright file="ArchetypeSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for archetypes.schema.json validation.
// Verifies schema structure, statTendency enum validation, and required fields.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the archetypes.schema.json JSON Schema.
/// Tests ensure the schema correctly validates archetype configuration files,
/// enforces statTendency enums, and validates required fields.
/// </summary>
[TestFixture]
public class ArchetypeSchemaTests
{
    /// <summary>
    /// Path to the archetypes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/archetypes.schema.json";

    /// <summary>
    /// Path to the actual archetypes.json configuration file.
    /// </summary>
    private const string ArchetypesJsonPath = "../../../../../config/archetypes.json";

    /// <summary>
    /// Loaded JSON Schema for archetype definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the archetypes schema.
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
        _schema.Title.Should().Be("Archetypes Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("ArchetypeDefinition", "should define ArchetypeDefinition");
    }

    /// <summary>
    /// Verifies the existing archetypes.json configuration validates against the schema without errors.
    /// This is the primary acceptance test ensuring existing data is compatible.
    /// </summary>
    [Test]
    public async Task ExistingArchetypesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual archetypes.json file
        var archetypesJsonContent = await File.ReadAllTextAsync(ArchetypesJsonPath);

        // Act: Validate the archetypes.json content against the schema
        var errors = _schema.Validate(archetypesJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing archetypes.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that statTendency must be a valid enum value.
    /// Invalid statTendency values should produce validation errors.
    /// </summary>
    [Test]
    public void StatTendency_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid statTendency value
        var invalidJson = """
        {
            "archetypes": [
                {
                    "id": "berserker",
                    "name": "Berserker",
                    "description": "A rage-fueled warrior.",
                    "playstyleSummary": "All-out aggression with no defense",
                    "statTendency": "aggressive",
                    "sortOrder": 1
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the statTendency enum
        errors.Should().NotBeEmpty(
            "statTendency 'aggressive' should fail enum validation");
        
        // Verify the error references the statTendency property
        var errorSummary = string.Join(", ", errors.Select(e => e.ToString()));
        errorSummary.Should().Contain("statTendency",
            "error should reference the statTendency property");
    }
}
