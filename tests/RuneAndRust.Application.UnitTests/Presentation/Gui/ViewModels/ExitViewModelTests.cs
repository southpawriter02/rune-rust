using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="ExitViewModel"/>.
/// </summary>
[TestFixture]
public class ExitViewModelTests
{
    /// <summary>
    /// Verifies that Symbol returns the up arrow for North direction.
    /// </summary>
    [Test]
    public void Symbol_ForNorth_ReturnsUpArrow()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.North, false);

        // Assert
        exit.Symbol.Should().Be("↑");
    }

    /// <summary>
    /// Verifies that Symbol returns the down arrow for South direction.
    /// </summary>
    [Test]
    public void Symbol_ForSouth_ReturnsDownArrow()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.South, false);

        // Assert
        exit.Symbol.Should().Be("↓");
    }

    /// <summary>
    /// Verifies that Symbol returns the right arrow for East direction.
    /// </summary>
    [Test]
    public void Symbol_ForEast_ReturnsRightArrow()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.East, false);

        // Assert
        exit.Symbol.Should().Be("→");
    }

    /// <summary>
    /// Verifies that Symbol returns the left arrow for West direction.
    /// </summary>
    [Test]
    public void Symbol_ForWest_ReturnsLeftArrow()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.West, false);

        // Assert
        exit.Symbol.Should().Be("←");
    }

    /// <summary>
    /// Verifies that Label combines symbol and direction name.
    /// </summary>
    [Test]
    public void Label_CombinesSymbolAndDirection()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.North, false);

        // Assert
        exit.Label.Should().Be("↑ North");
    }

    /// <summary>
    /// Verifies that Tooltip indicates locked state when exit is locked.
    /// </summary>
    [Test]
    public void Tooltip_WhenLocked_IndicatesLocked()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.East, true);

        // Assert
        exit.Tooltip.Should().Be("East (Locked)");
    }

    /// <summary>
    /// Verifies that Tooltip shows direction name when exit is unlocked.
    /// </summary>
    [Test]
    public void Tooltip_WhenUnlocked_ShowsDirectionOnly()
    {
        // Arrange
        var exit = new ExitViewModel(Direction.West, false);

        // Assert
        exit.Tooltip.Should().Be("West");
    }
}
