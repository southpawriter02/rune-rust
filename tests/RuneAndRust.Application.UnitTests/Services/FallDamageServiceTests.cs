using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="FallDamageService"/> and related fall damage mechanics.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Height-to-damage dice calculation</description></item>
///   <item><description>Maximum damage cap enforcement</description></item>
///   <item><description>Crash Landing DC scaling</description></item>
///   <item><description>Crash Landing damage reduction</description></item>
///   <item><description>Complete fall processing workflow</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class FallDamageServiceTests
{
    private Mock<IDiceService> _diceServiceMock = null!;
    private Mock<ILogger<FallDamageService>> _loggerMock = null!;
    private FallDamageService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _diceServiceMock = new Mock<IDiceService>();
        _loggerMock = new Mock<ILogger<FallDamageService>>();
        _service = new FallDamageService(_diceServiceMock.Object, _loggerMock.Object);
    }

    #region FallDamage Value Object Tests

    /// <summary>
    /// Tests that FallDamage correctly calculates damage dice from height.
    /// </summary>
    /// <remarks>
    /// Formula: DamageDice = Height / 10 (rounded down).
    /// </remarks>
    [Test]
    [TestCase(10, 1)]
    [TestCase(20, 2)]
    [TestCase(30, 3)]
    [TestCase(45, 4)]  // 45 / 10 = 4.5, rounds down to 4
    [TestCase(50, 5)]
    [TestCase(99, 9)]
    [TestCase(100, 10)]
    public void FallDamage_FromHeight_CalculatesCorrectDamageDice(int height, int expectedDice)
    {
        // Arrange & Act
        var fallDamage = FallDamage.FromHeight(height);

        // Assert
        fallDamage.DamageDice.Should().Be(expectedDice);
        fallDamage.FallHeight.Should().Be(height);
        fallDamage.DamageType.Should().Be("Bludgeoning");
    }

    /// <summary>
    /// Tests that damage dice cap at 10d10 for falls over 100 feet.
    /// </summary>
    [Test]
    [TestCase(100, 10)]
    [TestCase(150, 10)]
    [TestCase(200, 10)]
    [TestCase(1000, 10)]
    public void FallDamage_ExtremeHeights_CapsAtMaxDamage(int height, int expectedDice)
    {
        // Arrange & Act
        var fallDamage = FallDamage.FromHeight(height);

        // Assert
        fallDamage.DamageDice.Should().Be(expectedDice);
        fallDamage.TotalDamageDice.Should().Be(expectedDice);
    }

    /// <summary>
    /// Tests that falls below 10 feet cause no damage.
    /// </summary>
    [Test]
    [TestCase(0)]
    [TestCase(5)]
    [TestCase(9)]
    public void FallDamage_BelowThreshold_CausesNoDamage(int height)
    {
        // Arrange & Act
        var fallDamage = FallDamage.FromHeight(height);

        // Assert
        fallDamage.DamageDice.Should().Be(0);
        fallDamage.CausesDamage.Should().BeFalse();
        fallDamage.CanCrashLand.Should().BeFalse();
    }

    /// <summary>
    /// Tests that falls at or above 10 feet can attempt Crash Landing.
    /// </summary>
    [Test]
    [TestCase(10, true)]
    [TestCase(15, true)]
    [TestCase(9, false)]
    [TestCase(5, false)]
    public void FallDamage_CrashLandingEligibility_BasedOnHeight(int height, bool canCrashLand)
    {
        // Arrange & Act
        var fallDamage = FallDamage.FromHeight(height);

        // Assert
        fallDamage.CanCrashLand.Should().Be(canCrashLand);
    }

    /// <summary>
    /// Tests that bonus damage dice from fumbles are included in total.
    /// </summary>
    [Test]
    public void FallDamage_WithBonusDice_IncludesInTotal()
    {
        // Arrange & Act
        var fallDamage = FallDamage.FromHeight(30, FallSource.Leaping, bonusDice: 1);

        // Assert
        fallDamage.DamageDice.Should().Be(3);
        fallDamage.BonusDamageDice.Should().Be(1);
        fallDamage.TotalDamageDice.Should().Be(4);
    }

    /// <summary>
    /// Tests [The Long Fall] factory method with +1d10 bonus.
    /// </summary>
    [Test]
    public void FallDamage_TheLongFall_AddsBonusDamage()
    {
        // Arrange & Act
        var fallDamage = FallDamage.TheLongFall(30);

        // Assert
        fallDamage.DamageDice.Should().Be(3);
        fallDamage.BonusDamageDice.Should().Be(1);
        fallDamage.TotalDamageDice.Should().Be(4);
        fallDamage.Source.Should().Be(FallSource.Leaping);
    }

    /// <summary>
    /// Tests Crash Landing DC calculation from height.
    /// </summary>
    /// <remarks>
    /// DC = 2 + (Height / 10).
    /// </remarks>
    [Test]
    [TestCase(10, 3)]   // 2 + (10/10) = 3
    [TestCase(20, 4)]   // 2 + (20/10) = 4
    [TestCase(30, 5)]   // 2 + (30/10) = 5
    [TestCase(50, 7)]   // 2 + (50/10) = 7
    [TestCase(100, 12)] // 2 + (100/10) = 12
    public void FallDamage_CrashLandingDc_ScalesWithHeight(int height, int expectedDc)
    {
        // Arrange & Act
        var fallDamage = FallDamage.FromHeight(height);

        // Assert
        fallDamage.CrashLandingDc.Should().Be(expectedDc);
    }

    /// <summary>
    /// Tests that WithDiceReduction reduces damage appropriately.
    /// </summary>
    [Test]
    public void FallDamage_WithDiceReduction_ReducesDamage()
    {
        // Arrange
        var fallDamage = FallDamage.FromHeight(50); // 5d10

        // Act
        var reduced = fallDamage.WithDiceReduction(2);

        // Assert
        reduced.DamageDice.Should().Be(3);
        reduced.FallHeight.Should().Be(50); // Height unchanged
    }

    /// <summary>
    /// Tests that reduction cannot go below zero damage dice.
    /// </summary>
    [Test]
    public void FallDamage_ExcessReduction_CapsAtZero()
    {
        // Arrange
        var fallDamage = FallDamage.FromHeight(30); // 3d10

        // Act
        var reduced = fallDamage.WithDiceReduction(10); // More than available

        // Assert
        reduced.DamageDice.Should().Be(0);
        reduced.CausesDamage.Should().BeFalse();
    }

    #endregion

    #region CrashLandingResult Value Object Tests

    /// <summary>
    /// Tests successful Crash Landing with damage reduction.
    /// </summary>
    [Test]
    public void CrashLandingResult_ExceedsDc_ReducesDamage()
    {
        // Arrange & Act
        // DC 5, rolled 7 successes, original 5d10
        var result = CrashLandingResult.FromRoll(
            crashDc: 5,
            successes: 7,
            originalDamageDice: 5,
            outcome: SkillOutcome.FullSuccess);

        // Assert
        result.Margin.Should().Be(2);
        result.DiceReduced.Should().Be(2);
        result.FinalDamageDice.Should().Be(3);
        result.Succeeded.Should().BeTrue();
        result.ReducedDamage.Should().BeTrue();
    }

    /// <summary>
    /// Tests exactly meeting DC (marginal success, no reduction).
    /// </summary>
    [Test]
    public void CrashLandingResult_ExactlyMeetsDc_NoReduction()
    {
        // Arrange & Act
        var result = CrashLandingResult.FromRoll(
            crashDc: 5,
            successes: 5,
            originalDamageDice: 5,
            outcome: SkillOutcome.MarginalSuccess);

        // Assert
        result.Margin.Should().Be(0);
        result.DiceReduced.Should().Be(0);
        result.FinalDamageDice.Should().Be(5);
        result.Succeeded.Should().BeTrue();
        result.ReducedDamage.Should().BeFalse();
    }

    /// <summary>
    /// Tests failed Crash Landing (below DC).
    /// </summary>
    [Test]
    public void CrashLandingResult_BelowDc_NoReduction()
    {
        // Arrange & Act
        var result = CrashLandingResult.FromRoll(
            crashDc: 5,
            successes: 3,
            originalDamageDice: 5,
            outcome: SkillOutcome.Failure);

        // Assert
        result.Margin.Should().Be(-2);
        result.DiceReduced.Should().Be(0);
        result.FinalDamageDice.Should().Be(5);
        result.Succeeded.Should().BeFalse();
        result.ReducedDamage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that large margin can negate all damage.
    /// </summary>
    [Test]
    public void CrashLandingResult_LargeMargin_NegatesAllDamage()
    {
        // Arrange & Act
        // DC 3, rolled 8 successes, original 3d10
        var result = CrashLandingResult.FromRoll(
            crashDc: 3,
            successes: 8,
            originalDamageDice: 3,
            outcome: SkillOutcome.CriticalSuccess);

        // Assert
        result.Margin.Should().Be(5);
        result.DiceReduced.Should().Be(5); // Would reduce 5, but only 3 available
        result.FinalDamageDice.Should().Be(0);
        result.NegatedAllDamage.Should().BeTrue();
    }

    /// <summary>
    /// Tests NoAttempt factory method.
    /// </summary>
    [Test]
    public void CrashLandingResult_NoAttempt_PreservesDamage()
    {
        // Arrange & Act
        var result = CrashLandingResult.NoAttempt(5);

        // Assert
        result.WasAttempted.Should().BeFalse();
        result.Succeeded.Should().BeFalse();
        result.FinalDamageDice.Should().Be(5);
        result.DiceReduced.Should().Be(0);
    }

    #endregion

    #region FallDamageService Tests

    /// <summary>
    /// Tests that CalculateFallDamage returns correct damage parameters.
    /// </summary>
    [Test]
    public void CalculateFallDamage_FromHeight_ReturnsCorrectDamage()
    {
        // Arrange & Act
        var fallDamage = _service.CalculateFallDamage(30, FallSource.Climbing);

        // Assert
        fallDamage.FallHeight.Should().Be(30);
        fallDamage.DamageDice.Should().Be(3);
        fallDamage.Source.Should().Be(FallSource.Climbing);
        fallDamage.CanCrashLand.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetCrashLandingDc returns correct DC for various heights.
    /// </summary>
    [Test]
    [TestCase(10, 3)]
    [TestCase(30, 5)]
    [TestCase(50, 7)]
    [TestCase(100, 12)]
    public void GetCrashLandingDc_VariousHeights_ReturnsCorrectDc(int height, int expectedDc)
    {
        // Arrange & Act
        var dc = _service.GetCrashLandingDc(height);

        // Assert
        dc.Should().Be(expectedDc);
    }

    /// <summary>
    /// Tests that GetCrashLandingDc returns 0 for safe falls.
    /// </summary>
    [Test]
    [TestCase(5)]
    [TestCase(9)]
    public void GetCrashLandingDc_BelowThreshold_ReturnsZero(int height)
    {
        // Arrange & Act
        var dc = _service.GetCrashLandingDc(height);

        // Assert
        dc.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetDamageDice returns correct dice count.
    /// </summary>
    [Test]
    [TestCase(10, 1)]
    [TestCase(55, 5)]
    [TestCase(100, 10)]
    [TestCase(200, 10)]  // Capped
    public void GetDamageDice_VariousHeights_ReturnsCorrectCount(int height, int expectedDice)
    {
        // Arrange & Act
        var dice = _service.GetDamageDice(height);

        // Assert
        dice.Should().Be(expectedDice);
    }

    /// <summary>
    /// Tests that RollDamage sums dice correctly.
    /// </summary>
    [Test]
    public void RollDamage_RollsAndSumsDice()
    {
        // Arrange
        var mockRollResult = new DiceRollResult(
            pool: DicePool.D10(3),
            rolls: [5, 8, 3]);

        _diceServiceMock
            .Setup(d => d.Roll(
                It.IsAny<DicePool>(),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(mockRollResult);

        // Act
        var damage = _service.RollDamage(3);

        // Assert
        damage.Should().Be(16); // Sum of [5, 8, 3]
    }

    /// <summary>
    /// Tests that RollDamage returns 0 for no dice.
    /// </summary>
    [Test]
    public void RollDamage_ZeroDice_ReturnsZero()
    {
        // Arrange & Act
        var damage = _service.RollDamage(0);

        // Assert
        damage.Should().Be(0);
        _diceServiceMock.Verify(
            d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()),
            Times.Never);
    }

    /// <summary>
    /// Tests ProcessFall with successful Crash Landing.
    /// </summary>
    [Test]
    public void ProcessFall_SuccessfulCrashLanding_ReducesDamage()
    {
        // Arrange
        var fallResult = FallResult.FromHeight(30, FallSource.Climbing);

        // Mock Crash Landing roll: 6 successes vs DC 5 = margin +1
        // Rolls [8, 9, 10, 8, 8, 10] = 6 values ≥ 8 = 6 successes
        var crashRollResult = new DiceRollResult(
            pool: DicePool.D10(6),
            rolls: [8, 9, 10, 8, 8, 10]);

        // Mock damage roll: reduced from 3d10 to 2d10
        var damageRollResult = new DiceRollResult(
            pool: DicePool.D10(2),
            rolls: [7, 4]);

        _diceServiceMock
            .SetupSequence(d => d.Roll(
                It.IsAny<DicePool>(),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(crashRollResult)
            .Returns(damageRollResult);

        // Act
        var result = _service.ProcessFall("player-1", fallResult, baseDicePool: 5);

        // Assert
        result.CharacterId.Should().Be("player-1");
        result.FallHeight.Should().Be(30);
        result.CrashLanding.Succeeded.Should().BeTrue();
        result.CrashLanding.DiceReduced.Should().Be(1); // margin = 6 - 5 = 1
        result.FinalDamageDice.Should().Be(2); // 3 - 1 = 2
        result.DamageRolled.Should().Be(11);
    }

    /// <summary>
    /// Tests ProcessFall when Crash Landing is not attempted.
    /// </summary>
    [Test]
    public void ProcessFall_NoCrashLanding_TakesFullDamage()
    {
        // Arrange
        var fallResult = FallResult.FromHeight(30, FallSource.Environmental);

        var damageRollResult = new DiceRollResult(
            pool: DicePool.D10(3),
            rolls: [6, 8, 5]);

        _diceServiceMock
            .Setup(d => d.Roll(
                It.IsAny<DicePool>(),
                It.IsAny<AdvantageType>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(damageRollResult);

        // Act
        var result = _service.ProcessFall(
            "player-1",
            fallResult,
            baseDicePool: 5,
            attemptCrashLanding: false);

        // Assert
        result.CrashLandingAttempted.Should().BeFalse();
        result.FinalDamageDice.Should().Be(3);
        result.DamageRolled.Should().Be(19);
    }

    /// <summary>
    /// Tests ProcessFall for fall below damage threshold.
    /// </summary>
    [Test]
    public void ProcessFall_BelowThreshold_NoDamage()
    {
        // Arrange
        var fallResult = FallResult.FromHeight(8, FallSource.Environmental, triggeredByFumble: false);

        // Act
        var result = _service.ProcessFall("player-1", fallResult, baseDicePool: 5);

        // Assert
        result.TookDamage.Should().BeFalse();
        result.DamageRolled.Should().Be(0);
        result.FinalDamageDice.Should().Be(0);
    }

    #endregion
}
