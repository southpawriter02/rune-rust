namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SocialContext"/> value object.
/// </summary>
[TestFixture]
public class SocialContextTests
{
    #region CreateMinimal

    [Test]
    public void CreateMinimal_ReturnsValidContext()
    {
        // Arrange & Act
        var context = SocialContext.CreateMinimal("npc-001");

        // Assert
        context.TargetId.Should().Be("npc-001");
        context.InteractionType.Should().Be(SocialInteractionType.Persuasion);
        context.TargetDisposition.Category.Should().Be(NpcDisposition.Neutral);
        context.BaseDc.Should().Be(12);
        context.SocialModifiers.Should().BeEmpty();
    }

    [Test]
    public void CreateMinimal_WithInteractionType_SetsCorrectType()
    {
        // Arrange & Act
        var context = SocialContext.CreateMinimal("npc-001", SocialInteractionType.Intimidation);

        // Assert
        context.InteractionType.Should().Be(SocialInteractionType.Intimidation);
    }

    #endregion

    #region Modifier Aggregation

    [Test]
    public void TotalDiceModifier_AggregatesAllSources()
    {
        // Arrange
        var disposition = DispositionLevel.Create(60); // +2d10 from Friendly
        var socialMods = new List<SocialModifier>
        {
            SocialModifier.FactionStanding("guild", 1, "Honored"),
            SocialModifier.ArgumentAlignment(true)
        };
        var equipmentMods = new List<EquipmentModifier>
        {
            EquipmentModifier.Tool("evidence", "Written Evidence", 1, false)
        };
        var situationalMods = new List<SituationalModifier>
        {
            SituationalModifier.Assisted(1, "Ally")
        };


        var context = new SocialContext(
            InteractionType: SocialInteractionType.Persuasion,
            TargetId: "npc-001",
            TargetDisposition: disposition,
            TargetFactionId: "guild",
            CultureId: null,
            BaseDc: 12,
            SocialModifiers: socialMods.AsReadOnly(),
            EquipmentModifiers: equipmentMods.AsReadOnly(),
            SituationalModifiers: situationalMods.AsReadOnly(),
            AppliedStatuses: Array.Empty<string>());

        // Assert
        context.DispositionDiceModifier.Should().Be(2);
        context.SocialDiceModifier.Should().Be(2); // 1 + 1
        context.EquipmentDiceModifier.Should().Be(1);
        context.SituationalDiceModifier.Should().Be(1);
        context.TotalDiceModifier.Should().Be(6); // 2 + 2 + 1 + 1
    }

    [Test]
    public void TotalDcModifier_AggregatesAllSources()
    {
        // Arrange
        var socialMods = new List<SocialModifier>
        {
            SocialModifier.Suspicious(), // DC +4
            SocialModifier.Untrustworthy() // DC +3
        };

        var context = new SocialContext(
            InteractionType: SocialInteractionType.Deception,
            TargetId: "npc-001",
            TargetDisposition: DispositionLevel.CreateNeutral(),
            TargetFactionId: null,
            CultureId: null,
            BaseDc: 10,
            SocialModifiers: socialMods.AsReadOnly(),
            EquipmentModifiers: Array.Empty<EquipmentModifier>(),
            SituationalModifiers: Array.Empty<SituationalModifier>(),
            AppliedStatuses: Array.Empty<string>());

        // Assert
        context.SocialDcModifier.Should().Be(7); // 4 + 3
        context.TotalDcModifier.Should().Be(7);
        context.EffectiveDc.Should().Be(17); // 10 + 7
    }

    [Test]
    public void EffectiveDc_ClampedToMinimum1()
    {
        // Arrange
        var socialMods = new List<SocialModifier>
        {
            SocialModifier.Trusting() // DC -4
        };

        var context = new SocialContext(
            InteractionType: SocialInteractionType.Deception,
            TargetId: "npc-001",
            TargetDisposition: DispositionLevel.CreateNeutral(),
            TargetFactionId: null,
            CultureId: null,
            BaseDc: 2, // Very low base
            SocialModifiers: socialMods.AsReadOnly(),
            EquipmentModifiers: Array.Empty<EquipmentModifier>(),
            SituationalModifiers: Array.Empty<SituationalModifier>(),
            AppliedStatuses: Array.Empty<string>());

        // Assert
        context.TotalDcModifier.Should().Be(-4);
        context.EffectiveDc.Should().Be(1); // Clamped to min 1
    }

    #endregion

    #region Interaction Type Properties

    [Test]
    [TestCase(SocialInteractionType.Deception, true)]
    [TestCase(SocialInteractionType.Interrogation, true)]
    [TestCase(SocialInteractionType.Persuasion, false)]
    [TestCase(SocialInteractionType.Intimidation, false)]
    public void IsOpposed_ReflectsInteractionType(SocialInteractionType type, bool expectedOpposed)
    {
        // Arrange
        var context = SocialContext.CreateMinimal("npc-001", type);

        // Assert
        context.IsOpposed.Should().Be(expectedOpposed);
    }

    [Test]
    [TestCase(SocialInteractionType.Deception, true)]
    [TestCase(SocialInteractionType.Persuasion, false)]
    public void HasStressCost_ReflectsInteractionType(SocialInteractionType type, bool expectedStress)
    {
        // Arrange
        var context = SocialContext.CreateMinimal("npc-001", type);

        // Assert
        context.HasStressCost.Should().Be(expectedStress);
    }

    [Test]
    [TestCase(SocialInteractionType.Intimidation, true)]
    [TestCase(SocialInteractionType.Persuasion, false)]
    public void AlwaysCostsReputation_ReflectsInteractionType(SocialInteractionType type, bool expectedCost)
    {
        // Arrange
        var context = SocialContext.CreateMinimal("npc-001", type);

        // Assert
        context.AlwaysCostsReputation.Should().Be(expectedCost);
    }

    [Test]
    [TestCase(SocialInteractionType.Negotiation, true)]
    [TestCase(SocialInteractionType.Interrogation, true)]
    [TestCase(SocialInteractionType.Persuasion, false)]
    public void IsMultiRound_ReflectsInteractionType(SocialInteractionType type, bool expectedMultiRound)
    {
        // Arrange
        var context = SocialContext.CreateMinimal("npc-001", type);

        // Assert
        context.IsMultiRound.Should().Be(expectedMultiRound);
    }

    #endregion

    #region HasModifiers

    [Test]
    public void HasModifiers_FalseWhenEmpty()
    {
        // Arrange
        var context = SocialContext.CreateMinimal("npc-001");

        // Assert
        context.HasModifiers.Should().BeFalse();
        context.ModifierCount.Should().Be(0);
    }

    [Test]
    public void HasModifiers_TrueWhenDispositionNonZero()
    {
        // Arrange
        var context = new SocialContext(
            InteractionType: SocialInteractionType.Persuasion,
            TargetId: "npc-001",
            TargetDisposition: DispositionLevel.Create(60), // +2d10
            TargetFactionId: null,
            CultureId: null,
            BaseDc: 12,
            SocialModifiers: Array.Empty<SocialModifier>(),
            EquipmentModifiers: Array.Empty<EquipmentModifier>(),
            SituationalModifiers: Array.Empty<SituationalModifier>(),
            AppliedStatuses: Array.Empty<string>());

        // Assert
        context.HasModifiers.Should().BeTrue();
        context.ModifierCount.Should().Be(1); // Just disposition
    }

    #endregion

    #region FumbleType

    [Test]
    [TestCase(SocialInteractionType.Persuasion, FumbleType.TrustShattered)]
    [TestCase(SocialInteractionType.Deception, FumbleType.LieExposed)]
    [TestCase(SocialInteractionType.Intimidation, FumbleType.ChallengeAccepted)]
    public void FumbleType_ReflectsInteractionType(SocialInteractionType type, FumbleType expectedFumble)
    {
        // Arrange
        var context = SocialContext.CreateMinimal("npc-001", type);

        // Assert
        context.FumbleType.Should().Be(expectedFumble);
    }

    #endregion

    #region ToModifierBreakdown

    [Test]
    public void ToModifierBreakdown_IncludesAllComponents()
    {
        // Arrange
        var context = new SocialContext(
            InteractionType: SocialInteractionType.Persuasion,
            TargetId: "npc-001",
            TargetDisposition: DispositionLevel.Create(60),
            TargetFactionId: "guild",
            CultureId: null,
            BaseDc: 12,
            SocialModifiers: new[] { SocialModifier.ArgumentAlignment(true) },
            EquipmentModifiers: Array.Empty<EquipmentModifier>(),
            SituationalModifiers: Array.Empty<SituationalModifier>(),
            AppliedStatuses: Array.Empty<string>());

        // Act
        var breakdown = context.ToModifierBreakdown();

        // Assert
        breakdown.Should().Contain("Honest convincing"); // Description of Persuasion
        breakdown.Should().Contain("npc-001");
        breakdown.Should().Contain("Friendly");
        breakdown.Should().Contain("Base DC: 12");
        breakdown.Should().Contain("Disposition: +2d10");
        breakdown.Should().Contain("Effective DC: 12");

    }

    #endregion
}
