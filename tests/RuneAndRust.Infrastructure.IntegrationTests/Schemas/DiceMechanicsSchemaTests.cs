// ------------------------------------------------------------------------------
// <copyright file="DiceMechanicsSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for dice-mechanics.schema.json validation.
// Verifies schema structure, critical thresholds, advantage rules, exploding dice,
// keep rules, reroll rules, default dice, and difficulty class definitions.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the dice-mechanics.schema.json JSON Schema.
/// Tests ensure the schema correctly validates dice mechanics configuration files,
/// enforces critical threshold constraints, advantage/disadvantage rules,
/// exploding dice settings, keep/drop rules, reroll conditions, default dice
/// expressions, and difficulty class definitions.
/// </summary>
/// <remarks>
/// <para>
/// The dice mechanics schema provides 7 definitions:
/// <list type="bullet">
/// <item><description>CriticalThresholds - Critical success/failure configuration</description></item>
/// <item><description>AdvantageRules - Advantage/disadvantage mechanics</description></item>
/// <item><description>ExplodingDiceRules - Exploding dice configuration</description></item>
/// <item><description>KeepRules - Keep/drop dice rules</description></item>
/// <item><description>RerollRules - Automatic reroll conditions</description></item>
/// <item><description>DefaultDice - Default dice for roll categories</description></item>
/// <item><description>DifficultyClass - Named difficulty levels</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class DiceMechanicsSchemaTests
{
    /// <summary>
    /// Path to the dice mechanics schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/dice-mechanics.schema.json";

    /// <summary>
    /// Path to the dice-mechanics.json configuration file.
    /// </summary>
    private const string DiceMechanicsJsonPath = "../../../../../config/dice-mechanics.json";

    /// <summary>
    /// Loaded JSON Schema for dice mechanics definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the dice mechanics schema.
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

    #region DMS-001: Schema File Loads

    /// <summary>
    /// DMS-001: Verifies that dice-mechanics.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>All required definitions are present (7 total)</description></item>
    /// <item><description>Schema title and type are correct</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void DiceMechanicsSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Dice Mechanics Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "schema type should be object");

        // Assert: All 7 definitions should be present
        _schema.Definitions.Should().ContainKey("CriticalThresholds", "should define CriticalThresholds");
        _schema.Definitions.Should().ContainKey("AdvantageRules", "should define AdvantageRules");
        _schema.Definitions.Should().ContainKey("ExplodingDiceRules", "should define ExplodingDiceRules");
        _schema.Definitions.Should().ContainKey("KeepRules", "should define KeepRules");
        _schema.Definitions.Should().ContainKey("RerollRules", "should define RerollRules");
        _schema.Definitions.Should().ContainKey("DefaultDice", "should define DefaultDice");
        _schema.Definitions.Should().ContainKey("DifficultyClass", "should define DifficultyClass");
    }

    #endregion

    #region DMS-002: Valid Configuration Passes

    /// <summary>
    /// DMS-002: Verifies that complete dice-mechanics.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DiceMechanicsSchema_ValidConfiguration_PassesValidation()
    {
        // Arrange: Load the actual dice-mechanics.json file
        var jsonContent = await File.ReadAllTextAsync(DiceMechanicsJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing dice-mechanics.json should validate against schema without errors");
    }

    /// <summary>
    /// DMS-002: Verifies that minimal configuration with only required fields passes.
    /// </summary>
    [Test]
    public void DiceMechanicsSchema_MinimalConfiguration_PassesValidation()
    {
        // Arrange: Minimal configuration with only required fields
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimal configuration
        errors.Should().BeEmpty(
            "minimal configuration with only required fields should validate successfully");
    }

    #endregion

    #region DMS-003: Invalid Critical Thresholds Rejected

    /// <summary>
    /// DMS-003: Verifies that criticalMultiplier below 1 is rejected.
    /// </summary>
    [Test]
    public void CriticalThresholds_MultiplierBelowOne_FailsValidation()
    {
        // Arrange: Configuration with invalid criticalMultiplier
        var invalidJson = """
        {
            "criticalThresholds": {
                "naturalMin": 1,
                "naturalMax": "max",
                "criticalMultiplier": 0
            },
            "defaultDice": {
                "skillCheck": "1d10"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to multiplier below minimum
        errors.Should().NotBeEmpty(
            "criticalMultiplier: 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// DMS-003: Verifies that criticalMultiplier of exactly 1 passes (minimum valid).
    /// </summary>
    [Test]
    public void CriticalThresholds_MultiplierOne_PassesValidation()
    {
        // Arrange: Configuration with minimum valid criticalMultiplier
        var validJson = """
        {
            "criticalThresholds": {
                "criticalMultiplier": 1
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimum multiplier
        errors.Should().BeEmpty(
            "criticalMultiplier: 1 should pass validation");
    }

    /// <summary>
    /// DMS-003: Verifies that naturalMin below 1 is rejected.
    /// </summary>
    [Test]
    public void CriticalThresholds_NaturalMinBelowOne_FailsValidation()
    {
        // Arrange: Configuration with invalid naturalMin
        var invalidJson = """
        {
            "criticalThresholds": {
                "naturalMin": 0
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to naturalMin below minimum
        errors.Should().NotBeEmpty(
            "naturalMin: 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// DMS-003: Verifies that naturalMax as string "max" passes.
    /// </summary>
    [Test]
    public void CriticalThresholds_NaturalMaxStringMax_PassesValidation()
    {
        // Arrange: Configuration with naturalMax as "max"
        var validJson = """
        {
            "criticalThresholds": {
                "naturalMax": "max"
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for "max" value
        errors.Should().BeEmpty(
            "naturalMax: 'max' should pass validation");
    }

    /// <summary>
    /// DMS-003: Verifies that naturalMax as integer passes.
    /// </summary>
    [Test]
    public void CriticalThresholds_NaturalMaxInteger_PassesValidation()
    {
        // Arrange: Configuration with naturalMax as integer (expanded crit range)
        var validJson = """
        {
            "criticalThresholds": {
                "naturalMax": 19
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for integer value
        errors.Should().BeEmpty(
            "naturalMax: 19 should pass validation (expanded crit range)");
    }

    /// <summary>
    /// DMS-003: Verifies that naturalMax as integer 0 is rejected (must be >= 1).
    /// </summary>
    [Test]
    public void CriticalThresholds_NaturalMaxZeroInteger_FailsValidation()
    {
        // Arrange: Configuration with naturalMax as 0
        var invalidJson = """
        {
            "criticalThresholds": {
                "naturalMax": 0
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for integer below minimum
        errors.Should().NotBeEmpty(
            "naturalMax: 0 should fail validation (integer must be >= 1)");
    }

    /// <summary>
    /// DMS-003: Verifies that criticalBonusDice accepts valid dice expression.
    /// </summary>
    [Test]
    public void CriticalThresholds_ValidBonusDice_PassesValidation()
    {
        // Arrange: Configuration with valid bonus dice expression
        var validJson = """
        {
            "criticalThresholds": {
                "criticalBonusDice": "1d6"
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid dice expression
        errors.Should().BeEmpty(
            "criticalBonusDice: '1d6' should pass validation");
    }

    /// <summary>
    /// DMS-003: Verifies that criticalBonusDice rejects invalid dice expression.
    /// </summary>
    [Test]
    public void CriticalThresholds_InvalidBonusDice_FailsValidation()
    {
        // Arrange: Configuration with invalid bonus dice expression
        var invalidJson = """
        {
            "criticalThresholds": {
                "criticalBonusDice": "d6"
            },
            "defaultDice": {}
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for invalid dice expression
        errors.Should().NotBeEmpty(
            "criticalBonusDice: 'd6' should fail validation (missing count)");
    }

    #endregion

    #region DMS-004: Invalid Difficulty Class Rejected

    /// <summary>
    /// DMS-004: Verifies that uppercase difficulty class ID is rejected.
    /// Pattern requires: ^[a-z][a-z0-9-]*$
    /// </summary>
    [Test]
    public void DifficultyClass_UppercaseId_FailsValidation()
    {
        // Arrange: Configuration with uppercase DC ID
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "difficultyClasses": [
                {
                    "id": "Easy",
                    "name": "Easy",
                    "dc": 8
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for uppercase ID
        errors.Should().NotBeEmpty(
            "difficulty class ID 'Easy' should fail validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// DMS-004: Verifies that valid kebab-case difficulty class ID passes.
    /// </summary>
    [Test]
    public void DifficultyClass_ValidKebabCaseId_PassesValidation()
    {
        // Arrange: Configuration with valid kebab-case ID
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "difficultyClasses": [
                {
                    "id": "very-hard",
                    "name": "Very Hard",
                    "dc": 20
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid kebab-case ID
        errors.Should().BeEmpty(
            "difficulty class ID 'very-hard' should pass validation");
    }

    /// <summary>
    /// DMS-004: Verifies that difficulty class missing required ID fails.
    /// </summary>
    [Test]
    public void DifficultyClass_MissingId_FailsValidation()
    {
        // Arrange: Configuration with DC missing ID
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "difficultyClasses": [
                {
                    "name": "Easy",
                    "dc": 8
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "difficulty class missing 'id' should fail validation");
    }

    /// <summary>
    /// DMS-004: Verifies that difficulty class missing required dc fails.
    /// </summary>
    [Test]
    public void DifficultyClass_MissingDc_FailsValidation()
    {
        // Arrange: Configuration with DC missing dc value
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "difficultyClasses": [
                {
                    "id": "easy",
                    "name": "Easy"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "difficulty class missing 'dc' should fail validation");
    }

    /// <summary>
    /// DMS-004: Verifies that difficulty class with dc below 1 fails.
    /// </summary>
    [Test]
    public void DifficultyClass_DcBelowOne_FailsValidation()
    {
        // Arrange: Configuration with DC value of 0
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "difficultyClasses": [
                {
                    "id": "easy",
                    "name": "Easy",
                    "dc": 0
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for dc below minimum
        errors.Should().NotBeEmpty(
            "difficulty class with dc: 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// DMS-004: Verifies that difficulty class with negative sortOrder fails.
    /// </summary>
    [Test]
    public void DifficultyClass_NegativeSortOrder_FailsValidation()
    {
        // Arrange: Configuration with negative sortOrder
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "difficultyClasses": [
                {
                    "id": "easy",
                    "name": "Easy",
                    "dc": 8,
                    "sortOrder": -1
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for negative sortOrder
        errors.Should().NotBeEmpty(
            "difficulty class with sortOrder: -1 should fail validation (minimum is 0)");
    }

    #endregion

    #region DMS-005: Missing Required Fields Rejected

    /// <summary>
    /// DMS-005: Verifies that configuration missing criticalThresholds fails.
    /// </summary>
    [Test]
    public void MissingRequiredFields_NoCriticalThresholds_FailsValidation()
    {
        // Arrange: Configuration missing criticalThresholds
        var invalidJson = """
        {
            "defaultDice": {
                "skillCheck": "1d10"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "configuration missing 'criticalThresholds' should fail validation");
    }

    /// <summary>
    /// DMS-005: Verifies that configuration missing defaultDice fails.
    /// </summary>
    [Test]
    public void MissingRequiredFields_NoDefaultDice_FailsValidation()
    {
        // Arrange: Configuration missing defaultDice
        var invalidJson = """
        {
            "criticalThresholds": {
                "naturalMin": 1
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "configuration missing 'defaultDice' should fail validation");
    }

    /// <summary>
    /// DMS-005: Verifies that empty configuration fails.
    /// </summary>
    [Test]
    public void MissingRequiredFields_EmptyConfig_FailsValidation()
    {
        // Arrange: Empty configuration
        var invalidJson = "{}";

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for empty configuration
        errors.Should().NotBeEmpty(
            "empty configuration should fail validation (missing all required fields)");
    }

    #endregion

    #region DMS-006: Exploding Dice Validation

    /// <summary>
    /// DMS-006: Verifies that maxExplosions below 1 is rejected.
    /// </summary>
    [Test]
    public void ExplodingDice_MaxExplosionsZero_FailsValidation()
    {
        // Arrange: Configuration with maxExplosions: 0
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "explodingDice": {
                "enabled": true,
                "maxExplosions": 0
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for maxExplosions below minimum
        errors.Should().NotBeEmpty(
            "maxExplosions: 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// DMS-006: Verifies that maxExplosions of 1 passes (minimum valid).
    /// </summary>
    [Test]
    public void ExplodingDice_MaxExplosionsOne_PassesValidation()
    {
        // Arrange: Configuration with minimum valid maxExplosions
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "explodingDice": {
                "enabled": true,
                "maxExplosions": 1
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimum maxExplosions
        errors.Should().BeEmpty(
            "maxExplosions: 1 should pass validation");
    }

    /// <summary>
    /// DMS-006: Verifies that explodeOn enum validates correctly.
    /// </summary>
    [Test]
    [TestCase("Max")]
    [TestCase("Threshold")]
    public void ExplodingDice_ValidExplodeOn_PassesValidation(string explodeOn)
    {
        // Arrange: Configuration with valid explodeOn value
        var validJson = $$"""
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "explodingDice": {
                "enabled": true,
                "explodeOn": "{{explodeOn}}"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid enum value
        errors.Should().BeEmpty(
            $"explodeOn: '{explodeOn}' should pass validation");
    }

    /// <summary>
    /// DMS-006: Verifies that invalid explodeOn enum value is rejected.
    /// </summary>
    [Test]
    public void ExplodingDice_InvalidExplodeOn_FailsValidation()
    {
        // Arrange: Configuration with invalid explodeOn value
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "explodingDice": {
                "enabled": true,
                "explodeOn": "Always"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for invalid enum value
        errors.Should().NotBeEmpty(
            "explodeOn: 'Always' should fail validation (must be 'Max' or 'Threshold')");
    }

    /// <summary>
    /// DMS-006: Verifies that appliesToTypes validates die type IDs.
    /// </summary>
    [Test]
    public void ExplodingDice_ValidAppliesToTypes_PassesValidation()
    {
        // Arrange: Configuration with valid die type IDs
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "explodingDice": {
                "enabled": true,
                "appliesToTypes": ["d6", "d10", "d20"]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid die type IDs
        errors.Should().BeEmpty(
            "appliesToTypes with valid die type IDs should pass validation");
    }

    /// <summary>
    /// DMS-006: Verifies that invalid die type ID in appliesToTypes is rejected.
    /// </summary>
    [Test]
    public void ExplodingDice_InvalidAppliesToTypes_FailsValidation()
    {
        // Arrange: Configuration with invalid die type ID
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "explodingDice": {
                "enabled": true,
                "appliesToTypes": ["D6", "dice10"]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for invalid die type ID pattern
        errors.Should().NotBeEmpty(
            "appliesToTypes with invalid die type IDs should fail validation");
    }

    #endregion

    #region Advantage Rules Validation

    /// <summary>
    /// Verifies that advantageKeep enum validates correctly.
    /// </summary>
    [Test]
    [TestCase("Highest")]
    [TestCase("Lowest")]
    public void AdvantageRules_ValidKeepEnum_PassesValidation(string keepValue)
    {
        // Arrange: Configuration with valid advantageKeep value
        var validJson = $$"""
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "advantageRules": {
                "advantageKeep": "{{keepValue}}"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid enum value
        errors.Should().BeEmpty(
            $"advantageKeep: '{keepValue}' should pass validation");
    }

    /// <summary>
    /// Verifies that invalid advantageKeep enum value is rejected.
    /// </summary>
    [Test]
    public void AdvantageRules_InvalidKeepEnum_FailsValidation()
    {
        // Arrange: Configuration with invalid advantageKeep value
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "advantageRules": {
                "advantageKeep": "Best"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for invalid enum value
        errors.Should().NotBeEmpty(
            "advantageKeep: 'Best' should fail validation (must be 'Highest' or 'Lowest')");
    }

    /// <summary>
    /// Verifies that advantageDice below 1 is rejected.
    /// </summary>
    [Test]
    public void AdvantageRules_AdvantageDiceBelowOne_FailsValidation()
    {
        // Arrange: Configuration with advantageDice: 0
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "advantageRules": {
                "advantageDice": 0
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for advantageDice below minimum
        errors.Should().NotBeEmpty(
            "advantageDice: 0 should fail validation (minimum is 1)");
    }

    #endregion

    #region Default Dice Validation

    /// <summary>
    /// Verifies that valid dice expressions pass for all default dice categories.
    /// </summary>
    [Test]
    public void DefaultDice_ValidExpressions_PassesValidation()
    {
        // Arrange: Configuration with valid dice expressions
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {
                "skillCheck": "1d10",
                "attackRoll": "1d20",
                "saveRoll": "1d20",
                "damageRoll": "2d6+3",
                "initiativeRoll": "1d10+5",
                "healingRoll": "1d8"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid expressions
        errors.Should().BeEmpty(
            "valid dice expressions for all categories should pass validation");
    }

    /// <summary>
    /// Verifies that invalid dice expression is rejected.
    /// </summary>
    [Test]
    public void DefaultDice_InvalidExpression_FailsValidation()
    {
        // Arrange: Configuration with invalid dice expression
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {
                "skillCheck": "d10"
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for invalid expression
        errors.Should().NotBeEmpty(
            "skillCheck: 'd10' should fail validation (missing count)");
    }

    #endregion

    #region Keep Rules Validation

    /// <summary>
    /// Verifies that keepHighest with valid value passes.
    /// </summary>
    [Test]
    public void KeepRules_ValidKeepHighest_PassesValidation()
    {
        // Arrange: Configuration with valid keepHighest
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "keepRules": {
                "keepHighest": 3
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "keepHighest: 3 should pass validation");
    }

    /// <summary>
    /// Verifies that keepHighest below 1 is rejected.
    /// </summary>
    [Test]
    public void KeepRules_KeepHighestBelowOne_FailsValidation()
    {
        // Arrange: Configuration with keepHighest: 0
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "keepRules": {
                "keepHighest": 0
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "keepHighest: 0 should fail validation (minimum is 1)");
    }

    #endregion

    #region Reroll Rules Validation

    /// <summary>
    /// Verifies that maxRerolls below 1 is rejected.
    /// </summary>
    [Test]
    public void RerollRules_MaxRerollsBelowOne_FailsValidation()
    {
        // Arrange: Configuration with maxRerolls: 0
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "rerollRules": {
                "maxRerolls": 0
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "maxRerolls: 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that rerollBelow with valid value passes.
    /// </summary>
    [Test]
    public void RerollRules_ValidRerollBelow_PassesValidation()
    {
        // Arrange: Configuration with valid rerollBelow
        var validJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "rerollRules": {
                "rerollBelow": 3,
                "maxRerolls": 1
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "rerollBelow: 3 with maxRerolls: 1 should pass validation");
    }

    #endregion

    #region Additional Validation Tests

    /// <summary>
    /// Verifies that unknown properties are rejected (additionalProperties: false).
    /// </summary>
    [Test]
    public void Schema_UnknownRootProperty_FailsValidation()
    {
        // Arrange: Configuration with unknown property
        var invalidJson = """
        {
            "criticalThresholds": {},
            "defaultDice": {},
            "unknownProperty": true
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for unknown property
        errors.Should().NotBeEmpty(
            "unknown root property should fail validation (additionalProperties: false)");
    }

    /// <summary>
    /// Verifies that the actual dice-mechanics.json contains all 6 difficulty classes.
    /// </summary>
    [Test]
    public async Task DiceMechanicsJson_Contains6DifficultyClasses()
    {
        // Arrange: Load the actual dice-mechanics.json file
        var jsonContent = await File.ReadAllTextAsync(DiceMechanicsJsonPath);

        // Assert: Should contain all 6 standard difficulty classes
        jsonContent.Should().Contain("\"id\": \"trivial\"", "should contain trivial DC");
        jsonContent.Should().Contain("\"id\": \"easy\"", "should contain easy DC");
        jsonContent.Should().Contain("\"id\": \"moderate\"", "should contain moderate DC");
        jsonContent.Should().Contain("\"id\": \"hard\"", "should contain hard DC");
        jsonContent.Should().Contain("\"id\": \"very-hard\"", "should contain very-hard DC");
        jsonContent.Should().Contain("\"id\": \"nearly-impossible\"", "should contain nearly-impossible DC");
    }

    /// <summary>
    /// Verifies that the actual dice-mechanics.json contains all 6 default dice categories.
    /// </summary>
    [Test]
    public async Task DiceMechanicsJson_ContainsAllDefaultDiceCategories()
    {
        // Arrange: Load the actual dice-mechanics.json file
        var jsonContent = await File.ReadAllTextAsync(DiceMechanicsJsonPath);

        // Assert: Should contain all 6 default dice categories
        jsonContent.Should().Contain("\"skillCheck\":", "should contain skillCheck");
        jsonContent.Should().Contain("\"attackRoll\":", "should contain attackRoll");
        jsonContent.Should().Contain("\"saveRoll\":", "should contain saveRoll");
        jsonContent.Should().Contain("\"damageRoll\":", "should contain damageRoll");
        jsonContent.Should().Contain("\"initiativeRoll\":", "should contain initiativeRoll");
        jsonContent.Should().Contain("\"healingRoll\":", "should contain healingRoll");
    }

    #endregion
}
