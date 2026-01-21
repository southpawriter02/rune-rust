// ------------------------------------------------------------------------------
// <copyright file="TiersSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for tiers.schema.json validation.
// Verifies schema structure, multiplier range validation (>= 0.1), spawn weight
// non-negative validation, and backwards compatibility with existing tiers.json
// configuration containing 4 tier definitions.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the tiers.schema.json JSON Schema.
/// Tests ensure the schema correctly validates tier configuration files,
/// enforces multiplier minimum values (>= 0.1), spawn weight non-negativity,
/// and required fields (id, name, color, sortOrder). These tests support the
/// v0.14.3c Tier Schema implementation for monster difficulty customization.
/// </summary>
[TestFixture]
public class TiersSchemaTests
{
    /// <summary>
    /// Path to the tiers schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/tiers.schema.json";

    /// <summary>
    /// Path to the actual tiers.json configuration file.
    /// </summary>
    private const string TiersJsonPath = "../../../../../config/tiers.json";

    /// <summary>
    /// Loaded JSON Schema for tier definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the tiers schema.
    /// This is called before each test to ensure a fresh schema instance.
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
    /// Validates that the TierDefinition definition is present and correctly configured.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Monster Tier Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify TierDefinition is present for tier configuration
        _schema.Definitions.Should().ContainKey("TierDefinition", "should define TierDefinition");
    }

    /// <summary>
    /// Verifies the tiers.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 4 existing tiers (common, named, elite, boss) are valid.
    /// </summary>
    [Test]
    public async Task ExistingTiersJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual tiers.json file
        var tiersJsonContent = await File.ReadAllTextAsync(TiersJsonPath);

        // Act: Validate the tiers.json content against the schema
        var errors = _schema.Validate(tiersJsonContent);

        // Assert: No validation errors - all existing tiers should be valid
        errors.Should().BeEmpty(
            "existing tiers.json should validate against the schema without errors. All 4 tiers (common, named, elite, boss) must pass validation.");
    }

    /// <summary>
    /// Verifies multipliers must be >= 0.1.
    /// Multipliers below 0.1 (like 0.05) would result in nearly non-existent stats
    /// and are prevented to maintain balance. This tests healthMultiplier specifically
    /// but the same rule applies to attack, defense, experience, and loot multipliers.
    /// </summary>
    [Test]
    public void Multipliers_LessThanMinimum_FailsValidation()
    {
        // Arrange: JSON with multiplier below 0.1 minimum
        var invalidJson = """
        {
            "tiers": [
                {
                    "id": "weak",
                    "name": "Weak",
                    "healthMultiplier": 0.05,
                    "color": "grey",
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to multiplier below 0.1
        errors.Should().NotBeEmpty(
            "healthMultiplier 0.05 should fail validation (minimum is 0.1 to prevent near-zero stats)");
    }

    /// <summary>
    /// Verifies spawn weight must be non-negative (>= 0).
    /// Negative spawn weights are meaningless in probability calculations
    /// and would cause errors in tier selection algorithms.
    /// </summary>
    [Test]
    public void SpawnWeight_Negative_FailsValidation()
    {
        // Arrange: JSON with negative spawn weight
        var invalidJson = """
        {
            "tiers": [
                {
                    "id": "impossible",
                    "name": "Impossible",
                    "spawnWeight": -10,
                    "color": "black",
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative spawn weight
        errors.Should().NotBeEmpty(
            "spawnWeight -10 should fail validation (minimum is 0, use 0 for tiers that never spawn randomly)");
    }

    /// <summary>
    /// Verifies minimumLevel must be >= 1 when specified.
    /// Level 0 is not valid as player levels start at 1.
    /// This test ensures level gating works correctly.
    /// </summary>
    [Test]
    public void MinimumLevel_Zero_FailsValidation()
    {
        // Arrange: JSON with minimumLevel of 0 (invalid)
        var invalidJson = """
        {
            "tiers": [
                {
                    "id": "early_tier",
                    "name": "Early Tier",
                    "minimumLevel": 0,
                    "color": "green",
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to minimumLevel below 1
        errors.Should().NotBeEmpty(
            "minimumLevel 0 should fail validation (minimum is 1, player levels start at 1)");
    }

    /// <summary>
    /// Verifies additionalTraits array items must match the trait ID pattern.
    /// Trait IDs must be lowercase with underscores, matching the pattern used
    /// in traits.json for cross-reference consistency.
    /// </summary>
    [Test]
    public void AdditionalTraits_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with an invalid trait ID pattern (uppercase/hyphen)
        var invalidJson = """
        {
            "tiers": [
                {
                    "id": "special",
                    "name": "Special",
                    "additionalTraits": ["Invalid-Trait"],
                    "color": "purple",
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid trait ID pattern
        errors.Should().NotBeEmpty(
            "trait ID 'Invalid-Trait' should fail validation (must match ^[a-z][a-z0-9_]*$ pattern)");
    }
}
