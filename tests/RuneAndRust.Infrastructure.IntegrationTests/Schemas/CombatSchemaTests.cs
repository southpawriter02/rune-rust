// ------------------------------------------------------------------------------
// <copyright file="CombatSchemaTests.cs" company="Rune & Rust">
//     Copyright (c) Rune & Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for combat.schema.json validation.
// Verifies schema structure, flanking config, opportunity attack config,
// initiative config (base attribute and tie breaker enums), and death config validation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Unit tests validating the combat.schema.json JSON Schema.
/// Tests ensure the schema correctly validates combat rules configuration files,
/// enforces base attribute enum, tie breaker enum, bonus minimums, and death save minimums.
/// </summary>
[TestFixture]
public class CombatSchemaTests
{
    /// <summary>
    /// Path to the combat schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/combat.schema.json";

    /// <summary>
    /// Path to the actual combat.json configuration file.
    /// </summary>
    private const string CombatJsonPath = "../../../../../config/combat.json";

    /// <summary>
    /// Loaded JSON Schema for combat rules definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    /// <summary>
    /// Sets up the test fixture by loading the combat schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    /// <summary>
    /// Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// Validates that all required definitions are present (FlankingConfig,
    /// OpportunityAttackConfig, DisengageConfig, InitiativeConfig, DeathConfig).
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Combat Rules Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");

        // Verify all required definitions are present
        _schema.Definitions.Should().ContainKey("FlankingConfig", "should define FlankingConfig");
        _schema.Definitions.Should().ContainKey("OpportunityAttackConfig", "should define OpportunityAttackConfig");
        _schema.Definitions.Should().ContainKey("DisengageConfig", "should define DisengageConfig");
        _schema.Definitions.Should().ContainKey("InitiativeConfig", "should define InitiativeConfig");
        _schema.Definitions.Should().ContainKey("DeathConfig", "should define DeathConfig");
    }

    /// <summary>
    /// Verifies the existing combat.json configuration validates against the schema
    /// without errors. This is the primary backwards compatibility test ensuring
    /// all existing combat settings are valid.
    /// </summary>
    [Test]
    public async Task ExistingCombatJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual combat.json file
        var combatJsonContent = await File.ReadAllTextAsync(CombatJsonPath);

        // Act: Validate the combat.json content against the schema
        var errors = _schema.Validate(combatJsonContent);

        // Assert: No validation errors - all existing settings should be valid
        errors.Should().BeEmpty(
            "existing combat.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that tieBreaker must be a valid enum value
    /// (HigherAttribute, Random, PlayerFirst). Invalid values should fail validation.
    /// </summary>
    [Test]
    public void TieBreaker_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid tieBreaker enum value
        var invalidJson = """
        {
            "initiative": {
                "tieBreaker": "Alphabetical"
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid tieBreaker enum
        errors.Should().NotBeEmpty("invalid tieBreaker 'Alphabetical' should fail validation");
    }

    /// <summary>
    /// Verifies that baseAttribute must be a valid enum value (wits, finesse).
    /// Invalid values should fail validation.
    /// </summary>
    [Test]
    public void BaseAttribute_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid baseAttribute enum value
        var invalidJson = """
        {
            "initiative": {
                "baseAttribute": "strength"
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to invalid baseAttribute enum
        errors.Should().NotBeEmpty("invalid baseAttribute 'strength' should fail validation");
    }

    /// <summary>
    /// Verifies that bonus fields (attackBonus, damageBonus) must be >= 0.
    /// Negative values should fail validation.
    /// </summary>
    [Test]
    public void AttackBonus_NegativeValue_FailsValidation()
    {
        // Arrange: JSON with negative attackBonus
        var invalidJson = """
        {
            "flanking": {
                "attackBonus": -5
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to negative attackBonus
        errors.Should().NotBeEmpty("negative attackBonus -5 should fail validation (minimum is 0)");
    }

    /// <summary>
    /// Verifies that death save values must be >= 1.
    /// Zero values should fail validation.
    /// </summary>
    [Test]
    public void DeathSaveSuccesses_ZeroValue_FailsValidation()
    {
        // Arrange: JSON with zero deathSaveSuccesses
        var invalidJson = """
        {
            "death": {
                "deathSaveSuccesses": 0
            }
        }
        """;

        // Act: Validate the invalid JSON against the schema
        var errors = _schema.Validate(invalidJson);

        // Assert: Validation should fail due to deathSaveSuccesses being 0 (minimum is 1)
        errors.Should().NotBeEmpty("deathSaveSuccesses of 0 should fail validation (minimum is 1)");
    }
}
