using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for TemplateValidationService.
/// </summary>
[TestFixture]
public class TemplateValidationServiceTests
{
    private TemplateValidationService _service = null!;
    private RoomTemplateConfiguration _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new RoomTemplateConfiguration();
        _service = new TemplateValidationService(
            _config,
            NullLogger<TemplateValidationService>.Instance);
    }

    [Test]
    public void ValidateTemplate_ValidTemplate_ReturnsSuccess()
    {
        // Arrange
        var template = new RoomTemplate(
            templateId: "test-template",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: []);

        // Act
        var result = _service.ValidateTemplate(template);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateSlot_DescriptionWithoutPool_ReturnsError()
    {
        // Arrange
        var slot = new TemplateSlot
        {
            SlotId = "adjective",
            Type = SlotType.Description,
            DescriptorPool = null
        };

        // Act
        var errors = _service.ValidateSlot(slot, "test-template");

        // Assert
        errors.Should().Contain(e => e.Contains("descriptor pool"));
    }

    [Test]
    public void ValidateSlot_NegativeMinQuantity_ReturnsError()
    {
        // Arrange
        var slot = new TemplateSlot
        {
            SlotId = "monster_01",
            Type = SlotType.Monster,
            MinQuantity = -1
        };

        // Act
        var errors = _service.ValidateSlot(slot, "test-template");

        // Assert
        errors.Should().Contain(e => e.Contains("Minimum quantity cannot be negative"));
    }

    [Test]
    public void ValidateSlot_InvalidFillProbability_ReturnsError()
    {
        // Arrange
        var slot = new TemplateSlot
        {
            SlotId = "item_01",
            Type = SlotType.Item,
            FillProbability = 1.5f
        };

        // Act
        var errors = _service.ValidateSlot(slot, "test-template");

        // Assert
        errors.Should().Contain(e => e.Contains("Fill probability must be between"));
    }

    [Test]
    public void ValidateSlot_InvalidMonsterTier_ReturnsError()
    {
        // Arrange
        var constraints = new Dictionary<string, string> { ["minTier"] = "invalid" };
        var slot = TemplateSlot.Monster("monster_01", constraints: constraints);

        // Act
        var errors = _service.ValidateSlot(slot, "test-template");

        // Assert
        errors.Should().Contain(e => e.Contains("Invalid minTier"));
    }

    [Test]
    public void ValidateSlot_InvalidContainerLootQuality_ReturnsError()
    {
        // Arrange
        var constraints = new Dictionary<string, string> { ["lootQuality"] = "invalid" };
        var slot = TemplateSlot.Container("container_01", constraints: constraints);

        // Act
        var errors = _service.ValidateSlot(slot, "test-template");

        // Assert
        errors.Should().Contain(e => e.Contains("Invalid lootQuality"));
    }

    [Test]
    public void ValidateAllTemplates_ReturnsResultsForAllTemplates()
    {
        // Arrange
        _config.Templates["template1"] = new RoomTemplate(
            templateId: "template1",
            namePattern: "Room 1",
            descriptionPattern: "A room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: []);

        _config.Templates["template2"] = new RoomTemplate(
            templateId: "template2",
            namePattern: "Room 2",
            descriptionPattern: "Another room.",
            validBiomes: ["cave"],
            roomType: RoomType.Standard,
            slots: []);

        // Act
        var results = _service.ValidateAllTemplates();

        // Assert
        results.Should().HaveCount(2);
        results.Select(r => r.TemplateId).Should().Contain("template1");
        results.Select(r => r.TemplateId).Should().Contain("template2");
    }

    [Test]
    public void ValidateTemplate_WithDuplicateSlotIds_ReturnsError()
    {
        // Arrange
        var slots = new List<TemplateSlot>
        {
            TemplateSlot.Monster("monster_01"),
            TemplateSlot.Monster("monster_01")
        };
        var template = new RoomTemplate(
            templateId: "test-template",
            namePattern: "Test Room",
            descriptionPattern: "A test room.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: slots);

        // Act
        var result = _service.ValidateTemplate(template);

        // Assert
        result.Errors.Should().Contain(e => e.Contains("Duplicate slot ID"));
    }
}
