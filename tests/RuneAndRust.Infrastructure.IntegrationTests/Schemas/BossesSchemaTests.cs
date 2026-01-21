// ------------------------------------------------------------------------------
// <copyright file="BossesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for bosses.schema.json validation.
// Verifies schema structure, multi-phase boss definitions, health threshold
// range validation (0-100), behavior enum validation (5 values), phases array
// minimum validation (minItems: 1), and backwards compatibility with existing
// bosses.json containing 3 example bosses.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the bosses.schema.json JSON Schema.
/// Tests ensure the schema correctly validates boss encounter definitions,
/// enforces health thresholds (0-100%), behavior enum values (5 patterns),
/// phases array minimum (1+), and required fields. These tests support the
/// v0.14.3d Boss Schema implementation for multi-phase boss encounters.
/// </summary>
[TestFixture]
public class BossesSchemaTests
{
    /// <summary>
    /// Path to the bosses schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/bosses.schema.json";

    /// <summary>
    /// Path to the actual bosses.json configuration file.
    /// </summary>
    private const string BossesJsonPath = "../../../../../config/bosses.json";

    /// <summary>
    /// Loaded JSON Schema for boss encounter definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the bosses schema.
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
    /// Validates that all 8 definitions are present:
    /// BossDefinition, BossPhase, EnrageConfig, SummonConfig, VulnerabilityWindow,
    /// BossAnnouncement, QuantityRange, StatModifiers.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Boss Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all 8 definitions are present
        _schema.Definitions.Should().ContainKey("BossDefinition", "should define BossDefinition");
        _schema.Definitions.Should().ContainKey("BossPhase", "should define BossPhase");
        _schema.Definitions.Should().ContainKey("EnrageConfig", "should define EnrageConfig");
        _schema.Definitions.Should().ContainKey("SummonConfig", "should define SummonConfig");
        _schema.Definitions.Should().ContainKey("VulnerabilityWindow", "should define VulnerabilityWindow");
        _schema.Definitions.Should().ContainKey("BossAnnouncement", "should define BossAnnouncement");
        _schema.Definitions.Should().ContainKey("QuantityRange", "should define QuantityRange");
        _schema.Definitions.Should().ContainKey("StatModifiers", "should define StatModifiers");
    }

    /// <summary>
    /// Verifies the bosses.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 3 existing bosses (skeleton-king, shadow-weaver, stone-guardian) are valid.
    /// </summary>
    [Test]
    public async Task BossesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual bosses.json file
        var bossesJsonContent = await File.ReadAllTextAsync(BossesJsonPath);

        // Act: Validate the bosses.json content against the schema
        var errors = _schema.Validate(bossesJsonContent);

        // Assert: No validation errors - all existing bosses should be valid
        errors.Should().BeEmpty(
            "existing bosses.json should validate against the schema without errors. " +
            "All 3 bosses (skeleton-king, shadow-weaver, stone-guardian) must pass validation.");
    }

    /// <summary>
    /// Verifies phases array must have at least 1 item.
    /// Bosses without phases are invalid as phase-based combat is required.
    /// </summary>
    [Test]
    public void Phases_EmptyArray_FailsValidation()
    {
        // Arrange: JSON with empty phases array
        var invalidJson = """
        {
            "bosses": [
                {
                    "id": "no-phases-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with no phases",
                    "baseMonsterId": "skeleton",
                    "phases": []
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty phases array
        errors.Should().NotBeEmpty(
            "empty phases array should fail validation (minItems: 1 required)");
    }

    /// <summary>
    /// Verifies health threshold must be between 0 and 100.
    /// Health thresholds outside this range are invalid as they represent
    /// impossible health percentages.
    /// </summary>
    [Test]
    public void HealthThreshold_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with health threshold above 100
        var invalidJson = """
        {
            "bosses": [
                {
                    "id": "invalid-threshold-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid threshold",
                    "baseMonsterId": "skeleton",
                    "phases": [
                        {
                            "id": "phase_1",
                            "name": "Phase 1",
                            "healthThresholdPercent": 150,
                            "behavior": "Aggressive"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to health threshold > 100
        errors.Should().NotBeEmpty(
            "healthThresholdPercent 150 should fail validation (must be 0-100)");
    }

    /// <summary>
    /// Verifies behavior must be one of the valid enum values:
    /// Aggressive, Defensive, Tactical, Berserk, Summoner.
    /// Invalid behavior values are rejected.
    /// </summary>
    [Test]
    public void Behavior_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid behavior value
        var invalidJson = """
        {
            "bosses": [
                {
                    "id": "invalid-behavior-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid behavior",
                    "baseMonsterId": "skeleton",
                    "phases": [
                        {
                            "id": "phase_1",
                            "name": "Phase 1",
                            "healthThresholdPercent": 100,
                            "behavior": "InvalidBehavior"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid behavior enum
        errors.Should().NotBeEmpty(
            "behavior 'InvalidBehavior' should fail validation (must be Aggressive, Defensive, Tactical, Berserk, or Summoner)");
    }

    /// <summary>
    /// Verifies enrage trigger type must be one of the valid enum values:
    /// Timer, HealthPercent, TurnCount, AllyDeath.
    /// Invalid trigger types are rejected.
    /// </summary>
    [Test]
    public void EnrageTriggerType_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid enrage trigger type
        var invalidJson = """
        {
            "bosses": [
                {
                    "id": "invalid-enrage-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid enrage trigger",
                    "baseMonsterId": "skeleton",
                    "phases": [
                        {
                            "id": "phase_1",
                            "name": "Phase 1",
                            "healthThresholdPercent": 100,
                            "behavior": "Aggressive"
                        }
                    ],
                    "enrage": {
                        "triggerType": "InvalidTrigger",
                        "triggerValue": 100,
                        "statMultipliers": { "attack": 1.5 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid enrage trigger type
        errors.Should().NotBeEmpty(
            "enrage triggerType 'InvalidTrigger' should fail validation (must be Timer, HealthPercent, TurnCount, or AllyDeath)");
    }

    /// <summary>
    /// Verifies vulnerability window duration must be >= 1.
    /// Duration of 0 would mean no vulnerability window, which is invalid.
    /// </summary>
    [Test]
    public void VulnerabilityDuration_Zero_FailsValidation()
    {
        // Arrange: JSON with vulnerability duration of 0
        var invalidJson = """
        {
            "bosses": [
                {
                    "id": "invalid-vuln-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid vulnerability duration",
                    "baseMonsterId": "skeleton",
                    "phases": [
                        {
                            "id": "phase_1",
                            "name": "Phase 1",
                            "healthThresholdPercent": 100,
                            "behavior": "Aggressive"
                        }
                    ],
                    "vulnerabilityWindows": [
                        {
                            "id": "invalid_vuln",
                            "triggerType": "AfterAbility",
                            "triggerValue": "some_ability",
                            "durationTurns": 0
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to duration < 1
        errors.Should().NotBeEmpty(
            "durationTurns 0 should fail validation (must be >= 1)");
    }
}
