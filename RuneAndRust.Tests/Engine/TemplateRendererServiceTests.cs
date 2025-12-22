using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for TemplateRendererService (v0.3.8).
/// Tests variable substitution ({Adjective}, {Detail}) and atmospheric detail generation.
/// </summary>
public class TemplateRendererServiceTests
{
    private readonly IDiceService _mockDiceService;
    private readonly ILogger<TemplateRendererService> _mockLogger;
    private readonly TemplateRendererService _service;

    public TemplateRendererServiceTests()
    {
        _mockDiceService = Substitute.For<IDiceService>();
        _mockLogger = Substitute.For<ILogger<TemplateRendererService>>();
        _service = new TemplateRendererService(_mockDiceService, _mockLogger);
    }

    [Fact]
    public void RenderRoomName_SubstitutesAdjectiveToken_ReturnsCorrectName()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            NameTemplates = new List<string> { "The {Adjective} Chamber" },
            Adjectives = new List<string> { "Dark", "Forgotten", "Ancient" }
        };

        _mockDiceService.RollSingle(1, Arg.Any<string>()).Returns(1); // Select first name template
        _mockDiceService.RollSingle(3, Arg.Any<string>()).Returns(2); // Select "Forgotten"

        // Act
        var result = _service.RenderRoomName(template);

        // Assert
        Assert.Equal("The Forgotten Chamber", result);
    }

    [Fact]
    public void RenderRoomName_MultipleNameTemplates_SelectsRandomTemplate()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            NameTemplates = new List<string> { "The {Adjective} Hall", "Chamber of {Adjective} Echoes" },
            Adjectives = new List<string> { "Twisted" }
        };

        _mockDiceService.RollSingle(2, Arg.Any<string>()).Returns(2); // Select second template
        _mockDiceService.RollSingle(1, Arg.Any<string>()).Returns(1); // Select "Twisted"

        // Act
        var result = _service.RenderRoomName(template);

        // Assert
        Assert.Equal("Chamber of Twisted Echoes", result);
    }

    [Fact]
    public void RenderRoomName_EmptyNameTemplates_ReturnsUnnamedRoom()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            NameTemplates = new List<string>(),
            Adjectives = new List<string> { "Dark" }
        };

        // Act
        var result = _service.RenderRoomName(template);

        // Assert
        Assert.Equal("Unnamed Room", result);
    }

    [Fact]
    public void RenderRoomDescription_SubstitutesAdjectiveAndDetailTokens_ReturnsCorrectDescription()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            DescriptionTemplates = new List<string> { "A {Adjective} room dominates this space. {Detail}." },
            Adjectives = new List<string> { "Pulsing" },
            Details = new List<string> { "Runes flicker along the walls" }
        };

        var biome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            DescriptorCategories = new BiomeDescriptorCategories
            {
                Sounds = new List<string>(),
                Smells = new List<string>()
            }
        };

        _mockDiceService.RollSingle(1, Arg.Any<string>()).Returns(1); // Select first of each
        _mockDiceService.RollSingle(100, "Atmospheric detail chance").Returns(50); // No atmospheric (> 30)

        // Act
        var result = _service.RenderRoomDescription(template, biome);

        // Assert
        Assert.Equal("A pulsing room dominates this space. Runes flicker along the walls.", result);
    }

    [Fact]
    public void RenderRoomDescription_WithAtmosphericDetail_AppendsSound()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            DescriptionTemplates = new List<string> { "A dark chamber." },
            Adjectives = new List<string> { "dark" },
            Details = new List<string> { "Nothing moves" }
        };

        var biome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            DescriptorCategories = new BiomeDescriptorCategories
            {
                Sounds = new List<string> { "distant machinery grinding" },
                Smells = new List<string>()
            }
        };

        _mockDiceService.RollSingle(1, Arg.Any<string>()).Returns(1);
        _mockDiceService.RollSingle(100, "Atmospheric detail chance").Returns(25); // Trigger atmospheric (≤ 30)

        // Act
        var result = _service.RenderRoomDescription(template, biome);

        // Assert
        Assert.Contains("You hear distant machinery grinding", result);
    }

    [Fact]
    public void RenderRoomDescription_WithAtmosphericDetail_AppendsSmell()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            DescriptionTemplates = new List<string> { "A dark chamber." },
            Adjectives = new List<string> { "dark" },
            Details = new List<string> { "Nothing moves" }
        };

        var biome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            DescriptorCategories = new BiomeDescriptorCategories
            {
                Sounds = new List<string>(),
                Smells = new List<string> { "ozone and rust" }
            }
        };

        _mockDiceService.RollSingle(1, Arg.Any<string>()).Returns(1);
        _mockDiceService.RollSingle(100, "Atmospheric detail chance").Returns(15); // Trigger atmospheric

        // Act
        var result = _service.RenderRoomDescription(template, biome);

        // Assert
        Assert.Contains("The air smells of ozone and rust", result);
    }

    [Fact]
    public void RenderRoomDescription_EmptyDescriptionTemplates_ReturnsDefaultDescription()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            DescriptionTemplates = new List<string>(),
            Adjectives = new List<string> { "dark" },
            Details = new List<string>()
        };

        var biome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            DescriptorCategories = new BiomeDescriptorCategories()
        };

        // Act
        var result = _service.RenderRoomDescription(template, biome);

        // Assert
        Assert.Equal("This area is shrouded in mystery.", result);
    }

    [Fact]
    public void RenderRoomDescription_AdjectiveConvertedToLowercase_ForMidSentenceUsage()
    {
        // Arrange
        var template = new RoomTemplate
        {
            TemplateId = "test_room",
            DescriptionTemplates = new List<string> { "The {Adjective} atmosphere presses in." },
            Adjectives = new List<string> { "Oppressive" }, // Capitalized
            Details = new List<string> { "" }
        };

        var biome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            DescriptorCategories = new BiomeDescriptorCategories()
        };

        _mockDiceService.RollSingle(1, Arg.Any<string>()).Returns(1);
        _mockDiceService.RollSingle(100, "Atmospheric detail chance").Returns(50);

        // Act
        var result = _service.RenderRoomDescription(template, biome);

        // Assert
        Assert.Equal("The oppressive atmosphere presses in.", result);
    }
}
