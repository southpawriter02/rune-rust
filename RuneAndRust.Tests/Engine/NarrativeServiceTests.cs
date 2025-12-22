using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for NarrativeService (v0.3.4c).
/// Validates prologue text generation and Domain 4 compliance.
/// </summary>
public class NarrativeServiceTests
{
    private readonly NarrativeService _service;
    private readonly Mock<ILogger<NarrativeService>> _loggerMock;

    public NarrativeServiceTests()
    {
        _loggerMock = new Mock<ILogger<NarrativeService>>();
        _service = new NarrativeService(_loggerMock.Object);
    }

    #region GetPrologueText Tests

    [Fact]
    public void GetPrologueText_Scavenger_ContainsGreatLooms()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = BackgroundType.Scavenger,
            Archetype = ArchetypeType.Warrior
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains("Great Looms", result);
    }

    [Fact]
    public void GetPrologueText_Scholar_ContainsForbiddenSlates()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = BackgroundType.Scholar,
            Archetype = ArchetypeType.Adept
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains("forbidden slates", result);
    }

    [Theory]
    [InlineData(BackgroundType.Scavenger)]
    [InlineData(BackgroundType.Exile)]
    [InlineData(BackgroundType.Scholar)]
    [InlineData(BackgroundType.Soldier)]
    [InlineData(BackgroundType.Noble)]
    [InlineData(BackgroundType.Cultist)]
    public void GetPrologueText_AllBackgrounds_ContainsCharacterName(BackgroundType background)
    {
        // Arrange
        var characterName = "Ragnar";
        var character = new Character
        {
            Name = characterName,
            Background = background,
            Archetype = ArchetypeType.Warrior
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains(characterName, result);
    }

    [Theory]
    [InlineData(BackgroundType.Scavenger, ArchetypeType.Warrior)]
    [InlineData(BackgroundType.Exile, ArchetypeType.Skirmisher)]
    [InlineData(BackgroundType.Scholar, ArchetypeType.Adept)]
    [InlineData(BackgroundType.Soldier, ArchetypeType.Mystic)]
    [InlineData(BackgroundType.Noble, ArchetypeType.Warrior)]
    [InlineData(BackgroundType.Cultist, ArchetypeType.Mystic)]
    public void GetPrologueText_AllBackgrounds_ContainsArchetype(BackgroundType background, ArchetypeType archetype)
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = background,
            Archetype = archetype
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains(archetype.ToString(), result);
    }

    [Theory]
    [InlineData(BackgroundType.Scavenger)]
    [InlineData(BackgroundType.Exile)]
    [InlineData(BackgroundType.Scholar)]
    [InlineData(BackgroundType.Soldier)]
    [InlineData(BackgroundType.Noble)]
    [InlineData(BackgroundType.Cultist)]
    public void GetPrologueText_NoPrecisionMeasurements(BackgroundType background)
    {
        // Arrange - Domain 4 compliance check
        var character = new Character
        {
            Name = "TestChar",
            Background = background,
            Archetype = ArchetypeType.Warrior
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert - No precision measurements allowed (Domain 4)
        Assert.DoesNotContain("meter", result.ToLower());
        Assert.DoesNotContain("second", result.ToLower());
        Assert.DoesNotContain("percent", result.ToLower());
        Assert.DoesNotContain("%", result);
        Assert.DoesNotContain("°", result); // No temperature symbols
    }

    [Fact]
    public void GetPrologueText_Exile_ContainsWastelands()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = BackgroundType.Exile,
            Archetype = ArchetypeType.Skirmisher
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains("wastelands", result);
    }

    [Fact]
    public void GetPrologueText_Soldier_ContainsWallsFell()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = BackgroundType.Soldier,
            Archetype = ArchetypeType.Warrior
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains("walls fell", result);
    }

    [Fact]
    public void GetPrologueText_Noble_ContainsBlackGlass()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = BackgroundType.Noble,
            Archetype = ArchetypeType.Adept
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains("black glass", result);
    }

    [Fact]
    public void GetPrologueText_Cultist_ContainsCorrodedAltar()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestChar",
            Background = BackgroundType.Cultist,
            Archetype = ArchetypeType.Mystic
        };

        // Act
        var result = _service.GetPrologueText(character);

        // Assert
        Assert.Contains("Corroded Altar", result);
    }

    #endregion

    #region GetBackgroundDisplayName Tests

    [Theory]
    [InlineData(BackgroundType.Scavenger, "Scavenger")]
    [InlineData(BackgroundType.Exile, "Exile")]
    [InlineData(BackgroundType.Scholar, "Scholar")]
    [InlineData(BackgroundType.Soldier, "Soldier")]
    [InlineData(BackgroundType.Noble, "Noble")]
    [InlineData(BackgroundType.Cultist, "Cultist")]
    public void GetBackgroundDisplayName_ReturnsCorrectName(BackgroundType background, string expectedName)
    {
        // Act
        var result = _service.GetBackgroundDisplayName(background);

        // Assert
        Assert.Equal(expectedName, result);
    }

    #endregion

    #region GetBackgroundDescription Tests

    [Theory]
    [InlineData(BackgroundType.Scavenger)]
    [InlineData(BackgroundType.Exile)]
    [InlineData(BackgroundType.Scholar)]
    [InlineData(BackgroundType.Soldier)]
    [InlineData(BackgroundType.Noble)]
    [InlineData(BackgroundType.Cultist)]
    public void GetBackgroundDescription_ReturnsNonEmpty(BackgroundType background)
    {
        // Act
        var result = _service.GetBackgroundDescription(background);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void GetBackgroundDescription_Scavenger_ContainsSalvage()
    {
        // Act
        var result = _service.GetBackgroundDescription(BackgroundType.Scavenger);

        // Assert
        Assert.Contains("salvage", result.ToLower());
    }

    #endregion
}
