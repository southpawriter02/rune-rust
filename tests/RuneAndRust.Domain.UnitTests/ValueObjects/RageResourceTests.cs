// ═══════════════════════════════════════════════════════════════════════════════
// RageResourceTests.cs
// Unit tests for the RageResource value object covering initialization,
// gain/spend operations, threshold classification, decay, and display.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class RageResourceTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Factory / Initialization
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Create_Default_InitializesAtZeroWithDefaultMax()
    {
        // Arrange & Act
        var rage = RageResource.Create();

        // Assert
        rage.CurrentRage.Should().Be(0);
        rage.MaxRage.Should().Be(RageResource.DefaultMaxRage);
        rage.IsEnraged.Should().BeFalse();
        rage.GetRageLevel().Should().Be(RageLevel.Calm);
    }

    [Test]
    public void Create_WithCustomMax_SetsMaxRage()
    {
        // Arrange & Act
        var rage = RageResource.Create(maxRage: 50);

        // Assert
        rage.CurrentRage.Should().Be(0);
        rage.MaxRage.Should().Be(50);
    }

    [Test]
    public void Create_WithZeroMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => RageResource.Create(maxRage: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateAt_WithValidValues_SetsCurrentRage()
    {
        // Arrange & Act
        var rage = RageResource.CreateAt(75);

        // Assert
        rage.CurrentRage.Should().Be(75);
        rage.MaxRage.Should().Be(100);
        rage.GetRageLevel().Should().Be(RageLevel.Furious);
    }

    [Test]
    public void CreateAt_ExceedsMax_CapsAtMax()
    {
        // Arrange & Act
        var rage = RageResource.CreateAt(150, maxRage: 100);

        // Assert
        rage.CurrentRage.Should().Be(100);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gain
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Gain_AddsRageWithSource()
    {
        // Arrange
        var rage = RageResource.Create();

        // Act
        rage.Gain(25, "Test Source");

        // Assert
        rage.CurrentRage.Should().Be(25);
        rage.GainSource.Should().Be("Test Source");
        rage.LastGainedAt.Should().NotBeNull();
    }

    [Test]
    public void Gain_CapsAtMaxRage()
    {
        // Arrange
        var rage = RageResource.CreateAt(90);

        // Act
        rage.Gain(20, "Overflow Test");

        // Assert
        rage.CurrentRage.Should().Be(100);
    }

    [Test]
    public void GainFromDamage_AddsPainIsFuelGain()
    {
        // Arrange
        var rage = RageResource.Create();

        // Act
        rage.GainFromDamage(50);

        // Assert
        rage.CurrentRage.Should().Be(RageResource.PainIsFuelGain);
        rage.GainSource.Should().Be("Pain is Fuel");
    }

    [Test]
    public void GainFromBloodied_AddsBloodScentGain()
    {
        // Arrange
        var rage = RageResource.Create();

        // Act
        rage.GainFromBloodied();

        // Assert
        rage.CurrentRage.Should().Be(RageResource.BloodScentGain);
        rage.GainSource.Should().Be("Blood Scent");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spend
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Spend_WithSufficientRage_ReturnsTrue()
    {
        // Arrange
        var rage = RageResource.CreateAt(50);

        // Act
        var result = rage.Spend(20);

        // Assert
        result.Should().BeTrue();
        rage.CurrentRage.Should().Be(30);
    }

    [Test]
    public void Spend_WithInsufficientRage_ReturnsFalse()
    {
        // Arrange
        var rage = RageResource.CreateAt(10);

        // Act
        var result = rage.Spend(20);

        // Assert
        result.Should().BeFalse();
        rage.CurrentRage.Should().Be(10); // Unchanged
    }

    [Test]
    public void Spend_WithExactAmount_ReturnsTrue()
    {
        // Arrange
        var rage = RageResource.CreateAt(20);

        // Act
        var result = rage.Spend(20);

        // Assert
        result.Should().BeTrue();
        rage.CurrentRage.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Decay
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void DecayOutOfCombat_ReducesByDecayAmount()
    {
        // Arrange
        var rage = RageResource.CreateAt(50);

        // Act
        rage.DecayOutOfCombat();

        // Assert
        rage.CurrentRage.Should().Be(40);
    }

    [Test]
    public void DecayOutOfCombat_ClampsAtZero()
    {
        // Arrange
        var rage = RageResource.CreateAt(5);

        // Act
        rage.DecayOutOfCombat();

        // Assert
        rage.CurrentRage.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Threshold Classification
    // ─────────────────────────────────────────────────────────────────────────

    [TestCase(0, RageLevel.Calm)]
    [TestCase(19, RageLevel.Calm)]
    [TestCase(20, RageLevel.Irritated)]
    [TestCase(39, RageLevel.Irritated)]
    [TestCase(40, RageLevel.Angry)]
    [TestCase(59, RageLevel.Angry)]
    [TestCase(60, RageLevel.Furious)]
    [TestCase(79, RageLevel.Furious)]
    [TestCase(80, RageLevel.Enraged)]
    [TestCase(99, RageLevel.Enraged)]
    [TestCase(100, RageLevel.Berserk)]
    public void GetRageLevel_ReturnsCorrectLevel(int currentRage, RageLevel expected)
    {
        // Arrange
        var rage = RageResource.CreateAt(currentRage);

        // Act
        var level = rage.GetRageLevel();

        // Assert
        level.Should().Be(expected);
    }

    [Test]
    public void IsEnraged_At80_ReturnsTrue()
    {
        // Arrange
        var rage = RageResource.CreateAt(80);

        // Assert
        rage.IsEnraged.Should().BeTrue();
    }

    [Test]
    public void IsEnraged_At79_ReturnsFalse()
    {
        // Arrange
        var rage = RageResource.CreateAt(79);

        // Assert
        rage.IsEnraged.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetStatusString_FormatsCorrectly()
    {
        // Arrange
        var rage = RageResource.CreateAt(45);

        // Act
        var status = rage.GetStatusString();

        // Assert
        status.Should().Be("45/100 [Angry]");
    }

    [Test]
    public void GetFormattedValue_FormatsCorrectly()
    {
        // Arrange
        var rage = RageResource.CreateAt(75);

        // Act
        var formatted = rage.GetFormattedValue();

        // Assert
        formatted.Should().Be("75/100");
    }

    [Test]
    public void GetPercentage_CalculatesCorrectly()
    {
        // Arrange
        var rage = RageResource.CreateAt(50);

        // Act
        var pct = rage.GetPercentage();

        // Assert
        pct.Should().Be(50);
    }
}
