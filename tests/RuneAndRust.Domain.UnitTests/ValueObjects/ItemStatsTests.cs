using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ItemStats"/> value object.
/// </summary>
[TestFixture]
public class ItemStatsTests
{
    /// <summary>
    /// Verifies that Create with default parameters returns empty stats.
    /// </summary>
    [Test]
    public void Create_WithDefaults_ReturnsEmptyStats()
    {
        // Arrange & Act
        var stats = ItemStats.Create();

        // Assert
        stats.IsEmpty.Should().BeTrue();
        stats.Might.Should().Be(0);
        stats.Agility.Should().Be(0);
        stats.Will.Should().Be(0);
        stats.Fortitude.Should().Be(0);
        stats.Arcana.Should().Be(0);
        stats.BonusHealth.Should().Be(0);
        stats.BonusDamage.Should().Be(0);
        stats.BonusDefense.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Create with all stats sets all values correctly.
    /// </summary>
    [Test]
    public void Create_WithAllStats_SetsAllValues()
    {
        // Arrange & Act
        var stats = ItemStats.Create(
            might: 5,
            agility: 3,
            will: 2,
            fortitude: 4,
            arcana: 1,
            bonusHealth: 50,
            bonusDamage: 10,
            bonusDefense: 15);

        // Assert
        stats.Might.Should().Be(5);
        stats.Agility.Should().Be(3);
        stats.Will.Should().Be(2);
        stats.Fortitude.Should().Be(4);
        stats.Arcana.Should().Be(1);
        stats.BonusHealth.Should().Be(50);
        stats.BonusDamage.Should().Be(10);
        stats.BonusDefense.Should().Be(15);
        stats.IsEmpty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create allows negative stats for cursed items.
    /// </summary>
    [Test]
    public void Create_WithNegativeStats_AllowsCursedItems()
    {
        // Arrange & Act
        var stats = ItemStats.Create(might: -2, bonusHealth: -10);

        // Assert
        stats.Might.Should().Be(-2);
        stats.BonusHealth.Should().Be(-10);
        stats.IsEmpty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Empty returns all zero stats.
    /// </summary>
    [Test]
    public void Empty_ReturnsAllZeroStats()
    {
        // Arrange & Act
        var stats = ItemStats.Empty;

        // Assert
        stats.Might.Should().Be(0);
        stats.Agility.Should().Be(0);
        stats.Will.Should().Be(0);
        stats.Fortitude.Should().Be(0);
        stats.Arcana.Should().Be(0);
        stats.BonusHealth.Should().Be(0);
        stats.BonusDamage.Should().Be(0);
        stats.BonusDefense.Should().Be(0);
        stats.IsEmpty.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ToString formats stats correctly.
    /// </summary>
    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var stats = ItemStats.Create(might: 5, bonusDamage: 10);

        // Act
        var result = stats.ToString();

        // Assert
        result.Should().Contain("M:5");
        result.Should().Contain("DMG:10");
        result.Should().StartWith("Stats[");
        result.Should().EndWith("]");
    }

    /// <summary>
    /// Verifies that stats with same values are equal (record struct behavior).
    /// </summary>
    [Test]
    public void Equality_WithSameValues_AreEqual()
    {
        // Arrange
        var stats1 = ItemStats.Create(might: 5, agility: 3);
        var stats2 = ItemStats.Create(might: 5, agility: 3);

        // Assert
        stats1.Should().Be(stats2);
        (stats1 == stats2).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that NonZeroStatCount returns correct count.
    /// </summary>
    [Test]
    public void NonZeroStatCount_ReturnsCorrectCount()
    {
        // Arrange
        var emptyStats = ItemStats.Empty;
        var partialStats = ItemStats.Create(might: 5, agility: 3, bonusDamage: 10);
        var fullStats = ItemStats.Create(1, 2, 3, 4, 5, 6, 7, 8);

        // Assert
        emptyStats.NonZeroStatCount.Should().Be(0);
        partialStats.NonZeroStatCount.Should().Be(3);
        fullStats.NonZeroStatCount.Should().Be(8);
    }
}
