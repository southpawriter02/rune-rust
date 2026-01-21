// ------------------------------------------------------------------------------
// <copyright file="BiomesSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for biomes.schema.json validation.
// Verifies schema structure, climate/lighting/era/condition enum validation,
// spawn table configuration, terrain distribution percentages, hazard frequency
// ranges, descriptor configuration, and ID pattern enforcement.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the biomes.schema.json JSON Schema.
/// Tests ensure the schema correctly validates biome configuration files,
/// enforces environmental category enums, spawn table weights, terrain percentages,
/// hazard frequencies, and required fields.
/// </summary>
/// <remarks>
/// <para>
/// The biome schema validates configurations for environmental regions including:
/// <list type="bullet">
/// <item><description>Default category values (climate, lighting, era, condition)</description></item>
/// <item><description>Descriptor system integration (tags, pool overrides, term filters)</description></item>
/// <item><description>Monster spawn tables with depth-based filtering</description></item>
/// <item><description>Terrain distribution percentages</description></item>
/// <item><description>Hazard frequency with depth scaling</description></item>
/// <item><description>Audio associations (music themes, ambient sounds)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class BiomesSchemaTests
{
    /// <summary>
    /// Path to the biomes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/biomes.schema.json";

    /// <summary>
    /// Path to the actual biomes.json configuration file.
    /// </summary>
    private const string BiomesJsonPath = "../../../../../config/biomes.json";

    /// <summary>
    /// Loaded JSON Schema for biome definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the biomes schema.
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
    /// Validates that all required definitions are present (BiomeDefinition, DefaultCategories,
    /// SpawnTable, SpawnEntry, TerrainDistribution, TerrainEntry, HazardFrequency).
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All 7 required definitions are present</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Biome Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (7 total)
        _schema.Definitions.Should().ContainKey("BiomeDefinition", "should define BiomeDefinition");
        _schema.Definitions.Should().ContainKey("DefaultCategories", "should define DefaultCategories");
        _schema.Definitions.Should().ContainKey("SpawnTable", "should define SpawnTable");
        _schema.Definitions.Should().ContainKey("SpawnEntry", "should define SpawnEntry");
        _schema.Definitions.Should().ContainKey("TerrainDistribution", "should define TerrainDistribution");
        _schema.Definitions.Should().ContainKey("TerrainEntry", "should define TerrainEntry");
        _schema.Definitions.Should().ContainKey("HazardFrequency", "should define HazardFrequency");
    }

    /// <summary>
    /// Verifies the existing biomes.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring all
    /// 8 existing biomes (cave, dungeon, volcanic, frozen, swamp, ruins, forest, desert) are valid.
    /// </summary>
    /// <remarks>
    /// This test ensures that the schema:
    /// <list type="bullet">
    /// <item><description>Validates all required fields for each biome</description></item>
    /// <item><description>Accepts all valid climate and lighting enum values</description></item>
    /// <item><description>Accepts optional era and condition fields</description></item>
    /// <item><description>Validates all implied tags, emphasized terms, and excluded terms</description></item>
    /// <item><description>Validates descriptor pool override mappings</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public async Task BiomesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual biomes.json file
        var biomesJsonContent = await File.ReadAllTextAsync(BiomesJsonPath);

        // Act: Validate the biomes.json content against the schema
        var errors = _schema.Validate(biomesJsonContent);

        // Assert: No validation errors - all 8 existing biomes should be valid
        errors.Should().BeEmpty(
            "existing biomes.json with 8 biomes should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that climate must be one of the valid enum values
    /// (freezing, cold, temperate, warm, hot, scorching). Invalid values should fail validation.
    /// </summary>
    /// <remarks>
    /// Climate affects gameplay mechanics including:
    /// <list type="bullet">
    /// <item><description>freezing: Extreme cold, cold damage over time</description></item>
    /// <item><description>cold: Chilly environment, minor stamina drain</description></item>
    /// <item><description>temperate: Comfortable temperature, no effect</description></item>
    /// <item><description>warm: Heated environment, minor stamina drain</description></item>
    /// <item><description>hot: Very warm, increased stamina drain</description></item>
    /// <item><description>scorching: Extreme heat, fire damage over time</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Climate_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid climate enum value "boiling" (should be scorching)
        var invalidJson = """
        {
            "biomes": {
                "test-biome": {
                    "id": "test-biome",
                    "name": "Test Biome",
                    "description": "A test biome with invalid climate.",
                    "defaultCategoryValues": {
                        "climate": "boiling",
                        "lighting": "dim"
                    },
                    "impliedTags": ["test"],
                    "descriptorPoolOverrides": {},
                    "emphasizedTerms": [],
                    "excludedTerms": []
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid climate enum value
        errors.Should().NotBeEmpty("invalid climate 'boiling' should fail validation (valid: freezing, cold, temperate, warm, hot, scorching)");
    }

    /// <summary>
    /// Verifies that terrain percentage must be between 0 and 100 (inclusive).
    /// Percentages outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Terrain percentages control room generation:
    /// <list type="bullet">
    /// <item><description>0-100: Valid percentage range</description></item>
    /// <item><description>Percentages do not need to sum to 100 (remainder uses default floor)</description></item>
    /// <item><description>Zero percentage means terrain type is available but rare</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void TerrainPercentage_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with terrain percentage outside valid range (150 > 100)
        var invalidJson = """
        {
            "biomes": {
                "test-biome": {
                    "id": "test-biome",
                    "name": "Test Biome",
                    "description": "A test biome with invalid terrain percentage.",
                    "defaultCategoryValues": {
                        "climate": "temperate",
                        "lighting": "normal"
                    },
                    "impliedTags": ["test"],
                    "descriptorPoolOverrides": {},
                    "emphasizedTerms": [],
                    "excludedTerms": [],
                    "terrainDistribution": {
                        "entries": [
                            {
                                "terrainId": "stone_floor",
                                "percentage": 150
                            }
                        ]
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range terrain percentage
        errors.Should().NotBeEmpty("terrain percentage 150 should fail validation (max is 100)");
    }

    /// <summary>
    /// Verifies that hazard baseChance must be between 0 and 1 (inclusive).
    /// Probabilities outside this range should fail validation.
    /// </summary>
    /// <remarks>
    /// Hazard frequency calculation:
    /// <list type="formula">
    /// <item>Effective Chance = min(1.0, baseChance + (depth × depthScaling))</item>
    /// </list>
    /// <list type="bullet">
    /// <item><description>baseChance 0.0 to 1.0: Valid probability range</description></item>
    /// <item><description>depthScaling: Additional chance per floor depth</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void HazardBaseChance_OutOfRange_FailsValidation()
    {
        // Arrange: JSON with hazard baseChance outside valid range (1.5 > 1.0)
        var invalidJson = """
        {
            "biomes": {
                "test-biome": {
                    "id": "test-biome",
                    "name": "Test Biome",
                    "description": "A test biome with invalid hazard frequency.",
                    "defaultCategoryValues": {
                        "climate": "temperate",
                        "lighting": "normal"
                    },
                    "impliedTags": ["test"],
                    "descriptorPoolOverrides": {},
                    "emphasizedTerms": [],
                    "excludedTerms": [],
                    "hazardFrequency": {
                        "baseChance": 1.5,
                        "hazardTypes": ["spike_trap"]
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to out-of-range hazard base chance
        errors.Should().NotBeEmpty("hazard baseChance 1.5 should fail validation (max is 1.0)");
    }

    /// <summary>
    /// Verifies that spawn weight must be at least 1.
    /// Weights less than 1 (including 0 and negative) should fail validation.
    /// </summary>
    /// <remarks>
    /// Spawn weights are relative to other entries:
    /// <list type="bullet">
    /// <item><description>Weight must be >= 1</description></item>
    /// <item><description>Higher weight = more common spawn</description></item>
    /// <item><description>A monster with weight 100 spawns twice as often as one with weight 50</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void SpawnWeight_LessThanOne_FailsValidation()
    {
        // Arrange: JSON with spawn weight less than minimum (0 < 1)
        var invalidJson = """
        {
            "biomes": {
                "test-biome": {
                    "id": "test-biome",
                    "name": "Test Biome",
                    "description": "A test biome with invalid spawn weight.",
                    "defaultCategoryValues": {
                        "climate": "temperate",
                        "lighting": "normal"
                    },
                    "impliedTags": ["test"],
                    "descriptorPoolOverrides": {},
                    "emphasizedTerms": [],
                    "excludedTerms": [],
                    "monsterSpawnTable": {
                        "entries": [
                            {
                                "monsterId": "goblin",
                                "weight": 0
                            }
                        ]
                    }
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to spawn weight less than minimum
        errors.Should().NotBeEmpty("spawn weight 0 should fail validation (minimum is 1)");
    }

    /// <summary>
    /// Verifies that biome ID must follow the kebab-case pattern (^[a-z][a-z0-9-]*$).
    /// Invalid ID patterns (uppercase, spaces, special characters) should fail validation.
    /// </summary>
    /// <remarks>
    /// ID pattern rules:
    /// <list type="bullet">
    /// <item><description>Must start with a lowercase letter</description></item>
    /// <item><description>Can contain lowercase letters, numbers, and hyphens</description></item>
    /// <item><description>No spaces, underscores (in biome IDs), or special characters</description></item>
    /// <item><description>Valid examples: cave, volcanic, deep-cave, cave-system-1</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void BiomeId_InvalidPattern_FailsValidation()
    {
        // Arrange: JSON with invalid biome ID (uppercase, spaces)
        var invalidJson = """
        {
            "biomes": {
                "Test Biome": {
                    "id": "Test Biome",
                    "name": "Test Biome",
                    "description": "A biome with invalid ID pattern.",
                    "defaultCategoryValues": {
                        "climate": "temperate",
                        "lighting": "normal"
                    },
                    "impliedTags": ["test"],
                    "descriptorPoolOverrides": {},
                    "emphasizedTerms": [],
                    "excludedTerms": []
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid ID pattern
        errors.Should().NotBeEmpty("biome ID 'Test Biome' should fail pattern validation (must be lowercase kebab-case)");
    }

    /// <summary>
    /// Verifies that lighting must be one of the valid enum values
    /// (dark, dim, normal, bright). Invalid values should fail validation.
    /// </summary>
    /// <remarks>
    /// Lighting affects gameplay mechanics including:
    /// <list type="bullet">
    /// <item><description>dark: No natural light, severely reduced sight</description></item>
    /// <item><description>dim: Minimal light, reduced sight range</description></item>
    /// <item><description>normal: Standard lighting, normal visibility</description></item>
    /// <item><description>bright: Ample light, extended sight range</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void Lighting_InvalidEnum_FailsValidation()
    {
        // Arrange: JSON with invalid lighting enum value "pitch-black" (should be dark)
        var invalidJson = """
        {
            "biomes": {
                "test-biome": {
                    "id": "test-biome",
                    "name": "Test Biome",
                    "description": "A test biome with invalid lighting.",
                    "defaultCategoryValues": {
                        "climate": "temperate",
                        "lighting": "pitch-black"
                    },
                    "impliedTags": ["test"],
                    "descriptorPoolOverrides": {},
                    "emphasizedTerms": [],
                    "excludedTerms": []
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid lighting enum value
        errors.Should().NotBeEmpty("invalid lighting 'pitch-black' should fail validation (valid: dark, dim, normal, bright)");
    }
}
