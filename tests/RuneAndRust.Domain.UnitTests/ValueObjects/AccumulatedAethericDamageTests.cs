using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AccumulatedAethericDamage"/> value object.
/// Covers factory method, damage accumulation (immutable pattern), reset, and display.
/// </summary>
[TestFixture]
public class AccumulatedAethericDamageTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesAtZero()
    {
        // Act
        var tracker = AccumulatedAethericDamage.Create();

        // Assert
        tracker.TotalDamage.Should().Be(0);
        tracker.CastCount.Should().Be(0);
        tracker.LastDamageAt.Should().BeNull();
        tracker.CanUnravel.Should().BeFalse();
    }

    // ===== AddDamage Tests =====

    [Test]
    public void AddDamage_ReturnsNewInstanceWithDamageAdded()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create();

        // Act
        var updated = tracker.AddDamage(9);

        // Assert
        updated.TotalDamage.Should().Be(9);
        updated.CastCount.Should().Be(1);
        updated.LastDamageAt.Should().NotBeNull();
        updated.CanUnravel.Should().BeTrue();

        // Original should be unchanged (immutable pattern)
        tracker.TotalDamage.Should().Be(0);
        tracker.CastCount.Should().Be(0);
    }

    [Test]
    public void AddDamage_MultipleCasts_AccumulatesCorrectly()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create();

        // Act
        var after1 = tracker.AddDamage(7);
        var after2 = after1.AddDamage(5);
        var after3 = after2.AddDamage(11);

        // Assert
        after3.TotalDamage.Should().Be(23); // 7 + 5 + 11
        after3.CastCount.Should().Be(3);
    }

    [Test]
    public void AddDamage_NegativeValue_TreatedAsZero()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create();

        // Act
        var updated = tracker.AddDamage(-5);

        // Assert
        updated.TotalDamage.Should().Be(0);
        updated.CastCount.Should().Be(1); // Cast still counted
    }

    [Test]
    public void AddDamage_ZeroDamage_CountsCast()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create();

        // Act
        var updated = tracker.AddDamage(0);

        // Assert
        updated.TotalDamage.Should().Be(0);
        updated.CastCount.Should().Be(1);
    }

    // ===== Reset Tests =====

    [Test]
    public void Reset_ReturnsZeroedInstance()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create()
            .AddDamage(10)
            .AddDamage(15);

        // Act
        var reset = tracker.Reset();

        // Assert
        reset.TotalDamage.Should().Be(0);
        reset.CastCount.Should().Be(0);
        reset.LastDamageAt.Should().BeNull();
        reset.CanUnravel.Should().BeFalse();
    }

    // ===== Average Damage Tests =====

    [Test]
    public void GetAverageDamagePerCast_NoCasts_ReturnsZero()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create();

        // Act & Assert
        tracker.GetAverageDamagePerCast().Should().Be(0m);
    }

    [Test]
    public void GetAverageDamagePerCast_MultipleCasts_ReturnsCorrectAverage()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create()
            .AddDamage(6)
            .AddDamage(10);

        // Act & Assert
        tracker.GetAverageDamagePerCast().Should().Be(8m); // 16 / 2
    }

    // ===== Display Tests =====

    [Test]
    public void GetFormattedValue_NoCasts_ShowsNoCasts()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create();

        // Act
        var display = tracker.GetFormattedValue();

        // Assert
        display.Should().Contain("0 Aetheric damage");
        display.Should().Contain("no casts");
    }

    [Test]
    public void GetFormattedValue_WithCasts_ShowsDamageAndCasts()
    {
        // Arrange
        var tracker = AccumulatedAethericDamage.Create()
            .AddDamage(7)
            .AddDamage(9);

        // Act
        var display = tracker.GetFormattedValue();

        // Assert
        display.Should().Contain("16 Aetheric damage");
        display.Should().Contain("2 casts");
    }
}
