using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the ExplorationStatistics value object.
/// </summary>
[TestFixture]
public class ExplorationStatisticsTests
{
    [Test]
    public void ExplorationPercent_CalculatesCorrectly()
    {
        // Arrange
        var stats = new ExplorationStatistics(
            TotalRooms: 10,
            VisitedRooms: 5,
            ClearedRooms: 2,
            ExploredLevels: new[] { 0, 1 });

        // Act & Assert
        stats.ExplorationPercent.Should().Be(50f);
        stats.ClearedPercent.Should().Be(20f);
        stats.UnexploredRooms.Should().Be(5);
    }

    [Test]
    public void ExplorationPercent_ZeroTotal_ReturnsZero()
    {
        // Arrange
        var stats = new ExplorationStatistics(
            TotalRooms: 0,
            VisitedRooms: 0,
            ClearedRooms: 0,
            ExploredLevels: Array.Empty<int>());

        // Act & Assert
        stats.ExplorationPercent.Should().Be(0f);
        stats.ClearedPercent.Should().Be(0f);
    }

    [Test]
    public void DeepestLevel_ReturnsMaxLevel()
    {
        // Arrange
        var stats = new ExplorationStatistics(
            TotalRooms: 10,
            VisitedRooms: 5,
            ClearedRooms: 2,
            ExploredLevels: new[] { 0, 1, 3, 2 });

        // Act & Assert
        stats.DeepestLevel.Should().Be(3);
    }

    [Test]
    public void DeepestLevel_EmptyLevels_ReturnsZero()
    {
        // Arrange
        var stats = ExplorationStatistics.Empty;

        // Act & Assert
        stats.DeepestLevel.Should().Be(0);
    }
}
