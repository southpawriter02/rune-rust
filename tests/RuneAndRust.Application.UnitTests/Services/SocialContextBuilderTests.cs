namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SocialContextBuilder"/> service.
/// </summary>
[TestFixture]
public class SocialContextBuilderTests
{
    private SocialContextBuilder _builder = null!;

    [SetUp]
    public void SetUp()
    {
        _builder = new SocialContextBuilder();
    }

    #region Basic Building

    [Test]
    public void Build_WithMinimalConfiguration_Succeeds()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .Build();

        // Assert
        context.TargetId.Should().Be("npc-001");
        context.InteractionType.Should().Be(SocialInteractionType.Persuasion);
        context.TargetDisposition.Category.Should().Be(NpcDisposition.Neutral);
    }

    [Test]
    public void Build_WithoutTarget_ThrowsException()
    {
        // Arrange
        _builder.WithInteractionType(SocialInteractionType.Persuasion);

        // Act
        var act = () => _builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Target ID*");
    }

    [Test]
    public void Build_WithBaseDc_SetsCorrectDc()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithBaseDc(15)
            .Build();

        // Assert
        context.BaseDc.Should().Be(15);
        context.EffectiveDc.Should().Be(15);
    }

    #endregion

    #region Disposition

    [Test]
    public void Build_WithFriendlyTarget_HasPositiveDiceModifier()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.Create(60))
            .Build();

        // Assert
        context.DispositionDiceModifier.Should().Be(2);
        context.TotalDiceModifier.Should().Be(2);
    }

    [Test]
    public void Build_WithHostileTarget_HasNegativeDiceModifier()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.Create(-60))
            .Build();

        // Assert
        context.DispositionDiceModifier.Should().Be(-2);
        context.TotalDiceModifier.Should().Be(-2);
    }

    #endregion

    #region Faction Standing

    [Test]
    public void WithFaction_HonoredStanding_AddsPositiveModifier()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithFaction("merchants-guild", 80)
            .Build();

        // Assert
        context.SocialModifiers.Should().HaveCount(1);
        context.SocialDiceModifier.Should().Be(1);
        context.TargetFactionId.Should().Be("merchants-guild");
    }

    [Test]
    public void WithFaction_HostileStanding_AddsNegativeModifier()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithFaction("thieves-guild", -80)
            .Build();

        // Assert
        context.SocialModifiers.Should().HaveCount(1);
        context.SocialDiceModifier.Should().Be(-2);
    }

    [Test]
    public void WithFaction_NeutralStanding_AddsNoModifier()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithFaction("neutral-guild", 0)
            .Build();

        // Assert
        context.SocialModifiers.Should().BeEmpty();
        context.TargetFactionId.Should().Be("neutral-guild");
    }

    #endregion

    #region Social Modifiers

    [Test]
    public void TargetIsSuspicious_AddsDcModifier()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Deception)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .TargetIsSuspicious()
            .Build();

        // Assert
        context.SocialDcModifier.Should().Be(4);
        context.EffectiveDc.Should().Be(6); // 2 + 4
    }

    [Test]
    public void TargetIsTrusting_ReducesDc()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Deception)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithBaseDc(10)
            .TargetIsTrusting()
            .Build();

        // Assert
        context.SocialDcModifier.Should().Be(-4);
        context.EffectiveDc.Should().Be(6); // 10 - 4
    }

    [Test]
    public void WithArgumentAlignment_Aligned_AddsBonus()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithArgumentAlignment(true, "appeals to greed")
            .Build();

        // Assert
        context.SocialDiceModifier.Should().Be(1);
    }

    [Test]
    public void WithEvidence_AddsBonusDice()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithEvidence("Signed confession")
            .Build();

        // Assert
        context.SocialDiceModifier.Should().Be(2);
    }

    [Test]
    public void WithStrengthComparison_Stronger_AddsBonusToIntimidation()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Intimidation)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithStrengthComparison(true)
            .Build();

        // Assert
        context.SocialDiceModifier.Should().Be(1);
    }

    [Test]
    public void TargetHasBackup_AddsPenalty()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Intimidation)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .TargetHasBackup()
            .Build();

        // Assert
        context.SocialDiceModifier.Should().Be(-1);
    }

    [Test]
    public void WieldingArtifact_AddsBonusToIntimidation()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Intimidation)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WieldingArtifact("Shadowbane")
            .Build();

        // Assert
        context.SocialDiceModifier.Should().Be(1);
    }

    [Test]
    public void IsUntrustworthy_AddsDcToAllInteractions()
    {
        // Arrange & Act
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .IsUntrustworthy()
            .Build();

        // Assert
        context.SocialDcModifier.Should().Be(3);
    }

    #endregion

    #region Modifier Filtering

    [Test]
    public void Build_FiltersModifiersByInteractionType()
    {
        // Arrange - Add both a Persuasion-specific and a Deception-specific modifier
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("npc-001", DispositionLevel.CreateNeutral())
            .WithEvidence("Written proof") // Persuasion only
            .TargetIsSuspicious() // Deception only
            .Build();

        // Assert - Only the Evidence modifier should apply
        context.SocialModifiers.Should().HaveCount(1);
        context.SocialDiceModifier.Should().Be(2); // Evidence only
        context.SocialDcModifier.Should().Be(0); // Suspicious filtered out
    }

    #endregion

    #region Reset

    [Test]
    public void Reset_ClearsAllState()
    {
        // Arrange
        _builder
            .WithInteractionType(SocialInteractionType.Intimidation)
            .WithTarget("npc-001", DispositionLevel.Create(80))
            .WithFaction("guild", 90)
            .WithBaseDc(15);

        // Act
        _builder.Reset();

        // Assert - Building should fail without target
        var act = () => _builder.Build();
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Reset_AllowsRebuildingFromScratch()
    {
        // Arrange - Build first context
        var context1 = _builder
            .WithInteractionType(SocialInteractionType.Intimidation)
            .WithTarget("npc-001", DispositionLevel.Create(80))
            .Build();

        // Act - Reset and build different context
        var context2 = _builder
            .Reset()
            .WithInteractionType(SocialInteractionType.Deception)
            .WithTarget("npc-002", DispositionLevel.Create(-30))
            .Build();

        // Assert
        context1.TargetId.Should().Be("npc-001");
        context2.TargetId.Should().Be("npc-002");
        context2.InteractionType.Should().Be(SocialInteractionType.Deception);
    }

    #endregion

    #region Complex Scenarios

    [Test]
    public void Build_ComplexScenario_AggregatesAllModifiers()
    {
        // Arrange - A complex social situation
        var context = _builder
            .WithInteractionType(SocialInteractionType.Persuasion)
            .WithTarget("noble-001", DispositionLevel.Create(30)) // +1d10 NeutralPositive
            .WithFaction("noble-house", 80) // +1d10 Honored
            .WithBaseDc(15)
            .WithArgumentAlignment(true, "appeals to honor") // +1d10
            .WithEvidence("Royal decree") // +2d10
            .WithReputation("Hero of the Realm", 1) // +1d10
            .Build();

        // Assert
        context.TotalDiceModifier.Should().Be(6); // 1+1+1+2+1 = 6
        context.EffectiveDc.Should().Be(15);
        context.ModifierCount.Should().Be(5); // disposition + faction + alignment + evidence + reputation
    }

    #endregion
}
