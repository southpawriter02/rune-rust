// ------------------------------------------------------------------------------
// <copyright file="StressSourcesSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Integration tests for stress-sources.schema.json validation.
// Verifies schema structure, validates the existing stress-sources.json
// configuration, and ensures the schema rejects invalid stress source
// definitions (baseStress out of range, resistDc out of range, invalid ID format).
// Part of v0.18.0e implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Integration tests validating the stress-sources.schema.json JSON Schema.
/// Tests ensure the schema correctly validates stress source configuration files,
/// enforces ID patterns, numeric range constraints, and required field presence.
/// </summary>
/// <remarks>
/// <para>
/// The stress sources schema provides 2 definitions:
/// <list type="bullet">
/// <item><description>StressSourceDefinition — Individual stress event with id, baseStress, resistDc, description</description></item>
/// <item><description>RecoveryRate — Recovery formula configuration with formula string</description></item>
/// </list>
/// </para>
/// <para>
/// The schema validates 3 top-level required sections:
/// <list type="bullet">
/// <item><description><c>stressSources</c> — 6 category arrays (combat, exploration, narrative, heretical, environmental, corruption)</description></item>
/// <item><description><c>recoveryRates</c> — 4 rest type formulas (shortRest, longRest, sanctuary, milestone)</description></item>
/// <item><description><c>traumaCheckReset</c> — Passed/failed reset values (integers 0-100)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Schema Constraints Tested:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><c>baseStress</c> — integer, minimum 1, maximum 100</description></item>
/// <item><description><c>resistDc</c> — integer, minimum 0, maximum 10</description></item>
/// <item><description><c>id</c> — string, pattern <c>^[a-z][a-z0-9-]*$</c> (kebab-case)</description></item>
/// <item><description><c>version</c> — string, pattern <c>^\d+\.\d+$</c> (MAJOR.MINOR)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class StressSourcesSchemaTests
{
    /// <summary>
    /// Path to the stress sources schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/stress-sources.schema.json";

    /// <summary>
    /// Path to the stress-sources.json configuration file for validation.
    /// </summary>
    private const string ConfigPath = "../../../../../config/stress-sources.json";

    /// <summary>
    /// Loaded JSON Schema for stress source definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the stress sources schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region SSS-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// SSS-001: Verifies that stress-sources.schema.json loads and parses as valid JSON Schema.
    /// Confirms the schema title, root type, and presence of both definition types
    /// (StressSourceDefinition and RecoveryRate).
    /// </summary>
    [Test]
    public void StressSourcesSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Stress Sources Configuration");
        _schema.Type.Should().Be(JsonObjectType.Object);

        // Assert: Both definitions should be present
        _schema.Definitions.Should().ContainKey("StressSourceDefinition",
            "schema should define StressSourceDefinition");
        _schema.Definitions.Should().ContainKey("RecoveryRate",
            "schema should define RecoveryRate");
    }

    /// <summary>
    /// SSS-001: Verifies that the actual stress-sources.json configuration passes validation.
    /// This test confirms the configuration file is structurally valid, all 24 stress sources
    /// have valid IDs, baseStress, and resistDc values, and all required sections are present.
    /// </summary>
    [Test]
    public async Task StressSourcesSchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual stress-sources.json file
        var fullConfigPath = Path.GetFullPath(ConfigPath);
        var jsonContent = await File.ReadAllTextAsync(fullConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing stress-sources.json should validate against schema");
    }

    #endregion

    #region SSS-002: Stress Source Value Validation

    /// <summary>
    /// SSS-002: Verifies that the schema rejects a baseStress value exceeding the maximum of 100.
    /// The StressSourceDefinition.baseStress property has a JSON Schema constraint of
    /// minimum: 1, maximum: 100. A value of 150 should fail validation.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidBaseStress_Above100()
    {
        // Arrange: Configuration with baseStress of 150 (exceeds maximum 100)
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 150, "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to baseStress exceeding maximum
        errors.Should().NotBeEmpty(
            "baseStress of 150 exceeds maximum of 100");
    }

    /// <summary>
    /// SSS-002: Verifies that the schema rejects a baseStress value below the minimum of 1.
    /// A stress source with baseStress of 0 should fail validation since the minimum is 1
    /// (stress sources must apply at least 1 point of stress).
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidBaseStress_Below1()
    {
        // Arrange: Configuration with baseStress of 0 (below minimum 1)
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 0, "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to baseStress below minimum
        errors.Should().NotBeEmpty(
            "baseStress of 0 is below minimum of 1");
    }

    /// <summary>
    /// SSS-002: Verifies that the schema rejects a resistDc value exceeding the maximum of 10.
    /// The StressSourceDefinition.resistDc property has a JSON Schema constraint of
    /// minimum: 0, maximum: 10. A value of 15 should fail validation.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidResistDc_Above10()
    {
        // Arrange: Configuration with resistDc of 15 (exceeds maximum 10)
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 20, "resistDc": 15 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to resistDc exceeding maximum
        errors.Should().NotBeEmpty(
            "resistDc of 15 exceeds maximum of 10");
    }

    /// <summary>
    /// SSS-002: Verifies that the schema rejects a resistDc value below the minimum of 0.
    /// A negative resistDc value should fail validation.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidResistDc_BelowZero()
    {
        // Arrange: Configuration with resistDc of -1 (below minimum 0)
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 20, "resistDc": -1 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to resistDc below minimum
        errors.Should().NotBeEmpty(
            "resistDc of -1 is below minimum of 0");
    }

    #endregion

    #region SSS-003: Stress Source ID Pattern Validation

    /// <summary>
    /// SSS-003: Verifies that the schema rejects IDs that do not follow kebab-case format.
    /// The StressSourceDefinition.id property requires pattern <c>^[a-z][a-z0-9-]*$</c>.
    /// An ID with uppercase letters and spaces should fail validation.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidIdFormat_UppercaseWithSpaces()
    {
        // Arrange: Configuration with invalid ID format (uppercase, spaces)
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "Invalid Source ID", "baseStress": 20, "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty(
            "ID 'Invalid Source ID' does not match kebab-case pattern ^[a-z][a-z0-9-]*$");
    }

    /// <summary>
    /// SSS-003: Verifies that the schema rejects IDs that start with a number.
    /// Kebab-case IDs must start with a lowercase letter per the pattern <c>^[a-z][a-z0-9-]*$</c>.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidIdFormat_StartsWithNumber()
    {
        // Arrange: Configuration with ID starting with a number
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "1-invalid-id", "baseStress": 20, "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to ID not starting with a letter
        errors.Should().NotBeEmpty(
            "ID '1-invalid-id' does not match kebab-case pattern (must start with lowercase letter)");
    }

    #endregion

    #region SSS-004: Required Fields Validation

    /// <summary>
    /// SSS-004: Verifies that the schema rejects configurations missing the required version field.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsMissingVersion()
    {
        // Arrange: Configuration missing the required "version" field
        var invalidJson = """
        {
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 20, "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing version
        errors.Should().NotBeEmpty(
            "missing version field should fail validation");
    }

    /// <summary>
    /// SSS-004: Verifies that the schema rejects configurations missing the required
    /// recoveryRates section. All four recovery rate formulas are mandatory.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsMissingRecoveryRates()
    {
        // Arrange: Configuration missing the required "recoveryRates" section
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 20, "resistDc": 2 }
                ]
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing recoveryRates
        errors.Should().NotBeEmpty(
            "missing recoveryRates section should fail validation");
    }

    /// <summary>
    /// SSS-004: Verifies that the schema rejects configurations missing the required
    /// traumaCheckReset section.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsMissingTraumaCheckReset()
    {
        // Arrange: Configuration missing the required "traumaCheckReset" section
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 20, "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing traumaCheckReset
        errors.Should().NotBeEmpty(
            "missing traumaCheckReset section should fail validation");
    }

    /// <summary>
    /// SSS-004: Verifies that the schema rejects a stress source definition missing
    /// the required baseStress field.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsMissingBaseStress()
    {
        // Arrange: Stress source missing the required "baseStress" field
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "resistDc": 2 }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing baseStress
        errors.Should().NotBeEmpty(
            "stress source missing baseStress should fail validation");
    }

    #endregion

    #region SSS-005: Version Format Validation

    /// <summary>
    /// SSS-005: Verifies that the schema rejects version strings that don't match
    /// the MAJOR.MINOR pattern (<c>^\d+\.\d+$</c>).
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidVersionFormat()
    {
        // Arrange: Configuration with invalid version format (semantic versioning instead of MAJOR.MINOR)
        var invalidJson = """
        {
            "version": "1.0.0",
            "stressSources": {},
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to version not matching MAJOR.MINOR pattern
        errors.Should().NotBeEmpty(
            "version '1.0.0' does not match pattern ^\\d+\\.\\d+$");
    }

    #endregion

    #region SSS-006: Additional Properties Rejected

    /// <summary>
    /// SSS-006: Verifies that the schema rejects unknown properties at the root level.
    /// The schema sets <c>additionalProperties: false</c> to prevent configuration drift.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsUnknownRootProperties()
    {
        // Arrange: Configuration with an unknown root-level property
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {},
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 },
            "unknownProperty": "should not be here"
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown property
        errors.Should().NotBeEmpty(
            "unknown root property 'unknownProperty' should fail validation");
    }

    /// <summary>
    /// SSS-006: Verifies that the schema rejects unknown properties on stress source definitions.
    /// Individual StressSourceDefinition objects only allow id, baseStress, resistDc, and description.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsUnknownStressSourceProperties()
    {
        // Arrange: Stress source with an unknown property
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {
                "combat": [
                    { "id": "test-source", "baseStress": 20, "resistDc": 2, "unknownField": true }
                ]
            },
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown stress source property
        errors.Should().NotBeEmpty(
            "unknown stress source property 'unknownField' should fail validation");
    }

    #endregion

    #region SSS-007: Trauma Check Reset Value Validation

    /// <summary>
    /// SSS-007: Verifies that the schema rejects trauma reset values outside the 0-100 range.
    /// The passed value must be an integer between 0 and 100.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidTraumaResetPassed_Above100()
    {
        // Arrange: Configuration with trauma reset passed value of 150
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {},
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 150, "failed": 50 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to passed value exceeding 100
        errors.Should().NotBeEmpty(
            "trauma reset passed value of 150 exceeds maximum of 100");
    }

    /// <summary>
    /// SSS-007: Verifies that the schema rejects negative trauma reset values.
    /// The failed value must be an integer between 0 and 100.
    /// </summary>
    [Test]
    public void StressSourcesSchema_RejectsInvalidTraumaResetFailed_BelowZero()
    {
        // Arrange: Configuration with trauma reset failed value of -10
        var invalidJson = """
        {
            "version": "1.0",
            "stressSources": {},
            "recoveryRates": {
                "shortRest": { "formula": "WILL × 2" },
                "longRest": { "formula": "WILL × 5" },
                "sanctuary": { "formula": "FULL_RESET" },
                "milestone": { "formula": "25" }
            },
            "traumaCheckReset": { "passed": 75, "failed": -10 }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to failed value below 0
        errors.Should().NotBeEmpty(
            "trauma reset failed value of -10 is below minimum of 0");
    }

    #endregion

    #region SSS-008: DTO Deserialization

    /// <summary>
    /// SSS-008: Verifies that the stress-sources.json configuration deserializes correctly
    /// into <see cref="RuneAndRust.Application.DTOs.StressConfigurationDto"/>.
    /// Confirms all 6 categories contain 4 sources each (24 total), all recovery rates
    /// are present, and trauma reset values match expected defaults.
    /// </summary>
    [Test]
    public async Task StressSourcesConfig_DeserializesToDtos_Correctly()
    {
        // Arrange: Load the actual stress-sources.json file
        var fullConfigPath = Path.GetFullPath(ConfigPath);
        var jsonContent = await File.ReadAllTextAsync(fullConfigPath);

        // Act: Deserialize into the root DTO
        var config = System.Text.Json.JsonSerializer.Deserialize<
            RuneAndRust.Application.DTOs.StressConfigurationDto>(jsonContent);

        // Assert: Root DTO should be populated
        config.Should().NotBeNull("configuration should deserialize successfully");
        config!.Version.Should().Be("1.0");

        // Assert: All 6 categories present with 4 sources each
        config.StressSources.Combat.Should().HaveCount(4,
            "combat category should have 4 stress sources");
        config.StressSources.Exploration.Should().HaveCount(4,
            "exploration category should have 4 stress sources");
        config.StressSources.Narrative.Should().HaveCount(4,
            "narrative category should have 4 stress sources");
        config.StressSources.Heretical.Should().HaveCount(4,
            "heretical category should have 4 stress sources");
        config.StressSources.Environmental.Should().HaveCount(4,
            "environmental category should have 4 stress sources");
        config.StressSources.Corruption.Should().HaveCount(4,
            "corruption category should have 4 stress sources");

        // Assert: Recovery rates present
        config.RecoveryRates.ShortRest.Formula.Should().Be("WILL \u00d7 2");
        config.RecoveryRates.LongRest.Formula.Should().Be("WILL \u00d7 5");
        config.RecoveryRates.Sanctuary.Formula.Should().Be("FULL_RESET");
        config.RecoveryRates.Milestone.Formula.Should().Be("25");

        // Assert: Trauma reset values
        config.TraumaCheckReset.Passed.Should().Be(75);
        config.TraumaCheckReset.Failed.Should().Be(50);
    }

    /// <summary>
    /// SSS-008: Verifies that individual stress source definitions deserialize with correct
    /// property values. Spot-checks the first combat source (enemy-fear-aura) for all fields.
    /// </summary>
    [Test]
    public async Task StressSourceDefinition_DeserializesProperties_Correctly()
    {
        // Arrange: Load and deserialize the configuration
        var fullConfigPath = Path.GetFullPath(ConfigPath);
        var jsonContent = await File.ReadAllTextAsync(fullConfigPath);
        var config = System.Text.Json.JsonSerializer.Deserialize<
            RuneAndRust.Application.DTOs.StressConfigurationDto>(jsonContent);

        // Act: Get the first combat source
        var fearAura = config!.StressSources.Combat[0];

        // Assert: All properties match expected values
        fearAura.Id.Should().Be("enemy-fear-aura");
        fearAura.BaseStress.Should().Be(15);
        fearAura.ResistDc.Should().Be(2);
        fearAura.Description.Should().Be("An enemy's terrifying presence radiates fear.");
    }

    #endregion
}
