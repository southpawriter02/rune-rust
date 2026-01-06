using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class StatsWitsTests
{
    [Test]
    public void Constructor_WithValidWits_CreatesStats()
    {
        // Arrange & Act
        var stats = new Stats(100, 10, 5, 14);

        // Assert
        stats.Wits.Should().Be(14);
    }

    [Test]
    public void Constructor_WithDefaultWits_UsesDefaultValue()
    {
        // Arrange & Act
        var stats = new Stats(100, 10, 5);

        // Assert
        stats.Wits.Should().Be(10);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(21)]
    [TestCase(100)]
    public void Constructor_WithInvalidWits_ThrowsArgumentOutOfRangeException(int wits)
    {
        // Arrange & Act
        var act = () => new Stats(100, 10, 5, wits);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("wits");
    }

    [TestCase(1, 1)]    // 1/2 rounded up = 1
    [TestCase(2, 1)]    // 2/2 = 1
    [TestCase(3, 2)]    // 3/2 rounded up = 2
    [TestCase(10, 5)]   // 10/2 = 5
    [TestCase(11, 6)]   // 11/2 rounded up = 6
    [TestCase(14, 7)]   // 14/2 = 7
    [TestCase(15, 8)]   // 15/2 rounded up = 8
    [TestCase(20, 10)]  // 20/2 = 10
    public void PassivePerception_CalculatesCorrectly(int wits, int expectedPassive)
    {
        // Arrange
        var stats = new Stats(100, 10, 5, wits);

        // Act & Assert
        stats.PassivePerception.Should().Be(expectedPassive);
    }

    [Test]
    public void Default_HasDefaultWitsValue()
    {
        // Arrange & Act
        var stats = Stats.Default;

        // Assert
        stats.Wits.Should().Be(10);
        stats.PassivePerception.Should().Be(5);
    }

    [Test]
    public void ToString_IncludesWits()
    {
        // Arrange
        var stats = new Stats(100, 10, 5, 14);

        // Act
        var result = stats.ToString();

        // Assert
        result.Should().Contain("WITS: 14");
    }

    [Test]
    public void Stats_AreImmutable()
    {
        // Arrange
        var stats1 = new Stats(100, 10, 5, 14);

        // Act - Create a new stats with different WITS
        var stats2 = stats1 with { Wits = 16 };

        // Assert - Original should be unchanged
        stats1.Wits.Should().Be(14);
        stats2.Wits.Should().Be(16);
    }
}
