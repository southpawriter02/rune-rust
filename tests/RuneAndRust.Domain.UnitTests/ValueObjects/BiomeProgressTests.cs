using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for BiomeProgress value object.
/// </summary>
[TestFixture]
public class BiomeProgressTests
{
    [Test]
    public void DiscoverBiome_FirstTime_ReturnsTrue()
    {
        // Arrange
        var progress = new BiomeProgress();

        // Act
        var result = progress.DiscoverBiome("stone-corridors");

        // Assert
        result.Should().BeTrue();
        progress.HasDiscovered("stone-corridors").Should().BeTrue();
    }

    [Test]
    public void DiscoverBiome_SecondTime_ReturnsFalse()
    {
        // Arrange
        var progress = new BiomeProgress();
        progress.DiscoverBiome("stone-corridors");

        // Act
        var result = progress.DiscoverBiome("stone-corridors");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void RecordRoomVisit_IncrementsCount()
    {
        // Arrange
        var progress = new BiomeProgress();

        // Act
        progress.RecordRoomVisit("stone-corridors", 1);
        progress.RecordRoomVisit("stone-corridors", 2);

        // Assert
        progress.GetRoomsVisited("stone-corridors").Should().Be(2);
    }

    [Test]
    public void RecordRoomVisit_UpdatesDeepestDepth()
    {
        // Arrange
        var progress = new BiomeProgress();

        // Act
        progress.RecordRoomVisit("stone-corridors", 1);
        progress.RecordRoomVisit("stone-corridors", 5);
        progress.RecordRoomVisit("stone-corridors", 3);

        // Assert
        progress.GetDeepestDepth("stone-corridors").Should().Be(5);
    }

    [Test]
    public void RecordMonsterDefeat_IncrementsCount()
    {
        // Arrange
        var progress = new BiomeProgress();
        progress.DiscoverBiome("stone-corridors");

        // Act
        progress.RecordMonsterDefeat("stone-corridors");
        progress.RecordMonsterDefeat("stone-corridors");

        // Assert
        progress.GetMonstersDefeated("stone-corridors").Should().Be(2);
    }
}
