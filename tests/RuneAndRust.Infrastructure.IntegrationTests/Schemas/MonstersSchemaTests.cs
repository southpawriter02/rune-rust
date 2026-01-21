// ------------------------------------------------------------------------------
// <copyright file="MonstersSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for monsters.schema.json validation.
// Verifies schema structure, stat block validation, AI behavior enum validation,
// resistance range validation, embedded loot table validation, and backwards 
// compatibility with existing monsters.json configuration.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the monsters.schema.json JSON Schema.
/// Tests ensure the schema correctly validates monster configuration files,
/// enforces base stat ranges (health >= 1, attack/defense >= 0),
/// behavior enum values, resistance ranges (-100 to 100), embedded loot table
/// structure, and required fields. These tests support the v0.14.3a Monster Schema
/// implementation for comprehensive monster customization.
/// </summary>
[TestFixture]
public class MonstersSchemaTests
{
    /// <summary>
    /// Path to the monsters schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/monsters.schema.json";

    /// <summary>
    /// Path to the actual monsters.json configuration file.
    /// </summary>
    private const string MonstersJsonPath = "../../../../../config/monsters.json";

    /// <summary>
    /// Loaded JSON Schema for monster definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the monsters schema.
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
    /// Validates that all required definitions are present (MonsterDefinition, NameGenerator,
    /// EmbeddedLootTable, LootEntry, CurrencyDrop, LevelRange).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Monster Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present for monster configuration
        _schema.Definitions.Should().ContainKey("MonsterDefinition", "should define MonsterDefinition");
        _schema.Definitions.Should().ContainKey("NameGenerator", "should define NameGenerator for procedural naming");
        _schema.Definitions.Should().ContainKey("EmbeddedLootTable", "should define EmbeddedLootTable for inline drops");
        _schema.Definitions.Should().ContainKey("LootEntry", "should define LootEntry for item drops");
        _schema.Definitions.Should().ContainKey("CurrencyDrop", "should define CurrencyDrop for gold drops");
        _schema.Definitions.Should().ContainKey("LevelRange", "should define LevelRange for spawn restrictions");
    }

    /// <summary>
    /// Verifies the monsters.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 5 existing monsters (goblin, skeleton, orc, goblin_shaman, slime) are valid.
    /// </summary>
    [Test]
    public async Task ExistingMonstersJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual monsters.json file
        var monstersJsonContent = await File.ReadAllTextAsync(MonstersJsonPath);

        // Act: Validate the monsters.json content against the schema
        var errors = _schema.Validate(monstersJsonContent);

        // Assert: No validation errors - all existing monsters should be valid
        errors.Should().BeEmpty(
            "existing monsters.json should validate against the schema without errors. All 5 monsters (goblin, skeleton, orc, goblin_shaman, slime) must pass validation.");
    }

    /// <summary>
    /// Verifies baseHealth must be >= 1.
    /// A health value less than 1 should fail validation as monsters must have
    /// at least 1 hit point to exist in combat.
    /// </summary>
    [Test]
    public void BaseHealth_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with baseHealth of 0 (invalid - minimum is 1)
        var invalidJson = """
        {
            "monsters": [
                {
                    "id": "invalid_health_monster",
                    "name": "Invalid Monster",
                    "description": "A monster with invalid health.",
                    "baseHealth": 0,
                    "baseAttack": 5,
                    "baseDefense": 2,
                    "experienceValue": 10,
                    "behavior": "Aggressive",
                    "tags": ["beast"],
                    "possibleTiers": ["common"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to baseHealth being less than 1
        errors.Should().NotBeEmpty("baseHealth of 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies behavior must be a valid enum value.
    /// Only Aggressive, Defensive, Cowardly, Support, Chaotic, and Tactical are valid.
    /// Invalid behavior values should fail validation.
    /// </summary>
    [Test]
    public void Behavior_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid behavior enum value
        var invalidJson = """
        {
            "monsters": [
                {
                    "id": "invalid_behavior_monster",
                    "name": "Invalid Behavior Monster",
                    "description": "A monster with invalid behavior.",
                    "baseHealth": 30,
                    "baseAttack": 8,
                    "baseDefense": 2,
                    "experienceValue": 25,
                    "behavior": "Passive",
                    "tags": ["humanoid"],
                    "possibleTiers": ["common"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid behavior enum
        errors.Should().NotBeEmpty(
            "behavior 'Passive' should fail validation (valid values: Aggressive, Defensive, Cowardly, Support, Chaotic, Tactical)");
    }

    /// <summary>
    /// Verifies resistance values must be between -100 and 100.
    /// -100 represents double damage (weakness), 0 is normal damage,
    /// and 100 represents immunity. Values outside this range should fail.
    /// </summary>
    [Test]
    public void Resistance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with resistance value exceeding 100 (invalid maximum)
        var invalidJson = """
        {
            "monsters": [
                {
                    "id": "invalid_resistance_monster",
                    "name": "Invalid Resistance Monster",
                    "description": "A monster with invalid resistance values.",
                    "baseHealth": 50,
                    "baseAttack": 10,
                    "baseDefense": 5,
                    "experienceValue": 30,
                    "behavior": "Defensive",
                    "tags": ["construct"],
                    "baseResistances": {
                        "fire": 150
                    },
                    "possibleTiers": ["common", "elite"]
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
    /// Verifies embedded loot table structure is validated correctly.
    /// Tests that a valid loot table with entries and currency drops passes validation.
    /// Also verifies that the loot table structure follows the EmbeddedLootTable definition.
    /// </summary>
    [Test]
    public void EmbeddedLootTable_ValidStructure_PassesValidation()
    {
        // Arrange: JSON with valid embedded loot table structure
        var validJson = """
        {
            "monsters": [
                {
                    "id": "valid_loot_monster",
                    "name": "Loot Monster",
                    "description": "A monster with a valid loot table.",
                    "baseHealth": 40,
                    "baseAttack": 8,
                    "baseDefense": 3,
                    "experienceValue": 20,
                    "behavior": "Aggressive",
                    "tags": ["beast"],
                    "possibleTiers": ["common", "named"],
                    "lootTable": {
                        "entries": [
                            {
                                "itemId": "health_potion",
                                "weight": 100,
                                "minQuantity": 1,
                                "maxQuantity": 2,
                                "dropChance": 0.25
                            },
                            {
                                "itemId": "gold_ring",
                                "weight": 50,
                                "dropChance": 0.10
                            }
                        ],
                        "currencyDrops": [
                            {
                                "currencyId": "gold",
                                "minAmount": 10,
                                "maxAmount": 25,
                                "dropChance": 0.90
                            }
                        ]
                    }
                }
            ]
        }
        """;

        // Act: Validate the valid JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: No validation errors - valid loot table structure
        errors.Should().BeEmpty(
            "valid embedded loot table with entries and currency drops should pass validation");
    }

    /// <summary>
    /// Verifies that dropChance in loot entries must be within valid range (0.0-1.0).
    /// Values above 1.0 should fail validation as they represent invalid probabilities.
    /// </summary>
    [Test]
    public void LootEntry_DropChanceOutOfRange_FailsValidation()
    {
        // Arrange: JSON with drop chance above 1.0 in loot entry
        var invalidJson = """
        {
            "monsters": [
                {
                    "id": "invalid_drop_chance_monster",
                    "name": "Invalid Drop Chance Monster",
                    "description": "A monster with invalid drop chance.",
                    "baseHealth": 35,
                    "baseAttack": 7,
                    "baseDefense": 2,
                    "experienceValue": 15,
                    "behavior": "Chaotic",
                    "tags": ["ooze"],
                    "possibleTiers": ["common"],
                    "lootTable": {
                        "entries": [
                            {
                                "itemId": "slime_residue",
                                "dropChance": 1.5
                            }
                        ]
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to drop chance exceeding 1.0
        errors.Should().NotBeEmpty("dropChance 1.5 should fail validation (maximum is 1.0)");
    }
}
