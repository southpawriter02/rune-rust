using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="BalanceService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Width correctly sets DC</description></item>
///   <item><description>Stability modifiers apply correctly</description></item>
///   <item><description>Condition modifiers apply correctly</description></item>
///   <item><description>Failure triggers fall</description></item>
///   <item><description>Success outcomes and stamina costs</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class BalanceServiceTests
{
    private IDiceService _diceService = null!;
    private ILogger<BalanceService> _logger = null!;
    private BalanceService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _diceService = Substitute.For<IDiceService>();
        _logger = Substitute.For<ILogger<BalanceService>>();
        _sut = new BalanceService(_diceService, _logger);
    }

    #region Width DC Tests

    [Test]
    public void GetBaseDc_Wide_ReturnsDc2()
    {
        // Act
        var dc = _sut.GetBaseDc(BalanceWidth.Wide);

        // Assert
        dc.Should().Be(2);
    }

    [Test]
    public void GetBaseDc_Narrow_ReturnsDc3()
    {
        // Act
        var dc = _sut.GetBaseDc(BalanceWidth.Narrow);

        // Assert
        dc.Should().Be(3);
    }

    [Test]
    public void GetBaseDc_Cable_ReturnsDc4()
    {
        // Act
        var dc = _sut.GetBaseDc(BalanceWidth.Cable);

        // Assert
        dc.Should().Be(4);
    }

    [Test]
    public void GetBaseDc_RazorEdge_ReturnsDc5()
    {
        // Act
        var dc = _sut.GetBaseDc(BalanceWidth.RazorEdge);

        // Assert
        dc.Should().Be(5);
    }

    [Test]
    public void GetWidthCategory_24Inches_ReturnsWide()
    {
        // Act
        var category = _sut.GetWidthCategory(24);

        // Assert
        category.Should().Be(BalanceWidth.Wide);
    }

    [Test]
    public void GetWidthCategory_12Inches_ReturnsWide()
    {
        // Act
        var category = _sut.GetWidthCategory(12);

        // Assert
        category.Should().Be(BalanceWidth.Wide);
    }

    [Test]
    public void GetWidthCategory_6Inches_ReturnsNarrow()
    {
        // Act
        var category = _sut.GetWidthCategory(6);

        // Assert
        category.Should().Be(BalanceWidth.Narrow);
    }

    [Test]
    public void GetWidthCategory_3Inches_ReturnsCable()
    {
        // Act
        var category = _sut.GetWidthCategory(3);

        // Assert
        category.Should().Be(BalanceWidth.Cable);
    }

    [Test]
    public void GetWidthCategory_1Inch_ReturnsRazorEdge()
    {
        // Act
        var category = _sut.GetWidthCategory(1);

        // Assert
        category.Should().Be(BalanceWidth.RazorEdge);
    }

    #endregion

    #region Modifier Tests

    [Test]
    public void GetStabilityModifier_Stable_ReturnsZero()
    {
        // Act
        var modifier = _sut.GetStabilityModifier(SurfaceStability.Stable);

        // Assert
        modifier.Should().Be(0);
    }

    [Test]
    public void GetStabilityModifier_Unstable_ReturnsOne()
    {
        // Act
        var modifier = _sut.GetStabilityModifier(SurfaceStability.Unstable);

        // Assert
        modifier.Should().Be(1);
    }

    [Test]
    public void GetStabilityModifier_Swaying_ReturnsTwo()
    {
        // Act
        var modifier = _sut.GetStabilityModifier(SurfaceStability.Swaying);

        // Assert
        modifier.Should().Be(2);
    }

    [Test]
    public void GetConditionModifier_Dry_ReturnsZero()
    {
        // Act
        var modifier = _sut.GetConditionModifier(SurfaceCondition.Dry);

        // Assert
        modifier.Should().Be(0);
    }

    [Test]
    public void GetConditionModifier_Wet_ReturnsOne()
    {
        // Act
        var modifier = _sut.GetConditionModifier(SurfaceCondition.Wet);

        // Assert
        modifier.Should().Be(1);
    }

    [Test]
    public void GetConditionModifier_Icy_ReturnsTwo()
    {
        // Act
        var modifier = _sut.GetConditionModifier(SurfaceCondition.Icy);

        // Assert
        modifier.Should().Be(2);
    }

    #endregion

    #region DC Calculation Tests

    [Test]
    public void CalculateDc_NarrowStable_ReturnsDc3()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);

        // Act
        var dc = _sut.CalculateDc(surface);

        // Assert
        dc.Should().Be(3);
    }

    [Test]
    public void CalculateDc_NarrowUnstable_ReturnsDc4()
    {
        // Arrange
        var surface = BalanceSurface.CrumblingLedge(20, 30);

        // Act
        var dc = _sut.CalculateDc(surface);

        // Assert
        dc.Should().Be(4); // 3 base + 1 unstable
    }

    [Test]
    public void CalculateDc_NarrowSwaying_ReturnsDc5()
    {
        // Arrange
        var surface = BalanceSurface.RopeBridge(20, 30);

        // Act
        var dc = _sut.CalculateDc(surface);

        // Assert
        dc.Should().Be(5); // 3 base + 2 swaying
    }

    [Test]
    public void CalculateDc_WithWindModifier_IncreasesDc()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);

        // Act
        var dc = _sut.CalculateDc(surface, windModifier: 2);

        // Assert
        dc.Should().Be(5); // 3 base + 2 wind
    }

    [Test]
    public void CalculateDc_WithBalancePole_DecreasesDc()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);

        // Act
        var dc = _sut.CalculateDc(surface, hasBalancePole: true);

        // Assert
        dc.Should().Be(2); // 3 base - 1 pole
    }

    [Test]
    public void CalculateDc_NeverBelowOne()
    {
        // Arrange
        var surface = BalanceSurface.WidePlank(20, 5);

        // Act - Wide DC 2 with pole -1 should still be at least 1
        var dc = _sut.CalculateDc(surface, hasBalancePole: true);

        // Assert
        dc.Should().BeGreaterThanOrEqualTo(1);
    }

    [Test]
    public void CalculateDc_CombinedModifiers_AddCorrectly()
    {
        // Arrange - Narrow (DC 3) + Swaying (+2) + Wet (+1) + Wind (+1) + Encumbrance (+1) - Pole (-1)
        var surface = new BalanceSurface(
            BalanceWidth.Narrow,
            SurfaceStability.Swaying,
            20, 30,
            SurfaceCondition.Wet);

        // Act
        var dc = _sut.CalculateDc(surface, windModifier: 1, encumbranceModifier: 1, hasBalancePole: true);

        // Assert
        dc.Should().Be(7); // 3 + 2 + 1 + 1 + 1 - 1 = 7
    }

    #endregion

    #region Balance Check Requirement Tests

    [Test]
    public void RequiresBalanceCheck_24Inches_ReturnsFalse()
    {
        // Act
        var required = _sut.RequiresBalanceCheck(24);

        // Assert
        required.Should().BeFalse();
    }

    [Test]
    public void RequiresBalanceCheck_23Inches_ReturnsTrue()
    {
        // Act
        var required = _sut.RequiresBalanceCheck(23);

        // Assert
        required.Should().BeTrue();
    }

    [Test]
    public void RequiresBalanceCheck_6Inches_ReturnsTrue()
    {
        // Act
        var required = _sut.RequiresBalanceCheck(6);

        // Assert
        required.Should().BeTrue();
    }

    #endregion

    #region Balance Attempt Tests

    [Test]
    public void AttemptBalance_Success_ReturnsCrossedTrue()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        SetupDiceRoll(successes: 5, botches: 0); // DC 3, so margin = 2

        // Act
        var result = _sut.AttemptBalance(surface, dicePool: 6);

        // Assert
        result.Crossed.Should().BeTrue();
        result.FallTriggered.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void AttemptBalance_Failure_ReturnsFallTriggered()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        SetupDiceRoll(successes: 1, botches: 0); // DC 3, so margin = -2

        // Act
        var result = _sut.AttemptBalance(surface, dicePool: 4);

        // Assert
        result.Crossed.Should().BeFalse();
        result.FallTriggered.Should().BeTrue();
        result.FallHeightFeet.Should().Be(30);
        result.Outcome.Should().Be(SkillOutcome.Failure);
    }

    [Test]
    public void AttemptBalance_Fumble_ReturnsCriticalFailure()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        SetupDiceRoll(successes: 0, botches: 2); // Fumble

        // Act
        var result = _sut.AttemptBalance(surface, dicePool: 4);

        // Assert
        result.Crossed.Should().BeFalse();
        result.FallTriggered.Should().BeTrue();
        result.IsFumble.Should().BeTrue();
        result.Outcome.Should().Be(SkillOutcome.CriticalFailure);
    }

    [Test]
    public void AttemptBalance_CriticalSuccess_ReturnsZeroStamina()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        SetupDiceRoll(successes: 8, botches: 0); // DC 3, margin = 5 (critical)

        // Act
        var result = _sut.AttemptBalance(surface, dicePool: 8);

        // Assert
        result.Crossed.Should().BeTrue();
        result.IsCritical.Should().BeTrue();
        result.StaminaCost.Should().Be(0);
    }

    [Test]
    public void AttemptBalance_MarginalSuccess_ReturnsDoubleStamina()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        SetupDiceRoll(successes: 3, botches: 0); // DC 3, margin = 0 (marginal)

        // Act
        var result = _sut.AttemptBalance(surface, dicePool: 5);

        // Assert
        result.Crossed.Should().BeTrue();
        result.IsMarginalSuccess.Should().BeTrue();
        result.StaminaCost.Should().Be(2); // Double
    }

    #endregion

    #region BalanceSurface Value Object Tests

    [Test]
    public void BalanceSurface_NarrowLedge_HasCorrectProperties()
    {
        // Act
        var surface = BalanceSurface.NarrowLedge(20, 30);

        // Assert
        surface.Width.Should().Be(BalanceWidth.Narrow);
        surface.Stability.Should().Be(SurfaceStability.Stable);
        surface.LengthFeet.Should().Be(20);
        surface.HeightAboveGround.Should().Be(30);
        surface.BaseDc.Should().Be(3);
        surface.SurfaceDc.Should().Be(3);
    }

    [Test]
    public void BalanceSurface_RopeBridge_HasSwayingStability()
    {
        // Act
        var surface = BalanceSurface.RopeBridge(30, 50);

        // Assert
        surface.Width.Should().Be(BalanceWidth.Narrow);
        surface.Stability.Should().Be(SurfaceStability.Swaying);
        surface.StabilityModifier.Should().Be(2);
        surface.SurfaceDc.Should().Be(5); // 3 + 2
    }

    [Test]
    public void BalanceSurface_FallCausesDamage_TrueAbove10Feet()
    {
        // Act
        var highSurface = BalanceSurface.NarrowLedge(20, 15);
        var lowSurface = BalanceSurface.NarrowLedge(20, 5);

        // Assert
        highSurface.FallCausesDamage.Should().BeTrue();
        lowSurface.FallCausesDamage.Should().BeFalse();
    }

    [Test]
    public void BalanceSurface_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var surface = BalanceSurface.RopeBridge(30, 50);

        // Act
        var display = surface.ToDisplayString();

        // Assert
        display.Should().Contain("narrow");
        display.Should().Contain("30 ft long");
        display.Should().Contain("50 ft up");
        display.Should().Contain("swaying");
    }

    #endregion

    #region BalanceContext Value Object Tests

    [Test]
    public void BalanceContext_ForSurface_CalculatesFinalDc()
    {
        // Arrange
        var surface = BalanceSurface.CrumblingLedge(20, 30);

        // Act
        var context = BalanceContext.ForSurface(surface);

        // Assert
        context.FinalDc.Should().Be(4); // 3 narrow + 1 unstable
    }

    [Test]
    public void BalanceContext_WithWind_IncludesWindModifier()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);

        // Act
        var context = BalanceContext.WithWind(surface, windModifier: 2);

        // Assert
        context.WindModifier.Should().Be(2);
        context.FinalDc.Should().Be(5); // 3 + 2
    }

    [Test]
    public void BalanceContext_LongTraverse_SetsTraverseProperties()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(60, 30);

        // Act
        var context = BalanceContext.LongTraverse(surface, totalChecks: 3, currentCheck: 2);

        // Assert
        context.IsLongTraverse.Should().BeTrue();
        context.TotalChecksRequired.Should().Be(3);
        context.CheckNumber.Should().Be(2);
        context.IsFinalCheck.Should().BeFalse();
    }

    #endregion

    #region BalanceCheckResult Value Object Tests

    [Test]
    public void BalanceCheckResult_Success_HasCorrectProperties()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        var context = BalanceContext.ForSurface(surface);

        // Act
        var result = BalanceCheckResult.Success(context, netSuccesses: 5, SkillOutcome.FullSuccess);

        // Assert
        result.Crossed.Should().BeTrue();
        result.FallTriggered.Should().BeFalse();
        result.Margin.Should().Be(2); // 5 - 3
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void BalanceCheckResult_Failure_HasFallHeight()
    {
        // Arrange
        var surface = BalanceSurface.NarrowLedge(20, 30);
        var context = BalanceContext.ForSurface(surface);

        // Act
        var result = BalanceCheckResult.Failure(context, netSuccesses: 1, SkillOutcome.Failure);

        // Assert
        result.Crossed.Should().BeFalse();
        result.FallTriggered.Should().BeTrue();
        result.FallHeightFeet.Should().Be(30);
    }

    #endregion

    #region Helper Methods

    private void SetupDiceRoll(int successes, int botches)
    {
        var values = new List<int>();
        for (var i = 0; i < successes; i++)
            values.Add(8); // success value
        for (var i = 0; i < botches; i++)
            values.Add(1); // botch value
        while (values.Count < 5)
            values.Add(5);

        var result = new DiceRollResult(DicePool.D10(values.Count), values.ToArray());

        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(result);
    }

    #endregion
}
