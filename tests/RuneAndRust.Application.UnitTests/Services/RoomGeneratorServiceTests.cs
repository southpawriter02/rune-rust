using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class RoomGeneratorServiceTests
{
    private RoomGeneratorService _service = null!;
    private RoomTemplateConfiguration _templateConfig = null!;
    private GenerationRulesConfiguration _rulesConfig = null!;
    private BiomeConfiguration _biomeConfig = null!;

    [SetUp]
    public void SetUp()
    {
        _templateConfig = CreateTestTemplateConfig();
        _rulesConfig = CreateTestRulesConfig();
        _biomeConfig = CreateTestBiomeConfig();

        var logger = NullLogger<RoomGeneratorService>.Instance;
        _service = new RoomGeneratorService(
            _templateConfig,
            _rulesConfig,
            _biomeConfig,
            logger);
    }

    [Test]
    public void GetDepthDifficultyModifier_AtSurface_ReturnsOne()
    {
        // Act
        var modifier = _service.GetDepthDifficultyModifier(0);

        // Assert
        modifier.Should().Be(1.0f);
    }

    [Test]
    public void GetDepthDifficultyModifier_AtDepthTwo_ReturnsScaledModifier()
    {
        // Act
        var modifier = _service.GetDepthDifficultyModifier(2);

        // Assert - 1.0 + (2 * 0.15) = 1.30
        modifier.Should().BeApproximately(1.30f, 0.01f);
    }

    [Test]
    public void GetDepthDifficultyModifier_AtDepthFive_ReturnsScaledModifier()
    {
        // Act
        var modifier = _service.GetDepthDifficultyModifier(5);

        // Assert - 1.0 + (5 * 0.15) = 1.75
        modifier.Should().BeApproximately(1.75f, 0.01f);
    }

    [Test]
    public void GenerateRoom_WithValidTemplate_ReturnsRoom()
    {
        // Arrange
        var position = new Position3D(0, 0, 0);

        // Act
        var result = _service.GenerateRoom(position, "dungeon", 12345);

        // Assert
        result.Room.Should().NotBeNull();
        result.Room.Position.Should().Be(position);
        result.BiomeId.Should().Be("dungeon");
    }

    [Test]
    public void GenerateRoom_SameSeedSamePosition_ReturnsSameRoomName()
    {
        // Arrange
        var position = new Position3D(1, 1, 0);
        var seed = 12345;

        // Act
        var result1 = _service.GenerateRoom(position, "dungeon", seed);
        var result2 = _service.GenerateRoom(position, "dungeon", seed);

        // Assert
        result1.Room.Name.Should().Be(result2.Room.Name);
        result1.TemplateId.Should().Be(result2.TemplateId);
    }

    [Test]
    public void GenerateRoom_DifferentSeeds_MayReturnDifferentRooms()
    {
        // Arrange
        var position = new Position3D(1, 1, 0);

        // Act - Generate many rooms to check for variation
        var names = Enumerable.Range(0, 10)
            .Select(seed => _service.GenerateRoom(position, "dungeon", seed))
            .Select(r => r.Room.Name)
            .Distinct()
            .ToList();

        // Assert - With different seeds, we should get some variety
        names.Count.Should().BeGreaterThan(1);
    }

    [Test]
    public void DetermineExits_WithGuaranteedExit_IncludesGuaranteedDirection()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ExitProbabilities = new Dictionary<string, float>
            {
                ["north"] = 0.0f,
                ["south"] = 0.0f,
                ["east"] = 0.0f,
                ["west"] = 0.0f
            }
        };

        // Act
        var exits = _service.DetermineExits(template, 12345, Direction.South).ToList();

        // Assert
        exits.Should().Contain(Direction.South);
    }

    [Test]
    public void DetermineExits_EnforcesMinimumExits()
    {
        // Arrange
        var template = new RoomTemplate
        {
            ExitProbabilities = new Dictionary<string, float>
            {
                ["north"] = 0.0f,
                ["south"] = 0.0f,
                ["east"] = 0.0f,
                ["west"] = 0.0f,
                ["up"] = 0.0f,
                ["down"] = 0.0f
            }
        };

        // Act
        var exits = _service.DetermineExits(template, 12345).ToList();

        // Assert - Should have at least minExitsPerRoom (1)
        exits.Count.Should().BeGreaterThanOrEqualTo(_rulesConfig.MinExitsPerRoom);
    }

    [Test]
    public void DetermineBiomeForDepth_AtSurface_ReturnsDungeon()
    {
        // Act
        var biome = _service.DetermineBiomeForDepth(0, 12345);

        // Assert
        biome.Should().Be("dungeon");
    }

    [Test]
    public void DetermineBiomeForDepth_DeepLevel_MayReturnDifferentBiome()
    {
        // Arrange & Act - Try many seeds at depth 5
        var biomes = Enumerable.Range(0, 100)
            .Select(seed => _service.DetermineBiomeForDepth(5, seed))
            .Distinct()
            .ToList();

        // Assert - At depth 5, we should see cave and possibly volcanic
        biomes.Should().Contain("cave");
    }

    private static RoomTemplateConfiguration CreateTestTemplateConfig() => new()
    {
        Templates = new Dictionary<string, RoomTemplate>
        {
            ["dungeon_corridor"] = new RoomTemplate
            {
                Id = "dungeon_corridor",
                Biomes = ["dungeon"],
                Names = ["Dark Corridor", "Stone Passage", "Dusty Hallway"],
                DescriptionTemplates = ["A narrow stone corridor."],
                ExitProbabilities = new Dictionary<string, float>
                {
                    ["north"] = 0.6f,
                    ["south"] = 0.6f,
                    ["east"] = 0.4f,
                    ["west"] = 0.4f
                },
                Weight = 40
            },
            ["dungeon_chamber"] = new RoomTemplate
            {
                Id = "dungeon_chamber",
                Biomes = ["dungeon"],
                Names = ["Ancient Chamber", "Forgotten Hall"],
                DescriptionTemplates = ["A large chamber."],
                ExitProbabilities = new Dictionary<string, float>
                {
                    ["north"] = 0.5f,
                    ["south"] = 0.5f,
                    ["east"] = 0.5f,
                    ["west"] = 0.5f
                },
                Weight = 30
            },
            ["cave_cavern"] = new RoomTemplate
            {
                Id = "cave_cavern",
                Biomes = ["cave"],
                Names = ["Natural Cavern", "Underground Grotto"],
                DescriptionTemplates = ["A natural cavern."],
                ExitProbabilities = new Dictionary<string, float>
                {
                    ["north"] = 0.5f,
                    ["south"] = 0.5f,
                    ["east"] = 0.5f,
                    ["west"] = 0.5f
                },
                Weight = 35
            }
        }
    };

    private static GenerationRulesConfiguration CreateTestRulesConfig() => new()
    {
        MaxRoomsPerLevel = 50,
        MinExitsPerRoom = 1,
        MaxExitsPerRoom = 4,
        DepthDifficultyMultiplier = 0.15f,
        BiomeTransitionDepths = new Dictionary<string, BiomeDepthRange>
        {
            ["dungeon"] = new BiomeDepthRange { MinDepth = 0, MaxDepth = 3, TransitionProbability = 1.0f },
            ["cave"] = new BiomeDepthRange { MinDepth = 2, MaxDepth = 7, TransitionProbability = 0.6f },
            ["volcanic"] = new BiomeDepthRange { MinDepth = 5, MaxDepth = -1, TransitionProbability = 0.5f }
        }
    };

    private static BiomeConfiguration CreateTestBiomeConfig() => new()
    {
        Biomes = new Dictionary<string, BiomeDefinition>
        {
            ["dungeon"] = new BiomeDefinition
            {
                Id = "dungeon",
                Name = "Dungeon",
                ImpliedTags = ["stone", "dark"]
            },
            ["cave"] = new BiomeDefinition
            {
                Id = "cave",
                Name = "Cave",
                ImpliedTags = ["natural", "damp"]
            }
        }
    };
}
