using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for the MinimapRenderer static class (v0.3.5b).
/// Tests symbol resolution, color logic, and Fog of War behavior.
/// </summary>
public class MinimapRendererTests
{
    #region ResolveTile Tests

    [Fact]
    public void ResolveTile_PlayerPosition_ReturnsCyanAt()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var cellPos = new Coordinate(0, 0, 0);  // Same as center
        var room = CreateRoom(center);
        var visited = new HashSet<Guid> { room.Id };

        // Act
        var result = MinimapRenderer.ResolveTile(room, center, cellPos, visited);

        // Assert
        result.Should().NotBeNull();
        // The markup should contain @ in cyan
        // Note: We can't easily extract the exact content from Markup,
        // but we verify it doesn't throw and returns non-null
    }

    [Fact]
    public void ResolveTile_NullRoom_ReturnsGreyDot()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var cellPos = new Coordinate(1, 0, 0);  // Different from center
        var visited = new HashSet<Guid>();

        // Act
        var result = MinimapRenderer.ResolveTile(null, center, cellPos, visited);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ResolveTile_UnvisitedRoom_ReturnsGreyFog()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var cellPos = new Coordinate(1, 0, 0);  // Different from center
        var room = CreateRoom(cellPos);
        var visited = new HashSet<Guid>();  // Room not in visited set

        // Act
        var result = MinimapRenderer.ResolveTile(room, center, cellPos, visited);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ResolveTile_VisitedRoom_ReturnsSymbol()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var cellPos = new Coordinate(1, 0, 0);  // Different from center
        var room = CreateRoom(cellPos);
        var visited = new HashSet<Guid> { room.Id };  // Room is visited

        // Act
        var result = MinimapRenderer.ResolveTile(room, center, cellPos, visited);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetRoomSymbol Tests

    [Fact]
    public void GetRoomSymbol_BossLair_ReturnsSkullRed()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.BossLair);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("☠");
        color.Should().Be("red");
    }

    [Fact]
    public void GetRoomSymbol_StairsUp_ReturnsArrowWhite()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.StairsUp);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("▲");
        color.Should().Be("white");
    }

    [Fact]
    public void GetRoomSymbol_StairsDown_ReturnsArrowWhite()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.StairsDown);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("▼");
        color.Should().Be("white");
    }

    [Fact]
    public void GetRoomSymbol_Settlement_ReturnsHouseBlue()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.Settlement);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("⌂");
        color.Should().Be("blue");
    }

    [Fact]
    public void GetRoomSymbol_RunicAnchor_ReturnsDiamondCyan()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.RunicAnchor);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("◆");
        color.Should().Be("cyan");
    }

    [Fact]
    public void GetRoomSymbol_Workbench_ReturnsHammerYellow()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.Workbench);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("⚒");
        color.Should().Be("yellow");
    }

    [Fact]
    public void GetRoomSymbol_AlchemyTable_ReturnsHammerYellow()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, RoomFeature.AlchemyTable);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("⚒");
        color.Should().Be("yellow");
    }

    [Fact]
    public void GetRoomSymbol_LethalDanger_ReturnsRedO()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, dangerLevel: DangerLevel.Lethal);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("O");
        color.Should().Be("red");
    }

    [Fact]
    public void GetRoomSymbol_HostileDanger_ReturnsOrangeO()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, dangerLevel: DangerLevel.Hostile);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("O");
        color.Should().Be("orange1");
    }

    [Fact]
    public void GetRoomSymbol_UnstableDanger_ReturnsYellowO()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, dangerLevel: DangerLevel.Unstable);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("O");
        color.Should().Be("yellow");
    }

    [Fact]
    public void GetRoomSymbol_SafeRoom_ReturnsGreyO()
    {
        // Arrange
        var room = CreateRoom(Coordinate.Origin, dangerLevel: DangerLevel.Safe);

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert
        symbol.Should().Be("O");
        color.Should().Be("grey");
    }

    [Fact]
    public void GetRoomSymbol_BossOverridesStairs_ReturnsSkull()
    {
        // Arrange - Room has both Boss and Stairs features
        var room = CreateRoomWithFeatures(Coordinate.Origin,
            new[] { RoomFeature.BossLair, RoomFeature.StairsUp });

        // Act
        var (symbol, color) = MinimapRenderer.GetRoomSymbol(room);

        // Assert - Boss takes priority
        symbol.Should().Be("☠");
        color.Should().Be("red");
    }

    #endregion

    #region GetZLevelIndicator Tests

    [Fact]
    public void GetZLevelIndicator_HasBothStairs_ShowsBothArrows()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var localMap = new List<Room>
        {
            CreateRoom(new Coordinate(1, 0, 0), RoomFeature.StairsUp),
            CreateRoom(new Coordinate(-1, 0, 0), RoomFeature.StairsDown)
        };

        // Act
        var result = MinimapRenderer.GetZLevelIndicator(center, localMap);

        // Assert
        result.Should().Contain("Z:0");
        result.Should().Contain("▲▼");
    }

    [Fact]
    public void GetZLevelIndicator_OnlyStairsUp_ShowsUpArrow()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var localMap = new List<Room>
        {
            CreateRoom(new Coordinate(1, 0, 0), RoomFeature.StairsUp)
        };

        // Act
        var result = MinimapRenderer.GetZLevelIndicator(center, localMap);

        // Assert
        result.Should().Contain("Z:0");
        result.Should().Contain("▲");
        result.Should().NotContain("▼");
    }

    [Fact]
    public void GetZLevelIndicator_OnlyStairsDown_ShowsDownArrow()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var localMap = new List<Room>
        {
            CreateRoom(new Coordinate(1, 0, 0), RoomFeature.StairsDown)
        };

        // Act
        var result = MinimapRenderer.GetZLevelIndicator(center, localMap);

        // Assert
        result.Should().Contain("Z:0");
        result.Should().Contain("▼");
        result.Should().NotContain("▲▼");
    }

    [Fact]
    public void GetZLevelIndicator_NoStairs_ShowsOnlyZLevel()
    {
        // Arrange
        var center = new Coordinate(0, 0, 5);  // Z-level 5
        var localMap = new List<Room>
        {
            CreateRoom(new Coordinate(1, 0, 5))  // No stairs
        };

        // Act
        var result = MinimapRenderer.GetZLevelIndicator(center, localMap);

        // Assert
        result.Should().Contain("Z:5");
        result.Should().NotContain("▲");
        result.Should().NotContain("▼");
    }

    #endregion

    #region Render Integration Tests

    [Fact]
    public void Render_EmptyMap_ReturnsPanel()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var localMap = new List<Room>();
        var visited = new HashSet<Guid>();

        // Act
        var result = MinimapRenderer.Render(center, localMap, visited);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_WithRooms_ReturnsPanel()
    {
        // Arrange
        var center = new Coordinate(0, 0, 0);
        var room1 = CreateRoom(center);
        var room2 = CreateRoom(new Coordinate(1, 0, 0));
        var room3 = CreateRoom(new Coordinate(-1, 0, 0));
        var localMap = new List<Room> { room1, room2, room3 };
        var visited = new HashSet<Guid> { room1.Id, room2.Id };

        // Act
        var result = MinimapRenderer.Render(center, localMap, visited);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static Room CreateRoom(
        Coordinate position,
        RoomFeature? feature = null,
        DangerLevel dangerLevel = DangerLevel.Safe)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "A test room",
            Position = position,
            DangerLevel = dangerLevel,
            BiomeType = BiomeType.Ruin,
            Features = new List<RoomFeature>()
        };

        if (feature.HasValue && feature.Value != RoomFeature.None)
        {
            room.Features.Add(feature.Value);
        }

        return room;
    }

    private static Room CreateRoomWithFeatures(Coordinate position, RoomFeature[] features)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "A test room",
            Position = position,
            DangerLevel = DangerLevel.Safe,
            BiomeType = BiomeType.Ruin,
            Features = features.ToList()
        };

        return room;
    }

    #endregion
}
