// ------------------------------------------------------------------------------
// <copyright file="CorruptionSourcesSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Integration tests for corruption-sources.schema.json validation.
// Verifies schema structure, validates the existing corruption-sources.json
// configuration, and ensures the schema rejects invalid corruption source
// definitions (corruption out of range, invalid ID format, missing required
// fields, invalid version format, additional properties, missing thresholds,
// missing penalties). Includes DTO deserialization verification.
// Part of v0.18.1e implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Integration tests validating the corruption-sources.schema.json JSON Schema.
/// Tests ensure the schema correctly validates corruption source configuration files,
/// enforces ID patterns, numeric range constraints, required field presence, threshold
/// definitions, penalty formulas, and additional property restrictions.
/// </summary>
/// <remarks>
/// <para>
/// The corruption sources schema provides 3 definitions:
/// <list type="bullet">
/// <item><description>CorruptionSourceDefinition — Individual corruption event with id, name, and corruption amount type</description></item>
/// <item><description>ThresholdEffect — Threshold crossing effect with description, UI/faction/trauma/terminal triggers</description></item>
/// <item><description>PenaltyFormula — Penalty formula with formula string expression</description></item>
/// </list>
/// </para>
/// <para>
/// The schema validates 4 top-level required sections:
/// <list type="bullet">
/// <item><description><c>corruptionSources</c> — 4 category arrays (mysticMagic, hereticalAbility, environmental, items)</description></item>
/// <item><description><c>thresholdEffects</c> — 4 threshold definitions (25, 50, 75, 100)</description></item>
/// <item><description><c>penalties</c> — 3 penalty formulas (maxHpPercent, maxApPercent, resolveDice)</description></item>
/// <item><description><c>version</c> — Configuration version in MAJOR.MINOR format</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Schema Constraints Tested:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><c>minCorruption</c> — integer, minimum 0, maximum 100</description></item>
/// <item><description><c>maxCorruption</c> — integer, minimum 1, maximum 100</description></item>
/// <item><description><c>fixedCorruption</c> — integer, minimum 1, maximum 100</description></item>
/// <item><description><c>corruptionPerHp</c> — integer, minimum 1, maximum 10</description></item>
/// <item><description><c>id</c> — string, pattern <c>^[a-z][a-z0-9-]*$</c> (kebab-case)</description></item>
/// <item><description><c>version</c> — string, pattern <c>^\d+\.\d+$</c> (MAJOR.MINOR)</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class CorruptionSourcesSchemaTests
{
    /// <summary>
    /// Path to the corruption sources schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/corruption-sources.schema.json";

    /// <summary>
    /// Path to the corruption-sources.json configuration file for validation.
    /// </summary>
    private const string ConfigPath = "../../../../../config/corruption-sources.json";

    /// <summary>
    /// Loaded JSON Schema for corruption source definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the corruption sources schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region CSS-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// CSS-001: Verifies that corruption-sources.schema.json loads and parses as valid JSON Schema.
    /// Confirms the schema title, root type, and presence of all three definition types
    /// (CorruptionSourceDefinition, ThresholdEffect, and PenaltyFormula).
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Corruption Sources Configuration");
        _schema.Type.Should().Be(JsonObjectType.Object);

        // Assert: All three definitions should be present
        _schema.Definitions.Should().ContainKey("CorruptionSourceDefinition",
            "schema should define CorruptionSourceDefinition");
        _schema.Definitions.Should().ContainKey("ThresholdEffect",
            "schema should define ThresholdEffect");
        _schema.Definitions.Should().ContainKey("PenaltyFormula",
            "schema should define PenaltyFormula");
    }

    #endregion

    #region CSS-002: Existing Configuration Passes Validation

    /// <summary>
    /// CSS-002: Verifies that the actual corruption-sources.json configuration passes validation.
    /// This test confirms the configuration file is structurally valid, all 11 corruption sources
    /// have valid IDs and corruption amounts, all 4 thresholds are defined, and all 3 penalty
    /// formulas are present.
    /// </summary>
    [Test]
    public async Task CorruptionSourcesSchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual corruption-sources.json file
        var fullConfigPath = Path.GetFullPath(ConfigPath);
        var jsonContent = await File.ReadAllTextAsync(fullConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing corruption-sources.json should validate against schema");
    }

    #endregion

    #region CSS-003: Corruption Value Range Validation

    /// <summary>
    /// CSS-003: Verifies that the schema rejects a minCorruption value exceeding the maximum of 100.
    /// The CorruptionSourceDefinition.minCorruption property has a JSON Schema constraint of
    /// minimum: 0, maximum: 100. A value of 150 should fail validation.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsInvalidMinCorruption_Above100()
    {
        // Arrange: Configuration with minCorruption of 150 (exceeds maximum 100)
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {
                "mysticMagic": [
                    { "id": "test", "name": "Test", "minCorruption": 150 }
                ]
            },
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to minCorruption exceeding maximum
        errors.Should().NotBeEmpty(
            "minCorruption of 150 exceeds maximum of 100");
    }

    /// <summary>
    /// CSS-003: Verifies that the schema rejects a maxCorruption value of 0.
    /// The CorruptionSourceDefinition.maxCorruption property has a minimum of 1.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsInvalidMaxCorruption_BelowMinimum()
    {
        // Arrange: Configuration with maxCorruption of 0 (below minimum 1)
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {
                "mysticMagic": [
                    { "id": "test", "name": "Test", "maxCorruption": 0 }
                ]
            },
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to maxCorruption below minimum
        errors.Should().NotBeEmpty(
            "maxCorruption of 0 is below minimum of 1");
    }

    /// <summary>
    /// CSS-003: Verifies that the schema rejects a corruptionPerHp value exceeding the maximum of 10.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsInvalidCorruptionPerHp_Above10()
    {
        // Arrange: Configuration with corruptionPerHp of 15 (exceeds maximum 10)
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {
                "hereticalAbility": [
                    { "id": "test", "name": "Test", "corruptionPerHp": 15 }
                ]
            },
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to corruptionPerHp exceeding maximum
        errors.Should().NotBeEmpty(
            "corruptionPerHp of 15 exceeds maximum of 10");
    }

    #endregion

    #region CSS-004: ID Pattern Validation

    /// <summary>
    /// CSS-004: Verifies that the schema rejects IDs that do not follow kebab-case format.
    /// The CorruptionSourceDefinition.id property requires pattern <c>^[a-z][a-z0-9-]*$</c>.
    /// An ID with uppercase letters should fail validation.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsInvalidIdFormat_UpperCase()
    {
        // Arrange: Configuration with invalid ID format (CamelCase)
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {
                "mysticMagic": [
                    { "id": "InvalidCamelCase", "name": "Test" }
                ]
            },
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty(
            "ID 'InvalidCamelCase' does not match kebab-case pattern ^[a-z][a-z0-9-]*$");
    }

    /// <summary>
    /// CSS-004: Verifies that the schema rejects IDs that start with a number.
    /// Kebab-case IDs must start with a lowercase letter per the pattern <c>^[a-z][a-z0-9-]*$</c>.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsInvalidIdFormat_StartsWithNumber()
    {
        // Arrange: Configuration with ID starting with a number
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {
                "mysticMagic": [
                    { "id": "1-invalid-id", "name": "Test" }
                ]
            },
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to ID not starting with a letter
        errors.Should().NotBeEmpty(
            "ID '1-invalid-id' does not match kebab-case pattern (must start with lowercase letter)");
    }

    #endregion

    #region CSS-005: Required Fields Validation

    /// <summary>
    /// CSS-005: Verifies that the schema rejects configurations missing the required version field.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsMissingVersion()
    {
        // Arrange: Configuration missing the required "version" field
        var invalidJson = """
        {
            "corruptionSources": {},
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing version
        errors.Should().NotBeEmpty(
            "missing version field should fail validation");
    }

    /// <summary>
    /// CSS-005: Verifies that the schema rejects configurations missing the required
    /// corruptionSources section.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsMissingCorruptionSources()
    {
        // Arrange: Configuration missing the required "corruptionSources" section
        var invalidJson = """
        {
            "version": "1.0",
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing corruptionSources
        errors.Should().NotBeEmpty(
            "missing corruptionSources section should fail validation");
    }

    #endregion

    #region CSS-006: Threshold Effects Validation

    /// <summary>
    /// CSS-006: Verifies that the schema rejects configurations missing the required
    /// thresholdEffects section.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsMissingThresholdEffects()
    {
        // Arrange: Configuration missing the required "thresholdEffects" section
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {},
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing thresholdEffects
        errors.Should().NotBeEmpty(
            "missing thresholdEffects section should fail validation");
    }

    /// <summary>
    /// CSS-006: Verifies that the schema rejects threshold effects missing the required
    /// "100" threshold. All four thresholds (25, 50, 75, 100) are mandatory.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsMissingThreshold100()
    {
        // Arrange: Configuration missing the required "100" threshold
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {},
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing threshold 100
        errors.Should().NotBeEmpty(
            "missing threshold '100' should fail validation");
    }

    #endregion

    #region CSS-007: Version Format Validation

    /// <summary>
    /// CSS-007: Verifies that the schema rejects version strings that don't match
    /// the MAJOR.MINOR pattern (<c>^\d+\.\d+$</c>).
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsInvalidVersionFormat()
    {
        // Arrange: Configuration with invalid version format (semantic versioning instead of MAJOR.MINOR)
        var invalidJson = """
        {
            "version": "1.0.0",
            "corruptionSources": {},
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to version not matching MAJOR.MINOR pattern
        errors.Should().NotBeEmpty(
            "version '1.0.0' does not match pattern ^\\d+\\.\\d+$");
    }

    #endregion

    #region CSS-008: Additional Properties Rejected

    /// <summary>
    /// CSS-008: Verifies that the schema rejects unknown properties at the root level.
    /// The schema sets <c>additionalProperties: false</c> to prevent configuration drift.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsUnknownRootProperties()
    {
        // Arrange: Configuration with an unknown root-level property
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {},
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            },
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
    /// CSS-008: Verifies that the schema rejects unknown properties on corruption source definitions.
    /// Individual CorruptionSourceDefinition objects only allow defined properties.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsUnknownSourceProperties()
    {
        // Arrange: Corruption source with an unknown property
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {
                "mysticMagic": [
                    { "id": "test", "name": "Test", "unknownField": true }
                ]
            },
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" },
                "resolveDice": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown source property
        errors.Should().NotBeEmpty(
            "unknown source property 'unknownField' should fail validation");
    }

    #endregion

    #region CSS-009: Penalty Formulas Validation

    /// <summary>
    /// CSS-009: Verifies that the schema rejects configurations missing the required
    /// penalties section.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsMissingPenalties()
    {
        // Arrange: Configuration missing the required "penalties" section
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {},
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing penalties
        errors.Should().NotBeEmpty(
            "missing penalties section should fail validation");
    }

    /// <summary>
    /// CSS-009: Verifies that the schema rejects penalties missing the required resolveDice formula.
    /// </summary>
    [Test]
    public void CorruptionSourcesSchema_RejectsMissingResolveDicePenalty()
    {
        // Arrange: Configuration with penalties missing resolveDice
        var invalidJson = """
        {
            "version": "1.0",
            "corruptionSources": {},
            "thresholdEffects": {
                "25": { "description": "Test" },
                "50": { "description": "Test" },
                "75": { "description": "Test" },
                "100": { "description": "Test" }
            },
            "penalties": {
                "maxHpPercent": { "formula": "test" },
                "maxApPercent": { "formula": "test" }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing resolveDice penalty
        errors.Should().NotBeEmpty(
            "missing resolveDice penalty formula should fail validation");
    }

    #endregion

    #region CSS-010: DTO Deserialization

    /// <summary>
    /// CSS-010: Verifies that the corruption-sources.json configuration deserializes correctly
    /// into <see cref="RuneAndRust.Application.DTOs.CorruptionConfigurationDto"/>.
    /// Confirms all 4 categories contain the expected number of sources (3+4+2+2 = 11 total),
    /// all 4 thresholds are present, and all 3 penalty formulas are present.
    /// </summary>
    [Test]
    public async Task CorruptionSourcesConfig_DeserializesToDtos_Correctly()
    {
        // Arrange: Load the actual corruption-sources.json file
        var fullConfigPath = Path.GetFullPath(ConfigPath);
        var jsonContent = await File.ReadAllTextAsync(fullConfigPath);

        // Act: Deserialize into the root DTO
        var config = System.Text.Json.JsonSerializer.Deserialize<
            RuneAndRust.Application.DTOs.CorruptionConfigurationDto>(jsonContent);

        // Assert: Root DTO should be populated
        config.Should().NotBeNull("configuration should deserialize successfully");
        config!.Version.Should().Be("1.0");

        // Assert: All 4 categories present with correct source counts
        config.CorruptionSources.MysticMagic.Should().HaveCount(3,
            "mysticMagic category should have 3 corruption sources");
        config.CorruptionSources.HereticalAbility.Should().HaveCount(4,
            "hereticalAbility category should have 4 corruption sources");
        config.CorruptionSources.Environmental.Should().HaveCount(2,
            "environmental category should have 2 corruption sources");
        config.CorruptionSources.Items.Should().HaveCount(2,
            "items category should have 2 corruption sources");

        // Assert: All 4 thresholds present
        config.ThresholdEffects.Should().HaveCount(4);
        config.ThresholdEffects.Should().ContainKey("25");
        config.ThresholdEffects.Should().ContainKey("50");
        config.ThresholdEffects.Should().ContainKey("75");
        config.ThresholdEffects.Should().ContainKey("100");

        // Assert: All 3 penalty formulas present
        config.Penalties.MaxHpPercent.Formula.Should().Be("floor(corruption / 10) * 5");
        config.Penalties.MaxApPercent.Formula.Should().Be("floor(corruption / 10) * 5");
        config.Penalties.ResolveDice.Formula.Should().Be("floor(corruption / 20)");
    }

    #endregion

    #region CSS-011: Individual Source Definition Deserialization

    /// <summary>
    /// CSS-011: Verifies that individual corruption source definitions deserialize with correct
    /// property values. Spot-checks the first mysticMagic source (standard-spell) and a
    /// hereticalAbility per-HP source (blot-priest-hp-cast) for all fields.
    /// </summary>
    [Test]
    public async Task CorruptionSourceDefinition_DeserializesProperties_Correctly()
    {
        // Arrange: Load and deserialize the configuration
        var fullConfigPath = Path.GetFullPath(ConfigPath);
        var jsonContent = await File.ReadAllTextAsync(fullConfigPath);
        var config = System.Text.Json.JsonSerializer.Deserialize<
            RuneAndRust.Application.DTOs.CorruptionConfigurationDto>(jsonContent);

        // Act & Assert: Range-based source (standard-spell)
        var standardSpell = config!.CorruptionSources.MysticMagic[0];
        standardSpell.Id.Should().Be("standard-spell");
        standardSpell.Name.Should().Be("Standard Spell");
        standardSpell.MinCorruption.Should().Be(0);
        standardSpell.MaxCorruption.Should().Be(2);
        standardSpell.FixedCorruption.Should().BeNull();
        standardSpell.CorruptionPerHp.Should().BeNull();
        standardSpell.PerExposure.Should().BeFalse();

        // Act & Assert: Per-HP source (blot-priest-hp-cast)
        var sacrificialCasting = config.CorruptionSources.HereticalAbility[1];
        sacrificialCasting.Id.Should().Be("blot-priest-hp-cast");
        sacrificialCasting.Name.Should().Be("Sacrificial Casting");
        sacrificialCasting.MinCorruption.Should().BeNull();
        sacrificialCasting.MaxCorruption.Should().BeNull();
        sacrificialCasting.CorruptionPerHp.Should().Be(1);

        // Act & Assert: Fixed source (blot-priest-siphon)
        var lifeSiphon = config.CorruptionSources.HereticalAbility[2];
        lifeSiphon.Id.Should().Be("blot-priest-siphon");
        lifeSiphon.FixedCorruption.Should().Be(1);

        // Act & Assert: Per-exposure environmental source (blight-zone)
        var blightZone = config.CorruptionSources.Environmental[0];
        blightZone.Id.Should().Be("blight-zone");
        blightZone.PerExposure.Should().BeTrue();
        blightZone.MinCorruption.Should().Be(1);
        blightZone.MaxCorruption.Should().Be(3);

        // Act & Assert: Threshold effects
        config.ThresholdEffects["25"].UiWarning.Should().BeTrue();
        config.ThresholdEffects["50"].FactionLock.Should().BeTrue();
        config.ThresholdEffects["75"].TraumaId.Should().Be("machine-affinity");
        config.ThresholdEffects["100"].TerminalError.Should().BeTrue();
    }

    #endregion
}
