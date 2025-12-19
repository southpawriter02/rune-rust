using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the DescriptorEngine service.
/// Validates three-tier composition model for procedural descriptions.
/// </summary>
public class DescriptorEngineTests
{
    private readonly Mock<ILogger<DescriptorEngine>> _mockLogger;
    private readonly DescriptorEngine _sut;

    public DescriptorEngineTests()
    {
        _mockLogger = new Mock<ILogger<DescriptorEngine>>();
        _sut = new DescriptorEngine(_mockLogger.Object);
    }

    #region ComposeDescription Tests

    [Fact]
    public void ComposeDescription_WithOnlyBase_ReturnsBase()
    {
        // Arrange
        var baseTemplate = "A rusted chest.";

        // Act
        var result = _sut.ComposeDescription(baseTemplate, null, null);

        // Assert
        result.Should().Be("A rusted chest.");
    }

    [Fact]
    public void ComposeDescription_WithBaseAndModifier_CombinesBoth()
    {
        // Arrange
        var baseTemplate = "A rusted chest.";
        var modifier = "Dust motes drift in pale light.";

        // Act
        var result = _sut.ComposeDescription(baseTemplate, modifier, null);

        // Assert
        result.Should().Be("A rusted chest. Dust motes drift in pale light.");
    }

    [Fact]
    public void ComposeDescription_WithAllThreeTiers_CombinesAll()
    {
        // Arrange
        var baseTemplate = "A rusted chest.";
        var modifier = "Rust stains mark the floor.";
        var detail = "Silence pervades.";

        // Act
        var result = _sut.ComposeDescription(baseTemplate, modifier, detail);

        // Assert
        result.Should().Be("A rusted chest. Rust stains mark the floor. Silence pervades.");
    }

    [Fact]
    public void ComposeDescription_TrimsWhitespace()
    {
        // Arrange
        var baseTemplate = "  A rusted chest.  ";
        var modifier = "  Dust motes.  ";

        // Act
        var result = _sut.ComposeDescription(baseTemplate, modifier, null);

        // Assert
        result.Should().Be("A rusted chest. Dust motes.");
    }

    [Fact]
    public void ComposeDescription_WithEmptyModifier_SkipsModifier()
    {
        // Arrange
        var baseTemplate = "A rusted chest.";
        var detail = "Silence.";

        // Act
        var result = _sut.ComposeDescription(baseTemplate, "", detail);

        // Assert
        result.Should().Be("A rusted chest. Silence.");
    }

    [Fact]
    public void ComposeDescription_WithWhitespaceModifier_SkipsModifier()
    {
        // Arrange
        var baseTemplate = "A rusted chest.";
        var detail = "Silence.";

        // Act
        var result = _sut.ComposeDescription(baseTemplate, "   ", detail);

        // Assert
        result.Should().Be("A rusted chest. Silence.");
    }

    #endregion

    #region GetModifierForBiome Tests

    [Theory]
    [InlineData(BiomeType.Ruin)]
    [InlineData(BiomeType.Industrial)]
    [InlineData(BiomeType.Organic)]
    [InlineData(BiomeType.Void)]
    public void GetModifierForBiome_ReturnsNonEmptyString(BiomeType biome)
    {
        // Act
        var result = _sut.GetModifierForBiome(biome);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetModifierForBiome_Ruin_ContainsThematicContent()
    {
        // Arrange & Act - Run multiple times to sample different modifiers
        var modifiers = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            modifiers.Add(_sut.GetModifierForBiome(BiomeType.Ruin));
        }

        // Assert - Should have some variety (not all same)
        modifiers.Count.Should().BeGreaterThan(1, "should return varied modifiers");
    }

    [Fact]
    public void GetModifierForBiome_Industrial_ContainsThematicContent()
    {
        // Arrange & Act
        var modifiers = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            modifiers.Add(_sut.GetModifierForBiome(BiomeType.Industrial));
        }

        // Assert
        modifiers.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetModifierForBiome_Organic_ContainsThematicContent()
    {
        // Arrange & Act
        var modifiers = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            modifiers.Add(_sut.GetModifierForBiome(BiomeType.Organic));
        }

        // Assert
        modifiers.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetModifierForBiome_Void_ContainsThematicContent()
    {
        // Arrange & Act
        var modifiers = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            modifiers.Add(_sut.GetModifierForBiome(BiomeType.Void));
        }

        // Assert
        modifiers.Count.Should().BeGreaterThan(1);
    }

    #endregion

    #region GetDetailForDangerLevel Tests

    [Theory]
    [InlineData(DangerLevel.Safe)]
    [InlineData(DangerLevel.Unstable)]
    [InlineData(DangerLevel.Hostile)]
    [InlineData(DangerLevel.Lethal)]
    public void GetDetailForDangerLevel_ReturnsNonEmptyString(DangerLevel danger)
    {
        // Act
        var result = _sut.GetDetailForDangerLevel(danger);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetDetailForDangerLevel_Safe_ContainsThematicContent()
    {
        // Arrange & Act
        var details = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            details.Add(_sut.GetDetailForDangerLevel(DangerLevel.Safe));
        }

        // Assert
        details.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetDetailForDangerLevel_Lethal_ContainsThematicContent()
    {
        // Arrange & Act
        var details = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            details.Add(_sut.GetDetailForDangerLevel(DangerLevel.Lethal));
        }

        // Assert
        details.Count.Should().BeGreaterThan(1);
    }

    #endregion

    #region GenerateRoomDescription Tests

    [Fact]
    public void GenerateRoomDescription_CombinesAllTiers()
    {
        // Arrange
        var baseDescription = "A cold chamber.";

        // Act
        var result = _sut.GenerateRoomDescription(baseDescription, BiomeType.Ruin, DangerLevel.Safe);

        // Assert
        result.Should().StartWith("A cold chamber.");
        result.Length.Should().BeGreaterThan(baseDescription.Length, "should add modifier and detail");
    }

    [Fact]
    public void GenerateRoomDescription_IncludesBaseDescription()
    {
        // Arrange
        var baseDescription = "An ancient hall filled with debris.";

        // Act
        var result = _sut.GenerateRoomDescription(baseDescription, BiomeType.Industrial, DangerLevel.Hostile);

        // Assert
        result.Should().Contain("An ancient hall filled with debris");
    }

    [Fact]
    public void GenerateRoomDescription_ProducesVariedResults()
    {
        // Arrange
        var baseDescription = "A room.";
        var descriptions = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            descriptions.Add(_sut.GenerateRoomDescription(baseDescription, BiomeType.Void, DangerLevel.Unstable));
        }

        // Assert - Should produce varied combinations
        descriptions.Count.Should().BeGreaterThan(5, "should produce varied descriptions");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void ComposeDescription_LogsAtDebugLevel()
    {
        // Act
        _sut.ComposeDescription("Base", "Modifier", "Detail");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void GenerateRoomDescription_LogsAtInformationLevel()
    {
        // Act
        _sut.GenerateRoomDescription("A room.", BiomeType.Ruin, DangerLevel.Safe);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
