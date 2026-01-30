// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationDefinitionTests.cs
// Unit tests for the SpecializationDefinition entity verifying factory method
// creation, parameter validation, cross-validation of path type and parent
// archetype, computed properties, helper methods, and display formatting.
// Version: 0.17.4b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="SpecializationDefinition"/> entity.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that SpecializationDefinition correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates instances with valid parameters via the factory method</description></item>
///   <item><description>Defaults to SpecialResourceDefinition.None when no special resource is provided</description></item>
///   <item><description>Generates unique GUIDs for each instance</description></item>
///   <item><description>Trims whitespace from all string parameters</description></item>
///   <item><description>Validates all required string parameters, rejecting null or whitespace</description></item>
///   <item><description>Validates unlock cost is non-negative</description></item>
///   <item><description>Cross-validates path type against the specialization's inherent path type</description></item>
///   <item><description>Cross-validates parent archetype against the specialization's inherent archetype</description></item>
///   <item><description>Computes HasSpecialResource and IsHeretical correctly</description></item>
///   <item><description>Returns correct Corruption warnings for Heretical specializations</description></item>
///   <item><description>Formats ToString and GetSelectionDisplay output correctly</description></item>
/// </list>
/// </remarks>
/// <seealso cref="SpecializationDefinition"/>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="SpecialResourceDefinition"/>
[TestFixture]
public class SpecializationDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create() with valid parameters returns a fully populated
    /// SpecializationDefinition with all properties correctly assigned.
    /// Uses Berserkr (Heretical Warrior with Rage special resource) as
    /// a comprehensive test case.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Arrange
        var rage = SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Fury that builds with each strike and received blow");

        // Act
        var definition = SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "The Berserkr channels primal rage into devastating combat prowess.",
            "Embrace the fury. Let rage fuel your strikes.",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0,
            specialResource: rage);

        // Assert
        definition.Should().NotBeNull();
        definition.Id.Should().NotBeEmpty();
        definition.SpecializationId.Should().Be(SpecializationId.Berserkr);
        definition.DisplayName.Should().Be("Berserkr");
        definition.Tagline.Should().Be("Fury Unleashed");
        definition.Description.Should().Be("The Berserkr channels primal rage into devastating combat prowess.");
        definition.SelectionText.Should().Be("Embrace the fury. Let rage fuel your strikes.");
        definition.ParentArchetype.Should().Be(Archetype.Warrior);
        definition.PathType.Should().Be(SpecializationPathType.Heretical);
        definition.UnlockCost.Should().Be(0);
        definition.SpecialResource.HasResource.Should().BeTrue();
        definition.SpecialResource.ResourceId.Should().Be("rage");
    }

    /// <summary>
    /// Verifies that Create() without a special resource parameter defaults
    /// to SpecialResourceDefinition.None, and HasSpecialResource returns false.
    /// Uses Skald (Coherent Adept without special resource) as the test case.
    /// </summary>
    [Test]
    public void Create_WithoutSpecialResource_DefaultsToNone()
    {
        // Arrange & Act
        var definition = CreateValidSkaldDefinition();

        // Assert
        definition.HasSpecialResource.Should().BeFalse();
        definition.SpecialResource.HasResource.Should().BeFalse();
        definition.SpecialResource.ResourceId.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that Create() generates a unique GUID for each instance,
    /// even when called with the same specialization parameters.
    /// </summary>
    [Test]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var definition1 = CreateValidBerserkrDefinition();
        var definition2 = CreateValidBerserkrDefinition();

        // Assert
        definition1.Id.Should().NotBe(definition2.Id);
    }

    /// <summary>
    /// Verifies that Create() trims whitespace from all string parameters,
    /// ensuring consistent storage regardless of input formatting.
    /// </summary>
    [Test]
    public void Create_TrimsWhitespaceFromStrings()
    {
        // Arrange & Act
        var definition = SpecializationDefinition.Create(
            SpecializationId.Skald,
            "  Skald  ",
            "  Voice of the Saga  ",
            "  The Skald weaves words into power.  ",
            "  Sing the songs that shape the world.  ",
            Archetype.Adept,
            SpecializationPathType.Coherent,
            unlockCost: 0);

        // Assert
        definition.DisplayName.Should().Be("Skald");
        definition.Tagline.Should().Be("Voice of the Saga");
        definition.Description.Should().Be("The Skald weaves words into power.");
        definition.SelectionText.Should().Be("Sing the songs that shape the world.");
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
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            null!,
            "Tagline",
            "Description",
            "Selection text",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0);

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
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "   ",
            "Description",
            "Selection text",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0);

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
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Tagline",
            null!,
            "Selection text",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0);

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
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Tagline",
            "Description",
            null!,
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentOutOfRangeException when
    /// unlockCost is negative, which is not a valid Progression Point cost.
    /// </summary>
    [Test]
    public void Create_WithNegativeUnlockCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Tagline",
            "Description",
            "Selection text",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when the provided
    /// pathType does not match the specialization's inherent path type.
    /// Berserkr is inherently Heretical; passing Coherent should fail.
    /// </summary>
    [Test]
    public void Create_WithMismatchedPathType_ThrowsArgumentException()
    {
        // Arrange — Berserkr is Heretical, not Coherent
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "Description",
            "Selection text",
            Archetype.Warrior,
            SpecializationPathType.Coherent,
            unlockCost: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PathType mismatch*");
    }

    /// <summary>
    /// Verifies that Create() throws ArgumentException when the provided
    /// parentArchetype does not match the specialization's inherent archetype.
    /// Berserkr belongs to Warrior, not Mystic.
    /// </summary>
    [Test]
    public void Create_WithMismatchedArchetype_ThrowsArgumentException()
    {
        // Arrange — Berserkr belongs to Warrior, not Mystic
        var act = () => SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "Description",
            "Selection text",
            Archetype.Mystic,
            SpecializationPathType.Heretical,
            unlockCost: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ParentArchetype mismatch*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasSpecialResource returns true for a specialization
    /// created with a valid special resource (Berserkr with Rage).
    /// </summary>
    [Test]
    public void HasSpecialResource_WithResource_ReturnsTrue()
    {
        // Arrange
        var definition = CreateValidBerserkrDefinition();

        // Assert
        definition.HasSpecialResource.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasSpecialResource returns false for a specialization
    /// created without a special resource (Skald).
    /// </summary>
    [Test]
    public void HasSpecialResource_WithoutResource_ReturnsFalse()
    {
        // Arrange
        var definition = CreateValidSkaldDefinition();

        // Assert
        definition.HasSpecialResource.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsHeretical returns true for Heretical specializations.
    /// Berserkr is Heretical (corrupted Aether, Corruption risk).
    /// </summary>
    [Test]
    public void IsHeretical_ForHereticalSpec_ReturnsTrue()
    {
        // Arrange
        var definition = CreateValidBerserkrDefinition();

        // Assert
        definition.IsHeretical.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsHeretical returns false for Coherent specializations.
    /// Skjaldmaer is Coherent (stable reality, no Corruption risk).
    /// </summary>
    [Test]
    public void IsHeretical_ForCoherentSpec_ReturnsFalse()
    {
        // Arrange
        var definition = CreateValidSkjaldmaerDefinition();

        // Assert
        definition.IsHeretical.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetCorruptionWarning() returns a warning string for
    /// Heretical specializations. Berserkr is Heretical and should return
    /// a warning about Corruption risk.
    /// </summary>
    [Test]
    public void GetCorruptionWarning_ForHeretical_ReturnsWarningText()
    {
        // Arrange
        var definition = CreateValidBerserkrDefinition();

        // Act
        var warning = definition.GetCorruptionWarning();

        // Assert
        warning.Should().NotBeNullOrEmpty();
        warning.Should().Contain("Corruption");
    }

    /// <summary>
    /// Verifies that GetCorruptionWarning() returns null for Coherent
    /// specializations. Skjaldmaer is Coherent and has no Corruption risk.
    /// </summary>
    [Test]
    public void GetCorruptionWarning_ForCoherent_ReturnsNull()
    {
        // Arrange
        var definition = CreateValidSkjaldmaerDefinition();

        // Act
        var warning = definition.GetCorruptionWarning();

        // Assert
        warning.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetSelectionDisplay() returns a formatted multi-line
    /// string containing the display name, quoted tagline, and selection text.
    /// </summary>
    [Test]
    public void GetSelectionDisplay_ReturnsFormattedMultilineString()
    {
        // Arrange
        var definition = CreateValidBerserkrDefinition();

        // Act
        var display = definition.GetSelectionDisplay();

        // Assert
        display.Should().Be("Berserkr\n\"Fury Unleashed\"\n\nEmbrace the fury. Let rage fuel your strikes.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString() returns a formatted string containing the
    /// display name, parent archetype, and path type.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var definition = CreateValidBerserkrDefinition();

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Be("Berserkr (Warrior, Heretical)");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid Berserkr definition with Rage special resource.
    /// Berserkr is a Heretical Warrior specialization.
    /// </summary>
    /// <returns>A valid <see cref="SpecializationDefinition"/> for Berserkr.</returns>
    private static SpecializationDefinition CreateValidBerserkrDefinition()
    {
        var rage = SpecialResourceDefinition.Create(
            resourceId: "rage",
            displayName: "Rage",
            minValue: 0,
            maxValue: 100,
            startsAt: 0,
            regenPerTurn: 0,
            decayPerTurn: 5,
            description: "Fury that builds with each strike and received blow");

        return SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "The Berserkr channels primal rage into devastating combat prowess.",
            "Embrace the fury. Let rage fuel your strikes.",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            unlockCost: 0,
            specialResource: rage);
    }

    /// <summary>
    /// Creates a valid Skald definition without a special resource.
    /// Skald is a Coherent Adept specialization.
    /// </summary>
    /// <returns>A valid <see cref="SpecializationDefinition"/> for Skald.</returns>
    private static SpecializationDefinition CreateValidSkaldDefinition() =>
        SpecializationDefinition.Create(
            SpecializationId.Skald,
            "Skald",
            "Voice of the Saga",
            "The Skald weaves words into a force that inspires allies.",
            "Sing the saga. Your voice carries the strength of every hero.",
            Archetype.Adept,
            SpecializationPathType.Coherent,
            unlockCost: 0);

    /// <summary>
    /// Creates a valid Skjaldmaer definition with Block Charges special resource.
    /// Skjaldmaer is a Coherent Warrior specialization.
    /// </summary>
    /// <returns>A valid <see cref="SpecializationDefinition"/> for Skjaldmaer.</returns>
    private static SpecializationDefinition CreateValidSkjaldmaerDefinition()
    {
        var blockCharges = SpecialResourceDefinition.Create(
            resourceId: "block-charges",
            displayName: "Block Charges",
            minValue: 0,
            maxValue: 3,
            startsAt: 3,
            regenPerTurn: 1,
            decayPerTurn: 0,
            description: "Stored defensive reactions ready to deflect incoming attacks");

        return SpecializationDefinition.Create(
            SpecializationId.Skjaldmaer,
            "Skjaldmaer",
            "The Living Shield",
            "The Skjaldmaer stands as an immovable bulwark.",
            "Be the shield that never breaks.",
            Archetype.Warrior,
            SpecializationPathType.Coherent,
            unlockCost: 0,
            specialResource: blockCharges);
    }
}
