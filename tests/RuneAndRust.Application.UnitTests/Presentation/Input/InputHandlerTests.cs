using FluentAssertions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Presentation.Input;

namespace RuneAndRust.Application.UnitTests.Presentation.Input;

/// <summary>
/// Unit tests for <see cref="InputHandler"/>.
/// </summary>
[TestFixture]
public class InputHandlerTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private ICommandHistoryService _historyService = null!;
    private InputHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _historyService = new CommandHistoryService();
        _handler = new InputHandler(_historyService, _mockTerminal.Object);
    }

    #region Character Input Tests

    [Test]
    public void HandleKey_Character_AddsToInput()
    {
        // Act
        _handler.HandleKey(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

        // Assert
        _handler.CurrentInput.Should().Be("a");
        _handler.CursorPosition.Should().Be(1);
    }

    [Test]
    public void HandleKey_MultipleCharacters_BuildsInput()
    {
        // Act
        _handler.HandleKey(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));
        _handler.HandleKey(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));

        // Assert
        _handler.CurrentInput.Should().Be("hi");
    }

    #endregion

    #region Enter Tests

    [Test]
    public void HandleKey_Enter_ReturnsCommand()
    {
        // Arrange
        _handler.HandleKey(new ConsoleKeyInfo('g', ConsoleKey.G, false, false, false));
        _handler.HandleKey(new ConsoleKeyInfo('o', ConsoleKey.O, false, false, false));

        // Act
        var result = _handler.HandleKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));

        // Assert
        result.Should().Be("go");
    }

    [Test]
    public void HandleKey_Enter_AddsToHistory()
    {
        // Arrange
        _handler.HandleKey(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false));
        _handler.HandleKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));

        // Assert
        _historyService.Count.Should().Be(1);
    }

    #endregion

    #region History Navigation Tests

    [Test]
    public void HandleKey_UpArrow_RecallsHistory()
    {
        // Arrange - Add command to history
        _historyService.Add("previous");

        // Act
        _handler.HandleKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));

        // Assert
        _handler.CurrentInput.Should().Be("previous");
    }

    [Test]
    public void HandleKey_DownArrow_NavigatesForward()
    {
        // Arrange
        _historyService.Add("old");
        _handler.HandleKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));

        // Act
        _handler.HandleKey(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));

        // Assert
        _handler.CurrentInput.Should().Be("");
    }

    #endregion

    #region Cursor Movement Tests

    [Test]
    public void HandleKey_LeftArrow_MovesCursorLeft()
    {
        // Arrange
        _handler.HandleKey(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        _handler.HandleKey(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false));

        // Act
        _handler.HandleKey(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));

        // Assert
        _handler.CursorPosition.Should().Be(1);
    }

    [Test]
    public void HandleKey_Home_MovesCursorToStart()
    {
        // Arrange
        _handler.HandleKey(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false));
        _handler.HandleKey(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));

        // Act
        _handler.HandleKey(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false));

        // Assert
        _handler.CursorPosition.Should().Be(0);
    }

    #endregion

    #region Editing Tests

    [Test]
    public void HandleKey_Backspace_DeletesCharacter()
    {
        // Arrange
        _handler.HandleKey(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        _handler.HandleKey(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false));

        // Act
        _handler.HandleKey(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));

        // Assert
        _handler.CurrentInput.Should().Be("a");
    }

    #endregion

    #region Reset Tests

    [Test]
    public void Reset_ClearsInput()
    {
        // Arrange
        _handler.HandleKey(new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));

        // Act
        _handler.Reset();

        // Assert
        _handler.CurrentInput.Should().Be("");
        _handler.CursorPosition.Should().Be(0);
    }

    #endregion
}
