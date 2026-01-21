// -----------------------------------------------------------------------
// <copyright file="ProgressionSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
//     Licensed under the MIT License.
// </copyright>
// <summary>
//     Unit tests for progression.schema.json validation.
//     Tests schema structure, existing data validation, and field constraints.
// </summary>
// -----------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests for the progression.schema.json validation.
/// Validates XP curve types, level range constraints, and milestone structure.
/// </summary>
[TestFixture]
[Category("Schemas")]
[Category("Integration")]
public class ProgressionSchemaTests
{
    /// <summary>
    /// Relative path from test execution directory to config folder.
    /// </summary>
    private const string ConfigPath = "../../../../../config";

    /// <summary>
    /// Path to the progression schema file.
    /// </summary>
    private const string SchemaPath = ConfigPath + "/schemas/progression.schema.json";

    /// <summary>
    /// Path to the progression configuration file.
    /// </summary>
    private const string ProgressionPath = ConfigPath + "/progression.json";

    /// <summary>
    /// Cached schema instance to avoid repeated file reads.
    /// </summary>
    private JsonSchema? _schema;

    /// <summary>
    /// Loads the progression schema before each test.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Load and cache the schema for test performance
        var schemaJson = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaJson);
    }

    /// <summary>
    /// Verifies that progression.schema.json is a valid JSON Schema Draft-07 document.
    /// Ensures the schema can be parsed and has expected root properties.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Arrange & Act - schema loaded in SetUp

        // Assert - verify schema properties
        _schema.Should().NotBeNull("schema file should parse successfully");

        // Verify required root properties exist
        _schema!.Properties.Should().ContainKey("maxLevel", "required field should be defined");
        _schema.Properties.Should().ContainKey("baseXpRequirement", "required field should be defined");
        _schema.Properties.Should().ContainKey("curveType", "curve type enum should be defined");
        _schema.Properties.Should().ContainKey("levels", "levels array should be defined");

        // Verify definitions exist
        _schema.Definitions.Should().ContainKey("StatBonuses", "StatBonuses definition required");
        _schema.Definitions.Should().ContainKey("LevelMilestone", "LevelMilestone definition required");
    }

    /// <summary>
    /// Verifies that the existing progression.json configuration validates
    /// against the schema without any errors.
    /// </summary>
    [Test]
    public async Task ExistingProgressionJson_ValidatesAgainstSchema()
    {
        // Arrange - load existing progression configuration
        var configJson = await File.ReadAllTextAsync(ProgressionPath);

        // Act - validate against schema
        var errors = _schema!.Validate(configJson);

        // Assert - no validation errors
        errors.Should().BeEmpty(
            "existing progression.json should validate against schema without errors. " +
            $"Errors found: {string.Join(", ", errors.Select(e => e.ToString()))}");
    }

    /// <summary>
    /// Verifies that an invalid curveType value fails schema validation.
    /// The curveType field must be one of: Linear, Exponential, Custom.
    /// </summary>
    [Test]
    public void CurveType_InvalidEnumValue_FailsValidation()
    {
        // Arrange - create JSON with invalid curve type
        var invalidJson = new JObject
        {
            ["maxLevel"] = 20,
            ["baseXpRequirement"] = 100,
            ["curveType"] = "InvalidCurve" // Invalid - must be Linear/Exponential/Custom
        };

        // Act - validate against schema
        var errors = _schema!.Validate(invalidJson.ToString());

        // Assert - validation should fail
        errors.Should().NotBeEmpty("invalid curveType should be rejected");

        // Verify the error relates to curveType enum
        var errorText = string.Join(" ", errors.Select(e => e.ToString()));
        errorText.Should().Contain("curveType",
            "error message should reference curveType field");
    }

    /// <summary>
    /// Verifies that maxLevel values outside the valid range (1-100) fail validation.
    /// </summary>
    [Test]
    public void MaxLevel_OutOfRange_FailsValidation()
    {
        // Arrange - create JSON with maxLevel out of range
        var invalidJson = new JObject
        {
            ["maxLevel"] = 150, // Invalid - maximum is 100
            ["baseXpRequirement"] = 100
        };

        // Act - validate against schema
        var errors = _schema!.Validate(invalidJson.ToString());

        // Assert - validation should fail
        errors.Should().NotBeEmpty("maxLevel > 100 should be rejected");

        var errorText = string.Join(" ", errors.Select(e => e.ToString()));
        errorText.Should().Contain("maxLevel",
            "error message should reference maxLevel field");
    }
}
