// ═══════════════════════════════════════════════════════════════════════════════
// EmpoweredRuneTests.cs
// Unit tests for the EmpoweredRune value object, validating creation,
// tick/expiry behavior, element validation, and display methods.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="EmpoweredRune"/>.
/// </summary>
[TestFixture]
public class EmpoweredRuneTests
{
    private readonly Guid _weaponId = Guid.NewGuid();

    // ─────────────────────────────────────────────────────────────────────────
    // Creation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Create_WithValidFireElement_CreatesRuneWithCorrectDefaults()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(_weaponId, "fire");

        // Assert
        rune.TargetItemId.Should().Be(_weaponId);
        rune.ElementalDamageTypeId.Should().Be("fire");
        rune.BonusDice.Should().Be("1d6");
        rune.RemainingDuration.Should().Be(10);
        rune.OriginalDuration.Should().Be(10);
        rune.IsExpired.Should().BeFalse();
        rune.RuneId.Should().NotBeEmpty();
    }

    [Test]
    public void Create_WithMixedCaseElement_NormalizesToLowercase()
    {
        // Arrange & Act
        var rune = EmpoweredRune.Create(_weaponId, "Lightning");

        // Assert
        rune.ElementalDamageTypeId.Should().Be("lightning");
    }

    [Test]
    public void Create_WithInvalidElement_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => EmpoweredRune.Create(_weaponId, "poison");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid elemental damage type*");
    }

    [Test]
    public void Create_WithNullElement_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => EmpoweredRune.Create(_weaponId, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tick & Expiry Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Tick_DecrementsDurationByOne()
    {
        // Arrange
        var rune = EmpoweredRune.Create(_weaponId, "cold");

        // Act
        var ticked = rune.Tick();

        // Assert
        ticked.RemainingDuration.Should().Be(9);
        ticked.IsExpired.Should().BeFalse();
    }

    [Test]
    public void Tick_DoesNotModifyOriginalInstance()
    {
        // Arrange
        var rune = EmpoweredRune.Create(_weaponId, "fire");

        // Act
        _ = rune.Tick();

        // Assert — original unchanged
        rune.RemainingDuration.Should().Be(10);
    }

    [Test]
    public void Tick_AtZeroDuration_ClampsToZero()
    {
        // Arrange
        var rune = EmpoweredRune.Create(_weaponId, "fire");

        // Act — tick all the way down and one more
        for (var i = 0; i < EmpoweredRune.DefaultDuration; i++)
            rune = rune.Tick();

        var expiredTick = rune.Tick();

        // Assert
        rune.RemainingDuration.Should().Be(0);
        rune.IsExpired.Should().BeTrue();
        expiredTick.RemainingDuration.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Element Validation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [TestCase("fire", true)]
    [TestCase("cold", true)]
    [TestCase("lightning", true)]
    [TestCase("aetheric", true)]
    [TestCase("Fire", true)]
    [TestCase("COLD", true)]
    [TestCase("poison", false)]
    [TestCase("physical", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsValidElement_ReturnsCorrectResult(string? element, bool expected)
    {
        // Act
        var result = EmpoweredRune.IsValidElement(element!);

        // Assert
        result.Should().Be(expected);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetElementDisplay_ReturnsFlavorText()
    {
        // Arrange
        var rune = EmpoweredRune.Create(_weaponId, "fire");

        // Act
        var display = rune.GetElementDisplay();

        // Assert
        display.Should().Contain("Fire");
        display.Should().Contain("flame");
    }

    [Test]
    public void GetAverageDamage_ReturnsThreePointFive()
    {
        // Arrange
        var rune = EmpoweredRune.Create(_weaponId, "cold");

        // Act
        var avg = rune.GetAverageDamage();

        // Assert
        avg.Should().Be(3.5m);
    }
}
