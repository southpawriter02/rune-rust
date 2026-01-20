using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Enums;
using RuneAndRust.Infrastructure.Services.Audio;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="AudioService"/>.
/// </summary>
[TestFixture]
public class AudioServiceTests
{
    private AudioService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new AudioService(NullLogger<AudioService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    /// <summary>
    /// Verifies Play returns Empty when audio is disabled.
    /// </summary>
    [Test]
    public void Play_WhenDisabled_ReturnsEmptyGuid()
    {
        // Arrange
        _service.SetEnabled(false);

        // Act
        var result = _service.Play("test.ogg", AudioChannel.Effects);

        // Assert
        result.Should().Be(Guid.Empty);
    }

    /// <summary>
    /// Verifies Play returns Empty when channel is muted.
    /// </summary>
    [Test]
    public void Play_WhenChannelMuted_ReturnsEmptyGuid()
    {
        // Arrange
        _service.SetChannelMuted(AudioChannel.Effects, true);

        // Act
        var result = _service.Play("test.ogg", AudioChannel.Effects);

        // Assert
        result.Should().Be(Guid.Empty);
    }

    /// <summary>
    /// Verifies Play returns Empty when Master is muted.
    /// </summary>
    [Test]
    public void Play_WhenMasterMuted_ReturnsEmptyGuid()
    {
        // Arrange
        _service.SetChannelMuted(AudioChannel.Master, true);

        // Act
        var result = _service.Play("test.ogg", AudioChannel.Music);

        // Assert
        result.Should().Be(Guid.Empty);
    }

    /// <summary>
    /// Verifies SetChannelVolume clamps to 0-1 range.
    /// </summary>
    [Test]
    public void SetChannelVolume_ClampsToValidRange()
    {
        // Act
        _service.SetChannelVolume(AudioChannel.Music, 1.5f);
        var high = _service.GetChannelVolume(AudioChannel.Music);

        _service.SetChannelVolume(AudioChannel.Music, -0.5f);
        var low = _service.GetChannelVolume(AudioChannel.Music);

        // Assert
        high.Should().Be(1.0f);
        low.Should().Be(0.0f);
    }

    /// <summary>
    /// Verifies SetEnabled(false) stops all audio.
    /// </summary>
    [Test]
    public void SetEnabled_WhenFalse_StopsAllAudio()
    {
        // Arrange
        _service.IsEnabled.Should().BeTrue();

        // Act
        _service.SetEnabled(false);

        // Assert
        _service.IsEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies default channel volumes are set correctly.
    /// </summary>
    [Test]
    public void DefaultVolumes_AreSetCorrectly()
    {
        // Assert
        _service.GetChannelVolume(AudioChannel.Master).Should().Be(0.8f);
        _service.GetChannelVolume(AudioChannel.Music).Should().Be(0.6f);
        _service.GetChannelVolume(AudioChannel.Effects).Should().Be(0.8f);
        _service.GetChannelVolume(AudioChannel.UI).Should().Be(0.7f);
        _service.GetChannelVolume(AudioChannel.Voice).Should().Be(0.9f);
    }

    /// <summary>
    /// Verifies mute state can be toggled.
    /// </summary>
    [Test]
    public void SetChannelMuted_TogglesState()
    {
        // Arrange
        _service.IsChannelMuted(AudioChannel.Music).Should().BeFalse();

        // Act
        _service.SetChannelMuted(AudioChannel.Music, true);

        // Assert
        _service.IsChannelMuted(AudioChannel.Music).Should().BeTrue();

        // Act
        _service.SetChannelMuted(AudioChannel.Music, false);

        // Assert
        _service.IsChannelMuted(AudioChannel.Music).Should().BeFalse();
    }
}
