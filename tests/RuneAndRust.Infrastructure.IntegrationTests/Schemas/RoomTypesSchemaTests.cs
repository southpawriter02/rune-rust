// ------------------------------------------------------------------------------
// <copyright file="RoomTypesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for room-types.schema.json validation.
// Verifies schema structure, multiplier validation, lighting override enum validation,
// connection rules validation (secret passage chance, connection limits),
// and feature spawn rules validation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the room-types.schema.json JSON Schema.
/// Tests ensure the schema correctly validates room type configuration files,
/// enforces multiplier ranges, lighting enums, connection rules, and feature spawn rules.
/// </summary>
/// <remarks>
/// <para>
/// The room type schema validates configurations for room categories including:
/// <list type="bullet">
/// <item><description>Multipliers (monster spawn, loot, experience)</description></item>
/// <item><description>Display configuration (entry descriptions, indicators)</description></item>
/// <item><description>Behavioral configuration (allows rest, lighting override)</description></item>
/// <item><description>Connection rules (door types, secret passages, connection limits)</description></item>
/// <item><description>Feature spawn rules (spawn chances, quantities, conditions)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class RoomTypesSchemaTests
{
    /// <summary>
    /// Path to the room types schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/room-types.schema.json";

    /// <summary>
    /// Path to the actual room-types.json configuration file.
    /// </summary>
    private const string RoomTypesJsonPath = "../../../../../config/room-types.json";

    /// <summary>
    /// Loaded JSON Schema for room type definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the room types schema.
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
    /// Validates that all required definitions are present (RoomTypeDefinition, ConnectionRules,
    /// FeatureSpawnRule, SpawnConditions).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 4 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Room Type Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (4 total)
        _schema.Definitions.Should().ContainKey("RoomTypeDefinition", "should define RoomTypeDefinition");
        _schema.Definitions.Should().ContainKey("ConnectionRules", "should define ConnectionRules");
        _schema.Definitions.Should().ContainKey("FeatureSpawnRule", "should define FeatureSpawnRule");
        _schema.Definitions.Should().ContainKey("SpawnConditions", "should define SpawnConditions");
    }

    /// <summary>
    /// Verifies the existing room-types.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 6 existing room types (standard, treasure, trap, boss, safe, shrine) are valid.
    /// </summary>
    /// <remarks>
    /// This test ensures that the schema:
    /// <list type="bullet">
    /// <item><description>Validates all required fields for each room type</description></item>
    /// <item><description>Accepts valid multiplier values</description></item>
    /// <item><description>Accepts valid boolean allowsRest values</description></item>
    /// <item><description>Validates description pools as string arrays</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public async Task RoomTypesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual room-types.json file
        var roomTypesJsonContent = await File.ReadAllTextAsync(RoomTypesJsonPath);

        // Act: Validate the room-types.json content against the schema
        var errors = _schema.Validate(roomTypesJsonContent);

        // Assert: No validation errors - all 6 existing room types should be valid
        errors.Should().BeEmpty(
            "existing room-types.json with 6 room types should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that monsterSpawnMultiplier must be non-negative.
    /// Negative values should fail validation.
    /// </summary>
    /// <remarks>
    /// Multiplier validation rules:
    /// <list type="bullet">
    /// <item><description>0.0: No monsters spawn (valid)</description></item>
    /// <item><description>1.0: Normal spawn rate (valid)</description></item>
    /// <item><description>Negative values: Invalid</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void MonsterSpawnMultiplier_Negative_FailsValidation()
    {
        // Arrange: JSON with negative monster spawn multiplier
        var invalidJson = """
        {
            "roomTypes": {
                "test-room": {
                    "id": "test-room",
                    "name": "Test Room",
                    "monsterSpawnMultiplier": -0.5
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative multiplier
        errors.Should().NotBeEmpty("negative monsterSpawnMultiplier -0.5 should fail validation (minimum is 0)");
    }

    /// <summary>
    /// Verifies that lightingOverride must be one of the valid enum values
    /// (dark, dim, normal, bright). Invalid values should fail validation.
    /// </summary>
    /// <remarks>
    /// Lighting levels affect gameplay:
    /// <list type="bullet">
    /// <item><description>dark: No natural light, severely reduced sight</description></item>
    /// <item><description>dim: Minimal light, reduced sight range</description></item>
    /// <item><description>normal: Standard lighting, normal visibility</description></item>
    /// <item><description>bright: Ample light, extended sight range</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void LightingOverride_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid lighting override value "pitch-black"
        var invalidJson = """
        {
            "roomTypes": {
                "test-room": {
                    "id": "test-room",
                    "name": "Test Room",
                    "lightingOverride": "pitch-black"
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid lighting enum value
        errors.Should().NotBeEmpty("invalid lightingOverride 'pitch-black' should fail validation (valid: dark, dim, normal, bright)");
    }

    /// <summary>
    /// Verifies that secretPassageChance must be between 0 and 1 (inclusive).
    /// Values outside this probability range should fail validation.
    /// </summary>
    /// <remarks>
    /// Secret passage chance represents a probability:
    /// <list type="bullet">
    /// <item><description>0.0: Never generate secret passages (valid)</description></item>
    /// <item><description>0.15: 15% chance (valid)</description></item>
    /// <item><description>1.0: Always generate secret passage (valid)</description></item>
    /// <item><description>> 1.0 or < 0.0: Invalid probability</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void SecretPassageChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with secret passage chance outside valid range (1.5 > 1.0)
        var invalidJson = """
        {
            "roomTypes": {
                "test-room": {
                    "id": "test-room",
                    "name": "Test Room",
                    "connectionRules": {
                        "secretPassageChance": 1.5
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range secret passage chance
        errors.Should().NotBeEmpty("secretPassageChance 1.5 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies that minConnections must be at least 1.
    /// Values less than 1 (including 0 and negative) should fail validation.
    /// </summary>
    /// <remarks>
    /// Connection limits constrain room topology:
    /// <list type="bullet">
    /// <item><description>minConnections >= 1: Room must be reachable</description></item>
    /// <item><description>maxConnections >= 1: Room must have at least potential for exit</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void MinConnections_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with minConnections less than minimum (0 < 1)
        var invalidJson = """
        {
            "roomTypes": {
                "test-room": {
                    "id": "test-room",
                    "name": "Test Room",
                    "connectionRules": {
                        "minConnections": 0
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to minConnections less than minimum
        errors.Should().NotBeEmpty("minConnections 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that spawnChance in FeatureSpawnRule must be between 0 and 1.
    /// Values outside this probability range should fail validation.
    /// </summary>
    /// <remarks>
    /// Spawn chance represents a probability:
    /// <list type="bullet">
    /// <item><description>0.0: Never spawn (valid)</description></item>
    /// <item><description>0.8: 80% chance to spawn (valid)</description></item>
    /// <item><description>1.0: Always spawn (valid)</description></item>
    /// <item><description>> 1.0 or < 0.0: Invalid probability</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void SpawnChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with spawn chance outside valid range (1.5 > 1.0)
        var invalidJson = """
        {
            "roomTypes": {
                "test-room": {
                    "id": "test-room",
                    "name": "Test Room",
                    "featureSpawnRules": [
                        {
                            "featureId": "chest-wooden",
                            "spawnChance": 1.5
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range spawn chance
        errors.Should().NotBeEmpty("spawnChance 1.5 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies that room type ID must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// <item><description>No spaces, underscores, or special characters</description></item>
    /// <item><description>Valid examples: treasure, safe-haven, boss-chamber-1</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void RoomTypeId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid room type ID (uppercase, spaces)
        var invalidJson = """
        {
            "roomTypes": {
                "Test Room": {
                    "id": "Test Room",
                    "name": "Test Room"
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("room type ID 'Test Room' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that lootMultiplier must be non-negative.
    /// Negative values should fail validation.
    /// </summary>
    /// <remarks>
    /// Loot multiplier validation:
    /// <list type="bullet">
    /// <item><description>0.0: No loot drops (valid)</description></item>
    /// <item><description>1.0: Normal loot (valid)</description></item>
    /// <item><description>3.0: Triple loot for boss rooms (valid)</description></item>
    /// <item><description>Negative values: Invalid</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void LootMultiplier_Negative_FailsValidation()
    {
        // Arrange: JSON with negative loot multiplier
        var invalidJson = """
        {
            "roomTypes": {
                "test-room": {
                    "id": "test-room",
                    "name": "Test Room",
                    "lootMultiplier": -1.0
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative multiplier
        errors.Should().NotBeEmpty("negative lootMultiplier -1.0 should fail validation (minimum is 0)");
    }
}
