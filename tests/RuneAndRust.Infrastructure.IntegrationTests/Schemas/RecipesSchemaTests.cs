// ------------------------------------------------------------------------------
// <copyright file="RecipesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for recipes.schema.json validation.
// Verifies schema structure, category enum validation, materials array,
// quality tier enum, discovery type enum, failure chance range, and
// backward compatibility with the existing recipes.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the recipes.schema.json JSON Schema.
/// Tests ensure the schema correctly validates crafting recipe configuration files,
/// enforces category enums, quality tiers, discovery types, and range constraints.
/// </summary>
/// <remarks>
/// <para>
/// The recipe schema validates configurations including:
/// <list type="bullet">
/// <item><description>Recipe IDs (kebab-case pattern)</description></item>
/// <item><description>Category enum (9 crafting categories)</description></item>
/// <item><description>Materials array (minItems: 1)</description></item>
/// <item><description>Quality tier enum (5 tiers)</description></item>
/// <item><description>Discovery type enum (6 types)</description></item>
/// <item><description>Failure chance range (0-1)</description></item>
/// <item><description>Quantity range (min/max >= 1)</description></item>
/// <item><description>Required fields (id, name, description, category, materials, output)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class RecipesSchemaTests
{
    /// <summary>
    /// Path to the recipes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/recipes.schema.json";

    /// <summary>
    /// Path to the actual recipes.json configuration file.
    /// </summary>
    private const string RecipesJsonPath = "../../../../../config/recipes.json";

    /// <summary>
    /// Loaded JSON Schema for recipe definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the recipes schema.
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
        _schema.Title.Should().Be("Crafting Recipe Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (8 total)
        _schema.Definitions.Should().ContainKey("RecipeDefinition", "should define RecipeDefinition");
        _schema.Definitions.Should().ContainKey("MaterialRequirement", "should define MaterialRequirement");
        _schema.Definitions.Should().ContainKey("SkillRequirement", "should define SkillRequirement");
        _schema.Definitions.Should().ContainKey("OutputDefinition", "should define OutputDefinition");
        _schema.Definitions.Should().ContainKey("QuantityRange", "should define QuantityRange");
        _schema.Definitions.Should().ContainKey("QualityVariance", "should define QualityVariance");
        _schema.Definitions.Should().ContainKey("BonusOutput", "should define BonusOutput");
        _schema.Definitions.Should().ContainKey("DiscoveryCondition", "should define DiscoveryCondition");
    }

    /// <summary>
    /// Verifies the existing recipes.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all 9 example recipes are valid.
    /// </summary>
    /// <remarks>
    /// The recipes.json contains 9 recipes covering all 9 categories:
    /// Alchemy, Smithing, Cooking, Leatherworking, Enchanting, Jewelcrafting,
    /// Tailoring, Engineering, and Inscription.
    /// </remarks>
    [Test]
    public async Task RecipesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual recipes.json file
        var jsonContent = await File.ReadAllTextAsync(RecipesJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing recipes.json should be valid
        errors.Should().BeEmpty(
            "existing recipes.json with 9 recipes should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that category must be one of the 9 valid enum values.
    /// Invalid category values should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid categories: Alchemy, Smithing, Enchanting, Cooking, Leatherworking,
    /// Tailoring, Jewelcrafting, Engineering, Inscription.
    /// </remarks>
    [Test]
    public void Category_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid category "Blacksmithing" instead of "Smithing"
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Blacksmithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid category enum value
        errors.Should().NotBeEmpty("category 'Blacksmithing' should fail validation (valid: Alchemy, Smithing, etc.)");
    }

    /// <summary>
    /// Verifies that materials array must have at least 1 item.
    /// Empty materials array should fail validation.
    /// </summary>
    /// <remarks>
    /// Every recipe must consume or require at least one material.
    /// </remarks>
    [Test]
    public void Materials_EmptyArray_FailsValidation()
    {
        // Arrange: JSON with empty materials array
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "magic-spell",
                    "name": "Magic Spell",
                    "description": "A spell that requires no materials?",
                    "category": "Enchanting",
                    "materials": [],
                    "output": {
                        "itemId": "spell-effect",
                        "quantity": { "min": 1, "max": 1 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty materials array
        errors.Should().NotBeEmpty("empty materials array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that baseQuality must be one of the 5 valid quality tier enum values.
    /// Invalid quality values should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid quality tiers: Common, Uncommon, Rare, Epic, Legendary.
    /// </remarks>
    [Test]
    public void QualityVariance_InvalidBaseQuality_FailsValidation()
    {
        // Arrange: JSON with invalid baseQuality "Superior"
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 },
                        "qualityVariance": {
                            "baseQuality": "Superior"
                        }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid baseQuality enum value
        errors.Should().NotBeEmpty("baseQuality 'Superior' should fail validation (valid: Common, Uncommon, Rare, Epic, Legendary)");
    }

    /// <summary>
    /// Verifies that discovery condition type must be one of the 6 valid enum values.
    /// Invalid discovery types should fail validation.
    /// </summary>
    /// <remarks>
    /// Valid discovery types: Quest, Level, Teacher, Item, Exploration, Default.
    /// </remarks>
    [Test]
    public void DiscoveryCondition_InvalidType_FailsValidation()
    {
        // Arrange: JSON with invalid discovery type "Purchase"
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 }
                    },
                    "discoveryCondition": {
                        "type": "Purchase"
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid discovery type enum value
        errors.Should().NotBeEmpty("discovery type 'Purchase' should fail validation (valid: Quest, Level, Teacher, Item, Exploration, Default)");
    }

    /// <summary>
    /// Verifies that failureChance must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Failure chance is a probability represented as a decimal.
    /// </remarks>
    [Test]
    public void FailureChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with failureChance greater than 1 (200%)
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 }
                    },
                    "failureChance": 2.0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to failureChance out of range
        errors.Should().NotBeEmpty("failureChance 2.0 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that recipe ID must follow the kebab-case pattern.
    /// Invalid ID patterns should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void RecipeId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid recipe ID (uppercase and space)
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "Iron Sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("recipe ID 'Iron Sword' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that required fields must be present.
    /// Missing required fields should fail validation.
    /// </summary>
    /// <remarks>
    /// Required fields: id, name, description, category, materials, output.
    /// </remarks>
    [Test]
    public void RequiredFields_Missing_FailsValidation()
    {
        // Arrange: JSON missing required field 'output'
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ]
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to missing required field
        errors.Should().NotBeEmpty("missing required field 'output' should fail validation");
    }

    /// <summary>
    /// Verifies that material quantity must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Every material must require at least 1 item.
    /// </remarks>
    [Test]
    public void MaterialQuantity_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with material quantity of 0
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 0 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to quantity less than minimum
        errors.Should().NotBeEmpty("material quantity 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that bonus output chance must be between 0 and 1.
    /// Values outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Bonus output chance is a probability represented as a decimal.
    /// </remarks>
    [Test]
    public void BonusOutputChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with bonus output chance of 1.5 (150%)
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 },
                        "bonusOutputs": [
                            { "itemId": "scrap-iron", "quantity": 1, "chance": 1.5 }
                        ]
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to bonus output chance out of range
        errors.Should().NotBeEmpty("bonus output chance 1.5 should fail validation (must be 0-1)");
    }

    /// <summary>
    /// Verifies that skill requirement DC must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// DC (Difficulty Class) must be a positive integer.
    /// </remarks>
    [Test]
    public void SkillRequirementDc_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with skill DC of 0
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "skillRequirements": [
                        { "skillId": "smithing", "dc": 0 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 1, "max": 1 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to DC less than minimum
        errors.Should().NotBeEmpty("skill requirement DC 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that output quantity min must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    /// <remarks>
    /// Recipes must produce at least 1 item.
    /// </remarks>
    [Test]
    public void QuantityRangeMin_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with quantity min of 0
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Smithing",
                    "materials": [
                        { "itemId": "ingot-iron", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "weapon-iron-sword",
                        "quantity": { "min": 0, "max": 1 }
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to quantity min less than minimum
        errors.Should().NotBeEmpty("quantity min 0 should fail validation (minimum is 1)");
    }
}
