// ═══════════════════════════════════════════════════════════════════════════════
// ShadowLightLevelTests.cs
// Unit tests for the ShadowLightLevel value object, validating factory methods,
// query methods, and essence multiplier calculations.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="ShadowLightLevel"/>.
/// </summary>
[TestFixture]
public class ShadowLightLevelTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Factory Method Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CreateDarkness_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var level = ShadowLightLevel.CreateDarkness();

        // Assert
        level.CurrentLevel.Should().Be(LightLevelType.Darkness);
        level.SourceType.Should().Be("None");
        level.Intensity.Should().Be(0);
    }

    [Test]
    public void CreateDimLight_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var level = ShadowLightLevel.CreateDimLight();

        // Assert
        level.CurrentLevel.Should().Be(LightLevelType.DimLight);
        level.Intensity.Should().Be(25);
    }

    [Test]
    public void CreateBrightLight_ReturnsCorrectProperties()
    {
        // Arrange & Act
        var level = ShadowLightLevel.CreateBrightLight();

        // Assert
        level.CurrentLevel.Should().Be(LightLevelType.BrightLight);
        level.Intensity.Should().Be(80);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Query Method Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void IsDarkness_OnlyTrueForDarkness()
    {
        // Act & Assert
        ShadowLightLevel.CreateDarkness().IsDarkness().Should().BeTrue();
        ShadowLightLevel.CreateDimLight().IsDarkness().Should().BeFalse();
        ShadowLightLevel.CreateBrightLight().IsDarkness().Should().BeFalse();
    }

    [Test]
    public void IsShadow_TrueForDarknessAndDimLight()
    {
        // Act & Assert
        ShadowLightLevel.CreateDarkness().IsShadow().Should().BeTrue();
        ShadowLightLevel.CreateDimLight().IsShadow().Should().BeTrue();
        ShadowLightLevel.CreateNormalLight().IsShadow().Should().BeFalse();
        ShadowLightLevel.CreateBrightLight().IsShadow().Should().BeFalse();
    }

    [Test]
    public void IsBrightLight_TrueForBrightAndSunlight()
    {
        // Act & Assert
        ShadowLightLevel.CreateBrightLight().IsBrightLight().Should().BeTrue();
        ShadowLightLevel.CreateSunlight().IsBrightLight().Should().BeTrue();
        ShadowLightLevel.CreateNormalLight().IsBrightLight().Should().BeFalse();
        ShadowLightLevel.CreateDarkness().IsBrightLight().Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Essence Multiplier Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetShadowEssenceMultiplier_ReturnsCorrectValues()
    {
        // Act & Assert
        ShadowLightLevel.CreateDarkness().GetShadowEssenceMultiplier().Should().Be(1.0);
        ShadowLightLevel.CreateDimLight().GetShadowEssenceMultiplier().Should().Be(0.6);
        ShadowLightLevel.CreateNormalLight().GetShadowEssenceMultiplier().Should().Be(0.0);
        ShadowLightLevel.CreateBrightLight().GetShadowEssenceMultiplier().Should().Be(0.0);
    }
}
