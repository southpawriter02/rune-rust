// ------------------------------------------------------------------------------
// <copyright file="StanceSchemaTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for stances.schema.json validation.
// Verifies schema structure, stance identity validation, stat modifiers,
// switching rules, AI behavior patterns, and unlock conditions.
// Part of v0.14.10f implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the stances.schema.json JSON Schema.
/// Tests ensure the schema correctly validates combat stance configuration files,
/// enforces stance ID patterns, stat modifiers, switching rules, AI behavior,
/// and unlock conditions.
/// </summary>
/// <remarks>
/// <para>
/// The stances schema provides 13 definitions:
/// <list type="bullet">
/// <item><description>CombatStance - Core stance configuration</description></item>
/// <item><description>StatModifier - Stat bonus/penalty with type</description></item>
/// <item><description>AbilityRestriction - Ability limitation rules</description></item>
/// <item><description>SwitchingRule - Stance transition configuration</description></item>
/// <item><description>SwitchCondition - Switch requirement conditions</description></item>
/// <item><description>TriggerEffect - Effects on stance switch</description></item>
/// <item><description>AIBehavior - AI decision-making patterns</description></item>
/// <item><description>AICondition - AI evaluation conditions</description></item>
/// <item><description>UnlockCondition - Stance unlock requirements</description></item>
/// <item><description>ModifiableStat enum (8 stats)</description></item>
/// <item><description>ActionCost enum (4 types)</description></item>
/// <item><description>AIConditionType enum (7 types)</description></item>
/// <item><description>UnlockConditionType enum (5 types)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class StanceSchemaTests
{
    /// <summary>
    /// Path to the stances schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/stances.schema.json";

    /// <summary>
    /// Path to the stances.json configuration file for validation.
    /// </summary>
    private const string StancesConfigPath = "../../../../../config/stances.json";

    /// <summary>
    /// Loaded JSON Schema for stance definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the stances schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        var fullSchemaPath = Path.GetFullPath(SchemaPath);
        _schema = await JsonSchema.FromFileAsync(fullSchemaPath);
    }

    #region STN-001: Schema File Loads and Contains Expected Definitions

    /// <summary>
    /// STN-001: Verifies that stances.schema.json loads and parses as valid JSON Schema.
    /// </summary>
    [Test]
    public void StanceSchema_LoadsAndParses_Successfully()
    {
        // Assert: Schema should parse successfully
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Combat Stance Configuration Schema");
        _schema.Type.Should().Be(JsonObjectType.Object);

        // Assert: All core definitions should be present
        _schema.Definitions.Should().ContainKey("CombatStance");
        _schema.Definitions.Should().ContainKey("StatModifier");
        _schema.Definitions.Should().ContainKey("AbilityRestriction");
        _schema.Definitions.Should().ContainKey("SwitchingRule");
        _schema.Definitions.Should().ContainKey("SwitchCondition");
        _schema.Definitions.Should().ContainKey("TriggerEffect");
        _schema.Definitions.Should().ContainKey("AIBehavior");
        _schema.Definitions.Should().ContainKey("AICondition");
        _schema.Definitions.Should().ContainKey("UnlockCondition");
    }

    /// <summary>
    /// STN-001: Verifies that the actual stances.json configuration passes validation.
    /// </summary>
    [Test]
    public async Task StanceSchema_ExistingConfiguration_PassesValidation()
    {
        // Arrange: Load the actual stances.json file
        var jsonContent = await File.ReadAllTextAsync(StancesConfigPath);

        // Act: Validate the JSON content against the schema
        var errors = _schema.Validate(jsonContent);

        // Assert: Schema should validate successfully
        errors.Should().BeEmpty("existing stances.json should validate against schema");
    }

    /// <summary>
    /// STN-001: Verifies that version is required.
    /// </summary>
    [Test]
    public void StanceSchema_MissingVersion_FailsValidation()
    {
        var invalidJson = """
        {
            "defaultStance": "balanced",
            "stances": [{
                "id": "balanced",
                "name": "Balanced",
                "description": "A neutral stance"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing version should fail validation");
    }

    /// <summary>
    /// STN-001: Verifies that defaultStance is required.
    /// </summary>
    [Test]
    public void StanceSchema_MissingDefaultStance_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "stances": [{
                "id": "balanced",
                "name": "Balanced",
                "description": "A neutral stance"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing defaultStance should fail validation");
    }

    /// <summary>
    /// STN-001: Verifies that stances array is required.
    /// </summary>
    [Test]
    public void StanceSchema_MissingStances_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "balanced"
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("missing stances should fail validation");
    }

    /// <summary>
    /// STN-001: Verifies that empty stances array fails validation.
    /// </summary>
    [Test]
    public void StanceSchema_EmptyStancesArray_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "balanced",
            "stances": []
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty stances array should fail validation (minItems: 1)");
    }

    #endregion

    #region STN-002: Stance Identity Validation

    /// <summary>
    /// STN-002: Verifies that valid stance IDs pass validation.
    /// </summary>
    [Test]
    [TestCase("balanced")]
    [TestCase("aggressive")]
    [TestCase("defensive")]
    [TestCase("ultra-defensive")]
    public void StanceIdentity_ValidId_PassesValidation(string id)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "{{id}}",
            "stances": [{
                "id": "{{id}}",
                "name": "Test Stance",
                "description": "A test stance for validation"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"stance with valid ID '{id}' should pass validation");
    }

    /// <summary>
    /// STN-002: Verifies that invalid stance ID format fails validation.
    /// </summary>
    [Test]
    public void StanceIdentity_InvalidIdFormat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "Invalid_ID",
            "stances": [{
                "id": "Invalid_ID",
                "name": "Test",
                "description": "A test stance"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("stance with invalid ID format (underscore) should fail validation");
    }

    /// <summary>
    /// STN-002: Verifies that stance ID starting with number fails validation.
    /// </summary>
    [Test]
    public void StanceIdentity_IdStartsWithNumber_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "123stance",
            "stances": [{
                "id": "123stance",
                "name": "Test",
                "description": "A test stance"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("stance ID starting with number should fail validation");
    }

    /// <summary>
    /// STN-002: Verifies that empty stance name fails validation.
    /// </summary>
    [Test]
    public void StanceIdentity_EmptyName_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "",
                "description": "A test stance"
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty stance name should fail validation");
    }

    /// <summary>
    /// STN-002: Verifies that empty description fails validation.
    /// </summary>
    [Test]
    public void StanceIdentity_EmptyDescription_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": ""
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("empty description should fail validation");
    }

    #endregion

    #region STN-003: Stat Modifier Validation

    /// <summary>
    /// STN-003: Verifies that all 8 modifiable stats pass validation.
    /// </summary>
    [Test]
    [TestCase("attack")]
    [TestCase("defense")]
    [TestCase("speed")]
    [TestCase("accuracy")]
    [TestCase("evasion")]
    [TestCase("criticalChance")]
    [TestCase("criticalDamage")]
    [TestCase("damageReduction")]
    public void StatModifier_ValidStat_PassesValidation(string stat)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "statModifiers": [{
                    "stat": "{{stat}}",
                    "value": 10,
                    "type": "percentage"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"stat modifier with stat '{stat}' should pass validation");
    }

    /// <summary>
    /// STN-003: Verifies that invalid stat name fails validation.
    /// </summary>
    [Test]
    public void StatModifier_InvalidStat_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "statModifiers": [{
                    "stat": "invalidStat",
                    "value": 10,
                    "type": "percentage"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid stat name should fail validation");
    }

    /// <summary>
    /// STN-003: Verifies that both modifier types pass validation.
    /// </summary>
    [Test]
    [TestCase("flat")]
    [TestCase("percentage")]
    public void StatModifier_ValidType_PassesValidation(string type)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "statModifiers": [{
                    "stat": "attack",
                    "value": 10,
                    "type": "{{type}}"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"stat modifier with type '{type}' should pass validation");
    }

    /// <summary>
    /// STN-003: Verifies that negative modifier values pass validation.
    /// </summary>
    [Test]
    public void StatModifier_NegativeValue_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "statModifiers": [{
                    "stat": "defense",
                    "value": -20,
                    "type": "percentage",
                    "description": "-20% defense penalty"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("negative stat modifier value should pass validation");
    }

    /// <summary>
    /// STN-003: Verifies that multiple stat modifiers pass validation.
    /// </summary>
    [Test]
    public void StatModifier_Multiple_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "statModifiers": [
                    { "stat": "attack", "value": 20, "type": "percentage" },
                    { "stat": "defense", "value": -20, "type": "percentage" },
                    { "stat": "damageReduction", "value": 5, "type": "flat" }
                ]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("multiple stat modifiers should pass validation");
    }

    #endregion

    #region STN-004: Switching Rule Validation

    /// <summary>
    /// STN-004: Verifies that all 4 action costs pass validation.
    /// </summary>
    [Test]
    [TestCase("Free")]
    [TestCase("Swift")]
    [TestCase("Standard")]
    [TestCase("Full")]
    public void SwitchingRule_ValidActionCost_PassesValidation(string actionCost)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "switchingRule": {
                    "actionCost": "{{actionCost}}"
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"switching rule with action cost '{actionCost}' should pass validation");
    }

    /// <summary>
    /// STN-004: Verifies that invalid action cost fails validation.
    /// </summary>
    [Test]
    public void SwitchingRule_InvalidActionCost_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "switchingRule": {
                    "actionCost": "InvalidCost"
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid action cost should fail validation");
    }

    /// <summary>
    /// STN-004: Verifies that complete switching rule passes validation.
    /// </summary>
    [Test]
    public void SwitchingRule_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "switchingRule": {
                    "actionCost": "Swift",
                    "cooldown": 2,
                    "canSwitchDuringEnemyTurn": true,
                    "conditions": [
                        { "type": "HealthAbove", "value": 50 }
                    ],
                    "triggerEffects": [
                        { "type": "Buff", "target": "Self", "value": "focus", "duration": 2 }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete switching rule should pass validation");
    }

    /// <summary>
    /// STN-004: Verifies that all 7 switch condition types pass validation.
    /// </summary>
    [Test]
    [TestCase("HealthAbove")]
    [TestCase("HealthBelow")]
    [TestCase("HasStatus")]
    [TestCase("NotHasStatus")]
    [TestCase("InCombat")]
    [TestCase("OutOfCombat")]
    [TestCase("TurnNumber")]
    public void SwitchCondition_ValidType_PassesValidation(string conditionType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "switchingRule": {
                    "actionCost": "Swift",
                    "conditions": [
                        { "type": "{{conditionType}}", "value": 50 }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"switch condition with type '{conditionType}' should pass validation");
    }

    /// <summary>
    /// STN-004: Verifies that all 6 trigger effect types pass validation.
    /// </summary>
    [Test]
    [TestCase("Heal")]
    [TestCase("Damage")]
    [TestCase("ApplyStatus")]
    [TestCase("RemoveStatus")]
    [TestCase("Buff")]
    [TestCase("Debuff")]
    public void TriggerEffect_ValidType_PassesValidation(string effectType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "switchingRule": {
                    "actionCost": "Swift",
                    "triggerEffects": [
                        { "type": "{{effectType}}", "value": 10 }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"trigger effect with type '{effectType}' should pass validation");
    }

    /// <summary>
    /// STN-004: Verifies that global switching rules pass validation.
    /// </summary>
    [Test]
    public void GlobalSwitchingRules_Valid_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "globalSwitchingRules": {
                "actionCost": "Swift",
                "cooldown": 1
            },
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("global switching rules should pass validation");
    }

    #endregion

    #region STN-005: AI Behavior Validation

    /// <summary>
    /// STN-005: Verifies that all 7 AI condition types pass validation.
    /// </summary>
    [Test]
    [TestCase("HealthBelow")]
    [TestCase("HealthAbove")]
    [TestCase("EnemyCount")]
    [TestCase("AllyCount")]
    [TestCase("StatusActive")]
    [TestCase("TargetWeak")]
    [TestCase("OutNumbered")]
    public void AICondition_ValidType_PassesValidation(string conditionType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "aiBehavior": {
                    "priority": 5,
                    "useWhen": [
                        { "type": "{{conditionType}}", "value": 50 }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"AI condition with type '{conditionType}' should pass validation");
    }

    /// <summary>
    /// STN-005: Verifies that invalid AI condition type fails validation.
    /// </summary>
    [Test]
    public void AICondition_InvalidType_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "aiBehavior": {
                    "priority": 5,
                    "useWhen": [
                        { "type": "InvalidCondition" }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("invalid AI condition type should fail validation");
    }

    /// <summary>
    /// STN-005: Verifies that complete AI behavior passes validation.
    /// </summary>
    [Test]
    public void AIBehavior_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "aiBehavior": {
                    "priority": 5,
                    "useWhen": [
                        { "type": "HealthAbove", "value": 60 },
                        { "type": "EnemyCount", "value": 1, "weight": 2.0 }
                    ],
                    "avoidWhen": [
                        { "type": "HealthBelow", "value": 30 }
                    ],
                    "stickiness": 0.7
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete AI behavior should pass validation");
    }

    /// <summary>
    /// STN-005: Verifies that priority above maximum fails validation.
    /// </summary>
    [Test]
    public void AIBehavior_PriorityAboveMax_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "aiBehavior": {
                    "priority": 150,
                    "useWhen": [
                        { "type": "HealthAbove", "value": 50 }
                    ]
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("priority above 100 should fail validation");
    }

    /// <summary>
    /// STN-005: Verifies that stickiness outside range fails validation.
    /// </summary>
    [Test]
    public void AIBehavior_StickinessOutOfRange_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "aiBehavior": {
                    "priority": 5,
                    "useWhen": [
                        { "type": "HealthAbove", "value": 50 }
                    ],
                    "stickiness": 1.5
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("stickiness > 1.0 should fail validation");
    }

    /// <summary>
    /// STN-005: Verifies that AI behavior requires useWhen.
    /// </summary>
    [Test]
    public void AIBehavior_MissingUseWhen_FailsValidation()
    {
        var invalidJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "aiBehavior": {
                    "priority": 5
                }
            }]
        }
        """;
        var errors = _schema.Validate(invalidJson);
        errors.Should().NotBeEmpty("AI behavior without useWhen should fail validation");
    }

    #endregion

    #region STN-006: Unlock Condition and Additional Validation

    /// <summary>
    /// STN-006: Verifies that all 5 unlock condition types pass validation.
    /// </summary>
    [Test]
    [TestCase("Level")]
    [TestCase("Quest")]
    [TestCase("Skill")]
    [TestCase("Item")]
    [TestCase("Achievement")]
    public void UnlockCondition_ValidType_PassesValidation(string unlockType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "requiresUnlock": true,
                "unlockCondition": {
                    "type": "{{unlockType}}",
                    "value": 10,
                    "description": "Unlock requirement"
                }
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"unlock condition with type '{unlockType}' should pass validation");
    }

    /// <summary>
    /// STN-006: Verifies that ability restrictions pass validation.
    /// </summary>
    [Test]
    [TestCase("Category")]
    [TestCase("Specific")]
    [TestCase("Tag")]
    public void AbilityRestriction_ValidType_PassesValidation(string restrictionType)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "abilityRestrictions": [{
                    "type": "{{restrictionType}}",
                    "target": "defensive",
                    "description": "Restriction reason"
                }]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"ability restriction with type '{restrictionType}' should pass validation");
    }

    /// <summary>
    /// STN-006: Verifies that ability grants pass validation.
    /// </summary>
    [Test]
    public void Stance_WithAbilityGrants_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "abilityGrants": ["power-attack", "reckless-strike"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("stance with ability grants should pass validation");
    }

    /// <summary>
    /// STN-006: Verifies that incompatible stances pass validation.
    /// </summary>
    [Test]
    public void Stance_WithIncompatibleStances_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "incompatibleStances": ["defensive", "balanced"]
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("stance with incompatible stances should pass validation");
    }

    /// <summary>
    /// STN-006: Verifies that valid hex color passes validation.
    /// </summary>
    [Test]
    [TestCase("#CC3333")]
    [TestCase("#ffffff")]
    [TestCase("#000000")]
    public void Stance_ValidColor_PassesValidation(string color)
    {
        var validJson = $$"""
        {
            "version": "1.0.0",
            "defaultStance": "test",
            "stances": [{
                "id": "test",
                "name": "Test",
                "description": "A test stance",
                "color": "{{color}}"
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty($"stance with valid color '{color}' should pass validation");
    }

    /// <summary>
    /// STN-006: Verifies that complete stance with all fields passes validation.
    /// </summary>
    [Test]
    public void Stance_Complete_PassesValidation()
    {
        var validJson = """
        {
            "version": "1.0.0",
            "defaultStance": "aggressive",
            "stances": [{
                "id": "aggressive",
                "name": "Aggressive",
                "description": "An offensive stance for increased damage.",
                "iconId": "icon-stance-aggressive",
                "color": "#CC3333",
                "statModifiers": [
                    { "stat": "attack", "value": 20, "type": "percentage" },
                    { "stat": "defense", "value": -20, "type": "percentage" }
                ],
                "abilityGrants": ["power-attack"],
                "abilityRestrictions": [
                    { "type": "Category", "target": "defensive" }
                ],
                "incompatibleStances": ["defensive"],
                "switchingRule": {
                    "actionCost": "Swift",
                    "cooldown": 1,
                    "conditions": [
                        { "type": "NotHasStatus", "value": "stunned" }
                    ]
                },
                "aiBehavior": {
                    "priority": 3,
                    "useWhen": [
                        { "type": "HealthAbove", "value": 60 }
                    ],
                    "stickiness": 0.6
                },
                "visualEffectId": "vfx-aggressive",
                "soundEffectId": "sfx-aggressive",
                "isDefault": false,
                "requiresUnlock": false,
                "tags": ["offensive", "damage"],
                "sortOrder": 2
            }]
        }
        """;
        var errors = _schema.Validate(validJson);
        errors.Should().BeEmpty("complete stance with all fields should pass validation");
    }

    #endregion
}
