using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="MusicService"/>.
/// </summary>
[TestFixture]
public class MusicServiceTests
{
    private Mock<IAudioService> _audioServiceMock = null!;
    private MusicService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _audioServiceMock = new Mock<IAudioService>();
        _audioServiceMock.Setup(a => a.Play(It.IsAny<string>(), It.IsAny<AudioChannel>(), It.IsAny<float>(), It.IsAny<bool>()))
            .Returns(Guid.NewGuid());
        _audioServiceMock.Setup(a => a.GetChannelVolume(AudioChannel.Music)).Returns(0.6f);

        _service = new MusicService(_audioServiceMock.Object, NullLogger<MusicService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    /// <summary>
    /// Verifies PlayTrack starts playback via AudioService.
    /// </summary>
    [Test]
    public void PlayTrack_CallsAudioServicePlay()
    {
        // Act
        _service.PlayTrack("music/test.ogg", loop: true);

        // Assert
        _audioServiceMock.Verify(a => a.Play("music/test.ogg", AudioChannel.Music, 1.0f, true), Times.Once);
        _service.CurrentTrack.Should().Be("music/test.ogg");
        _service.IsPlaying.Should().BeTrue();
    }

    /// <summary>
    /// Verifies Stop clears playback state.
    /// </summary>
    [Test]
    public void Stop_ClearsPlaybackState()
    {
        // Arrange
        _service.PlayTrack("music/test.ogg");

        // Act
        _service.Stop();

        // Assert
        _service.CurrentTrack.Should().BeNull();
        _service.IsPlaying.Should().BeFalse();
    }

    /// <summary>
    /// Verifies Pause sets IsPaused to true.
    /// </summary>
    [Test]
    public void Pause_SetsIsPausedTrue()
    {
        // Arrange
        _service.PlayTrack("music/test.ogg");

        // Act
        _service.Pause();

        // Assert
        _service.IsPaused.Should().BeTrue();
        _service.IsPlaying.Should().BeFalse();
    }

    /// <summary>
    /// Verifies Resume sets IsPaused to false.
    /// </summary>
    [Test]
    public void Resume_SetsIsPausedFalse()
    {
        // Arrange
        _service.PlayTrack("music/test.ogg");
        _service.Pause();

        // Act
        _service.Resume();

        // Assert
        _service.IsPaused.Should().BeFalse();
        _service.IsPlaying.Should().BeTrue();
    }

    /// <summary>
    /// Verifies SetTheme changes CurrentTheme.
    /// </summary>
    [Test]
    public void SetTheme_ChangesCurrentTheme()
    {
        // Arrange
        _service.CurrentTheme.Should().Be(MusicTheme.None);

        // Act
        _service.SetTheme(MusicTheme.Combat);

        // Assert
        _service.CurrentTheme.Should().Be(MusicTheme.Combat);
    }

    /// <summary>
    /// Verifies SetTheme is idempotent for same theme.
    /// </summary>
    [Test]
    public void SetTheme_SameTheme_IsIdempotent()
    {
        // Arrange
        var eventFired = 0;
        _service.OnThemeChanged += _ => eventFired++;

        // Act
        _service.SetTheme(MusicTheme.Exploration);
        _service.SetTheme(MusicTheme.Exploration); // Same theme

        // Assert
        eventFired.Should().Be(1, "same theme should not fire event again");
    }

    /// <summary>
    /// Verifies GetVolume returns AudioService channel volume.
    /// </summary>
    [Test]
    public void GetVolume_ReturnsAudioServiceChannelVolume()
    {
        // Act
        var volume = _service.GetVolume();

        // Assert
        volume.Should().Be(0.6f);
        _audioServiceMock.Verify(a => a.GetChannelVolume(AudioChannel.Music), Times.Once);
    }

    /// <summary>
    /// Verifies SetVolume calls AudioService with clamped value.
    /// </summary>
    [Test]
    public void SetVolume_CallsAudioServiceWithClampedValue()
    {
        // Act
        _service.SetVolume(1.5f); // Over max

        // Assert
        _audioServiceMock.Verify(a => a.SetChannelVolume(AudioChannel.Music, 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies OnTrackChanged fires when playing track.
    /// </summary>
    [Test]
    public void PlayTrack_FiresOnTrackChangedEvent()
    {
        // Arrange
        string? firedTrack = null;
        _service.OnTrackChanged += track => firedTrack = track;

        // Act
        _service.PlayTrack("music/event_test.ogg");

        // Assert
        firedTrack.Should().Be("music/event_test.ogg");
    }

    /// <summary>
    /// Verifies OnThemeChanged fires when theme changes.
    /// </summary>
    [Test]
    public void SetTheme_FiresOnThemeChangedEvent()
    {
        // Arrange
        MusicTheme? firedTheme = null;
        _service.OnThemeChanged += theme => firedTheme = theme;

        // Act
        _service.SetTheme(MusicTheme.BossCombat);

        // Assert
        firedTheme.Should().Be(MusicTheme.BossCombat);
    }
}
