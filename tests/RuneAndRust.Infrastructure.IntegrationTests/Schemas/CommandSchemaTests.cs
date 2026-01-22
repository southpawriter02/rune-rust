// ------------------------------------------------------------------------------
// <copyright file="CommandSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for commands.schema.json validation.
// Verifies schema structure, command identity validation, argument types,
// context restrictions, alias uniqueness, and help structure parsing.
// Part of v0.14.10d implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the commands.schema.json JSON Schema.
/// Tests ensure the schema correctly validates command configuration files,
/// enforces command ID patterns, argument types, context restrictions,
/// alias uniqueness, and help documentation structure.
/// </summary>
/// <remarks>
/// <para>
/// The command schema provides 8 definitions:
/// <list type="bullet">
/// <item><description>CommandCategory - 8 command categories</description></item>
/// <item><description>CommandCategoryDefinition - Category metadata</description></item>
/// <item><description>ArgumentType - 10 argument types</description></item>
/// <item><description>CommandContext - 9 context restrictions</description></item>
/// <item><description>Command - Complete command definition</description></item>
/// <item><description>CommandArgument - Typed argument with validation</description></item>
/// <item><description>CommandHelp - Help documentation structure</description></item>
/// <item><description>CommandExample - Example input/description pair</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class CommandSchemaTests
{
    /// <summary>
    /// Path to the command schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/commands.schema.json";

    /// <summary>
    /// Path to the commands.json configuration file for validation.
    /// </summary>
    private const string CommandsConfigPath = "../../../../../config/commands.json";

    /// <summary>
    /// Loaded JSON Schema for command definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the command schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region CMD-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// CMD-001: Verifies that commands.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    [Test]
    public void CommandSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Command Configuration Schema");
        _schema.Type.Should().Be(JsonObjectType.Object);

        // Assert: All 8 definitions should be present
        _schema.Definitions.Should().ContainKey("CommandCategory");
        _schema.Definitions.Should().ContainKey("CommandCategoryDefinition");
        _schema.Definitions.Should().ContainKey("ArgumentType");
        _schema.Definitions.Should().ContainKey("CommandContext");
        _schema.Definitions.Should().ContainKey("Command");
        _schema.Definitions.Should().ContainKey("CommandArgument");
        _schema.Definitions.Should().ContainKey("CommandHelp");
        _schema.Definitions.Should().ContainKey("CommandExample");
    }

    /// <summary>
    /// CMD-001: Verifies that the actual commands.json configuration passes validation.
    /// </summary>
    [Test]
    public async Task CommandSchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual commands.json file
        var jsonContent = await File.ReadAllTextAsync(CommandsConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty("existing commands.json should validate against schema");
    }

    /// <summary>
    /// CMD-001: Verifies that version is required.
    /// </summary>
    [Test]
    public void CommandSchema_MissingVersion_FailsValidation()
    {
        var invalidJson = """
        {
            "commands": [{
                "id": "look",
                "name": "look",
                "category": "Interaction",
                "description": "Look around"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing version should fail validation");
    }

    #endregion

    #region CMD-002: Command Identity Validation

    /// <summary>
    /// CMD-002: Verifies that valid command IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("look")]
    [TestCase("attack")]
    [TestCase("go-north")]
    [TestCase("cast-spell")]
    public void CommandIdentity_ValidId_PassesValidation(string id)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "commands": [{
                "id": "{{id}}",
                "name": "test",
                "category": "Interaction",
                "description": "Test command"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"command with valid ID '{id}' should pass validation");
    }

    /// <summary>
    /// CMD-002: Verifies that invalid command ID format fails validation.
    /// </summary>
    [Test]
    public void CommandIdentity_InvalidIdFormat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "Invalid_ID",
                "name": "test",
                "category": "Interaction",
                "description": "Test command"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("command with invalid ID format (underscore) should fail validation");
    }

    /// <summary>
    /// CMD-002: Verifies that command ID starting with number fails validation.
    /// </summary>
    [Test]
    public void CommandIdentity_IdStartsWithNumber_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "123command",
                "name": "test",
                "category": "Interaction",
                "description": "Test command"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("command ID starting with number should fail validation");
    }

    /// <summary>
    /// CMD-002: Verifies that command name with spaces fails validation.
    /// </summary>
    [Test]
    public void CommandIdentity_NameWithSpaces_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test command",
                "category": "Interaction",
                "description": "Test command"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("command name with spaces should fail validation");
    }

    /// <summary>
    /// CMD-002: Verifies that empty description fails validation.
    /// </summary>
    [Test]
    public void CommandIdentity_EmptyDescription_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": ""
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("command with empty description should fail validation");
    }

    #endregion

    #region CMD-003: Argument Type Validation

    /// <summary>
    /// CMD-003: Verifies that all 10 argument types pass validation.
    /// </summary>
    [Test]
    [TestCase("String")]
    [TestCase("Number")]
    [TestCase("Direction")]
    [TestCase("Target")]
    [TestCase("Item")]
    [TestCase("Ability")]
    [TestCase("NPC")]
    [TestCase("Location")]
    [TestCase("Quantity")]
    [TestCase("Boolean")]
    public void ArgumentType_ValidEnum_PassesValidation(string argType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "arguments": [{
                    "name": "testArg",
                    "type": "{{argType}}",
                    "description": "Test argument"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"argument type '{argType}' should pass validation");
    }

    /// <summary>
    /// CMD-003: Verifies that invalid argument type fails validation.
    /// </summary>
    [Test]
    public void ArgumentType_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "arguments": [{
                    "name": "testArg",
                    "type": "InvalidType"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid argument type should fail validation");
    }

    /// <summary>
    /// CMD-003: Verifies that argument name follows camelCase pattern.
    /// </summary>
    [Test]
    public void ArgumentName_ValidCamelCase_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "arguments": [{
                    "name": "targetName",
                    "type": "String"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("argument name in camelCase should pass validation");
    }

    /// <summary>
    /// CMD-003: Verifies that argument with validValues passes validation.
    /// </summary>
    [Test]
    public void ArgumentValidValues_WithValues_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "arguments": [{
                    "name": "direction",
                    "type": "String",
                    "validValues": ["north", "south", "east", "west"]
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("argument with validValues should pass validation");
    }

    /// <summary>
    /// CMD-003: Verifies that variadic argument passes validation.
    /// </summary>
    [Test]
    public void ArgumentVariadic_FlagSet_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "say",
                "name": "say",
                "category": "Social",
                "description": "Say something",
                "arguments": [{
                    "name": "message",
                    "type": "String",
                    "isVariadic": true
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("variadic argument should pass validation");
    }

    #endregion

    #region CMD-004: Context Restriction Validation

    /// <summary>
    /// CMD-004: Verifies that all 9 context restrictions pass validation.
    /// </summary>
    [Test]
    [TestCase("Exploration")]
    [TestCase("Combat")]
    [TestCase("Dialogue")]
    [TestCase("Inventory")]
    [TestCase("Menu")]
    [TestCase("Rest")]
    [TestCase("Crafting")]
    [TestCase("Trading")]
    [TestCase("Any")]
    public void ContextRestriction_ValidEnum_PassesValidation(string context)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "contexts": ["{{context}}"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"context '{context}' should pass validation");
    }

    /// <summary>
    /// CMD-004: Verifies that invalid context fails validation.
    /// </summary>
    [Test]
    public void ContextRestriction_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "contexts": ["InvalidContext"]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid context should fail validation");
    }

    /// <summary>
    /// CMD-004: Verifies that multiple contexts pass validation.
    /// </summary>
    [Test]
    public void ContextRestriction_MultipleContexts_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "look",
                "name": "look",
                "category": "Interaction",
                "description": "Look around",
                "contexts": ["Exploration", "Combat", "Inventory"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("multiple contexts should pass validation");
    }

    /// <summary>
    /// CMD-004: Verifies that empty contexts array fails validation.
    /// </summary>
    [Test]
    public void ContextRestriction_EmptyArray_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "contexts": []
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty contexts array should fail validation (minItems: 1)");
    }

    #endregion

    #region CMD-005: Category and Alias Validation

    /// <summary>
    /// CMD-005: Verifies that all 8 command categories pass validation.
    /// </summary>
    [Test]
    [TestCase("Movement")]
    [TestCase("Combat")]
    [TestCase("Interaction")]
    [TestCase("Inventory")]
    [TestCase("Information")]
    [TestCase("Social")]
    [TestCase("System")]
    [TestCase("Debug")]
    public void CommandCategory_ValidEnum_PassesValidation(string category)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "{{category}}",
                "description": "Test command"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"command category '{category}' should pass validation");
    }

    /// <summary>
    /// CMD-005: Verifies that invalid category fails validation.
    /// </summary>
    [Test]
    public void CommandCategory_InvalidEnum_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "InvalidCategory",
                "description": "Test command"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid category should fail validation");
    }

    /// <summary>
    /// CMD-005: Verifies that command aliases pass validation.
    /// </summary>
    [Test]
    public void CommandAliases_ValidArray_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "look",
                "name": "look",
                "aliases": ["examine", "inspect", "view"],
                "category": "Interaction",
                "description": "Look at something"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("command with aliases should pass validation");
    }

    /// <summary>
    /// CMD-005: Verifies that global aliases pass validation.
    /// </summary>
    [Test]
    public void GlobalAliases_ValidObject_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "globalAliases": {
                "l": "look",
                "n": "north",
                "i": "inventory"
            },
            "commands": [{
                "id": "look",
                "name": "look",
                "category": "Interaction",
                "description": "Look around"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("global aliases should pass validation");
    }

    /// <summary>
    /// CMD-005: Verifies that category definition passes validation.
    /// </summary>
    [Test]
    public void CategoryDefinition_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "categories": [{
                "id": "Movement",
                "name": "Movement Commands",
                "description": "Commands for navigating the world",
                "sortOrder": 1,
                "icon": "icon-movement"
            }],
            "commands": [{
                "id": "north",
                "name": "north",
                "category": "Movement",
                "description": "Move north"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete category definition should pass validation");
    }

    #endregion

    #region CMD-006: Help Structure Validation

    /// <summary>
    /// CMD-006: Verifies that complete help structure passes validation.
    /// </summary>
    [Test]
    public void HelpStructure_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "attack",
                "name": "attack",
                "category": "Combat",
                "description": "Attack a target",
                "help": {
                    "usage": "attack <target> [with <weapon>]",
                    "examples": [
                        { "input": "attack goblin", "description": "Attack the goblin" },
                        { "input": "attack orc with sword", "description": "Attack orc with sword" }
                    ],
                    "notes": "If only one enemy is present, target can be omitted.",
                    "seeAlso": ["defend", "cast", "flee"]
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete help structure should pass validation");
    }

    /// <summary>
    /// CMD-006: Verifies that minimal help with only usage passes validation.
    /// </summary>
    [Test]
    public void HelpStructure_MinimalWithUsage_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "north",
                "name": "north",
                "category": "Movement",
                "description": "Move north",
                "help": {
                    "usage": "north"
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("help with only usage should pass validation");
    }

    /// <summary>
    /// CMD-006: Verifies that help without usage fails validation.
    /// </summary>
    [Test]
    public void HelpStructure_MissingUsage_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "help": {
                    "notes": "Some notes without usage"
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("help without usage should fail validation (usage is required)");
    }

    /// <summary>
    /// CMD-006: Verifies that seeAlso with valid command IDs passes validation.
    /// </summary>
    [Test]
    public void HelpSeeAlso_ValidCommandIds_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "attack",
                "name": "attack",
                "category": "Combat",
                "description": "Attack a target",
                "help": {
                    "usage": "attack <target>",
                    "seeAlso": ["defend", "flee", "cast-spell"]
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("seeAlso with valid command IDs should pass validation");
    }

    /// <summary>
    /// CMD-006: Verifies that example requires input and description.
    /// </summary>
    [Test]
    public void HelpExample_MissingDescription_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "test",
                "name": "test",
                "category": "Interaction",
                "description": "Test command",
                "help": {
                    "usage": "test",
                    "examples": [
                        { "input": "test something" }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("example without description should fail validation");
    }

    #endregion

    #region Additional Validation Tests

    /// <summary>
    /// Verifies that ignoredWords array passes validation.
    /// </summary>
    [Test]
    public void IgnoredWords_ValidArray_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "ignoredWords": ["the", "a", "an", "to", "at", "with"],
            "commands": [{
                "id": "look",
                "name": "look",
                "category": "Interaction",
                "description": "Look around"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("ignoredWords array should pass validation");
    }

    /// <summary>
    /// Verifies that isHidden flag passes validation.
    /// </summary>
    [Test]
    public void CommandIsHidden_FlagSet_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "teleport",
                "name": "teleport",
                "category": "Debug",
                "description": "Debug teleport command",
                "isHidden": true
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("isHidden flag should pass validation");
    }

    /// <summary>
    /// Verifies that requiresTarget flag passes validation.
    /// </summary>
    [Test]
    public void CommandRequiresTarget_FlagSet_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "attack",
                "name": "attack",
                "category": "Combat",
                "description": "Attack a target",
                "requiresTarget": true
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("requiresTarget flag should pass validation");
    }

    /// <summary>
    /// Verifies that canChain flag passes validation.
    /// </summary>
    [Test]
    public void CommandCanChain_FlagSet_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "look",
                "name": "look",
                "category": "Interaction",
                "description": "Look around",
                "canChain": true
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("canChain flag should pass validation");
    }

    /// <summary>
    /// Verifies that cooldown value passes validation.
    /// </summary>
    [Test]
    public void CommandCooldown_ValidValue_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "special",
                "name": "special",
                "category": "Combat",
                "description": "Special ability",
                "cooldown": 3
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("cooldown value should pass validation");
    }

    /// <summary>
    /// Verifies that negative cooldown fails validation.
    /// </summary>
    [Test]
    public void CommandCooldown_NegativeValue_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "special",
                "name": "special",
                "category": "Combat",
                "description": "Special ability",
                "cooldown": -1
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("negative cooldown should fail validation");
    }

    /// <summary>
    /// Verifies that tags array passes validation.
    /// </summary>
    [Test]
    public void CommandTags_ValidArray_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "commands": [{
                "id": "attack",
                "name": "attack",
                "category": "Combat",
                "description": "Attack a target",
                "tags": ["combat", "offensive", "basic"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("tags array should pass validation");
    }

    /// <summary>
    /// Verifies that empty commands array fails validation.
    /// </summary>
    [Test]
    public void Commands_EmptyArray_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "commands": []
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty commands array should fail validation (minItems: 1)");
    }

    #endregion
}
