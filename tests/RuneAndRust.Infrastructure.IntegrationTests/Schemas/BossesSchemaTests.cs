// ------------------------------------------------------------------------------
// <copyright file="BossesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for bosses.schema.json validation.
// Verifies schema structure, multi-phase boss definitions, health threshold
// range validation (0-100), behavior enum validation (6 values), phases array
// minimum validation (minItems: 1), and backwards compatibility with existing
// bosses.json containing 4 example bosses.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the bosses.schema.json JSON Schema.
/// Tests ensure the schema correctly validates boss encounter definitions,
/// enforces health thresholds (0-100%), behavior enum values (6 patterns),
/// phases array minimum (1+), and required fields.
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
    /// Validates that all 5 definitions are present:
    /// BossDefinition, BossPhase, StatModifier, BossLoot, SummonConfig.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Boss Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all 5 definitions are present
        _schema.Definitions.Should().ContainKey("BossDefinition", "should define BossDefinition");
        _schema.Definitions.Should().ContainKey("BossPhase", "should define BossPhase");
        _schema.Definitions.Should().ContainKey("StatModifier", "should define StatModifier");
        _schema.Definitions.Should().ContainKey("BossLoot", "should define BossLoot");
        _schema.Definitions.Should().ContainKey("SummonConfig", "should define SummonConfig");
    }

    /// <summary>
    /// Verifies the bosses.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 4 existing bosses are valid.
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
            "All 4 bosses must pass validation.");
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
                    "bossId": "no-phases-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with no phases",
                    "baseMonsterDefinitionId": "skeleton",
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
                    "bossId": "invalid-threshold-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid threshold",
                    "baseMonsterDefinitionId": "skeleton",
                    "phases": [
                        {
                            "phaseNumber": 1,
                            "name": "Phase 1",
                            "healthThreshold": 150,
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
            "healthThreshold 150 should fail validation (must be 0-100)");
    }

    /// <summary>
    /// Verifies behavior must be one of the valid enum values:
    /// Aggressive, Defensive, Tactical, Berserk, Summoner, Enraged.
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
                    "bossId": "invalid-behavior-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid behavior",
                    "baseMonsterDefinitionId": "skeleton",
                    "phases": [
                        {
                            "phaseNumber": 1,
                            "name": "Phase 1",
                            "healthThreshold": 100,
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
            "behavior 'InvalidBehavior' should fail validation (must be Aggressive, Defensive, Tactical, Berserk, Summoner, or Enraged)");
    }

    /// <summary>
    /// Verifies that loot drop chance must be between 0 and 1.
    /// Values outside this range are rejected.
    /// </summary>
    [Test]
    public void LootChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with loot chance above 1.0
        var invalidJson = """
        {
            "bosses": [
                {
                    "bossId": "invalid-loot-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid loot chance",
                    "baseMonsterDefinitionId": "skeleton",
                    "phases": [
                        {
                            "phaseNumber": 1,
                            "name": "Phase 1",
                            "healthThreshold": 100,
                            "behavior": "Aggressive"
                        }
                    ],
                    "loot": [
                        {
                            "itemId": "gold",
                            "amount": 100,
                            "chance": 1.5
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to chance > 1.0
        errors.Should().NotBeEmpty(
            "loot chance 1.5 should fail validation (must be 0.0-1.0)");
    }

    /// <summary>
    /// Verifies that summon count must be at least 1.
    /// Zero summons is invalid.
    /// </summary>
    [Test]
    public void SummonCount_Zero_FailsValidation()
    {
        // Arrange: JSON with summon count of 0
        var invalidJson = """
        {
            "bosses": [
                {
                    "bossId": "invalid-summon-boss",
                    "name": "Invalid Boss",
                    "description": "A boss with invalid summon count",
                    "baseMonsterDefinitionId": "skeleton",
                    "phases": [
                        {
                            "phaseNumber": 1,
                            "name": "Phase 1",
                            "healthThreshold": 100,
                            "behavior": "Summoner",
                            "summonConfig": {
                                "monsterDefinitionId": "skeleton-minion",
                                "count": 0
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to count < 1
        errors.Should().NotBeEmpty(
            "summon count 0 should fail validation (must be >= 1)");
    }
}
