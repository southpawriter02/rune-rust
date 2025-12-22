using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for the RoomRenderer static class (v0.3.5c).
/// Tests biome color mapping, object/enemy formatting, and section building.
/// </summary>
public class RoomRendererTests
{
    #region GetBiomeColor Tests

    [Fact]
    public void GetBiomeColor_Industrial_ReturnsOrange()
    {
        // Act
        var result = RoomRenderer.GetBiomeColor(BiomeType.Industrial);

        // Assert
        result.Should().Be("orange1");
    }

    [Fact]
    public void GetBiomeColor_Organic_ReturnsGreen()
    {
        // Act
        var result = RoomRenderer.GetBiomeColor(BiomeType.Organic);

        // Assert
        result.Should().Be("green");
    }

    [Fact]
    public void GetBiomeColor_Void_ReturnsPurple()
    {
        // Act
        var result = RoomRenderer.GetBiomeColor(BiomeType.Void);

        // Assert
        result.Should().Be("purple");
    }

    [Fact]
    public void GetBiomeColor_Ruin_ReturnsGrey()
    {
        // Act
        var result = RoomRenderer.GetBiomeColor(BiomeType.Ruin);

        // Assert
        result.Should().Be("grey");
    }

    [Fact]
    public void GetBiomeColor_UnknownValue_ReturnsGrey()
    {
        // Act - Cast an invalid int to BiomeType
        var result = RoomRenderer.GetBiomeColor((BiomeType)999);

        // Assert
        result.Should().Be("grey");
    }

    #endregion

    #region FormatObjectName Tests

    [Fact]
    public void FormatObjectName_Container_ReturnsGold()
    {
        // Act
        var result = RoomRenderer.FormatObjectName("Iron Chest", isContainer: true, isLocked: false);

        // Assert
        result.Should().Contain("[gold1]");
        result.Should().Contain("Iron Chest");
        result.Should().NotContain("(locked)");
    }

    [Fact]
    public void FormatObjectName_LockedContainer_ShowsLockIndicator()
    {
        // Act
        var result = RoomRenderer.FormatObjectName("Sealed Vault", isContainer: true, isLocked: true);

        // Assert
        result.Should().Contain("[gold1]");
        result.Should().Contain("Sealed Vault");
        result.Should().Contain("(locked)");
    }

    [Fact]
    public void FormatObjectName_GenericObject_ReturnsGrey()
    {
        // Act
        var result = RoomRenderer.FormatObjectName("Pile of Debris", isContainer: false, isLocked: false);

        // Assert
        result.Should().Contain("[grey]");
        result.Should().Contain("Pile of Debris");
    }

    [Fact]
    public void FormatObjectName_NonContainerLocked_IgnoresLockState()
    {
        // Non-containers shouldn't show lock state even if isLocked is true
        // Act
        var result = RoomRenderer.FormatObjectName("Broken Machine", isContainer: false, isLocked: true);

        // Assert
        result.Should().NotContain("(locked)");
        result.Should().Contain("[grey]");
    }

    [Fact]
    public void FormatObjectName_EscapesMarkupCharacters()
    {
        // Act
        var result = RoomRenderer.FormatObjectName("Chest [Ancient]", isContainer: true, isLocked: false);

        // Assert - Should escape the brackets
        result.Should().Contain("[[Ancient]]");
    }

    #endregion

    #region FormatEnemyName Tests

    [Fact]
    public void FormatEnemyName_IncludesHealthStatus()
    {
        // Act - 50% health is in the "Bloodied" range (26-50%)
        var result = RoomRenderer.FormatEnemyName("Rust-Crawler", 50, 100);

        // Assert
        result.Should().Contain("[red]");
        result.Should().Contain("Rust-Crawler");
        result.Should().Contain("(Bloodied)");
    }

    [Fact]
    public void FormatEnemyName_FullHealth_ShowsHealthy()
    {
        // Act
        var result = RoomRenderer.FormatEnemyName("Forge-Wraith", 100, 100);

        // Assert
        result.Should().Contain("(Healthy)");
    }

    [Fact]
    public void FormatEnemyName_ZeroHp_ShowsDead()
    {
        // Act
        var result = RoomRenderer.FormatEnemyName("Defeated Foe", 0, 100);

        // Assert
        result.Should().Contain("(Dead)");
    }

    [Fact]
    public void FormatEnemyName_ZeroMaxHp_ShowsDead()
    {
        // Edge case: MaxHp is 0 (should not divide by zero)
        // Act
        var result = RoomRenderer.FormatEnemyName("Invalid Enemy", 50, 0);

        // Assert
        result.Should().Contain("(Dead)");
    }

    #endregion

    #region GetNarrativeHealth Tests

    [Fact]
    public void GetNarrativeHealth_Above75_ReturnsHealthy()
    {
        // Act
        var result = RoomRenderer.GetNarrativeHealth(80);

        // Assert
        result.Should().Be("Healthy");
    }

    [Fact]
    public void GetNarrativeHealth_Exactly75_ReturnsWounded()
    {
        // 75 is NOT > 75, so it falls to the next tier
        // Act
        var result = RoomRenderer.GetNarrativeHealth(75);

        // Assert
        result.Should().Be("Wounded");
    }

    [Fact]
    public void GetNarrativeHealth_51To75_ReturnsWounded()
    {
        // Act
        var result = RoomRenderer.GetNarrativeHealth(60);

        // Assert
        result.Should().Be("Wounded");
    }

    [Fact]
    public void GetNarrativeHealth_26To50_ReturnsBloodied()
    {
        // Act
        var result = RoomRenderer.GetNarrativeHealth(35);

        // Assert
        result.Should().Be("Bloodied");
    }

    [Fact]
    public void GetNarrativeHealth_1To25_ReturnsCritical()
    {
        // Act
        var result = RoomRenderer.GetNarrativeHealth(10);

        // Assert
        result.Should().Be("Critical");
    }

    [Fact]
    public void GetNarrativeHealth_Zero_ReturnsDead()
    {
        // Act
        var result = RoomRenderer.GetNarrativeHealth(0);

        // Assert
        result.Should().Be("Dead");
    }

    [Fact]
    public void GetNarrativeHealth_Negative_ReturnsDead()
    {
        // Act
        var result = RoomRenderer.GetNarrativeHealth(-10);

        // Assert
        result.Should().Be("Dead");
    }

    #endregion

    #region BuildExitsSection Tests

    [Fact]
    public void BuildExitsSection_WithExits_FormatsCorrectly()
    {
        // Act
        var result = RoomRenderer.BuildExitsSection("north, east, down");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BuildExitsSection_NoExits_ShowsNoExitsMessage()
    {
        // Act
        var result = RoomRenderer.BuildExitsSection("");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BuildExitsSection_NullExits_ShowsNoExitsMessage()
    {
        // Act
        var result = RoomRenderer.BuildExitsSection(null!);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region BuildEntitySection Tests

    [Fact]
    public void BuildEntitySection_Empty_ShowsEmptyMessage()
    {
        // Act
        var result = RoomRenderer.BuildEntitySection(new List<string>(), new List<string>());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BuildEntitySection_WithObjects_ReturnsRenderable()
    {
        // Arrange
        var objects = new List<string> { "[gold1]Chest[/]", "[grey]Debris[/]" };

        // Act
        var result = RoomRenderer.BuildEntitySection(objects, new List<string>());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BuildEntitySection_WithEnemies_ReturnsRenderable()
    {
        // Arrange
        var enemies = new List<string> { "[red]Wolf[/] [grey](Healthy)[/]" };

        // Act
        var result = RoomRenderer.BuildEntitySection(new List<string>(), enemies);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BuildEntitySection_WithBoth_ReturnsRenderable()
    {
        // Arrange
        var objects = new List<string> { "[gold1]Chest[/]" };
        var enemies = new List<string> { "[red]Wolf[/] [grey](Healthy)[/]" };

        // Act
        var result = RoomRenderer.BuildEntitySection(objects, enemies);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region BuildDescription Tests

    [Fact]
    public void BuildDescription_ReturnsRenderable()
    {
        // Act
        var result = RoomRenderer.BuildDescription("A dark corridor stretches before you.");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void BuildDescription_EscapesMarkup()
    {
        // Description with potential markup should be escaped
        // Act
        var result = RoomRenderer.BuildDescription("Warning: [DANGER] ahead!");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Render Integration Tests

    [Fact]
    public void Render_ReturnsPanel()
    {
        // Arrange
        var vm = CreateTestViewModel();

        // Act
        var result = RoomRenderer.Render(vm);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_WithEmptyLists_ReturnsPanel()
    {
        // Arrange
        var vm = CreateTestViewModel(
            visibleObjects: new List<string>(),
            visibleEnemies: new List<string>(),
            exits: ""
        );

        // Act
        var result = RoomRenderer.Render(vm);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Render_WithObjectsAndEnemies_ReturnsPanel()
    {
        // Arrange
        var vm = CreateTestViewModel(
            visibleObjects: new List<string> { "[gold1]Chest[/]", "[grey]Debris[/]" },
            visibleEnemies: new List<string> { "[red]Wolf[/] [grey](Healthy)[/]" },
            exits: "north, south"
        );

        // Act
        var result = RoomRenderer.Render(vm);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static ExplorationViewModel CreateTestViewModel(
        string roomName = "Test Chamber",
        string roomDescription = "A dusty room filled with ancient machinery.",
        List<string>? visibleObjects = null,
        List<string>? visibleEnemies = null,
        string exits = "north, east",
        string biomeColor = "orange1")
    {
        return new ExplorationViewModel(
            CharacterName: "Test Character",
            CurrentHp: 50,
            MaxHp: 100,
            CurrentStamina: 30,
            MaxStamina: 50,
            CurrentStress: 25,
            MaxStress: 100,
            CurrentCorruption: 10,
            MaxCorruption: 100,
            RoomName: roomName,
            RoomDescription: roomDescription,
            TurnCount: 1,
            PlayerPosition: Coordinate.Origin,
            LocalMapRooms: new List<Room>(),
            VisitedRoomIds: new HashSet<Guid>(),
            VisibleObjects: visibleObjects ?? new List<string> { "[gold1]Iron Chest[/]" },
            VisibleEnemies: visibleEnemies ?? new List<string>(),
            Exits: exits,
            BiomeColor: biomeColor
        );
    }

    #endregion
}
