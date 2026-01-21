// ------------------------------------------------------------------------------
// <copyright file="AbilitySchemaExpansionTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for v0.14.1a abilities.schema.json expansion.
// Verifies schema structure, backwards compatibility with existing abilities.json,
// combo chain validation, prerequisites validation, AoE parameters conditional
// validation, and charge configuration validation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the expanded abilities.schema.json (v0.14.1a).
/// Tests ensure the schema correctly validates ability configurations with new features:
/// combo chains, prerequisites, AoE parameters, and charge-based abilities.
/// </summary>
[TestFixture]
public class AbilitySchemaExpansionTests
{
    /// <summary>
    /// Path to the abilities schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/abilities.schema.json";

    /// <summary>
    /// Path to the actual abilities.json configuration file.
    /// </summary>
    private const string AbilitiesJsonPath = "../../../../../config/abilities.json";

    /// <summary>
    /// Loaded JSON Schema for ability definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the abilities schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    /// <summary>
    /// Verifies the expanded schema file is valid JSON Schema Draft-07 with expected structure.
    /// Checks for all new definitions added in v0.14.1a.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Abilities Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        
        // Verify all definitions exist (existing + new)
        _schema.Definitions.Should().ContainKey("Ability", "should define Ability");
        _schema.Definitions.Should().ContainKey("Effect", "should define Effect");
        _schema.Definitions.Should().ContainKey("ComboChain", "should define ComboChain (v0.14.1a)");
        _schema.Definitions.Should().ContainKey("Prerequisites", "should define Prerequisites (v0.14.1a)");
        _schema.Definitions.Should().ContainKey("AoEParameters", "should define AoEParameters (v0.14.1a)");
        _schema.Definitions.Should().ContainKey("ChargeConfig", "should define ChargeConfig (v0.14.1a)");
    }

    /// <summary>
    /// Verifies the existing abilities.json configuration validates against the expanded schema
    /// without errors. This is the primary acceptance test ensuring backwards compatibility.
    /// </summary>
    [Test]
    public async Task ExistingAbilitiesJson_ValidatesAgainstExpandedSchema()
    {
        // Arrange: Load the actual abilities.json file
        var abilitiesJsonContent = await File.ReadAllTextAsync(AbilitiesJsonPath);

        // Act: Validate the abilities.json content against the expanded schema
        var errors = _schema.Validate(abilitiesJsonContent);

        // Assert: No validation errors - backwards compatibility confirmed
        errors.Should().BeEmpty(
            "existing abilities.json should validate against the expanded schema without errors");
    }

    /// <summary>
    /// Verifies that a valid combo chain configuration passes schema validation.
    /// Tests nextAbilityId pattern, comboWindowTurns minimum, and comboBonusDamage range.
    /// </summary>
    [Test]
    public void ComboChain_ValidConfiguration_PassesValidation()
    {
        // Arrange: JSON with valid combo chain configuration
        var validJson = """
        {
            "abilities": [
                {
                    "id": "slash",
                    "name": "Slash",
                    "description": "A basic sword slash that chains into double-slash.",
                    "classIds": ["shieldmaiden"],
                    "targetType": "SingleEnemy",
                    "comboChain": {
                        "nextAbilityId": "double-slash",
                        "comboWindowTurns": 1,
                        "comboBonusDamage": 0.25
                    }
                }
            ]
        }
        """;

        // Act: Validate the JSON
        var errors = _schema.Validate(validJson);

        // Assert: Should pass validation
        errors.Should().BeEmpty(
            "valid combo chain configuration should pass schema validation");
    }

    /// <summary>
    /// Verifies that a valid prerequisites configuration passes schema validation.
    /// Tests requiredLevel, requiredClassIds, requiredAbilityIds, and requiredArchetypeId.
    /// </summary>
    [Test]
    public void Prerequisites_ValidConfiguration_PassesValidation()
    {
        // Arrange: JSON with valid prerequisites configuration
        var validJson = """
        {
            "abilities": [
                {
                    "id": "whirlwind",
                    "name": "Whirlwind",
                    "description": "A spinning attack that hits all nearby enemies.",
                    "classIds": ["shieldmaiden", "berserker"],
                    "targetType": "AllEnemies",
                    "prerequisites": {
                        "requiredLevel": 10,
                        "requiredClassIds": ["shieldmaiden", "berserker"],
                        "requiredAbilityIds": ["slash", "cleave"],
                        "requiredArchetypeId": "warrior"
                    }
                }
            ]
        }
        """;

        // Act: Validate the JSON
        var errors = _schema.Validate(validJson);

        // Assert: Should pass validation
        errors.Should().BeEmpty(
            "valid prerequisites configuration should pass schema validation");
    }

    /// <summary>
    /// Verifies that a Cone ability with proper aoeParameters passes schema validation.
    /// Tests that spatial target types can be configured correctly with required parameters.
    /// </summary>
    [Test]
    public void ConeAbility_WithValidAoEParameters_PassesValidation()
    {
        // Arrange: JSON with Cone targetType and proper aoeParameters
        var validJson = """
        {
            "abilities": [
                {
                    "id": "dragon-breath",
                    "name": "Dragon Breath",
                    "description": "Breathe fire in a cone.",
                    "classIds": ["galdr-caster"],
                    "targetType": "Cone",
                    "aoeParameters": {
                        "shape": "Cone",
                        "radius": 4,
                        "coneAngle": 60,
                        "includeOrigin": false
                    }
                }
            ]
        }
        """;

        // Act: Validate the JSON
        var errors = _schema.Validate(validJson);

        // Assert: Should pass validation with proper Cone configuration
        errors.Should().BeEmpty(
            "Cone ability with valid aoeParameters should pass schema validation");
    }

    /// <summary>
    /// Verifies that a valid charge configuration passes schema validation.
    /// Tests maxCharges, chargeRegenTurns (required), and startingCharges (optional).
    /// </summary>
    [Test]
    public void ChargeConfig_ValidConfiguration_PassesValidation()
    {
        // Arrange: JSON with valid charge configuration
        var validJson = """
        {
            "abilities": [
                {
                    "id": "blink",
                    "name": "Blink",
                    "description": "Instantly teleport a short distance. Has 3 charges.",
                    "classIds": ["galdr-caster"],
                    "targetType": "None",
                    "charges": {
                        "maxCharges": 3,
                        "chargeRegenTurns": 2,
                        "startingCharges": 3
                    }
                }
            ]
        }
        """;

        // Act: Validate the JSON
        var errors = _schema.Validate(validJson);

        // Assert: Should pass validation
        errors.Should().BeEmpty(
            "valid charge configuration should pass schema validation");
    }
}
