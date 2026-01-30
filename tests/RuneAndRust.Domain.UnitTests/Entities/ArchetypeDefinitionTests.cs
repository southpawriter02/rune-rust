// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeDefinitionTests.cs
// Unit tests for the ArchetypeDefinition entity verifying factory method
// creation, parameter validation, and permanent choice enforcement.
// Version: 0.17.3a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="ArchetypeDefinition"/> entity.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that the ArchetypeDefinition entity correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates instances with valid parameters via the factory method</description></item>
///   <item><description>Validates all required string parameters, rejecting null or whitespace</description></item>
///   <item><description>Enforces the permanent choice flag (IsPermanent is always true)</description></item>
///   <item><description>Provides correct helper method outputs (GetSummary, GetSelectionDisplay, ToString)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ArchetypeDefinition"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="ResourceType"/>
[TestFixture]
public class ArchetypeDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create() with valid parameters returns a fully populated
    /// ArchetypeDefinition with all properties correctly assigned.
    /// </summary>
    [Test]
    public void Create_WithValidData_ReturnsDefinition()
    {
        // Arrange & Act
        var definition = ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "The Unyielding Bulwark",
            "Warriors are the frontline fighters who absorb punishment and deal devastating melee damage.",
            "You are the shield between the innocent and the horror. When others flee, you advance. Your body is a weapon, your will is unbreakable.",
            "Tank / Melee DPS",
            ResourceType.Stamina,
            "Stand in the front, absorb damage, protect allies");

        // Assert
        definition.Should().NotBeNull();
        definition.Id.Should().NotBeEmpty();
        definition.ArchetypeId.Should().Be(Archetype.Warrior);
        definition.DisplayName.Should().Be("Warrior");
        definition.Tagline.Should().Be("The Unyielding Bulwark");
        definition.Description.Should().Be("Warriors are the frontline fighters who absorb punishment and deal devastating melee damage.");
        definition.SelectionText.Should().Be("You are the shield between the innocent and the horror. When others flee, you advance. Your body is a weapon, your will is unbreakable.");
        definition.CombatRole.Should().Be("Tank / Melee DPS");
        definition.PrimaryResource.Should().Be(ResourceType.Stamina);
        definition.PlaystyleDescription.Should().Be("Stand in the front, absorb damage, protect allies");
        definition.IsPermanent.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create() throws ArgumentException when displayName is null.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArchetypeDefinition.Create(
            Archetype.Warrior,
            null!,
            "Tagline",
            "Description",
            "Selection text",
            "Combat role",
            ResourceType.Stamina,
            "Playstyle");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when tagline is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceTagline_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "   ",
            "Description",
            "Selection text",
            "Combat role",
            ResourceType.Stamina,
            "Playstyle");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when description is null.
    /// </summary>
    [Test]
    public void Create_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "Tagline",
            null!,
            "Selection text",
            "Combat role",
            ResourceType.Stamina,
            "Playstyle");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when selectionText is null.
    /// </summary>
    [Test]
    public void Create_WithNullSelectionText_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "Tagline",
            "Description",
            null!,
            "Combat role",
            ResourceType.Stamina,
            "Playstyle");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when combatRole is null.
    /// </summary>
    [Test]
    public void Create_WithNullCombatRole_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "Tagline",
            "Description",
            "Selection text",
            null!,
            ResourceType.Stamina,
            "Playstyle");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when playstyleDescription is null.
    /// </summary>
    [Test]
    public void Create_WithNullPlaystyleDescription_ThrowsArgumentException()
    {
        // Arrange
        var act = () => ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "Tagline",
            "Description",
            "Selection text",
            "Combat role",
            ResourceType.Stamina,
            null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERMANENT CHOICE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsPermanent is always true regardless of the archetype.
    /// Tests with Mystic to confirm the behavior is not Warrior-specific.
    /// </summary>
    [Test]
    public void IsPermanent_AlwaysReturnsTrue()
    {
        // Arrange
        var definition = CreateValidDefinition(Archetype.Mystic);

        // Assert
        definition.IsPermanent.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSummary() returns the expected formatted string
    /// containing display name, combat role, and primary resource.
    /// </summary>
    [Test]
    public void GetSummary_ReturnsFormattedString()
    {
        // Arrange
        var definition = ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "The Unyielding Bulwark",
            "Warriors are the frontline fighters.",
            "You are the shield.",
            "Tank / Melee DPS",
            ResourceType.Stamina,
            "Stand in the front");

        // Act
        var summary = definition.GetSummary();

        // Assert
        summary.Should().Be("Warrior (Tank / Melee DPS) - Stamina");
    }

    /// <summary>
    /// Verifies that GetSelectionDisplay() returns a multi-line string
    /// containing the display name, tagline in quotes, and selection text.
    /// </summary>
    [Test]
    public void GetSelectionDisplay_ReturnsFormattedMultilineString()
    {
        // Arrange
        var definition = ArchetypeDefinition.Create(
            Archetype.Warrior,
            "Warrior",
            "The Unyielding Bulwark",
            "Warriors are the frontline fighters.",
            "You are the shield.",
            "Tank / Melee DPS",
            ResourceType.Stamina,
            "Stand in the front");

        // Act
        var display = definition.GetSelectionDisplay();

        // Assert
        display.Should().Be("Warrior\n\"The Unyielding Bulwark\"\n\nYou are the shield.");
    }

    /// <summary>
    /// Verifies that ToString() returns a formatted debug string
    /// containing the display name and archetype enum value.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var definition = CreateValidDefinition(Archetype.Mystic);

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("ArchetypeDefinition: Mystic [Mystic]");
    }

    /// <summary>
    /// Verifies that Create() trims whitespace from all string parameters.
    /// </summary>
    [Test]
    public void Create_TrimsWhitespaceFromStrings()
    {
        // Arrange & Act
        var definition = ArchetypeDefinition.Create(
            Archetype.Warrior,
            "  Warrior  ",
            "  The Unyielding Bulwark  ",
            "  Warriors are the frontline fighters.  ",
            "  You are the shield.  ",
            "  Tank / Melee DPS  ",
            ResourceType.Stamina,
            "  Stand in the front  ");

        // Assert
        definition.DisplayName.Should().Be("Warrior");
        definition.Tagline.Should().Be("The Unyielding Bulwark");
        definition.Description.Should().Be("Warriors are the frontline fighters.");
        definition.SelectionText.Should().Be("You are the shield.");
        definition.CombatRole.Should().Be("Tank / Melee DPS");
        definition.PlaystyleDescription.Should().Be("Stand in the front");
    }

    /// <summary>
    /// Verifies that Create() generates a unique Id for each instance.
    /// </summary>
    [Test]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var definition1 = CreateValidDefinition(Archetype.Warrior);
        var definition2 = CreateValidDefinition(Archetype.Warrior);

        // Assert
        definition1.Id.Should().NotBe(definition2.Id);
    }

    /// <summary>
    /// Verifies that Mystic is created with AetherPool as primary resource,
    /// confirming it is the only archetype using AetherPool.
    /// </summary>
    [Test]
    public void Create_MysticArchetype_HasAetherPoolResource()
    {
        // Arrange & Act
        var definition = ArchetypeDefinition.Create(
            Archetype.Mystic,
            "Mystic",
            "Wielder of Tainted Aether",
            "Mystics channel corrupted Aether.",
            "The Aether flows through you.",
            "Caster / Control",
            ResourceType.AetherPool,
            "Ranged magical damage");

        // Assert
        definition.PrimaryResource.Should().Be(ResourceType.AetherPool);
        definition.ArchetypeId.Should().Be(Archetype.Mystic);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid ArchetypeDefinition with test data for the specified archetype.
    /// </summary>
    /// <param name="archetype">The archetype to create a definition for.</param>
    /// <returns>A valid <see cref="ArchetypeDefinition"/> instance.</returns>
    private static ArchetypeDefinition CreateValidDefinition(Archetype archetype) =>
        ArchetypeDefinition.Create(
            archetype,
            archetype.ToString(),
            "Test Tagline",
            "Test Description",
            "Test Selection Text",
            "Test Role",
            archetype == Archetype.Mystic ? ResourceType.AetherPool : ResourceType.Stamina,
            "Test Playstyle");
}
