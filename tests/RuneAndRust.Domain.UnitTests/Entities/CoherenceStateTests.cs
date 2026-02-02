namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="CoherenceState"/> entity.
/// Verifies factory method, threshold calculations, spell power bonus, critical cast chance,
/// cascade risk, apotheosis state, and meditation availability.
/// </summary>
[TestFixture]
public class CoherenceStateTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly Guid _characterId = Guid.NewGuid();

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method — Valid Creation
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithDefaultCoherence_ReturnsBalancedThreshold()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId);

        // Assert
        coherenceState.CurrentCoherence.Should().Be(50);
        coherenceState.Threshold.Should().Be(CoherenceThreshold.Balanced);
        coherenceState.SpellPowerBonus.Should().Be(0);
        coherenceState.CriticalCastChance.Should().Be(5);
        coherenceState.CascadeRisk.Should().Be(0);
        coherenceState.InApotheosis.Should().BeFalse();
        coherenceState.CanMeditate.Should().BeTrue();
    }

    [Test]
    public void Create_WithZeroCoherence_ReturnsDestabilizedThreshold()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 0);

        // Assert
        coherenceState.Threshold.Should().Be(CoherenceThreshold.Destabilized);
        coherenceState.CurrentCoherence.Should().Be(0);
        coherenceState.SpellPowerBonus.Should().Be(-2);
        coherenceState.CriticalCastChance.Should().Be(0);
        coherenceState.CascadeRisk.Should().Be(25);
        coherenceState.InApotheosis.Should().BeFalse();
    }

    [Test]
    public void Create_WithMaxCoherence_ReturnsApotheosisState()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 100);

        // Assert
        coherenceState.Threshold.Should().Be(CoherenceThreshold.Apotheosis);
        coherenceState.CurrentCoherence.Should().Be(100);
        coherenceState.SpellPowerBonus.Should().Be(5);
        coherenceState.CriticalCastChance.Should().Be(20);
        coherenceState.CascadeRisk.Should().Be(0);
        coherenceState.InApotheosis.Should().BeTrue();
        coherenceState.ApotheosisAbilitiesUnlocked.Should().BeTrue();
    }

    [Test]
    public void Create_GeneratesCorrectCharacterId()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50);

        // Assert
        coherenceState.CharacterId.Should().Be(_characterId);
    }

    [Test]
    public void Create_WithEmptyCharacterId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => CoherenceState.Create(Guid.Empty, initialCoherence: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("characterId")
            .WithMessage("*CharacterId cannot be empty*");
    }

    [Test]
    public void Create_WithTurnsInApotheosis_SetsProperty()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 90, turnsInApotheosis: 3);

        // Assert
        coherenceState.TurnsInApotheosis.Should().Be(3);
    }

    [Test]
    public void Create_WithLastCastTime_SetsProperty()
    {
        // Arrange
        var lastCastTime = DateTime.UtcNow.AddMinutes(-5);

        // Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50, lastCastTime: lastCastTime);

        // Assert
        coherenceState.LastCastTime.Should().Be(lastCastTime);
    }

    [Test]
    public void Create_WithIsInCombatTrue_SetsIsCombatAndCanMeditateFalse()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50, isInCombat: true);

        // Assert
        coherenceState.IsCombat.Should().BeTrue();
        coherenceState.CanMeditate.Should().BeFalse();
    }

    [Test]
    public void Create_WithApotheosisAbilitiesUnlocked_SetsProperty()
    {
        // Arrange & Act (not in Apotheosis but abilities previously unlocked)
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50, apotheosisAbilitiesUnlocked: true);

        // Assert
        coherenceState.ApotheosisAbilitiesUnlocked.Should().BeTrue();
        coherenceState.InApotheosis.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Threshold Boundary Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(0, CoherenceThreshold.Destabilized)]
    [TestCase(10, CoherenceThreshold.Destabilized)]
    [TestCase(20, CoherenceThreshold.Destabilized)]
    [TestCase(21, CoherenceThreshold.Unstable)]
    [TestCase(30, CoherenceThreshold.Unstable)]
    [TestCase(40, CoherenceThreshold.Unstable)]
    [TestCase(41, CoherenceThreshold.Balanced)]
    [TestCase(50, CoherenceThreshold.Balanced)]
    [TestCase(60, CoherenceThreshold.Balanced)]
    [TestCase(61, CoherenceThreshold.Focused)]
    [TestCase(70, CoherenceThreshold.Focused)]
    [TestCase(80, CoherenceThreshold.Focused)]
    [TestCase(81, CoherenceThreshold.Apotheosis)]
    [TestCase(90, CoherenceThreshold.Apotheosis)]
    [TestCase(100, CoherenceThreshold.Apotheosis)]
    public void Create_AtBoundaryValues_ReturnsCorrectThresholds(int coherence, CoherenceThreshold expected)
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: coherence);

        // Assert
        coherenceState.Threshold.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Spell Power Bonus Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(CoherenceThreshold.Destabilized, -2)]
    [TestCase(CoherenceThreshold.Unstable, -1)]
    [TestCase(CoherenceThreshold.Balanced, 0)]
    [TestCase(CoherenceThreshold.Focused, 2)]
    [TestCase(CoherenceThreshold.Apotheosis, 5)]
    public void SpellPowerBonus_CalculatesCorrectly_ByThreshold(CoherenceThreshold threshold, int expectedBonus)
    {
        // Arrange - Pick a coherence value in the middle of each threshold range
        var coherence = threshold switch
        {
            CoherenceThreshold.Destabilized => 10,
            CoherenceThreshold.Unstable => 30,
            CoherenceThreshold.Balanced => 50,
            CoherenceThreshold.Focused => 70,
            CoherenceThreshold.Apotheosis => 90,
            _ => 0
        };

        // Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: coherence);

        // Assert
        coherenceState.SpellPowerBonus.Should().Be(expectedBonus);
    }

    [Test]
    public void SpellPowerBonus_RangesFromMinus2ToPlus5()
    {
        // Arrange & Act
        var destabilized = CoherenceState.Create(_characterId, initialCoherence: 10);
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);

        // Assert
        destabilized.SpellPowerBonus.Should().Be(-2);
        apotheosis.SpellPowerBonus.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Critical Cast Chance Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(CoherenceThreshold.Destabilized, 0)]
    [TestCase(CoherenceThreshold.Unstable, 0)]
    [TestCase(CoherenceThreshold.Balanced, 5)]
    [TestCase(CoherenceThreshold.Focused, 10)]
    [TestCase(CoherenceThreshold.Apotheosis, 20)]
    public void CriticalCastChance_CalculatesCorrectly_ByThreshold(CoherenceThreshold threshold, int expectedChance)
    {
        // Arrange - Pick a coherence value in the middle of each threshold range
        var coherence = threshold switch
        {
            CoherenceThreshold.Destabilized => 10,
            CoherenceThreshold.Unstable => 30,
            CoherenceThreshold.Balanced => 50,
            CoherenceThreshold.Focused => 70,
            CoherenceThreshold.Apotheosis => 90,
            _ => 0
        };

        // Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: coherence);

        // Assert
        coherenceState.CriticalCastChance.Should().Be(expectedChance);
    }

    [Test]
    public void CriticalCastChance_RangesFrom0To20Percent()
    {
        // Arrange & Act
        var destabilized = CoherenceState.Create(_characterId, initialCoherence: 10);
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);

        // Assert
        destabilized.CriticalCastChance.Should().Be(0);
        apotheosis.CriticalCastChance.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cascade Risk Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(CoherenceThreshold.Destabilized, 25)]
    [TestCase(CoherenceThreshold.Unstable, 10)]
    [TestCase(CoherenceThreshold.Balanced, 0)]
    [TestCase(CoherenceThreshold.Focused, 0)]
    [TestCase(CoherenceThreshold.Apotheosis, 0)]
    public void CascadeRisk_CalculatesCorrectly_ByThreshold(CoherenceThreshold threshold, int expectedRisk)
    {
        // Arrange - Pick a coherence value in the middle of each threshold range
        var coherence = threshold switch
        {
            CoherenceThreshold.Destabilized => 10,
            CoherenceThreshold.Unstable => 30,
            CoherenceThreshold.Balanced => 50,
            CoherenceThreshold.Focused => 70,
            CoherenceThreshold.Apotheosis => 90,
            _ => 0
        };

        // Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: coherence);

        // Assert
        coherenceState.CascadeRisk.Should().Be(expectedRisk);
    }

    [Test]
    public void CascadeRisk_At25PercentForDestabilized()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 10);

        // Assert
        coherenceState.CascadeRisk.Should().Be(25);
    }

    [Test]
    public void CascadeRisk_At10PercentForUnstable()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 30);

        // Assert
        coherenceState.CascadeRisk.Should().Be(10);
    }

    [Test]
    public void CascadeRisk_AtZeroForBalancedAndAbove()
    {
        // Arrange & Act
        var balanced = CoherenceState.Create(_characterId, initialCoherence: 50);
        var focused = CoherenceState.Create(_characterId, initialCoherence: 70);
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);

        // Assert
        balanced.CascadeRisk.Should().Be(0);
        focused.CascadeRisk.Should().Be(0);
        apotheosis.CascadeRisk.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Apotheosis State Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void InApotheosis_TrueOnlyAtApotheosisThreshold()
    {
        // Arrange & Act
        var destabilized = CoherenceState.Create(_characterId, initialCoherence: 10);
        var unstable = CoherenceState.Create(_characterId, initialCoherence: 30);
        var balanced = CoherenceState.Create(_characterId, initialCoherence: 50);
        var focused = CoherenceState.Create(_characterId, initialCoherence: 70);
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);

        // Assert
        destabilized.InApotheosis.Should().BeFalse();
        unstable.InApotheosis.Should().BeFalse();
        balanced.InApotheosis.Should().BeFalse();
        focused.InApotheosis.Should().BeFalse();
        apotheosis.InApotheosis.Should().BeTrue();
    }

    [Test]
    public void InApotheosis_AtBoundary81_IsTrue()
    {
        // Arrange & Act
        var justApotheosis = CoherenceState.Create(_characterId, initialCoherence: 81);
        var justBelowApotheosis = CoherenceState.Create(_characterId, initialCoherence: 80);

        // Assert
        justApotheosis.InApotheosis.Should().BeTrue();
        justBelowApotheosis.InApotheosis.Should().BeFalse();
    }

    [Test]
    public void ApotheosisAbilitiesUnlocked_TrueWhenInApotheosis()
    {
        // Arrange & Act
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);

        // Assert
        apotheosis.ApotheosisAbilitiesUnlocked.Should().BeTrue();
    }

    [Test]
    public void ApotheosisStressCost_Returns10WhenInApotheosis()
    {
        // Arrange & Act
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);
        var notApotheosis = CoherenceState.Create(_characterId, initialCoherence: 50);

        // Assert
        apotheosis.ApotheosisStressCost.Should().Be(10);
        notApotheosis.ApotheosisStressCost.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Meditation Availability Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CanMeditate_TrueWhenNotInCombat()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50, isInCombat: false);

        // Assert
        coherenceState.CanMeditate.Should().BeTrue();
    }

    [Test]
    public void CanMeditate_FalseWhenInCombat()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50, isInCombat: true);

        // Assert
        coherenceState.CanMeditate.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Edge Case Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNegativeCoherence_ClampsToZero()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: -50);

        // Assert
        coherenceState.CurrentCoherence.Should().Be(0);
        coherenceState.Threshold.Should().Be(CoherenceThreshold.Destabilized);
    }

    [Test]
    public void Create_WithCoherenceOver100_ClampsToMaximum()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 150);

        // Assert
        coherenceState.CurrentCoherence.Should().Be(100);
        coherenceState.Threshold.Should().Be(CoherenceThreshold.Apotheosis);
    }

    [Test]
    public void Create_WithNegativeTurnsInApotheosis_ClampsToZero()
    {
        // Arrange & Act
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 90, turnsInApotheosis: -5);

        // Assert
        coherenceState.TurnsInApotheosis.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(-10, CoherenceThreshold.Destabilized)]
    [TestCase(0, CoherenceThreshold.Destabilized)]
    [TestCase(20, CoherenceThreshold.Destabilized)]
    [TestCase(21, CoherenceThreshold.Unstable)]
    [TestCase(40, CoherenceThreshold.Unstable)]
    [TestCase(41, CoherenceThreshold.Balanced)]
    [TestCase(60, CoherenceThreshold.Balanced)]
    [TestCase(61, CoherenceThreshold.Focused)]
    [TestCase(80, CoherenceThreshold.Focused)]
    [TestCase(81, CoherenceThreshold.Apotheosis)]
    [TestCase(100, CoherenceThreshold.Apotheosis)]
    [TestCase(200, CoherenceThreshold.Apotheosis)]
    public void DetermineThreshold_ReturnsCorrectThreshold(int coherence, CoherenceThreshold expected)
    {
        // Arrange & Act
        var threshold = CoherenceState.DetermineThreshold(coherence);

        // Assert
        threshold.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constants Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Constants_HaveCorrectValues()
    {
        // Assert
        CoherenceState.MinCoherence.Should().Be(0);
        CoherenceState.MaxCoherence.Should().Be(100);
        CoherenceState.DefaultCoherence.Should().Be(50);
        CoherenceState.DestabilizedThreshold.Should().Be(20);
        CoherenceState.UnstableThreshold.Should().Be(40);
        CoherenceState.BalancedThreshold.Should().Be(60);
        CoherenceState.FocusedThreshold.Should().Be(80);
        CoherenceState.CascadeRiskDestabilized.Should().Be(25);
        CoherenceState.CascadeRiskUnstable.Should().Be(10);
        CoherenceState.ApotheosisStressCostPerTurn.Should().Be(10);
        CoherenceState.MeditationGain.Should().Be(20);
        CoherenceState.CastGain.Should().Be(5);
        CoherenceState.ChannelGainPerTurn.Should().Be(3);
        CoherenceState.BalancedCritChance.Should().Be(5);
        CoherenceState.FocusedCritChance.Should().Be(10);
        CoherenceState.ApotheosisCritChance.Should().Be(20);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_BalancedState_ReturnsBasicFormat()
    {
        // Arrange
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50);

        // Act
        var result = coherenceState.ToString();

        // Assert
        result.Should().Contain("Coherence[Balanced]");
        result.Should().Contain("50/100");
        result.Should().Contain("Power +0");
        result.Should().Contain("Crit 5%");
        result.Should().NotContain("[CASCADE RISK");
        result.Should().NotContain("[APOTHEOSIS");
        result.Should().NotContain("[COMBAT]");
    }

    [Test]
    public void ToString_DestabilizedState_IncludesCascadeRisk()
    {
        // Arrange
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 10);

        // Act
        var result = coherenceState.ToString();

        // Assert
        result.Should().Contain("Coherence[Destabilized]");
        result.Should().Contain("10/100");
        result.Should().Contain("Power -2");
        result.Should().Contain("[CASCADE RISK 25%]");
    }

    [Test]
    public void ToString_UnstableState_IncludesCascadeRisk()
    {
        // Arrange
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 30);

        // Act
        var result = coherenceState.ToString();

        // Assert
        result.Should().Contain("Coherence[Unstable]");
        result.Should().Contain("[CASCADE RISK 10%]");
    }

    [Test]
    public void ToString_ApotheosisState_IncludesApotheosisIndicator()
    {
        // Arrange
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 90, turnsInApotheosis: 2);

        // Act
        var result = coherenceState.ToString();

        // Assert
        result.Should().Contain("Coherence[Apotheosis]");
        result.Should().Contain("90/100");
        result.Should().Contain("Power +5");
        result.Should().Contain("Crit 20%");
        result.Should().Contain("[APOTHEOSIS Turn 2]");
    }

    [Test]
    public void ToString_InCombat_IncludesCombatIndicator()
    {
        // Arrange
        var coherenceState = CoherenceState.Create(_characterId, initialCoherence: 50, isInCombat: true);

        // Act
        var result = coherenceState.ToString();

        // Assert
        result.Should().Contain("[COMBAT]");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Comprehensive Threshold Progression Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ThresholdProgression_VerifyAllBonusesScale()
    {
        // Verify complete progression from Destabilized to Apotheosis
        var destabilized = CoherenceState.Create(_characterId, initialCoherence: 10);
        var unstable = CoherenceState.Create(_characterId, initialCoherence: 30);
        var balanced = CoherenceState.Create(_characterId, initialCoherence: 50);
        var focused = CoherenceState.Create(_characterId, initialCoherence: 70);
        var apotheosis = CoherenceState.Create(_characterId, initialCoherence: 90);

        // Spell power bonus: -2/-1/0/+2/+5
        destabilized.SpellPowerBonus.Should().Be(-2);
        unstable.SpellPowerBonus.Should().Be(-1);
        balanced.SpellPowerBonus.Should().Be(0);
        focused.SpellPowerBonus.Should().Be(2);
        apotheosis.SpellPowerBonus.Should().Be(5);

        // Critical cast chance: 0/0/5/10/20
        destabilized.CriticalCastChance.Should().Be(0);
        unstable.CriticalCastChance.Should().Be(0);
        balanced.CriticalCastChance.Should().Be(5);
        focused.CriticalCastChance.Should().Be(10);
        apotheosis.CriticalCastChance.Should().Be(20);

        // Cascade risk: 25/10/0/0/0
        destabilized.CascadeRisk.Should().Be(25);
        unstable.CascadeRisk.Should().Be(10);
        balanced.CascadeRisk.Should().Be(0);
        focused.CascadeRisk.Should().Be(0);
        apotheosis.CascadeRisk.Should().Be(0);

        // InApotheosis: false/false/false/false/true
        destabilized.InApotheosis.Should().BeFalse();
        unstable.InApotheosis.Should().BeFalse();
        balanced.InApotheosis.Should().BeFalse();
        focused.InApotheosis.Should().BeFalse();
        apotheosis.InApotheosis.Should().BeTrue();
    }
}
