// ------------------------------------------------------------------------------
// <copyright file="VendorsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for vendors.schema.json validation.
// Verifies schema structure, specialty enum validation, restock type enum,
// operating hours validation, price modifier ranges, and
// backward compatibility with the existing vendors.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the vendors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates vendor NPC configuration files,
/// enforces specialty enums, restock types, operating hours, and price modifier constraints.
/// </summary>
/// <remarks>
/// <para>
/// The vendor schema validates configurations including:
/// <list type="bullet">
/// <item><description>Vendor IDs (kebab-case pattern)</description></item>
/// <item><description>Specialty enum (10 vendor types)</description></item>
/// <item><description>Inventory items with stock quantities</description></item>
/// <item><description>Price modifiers (multipliers, effects, maxDiscount)</description></item>
/// <item><description>Restock rule types (5 types)</description></item>
/// <item><description>Operating hours (0-23 range, day enums)</description></item>
/// <item><description>Required fields (id, name, description, specialty, inventory, priceModifiers)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class VendorsSchemaTests
{
    /// <summary>
    /// Path to the vendors schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/vendors.schema.json";

    /// <summary>
    /// Path to the actual vendors.json configuration file.
    /// </summary>
    private const string VendorsJsonPath = "../../../../../config/vendors.json";

    /// <summary>
    /// Loaded JSON Schema for vendor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the vendors schema.
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
    /// Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all required definitions are present (5 total).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 5 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Vendor Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (5 total)
        _schema.Definitions.Should().ContainKey("VendorDefinition", "should define VendorDefinition");
        _schema.Definitions.Should().ContainKey("InventoryItem", "should define InventoryItem");
        _schema.Definitions.Should().ContainKey("PriceModifiers", "should define PriceModifiers");
        _schema.Definitions.Should().ContainKey("RestockRule", "should define RestockRule");
        _schema.Definitions.Should().ContainKey("OperatingHours", "should define OperatingHours");
    }

    /// <summary>
    /// Verifies the existing vendors.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all 5 example vendors are valid.
    /// </summary>
    /// <remarks>
    /// The vendors.json contains 5 vendors covering specialties:
    /// Blacksmith, Alchemist, Innkeeper, Exotic, and General.
    /// </remarks>
    [Test]
    public async Task VendorsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual vendors.json file
        var jsonContent = await File.ReadAllTextAsync(VendorsJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing vendors.json should be valid
        errors.Should().BeEmpty(
            "existing vendors.json with 5 vendors should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that specialty must be one of the 10 valid enum values.
    /// Invalid specialty values should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid specialties: General, Blacksmith, Armorer, Alchemist, Enchanter,
    /// Jeweler, Tailor, Innkeeper, Provisioner, Exotic.
    /// </remarks>
    [Test]
    public void Specialty_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid specialty "WeaponSmith"
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "weapon-shop",
                    "name": "Weapon Shop",
                    "description": "A weapon shop.",
                    "specialty": "WeaponSmith",
                    "inventory": [],
                    "priceModifiers": {}
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid specialty enum value
        errors.Should().NotBeEmpty("specialty 'WeaponSmith' should fail validation (valid: General, Blacksmith, etc.)");
    }

    /// <summary>
    /// Verifies that restock rule type must be one of the 5 valid enum values.
    /// Invalid restock types should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid restock types: Timer, OnRest, OnLevelUp, Manual, Never.
    /// </remarks>
    [Test]
    public void RestockRuleType_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid restock type "Daily"
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [],
                    "priceModifiers": {},
                    "restockRule": {
                        "type": "Daily"
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid restock type enum value
        errors.Should().NotBeEmpty("restock type 'Daily' should fail validation (valid: Timer, OnRest, OnLevelUp, Manual, Never)");
    }

    /// <summary>
    /// Verifies that maxDiscount must be between 0 and 0.9.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Maximum discount cannot exceed 90% to prevent free items.
    /// </remarks>
    [Test]
    public void MaxDiscount_ExceedsMaximum_FailsValidation()
    {
        // Arrange: JSON with maxDiscount greater than 0.9 (95%)
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [],
                    "priceModifiers": {
                        "maxDiscount": 0.95
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to maxDiscount exceeding 0.9
        errors.Should().NotBeEmpty("maxDiscount 0.95 should fail validation (maximum is 0.9)");
    }

    /// <summary>
    /// Verifies that vendor ID must follow the kebab-case pattern.
    /// Invalid ID patterns should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void VendorId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid vendor ID (uppercase and space)
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "General Store",
                    "name": "General Store",
                    "description": "A general store.",
                    "specialty": "General",
                    "inventory": [],
                    "priceModifiers": {}
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("vendor ID 'General Store' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that required fields must be present.
    /// Missing required fields should fail validation.
    /// </summary>
    /// <remarks>
    /// Required fields: id, name, description, specialty, inventory, priceModifiers.
    /// </remarks>
    [Test]
    public void RequiredFields_Missing_FailsValidation()
    {
        // Arrange: JSON missing required field 'priceModifiers'
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": []
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing required field
        errors.Should().NotBeEmpty("missing required field 'priceModifiers' should fail validation");
    }

    /// <summary>
    /// Verifies that operating hours startHour must be between 0 and 23.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Operating hours use 24-hour format (0-23).
    /// </remarks>
    [Test]
    public void OperatingHours_StartHourOutOfRange_FailsValidation()
    {
        // Arrange: JSON with startHour of 25 (out of 0-23 range)
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [],
                    "priceModifiers": {},
                    "operatingHours": {
                        "startHour": 25,
                        "endHour": 18
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to startHour out of range
        errors.Should().NotBeEmpty("operatingHours startHour 25 should fail validation (must be 0-23)");
    }

    /// <summary>
    /// Verifies that closedDays must contain valid day names.
    /// Invalid day names should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid days: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday.
    /// </remarks>
    [Test]
    public void ClosedDays_InvalidDayName_FailsValidation()
    {
        // Arrange: JSON with invalid day name "Funday"
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [],
                    "priceModifiers": {},
                    "operatingHours": {
                        "startHour": 9,
                        "endHour": 17,
                        "closedDays": ["Funday"]
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid day name
        errors.Should().NotBeEmpty("closedDays 'Funday' should fail validation (valid: Monday, Tuesday, etc.)");
    }

    /// <summary>
    /// Verifies that stockQuantity must be at least -1.
    /// Values less than -1 should fail validation (only -1 means infinite).
    /// </summary>
    /// <remarks>
    /// Stock quantity: -1 = infinite, 0 = sold out, >0 = limited.
    /// </remarks>
    [Test]
    public void StockQuantity_LessThanMinusOne_FailsValidation()
    {
        // Arrange: JSON with stockQuantity of -2 (invalid, only -1 allowed for infinite)
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [
                        {
                            "itemId": "item-test",
                            "stockQuantity": -2
                        }
                    ],
                    "priceModifiers": {}
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to stockQuantity less than -1
        errors.Should().NotBeEmpty("stockQuantity -2 should fail validation (minimum is -1)");
    }

    /// <summary>
    /// Verifies that baseBuyMultiplier must be at least 0.1.
    /// Values less than 0.1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Price multipliers must be positive to avoid free or negative prices.
    /// </remarks>
    [Test]
    public void BaseBuyMultiplier_LessThanMinimum_FailsValidation()
    {
        // Arrange: JSON with baseBuyMultiplier of 0.05 (below 0.1 minimum)
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [],
                    "priceModifiers": {
                        "baseBuyMultiplier": 0.05
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to baseBuyMultiplier below minimum
        errors.Should().NotBeEmpty("baseBuyMultiplier 0.05 should fail validation (minimum is 0.1)");
    }

    /// <summary>
    /// Verifies that itemType must be one of the 3 valid enum values.
    /// Invalid item types should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid item types: Item, Weapon, Armor.
    /// </remarks>
    [Test]
    public void ItemType_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid itemType "Consumable"
        var invalidJson = """
        {
            "vendors": [
                {
                    "id": "shop",
                    "name": "Shop",
                    "description": "A shop.",
                    "specialty": "General",
                    "inventory": [
                        {
                            "itemId": "potion-health",
                            "itemType": "Consumable",
                            "stockQuantity": 5
                        }
                    ],
                    "priceModifiers": {}
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid itemType enum value
        errors.Should().NotBeEmpty("itemType 'Consumable' should fail validation (valid: Item, Weapon, Armor)");
    }
}
