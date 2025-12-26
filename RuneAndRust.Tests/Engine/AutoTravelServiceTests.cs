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
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for AutoTravelService (v0.3.20c).
/// Validates precondition checks, path safety, and travel execution.
/// </summary>
public class AutoTravelServiceTests
{
    private readonly Mock<ILogger<AutoTravelService>> _mockLogger;
    private readonly GameState _gameState;
    private readonly Mock<IRoomPathfinderService> _mockPathfinder;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IHazardService> _mockHazardService;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly AutoTravelService _sut;

    public AutoTravelServiceTests()
    {
        _mockLogger = new Mock<ILogger<AutoTravelService>>();
        _gameState = new GameState
        {
            Phase = GamePhase.Exploration,
            CurrentCharacter = CreateTestCharacter(),
            CurrentRoomId = Guid.NewGuid()
        };
        _mockPathfinder = new Mock<IRoomPathfinderService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockHazardService = new Mock<IHazardService>();
        _mockInputHandler = new Mock<IInputHandler>();

        _sut = new AutoTravelService(
            _mockLogger.Object,
            _gameState,
            _mockPathfinder.Object,
            _mockNavigationService.Object,
            _mockInventoryService.Object,
            _mockRoomRepository.Object,
            _mockHazardService.Object,
            _mockInputHandler.Object);
    }

    #region ValidateTravelPreconditionsAsync Tests

    [Fact]
    public async Task ValidatePreconditions_BlocksExhausted()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.AddStatusEffect(StatusEffectType.Exhausted);

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(character))
            .ReturnsAsync(BurdenState.Light);

        // Act
        var error = await _sut.ValidateTravelPreconditionsAsync(character);

        // Assert
        error.Should().NotBeNull();
        error.Should().Contain("exhausted");
    }

    [Fact]
    public async Task ValidatePreconditions_BlocksOverburdened()
    {
        // Arrange
        var character = CreateTestCharacter();

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(character))
            .ReturnsAsync(BurdenState.Overburdened);

        // Act
        var error = await _sut.ValidateTravelPreconditionsAsync(character);

        // Assert
        error.Should().NotBeNull();
        error.Should().Contain("overburdened");
    }

    [Fact]
    public async Task ValidatePreconditions_AllowsHealthy()
    {
        // Arrange
        var character = CreateTestCharacter();

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(character))
            .ReturnsAsync(BurdenState.Light);

        // Act
        var error = await _sut.ValidateTravelPreconditionsAsync(character);

        // Assert
        error.Should().BeNull();
    }

    [Fact]
    public async Task ValidatePreconditions_BlocksDuringCombat()
    {
        // Arrange
        _gameState.Phase = GamePhase.Combat;
        var character = CreateTestCharacter();

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(character))
            .ReturnsAsync(BurdenState.Light);

        // Act
        var error = await _sut.ValidateTravelPreconditionsAsync(character);

        // Assert
        error.Should().NotBeNull();
        error.Should().Contain("combat");
    }

    #endregion

    #region ValidatePathSafetyAsync Tests

    [Fact]
    public async Task ValidatePathSafety_BlocksLethalRooms()
    {
        // Arrange
        var lethalRoom = CreateRoom(DangerLevel.Lethal);
        var path = new List<Guid> { lethalRoom.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { lethalRoom });

        _mockHazardService.Setup(h => h.GetActiveHazardsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<DynamicHazard>());

        // Act
        var error = await _sut.ValidatePathSafetyAsync(path);

        // Assert
        error.Should().NotBeNull();
        error.Should().Contain("dangerous");
    }

    [Fact]
    public async Task ValidatePathSafety_BlocksMovementHazards()
    {
        // Arrange
        var room = CreateRoom(DangerLevel.Safe);
        var hazard = new DynamicHazard
        {
            Name = "Steam Vent",
            Trigger = TriggerType.Movement,
            State = HazardState.Dormant
        };

        var path = new List<Guid> { room.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { room });

        _mockHazardService.Setup(h => h.GetActiveHazardsAsync(room.Id))
            .ReturnsAsync(new List<DynamicHazard> { hazard });

        // Act
        var error = await _sut.ValidatePathSafetyAsync(path);

        // Assert
        error.Should().NotBeNull();
        error.Should().Contain("Hazard");
        error.Should().Contain("Steam Vent");
    }

    [Fact]
    public async Task ValidatePathSafety_AllowsSafeRooms()
    {
        // Arrange
        var safeRoom = CreateRoom(DangerLevel.Safe);
        var path = new List<Guid> { safeRoom.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { safeRoom });

        _mockHazardService.Setup(h => h.GetActiveHazardsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<DynamicHazard>());

        // Act
        var error = await _sut.ValidatePathSafetyAsync(path);

        // Assert
        error.Should().BeNull();
    }

    [Fact]
    public async Task ValidatePathSafety_IgnoresCooldownHazards()
    {
        // Arrange
        var room = CreateRoom(DangerLevel.Safe);
        var hazard = new DynamicHazard
        {
            Name = "Steam Vent",
            Trigger = TriggerType.Movement,
            State = HazardState.Cooldown // Not dormant
        };

        var path = new List<Guid> { room.Id };

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { room });

        _mockHazardService.Setup(h => h.GetActiveHazardsAsync(room.Id))
            .ReturnsAsync(new List<DynamicHazard> { hazard });

        // Act
        var error = await _sut.ValidatePathSafetyAsync(path);

        // Assert
        error.Should().BeNull(); // Cooldown hazards don't block
    }

    #endregion

    #region ExecuteTravelAsync Tests

    [Fact]
    public async Task ExecuteTravel_CompletesFullPath()
    {
        // Arrange
        var directions = new List<Direction> { Direction.East, Direction.North };
        var destRoom = CreateRoom(DangerLevel.Safe);
        destRoom.Name = "Destination";

        _mockNavigationService.Setup(n => n.MoveAsync(It.IsAny<Direction>()))
            .ReturnsAsync("You move...");

        _mockNavigationService.Setup(n => n.GetCurrentRoomAsync())
            .ReturnsAsync(destRoom);

        _gameState.TurnCount = 0;

        // Act
        var result = await _sut.ExecuteTravelAsync(directions);

        // Assert
        result.Success.Should().BeTrue();
        result.RoomsTraveled.Should().Be(2);
        result.FinalRoom.Should().NotBeNull();
        result.FinalRoom!.Name.Should().Be("Destination");
    }

    [Fact]
    public async Task ExecuteTravel_StopsOnCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel

        var directions = new List<Direction> { Direction.East, Direction.North };
        var currentRoom = CreateRoom(DangerLevel.Safe);
        currentRoom.Name = "Midway";

        _mockNavigationService.Setup(n => n.GetCurrentRoomAsync())
            .ReturnsAsync(currentRoom);

        // Act
        var result = await _sut.ExecuteTravelAsync(directions, cts.Token);

        // Assert
        result.Success.Should().BeFalse();
        result.InterruptReason.Should().Be(TravelInterruptReason.UserCancelled);
        result.RoomsTraveled.Should().Be(0); // Cancelled before first step
    }

    [Fact]
    public async Task ExecuteTravel_StopsOnCombatPhaseChange()
    {
        // Arrange
        var directions = new List<Direction> { Direction.East, Direction.North };
        var currentRoom = CreateRoom(DangerLevel.Safe);
        currentRoom.Name = "Ambush Site";

        _mockNavigationService.Setup(n => n.MoveAsync(It.IsAny<Direction>()))
            .Callback(() => _gameState.Phase = GamePhase.Combat) // Simulate ambush
            .ReturnsAsync("You move...");

        _mockNavigationService.Setup(n => n.GetCurrentRoomAsync())
            .ReturnsAsync(currentRoom);

        // Act
        var result = await _sut.ExecuteTravelAsync(directions);

        // Assert
        result.Success.Should().BeFalse();
        result.InterruptReason.Should().Be(TravelInterruptReason.CombatTriggered);
    }

    [Fact]
    public async Task ExecuteTravel_ReturnsAlreadyThere_WhenEmptyPath()
    {
        // Arrange
        var directions = new List<Direction>();

        // Act
        var result = await _sut.ExecuteTravelAsync(directions);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already");
    }

    #endregion

    #region TravelToAsync Tests

    [Fact]
    public async Task TravelTo_ResolvesHomeKeyword()
    {
        // Arrange
        var startRoom = CreateRoom(DangerLevel.Safe);
        var anchorRoom = CreateRoom(DangerLevel.Safe);
        anchorRoom.Features.Add(RoomFeature.RunicAnchor);
        anchorRoom.Name = "Runic Sanctuary";

        _gameState.CurrentRoomId = startRoom.Id;
        _gameState.VisitedRoomIds = new HashSet<Guid> { startRoom.Id, anchorRoom.Id };

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(It.IsAny<Character>()))
            .ReturnsAsync(BurdenState.Light);

        _mockPathfinder.Setup(p => p.FindNearestFeatureAsync(
                startRoom.Id, RoomFeature.RunicAnchor, It.IsAny<HashSet<Guid>>()))
            .ReturnsAsync(anchorRoom);

        _mockPathfinder.Setup(p => p.FindPathAsync(
                startRoom.Id, anchorRoom.Id, It.IsAny<HashSet<Guid>>()))
            .ReturnsAsync(RoomPathResult.Succeeded(
                new List<Guid> { anchorRoom.Id },
                new List<Direction> { Direction.East }));

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { anchorRoom });

        _mockHazardService.Setup(h => h.GetActiveHazardsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<DynamicHazard>());

        _mockNavigationService.Setup(n => n.MoveAsync(It.IsAny<Direction>()))
            .ReturnsAsync("You move...");

        _mockNavigationService.Setup(n => n.GetCurrentRoomAsync())
            .ReturnsAsync(anchorRoom);

        // Act
        var result = await _sut.TravelToAsync("home");

        // Assert
        result.Success.Should().BeTrue();
        _mockPathfinder.Verify(p => p.FindNearestFeatureAsync(
            startRoom.Id, RoomFeature.RunicAnchor, It.IsAny<HashSet<Guid>>()), Times.Once);
    }

    [Fact]
    public async Task TravelTo_ResolvesRoomName()
    {
        // Arrange
        var startRoom = CreateRoom(DangerLevel.Safe);
        var targetRoom = CreateRoom(DangerLevel.Safe);
        targetRoom.Name = "Grand Hall";

        _gameState.CurrentRoomId = startRoom.Id;
        _gameState.VisitedRoomIds = new HashSet<Guid> { startRoom.Id, targetRoom.Id };

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(It.IsAny<Character>()))
            .ReturnsAsync(BurdenState.Light);

        _mockPathfinder.Setup(p => p.FindRoomsByNameAsync("grand", It.IsAny<HashSet<Guid>>()))
            .ReturnsAsync(new List<Room> { targetRoom });

        _mockPathfinder.Setup(p => p.FindPathAsync(
                startRoom.Id, targetRoom.Id, It.IsAny<HashSet<Guid>>()))
            .ReturnsAsync(RoomPathResult.Succeeded(
                new List<Guid> { targetRoom.Id },
                new List<Direction> { Direction.North }));

        _mockRoomRepository.Setup(r => r.GetBatchAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Room> { targetRoom });

        _mockHazardService.Setup(h => h.GetActiveHazardsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<DynamicHazard>());

        _mockNavigationService.Setup(n => n.MoveAsync(It.IsAny<Direction>()))
            .ReturnsAsync("You move...");

        _mockNavigationService.Setup(n => n.GetCurrentRoomAsync())
            .ReturnsAsync(targetRoom);

        // Act
        var result = await _sut.TravelToAsync("grand");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TravelTo_ReportsNoPath()
    {
        // Arrange
        var startRoom = CreateRoom(DangerLevel.Safe);
        var targetRoom = CreateRoom(DangerLevel.Safe);
        targetRoom.Name = "Unreachable";

        _gameState.CurrentRoomId = startRoom.Id;
        _gameState.VisitedRoomIds = new HashSet<Guid> { startRoom.Id, targetRoom.Id };

        _mockInventoryService.Setup(s => s.CalculateBurdenAsync(It.IsAny<Character>()))
            .ReturnsAsync(BurdenState.Light);

        _mockPathfinder.Setup(p => p.FindRoomsByNameAsync("unreachable", It.IsAny<HashSet<Guid>>()))
            .ReturnsAsync(new List<Room> { targetRoom });

        _mockPathfinder.Setup(p => p.FindPathAsync(
                startRoom.Id, targetRoom.Id, It.IsAny<HashSet<Guid>>()))
            .ReturnsAsync(RoomPathResult.Failed("No path through explored areas."));

        // Act
        var result = await _sut.TravelToAsync("unreachable");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No path");
    }

    #endregion

    #region Helper Methods

    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Character",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior,
            Sturdiness = 5,
            Might = 5,
            Wits = 5,
            Will = 5,
            Finesse = 5,
            MaxHP = 100,
            CurrentHP = 100
        };
    }

    private static Room CreateRoom(DangerLevel dangerLevel)
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "A test room.",
            Position = new Coordinate(0, 0, 0),
            BiomeType = BiomeType.Ruin,
            DangerLevel = dangerLevel
        };
    }

    #endregion
}
