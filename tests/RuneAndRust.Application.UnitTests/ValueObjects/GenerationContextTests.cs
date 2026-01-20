using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.ValueObjects;

[TestFixture]
public class GenerationContextTests
{
    [Test]
    public void CreatePositionSeed_SamePosition_ReturnsSameSeed()
    {
        // Arrange
        var position = new Position3D(1, 2, 3);
        var baseSeed = 12345;

        // Act
        var seed1 = GenerationContext.CreatePositionSeed(position, baseSeed);
        var seed2 = GenerationContext.CreatePositionSeed(position, baseSeed);

        // Assert
        seed1.Should().Be(seed2);
    }

    [Test]
    public void CreatePositionSeed_DifferentPositions_ReturnsDifferentSeeds()
    {
        // Arrange
        var position1 = new Position3D(1, 2, 3);
        var position2 = new Position3D(4, 5, 6);
        var baseSeed = 12345;

        // Act
        var seed1 = GenerationContext.CreatePositionSeed(position1, baseSeed);
        var seed2 = GenerationContext.CreatePositionSeed(position2, baseSeed);

        // Assert
        seed1.Should().NotBe(seed2);
    }

    [Test]
    public void Depth_ReturnsPositionZ()
    {
        // Arrange
        var context = new GenerationContext(
            new Position3D(0, 0, 5),
            12345,
            "dungeon",
            1.5f);

        // Act & Assert
        context.Depth.Should().Be(5);
    }

    [Test]
    public void CreateRandom_ReturnsDeterministicRandom()
    {
        // Arrange
        var context = new GenerationContext(
            new Position3D(0, 0, 0),
            12345,
            "dungeon",
            1.0f);

        // Act
        var random1 = context.CreateRandom();
        var value1 = random1.Next(100);
        
        var random2 = context.CreateRandom();
        var value2 = random2.Next(100);

        // Assert - Same seed produces same first value
        value1.Should().Be(value2);
    }
}
