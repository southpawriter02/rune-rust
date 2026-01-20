using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Room hazard zone management methods.
/// </summary>
[TestFixture]
public class RoomHazardTests
{
    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _room = new Room("Test Room", "A test room.", Position3D.Origin);
    }

    [Test]
    public void AddHazardZone_AddsToCollection()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "fire-pit", "Fire Pit", "Test",
            HazardType.Fire);

        // Act
        _room.AddHazardZone(hazard);

        // Assert
        _room.HazardZones.Should().HaveCount(1);
        _room.HazardZones.Should().Contain(hazard);
        _room.HasHazards.Should().BeTrue();
    }

    [Test]
    public void RemoveHazardZone_RemovesFromCollection()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "fire-pit", "Fire Pit", "Test",
            HazardType.Fire);
        _room.AddHazardZone(hazard);

        // Act
        var removed = _room.RemoveHazardZone(hazard);

        // Assert
        removed.Should().BeTrue();
        _room.HazardZones.Should().BeEmpty();
        _room.HasHazards.Should().BeFalse();
    }

    [Test]
    public void GetActiveHazards_ReturnsOnlyActive()
    {
        // Arrange
        var activeHazard = HazardZone.Create(
            "active", "Active Hazard", "Test",
            HazardType.Fire);
        var inactiveHazard = HazardZone.Create(
            "inactive", "Inactive Hazard", "Test",
            HazardType.Ice);
        inactiveHazard.Deactivate();

        _room.AddHazardZone(activeHazard);
        _room.AddHazardZone(inactiveHazard);

        // Act
        var active = _room.GetActiveHazards().ToList();

        // Assert
        active.Should().HaveCount(1);
        active.Should().Contain(activeHazard);
        active.Should().NotContain(inactiveHazard);
        _room.HasActiveHazards.Should().BeTrue();
    }

    [Test]
    public void GetHazardByKeyword_FindsHazard()
    {
        // Arrange
        var hazard = HazardZone.Create(
            "fire-pit", "Blazing Fire Pit", "Test",
            HazardType.Fire,
            keywords: new[] { "fire", "pit", "flames" });
        _room.AddHazardZone(hazard);

        // Act
        var foundByKeyword = _room.GetHazardByKeyword("fire");
        var foundByName = _room.GetHazardByKeyword("Blazing");
        var notFound = _room.GetHazardByKeyword("water");

        // Assert
        foundByKeyword.Should().Be(hazard);
        foundByName.Should().Be(hazard);
        notFound.Should().BeNull();
    }

    [Test]
    public void GetHazardsByType_ReturnsMatchingHazards()
    {
        // Arrange
        var fireHazard1 = HazardZone.Create("fire1", "Fire 1", "Test", HazardType.Fire);
        var fireHazard2 = HazardZone.Create("fire2", "Fire 2", "Test", HazardType.Fire);
        var iceHazard = HazardZone.Create("ice", "Ice", "Test", HazardType.Ice);

        _room.AddHazardZone(fireHazard1);
        _room.AddHazardZone(fireHazard2);
        _room.AddHazardZone(iceHazard);

        // Act
        var fireHazards = _room.GetHazardsByType(HazardType.Fire).ToList();
        var iceHazards = _room.GetHazardsByType(HazardType.Ice).ToList();

        // Assert
        fireHazards.Should().HaveCount(2);
        iceHazards.Should().HaveCount(1);
    }

    [Test]
    public void RemoveExpiredHazards_RemovesInactiveExpired()
    {
        // Arrange
        var permanentHazard = HazardZone.Create("perm", "Permanent", "Test", HazardType.Fire);
        var tempHazard = HazardZone.Create("temp", "Temporary", "Test", HazardType.Ice, duration: 1);

        _room.AddHazardZone(permanentHazard);
        _room.AddHazardZone(tempHazard);

        // Tick the temporary hazard to expire it
        tempHazard.ProcessTurnTick();

        // Act
        var removed = _room.RemoveExpiredHazards();

        // Assert
        removed.Should().Be(1);
        _room.HazardZones.Should().HaveCount(1);
        _room.HazardZones.Should().Contain(permanentHazard);
    }

    [Test]
    public void HasActiveHazards_WhenNoActiveHazards_ReturnsFalse()
    {
        // Arrange
        var hazard = HazardZone.Create("test", "Test", "Test", HazardType.Fire);
        hazard.Deactivate();
        _room.AddHazardZone(hazard);

        // Act & Assert
        _room.HasHazards.Should().BeTrue();     // Has hazards (even if inactive)
        _room.HasActiveHazards.Should().BeFalse(); // But no active ones
    }
}
