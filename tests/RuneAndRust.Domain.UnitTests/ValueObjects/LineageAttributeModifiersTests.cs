using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="LineageAttributeModifiers"/> value object.
/// </summary>
/// <remarks>
/// Verifies correct behavior of static factory properties, modifier methods,
/// computed properties, and ToString formatting.
/// </remarks>
[TestFixture]
public class LineageAttributeModifiersTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC FACTORY PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ClanBorn has a flexible bonus of +1.
    /// </summary>
    [Test]
    public void ClanBorn_HasFlexibleBonus_ReturnsTrue()
    {
        // Arrange & Act
        var clanBorn = LineageAttributeModifiers.ClanBorn;

        // Assert
        clanBorn.HasFlexibleBonus.Should().BeTrue();
        clanBorn.FlexibleBonusAmount.Should().Be(1);
        clanBorn.TotalFixedModifiers.Should().Be(0);
    }

    /// <summary>
    /// Verifies that RuneMarked has +2 WILL and -1 STURDINESS.
    /// </summary>
    [Test]
    public void RuneMarked_HasCorrectModifiers()
    {
        // Arrange & Act
        var runeMarked = LineageAttributeModifiers.RuneMarked;

        // Assert
        runeMarked.WillModifier.Should().Be(2);
        runeMarked.SturdinessModifier.Should().Be(-1);
        runeMarked.MightModifier.Should().Be(0);
        runeMarked.FinesseModifier.Should().Be(0);
        runeMarked.WitsModifier.Should().Be(0);
        runeMarked.HasFlexibleBonus.Should().BeFalse();
        runeMarked.TotalFixedModifiers.Should().Be(1);
    }

    /// <summary>
    /// Verifies that IronBlooded has +2 STURDINESS and -1 WILL.
    /// </summary>
    [Test]
    public void IronBlooded_HasCorrectModifiers()
    {
        // Arrange & Act
        var ironBlooded = LineageAttributeModifiers.IronBlooded;

        // Assert
        ironBlooded.SturdinessModifier.Should().Be(2);
        ironBlooded.WillModifier.Should().Be(-1);
        ironBlooded.MightModifier.Should().Be(0);
        ironBlooded.FinesseModifier.Should().Be(0);
        ironBlooded.WitsModifier.Should().Be(0);
        ironBlooded.HasFlexibleBonus.Should().BeFalse();
        ironBlooded.TotalFixedModifiers.Should().Be(1);
    }

    /// <summary>
    /// Verifies that VargrKin has +1 MIGHT, +1 FINESSE, and -1 WILL.
    /// </summary>
    [Test]
    public void VargrKin_HasCorrectModifiers()
    {
        // Arrange & Act
        var vargrKin = LineageAttributeModifiers.VargrKin;

        // Assert
        vargrKin.MightModifier.Should().Be(1);
        vargrKin.FinesseModifier.Should().Be(1);
        vargrKin.WillModifier.Should().Be(-1);
        vargrKin.WitsModifier.Should().Be(0);
        vargrKin.SturdinessModifier.Should().Be(0);
        vargrKin.HasFlexibleBonus.Should().BeFalse();
        vargrKin.TotalFixedModifiers.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GETMODIFIER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetModifier returns correct values for each attribute.
    /// </summary>
    [Test]
    public void GetModifier_ForEachAttribute_ReturnsCorrectValue()
    {
        // Arrange
        var vargrKin = LineageAttributeModifiers.VargrKin;

        // Act & Assert
        vargrKin.GetModifier(CoreAttribute.Might).Should().Be(1);
        vargrKin.GetModifier(CoreAttribute.Finesse).Should().Be(1);
        vargrKin.GetModifier(CoreAttribute.Wits).Should().Be(0);
        vargrKin.GetModifier(CoreAttribute.Will).Should().Be(-1);
        vargrKin.GetModifier(CoreAttribute.Sturdiness).Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GETEFFECTIVEMODIFIER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetEffectiveModifier includes flexible bonus when target matches.
    /// </summary>
    [Test]
    public void GetEffectiveModifier_WithFlexibleBonusTarget_IncludesBonus()
    {
        // Arrange
        var clanBorn = LineageAttributeModifiers.ClanBorn;

        // Act
        var mightWithBonus = clanBorn.GetEffectiveModifier(CoreAttribute.Might, CoreAttribute.Might);
        var finesseWithoutBonus = clanBorn.GetEffectiveModifier(CoreAttribute.Finesse, CoreAttribute.Might);

        // Assert
        mightWithBonus.Should().Be(1);
        finesseWithoutBonus.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetEffectiveModifier returns base modifier when no target specified.
    /// </summary>
    [Test]
    public void GetEffectiveModifier_WithNoTarget_ReturnsBaseModifier()
    {
        // Arrange
        var clanBorn = LineageAttributeModifiers.ClanBorn;

        // Act
        var result = clanBorn.GetEffectiveModifier(CoreAttribute.Might, null);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetEffectiveModifier returns correct value for non-flexible lineages.
    /// </summary>
    [Test]
    public void GetEffectiveModifier_ForNonFlexibleLineage_ReturnsFixedModifier()
    {
        // Arrange
        var runeMarked = LineageAttributeModifiers.RuneMarked;

        // Act - Even with a target, non-flexible lineages don't add bonus
        var willWithTarget = runeMarked.GetEffectiveModifier(CoreAttribute.Will, CoreAttribute.Will);
        var sturdinessWithTarget = runeMarked.GetEffectiveModifier(CoreAttribute.Sturdiness, CoreAttribute.Sturdiness);

        // Assert
        willWithTarget.Should().Be(2);
        sturdinessWithTarget.Should().Be(-1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString includes flexible bonus notation for ClanBorn.
    /// </summary>
    [Test]
    public void ToString_ForClanBorn_IncludesFlexibleBonusNotation()
    {
        // Arrange
        var clanBorn = LineageAttributeModifiers.ClanBorn;

        // Act
        var result = clanBorn.ToString();

        // Assert
        result.Should().Contain("+1 to any");
    }

    /// <summary>
    /// Verifies that ToString shows modifiers correctly for RuneMarked.
    /// </summary>
    [Test]
    public void ToString_ForRuneMarked_ShowsModifiersCorrectly()
    {
        // Arrange
        var runeMarked = LineageAttributeModifiers.RuneMarked;

        // Act
        var result = runeMarked.ToString();

        // Assert
        result.Should().Contain("WILL +2");
        result.Should().Contain("STURDINESS -1");
    }
}
