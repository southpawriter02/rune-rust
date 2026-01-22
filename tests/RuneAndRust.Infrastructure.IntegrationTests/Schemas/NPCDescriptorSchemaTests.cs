// ------------------------------------------------------------------------------
// <copyright file="NPCDescriptorSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for npc-descriptors.schema.json validation.
// Verifies schema structure, physical descriptor validation, ambient bark structure,
// reaction descriptor validation, archetype enums, and disposition enums.
// Part of v0.14.9c implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the npc-descriptors.schema.json JSON Schema.
/// Tests ensure the schema correctly validates NPC flavor text configuration files
/// including physical descriptors, ambient barks, and reaction descriptors.
/// Validates 8 archetypes, 16 bark types, 15 reaction types, and 24 trigger events.
/// </summary>
/// <remarks>
/// <para>
/// The NPC descriptor schema provides 10 definitions:
/// <list type="bullet">
/// <item><description>NPCPhysicalDescriptor - Physical appearance descriptions</description></item>
/// <item><description>NPCAmbientBarkDescriptor - Ambient dialogue barks</description></item>
/// <item><description>NPCReactionDescriptor - Emotional reaction descriptions</description></item>
/// <item><description>NPCArchetype - 8 NPC archetypes enum</description></item>
/// <item><description>NPCCondition - 6 condition states enum</description></item>
/// <item><description>Disposition - 5 disposition levels enum</description></item>
/// <item><description>BarkType - 16 bark types enum</description></item>
/// <item><description>ReactionType - 15 reaction types enum</description></item>
/// <item><description>TriggerEvent - 24 trigger events enum</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class NPCDescriptorSchemaTests
{
    /// <summary>
    /// Path to the NPC descriptor schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/npc-descriptors.schema.json";

    /// <summary>
    /// Path to the physical-descriptors.json configuration file for validation.
    /// </summary>
    private const string PhysicalDescriptorsPath = "../../../../../config/npc-descriptors/physical-descriptors.json";

    /// <summary>
    /// Path to the ambient-barks.json configuration file for validation.
    /// </summary>
    private const string AmbientBarksPath = "../../../../../config/npc-descriptors/ambient-barks.json";

    /// <summary>
    /// Path to the reaction-descriptors.json configuration file for validation.
    /// </summary>
    private const string ReactionDescriptorsPath = "../../../../../config/npc-descriptors/reaction-descriptors.json";

    /// <summary>
    /// Loaded JSON Schema for NPC descriptor definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the NPC descriptor schema.
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

    #region NPC-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// NPC-001: Verifies that npc-descriptors.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>All required definitions are present (10 total)</description></item>
    /// <item><description>Schema title and type are correct</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void NPCDescriptorSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("NPC Descriptor Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "schema type should be object");

        // Assert: All definitions should be present
        _schema.Definitions.Should().ContainKey("NPCPhysicalDescriptor", "should define NPCPhysicalDescriptor");
        _schema.Definitions.Should().ContainKey("NPCAmbientBarkDescriptor", "should define NPCAmbientBarkDescriptor");
        _schema.Definitions.Should().ContainKey("NPCReactionDescriptor", "should define NPCReactionDescriptor");
        _schema.Definitions.Should().ContainKey("NPCArchetype", "should define NPCArchetype");
        _schema.Definitions.Should().ContainKey("NPCCondition", "should define NPCCondition");
        _schema.Definitions.Should().ContainKey("Disposition", "should define Disposition");
        _schema.Definitions.Should().ContainKey("BarkType", "should define BarkType");
        _schema.Definitions.Should().ContainKey("ReactionType", "should define ReactionType");
        _schema.Definitions.Should().ContainKey("TriggerEvent", "should define TriggerEvent");
    }

    #endregion

    #region NPC-002: Existing Configuration Files Pass Validation

    /// <summary>
    /// NPC-002: Verifies that physical-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task NPCDescriptorSchema_PhysicalDescriptors_PassesValidation()
    {
        // Arrange: Load the actual physical-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(PhysicalDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing physical-descriptors.json should validate against schema without errors");
    }

    /// <summary>
    /// NPC-002: Verifies that ambient-barks.json passes schema validation.
    /// </summary>
    [Test]
    public async Task NPCDescriptorSchema_AmbientBarks_PassesValidation()
    {
        // Arrange: Load the actual ambient-barks.json file
        var jsonContent = await File.ReadAllTextAsync(AmbientBarksPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing ambient-barks.json should validate against schema without errors");
    }

    /// <summary>
    /// NPC-002: Verifies that reaction-descriptors.json passes schema validation.
    /// </summary>
    [Test]
    public async Task NPCDescriptorSchema_ReactionDescriptors_PassesValidation()
    {
        // Arrange: Load the actual reaction-descriptors.json file
        var jsonContent = await File.ReadAllTextAsync(ReactionDescriptorsPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing reaction-descriptors.json should validate against schema without errors");
    }

    #endregion

    #region NPC-003: NPCPhysicalDescriptor Validation

    /// <summary>
    /// NPC-003: Verifies that minimal valid physical descriptor passes validation.
    /// </summary>
    [Test]
    public void NPCPhysicalDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid physical configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "dvergr_tinkerer_fullbody": [
                    {
                        "id": "test_001",
                        "text": "A stocky Dvergr covered in soot.",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal physical descriptor should validate successfully");
    }

    /// <summary>
    /// NPC-003: Verifies that complete physical descriptor passes validation.
    /// </summary>
    [Test]
    public void NPCPhysicalDescriptor_Complete_PassesValidation()
    {
        // Arrange: Complete physical configuration with all optional fields
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "dvergr_tinkerer_fullbody": [
                    {
                        "id": "dvergr_tinkerer_fullbody_001",
                        "text": "A stocky Dvergr covered in soot and machine oil.",
                        "weight": 10,
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody",
                        "condition": "Healthy",
                        "biome": null,
                        "ageCategory": "MiddleAged",
                        "tags": ["Memorable", "Technical"]
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "complete physical descriptor should validate successfully");
    }

    /// <summary>
    /// NPC-003: Verifies that descriptor missing required 'archetype' fails validation.
    /// </summary>
    [Test]
    public void NPCPhysicalDescriptor_MissingArchetype_FailsValidation()
    {
        // Arrange: Descriptor missing required 'archetype'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "A stocky figure...",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "descriptor missing 'archetype' should fail validation");
    }

    /// <summary>
    /// NPC-003: Verifies that descriptor missing required 'descriptorType' fails validation.
    /// </summary>
    [Test]
    public void NPCPhysicalDescriptor_MissingDescriptorType_FailsValidation()
    {
        // Arrange: Descriptor missing required 'descriptorType'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "A stocky figure...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "descriptor missing 'descriptorType' should fail validation");
    }

    /// <summary>
    /// NPC-003: Verifies that all valid descriptor types pass validation.
    /// </summary>
    [Test]
    [TestCase("FullBody")]
    [TestCase("Face")]
    [TestCase("Clothing")]
    [TestCase("Equipment")]
    [TestCase("Bearing")]
    [TestCase("Distinguishing")]
    public void NPCPhysicalDescriptor_ValidDescriptorType_PassesValidation(string descriptorType)
    {
        // Arrange: Descriptor with valid descriptor type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "{{descriptorType}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"descriptorType '{descriptorType}' should pass validation");
    }

    /// <summary>
    /// NPC-003: Verifies that invalid descriptor type fails validation.
    /// </summary>
    [Test]
    public void NPCPhysicalDescriptor_InvalidDescriptorType_FailsValidation()
    {
        // Arrange: Descriptor with invalid descriptor type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "InvalidType"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid descriptorType 'InvalidType' should fail validation");
    }

    #endregion

    #region NPC-004: NPCAmbientBarkDescriptor Validation

    /// <summary>
    /// NPC-004: Verifies that minimal valid bark descriptor passes validation.
    /// </summary>
    [Test]
    public void NPCAmbientBarkDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid bark configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "dvergr_tinkerer_atwork": [
                    {
                        "id": "test_001",
                        "text": "Tolerance specifications are off...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "barkType": "AtWork"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal bark descriptor should validate successfully");
    }

    /// <summary>
    /// NPC-004: Verifies that bark descriptor missing 'barkType' fails validation.
    /// </summary>
    [Test]
    public void NPCAmbientBarkDescriptor_MissingBarkType_FailsValidation()
    {
        // Arrange: Descriptor missing required 'barkType'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test bark...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "bark descriptor missing 'barkType' should fail validation");
    }

    /// <summary>
    /// NPC-004: Verifies that all valid bark types pass validation.
    /// </summary>
    [Test]
    [TestCase("AtWork")]
    [TestCase("IdleConversation")]
    [TestCase("Observation")]
    [TestCase("Warning")]
    [TestCase("Celebration")]
    [TestCase("Concern")]
    [TestCase("Suspicion")]
    [TestCase("Encouragement")]
    [TestCase("Complaint")]
    [TestCase("Teaching")]
    [TestCase("Threat")]
    [TestCase("Insult")]
    [TestCase("Wounded")]
    [TestCase("Fleeing")]
    [TestCase("BattleCry")]
    [TestCase("Greeting")]
    public void NPCAmbientBarkDescriptor_ValidBarkType_PassesValidation(string barkType)
    {
        // Arrange: Descriptor with valid bark type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test bark...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "barkType": "{{barkType}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"barkType '{barkType}' should pass validation");
    }

    /// <summary>
    /// NPC-004: Verifies that invalid bark type fails validation.
    /// </summary>
    [Test]
    public void NPCAmbientBarkDescriptor_InvalidBarkType_FailsValidation()
    {
        // Arrange: Descriptor with invalid bark type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test bark...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "barkType": "InvalidBarkType"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid barkType should fail validation");
    }

    #endregion

    #region NPC-005: NPCReactionDescriptor Validation

    /// <summary>
    /// NPC-005: Verifies that minimal valid reaction descriptor passes validation.
    /// </summary>
    [Test]
    public void NPCReactionDescriptor_MinimalValid_PassesValidation()
    {
        // Arrange: Minimal valid reaction configuration
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "dvergr_tinkerer_impressed": [
                    {
                        "id": "test_001",
                        "text": "You have the look of someone who can fix things.",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "Impressed",
                        "triggerEvent": "MechanismRepaired"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "minimal reaction descriptor should validate successfully");
    }

    /// <summary>
    /// NPC-005: Verifies that complete reaction descriptor passes validation.
    /// </summary>
    [Test]
    public void NPCReactionDescriptor_Complete_PassesValidation()
    {
        // Arrange: Complete reaction configuration with all optional fields
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "dvergr_tinkerer_impressed": [
                    {
                        "id": "dvergr_tinkerer_impressed_001",
                        "text": "You have the look of someone who can actually fix things. Rare, these days.",
                        "weight": 10,
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "Impressed",
                        "triggerEvent": "MechanismRepaired",
                        "intensity": "Moderate",
                        "priorDisposition": "Neutral",
                        "actionTendency": "Assist",
                        "biome": null,
                        "tags": ["Social", "Positive"]
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "complete reaction descriptor should validate successfully");
    }

    /// <summary>
    /// NPC-005: Verifies that reaction descriptor missing 'triggerEvent' fails validation.
    /// </summary>
    [Test]
    public void NPCReactionDescriptor_MissingTriggerEvent_FailsValidation()
    {
        // Arrange: Descriptor missing required 'triggerEvent'
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test reaction...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "Impressed"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "reaction descriptor missing 'triggerEvent' should fail validation");
    }

    /// <summary>
    /// NPC-005: Verifies that all valid reaction types pass validation.
    /// </summary>
    [Test]
    [TestCase("Surprised")]
    [TestCase("Angry")]
    [TestCase("Fearful")]
    [TestCase("Relieved")]
    [TestCase("Suspicious")]
    [TestCase("Joyful")]
    [TestCase("Pained")]
    [TestCase("Confused")]
    [TestCase("Impressed")]
    [TestCase("Disgusted")]
    [TestCase("Grateful")]
    [TestCase("Betrayed")]
    [TestCase("Proud")]
    [TestCase("Curious")]
    [TestCase("Resigned")]
    public void NPCReactionDescriptor_ValidReactionType_PassesValidation(string reactionType)
    {
        // Arrange: Descriptor with valid reaction type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test reaction...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "{{reactionType}}",
                        "triggerEvent": "PlayerApproaches"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"reactionType '{reactionType}' should pass validation");
    }

    /// <summary>
    /// NPC-005: Verifies that invalid reaction type fails validation.
    /// </summary>
    [Test]
    public void NPCReactionDescriptor_InvalidReactionType_FailsValidation()
    {
        // Arrange: Descriptor with invalid reaction type
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test reaction...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "InvalidReaction",
                        "triggerEvent": "PlayerApproaches"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid reactionType should fail validation");
    }

    #endregion

    #region NPC-006: NPCArchetype Enum Validation

    /// <summary>
    /// NPC-006: Verifies that all 8 NPC archetypes pass validation.
    /// </summary>
    [Test]
    [TestCase("Dvergr")]
    [TestCase("Seidkona")]
    [TestCase("Bandit")]
    [TestCase("Raider")]
    [TestCase("Merchant")]
    [TestCase("Guard")]
    [TestCase("Citizen")]
    [TestCase("Forlorn")]
    public void NPCArchetype_ValidArchetype_PassesValidation(string archetype)
    {
        // Arrange: Descriptor with valid archetype
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "{{archetype}}",
                        "subtype": "TestSubtype",
                        "descriptorType": "FullBody"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"archetype '{archetype}' should pass validation");
    }

    /// <summary>
    /// NPC-006: Verifies that invalid archetype fails validation.
    /// </summary>
    [Test]
    [TestCase("InvalidArchetype")]
    [TestCase("dvergr")]
    [TestCase("DVERGR")]
    public void NPCArchetype_InvalidArchetype_FailsValidation(string archetype)
    {
        // Arrange: Descriptor with invalid archetype
        var invalidJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "{{archetype}}",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"invalid archetype '{archetype}' should fail validation");
    }

    #endregion

    #region NPC-007: Disposition Enum Validation

    /// <summary>
    /// NPC-007: Verifies that all valid dispositions pass validation.
    /// </summary>
    [Test]
    [TestCase("Hostile")]
    [TestCase("Unfriendly")]
    [TestCase("Neutral")]
    [TestCase("Friendly")]
    [TestCase("Allied")]
    public void Disposition_ValidDisposition_PassesValidation(string disposition)
    {
        // Arrange: Descriptor with valid disposition
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test bark...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "barkType": "AtWork",
                        "dispositionContext": "{{disposition}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"disposition '{disposition}' should pass validation");
    }

    /// <summary>
    /// NPC-007: Verifies that null disposition passes validation (optional field).
    /// </summary>
    [Test]
    public void Disposition_Null_PassesValidation()
    {
        // Arrange: Descriptor with null disposition
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test bark...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "barkType": "AtWork",
                        "dispositionContext": null
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "null disposition should pass validation");
    }

    /// <summary>
    /// NPC-007: Verifies that invalid disposition fails validation.
    /// </summary>
    [Test]
    [TestCase("hostile")]
    [TestCase("VeryFriendly")]
    [TestCase("Invalid")]
    public void Disposition_InvalidDisposition_FailsValidation(string disposition)
    {
        // Arrange: Descriptor with invalid disposition
        var invalidJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-barks",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test bark...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "barkType": "AtWork",
                        "dispositionContext": "{{disposition}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"invalid disposition '{disposition}' should fail validation");
    }

    #endregion

    #region NPC-008: TriggerEvent Enum Validation

    /// <summary>
    /// NPC-008: Verifies that all valid trigger events pass validation.
    /// </summary>
    [Test]
    [TestCase("PlayerApproaches")]
    [TestCase("PlayerAttacks")]
    [TestCase("PlayerHelps")]
    [TestCase("PlayerGifts")]
    [TestCase("PlayerSteals")]
    [TestCase("AllyKilled")]
    [TestCase("EnemyKilled")]
    [TestCase("TakingDamage")]
    [TestCase("VictoryAchieved")]
    [TestCase("TreasureFound")]
    [TestCase("SecretRevealed")]
    [TestCase("MechanismRepaired")]
    [TestCase("AncientKnowledgeFound")]
    [TestCase("TrapTriggered")]
    [TestCase("BlightEncounter")]
    [TestCase("StructuralCollapse")]
    [TestCase("AmbushDetected")]
    [TestCase("QuestCompleted")]
    [TestCase("BetrayalDetected")]
    [TestCase("GiftReceived")]
    [TestCase("TheftDetected")]
    [TestCase("MagicWitnessed")]
    [TestCase("RuneActivated")]
    [TestCase("ProphecyFulfilled")]
    public void TriggerEvent_ValidEvent_PassesValidation(string triggerEvent)
    {
        // Arrange: Descriptor with valid trigger event
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test reaction...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "Impressed",
                        "triggerEvent": "{{triggerEvent}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"triggerEvent '{triggerEvent}' should pass validation");
    }

    /// <summary>
    /// NPC-008: Verifies that invalid trigger event fails validation.
    /// </summary>
    [Test]
    public void TriggerEvent_InvalidEvent_FailsValidation()
    {
        // Arrange: Descriptor with invalid trigger event
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-reactions",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test reaction...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "reactionType": "Impressed",
                        "triggerEvent": "InvalidTrigger"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid triggerEvent should fail validation");
    }

    #endregion

    #region NPC-009: NPCCondition Enum Validation

    /// <summary>
    /// NPC-009: Verifies that all valid NPC conditions pass validation.
    /// </summary>
    [Test]
    [TestCase("Healthy")]
    [TestCase("Wounded")]
    [TestCase("Exhausted")]
    [TestCase("Affluent")]
    [TestCase("Impoverished")]
    [TestCase("BattleReady")]
    public void NPCCondition_ValidCondition_PassesValidation(string condition)
    {
        // Arrange: Descriptor with valid condition
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody",
                        "condition": "{{condition}}"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"condition '{condition}' should pass validation");
    }

    /// <summary>
    /// NPC-009: Verifies that null condition passes validation (optional field).
    /// </summary>
    [Test]
    public void NPCCondition_Null_PassesValidation()
    {
        // Arrange: Descriptor with null condition
        var validJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody",
                        "condition": null
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "null condition should pass validation");
    }

    /// <summary>
    /// NPC-009: Verifies that invalid condition fails validation.
    /// </summary>
    [Test]
    public void NPCCondition_InvalidCondition_FailsValidation()
    {
        // Arrange: Descriptor with invalid condition
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody",
                        "condition": "InvalidCondition"
                    }
                ]
            }
        }
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "invalid condition should fail validation");
    }

    #endregion

    #region NPC-010: Configuration Requirements Validation

    /// <summary>
    /// NPC-010: Verifies that configuration missing 'version' fails validation.
    /// </summary>
    [Test]
    public void Configuration_MissingVersion_FailsValidation()
    {
        // Arrange: Configuration missing required 'version'
        var invalidJson = """
        {
            "category": "npc-physical",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
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
    /// NPC-010: Verifies that configuration missing 'category' fails validation.
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
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
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
    /// NPC-010: Verifies that all valid categories pass validation.
    /// </summary>
    [Test]
    [TestCase("npc-physical")]
    [TestCase("npc-barks")]
    [TestCase("npc-reactions")]
    public void Configuration_ValidCategory_PassesValidation(string category)
    {
        // Arrange: Configuration with valid category
        // Use minimal descriptor that works for physical type
        var validJson = $$"""
        {
            "version": "1.0.0",
            "category": "{{category}}",
            "pools": {
                "test_pool": [
                    {
                        "id": "test_001",
                        "text": "Test descriptor...",
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
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
    /// NPC-010: Verifies that invalid category fails validation.
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
                        "archetype": "Dvergr",
                        "subtype": "Tinkerer",
                        "descriptorType": "FullBody"
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

    /// <summary>
    /// NPC-010: Verifies that empty pool fails validation.
    /// </summary>
    [Test]
    public void Configuration_EmptyPool_FailsValidation()
    {
        // Arrange: Configuration with empty pool
        var invalidJson = """
        {
            "version": "1.0.0",
            "category": "npc-physical",
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
}
