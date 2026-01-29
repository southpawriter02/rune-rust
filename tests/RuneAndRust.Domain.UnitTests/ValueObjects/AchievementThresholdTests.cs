namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="AchievementThreshold"/> value object.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>Count-based thresholds are created correctly</description></item>
///   <item><description>Class-based thresholds normalize class IDs</description></item>
///   <item><description>Default thresholds contain expected values</description></item>
///   <item><description>Validation rejects invalid inputs</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AchievementThresholdTests
{
    #region CreateCountBased Tests

    /// <summary>
    /// Verifies that a count-based threshold is created with correct properties.
    /// </summary>
    [Test]
    public void CreateCountBased_WithValidData_CreatesThreshold()
    {
        // Act
        var threshold = AchievementThreshold.CreateCountBased(
            UniqueAchievementType.CollectorBronze,
            "Test Bronze",
            "Find 5 items",
            requiredCount: 5);

        // Assert
        threshold.Type.Should().Be(UniqueAchievementType.CollectorBronze);
        threshold.DisplayName.Should().Be("Test Bronze");
        threshold.Description.Should().Be("Find 5 items");
        threshold.RequiredCount.Should().Be(5);
        threshold.ClassId.Should().BeNull();
    }

    /// <summary>
    /// Verifies that null display name throws ArgumentException.
    /// </summary>
    [Test]
    public void CreateCountBased_WithNullDisplayName_ThrowsArgumentException()
    {
        // Act
        var act = () => AchievementThreshold.CreateCountBased(
            UniqueAchievementType.CollectorBronze,
            null!,
            "Find items",
            requiredCount: 5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("displayName");
    }

    /// <summary>
    /// Verifies that negative required count throws ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void CreateCountBased_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => AchievementThreshold.CreateCountBased(
            UniqueAchievementType.CollectorBronze,
            "Bronze",
            "Find items",
            requiredCount: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("requiredCount");
    }

    #endregion

    #region CreateClassBased Tests

    /// <summary>
    /// Verifies that class-based thresholds normalize class IDs to lowercase.
    /// </summary>
    [Test]
    public void CreateClassBased_NormalizesClassIdToLowercase()
    {
        // Act
        var threshold = AchievementThreshold.CreateClassBased(
            "Warrior Master",
            "Find all warrior uniques",
            "WARRIOR");

        // Assert
        threshold.Type.Should().Be(UniqueAchievementType.ClassMaster);
        threshold.ClassId.Should().Be("warrior");
        threshold.RequiredCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that null class ID throws ArgumentException.
    /// </summary>
    [Test]
    public void CreateClassBased_WithNullClassId_ThrowsArgumentException()
    {
        // Act
        var act = () => AchievementThreshold.CreateClassBased(
            "Warrior Master",
            "Find all warrior uniques",
            null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("classId");
    }

    #endregion

    #region Defaults Tests

    /// <summary>
    /// Verifies that Defaults.All contains the four standard thresholds.
    /// </summary>
    [Test]
    public void Defaults_All_ContainsFourThresholds()
    {
        // Act
        var defaults = AchievementThreshold.Defaults.All;

        // Assert
        defaults.Should().HaveCount(4);
        defaults.Should().Contain(t => t.Type == UniqueAchievementType.FirstMythForged);
        defaults.Should().Contain(t => t.Type == UniqueAchievementType.CollectorBronze);
        defaults.Should().Contain(t => t.Type == UniqueAchievementType.CollectorSilver);
        defaults.Should().Contain(t => t.Type == UniqueAchievementType.CollectorGold);
    }

    /// <summary>
    /// Verifies that default thresholds have correct required counts.
    /// </summary>
    [Test]
    [TestCase(UniqueAchievementType.FirstMythForged, 1)]
    [TestCase(UniqueAchievementType.CollectorBronze, 5)]
    [TestCase(UniqueAchievementType.CollectorSilver, 15)]
    [TestCase(UniqueAchievementType.CollectorGold, 0)]
    public void Defaults_HaveCorrectRequiredCounts(
        UniqueAchievementType type,
        int expectedCount)
    {
        // Act
        var threshold = AchievementThreshold.Defaults.All
            .First(t => t.Type == type);

        // Assert
        threshold.RequiredCount.Should().Be(expectedCount);
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies that ToString returns a useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var threshold = AchievementThreshold.CreateCountBased(
            UniqueAchievementType.CollectorBronze,
            "Collector: Bronze",
            "Find 5 items",
            5);

        // Act
        var result = threshold.ToString();

        // Assert
        result.Should().Contain("AchievementThreshold");
        result.Should().Contain("CollectorBronze");
        result.Should().Contain("Collector: Bronze");
    }

    #endregion
}
