// ------------------------------------------------------------------------------
// <copyright file="StatusEffectsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for status-effects.schema.json validation.
// Verifies schema structure, category enum, duration type enum, stacking rule enum,
// stat modifier validation, and ID pattern enforcement.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the status-effects.schema.json JSON Schema.
/// Tests ensure the schema correctly validates status effect configuration files,
/// enforces category enum, duration types, stacking rules, stat modifiers, and ID patterns.
/// </summary>
[TestFixture]
public class StatusEffectsSchemaTests
{
    /// <summary>
    /// Path to the status effects schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/status-effects.schema.json";

    /// <summary>
    /// Path to the actual status-effects.json configuration file.
    /// </summary>
    private const string StatusEffectsJsonPath = "../../../../../config/status-effects.json";

    /// <summary>
    /// Loaded JSON Schema for status effect definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the status effects schema.
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
    /// Validates that all required definitions are present (StatusEffect, StatModifier,
    /// TriggerCondition, EffectImmunity).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Status Effects Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("StatusEffect", "should define StatusEffect");
        _schema.Definitions.Should().ContainKey("StatModifier", "should define StatModifier");
        _schema.Definitions.Should().ContainKey("TriggerCondition", "should define TriggerCondition");
        _schema.Definitions.Should().ContainKey("EffectImmunity", "should define EffectImmunity");
    }

    /// <summary>
    /// Verifies the existing status-effects.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all 28
    /// existing status effects are valid according to the new schema.
    /// </summary>
    [Test]
    public async Task ExistingStatusEffectsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual status-effects.json file
        var statusEffectsJsonContent = await File.ReadAllTextAsync(StatusEffectsJsonPath);

        // Act: Validate the status-effects.json content against the schema
        var errors = _schema.Validate(statusEffectsJsonContent);

        // Assert: No validation errors - all 28 existing effects should be valid
        errors.Should().BeEmpty(
            "existing status-effects.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that category must be a valid enum value (Buff, Debuff, Environmental).
    /// Invalid category values should produce validation errors.
    /// </summary>
    [Test]
    public void Category_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid category enum value
        var invalidJson = """
        {
            "effects": [
                {
                    "id": "test-effect",
                    "name": "Test Effect",
                    "description": "A test effect with invalid category.",
                    "category": "Curse"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid category enum
        errors.Should().NotBeEmpty("invalid category 'Curse' should fail validation");
    }

    /// <summary>
    /// Verifies that durationType must be a valid enum value
    /// (Turns, Permanent, Triggered, ResourceBased). Invalid values should fail validation.
    /// </summary>
    [Test]
    public void DurationType_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid durationType enum value
        var invalidJson = """
        {
            "effects": [
                {
                    "id": "test-effect",
                    "name": "Test Effect",
                    "description": "A test effect with invalid duration type.",
                    "category": "Debuff",
                    "durationType": "Seconds"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid durationType enum
        errors.Should().NotBeEmpty("invalid durationType 'Seconds' should fail validation");
    }

    /// <summary>
    /// Verifies that stackingRule must be a valid enum value
    /// (None, RefreshDuration, StackIntensity, StackDuration, Block).
    /// Invalid values should fail validation.
    /// </summary>
    [Test]
    public void StackingRule_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid stackingRule enum value
        var invalidJson = """
        {
            "effects": [
                {
                    "id": "test-effect",
                    "name": "Test Effect",
                    "description": "A test effect with invalid stacking rule.",
                    "category": "Buff",
                    "stackingRule": "Refresh"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid stackingRule enum
        errors.Should().NotBeEmpty("invalid stackingRule 'Refresh' should fail validation");
    }

    /// <summary>
    /// Verifies that StatModifier requires all three fields: statId, modifierType, and value.
    /// Missing required fields in StatModifier should produce validation errors.
    /// </summary>
    [Test]
    public void StatModifier_MissingRequiredFields_FailsValidation()
    {
        // Arrange: JSON with StatModifier missing required 'value' field
        var invalidJson = """
        {
            "effects": [
                {
                    "id": "test-effect",
                    "name": "Test Effect",
                    "description": "A test effect with incomplete stat modifier.",
                    "category": "Buff",
                    "statModifiers": [
                        {
                            "statId": "attack",
                            "modifierType": "Flat"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing 'value' field in StatModifier
        errors.Should().NotBeEmpty("StatModifier missing 'value' should fail validation");
    }

    /// <summary>
    /// Verifies that effect IDs must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    [Test]
    public void EffectId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid effect ID (uppercase, spaces)
        var invalidJson = """
        {
            "effects": [
                {
                    "id": "Test Effect",
                    "name": "Test Effect",
                    "description": "A test effect with invalid ID pattern.",
                    "category": "Debuff"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("effect ID 'Test Effect' should fail pattern validation");
    }
}
