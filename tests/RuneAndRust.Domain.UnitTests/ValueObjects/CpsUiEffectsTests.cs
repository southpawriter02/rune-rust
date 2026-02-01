namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="CpsUiEffects"/> value object.
/// Verifies distortion scaling, text corruption flags, factory methods,
/// and ToString formatting.
/// </summary>
[TestFixture]
public class CpsUiEffectsTests
{
    // -------------------------------------------------------------------------
    // Factory Methods — ForStage Dispatch
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None, 0.0f)]
    [TestCase(CpsStage.WeightOfKnowing, 0.1f)]
    [TestCase(CpsStage.GlimmerMadness, 0.4f)]
    [TestCase(CpsStage.RuinMadness, 0.7f)]
    [TestCase(CpsStage.HollowShell, 1.0f)]
    public void ForStage_DistortionIntensityScales_WithSeverity(
        CpsStage stage, float expectedIntensity)
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForStage(stage);

        // Assert
        effects.DistortionIntensity.Should().Be(expectedIntensity);
        effects.Stage.Should().Be(stage);
    }

    // -------------------------------------------------------------------------
    // Text Effects — TextGlitching
    // -------------------------------------------------------------------------

    [Test]
    public void TextGlitching_EnabledAt_GlimmerMadnessAndAbove()
    {
        // Assert
        CpsUiEffects.ForNone().TextGlitching.Should().BeFalse();
        CpsUiEffects.ForWeightOfKnowing().TextGlitching.Should().BeFalse();
        CpsUiEffects.ForGlimmerMadness().TextGlitching.Should().BeTrue();
        CpsUiEffects.ForRuinMadness().TextGlitching.Should().BeTrue();
        // HollowShell has TextGlitching false (screen goes dark)
        CpsUiEffects.ForHollowShell().TextGlitching.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Text Effects — LeetSpeakLevel
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None, 0)]
    [TestCase(CpsStage.WeightOfKnowing, 0)]
    [TestCase(CpsStage.GlimmerMadness, 2)]
    [TestCase(CpsStage.RuinMadness, 4)]
    [TestCase(CpsStage.HollowShell, 0)] // Screen goes dark
    public void LeetSpeakLevel_ProgressesWithStage(
        CpsStage stage, int expectedLevel)
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForStage(stage);

        // Assert
        effects.LeetSpeakLevel.Should().Be(expectedLevel);
    }

    // -------------------------------------------------------------------------
    // Computed Properties — HasDistortion
    // -------------------------------------------------------------------------

    [Test]
    public void HasDistortion_False_ForNoneStage()
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForNone();

        // Assert
        effects.HasDistortion.Should().BeFalse();
    }

    [Test]
    [TestCase(CpsStage.WeightOfKnowing)]
    [TestCase(CpsStage.GlimmerMadness)]
    [TestCase(CpsStage.RuinMadness)]
    [TestCase(CpsStage.HollowShell)]
    public void HasDistortion_True_ForAllOtherStages(CpsStage stage)
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForStage(stage);

        // Assert
        effects.HasDistortion.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Computed Properties — HasTextCorruption
    // -------------------------------------------------------------------------

    [Test]
    public void HasTextCorruption_False_ForNoneAndWeightOfKnowing()
    {
        // Assert
        CpsUiEffects.ForNone().HasTextCorruption.Should().BeFalse();
        CpsUiEffects.ForWeightOfKnowing().HasTextCorruption.Should().BeFalse();
    }

    [Test]
    public void HasTextCorruption_True_ForGlimmerMadnessAndRuinMadness()
    {
        // Assert
        CpsUiEffects.ForGlimmerMadness().HasTextCorruption.Should().BeTrue();
        CpsUiEffects.ForRuinMadness().HasTextCorruption.Should().BeTrue();
    }

    [Test]
    public void HasTextCorruption_False_ForHollowShell()
    {
        // HollowShell: Screen goes dark, no text corruption
        CpsUiEffects.ForHollowShell().HasTextCorruption.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Peripheral Static
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None, false)]
    [TestCase(CpsStage.WeightOfKnowing, true)]
    [TestCase(CpsStage.GlimmerMadness, true)]
    [TestCase(CpsStage.RuinMadness, true)]
    [TestCase(CpsStage.HollowShell, false)] // Screen goes dark
    public void PeripheralStatic_CorrectPerStage(CpsStage stage, bool expected)
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForStage(stage);

        // Assert
        effects.PeripheralStatic.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // System Warnings
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None, false)]
    [TestCase(CpsStage.WeightOfKnowing, false)]
    [TestCase(CpsStage.GlimmerMadness, false)]
    [TestCase(CpsStage.RuinMadness, true)]
    [TestCase(CpsStage.HollowShell, true)]
    public void SystemWarnings_EnabledAtRuinMadnessAndAbove(
        CpsStage stage, bool expected)
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForStage(stage);

        // Assert
        effects.SystemWarnings.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Screen Lag
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(CpsStage.None, false)]
    [TestCase(CpsStage.WeightOfKnowing, false)]
    [TestCase(CpsStage.GlimmerMadness, false)]
    [TestCase(CpsStage.RuinMadness, true)]
    [TestCase(CpsStage.HollowShell, true)]
    public void ScreenLag_EnabledAtRuinMadnessAndAbove(
        CpsStage stage, bool expected)
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForStage(stage);

        // Assert
        effects.ScreenLag.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Color Tint
    // -------------------------------------------------------------------------

    [Test]
    public void ColorTint_ProgressesFromWhiteToBlack()
    {
        // Assert progression: white → gray → warm → red → black
        CpsUiEffects.ForNone().ColorTint.Should().Be("#FFFFFF");
        CpsUiEffects.ForWeightOfKnowing().ColorTint.Should().Be("#E8E8E8");
        CpsUiEffects.ForGlimmerMadness().ColorTint.Should().Be("#FFE0B0");
        CpsUiEffects.ForRuinMadness().ColorTint.Should().Be("#FF8080");
        CpsUiEffects.ForHollowShell().ColorTint.Should().Be("#000000");
    }

    // -------------------------------------------------------------------------
    // Individual Factory Methods
    // -------------------------------------------------------------------------

    [Test]
    public void ForNone_ReturnsCleanUi()
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForNone();

        // Assert
        effects.Stage.Should().Be(CpsStage.None);
        effects.DistortionIntensity.Should().Be(0.0f);
        effects.PeripheralStatic.Should().BeFalse();
        effects.TextGlitching.Should().BeFalse();
        effects.LeetSpeakLevel.Should().Be(0);
        effects.SystemWarnings.Should().BeFalse();
        effects.ScreenLag.Should().BeFalse();
    }

    [Test]
    public void ForRuinMadness_ReturnsHeavyDistortion()
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForRuinMadness();

        // Assert
        effects.Stage.Should().Be(CpsStage.RuinMadness);
        effects.DistortionIntensity.Should().Be(0.7f);
        effects.PeripheralStatic.Should().BeTrue();
        effects.TextGlitching.Should().BeTrue();
        effects.LeetSpeakLevel.Should().Be(4);
        effects.SystemWarnings.Should().BeTrue();
        effects.ScreenLag.Should().BeTrue();
    }

    [Test]
    public void ForHollowShell_ReturnsBlackoutEffects()
    {
        // Arrange & Act
        var effects = CpsUiEffects.ForHollowShell();

        // Assert
        effects.Stage.Should().Be(CpsStage.HollowShell);
        effects.DistortionIntensity.Should().Be(1.0f);
        effects.PeripheralStatic.Should().BeFalse(); // Screen goes dark
        effects.TextGlitching.Should().BeFalse();
        effects.LeetSpeakLevel.Should().Be(0);
        effects.SystemWarnings.Should().BeTrue(); // Dramatic effect
        effects.ColorTint.Should().Be("#000000");
    }

    // -------------------------------------------------------------------------
    // ToString — Formatting
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_IncludesKeyInfo()
    {
        // Arrange
        var effects = CpsUiEffects.ForGlimmerMadness();

        // Act
        var str = effects.ToString();

        // Assert
        str.Should().Contain("GlimmerMadness");
        str.Should().Contain("Distortion=");
        str.Should().Contain("Glitch=True");
        str.Should().Contain("Leet=2");
    }

    [Test]
    public void ToString_ForNone_ShowsNoDistortion()
    {
        // Arrange
        var effects = CpsUiEffects.ForNone();

        // Act
        var str = effects.ToString();

        // Assert
        str.Should().Contain("None");
        str.Should().Contain("Glitch=False");
        str.Should().Contain("Leet=0");
    }

    // -------------------------------------------------------------------------
    // Record Equality
    // -------------------------------------------------------------------------

    [Test]
    public void CpsUiEffects_SameStage_AreEqual()
    {
        // Arrange
        var effects1 = CpsUiEffects.ForGlimmerMadness();
        var effects2 = CpsUiEffects.ForGlimmerMadness();

        // Assert
        effects1.Should().Be(effects2);
        (effects1 == effects2).Should().BeTrue();
    }

    [Test]
    public void CpsUiEffects_DifferentStages_AreNotEqual()
    {
        // Arrange
        var effects1 = CpsUiEffects.ForNone();
        var effects2 = CpsUiEffects.ForRuinMadness();

        // Assert
        effects1.Should().NotBe(effects2);
    }
}
