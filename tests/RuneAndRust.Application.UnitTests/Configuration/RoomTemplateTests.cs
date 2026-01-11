using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Configuration;

/// <summary>
/// Tests for RoomTemplate entity from Domain layer.
/// </summary>
[TestFixture]
public class RoomTemplateTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesTemplate()
    {
        // Arrange & Act
        var template = new RoomTemplate(
            templateId: "test-template",
            namePattern: "Test {adjective} Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            weight: 10);

        // Assert
        template.TemplateId.Should().Be("test-template");
        template.NamePattern.Should().Be("Test {adjective} Room");
        template.RoomType.Should().Be(RoomType.Standard);
        template.Weight.Should().Be(10);
    }

    [Test]
    public void Constructor_WithEmptyTemplateId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RoomTemplate(
            templateId: "",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: []);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNoValidBiomes_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: [],
            roomType: RoomType.Standard,
            slots: []);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void IsValidForBiome_WithMatchingBiome_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon", "cave"],
            roomType: RoomType.Standard,
            slots: []);

        // Act & Assert
        template.IsValidForBiome("dungeon").Should().BeTrue();
        template.IsValidForBiome("cave").Should().BeTrue();
    }

    [Test]
    public void IsValidForBiome_WithNonMatchingBiome_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: []);

        // Act & Assert
        template.IsValidForBiome("volcanic").Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_WithinRange_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            minDepth: 2,
            maxDepth: 5);

        // Act & Assert
        template.IsValidForDepth(3).Should().BeTrue();
    }

    [Test]
    public void IsValidForDepth_BelowMinDepth_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            minDepth: 2,
            maxDepth: 5);

        // Act & Assert
        template.IsValidForDepth(1).Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_AboveMaxDepth_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            minDepth: 2,
            maxDepth: 5);

        // Act & Assert
        template.IsValidForDepth(6).Should().BeFalse();
    }

    [Test]
    public void IsValidForDepth_NoMaxDepth_AcceptsAnyDepthAboveMin()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            minDepth: 2,
            maxDepth: null);

        // Act & Assert
        template.IsValidForDepth(100).Should().BeTrue();
    }

    [Test]
    public void HasAllTags_WithAllTagsPresent_ReturnsTrue()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            tags: ["corridor", "dark", "narrow"]);

        // Act & Assert
        template.HasAllTags(["corridor", "dark"]).Should().BeTrue();
    }

    [Test]
    public void HasAllTags_WithMissingTags_ReturnsFalse()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            tags: ["corridor"]);

        // Act & Assert
        template.HasAllTags(["corridor", "dark"]).Should().BeFalse();
    }

    [Test]
    public void GetSlotsByType_ReturnsMatchingSlots()
    {
        // Arrange
        var monsterSlot = TemplateSlot.Monster("monster_01");
        var itemSlot = TemplateSlot.Item("item_01");
        var descSlot = TemplateSlot.Description("adjective", "room.adjectives");

        var template = new RoomTemplate(
            templateId: "test",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [monsterSlot, itemSlot, descSlot]);

        // Act & Assert
        template.GetSlotsByType(SlotType.Monster).Should().HaveCount(1);
        template.GetSlotsByType(SlotType.Description).Should().HaveCount(1);
    }
}
