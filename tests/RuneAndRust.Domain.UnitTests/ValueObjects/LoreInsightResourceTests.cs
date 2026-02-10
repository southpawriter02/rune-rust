// ═══════════════════════════════════════════════════════════════════════════════
// LoreInsightResourceTests.cs
// Unit tests for the LoreInsightResource value object, validating creation,
// spending, generation (from discovery types), and rest-based restoration.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="LoreInsightResource"/>.
/// </summary>
[TestFixture]
public class LoreInsightResourceTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Creation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CreateDefault_InitializesWithTenInsight()
    {
        // Arrange & Act
        var resource = LoreInsightResource.CreateDefault();

        // Assert
        resource.CurrentInsight.Should().Be(10);
        resource.MaxInsight.Should().Be(10);
        resource.HasInsight.Should().BeTrue();
        resource.IsFullyCharged.Should().BeTrue();
        resource.DiscoveryCount.Should().Be(0);
        resource.LastGeneratedAt.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TrySpend Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void TrySpend_WithSufficientInsight_SucceedsAndReduces()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();

        // Act
        var (success, updated) = resource.TrySpend(3);

        // Assert
        success.Should().BeTrue();
        updated.CurrentInsight.Should().Be(7);
        updated.MaxInsight.Should().Be(10);
    }

    [Test]
    public void TrySpend_WithInsufficientInsight_ReturnsFalseAndUnchanged()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(10);

        // Act
        var (success, unchanged) = depleted.TrySpend(1);

        // Assert
        success.Should().BeFalse();
        unchanged.CurrentInsight.Should().Be(0);
    }

    [Test]
    public void TrySpend_DoesNotMutateOriginalInstance()
    {
        // Arrange
        var original = LoreInsightResource.CreateDefault();

        // Act
        var (_, _) = original.TrySpend(5);

        // Assert — original unchanged
        original.CurrentInsight.Should().Be(10);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GenerateFromDiscovery Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GenerateFromDiscovery_JotunMachinery_AddsOneInsight()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(5);

        // Act
        var updated = depleted.GenerateFromDiscovery(LoreDiscoveryType.JotunMachinery);

        // Assert
        updated.CurrentInsight.Should().Be(6);
        updated.DiscoveryCount.Should().Be(1);
        updated.LastGeneratedAt.Should().NotBeNull();
    }

    [Test]
    public void GenerateFromDiscovery_Ruin_AddsTwoInsight()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(5);

        // Act
        var updated = depleted.GenerateFromDiscovery(LoreDiscoveryType.Ruin);

        // Assert
        updated.CurrentInsight.Should().Be(7);
        updated.DiscoveryCount.Should().Be(1);
    }

    [Test]
    public void GenerateFromDiscovery_SignificantAndCritical_AddsBonusInsight()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(8);

        // Act — JotunMachinery (1 base) + significant (+1) + critical (+1) = 3
        var updated = depleted.GenerateFromDiscovery(
            LoreDiscoveryType.JotunMachinery, isSignificant: true, isCritical: true);

        // Assert
        updated.CurrentInsight.Should().Be(5);
    }

    [Test]
    public void GenerateFromDiscovery_CapsAtMaxInsight()
    {
        // Arrange — already at 10/10
        var resource = LoreInsightResource.CreateDefault();

        // Act
        var updated = resource.GenerateFromDiscovery(LoreDiscoveryType.Ruin);

        // Assert — still capped at 10
        updated.CurrentInsight.Should().Be(10);
        updated.DiscoveryCount.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Restoration Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void RestoreAll_SetsToMaxAndUpdatesTimestamp()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(7);

        // Act
        var restored = depleted.RestoreAll();

        // Assert
        restored.CurrentInsight.Should().Be(10);
        restored.IsFullyCharged.Should().BeTrue();
        restored.LastGeneratedAt.Should().NotBeNull();
    }

    [Test]
    public void Restore_PartialAmount_CapsAtMax()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(2);

        // Act — restore 5 when at 8/10
        var restored = depleted.Restore(5);

        // Assert — capped at 10
        restored.CurrentInsight.Should().Be(10);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display & Utility Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetFormattedValue_ReturnsExpectedFormat()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, spent) = resource.TrySpend(3);

        // Act & Assert
        spent.GetFormattedValue().Should().Be("7/10");
    }

    [Test]
    public void GetPercentage_ReturnsIntegerPercentage()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();
        var (_, spent) = resource.TrySpend(3);

        // Act & Assert
        spent.GetPercentage().Should().Be(70);
    }

    [Test]
    public void CanSpend_WithSufficientAmount_ReturnsTrue()
    {
        // Arrange
        var resource = LoreInsightResource.CreateDefault();

        // Act & Assert
        resource.CanSpend(5).Should().BeTrue();
        resource.CanSpend(10).Should().BeTrue();
        resource.CanSpend(11).Should().BeFalse();
    }
}
