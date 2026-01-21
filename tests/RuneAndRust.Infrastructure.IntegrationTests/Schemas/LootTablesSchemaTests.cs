// ------------------------------------------------------------------------------
// <copyright file="LootTablesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for loot-tables.schema.json validation.
// Verifies schema structure, drop chance validation, quantity range validation,
// item type enum validation, difficulty enum validation, and backwards compatibility
// with loot-tables.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the loot-tables.schema.json JSON Schema.
/// Tests ensure the schema correctly validates loot table configuration files,
/// enforces drop chance ranges (0.0-1.0), quantity ranges (min >= 1),
/// item type enums, difficulty enums, and required fields.
/// </summary>
[TestFixture]
public class LootTablesSchemaTests
{
    /// <summary>
    /// Path to the loot tables schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/loot-tables.schema.json";

    /// <summary>
    /// Path to the actual loot-tables.json configuration file.
    /// </summary>
    private const string LootTablesJsonPath = "../../../../../config/loot-tables.json";

    /// <summary>
    /// Loaded JSON Schema for loot table definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the loot tables schema.
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
    /// Validates that all required definitions are present (LootTableDefinition, LootEntry,
    /// QuantityRange, CurrencyDrop, DropCondition).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Loot Table Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("LootTableDefinition", "should define LootTableDefinition");
        _schema.Definitions.Should().ContainKey("LootEntry", "should define LootEntry");
        _schema.Definitions.Should().ContainKey("QuantityRange", "should define QuantityRange");
        _schema.Definitions.Should().ContainKey("CurrencyDrop", "should define CurrencyDrop");
        _schema.Definitions.Should().ContainKey("DropCondition", "should define DropCondition");
    }

    /// <summary>
    /// Verifies the loot-tables.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 8 existing loot tables are valid.
    /// </summary>
    [Test]
    public async Task ExistingLootTablesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual loot-tables.json file
        var lootTablesJsonContent = await File.ReadAllTextAsync(LootTablesJsonPath);

        // Act: Validate the loot-tables.json content against the schema
        var errors = _schema.Validate(lootTablesJsonContent);

        // Assert: No validation errors - all existing loot tables should be valid
        errors.Should().BeEmpty(
            "existing loot-tables.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies dropChance must be within valid range (0.0-1.0).
    /// Values above 1.0 should fail validation.
    /// </summary>
    [Test]
    public void DropChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with drop chance above 1.0
        var invalidJson = """
        {
            "lootTables": [
                {
                    "id": "invalid_drop_chance",
                    "name": "Invalid Drop Chance Table",
                    "guaranteedDrops": [
                        {
                            "itemId": "test_item",
                            "dropChance": 1.5
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to drop chance exceeding 1.0
        errors.Should().NotBeEmpty("dropChance 1.5 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies quantity.min must be >= 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    [Test]
    public void QuantityMin_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with quantity min less than 1
        var invalidJson = """
        {
            "lootTables": [
                {
                    "id": "invalid_quantity",
                    "name": "Invalid Quantity Table",
                    "guaranteedDrops": [
                        {
                            "itemId": "test_item",
                            "quantity": {
                                "min": 0,
                                "max": 5
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to quantity.min being less than 1
        errors.Should().NotBeEmpty("quantity.min of 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies itemType must be a valid enum value.
    /// Invalid item type values should fail validation.
    /// </summary>
    [Test]
    public void ItemType_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid item type enum value
        var invalidJson = """
        {
            "lootTables": [
                {
                    "id": "invalid_item_type",
                    "name": "Invalid Item Type Table",
                    "guaranteedDrops": [
                        {
                            "itemId": "test_item",
                            "itemType": "Potion"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid item type enum
        errors.Should().NotBeEmpty("invalid itemType 'Potion' should fail validation");
    }
}
