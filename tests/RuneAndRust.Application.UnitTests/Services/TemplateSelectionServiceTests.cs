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
/// Tests for TemplateSelectionService.
/// </summary>
[TestFixture]
public class TemplateSelectionServiceTests
{
    private TemplateSelectionService _service = null!;
    private RoomTemplateConfiguration _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = CreateTestConfiguration();
        _service = new TemplateSelectionService(_config, NullLogger<TemplateSelectionService>.Instance);
    }

    [Test]
    public void SelectTemplate_WithValidContext_ReturnsMatchingTemplate()
    {
        // Arrange
        var context = new TemplateSelectionContext { Biome = "dungeon", Depth = 1, Seed = 42 };

        // Act
        var result = _service.SelectTemplate(context);

        // Assert
        result.Should().NotBeNull();
        result!.ValidBiomes.Should().Contain("dungeon");
        result.MinDepth.Should().BeLessThanOrEqualTo(1);
    }

    [Test]
    public void SelectTemplate_WithNoMatches_ReturnsNull()
    {
        // Arrange
        var context = new TemplateSelectionContext { Biome = "underwater", Depth = 1 };

        // Act
        var result = _service.SelectTemplate(context);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetValidTemplates_FiltersByBiome_ReturnsOnlyMatching()
    {
        // Arrange
        var context = new TemplateSelectionContext { Biome = "cave", Depth = 3 };

        // Act
        var results = _service.GetValidTemplates(context);

        // Assert
        results.Should().OnlyContain(t => t.IsValidForBiome("cave"));
    }

    [Test]
    public void GetValidTemplates_FiltersByDepth_ReturnsOnlyInRange()
    {
        // Arrange
        var context = new TemplateSelectionContext { Biome = "dungeon", Depth = 5 };

        // Act
        var results = _service.GetValidTemplates(context);

        // Assert
        results.Should().OnlyContain(t => t.IsValidForDepth(5));
    }

    [Test]
    public void GetValidTemplates_FiltersByRequiredTags_ReturnsOnlyMatching()
    {
        // Arrange
        var context = new TemplateSelectionContext
        {
            Biome = "dungeon",
            Depth = 1,
            RequiredTags = ["corridor"]
        };

        // Act
        var results = _service.GetValidTemplates(context);

        // Assert
        results.Should().OnlyContain(t => t.HasAllTags(new[] { "corridor" }));
    }

    [Test]
    public void GetValidTemplates_FiltersByExcludedTags_ExcludesMatching()
    {
        // Arrange
        var context = new TemplateSelectionContext
        {
            Biome = "dungeon",
            Depth = 1,
            ExcludedTags = ["treasure"]
        };

        // Act
        var results = _service.GetValidTemplates(context);

        // Assert
        results.Should().OnlyContain(t => t.HasNoTags(new[] { "treasure" }));
    }

    [Test]
    public void GetFallbackTemplate_ReturnsValidTemplate()
    {
        // Act
        var result = _service.GetFallbackTemplate("volcanic");

        // Assert
        result.Should().NotBeNull();
        result.TemplateId.Should().StartWith("fallback-");
        result.IsValidForBiome("volcanic").Should().BeTrue();
        result.Tags.Should().Contain("fallback");
    }

    [Test]
    public void SelectTemplate_WithSeed_ReturnsDeterministicResult()
    {
        // Arrange
        var context = new TemplateSelectionContext { Biome = "dungeon", Depth = 1, Seed = 12345 };

        // Act
        var result1 = _service.SelectTemplate(context);
        var result2 = _service.SelectTemplate(context);

        // Assert
        result1!.TemplateId.Should().Be(result2!.TemplateId);
    }

    private static RoomTemplateConfiguration CreateTestConfiguration()
    {
        var config = new RoomTemplateConfiguration();

        config.Templates["dungeon-corridor"] = new RoomTemplate(
            templateId: "dungeon-corridor",
            namePattern: "Dark Corridor",
            descriptionPattern: "A narrow passage.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Standard,
            slots: [],
            weight: 40,
            minDepth: 0,
            maxDepth: null,
            tags: ["corridor", "passage"]);

        config.Templates["dungeon-vault"] = new RoomTemplate(
            templateId: "dungeon-vault",
            namePattern: "Treasure Vault",
            descriptionPattern: "A secure vault.",
            validBiomes: ["dungeon"],
            roomType: RoomType.Treasure,
            slots: [],
            weight: 10,
            minDepth: 2,
            maxDepth: null,
            tags: ["treasure", "vault"]);

        config.Templates["cave-cavern"] = new RoomTemplate(
            templateId: "cave-cavern",
            namePattern: "Crystal Cavern",
            descriptionPattern: "A glittering cave.",
            validBiomes: ["cave"],
            roomType: RoomType.Standard,
            slots: [],
            weight: 35,
            minDepth: 1,
            maxDepth: 7,
            tags: ["natural", "crystal"]);

        return config;
    }
}
