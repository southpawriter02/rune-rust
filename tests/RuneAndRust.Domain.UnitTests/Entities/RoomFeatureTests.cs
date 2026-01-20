using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class RoomFeatureTests
{
    private Room _room = null!;

    [SetUp]
    public void Setup()
    {
        _room = new Room("Test Room", "A test room", new Position(0, 0), Biome.Citadel);
    }

    [Test]
    public void NewRoom_HasNoFeatures()
    {
        // Assert
        _room.Features.Should().BeEmpty();
        _room.HasFeatures.Should().BeFalse();
    }

    [Test]
    public void AddFeature_AddsFeatureToRoom()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            _room.Id);

        // Act
        _room.AddFeature(feature);

        // Assert
        _room.Features.Should().HaveCount(1);
        _room.Features.Should().Contain(feature);
        _room.HasFeatures.Should().BeTrue();
    }

    [Test]
    public void AddFeature_WithNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _room.AddFeature(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddFeature_MultipleFeatures_AddsAllFeatures()
    {
        // Arrange
        var feature1 = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            _room.Id);
        var feature2 = RoomFeatureInstance.Create(
            RoomFeatureType.LightSource,
            "reading_lamp",
            "Reading Lamp",
            _room.Id);

        // Act
        _room.AddFeature(feature1);
        _room.AddFeature(feature2);

        // Assert
        _room.Features.Should().HaveCount(2);
        _room.Features.Should().Contain(feature1);
        _room.Features.Should().Contain(feature2);
    }

    [Test]
    public void GetFeatureByName_WithExistingFeature_ReturnsFeature()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            _room.Id);
        _room.AddFeature(feature);

        // Act
        var found = _room.GetFeatureByName("Weapon Rack");

        // Assert
        found.Should().Be(feature);
    }

    [Test]
    public void GetFeatureByName_WithFeatureIdFormat_ReturnsFeature()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            _room.Id);
        _room.AddFeature(feature);

        // Act
        var found = _room.GetFeatureByName("weapon_rack");

        // Assert
        found.Should().Be(feature);
    }

    [Test]
    public void GetFeatureByName_CaseInsensitive_ReturnsFeature()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Decoration,
            "broken_fountain",
            "Broken Fountain",
            _room.Id);
        _room.AddFeature(feature);

        // Act
        var found = _room.GetFeatureByName("broken fountain");

        // Assert
        found.Should().Be(feature);
    }

    [Test]
    public void GetFeatureByName_WithNonExistingFeature_ReturnsNull()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            _room.Id);
        _room.AddFeature(feature);

        // Act
        var found = _room.GetFeatureByName("bookshelf");

        // Assert
        found.Should().BeNull();
    }

    [Test]
    public void GetFeatureByName_WithEmptyRoom_ReturnsNull()
    {
        // Act
        var found = _room.GetFeatureByName("weapon_rack");

        // Assert
        found.Should().BeNull();
    }

    [Test]
    public void Features_IsReadOnly()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            _room.Id);
        _room.AddFeature(feature);

        // Assert
        _room.Features.Should().BeAssignableTo<IReadOnlyList<RoomFeatureInstance>>();
    }
}
