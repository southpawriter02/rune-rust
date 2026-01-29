using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="LineageDefinition"/> entity.
/// </summary>
/// <remarks>
/// Verifies factory method behavior, validation, and helper methods.
/// </remarks>
[TestFixture]
public class LineageDefinitionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create successfully creates a definition with valid parameters.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesDefinition()
    {
        // Arrange & Act
        var definition = LineageDefinition.Create(
            Lineage.ClanBorn,
            "Clan-Born",
            "Descendants of survivors with untainted bloodlines.",
            "The Stable Code – Humanity's baseline.",
            LineageAttributeModifiers.ClanBorn,
            LineagePassiveBonuses.ClanBorn,
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn,
            "No distinctive physical mutations.",
            "Trusted as community leaders.");

        // Assert
        definition.Should().NotBeNull();
        definition.Id.Should().NotBe(Guid.Empty);
        definition.LineageId.Should().Be(Lineage.ClanBorn);
        definition.DisplayName.Should().Be("Clan-Born");
        definition.Description.Should().Contain("untainted bloodlines");
        definition.SelectionText.Should().Contain("Stable Code");
        definition.AttributeModifiers.HasFlexibleBonus.Should().BeTrue();
        definition.AppearanceNotes.Should().Be("No distinctive physical mutations.");
        definition.SocialRole.Should().Be("Trusted as community leaders.");
    }

    /// <summary>
    /// Verifies that Create throws when displayName is null or whitespace.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => LineageDefinition.Create(
            Lineage.ClanBorn,
            null!,
            "Description",
            "Selection text",
            LineageAttributeModifiers.ClanBorn,
            LineagePassiveBonuses.ClanBorn,
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

    /// <summary>
    /// Verifies that Create throws when description is empty.
    /// </summary>
    [Test]
    public void Create_WithEmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => LineageDefinition.Create(
            Lineage.ClanBorn,
            "Clan-Born",
            "",
            "Selection text",
            LineageAttributeModifiers.ClanBorn,
            LineagePassiveBonuses.ClanBorn,
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("description");
    }

    /// <summary>
    /// Verifies that Create throws when selectionText is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceSelectionText_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => LineageDefinition.Create(
            Lineage.ClanBorn,
            "Clan-Born",
            "Description",
            "   ",
            LineageAttributeModifiers.ClanBorn,
            LineagePassiveBonuses.ClanBorn,
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("selectionText");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that RequiresFlexibleBonusSelection returns true for ClanBorn.
    /// </summary>
    [Test]
    public void RequiresFlexibleBonusSelection_ForClanBorn_ReturnsTrue()
    {
        // Arrange
        var definition = LineageDefinition.Create(
            Lineage.ClanBorn,
            "Clan-Born",
            "Descendants of survivors.",
            "The Stable Code.",
            LineageAttributeModifiers.ClanBorn,
            LineagePassiveBonuses.ClanBorn,
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn);

        // Act
        var result = definition.RequiresFlexibleBonusSelection();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that RequiresFlexibleBonusSelection returns false for non-ClanBorn lineages.
    /// </summary>
    [TestCase(Lineage.RuneMarked)]
    [TestCase(Lineage.IronBlooded)]
    [TestCase(Lineage.VargrKin)]
    public void RequiresFlexibleBonusSelection_ForOtherLineages_ReturnsFalse(Lineage lineage)
    {
        // Arrange
        var modifiers = lineage switch
        {
            Lineage.RuneMarked => LineageAttributeModifiers.RuneMarked,
            Lineage.IronBlooded => LineageAttributeModifiers.IronBlooded,
            Lineage.VargrKin => LineageAttributeModifiers.VargrKin,
            _ => throw new ArgumentException("Unexpected lineage")
        };

        var passiveBonuses = lineage switch
        {
            Lineage.RuneMarked => LineagePassiveBonuses.RuneMarked,
            Lineage.IronBlooded => LineagePassiveBonuses.IronBlooded,
            Lineage.VargrKin => LineagePassiveBonuses.VargrKin,
            _ => throw new ArgumentException("Unexpected lineage")
        };

        var trait = lineage switch
        {
            Lineage.RuneMarked => LineageTrait.AetherTainted,
            Lineage.IronBlooded => LineageTrait.HazardAcclimation,
            Lineage.VargrKin => LineageTrait.PrimalClarity,
            _ => throw new ArgumentException("Unexpected lineage")
        };

        var traumaBaseline = lineage switch
        {
            Lineage.RuneMarked => LineageTraumaBaseline.RuneMarked,
            Lineage.IronBlooded => LineageTraumaBaseline.IronBlooded,
            Lineage.VargrKin => LineageTraumaBaseline.VargrKin,
            _ => throw new ArgumentException("Unexpected lineage")
        };

        var definition = LineageDefinition.Create(
            lineage,
            lineage.ToString(),
            "Test description for this lineage.",
            "Test selection text.",
            modifiers,
            passiveBonuses,
            trait,
            traumaBaseline);

        // Act
        var result = definition.RequiresFlexibleBonusSelection();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetTotalFixedModifiers returns correct sum.
    /// </summary>
    [Test]
    public void GetTotalFixedModifiers_ForRuneMarked_ReturnsCorrectSum()
    {
        // Arrange
        var definition = LineageDefinition.Create(
            Lineage.RuneMarked,
            "Rune-Marked",
            "Those whose blood carries the All-Rune's echo.",
            "The Tainted Aether.",
            LineageAttributeModifiers.RuneMarked,
            LineagePassiveBonuses.RuneMarked,
            LineageTrait.AetherTainted,
            LineageTraumaBaseline.RuneMarked);

        // Act
        var result = definition.GetTotalFixedModifiers();

        // Assert - RuneMarked has +2 WILL, -1 STURDINESS = net +1
        result.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OPTIONAL PARAMETER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that optional parameters default to empty strings when not provided.
    /// </summary>
    [Test]
    public void Create_WithoutOptionalParameters_SetsEmptyStrings()
    {
        // Arrange & Act
        var definition = LineageDefinition.Create(
            Lineage.IronBlooded,
            "Iron-Blooded",
            "Bloodlines hardened by Blight-metal.",
            "The Corrupted Earth.",
            LineageAttributeModifiers.IronBlooded,
            LineagePassiveBonuses.IronBlooded,
            LineageTrait.HazardAcclimation,
            LineageTraumaBaseline.IronBlooded);

        // Assert
        definition.AppearanceNotes.Should().BeEmpty();
        definition.SocialRole.Should().BeEmpty();
    }
}
