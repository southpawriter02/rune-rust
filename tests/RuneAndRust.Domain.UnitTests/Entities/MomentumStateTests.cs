namespace RuneAndRust.Domain.UnitTests.Entities;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="MomentumState"/> entity.
/// Verifies factory method, threshold calculations, bonus attacks, movement bonus,
/// attack/defense bonuses, critical chance, and consecutive hit tracking.
/// </summary>
[TestFixture]
public class MomentumStateTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Test Data
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly Guid _characterId = Guid.NewGuid();

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method — Valid Creation
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithZeroMomentum_ReturnsStationaryThreshold()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 0);

        // Assert
        momentumState.Threshold.Should().Be(MomentumThreshold.Stationary);
        momentumState.CurrentMomentum.Should().Be(0);
        momentumState.BonusAttacks.Should().Be(0);
        momentumState.AttackBonus.Should().Be(0);
        momentumState.DefenseBonus.Should().Be(0);
        momentumState.MovementBonus.Should().Be(0);
        momentumState.CriticalChance.Should().BeNull();
        momentumState.ConsecutiveHits.Should().Be(0);
    }

    [Test]
    public void Create_WithMaxMomentum_ReturnsUnstoppableState()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 100);

        // Assert
        momentumState.Threshold.Should().Be(MomentumThreshold.Unstoppable);
        momentumState.CurrentMomentum.Should().Be(100);
        momentumState.BonusAttacks.Should().Be(2);
        momentumState.AttackBonus.Should().Be(4);
        momentumState.DefenseBonus.Should().Be(4);
        momentumState.MovementBonus.Should().Be(5);
        momentumState.CriticalChance.Should().Be(10);
    }

    [Test]
    public void Create_GeneratesCorrectCharacterId()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 50);

        // Assert
        momentumState.CharacterId.Should().Be(_characterId);
    }

    [Test]
    public void Create_WithEmptyCharacterId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => MomentumState.Create(Guid.Empty, initialMomentum: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("characterId")
            .WithMessage("*CharacterId cannot be empty*");
    }

    [Test]
    public void Create_WithConsecutiveHits_SetsProperty()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 50, consecutiveHits: 3);

        // Assert
        momentumState.ConsecutiveHits.Should().Be(3);
    }

    [Test]
    public void Create_WithLastActionTime_SetsProperty()
    {
        // Arrange
        var lastActionTime = DateTime.UtcNow.AddMinutes(-5);

        // Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 50, lastActionTime: lastActionTime);

        // Assert
        momentumState.LastActionTime.Should().Be(lastActionTime);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Threshold Boundary Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(0, MomentumThreshold.Stationary)]
    [TestCase(10, MomentumThreshold.Stationary)]
    [TestCase(20, MomentumThreshold.Stationary)]
    [TestCase(21, MomentumThreshold.Moving)]
    [TestCase(30, MomentumThreshold.Moving)]
    [TestCase(40, MomentumThreshold.Moving)]
    [TestCase(41, MomentumThreshold.Flowing)]
    [TestCase(50, MomentumThreshold.Flowing)]
    [TestCase(60, MomentumThreshold.Flowing)]
    [TestCase(61, MomentumThreshold.Surging)]
    [TestCase(70, MomentumThreshold.Surging)]
    [TestCase(80, MomentumThreshold.Surging)]
    [TestCase(81, MomentumThreshold.Unstoppable)]
    [TestCase(90, MomentumThreshold.Unstoppable)]
    [TestCase(100, MomentumThreshold.Unstoppable)]
    public void Create_AtBoundaryValues_ReturnsCorrectThresholds(int momentum, MomentumThreshold expected)
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: momentum);

        // Assert
        momentumState.Threshold.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Bonus Attack Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(MomentumThreshold.Stationary, 0)]
    [TestCase(MomentumThreshold.Moving, 0)]
    [TestCase(MomentumThreshold.Flowing, 1)]
    [TestCase(MomentumThreshold.Surging, 1)]
    [TestCase(MomentumThreshold.Unstoppable, 2)]
    public void BonusAttacks_ScalesCorrectly_WithThreshold(MomentumThreshold threshold, int expectedAttacks)
    {
        // Arrange - Pick a momentum value in the middle of each threshold range
        var momentum = threshold switch
        {
            MomentumThreshold.Stationary => 10,
            MomentumThreshold.Moving => 30,
            MomentumThreshold.Flowing => 50,
            MomentumThreshold.Surging => 70,
            MomentumThreshold.Unstoppable => 90,
            _ => 0
        };

        // Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: momentum);

        // Assert
        momentumState.BonusAttacks.Should().Be(expectedAttacks);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Movement Bonus Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(0, 0)]
    [TestCase(10, 0)]
    [TestCase(19, 0)]
    [TestCase(20, 1)]
    [TestCase(39, 1)]
    [TestCase(40, 2)]
    [TestCase(59, 2)]
    [TestCase(60, 3)]
    [TestCase(79, 3)]
    [TestCase(80, 4)]
    [TestCase(99, 4)]
    [TestCase(100, 5)]
    public void MovementBonus_CalculatedCorrectly_ForAllRanges(int momentum, int expectedBonus)
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: momentum);

        // Assert
        momentumState.MovementBonus.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Attack Bonus Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(MomentumThreshold.Stationary, 0)]
    [TestCase(MomentumThreshold.Moving, 1)]
    [TestCase(MomentumThreshold.Flowing, 2)]
    [TestCase(MomentumThreshold.Surging, 3)]
    [TestCase(MomentumThreshold.Unstoppable, 4)]
    public void AttackBonus_ScalesWithThreshold_Correctly(MomentumThreshold threshold, int expectedBonus)
    {
        // Arrange - Pick a momentum value in the middle of each threshold range
        var momentum = threshold switch
        {
            MomentumThreshold.Stationary => 10,
            MomentumThreshold.Moving => 30,
            MomentumThreshold.Flowing => 50,
            MomentumThreshold.Surging => 70,
            MomentumThreshold.Unstoppable => 90,
            _ => 0
        };

        // Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: momentum);

        // Assert
        momentumState.AttackBonus.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Defense Bonus Calculation Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(MomentumThreshold.Stationary, 0)]
    [TestCase(MomentumThreshold.Moving, 1)]
    [TestCase(MomentumThreshold.Flowing, 2)]
    [TestCase(MomentumThreshold.Surging, 3)]
    [TestCase(MomentumThreshold.Unstoppable, 4)]
    public void DefenseBonus_ScalesWithThreshold_Correctly(MomentumThreshold threshold, int expectedBonus)
    {
        // Arrange - Pick a momentum value in the middle of each threshold range
        var momentum = threshold switch
        {
            MomentumThreshold.Stationary => 10,
            MomentumThreshold.Moving => 30,
            MomentumThreshold.Flowing => 50,
            MomentumThreshold.Surging => 70,
            MomentumThreshold.Unstoppable => 90,
            _ => 0
        };

        // Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: momentum);

        // Assert
        momentumState.DefenseBonus.Should().Be(expectedBonus);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Critical Chance Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void CriticalChance_OnlyAtUnstoppable()
    {
        // Arrange & Act
        var stationaryState = MomentumState.Create(_characterId, initialMomentum: 10);
        var movingState = MomentumState.Create(_characterId, initialMomentum: 30);
        var flowingState = MomentumState.Create(_characterId, initialMomentum: 50);
        var surgingState = MomentumState.Create(_characterId, initialMomentum: 70);
        var unstoppableState = MomentumState.Create(_characterId, initialMomentum: 90);

        // Assert
        stationaryState.CriticalChance.Should().BeNull();
        movingState.CriticalChance.Should().BeNull();
        flowingState.CriticalChance.Should().BeNull();
        surgingState.CriticalChance.Should().BeNull();
        unstoppableState.CriticalChance.Should().Be(10);
    }

    [Test]
    public void CriticalChance_AtBoundary81_Is10Percent()
    {
        // Arrange & Act
        var justUnstoppable = MomentumState.Create(_characterId, initialMomentum: 81);
        var justBelowUnstoppable = MomentumState.Create(_characterId, initialMomentum: 80);

        // Assert
        justUnstoppable.CriticalChance.Should().Be(10);
        justBelowUnstoppable.CriticalChance.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Edge Case Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithNegativeMomentum_ClampsToZero()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: -50);

        // Assert
        momentumState.CurrentMomentum.Should().Be(0);
        momentumState.Threshold.Should().Be(MomentumThreshold.Stationary);
    }

    [Test]
    public void Create_WithMomentumOver100_ClampsToMaximum()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 150);

        // Assert
        momentumState.CurrentMomentum.Should().Be(100);
        momentumState.Threshold.Should().Be(MomentumThreshold.Unstoppable);
    }

    [Test]
    public void Create_WithNegativeConsecutiveHits_ClampsToZero()
    {
        // Arrange & Act
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 50, consecutiveHits: -5);

        // Assert
        momentumState.ConsecutiveHits.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [TestCase(-10, MomentumThreshold.Stationary)]
    [TestCase(0, MomentumThreshold.Stationary)]
    [TestCase(20, MomentumThreshold.Stationary)]
    [TestCase(21, MomentumThreshold.Moving)]
    [TestCase(40, MomentumThreshold.Moving)]
    [TestCase(41, MomentumThreshold.Flowing)]
    [TestCase(60, MomentumThreshold.Flowing)]
    [TestCase(61, MomentumThreshold.Surging)]
    [TestCase(80, MomentumThreshold.Surging)]
    [TestCase(81, MomentumThreshold.Unstoppable)]
    [TestCase(100, MomentumThreshold.Unstoppable)]
    [TestCase(200, MomentumThreshold.Unstoppable)]
    public void DetermineThreshold_ReturnsCorrectThreshold(int momentum, MomentumThreshold expected)
    {
        // Arrange & Act
        var threshold = MomentumState.DetermineThreshold(momentum);

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
        MomentumState.MinMomentum.Should().Be(0);
        MomentumState.MaxMomentum.Should().Be(100);
        MomentumState.MovingThreshold.Should().Be(20);
        MomentumState.FlowingThreshold.Should().Be(40);
        MomentumState.SurgingThreshold.Should().Be(60);
        MomentumState.UnstoppableThreshold.Should().Be(80);
        MomentumState.DecayOnMiss.Should().Be(25);
        MomentumState.DecayOnStun.Should().Be(100);
        MomentumState.DecayOnIdleTurn.Should().Be(15);
        MomentumState.GainPerSuccessfulAttack.Should().Be(10);
        MomentumState.GainPerKill.Should().Be(20);
        MomentumState.ChainBonusPerHit.Should().Be(5);
        MomentumState.UnstoppableCritBonus.Should().Be(10);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ToString_StationaryState_ReturnsBasicFormat()
    {
        // Arrange
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 10);

        // Act
        var result = momentumState.ToString();

        // Assert
        result.Should().Contain("Momentum[Stationary]");
        result.Should().Contain("10/100");
        result.Should().Contain("ATK +0");
        result.Should().Contain("DEF +0");
        result.Should().Contain("Extra Atks 0");
        result.Should().NotContain("[+10% CRIT]");
        result.Should().NotContain("[Chain x");
    }

    [Test]
    public void ToString_UnstoppableState_IncludesCriticalChance()
    {
        // Arrange
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 90);

        // Act
        var result = momentumState.ToString();

        // Assert
        result.Should().Contain("Momentum[Unstoppable]");
        result.Should().Contain("90/100");
        result.Should().Contain("ATK +4");
        result.Should().Contain("DEF +4");
        result.Should().Contain("Extra Atks 2");
        result.Should().Contain("[+10% CRIT]");
    }

    [Test]
    public void ToString_WithConsecutiveHits_IncludesChainInfo()
    {
        // Arrange
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 50, consecutiveHits: 3);

        // Act
        var result = momentumState.ToString();

        // Assert
        result.Should().Contain("[Chain x3]");
    }

    [Test]
    public void ToString_ZeroConsecutiveHits_OmitsChainInfo()
    {
        // Arrange
        var momentumState = MomentumState.Create(_characterId, initialMomentum: 50, consecutiveHits: 0);

        // Act
        var result = momentumState.ToString();

        // Assert
        result.Should().NotContain("[Chain x");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Comprehensive Threshold Progression Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ThresholdProgression_VerifyAllBonusesScale()
    {
        // Verify complete progression from Stationary to Unstoppable
        var stationary = MomentumState.Create(_characterId, initialMomentum: 10);
        var moving = MomentumState.Create(_characterId, initialMomentum: 30);
        var flowing = MomentumState.Create(_characterId, initialMomentum: 50);
        var surging = MomentumState.Create(_characterId, initialMomentum: 70);
        var unstoppable = MomentumState.Create(_characterId, initialMomentum: 90);

        // Bonus attacks: 0/0/1/1/2
        stationary.BonusAttacks.Should().Be(0);
        moving.BonusAttacks.Should().Be(0);
        flowing.BonusAttacks.Should().Be(1);
        surging.BonusAttacks.Should().Be(1);
        unstoppable.BonusAttacks.Should().Be(2);

        // Attack bonus: 0/1/2/3/4
        stationary.AttackBonus.Should().Be(0);
        moving.AttackBonus.Should().Be(1);
        flowing.AttackBonus.Should().Be(2);
        surging.AttackBonus.Should().Be(3);
        unstoppable.AttackBonus.Should().Be(4);

        // Defense bonus: 0/1/2/3/4
        stationary.DefenseBonus.Should().Be(0);
        moving.DefenseBonus.Should().Be(1);
        flowing.DefenseBonus.Should().Be(2);
        surging.DefenseBonus.Should().Be(3);
        unstoppable.DefenseBonus.Should().Be(4);

        // Critical chance: null/null/null/null/10
        stationary.CriticalChance.Should().BeNull();
        moving.CriticalChance.Should().BeNull();
        flowing.CriticalChance.Should().BeNull();
        surging.CriticalChance.Should().BeNull();
        unstoppable.CriticalChance.Should().Be(10);
    }
}
