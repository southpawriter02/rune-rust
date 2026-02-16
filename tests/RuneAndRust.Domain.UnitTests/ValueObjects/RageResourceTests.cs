using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="RageResource"/> value object.
/// </summary>
[TestFixture]
public class RageResourceTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesAtZeroWithDefaultMax()
    {
        // Act
        var resource = RageResource.Create();

        // Assert
        resource.CurrentRage.Should().Be(0);
        resource.MaxRage.Should().Be(RageResource.DefaultMaxRage);
        resource.LastModifiedAt.Should().NotBeNull();
        resource.LastModificationSource.Should().Be("Initialized");
    }

    [Test]
    public void Create_WithCustomMax_InitializesCorrectly()
    {
        // Act
        var resource = RageResource.Create(50);

        // Assert
        resource.CurrentRage.Should().Be(0);
        resource.MaxRage.Should().Be(50);
    }

    [Test]
    public void Create_WithZeroOrNegativeMax_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        FluentActions.Invoking(() => RageResource.Create(0))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => RageResource.Create(-5))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateAt_InitializesAtSpecifiedValue()
    {
        // Act
        var resource = RageResource.CreateAt(50, 100);

        // Assert
        resource.CurrentRage.Should().Be(50);
        resource.MaxRage.Should().Be(100);
    }

    [Test]
    public void CreateAt_ClampsToMax()
    {
        // Act
        var resource = RageResource.CreateAt(150, 100);

        // Assert
        resource.CurrentRage.Should().Be(100); // Clamped to max
    }

    [Test]
    public void CreateAt_ClampsToZero()
    {
        // Act
        var resource = RageResource.CreateAt(-10, 100);

        // Assert
        resource.CurrentRage.Should().Be(0); // Clamped to 0
    }

    // ===== Gain Tests =====

    [Test]
    public void Gain_AddsRageUpToMax()
    {
        // Arrange
        var resource = RageResource.Create();

        // Act
        var gained = resource.Gain(30, "Test");

        // Assert
        gained.Should().Be(30);
        resource.CurrentRage.Should().Be(30);
        resource.LastModificationSource.Should().Be("Test");
    }

    [Test]
    public void Gain_CapsAtMax()
    {
        // Arrange
        var resource = RageResource.CreateAt(90, 100);

        // Act
        var gained = resource.Gain(20, "Test");

        // Assert
        gained.Should().Be(10); // Only 10 gained (capped at 100)
        resource.CurrentRage.Should().Be(100);
    }

    [Test]
    public void Gain_WithZeroOrNegative_ReturnsZero()
    {
        // Arrange
        var resource = RageResource.Create();

        // Act & Assert
        resource.Gain(0, "Test").Should().Be(0);
        resource.Gain(-5, "Test").Should().Be(0);
        resource.CurrentRage.Should().Be(0); // Unchanged
    }

    [Test]
    public void GainFromDamage_AddsPainIsFuelAmount()
    {
        // Arrange
        var resource = RageResource.Create();

        // Act
        var gained = resource.GainFromDamage(10);

        // Assert
        gained.Should().Be(RageResource.PainIsFuelGain);
        resource.CurrentRage.Should().Be(RageResource.PainIsFuelGain);
    }

    [Test]
    public void GainFromDamage_WithZeroDamage_ReturnsZero()
    {
        // Arrange
        var resource = RageResource.Create();

        // Act
        var gained = resource.GainFromDamage(0);

        // Assert
        gained.Should().Be(0);
        resource.CurrentRage.Should().Be(0);
    }

    [Test]
    public void GainFromBloodied_AddsBloodScentAmount()
    {
        // Arrange
        var resource = RageResource.Create();

        // Act
        var gained = resource.GainFromBloodied();

        // Assert
        gained.Should().Be(RageResource.BloodScentGain);
        resource.CurrentRage.Should().Be(RageResource.BloodScentGain);
    }

    // ===== Spend Tests =====

    [Test]
    public void Spend_WithSufficientRage_ReturnsTrue()
    {
        // Arrange
        var resource = RageResource.CreateAt(50, 100);

        // Act
        var result = resource.Spend(20);

        // Assert
        result.Should().BeTrue();
        resource.CurrentRage.Should().Be(30);
    }

    [Test]
    public void Spend_WithInsufficientRage_ReturnsFalse()
    {
        // Arrange
        var resource = RageResource.CreateAt(10, 100);

        // Act
        var result = resource.Spend(20);

        // Assert
        result.Should().BeFalse();
        resource.CurrentRage.Should().Be(10); // Unchanged
    }

    [Test]
    public void Spend_WithZeroOrNegative_ReturnsFalse()
    {
        // Arrange
        var resource = RageResource.CreateAt(50, 100);

        // Act & Assert
        resource.Spend(0).Should().BeFalse();
        resource.Spend(-5).Should().BeFalse();
        resource.CurrentRage.Should().Be(50); // Unchanged
    }

    // ===== Decay Tests =====

    [Test]
    public void DecayOutOfCombat_ReducesByDecayAmount()
    {
        // Arrange
        var resource = RageResource.CreateAt(50, 100);

        // Act
        var decayed = resource.DecayOutOfCombat();

        // Assert
        decayed.Should().Be(RageResource.OutOfCombatDecay);
        resource.CurrentRage.Should().Be(40);
    }

    [Test]
    public void DecayOutOfCombat_DoesNotGoBelowZero()
    {
        // Arrange
        var resource = RageResource.CreateAt(5, 100);

        // Act
        var decayed = resource.DecayOutOfCombat();

        // Assert
        decayed.Should().Be(5); // Only 5 lost (capped at 0)
        resource.CurrentRage.Should().Be(0);
    }

    // ===== Threshold Classification Tests =====

    [Test]
    public void GetRageLevel_ReturnsCorrectLevel()
    {
        // Calm (0-19)
        RageResource.CreateAt(0, 100).GetRageLevel().Should().Be(RageLevel.Calm);
        RageResource.CreateAt(19, 100).GetRageLevel().Should().Be(RageLevel.Calm);

        // Irritated (20-39)
        RageResource.CreateAt(20, 100).GetRageLevel().Should().Be(RageLevel.Irritated);
        RageResource.CreateAt(39, 100).GetRageLevel().Should().Be(RageLevel.Irritated);

        // Angry (40-59)
        RageResource.CreateAt(40, 100).GetRageLevel().Should().Be(RageLevel.Angry);
        RageResource.CreateAt(59, 100).GetRageLevel().Should().Be(RageLevel.Angry);

        // Furious (60-79)
        RageResource.CreateAt(60, 100).GetRageLevel().Should().Be(RageLevel.Furious);
        RageResource.CreateAt(79, 100).GetRageLevel().Should().Be(RageLevel.Furious);

        // Enraged (80-99)
        RageResource.CreateAt(80, 100).GetRageLevel().Should().Be(RageLevel.Enraged);
        RageResource.CreateAt(99, 100).GetRageLevel().Should().Be(RageLevel.Enraged);

        // Berserk (100)
        RageResource.CreateAt(100, 100).GetRageLevel().Should().Be(RageLevel.Berserk);
    }

    [Test]
    public void IsAtThreshold_ReturnsTrueWhenMet()
    {
        // Arrange
        var resource = RageResource.CreateAt(85, 100);

        // Assert
        resource.IsAtThreshold(RageLevel.Calm).Should().BeTrue();
        resource.IsAtThreshold(RageLevel.Enraged).Should().BeTrue();
        resource.IsAtThreshold(RageLevel.Berserk).Should().BeFalse();
    }

    [Test]
    public void IsEnraged_ReturnsTrueAt80Plus()
    {
        // Assert
        RageResource.CreateAt(79, 100).IsEnraged.Should().BeFalse();
        RageResource.CreateAt(80, 100).IsEnraged.Should().BeTrue();
        RageResource.CreateAt(100, 100).IsEnraged.Should().BeTrue();
    }

    // ===== Display Tests =====

    [Test]
    public void GetStatusString_ReturnsFormattedString()
    {
        // Arrange
        var resource = RageResource.CreateAt(75, 100);

        // Act
        var status = resource.GetStatusString();

        // Assert
        status.Should().Be("Rage: 75/100 [Furious]");
    }

    [Test]
    public void GetFormattedValue_ReturnsValueSlashMax()
    {
        // Arrange
        var resource = RageResource.CreateAt(42, 100);

        // Assert
        resource.GetFormattedValue().Should().Be("42/100");
    }

    [Test]
    public void GetPercentage_ReturnsCorrectPercentage()
    {
        // Assert
        RageResource.CreateAt(50, 100).GetPercentage().Should().Be(50);
        RageResource.CreateAt(0, 100).GetPercentage().Should().Be(0);
        RageResource.CreateAt(100, 100).GetPercentage().Should().Be(100);
    }
}
