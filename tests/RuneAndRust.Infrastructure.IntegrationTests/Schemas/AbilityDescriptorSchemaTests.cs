// ------------------------------------------------------------------------------
// <copyright file="AbilityDescriptorSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for ability-descriptors.schema.json validation.
// Verifies schema structure, Galdr descriptor validation, rune school enums,
// success level enums, miscast types, and pool ID patterns.
// Part of v0.14.9b implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the ability-descriptors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates ability and Galdr flavor text
/// configuration files, enforces descriptor structure requirements, rune school
/// enum validation, success level constraints, and pool ID patterns.
/// </summary>
/// <remarks>
/// <para>
/// The ability descriptor schema provides 8 definitions:
/// <list type="bullet">
/// <item><description>GaldrActionDescriptor - Casting sequence flavor text</description></item>
/// <item><description>GaldrManifestationDescriptor - Sensory manifestation effects</description></item>
/// <item><description>GaldrOutcomeDescriptor - Ability outcome narratives</description></item>
/// <item><description>GaldrMiscastDescriptor - Magical failure narratives</description></item>
/// <item><description>WeaponArtDescriptor - Combat ability flavor text</description></item>
/// <item><description>SkillUsageDescriptor - Skill action flavor text</description></item>
/// <item><description>RuneSchool - 24 Elder Futhark runes enum</description></item>
/// <item><description>SuccessLevel - Success/failure levels enum</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class AbilityDescriptorSchemaTests
{
    /// <summary>
    /// Path to the ability descriptor schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/ability-descriptors.schema.json";

    /// <summary>
    /// Path to the galdr-actions.json configuration file for validation.
    /// </summary>
    private const string GaldrActionsPath = "../../../../../config/ability-descriptors/galdr-actions.json";

    /// <summary>
    /// Path to the galdr-miscasts.json configuration file for validation.
    /// </summary>
    private const string GaldrMiscastsPath = "../../../../../config/ability-descriptors/galdr-miscasts.json";

    /// <summary>
    /// Path to the galdr-outcomes.json configuration file for validation.
    /// </summary>
    private const string GaldrOutcomesPath = "../../../../../config/ability-descriptors/galdr-outcomes.json";

    /// <summary>
    /// Path to the weapon-arts.json configuration file for validation.
    /// </summary>
    private const string WeaponArtsPath = "../../../../../config/ability-descriptors/weapon-arts.json";

    /// <summary>
    /// Loaded JSON Schema for ability descriptor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the ability descriptor schema.
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

    #region ABL-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// ABL-001: Verifies that ability-descriptors.schema.json loads and parses as valid JSON Schema.
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
    public void AbilityDescriptorSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Ability Descriptor Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "schema type should be object");

        // Assert: All 8 definitions should be present
        _schema.Definitions.Should().ContainKey("GaldrActionDescriptor", "should define GaldrActionDescriptor");
        _schema.Definitions.Should().ContainKey("GaldrManifestationDescriptor", "should define GaldrManifestationDescriptor");
        _schema.Definitions.Should().ContainKey("GaldrOutcomeDescriptor", "should define GaldrOutcomeDescriptor");
        _schema.Definitions.Should().ContainKey("GaldrMiscastDescriptor", "should define GaldrMiscastDescriptor");
        _schema.Definitions.Should().ContainKey("WeaponArtDescriptor", "should define WeaponArtDescriptor");
        _schema.Definitions.Should().ContainKey("SkillUsageDescriptor", "should define SkillUsageDescriptor");
        _schema.Definitions.Should().ContainKey("RuneSchool", "should define RuneSchool");
        _schema.Definitions.Should().ContainKey("SuccessLevel", "should define SuccessLevel");
    }

    #endregion

    #region ABL-002: Existing Configuration Files Pass Validation

    /// <summary>
    /// ABL-002: Verifies that galdr-actions.json passes schema validation.
    /// </summary>
    [Test]
    public async Task AbilityDescriptorSchema_GaldrActions_PassesValidation()
    {
        // Arrange: Load the actual galdr-actions.json file
        var jsonContent = await File.ReadAllTextAsync(GaldrActionsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing galdr-actions.json should validate against schema without errors");
    }

    /// <summary>
    /// ABL-002: Verifies that galdr-miscasts.json passes schema validation.
    /// </summary>
    [Test]
    public async Task AbilityDescriptorSchema_GaldrMiscasts_PassesValidation()
    {
        // Arrange: Load the actual galdr-miscasts.json file
        var jsonContent = await File.ReadAllTextAsync(GaldrMiscastsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing galdr-miscasts.json should validate against schema without errors");
    }

    /// <summary>
    /// ABL-002: Verifies that galdr-outcomes.json passes schema validation.
    /// </summary>
    [Test]
    public async Task AbilityDescriptorSchema_GaldrOutcomes_PassesValidation()
    {
        // Arrange: Load the actual galdr-outcomes.json file
        var jsonContent = await File.ReadAllTextAsync(GaldrOutcomesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing galdr-outcomes.json should validate against schema without errors");
    }

    /// <summary>
    /// ABL-002: Verifies that weapon-arts.json passes schema validation.
    /// </summary>
    [Test]
    public async Task AbilityDescriptorSchema_WeaponArts_PassesValidation()
    {
        // Arrange: Load the actual weapon-arts.json file
        var jsonContent = await File.ReadAllTextAsync(WeaponArtsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing weapon-arts.json should validate against schema without errors");
    }

    #endregion

    #region ABL-003: GaldrActionDescriptor Validation

    /// <summary>
    /// ABL-003: Verifies that minimal valid Galdr action descriptor passes validation.
    /// </summary>
    [Test]
    public void GaldrActionDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid Galdr action configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimal valid descriptor
        errors.Should().BeEmpty(
            "minimal Galdr action descriptor with required fields should validate successfully");
    }

    /// <summary>
    /// ABL-003: Verifies that complete Galdr action descriptor passes validation.
    /// </summary>
    [Test]
    public void GaldrActionDescriptor_Complete_PassesValidation()
    {
        // Arrange: Complete Galdr action configuration with all optional fields
        var validJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "fehu_flamebolt_solid": [
                    {
                        "id": "fehu_flamebolt_solid_001",
                        "text": "You sing the Fehu rune, fire answers your call!",
                        "weight": 10,
                        "actionType": "Invocation",
                        "runeSchool": "Fehu",
                        "abilityName": "FlameBolt",
                        "successLevel": "SolidSuccess",
                        "biome": "Muspelheim",
                        "tags": ["Verbose", "Dramatic"]
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "complete Galdr action descriptor should validate successfully");
    }

    /// <summary>
    /// ABL-003: Verifies that descriptor missing required 'id' fails validation.
    /// </summary>
    [Test]
    public void GaldrActionDescriptor_MissingId_FailsValidation()
    {
        // Arrange: Descriptor missing required 'id'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "text": "You invoke the rune...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "descriptor missing 'id' should fail validation");
    }

    /// <summary>
    /// ABL-003: Verifies that descriptor missing required 'text' fails validation.
    /// </summary>
    [Test]
    public void GaldrActionDescriptor_MissingText_FailsValidation()
    {
        // Arrange: Descriptor missing required 'text'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "descriptor missing 'text' should fail validation");
    }

    /// <summary>
    /// ABL-003: Verifies that descriptor missing required 'actionType' fails validation.
    /// </summary>
    [Test]
    public void GaldrActionDescriptor_MissingActionType_FailsValidation()
    {
        // Arrange: Descriptor missing required 'actionType'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune..."
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "descriptor missing 'actionType' should fail validation");
    }

    /// <summary>
    /// ABL-003: Verifies that all valid action types pass validation.
    /// </summary>
    [Test]
    [TestCase("Invocation")]
    [TestCase("Chant")]
    [TestCase("RuneManifestation")]
    [TestCase("Discharge")]
    [TestCase("Aftermath")]
    [TestCase("EffectTrigger")]
    [TestCase("Activation")]
    public void GaldrActionDescriptor_ValidActionType_PassesValidation(string actionType)
    {
        // Arrange: Descriptor with valid action type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "actionType": "{{actionType}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"actionType '{actionType}' should pass validation");
    }

    /// <summary>
    /// ABL-003: Verifies that invalid action type fails validation.
    /// </summary>
    [Test]
    public void GaldrActionDescriptor_InvalidActionType_FailsValidation()
    {
        // Arrange: Descriptor with invalid action type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "actionType": "InvalidAction"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid actionType 'InvalidAction' should fail validation");
    }

    #endregion

    #region ABL-004: Rune School Enum Validation

    /// <summary>
    /// ABL-004: Verifies that all 24 Elder Futhark rune schools pass validation.
    /// </summary>
    [Test]
    [TestCase("Fehu")]
    [TestCase("Uruz")]
    [TestCase("Thurisaz")]
    [TestCase("Ansuz")]
    [TestCase("Raido")]
    [TestCase("Kenaz")]
    [TestCase("Gebo")]
    [TestCase("Wunjo")]
    [TestCase("Hagalaz")]
    [TestCase("Naudiz")]
    [TestCase("Isa")]
    [TestCase("Jera")]
    [TestCase("Eihwaz")]
    [TestCase("Perthro")]
    [TestCase("Algiz")]
    [TestCase("Sowilo")]
    [TestCase("Tiwaz")]
    [TestCase("Berkanan")]
    [TestCase("Ehwaz")]
    [TestCase("Mannaz")]
    [TestCase("Laguz")]
    [TestCase("Ingwaz")]
    [TestCase("Dagaz")]
    [TestCase("Othala")]
    public void RuneSchool_ValidRune_PassesValidation(string runeSchool)
    {
        // Arrange: Descriptor with valid rune school
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation",
                        "runeSchool": "{{runeSchool}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"runeSchool '{runeSchool}' should pass validation");
    }

    /// <summary>
    /// ABL-004: Verifies that invalid rune school fails validation.
    /// </summary>
    [Test]
    [TestCase("InvalidRune")]
    [TestCase("fehu")]
    [TestCase("FEHU")]
    public void RuneSchool_InvalidRune_FailsValidation(string runeSchool)
    {
        // Arrange: Descriptor with invalid rune school
        var invalidJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation",
                        "runeSchool": "{{runeSchool}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"invalid runeSchool '{runeSchool}' should fail validation");
    }

    /// <summary>
    /// ABL-004: Verifies that null rune school passes validation (generic descriptors).
    /// </summary>
    [Test]
    public void RuneSchool_Null_PassesValidation()
    {
        // Arrange: Descriptor with null rune school (generic)
        var validJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation",
                        "runeSchool": null
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "null runeSchool should pass validation for generic descriptors");
    }

    #endregion

    #region ABL-005: Success Level Enum Validation

    /// <summary>
    /// ABL-005: Verifies that all valid success levels pass validation.
    /// </summary>
    [Test]
    [TestCase("MinorSuccess")]
    [TestCase("SolidSuccess")]
    [TestCase("ExceptionalSuccess")]
    [TestCase("Failure")]
    [TestCase("CriticalFailure")]
    public void SuccessLevel_ValidLevel_PassesValidation(string successLevel)
    {
        // Arrange: Descriptor with valid success level
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation",
                        "successLevel": "{{successLevel}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"successLevel '{successLevel}' should pass validation");
    }

    /// <summary>
    /// ABL-005: Verifies that invalid success level fails validation.
    /// </summary>
    [Test]
    [TestCase("minor_success")]
    [TestCase("Invalid")]
    [TestCase("SOLID_SUCCESS")]
    public void SuccessLevel_InvalidLevel_FailsValidation(string successLevel)
    {
        // Arrange: Descriptor with invalid success level
        var invalidJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation",
                        "successLevel": "{{successLevel}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"invalid successLevel '{successLevel}' should fail validation");
    }

    /// <summary>
    /// ABL-005: Verifies that null success level passes validation (any level).
    /// </summary>
    [Test]
    public void SuccessLevel_Null_PassesValidation()
    {
        // Arrange: Descriptor with null success level (applies to any)
        var validJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "You invoke the rune...",
                        "actionType": "Invocation",
                        "successLevel": null
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "null successLevel should pass validation");
    }

    #endregion

    #region ABL-006: GaldrMiscastDescriptor Validation

    /// <summary>
    /// ABL-006: Verifies that minimal valid miscast descriptor passes validation.
    /// </summary>
    [Test]
    public void GaldrMiscastDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid miscast configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "galdr-miscasts",
            "pools": {
                "miscast_fizzle_minor": [
                    {
                        "id": "fizzle_001",
                        "text": "Your spell fizzles...",
                        "miscastType": "Fizzle",
                        "severity": "Minor"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal miscast descriptor should validate successfully");
    }

    /// <summary>
    /// ABL-006: Verifies that all valid miscast types pass validation.
    /// </summary>
    [Test]
    [TestCase("BlightCorruption")]
    [TestCase("Paradox")]
    [TestCase("Backlash")]
    [TestCase("Fizzle")]
    [TestCase("WildMagic")]
    [TestCase("AlfheimDistortion")]
    [TestCase("RunicInversion")]
    public void GaldrMiscastDescriptor_ValidMiscastType_PassesValidation(string miscastType)
    {
        // Arrange: Descriptor with valid miscast type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-miscasts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Magic goes wrong...",
                        "miscastType": "{{miscastType}}",
                        "severity": "Moderate"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"miscastType '{miscastType}' should pass validation");
    }

    /// <summary>
    /// ABL-006: Verifies that invalid miscast type fails validation.
    /// </summary>
    [Test]
    public void GaldrMiscastDescriptor_InvalidMiscastType_FailsValidation()
    {
        // Arrange: Descriptor with invalid miscast type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-miscasts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Magic goes wrong...",
                        "miscastType": "InvalidType",
                        "severity": "Moderate"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid miscastType should fail validation");
    }

    /// <summary>
    /// ABL-006: Verifies that all valid severity levels pass validation.
    /// </summary>
    [Test]
    [TestCase("Minor")]
    [TestCase("Moderate")]
    [TestCase("Severe")]
    [TestCase("Catastrophic")]
    public void GaldrMiscastDescriptor_ValidSeverity_PassesValidation(string severity)
    {
        // Arrange: Descriptor with valid severity
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-miscasts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Magic goes wrong...",
                        "miscastType": "Fizzle",
                        "severity": "{{severity}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"severity '{severity}' should pass validation");
    }

    /// <summary>
    /// ABL-006: Verifies that miscast descriptor missing severity fails validation.
    /// </summary>
    [Test]
    public void GaldrMiscastDescriptor_MissingSeverity_FailsValidation()
    {
        // Arrange: Descriptor missing required severity
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-miscasts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Magic goes wrong...",
                        "miscastType": "Fizzle"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "miscast descriptor missing 'severity' should fail validation");
    }

    /// <summary>
    /// ABL-006: Verifies that miscast with mechanical effects passes validation.
    /// </summary>
    [Test]
    public void GaldrMiscastDescriptor_WithMechanicalEffect_PassesValidation()
    {
        // Arrange: Miscast with mechanical effects
        var validJson = """
        {
            "version": "1.0.0",
            "category": "galdr-miscasts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "The Blight corrupts your spell!",
                        "miscastType": "BlightCorruption",
                        "severity": "Moderate",
                        "runeSchool": "Fehu",
                        "corruptionSource": "RunicBlight",
                        "mechanicalEffect": {
                            "damage": 6,
                            "status": "Corrupted",
                            "duration": 2,
                            "target": "Self",
                            "corruption": 1
                        }
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "miscast descriptor with mechanical effects should validate successfully");
    }

    #endregion

    #region ABL-007: Pool ID Pattern Validation

    /// <summary>
    /// ABL-007: Verifies that valid pool IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("fehu_flamebolt_solid")]
    [TestCase("generic_invocation")]
    [TestCase("miscast_blight_moderate")]
    [TestCase("test123")]
    [TestCase("a")]
    public void PoolIdPattern_Valid_PassesValidation(string poolId)
    {
        // Arrange: Configuration with valid pool ID
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "{{poolId}}": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"pool ID '{poolId}' should pass validation");
    }

    /// <summary>
    /// ABL-007: Documents the expected pool ID pattern validation behavior.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pool IDs should follow the pattern ^[a-z][a-z0-9_]*$ (snake_case starting
    /// with lowercase letter). The schema includes a propertyNames constraint,
    /// but NJsonSchema library does not fully support propertyNames pattern
    /// validation at runtime.
    /// </para>
    /// <para>
    /// This test documents the expected behavior rather than testing it, since
    /// the validation must be performed by tooling that fully supports JSON Schema
    /// Draft-07 (such as VS Code's built-in JSON Schema validation or external
    /// schema validators).
    /// </para>
    /// </remarks>
    [Test]
    [Ignore("NJsonSchema does not fully support propertyNames pattern validation. " +
            "Pool ID patterns are validated by external tools (VS Code, CLI validators).")]
    [TestCase("Fehu_FlameBolt")]
    [TestCase("UPPERCASE")]
    [TestCase("123_starts_with_number")]
    [TestCase("has-hyphen")]
    public void PoolIdPattern_Invalid_FailsValidation(string poolId)
    {
        // Arrange: Configuration with invalid pool ID
        var invalidJson = $$"""
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "{{poolId}}": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail (when using a full JSON Schema validator)
        errors.Should().NotBeEmpty(
            $"pool ID '{poolId}' should fail validation");
    }

    /// <summary>
    /// ABL-007: Verifies that empty pool fails validation.
    /// </summary>
    [Test]
    public void PoolIdPattern_EmptyPool_FailsValidation()
    {
        // Arrange: Configuration with empty pool
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions",
            "pools": {
                "test_pool": []
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "empty pool should fail validation (minItems: 1)");
    }

    #endregion

    #region ABL-008: WeaponArtDescriptor Validation

    /// <summary>
    /// ABL-008: Verifies that valid weapon art descriptor passes validation.
    /// </summary>
    [Test]
    public void WeaponArtDescriptor_Valid_PassesValidation()
    {
        // Arrange: Valid weapon art configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "weapon-arts",
            "pools": {
                "whirlwind_twohanded": [
                    {
                        "id": "whirlwind_001",
                        "text": "You spin your {Weapon} in a deadly arc!",
                        "abilityCategory": "WeaponArt",
                        "abilityName": "WhirlwindStrike",
                        "weaponType": "TwoHanded",
                        "successLevel": "SolidSuccess"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "valid weapon art descriptor should pass validation");
    }

    /// <summary>
    /// ABL-008: Verifies that all valid ability categories pass validation.
    /// </summary>
    [Test]
    [TestCase("WeaponArt")]
    [TestCase("TacticalAbility")]
    [TestCase("DefensiveAbility")]
    [TestCase("PassiveAbility")]
    [TestCase("ResourceAbility")]
    public void WeaponArtDescriptor_ValidAbilityCategory_PassesValidation(string category)
    {
        // Arrange: Descriptor with valid ability category
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "weapon-arts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test ability...",
                        "abilityCategory": "{{category}}",
                        "abilityName": "TestAbility"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"abilityCategory '{category}' should pass validation");
    }

    /// <summary>
    /// ABL-008: Verifies that all valid weapon types pass validation.
    /// </summary>
    [Test]
    [TestCase("TwoHanded")]
    [TestCase("OneHanded")]
    [TestCase("DualWield")]
    [TestCase("Bow")]
    [TestCase("Crossbow")]
    [TestCase("Unarmed")]
    [TestCase("Shield")]
    public void WeaponArtDescriptor_ValidWeaponType_PassesValidation(string weaponType)
    {
        // Arrange: Descriptor with valid weapon type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "weapon-arts",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test ability...",
                        "abilityCategory": "WeaponArt",
                        "abilityName": "TestAbility",
                        "weaponType": "{{weaponType}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"weaponType '{weaponType}' should pass validation");
    }

    #endregion

    #region ABL-009: Configuration Requirements Validation

    /// <summary>
    /// ABL-009: Verifies that configuration missing 'version' fails validation.
    /// </summary>
    [Test]
    public void Configuration_MissingVersion_FailsValidation()
    {
        // Arrange: Configuration missing required 'version'
        var invalidJson = """
        {
            "category": "galdr-actions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "configuration missing 'version' should fail validation");
    }

    /// <summary>
    /// ABL-009: Verifies that configuration missing 'category' fails validation.
    /// </summary>
    [Test]
    public void Configuration_MissingCategory_FailsValidation()
    {
        // Arrange: Configuration missing required 'category'
        var invalidJson = """
        {
            "version": "1.0.0",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "configuration missing 'category' should fail validation");
    }

    /// <summary>
    /// ABL-009: Verifies that configuration missing 'pools' fails validation.
    /// </summary>
    [Test]
    public void Configuration_MissingPools_FailsValidation()
    {
        // Arrange: Configuration missing required 'pools'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "galdr-actions"
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "configuration missing 'pools' should fail validation");
    }

    /// <summary>
    /// ABL-009: Verifies that all valid categories pass validation.
    /// </summary>
    [Test]
    [TestCase("galdr-actions")]
    [TestCase("galdr-manifestations")]
    [TestCase("galdr-outcomes")]
    [TestCase("galdr-miscasts")]
    [TestCase("weapon-arts")]
    [TestCase("skill-usage")]
    public void Configuration_ValidCategory_PassesValidation(string category)
    {
        // Arrange: Configuration with valid category
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "{{category}}",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"category '{category}' should pass validation");
    }

    /// <summary>
    /// ABL-009: Verifies that invalid category fails validation.
    /// </summary>
    [Test]
    public void Configuration_InvalidCategory_FailsValidation()
    {
        // Arrange: Configuration with invalid category
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "invalid-category",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "actionType": "Invocation"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid category should fail validation");
    }

    #endregion
}
