// ------------------------------------------------------------------------------
// <copyright file="FactionsSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for factions.schema.json validation.
// Verifies schema structure, faction identity validation, threshold ranges,
// relationship enums, reputation modifiers, and perk structure.
// Part of v0.14.10a implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the factions.schema.json JSON Schema.
/// Tests ensure the schema correctly validates faction configuration files,
/// enforces faction identity patterns, threshold range constraints,
/// relationship type validation, and perk threshold requirements.
/// </summary>
/// <remarks>
/// <para>
/// The factions schema provides 8 definitions:
/// <list type="bullet">
/// <item><description>Faction - Complete faction definition with identity, thresholds, and benefits</description></item>
/// <item><description>ReputationThresholds - Six threshold levels from hostile to exalted</description></item>
/// <item><description>ThresholdRange - Single threshold with min/max/label</description></item>
/// <item><description>FactionRelationship - Relationship between factions with propagation</description></item>
/// <item><description>ReputationModifier - Action that modifies reputation</description></item>
/// <item><description>ModifierCondition - Conditional modifier for reputation changes</description></item>
/// <item><description>FactionPerk - Benefit unlocked at reputation threshold</description></item>
/// <item><description>VendorAccess - Faction-specific vendor access rules</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class FactionsSchemaTests
{
    /// <summary>
    /// Path to the factions schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/factions.schema.json";

    /// <summary>
    /// Path to the factions.json configuration file for validation.
    /// </summary>
    private const string FactionsConfigPath = "../../../../../config/factions.json";

    /// <summary>
    /// Loaded JSON Schema for faction definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the factions schema.
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

    #region FAC-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// FAC-001: Verifies that factions.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>All required definitions are present (8 total)</description></item>
    /// <item><description>Schema title and type are correct</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void FactionsSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Faction Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "schema type should be object");

        // Assert: All 8 definitions should be present
        _schema.Definitions.Should().ContainKey("Faction", "should define Faction");
        _schema.Definitions.Should().ContainKey("ReputationThresholds", "should define ReputationThresholds");
        _schema.Definitions.Should().ContainKey("ThresholdRange", "should define ThresholdRange");
        _schema.Definitions.Should().ContainKey("FactionRelationship", "should define FactionRelationship");
        _schema.Definitions.Should().ContainKey("ReputationModifier", "should define ReputationModifier");
        _schema.Definitions.Should().ContainKey("ModifierCondition", "should define ModifierCondition");
        _schema.Definitions.Should().ContainKey("FactionPerk", "should define FactionPerk");
        _schema.Definitions.Should().ContainKey("VendorAccess", "should define VendorAccess");
    }

    /// <summary>
    /// FAC-001: Verifies that the actual factions.json configuration passes validation.
    /// </summary>
    [Test]
    public async Task FactionsSchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual factions.json file
        var jsonContent = await File.ReadAllTextAsync(FactionsConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing factions.json should validate against schema without errors");
    }

    #endregion

    #region FAC-002: Faction Identity Validation

    /// <summary>
    /// FAC-002: Verifies that valid faction IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("rust-clans")]
    [TestCase("midgard-combine")]
    [TestCase("iron-banes")]
    [TestCase("dvergr-guild")]
    [TestCase("a1")]
    public void FactionIdentity_ValidId_PassesValidation(string id)
    {
        // Arrange: Valid faction with proper kebab-case ID
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "{{id}}",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"faction with valid ID '{id}' should pass validation");
    }

    /// <summary>
    /// FAC-002: Verifies that invalid faction IDs fail validation (uppercase).
    /// </summary>
    [Test]
    public void FactionIdentity_UppercaseId_FailsValidation()
    {
        // Arrange: Faction with uppercase ID (invalid pattern)
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "RustClans",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "faction with uppercase ID should fail validation");
    }

    /// <summary>
    /// FAC-002: Verifies that invalid faction IDs fail validation (underscore).
    /// </summary>
    [Test]
    public void FactionIdentity_UnderscoreId_FailsValidation()
    {
        // Arrange: Faction with underscore ID (invalid pattern)
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "rust_clans",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "faction with underscore in ID should fail validation");
    }

    /// <summary>
    /// FAC-002: Verifies that empty faction name fails validation.
    /// </summary>
    [Test]
    public void FactionIdentity_EmptyName_FailsValidation()
    {
        // Arrange: Faction with empty name
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "faction with empty name should fail validation");
    }

    /// <summary>
    /// FAC-002: Verifies that short description fails validation.
    /// </summary>
    [Test]
    public void FactionIdentity_ShortDescription_FailsValidation()
    {
        // Arrange: Faction with short description (less than 10 chars)
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "Short"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "faction with description less than 10 characters should fail validation");
    }

    #endregion

    #region FAC-003: Threshold Range Validation

    /// <summary>
    /// FAC-003: Verifies that valid default thresholds pass validation.
    /// </summary>
    [Test]
    public void ThresholdRange_ValidDefaultThresholds_PassesValidation()
    {
        // Arrange: Valid default thresholds covering full range
        var validJson = """
        {
            "version": "1.0.0",
            "defaultThresholds": {
                "hostile": { "min": -1000, "max": -501, "label": "Hostile" },
                "unfriendly": { "min": -500, "max": -1, "label": "Unfriendly" },
                "neutral": { "min": 0, "max": 499, "label": "Neutral" },
                "friendly": { "min": 500, "max": 2999, "label": "Friendly" },
                "allied": { "min": 3000, "max": 8999, "label": "Allied" },
                "exalted": { "min": 9000, "max": 10000, "label": "Exalted" }
            },
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "valid default thresholds should pass validation");
    }

    /// <summary>
    /// FAC-003: Verifies that missing threshold level fails validation.
    /// </summary>
    [Test]
    public void ThresholdRange_MissingThresholdLevel_FailsValidation()
    {
        // Arrange: Thresholds missing 'allied' level
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultThresholds": {
                "hostile": { "min": -1000, "max": -501, "label": "Hostile" },
                "unfriendly": { "min": -500, "max": -1, "label": "Unfriendly" },
                "neutral": { "min": 0, "max": 499, "label": "Neutral" },
                "friendly": { "min": 500, "max": 2999, "label": "Friendly" },
                "exalted": { "min": 9000, "max": 10000, "label": "Exalted" }
            },
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "thresholds missing 'allied' level should fail validation");
    }

    /// <summary>
    /// FAC-003: Verifies that threshold with empty label fails validation.
    /// </summary>
    [Test]
    public void ThresholdRange_EmptyLabel_FailsValidation()
    {
        // Arrange: Threshold with empty label
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultThresholds": {
                "hostile": { "min": -1000, "max": -501, "label": "" },
                "unfriendly": { "min": -500, "max": -1, "label": "Unfriendly" },
                "neutral": { "min": 0, "max": 499, "label": "Neutral" },
                "friendly": { "min": 500, "max": 2999, "label": "Friendly" },
                "allied": { "min": 3000, "max": 8999, "label": "Allied" },
                "exalted": { "min": 9000, "max": 10000, "label": "Exalted" }
            },
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "threshold with empty label should fail validation");
    }

    #endregion

    #region FAC-004: Relationship Enum Validation

    /// <summary>
    /// FAC-004: Verifies that all valid relationship types pass validation.
    /// </summary>
    [Test]
    [TestCase("Allied")]
    [TestCase("Friendly")]
    [TestCase("Neutral")]
    [TestCase("Unfriendly")]
    [TestCase("Hostile")]
    [TestCase("AtWar")]
    public void RelationshipType_ValidEnum_PassesValidation(string relationshipType)
    {
        // Arrange: Faction with valid relationship type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "relationships": [
                        {
                            "factionId": "other-faction",
                            "relationship": "{{relationshipType}}"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"relationship type '{relationshipType}' should pass validation");
    }

    /// <summary>
    /// FAC-004: Verifies that invalid relationship type fails validation.
    /// </summary>
    [Test]
    public void RelationshipType_InvalidEnum_FailsValidation()
    {
        // Arrange: Faction with invalid relationship type
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "relationships": [
                        {
                            "factionId": "other-faction",
                            "relationship": "Enemy"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "relationship type 'Enemy' should fail validation (not in enum)");
    }

    /// <summary>
    /// FAC-004: Verifies that lowercase relationship type fails validation (case-sensitive).
    /// </summary>
    [Test]
    public void RelationshipType_LowercaseEnum_FailsValidation()
    {
        // Arrange: Faction with lowercase relationship type
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "relationships": [
                        {
                            "factionId": "other-faction",
                            "relationship": "allied"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "lowercase 'allied' relationship type should fail validation (case-sensitive)");
    }

    /// <summary>
    /// FAC-004: Verifies that relationship with reputationLink passes validation.
    /// </summary>
    [Test]
    public void RelationshipType_WithReputationLink_PassesValidation()
    {
        // Arrange: Faction with relationship including reputationLink
        var validJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "relationships": [
                        {
                            "factionId": "other-faction",
                            "relationship": "Friendly",
                            "reputationLink": 50,
                            "description": "Trade partners"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "relationship with reputationLink should pass validation");
    }

    #endregion

    #region FAC-005: Reputation Modifier Validation

    /// <summary>
    /// FAC-005: Verifies that all valid action types pass validation.
    /// </summary>
    [Test]
    [TestCase("KillMember")]
    [TestCase("HelpMember")]
    [TestCase("CompleteQuest")]
    [TestCase("FailQuest")]
    [TestCase("Theft")]
    [TestCase("Gift")]
    [TestCase("Betrayal")]
    [TestCase("Discovery")]
    public void ReputationModifier_ValidActionType_PassesValidation(string actionType)
    {
        // Arrange: Faction with valid action type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "reputationModifiers": [
                        {
                            "action": "{{actionType}}",
                            "amount": 10,
                            "description": "Test action description here"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"action type '{actionType}' should pass validation");
    }

    /// <summary>
    /// FAC-005: Verifies that invalid action type fails validation.
    /// </summary>
    [Test]
    public void ReputationModifier_InvalidActionType_FailsValidation()
    {
        // Arrange: Faction with invalid action type
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "reputationModifiers": [
                        {
                            "action": "InvalidAction",
                            "amount": 10,
                            "description": "Test action description here"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "action type 'InvalidAction' should fail validation");
    }

    /// <summary>
    /// FAC-005: Verifies that modifier missing amount fails validation.
    /// </summary>
    [Test]
    public void ReputationModifier_MissingAmount_FailsValidation()
    {
        // Arrange: Modifier missing required amount field
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "reputationModifiers": [
                        {
                            "action": "KillMember",
                            "description": "Killing a faction member"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "modifier missing 'amount' should fail validation");
    }

    /// <summary>
    /// FAC-005: Verifies that modifier missing description fails validation.
    /// </summary>
    [Test]
    public void ReputationModifier_MissingDescription_FailsValidation()
    {
        // Arrange: Modifier missing required description field
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "reputationModifiers": [
                        {
                            "action": "KillMember",
                            "amount": -20
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "modifier missing 'description' should fail validation");
    }

    /// <summary>
    /// FAC-005: Verifies that modifier with conditions passes validation.
    /// </summary>
    [Test]
    public void ReputationModifier_WithConditions_PassesValidation()
    {
        // Arrange: Modifier with conditions array
        var validJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "reputationModifiers": [
                        {
                            "action": "KillMember",
                            "amount": -20,
                            "conditions": [
                                { "type": "Witnessed", "modifier": 1.0 },
                                { "type": "Unwitnessed", "modifier": 0.0 }
                            ],
                            "description": "Killing a faction member"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "modifier with conditions should pass validation");
    }

    #endregion

    #region FAC-006: Faction Perk Validation

    /// <summary>
    /// FAC-006: Verifies that valid perk with friendly threshold passes validation.
    /// </summary>
    [Test]
    [TestCase("friendly")]
    [TestCase("allied")]
    [TestCase("exalted")]
    public void FactionPerk_ValidThreshold_PassesValidation(string threshold)
    {
        // Arrange: Perk with valid threshold
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "perks": [
                        {
                            "id": "test-perk",
                            "name": "Test Perk",
                            "description": "A test perk for validation purposes.",
                            "requiredThreshold": "{{threshold}}",
                            "effect": {
                                "type": "PriceModifier",
                                "target": "FactionVendors",
                                "value": -0.15
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"perk with '{threshold}' threshold should pass validation");
    }

    /// <summary>
    /// FAC-006: Verifies that perk with neutral threshold fails validation.
    /// </summary>
    [Test]
    public void FactionPerk_NeutralThreshold_FailsValidation()
    {
        // Arrange: Perk with neutral threshold (not valid for perks)
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "perks": [
                        {
                            "id": "test-perk",
                            "name": "Test Perk",
                            "description": "A test perk for validation purposes.",
                            "requiredThreshold": "neutral",
                            "effect": {
                                "type": "PriceModifier",
                                "target": "FactionVendors",
                                "value": -0.15
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "perk with 'neutral' threshold should fail validation (not valid for perks)");
    }

    /// <summary>
    /// FAC-006: Verifies that perk missing effect fails validation.
    /// </summary>
    [Test]
    public void FactionPerk_MissingEffect_FailsValidation()
    {
        // Arrange: Perk missing required effect field
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "perks": [
                        {
                            "id": "test-perk",
                            "name": "Test Perk",
                            "description": "A test perk for validation purposes.",
                            "requiredThreshold": "friendly"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "perk missing 'effect' should fail validation");
    }

    /// <summary>
    /// FAC-006: Verifies that all valid effect types pass validation.
    /// </summary>
    [Test]
    [TestCase("PriceModifier")]
    [TestCase("StatBonus")]
    [TestCase("AccessGrant")]
    [TestCase("AbilityGrant")]
    [TestCase("ReputationBonus")]
    public void FactionPerk_ValidEffectType_PassesValidation(string effectType)
    {
        // Arrange: Perk with valid effect type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "perks": [
                        {
                            "id": "test-perk",
                            "name": "Test Perk",
                            "description": "A test perk for validation purposes.",
                            "requiredThreshold": "friendly",
                            "effect": {
                                "type": "{{effectType}}",
                                "target": "TestTarget",
                                "value": 10
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"perk with effect type '{effectType}' should pass validation");
    }

    /// <summary>
    /// FAC-006: Verifies that perk with invalid effect type fails validation.
    /// </summary>
    [Test]
    public void FactionPerk_InvalidEffectType_FailsValidation()
    {
        // Arrange: Perk with invalid effect type
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "perks": [
                        {
                            "id": "test-perk",
                            "name": "Test Perk",
                            "description": "A test perk for validation purposes.",
                            "requiredThreshold": "friendly",
                            "effect": {
                                "type": "InvalidType",
                                "target": "TestTarget",
                                "value": 10
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "perk with invalid effect type should fail validation");
    }

    #endregion

    #region Additional Validation Tests

    /// <summary>
    /// Verifies that vendor access with valid threshold passes validation.
    /// </summary>
    [Test]
    [TestCase("hostile")]
    [TestCase("unfriendly")]
    [TestCase("neutral")]
    [TestCase("friendly")]
    [TestCase("allied")]
    [TestCase("exalted")]
    public void VendorAccess_ValidThreshold_PassesValidation(string threshold)
    {
        // Arrange: Vendor access with valid threshold
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "vendors": [
                        {
                            "vendorId": "test-vendor",
                            "requiredThreshold": "{{threshold}}"
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"vendor access with '{threshold}' threshold should pass validation");
    }

    /// <summary>
    /// Verifies that vendor access with exclusive items passes validation.
    /// </summary>
    [Test]
    public void VendorAccess_WithExclusiveItems_PassesValidation()
    {
        // Arrange: Vendor access with exclusive items array
        var validJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "vendors": [
                        {
                            "vendorId": "test-vendor",
                            "requiredThreshold": "allied",
                            "priceModifier": 0.9,
                            "exclusiveItems": ["item-1", "item-2", "item-3"]
                        }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "vendor access with exclusive items should pass validation");
    }

    /// <summary>
    /// Verifies that valid hex color passes validation.
    /// </summary>
    [Test]
    [TestCase("#B87333")]
    [TestCase("#8B4513")]
    [TestCase("#8B0000")]
    [TestCase("#4A4A4A")]
    [TestCase("#FFFFFF")]
    [TestCase("#000000")]
    public void FactionColor_ValidHexColor_PassesValidation(string color)
    {
        // Arrange: Faction with valid hex color
        var validJson = $$"""
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "color": "{{color}}"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"faction with color '{color}' should pass validation");
    }

    /// <summary>
    /// Verifies that invalid hex color fails validation.
    /// </summary>
    [Test]
    public void FactionColor_InvalidHexColor_FailsValidation()
    {
        // Arrange: Faction with invalid hex color (missing #)
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes.",
                    "color": "B87333"
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "faction with hex color missing '#' should fail validation");
    }

    /// <summary>
    /// Verifies that hidden faction flag is accepted.
    /// </summary>
    [Test]
    public void FactionHidden_TrueValue_PassesValidation()
    {
        // Arrange: Hidden faction
        var validJson = """
        {
            "version": "1.0.0",
            "factions": [
                {
                    "id": "secret-faction",
                    "name": "Secret Faction",
                    "description": "A secret faction hidden until discovered.",
                    "isHidden": true
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "hidden faction should pass validation");
    }

    /// <summary>
    /// Verifies that version must match semver pattern.
    /// </summary>
    [Test]
    public void Version_InvalidFormat_FailsValidation()
    {
        // Arrange: Invalid version format
        var invalidJson = """
        {
            "version": "v1.0",
            "factions": [
                {
                    "id": "test-faction",
                    "name": "Test Faction",
                    "description": "A test faction for validation purposes."
                }
            ]
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "version 'v1.0' should fail validation (not semver format)");
    }

    /// <summary>
    /// Verifies that empty factions array fails validation.
    /// </summary>
    [Test]
    public void Factions_EmptyArray_FailsValidation()
    {
        // Arrange: Empty factions array
        var invalidJson = """
        {
            "version": "1.0.0",
            "factions": []
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "empty factions array should fail validation (minItems: 1)");
    }

    #endregion
}
