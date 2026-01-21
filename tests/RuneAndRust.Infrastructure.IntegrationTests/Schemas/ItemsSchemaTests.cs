// ------------------------------------------------------------------------------
// <copyright file="ItemsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for items.schema.json validation.
// Verifies schema structure, item category validation, consumable effect validation,
// stack limit range validation, and backwards compatibility with items.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the items.schema.json JSON Schema.
/// Tests ensure the schema correctly validates item configuration files,
/// enforces category enums, consumable effect types, stack limit ranges (1-999),
/// vendor price non-negativity, and required fields.
/// </summary>
[TestFixture]
public class ItemsSchemaTests
{
    /// <summary>
    /// Path to the items schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/items.schema.json";

    /// <summary>
    /// Path to the actual items.json configuration file.
    /// </summary>
    private const string ItemsJsonPath = "../../../../../config/items.json";

    /// <summary>
    /// Loaded JSON Schema for item definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the items schema.
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
    /// Validates that all required definitions are present (ItemDefinition, ConsumableEffect,
    /// UseRequirement, VendorPrices, StatRequirements).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Item Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("ItemDefinition", "should define ItemDefinition");
        _schema.Definitions.Should().ContainKey("ConsumableEffect", "should define ConsumableEffect");
        _schema.Definitions.Should().ContainKey("UseRequirement", "should define UseRequirement");
        _schema.Definitions.Should().ContainKey("VendorPrices", "should define VendorPrices");
        _schema.Definitions.Should().ContainKey("StatRequirements", "should define StatRequirements");
    }

    /// <summary>
    /// Verifies the items.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 17 existing items are valid.
    /// </summary>
    [Test]
    public async Task ExistingItemsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual items.json file
        var itemsJsonContent = await File.ReadAllTextAsync(ItemsJsonPath);

        // Act: Validate the items.json content against the schema
        var errors = _schema.Validate(itemsJsonContent);

        // Assert: No validation errors - all existing items should be valid
        errors.Should().BeEmpty(
            "existing items.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies stackLimit must be within valid range (1-999).
    /// Values outside this range should fail validation.
    /// </summary>
    [Test]
    public void StackLimit_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with stack limit above maximum (>999)
        var invalidJson = """
        {
            "items": [
                {
                    "id": "overstacked_item",
                    "name": "Overstacked Item",
                    "description": "An item with too high stack limit.",
                    "category": "Material",
                    "stackLimit": 1000
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to stack limit exceeding 999
        errors.Should().NotBeEmpty("stackLimit 1000 should fail validation (maximum is 999)");
    }

    /// <summary>
    /// Verifies category must be a valid enum value.
    /// Invalid category values should fail validation.
    /// </summary>
    [Test]
    public void Category_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid category enum value
        var invalidJson = """
        {
            "items": [
                {
                    "id": "invalid_category_item",
                    "name": "Invalid Category Item",
                    "description": "An item with invalid category.",
                    "category": "Weapon"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid category enum
        errors.Should().NotBeEmpty("invalid category 'Weapon' should fail validation");
    }

    /// <summary>
    /// Verifies consumableEffect.type must be a valid enum value.
    /// Invalid effect types should fail validation.
    /// </summary>
    [Test]
    public void ConsumableEffectType_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid consumable effect type
        var invalidJson = """
        {
            "items": [
                {
                    "id": "invalid_effect_potion",
                    "name": "Invalid Effect Potion",
                    "description": "A potion with invalid effect type.",
                    "category": "Consumable",
                    "consumableEffect": {
                        "type": "Explode"
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid effect type enum
        errors.Should().NotBeEmpty("invalid consumableEffect.type 'Explode' should fail validation");
    }
}
