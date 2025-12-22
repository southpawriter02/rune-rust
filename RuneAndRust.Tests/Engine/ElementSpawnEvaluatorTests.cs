using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Entities;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for ElementSpawnEvaluator (v0.3.8).
/// Tests all Phase 1 spawn rules: NeverInEntryHall, NeverInBossArena, OnlyInLargeRooms,
/// RequiredArchetype, RequiresRoomNameContains, HigherWeightInSecretRooms.
/// </summary>
public class ElementSpawnEvaluatorTests
{
    private readonly ILogger<ElementSpawnEvaluator> _mockLogger;
    private readonly ElementSpawnEvaluator _evaluator;

    public ElementSpawnEvaluatorTests()
    {
        _mockLogger = Substitute.For<ILogger<ElementSpawnEvaluator>>();
        _evaluator = new ElementSpawnEvaluator(_mockLogger);
    }

    [Fact]
    public void CanSpawn_NeverInEntryHall_BlocksEntryHall()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Boss Enemy",
            SpawnRules = new ElementSpawnRules { NeverInEntryHall = true }
        };

        var room = new Room { Name = "Entry Hall" };
        var template = new RoomTemplate { Archetype = "EntryHall" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanSpawn_NeverInEntryHall_AllowsNonEntryHall()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Boss Enemy",
            SpawnRules = new ElementSpawnRules { NeverInEntryHall = true }
        };

        var room = new Room { Name = "Dark Chamber" };
        var template = new RoomTemplate { Archetype = "Chamber" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSpawn_NeverInBossArena_BlocksBossArena()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Common Loot",
            SpawnRules = new ElementSpawnRules { NeverInBossArena = true }
        };

        var room = new Room { Name = "Reactor Core" };
        var template = new RoomTemplate { Archetype = "BossArena" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanSpawn_OnlyInLargeRooms_BlocksSmallRooms()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Massive Pillar",
            SpawnRules = new ElementSpawnRules { OnlyInLargeRooms = true }
        };

        var room = new Room { Name = "Tight Corridor" };
        var template = new RoomTemplate { Size = "Small" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanSpawn_OnlyInLargeRooms_AllowsLargeRooms()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Massive Pillar",
            SpawnRules = new ElementSpawnRules { OnlyInLargeRooms = true }
        };

        var room = new Room { Name = "Grand Hall" };
        var template = new RoomTemplate { Size = "Large" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSpawn_RequiredArchetype_BlocksMismatchedArchetype()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Arena-Specific Hazard",
            SpawnRules = new ElementSpawnRules { RequiredArchetype = "BossArena" }
        };

        var room = new Room { Name = "Storage Room" };
        var template = new RoomTemplate { Archetype = "Chamber" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanSpawn_RequiredArchetype_AllowsMatchingArchetype()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Arena-Specific Hazard",
            SpawnRules = new ElementSpawnRules { RequiredArchetype = "BossArena" }
        };

        var room = new Room { Name = "Reactor Core" };
        var template = new RoomTemplate { Archetype = "BossArena" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSpawn_RequiresRoomNameContains_BlocksNonMatchingNames()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Reactor-Specific Hazard",
            SpawnRules = new ElementSpawnRules
            {
                RequiresRoomNameContains = new List<string> { "Reactor", "Core" }
            }
        };

        var room = new Room { Name = "Dark Corridor" };
        var template = new RoomTemplate { Archetype = "Corridor" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanSpawn_RequiresRoomNameContains_AllowsMatchingKeyword()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Reactor-Specific Hazard",
            SpawnRules = new ElementSpawnRules
            {
                RequiresRoomNameContains = new List<string> { "Reactor", "Core" }
            }
        };

        var room = new Room { Name = "The Pulsing Reactor Chamber" };
        var template = new RoomTemplate { Archetype = "Chamber" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSpawn_MultipleRulesCombined_EnforcesAllRules()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Complex Element",
            SpawnRules = new ElementSpawnRules
            {
                NeverInEntryHall = true,
                OnlyInLargeRooms = true,
                RequiredArchetype = "Chamber"
            }
        };

        var room = new Room { Name = "Grand Chamber" };
        var template = new RoomTemplate { Archetype = "Chamber", Size = "Large" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSpawn_MultipleRulesFail_BlocksSpawn()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Complex Element",
            SpawnRules = new ElementSpawnRules
            {
                NeverInEntryHall = true,
                OnlyInLargeRooms = true, // This will fail
                RequiredArchetype = "Chamber"
            }
        };

        var room = new Room { Name = "Small Chamber" };
        var template = new RoomTemplate { Archetype = "Chamber", Size = "Small" };
        var context = new SpawnContext();

        // Act
        var result = _evaluator.CanSpawn(element, room, template, context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAdjustedWeight_NoModifiers_ReturnsBaseWeight()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Standard Hazard",
            Weight = 1.5f,
            SpawnRules = new ElementSpawnRules()
        };

        var room = new Room { Name = "Test Room" };
        var template = new RoomTemplate { Tags = new List<string> { "Standard" } };

        // Act
        var result = _evaluator.GetAdjustedWeight(element, room, template);

        // Assert
        Assert.Equal(1.5f, result);
    }

    [Fact]
    public void GetAdjustedWeight_HigherWeightInSecretRooms_AppliesMultiplier()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Rare Loot",
            Weight = 1.0f,
            SpawnRules = new ElementSpawnRules
            {
                HigherWeightInSecretRooms = true,
                SecretRoomWeightMultiplier = 3.0f
            }
        };

        var room = new Room { Name = "Hidden Cache" };
        var template = new RoomTemplate { Tags = new List<string> { "Secret" } };

        // Act
        var result = _evaluator.GetAdjustedWeight(element, room, template);

        // Assert
        Assert.Equal(3.0f, result);
    }

    [Fact]
    public void GetAdjustedWeight_HigherWeightInSecretRooms_UsesDefaultMultiplier()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Rare Loot",
            Weight = 1.0f,
            SpawnRules = new ElementSpawnRules
            {
                HigherWeightInSecretRooms = true
                // No multiplier specified, should default to 2.0
            }
        };

        var room = new Room { Name = "Hidden Cache" };
        var template = new RoomTemplate { Tags = new List<string> { "Secret" } };

        // Act
        var result = _evaluator.GetAdjustedWeight(element, room, template);

        // Assert
        Assert.Equal(2.0f, result);
    }

    [Fact]
    public void GetAdjustedWeight_HigherWeightInSecretRooms_OnlyAppliesInSecretRooms()
    {
        // Arrange
        var element = new BiomeElement
        {
            ElementName = "Rare Loot",
            Weight = 1.0f,
            SpawnRules = new ElementSpawnRules
            {
                HigherWeightInSecretRooms = true,
                SecretRoomWeightMultiplier = 3.0f
            }
        };

        var room = new Room { Name = "Normal Chamber" };
        var template = new RoomTemplate { Tags = new List<string> { "Standard" } };

        // Act
        var result = _evaluator.GetAdjustedWeight(element, room, template);

        // Assert
        Assert.Equal(1.0f, result); // Base weight, no multiplier
    }
}
