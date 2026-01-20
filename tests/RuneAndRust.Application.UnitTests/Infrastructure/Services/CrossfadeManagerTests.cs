using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CrossfadeManager"/>.
/// </summary>
[TestFixture]
public class CrossfadeManagerTests
{
    private Mock<IMusicService> _musicServiceMock = null!;
    private Mock<IMusicThemeConfig> _themeConfigMock = null!;
    private CrossfadeManager _manager = null!;

    [SetUp]
    public void SetUp()
    {
        _musicServiceMock = new Mock<IMusicService>();
        _musicServiceMock.Setup(m => m.GetVolume()).Returns(0.8f);

        _themeConfigMock = new Mock<IMusicThemeConfig>();
        _themeConfigMock.Setup(c => c.GetTrackForTheme(It.IsAny<MusicTheme>()))
            .Returns("audio/music/test.ogg");

        _manager = new CrossfadeManager(
            _musicServiceMock.Object,
            _themeConfigMock.Object,
            NullLogger<CrossfadeManager>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _manager.Dispose();
    }

    /// <summary>
    /// Verifies CrossfadeTo sets IsTransitioning.
    /// </summary>
    [Test]
    public void CrossfadeTo_SetsIsTransitioning()
    {
        // Act
        _manager.CrossfadeTo("audio/music/test.ogg", 0.1f);

        // Assert
        _manager.IsTransitioning.Should().BeTrue();
    }

    /// <summary>
    /// Verifies Cancel stops active transition.
    /// </summary>
    [Test]
    public void Cancel_StopsActiveTransition()
    {
        // Arrange
        _manager.CrossfadeTo("audio/music/test.ogg", 10.0f); // Long duration
        _manager.IsTransitioning.Should().BeTrue();

        // Act
        _manager.Cancel();

        // Assert
        _manager.IsTransitioning.Should().BeFalse();
    }

    /// <summary>
    /// Verifies FadeOut sets IsTransitioning.
    /// </summary>
    [Test]
    public void FadeOut_SetsIsTransitioning()
    {
        // Act
        _manager.FadeOut(0.1f);

        // Assert
        _manager.IsTransitioning.Should().BeTrue();
    }

    /// <summary>
    /// Verifies FadeIn sets IsTransitioning.
    /// </summary>
    [Test]
    public void FadeIn_SetsIsTransitioning()
    {
        // Act
        _manager.FadeIn(0.1f);

        // Assert
        _manager.IsTransitioning.Should().BeTrue();
    }

    /// <summary>
    /// Verifies DuckAndPlay reduces volume.
    /// </summary>
    [Test]
    public void DuckAndPlay_ReducesVolume()
    {
        // Arrange
        var volumeSet = 0f;
        _musicServiceMock.Setup(m => m.SetVolume(It.IsAny<float>()))
            .Callback<float>(v => volumeSet = v);

        // Act
        _manager.DuckAndPlay("audio/music/stinger.ogg", 0.2f);

        // Assert - Volume should be 0.8 * 0.2 = 0.16
        volumeSet.Should().BeApproximately(0.16f, 0.01f);
    }

    /// <summary>
    /// Verifies DuckAndPlay calls PlayStinger.
    /// </summary>
    [Test]
    public void DuckAndPlay_CallsPlayStinger()
    {
        // Act
        _manager.DuckAndPlay("audio/music/stinger.ogg", 0.2f);

        // Assert
        _musicServiceMock.Verify(m => m.PlayStinger("audio/music/stinger.ogg", It.IsAny<Action?>()), Times.Once);
    }

    /// <summary>
    /// Verifies CrossfadeToTheme gets track from config.
    /// </summary>
    [Test]
    public void CrossfadeToTheme_GetsTrackFromConfig()
    {
        // Act
        _manager.CrossfadeToTheme(MusicTheme.Combat, 0.1f);

        // Assert
        _themeConfigMock.Verify(c => c.GetTrackForTheme(MusicTheme.Combat), Times.Once);
    }

    /// <summary>
    /// Verifies CrossfadeToTheme checks for intro track.
    /// </summary>
    [Test]
    public void CrossfadeToTheme_ChecksForIntroTrack()
    {
        // Act
        _manager.CrossfadeToTheme(MusicTheme.BossCombat, 0.1f);

        // Assert
        _themeConfigMock.Verify(c => c.GetIntroTrack(MusicTheme.BossCombat), Times.Once);
    }

    /// <summary>
    /// Verifies IsTransitioning is false by default.
    /// </summary>
    [Test]
    public void IsTransitioning_ByDefault_IsFalse()
    {
        // Assert
        _manager.IsTransitioning.Should().BeFalse();
    }
}
