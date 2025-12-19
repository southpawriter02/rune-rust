using FluentAssertions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the DataCapture entity.
/// Validates capture creation, properties, defaults, and relationships.
/// </summary>
public class DataCaptureTests
{
    #region Identity Tests

    [Fact]
    public void DataCapture_NewCapture_HasUniqueId()
    {
        // Arrange & Act
        var capture1 = new DataCapture();
        var capture2 = new DataCapture();

        // Assert
        capture1.Id.Should().NotBeEmpty();
        capture2.Id.Should().NotBeEmpty();
        capture1.Id.Should().NotBe(capture2.Id, "each capture should have a unique Id");
    }

    #endregion

    #region Ownership Tests

    [Fact]
    public void DataCapture_CharacterId_DefaultsToEmptyGuid()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.CharacterId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void DataCapture_CharacterId_CanBeSet()
    {
        // Arrange
        var capture = new DataCapture();
        var characterId = Guid.NewGuid();

        // Act
        capture.CharacterId = characterId;

        // Assert
        capture.CharacterId.Should().Be(characterId);
    }

    #endregion

    #region Assignment Tests

    [Fact]
    public void DataCapture_CodexEntryId_DefaultsToNull()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.CodexEntryId.Should().BeNull();
    }

    [Fact]
    public void DataCapture_CodexEntryId_CanBeSet()
    {
        // Arrange
        var capture = new DataCapture();
        var entryId = Guid.NewGuid();

        // Act
        capture.CodexEntryId = entryId;

        // Assert
        capture.CodexEntryId.Should().Be(entryId);
    }

    [Fact]
    public void DataCapture_CodexEntryId_CanBeSetToNull()
    {
        // Arrange
        var capture = new DataCapture { CodexEntryId = Guid.NewGuid() };

        // Act
        capture.CodexEntryId = null;

        // Assert
        capture.CodexEntryId.Should().BeNull();
    }

    #endregion

    #region Classification Tests

    [Fact]
    public void DataCapture_Type_DefaultsToTextFragment()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.Type.Should().Be(CaptureType.TextFragment);
    }

    [Theory]
    [InlineData(CaptureType.TextFragment)]
    [InlineData(CaptureType.EchoRecording)]
    [InlineData(CaptureType.VisualRecord)]
    [InlineData(CaptureType.Specimen)]
    [InlineData(CaptureType.OralHistory)]
    [InlineData(CaptureType.RunicTrace)]
    public void DataCapture_Type_CanBeSetToAllTypes(CaptureType type)
    {
        // Arrange
        var capture = new DataCapture();

        // Act
        capture.Type = type;

        // Assert
        capture.Type.Should().Be(type);
    }

    #endregion

    #region Content Tests

    [Fact]
    public void DataCapture_FragmentContent_DefaultsToEmptyString()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.FragmentContent.Should().BeEmpty();
    }

    [Fact]
    public void DataCapture_FragmentContent_CanBeSet()
    {
        // Arrange
        var capture = new DataCapture();
        var content = "The servo-motor shows signs of organic fungal infiltration...";

        // Act
        capture.FragmentContent = content;

        // Assert
        capture.FragmentContent.Should().Be(content);
    }

    [Fact]
    public void DataCapture_Source_DefaultsToEmptyString()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.Source.Should().BeEmpty();
    }

    [Fact]
    public void DataCapture_Source_CanBeSet()
    {
        // Arrange
        var capture = new DataCapture();
        var source = "Found on Rusted Servitor corpse";

        // Act
        capture.Source = source;

        // Assert
        capture.Source.Should().Be(source);
    }

    #endregion

    #region Quality Tests

    [Fact]
    public void DataCapture_Quality_DefaultsToFifteen()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.Quality.Should().Be(15);
    }

    [Fact]
    public void DataCapture_Quality_CanBeSetToStandard()
    {
        // Arrange
        var capture = new DataCapture();

        // Act
        capture.Quality = 15;

        // Assert
        capture.Quality.Should().Be(15);
    }

    [Fact]
    public void DataCapture_Quality_CanBeSetToSpecialist()
    {
        // Arrange
        var capture = new DataCapture();

        // Act
        capture.Quality = 30;

        // Assert
        capture.Quality.Should().Be(30);
    }

    #endregion

    #region State Tests

    [Fact]
    public void DataCapture_IsAnalyzed_DefaultsToFalse()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.IsAnalyzed.Should().BeFalse();
    }

    [Fact]
    public void DataCapture_IsAnalyzed_CanBeSetToTrue()
    {
        // Arrange
        var capture = new DataCapture();

        // Act
        capture.IsAnalyzed = true;

        // Assert
        capture.IsAnalyzed.Should().BeTrue();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void DataCapture_DiscoveredAt_IsSetAutomatically()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var capture = new DataCapture();

        // Assert
        var after = DateTime.UtcNow;
        capture.DiscoveredAt.Should().BeOnOrAfter(before);
        capture.DiscoveredAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void DataCapture_CodexEntry_DefaultsToNull()
    {
        // Arrange & Act
        var capture = new DataCapture();

        // Assert
        capture.CodexEntry.Should().BeNull();
    }

    [Fact]
    public void DataCapture_CodexEntry_CanBeSet()
    {
        // Arrange
        var capture = new DataCapture();
        var entry = new CodexEntry { Title = "Rusted Servitor" };

        // Act
        capture.CodexEntry = entry;
        capture.CodexEntryId = entry.Id;

        // Assert
        capture.CodexEntry.Should().NotBeNull();
        capture.CodexEntry!.Title.Should().Be("Rusted Servitor");
        capture.CodexEntryId.Should().Be(entry.Id);
    }

    #endregion

    #region Full Capture Creation Tests

    [Fact]
    public void DataCapture_FullCreation_AllPropertiesSet()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var entryId = Guid.NewGuid();

        // Act
        var capture = new DataCapture
        {
            CharacterId = characterId,
            CodexEntryId = entryId,
            Type = CaptureType.Specimen,
            FragmentContent = "The servo-motor shows signs of organic fungal infiltration...",
            Source = "Found on Rusted Servitor corpse",
            Quality = 30,
            IsAnalyzed = true
        };

        // Assert
        capture.Id.Should().NotBeEmpty();
        capture.CharacterId.Should().Be(characterId);
        capture.CodexEntryId.Should().Be(entryId);
        capture.Type.Should().Be(CaptureType.Specimen);
        capture.FragmentContent.Should().Contain("servo-motor");
        capture.Source.Should().Contain("Servitor");
        capture.Quality.Should().Be(30);
        capture.IsAnalyzed.Should().BeTrue();
        capture.DiscoveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DataCapture_UnassignedCapture_HasNullCodexEntryId()
    {
        // Arrange & Act
        var capture = new DataCapture
        {
            CharacterId = Guid.NewGuid(),
            Type = CaptureType.OralHistory,
            FragmentContent = "A rumor heard in the tavern...",
            Source = "Overheard conversation"
        };

        // Assert
        capture.CodexEntryId.Should().BeNull();
        capture.CodexEntry.Should().BeNull();
    }

    #endregion
}
