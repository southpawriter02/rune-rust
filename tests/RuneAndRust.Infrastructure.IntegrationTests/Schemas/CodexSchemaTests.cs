// ------------------------------------------------------------------------------
// <copyright file="CodexSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for codex.schema.json validation.
// Verifies schema structure, entry identity validation, category references,
// unlock condition types, progression thresholds, and keyword matching.
// Part of v0.14.10c implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the codex.schema.json JSON Schema.
/// Tests ensure the schema correctly validates codex configuration files,
/// enforces entry ID patterns, category references, unlock conditions,
/// progression thresholds, and capture type requirements.
/// </summary>
/// <remarks>
/// <para>
/// The codex schema provides 9 definitions:
/// <list type="bullet">
/// <item><description>CodexCategory - Category for organizing entries</description></item>
/// <item><description>CodexSubcategory - Subcategory within a category</description></item>
/// <item><description>CodexEntry - Entry with sections, keywords, and unlock conditions</description></item>
/// <item><description>EntrySection - Progressive content section</description></item>
/// <item><description>UnlockCondition - Polymorphic unlock condition (8 types)</description></item>
/// <item><description>EntryRelation - Typed relationship to other entries</description></item>
/// <item><description>ProgressionLevel - Revelation stage definition</description></item>
/// <item><description>ProgressionReward - Reward granted at progression levels</description></item>
/// <item><description>ProgressionDefaults - Default progression configuration</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class CodexSchemaTests
{
    /// <summary>
    /// Path to the codex schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/codex.schema.json";

    /// <summary>
    /// Path to the codex.json configuration file for validation.
    /// </summary>
    private const string CodexConfigPath = "../../../../../config/codex.json";

    /// <summary>
    /// Loaded JSON Schema for codex definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the codex schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region COD-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// COD-001: Verifies that codex.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    [Test]
    public void CodexSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Codex Configuration Schema");
        _schema.Type.Should().Be(JsonObjectType.Object);

        // Assert: All 9 definitions should be present
        _schema.Definitions.Should().ContainKey("CodexCategory");
        _schema.Definitions.Should().ContainKey("CodexSubcategory");
        _schema.Definitions.Should().ContainKey("CodexEntry");
        _schema.Definitions.Should().ContainKey("EntrySection");
        _schema.Definitions.Should().ContainKey("UnlockCondition");
        _schema.Definitions.Should().ContainKey("EntryRelation");
        _schema.Definitions.Should().ContainKey("ProgressionLevel");
        _schema.Definitions.Should().ContainKey("ProgressionReward");
        _schema.Definitions.Should().ContainKey("ProgressionDefaults");
    }

    /// <summary>
    /// COD-001: Verifies that the actual codex.json configuration passes validation.
    /// </summary>
    [Test]
    public async Task CodexSchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual codex.json file
        var jsonContent = await File.ReadAllTextAsync(CodexConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty("existing codex.json should validate against schema");
    }

    #endregion

    #region COD-002: Entry Identity Validation

    /// <summary>
    /// COD-002: Verifies that valid entry IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("codex-blight-001")]
    [TestCase("codex-beast-012")]
    [TestCase("codex-faction-003")]
    [TestCase("codex-lore-999")]
    public void EntryIdentity_ValidId_PassesValidation(string id)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "{{id}}",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{
                    "id": "section-overview",
                    "title": "Overview",
                    "content": "Test content for the section.",
                    "unlockThreshold": 0.0
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"entry with valid ID '{id}' should pass validation");
    }

    /// <summary>
    /// COD-002: Verifies that invalid entry ID format fails validation.
    /// </summary>
    [Test]
    public void EntryIdentity_InvalidIdFormat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "invalid_id_format",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("entry with invalid ID format should fail validation");
    }

    /// <summary>
    /// COD-002: Verifies that entry ID without number suffix fails validation.
    /// </summary>
    [Test]
    public void EntryIdentity_MissingNumberSuffix_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-blight",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("entry ID without number suffix should fail validation");
    }

    /// <summary>
    /// COD-002: Verifies that empty entry title fails validation.
    /// </summary>
    [Test]
    public void EntryIdentity_EmptyTitle_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-blight-001",
                "categoryId": "lore",
                "title": "",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("entry with empty title should fail validation");
    }

    #endregion

    #region COD-003: Category and Subcategory Validation

    /// <summary>
    /// COD-003: Verifies that valid category structure passes validation.
    /// </summary>
    [Test]
    public void Category_ValidStructure_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [
                {
                    "id": "lore",
                    "name": "Lore",
                    "description": "Historical events and world history",
                    "icon": "icon-lore-book",
                    "sortOrder": 1,
                    "subcategories": [
                        { "id": "origins", "name": "Origins", "sortOrder": 1 },
                        { "id": "history", "name": "History", "sortOrder": 2 }
                    ]
                }
            ],
            "entries": []
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid category structure should pass validation");
    }

    /// <summary>
    /// COD-003: Verifies that category with invalid ID pattern fails validation.
    /// </summary>
    [Test]
    public void Category_InvalidIdPattern_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "Lore", "name": "Lore" }],
            "entries": []
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("category with uppercase ID should fail validation");
    }

    /// <summary>
    /// COD-003: Verifies that empty categories array fails validation.
    /// </summary>
    [Test]
    public void Category_EmptyArray_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [],
            "entries": []
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty categories array should fail validation (minItems: 1)");
    }

    #endregion

    #region COD-004: Unlock Condition Validation

    /// <summary>
    /// COD-004: Verifies that CaptureCollected unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_CaptureCollected_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-test-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "CaptureCollected",
                    "captureId": "cap-test-capture-001",
                    "minQuality": 50
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("CaptureCollected unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that QuestComplete unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_QuestComplete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-test-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "QuestComplete",
                    "questId": "iron-path"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("QuestComplete unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that EnemyKilled unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_EnemyKilled_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "bestiary", "name": "Bestiary" }],
            "entries": [{
                "id": "codex-beast-001",
                "categoryId": "bestiary",
                "title": "Test Beast",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "EnemyKilled",
                    "enemyId": "hollow-stalker",
                    "count": 5,
                    "bossOnly": false
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("EnemyKilled unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that LocationVisited unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_LocationVisited_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "locations", "name": "Locations" }],
            "entries": [{
                "id": "codex-location-001",
                "categoryId": "locations",
                "title": "Test Location",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "LocationVisited",
                    "locationId": "shattered-spire"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("LocationVisited unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that ItemObtained unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_ItemObtained_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "items", "name": "Items" }],
            "entries": [{
                "id": "codex-item-001",
                "categoryId": "items",
                "title": "Test Item",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "ItemObtained",
                    "itemId": "echo-stone",
                    "quantity": 1
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("ItemObtained unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that FactionReputation unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_FactionReputation_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "characters", "name": "Characters" }],
            "entries": [{
                "id": "codex-faction-001",
                "categoryId": "characters",
                "title": "Test Faction",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "FactionReputation",
                    "factionId": "hollow-covenant",
                    "standing": "Friendly"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("FactionReputation unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that LevelReached unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_LevelReached_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "mechanics", "name": "Mechanics" }],
            "entries": [{
                "id": "codex-mech-001",
                "categoryId": "mechanics",
                "title": "Test Mechanic",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "LevelReached",
                    "minLevel": 10
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("LevelReached unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that ManualUnlock unlock condition passes validation.
    /// </summary>
    [Test]
    public void UnlockCondition_ManualUnlock_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Lore",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "ManualUnlock",
                    "triggerId": "story-event-finale",
                    "description": "Unlocked after completing the main story"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("ManualUnlock unlock condition should pass validation");
    }

    /// <summary>
    /// COD-004: Verifies that invalid unlock condition type fails validation.
    /// </summary>
    [Test]
    public void UnlockCondition_InvalidType_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Lore",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "unlockConditions": [{
                    "type": "InvalidType",
                    "someParam": "value"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid unlock condition type should fail validation");
    }

    #endregion

    #region COD-005: Progression and Section Validation

    /// <summary>
    /// COD-005: Verifies that valid progression levels pass validation.
    /// </summary>
    [Test]
    [TestCase("Fragment")]
    [TestCase("Partial")]
    [TestCase("Complete")]
    [TestCase("Mastery")]
    public void ProgressionLevel_ValidEnum_PassesValidation(string level)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "progressionDefaults": {
                "levels": [{
                    "level": "{{level}}",
                    "threshold": 0.50,
                    "description": "Test level"
                }]
            },
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": []
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"progression level '{level}' should pass validation");
    }

    /// <summary>
    /// COD-005: Verifies that section unlock threshold must be valid range.
    /// </summary>
    [Test]
    public void Section_ValidThreshold_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [
                    { "id": "section-intro", "title": "Intro", "content": "Always visible", "unlockThreshold": 0.0 },
                    { "id": "section-partial", "title": "Partial", "content": "At 50%", "unlockThreshold": 0.5 },
                    { "id": "section-mastery", "title": "Mastery", "content": "At 100%", "unlockThreshold": 1.0 }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid section thresholds should pass validation");
    }

    /// <summary>
    /// COD-005: Verifies that section threshold above 1.0 fails validation.
    /// </summary>
    [Test]
    public void Section_ThresholdAboveOne_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [
                    { "id": "section-invalid", "title": "Invalid", "content": "Threshold too high", "unlockThreshold": 1.5 }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("section threshold above 1.0 should fail validation");
    }

    /// <summary>
    /// COD-005: Verifies that section threshold below 0.0 fails validation.
    /// </summary>
    [Test]
    public void Section_ThresholdBelowZero_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [
                    { "id": "section-invalid", "title": "Invalid", "content": "Threshold too low", "unlockThreshold": -0.1 }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("section threshold below 0.0 should fail validation");
    }

    /// <summary>
    /// COD-005: Verifies that entry with empty sections array fails validation.
    /// </summary>
    [Test]
    public void Section_EmptyArray_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": []
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("entry with empty sections should fail validation (minItems: 1)");
    }

    #endregion

    #region COD-006: Capture Requirement and Relation Validation

    /// <summary>
    /// COD-006: Verifies that all capture requirement types pass validation.
    /// </summary>
    [Test]
    [TestCase("TextFragment")]
    [TestCase("EchoRecording")]
    [TestCase("VisualRecord")]
    [TestCase("Specimen")]
    [TestCase("OralHistory")]
    [TestCase("RunicTrace")]
    public void CaptureRequirement_ValidEnum_PassesValidation(string captureType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{
                    "id": "section-special",
                    "title": "Special Section",
                    "content": "Requires specific capture type",
                    "unlockThreshold": 0.5,
                    "captureRequirement": "{{captureType}}"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"capture requirement '{captureType}' should pass validation");
    }

    /// <summary>
    /// COD-006: Verifies that invalid capture requirement fails validation.
    /// </summary>
    [Test]
    public void CaptureRequirement_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{
                    "id": "section-special",
                    "title": "Special Section",
                    "content": "Invalid capture type",
                    "unlockThreshold": 0.5,
                    "captureRequirement": "InvalidType"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid capture requirement should fail validation");
    }

    /// <summary>
    /// COD-006: Verifies that all relation types pass validation.
    /// </summary>
    [Test]
    [TestCase("SeeAlso")]
    [TestCase("Prerequisite")]
    [TestCase("Sequel")]
    [TestCase("Contradiction")]
    [TestCase("Expansion")]
    public void EntryRelation_ValidRelationType_PassesValidation(string relationType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "relatedEntries": [{
                    "entryId": "codex-lore-002",
                    "relationType": "{{relationType}}",
                    "description": "Related entry"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"relation type '{relationType}' should pass validation");
    }

    /// <summary>
    /// COD-006: Verifies that invalid relation type fails validation.
    /// </summary>
    [Test]
    public void EntryRelation_InvalidRelationType_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }],
                "relatedEntries": [{
                    "entryId": "codex-lore-002",
                    "relationType": "InvalidRelation"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid relation type should fail validation");
    }

    /// <summary>
    /// COD-006: Verifies that era enum values pass validation.
    /// </summary>
    [Test]
    [TestCase("pre-blight")]
    [TestCase("blight")]
    [TestCase("post-blight")]
    [TestCase("unknown")]
    public void Entry_ValidEra_PassesValidation(string era)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "era": "{{era}}",
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"era '{era}' should pass validation");
    }

    /// <summary>
    /// COD-006: Verifies that spoiler level enum values pass validation.
    /// </summary>
    [Test]
    [TestCase("none")]
    [TestCase("minor")]
    [TestCase("major")]
    public void Section_ValidSpoilerLevel_PassesValidation(string spoilerLevel)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "sections": [{
                    "id": "section-spoiler",
                    "title": "Spoiler Section",
                    "content": "Contains spoilers",
                    "unlockThreshold": 1.0,
                    "spoilerLevel": "{{spoilerLevel}}"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"spoiler level '{spoilerLevel}' should pass validation");
    }

    #endregion

    #region Additional Validation Tests

    /// <summary>
    /// Verifies that progression rewards pass validation.
    /// </summary>
    [Test]
    [TestCase("Legend")]
    [TestCase("Achievement")]
    [TestCase("Unlock")]
    [TestCase("Item")]
    public void ProgressionReward_ValidType_PassesValidation(string rewardType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "progressionDefaults": {
                "levels": [{
                    "level": "Mastery",
                    "threshold": 1.0,
                    "rewards": [{ "type": "{{rewardType}}", "value": 10 }]
                }]
            },
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": []
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"progression reward type '{rewardType}' should pass validation");
    }

    /// <summary>
    /// Verifies that condition logic enum values pass validation.
    /// </summary>
    [Test]
    [TestCase("and")]
    [TestCase("or")]
    public void Entry_ValidConditionLogic_PassesValidation(string logic)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{ "id": "lore", "name": "Lore" }],
            "entries": [{
                "id": "codex-lore-001",
                "categoryId": "lore",
                "title": "Test Entry",
                "conditionLogic": "{{logic}}",
                "unlockConditions": [
                    { "type": "LevelReached", "minLevel": 5 },
                    { "type": "QuestComplete", "questId": "intro-quest" }
                ],
                "sections": [{ "id": "section-overview", "title": "Overview", "content": "Test content", "unlockThreshold": 0.0 }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"condition logic '{logic}' should pass validation");
    }

    #endregion
}
