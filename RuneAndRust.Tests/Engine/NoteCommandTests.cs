using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the note command in CommandParser (v0.3.20a).
/// Validates note set, read, clear, and persistence behavior.
/// </summary>
public class NoteCommandTests
{
    private readonly Mock<ILogger<CommandParser>> _mockLogger;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly CommandParser _sut;
    private readonly GameState _state;
    private readonly Guid _testRoomId = Guid.NewGuid();

    public NoteCommandTests()
    {
        _mockLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _state = new GameState
        {
            Phase = GamePhase.Exploration,
            CurrentRoomId = _testRoomId
        };

        _sut = new CommandParser(
            _mockLogger.Object,
            _mockInputHandler.Object,
            _state);
    }

    #region Note Set Tests

    [Fact]
    public async Task NoteCommand_SetNote_AddsNoteToState()
    {
        // Arrange
        var noteText = "Danger ahead!";

        // Act
        await _sut.ParseAndExecuteAsync($"note {noteText}", _state);

        // Assert
        _state.UserNotes.Should().ContainKey(_testRoomId);
        _state.UserNotes[_testRoomId].Should().Be(noteText);
    }

    [Fact]
    public async Task NoteCommand_SetNote_DisplaysConfirmation()
    {
        // Arrange
        var noteText = "Treasure here";

        // Act
        await _sut.ParseAndExecuteAsync($"note {noteText}", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayMessage(It.Is<string>(s =>
            s.Contains("Note set") && s.Contains(noteText))), Times.Once);
    }

    [Fact]
    public async Task NoteCommand_OverwriteExistingNote_ReplacesNote()
    {
        // Arrange
        _state.UserNotes[_testRoomId] = "Old note";

        // Act
        await _sut.ParseAndExecuteAsync("note New note", _state);

        // Assert
        _state.UserNotes[_testRoomId].Should().Be("New note");
    }

    [Fact]
    public async Task NoteCommand_LongNote_TruncatesToMaxLength()
    {
        // Arrange
        var longNote = new string('x', 150); // Exceeds 100 char limit

        // Act
        await _sut.ParseAndExecuteAsync($"note {longNote}", _state);

        // Assert
        _state.UserNotes[_testRoomId].Should().HaveLength(100);
        _mockInputHandler.Verify(h => h.DisplayMessage(It.Is<string>(s =>
            s.Contains("truncated"))), Times.Once);
    }

    #endregion

    #region Note Read Tests

    [Fact]
    public async Task NoteCommand_ReadExistingNote_DisplaysNote()
    {
        // Arrange
        _state.UserNotes[_testRoomId] = "Existing note";

        // Act
        await _sut.ParseAndExecuteAsync("note", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayMessage(It.Is<string>(s =>
            s.Contains("Existing note"))), Times.Once);
    }

    [Fact]
    public async Task NoteCommand_ReadNonExistentNote_DisplaysNoNoteMessage()
    {
        // Arrange - no notes set

        // Act
        await _sut.ParseAndExecuteAsync("note", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayMessage(It.Is<string>(s =>
            s.Contains("No note"))), Times.Once);
    }

    #endregion

    #region Note Clear Tests

    [Fact]
    public async Task NoteCommand_ClearExistingNote_RemovesNote()
    {
        // Arrange
        _state.UserNotes[_testRoomId] = "Note to clear";

        // Act
        await _sut.ParseAndExecuteAsync("note clear", _state);

        // Assert
        _state.UserNotes.Should().NotContainKey(_testRoomId);
    }

    [Fact]
    public async Task NoteCommand_ClearNonExistentNote_DisplaysNoNoteMessage()
    {
        // Arrange - no notes set

        // Act
        await _sut.ParseAndExecuteAsync("note clear", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayMessage(It.Is<string>(s =>
            s.Contains("No note to clear"))), Times.Once);
    }

    [Fact]
    public async Task NoteCommand_ClearCaseInsensitive_Works()
    {
        // Arrange
        _state.UserNotes[_testRoomId] = "Note to clear";

        // Act
        await _sut.ParseAndExecuteAsync("note CLEAR", _state);

        // Assert
        _state.UserNotes.Should().NotContainKey(_testRoomId);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task NoteCommand_NoCurrentRoom_DisplaysError()
    {
        // Arrange
        _state.CurrentRoomId = null;

        // Act
        await _sut.ParseAndExecuteAsync("note Test", _state);

        // Assert
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s =>
            s.Contains("nowhere"))), Times.Once);
    }

    [Fact]
    public async Task NoteCommand_JustNoteWord_ReadsNote()
    {
        // Arrange - "note " with trailing space trims to "note", which reads the note

        // Act
        await _sut.ParseAndExecuteAsync("note ", _state);

        // Assert - Trailing whitespace trims to "note", which reads the note (no note set)
        _mockInputHandler.Verify(h => h.DisplayMessage(It.Is<string>(s =>
            s.Contains("No note"))), Times.Once);
    }

    #endregion

    #region Persistence Tests

    [Fact]
    public void GameState_Reset_ClearsUserNotes()
    {
        // Arrange
        _state.UserNotes[_testRoomId] = "Test note";
        _state.UserNotes[Guid.NewGuid()] = "Another note";

        // Act
        _state.Reset();

        // Assert
        _state.UserNotes.Should().BeEmpty();
    }

    #endregion
}
