using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Dungeon seed properties.
/// </summary>
[TestFixture]
public class DungeonSeedTests
{
    [Test]
    public void Constructor_WithSeed_StoresSeed()
    {
        // Arrange & Act
        var dungeon = new Dungeon("Test Dungeon", 305419896);

        // Assert
        dungeon.Seed.Should().Be(305419896);
        dungeon.Name.Should().Be("Test Dungeon");
    }

    [Test]
    public void Constructor_WithoutSeed_HasZeroSeed()
    {
        // Arrange & Act
        var dungeon = new Dungeon("Test Dungeon");

        // Assert
        dungeon.Seed.Should().Be(0);
    }

    [Test]
    public void CreateSeeded_CreatesSeededDungeon()
    {
        // Arrange & Act
        var dungeon = Dungeon.CreateSeeded("Shared Dungeon", 12345678);

        // Assert
        dungeon.Seed.Should().Be(12345678);
        dungeon.Name.Should().Be("Shared Dungeon");
    }
}
