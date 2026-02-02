namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="RageState"/> entity.
/// Verifies factory method, threshold calculations, damage bonus, soak bonus,
/// and special effects at various rage levels.
/// </summary>
[TestFixture]
public class RageStateTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly Guid _characterId = Guid.NewGuid();

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method — Valid Creation
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithZeroRage_ReturnsCalmThreshold()
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: 0);

        // Assert
        rageState.Threshold.Should().Be(RageThreshold.Calm);
        rageState.CurrentRage.Should().Be(0);
        rageState.DamageBonus.Should().Be(0);
        rageState.SoakBonus.Should().Be(0);
        rageState.FearImmune.Should().BeFalse();
        rageState.MustAttackNearest.Should().BeFalse();
        rageState.PartyStressReduction.Should().BeNull();
    }

    [Test]
    public void Create_WithMaxRage_ReturnsFrenzyBeyondReasonState()
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: 100);

        // Assert
        rageState.Threshold.Should().Be(RageThreshold.FrenzyBeyondReason);
        rageState.CurrentRage.Should().Be(100);
        rageState.DamageBonus.Should().Be(10);
        rageState.SoakBonus.Should().Be(4);
        rageState.FearImmune.Should().BeTrue();
        rageState.MustAttackNearest.Should().BeTrue();
        rageState.PartyStressReduction.Should().Be(10);
    }

    [Test]
    public void Create_GeneratesCorrectCharacterId()
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: 50);

        // Assert
        rageState.CharacterId.Should().Be(_characterId);
    }

    [Test]
    public void Create_WithEmptyCharacterId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => RageState.Create(Guid.Empty, initialRage: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("characterId")
            .WithMessage("*CharacterId cannot be empty*");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Threshold Boundary Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(0, RageThreshold.Calm)]
    [TestCase(10, RageThreshold.Calm)]
    [TestCase(20, RageThreshold.Calm)]
    [TestCase(21, RageThreshold.Simmering)]
    [TestCase(30, RageThreshold.Simmering)]
    [TestCase(40, RageThreshold.Simmering)]
    [TestCase(41, RageThreshold.Burning)]
    [TestCase(50, RageThreshold.Burning)]
    [TestCase(60, RageThreshold.Burning)]
    [TestCase(61, RageThreshold.BerserkFury)]
    [TestCase(70, RageThreshold.BerserkFury)]
    [TestCase(80, RageThreshold.BerserkFury)]
    [TestCase(81, RageThreshold.FrenzyBeyondReason)]
    [TestCase(90, RageThreshold.FrenzyBeyondReason)]
    [TestCase(100, RageThreshold.FrenzyBeyondReason)]
    public void Create_AtBoundaryValues_ReturnsCorrectThresholds(int rage, RageThreshold expected)
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: rage);

        // Assert
        rageState.Threshold.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Damage Bonus Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(0, 0)]
    [TestCase(5, 0)]
    [TestCase(9, 0)]
    [TestCase(10, 1)]
    [TestCase(15, 1)]
    [TestCase(25, 2)]
    [TestCase(50, 5)]
    [TestCase(99, 9)]
    [TestCase(100, 10)]
    public void DamageBonus_CalculatesCorrectly_ForAllRanges(int rage, int expectedBonus)
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: rage);

        // Assert
        rageState.DamageBonus.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Soak Bonus Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(RageThreshold.Calm, 0)]
    [TestCase(RageThreshold.Simmering, 1)]
    [TestCase(RageThreshold.Burning, 2)]
    [TestCase(RageThreshold.BerserkFury, 3)]
    [TestCase(RageThreshold.FrenzyBeyondReason, 4)]
    public void SoakBonus_ScalesWithThreshold_Correctly(RageThreshold threshold, int expectedSoak)
    {
        // Arrange - Pick a rage value in the middle of each threshold range
        var rage = threshold switch
        {
            RageThreshold.Calm => 10,
            RageThreshold.Simmering => 30,
            RageThreshold.Burning => 50,
            RageThreshold.BerserkFury => 70,
            RageThreshold.FrenzyBeyondReason => 90,
            _ => 0
        };

        // Act
        var rageState = RageState.Create(_characterId, initialRage: rage);

        // Assert
        rageState.SoakBonus.Should().Be(expectedSoak);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Damage Taken Reduction Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(0, 0)]
    [TestCase(19, 0)]
    [TestCase(20, 5)]
    [TestCase(39, 5)]
    [TestCase(40, 10)]
    [TestCase(59, 10)]
    [TestCase(60, 15)]
    [TestCase(79, 15)]
    [TestCase(80, 20)]
    [TestCase(99, 20)]
    [TestCase(100, 25)]
    public void DamageTakenReduction_CalculatesCorrectly(int rage, int expectedReduction)
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: rage);

        // Assert
        rageState.DamageTakenReduction.Should().Be(expectedReduction);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Special Effects Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void FearImmune_OnlyTrueAtFrenzyBeyondReason()
    {
        // Arrange & Act
        var calmState = RageState.Create(_characterId, initialRage: 10);
        var simmeringState = RageState.Create(_characterId, initialRage: 30);
        var burningState = RageState.Create(_characterId, initialRage: 50);
        var berserkState = RageState.Create(_characterId, initialRage: 70);
        var frenzyState = RageState.Create(_characterId, initialRage: 90);

        // Assert
        calmState.FearImmune.Should().BeFalse();
        simmeringState.FearImmune.Should().BeFalse();
        burningState.FearImmune.Should().BeFalse();
        berserkState.FearImmune.Should().BeFalse();
        frenzyState.FearImmune.Should().BeTrue();
    }

    [Test]
    public void MustAttackNearest_TrueAtBerserkFuryAndAbove()
    {
        // Arrange & Act
        var calmState = RageState.Create(_characterId, initialRage: 10);
        var simmeringState = RageState.Create(_characterId, initialRage: 30);
        var burningState = RageState.Create(_characterId, initialRage: 50);
        var berserkState = RageState.Create(_characterId, initialRage: 70);
        var frenzyState = RageState.Create(_characterId, initialRage: 90);

        // Assert
        calmState.MustAttackNearest.Should().BeFalse();
        simmeringState.MustAttackNearest.Should().BeFalse();
        burningState.MustAttackNearest.Should().BeFalse();
        berserkState.MustAttackNearest.Should().BeTrue();
        frenzyState.MustAttackNearest.Should().BeTrue();
    }

    [Test]
    public void PartyStressReduction_OnlyAtFrenzyBeyondReason()
    {
        // Arrange & Act
        var berserkState = RageState.Create(_characterId, initialRage: 70);
        var frenzyState = RageState.Create(_characterId, initialRage: 90);

        // Assert
        berserkState.PartyStressReduction.Should().BeNull();
        frenzyState.PartyStressReduction.Should().Be(10);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Edge Case Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNegativeRage_ClampsToZero()
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: -50);

        // Assert
        rageState.CurrentRage.Should().Be(0);
        rageState.Threshold.Should().Be(RageThreshold.Calm);
    }

    [Test]
    public void Create_WithRageOver100_ClampsToMaximum()
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: 150);

        // Assert
        rageState.CurrentRage.Should().Be(100);
        rageState.Threshold.Should().Be(RageThreshold.FrenzyBeyondReason);
    }

    [Test]
    public void Create_WithLastCombatTime_SetsProperty()
    {
        // Arrange
        var lastCombatTime = DateTime.UtcNow.AddMinutes(-5);

        // Act
        var rageState = RageState.Create(_characterId, initialRage: 50, lastCombatTime: lastCombatTime);

        // Assert
        rageState.LastCombatTime.Should().Be(lastCombatTime);
    }

    [Test]
    public void DecayPerTurn_ReturnsConstant()
    {
        // Arrange & Act
        var rageState = RageState.Create(_characterId, initialRage: 50);

        // Assert
        rageState.DecayPerTurn.Should().Be(RageState.DecayPerNonCombatTurn);
        rageState.DecayPerTurn.Should().Be(10);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(-10, RageThreshold.Calm)]
    [TestCase(0, RageThreshold.Calm)]
    [TestCase(20, RageThreshold.Calm)]
    [TestCase(21, RageThreshold.Simmering)]
    [TestCase(80, RageThreshold.BerserkFury)]
    [TestCase(81, RageThreshold.FrenzyBeyondReason)]
    [TestCase(200, RageThreshold.FrenzyBeyondReason)]
    public void DetermineThreshold_ReturnsCorrectThreshold(int rage, RageThreshold expected)
    {
        // Arrange & Act
        var threshold = RageState.DetermineThreshold(rage);

        // Assert
        threshold.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_CalmState_ReturnsBasicFormat()
    {
        // Arrange
        var rageState = RageState.Create(_characterId, initialRage: 10);

        // Act
        var result = rageState.ToString();

        // Assert
        result.Should().Contain("Rage[Calm]");
        result.Should().Contain("10/100");
        result.Should().Contain("DMG +1");
        result.Should().Contain("SOAK +0");
        result.Should().NotContain("[FEAR IMMUNE]");
        result.Should().NotContain("[MUST ATTACK]");
    }

    [Test]
    public void ToString_FrenzyState_IncludesSpecialEffects()
    {
        // Arrange
        var rageState = RageState.Create(_characterId, initialRage: 90);

        // Act
        var result = rageState.ToString();

        // Assert
        result.Should().Contain("Rage[FrenzyBeyondReason]");
        result.Should().Contain("[FEAR IMMUNE]");
        result.Should().Contain("[MUST ATTACK]");
    }
}
