// ------------------------------------------------------------------------------
// <copyright file="QuestsSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for quests.schema.json validation.
// Part of v0.14.10b implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the quests.schema.json JSON Schema.
/// Tests ensure the schema correctly validates quest configuration files.
/// </summary>
[TestFixture]
public class QuestsSchemaTests
{
    private const string SchemaPath = "../../../../../config/schemas/quests.schema.json";
    private const string QuestsConfigPath = "../../../../../config/quests.json";
    private JsonSchema _schema = null!;

    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region QST-001: Schema Loads and Contains Expected Definitions

    [Test]
    public void QuestsSchema_LoadsAndParses_Successfully()
    {
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Quest Configuration");
        _schema.Type.Should().Be(JsonObjectType.Object);

        _schema.Definitions.Should().ContainKey("Quest");
        _schema.Definitions.Should().ContainKey("QuestStage");
        _schema.Definitions.Should().ContainKey("Objective");
        _schema.Definitions.Should().ContainKey("ObjectiveCondition");
        _schema.Definitions.Should().ContainKey("Prerequisite");
        _schema.Definitions.Should().ContainKey("Reward");
        _schema.Definitions.Should().ContainKey("QuestBranch");
        _schema.Definitions.Should().ContainKey("BranchCondition");
        _schema.Definitions.Should().ContainKey("FailureCondition");
        _schema.Definitions.Should().ContainKey("Consequence");
        _schema.Definitions.Should().ContainKey("StageAction");
        _schema.Definitions.Should().ContainKey("QuestGiver");
    }

    [Test]
    public async Task QuestsSchema_ExistingConfiguration_PassesValidation()
    {
        var jsonContent = await File.ReadAllTextAsync(QuestsConfigPath);
        var errors = _schema.Validate(jsonContent);
        errors.Should().BeEmpty("existing quests.json should validate against schema");
    }

    #endregion

    #region QST-002: Quest Identity Validation

    [Test]
    [TestCase("iron-path")]
    [TestCase("lost-tools")]
    [TestCase("patrol-route")]
    [TestCase("a1")]
    public void QuestIdentity_ValidId_PassesValidation(string id)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "quests": [{
                "id": "{{id}}",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "SideQuest",
                "stages": [{
                    "id": "stage-1",
                    "name": "Stage One",
                    "description": "First stage",
                    "objectives": [{
                        "id": "obj-1",
                        "type": "Talk",
                        "description": "Talk to NPC"
                    }]
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"quest with valid ID '{id}' should pass validation");
    }

    [Test]
    public void QuestIdentity_UppercaseId_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "IronPath",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("quest with uppercase ID should fail validation");
    }

    [Test]
    public void QuestIdentity_EmptyName_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "",
                "description": "A test quest for validation purposes.",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("quest with empty name should fail validation");
    }

    #endregion

    #region QST-003: Quest Category Validation

    [Test]
    [TestCase("MainStory")]
    [TestCase("SideQuest")]
    [TestCase("FactionQuest")]
    [TestCase("Bounty")]
    [TestCase("Exploration")]
    [TestCase("Crafting")]
    [TestCase("Daily")]
    [TestCase("Event")]
    public void QuestCategory_ValidEnum_PassesValidation(string category)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "{{category}}",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"category '{category}' should pass validation");
    }

    [Test]
    public void QuestCategory_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "InvalidCategory",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid category should fail validation");
    }

    [Test]
    public void QuestCategory_LowercaseEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "mainstory",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("lowercase category should fail validation (case-sensitive)");
    }

    #endregion

    #region QST-004: Stage Sequence Validation

    [Test]
    public void StageSequence_ValidStages_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "SideQuest",
                "stages": [
                    { "id": "stage-1", "name": "First Stage", "description": "Start here", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }], "nextStageId": "stage-2" },
                    { "id": "stage-2", "name": "Final Stage", "description": "End here", "objectives": [{ "id": "o2", "type": "Talk", "description": "Return here" }], "nextStageId": null }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid stage sequence should pass validation");
    }

    [Test]
    public void StageSequence_EmptyStages_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "SideQuest",
                "stages": []
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("quest with empty stages should fail validation");
    }

    [Test]
    public void StageSequence_MissingObjectives_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test Quest",
                "description": "A test quest for validation purposes.",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "Stage", "description": "Stage desc" }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("stage missing objectives should fail validation");
    }

    #endregion

    #region QST-005: Objective Type Validation

    [Test]
    [TestCase("Kill")]
    [TestCase("Collect")]
    [TestCase("Talk")]
    [TestCase("Explore")]
    [TestCase("Escort")]
    [TestCase("Defend")]
    [TestCase("Craft")]
    [TestCase("Discover")]
    [TestCase("Deliver")]
    [TestCase("Use")]
    [TestCase("Survive")]
    [TestCase("Reach")]
    public void ObjectiveType_ValidEnum_PassesValidation(string objType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "{{objType}}", "description": "Objective task" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"objective type '{objType}' should pass validation");
    }

    [Test]
    public void ObjectiveType_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Hunt", "description": "Objective task" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid objective type 'Hunt' should fail validation");
    }

    #endregion

    #region QST-006: Reward Type Validation

    [Test]
    [TestCase("Experience")]
    [TestCase("Gold")]
    [TestCase("Item")]
    [TestCase("Reputation")]
    [TestCase("Ability")]
    [TestCase("Title")]
    [TestCase("Unlock")]
    [TestCase("Flag")]
    public void RewardType_ValidEnum_PassesValidation(string rewardType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }],
                "rewards": [{ "type": "{{rewardType}}" }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"reward type '{rewardType}' should pass validation");
    }

    [Test]
    public void RewardType_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }],
                "rewards": [{ "type": "Money" }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid reward type 'Money' should fail validation");
    }

    [Test]
    public void RewardType_WithFullDetails_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }],
                "rewards": [
                    { "type": "Experience", "amount": 500, "description": "+500 Legend" },
                    { "type": "Reputation", "target": "rust-clans", "amount": 50 }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("reward with full details should pass validation");
    }

    #endregion

    #region Additional Validation Tests

    [Test]
    public void Prerequisite_ValidTypes_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "prerequisites": [
                    { "type": "Level", "value": 5, "operator": "greaterThan" },
                    { "type": "QuestComplete", "target": "intro-quest", "operator": "has" }
                ],
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid prerequisites should pass validation");
    }

    [Test]
    public void FailureCondition_ValidTypes_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }],
                "failureConditions": [
                    { "type": "NPCDeath", "target": "npc-test", "description": "NPC died" },
                    { "type": "TimeExpired", "description": "Time ran out" }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid failure conditions should pass validation");
    }

    [Test]
    public void QuestBranch_ValidStructure_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "stages": [
                    { "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] },
                    { "id": "alt-s1", "name": "Alt S1", "description": "Alt Stage", "objectives": [{ "id": "o2", "type": "Talk", "description": "Talk to NPC" }], "isOptional": true }
                ],
                "branches": [{
                    "id": "branch-1",
                    "condition": { "type": "ObjectiveComplete", "target": "o1" },
                    "targetStageId": "alt-s1"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid branch structure should pass validation");
    }

    [Test]
    public void QuestGiver_ValidStructure_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "quests": [{
                "id": "test-quest",
                "name": "Test",
                "description": "Test quest description",
                "category": "SideQuest",
                "questGiver": {
                    "npcId": "npc-test",
                    "dialogueNodeId": "test-dialogue",
                    "turnInNpcId": "npc-other",
                    "turnInDialogueNodeId": "complete-dialogue"
                },
                "stages": [{ "id": "s1", "name": "S1", "description": "Stage", "objectives": [{ "id": "o1", "type": "Talk", "description": "Talk to NPC" }] }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid quest giver should pass validation");
    }

    #endregion
}
