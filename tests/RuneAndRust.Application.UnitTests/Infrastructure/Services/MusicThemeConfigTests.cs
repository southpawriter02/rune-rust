using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Enums;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="MusicThemeConfig"/>.
/// </summary>
[TestFixture]
public class MusicThemeConfigTests
{
    private MusicThemeConfig _config = null!;

    [SetUp]
    public void SetUp()
    {
        // Use default configuration
        _config = new MusicThemeConfig(NullLogger<MusicThemeConfig>.Instance);
    }

    /// <summary>
    /// Verifies GetTrackForTheme returns a track.
    /// </summary>
    [Test]
    public void GetTrackForTheme_WithValidTheme_ReturnsTrack()
    {
        // Act
        var track = _config.GetTrackForTheme(MusicTheme.MainMenu);

        // Assert
        track.Should().NotBeNull();
        track.Should().Be("audio/music/menu_theme.ogg");
    }

    /// <summary>
    /// Verifies GetTrackForTheme returns null for undefined theme.
    /// </summary>
    [Test]
    public void GetTrackForTheme_WithUndefinedTheme_ReturnsNull()
    {
        // Act
        var track = _config.GetTrackForTheme(MusicTheme.None);

        // Assert
        track.Should().BeNull();
    }

    /// <summary>
    /// Verifies sequential mode cycles through tracks.
    /// </summary>
    [Test]
    public void GetTrackForTheme_SequentialMode_CyclesTracks()
    {
        // Arrange - BossCombat is sequential with 2 tracks
        var track1 = _config.GetTrackForTheme(MusicTheme.BossCombat);
        var track2 = _config.GetTrackForTheme(MusicTheme.BossCombat);
        var track3 = _config.GetTrackForTheme(MusicTheme.BossCombat);

        // Assert
        track1.Should().Be("audio/music/boss_epic.ogg");
        track2.Should().Be("audio/music/boss_epic_phase2.ogg");
        track3.Should().Be("audio/music/boss_epic.ogg"); // Cycles back
    }

    /// <summary>
    /// Verifies GetIntroTrack returns intro track.
    /// </summary>
    [Test]
    public void GetIntroTrack_WithIntroTrack_ReturnsTrack()
    {
        // Act
        var intro = _config.GetIntroTrack(MusicTheme.Combat);

        // Assert
        intro.Should().NotBeNull();
        intro.Should().Be("audio/music/combat_intro.ogg");
    }

    /// <summary>
    /// Verifies GetStingerTrack returns stinger track.
    /// </summary>
    [Test]
    public void GetStingerTrack_WithValidStinger_ReturnsTrack()
    {
        // Act
        var track = _config.GetStingerTrack("victory");

        // Assert
        track.Should().NotBeNull();
        track.Should().Be("audio/music/victory_fanfare.ogg");
    }

    /// <summary>
    /// Verifies GetStingerTrack is case-insensitive.
    /// </summary>
    [Test]
    public void GetStingerTrack_IsCaseInsensitive()
    {
        // Act
        var track = _config.GetStingerTrack("VICTORY");

        // Assert
        track.Should().Be("audio/music/victory_fanfare.ogg");
    }

    /// <summary>
    /// Verifies GetAvailableStingers returns all stingers.
    /// </summary>
    [Test]
    public void GetAvailableStingers_ReturnsAllStingers()
    {
        // Act
        var stingers = _config.GetAvailableStingers();

        // Assert
        stingers.Should().HaveCount(4);
        stingers.Should().Contain("victory");
        stingers.Should().Contain("defeat");
        stingers.Should().Contain("level-up");
        stingers.Should().Contain("quest-complete");
    }

    /// <summary>
    /// Verifies GetThemeVolume returns configured volume.
    /// </summary>
    [Test]
    public void GetThemeVolume_ReturnsConfiguredVolume()
    {
        // Act
        var volume = _config.GetThemeVolume(MusicTheme.MainMenu);

        // Assert
        volume.Should().Be(0.7f);
    }

    /// <summary>
    /// Verifies ShouldShuffle returns configured value.
    /// </summary>
    [Test]
    public void ShouldShuffle_WithShuffleTheme_ReturnsTrue()
    {
        // Act
        var shuffle = _config.ShouldShuffle(MusicTheme.Exploration);

        // Assert
        shuffle.Should().BeTrue();
    }
}
