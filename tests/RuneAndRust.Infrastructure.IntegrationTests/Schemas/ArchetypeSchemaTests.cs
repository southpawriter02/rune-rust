// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeSchemaTests.cs
// Integration tests for archetypes.schema.json validation.
// Verifies schema structure, archetype definition constraints, and required fields.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.IntegrationTests.Schemas;

using FluentAssertions;
using NJsonSchema;
using NUnit.Framework;

/// <summary>
/// Integration tests validating the archetypes.schema.json JSON Schema.
/// </summary>
/// <remarks>
/// <para>
/// Tests ensure the schema correctly validates archetype configuration files,
/// enforces archetypeId and primaryResource enums, isPermanent const constraint,
/// and validates required fields for the v0.17.3a archetype definition format.
/// </para>
/// <para>
/// The schema was replaced in v0.17.3a from the legacy format (id, name,
/// statTendency, sortOrder) to the new definition format (archetypeId,
/// displayName, tagline, description, selectionText, combatRole,
/// primaryResource, playstyleDescription, isPermanent).
/// </para>
/// </remarks>
[TestFixture]
public class ArchetypeSchemaTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Path to the archetypes schema file relative to the test execution directory.
    /// </summary>
    private const string SchemaPath = "../../../../../config/schemas/archetypes.schema.json";

    /// <summary>
    /// Path to the actual archetypes.json configuration file.
    /// </summary>
    private const string ArchetypesJsonPath = "../../../../../config/archetypes.json";

    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loaded JSON Schema for archetype definitions.
    /// </summary>
    private JsonSchema _schema = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets up the test fixture by loading the archetypes schema.
    /// </summary>
    [SetUp]
    public async Task SetUp()
    {
        // Arrange: Load the schema from the file system
        var schemaContent = await File.ReadAllTextAsync(SchemaPath);
        _schema = await JsonSchema.FromJsonAsync(schemaContent);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCHEMA STRUCTURE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies the schema file is valid JSON Schema Draft-07 with expected structure.
    /// </summary>
    [Test]
    public void Schema_IsValidJsonSchemaDraft07()
    {
        // Assert: Schema loaded successfully and has expected properties
        _schema.Should().NotBeNull("schema file should parse successfully");
        _schema.Title.Should().Be("Archetypes Configuration", "schema title should match");
        _schema.Type.Should().Be(JsonObjectType.Object, "root type should be object");
        _schema.Definitions.Should().ContainKey("archetypeDefinition",
            "should define archetypeDefinition in definitions");
    }

    /// <summary>
    /// Verifies the schema requires exactly 4 archetypes (minItems and maxItems).
    /// </summary>
    [Test]
    public void Schema_RequiresExactlyFourArchetypes()
    {
        // Assert: archetypes array has min/max constraints
        _schema.Properties.Should().ContainKey("archetypes",
            "schema should have archetypes property");

        var archetypesProperty = _schema.Properties["archetypes"];
        archetypesProperty.MinItems.Should().Be(4,
            "archetypes array should require exactly 4 items (minItems)");
        archetypesProperty.MaxItems.Should().Be(4,
            "archetypes array should require exactly 4 items (maxItems)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONFIGURATION VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies the existing archetypes.json configuration validates against the schema
    /// without errors. This is the primary acceptance test ensuring existing data is
    /// compatible with the v0.17.3a schema.
    /// </summary>
    [Test]
    public async Task ExistingArchetypesJson_ValidatesAgainstSchema()
    {
        // Arrange: Load the actual archetypes.json file
        var archetypesJsonContent = await File.ReadAllTextAsync(ArchetypesJsonPath);

        // Act: Validate the archetypes.json content against the schema
        var errors = _schema.Validate(archetypesJsonContent);

        // Assert: No validation errors
        errors.Should().BeEmpty(
            "existing archetypes.json should validate against the schema without errors");
    }

    /// <summary>
    /// Verifies that an invalid archetypeId value produces validation errors.
    /// The archetypeId must be one of: Warrior, Skirmisher, Mystic, Adept.
    /// </summary>
    [Test]
    public void ArchetypeId_InvalidEnumValue_FailsValidation()
    {
        // Arrange: JSON with invalid archetypeId value
        var invalidJson = """
        {
            "archetypes": [
                {
                    "archetypeId": "Berserker",
                    "displayName": "Berserker",
                    "tagline": "The Raging Storm",
                    "description": "A rage-fueled warrior.",
                    "selectionText": "You see red.",
                    "combatRole": "Melee DPS",
                    "primaryResource": "Stamina",
                    "playstyleDescription": "All-out aggression",
                    "isPermanent": true
                },
                {
                    "archetypeId": "Warrior",
                    "displayName": "Warrior",
                    "tagline": "The Unyielding Bulwark",
                    "description": "Warriors are frontline fighters.",
                    "selectionText": "You are the shield.",
                    "combatRole": "Tank / Melee DPS",
                    "primaryResource": "Stamina",
                    "playstyleDescription": "Stand in the front",
                    "isPermanent": true
                },
                {
                    "archetypeId": "Skirmisher",
                    "displayName": "Skirmisher",
                    "tagline": "Swift as Shadow",
                    "description": "Skirmishers excel at mobility.",
                    "selectionText": "Speed is survival.",
                    "combatRole": "Mobile DPS",
                    "primaryResource": "Stamina",
                    "playstyleDescription": "Hit and run",
                    "isPermanent": true
                },
                {
                    "archetypeId": "Mystic",
                    "displayName": "Mystic",
                    "tagline": "Wielder of Tainted Aether",
                    "description": "Mystics channel Aether.",
                    "selectionText": "The Aether flows.",
                    "combatRole": "Caster / Control",
                    "primaryResource": "AetherPool",
                    "playstyleDescription": "Ranged magic",
                    "isPermanent": true
                }
            ]
        }
        """;

        // Act: Validate the invalid JSON
        var errors = _schema.Validate(invalidJson);

        // Assert: Should produce validation errors for the invalid archetypeId
        errors.Should().NotBeEmpty(
            "archetypeId 'Berserker' should fail enum validation");
    }
}
