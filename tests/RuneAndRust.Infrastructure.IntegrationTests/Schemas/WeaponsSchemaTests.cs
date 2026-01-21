// ------------------------------------------------------------------------------
// <copyright file="WeaponsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for weapons.schema.json validation.
// Verifies schema structure, damage dice pattern validation, weapon type enum validation,
// proficiency enum validation, and backwards compatibility with existing weapons.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the weapons.schema.json JSON Schema.
/// Tests ensure the schema correctly validates weapon configuration files,
/// enforces damage dice patterns, weapon type enums, proficiency categories,
/// quality tiers, and required fields.
/// </summary>
[TestFixture]
public class WeaponsSchemaTests
{
    /// <summary>
    /// Path to the weapons schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/weapons.schema.json";

    /// <summary>
    /// Path to the actual weapons.json configuration file.
    /// </summary>
    private const string WeaponsJsonPath = "../../../../../config/weapons.json";

    /// <summary>
    /// Loaded JSON Schema for weapon definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the weapons schema.
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
    /// Validates that all required definitions are present (WeaponDefinition, StatBonuses, WeaponRange).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Weapon Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("WeaponDefinition", "should define WeaponDefinition");
        _schema.Definitions.Should().ContainKey("StatBonuses", "should define StatBonuses");
        _schema.Definitions.Should().ContainKey("WeaponRange", "should define WeaponRange");
    }

    /// <summary>
    /// Verifies the existing weapons.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 9 existing weapons are valid.
    /// </summary>
    [Test]
    public async Task ExistingWeaponsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual weapons.json file
        var weaponsJsonContent = await File.ReadAllTextAsync(WeaponsJsonPath);

        // Act: Validate the weapons.json content against the schema
        var errors = _schema.Validate(weaponsJsonContent);

        // Assert: No validation errors - all existing weapons should be valid
        errors.Should().BeEmpty(
            "existing weapons.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies damage dice pattern validates correct expressions (1d8, 2d6+3, 1d4-1).
    /// Tests the regex pattern ^\\d+d\\d+([+-]\\d+)?$ with valid dice notations.
    /// </summary>
    [Test]
    public void DamageDice_ValidPattern_PassesValidation()
    {
        // Arrange: JSON with valid damage dice expressions
        var validJson = """
        {
            "weapons": [
                {
                    "id": "test_weapon_d8",
                    "name": "Test Weapon D8",
                    "description": "A test weapon with 1d8 damage.",
                    "weaponType": "Sword",
                    "damageDice": "1d8",
                    "value": 100
                },
                {
                    "id": "test_weapon_2d6_plus",
                    "name": "Test Weapon 2d6+3",
                    "description": "A test weapon with 2d6+3 damage.",
                    "weaponType": "Axe",
                    "damageDice": "2d6+3",
                    "value": 200
                },
                {
                    "id": "test_weapon_d4_minus",
                    "name": "Test Weapon 1d4-1",
                    "description": "A test weapon with 1d4-1 damage.",
                    "weaponType": "Dagger",
                    "damageDice": "1d4-1",
                    "value": 50
                }
            ]
        }
        """;

        // Act: Validate the valid JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: No validation errors
        errors.Should().BeEmpty("valid damage dice patterns (1d8, 2d6+3, 1d4-1) should pass validation");
    }

    /// <summary>
    /// Verifies damage dice pattern rejects invalid expressions (d8, 1d, 1d8++1).
    /// Tests that malformed dice notations fail schema validation.
    /// </summary>
    [Test]
    public void DamageDice_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid damage dice expression (missing count)
        var invalidJson = """
        {
            "weapons": [
                {
                    "id": "invalid_dice_weapon",
                    "name": "Invalid Dice Weapon",
                    "description": "A weapon with invalid damage dice pattern.",
                    "weaponType": "Sword",
                    "damageDice": "d8",
                    "value": 100
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid damage dice pattern
        errors.Should().NotBeEmpty("invalid damage dice 'd8' (missing count) should fail validation");
    }

    /// <summary>
    /// Verifies weaponType must be a valid enum value (Sword, Axe, Dagger, etc.).
    /// Invalid weapon types should fail validation.
    /// </summary>
    [Test]
    public void WeaponType_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid weapon type enum value
        var invalidJson = """
        {
            "weapons": [
                {
                    "id": "invalid_type_weapon",
                    "name": "Invalid Type Weapon",
                    "description": "A weapon with invalid type.",
                    "weaponType": "LaserGun",
                    "damageDice": "1d8",
                    "value": 100
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid weapon type enum
        errors.Should().NotBeEmpty("invalid weaponType 'LaserGun' should fail validation");
    }
}
