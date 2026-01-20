using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

/// <summary>
/// Unit tests for <see cref="NavigationService"/>.
/// </summary>
[TestFixture]
public class NavigationServiceTests
{
    private NavigationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new NavigationService();
    }

    /// <summary>
    /// Verifies that CurrentWindow returns null when no application is running.
    /// </summary>
    [Test]
    public void CurrentWindow_WhenNoApplication_ReturnsNull()
    {
        // Act
        var window = _service.CurrentWindow;

        // Assert
        window.Should().BeNull();
    }

    /// <summary>
    /// Verifies that ShowLoadDialogAsync returns null when no window is available.
    /// </summary>
    [Test]
    public async Task ShowLoadDialogAsync_WhenNoWindow_ReturnsNull()
    {
        // Act
        var result = await _service.ShowLoadDialogAsync();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that ShowSaveDialogAsync returns null when no window is available.
    /// </summary>
    [Test]
    public async Task ShowSaveDialogAsync_WhenNoWindow_ReturnsNull()
    {
        // Act
        var result = await _service.ShowSaveDialogAsync();

        // Assert
        result.Should().BeNull();
    }
}
