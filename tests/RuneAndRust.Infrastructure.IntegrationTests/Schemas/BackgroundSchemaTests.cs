// ------------------------------------------------------------------------------
// <copyright file="BackgroundSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Integration tests for backgrounds.schema.json validation.
// Verifies schema structure, backgroundId enum validation, skill grant
// and equipment grant range constraints, and required fields.
// Version: 0.17.1c
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Integration tests validating the backgrounds.schema.json JSON Schema.
/// Tests ensure the schema correctly validates background configuration files,
/// enforces backgroundId enums, skill grant bonus ranges, equipment grant
/// constraints, and required fields.
/// </summary>
[TestFixture]
public class BackgroundSchemaTests
{
    /// <summary>
    /// Path to the backgrounds schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/backgrounds.schema.json";

    /// <summary>
    /// Path to the actual backgrounds.json configuration file.
    /// </summary>
    private const string BackgroundsJsonPath = "../../../../../config/backgrounds.json";

    /// <summary>
    /// Loaded JSON Schema for background definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the backgrounds schema.
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
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Backgrounds Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("backgroundDefinition", "should define backgroundDefinition");
        _schema.Definitions.Should().ContainKey("skillGrant", "should define skillGrant");
        _schema.Definitions.Should().ContainKey("equipmentGrant", "should define equipmentGrant");
    }

    /// <summary>
    /// Verifies the existing backgrounds.json configuration validates against the schema without errors.
    /// This is the primary acceptance test ensuring existing data is compatible.
    /// </summary>
    [Test]
    public async Task ExistingBackgroundsJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual backgrounds.json file
        var backgroundsJsonContent = await File.ReadAllTextAsync(BackgroundsJsonPath);

        // Act: Validate the backgrounds.json content against the schema
        var errors = _schema.Validate(backgroundsJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing backgrounds.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that backgroundId must be a valid enum value.
    /// Invalid backgroundId values should produce validation errors.
    /// </summary>
    [Test]
    public void BackgroundId_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid backgroundId value
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "backgroundId": "Noble",
                    "displayName": "Noble",
                    "description": "Born into wealth and privilege, you commanded respect.",
                    "selectionText": "Your blood carried weight. Your name opened doors.",
                    "professionBefore": "Aristocrat",
                    "socialStanding": "High-born, respected by all"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the backgroundId enum
        errors.Should().NotBeEmpty(
            "backgroundId 'Noble' should fail enum validation — only the 6 defined backgrounds are valid");
    }

    /// <summary>
    /// Verifies that skill grant bonusAmount must be within 0 to 5 range.
    /// Values above 5 should produce validation errors.
    /// </summary>
    [Test]
    public void SkillGrant_BonusAmountOutOfRange_FailsValidation()
    {
        // Arrange: JSON with out-of-range skill grant bonus (bonusAmount: 10)
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "backgroundId": "VillageSmith",
                    "displayName": "Village Smith",
                    "description": "You worked the forge, shaping metal into tools.",
                    "selectionText": "The ring of hammer on anvil was your morning song.",
                    "professionBefore": "Blacksmith and metalworker",
                    "socialStanding": "Respected craftsperson",
                    "skillGrants": [
                        { "skillId": "craft", "bonusAmount": 10, "grantType": "Permanent" }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the bonus range
        errors.Should().NotBeEmpty(
            "skill grant bonusAmount 10 should fail range validation (max is 5)");
    }

    /// <summary>
    /// Verifies that skill grant bonusAmount cannot be negative (minimum: 0).
    /// </summary>
    [Test]
    public void SkillGrant_NegativeBonusAmount_FailsValidation()
    {
        // Arrange: JSON with negative skill grant bonus
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "backgroundId": "VillageSmith",
                    "displayName": "Village Smith",
                    "description": "You worked the forge, shaping metal into tools.",
                    "selectionText": "The ring of hammer on anvil was your morning song.",
                    "professionBefore": "Blacksmith and metalworker",
                    "socialStanding": "Respected craftsperson",
                    "skillGrants": [
                        { "skillId": "craft", "bonusAmount": -1, "grantType": "Permanent" }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the negative value
        errors.Should().NotBeEmpty(
            "skill grant bonusAmount -1 should fail minimum validation (min is 0)");
    }

    /// <summary>
    /// Verifies that equipment grant quantity must be at least 1.
    /// Zero quantity should produce validation errors.
    /// </summary>
    [Test]
    public void EquipmentGrant_ZeroQuantity_FailsValidation()
    {
        // Arrange: JSON with zero equipment grant quantity
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "backgroundId": "ClanGuard",
                    "displayName": "Clan Guard",
                    "description": "You stood on the walls, shield in hand.",
                    "selectionText": "The weight of the shield, the length of the spear.",
                    "professionBefore": "Warrior and protector",
                    "socialStanding": "Honored defender",
                    "equipmentGrants": [
                        { "itemId": "shield", "quantity": 0, "isEquipped": true, "slot": "Shield" }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for zero quantity
        errors.Should().NotBeEmpty(
            "equipment grant quantity 0 should fail minimum validation (min is 1)");
    }

    /// <summary>
    /// Verifies that required fields must be present in background definitions.
    /// Missing required fields should produce validation errors.
    /// </summary>
    [Test]
    public void BackgroundDefinition_MissingRequiredFields_FailsValidation()
    {
        // Arrange: JSON missing required fields (no displayName, no description)
        var invalidJson = """
        {
            "backgrounds": [
                {
                    "backgroundId": "VillageSmith"
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for missing required fields
        errors.Should().NotBeEmpty(
            "background definition missing displayName, description, selectionText, " +
            "professionBefore, and socialStanding should fail validation");
    }
}
