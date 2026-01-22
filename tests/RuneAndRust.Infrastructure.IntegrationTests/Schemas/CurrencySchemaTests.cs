// ------------------------------------------------------------------------------
// <copyright file="CurrencySchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for currency.schema.json validation.
// Verifies schema structure, symbol position enum validation, decimal places range,
// exchange rate minimum validation, max amount validation, and backward compatibility
// with the existing currency.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the currency.schema.json JSON Schema.
/// Tests ensure the schema correctly validates currency configuration files,
/// enforces symbol position enums, decimal places ranges, exchange rate minimums,
/// and max amount constraints.
/// </summary>
/// <remarks>
/// <para>
/// The currency schema validates configurations including:
/// <list type="bullet">
/// <item><description>Currency IDs (kebab-case pattern)</description></item>
/// <item><description>Symbol length (1-3 characters)</description></item>
/// <item><description>Symbol positions (Before, After)</description></item>
/// <item><description>Decimal places (0-4 range)</description></item>
/// <item><description>Exchange rates (minimum 0.001)</description></item>
/// <item><description>Max amount caps (minimum 1)</description></item>
/// <item><description>Required fields (id, name, symbol, color)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class CurrencySchemaTests
{
    /// <summary>
    /// Path to the currency schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/currency.schema.json";

    /// <summary>
    /// Path to the actual currency.json configuration file.
    /// </summary>
    private const string CurrencyJsonPath = "../../../../../config/currency.json";

    /// <summary>
    /// Loaded JSON Schema for currency definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the currency schema.
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
    /// Validates that all required definitions are present (2 total).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 2 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Currency Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (2 total)
        _schema.Definitions.Should().ContainKey("CurrencyDefinition", "should define CurrencyDefinition");
        _schema.Definitions.Should().ContainKey("ExchangeRate", "should define ExchangeRate");
    }

    /// <summary>
    /// Verifies the existing currency.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// the existing gold currency definition is valid.
    /// </summary>
    /// <remarks>
    /// The existing currency.json contains 1 currency (gold) with:
    /// id, name, pluralName, symbol, color, sortOrder.
    /// </remarks>
    [Test]
    public async Task CurrencyJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual currency.json file
        var jsonContent = await File.ReadAllTextAsync(CurrencyJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing currency.json should be valid
        errors.Should().BeEmpty(
            "existing currency.json with gold currency should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that symbolPosition must be one of the valid enum values.
    /// Invalid symbol positions should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid symbol positions: Before, After.
    /// Before: G100, After: 100G.
    /// </remarks>
    [Test]
    public void SymbolPosition_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid symbol position "Left"
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "gold",
                    "name": "Gold",
                    "symbol": "G",
                    "symbolPosition": "Left",
                    "color": "yellow"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid symbol position enum value
        errors.Should().NotBeEmpty("invalid symbolPosition 'Left' should fail validation (valid: Before, After)");
    }

    /// <summary>
    /// Verifies that decimalPlaces must be between 0 and 4 (inclusive).
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Decimal places range:
    /// <list type="bullet">
    /// <item><description>0: Whole numbers (gold, silver)</description></item>
    /// <item><description>2: Standard monetary (dollars)</description></item>
    /// <item><description>4: Ultra precision (crypto)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void DecimalPlaces_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with decimal places outside valid range (5 > 4)
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "gold",
                    "name": "Gold",
                    "symbol": "G",
                    "color": "yellow",
                    "decimalPlaces": 5
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range decimal places
        errors.Should().NotBeEmpty("decimalPlaces 5 should fail validation (maximum is 4)");
    }

    /// <summary>
    /// Verifies that exchange rate must be at least 0.001.
    /// Values less than 0.001 should fail validation.
    /// </summary>
    /// <remarks>
    /// Minimum rate prevents:
    /// <list type="bullet">
    /// <item><description>Zero rates (division by zero)</description></item>
    /// <item><description>Near-zero rates (impractical conversions)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void ExchangeRate_BelowMinimum_FailsValidation()
    {
        // Arrange: JSON with exchange rate below minimum (0.0001 < 0.001)
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "gold",
                    "name": "Gold",
                    "symbol": "G",
                    "color": "yellow",
                    "exchangeRates": [
                        {
                            "targetCurrencyId": "silver",
                            "rate": 0.0001
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to rate below minimum
        errors.Should().NotBeEmpty("exchange rate 0.0001 should fail validation (minimum is 0.001)");
    }

    /// <summary>
    /// Verifies that maxAmount must be at least 1.
    /// Values less than 1 (including 0 and negative) should fail validation.
    /// </summary>
    /// <remarks>
    /// Maximum amount must be positive to allow at least 1 unit of currency.
    /// </remarks>
    [Test]
    public void MaxAmount_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with maxAmount less than minimum (0 < 1)
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "gold",
                    "name": "Gold",
                    "symbol": "G",
                    "color": "yellow",
                    "maxAmount": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to maxAmount less than minimum
        errors.Should().NotBeEmpty("maxAmount 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that currency ID must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// <item><description>Valid examples: gold, silver, arena-tokens</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void CurrencyId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid currency ID (uppercase, spaces)
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "Gold Coins",
                    "name": "Gold Coins",
                    "symbol": "G",
                    "color": "yellow"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("currency ID 'Gold Coins' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that symbol must be 1-3 characters.
    /// Symbols with more than 3 characters should fail validation.
    /// </summary>
    /// <remarks>
    /// Symbol length allows:
    /// <list type="bullet">
    /// <item><description>1 char: G, $, €</description></item>
    /// <item><description>2 char: CR, GP</description></item>
    /// <item><description>3 char: BTC, USD</description></item>
    /// <item><description>Emoji: 💎 (counts as 1)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Symbol_TooLong_FailsValidation()
    {
        // Arrange: JSON with symbol longer than maximum (GOLD = 4 > 3)
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "gold",
                    "name": "Gold",
                    "symbol": "GOLD",
                    "color": "yellow"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to symbol too long
        errors.Should().NotBeEmpty("symbol 'GOLD' (4 chars) should fail validation (maximum is 3)");
    }

    /// <summary>
    /// Verifies that required fields must be present.
    /// Missing required fields should fail validation.
    /// </summary>
    /// <remarks>
    /// Required fields: id, name, symbol, color.
    /// </remarks>
    [Test]
    public void RequiredFields_Missing_FailsValidation()
    {
        // Arrange: JSON missing required field 'color'
        var invalidJson = """
        {
            "currencies": [
                {
                    "id": "gold",
                    "name": "Gold",
                    "symbol": "G"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing required field
        errors.Should().NotBeEmpty("missing required field 'color' should fail validation");
    }
}
