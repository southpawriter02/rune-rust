using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Settings;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the AudioService class (v0.3.19a).
/// Validates volume filtering, provider invocation, and system cue lookup.
/// </summary>
public class AudioServiceTests
{
    private readonly Mock<IAudioProvider> _mockProvider;
    private readonly Mock<ILogger<AudioService>> _mockLogger;
    private readonly AudioService _sut;

    public AudioServiceTests()
    {
        _mockProvider = new Mock<IAudioProvider>();
        _mockLogger = new Mock<ILogger<AudioService>>();

        // Default provider is supported
        _mockProvider.Setup(p => p.IsSupported).Returns(true);

        _sut = new AudioService(
            _mockProvider.Object,
            _mockLogger.Object);

        // Reset GameSettings to default for each test
        GameSettings.MasterVolume = 100;
    }

    #region IsMuted Tests

    [Fact]
    public void IsMuted_VolumeZero_ReturnsTrue()
    {
        // Arrange
        GameSettings.MasterVolume = 0;

        // Act & Assert
        _sut.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void IsMuted_VolumeNonZero_ReturnsFalse()
    {
        // Arrange
        GameSettings.MasterVolume = 50;

        // Act & Assert
        _sut.IsMuted.Should().BeFalse();
    }

    [Fact]
    public void IsMuted_FullVolume_ReturnsFalse()
    {
        // Arrange
        GameSettings.MasterVolume = 100;

        // Act & Assert
        _sut.IsMuted.Should().BeFalse();
    }

    #endregion

    #region PlayAsync Tests

    [Fact]
    public async Task PlayAsync_WhenMuted_DoesNotCallProvider()
    {
        // Arrange
        GameSettings.MasterVolume = 0;
        var cue = SoundCue.UiClick;

        // Act
        await _sut.PlayAsync(cue);

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(It.IsAny<SoundCue>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsync_WhenNotMuted_CallsProvider()
    {
        // Arrange
        GameSettings.MasterVolume = 100;
        var cue = SoundCue.UiClick;

        // Act
        await _sut.PlayAsync(cue);

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(cue, 100),
            Times.Once);
    }

    [Fact]
    public async Task PlayAsync_PassesCorrectVolume()
    {
        // Arrange
        GameSettings.MasterVolume = 75;
        var cue = SoundCue.UiClick;

        // Act
        await _sut.PlayAsync(cue);

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(cue, 75),
            Times.Once);
    }

    [Fact]
    public async Task PlayAsync_ProviderNotSupported_DoesNotCallProvider()
    {
        // Arrange
        _mockProvider.Setup(p => p.IsSupported).Returns(false);
        GameSettings.MasterVolume = 100;
        var cue = SoundCue.UiClick;

        // Act
        await _sut.PlayAsync(cue);

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(It.IsAny<SoundCue>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsync_ProviderThrows_DoesNotRethrow()
    {
        // Arrange
        GameSettings.MasterVolume = 100;
        var cue = SoundCue.UiClick;
        _mockProvider.Setup(p => p.PlayAsync(It.IsAny<SoundCue>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Audio hardware failure"));

        // Act & Assert
        var act = () => _sut.PlayAsync(cue);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PlayAsync_CombatCue_CallsProviderWithCorrectFrequency()
    {
        // Arrange
        GameSettings.MasterVolume = 100;
        var cue = SoundCue.CombatHitHeavy;

        // Act
        await _sut.PlayAsync(cue);

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(
                It.Is<SoundCue>(c => c.Frequency == 300 && c.DurationMs == 250),
                100),
            Times.Once);
    }

    #endregion

    #region PlaySystemCueAsync Tests

    [Fact]
    public async Task PlaySystemCueAsync_ValidCueId_CallsProvider()
    {
        // Arrange
        GameSettings.MasterVolume = 100;

        // Act
        await _sut.PlaySystemCueAsync("ui_click");

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(
                It.Is<SoundCue>(c => c.Id == "ui_click"),
                100),
            Times.Once);
    }

    [Fact]
    public async Task PlaySystemCueAsync_InvalidCueId_DoesNotCallProvider()
    {
        // Arrange
        GameSettings.MasterVolume = 100;

        // Act
        await _sut.PlaySystemCueAsync("nonexistent_cue");

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(It.IsAny<SoundCue>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaySystemCueAsync_CaseInsensitive_CallsProvider()
    {
        // Arrange
        GameSettings.MasterVolume = 100;

        // Act
        await _sut.PlaySystemCueAsync("UI_CLICK");

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(
                It.Is<SoundCue>(c => c.Id == "ui_click"),
                100),
            Times.Once);
    }

    [Fact]
    public async Task PlaySystemCueAsync_AllKnownCues_AreResolvable()
    {
        // Arrange
        GameSettings.MasterVolume = 100;
        var knownCueIds = new[]
        {
            "ui_click",
            "ui_select",
            "ui_error",
            "combat_hit_light",
            "combat_hit_heavy",
            "combat_critical"
        };

        // Act
        foreach (var cueId in knownCueIds)
        {
            await _sut.PlaySystemCueAsync(cueId);
        }

        // Assert
        _mockProvider.Verify(
            p => p.PlayAsync(It.IsAny<SoundCue>(), It.IsAny<int>()),
            Times.Exactly(knownCueIds.Length));
    }

    #endregion

    #region MasterVolume Property Tests

    [Fact]
    public void MasterVolume_ReturnsGameSettingsValue()
    {
        // Arrange
        GameSettings.MasterVolume = 42;

        // Act & Assert
        _sut.MasterVolume.Should().Be(42);
    }

    [Fact]
    public void MasterVolume_ReflectsChangesToGameSettings()
    {
        // Arrange
        GameSettings.MasterVolume = 100;

        // Act
        GameSettings.MasterVolume = 25;

        // Assert
        _sut.MasterVolume.Should().Be(25);
    }

    #endregion
}
