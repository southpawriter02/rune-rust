// ------------------------------------------------------------------------------
// <copyright file="DiceTypesSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for dice-types.schema.json validation.
// Verifies schema structure, die type definitions, faces validation,
// die type ID pattern validation, expression pattern validation, and color hex validation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;
using System.Text.RegularExpressions;

/// <summary>
/// Unit tests validating the dice-types.schema.json JSON Schema.
/// Tests ensure the schema correctly validates dice type configuration files,
/// enforces die type ID patterns, faces constraints, value ranges,
/// and dice expression pattern validation.
/// </summary>
/// <remarks>
/// <para>
/// The dice types schema provides:
/// <list type="bullet">
/// <item><description>DieType definition with core properties (id, name, faces)</description></item>
/// <item><description>Computed properties (minValue, maxValue, average)</description></item>
/// <item><description>Display properties (color, iconId, description)</description></item>
/// <item><description>Classification properties (isStandard, sortOrder)</description></item>
/// <item><description>DiceExpression definition for parsed expressions</description></item>
/// <item><description>DiceExpressionPattern for regex validation</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class DiceTypesSchemaTests
{
    /// <summary>
    /// Path to the dice types schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/dice-types.schema.json";

    /// <summary>
    /// Path to the dice-types.json configuration file.
    /// </summary>
    private const string DiceTypesJsonPath = "../../../../../config/dice-types.json";

    /// <summary>
    /// Loaded JSON Schema for dice type definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the dice types schema.
    /// </summary>
    /// <remarks>
    /// Loads the schema from the file system before each test execution.
    /// Uses FromFileAsync with full path to properly resolve $ref references.
    /// The schema is validated during loading to ensure it is valid JSON Schema Draft-07.
    /// </remarks>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system with full path for reference resolution
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region DTS-001: Valid Dice Types Configuration

    /// <summary>
    /// DTS-001: Verifies that a complete dice types configuration with all fields
    /// populated correctly passes schema validation.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>All required definitions are present (3 total)</description></item>
    /// <item><description>dice-types.json validates against schema</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public async Task DiceTypesSchema_ValidConfiguration_PassesValidation()
    {
        // Arrange: Load the actual dice-types.json file
        var jsonContent = await File.ReadAllTextAsync(DiceTypesJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Dice Types Configuration Schema", "schema title should match");
        _schema.Definitions.Should().ContainKey("DieType", "should define DieType");
        _schema.Definitions.Should().ContainKey("DiceExpression", "should define DiceExpression");
        _schema.Definitions.Should().ContainKey("DiceExpressionPattern", "should define DiceExpressionPattern");
        errors.Should().BeEmpty(
            "existing dice-types.json with 7 standard dice should validate against schema without errors");
    }

    #endregion

    #region DTS-002: Die Type Structure Validation

    /// <summary>
    /// DTS-002: Verifies that die type structure validates required and optional fields correctly.
    /// Minimal die type with only required fields should pass.
    /// </summary>
    [Test]
    public void DieTypeStructure_MinimalFields_PassesValidation()
    {
        // Arrange: Minimal die type with only required fields
        var validJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimal die type
        errors.Should().BeEmpty(
            "minimal die type with only required fields (id, name, faces) should validate successfully");
    }

    /// <summary>
    /// DTS-002: Verifies that die type with optional fields passes validation.
    /// </summary>
    [Test]
    public void DieTypeStructure_WithOptionalFields_PassesValidation()
    {
        // Arrange: Die type with optional fields
        var validJson = """
        {
            "dieTypes": [
                {
                    "id": "d8",
                    "name": "Eight-sided Die",
                    "faces": 8,
                    "minValue": 1,
                    "maxValue": 8,
                    "average": 4.5,
                    "isStandard": true,
                    "sortOrder": 2,
                    "color": "#2196F3",
                    "iconId": "dice_d8",
                    "description": "Octahedron die used for medium weapons"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for complete die type
        errors.Should().BeEmpty(
            "die type with all optional fields populated should validate successfully");
    }

    /// <summary>
    /// DTS-002: Verifies that die type missing required 'name' field fails validation.
    /// </summary>
    [Test]
    public void DieTypeStructure_MissingName_FailsValidation()
    {
        // Arrange: Die type missing required 'name' field
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing name
        errors.Should().NotBeEmpty(
            "die type missing required 'name' field should fail validation");
    }

    /// <summary>
    /// DTS-002: Verifies that die type missing required 'id' field fails validation.
    /// </summary>
    [Test]
    public void DieTypeStructure_MissingId_FailsValidation()
    {
        // Arrange: Die type missing required 'id' field
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing id
        errors.Should().NotBeEmpty(
            "die type missing required 'id' field should fail validation");
    }

    /// <summary>
    /// DTS-002: Verifies that die type missing required 'faces' field fails validation.
    /// </summary>
    [Test]
    public void DieTypeStructure_MissingFaces_FailsValidation()
    {
        // Arrange: Die type missing required 'faces' field
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing faces
        errors.Should().NotBeEmpty(
            "die type missing required 'faces' field should fail validation");
    }

    /// <summary>
    /// DTS-002: Verifies that die type with unknown fields fails validation.
    /// </summary>
    [Test]
    public void DieTypeStructure_UnknownField_FailsValidation()
    {
        // Arrange: Die type with unknown field
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "unknownField": true
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown field
        errors.Should().NotBeEmpty(
            "die type with unknown field should fail validation (additionalProperties: false)");
    }

    #endregion

    #region DTS-003: Faces Validation

    /// <summary>
    /// DTS-003: Verifies that faces value of 2 (minimum, coin flip) passes validation.
    /// </summary>
    [Test]
    public void FacesValidation_MinimumValue_PassesValidation()
    {
        // Arrange: Die type with minimum faces value
        var validJson = """
        {
            "dieTypes": [
                {
                    "id": "d2",
                    "name": "Coin Flip",
                    "faces": 2
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimum faces value
        errors.Should().BeEmpty(
            "faces value of 2 (coin flip minimum) should pass validation");
    }

    /// <summary>
    /// DTS-003: Verifies that faces value of 100 passes validation.
    /// </summary>
    [Test]
    public void FacesValidation_LargeValue_PassesValidation()
    {
        // Arrange: Die type with large faces value
        var validJson = """
        {
            "dieTypes": [
                {
                    "id": "d100",
                    "name": "Percentile Die",
                    "faces": 100
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for large faces value
        errors.Should().BeEmpty(
            "faces value of 100 should pass validation");
    }

    /// <summary>
    /// DTS-003: Verifies that faces value of 1 (below minimum) fails validation.
    /// </summary>
    [Test]
    public void FacesValidation_BelowMinimum_FailsValidation()
    {
        // Arrange: Die type with faces below minimum
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d1",
                    "name": "Invalid Die",
                    "faces": 1
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for faces below 2
        errors.Should().NotBeEmpty(
            "faces value of 1 should fail validation (minimum is 2)");
    }

    /// <summary>
    /// DTS-003: Verifies that faces value of 0 fails validation.
    /// </summary>
    [Test]
    public void FacesValidation_Zero_FailsValidation()
    {
        // Arrange: Die type with zero faces
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d0",
                    "name": "Invalid Die",
                    "faces": 0
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for faces of 0
        errors.Should().NotBeEmpty(
            "faces value of 0 should fail validation (minimum is 2)");
    }

    /// <summary>
    /// DTS-003: Verifies that negative faces value fails validation.
    /// </summary>
    [Test]
    public void FacesValidation_Negative_FailsValidation()
    {
        // Arrange: Die type with negative faces
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d-1",
                    "name": "Invalid Die",
                    "faces": -1
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for negative faces
        errors.Should().NotBeEmpty(
            "negative faces value should fail validation");
    }

    #endregion

    #region DTS-004: Die Type ID Pattern Validation

    /// <summary>
    /// DTS-004: Verifies that valid die type IDs pass validation.
    /// Pattern: ^d\d+$ (d followed by one or more digits)
    /// </summary>
    [Test]
    [TestCase("d4")]
    [TestCase("d6")]
    [TestCase("d8")]
    [TestCase("d10")]
    [TestCase("d12")]
    [TestCase("d20")]
    [TestCase("d100")]
    [TestCase("d7")]
    [TestCase("d30")]
    public void DieTypeIdPattern_ValidIds_PassValidation(string dieId)
    {
        // Arrange: Die type with valid ID
        var validJson = $$"""
        {
            "dieTypes": [
                {
                    "id": "{{dieId}}",
                    "name": "Test Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid die type ID
        errors.Should().BeEmpty(
            $"die type ID '{dieId}' should pass pattern validation (^d\\d+$)");
    }

    /// <summary>
    /// DTS-004: Verifies that uppercase die type ID fails validation.
    /// </summary>
    [Test]
    public void DieTypeIdPattern_Uppercase_FailsValidation()
    {
        // Arrange: Die type with uppercase ID
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "D6",
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for uppercase ID
        errors.Should().NotBeEmpty(
            "uppercase die type ID 'D6' should fail pattern validation");
    }

    /// <summary>
    /// DTS-004: Verifies that die type ID with hyphen fails validation.
    /// </summary>
    [Test]
    public void DieTypeIdPattern_WithHyphen_FailsValidation()
    {
        // Arrange: Die type with hyphen in ID
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d-6",
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for ID with hyphen
        errors.Should().NotBeEmpty(
            "die type ID 'd-6' should fail pattern validation (hyphen not allowed)");
    }

    /// <summary>
    /// DTS-004: Verifies that die type ID with wrong prefix fails validation.
    /// </summary>
    [Test]
    public void DieTypeIdPattern_WrongPrefix_FailsValidation()
    {
        // Arrange: Die type with wrong prefix
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "dice6",
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for wrong prefix
        errors.Should().NotBeEmpty(
            "die type ID 'dice6' should fail pattern validation (must start with 'd')");
    }

    /// <summary>
    /// DTS-004: Verifies that die type ID with wrong order fails validation.
    /// </summary>
    [Test]
    public void DieTypeIdPattern_WrongOrder_FailsValidation()
    {
        // Arrange: Die type with number before 'd'
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "6d",
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for wrong order
        errors.Should().NotBeEmpty(
            "die type ID '6d' should fail pattern validation (wrong order)");
    }

    /// <summary>
    /// DTS-004: Verifies that die type ID without number fails validation.
    /// </summary>
    [Test]
    public void DieTypeIdPattern_NoNumber_FailsValidation()
    {
        // Arrange: Die type with just 'd'
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d",
                    "name": "Invalid Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing number
        errors.Should().NotBeEmpty(
            "die type ID 'd' should fail pattern validation (missing number)");
    }

    #endregion

    #region DTS-005: Expression Pattern Validation

    /// <summary>
    /// DTS-005: Verifies that valid dice expressions match the pattern.
    /// Pattern: ^\d+d\d+([+-]\d+)?$
    /// </summary>
    [Test]
    [TestCase("1d6", true)]
    [TestCase("2d10", true)]
    [TestCase("1d8+2", true)]
    [TestCase("3d6-1", true)]
    [TestCase("10d4+10", true)]
    [TestCase("1d100", true)]
    [TestCase("d6", false)]        // Missing count
    [TestCase("1d", false)]         // Missing faces
    [TestCase("1d6+", false)]       // Modifier without value
    [TestCase("1D6", false)]        // Uppercase D
    [TestCase("1d6*2", false)]      // Invalid operator
    [TestCase("-1d6", false)]       // Negative count
    [TestCase("1.5d6", false)]      // Decimal count
    public void ExpressionPattern_Validation(string expression, bool shouldMatch)
    {
        // Arrange: Use the standard dice expression pattern
        var pattern = @"^\d+d\d+([+-]\d+)?$";
        var regex = new Regex(pattern);

        // Act: Test the expression against the pattern
        var isMatch = regex.IsMatch(expression);

        // Assert: Expression should match or not match as expected
        isMatch.Should().Be(
            shouldMatch,
            $"expression '{expression}' should {(shouldMatch ? "match" : "not match")} pattern");
    }

    /// <summary>
    /// DTS-005: Verifies that expression pattern is included in the dice-types.json configuration.
    /// </summary>
    [Test]
    public async Task ExpressionPattern_DefinedInConfiguration_IncludesExamples()
    {
        // Arrange: Load the actual dice-types.json file
        var jsonContent = await File.ReadAllTextAsync(DiceTypesJsonPath);

        // Assert: Configuration should include expression pattern
        jsonContent.Should().Contain("expressionPattern",
            "dice-types.json should include expressionPattern definition");
        jsonContent.Should().Contain("pattern",
            "expressionPattern should include pattern property");
        jsonContent.Should().Contain("captureGroups",
            "expressionPattern should include captureGroups documentation");
        jsonContent.Should().Contain("examples",
            "expressionPattern should include examples array");
    }

    #endregion

    #region DTS-006: Color Hex Pattern Validation

    /// <summary>
    /// DTS-006: Verifies that valid hex color codes pass validation.
    /// Pattern: ^#[0-9A-Fa-f]{6}$
    /// </summary>
    [Test]
    [TestCase("#FF5722")]
    [TestCase("#4CAF50")]
    [TestCase("#000000")]
    [TestCase("#FFFFFF")]
    [TestCase("#abcdef")]
    [TestCase("#123456")]
    public void ColorHexPattern_ValidColors_PassValidation(string color)
    {
        // Arrange: Die type with valid color
        var validJson = $$"""
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "color": "{{color}}"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid hex color
        errors.Should().BeEmpty(
            $"color '{color}' should pass hex pattern validation");
    }

    /// <summary>
    /// DTS-006: Verifies that 3-digit hex color fails validation (6 digits required).
    /// </summary>
    [Test]
    public void ColorHexPattern_ThreeDigit_FailsValidation()
    {
        // Arrange: Die type with 3-digit hex color
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "color": "#fff"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for 3-digit hex
        errors.Should().NotBeEmpty(
            "3-digit hex color '#fff' should fail validation (6 digits required)");
    }

    /// <summary>
    /// DTS-006: Verifies that hex color without hash fails validation.
    /// </summary>
    [Test]
    public void ColorHexPattern_MissingHash_FailsValidation()
    {
        // Arrange: Die type with color missing hash
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "color": "FF5722"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing hash
        errors.Should().NotBeEmpty(
            "hex color 'FF5722' without # should fail validation");
    }

    /// <summary>
    /// DTS-006: Verifies that hex color with invalid characters fails validation.
    /// </summary>
    [Test]
    public void ColorHexPattern_InvalidChars_FailsValidation()
    {
        // Arrange: Die type with invalid hex characters
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "color": "#GGGGGG"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for invalid hex chars
        errors.Should().NotBeEmpty(
            "hex color '#GGGGGG' with invalid characters should fail validation");
    }

    /// <summary>
    /// DTS-006: Verifies that named color fails validation (hex only).
    /// </summary>
    [Test]
    public void ColorHexPattern_NamedColor_FailsValidation()
    {
        // Arrange: Die type with named color
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "color": "red"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for named color
        errors.Should().NotBeEmpty(
            "named color 'red' should fail validation (hex required)");
    }

    /// <summary>
    /// DTS-006: Verifies that omitting optional color field passes validation.
    /// </summary>
    [Test]
    public void ColorHexPattern_Omitted_PassesValidation()
    {
        // Arrange: Die type without color field
        var validJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass when color is omitted
        errors.Should().BeEmpty(
            "omitting optional color field should pass validation");
    }

    #endregion

    #region Additional Validation Tests

    /// <summary>
    /// Verifies that empty dieTypes array fails validation (minItems: 1).
    /// </summary>
    [Test]
    public void DieTypes_EmptyArray_FailsValidation()
    {
        // Arrange: Configuration with empty dieTypes array
        var invalidJson = """
        {
            "dieTypes": []
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for empty array
        errors.Should().NotBeEmpty(
            "empty dieTypes array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that missing dieTypes field fails validation (required).
    /// </summary>
    [Test]
    public void DieTypes_MissingField_FailsValidation()
    {
        // Arrange: Configuration missing dieTypes
        var invalidJson = """
        {
            "version": "1.0.0"
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "missing dieTypes field should fail validation (required)");
    }

    /// <summary>
    /// Verifies that sortOrder with negative value fails validation.
    /// </summary>
    [Test]
    public void SortOrder_Negative_FailsValidation()
    {
        // Arrange: Die type with negative sortOrder
        var invalidJson = """
        {
            "dieTypes": [
                {
                    "id": "d6",
                    "name": "Six-sided Die",
                    "faces": 6,
                    "sortOrder": -1
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for negative sortOrder
        errors.Should().NotBeEmpty(
            "negative sortOrder should fail validation (minimum: 0)");
    }

    /// <summary>
    /// Verifies that the actual dice-types.json contains all 7 standard dice.
    /// </summary>
    [Test]
    public async Task DiceTypesJson_Contains7StandardDice()
    {
        // Arrange: Load the actual dice-types.json file
        var jsonContent = await File.ReadAllTextAsync(DiceTypesJsonPath);

        // Assert: Should contain all standard dice
        jsonContent.Should().Contain("\"id\": \"d4\"", "should contain d4");
        jsonContent.Should().Contain("\"id\": \"d6\"", "should contain d6");
        jsonContent.Should().Contain("\"id\": \"d8\"", "should contain d8");
        jsonContent.Should().Contain("\"id\": \"d10\"", "should contain d10");
        jsonContent.Should().Contain("\"id\": \"d12\"", "should contain d12");
        jsonContent.Should().Contain("\"id\": \"d20\"", "should contain d20");
        jsonContent.Should().Contain("\"id\": \"d100\"", "should contain d100");
    }

    #endregion
}
