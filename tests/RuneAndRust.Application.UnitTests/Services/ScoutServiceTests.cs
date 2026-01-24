using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ScoutService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Base DC calculation for each terrain type</description></item>
///   <item><description>Scout DC with visibility modifiers</description></item>
///   <item><description>Room revelation formula (1 + netSuccesses/2, capped)</description></item>
///   <item><description>Scouting prerequisites (CanScout, GetScoutBlockedReason)</description></item>
///   <item><description>Terrain display information</description></item>
///   <item><description>ScoutResult value object (factory methods, computed properties)</description></item>
///   <item><description>ScoutContext value object (factory methods, computed properties)</description></item>
///   <item><description>ScoutedRoom value object (factory methods, computed properties)</description></item>
///   <item><description>DetectedEnemy value object (factory methods, computed properties)</description></item>
///   <item><description>DetectedHazard value object (factory methods, computed properties)</description></item>
///   <item><description>PointOfInterest value object (factory methods, computed properties)</description></item>
///   <item><description>ThreatLevel enum extensions</description></item>
///   <item><description>HazardSeverity enum extensions</description></item>
///   <item><description>PointOfInterestType enum extensions</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ScoutServiceTests
{
    private SkillCheckService _skillCheckService = null!;
    private ILogger<ScoutService> _logger = null!;
    private ScoutService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _skillCheckService = CreateMockSkillCheckService();
        _logger = Substitute.For<ILogger<ScoutService>>();
        _sut = new ScoutService(_skillCheckService, _logger);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BASE DC TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Base DC Tests

    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, 8)]
    [TestCase(NavigationTerrainType.ModerateRuins, 12)]
    [TestCase(NavigationTerrainType.DenseRuins, 16)]
    [TestCase(NavigationTerrainType.Labyrinthine, 20)]
    [TestCase(NavigationTerrainType.GlitchedLabyrinth, 24)]
    public void GetBaseDc_ReturnsCorrectDcForEachTerrainType(
        NavigationTerrainType terrainType,
        int expectedDc)
    {
        // Act
        var dc = _sut.GetBaseDc(terrainType);

        // Assert
        dc.Should().Be(expectedDc);
    }

    [Test]
    public void GetBaseDc_UnknownTerrainType_ReturnsDefaultModerate()
    {
        // Arrange
        var unknownType = (NavigationTerrainType)99;

        // Act
        var dc = _sut.GetBaseDc(unknownType);

        // Assert
        dc.Should().Be(12); // Default to ModerateRuins DC
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUT DC CALCULATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Scout DC Calculation Tests

    [Test]
    public void GetScoutDc_OpenWastelandNormalVisibility_ReturnsEight()
    {
        // Act
        var dc = _sut.GetScoutDc(NavigationTerrainType.OpenWasteland, 0);

        // Assert
        dc.Should().Be(8);
    }

    [Test]
    public void GetScoutDc_ModerateRuinsPoorVisibility_ReturnsFourteen()
    {
        // Act
        var dc = _sut.GetScoutDc(NavigationTerrainType.ModerateRuins, 2);

        // Assert
        dc.Should().Be(14); // 12 + 2
    }

    [Test]
    public void GetScoutDc_DenseRuinsExcellentVisibility_ReturnsFourteen()
    {
        // Act
        var dc = _sut.GetScoutDc(NavigationTerrainType.DenseRuins, -2);

        // Assert
        dc.Should().Be(14); // 16 - 2
    }

    [Test]
    public void GetScoutDc_GlitchedLabyrinthStaticStorm_ReturnsThirty()
    {
        // Act
        var dc = _sut.GetScoutDc(NavigationTerrainType.GlitchedLabyrinth, 6);

        // Assert
        dc.Should().Be(30); // 24 + 6
    }

    [Test]
    public void GetScoutDc_ExtremeNegativeModifier_ClampsToMinimumOne()
    {
        // Act
        var dc = _sut.GetScoutDc(NavigationTerrainType.OpenWasteland, -20);

        // Assert
        dc.Should().Be(1); // Min DC is 1
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // ROOM REVELATION FORMULA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Room Revelation Tests

    [Test]
    [TestCase(-5, 5, 0)]
    [TestCase(-1, 5, 0)]
    public void CalculateRoomsRevealed_Failure_ReturnsZero(
        int netSuccesses,
        int adjacentCount,
        int expected)
    {
        // Act
        var rooms = _sut.CalculateRoomsRevealed(netSuccesses, adjacentCount);

        // Assert
        rooms.Should().Be(expected);
    }

    [Test]
    [TestCase(0, 5, 1)]
    [TestCase(1, 5, 1)]
    public void CalculateRoomsRevealed_MarginalSuccess_ReturnsOne(
        int netSuccesses,
        int adjacentCount,
        int expected)
    {
        // Act
        var rooms = _sut.CalculateRoomsRevealed(netSuccesses, adjacentCount);

        // Assert
        rooms.Should().Be(expected);
    }

    [Test]
    [TestCase(2, 5, 2)]
    [TestCase(3, 5, 2)]
    public void CalculateRoomsRevealed_GoodSuccess_ReturnsTwo(
        int netSuccesses,
        int adjacentCount,
        int expected)
    {
        // Act
        var rooms = _sut.CalculateRoomsRevealed(netSuccesses, adjacentCount);

        // Assert
        rooms.Should().Be(expected);
    }

    [Test]
    [TestCase(4, 5, 3)]
    [TestCase(5, 5, 3)]
    public void CalculateRoomsRevealed_ExcellentSuccess_ReturnsThree(
        int netSuccesses,
        int adjacentCount,
        int expected)
    {
        // Act
        var rooms = _sut.CalculateRoomsRevealed(netSuccesses, adjacentCount);

        // Assert
        rooms.Should().Be(expected);
    }

    [Test]
    [TestCase(6, 5, 4)]
    [TestCase(7, 5, 4)]
    [TestCase(10, 5, 5)] // Capped at adjacent count
    public void CalculateRoomsRevealed_OutstandingSuccess_ReturnsFourOrMore(
        int netSuccesses,
        int adjacentCount,
        int expected)
    {
        // Act
        var rooms = _sut.CalculateRoomsRevealed(netSuccesses, adjacentCount);

        // Assert
        rooms.Should().Be(expected);
    }

    [Test]
    public void CalculateRoomsRevealed_ExceedsAdjacent_CapsAtAdjacent()
    {
        // Arrange - only 2 adjacent rooms but excellent success
        var netSuccesses = 10;
        var adjacentCount = 2;

        // Act
        var rooms = _sut.CalculateRoomsRevealed(netSuccesses, adjacentCount);

        // Assert
        rooms.Should().Be(2); // Cannot reveal more than adjacent
    }

    [Test]
    public void CalculateRoomsRevealed_ZeroAdjacentRooms_ReturnsZero()
    {
        // Act
        var rooms = _sut.CalculateRoomsRevealed(5, 0);

        // Assert
        rooms.Should().Be(0); // No rooms to reveal
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUTING PREREQUISITES TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Scouting Prerequisites Tests

    [Test]
    public void CanScout_WithAdjacentRooms_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = ScoutContext.Create(
            player.Id.ToString(),
            "room-1",
            NavigationTerrainType.ModerateRuins,
            new[] { "room-2", "room-3" });

        // Act
        var canScout = _sut.CanScout(player, context);

        // Assert
        canScout.Should().BeTrue();
    }

    [Test]
    public void CanScout_NoAdjacentRooms_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = ScoutContext.Create(
            player.Id.ToString(),
            "room-1",
            NavigationTerrainType.ModerateRuins,
            Array.Empty<string>());

        // Act
        var canScout = _sut.CanScout(player, context);

        // Assert
        canScout.Should().BeFalse();
    }

    [Test]
    public void GetScoutBlockedReason_WithAdjacentRooms_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = ScoutContext.Create(
            player.Id.ToString(),
            "room-1",
            NavigationTerrainType.ModerateRuins,
            new[] { "room-2" });

        // Act
        var reason = _sut.GetScoutBlockedReason(player, context);

        // Assert
        reason.Should().BeNull();
    }

    [Test]
    public void GetScoutBlockedReason_NoAdjacentRooms_ReturnsReason()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = ScoutContext.Create(
            player.Id.ToString(),
            "room-1",
            NavigationTerrainType.ModerateRuins,
            Array.Empty<string>());

        // Act
        var reason = _sut.GetScoutBlockedReason(player, context);

        // Assert
        reason.Should().NotBeNullOrEmpty();
        reason.Should().Contain("no adjacent rooms");
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // TERRAIN DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Terrain Display Tests

    [Test]
    [TestCase(NavigationTerrainType.OpenWasteland, "Open Wasteland")]
    [TestCase(NavigationTerrainType.ModerateRuins, "Moderate Ruins")]
    [TestCase(NavigationTerrainType.DenseRuins, "Dense Ruins")]
    [TestCase(NavigationTerrainType.Labyrinthine, "Labyrinthine")]
    [TestCase(NavigationTerrainType.GlitchedLabyrinth, "Glitched Labyrinth")]
    public void GetTerrainDisplayName_ReturnsCorrectName(
        NavigationTerrainType terrainType,
        string expectedName)
    {
        // Act
        var name = _sut.GetTerrainDisplayName(terrainType);

        // Assert
        name.Should().Be(expectedName);
    }

    [Test]
    public void GetTerrainScoutingDescription_AllTerrains_ReturnsNonEmpty()
    {
        // Act & Assert
        foreach (var terrain in Enum.GetValues<NavigationTerrainType>())
        {
            var description = _sut.GetTerrainScoutingDescription(terrain);
            description.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void GetTerrainScoutingDescription_GlitchedLabyrinth_MentionsReliability()
    {
        // Act
        var description = _sut.GetTerrainScoutingDescription(
            NavigationTerrainType.GlitchedLabyrinth);

        // Assert
        description.Should().Match(d => d.Contains("unreliable") || d.Contains("impossible"));
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUT RESULT VALUE OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region ScoutResult Value Object Tests

    [Test]
    public void ScoutResult_Success_HasCorrectProperties()
    {
        // Arrange
        var rooms = new List<ScoutedRoom>
        {
            ScoutedRoom.Empty("room-1", "Test Room")
        };

        // Act
        var result = ScoutResult.Success(rooms, 3, 12, "Test roll");

        // Assert
        result.ScoutSucceeded.Should().BeTrue();
        result.NetSuccesses.Should().Be(3);
        result.TargetDc.Should().Be(12);
        result.RoomCount.Should().Be(1);
        result.RollDetails.Should().Be("Test roll");
    }

    [Test]
    public void ScoutResult_Failure_HasCorrectProperties()
    {
        // Act
        var result = ScoutResult.Failure(-2, 12, "Failed roll");

        // Assert
        result.ScoutSucceeded.Should().BeFalse();
        result.NetSuccesses.Should().Be(-2);
        result.TargetDc.Should().Be(12);
        result.RoomCount.Should().Be(0);
    }

    [Test]
    public void ScoutResult_Empty_HasCorrectProperties()
    {
        // Act
        var result = ScoutResult.Empty("Test reason");

        // Assert
        result.ScoutSucceeded.Should().BeFalse();
        result.RoomCount.Should().Be(0);
        result.RollDetails.Should().Contain("Test reason");
    }

    [Test]
    public void ScoutResult_IsCriticalSuccess_ReturnsTrueWhenNetFourOrMore()
    {
        // Arrange
        var rooms = new List<ScoutedRoom>
        {
            ScoutedRoom.Empty("room-1", "Room 1"),
            ScoutedRoom.Empty("room-2", "Room 2"),
            ScoutedRoom.Empty("room-3", "Room 3")
        };

        // Act
        var result = ScoutResult.Success(rooms, 4, 12);

        // Assert
        result.IsCriticalSuccess.Should().BeTrue();
    }

    [Test]
    public void ScoutResult_IsCriticalSuccess_ReturnsFalseWhenNetLessThanFour()
    {
        // Arrange
        var rooms = new List<ScoutedRoom>
        {
            ScoutedRoom.Empty("room-1", "Room 1")
        };

        // Act
        var result = ScoutResult.Success(rooms, 3, 12);

        // Assert
        result.IsCriticalSuccess.Should().BeFalse();
    }

    [Test]
    public void ScoutResult_CalculateRoomsRevealed_MatchesServiceCalculation()
    {
        // Arrange
        var testCases = new[]
        {
            (NetSuccesses: -1, Adjacent: 5, Expected: 0),
            (NetSuccesses: 0, Adjacent: 5, Expected: 1),
            (NetSuccesses: 2, Adjacent: 5, Expected: 2),
            (NetSuccesses: 4, Adjacent: 5, Expected: 3),
            (NetSuccesses: 10, Adjacent: 3, Expected: 3), // Capped
        };

        // Act & Assert
        foreach (var (net, adjacent, expected) in testCases)
        {
            var result = ScoutResult.CalculateRoomsRevealed(net, adjacent);
            result.Should().Be(expected);
        }
    }

    [Test]
    public void ScoutResult_ToDisplayString_Success_ContainsRoomCount()
    {
        // Arrange
        var rooms = new List<ScoutedRoom>
        {
            ScoutedRoom.Empty("room-1", "Room 1"),
            ScoutedRoom.Empty("room-2", "Room 2")
        };
        var result = ScoutResult.Success(rooms, 3, 12);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("2");
        display.Should().Contain("room");
    }

    [Test]
    public void ScoutResult_ToDisplayString_Failure_ContainsFailureMessage()
    {
        // Arrange
        var result = ScoutResult.Failure(-2, 12);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().Contain("learn nothing");
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUT CONTEXT VALUE OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region ScoutContext Value Object Tests

    [Test]
    public void ScoutContext_Create_HasCorrectDefaultValues()
    {
        // Act
        var context = ScoutContext.Create(
            "player-1",
            "room-1",
            NavigationTerrainType.ModerateRuins,
            new[] { "room-2", "room-3" });

        // Assert
        context.PlayerId.Should().Be("player-1");
        context.CurrentRoomId.Should().Be("room-1");
        context.TerrainType.Should().Be(NavigationTerrainType.ModerateRuins);
        context.AdjacentRoomCount.Should().Be(2);
        context.EquipmentBonus.Should().Be(0);
        context.VisibilityModifier.Should().Be(0);
    }

    [Test]
    public void ScoutContext_CreateWithModifiers_HasCorrectValues()
    {
        // Act
        var context = ScoutContext.CreateWithModifiers(
            "player-1",
            "room-1",
            NavigationTerrainType.DenseRuins,
            new[] { "room-2" },
            equipmentBonus: 2,
            visibilityModifier: -1);

        // Assert
        context.EquipmentBonus.Should().Be(2);
        context.VisibilityModifier.Should().Be(-1);
    }

    [Test]
    public void ScoutContext_BaseDc_MatchesTerrainType()
    {
        // Arrange
        var context = ScoutContext.Create(
            "player-1",
            "room-1",
            NavigationTerrainType.Labyrinthine,
            Array.Empty<string>());

        // Act
        var baseDc = context.BaseDc;

        // Assert
        baseDc.Should().Be(20);
    }

    [Test]
    public void ScoutContext_FinalDc_IncludesVisibilityModifier()
    {
        // Arrange
        var context = ScoutContext.CreateWithModifiers(
            "player-1",
            "room-1",
            NavigationTerrainType.ModerateRuins,
            Array.Empty<string>(),
            0,
            visibilityModifier: 4);

        // Act
        var finalDc = context.FinalDc;

        // Assert
        finalDc.Should().Be(16); // 12 + 4
    }

    [Test]
    public void ScoutContext_FinalDc_ClampsToMinimumOne()
    {
        // Arrange
        var context = ScoutContext.CreateWithModifiers(
            "player-1",
            "room-1",
            NavigationTerrainType.OpenWasteland,
            Array.Empty<string>(),
            0,
            visibilityModifier: -20);

        // Act
        var finalDc = context.FinalDc;

        // Assert
        finalDc.Should().Be(1);
    }

    [Test]
    public void ScoutContext_HasAdjacentRooms_ReturnsTrueWhenRoomsExist()
    {
        // Arrange
        var context = ScoutContext.Create(
            "player-1",
            "room-1",
            NavigationTerrainType.ModerateRuins,
            new[] { "room-2" });

        // Assert
        context.HasAdjacentRooms.Should().BeTrue();
    }

    [Test]
    public void ScoutContext_HasAdjacentRooms_ReturnsFalseWhenEmpty()
    {
        // Arrange
        var context = ScoutContext.Create(
            "player-1",
            "room-1",
            NavigationTerrainType.ModerateRuins,
            Array.Empty<string>());

        // Assert
        context.HasAdjacentRooms.Should().BeFalse();
    }

    [Test]
    public void ScoutContext_HasGoodVisibility_ReturnsTrueWhenNegativeModifier()
    {
        // Arrange
        var context = ScoutContext.CreateWithModifiers(
            "player-1", "room-1",
            NavigationTerrainType.ModerateRuins,
            Array.Empty<string>(), 0, -2);

        // Assert
        context.HasGoodVisibility.Should().BeTrue();
        context.HasPoorVisibility.Should().BeFalse();
    }

    [Test]
    public void ScoutContext_HasPoorVisibility_ReturnsTrueWhenPositiveModifier()
    {
        // Arrange
        var context = ScoutContext.CreateWithModifiers(
            "player-1", "room-1",
            NavigationTerrainType.ModerateRuins,
            Array.Empty<string>(), 0, 2);

        // Assert
        context.HasPoorVisibility.Should().BeTrue();
        context.HasGoodVisibility.Should().BeFalse();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // SCOUTED ROOM VALUE OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region ScoutedRoom Value Object Tests

    [Test]
    public void ScoutedRoom_Create_HasCorrectProperties()
    {
        // Act
        var room = ScoutedRoom.Create(
            roomId: "room-1",
            roomName: "Test Room",
            description: "A test room",
            terrainType: NavigationTerrainType.ModerateRuins);

        // Assert
        room.RoomId.Should().Be("room-1");
        room.RoomName.Should().Be("Test Room");
        room.Description.Should().Be("A test room");
        room.TerrainType.Should().Be(NavigationTerrainType.ModerateRuins);
        room.HasEnemies.Should().BeFalse();
        room.HasHazards.Should().BeFalse();
        room.HasPointsOfInterest.Should().BeFalse();
    }

    [Test]
    public void ScoutedRoom_WithEnemies_HasCorrectThreatAssessment()
    {
        // Arrange
        var enemies = new List<DetectedEnemy>
        {
            DetectedEnemy.CreateWithThreatLevel("Raiders", 3, ThreatLevel.Moderate),
            DetectedEnemy.CreateWithThreatLevel("Raider Boss", 1, ThreatLevel.High)
        };

        // Act
        var room = ScoutedRoom.Create(
            "room-1", "Raider Den", "Hostile territory",
            NavigationTerrainType.DenseRuins,
            enemies: enemies);

        // Assert
        room.HasEnemies.Should().BeTrue();
        room.TotalEnemyCount.Should().Be(4);
        room.HighestThreatLevel.Should().Be(ThreatLevel.High);
    }

    [Test]
    public void ScoutedRoom_WithHazards_HasCorrectSeverityAssessment()
    {
        // Arrange
        var hazards = new List<DetectedHazard>
        {
            DetectedHazard.Create(DetectableHazardType.ToxicZone, HazardSeverity.Moderate),
            DetectedHazard.Create(DetectableHazardType.GlitchPocket, HazardSeverity.Severe)
        };

        // Act
        var room = ScoutedRoom.Create(
            "room-1", "Contaminated Area", "Dangerous environment",
            NavigationTerrainType.GlitchedLabyrinth,
            hazards: hazards);

        // Assert
        room.HasHazards.Should().BeTrue();
        room.HighestHazardSeverity.Should().Be(HazardSeverity.Severe);
    }

    [Test]
    public void ScoutedRoom_IsDangerous_TrueWhenHighThreatOrSevereHazard()
    {
        // Arrange
        var enemies = new List<DetectedEnemy>
        {
            DetectedEnemy.CreateWithThreatLevel("Elite Guard", 1, ThreatLevel.High)
        };

        // Act
        var room = ScoutedRoom.Create(
            "room-1", "Guard Post", "Dangerous",
            NavigationTerrainType.ModerateRuins,
            enemies: enemies);

        // Assert
        room.IsDangerous.Should().BeTrue();
    }

    [Test]
    public void ScoutedRoom_Empty_HasNoThreatData()
    {
        // Act
        var room = ScoutedRoom.Empty("room-1", "Empty Room");

        // Assert
        room.HasEnemies.Should().BeFalse();
        room.HasHazards.Should().BeFalse();
        room.HasPointsOfInterest.Should().BeFalse();
        room.HasThreats.Should().BeFalse();
        room.IsDangerous.Should().BeFalse();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DETECTED ENEMY VALUE OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region DetectedEnemy Value Object Tests

    [Test]
    public void DetectedEnemy_Create_FromChallengeRating()
    {
        // Act
        var enemy = DetectedEnemy.Create("Feral Dog", 2, challengeRating: 1);

        // Assert
        enemy.EnemyType.Should().Be("Feral Dog");
        enemy.Count.Should().Be(2);
        enemy.ThreatLevel.Should().Be(ThreatLevel.Low); // CR 1 = Low
    }

    [Test]
    public void DetectedEnemy_CreateWithThreatLevel_HasCorrectProperties()
    {
        // Act
        var enemy = DetectedEnemy.CreateWithThreatLevel(
            "Glitch Horror", 1, ThreatLevel.Extreme, "center of room");

        // Assert
        enemy.EnemyType.Should().Be("Glitch Horror");
        enemy.ThreatLevel.Should().Be(ThreatLevel.Extreme);
        enemy.Position.Should().Be("center of room");
    }

    [Test]
    public void DetectedEnemy_IsGroup_TrueWhenCountGreaterThanOne()
    {
        // Arrange
        var singleEnemy = DetectedEnemy.CreateWithThreatLevel("Scout", 1, ThreatLevel.Low);
        var groupEnemy = DetectedEnemy.CreateWithThreatLevel("Scouts", 3, ThreatLevel.Low);

        // Assert
        singleEnemy.IsGroup.Should().BeFalse();
        groupEnemy.IsGroup.Should().BeTrue();
    }

    [Test]
    public void DetectedEnemy_IsHighPriority_TrueForHighOrExtremeThreat()
    {
        // Arrange
        var lowThreat = DetectedEnemy.CreateWithThreatLevel("Dog", 1, ThreatLevel.Low);
        var moderateThreat = DetectedEnemy.CreateWithThreatLevel("Raider", 1, ThreatLevel.Moderate);
        var highThreat = DetectedEnemy.CreateWithThreatLevel("Boss", 1, ThreatLevel.High);
        var extremeThreat = DetectedEnemy.CreateWithThreatLevel("Horror", 1, ThreatLevel.Extreme);

        // Assert
        lowThreat.IsHighPriority.Should().BeFalse();
        moderateThreat.IsHighPriority.Should().BeFalse();
        highThreat.IsHighPriority.Should().BeTrue();
        extremeThreat.IsHighPriority.Should().BeTrue();
    }

    [Test]
    public void DetectedEnemy_IsHighPriority_TrueForModerateGroup()
    {
        // Arrange - group of moderate threats is high priority
        var moderateGroup = DetectedEnemy.CreateWithThreatLevel(
            "Raiders", 3, ThreatLevel.Moderate);

        // Assert
        moderateGroup.IsHighPriority.Should().BeTrue();
    }

    [Test]
    public void DetectedEnemy_ToDisplayString_ContainsCountAndThreat()
    {
        // Arrange
        var enemy = DetectedEnemy.CreateWithThreatLevel("Raiders", 3, ThreatLevel.Moderate);

        // Act
        var display = enemy.ToDisplayString();

        // Assert
        display.Should().Contain("3");
        display.Should().Contain("Raiders");
        display.Should().Contain("moderate");
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DETECTED HAZARD VALUE OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region DetectedHazard Value Object Tests

    [Test]
    public void DetectedHazard_Create_HasCorrectProperties()
    {
        // Act
        var hazard = DetectedHazard.Create(
            DetectableHazardType.ToxicZone,
            HazardSeverity.Moderate);

        // Assert
        hazard.HazardType.Should().Be(DetectableHazardType.ToxicZone);
        hazard.Severity.Should().Be(HazardSeverity.Moderate);
        hazard.Description.Should().NotBeNullOrEmpty();
        hazard.AvoidanceHint.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void DetectedHazard_CreateCustom_UsesCustomValues()
    {
        // Act
        var hazard = DetectedHazard.CreateCustom(
            DetectableHazardType.HiddenPit,
            HazardSeverity.Severe,
            "A deep chasm hidden by debris",
            "Use rope to descend safely");

        // Assert
        hazard.Description.Should().Be("A deep chasm hidden by debris");
        hazard.AvoidanceHint.Should().Be("Use rope to descend safely");
    }

    [Test]
    public void DetectedHazard_ShouldAvoid_TrueForSevereOrLethal()
    {
        // Arrange
        var minor = DetectedHazard.Create(
            DetectableHazardType.ObviousDanger, HazardSeverity.Minor);
        var severe = DetectedHazard.Create(
            DetectableHazardType.ToxicZone, HazardSeverity.Severe);
        var lethal = DetectedHazard.Create(
            DetectableHazardType.GlitchPocket, HazardSeverity.Lethal);

        // Assert
        minor.ShouldAvoid.Should().BeFalse();
        severe.ShouldAvoid.Should().BeTrue();
        lethal.ShouldAvoid.Should().BeTrue();
    }

    [Test]
    public void DetectedHazard_IsLethal_TrueOnlyForLethalSeverity()
    {
        // Arrange
        var severe = DetectedHazard.Create(
            DetectableHazardType.ToxicZone, HazardSeverity.Severe);
        var lethal = DetectedHazard.Create(
            DetectableHazardType.GlitchPocket, HazardSeverity.Lethal);

        // Assert
        severe.IsLethal.Should().BeFalse();
        lethal.IsLethal.Should().BeTrue();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // POINT OF INTEREST VALUE OBJECT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region PointOfInterest Value Object Tests

    [Test]
    public void PointOfInterest_Create_HasCorrectProperties()
    {
        // Act
        var poi = PointOfInterest.Create(
            PointOfInterestType.Container,
            "A rusted metal locker");

        // Assert
        poi.InterestType.Should().Be(PointOfInterestType.Container);
        poi.Description.Should().Be("A rusted metal locker");
        poi.InteractionHint.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void PointOfInterest_FactoryMethods_CreateCorrectTypes()
    {
        // Act & Assert
        PointOfInterest.Container("A chest").InterestType
            .Should().Be(PointOfInterestType.Container);
        PointOfInterest.ScavengerSign("Faction marker").InterestType
            .Should().Be(PointOfInterestType.ScavengerSign);
        PointOfInterest.Mechanism("Control panel").InterestType
            .Should().Be(PointOfInterestType.Mechanism);
        PointOfInterest.ResourceNode("Salvage pile").InterestType
            .Should().Be(PointOfInterestType.ResourceNode);
        PointOfInterest.Landmark("Ancient monument").InterestType
            .Should().Be(PointOfInterestType.Landmark);
        PointOfInterest.Other("Strange glow").InterestType
            .Should().Be(PointOfInterestType.Other);
    }

    [Test]
    public void PointOfInterest_MayContainItems_TrueForContainerAndResource()
    {
        // Arrange
        var container = PointOfInterest.Container("Chest");
        var resource = PointOfInterest.ResourceNode("Salvage");
        var landmark = PointOfInterest.Landmark("Monument");

        // Assert
        container.MayContainItems.Should().BeTrue();
        resource.MayContainItems.Should().BeTrue();
        landmark.MayContainItems.Should().BeFalse();
    }

    [Test]
    public void PointOfInterest_RequiresSkillCheck_CorrectForEachType()
    {
        // Arrange
        var sign = PointOfInterest.ScavengerSign("Sign");
        var mechanism = PointOfInterest.Mechanism("Panel");
        var container = PointOfInterest.Container("Chest");

        // Assert
        sign.RequiresSkillCheck.Should().BeTrue();
        mechanism.RequiresSkillCheck.Should().BeTrue();
        container.RequiresSkillCheck.Should().BeFalse();
    }

    [Test]
    public void PointOfInterest_MayBeDangerous_TrueForContainerAndMechanism()
    {
        // Arrange
        var container = PointOfInterest.Container("Chest");
        var mechanism = PointOfInterest.Mechanism("Panel");
        var landmark = PointOfInterest.Landmark("Monument");

        // Assert
        container.MayBeDangerous.Should().BeTrue(); // May be trapped
        mechanism.MayBeDangerous.Should().BeTrue(); // May backfire
        landmark.MayBeDangerous.Should().BeFalse();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // THREAT LEVEL ENUM EXTENSION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region ThreatLevel Enum Extension Tests

    [Test]
    [TestCase(0, ThreatLevel.Low)]
    [TestCase(2, ThreatLevel.Low)]
    [TestCase(3, ThreatLevel.Moderate)]
    [TestCase(5, ThreatLevel.Moderate)]
    [TestCase(6, ThreatLevel.High)]
    [TestCase(8, ThreatLevel.High)]
    [TestCase(9, ThreatLevel.Extreme)]
    [TestCase(15, ThreatLevel.Extreme)]
    public void ThreatLevel_FromChallengeRating_ReturnsCorrectLevel(
        int cr, ThreatLevel expected)
    {
        // Act
        var level = ThreatLevelExtensions.FromChallengeRating(cr);

        // Assert
        level.Should().Be(expected);
    }

    [Test]
    public void ThreatLevel_GetDisplayName_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var level in Enum.GetValues<ThreatLevel>())
        {
            level.GetDisplayName().Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void ThreatLevel_GetShortDescriptor_ReturnsExpectedValues()
    {
        // Assert
        ThreatLevel.Low.GetShortDescriptor().Should().Be("minor");
        ThreatLevel.Moderate.GetShortDescriptor().Should().Be("moderate");
        ThreatLevel.High.GetShortDescriptor().Should().Be("dangerous");
        ThreatLevel.Extreme.GetShortDescriptor().Should().Be("deadly");
    }

    [Test]
    public void ThreatLevel_SuggestsCaution_TrueForHighAndExtreme()
    {
        // Assert
        ThreatLevel.Low.SuggestsCaution().Should().BeFalse();
        ThreatLevel.Moderate.SuggestsCaution().Should().BeFalse();
        ThreatLevel.High.SuggestsCaution().Should().BeTrue();
        ThreatLevel.Extreme.SuggestsCaution().Should().BeTrue();
    }

    [Test]
    public void ThreatLevel_SuggestsRetreat_TrueOnlyForExtreme()
    {
        // Assert
        ThreatLevel.High.SuggestsRetreat().Should().BeFalse();
        ThreatLevel.Extreme.SuggestsRetreat().Should().BeTrue();
    }

    [Test]
    public void ThreatLevel_GetTacticalRecommendation_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var level in Enum.GetValues<ThreatLevel>())
        {
            level.GetTacticalRecommendation().Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // HAZARD SEVERITY ENUM EXTENSION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region HazardSeverity Enum Extension Tests

    [Test]
    public void HazardSeverity_GetDisplayName_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var severity in Enum.GetValues<HazardSeverity>())
        {
            severity.GetDisplayName().Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void HazardSeverity_GetShortDescriptor_ReturnsExpectedValues()
    {
        // Assert
        HazardSeverity.Minor.GetShortDescriptor().Should().Be("minor");
        HazardSeverity.Moderate.GetShortDescriptor().Should().Be("moderate");
        HazardSeverity.Severe.GetShortDescriptor().Should().Be("severe");
        HazardSeverity.Lethal.GetShortDescriptor().Should().Be("lethal");
    }

    [Test]
    public void HazardSeverity_SuggestsAvoidance_TrueForModerateAndAbove()
    {
        // Assert - Minor is fine to proceed through, but Moderate+ suggests avoidance
        HazardSeverity.Minor.SuggestsAvoidance().Should().BeFalse();
        HazardSeverity.Moderate.SuggestsAvoidance().Should().BeTrue();
        HazardSeverity.Severe.SuggestsAvoidance().Should().BeTrue();
        HazardSeverity.Lethal.SuggestsAvoidance().Should().BeTrue();
    }

    [Test]
    public void HazardSeverity_IsPotentiallyLethal_TrueOnlyForLethal()
    {
        // Assert
        HazardSeverity.Severe.IsPotentiallyLethal().Should().BeFalse();
        HazardSeverity.Lethal.IsPotentiallyLethal().Should().BeTrue();
    }

    [Test]
    public void HazardSeverity_GetTypicalDamageDice_ReturnsValidDice()
    {
        // Act & Assert
        foreach (var severity in Enum.GetValues<HazardSeverity>())
        {
            var dice = severity.GetTypicalDamageDice();
            dice.Should().NotBeNullOrEmpty();
            dice.Should().MatchRegex(@"^\d+d\d+");
        }
    }

    [Test]
    public void HazardSeverity_RequiresSpecialEquipment_TrueForSevereAndLethal()
    {
        // Assert
        HazardSeverity.Minor.RequiresSpecialEquipment().Should().BeFalse();
        HazardSeverity.Moderate.RequiresSpecialEquipment().Should().BeFalse();
        HazardSeverity.Severe.RequiresSpecialEquipment().Should().BeTrue();
        HazardSeverity.Lethal.RequiresSpecialEquipment().Should().BeTrue();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // POINT OF INTEREST TYPE ENUM EXTENSION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    #region PointOfInterestType Enum Extension Tests

    [Test]
    public void PointOfInterestType_GetDisplayName_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var poiType in Enum.GetValues<PointOfInterestType>())
        {
            poiType.GetDisplayName().Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void PointOfInterestType_GetDescription_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var poiType in Enum.GetValues<PointOfInterestType>())
        {
            poiType.GetDescription().Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void PointOfInterestType_GetDefaultInteractionHint_ReturnsNonEmptyForAll()
    {
        // Act & Assert
        foreach (var poiType in Enum.GetValues<PointOfInterestType>())
        {
            poiType.GetDefaultInteractionHint().Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void PointOfInterestType_ContainsItems_TrueForContainerAndResource()
    {
        // Assert
        PointOfInterestType.Container.ContainsItems().Should().BeTrue();
        PointOfInterestType.ResourceNode.ContainsItems().Should().BeTrue();
        PointOfInterestType.Mechanism.ContainsItems().Should().BeFalse();
        PointOfInterestType.Landmark.ContainsItems().Should().BeFalse();
    }

    [Test]
    public void PointOfInterestType_RequiresSkillCheck_CorrectForTypes()
    {
        // Assert
        PointOfInterestType.ScavengerSign.RequiresSkillCheck().Should().BeTrue();
        PointOfInterestType.Mechanism.RequiresSkillCheck().Should().BeTrue();
        PointOfInterestType.ResourceNode.RequiresSkillCheck().Should().BeTrue();
        PointOfInterestType.Container.RequiresSkillCheck().Should().BeFalse();
        PointOfInterestType.Landmark.RequiresSkillCheck().Should().BeFalse();
    }

    [Test]
    public void PointOfInterestType_MayBeDangerous_CorrectForTypes()
    {
        // Assert
        PointOfInterestType.Container.MayBeDangerous().Should().BeTrue();
        PointOfInterestType.Mechanism.MayBeDangerous().Should().BeTrue();
        PointOfInterestType.ScavengerSign.MayBeDangerous().Should().BeFalse();
        PointOfInterestType.Landmark.MayBeDangerous().Should().BeFalse();
    }

    [Test]
    public void PointOfInterestType_GetAssociatedSkillId_ReturnsExpectedSkills()
    {
        // Assert
        PointOfInterestType.Container.GetAssociatedSkillId().Should().Be("perception");
        PointOfInterestType.ScavengerSign.GetAssociatedSkillId().Should().Be("wasteland-survival");
        PointOfInterestType.Mechanism.GetAssociatedSkillId().Should().Be("technology");
        PointOfInterestType.ResourceNode.GetAssociatedSkillId().Should().Be("wasteland-survival");
        PointOfInterestType.Landmark.GetAssociatedSkillId().Should().BeNull();
        PointOfInterestType.Other.GetAssociatedSkillId().Should().BeNull();
    }

    [Test]
    public void PointOfInterestType_GetIconCharacter_ReturnsDistinctCharacters()
    {
        // Arrange
        var icons = new HashSet<char>();

        // Act
        foreach (var poiType in Enum.GetValues<PointOfInterestType>())
        {
            icons.Add(poiType.GetIconCharacter());
        }

        // Assert - each type should have a distinct icon (except possibly Other)
        icons.Count.Should().BeGreaterThanOrEqualTo(5);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Helper Methods

    /// <summary>
    /// Creates a mock SkillCheckService for testing.
    /// </summary>
    private static SkillCheckService CreateMockSkillCheckService()
    {
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();
#pragma warning disable CS0618 // Type or member is obsolete
        var diceService = new DiceService(diceLogger, seededRandom);
#pragma warning restore CS0618
        var configProvider = Substitute.For<IGameConfigurationProvider>();
        var logger = Substitute.For<ILogger<SkillCheckService>>();

        return new SkillCheckService(diceService, configProvider, logger);
    }

    /// <summary>
    /// Creates a test player with standard attributes for scouting tests.
    /// </summary>
    private static Player CreateTestPlayer()
    {
        return new Player(
            name: "Test Scout",
            raceId: "human",
            backgroundId: "scavenger",
            attributes: new PlayerAttributes(
                might: 8,
                fortitude: 8,
                will: 10,
                wits: 12,
                finesse: 10
            )
        );
    }

    #endregion
}
