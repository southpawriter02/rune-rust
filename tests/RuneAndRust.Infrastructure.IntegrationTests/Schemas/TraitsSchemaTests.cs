// ------------------------------------------------------------------------------
// <copyright file="TraitsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for traits.schema.json validation.
// Verifies schema structure, effect enum validation, tags enum array validation,
// stat modifier percentage validation, and backwards compatibility with 
// existing traits.json configuration.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the traits.schema.json JSON Schema.
/// Tests ensure the schema correctly validates trait configuration files,
/// enforces effect enum values (11 types), tags enum values (7 types),
/// stat modifier percentage ranges (0-1), resistance ranges (-100 to 100),
/// and required fields. These tests support the v0.14.3b Trait Schema
/// implementation for comprehensive monster trait customization.
/// </summary>
[TestFixture]
public class TraitsSchemaTests
{
    /// <summary>
    /// Path to the traits schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/traits.schema.json";

    /// <summary>
    /// Path to the actual traits.json configuration file.
    /// </summary>
    private const string TraitsJsonPath = "../../../../../config/traits.json";

    /// <summary>
    /// Loaded JSON Schema for trait definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the traits schema.
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
    /// Validates that all required definitions are present (TraitDefinition, StatModifiers,
    /// ResistanceModifiers).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Monster Trait Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present for trait configuration
        _schema.Definitions.Should().ContainKey("TraitDefinition", "should define TraitDefinition");
        _schema.Definitions.Should().ContainKey("StatModifiers", "should define StatModifiers for stat bonuses");
        _schema.Definitions.Should().ContainKey("ResistanceModifiers", "should define ResistanceModifiers for resistance/immunity grants");
    }

    /// <summary>
    /// Verifies the traits.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 5 existing traits (regenerating, flying, venomous, armored, berserker) are valid.
    /// </summary>
    [Test]
    public async Task ExistingTraitsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual traits.json file
        var traitsJsonContent = await File.ReadAllTextAsync(TraitsJsonPath);

        // Act: Validate the traits.json content against the schema
        var errors = _schema.Validate(traitsJsonContent);

        // Assert: No validation errors - all existing traits should be valid
        errors.Should().BeEmpty(
            "existing traits.json should validate against the schema without errors. All 5 traits (regenerating, flying, venomous, armored, berserker) must pass validation.");
    }

    /// <summary>
    /// Verifies effect must be a valid enum value.
    /// Only the 11 defined effect types are valid: Regeneration, Flying, Venomous,
    /// Armored, Berserker, SpellResistant, Undying, Splitting, Burrowing, Invisible, Teleporting.
    /// Invalid effect values should fail validation.
    /// </summary>
    [Test]
    public void Effect_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid effect enum value
        var invalidJson = """
        {
            "traits": [
                {
                    "id": "invalid_effect_trait",
                    "name": "Invalid Effect Trait",
                    "description": "A trait with an invalid effect type.",
                    "effect": "CustomEffect",
                    "effectValue": 10,
                    "tags": ["passive"],
                    "color": "blue",
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid effect enum
        errors.Should().NotBeEmpty(
            "effect 'CustomEffect' should fail validation (valid values: Regeneration, Flying, Venomous, Armored, Berserker, SpellResistant, Undying, Splitting, Burrowing, Invisible, Teleporting)");
    }

    /// <summary>
    /// Verifies tags must contain valid enum values.
    /// Only the 7 defined tag types are valid: passive, triggered, offensive,
    /// defensive, mobility, healing, poison.
    /// Invalid tag values should fail validation.
    /// </summary>
    [Test]
    public void Tags_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid tag enum value
        var invalidJson = """
        {
            "traits": [
                {
                    "id": "invalid_tag_trait",
                    "name": "Invalid Tag Trait",
                    "description": "A trait with an invalid tag.",
                    "effect": "Regeneration",
                    "effectValue": 5,
                    "tags": ["custom_tag"],
                    "color": "green",
                    "sortOrder": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid tag enum
        errors.Should().NotBeEmpty(
            "tag 'custom_tag' should fail validation (valid values: passive, triggered, offensive, defensive, mobility, healing, poison)");
    }

    /// <summary>
    /// Verifies stat modifier percentages must be between 0 and 1.
    /// Values above 1.0 should fail validation as percentages are expressed as decimals.
    /// </summary>
    [Test]
    public void StatModifiers_PercentageOutOfRange_FailsValidation()
    {
        // Arrange: JSON with percentage modifier exceeding 1.0
        var invalidJson = """
        {
            "traits": [
                {
                    "id": "invalid_percent_trait",
                    "name": "Invalid Percentage Trait",
                    "description": "A trait with invalid percentage modifier.",
                    "effect": "Armored",
                    "effectValue": 5,
                    "tags": ["defensive", "passive"],
                    "color": "grey",
                    "sortOrder": 0,
                    "statModifiers": {
                        "bonusHealthPercent": 1.5
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to percentage exceeding 1.0
        errors.Should().NotBeEmpty(
            "bonusHealthPercent 1.5 should fail validation (maximum is 1.0, representing 100%)");
    }

    /// <summary>
    /// Verifies resistance values must be between -100 and 100.
    /// -100 represents double damage (weakness), 0 is normal damage,
    /// and 100 represents immunity. Values outside this range should fail.
    /// </summary>
    [Test]
    public void ResistanceModifiers_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with resistance value exceeding 100 (invalid maximum)
        var invalidJson = """
        {
            "traits": [
                {
                    "id": "invalid_resistance_trait",
                    "name": "Invalid Resistance Trait",
                    "description": "A trait with invalid resistance value.",
                    "effect": "SpellResistant",
                    "effectValue": 25,
                    "tags": ["defensive", "passive"],
                    "color": "purple",
                    "sortOrder": 0,
                    "resistanceModifiers": {
                        "resistances": {
                            "fire": 150
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to resistance exceeding 100
        errors.Should().NotBeEmpty(
            "resistance value 150 should fail validation (maximum is 100, representing immunity)");
    }

    /// <summary>
    /// Verifies visualSize must be a valid enum value.
    /// Only the 5 defined size options are valid: Tiny, Small, Medium, Large, Huge.
    /// Invalid size values should fail validation.
    /// </summary>
    [Test]
    public void VisualSize_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid visualSize enum value
        var invalidJson = """
        {
            "traits": [
                {
                    "id": "invalid_size_trait",
                    "name": "Invalid Size Trait",
                    "description": "A trait with an invalid visual size.",
                    "effect": "Armored",
                    "effectValue": 5,
                    "tags": ["defensive"],
                    "color": "grey",
                    "sortOrder": 0,
                    "visualSize": "Gigantic"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid visualSize enum
        errors.Should().NotBeEmpty(
            "visualSize 'Gigantic' should fail validation (valid values: Tiny, Small, Medium, Large, Huge)");
    }
}
