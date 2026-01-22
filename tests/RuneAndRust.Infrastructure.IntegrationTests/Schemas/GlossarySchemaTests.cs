// ------------------------------------------------------------------------------
// <copyright file="GlossarySchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for glossary.schema.json validation.
// Verifies schema structure, term identity validation, category references,
// cross-reference relationships, display rules, and abbreviation handling.
// Part of v0.14.10e implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the glossary.schema.json JSON Schema.
/// Tests ensure the schema correctly validates glossary configuration files,
/// enforces term ID patterns, category references, cross-reference relationships,
/// display rules, and abbreviation uniqueness.
/// </summary>
/// <remarks>
/// <para>
/// The glossary schema provides 7 definitions:
/// <list type="bullet">
/// <item><description>TermCategory - Category for organizing terms</description></item>
/// <item><description>GlossaryTerm - Complete term with definition and metadata</description></item>
/// <item><description>CrossReference - Typed relationship to other terms</description></item>
/// <item><description>CrossReferenceType - 4 relationship types</description></item>
/// <item><description>DisplayContext - 5 display contexts</description></item>
/// <item><description>DisplayRule - Context-specific display configuration</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class GlossarySchemaTests
{
    /// <summary>
    /// Path to the glossary schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/glossary.schema.json";

    /// <summary>
    /// Path to the glossary.json configuration file for validation.
    /// </summary>
    private const string GlossaryConfigPath = "../../../../../config/glossary.json";

    /// <summary>
    /// Loaded JSON Schema for glossary definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the glossary schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region GLO-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// GLO-001: Verifies that glossary.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    [Test]
    public void GlossarySchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Glossary Configuration Schema");
        _schema.Type.Should().Be(JsonObjectType.Object);

        // Assert: All 7 definitions should be present
        _schema.Definitions.Should().ContainKey("TermCategory");
        _schema.Definitions.Should().ContainKey("GlossaryTerm");
        _schema.Definitions.Should().ContainKey("CrossReference");
        _schema.Definitions.Should().ContainKey("CrossReferenceType");
        _schema.Definitions.Should().ContainKey("DisplayContext");
        _schema.Definitions.Should().ContainKey("DisplayRule");
    }

    /// <summary>
    /// GLO-001: Verifies that the actual glossary.json configuration passes validation.
    /// </summary>
    [Test]
    public async Task GlossarySchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual glossary.json file
        var jsonContent = await File.ReadAllTextAsync(GlossaryConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty("existing glossary.json should validate against schema");
    }

    /// <summary>
    /// GLO-001: Verifies that version is required.
    /// </summary>
    [Test]
    public void GlossarySchema_MissingVersion_FailsValidation()
    {
        var invalidJson = """
        {
            "terms": [{
                "id": "test",
                "term": "Test",
                "definition": "A test term",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing version should fail validation");
    }

    /// <summary>
    /// GLO-001: Verifies that terms array is required.
    /// </summary>
    [Test]
    public void GlossarySchema_MissingTerms_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0"
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing terms should fail validation");
    }

    #endregion

    #region GLO-002: Term Identity Validation

    /// <summary>
    /// GLO-002: Verifies that valid term IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("attack")]
    [TestCase("critical-hit")]
    [TestCase("damage-over-time")]
    [TestCase("runic-blight")]
    public void TermIdentity_ValidId_PassesValidation(string id)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "terms": [{
                "id": "{{id}}",
                "term": "Test Term",
                "definition": "A test definition",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"term with valid ID '{id}' should pass validation");
    }

    /// <summary>
    /// GLO-002: Verifies that invalid term ID format fails validation.
    /// </summary>
    [Test]
    public void TermIdentity_InvalidIdFormat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "Invalid_ID",
                "term": "Test",
                "definition": "A test definition",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("term with invalid ID format (underscore) should fail validation");
    }

    /// <summary>
    /// GLO-002: Verifies that term ID starting with number fails validation.
    /// </summary>
    [Test]
    public void TermIdentity_IdStartsWithNumber_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "123term",
                "term": "Test",
                "definition": "A test definition",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("term ID starting with number should fail validation");
    }

    /// <summary>
    /// GLO-002: Verifies that empty term name fails validation.
    /// </summary>
    [Test]
    public void TermIdentity_EmptyTermName_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "test",
                "term": "",
                "definition": "A test definition",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty term name should fail validation");
    }

    /// <summary>
    /// GLO-002: Verifies that empty definition fails validation.
    /// </summary>
    [Test]
    public void TermIdentity_EmptyDefinition_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "test",
                "term": "Test",
                "definition": "",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty definition should fail validation");
    }

    /// <summary>
    /// GLO-002: Verifies that missing categoryId fails validation.
    /// </summary>
    [Test]
    public void TermIdentity_MissingCategoryId_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "test",
                "term": "Test",
                "definition": "A test definition"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing categoryId should fail validation");
    }

    #endregion

    #region GLO-003: Category Validation

    /// <summary>
    /// GLO-003: Verifies that valid category definition passes validation.
    /// </summary>
    [Test]
    public void Category_ValidDefinition_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{
                "id": "combat",
                "name": "Combat",
                "description": "Battle mechanics and terms",
                "iconId": "icon-sword",
                "sortOrder": 1,
                "color": "#CC3333"
            }],
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Attack action",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete category definition should pass validation");
    }

    /// <summary>
    /// GLO-003: Verifies that minimal category (id and name only) passes validation.
    /// </summary>
    [Test]
    public void Category_MinimalDefinition_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{
                "id": "combat",
                "name": "Combat"
            }],
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Attack action",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("minimal category (id, name) should pass validation");
    }

    /// <summary>
    /// GLO-003: Verifies that invalid category ID format fails validation.
    /// </summary>
    [Test]
    public void Category_InvalidIdFormat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{
                "id": "Invalid_Category",
                "name": "Combat"
            }],
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Attack action",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("category with invalid ID format should fail validation");
    }

    /// <summary>
    /// GLO-003: Verifies that valid hex color passes validation.
    /// </summary>
    [Test]
    [TestCase("#CC3333")]
    [TestCase("#ffffff")]
    [TestCase("#000000")]
    [TestCase("#AbCdEf")]
    public void Category_ValidColor_PassesValidation(string color)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "categories": [{
                "id": "combat",
                "name": "Combat",
                "color": "{{color}}"
            }],
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Attack action",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"category with valid color '{color}' should pass validation");
    }

    /// <summary>
    /// GLO-003: Verifies that invalid hex color format fails validation.
    /// </summary>
    [Test]
    public void Category_InvalidColorFormat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "categories": [{
                "id": "combat",
                "name": "Combat",
                "color": "red"
            }],
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Attack action",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("category with invalid color format should fail validation");
    }

    #endregion

    #region GLO-004: Cross-Reference Validation

    /// <summary>
    /// GLO-004: Verifies that all 4 relationship types pass validation.
    /// </summary>
    [Test]
    [TestCase("SeeAlso")]
    [TestCase("Contrast")]
    [TestCase("Prerequisite")]
    [TestCase("Related")]
    public void CrossReference_ValidRelationship_PassesValidation(string relationship)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "crossReferences": [{
                    "termId": "attack",
                    "relationship": "{{relationship}}"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"cross-reference with relationship '{relationship}' should pass validation");
    }

    /// <summary>
    /// GLO-004: Verifies that invalid relationship type fails validation.
    /// </summary>
    [Test]
    public void CrossReference_InvalidRelationship_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "crossReferences": [{
                    "termId": "attack",
                    "relationship": "InvalidRelationship"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid relationship type should fail validation");
    }

    /// <summary>
    /// GLO-004: Verifies that cross-reference with note passes validation.
    /// </summary>
    [Test]
    public void CrossReference_WithNote_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "crossReferences": [{
                    "termId": "attack",
                    "relationship": "Prerequisite",
                    "note": "Understanding attack rolls helps with critical hits"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("cross-reference with note should pass validation");
    }

    /// <summary>
    /// GLO-004: Verifies that cross-reference missing termId fails validation.
    /// </summary>
    [Test]
    public void CrossReference_MissingTermId_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "crossReferences": [{
                    "relationship": "Related"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("cross-reference without termId should fail validation");
    }

    /// <summary>
    /// GLO-004: Verifies that multiple cross-references pass validation.
    /// </summary>
    [Test]
    public void CrossReference_Multiple_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "crossReferences": [
                    { "termId": "attack", "relationship": "Prerequisite" },
                    { "termId": "damage", "relationship": "Related" },
                    { "termId": "glancing-blow", "relationship": "Contrast" }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("multiple cross-references should pass validation");
    }

    #endregion

    #region GLO-005: Display Rule Validation

    /// <summary>
    /// GLO-005: Verifies that all 5 display contexts pass validation.
    /// </summary>
    [Test]
    [TestCase("Tooltip")]
    [TestCase("Help")]
    [TestCase("Codex")]
    [TestCase("Combat")]
    [TestCase("Inventory")]
    public void DisplayRule_ValidContext_PassesValidation(string context)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "terms": [{
                "id": "stress",
                "term": "Stress",
                "definition": "Mental strain",
                "categoryId": "mechanics",
                "displayRules": [{
                    "context": "{{context}}",
                    "showDefinition": true
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"display rule with context '{context}' should pass validation");
    }

    /// <summary>
    /// GLO-005: Verifies that invalid display context fails validation.
    /// </summary>
    [Test]
    public void DisplayRule_InvalidContext_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "stress",
                "term": "Stress",
                "definition": "Mental strain",
                "categoryId": "mechanics",
                "displayRules": [{
                    "context": "InvalidContext"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid display context should fail validation");
    }

    /// <summary>
    /// GLO-005: Verifies that complete display rule passes validation.
    /// </summary>
    [Test]
    public void DisplayRule_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "stress",
                "term": "Stress",
                "definition": "Mental strain",
                "categoryId": "mechanics",
                "displayRules": [{
                    "context": "Combat",
                    "showDefinition": true,
                    "showExamples": false,
                    "maxLength": 80,
                    "highlightInText": true,
                    "useAbbreviation": false,
                    "showCrossReferences": false
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete display rule should pass validation");
    }

    /// <summary>
    /// GLO-005: Verifies that default display rules pass validation.
    /// </summary>
    [Test]
    public void DefaultDisplayRules_Valid_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultDisplayRules": [
                {
                    "context": "Tooltip",
                    "showDefinition": true,
                    "maxLength": 120
                },
                {
                    "context": "Help",
                    "showDefinition": true,
                    "showExamples": true,
                    "showCrossReferences": true
                }
            ],
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Offensive action",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("default display rules should pass validation");
    }

    /// <summary>
    /// GLO-005: Verifies that maxLength below minimum fails validation.
    /// </summary>
    [Test]
    public void DisplayRule_MaxLengthTooSmall_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "stress",
                "term": "Stress",
                "definition": "Mental strain",
                "categoryId": "mechanics",
                "displayRules": [{
                    "context": "Combat",
                    "maxLength": 5
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("maxLength below minimum (10) should fail validation");
    }

    #endregion

    #region GLO-006: Abbreviation and Alias Validation

    /// <summary>
    /// GLO-006: Verifies that abbreviation passes validation.
    /// </summary>
    [Test]
    public void Term_WithAbbreviation_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Offensive action",
                "categoryId": "combat",
                "abbreviation": "ATK"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("term with abbreviation should pass validation");
    }

    /// <summary>
    /// GLO-006: Verifies that abbreviation over max length fails validation.
    /// </summary>
    [Test]
    public void Term_AbbreviationTooLong_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "attack",
                "term": "Attack",
                "definition": "Offensive action",
                "categoryId": "combat",
                "abbreviation": "VERYLONGABBREV"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("abbreviation over 10 characters should fail validation");
    }

    /// <summary>
    /// GLO-006: Verifies that term with aliases passes validation.
    /// </summary>
    [Test]
    public void Term_WithAliases_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "aliases": ["crit", "critical", "crit hit"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("term with aliases should pass validation");
    }

    /// <summary>
    /// GLO-006: Verifies that short definition passes validation.
    /// </summary>
    [Test]
    public void Term_WithShortDefinition_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A critical hit occurs when you roll maximum damage, dealing double damage and ignoring armor. Triggered by max roll or specific abilities.",
                "shortDefinition": "Maximum damage roll dealing double damage.",
                "categoryId": "combat"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("term with short definition should pass validation");
    }

    #endregion

    #region Additional Validation Tests

    /// <summary>
    /// Verifies that term with examples passes validation.
    /// </summary>
    [Test]
    public void Term_WithExamples_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "examples": [
                    "Your strike lands a Critical Hit! 24 damage.",
                    "The assassin scores a critical, bypassing your shield."
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("term with examples should pass validation");
    }

    /// <summary>
    /// Verifies that term with tags passes validation.
    /// </summary>
    [Test]
    public void Term_WithTags_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "tags": ["damage", "luck", "combat-basics"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("term with tags should pass validation");
    }

    /// <summary>
    /// Verifies that isGameMechanics flag passes validation.
    /// </summary>
    [Test]
    public void Term_IsGameMechanics_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "galdr",
                "term": "Galdr",
                "definition": "Norse runic magic",
                "categoryId": "magic",
                "isGameMechanics": false
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("isGameMechanics flag should pass validation");
    }

    /// <summary>
    /// Verifies that localizationKey with valid pattern passes validation.
    /// </summary>
    [Test]
    public void Term_ValidLocalizationKey_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "localizationKey": "glossary.combat.critical_hit"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("valid localizationKey should pass validation");
    }

    /// <summary>
    /// Verifies that term with firstAppearance passes validation.
    /// </summary>
    [Test]
    public void Term_WithFirstAppearance_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A devastating blow",
                "categoryId": "combat",
                "firstAppearance": "Tutorial"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("term with firstAppearance should pass validation");
    }

    /// <summary>
    /// Verifies that empty terms array fails validation.
    /// </summary>
    [Test]
    public void Terms_EmptyArray_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "terms": []
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty terms array should fail validation (minItems: 1)");
    }

    /// <summary>
    /// Verifies that complete term with all optional fields passes validation.
    /// </summary>
    [Test]
    public void Term_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "terms": [{
                "id": "critical-hit",
                "term": "Critical Hit",
                "definition": "A critical hit occurs when you roll the maximum value on your attack die.",
                "shortDefinition": "Maximum damage roll dealing double damage.",
                "categoryId": "combat",
                "abbreviation": "Crit",
                "aliases": ["crit", "critical"],
                "crossReferences": [
                    { "termId": "attack", "relationship": "Prerequisite" },
                    { "termId": "damage", "relationship": "Related" }
                ],
                "displayRules": [
                    { "context": "Combat", "maxLength": 60 }
                ],
                "examples": [
                    "Your strike lands a Critical Hit!"
                ],
                "localizationKey": "glossary.combat.critical_hit",
                "sortOrder": 5,
                "tags": ["damage", "luck"],
                "isGameMechanics": true,
                "firstAppearance": "Tutorial"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete term with all fields should pass validation");
    }

    #endregion
}
