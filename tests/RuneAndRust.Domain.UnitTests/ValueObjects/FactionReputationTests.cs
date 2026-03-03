using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="FactionReputation"/> value object.
/// Covers factory methods, tier derivation, clamping, price modifiers,
/// and all tier boundary values per SPEC-REPUTATION-001.
/// </summary>
[TestFixture]
public class FactionReputationTests
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Neutral_CreatesReputationAtZero()
    {
        // Act
        var rep = FactionReputation.Neutral("iron-banes");

        // Assert
        rep.FactionId.Should().Be("iron-banes");
        rep.Value.Should().Be(0);
        rep.Tier.Should().Be(ReputationTier.Neutral);
        rep.PriceModifier.Should().Be(1.00);
    }

    [Test]
    public void Neutral_ThrowsOnNullFactionId()
    {
        // Act & Assert
        FluentActions.Invoking(() => FactionReputation.Neutral(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Create_SetsValueAndDerivesTier()
    {
        // Act
        var rep = FactionReputation.Create("rust-clans", 50);

        // Assert
        rep.FactionId.Should().Be("rust-clans");
        rep.Value.Should().Be(50);
        rep.Tier.Should().Be(ReputationTier.Allied);
    }

    [Test]
    public void Create_ThrowsOnNullFactionId()
    {
        // Act & Assert
        FluentActions.Invoking(() => FactionReputation.Create(null!, 50))
            .Should().Throw<ArgumentNullException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLAMPING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_ClampsValueAboveMax()
    {
        // Act
        var rep = FactionReputation.Create("iron-banes", 200);

        // Assert
        rep.Value.Should().Be(FactionReputation.MaxValue); // 100
        rep.Tier.Should().Be(ReputationTier.Exalted);
    }

    [Test]
    public void Create_ClampsValueBelowMin()
    {
        // Act
        var rep = FactionReputation.Create("iron-banes", -200);

        // Assert
        rep.Value.Should().Be(FactionReputation.MinValue); // -100
        rep.Tier.Should().Be(ReputationTier.Hated);
    }

    [Test]
    public void WithDelta_ClampsAtMaxValue()
    {
        // Arrange
        var rep = FactionReputation.Create("iron-banes", 95);

        // Act — adding 20 would exceed +100
        var newRep = rep.WithDelta(20);

        // Assert
        newRep.Value.Should().Be(100);
    }

    [Test]
    public void WithDelta_ClampsAtMinValue()
    {
        // Arrange
        var rep = FactionReputation.Create("iron-banes", -95);

        // Act — subtracting 20 would go below -100
        var newRep = rep.WithDelta(-20);

        // Assert
        newRep.Value.Should().Be(-100);
    }

    // ═══════════════════════════════════════════════════════════════
    // IMMUTABILITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WithDelta_ReturnsNewInstance_DoesNotMutateOriginal()
    {
        // Arrange
        var original = FactionReputation.Create("iron-banes", 0);

        // Act
        var modified = original.WithDelta(50);

        // Assert — original unchanged
        original.Value.Should().Be(0);
        original.Tier.Should().Be(ReputationTier.Neutral);

        // Assert — new instance has new value
        modified.Value.Should().Be(50);
        modified.Tier.Should().Be(ReputationTier.Allied);
    }

    // ═══════════════════════════════════════════════════════════════
    // TIER BOUNDARY TESTS (design doc v1.2, Section 2)
    //
    // These tests verify every boundary value listed in the acceptance
    // criteria (AC #17): -100, -76, -75, -26, -25, +24, +25, +49,
    // +50, +74, +75, +100
    // ═══════════════════════════════════════════════════════════════

    [TestCase(-100, ReputationTier.Hated)]
    [TestCase(-76, ReputationTier.Hated)]
    [TestCase(-75, ReputationTier.Hostile)]
    [TestCase(-26, ReputationTier.Hostile)]
    [TestCase(-25, ReputationTier.Neutral)]
    [TestCase(0, ReputationTier.Neutral)]
    [TestCase(24, ReputationTier.Neutral)]
    [TestCase(25, ReputationTier.Friendly)]
    [TestCase(49, ReputationTier.Friendly)]
    [TestCase(50, ReputationTier.Allied)]
    [TestCase(74, ReputationTier.Allied)]
    [TestCase(75, ReputationTier.Exalted)]
    [TestCase(100, ReputationTier.Exalted)]
    public void GetTierForValue_ReturnsCorrectTier(int value, ReputationTier expectedTier)
    {
        // Act
        var tier = FactionReputation.GetTierForValue(value);

        // Assert
        tier.Should().Be(expectedTier);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRICE MODIFIER TESTS
    // ═══════════════════════════════════════════════════════════════

    [TestCase(ReputationTier.Hated, 1.50)]
    [TestCase(ReputationTier.Hostile, 1.25)]
    [TestCase(ReputationTier.Neutral, 1.00)]
    [TestCase(ReputationTier.Friendly, 0.90)]
    [TestCase(ReputationTier.Allied, 0.80)]
    [TestCase(ReputationTier.Exalted, 0.70)]
    public void GetPriceModifierForTier_ReturnsCorrectModifier(ReputationTier tier, double expectedModifier)
    {
        // Act
        var modifier = FactionReputation.GetPriceModifierForTier(tier);

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    [Test]
    public void PriceModifier_DerivedFromTier()
    {
        // Arrange & Act
        var hated = FactionReputation.Create("test", -100);
        var neutral = FactionReputation.Create("test", 0);
        var exalted = FactionReputation.Create("test", 100);

        // Assert
        hated.PriceModifier.Should().Be(1.50);
        neutral.PriceModifier.Should().Be(1.00);
        exalted.PriceModifier.Should().Be(0.70);
    }

    // ═══════════════════════════════════════════════════════════════
    // TIER TRANSITION VIA DELTA TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WithDelta_CausesTierTransition_NeutralToFriendly()
    {
        // Arrange — start at 20 (Neutral, near boundary)
        var rep = FactionReputation.Create("iron-banes", 20);
        rep.Tier.Should().Be(ReputationTier.Neutral);

        // Act — +10 pushes to 30 (Friendly)
        var newRep = rep.WithDelta(10);

        // Assert
        newRep.Value.Should().Be(30);
        newRep.Tier.Should().Be(ReputationTier.Friendly);
    }

    [Test]
    public void WithDelta_CausesTierTransition_NeutralToHostile()
    {
        // Arrange — start at -20 (Neutral, near boundary)
        var rep = FactionReputation.Create("iron-banes", -20);
        rep.Tier.Should().Be(ReputationTier.Neutral);

        // Act — -10 pushes to -30 (Hostile)
        var newRep = rep.WithDelta(-10);

        // Assert
        newRep.Value.Should().Be(-30);
        newRep.Tier.Should().Be(ReputationTier.Hostile);
    }

    // ═══════════════════════════════════════════════════════════════
    // WITNESS CONTEXT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void WitnessContext_Direct_HasFullMultiplier()
    {
        var ctx = WitnessContext.Direct;
        ctx.Type.Should().Be(WitnessType.Direct);
        ctx.Multiplier.Should().Be(1.0);
    }

    [Test]
    public void WitnessContext_Witnessed_Has75PercentMultiplier()
    {
        var ctx = WitnessContext.Witnessed;
        ctx.Type.Should().Be(WitnessType.Witnessed);
        ctx.Multiplier.Should().Be(0.75);
    }

    [Test]
    public void WitnessContext_Unwitnessed_HasZeroMultiplier()
    {
        var ctx = WitnessContext.Unwitnessed;
        ctx.Type.Should().Be(WitnessType.Unwitnessed);
        ctx.Multiplier.Should().Be(0.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // ToString TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_FormatsCorrectly_PositiveValue()
    {
        var rep = FactionReputation.Create("iron-banes", 35);
        rep.ToString().Should().Be("iron-banes: Friendly (+35)");
    }

    [Test]
    public void ToString_FormatsCorrectly_NegativeValue()
    {
        var rep = FactionReputation.Create("god-sleeper-cultists", -50);
        rep.ToString().Should().Be("god-sleeper-cultists: Hostile (-50)");
    }

    [Test]
    public void ToString_FormatsCorrectly_ZeroValue()
    {
        var rep = FactionReputation.Create("rust-clans", 0);
        rep.ToString().Should().Be("rust-clans: Neutral (0)");
    }
}
