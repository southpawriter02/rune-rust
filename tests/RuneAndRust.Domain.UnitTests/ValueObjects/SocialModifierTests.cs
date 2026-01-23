namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SocialModifier"/> value object.
/// </summary>
[TestFixture]
public class SocialModifierTests
{
    #region Factory Methods

    [Test]
    public void FactionStanding_CreatesCorrectModifier()
    {
        // Arrange & Act
        var modifier = SocialModifier.FactionStanding("merchants-guild", 1, "Honored");

        // Assert
        modifier.Source.Should().Be("Faction Standing");
        modifier.Description.Should().Contain("Honored");
        modifier.Description.Should().Contain("merchants-guild");
        modifier.DiceModifier.Should().Be(1);
        modifier.DcModifier.Should().Be(0);
        modifier.ModifierType.Should().Be(SocialModifierType.FactionRelation);
    }

    [Test]
    [TestCase(true, 1, "aligns")]
    [TestCase(false, -1, "contradicts")]
    public void ArgumentAlignment_CreatesCorrectModifier(bool aligned, int expectedDice, string expectedContains)
    {
        // Arrange & Act
        var modifier = SocialModifier.ArgumentAlignment(aligned, "greed");

        // Assert
        modifier.DiceModifier.Should().Be(expectedDice);
        modifier.Description.Should().Contain(expectedContains);
        modifier.ModifierType.Should().Be(SocialModifierType.ArgumentQuality);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Persuasion);
    }

    [Test]
    public void Evidence_CreatesModifierWithPlus2d10()
    {
        // Arrange & Act
        var modifier = SocialModifier.Evidence("Written confession from the spy");

        // Assert
        modifier.DiceModifier.Should().Be(2);
        modifier.Description.Should().Be("Written confession from the spy");
        modifier.ModifierType.Should().Be(SocialModifierType.Evidence);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Persuasion);
    }

    [Test]
    public void Suspicious_CreatesModifierWithDcPlus4()
    {
        // Arrange & Act
        var modifier = SocialModifier.Suspicious();

        // Assert
        modifier.DcModifier.Should().Be(4);
        modifier.DiceModifier.Should().Be(0);
        modifier.Description.Should().Contain("suspicious");
        modifier.ModifierType.Should().Be(SocialModifierType.TargetState);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Deception);
    }

    [Test]
    public void Trusting_CreatesModifierWithDcMinus4()
    {
        // Arrange & Act
        var modifier = SocialModifier.Trusting();

        // Assert
        modifier.DcModifier.Should().Be(-4);
        modifier.DiceModifier.Should().Be(0);
        modifier.Description.Should().Contain("trusting");
        modifier.ModifierType.Should().Be(SocialModifierType.TargetState);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Deception);
    }

    [Test]
    [TestCase(true, 1, "weaker")]
    [TestCase(false, -1, "stronger")]
    public void StrengthComparison_CreatesCorrectModifier(bool playerStronger, int expectedDice, string expectedContains)
    {
        // Arrange & Act
        var modifier = SocialModifier.StrengthComparison(playerStronger);

        // Assert
        modifier.DiceModifier.Should().Be(expectedDice);
        modifier.Description.Should().Contain(expectedContains);
        modifier.ModifierType.Should().Be(SocialModifierType.TargetState);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Intimidation);
    }

    [Test]
    public void Untrustworthy_CreatesModifierWithDcPlus3()
    {
        // Arrange & Act
        var modifier = SocialModifier.Untrustworthy();

        // Assert
        modifier.DcModifier.Should().Be(3);
        modifier.Description.Should().Contain("Untrustworthy");
        modifier.ModifierType.Should().Be(SocialModifierType.Reputation);
        modifier.AppliesToInteractionTypes.Should().BeNull();
    }

    [Test]
    public void HasBackup_CreatesModifierWithMinus1d10()
    {
        // Arrange & Act
        var modifier = SocialModifier.HasBackup();

        // Assert
        modifier.DiceModifier.Should().Be(-1);
        modifier.Description.Should().Contain("backup");
        modifier.ModifierType.Should().Be(SocialModifierType.TargetState);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Intimidation);
    }

    [Test]
    public void WieldingArtifact_CreatesModifierWithPlus1d10()
    {
        // Arrange & Act
        var modifier = SocialModifier.WieldingArtifact("Shadowbane");

        // Assert
        modifier.DiceModifier.Should().Be(1);
        modifier.Description.Should().Contain("Shadowbane");
        modifier.ModifierType.Should().Be(SocialModifierType.Equipment);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Intimidation);
    }

    [Test]
    public void Reputation_CreatesCorrectModifier()
    {
        // Arrange & Act
        var modifier = SocialModifier.Reputation("Feared", 1, new[] { SocialInteractionType.Intimidation });

        // Assert
        modifier.DiceModifier.Should().Be(1);
        modifier.Description.Should().Contain("Feared");
        modifier.ModifierType.Should().Be(SocialModifierType.Reputation);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Intimidation);
    }

    [Test]
    [TestCase(true, 1)]
    [TestCase(false, -1)]
    public void CulturalKnowledge_CreatesCorrectModifier(bool fluent, int expectedDice)
    {
        // Arrange & Act
        var modifier = SocialModifier.CulturalKnowledge("Dvergr", fluent);

        // Assert
        modifier.DiceModifier.Should().Be(expectedDice);
        modifier.Description.Should().Contain("Dvergr");
        modifier.ModifierType.Should().Be(SocialModifierType.Cultural);
        modifier.AppliesToInteractionTypes.Should().Contain(SocialInteractionType.Protocol);
    }

    #endregion

    #region AppliesTo

    [Test]
    public void AppliesTo_WhenNoTypesSpecified_ReturnsTrue()
    {
        // Arrange
        var modifier = SocialModifier.Untrustworthy(); // No specific types

        // Act & Assert
        modifier.AppliesTo(SocialInteractionType.Persuasion).Should().BeTrue();
        modifier.AppliesTo(SocialInteractionType.Deception).Should().BeTrue();
        modifier.AppliesTo(SocialInteractionType.Intimidation).Should().BeTrue();
    }

    [Test]
    public void AppliesTo_WhenTypesSpecified_FiltersCorrectly()
    {
        // Arrange
        var modifier = SocialModifier.Suspicious(); // Only Deception

        // Act & Assert
        modifier.AppliesTo(SocialInteractionType.Deception).Should().BeTrue();
        modifier.AppliesTo(SocialInteractionType.Persuasion).Should().BeFalse();
        modifier.AppliesTo(SocialInteractionType.Intimidation).Should().BeFalse();
    }

    #endregion

    #region ToShortDescription

    [Test]
    public void ToShortDescription_IncludesDiceModifier()
    {
        // Arrange
        var modifier = SocialModifier.FactionStanding("guild", 2, "Honored");

        // Act
        var description = modifier.ToShortDescription();

        // Assert
        description.Should().Contain("+2d10");
    }

    [Test]
    public void ToShortDescription_IncludesDcModifier()
    {
        // Arrange
        var modifier = SocialModifier.Suspicious();

        // Act
        var description = modifier.ToShortDescription();

        // Assert
        description.Should().Contain("DC +4");
    }

    [Test]
    public void ToShortDescription_NegativeDiceModifier_FormatsCorrectly()
    {
        // Arrange
        var modifier = SocialModifier.HasBackup();

        // Act
        var description = modifier.ToShortDescription();

        // Assert
        description.Should().Contain("-1d10");
    }

    #endregion

    #region Category

    [Test]
    public void Category_ReturnsSocial()
    {
        // Arrange
        var modifier = SocialModifier.FactionStanding("guild", 1, "Friendly");

        // Assert
        modifier.Category.Should().Be(ModifierCategory.Social);
    }

    #endregion
}
