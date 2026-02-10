// ═══════════════════════════════════════════════════════════════════════════════
// ShadowEssenceResourceTests.cs
// Unit tests for the ShadowEssenceResource value object, validating creation,
// spending, generation, darkness-based generation, and restoration.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="ShadowEssenceResource"/>.
/// </summary>
[TestFixture]
public class ShadowEssenceResourceTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Creation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CreateDefault_InitializesWithFiftyEssence()
    {
        // Arrange & Act
        var resource = ShadowEssenceResource.CreateDefault();

        // Assert
        resource.CurrentEssence.Should().Be(50);
        resource.MaxEssence.Should().Be(50);
        resource.GenerationRate.Should().Be(5);
        resource.HasEssence.Should().BeTrue();
        resource.IsFullyCharged.Should().BeTrue();
        resource.DarknessGenerationCount.Should().Be(0);
        resource.LastGeneratedAt.Should().BeNull();
    }

    [Test]
    public void Create_WithCustomMax_InitializesCorrectly()
    {
        // Arrange & Act
        var resource = ShadowEssenceResource.Create(30);

        // Assert
        resource.CurrentEssence.Should().Be(30);
        resource.MaxEssence.Should().Be(30);
    }

    [Test]
    public void Create_WithZeroMax_ThrowsArgumentOutOfRange()
    {
        // Arrange & Act
        var act = () => ShadowEssenceResource.Create(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spending Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void TrySpend_WithSufficientEssence_SucceedsAndReduces()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (success, updated) = resource.TrySpend(10);

        // Assert
        success.Should().BeTrue();
        updated.CurrentEssence.Should().Be(40);
        resource.CurrentEssence.Should().Be(50, "original should be unchanged");
    }

    [Test]
    public void TrySpend_WithInsufficientEssence_FailsAndPreservesOriginal()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (success, returned) = resource.TrySpend(51);

        // Assert
        success.Should().BeFalse();
        returned.CurrentEssence.Should().Be(50, "should return unchanged resource");
    }

    [Test]
    public void CanSpend_WithSufficientEssence_ReturnsTrue()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        resource.CanSpend(10).Should().BeTrue();
        resource.CanSpend(50).Should().BeTrue();
        resource.CanSpend(0).Should().BeTrue();
    }

    [Test]
    public void CanSpend_WithInsufficientEssence_ReturnsFalse()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        resource.CanSpend(51).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Generate_AddEssence_CappedAtMax()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(20); // Now at 30/50

        // Act
        var restored = depleted.Generate(10); // Should be 40/50

        // Assert
        restored.CurrentEssence.Should().Be(40);
        restored.LastGeneratedAt.Should().NotBeNull();
    }

    [Test]
    public void Generate_AtMax_DoesNotExceedMax()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act — already at max, generate more
        var result = resource.Generate(10);

        // Assert — capped at max
        result.CurrentEssence.Should().Be(50);
    }

    [Test]
    public void GenerateFromDarkness_InDarkness_GeneratesFiveEssence()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(20); // Now at 30/50

        // Act
        var updated = depleted.GenerateFromDarkness(LightLevelType.Darkness);

        // Assert
        updated.CurrentEssence.Should().Be(35); // +5 from Darkness
        updated.DarknessGenerationCount.Should().Be(1);
    }

    [Test]
    public void GenerateFromDarkness_InDimLight_GeneratesThreeEssence()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(20); // Now at 30/50

        // Act
        var updated = depleted.GenerateFromDarkness(LightLevelType.DimLight);

        // Assert
        updated.CurrentEssence.Should().Be(33); // +3 from DimLight
    }

    [Test]
    public void GenerateFromDarkness_InBrightLight_GeneratesNothing()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(20); // Now at 30/50

        // Act
        var updated = depleted.GenerateFromDarkness(LightLevelType.BrightLight);

        // Assert
        updated.CurrentEssence.Should().Be(30); // No generation
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Restoration Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void RestoreAll_SetsToMaxAndUpdatesTimestamp()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(40); // Now at 10/50

        // Act
        var restored = depleted.RestoreAll();

        // Assert
        restored.CurrentEssence.Should().Be(50);
        restored.LastGeneratedAt.Should().NotBeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetFormattedValue_ReturnsCurrentSlashMax()
    {
        // Arrange & Act
        var resource = ShadowEssenceResource.CreateDefault();

        // Assert
        resource.GetFormattedValue().Should().Be("50/50");
    }

    [Test]
    public void GetPercentage_ReturnsCorrectPercentage()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, half) = resource.TrySpend(25);

        // Act & Assert
        resource.GetPercentage().Should().Be(100);
        half.GetPercentage().Should().Be(50);
    }

    [Test]
    public void IsInShadow_DarknessAndDimLight_ReturnsTrue()
    {
        // Act & Assert
        ShadowEssenceResource.IsInShadow(LightLevelType.Darkness).Should().BeTrue();
        ShadowEssenceResource.IsInShadow(LightLevelType.DimLight).Should().BeTrue();
        ShadowEssenceResource.IsInShadow(LightLevelType.NormalLight).Should().BeFalse();
        ShadowEssenceResource.IsInShadow(LightLevelType.BrightLight).Should().BeFalse();
    }
}
