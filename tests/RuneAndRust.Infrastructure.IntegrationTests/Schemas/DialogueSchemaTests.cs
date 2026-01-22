// ------------------------------------------------------------------------------
// <copyright file="DialogueSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for dialogue.schema.json validation.
// Verifies schema structure, dialogue node validation, option structure,
// skill check attributes, outcome types, and node ID patterns.
// Part of v0.14.9a implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the dialogue.schema.json JSON Schema.
/// Tests ensure the schema correctly validates NPC dialogue tree configuration files,
/// enforces node structure requirements, skill check attribute constraints,
/// outcome type validation, and node ID pattern matching.
/// </summary>
/// <remarks>
/// <para>
/// The dialogue schema provides 5 definitions:
/// <list type="bullet">
/// <item><description>DialogueNode - Single point in conversation with NPC text and options</description></item>
/// <item><description>DialogueOption - Player response choice with navigation and effects</description></item>
/// <item><description>SkillCheck - Attribute-based check with target value</description></item>
/// <item><description>Outcome - Result of selecting an option (combat, reputation, quests, etc.)</description></item>
/// <item><description>Condition - Visibility control for nodes and options</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class DialogueSchemaTests
{
    /// <summary>
    /// Path to the dialogue schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/dialogue.schema.json";

    /// <summary>
    /// Path to the bjorn_dialogues.json configuration file for validation.
    /// </summary>
    private const string BjornDialoguesPath = "../../../../../docs/design/descriptors/dialogues/bjorn_dialogues.json";

    /// <summary>
    /// Path to the sigrun_dialogues.json configuration file for validation.
    /// </summary>
    private const string SigrunDialoguesPath = "../../../../../docs/design/descriptors/dialogues/sigrun_dialogues.json";

    /// <summary>
    /// Path to the astrid_dialogues.json configuration file for validation.
    /// </summary>
    private const string AstridDialoguesPath = "../../../../../docs/design/descriptors/dialogues/astrid_dialogues.json";

    /// <summary>
    /// Path to the eydis_dialogues.json configuration file for validation.
    /// </summary>
    private const string EydisDialoguesPath = "../../../../../docs/design/descriptors/dialogues/eydis_dialogues.json";

    /// <summary>
    /// Path to the gunnar_dialogues.json configuration file for validation.
    /// </summary>
    private const string GunnarDialoguesPath = "../../../../../docs/design/descriptors/dialogues/gunnar_dialogues.json";

    /// <summary>
    /// Path to the kjartan_dialogues.json configuration file for validation.
    /// </summary>
    private const string KjartanDialoguesPath = "../../../../../docs/design/descriptors/dialogues/kjartan_dialogues.json";

    /// <summary>
    /// Path to the rolf_dialogues.json configuration file for validation.
    /// </summary>
    private const string RolfDialoguesPath = "../../../../../docs/design/descriptors/dialogues/rolf_dialogues.json";

    /// <summary>
    /// Path to the thorvald_dialogues.json configuration file for validation.
    /// </summary>
    private const string ThorvaldDialoguesPath = "../../../../../docs/design/descriptors/dialogues/thorvald_dialogues.json";

    /// <summary>
    /// Loaded JSON Schema for dialogue definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the dialogue schema.
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

    #region DLG-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// DLG-001: Verifies that dialogue.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    /// <remarks>
    /// This test validates the foundational schema structure including:
    /// <list type="bullet">
    /// <item><description>Schema parses successfully as JSON Schema</description></item>
    /// <item><description>All required definitions are present (5 total)</description></item>
    /// <item><description>Schema title and type are correct</description></item>
    /// </list>
    /// </remarks>
    [Test]
    public void DialogueSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Dialogue Tree Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Array, "schema type should be array");

        // Assert: All 5 definitions should be present
        _schema.Definitions.Should().ContainKey("DialogueNode", "should define DialogueNode");
        _schema.Definitions.Should().ContainKey("DialogueOption", "should define DialogueOption");
        _schema.Definitions.Should().ContainKey("SkillCheck", "should define SkillCheck");
        _schema.Definitions.Should().ContainKey("Outcome", "should define Outcome");
        _schema.Definitions.Should().ContainKey("Condition", "should define Condition");
    }

    #endregion

    #region DLG-002: Existing Dialogue Files Pass Validation

    /// <summary>
    /// DLG-002: Verifies that bjorn_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_BjornDialogues_PassesValidation()
    {
        // Arrange: Load the actual bjorn_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(BjornDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing bjorn_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that sigrun_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_SigrunDialogues_PassesValidation()
    {
        // Arrange: Load the actual sigrun_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(SigrunDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing sigrun_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that astrid_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_AstridDialogues_PassesValidation()
    {
        // Arrange: Load the actual astrid_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(AstridDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing astrid_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that eydis_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_EydisDialogues_PassesValidation()
    {
        // Arrange: Load the actual eydis_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(EydisDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing eydis_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that gunnar_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_GunnarDialogues_PassesValidation()
    {
        // Arrange: Load the actual gunnar_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(GunnarDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing gunnar_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that kjartan_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_KjartanDialogues_PassesValidation()
    {
        // Arrange: Load the actual kjartan_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(KjartanDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing kjartan_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that rolf_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_RolfDialogues_PassesValidation()
    {
        // Arrange: Load the actual rolf_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(RolfDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing rolf_dialogues.json should validate against schema without errors");
    }

    /// <summary>
    /// DLG-002: Verifies that thorvald_dialogues.json passes schema validation.
    /// </summary>
    [Test]
    public async Task DialogueSchema_ThorvaldDialogues_PassesValidation()
    {
        // Arrange: Load the actual thorvald_dialogues.json file
        var jsonContent = await File.ReadAllTextAsync(ThorvaldDialoguesPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty(
            "existing thorvald_dialogues.json should validate against schema without errors");
    }

    #endregion

    #region DLG-003: Node Structure Validation

    /// <summary>
    /// DLG-003: Verifies that minimal valid node passes validation.
    /// </summary>
    [Test]
    public void NodeStructure_MinimalValidNode_PassesValidation()
    {
        // Arrange: Minimal valid node
        var validJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello, traveler.",
                "Options": [
                    {
                        "Text": "Hello.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for minimal valid node
        errors.Should().BeEmpty(
            "minimal node with required fields should validate successfully");
    }

    /// <summary>
    /// DLG-003: Verifies that node with EndsConversation passes validation.
    /// </summary>
    [Test]
    public void NodeStructure_WithEndsConversation_PassesValidation()
    {
        // Arrange: Node with EndsConversation flag
        var validJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello, traveler.",
                "Options": [
                    {
                        "Text": "Goodbye.",
                        "NextNodeId": null
                    }
                ],
                "EndsConversation": true
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "node with EndsConversation should validate successfully");
    }

    /// <summary>
    /// DLG-003: Verifies that node missing Id fails validation.
    /// </summary>
    [Test]
    public void NodeStructure_MissingId_FailsValidation()
    {
        // Arrange: Node missing required Id field
        var invalidJson = """
        [
            {
                "Text": "Hello, traveler.",
                "Options": [
                    {
                        "Text": "Hello.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "node missing 'Id' should fail validation");
    }

    /// <summary>
    /// DLG-003: Verifies that node missing Text fails validation.
    /// </summary>
    [Test]
    public void NodeStructure_MissingText_FailsValidation()
    {
        // Arrange: Node missing required Text field
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Options": [
                    {
                        "Text": "Hello.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "node missing 'Text' should fail validation");
    }

    /// <summary>
    /// DLG-003: Verifies that node missing Options fails validation.
    /// </summary>
    [Test]
    public void NodeStructure_MissingOptions_FailsValidation()
    {
        // Arrange: Node missing required Options field
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello, traveler."
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for missing required field
        errors.Should().NotBeEmpty(
            "node missing 'Options' should fail validation");
    }

    /// <summary>
    /// DLG-003: Verifies that node with empty Options fails validation.
    /// </summary>
    [Test]
    public void NodeStructure_EmptyOptions_FailsValidation()
    {
        // Arrange: Node with empty Options array
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello, traveler.",
                "Options": []
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for empty Options
        errors.Should().NotBeEmpty(
            "node with empty 'Options' array should fail validation");
    }

    /// <summary>
    /// DLG-003: Verifies that node with empty Text fails validation.
    /// </summary>
    [Test]
    public void NodeStructure_EmptyText_FailsValidation()
    {
        // Arrange: Node with empty Text
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "",
                "Options": [
                    {
                        "Text": "Hello.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for empty Text
        errors.Should().NotBeEmpty(
            "node with empty 'Text' should fail validation");
    }

    /// <summary>
    /// DLG-003: Verifies that node with unknown property fails validation.
    /// </summary>
    [Test]
    public void NodeStructure_UnknownProperty_FailsValidation()
    {
        // Arrange: Node with unknown property
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Hi.",
                        "NextNodeId": null
                    }
                ],
                "Speaker": "Bjorn"
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail for unknown property
        errors.Should().NotBeEmpty(
            "node with unknown property 'Speaker' should fail validation");
    }

    #endregion

    #region DLG-004: Option Structure Validation

    /// <summary>
    /// DLG-004: Verifies that option with null NextNodeId passes validation.
    /// </summary>
    [Test]
    public void OptionStructure_NullNextNodeId_PassesValidation()
    {
        // Arrange: Option with null NextNodeId (ends conversation)
        var validJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Goodbye.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "option with null NextNodeId should validate successfully");
    }

    /// <summary>
    /// DLG-004: Verifies that option with string NextNodeId passes validation.
    /// </summary>
    [Test]
    public void OptionStructure_StringNextNodeId_PassesValidation()
    {
        // Arrange: Option with string NextNodeId
        var validJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Tell me more.",
                        "NextNodeId": "test_info"
                    }
                ]
            },
            {
                "Id": "test_info",
                "Text": "Here's more info.",
                "Options": [
                    {
                        "Text": "Thanks.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "option with string NextNodeId should validate successfully");
    }

    /// <summary>
    /// DLG-004: Verifies that option with SkillCheck passes validation.
    /// </summary>
    [Test]
    public void OptionStructure_WithSkillCheck_PassesValidation()
    {
        // Arrange: Option with SkillCheck
        var validJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "You shall not pass!",
                "Options": [
                    {
                        "Text": "[WILL 4] Stand down.",
                        "NextNodeId": "test_success",
                        "SkillCheck": {
                            "Attribute": "will",
                            "TargetValue": 4,
                            "Skill": null,
                            "SkillRanks": 0
                        }
                    }
                ]
            },
            {
                "Id": "test_success",
                "Text": "Very well.",
                "Options": [
                    {
                        "Text": "Thanks.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "option with SkillCheck should validate successfully");
    }

    /// <summary>
    /// DLG-004: Verifies that option with Outcome passes validation.
    /// </summary>
    [Test]
    public void OptionStructure_WithOutcome_PassesValidation()
    {
        // Arrange: Option with Outcome
        var validJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Need help?",
                "Options": [
                    {
                        "Text": "Sure, I'll help.",
                        "NextNodeId": null,
                        "Outcome": {
                            "Type": "QuestGiven",
                            "Data": "quest_helper",
                            "ReputationChange": 0,
                            "AffectedFaction": null
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "option with Outcome should validate successfully");
    }

    /// <summary>
    /// DLG-004: Verifies that option missing Text fails validation.
    /// </summary>
    [Test]
    public void OptionStructure_MissingText_FailsValidation()
    {
        // Arrange: Option missing Text
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello.",
                "Options": [
                    {
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "option missing 'Text' should fail validation");
    }

    /// <summary>
    /// DLG-004: Verifies that option missing NextNodeId fails validation.
    /// </summary>
    [Test]
    public void OptionStructure_MissingNextNodeId_FailsValidation()
    {
        // Arrange: Option missing NextNodeId
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Hi there."
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "option missing 'NextNodeId' should fail validation");
    }

    /// <summary>
    /// DLG-004: Verifies that option with empty Text fails validation.
    /// </summary>
    [Test]
    public void OptionStructure_EmptyText_FailsValidation()
    {
        // Arrange: Option with empty Text
        var invalidJson = """
        [
            {
                "Id": "test_greeting",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "option with empty 'Text' should fail validation");
    }

    #endregion

    #region DLG-005: SkillCheck Validation

    /// <summary>
    /// DLG-005: Verifies that all valid attributes pass validation.
    /// </summary>
    [Test]
    [TestCase("might")]
    [TestCase("finesse")]
    [TestCase("will")]
    [TestCase("wits")]
    public void SkillCheck_ValidAttribute_PassesValidation(string attribute)
    {
        // Arrange: SkillCheck with valid attribute
        var validJson = $$"""
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "[{{attribute.ToUpper()}} 4] Try it.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "Attribute": "{{attribute}}",
                            "TargetValue": 4,
                            "Skill": null,
                            "SkillRanks": 0
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"SkillCheck with attribute '{attribute}' should pass validation");
    }

    /// <summary>
    /// DLG-005: Verifies that empty attribute for skill-only checks passes validation.
    /// </summary>
    [Test]
    public void SkillCheck_EmptyAttributeSkillOnly_PassesValidation()
    {
        // Arrange: SkillCheck with empty attribute (skill-only check)
        var validJson = """
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "[Bone-Setter] Use special skill.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "Attribute": "",
                            "TargetValue": 0,
                            "Skill": "BoneSetter",
                            "SkillRanks": 0
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass for skill-only check
        errors.Should().BeEmpty(
            "SkillCheck with empty attribute (skill-only) should pass validation");
    }

    /// <summary>
    /// DLG-005: Verifies that invalid attribute fails validation.
    /// </summary>
    [Test]
    public void SkillCheck_InvalidAttribute_FailsValidation()
    {
        // Arrange: SkillCheck with invalid attribute
        var invalidJson = """
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "[STRENGTH 5] Try it.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "Attribute": "strength",
                            "TargetValue": 5
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "SkillCheck with invalid attribute 'strength' should fail validation");
    }

    /// <summary>
    /// DLG-005: Verifies that uppercase attribute fails validation.
    /// </summary>
    [Test]
    public void SkillCheck_UppercaseAttribute_FailsValidation()
    {
        // Arrange: SkillCheck with uppercase attribute
        var invalidJson = """
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "[WILL 4] Try it.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "Attribute": "WILL",
                            "TargetValue": 4
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "SkillCheck with uppercase attribute 'WILL' should fail validation");
    }

    /// <summary>
    /// DLG-005: Verifies that negative TargetValue fails validation.
    /// </summary>
    [Test]
    public void SkillCheck_NegativeTargetValue_FailsValidation()
    {
        // Arrange: SkillCheck with negative TargetValue
        var invalidJson = """
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "[WILL -1] Try it.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "Attribute": "will",
                            "TargetValue": -1
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "SkillCheck with negative TargetValue should fail validation");
    }

    /// <summary>
    /// DLG-005: Verifies that missing Attribute fails validation.
    /// </summary>
    [Test]
    public void SkillCheck_MissingAttribute_FailsValidation()
    {
        // Arrange: SkillCheck missing Attribute
        var invalidJson = """
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "Try it.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "TargetValue": 5
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "SkillCheck missing 'Attribute' should fail validation");
    }

    /// <summary>
    /// DLG-005: Verifies that missing TargetValue fails validation.
    /// </summary>
    [Test]
    public void SkillCheck_MissingTargetValue_FailsValidation()
    {
        // Arrange: SkillCheck missing TargetValue
        var invalidJson = """
        [
            {
                "Id": "test_check",
                "Text": "A challenge!",
                "Options": [
                    {
                        "Text": "Try it.",
                        "NextNodeId": null,
                        "SkillCheck": {
                            "Attribute": "will"
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "SkillCheck missing 'TargetValue' should fail validation");
    }

    #endregion

    #region DLG-006: Outcome Type Validation

    /// <summary>
    /// DLG-006: Verifies that all valid outcome types pass validation.
    /// </summary>
    [Test]
    [TestCase("InitiateCombat", "enemy_id")]
    [TestCase("ReputationChange", "Good deed")]
    [TestCase("Information", "Secret revealed")]
    [TestCase("EndConversation", "")]
    [TestCase("QuestGiven", "quest_id")]
    [TestCase("QuestUpdate", "quest_id:completed")]
    [TestCase("ItemGiven", "item_id")]
    [TestCase("ItemReceived", "item_id")]
    [TestCase("ItemTaken", "item_id")]
    [TestCase("FlagSet", "flag_name")]
    public void OutcomeType_ValidTypes_PassesValidation(string type, string data)
    {
        // Arrange: Outcome with valid type
        var validJson = $$"""
        [
            {
                "Id": "test_outcome",
                "Text": "Something happens.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Outcome": {
                            "Type": "{{type}}",
                            "Data": "{{data}}",
                            "ReputationChange": 0,
                            "AffectedFaction": null
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"Outcome with type '{type}' should pass validation");
    }

    /// <summary>
    /// DLG-006: Verifies that invalid outcome type fails validation.
    /// </summary>
    [Test]
    public void OutcomeType_InvalidType_FailsValidation()
    {
        // Arrange: Outcome with invalid type
        var invalidJson = """
        [
            {
                "Id": "test_outcome",
                "Text": "Something happens.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Outcome": {
                            "Type": "GiveGold",
                            "Data": "100"
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Outcome with invalid type 'GiveGold' should fail validation");
    }

    /// <summary>
    /// DLG-006: Verifies that outcome missing Type fails validation.
    /// </summary>
    [Test]
    public void OutcomeType_MissingType_FailsValidation()
    {
        // Arrange: Outcome missing Type
        var invalidJson = """
        [
            {
                "Id": "test_outcome",
                "Text": "Something happens.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Outcome": {
                            "Data": "test"
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Outcome missing 'Type' should fail validation");
    }

    /// <summary>
    /// DLG-006: Verifies that outcome missing Data fails validation.
    /// </summary>
    [Test]
    public void OutcomeType_MissingData_FailsValidation()
    {
        // Arrange: Outcome missing Data
        var invalidJson = """
        [
            {
                "Id": "test_outcome",
                "Text": "Something happens.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Outcome": {
                            "Type": "InitiateCombat"
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Outcome missing 'Data' should fail validation");
    }

    /// <summary>
    /// DLG-006: Verifies that Outcome with ReputationChange and AffectedFaction passes.
    /// </summary>
    [Test]
    public void OutcomeType_ReputationChangeWithFaction_PassesValidation()
    {
        // Arrange: Outcome with reputation change
        var validJson = """
        [
            {
                "Id": "test_rep",
                "Text": "You helped!",
                "Options": [
                    {
                        "Text": "Thanks.",
                        "NextNodeId": null,
                        "Outcome": {
                            "Type": "ReputationChange",
                            "Data": "Helped the faction",
                            "ReputationChange": 10,
                            "AffectedFaction": "RustClans"
                        }
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "Outcome with ReputationChange and AffectedFaction should pass validation");
    }

    #endregion

    #region DLG-007: Node ID Pattern Validation

    /// <summary>
    /// DLG-007: Verifies that valid node IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("npc_greeting")]
    [TestCase("npc_quest_hook")]
    [TestCase("merchant_1_trade")]
    [TestCase("a")]
    [TestCase("abc123")]
    [TestCase("test_node_1")]
    [TestCase("forlorn_warning_final")]
    public void NodeIdPattern_ValidIds_PassesValidation(string nodeId)
    {
        // Arrange: Node with valid ID
        var validJson = $$"""
        [
            {
                "Id": "{{nodeId}}",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Hi.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"Node ID '{nodeId}' should pass validation");
    }

    /// <summary>
    /// DLG-007: Verifies that invalid node IDs fail validation.
    /// </summary>
    [Test]
    [TestCase("NPC_greeting", "uppercase")]
    [TestCase("npc-greeting", "hyphen")]
    [TestCase("1_greeting", "starts with number")]
    [TestCase("_greeting", "starts with underscore")]
    public void NodeIdPattern_InvalidIds_FailsValidation(string nodeId, string reason)
    {
        // Arrange: Node with invalid ID
        var invalidJson = $$"""
        [
            {
                "Id": "{{nodeId}}",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Hi.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            $"Node ID '{nodeId}' should fail validation ({reason})");
    }

    /// <summary>
    /// DLG-007: Verifies that empty node ID fails validation.
    /// </summary>
    [Test]
    public void NodeIdPattern_Empty_FailsValidation()
    {
        // Arrange: Node with empty ID
        var invalidJson = """
        [
            {
                "Id": "",
                "Text": "Hello.",
                "Options": [
                    {
                        "Text": "Hi.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Empty node ID should fail validation");
    }

    #endregion

    #region DLG-008: Condition Validation

    /// <summary>
    /// DLG-008: Verifies that conditions on nodes pass validation.
    /// </summary>
    [Test]
    public void Condition_OnNode_PassesValidation()
    {
        // Arrange: Node with conditions
        var validJson = """
        [
            {
                "Id": "test_conditional",
                "Text": "You've proven yourself.",
                "Options": [
                    {
                        "Text": "Thanks.",
                        "NextNodeId": null
                    }
                ],
                "Conditions": [
                    {
                        "Type": "HasReputation",
                        "Target": "RustClans",
                        "Value": 25,
                        "Operator": "greaterThanOrEquals"
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "Node with conditions should pass validation");
    }

    /// <summary>
    /// DLG-008: Verifies that conditions on options pass validation.
    /// </summary>
    [Test]
    public void Condition_OnOption_PassesValidation()
    {
        // Arrange: Option with conditions
        var validJson = """
        [
            {
                "Id": "test_conditional",
                "Text": "What do you want?",
                "Options": [
                    {
                        "Text": "I've already helped you.",
                        "NextNodeId": "test_grateful",
                        "Conditions": [
                            {
                                "Type": "HasQuestState",
                                "Target": "quest_helper",
                                "Value": "completed",
                                "Operator": "equals"
                            }
                        ]
                    },
                    {
                        "Text": "Nothing.",
                        "NextNodeId": null
                    }
                ]
            },
            {
                "Id": "test_grateful",
                "Text": "Thank you again!",
                "Options": [
                    {
                        "Text": "Sure.",
                        "NextNodeId": null
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            "Option with conditions should pass validation");
    }

    /// <summary>
    /// DLG-008: Verifies that all valid condition types pass validation.
    /// </summary>
    [Test]
    [TestCase("HasItem", "scrap_metal", "5")]
    [TestCase("HasReputation", "MidgardCombine", "25")]
    [TestCase("HasQuestState", "quest_helper", "completed")]
    [TestCase("HasFlag", "boss_defeated", "true")]
    [TestCase("SkillLevel", "Persuasion", "3")]
    public void Condition_ValidTypes_PassesValidation(string type, string target, string value)
    {
        // Determine if value should be integer or string
        var valueJson = int.TryParse(value, out _) ? value : $"\"{value}\"";

        // Arrange: Condition with valid type
        var validJson = $$"""
        [
            {
                "Id": "test_condition",
                "Text": "Testing.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Conditions": [
                            {
                                "Type": "{{type}}",
                                "Target": "{{target}}",
                                "Value": {{valueJson}},
                                "Operator": "equals"
                            }
                        ]
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"Condition with type '{type}' should pass validation");
    }

    /// <summary>
    /// DLG-008: Verifies that all valid operators pass validation.
    /// </summary>
    [Test]
    [TestCase("equals")]
    [TestCase("notEquals")]
    [TestCase("greaterThan")]
    [TestCase("lessThan")]
    [TestCase("greaterThanOrEquals")]
    [TestCase("lessThanOrEquals")]
    public void Condition_ValidOperators_PassesValidation(string op)
    {
        // Arrange: Condition with valid operator
        var validJson = $$"""
        [
            {
                "Id": "test_condition",
                "Text": "Testing.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Conditions": [
                            {
                                "Type": "HasReputation",
                                "Target": "RustClans",
                                "Value": 25,
                                "Operator": "{{op}}"
                            }
                        ]
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(validJson);

        // Assert: Validation should pass
        errors.Should().BeEmpty(
            $"Condition with operator '{op}' should pass validation");
    }

    /// <summary>
    /// DLG-008: Verifies that invalid condition type fails validation.
    /// </summary>
    [Test]
    public void Condition_InvalidType_FailsValidation()
    {
        // Arrange: Condition with invalid type
        var invalidJson = """
        [
            {
                "Id": "test_condition",
                "Text": "Testing.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Conditions": [
                            {
                                "Type": "HasGold",
                                "Target": "gold",
                                "Value": 100,
                                "Operator": "greaterThan"
                            }
                        ]
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Condition with invalid type 'HasGold' should fail validation");
    }

    /// <summary>
    /// DLG-008: Verifies that invalid operator fails validation.
    /// </summary>
    [Test]
    public void Condition_InvalidOperator_FailsValidation()
    {
        // Arrange: Condition with invalid operator
        var invalidJson = """
        [
            {
                "Id": "test_condition",
                "Text": "Testing.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Conditions": [
                            {
                                "Type": "HasReputation",
                                "Target": "RustClans",
                                "Value": 25,
                                "Operator": "contains"
                            }
                        ]
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Condition with invalid operator 'contains' should fail validation");
    }

    /// <summary>
    /// DLG-008: Verifies that condition missing required fields fails validation.
    /// </summary>
    [Test]
    public void Condition_MissingRequiredFields_FailsValidation()
    {
        // Arrange: Condition missing required fields
        var invalidJson = """
        [
            {
                "Id": "test_condition",
                "Text": "Testing.",
                "Options": [
                    {
                        "Text": "Okay.",
                        "NextNodeId": null,
                        "Conditions": [
                            {
                                "Type": "HasReputation"
                            }
                        ]
                    }
                ]
            }
        ]
        """;

        // Act: Validate the JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail
        errors.Should().NotBeEmpty(
            "Condition missing 'Target' and 'Value' should fail validation");
    }

    #endregion
}
