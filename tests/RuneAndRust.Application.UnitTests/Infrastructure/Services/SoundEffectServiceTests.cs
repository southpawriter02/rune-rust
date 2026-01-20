using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Enums;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SoundEffectService"/>.
/// </summary>
[TestFixture]
public class SoundEffectServiceTests
{
    private Mock<IAudioService> _audioServiceMock = null!;
    private SoundEffectConfig _config = null!;
    private SoundEffectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _audioServiceMock = new Mock<IAudioService>();
        _audioServiceMock.Setup(a => a.Play(It.IsAny<string>(), It.IsAny<AudioChannel>(), It.IsAny<float>(), It.IsAny<bool>()))
            .Returns(Guid.NewGuid());
        _audioServiceMock.Setup(a => a.GetChannelVolume(AudioChannel.Effects)).Returns(0.8f);

        _config = new SoundEffectConfig(NullLogger<SoundEffectConfig>.Instance);

        _service = new SoundEffectService(
            _audioServiceMock.Object,
            _config,
            NullLogger<SoundEffectService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    /// <summary>
    /// Verifies Play by effect ID works.
    /// </summary>
    [Test]
    public void Play_WithValidEffectId_CallsAudioService()
    {
        // Act
        _service.Play("attack-hit");

        // Assert
        _audioServiceMock.Verify(a => a.Play(
            It.IsAny<string>(),
            AudioChannel.Effects,
            It.IsAny<float>(),
            false), Times.Once);
    }

    /// <summary>
    /// Verifies Play with missing effect logs warning.
    /// </summary>
    [Test]
    public void Play_WithMissingEffect_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _service.Play("nonexistent-effect");
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies PlayRandom selects from category.
    /// </summary>
    [Test]
    public void PlayRandom_WithValidCategory_PlaysEffect()
    {
        // Act
        _service.PlayRandom("combat");

        // Assert
        _audioServiceMock.Verify(a => a.Play(
            It.IsAny<string>(),
            AudioChannel.Effects,
            It.IsAny<float>(),
            false), Times.Once);
    }

    /// <summary>
    /// Verifies PlayRandom with missing category doesn't throw.
    /// </summary>
    [Test]
    public void PlayRandom_WithMissingCategory_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _service.PlayRandom("nonexistent-category");
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies StopAll stops all active playbacks.
    /// </summary>
    [Test]
    public void StopAll_StopsAllActivePlaybacks()
    {
        // Arrange
        _service.Play("attack-hit");
        _service.Play("button-click");

        // Act
        _service.StopAll();

        // Assert
        _audioServiceMock.Verify(a => a.Stop(It.IsAny<Guid>()), Times.AtLeast(2));
    }

    /// <summary>
    /// Verifies SetVolume calls AudioService.
    /// </summary>
    [Test]
    public void SetVolume_CallsAudioServiceWithClampedValue()
    {
        // Act
        _service.SetVolume(1.5f);

        // Assert
        _audioServiceMock.Verify(a => a.SetChannelVolume(AudioChannel.Effects, 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies GetVolume returns AudioService value.
    /// </summary>
    [Test]
    public void GetVolume_ReturnsAudioServiceChannelVolume()
    {
        // Act
        var volume = _service.GetVolume();

        // Assert
        volume.Should().Be(0.8f);
    }

    /// <summary>
    /// Verifies GetAvailableEffects returns all effect IDs.
    /// </summary>
    [Test]
    public void GetAvailableEffects_ReturnsAllEffectIds()
    {
        // Act
        var effects = _service.GetAvailableEffects();

        // Assert
        effects.Should().NotBeEmpty();
        effects.Should().Contain("attack-hit");
        effects.Should().Contain("button-click");
    }

    /// <summary>
    /// Verifies GetCategories returns all categories.
    /// </summary>
    [Test]
    public void GetCategories_ReturnsAllCategories()
    {
        // Act
        var categories = _service.GetCategories();

        // Assert
        categories.Should().NotBeEmpty();
        categories.Should().Contain("combat");
        categories.Should().Contain("ui");
    }

    /// <summary>
    /// Verifies config loads effect definitions correctly.
    /// </summary>
    [Test]
    public void Config_GetEffect_ReturnsDefinition()
    {
        // Act
        var effect = _config.GetEffect("attack-hit");

        // Assert
        effect.Should().NotBeNull();
        effect!.EffectId.Should().Be("attack-hit");
        effect.Category.Should().Be("combat");
        effect.Randomize.Should().BeTrue();
    }
}
