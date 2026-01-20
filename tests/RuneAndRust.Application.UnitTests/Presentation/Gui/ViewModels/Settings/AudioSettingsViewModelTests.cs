using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Presentation.Gui.ViewModels.Settings;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels.Settings;

/// <summary>
/// Unit tests for <see cref="AudioSettingsViewModel"/>.
/// </summary>
[TestFixture]
public class AudioSettingsViewModelTests
{
    private AudioSettingsViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new AudioSettingsViewModel();
    }

    /// <summary>
    /// Verifies LoadSettings populates all properties.
    /// </summary>
    [Test]
    public void LoadSettings_PopulatesAllProperties()
    {
        // Arrange
        var settings = new AudioSettings
        {
            Enabled = false,
            MasterVolume = 0.5f,
            MusicVolume = 0.4f,
            EffectsVolume = 0.3f,
            UiVolume = 0.2f,
            Muted = true,
            TuiBellEnabled = false
        };

        // Act
        _viewModel.LoadSettings(settings);

        // Assert
        _viewModel.IsAudioEnabled.Should().BeFalse();
        _viewModel.MasterVolume.Should().Be(0.5f);
        _viewModel.MusicVolume.Should().Be(0.4f);
        _viewModel.EffectsVolume.Should().Be(0.3f);
        _viewModel.UiVolume.Should().Be(0.2f);
        _viewModel.IsMuted.Should().BeTrue();
        _viewModel.TuiBellEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies GetSettings returns current values.
    /// </summary>
    [Test]
    public void GetSettings_ReturnsCurrentValues()
    {
        // Arrange
        _viewModel.IsAudioEnabled = true;
        _viewModel.MasterVolume = 0.9f;
        _viewModel.MusicVolume = 0.7f;
        _viewModel.EffectsVolume = 0.6f;
        _viewModel.UiVolume = 0.5f;
        _viewModel.IsMuted = false;
        _viewModel.TuiBellEnabled = true;

        // Act
        var settings = _viewModel.GetSettings();

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.MasterVolume.Should().Be(0.9f);
        settings.MusicVolume.Should().Be(0.7f);
        settings.EffectsVolume.Should().Be(0.6f);
        settings.UiVolume.Should().Be(0.5f);
        settings.Muted.Should().BeFalse();
        settings.TuiBellEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies RevertSettings restores saved values.
    /// </summary>
    [Test]
    public void RevertSettings_RestoresSavedValues()
    {
        // Arrange
        var original = new AudioSettings { MasterVolume = 0.5f };
        _viewModel.LoadSettings(original);
        _viewModel.SaveSettings();

        // Modify
        _viewModel.MasterVolume = 0.9f;
        _viewModel.MasterVolume.Should().Be(0.9f);

        // Act
        _viewModel.RevertSettings();

        // Assert
        _viewModel.MasterVolume.Should().Be(0.5f);
    }

    /// <summary>
    /// Verifies percentage display is correct.
    /// </summary>
    [Test]
    public void VolumePercent_DisplaysCorrectPercentage()
    {
        // Arrange & Act
        _viewModel.MasterVolume = 0.75f;
        _viewModel.MusicVolume = 0.5f;
        _viewModel.EffectsVolume = 0.25f;
        _viewModel.UiVolume = 1.0f;

        // Assert
        _viewModel.MasterVolumePercent.Should().Be("75%");
        _viewModel.MusicVolumePercent.Should().Be("50%");
        _viewModel.EffectsVolumePercent.Should().Be("25%");
        _viewModel.UiVolumePercent.Should().Be("100%");
    }

    /// <summary>
    /// Verifies SaveSettings stores current values.
    /// </summary>
    [Test]
    public void SaveSettings_StoresCurrentValues()
    {
        // Arrange
        _viewModel.LoadSettings(new AudioSettings());
        _viewModel.MasterVolume = 0.33f;

        // Act
        _viewModel.SaveSettings();
        _viewModel.MasterVolume = 0.99f;
        _viewModel.RevertSettings();

        // Assert - Should revert to 0.33f, not the default
        _viewModel.MasterVolume.Should().Be(0.33f);
    }

    /// <summary>
    /// Verifies default values are set correctly.
    /// </summary>
    [Test]
    public void DefaultValues_AreSetCorrectly()
    {
        // Assert
        _viewModel.IsAudioEnabled.Should().BeTrue();
        _viewModel.MasterVolume.Should().Be(0.8f);
        _viewModel.MusicVolume.Should().Be(0.6f);
        _viewModel.EffectsVolume.Should().Be(0.8f);
        _viewModel.UiVolume.Should().Be(0.7f);
        _viewModel.IsMuted.Should().BeFalse();
        _viewModel.TuiBellEnabled.Should().BeTrue();
    }
}
