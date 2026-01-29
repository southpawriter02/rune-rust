// ------------------------------------------------------------------------------
// <copyright file="RecipesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for recipes.schema.json validation.
// Verifies schema structure, category enum validation, ingredients array,
// output definition, and backward compatibility with existing recipes.json.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the recipes.schema.json JSON Schema.
/// Tests ensure the schema correctly validates crafting recipe configuration files,
/// enforces category enums, ingredient requirements, and output constraints.
/// </summary>
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
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    /// <summary>
    /// Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all 3 definitions are present: RecipeDefinition, Ingredient, Output.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Crafting Recipe Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all 3 definitions are present
        _schema.Definitions.Should().ContainKey("RecipeDefinition", "should define RecipeDefinition");
        _schema.Definitions.Should().ContainKey("Ingredient", "should define Ingredient");
        _schema.Definitions.Should().ContainKey("Output", "should define Output");
    }

    /// <summary>
    /// Verifies the existing recipes.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all recipes are valid.
    /// </summary>
    [Test]
    public async Task RecipesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual recipes.json file
        var jsonContent = await File.ReadAllTextAsync(RecipesJsonPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing recipes.json should be valid
        errors.Should().BeEmpty(
            "existing recipes.json should validate against schema without errors");
    }

    /// <summary>
    /// Verifies that category must be one of the 7 valid enum values.
    /// Invalid category values should fail validation.
    /// </summary>
    [Test]
    public void Category_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid category "Blacksmithing" instead of "Weapon"
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Blacksmithing",
                    "requiredStation": "anvil",
                    "ingredients": [
                        { "resourceId": "iron-ore", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "iron-sword",
                        "quantity": 1
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid category enum value
        errors.Should().NotBeEmpty("category 'Blacksmithing' should fail validation (valid: Weapon, Armor, Potion, Consumable, Accessory, Tool, Material)");
    }

    /// <summary>
    /// Verifies that ingredients array must have at least 1 item.
    /// Empty ingredients array should fail validation.
    /// </summary>
    [Test]
    public void Ingredients_EmptyArray_FailsValidation()
    {
        // Arrange: JSON with empty ingredients array
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "magic-spell",
                    "name": "Magic Spell",
                    "description": "A spell that requires no materials?",
                    "category": "Consumable",
                    "requiredStation": "alchemy-table",
                    "ingredients": [],
                    "output": {
                        "itemId": "spell-effect",
                        "quantity": 1
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to empty ingredients array
        errors.Should().NotBeEmpty("empty ingredients array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that recipe ID must follow the kebab-case pattern.
    /// Invalid ID patterns should fail validation.
    /// </summary>
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
                    "category": "Weapon",
                    "requiredStation": "anvil",
                    "ingredients": [
                        { "resourceId": "iron-ore", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "iron-sword",
                        "quantity": 1
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
                    "category": "Weapon",
                    "requiredStation": "anvil",
                    "ingredients": [
                        { "resourceId": "iron-ore", "quantity": 3 }
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
    /// Verifies that ingredient quantity must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    [Test]
    public void IngredientQuantity_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with ingredient quantity of 0
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Weapon",
                    "requiredStation": "anvil",
                    "ingredients": [
                        { "resourceId": "iron-ore", "quantity": 0 }
                    ],
                    "output": {
                        "itemId": "iron-sword",
                        "quantity": 1
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to quantity less than minimum
        errors.Should().NotBeEmpty("ingredient quantity 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that output quantity must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    [Test]
    public void OutputQuantity_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with output quantity of 0
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Weapon",
                    "requiredStation": "anvil",
                    "ingredients": [
                        { "resourceId": "iron-ore", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "iron-sword",
                        "quantity": 0
                    }
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to output quantity less than minimum
        errors.Should().NotBeEmpty("output quantity 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that difficulty class must be at least 1.
    /// Values less than 1 should fail validation.
    /// </summary>
    [Test]
    public void DifficultyClass_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with difficulty class of 0
        var invalidJson = """
        {
            "recipes": [
                {
                    "id": "iron-sword",
                    "name": "Iron Sword",
                    "description": "A sturdy iron blade.",
                    "category": "Weapon",
                    "requiredStation": "anvil",
                    "ingredients": [
                        { "resourceId": "iron-ore", "quantity": 3 }
                    ],
                    "output": {
                        "itemId": "iron-sword",
                        "quantity": 1
                    },
                    "difficultyClass": 0
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to DC less than minimum
        errors.Should().NotBeEmpty("difficulty class 0 should fail validation (minimum is 1)");
    }
}
