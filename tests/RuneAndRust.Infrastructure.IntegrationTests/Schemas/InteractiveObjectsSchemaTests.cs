// ------------------------------------------------------------------------------
// <copyright file="InteractiveObjectsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for interactive-objects.schema.json validation.
// Verifies schema structure, object type enum validation, state enum validation,
// interaction enum validation, skill check DC validation, trigger chance range,
// item chance range, and backward compatibility with existing interactive-objects.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the interactive-objects.schema.json JSON Schema.
/// Tests ensure the schema correctly validates interactive object configuration files,
/// enforces object type enums, state enums, interaction enums, skill check DCs,
/// reward configurations, and trap effects.
/// </summary>
/// <remarks>
/// <para>
/// The interactive objects schema validates configurations including:
/// <list type="bullet">
/// <item><description>Object types (14 values: Door, Chest, Lever, etc.)</description></item>
/// <item><description>States (12 values: Closed, Open, Locked, etc.)</description></item>
/// <item><description>Interactions (12 values: Open, Close, Examine, etc.)</description></item>
/// <item><description>Skill checks with DC validation</description></item>
/// <item><description>State transitions with from/to/via</description></item>
/// <item><description>Rewards with loot tables, items, gold, XP</description></item>
/// <item><description>Trap effects with damage and saving throws</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class InteractiveObjectsSchemaTests
{
    /// <summary>
    /// Path to the interactive objects schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/interactive-objects.schema.json";

    /// <summary>
    /// Path to the actual interactive-objects.json configuration file.
    /// </summary>
    private const string InteractiveObjectsJsonPath = "../../../../../config/interactive-objects.json";

    /// <summary>
    /// Loaded JSON Schema for interactive object definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the interactive objects schema.
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
    /// Validates that all required definitions are present (8 total).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 8 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Interactive Object Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (8 total)
        _schema.Definitions.Should().ContainKey("InteractiveObjectDefinition", "should define InteractiveObjectDefinition");
        _schema.Definitions.Should().ContainKey("SkillCheck", "should define SkillCheck");
        _schema.Definitions.Should().ContainKey("StateTransition", "should define StateTransition");
        _schema.Definitions.Should().ContainKey("Reward", "should define Reward");
        _schema.Definitions.Should().ContainKey("ItemReward", "should define ItemReward");
        _schema.Definitions.Should().ContainKey("QuantityRange", "should define QuantityRange");
        _schema.Definitions.Should().ContainKey("TrapEffect", "should define TrapEffect");
        _schema.Definitions.Should().ContainKey("SavingThrow", "should define SavingThrow");
    }

    /// <summary>
    /// Verifies the existing interactive-objects.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 12 existing interactive objects are valid.
    /// </summary>
    /// <remarks>
    /// This test ensures that the schema validates all existing objects:
    /// iron-door-basic, wooden-door-basic, wooden-chest-basic, iron-chest-basic,
    /// rusty-lever-basic, wooden-crate-basic, storage-barrel-basic, spider-web-basic,
    /// stone-statue-basic, ancient-altar-basic, wall-torch-basic, stone-inscription-basic.
    /// </remarks>
    [Test]
    public async Task InteractiveObjectsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual interactive-objects.json file
        var jsonContent = await File.ReadAllTextAsync(InteractiveObjectsJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - all 12 existing interactive objects should be valid
        errors.Should().BeEmpty(
            "existing interactive-objects.json with 12 objects should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that objectType must be one of the 14 valid enum values.
    /// Invalid object types should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid object types: Door, Chest, Lever, Altar, Barrel, Crate, Web,
    /// Statue, LightSource, Inscription, Trap, Switch, Bookshelf, Fountain.
    /// </remarks>
    [Test]
    public void ObjectType_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid object type "Table"
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-object",
                    "name": "Test Object",
                    "description": "A test object.",
                    "objectType": "Table",
                    "defaultState": "Normal",
                    "allowedInteractions": ["Examine"],
                    "keywords": ["test"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid object type enum value
        errors.Should().NotBeEmpty("invalid objectType 'Table' should fail validation (14 valid types)");
    }

    /// <summary>
    /// Verifies that defaultState must be one of the 12 valid enum values.
    /// Invalid states should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid states: Closed, Open, Locked, Broken, Lit, Unlit, Active, Inactive,
    /// Normal, Triggered, Down, Up.
    /// </remarks>
    [Test]
    public void DefaultState_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid default state "Hidden"
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-object",
                    "name": "Test Object",
                    "description": "A test object.",
                    "objectType": "Chest",
                    "defaultState": "Hidden",
                    "allowedInteractions": ["Examine"],
                    "keywords": ["test"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid default state enum value
        errors.Should().NotBeEmpty("invalid defaultState 'Hidden' should fail validation (12 valid states)");
    }

    /// <summary>
    /// Verifies that allowedInteractions must contain valid enum values.
    /// Invalid interactions should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid interactions: Open, Close, Examine, Search, Activate, Break,
    /// Unlock, Lock, Read, Use, Push, Pull.
    /// </remarks>
    [Test]
    public void AllowedInteractions_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid interaction "Destroy"
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-object",
                    "name": "Test Object",
                    "description": "A test object.",
                    "objectType": "Chest",
                    "defaultState": "Closed",
                    "allowedInteractions": ["Destroy"],
                    "keywords": ["test"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid interaction enum value
        errors.Should().NotBeEmpty("invalid interaction 'Destroy' should fail validation (12 valid interactions)");
    }

    /// <summary>
    /// Verifies that skillChecks.dc must be at least 1.
    /// Values less than 1 (including 0 and negative) should fail validation.
    /// </summary>
    /// <remarks>
    /// Difficulty class must be positive for skill checks to be meaningful.
    /// </remarks>
    [Test]
    public void SkillCheckDC_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with skill check DC less than minimum (0 < 1)
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-object",
                    "name": "Test Object",
                    "description": "A test object.",
                    "objectType": "Chest",
                    "defaultState": "Locked",
                    "allowedInteractions": ["Unlock", "Examine"],
                    "keywords": ["test"],
                    "skillChecks": {
                        "Unlock": {
                            "skillId": "lockpicking",
                            "dc": 0
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to dc less than minimum
        errors.Should().NotBeEmpty("skillCheck.dc 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that trapEffect.triggerChance must be between 0 and 1 (inclusive).
    /// Values outside this probability range should fail validation.
    /// </summary>
    /// <remarks>
    /// Trigger chance is a probability:
    /// <list type="bullet">
    /// <item><description>1.0: 100% - Always triggers</description></item>
    /// <item><description>0.5: 50% - Half the time</description></item>
    /// <item><description>0.0: Never triggers</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void TriggerChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with trigger chance outside valid range (1.5 > 1.0)
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-trap",
                    "name": "Test Trap",
                    "description": "A test trap.",
                    "objectType": "Trap",
                    "defaultState": "Normal",
                    "allowedInteractions": ["Examine"],
                    "keywords": ["trap"],
                    "trapEffect": {
                        "damage": "1d6",
                        "damageType": "piercing",
                        "triggerChance": 1.5
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range trigger chance
        errors.Should().NotBeEmpty("triggerChance 1.5 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies that ItemReward.chance must be between 0 and 1 (inclusive).
    /// Values outside this probability range should fail validation.
    /// </summary>
    /// <remarks>
    /// Item reward chance is a probability:
    /// <list type="bullet">
    /// <item><description>1.0: Guaranteed item</description></item>
    /// <item><description>0.5: 50% chance</description></item>
    /// <item><description>0.0: Never granted</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void ItemRewardChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with item reward chance outside valid range (2.0 > 1.0)
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-chest",
                    "name": "Test Chest",
                    "description": "A test chest.",
                    "objectType": "Chest",
                    "defaultState": "Closed",
                    "allowedInteractions": ["Open", "Examine"],
                    "keywords": ["chest"],
                    "rewards": {
                        "Open": {
                            "fixedItems": [
                                {
                                    "itemId": "healing-potion",
                                    "chance": 2.0
                                }
                            ]
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range item chance
        errors.Should().NotBeEmpty("itemReward.chance 2.0 should fail validation (maximum is 1.0)");
    }

    /// <summary>
    /// Verifies that object ID must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// <item><description>Valid examples: iron-door-basic, wooden-chest-1</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void ObjectId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid object ID (uppercase, spaces)
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "Iron Door Basic",
                    "name": "Iron Door",
                    "description": "A heavy iron door.",
                    "objectType": "Door",
                    "defaultState": "Closed",
                    "allowedInteractions": ["Open", "Close", "Examine"],
                    "keywords": ["door"]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("object ID 'Iron Door Basic' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that trapEffect.damage must follow the dice expression pattern.
    /// Invalid dice expressions should fail validation.
    /// </summary>
    /// <remarks>
    /// Dice expression pattern: ^[0-9]+d[0-9]+([+-][0-9]+)?$
    /// <list type="bullet">
    /// <item><description>Valid: 1d6, 2d4, 1d8+2, 2d6-1</description></item>
    /// <item><description>Invalid: d6, 1d, damage, 1d6+</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void TrapDamage_InvalidDicePattern_FailsValidation()
    {
        // Arrange: JSON with invalid dice expression "damage"
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-trap",
                    "name": "Test Trap",
                    "description": "A test trap.",
                    "objectType": "Trap",
                    "defaultState": "Normal",
                    "allowedInteractions": ["Examine"],
                    "keywords": ["trap"],
                    "trapEffect": {
                        "damage": "damage",
                        "damageType": "piercing"
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid dice expression pattern
        errors.Should().NotBeEmpty("damage 'damage' should fail pattern validation (must be dice expression like 1d6)");
    }

    /// <summary>
    /// Verifies that savingThrow.attribute in trapEffect must be one of the valid enum values.
    /// Invalid attribute values should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid saving throw attributes: might, fortitude, will, wits, finesse.
    /// </remarks>
    [Test]
    public void TrapSavingThrowAttribute_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid saving throw attribute "dexterity"
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-trap",
                    "name": "Test Trap",
                    "description": "A test trap.",
                    "objectType": "Trap",
                    "defaultState": "Normal",
                    "allowedInteractions": ["Examine"],
                    "keywords": ["trap"],
                    "trapEffect": {
                        "damage": "1d6",
                        "damageType": "piercing",
                        "savingThrow": {
                            "attribute": "dexterity",
                            "dc": 12
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid attribute enum value
        errors.Should().NotBeEmpty("savingThrow.attribute 'dexterity' should fail validation (valid: might, fortitude, will, wits, finesse)");
    }

    /// <summary>
    /// Verifies that stateTransition.viaInteraction must be one of the valid enum values.
    /// Invalid interactions in state transitions should fail validation.
    /// </summary>
    /// <remarks>
    /// State transitions must use valid interaction types for the viaInteraction field.
    /// </remarks>
    [Test]
    public void StateTransitionViaInteraction_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid viaInteraction "Kick"
        var invalidJson = """
        {
            "interactiveObjects": [
                {
                    "id": "test-door",
                    "name": "Test Door",
                    "description": "A test door.",
                    "objectType": "Door",
                    "defaultState": "Closed",
                    "allowedInteractions": ["Open", "Examine"],
                    "keywords": ["door"],
                    "stateTransitions": [
                        {
                            "fromState": "Closed",
                            "toState": "Open",
                            "viaInteraction": "Kick"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid interaction enum value
        errors.Should().NotBeEmpty("viaInteraction 'Kick' should fail validation (12 valid interactions)");
    }
}
