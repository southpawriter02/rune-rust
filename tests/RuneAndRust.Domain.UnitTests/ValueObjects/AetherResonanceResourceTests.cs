using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="AetherResonanceResource"/> value object.
/// Covers factory methods, build behavior, reset, Corruption risk thresholds,
/// and display methods.
/// </summary>
[TestFixture]
public class AetherResonanceResourceTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesAtZeroWithDefaultMax()
    {
        // Act
        var resource = AetherResonanceResource.Create();

        // Assert
        resource.CurrentResonance.Should().Be(0);
        resource.MaxResonance.Should().Be(AetherResonanceResource.DefaultMaxResonance);
        resource.LastModifiedAt.Should().NotBeNull();
        resource.LastModificationSource.Should().Be("Initialized");
    }

    [Test]
    public void Create_WithCustomMax_InitializesCorrectly()
    {
        // Act
        var resource = AetherResonanceResource.Create(15);

        // Assert
        resource.CurrentResonance.Should().Be(0);
        resource.MaxResonance.Should().Be(15);
    }

    [Test]
    public void Create_WithZeroOrNegativeMax_ThrowsArgumentOutOfRange()
    {
        // Act & Assert
        FluentActions.Invoking(() => AetherResonanceResource.Create(0))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => AetherResonanceResource.Create(-5))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateAt_InitializesAtSpecifiedValue()
    {
        // Act
        var resource = AetherResonanceResource.CreateAt(7, 10);

        // Assert
        resource.CurrentResonance.Should().Be(7);
        resource.MaxResonance.Should().Be(10);
    }

    [Test]
    public void CreateAt_ClampsToMax()
    {
        // Act
        var resource = AetherResonanceResource.CreateAt(15, 10);

        // Assert
        resource.CurrentResonance.Should().Be(10);
    }

    [Test]
    public void CreateAt_ClampsToZero()
    {
        // Act
        var resource = AetherResonanceResource.CreateAt(-3, 10);

        // Assert
        resource.CurrentResonance.Should().Be(0);
    }

    // ===== Build Tests =====

    [Test]
    public void Build_IncreasesResonance()
    {
        // Arrange
        var resource = AetherResonanceResource.Create();

        // Act
        var gained = resource.Build(1, "Seiðr Bolt cast");

        // Assert
        gained.Should().Be(1);
        resource.CurrentResonance.Should().Be(1);
        resource.LastModificationSource.Should().Be("Seiðr Bolt cast");
    }

    [Test]
    public void Build_CapsAtMaxResonance()
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(9, 10);

        // Act
        var gained = resource.Build(3, "Seiðr Bolt cast");

        // Assert
        gained.Should().Be(1); // Only 1 gained (9 + 3 capped to 10)
        resource.CurrentResonance.Should().Be(10);
    }

    [Test]
    public void Build_AtMaxResonance_ReturnsZero()
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(10, 10);

        // Act
        var gained = resource.Build(1, "Seiðr Bolt cast");

        // Assert
        gained.Should().Be(0);
        resource.CurrentResonance.Should().Be(10);
    }

    [Test]
    public void Build_ZeroOrNegativeAmount_ReturnsZero()
    {
        // Arrange
        var resource = AetherResonanceResource.Create();

        // Act & Assert
        resource.Build(0, "test").Should().Be(0);
        resource.Build(-1, "test").Should().Be(0);
        resource.CurrentResonance.Should().Be(0);
    }

    // ===== Reset Tests =====

    [Test]
    public void Reset_SetsResonanceToZero()
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(8, 10);

        // Act
        resource.Reset("Unraveling capstone");

        // Assert
        resource.CurrentResonance.Should().Be(0);
        resource.LastModificationSource.Should().Be("Unraveling capstone");
    }

    // ===== Corruption Risk Tests =====

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(4, 0)]
    [TestCase(5, 5)]
    [TestCase(6, 5)]
    [TestCase(7, 5)]
    [TestCase(8, 15)]
    [TestCase(9, 15)]
    [TestCase(10, 25)]
    public void GetCorruptionRiskPercent_ReturnsCorrectPercentage(int resonance, int expectedPercent)
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(resonance, 10);

        // Act & Assert
        resource.GetCorruptionRiskPercent().Should().Be(expectedPercent);
    }

    [Test]
    [TestCase(0, false)]
    [TestCase(4, false)]
    [TestCase(5, true)]
    [TestCase(10, true)]
    public void IsInCorruptionRange_ReturnsCorrectValue(int resonance, bool expected)
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(resonance, 10);

        // Act & Assert
        resource.IsInCorruptionRange.Should().Be(expected);
    }

    [Test]
    [TestCase(9, false)]
    [TestCase(10, true)]
    public void IsAtMaxResonance_ReturnsCorrectValue(int resonance, bool expected)
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(resonance, 10);

        // Act & Assert
        resource.IsAtMaxResonance.Should().Be(expected);
    }

    // ===== Resonance Level Tests =====

    [Test]
    [TestCase(0, "Safe")]
    [TestCase(4, "Safe")]
    [TestCase(5, "Risky")]
    [TestCase(7, "Risky")]
    [TestCase(8, "Dangerous")]
    [TestCase(9, "Dangerous")]
    [TestCase(10, "Critical")]
    public void GetResonanceLevel_ReturnsCorrectLevel(int resonance, string expectedLevel)
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(resonance, 10);

        // Act & Assert
        resource.GetResonanceLevel().Should().Be(expectedLevel);
    }

    // ===== Display Tests =====

    [Test]
    public void GetStatusString_ContainsResonanceAndRisk()
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(7, 10);

        // Act
        var status = resource.GetStatusString();

        // Assert
        status.Should().Contain("7/10");
        status.Should().Contain("Risky");
        status.Should().Contain("5%");
    }

    [Test]
    public void GetFormattedValue_ReturnsCorrectFormat()
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(3, 10);

        // Act & Assert
        resource.GetFormattedValue().Should().Be("3/10");
    }

    [Test]
    public void GetStatusString_SafeLevel_ShowsNoCorruptionRisk()
    {
        // Arrange
        var resource = AetherResonanceResource.CreateAt(2, 10);

        // Act
        var status = resource.GetStatusString();

        // Assert
        status.Should().Contain("Safe");
        status.Should().Contain("no Corruption risk");
    }
}
