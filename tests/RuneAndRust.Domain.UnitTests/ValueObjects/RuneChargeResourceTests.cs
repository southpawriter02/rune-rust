// ═══════════════════════════════════════════════════════════════════════════════
// RuneChargeResourceTests.cs
// Unit tests for the RuneChargeResource value object, validating creation,
// spending, restoration, and craft-based generation mechanics.
// Version: 0.20.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="RuneChargeResource"/> value object.
/// </summary>
[TestFixture]
public class RuneChargeResourceTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Creation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CreateDefault_InitializesWithFiveCharges()
    {
        // Arrange & Act
        var resource = RuneChargeResource.CreateDefault();

        // Assert
        resource.CurrentCharges.Should().Be(5);
        resource.MaxCharges.Should().Be(5);
        resource.HasCharges.Should().BeTrue();
        resource.IsFullyCharged.Should().BeTrue();
        resource.LastGeneratedAt.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spending Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void TrySpend_WithSufficientCharges_ReturnsSuccessAndReduced()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();

        // Act
        var (success, updated) = resource.TrySpend(1);

        // Assert
        success.Should().BeTrue();
        updated.CurrentCharges.Should().Be(4);
        updated.MaxCharges.Should().Be(5);
    }

    [Test]
    public void TrySpend_WithInsufficientCharges_ReturnsFalseUnchanged()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(4);

        // Act — try to spend 3 from 1 remaining
        var (success, result) = depleted.TrySpend(3);

        // Assert
        success.Should().BeFalse();
        result.CurrentCharges.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Restoration Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void RestoreAll_SetsToMaxAndUpdatesTimestamp()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(5);
        depleted.CurrentCharges.Should().Be(0);

        // Act
        var restored = depleted.RestoreAll();

        // Assert
        restored.CurrentCharges.Should().Be(5);
        restored.MaxCharges.Should().Be(5);
        restored.IsFullyCharged.Should().BeTrue();
        restored.LastGeneratedAt.Should().NotBeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Craft Generation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GenerateFromCraft_StandardCraft_AddsOneCharge()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(3);
        depleted.CurrentCharges.Should().Be(2);

        // Act
        var generated = depleted.GenerateFromCraft(isComplexCraft: false);

        // Assert
        generated.CurrentCharges.Should().Be(3);
        generated.LastGeneratedAt.Should().NotBeNull();
    }

    [Test]
    public void GenerateFromCraft_ComplexCraft_AddsTwoCharges()
    {
        // Arrange
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(3);
        depleted.CurrentCharges.Should().Be(2);

        // Act
        var generated = depleted.GenerateFromCraft(isComplexCraft: true);

        // Assert
        generated.CurrentCharges.Should().Be(4);
        generated.LastGeneratedAt.Should().NotBeNull();
    }

    [Test]
    public void GenerateFromCraft_CapsAtMaxCharges()
    {
        // Arrange — only 1 charge missing
        var resource = RuneChargeResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(1);
        depleted.CurrentCharges.Should().Be(4);

        // Act — complex craft would add 2, but is capped at max
        var generated = depleted.GenerateFromCraft(isComplexCraft: true);

        // Assert
        generated.CurrentCharges.Should().Be(5);
        generated.IsFullyCharged.Should().BeTrue();
    }
}
