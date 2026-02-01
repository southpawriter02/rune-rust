namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CpsStageChangeResult"/> value object.
/// Verifies stage transition detection, critical transition flags,
/// factory methods, and ToString formatting.
/// </summary>
[TestFixture]
public class CpsStageChangeResultTests
{
    // -------------------------------------------------------------------------
    // Computed Properties — StageChanged
    // -------------------------------------------------------------------------

    [Test]
    public void StageChanged_True_WhenStagesDiffer()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.None,
            CpsStage.WeightOfKnowing);

        // Assert
        result.StageChanged.Should().BeTrue();
    }

    [Test]
    public void StageChanged_False_WhenStagesSame()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.GlimmerMadness,
            CpsStage.GlimmerMadness);

        // Assert
        result.StageChanged.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Computed Properties — StageWorsened and StageImproved
    // -------------------------------------------------------------------------

    [Test]
    public void StageWorsened_True_WhenNewStageHigher()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.WeightOfKnowing,
            CpsStage.GlimmerMadness);

        // Assert
        result.StageWorsened.Should().BeTrue();
        result.StageImproved.Should().BeFalse();
    }

    [Test]
    public void StageImproved_True_WhenNewStageLower()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.RuinMadness,
            CpsStage.GlimmerMadness);

        // Assert
        result.StageImproved.Should().BeTrue();
        result.StageWorsened.Should().BeFalse();
    }

    [Test]
    public void StageWorsened_And_StageImproved_BothFalse_WhenNoChange()
    {
        // Arrange & Act
        var result = CpsStageChangeResult.NoChange(CpsStage.GlimmerMadness);

        // Assert
        result.StageWorsened.Should().BeFalse();
        result.StageImproved.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Critical Transitions — EnteredRuinMadness
    // -------------------------------------------------------------------------

    [Test]
    public void EnteredRuinMadness_True_WhenTransitioningFromGlimmerMadness()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.GlimmerMadness,
            CpsStage.RuinMadness);

        // Assert
        result.EnteredRuinMadness.Should().BeTrue();
        result.EnteredHollowShell.Should().BeFalse();
        result.IsCriticalTransition.Should().BeTrue();
    }

    [Test]
    public void EnteredRuinMadness_True_WhenTransitioningFromNone()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.None,
            CpsStage.RuinMadness);

        // Assert
        result.EnteredRuinMadness.Should().BeTrue();
    }

    [Test]
    public void EnteredRuinMadness_False_WhenAlreadyInRuinMadness()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.RuinMadness,
            CpsStage.RuinMadness);

        // Assert
        result.EnteredRuinMadness.Should().BeFalse();
        result.StageChanged.Should().BeFalse();
    }

    [Test]
    public void EnteredRuinMadness_False_WhenTransitioningFromHollowShell()
    {
        // Arrange & Act (recovering from HollowShell to RuinMadness)
        var result = new CpsStageChangeResult(
            CpsStage.HollowShell,
            CpsStage.RuinMadness);

        // Assert — Not "entering" RuinMadness, recovering TO it
        result.EnteredRuinMadness.Should().BeFalse();
        result.StageImproved.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Critical Transitions — EnteredHollowShell
    // -------------------------------------------------------------------------

    [Test]
    public void EnteredHollowShell_True_WhenTransitioningFromRuinMadness()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.RuinMadness,
            CpsStage.HollowShell);

        // Assert
        result.EnteredHollowShell.Should().BeTrue();
        result.IsCriticalTransition.Should().BeTrue();
    }

    [Test]
    public void EnteredHollowShell_True_WhenTransitioningFromNone()
    {
        // Arrange & Act (catastrophic stress spike)
        var result = new CpsStageChangeResult(
            CpsStage.None,
            CpsStage.HollowShell);

        // Assert
        result.EnteredHollowShell.Should().BeTrue();
    }

    [Test]
    public void EnteredHollowShell_False_WhenAlreadyInHollowShell()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.HollowShell,
            CpsStage.HollowShell);

        // Assert
        result.EnteredHollowShell.Should().BeFalse();
        result.StageChanged.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsCriticalTransition
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None, CpsStage.WeightOfKnowing, false)]
    [TestCase(CpsStage.WeightOfKnowing, CpsStage.GlimmerMadness, false)]
    [TestCase(CpsStage.GlimmerMadness, CpsStage.RuinMadness, true)]
    [TestCase(CpsStage.RuinMadness, CpsStage.HollowShell, true)]
    [TestCase(CpsStage.RuinMadness, CpsStage.RuinMadness, false)]
    public void IsCriticalTransition_CorrectForVariousTransitions(
        CpsStage previous,
        CpsStage newStage,
        bool expectedCritical)
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(previous, newStage);

        // Assert
        result.IsCriticalTransition.Should().Be(expectedCritical);
    }

    // -------------------------------------------------------------------------
    // LeftRuinMadness
    // -------------------------------------------------------------------------

    [Test]
    public void LeftRuinMadness_True_WhenRecoveringFromRuinMadness()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.RuinMadness,
            CpsStage.GlimmerMadness);

        // Assert
        result.LeftRuinMadness.Should().BeTrue();
        result.StageImproved.Should().BeTrue();
    }

    [Test]
    public void LeftRuinMadness_False_WhenNotPreviouslyInRuinMadness()
    {
        // Arrange & Act
        var result = new CpsStageChangeResult(
            CpsStage.GlimmerMadness,
            CpsStage.WeightOfKnowing);

        // Assert
        result.LeftRuinMadness.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Factory Methods — NoChange
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None)]
    [TestCase(CpsStage.WeightOfKnowing)]
    [TestCase(CpsStage.GlimmerMadness)]
    [TestCase(CpsStage.RuinMadness)]
    [TestCase(CpsStage.HollowShell)]
    public void NoChange_CreatesSameStageResult(CpsStage stage)
    {
        // Arrange & Act
        var result = CpsStageChangeResult.NoChange(stage);

        // Assert
        result.PreviousStage.Should().Be(stage);
        result.NewStage.Should().Be(stage);
        result.StageChanged.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Factory Methods — FromStressChange
    // -------------------------------------------------------------------------

    [Test]
    public void FromStressChange_CalculatesCorrectStages()
    {
        // Arrange & Act
        var result = CpsStageChangeResult.FromStressChange(
            previousStress: 55,
            newStress: 65);

        // Assert
        result.PreviousStage.Should().Be(CpsStage.GlimmerMadness);
        result.NewStage.Should().Be(CpsStage.RuinMadness);
        result.StageWorsened.Should().BeTrue();
        result.EnteredRuinMadness.Should().BeTrue();
    }

    [Test]
    public void FromStressChange_DetectsNoChange_WhenWithinSameStage()
    {
        // Arrange & Act
        var result = CpsStageChangeResult.FromStressChange(
            previousStress: 25,
            newStress: 35);

        // Assert
        result.PreviousStage.Should().Be(CpsStage.WeightOfKnowing);
        result.NewStage.Should().Be(CpsStage.WeightOfKnowing);
        result.StageChanged.Should().BeFalse();
    }

    [Test]
    public void FromStressChange_DetectsImprovement()
    {
        // Arrange & Act
        var result = CpsStageChangeResult.FromStressChange(
            previousStress: 70,
            newStress: 30);

        // Assert
        result.PreviousStage.Should().Be(CpsStage.RuinMadness);
        result.NewStage.Should().Be(CpsStage.WeightOfKnowing);
        result.StageImproved.Should().BeTrue();
        result.LeftRuinMadness.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // ToString — Formatting
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_NoChange_IndicatesUnchanged()
    {
        // Arrange
        var result = CpsStageChangeResult.NoChange(CpsStage.GlimmerMadness);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("GlimmerMadness");
        str.Should().Contain("unchanged");
    }

    [Test]
    public void ToString_Worsened_IndicatesDirection()
    {
        // Arrange
        var result = new CpsStageChangeResult(
            CpsStage.WeightOfKnowing,
            CpsStage.GlimmerMadness);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("WORSENED");
        str.Should().Contain("WeightOfKnowing");
        str.Should().Contain("GlimmerMadness");
        str.Should().Contain("→");
    }

    [Test]
    public void ToString_Improved_IndicatesDirection()
    {
        // Arrange
        var result = new CpsStageChangeResult(
            CpsStage.GlimmerMadness,
            CpsStage.WeightOfKnowing);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("IMPROVED");
    }

    [Test]
    public void ToString_CriticalTransition_IndicatesCritical()
    {
        // Arrange
        var result = new CpsStageChangeResult(
            CpsStage.GlimmerMadness,
            CpsStage.RuinMadness);

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("[CRITICAL!]");
    }
}
