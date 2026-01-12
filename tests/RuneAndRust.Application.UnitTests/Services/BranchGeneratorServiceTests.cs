using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for BranchGeneratorService.
/// </summary>
[TestFixture]
public class BranchGeneratorServiceTests
{
    private SeededRandomService _random = null!;
    private BranchGeneratorService _service = null!;
    private const int TestSeed = 12345;

    [SetUp]
    public void SetUp()
    {
        _random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        _service = new BranchGeneratorService(_random);
    }

    [Test]
    public void DecideBranching_EnsuresAtLeastOneMainPath()
    {
        // Arrange
        var position = new Position3D(0, 0, 0);
        var exits = new[] { Direction.North, Direction.East, Direction.South };

        // Act
        var result = _service.DecideBranching(position, exits, _ => false);

        // Assert
        result.AllExits.Should().NotBeEmpty();
        result.IsDeadEnd.Should().BeFalse();
    }

    [Test]
    public void DecideBranching_RespectsMainPathProbability()
    {
        // Arrange
        var rules = new BranchRules { MainPathProbability = 0.9f };
        var mainPathCount = 0;

        // Act
        for (int i = 0; i < 100; i++)
        {
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new BranchGeneratorService(random, rules);
            var position = new Position3D(i, 0, 0);
            var result = service.DecideBranching(position, new[] { Direction.North }, _ => false);

            if (result.MainPaths.Any())
                mainPathCount++;
        }

        // Assert - with 90% probability, should have high main path count
        mainPathCount.Should().BeGreaterThan(70);
    }

    [Test]
    public void DecideBranching_CreatesSidePaths()
    {
        // Arrange
        var rules = new BranchRules { MainPathProbability = 0.3f, SidePathProbability = 0.5f };
        var sidePathCount = 0;

        // Act
        for (int i = 0; i < 100; i++)
        {
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new BranchGeneratorService(random, rules);
            var position = new Position3D(i, i, 0);
            var result = service.DecideBranching(position, new[] { Direction.North, Direction.East }, _ => false);

            sidePathCount += result.SidePaths.Count();
        }

        // Assert - should have some side paths
        sidePathCount.Should().BeGreaterThan(20);
    }

    [Test]
    public void DecideBranching_CreatesDeadEnds()
    {
        // Arrange
        var rules = new BranchRules { MainPathProbability = 0.2f, SidePathProbability = 0.2f, DeadEndProbability = 0.4f };
        var deadEndCount = 0;

        // Act
        for (int i = 0; i < 100; i++)
        {
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new BranchGeneratorService(random, rules);
            var position = new Position3D(i, i, 0);
            var result = service.DecideBranching(position, new[] { Direction.North, Direction.East }, _ => false);

            deadEndCount += result.DeadEnds.Count();
        }

        // Assert - should have some dead ends
        deadEndCount.Should().BeGreaterThan(10);
    }

    [Test]
    public void DecideBranching_DetectsExistingRoom_AsLoop()
    {
        // Arrange
        var position = new Position3D(0, 0, 0);
        var exits = new[] { Direction.North };
        var rules = new BranchRules { LoopProbability = 1.0f }; // Always loop

        var random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service = new BranchGeneratorService(random, rules);

        // Act - target position already has a room
        var result = service.DecideBranching(position, exits, pos => pos == new Position3D(0, 1, 0));

        // Assert
        result.Loops.Should().Contain(Direction.North);
    }

    [Test]
    public void DecideBranching_BlocksLoop_WhenRollExceedsProbability()
    {
        // Arrange
        var position = new Position3D(0, 0, 0);
        var exits = new[] { Direction.North };
        var rules = new BranchRules { LoopProbability = 0.0f }; // Never loop

        var random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service = new BranchGeneratorService(random, rules);

        // Act - target position already has a room
        var result = service.DecideBranching(position, exits, pos => pos == new Position3D(0, 1, 0));

        // Assert
        result.Loops.Should().BeEmpty();
        result.ExitDecisions[Direction.North].Should().Be(BranchType.None);
    }

    [Test]
    public void DecideBranching_SeededRandom_IsDeterministic()
    {
        // Arrange
        var position = new Position3D(5, 5, 2);
        var exits = new[] { Direction.North, Direction.East, Direction.South };

        // Act
        var random1 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service1 = new BranchGeneratorService(random1);
        var result1 = service1.DecideBranching(position, exits, _ => false);

        var random2 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service2 = new BranchGeneratorService(random2);
        var result2 = service2.DecideBranching(position, exits, _ => false);

        // Assert
        result1.ExitDecisions.Should().BeEquivalentTo(result2.ExitDecisions);
    }

    [Test]
    public void GenerateDeadEndContent_TreasureCache_WithinProbability()
    {
        // Arrange
        var rules = new BranchRules
        {
            DeadEndChances = new DeadEndContentChances { TreasureCache = 1.0f } // Always treasure
        };
        var random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service = new BranchGeneratorService(random, rules);
        var position = new Position3D(3, 3, 1);
        var difficulty = DifficultyRating.Starting;

        // Act
        var result = service.GenerateDeadEndContent(position, difficulty);

        // Assert
        result.Should().Be(DeadEndContent.TreasureCache);
    }

    [Test]
    public void GenerateDeadEndContent_SeededRandom_IsDeterministic()
    {
        // Arrange
        var position = new Position3D(7, 2, 3);
        var difficulty = new DifficultyRating { Level = 25, RoomTypeModifier = 1.0f };

        // Act
        var random1 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service1 = new BranchGeneratorService(random1);
        var result1 = service1.GenerateDeadEndContent(position, difficulty);

        var random2 = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        var service2 = new BranchGeneratorService(random2);
        var result2 = service2.GenerateDeadEndContent(position, difficulty);

        // Assert
        result1.Should().Be(result2);
    }
}
