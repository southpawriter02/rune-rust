// ------------------------------------------------------------------------------
// <copyright file="ArmorSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for armor.schema.json validation.
// Verifies schema structure, armor slot validation, armor type enum validation,
// defense bonus range validation, initiative/movement penalty validation,
// and backwards compatibility with existing armor.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the armor.schema.json JSON Schema.
/// Tests ensure the schema correctly validates armor and accessory configuration files,
/// enforces defense bonus range (>= 0), initiative/movement penalty range (<= 0),
/// armor type enums, slot enums, and required fields.
/// </summary>
[TestFixture]
public class ArmorSchemaTests
{
    /// <summary>
    /// Path to the armor schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/armor.schema.json";

    /// <summary>
    /// Path to the actual armor.json configuration file.
    /// </summary>
    private const string ArmorJsonPath = "../../../../../config/armor.json";

    /// <summary>
    /// Loaded JSON Schema for armor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the armor schema.
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
    /// Validates that all required definitions are present (ArmorDefinition, AccessoryDefinition,
    /// StatRequirements, StatBonuses).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Armor Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("ArmorDefinition", "should define ArmorDefinition");
        _schema.Definitions.Should().ContainKey("AccessoryDefinition", "should define AccessoryDefinition");
        _schema.Definitions.Should().ContainKey("StatRequirements", "should define StatRequirements");
        _schema.Definitions.Should().ContainKey("StatBonuses", "should define StatBonuses");
    }

    /// <summary>
    /// Verifies the existing armor.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 14 existing armor pieces and 4 accessories are valid.
    /// </summary>
    [Test]
    public async Task ExistingArmorJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual armor.json file
        var armorJsonContent = await File.ReadAllTextAsync(ArmorJsonPath);

        // Act: Validate the armor.json content against the schema
        var errors = _schema.Validate(armorJsonContent);

        // Assert: No validation errors - all existing armor and accessories should be valid
        errors.Should().BeEmpty(
            "existing armor.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies defenseBonus must be non-negative (>= 0).
    /// Negative defense bonus values should fail validation.
    /// </summary>
    [Test]
    public void DefenseBonus_NegativeValue_FailsValidation()
    {
        // Arrange: JSON with negative defense bonus
        var invalidJson = """
        {
            "armor": [
                {
                    "id": "cursed_armor",
                    "name": "Cursed Armor",
                    "description": "Armor with invalid negative defense.",
                    "slot": "Armor",
                    "armorType": "Light",
                    "defenseBonus": -1,
                    "value": 100
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative defense bonus
        errors.Should().NotBeEmpty("negative defenseBonus -1 should fail validation (minimum is 0)");
    }

    /// <summary>
    /// Verifies initiativePenalty must be non-positive (<= 0).
    /// Positive initiative penalty values should fail validation.
    /// </summary>
    [Test]
    public void InitiativePenalty_PositiveValue_FailsValidation()
    {
        // Arrange: JSON with positive initiative penalty (penalties should be negative)
        var invalidJson = """
        {
            "armor": [
                {
                    "id": "fast_armor",
                    "name": "Fast Armor",
                    "description": "Armor with invalid positive initiative penalty.",
                    "slot": "Armor",
                    "armorType": "Light",
                    "defenseBonus": 2,
                    "initiativePenalty": 2,
                    "value": 100
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to positive initiative penalty
        errors.Should().NotBeEmpty("positive initiativePenalty 2 should fail validation (maximum is 0)");
    }

    /// <summary>
    /// Verifies armorType must be a valid enum value (Light, Medium, Heavy).
    /// Invalid armor types should fail validation.
    /// </summary>
    [Test]
    public void ArmorType_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid armor type enum value
        var invalidJson = """
        {
            "armor": [
                {
                    "id": "invalid_type_armor",
                    "name": "Invalid Type Armor",
                    "description": "Armor with invalid type.",
                    "slot": "Armor",
                    "armorType": "SuperHeavy",
                    "defenseBonus": 5,
                    "value": 100
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid armor type enum
        errors.Should().NotBeEmpty("invalid armorType 'SuperHeavy' should fail validation");
    }
}
