namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CpsState"/> value object.
/// Verifies factory methods, computed properties, stage determination,
/// clamping behavior, and ToString formatting.
/// </summary>
[TestFixture]
public class CpsStateTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — Create with Valid Stress
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithZeroStress_ReturnsNoneStage()
    {
        // Arrange & Act
        var state = CpsState.Create(0);

        // Assert
        state.Stage.Should().Be(CpsStage.None);
        state.CurrentStress.Should().Be(0);
        state.RequiresPanicCheck.Should().BeFalse();
        state.IsTerminal.Should().BeFalse();
        state.IsClear.Should().BeTrue();
        state.PercentageToHollowShell.Should().Be(0);
    }

    [Test]
    [TestCase(0, CpsStage.None)]
    [TestCase(19, CpsStage.None)]
    [TestCase(20, CpsStage.WeightOfKnowing)]
    [TestCase(39, CpsStage.WeightOfKnowing)]
    [TestCase(40, CpsStage.GlimmerMadness)]
    [TestCase(59, CpsStage.GlimmerMadness)]
    [TestCase(60, CpsStage.RuinMadness)]
    [TestCase(79, CpsStage.RuinMadness)]
    [TestCase(80, CpsStage.HollowShell)]
    [TestCase(100, CpsStage.HollowShell)]
    public void Create_AtBoundaryValues_ReturnsCorrectStages(
        int stress,
        CpsStage expectedStage)
    {
        // Arrange & Act
        var state = CpsState.Create(stress);

        // Assert
        state.Stage.Should().Be(expectedStage);
        state.CurrentStress.Should().Be(stress);
    }

    [Test]
    public void Create_WithMaxStress_ReturnsTerminalState()
    {
        // Arrange & Act
        var state = CpsState.Create(100);

        // Assert
        state.Stage.Should().Be(CpsStage.HollowShell);
        state.IsTerminal.Should().BeTrue();
        state.IsHollow.Should().BeTrue();
        state.RequiresPanicCheck.Should().BeTrue();
        state.PercentageToHollowShell.Should().Be(1.0);
    }

    // -------------------------------------------------------------------------
    // Factory Methods — Clamping Behavior
    // -------------------------------------------------------------------------

    [Test]
    public void Create_ClampsNegativeToZero()
    {
        // Arrange & Act
        var state = CpsState.Create(-10);

        // Assert
        state.CurrentStress.Should().Be(0);
        state.Stage.Should().Be(CpsStage.None);
    }

    [Test]
    public void Create_ClampsAbove100()
    {
        // Arrange & Act
        var state = CpsState.Create(150);

        // Assert
        state.CurrentStress.Should().Be(100);
        state.Stage.Should().Be(CpsStage.HollowShell);
    }

    [Test]
    [TestCase(-10, 0)]
    [TestCase(-1, 0)]
    [TestCase(101, 100)]
    [TestCase(150, 100)]
    public void Create_WithOutOfRangeStress_ClampsToValidRange(
        int inputStress,
        int expectedStress)
    {
        // Arrange & Act
        var state = CpsState.Create(inputStress);

        // Assert
        state.CurrentStress.Should().Be(expectedStress);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — RequiresPanicCheck
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, false)]
    [TestCase(19, false)]
    [TestCase(20, false)]
    [TestCase(39, false)]
    [TestCase(40, false)]
    [TestCase(59, false)]
    [TestCase(60, true)]
    [TestCase(79, true)]
    [TestCase(80, true)]
    [TestCase(100, true)]
    public void RequiresPanicCheck_TrueOnlyForRuinMadnessAndAbove(
        int stress,
        bool expected)
    {
        // Arrange & Act
        var state = CpsState.Create(stress);

        // Assert
        state.RequiresPanicCheck.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — IsTerminal
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, false)]
    [TestCase(19, false)]
    [TestCase(59, false)]
    [TestCase(79, false)]
    [TestCase(80, true)]
    [TestCase(100, true)]
    public void IsTerminal_TrueOnlyForHollowShellStage(
        int stress,
        bool expected)
    {
        // Arrange & Act
        var state = CpsState.Create(stress);

        // Assert
        state.IsTerminal.Should().Be(expected);
        state.IsHollow.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — PercentageToHollowShell
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, 0.0)]
    [TestCase(20, 0.25)]
    [TestCase(40, 0.5)]
    [TestCase(60, 0.75)]
    [TestCase(80, 1.0)]
    [TestCase(100, 1.0)]
    public void PercentageToHollowShell_CalculatesCorrectly(
        int stress,
        double expected)
    {
        // Arrange & Act
        var state = CpsState.Create(stress);

        // Assert
        state.PercentageToHollowShell.Should().BeApproximately(expected, 0.001);
    }

    [Test]
    public void PercentageToHollowShell_CapsAtOneForTerminalState()
    {
        // Arrange & Act
        var state = CpsState.Create(100);

        // Assert
        state.PercentageToHollowShell.Should().Be(1.0);
    }

    // -------------------------------------------------------------------------
    // Arrow-Expression Properties — IsClear and IsHollow
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, true)]
    [TestCase(19, true)]
    [TestCase(20, false)]
    [TestCase(50, false)]
    [TestCase(80, false)]
    public void IsClear_TrueOnlyForNoneStage(int stress, bool expected)
    {
        // Arrange & Act
        var state = CpsState.Create(stress);

        // Assert
        state.IsClear.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // DetermineStage — Static Method
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0, CpsStage.None)]
    [TestCase(19, CpsStage.None)]
    [TestCase(20, CpsStage.WeightOfKnowing)]
    [TestCase(39, CpsStage.WeightOfKnowing)]
    [TestCase(40, CpsStage.GlimmerMadness)]
    [TestCase(59, CpsStage.GlimmerMadness)]
    [TestCase(60, CpsStage.RuinMadness)]
    [TestCase(79, CpsStage.RuinMadness)]
    [TestCase(80, CpsStage.HollowShell)]
    [TestCase(100, CpsStage.HollowShell)]
    public void DetermineStage_ReturnsCorrectStage(
        int stress,
        CpsStage expectedStage)
    {
        // Arrange & Act
        var result = CpsState.DetermineStage(stress);

        // Assert
        result.Should().Be(expectedStage);
    }

    // -------------------------------------------------------------------------
    // ToString — Formatting
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_WithClearState_ReturnsBasicFormat()
    {
        // Arrange
        var state = CpsState.Create(15);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("CPS[None]: Stress=15");
        result.Should().NotContain("PANIC CHECK");
        result.Should().NotContain("TERMINAL");
    }

    [Test]
    public void ToString_WithRuinMadness_IndicatesPanicCheck()
    {
        // Arrange
        var state = CpsState.Create(65);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("RuinMadness");
        result.Should().Contain("[PANIC CHECK]");
        result.Should().NotContain("TERMINAL");
    }

    [Test]
    public void ToString_WithHollowShell_IndicatesTerminal()
    {
        // Arrange
        var state = CpsState.Create(85);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("HollowShell");
        result.Should().Contain("[PANIC CHECK]");
        result.Should().Contain("[TERMINAL!]");
    }

    [Test]
    public void ToString_ZeroStress_FormatsCorrectly()
    {
        // Arrange
        var state = CpsState.Create(0);

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Be("CPS[None]: Stress=0");
    }

    // -------------------------------------------------------------------------
    // Record Equality — Value-Based Comparison
    // -------------------------------------------------------------------------

    [Test]
    public void CpsState_WithSameStress_AreEqual()
    {
        // Arrange
        var state1 = CpsState.Create(45);
        var state2 = CpsState.Create(45);

        // Assert
        state1.Should().Be(state2);
        (state1 == state2).Should().BeTrue();
    }

    [Test]
    public void CpsState_WithDifferentStress_AreNotEqual()
    {
        // Arrange
        var state1 = CpsState.Create(45);
        var state2 = CpsState.Create(65);

        // Assert
        state1.Should().NotBe(state2);
        (state1 != state2).Should().BeTrue();
    }
}
