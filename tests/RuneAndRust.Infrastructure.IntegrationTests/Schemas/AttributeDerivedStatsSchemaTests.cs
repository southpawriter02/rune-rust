// ------------------------------------------------------------------------------
// <copyright file="AttributeDerivedStatsSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Integration tests for the derivedStats section of attributes.schema.json
// validation. Verifies schema structure, formula definitions, bonus constraints,
// and required fields for derived stat configuration.
// Version: 0.17.2d
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Integration tests validating the derivedStats section of attributes.schema.json.
/// Tests ensure the schema correctly validates derived stat formula configuration,
/// enforces required stat definitions, and validates the existing attributes.json
/// configuration including the new derivedStats section.
/// </summary>
[TestFixture]
public class AttributeDerivedStatsSchemaTests
{
    /// <summary>
    /// Path to the attributes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/attributes.schema.json";

    /// <summary>
    /// Path to the actual attributes.json configuration file.
    /// </summary>
    private const string AttributesJsonPath = "../../../../../config/attributes.json";

    /// <summary>
    /// Loaded JSON Schema for attribute definitions including derived stats configuration.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the attributes schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCHEMA STRUCTURE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies the schema contains the derivedStatsConfiguration and derivedStatFormula
    /// definitions required for derived stat validation.
    /// </summary>
    [Test]
    public void Schema_ContainsDerivedStatsDefinitions()
    {
        // Assert
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Definitions.Should().ContainKey("derivedStatsConfiguration",
            "should define derivedStatsConfiguration");
        _schema.Definitions.Should().ContainKey("derivedStatFormula",
            "should define derivedStatFormula");
    }

    /// <summary>
    /// Verifies the schema requires the derivedStats property at the root level.
    /// </summary>
    [Test]
    public void Schema_RequiresDerivedStatsProperty()
    {
        // Assert
        _schema.RequiredProperties.Should().Contain("derivedStats",
            "derivedStats should be a required root property");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXISTING DATA VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies the existing attributes.json configuration validates against the
    /// updated schema including the derivedStats section without errors.
    /// </summary>
    [Test]
    public async Task ExistingAttributesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual attributes.json file
        var attributesJsonContent = await File.ReadAllTextAsync(AttributesJsonPath);

        // Act: Validate the attributes.json content against the schema
        var errors = _schema.Validate(attributesJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing attributes.json should validate against the schema without errors. " +
            "Errors: {0}",
            string.Join("; ", errors.Select(e => $"{e.Path}: {e.Kind}")));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INVALID DATA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that a configuration missing the derivedStats section fails validation.
    /// </summary>
    [Test]
    public void MissingDerivedStatsSection_FailsValidation()
    {
        // Arrange: Valid attributes, recommendedBuilds, and pointBuy, but missing derivedStats
        var invalidJson = """
        {
            "attributes": [
                {
                    "attribute": "Might",
                    "displayName": "MIGHT",
                    "shortDescription": "Physical power and raw strength",
                    "detailedDescription": "Might represents your character's physical power. It affects melee damage, carrying capacity, and physical feats of strength.",
                    "affectedStats": ["Melee Damage"],
                    "affectedSkills": ["Combat"]
                },
                {
                    "attribute": "Finesse",
                    "displayName": "FINESSE",
                    "shortDescription": "Agility and precision movement",
                    "detailedDescription": "Finesse represents your character's agility and hand-eye coordination. It affects ranged accuracy and evasion.",
                    "affectedStats": ["Initiative"],
                    "affectedSkills": ["Stealth"]
                },
                {
                    "attribute": "Wits",
                    "displayName": "WITS",
                    "shortDescription": "Perception and knowledge base",
                    "detailedDescription": "Wits represents your character's mental acuity and learned knowledge. It affects perception and crafting.",
                    "affectedStats": ["Perception"],
                    "affectedSkills": ["Lore"]
                },
                {
                    "attribute": "Will",
                    "displayName": "WILL",
                    "shortDescription": "Mental fortitude and magical affinity",
                    "detailedDescription": "Will represents your character's mental fortitude and connection to the Aether. It determines Aether Pool.",
                    "affectedStats": ["Aether Pool"],
                    "affectedSkills": ["Rhetoric"]
                },
                {
                    "attribute": "Sturdiness",
                    "displayName": "STURDINESS",
                    "shortDescription": "Endurance and physical resilience",
                    "detailedDescription": "Sturdiness represents your character's toughness and endurance. It is the primary determinant of maximum HP.",
                    "affectedStats": ["Max HP"],
                    "affectedSkills": ["Endurance"]
                }
            ],
            "recommendedBuilds": [
                {
                    "archetypeId": "warrior",
                    "might": 4,
                    "finesse": 3,
                    "wits": 2,
                    "will": 2,
                    "sturdiness": 4,
                    "totalPoints": 15
                }
            ],
            "pointBuy": {
                "startingPoints": 15,
                "adeptStartingPoints": 14,
                "minAttributeValue": 1,
                "maxAttributeValue": 10,
                "costTable": [
                    { "targetValue": 2, "individualCost": 1, "cumulativeCost": 1 }
                ]
            }
        }
        """;

        // Act
        var errors = _schema.Validate(invalidJson);

        // Assert
        errors.Should().NotBeEmpty("missing derivedStats section should cause validation failure");
    }
}
