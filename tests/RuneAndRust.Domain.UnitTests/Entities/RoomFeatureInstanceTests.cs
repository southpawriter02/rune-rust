using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

[TestFixture]
public class RoomFeatureInstanceTests
{
    [Test]
    public void Create_WithValidParameters_ReturnsInstance()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            roomId);

        // Assert
        feature.Should().NotBeNull();
        feature.Id.Should().NotBeEmpty();
        feature.RoomId.Should().Be(roomId);
        feature.FeatureType.Should().Be(RoomFeatureType.Interactable);
        feature.FeatureId.Should().Be("weapon_rack");
        feature.DisplayName.Should().Be("Weapon Rack");
        feature.DescriptorOverride.Should().BeNull();
        feature.IsExamined.Should().BeFalse();
    }

    [Test]
    public void Create_WithDescriptorOverride_StoresOverride()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var customDescription = "A specially crafted weapon rack.";

        // Act
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            roomId,
            customDescription);

        // Assert
        feature.DescriptorOverride.Should().Be(customDescription);
    }

    [Test]
    public void Create_WithNullFeatureId_ThrowsArgumentNullException()
    {
        // Act
        var act = () => RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            null!,
            "Weapon Rack",
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentNullException()
    {
        // Act
        var act = () => RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            null!,
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void MarkExamined_SetsIsExaminedToTrue()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Decoration,
            "broken_fountain",
            "Broken Fountain",
            Guid.NewGuid());

        // Act
        feature.MarkExamined();

        // Assert
        feature.IsExamined.Should().BeTrue();
    }

    [Test]
    public void MatchesName_WithExactDisplayName_ReturnsTrue()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            Guid.NewGuid());

        // Act & Assert
        feature.MatchesName("Weapon Rack").Should().BeTrue();
    }

    [Test]
    public void MatchesName_WithDisplayNameCaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            Guid.NewGuid());

        // Act & Assert
        feature.MatchesName("weapon rack").Should().BeTrue();
        feature.MatchesName("WEAPON RACK").Should().BeTrue();
    }

    [Test]
    public void MatchesName_WithFeatureId_ReturnsTrue()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            Guid.NewGuid());

        // Act & Assert
        feature.MatchesName("weapon_rack").Should().BeTrue();
    }

    [Test]
    public void MatchesName_WithFeatureIdSpaceSeparated_ReturnsTrue()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            Guid.NewGuid());

        // Act & Assert
        feature.MatchesName("weapon rack").Should().BeTrue();
    }

    [Test]
    public void MatchesName_WithUnrelatedName_ReturnsFalse()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            Guid.NewGuid());

        // Act & Assert
        feature.MatchesName("bookshelf").Should().BeFalse();
    }

    [Test]
    public void MatchesName_WithNullOrEmpty_ReturnsFalse()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.Interactable,
            "weapon_rack",
            "Weapon Rack",
            Guid.NewGuid());

        // Act & Assert
        feature.MatchesName(null!).Should().BeFalse();
        feature.MatchesName("").Should().BeFalse();
        feature.MatchesName("   ").Should().BeFalse();
    }

    [Test]
    public void ToString_ReturnsDisplayNameAndType()
    {
        // Arrange
        var feature = RoomFeatureInstance.Create(
            RoomFeatureType.LightSource,
            "glowing_fungus",
            "Glowing Fungus",
            Guid.NewGuid());

        // Act
        var result = feature.ToString();

        // Assert
        result.Should().Be("Glowing Fungus (LightSource)");
    }

    [TestCase(RoomFeatureType.Interactable)]
    [TestCase(RoomFeatureType.Decoration)]
    [TestCase(RoomFeatureType.LightSource)]
    [TestCase(RoomFeatureType.Hazard)]
    public void Create_WithDifferentFeatureTypes_SetsCorrectType(RoomFeatureType featureType)
    {
        // Act
        var feature = RoomFeatureInstance.Create(
            featureType,
            "test_feature",
            "Test Feature",
            Guid.NewGuid());

        // Assert
        feature.FeatureType.Should().Be(featureType);
    }
}
