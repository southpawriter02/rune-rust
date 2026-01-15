using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="MainMenuViewModel"/>.
/// </summary>
[TestFixture]
public class MainMenuViewModelTests
{
    private MainMenuViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new MainMenuViewModel();
    }

    [Test]
    public void Version_ReturnsVersionString()
    {
        // Assert
        _viewModel.Version.Should().Contain("v0.7.0");
    }

    [Test]
    public void NewGameCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.NewGameCommand.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void LoadGameCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.LoadGameCommand.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void OpenSettingsCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.OpenSettingsCommand.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void QuitCommand_CanExecute_ReturnsTrue()
    {
        // Assert
        _viewModel.QuitCommand.CanExecute(null).Should().BeTrue();
    }
}
