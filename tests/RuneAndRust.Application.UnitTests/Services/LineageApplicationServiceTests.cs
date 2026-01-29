// ═══════════════════════════════════════════════════════════════════════════════
// LineageApplicationServiceTests.cs
// Unit tests for LineageApplicationService (v0.17.0f).
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="LineageApplicationService"/> (v0.17.0f).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Successful lineage application with all components</description></item>
///   <item><description>Validation failure when character already has a lineage</description></item>
///   <item><description>Validation failure for unknown lineage</description></item>
///   <item><description>Validation failure for null character</description></item>
///   <item><description>Clan-Born flexible bonus application</description></item>
///   <item><description>Clan-Born without flexible bonus returns failure</description></item>
///   <item><description>Non-Clan-Born with flexible bonus returns failure</description></item>
///   <item><description>GetApplicableLineages returns empty when lineage already set</description></item>
///   <item><description>GetApplicableLineages returns all four for new character</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class LineageApplicationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<ILineageProvider> _mockProvider = null!;
    private Mock<ILogger<LineageApplicationService>> _mockLogger = null!;
    private LineageApplicationService _service = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<ILineageProvider>();
        _mockLogger = new Mock<ILogger<LineageApplicationService>>();
        _service = new LineageApplicationService(_mockProvider.Object, _mockLogger.Object);

        // Set up default provider responses for Rune-Marked
        SetupRuneMarkedProvider();

        // Set up default provider responses for Clan-Born
        SetupClanBornProvider();

        // Set up GetAllLineages to return all four
        SetupGetAllLineages();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ApplyLineage TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ApplyLineage with a valid character applies all lineage
    /// components and returns a success result with correct details.
    /// </summary>
    [Test]
    public void ApplyLineage_WithValidCharacter_AppliesAllComponents()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyLineage(player, Lineage.RuneMarked);

        // Assert - Result is successful
        result.IsSuccess.Should().BeTrue();
        result.FailureReason.Should().BeNull();
        result.AppliedLineage.Should().Be(Lineage.RuneMarked);

        // Assert - Attribute changes recorded
        result.AttributeChanges.Should().NotBeNull();
        result.AttributeChanges!.Should().ContainKey(CoreAttribute.Will);
        result.AttributeChanges![CoreAttribute.Will].Should().Be(2);
        result.AttributeChanges!.Should().ContainKey(CoreAttribute.Sturdiness);
        result.AttributeChanges![CoreAttribute.Sturdiness].Should().Be(-1);

        // Assert - Bonuses applied
        result.BonusesApplied.Should().NotBeNull();
        result.BonusesApplied!.Value.MaxApBonus.Should().Be(5);

        // Assert - Trait registered
        result.TraitRegistered.Should().NotBeNull();
        result.TraitRegistered!.Value.TraitName.Should().Contain("Aether-Tainted");

        // Assert - Trauma baseline set
        result.TraumaBaselineSet.Should().NotBeNull();
        result.TraumaBaselineSet!.Value.StartingCorruption.Should().Be(5);
        result.TraumaBaselineSet!.Value.CorruptionResistanceModifier.Should().Be(-1);

        // Assert - Player entity was modified
        player.HasLineage.Should().BeTrue();
        player.SelectedLineage.Should().Be(Lineage.RuneMarked);
        player.LineageTrait.Should().NotBeNull();
        player.Corruption.Should().Be(5);
        player.CorruptionResistanceModifier.Should().Be(-1);
        player.LineageMaxApModifier.Should().Be(5);
    }

    /// <summary>
    /// Verifies that ApplyLineage fails when the character already has a lineage.
    /// </summary>
    [Test]
    public void ApplyLineage_CharacterAlreadyHasLineage_ReturnsFail()
    {
        // Arrange
        var player = new Player("TestHero");
        player.SetLineage(Lineage.ClanBorn);

        // Act
        var result = _service.ApplyLineage(player, Lineage.RuneMarked);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("already has a lineage");
        result.AppliedLineage.Should().BeNull();
        result.AttributeChanges.Should().BeNull();
    }

    /// <summary>
    /// Verifies that ApplyLineage fails when the lineage is not found in the provider.
    /// </summary>
    [Test]
    public void ApplyLineage_InvalidLineage_ReturnsFail()
    {
        // Arrange
        var player = new Player("TestHero");
        var unknownLineage = (Lineage)99;
        _mockProvider.Setup(p => p.GetLineage(unknownLineage)).Returns((LineageDefinition?)null);

        // Act
        var result = _service.ApplyLineage(player, unknownLineage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("Unknown lineage");
    }

    /// <summary>
    /// Verifies that Clan-Born lineage with a flexible bonus attribute applies correctly,
    /// including the +1 to the chosen attribute.
    /// </summary>
    [Test]
    public void ApplyLineage_ClanBornWithFlexibleBonus_AppliesCorrectly()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyLineage(player, Lineage.ClanBorn, CoreAttribute.Might);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AppliedLineage.Should().Be(Lineage.ClanBorn);

        // Flexible bonus should be in attribute changes
        result.AttributeChanges.Should().ContainKey(CoreAttribute.Might);
        result.AttributeChanges![CoreAttribute.Might].Should().Be(1);

        // Player should have lineage set
        player.HasLineage.Should().BeTrue();
        player.SelectedLineage.Should().Be(Lineage.ClanBorn);

        // HP bonus should be applied
        player.LineageMaxHpModifier.Should().Be(5);
    }

    /// <summary>
    /// Verifies that Clan-Born lineage without a flexible bonus attribute returns failure.
    /// </summary>
    [Test]
    public void ApplyLineage_ClanBornWithoutFlexibleBonus_ReturnsFail()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyLineage(player, Lineage.ClanBorn);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("requires a flexible bonus attribute");

        // Player should NOT have lineage set
        player.HasLineage.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that non-Clan-Born lineage with a flexible bonus attribute returns failure.
    /// </summary>
    [Test]
    public void ApplyLineage_NonClanBornWithFlexibleBonus_ReturnsFail()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyLineage(player, Lineage.RuneMarked, CoreAttribute.Will);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("does not support flexible bonus");

        // Player should NOT have lineage set
        player.HasLineage.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ValidateLineageSelection TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ValidateLineageSelection returns failure for null character.
    /// </summary>
    [Test]
    public void ValidateLineageSelection_NullCharacter_ReturnsFail()
    {
        // Arrange & Act
        var result = _service.ValidateLineageSelection(null!, Lineage.ClanBorn);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().Contain("null");
    }

    /// <summary>
    /// Verifies that ValidateLineageSelection returns valid for a valid selection.
    /// </summary>
    [Test]
    public void ValidateLineageSelection_ValidSelection_ReturnsValid()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ValidateLineageSelection(player, Lineage.RuneMarked);

        // Assert
        result.IsValid.Should().BeTrue();
        result.FailureReason.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetApplicableLineages TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetApplicableLineages returns empty when character already has a lineage.
    /// </summary>
    [Test]
    public void GetApplicableLineages_CharacterHasLineage_ReturnsEmpty()
    {
        // Arrange
        var player = new Player("TestHero");
        player.SetLineage(Lineage.ClanBorn);

        // Act
        var lineages = _service.GetApplicableLineages(player);

        // Assert
        lineages.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetApplicableLineages returns all four lineages for a new character.
    /// </summary>
    [Test]
    public void GetApplicableLineages_NewCharacter_ReturnsAllFour()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var lineages = _service.GetApplicableLineages(player);

        // Assert
        lineages.Should().HaveCount(4);
        lineages.Should().Contain(Lineage.ClanBorn);
        lineages.Should().Contain(Lineage.RuneMarked);
        lineages.Should().Contain(Lineage.IronBlooded);
        lineages.Should().Contain(Lineage.VargrKin);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Configures the mock provider with Rune-Marked lineage data.
    /// </summary>
    private void SetupRuneMarkedProvider()
    {
        var runeMarkedDef = LineageDefinition.Create(
            Lineage.RuneMarked,
            "Rune-Marked",
            "The Tainted Aether - descendants scarred by direct Runic Blight exposure.",
            "The Tainted Aether – Your bloodline carries the Blight's mark.",
            new LineageAttributeModifiers(0, 0, 0, 2, -1, false, 0),
            LineagePassiveBonuses.Create(0, 5, 0, 0,
                new List<SkillBonus> { SkillBonus.Create("lore", 1) }),
            LineageTrait.AetherTainted,
            LineageTraumaBaseline.RuneMarked);

        _mockProvider.Setup(p => p.GetLineage(Lineage.RuneMarked))
            .Returns(runeMarkedDef);
    }

    /// <summary>
    /// Configures the mock provider with Clan-Born lineage data.
    /// </summary>
    private void SetupClanBornProvider()
    {
        var clanBornDef = LineageDefinition.Create(
            Lineage.ClanBorn,
            "Clan-Born",
            "The Stable Code - descendants of survivors with untainted bloodlines.",
            "The Stable Code – Your ancestors endured.",
            new LineageAttributeModifiers(0, 0, 0, 0, 0, true, 1),
            LineagePassiveBonuses.Create(5, 0, 0, 0,
                new List<SkillBonus> { SkillBonus.Create("social", 1) }),
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn);

        _mockProvider.Setup(p => p.GetLineage(Lineage.ClanBorn))
            .Returns(clanBornDef);
    }

    /// <summary>
    /// Configures the mock provider to return all four lineage definitions.
    /// </summary>
    private void SetupGetAllLineages()
    {
        var clanBornDef = LineageDefinition.Create(
            Lineage.ClanBorn,
            "Clan-Born",
            "The Stable Code",
            "The Stable Code",
            LineageAttributeModifiers.ClanBorn,
            LineagePassiveBonuses.ClanBorn,
            LineageTrait.SurvivorsResolve,
            LineageTraumaBaseline.ClanBorn);

        var runeMarkedDef = LineageDefinition.Create(
            Lineage.RuneMarked,
            "Rune-Marked",
            "The Tainted Aether",
            "The Tainted Aether",
            LineageAttributeModifiers.RuneMarked,
            LineagePassiveBonuses.RuneMarked,
            LineageTrait.AetherTainted,
            LineageTraumaBaseline.RuneMarked);

        var ironBloodedDef = LineageDefinition.Create(
            Lineage.IronBlooded,
            "Iron-Blooded",
            "The Corrupted Earth",
            "The Corrupted Earth",
            LineageAttributeModifiers.IronBlooded,
            LineagePassiveBonuses.IronBlooded,
            LineageTrait.HazardAcclimation,
            LineageTraumaBaseline.IronBlooded);

        var vargrKinDef = LineageDefinition.Create(
            Lineage.VargrKin,
            "Vargr-Kin",
            "The Uncorrupted Echo",
            "The Uncorrupted Echo",
            LineageAttributeModifiers.VargrKin,
            LineagePassiveBonuses.VargrKin,
            LineageTrait.PrimalClarity,
            LineageTraumaBaseline.VargrKin);

        _mockProvider.Setup(p => p.GetAllLineages())
            .Returns(new List<LineageDefinition>
            {
                clanBornDef, runeMarkedDef, ironBloodedDef, vargrKinDef
            });
    }
}
