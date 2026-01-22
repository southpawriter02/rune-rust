// ------------------------------------------------------------------------------
// <copyright file="TerrainSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for terrain.schema.json validation (expanded version).
// Verifies schema structure, cover level enum validation, visibility modifier range,
// hazard effect trigger type validation, saving throw attribute validation,
// status effect chance range, and backward compatibility with existing terrain.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the expanded terrain.schema.json JSON Schema.
/// Tests ensure the schema correctly validates terrain configuration files,
/// enforces cover level enums, visibility modifier ranges, hazard effects with
/// saving throws, and status effect application.
/// </summary>
/// <remarks>
/// <para>
/// The terrain schema validates configurations including:
/// <list type="bullet">
/// <item><description>Cover levels (None, Half, ThreeQuarters, Full)</description></item>
/// <item><description>Visibility modifiers (0.0 to 1.0 range)</description></item>
/// <item><description>Concealment for stealth mechanics</description></item>
/// <item><description>Hazard effects with trigger types and saving throws</description></item>
/// <item><description>Status effect application with chance</description></item>
/// <item><description>Terrain tags and biome associations</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class TerrainSchemaTests
{
    /// <summary>
    /// Path to the terrain schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/terrain.schema.json";

    /// <summary>
    /// Path to the actual terrain.json configuration file.
    /// </summary>
    private const string TerrainJsonPath = "../../../../../config/terrain.json";

    /// <summary>
    /// Loaded JSON Schema for terrain definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the terrain schema.
    /// </summary>
    /// <remarks>
    /// Loads the schema from the file system before each test execution.
    /// The schema is validated during loading to ensure it is valid JSON Schema Draft-07.
    /// </remarks>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    /// <summary>
    /// Verifies the expanded schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all required definitions are present (TerrainDefinition, HazardEffect,
    /// SavingThrow).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 3 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Terrain Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (3 total)
        _schema.Definitions.Should().ContainKey("TerrainDefinition", "should define TerrainDefinition");
        _schema.Definitions.Should().ContainKey("HazardEffect", "should define HazardEffect");
        _schema.Definitions.Should().ContainKey("SavingThrow", "should define SavingThrow");
    }

    /// <summary>
    /// Verifies the existing terrain.json configuration validates against the expanded schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 10 existing terrain types are valid with the new optional properties.
    /// </summary>
    /// <remarks>
    /// This test ensures that the expanded schema:
    /// <list type="bullet">
    /// <item><description>Validates all required fields for each terrain type</description></item>
    /// <item><description>Accepts existing terrain without new optional properties</description></item>
    /// <item><description>Maintains backward compatibility with legacy damageOnEntry field</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public async Task TerrainJson_ValidatesAgainstExpandedSchema()
    {
        // Arrange: Load the actual terrain.json file
        var terrainJsonContent = await File.ReadAllTextAsync(TerrainJsonPath);

        // Act: Validate the terrain.json content against the expanded schema
        var errors = _schema.Validate(terrainJsonContent);

        // Assert: No validation errors - all 10 existing terrain types should be valid
        errors.Should().BeEmpty(
            "existing terrain.json with 10 terrain types should validate against expanded schema without errors");
    }

    /// <summary>
    /// Verifies that coverLevel must be one of the valid enum values.
    /// Invalid values should fail validation.
    /// </summary>
    /// <remarks>
    /// Cover levels affect combat defense:
    /// <list type="bullet">
    /// <item><description>None: +0 defense (open ground)</description></item>
    /// <item><description>Half: +2 defense (low wall, crates)</description></item>
    /// <item><description>ThreeQuarters: +4 defense (pillar, corner)</description></item>
    /// <item><description>Full: +6 defense or blocked (solid wall)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void CoverLevel_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid cover level value "Quarter"
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Normal",
                    "coverLevel": "Quarter"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid cover level enum value
        errors.Should().NotBeEmpty("invalid coverLevel 'Quarter' should fail validation (valid: None, Half, ThreeQuarters, Full)");
    }

    /// <summary>
    /// Verifies that visibilityModifier must be between 0 and 1 (inclusive).
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Visibility modifier affects line of sight:
    /// <list type="bullet">
    /// <item><description>1.0: Full visibility (clear floor)</description></item>
    /// <item><description>0.8: Slight obscuring (fire, light mist)</description></item>
    /// <item><description>0.5: 50% visibility (heavy mist)</description></item>
    /// <item><description>0.0: No visibility (solid wall)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void VisibilityModifier_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with visibility modifier outside valid range (1.5 > 1.0)
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Normal",
                    "visibilityModifier": 1.5
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range visibility modifier
        errors.Should().NotBeEmpty("visibilityModifier 1.5 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies that hazardEffect.triggerType must be one of the valid enum values.
    /// Invalid trigger types should fail validation.
    /// </summary>
    /// <remarks>
    /// Trigger types define when hazard damage is applied:
    /// <list type="bullet">
    /// <item><description>OnEntry: When stepping onto tile (spike trap, fire)</description></item>
    /// <item><description>PerTurn: Each turn while on tile (lava, acid pool)</description></item>
    /// <item><description>OnExit: When leaving tile (tar pit, web)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void HazardEffectTriggerType_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid hazard trigger type "OnTouch"
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Hazardous",
                    "hazardEffect": {
                        "triggerType": "OnTouch",
                        "damage": "1d6",
                        "damageType": "fire"
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid trigger type enum value
        errors.Should().NotBeEmpty("invalid triggerType 'OnTouch' should fail validation (valid: OnEntry, PerTurn, OnExit)");
    }

    /// <summary>
    /// Verifies that savingThrow.attribute must be one of the valid enum values.
    /// Invalid attribute values should fail validation.
    /// </summary>
    /// <remarks>
    /// Saving throw attributes:
    /// <list type="bullet">
    /// <item><description>might: Physical hazards requiring brute force</description></item>
    /// <item><description>fortitude: Resisting poison, disease, temperatures</description></item>
    /// <item><description>will: Mental effects, fear, psychic damage</description></item>
    /// <item><description>wits: Noticing traps, quick reactions</description></item>
    /// <item><description>finesse: Dodging, acrobatic avoidance</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void SavingThrowAttribute_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid saving throw attribute "dexterity"
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Hazardous",
                    "hazardEffect": {
                        "triggerType": "OnEntry",
                        "damage": "1d6",
                        "damageType": "fire",
                        "savingThrow": {
                            "attribute": "dexterity",
                            "dc": 12
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid attribute enum value
        errors.Should().NotBeEmpty("invalid attribute 'dexterity' should fail validation (valid: might, fortitude, will, wits, finesse)");
    }

    /// <summary>
    /// Verifies that statusEffectChance must be between 0 and 1 (inclusive).
    /// Values outside this probability range should fail validation.
    /// </summary>
    /// <remarks>
    /// Status effect chance values:
    /// <list type="bullet">
    /// <item><description>1.0: 100% - Guaranteed effect (lava → burning)</description></item>
    /// <item><description>0.5: 50% - Moderate chance (fire → burning)</description></item>
    /// <item><description>0.0: Never - Effect disabled</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void StatusEffectChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with status effect chance outside valid range (1.5 > 1.0)
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Hazardous",
                    "appliesStatusEffect": "burning",
                    "statusEffectChance": 1.5
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range status effect chance
        errors.Should().NotBeEmpty("statusEffectChance 1.5 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies that savingThrow.dc must be at least 1.
    /// Values less than 1 (including 0 and negative) should fail validation.
    /// </summary>
    /// <remarks>
    /// Difficulty class must be positive:
    /// <list type="bullet">
    /// <item><description>DC 1+: Valid difficulty class</description></item>
    /// <item><description>DC 0 or negative: Invalid</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void SavingThrowDC_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with saving throw DC less than minimum (0 < 1)
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Hazardous",
                    "hazardEffect": {
                        "triggerType": "OnEntry",
                        "damage": "1d6",
                        "damageType": "fire",
                        "savingThrow": {
                            "attribute": "finesse",
                            "dc": 0
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to dc less than minimum
        errors.Should().NotBeEmpty("savingThrow.dc 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that terrain ID must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// <item><description>Valid examples: fire, acid-pool, spike-trap-1</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void TerrainId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid terrain ID (uppercase, spaces)
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "Fire Terrain",
                    "name": "Fire Terrain",
                    "type": "Hazardous"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("terrain ID 'Fire Terrain' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that hazardEffect.damage must follow the dice expression pattern.
    /// Invalid dice expressions should fail validation.
    /// </summary>
    /// <remarks>
    /// Dice expression pattern: ^[0-9]+d[0-9]+([+-][0-9]+)?$
    /// <list type="bullet">
    /// <item><description>Valid: 1d6, 2d4, 1d8+2, 2d6-1</description></item>
    /// <item><description>Invalid: d6, 1d, damage, 1d6+</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void HazardDamage_InvalidDicePattern_FailsValidation()
    {
        // Arrange: JSON with invalid dice expression "damage"
        var invalidJson = """
        {
            "terrainDefinitions": [
                {
                    "id": "test-terrain",
                    "name": "Test Terrain",
                    "type": "Hazardous",
                    "hazardEffect": {
                        "triggerType": "OnEntry",
                        "damage": "damage",
                        "damageType": "fire"
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid dice expression pattern
        errors.Should().NotBeEmpty("damage 'damage' should fail pattern validation (must be dice expression like 1d6)");
    }
}
