using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="SettingsViewModel"/>.
/// </summary>
[TestFixture]
public class SettingsViewModelTests
{
    private SettingsViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new SettingsViewModel();
    }

    [Test]
    public void MasterVolume_DefaultValue_Is80()
    {
        // Assert
        _viewModel.MasterVolume.Should().Be(80);
    }

    [Test]
    public void MusicVolume_DefaultValue_Is60()
    {
        // Assert
        _viewModel.MusicVolume.Should().Be(60);
    }

    [Test]
    public void SfxVolume_DefaultValue_Is80()
    {
        // Assert
        _viewModel.SfxVolume.Should().Be(80);
    }

    [Test]
    public void WindowModes_ContainsExpectedOptions()
    {
        // Assert
        _viewModel.WindowModes.Should().HaveCount(3)
            .And.Contain("Windowed")
            .And.Contain("Fullscreen")
            .And.Contain("Borderless");
    }

    [Test]
    public void Resolutions_ContainsExpectedOptions()
    {
        // Assert
        _viewModel.Resolutions.Should().HaveCount(3)
            .And.Contain("1280x720")
            .And.Contain("1920x1080")
            .And.Contain("2560x1440");
    }

    [Test]
    public void SelectedWindowMode_DefaultValue_IsWindowed()
    {
        // Assert
        _viewModel.SelectedWindowMode.Should().Be("Windowed");
    }

    [Test]
    public void ApplyCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.ApplyCommand.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void CancelCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.CancelCommand.CanExecute(null).Should().BeTrue();
    }
}
