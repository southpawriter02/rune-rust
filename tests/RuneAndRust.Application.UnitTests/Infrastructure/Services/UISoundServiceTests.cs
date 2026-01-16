using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="UISoundService"/> and <see cref="InteractionSoundService"/>.
/// </summary>
[TestFixture]
public class UISoundServiceTests
{
    private Mock<ISoundEffectService> _sfxMock = null!;
    private UISoundService _uiService = null!;
    private InteractionSoundService _interactionService = null!;

    [SetUp]
    public void SetUp()
    {
        _sfxMock = new Mock<ISoundEffectService>();

        _uiService = new UISoundService(
            _sfxMock.Object,
            NullLogger<UISoundService>.Instance);

        _interactionService = new InteractionSoundService(
            _sfxMock.Object,
            NullLogger<InteractionSoundService>.Instance);
    }

    /// <summary>
    /// Verifies PlayButtonClick plays button-click sound.
    /// </summary>
    [Test]
    public void PlayButtonClick_PlaysButtonClick()
    {
        // Act
        _uiService.PlayButtonClick();

        // Assert
        _sfxMock.Verify(s => s.Play("button-click", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayMenuOpen plays menu-open sound.
    /// </summary>
    [Test]
    public void PlayMenuOpen_PlaysMenuOpen()
    {
        // Act
        _uiService.PlayMenuOpen();

        // Assert
        _sfxMock.Verify(s => s.Play("menu-open", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayItemPickup plays item-pickup sound.
    /// </summary>
    [Test]
    public void PlayItemPickup_PlaysItemPickup()
    {
        // Act
        _interactionService.PlayItemPickup();

        // Assert
        _sfxMock.Verify(s => s.Play("item-pickup", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayPuzzleComplete plays puzzle-complete sound.
    /// </summary>
    [Test]
    public void PlayPuzzleComplete_PlaysPuzzleComplete()
    {
        // Act
        _interactionService.PlayPuzzleComplete();

        // Assert
        _sfxMock.Verify(s => s.Play("puzzle-complete", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayFootstep maps stone terrain to footstep-stone.
    /// </summary>
    [Test]
    public void PlayFootstep_StoneTerrain_PlaysFootstepStone()
    {
        // Act
        _interactionService.PlayFootstep("stone");

        // Assert
        _sfxMock.Verify(s => s.Play("footstep-stone", 0.5f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayFootstep maps wood terrain to footstep-wood.
    /// </summary>
    [Test]
    public void PlayFootstep_WoodTerrain_PlaysFootstepWood()
    {
        // Act
        _interactionService.PlayFootstep("wood");

        // Assert
        _sfxMock.Verify(s => s.Play("footstep-wood", 0.5f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayFootstep maps grass terrain to footstep-grass.
    /// </summary>
    [Test]
    public void PlayFootstep_GrassTerrain_PlaysFootstepGrass()
    {
        // Act
        _interactionService.PlayFootstep("forest");

        // Assert
        _sfxMock.Verify(s => s.Play("footstep-grass", 0.5f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayFootstep with unknown terrain falls back to stone.
    /// </summary>
    [Test]
    public void PlayFootstep_UnknownTerrain_FallsBackToStone()
    {
        // Act
        _interactionService.PlayFootstep("lava");

        // Assert
        _sfxMock.Verify(s => s.Play("footstep-stone", 0.5f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayDoorOpen plays door-open sound.
    /// </summary>
    [Test]
    public void PlayDoorOpen_PlaysDoorOpen()
    {
        // Act
        _interactionService.PlayDoorOpen();

        // Assert
        _sfxMock.Verify(s => s.Play("door-open", 1.0f), Times.Once);
    }

    /// <summary>
    /// Verifies PlayChestOpen plays chest-open sound.
    /// </summary>
    [Test]
    public void PlayChestOpen_PlaysChestOpen()
    {
        // Act
        _interactionService.PlayChestOpen();

        // Assert
        _sfxMock.Verify(s => s.Play("chest-open", 1.0f), Times.Once);
    }
}
