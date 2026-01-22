// ------------------------------------------------------------------------------
// <copyright file="CombatDescriptorSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for combat-descriptors.schema.json validation.
// Verifies schema structure, combat category validation, hit/death pool types,
// damage threshold validation, and variable substitution for player death messages.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the combat-descriptors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates combat descriptor configuration files,
/// enforces combat-specific category restrictions, hit/death pool type enumerations,
/// and combat-specific descriptor properties (weaponTypes, damageTypes, isCritical).
/// </summary>
/// <remarks>
/// <para>
/// The combat descriptors schema extends the master descriptors.schema.json with:
/// <list type="bullet">
/// <item><description>CombatCategory enum restricting category to combat-hits, combat-deaths, combat</description></item>
/// <item><description>HitPoolTypes enum for weapon-based hit pools (11 values)</description></item>
/// <item><description>DeathPoolTypes enum for creature-based death pools (10 values)</description></item>
/// <item><description>CombatDescriptor with weaponTypes, damageTypes, isCritical properties</description></item>
/// <item><description>Damage threshold validation (0-1 range) inherited from master</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class CombatDescriptorSchemaTests
{
    /// <summary>
    /// Path to the combat descriptors schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/combat-descriptors.schema.json";

    /// <summary>
    /// Path to the combat-hits.json descriptor configuration file.
    /// </summary>
    private const string CombatHitsJsonPath = "../../../../../config/descriptors/combat-hits.json";

    /// <summary>
    /// Path to the combat-deaths.json descriptor configuration file.
    /// </summary>
    private const string CombatDeathsJsonPath = "../../../../../config/descriptors/combat-deaths.json";

    /// <summary>
    /// Loaded JSON Schema for combat descriptor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the combat descriptors schema.
    /// </summary>
    /// <remarks>
    /// Loads the schema from the file system before each test execution.
    /// Uses FromFileAsync with full path to properly resolve $ref to descriptors.schema.json.
    /// The schema is validated during loading to ensure it is valid JSON Schema Draft-07.
    /// </remarks>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system with full path for reference resolution
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region CSC-001: Schema Loading

    /// <summary>
    /// CSC-001: Verifies the combat-descriptors.schema.json loads successfully
    /// and is a valid JSON Schema with expected structure and definitions.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>Schema title matches expected value</description></item>
    /// <item><description>Root type is object</description></item>
    /// <item><description>All required definitions are present (7 total)</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void CombatDescriptorSchema_LoadsSuccessfully_ReturnsValidSchema()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Combat Descriptor Configuration Schema", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present (7 combat-specific definitions)
        _schema.Definitions.Should().ContainKey("CombatCategory", "should define CombatCategory enum");
        _schema.Definitions.Should().ContainKey("HitPoolTypes", "should define HitPoolTypes enum");
        _schema.Definitions.Should().ContainKey("DeathPoolTypes", "should define DeathPoolTypes enum");
        _schema.Definitions.Should().ContainKey("CombatDescriptorPool", "should define CombatDescriptorPool");
        _schema.Definitions.Should().ContainKey("CombatDescriptor", "should define CombatDescriptor");
        _schema.Definitions.Should().ContainKey("VariablePattern", "should define VariablePattern documentation");
    }

    #endregion

    #region CSC-002: Category Validation

    /// <summary>
    /// CSC-002: Verifies that non-combat categories (e.g., 'environmental') fail validation
    /// against the combat-descriptors schema.
    /// </summary>
    /// <remarks>
    /// The CombatCategory enum only allows: combat-hits, combat-deaths, combat.
    /// Other category values should be rejected.
    /// </remarks>
    [Test]
    public void CombatDescriptorSchema_InvalidCategory_FailsValidation()
    {
        // Arrange: JSON with non-combat category
        var invalidJson = """
        {
            "category": "environmental",
            "pools": {
                "lighting": {
                    "id": "lighting",
                    "name": "Lighting Descriptions",
                    "descriptors": [
                        { "id": "dim_light", "text": "dimly lit" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to non-combat category
        errors.Should().NotBeEmpty(
            "category 'environmental' should fail validation against combat schema (must be combat-hits, combat-deaths, or combat)");
    }

    /// <summary>
    /// CSC-002: Verifies that valid combat categories pass validation successfully.
    /// Tests all three valid values: combat-hits, combat-deaths, combat.
    /// </summary>
    /// <remarks>
    /// Each test case validates a different allowed category value.
    /// </remarks>
    [Test]
    [TestCase("combat-hits")]
    [TestCase("combat-deaths")]
    [TestCase("combat")]
    public void CombatDescriptorSchema_ValidCategory_PassesValidation(string category)
    {
        // Arrange: JSON with valid combat category
        var validJson = $$"""
        {
            "category": "{{category}}",
            "pools": {
                "test_pool": {
                    "id": "test_pool",
                    "name": "Test Pool",
                    "descriptors": [
                        { "id": "test_desc", "text": "test text" }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid combat category
        errors.Should().BeEmpty($"category '{category}' should be a valid combat category");
    }

    #endregion

    #region CSC-003: Combat-Hits Validation

    /// <summary>
    /// CSC-003: Verifies the existing combat-hits.json configuration validates
    /// against the combat-descriptors schema without errors.
    /// </summary>
    /// <remarks>
    /// The combat-hits.json contains 11 pools with weapon-specific hit descriptions,
    /// critical hit descriptions, miss descriptions, and fumble descriptions.
    /// Pool IDs match the HitPoolTypes enum values.
    /// </remarks>
    [Test]
    public async Task CombatHitsJson_ValidatesAgainstSchema_Succeeds()
    {
        // Arrange: Load the actual combat-hits.json file
        var jsonContent = await File.ReadAllTextAsync(CombatHitsJsonPath);

        // Act: Validate the JSON content against the combat schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing combat-hits.json should be valid
        errors.Should().BeEmpty(
            "existing combat-hits.json with 11 pools should validate against combat schema without errors");
    }

    #endregion

    #region CSC-004: Combat-Deaths Validation

    /// <summary>
    /// CSC-004: Verifies the existing combat-deaths.json configuration validates
    /// against the combat-descriptors schema without errors.
    /// </summary>
    /// <remarks>
    /// The combat-deaths.json contains 10 pools including creature-type death descriptions,
    /// critical kill descriptions, player death messages (with variables), and low health warnings.
    /// Pool IDs match the DeathPoolTypes enum values.
    /// </remarks>
    [Test]
    public async Task CombatDeathsJson_ValidatesAgainstSchema_Succeeds()
    {
        // Arrange: Load the actual combat-deaths.json file
        var jsonContent = await File.ReadAllTextAsync(CombatDeathsJsonPath);

        // Act: Validate the JSON content against the combat schema
        var errors = _schema.Validate(jsonContent);

        // Assert: No validation errors - existing combat-deaths.json should be valid
        errors.Should().BeEmpty(
            "existing combat-deaths.json with 10 pools should validate against combat schema without errors");
    }

    #endregion

    #region CSC-005: Damage Threshold Validation

    /// <summary>
    /// CSC-005: Verifies that damage thresholds outside the 0-1 range fail validation.
    /// minDamagePercent exceeding 1 should be rejected.
    /// </summary>
    /// <remarks>
    /// Damage percentages are normalized (0 = 0%, 1 = 100%).
    /// Values outside this range are invalid.
    /// </remarks>
    [Test]
    public void CombatDescriptor_InvalidDamageThreshold_FailsValidation()
    {
        // Arrange: JSON with minDamagePercent above 1
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit_sword": {
                    "id": "hit_sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": [
                        {
                            "id": "massive_strike",
                            "text": "delivers a massive strike",
                            "weight": 10,
                            "minDamagePercent": 1.5
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to minDamagePercent out of range
        errors.Should().NotBeEmpty(
            "minDamagePercent 1.5 should fail validation (must be between 0 and 1)");
    }

    /// <summary>
    /// CSC-005: Verifies that valid damage thresholds within the 0-1 range pass validation.
    /// </summary>
    /// <remarks>
    /// Tests boundary values (0, 0.5, 1) for both min and max damage percentages.
    /// </remarks>
    [Test]
    public void CombatDescriptor_ValidDamageThreshold_PassesValidation()
    {
        // Arrange: JSON with valid damage threshold values
        var validJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit_sword": {
                    "id": "hit_sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": [
                        {
                            "id": "light_scratch",
                            "text": "scratches lightly",
                            "minDamagePercent": 0,
                            "maxDamagePercent": 0.25
                        },
                        {
                            "id": "solid_hit",
                            "text": "lands a solid hit",
                            "minDamagePercent": 0.25,
                            "maxDamagePercent": 0.5
                        },
                        {
                            "id": "devastating_blow",
                            "text": "delivers a devastating blow",
                            "minDamagePercent": 0.75,
                            "maxDamagePercent": 1.0
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for valid damage percentages
        errors.Should().BeEmpty(
            "valid damage thresholds within 0-1 range should pass validation");
    }

    #endregion

    #region CSC-006: Player Death Variables

    /// <summary>
    /// CSC-006: Verifies that descriptors with {player} and {killer} variable
    /// placeholders validate successfully against the combat-descriptors schema.
    /// </summary>
    /// <remarks>
    /// Player death messages commonly use variable substitution for dynamic text.
    /// The variables array documents which placeholders are used in the text.
    /// </remarks>
    [Test]
    public void PlayerDeathPool_ContainsVariables_ValidatesSuccessfully()
    {
        // Arrange: JSON with player_death pool containing variable placeholders
        var validJson = """
        {
            "category": "combat-deaths",
            "pools": {
                "player_death": {
                    "id": "player_death",
                    "name": "Player Death Messages",
                    "description": "Messages displayed when the player dies in combat",
                    "descriptors": [
                        {
                            "id": "falls_battle",
                            "text": "{player} has fallen in battle against the {killer}...",
                            "weight": 25,
                            "variables": ["player", "killer"]
                        },
                        {
                            "id": "journey_ends",
                            "text": "{player}'s journey ends here, cut down by the {killer}.",
                            "weight": 20,
                            "variables": ["player", "killer"]
                        },
                        {
                            "id": "final_stand",
                            "text": "Despite a valiant effort, {player} succumbs to the {killer}.",
                            "weight": 15,
                            "variables": ["player", "killer"],
                            "tags": ["heroic"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for descriptors with variable placeholders
        errors.Should().BeEmpty(
            "player_death pool with {player} and {killer} variables should validate successfully");
    }

    #endregion

    #region Combat-Specific Properties Validation

    /// <summary>
    /// Verifies that combat-specific properties (weaponTypes, damageTypes, isCritical)
    /// validate successfully when provided on descriptors.
    /// </summary>
    /// <remarks>
    /// These properties are unique to combat descriptors and allow filtering
    /// by weapon type, damage type, and critical hit status.
    /// </remarks>
    [Test]
    public void CombatDescriptor_CombatSpecificProperties_PassesValidation()
    {
        // Arrange: JSON with combat-specific properties
        var validJson = """
        {
            "category": "combat-hits",
            "pools": {
                "critical_hit": {
                    "id": "critical_hit",
                    "name": "Critical Hit Descriptions",
                    "descriptors": [
                        {
                            "id": "devastating_slash",
                            "text": "delivers a devastating slash",
                            "weight": 25,
                            "weaponTypes": ["sword", "axe"],
                            "damageTypes": ["slashing"],
                            "isCritical": true
                        },
                        {
                            "id": "piercing_strike",
                            "text": "drives the weapon deep",
                            "weight": 20,
                            "weaponTypes": ["dagger", "spear"],
                            "damageTypes": ["piercing"],
                            "isCritical": true
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for combat-specific properties
        errors.Should().BeEmpty(
            "combat descriptors with weaponTypes, damageTypes, and isCritical should validate successfully");
    }

    /// <summary>
    /// Verifies that unknown properties on combat descriptors fail validation.
    /// Schema enforces additionalProperties: false.
    /// </summary>
    /// <remarks>
    /// Ensures strict schema validation prevents typos and unsupported properties.
    /// </remarks>
    [Test]
    public void CombatDescriptor_UnknownProperty_FailsValidation()
    {
        // Arrange: JSON with unknown property on descriptor
        var invalidJson = """
        {
            "category": "combat-hits",
            "pools": {
                "hit_sword": {
                    "id": "hit_sword",
                    "name": "Sword Hit Descriptions",
                    "descriptors": [
                        {
                            "id": "test_hit",
                            "text": "test hit text",
                            "unknownProperty": "invalid"
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to unknown property
        errors.Should().NotBeEmpty(
            "unknown property 'unknownProperty' on descriptor should fail validation");
    }

    #endregion

    #region Tag and Theme Validation

    /// <summary>
    /// Verifies that descriptors with creature-specific tags validate successfully.
    /// Tags allow filtering death descriptions by creature type traits.
    /// </summary>
    /// <remarks>
    /// Death descriptors commonly use tags like 'skeleton', 'zombie', 'humanoid'
    /// to select appropriate descriptions for specific creature types.
    /// </remarks>
    [Test]
    public void CombatDescriptor_WithTags_PassesValidation()
    {
        // Arrange: JSON with tag-filtered death descriptor
        var validJson = """
        {
            "category": "combat-deaths",
            "pools": {
                "death_undead": {
                    "id": "death_undead",
                    "name": "Undead Death Descriptions",
                    "descriptors": [
                        {
                            "id": "crumbles_bones",
                            "text": "crumbles into a pile of ancient bones",
                            "weight": 20,
                            "tags": ["skeleton"]
                        },
                        {
                            "id": "falls_shambling",
                            "text": "falls and lies still at last",
                            "weight": 20,
                            "tags": ["zombie"]
                        },
                        {
                            "id": "dark_energy_fades",
                            "text": "the dark energy animating it fades away",
                            "weight": 15,
                            "tags": ["ghost", "wraith"]
                        }
                    ]
                }
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for descriptors with tags
        errors.Should().BeEmpty(
            "death descriptors with creature-specific tags should validate successfully");
    }

    #endregion
}
