using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the EnvironmentPopulator class (v0.3.3c).
/// Validates biome-based hazard and condition assignment during dungeon generation.
/// </summary>
public class EnvironmentPopulatorTests
{
    private readonly Mock<IRepository<HazardTemplate>> _mockHazardTemplateRepo;
    private readonly Mock<IRepository<AmbientCondition>> _mockConditionRepo;
    private readonly Mock<IRepository<BiomeElement>> _mockBiomeElementRepo;
    private readonly Mock<IElementSpawnEvaluator> _mockSpawnEvaluator;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<ILogger<EnvironmentPopulator>> _mockLogger;
    private readonly EnvironmentPopulator _sut;

    public EnvironmentPopulatorTests()
    {
        _mockHazardTemplateRepo = new Mock<IRepository<HazardTemplate>>();
        _mockConditionRepo = new Mock<IRepository<AmbientCondition>>();
        _mockBiomeElementRepo = new Mock<IRepository<BiomeElement>>();
        _mockSpawnEvaluator = new Mock<IElementSpawnEvaluator>();
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<EnvironmentPopulator>>();

        _sut = new EnvironmentPopulator(
            _mockHazardTemplateRepo.Object,
            _mockConditionRepo.Object,
            _mockBiomeElementRepo.Object,
            _mockSpawnEvaluator.Object,
            _mockDiceService.Object,
            _mockLogger.Object);
    }

    #region BiomeEnvironmentMapping Tests

    [Theory]
    [InlineData(BiomeType.Ruin)]
    [InlineData(BiomeType.Industrial)]
    [InlineData(BiomeType.Organic)]
    [InlineData(BiomeType.Void)]
    public void GetConditionTypes_AllBiomes_ReturnsNonEmptyList(BiomeType biome)
    {
        // Act
        var result = BiomeEnvironmentMapping.GetConditionTypes(biome);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GetConditionTypes_RuinBiome_ReturnsCorrectTypes()
    {
        // Act
        var result = BiomeEnvironmentMapping.GetConditionTypes(BiomeType.Ruin);

        // Assert
        result.Should().Contain(ConditionType.LowVisibility);
        result.Should().Contain(ConditionType.DreadPresence);
    }

    [Fact]
    public void GetConditionTypes_IndustrialBiome_ReturnsCorrectTypes()
    {
        // Act
        var result = BiomeEnvironmentMapping.GetConditionTypes(BiomeType.Industrial);

        // Assert
        result.Should().Contain(ConditionType.ToxicAtmosphere);
        result.Should().Contain(ConditionType.StaticField);
        result.Should().Contain(ConditionType.ScorchingHeat);
    }

    [Fact]
    public void GetConditionTypes_OrganicBiome_ReturnsCorrectTypes()
    {
        // Act
        var result = BiomeEnvironmentMapping.GetConditionTypes(BiomeType.Organic);

        // Assert
        result.Should().Contain(ConditionType.BlightedGround);
        result.Should().Contain(ConditionType.PsychicResonance);
        result.Should().Contain(ConditionType.ToxicAtmosphere);
    }

    [Fact]
    public void GetConditionTypes_VoidBiome_ReturnsCorrectTypes()
    {
        // Act
        var result = BiomeEnvironmentMapping.GetConditionTypes(BiomeType.Void);

        // Assert
        result.Should().Contain(ConditionType.PsychicResonance);
        result.Should().Contain(ConditionType.DreadPresence);
        result.Should().Contain(ConditionType.DeepCold);
    }

    [Theory]
    [InlineData(DangerLevel.Safe, 0.1f)]
    [InlineData(DangerLevel.Unstable, 0.3f)]
    [InlineData(DangerLevel.Hostile, 0.5f)]
    [InlineData(DangerLevel.Lethal, 0.7f)]
    public void GetDangerMultiplier_ReturnsCorrectValue(DangerLevel level, float expected)
    {
        // Act
        var result = BiomeEnvironmentMapping.GetDangerMultiplier(level);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region PopulateRoomAsync Tests

    [Fact]
    public async Task PopulateRoom_HighChanceRoll_AssignsHazard()
    {
        // Arrange
        var room = CreateTestRoom(BiomeType.Ruin, DangerLevel.Lethal);
        var template = CreateTestHazardTemplate("Pressure Plate", BiomeType.Ruin);

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate> { template });
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition>());

        // Roll low enough to pass hazard chance (roll / 100 <= hazardChance)
        // With Lethal danger: hazardChance = 0.2 + 0.7 = 0.9 (90%)
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(10); // 10/100 = 0.1 <= 0.9, passes
        _mockDiceService.Setup(d => d.RollSingle(1, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.Hazards.Should().HaveCount(1);
        result.Hazards.First().Name.Should().Be("Pressure Plate");
    }

    [Fact]
    public async Task PopulateRoom_LowChanceRoll_NoHazard()
    {
        // Arrange
        var room = CreateTestRoom(BiomeType.Ruin, DangerLevel.Safe);
        var template = CreateTestHazardTemplate("Pressure Plate", BiomeType.Ruin);

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate> { template });
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition>());

        // Roll high (fails chance check)
        // With Safe danger: hazardChance = 0.2 + 0.1 = 0.3 (30%)
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(95); // 95/100 = 0.95 > 0.3, fails

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.Hazards.Should().BeEmpty();
    }

    [Fact]
    public async Task PopulateRoom_BiomeFiltersTemplates()
    {
        // Arrange
        var room = CreateTestRoom(BiomeType.Industrial, DangerLevel.Lethal);
        var ruinTemplate = CreateTestHazardTemplate("Pressure Plate", BiomeType.Ruin);
        var industrialTemplate = CreateTestHazardTemplate("Steam Vent", BiomeType.Industrial);

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate> { ruinTemplate, industrialTemplate });
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition>());

        // Always pass chance checks
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(1);
        _mockDiceService.Setup(d => d.RollSingle(1, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.Hazards.Should().HaveCount(1);
        result.Hazards.First().Name.Should().Be("Steam Vent");
    }

    [Fact]
    public async Task PopulateRoom_NoMatchingTemplates_NoHazard()
    {
        // Arrange
        var room = CreateTestRoom(BiomeType.Void, DangerLevel.Lethal);
        var ruinTemplate = CreateTestHazardTemplate("Pressure Plate", BiomeType.Ruin);

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate> { ruinTemplate });
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition>());

        // Always pass chance checks
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.Hazards.Should().BeEmpty();
    }

    [Fact]
    public async Task PopulateRoom_AssignsCondition()
    {
        // Arrange
        var room = CreateTestRoom(BiomeType.Industrial, DangerLevel.Lethal);
        var condition = CreateTestCondition(ConditionType.ToxicAtmosphere, BiomeType.Industrial);

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate>());
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition> { condition });

        // Fail hazard check, pass condition check
        var callCount = 0;
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(() => ++callCount == 1 ? 99 : 1); // First call (hazard) fails, second (condition) passes
        _mockDiceService.Setup(d => d.RollSingle(1, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.ConditionId.Should().Be(condition.Id);
    }

    [Fact]
    public async Task PopulateRoom_ExistingCondition_NotOverwritten()
    {
        // Arrange
        var existingConditionId = Guid.NewGuid();
        var room = CreateTestRoom(BiomeType.Industrial, DangerLevel.Lethal);
        room.ConditionId = existingConditionId;

        var condition = CreateTestCondition(ConditionType.ToxicAtmosphere, BiomeType.Industrial);

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate>());
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition> { condition });

        // Always pass chance checks
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.ConditionId.Should().Be(existingConditionId); // Original preserved
    }

    [Fact]
    public async Task PopulateRoom_CreatesHazardFromTemplate()
    {
        // Arrange
        var room = CreateTestRoom(BiomeType.Industrial, DangerLevel.Lethal);
        var template = new HazardTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Steam Vent",
            Description = "Hot steam bursts forth",
            HazardType = HazardType.Environmental,
            Trigger = TriggerType.TurnStart,
            EffectScript = "DAMAGE:Fire:1d4",
            MaxCooldown = 2,
            OneTimeUse = false,
            BiomeTags = new List<BiomeType> { BiomeType.Industrial }
        };

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<HazardTemplate> { template });
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition>());

        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(1);
        _mockDiceService.Setup(d => d.RollSingle(1, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = await _sut.PopulateRoomAsync(room);

        // Assert
        result.Hazards.Should().HaveCount(1);
        var hazard = result.Hazards.First();
        hazard.Name.Should().Be(template.Name);
        hazard.Description.Should().Be(template.Description);
        hazard.HazardType.Should().Be(template.HazardType);
        hazard.Trigger.Should().Be(template.Trigger);
        hazard.EffectScript.Should().Be(template.EffectScript);
        hazard.MaxCooldown.Should().Be(template.MaxCooldown);
        hazard.OneTimeUse.Should().Be(template.OneTimeUse);
        hazard.State.Should().Be(HazardState.Dormant);
        hazard.RoomId.Should().Be(room.Id);
    }

    #endregion

    #region PopulateDungeonAsync Tests

    [Fact]
    public async Task PopulateDungeon_ProcessesAllRooms()
    {
        // Arrange
        var rooms = new List<Room>
        {
            CreateTestRoom(BiomeType.Ruin, DangerLevel.Unstable),
            CreateTestRoom(BiomeType.Industrial, DangerLevel.Hostile),
            CreateTestRoom(BiomeType.Organic, DangerLevel.Lethal)
        };

        var templates = new List<HazardTemplate>
        {
            CreateTestHazardTemplate("Pressure Plate", BiomeType.Ruin),
            CreateTestHazardTemplate("Steam Vent", BiomeType.Industrial),
            CreateTestHazardTemplate("Spore Pod", BiomeType.Organic)
        };

        _mockHazardTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(templates);
        _mockConditionRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AmbientCondition>());

        // Always pass hazard chance
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(1);
        _mockDiceService.Setup(d => d.RollSingle(It.IsInRange(1, 10, Moq.Range.Inclusive), It.IsAny<string>()))
            .Returns(1);

        // Act
        await _sut.PopulateDungeonAsync(rooms);

        // Assert - each room should have 1 hazard matching its biome
        rooms[0].Hazards.Should().HaveCount(1);
        rooms[0].Hazards.First().Name.Should().Be("Pressure Plate");
        rooms[1].Hazards.Should().HaveCount(1);
        rooms[1].Hazards.First().Name.Should().Be("Steam Vent");
        rooms[2].Hazards.Should().HaveCount(1);
        rooms[2].Hazards.First().Name.Should().Be("Spore Pod");
    }

    [Fact]
    public async Task PopulateDungeon_EmptyRoomList_NoErrors()
    {
        // Arrange
        var rooms = new List<Room>();

        // Act
        var action = async () => await _sut.PopulateDungeonAsync(rooms);

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Helper Methods

    private static Room CreateTestRoom(BiomeType biome, DangerLevel danger)
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = $"Test {biome} Room",
            Description = "A test room",
            Position = new Coordinate(0, 0, 0),
            BiomeType = biome,
            DangerLevel = danger
        };
    }

    private static HazardTemplate CreateTestHazardTemplate(string name, BiomeType biome)
    {
        return new HazardTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Test {name} description",
            HazardType = HazardType.Mechanical,
            Trigger = TriggerType.Movement,
            EffectScript = "DAMAGE:Physical:1d6",
            MaxCooldown = 2,
            OneTimeUse = false,
            BiomeTags = new List<BiomeType> { biome }
        };
    }

    private static AmbientCondition CreateTestCondition(ConditionType type, BiomeType biome)
    {
        return new AmbientCondition
        {
            Id = Guid.NewGuid(),
            Type = type,
            Name = type.ToString(),
            Description = $"Test {type} condition",
            Color = "grey",
            TickScript = "DAMAGE:Poison:1d4",
            TickChance = 1.0f,
            BiomeTags = new List<BiomeType> { biome }
        };
    }

    #endregion
}
