using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class RoomHiddenElementsTests
{
    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _room = new Room("Test Room", "A test room", new Position(0, 0), Biome.Citadel);
    }

    [Test]
    public void Room_InitiallyHasNoHiddenElements()
    {
        // Assert
        _room.HiddenElements.Should().BeEmpty();
        _room.HasUnrevealedElements.Should().BeFalse();
        _room.HasRevealedElements.Should().BeFalse();
    }

    [Test]
    public void AddHiddenElement_AddsElementToRoom()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Test Trap", "A trap", 15, 10, 12, _room.Id);

        // Act
        _room.AddHiddenElement(trap);

        // Assert
        _room.HiddenElements.Should().HaveCount(1);
        _room.HasUnrevealedElements.Should().BeTrue();
    }

    [Test]
    public void GetUnrevealedElements_ReturnsOnlyUnrevealed()
    {
        // Arrange
        var trap1 = HiddenElement.CreateTrap("Trap 1", "A trap", 15, 10, 12, _room.Id);
        var trap2 = HiddenElement.CreateTrap("Trap 2", "Another trap", 15, 10, 12, _room.Id);
        _room.AddHiddenElement(trap1);
        _room.AddHiddenElement(trap2);
        trap1.Reveal();

        // Act
        var unrevealed = _room.GetUnrevealedElements().ToList();

        // Assert
        unrevealed.Should().HaveCount(1);
        unrevealed.Should().Contain(trap2);
        unrevealed.Should().NotContain(trap1);
    }

    [Test]
    public void GetRevealedElements_ReturnsOnlyRevealed()
    {
        // Arrange
        var trap1 = HiddenElement.CreateTrap("Trap 1", "A trap", 15, 10, 12, _room.Id);
        var trap2 = HiddenElement.CreateTrap("Trap 2", "Another trap", 15, 10, 12, _room.Id);
        _room.AddHiddenElement(trap1);
        _room.AddHiddenElement(trap2);
        trap1.Reveal();

        // Act
        var revealed = _room.GetRevealedElements().ToList();

        // Assert
        revealed.Should().HaveCount(1);
        revealed.Should().Contain(trap1);
        revealed.Should().NotContain(trap2);
    }

    [Test]
    public void CheckPassivePerception_RevealsElementsAtOrBelowDC()
    {
        // Arrange
        var easyTrap = HiddenElement.CreateTrap("Easy Trap", "Easy to spot", 10, 10, 12, _room.Id);
        var mediumTrap = HiddenElement.CreateTrap("Medium Trap", "Medium difficulty", 15, 10, 12, _room.Id);
        var hardTrap = HiddenElement.CreateTrap("Hard Trap", "Hard to spot", 20, 10, 12, _room.Id);

        _room.AddHiddenElement(easyTrap);
        _room.AddHiddenElement(mediumTrap);
        _room.AddHiddenElement(hardTrap);

        // Act - Passive perception of 15
        var revealed = _room.CheckPassivePerception(15);

        // Assert
        revealed.Should().HaveCount(2);
        revealed.Should().Contain(easyTrap);
        revealed.Should().Contain(mediumTrap);
        revealed.Should().NotContain(hardTrap);

        easyTrap.IsRevealed.Should().BeTrue();
        mediumTrap.IsRevealed.Should().BeTrue();
        hardTrap.IsRevealed.Should().BeFalse();
    }

    [Test]
    public void CheckPassivePerception_DoesNotRevealAlreadyRevealed()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Trap", "A trap", 10, 10, 12, _room.Id);
        _room.AddHiddenElement(trap);
        trap.Reveal();

        // Act
        var revealed = _room.CheckPassivePerception(20);

        // Assert
        revealed.Should().BeEmpty();
    }

    [Test]
    public void CheckPassivePerception_ReturnsEmptyListWhenNoElementsDetectable()
    {
        // Arrange
        var hardTrap = HiddenElement.CreateTrap("Hard Trap", "Very hard to spot", 25, 10, 12, _room.Id);
        _room.AddHiddenElement(hardTrap);

        // Act
        var revealed = _room.CheckPassivePerception(10);

        // Assert
        revealed.Should().BeEmpty();
        hardTrap.IsRevealed.Should().BeFalse();
    }

    [Test]
    public void PerformActiveSearch_UsesFullCheckResult()
    {
        // Arrange
        var hardTrap = HiddenElement.CreateTrap("Hard Trap", "Very hard to spot", 20, 10, 12, _room.Id);
        _room.AddHiddenElement(hardTrap);

        // Act - Active search result of 22 (high roll + WITS)
        var revealed = _room.PerformActiveSearch(22);

        // Assert
        revealed.Should().HaveCount(1);
        revealed.Should().Contain(hardTrap);
        hardTrap.IsRevealed.Should().BeTrue();
    }

    [Test]
    public void Room_WithBiome_StoresBiomeCorrectly()
    {
        // Arrange & Act
        var rootsRoom = new Room("Root Chamber", "A fungal chamber", new Position(0, 0), Biome.TheRoots);
        var fireRoom = new Room("Magma Forge", "A volcanic forge", new Position(1, 0), Biome.Muspelheim);

        // Assert
        rootsRoom.Biome.Should().Be(Biome.TheRoots);
        fireRoom.Biome.Should().Be(Biome.Muspelheim);
    }

    [Test]
    public void Room_DefaultBiome_IsCitadel()
    {
        // Arrange & Act
        var room = new Room("Standard Room", "A standard room", new Position(0, 0));

        // Assert
        room.Biome.Should().Be(Biome.Citadel);
    }

    [Test]
    public void HasUnrevealedElements_UpdatesWhenElementsRevealed()
    {
        // Arrange
        var trap = HiddenElement.CreateTrap("Trap", "A trap", 10, 10, 12, _room.Id);
        _room.AddHiddenElement(trap);

        // Assert - Initially has unrevealed
        _room.HasUnrevealedElements.Should().BeTrue();
        _room.HasRevealedElements.Should().BeFalse();

        // Act
        trap.Reveal();

        // Assert - Now has revealed but no unrevealed
        _room.HasUnrevealedElements.Should().BeFalse();
        _room.HasRevealedElements.Should().BeTrue();
    }
}
