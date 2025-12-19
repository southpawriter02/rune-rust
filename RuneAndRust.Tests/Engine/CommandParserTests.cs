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
/// Extensive tests for the CommandParser class.
/// Validates command parsing, state transitions, and logging behavior across all game phases.
/// </summary>
public class CommandParserTests
{
    private readonly Mock<ILogger<CommandParser>> _mockLogger;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly CommandParser _sut;
    private readonly GameState _state;

    public CommandParserTests()
    {
        _mockLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _sut = new CommandParser(_mockLogger.Object, _mockInputHandler.Object);
        _state = new GameState();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new CommandParser(_mockLogger.Object, _mockInputHandler.Object);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Empty Input Tests

    [Fact]
    public void ParseAndExecute_EmptyInput_ShouldNotChangeState()
    {
        // Arrange
        var initialPhase = _state.Phase;

        // Act
        _sut.ParseAndExecute("", _state);

        // Assert
        _state.Phase.Should().Be(initialPhase);
    }

    [Fact]
    public void ParseAndExecute_WhitespaceInput_ShouldNotChangeState()
    {
        // Arrange
        var initialPhase = _state.Phase;

        // Act
        _sut.ParseAndExecute("   ", _state);

        // Assert
        _state.Phase.Should().Be(initialPhase);
    }

    [Fact]
    public void ParseAndExecute_NullInput_ShouldNotChangeState()
    {
        // Arrange
        var initialPhase = _state.Phase;

        // Act
        _sut.ParseAndExecute(null!, _state);

        // Assert
        _state.Phase.Should().Be(initialPhase);
    }

    #endregion

    #region MainMenu Phase Tests

    [Theory]
    [InlineData("start")]
    [InlineData("START")]
    [InlineData("Start")]
    [InlineData("play")]
    public void ParseAndExecute_MainMenu_StartCommands_ShouldTransitionToExploration(string command)
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Exploration);
        _state.IsSessionActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("new")]
    [InlineData("create")]
    public void ParseAndExecute_MainMenu_NewCommand_ShouldRequireCharacterCreation(string command)
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        var result = _sut.ParseAndExecute(command, _state);

        // Assert
        result.RequiresCharacterCreation.Should().BeTrue();
        _state.Phase.Should().Be(GamePhase.MainMenu); // Should not change phase yet
    }

    [Theory]
    [InlineData("quit")]
    [InlineData("QUIT")]
    [InlineData("exit")]
    [InlineData("q")]
    public void ParseAndExecute_MainMenu_QuitCommands_ShouldTransitionToQuit(string command)
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Quit);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("?")]
    public void ParseAndExecute_MainMenu_HelpCommands_ShouldDisplayHelp(string command)
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("MAIN MENU"))), Times.Once);
        _state.Phase.Should().Be(GamePhase.MainMenu); // Should not change phase
    }

    [Fact]
    public void ParseAndExecute_MainMenu_UnknownCommand_ShouldDisplayError()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute("unknown", _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown command"))), Times.Once);
        _state.Phase.Should().Be(GamePhase.MainMenu);
    }

    [Fact]
    public void ParseAndExecute_MainMenu_Start_ShouldResetTurnCount()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;
        _state.TurnCount = 100;

        // Act
        _sut.ParseAndExecute("start", _state);

        // Assert
        _state.TurnCount.Should().Be(0);
    }

    [Fact]
    public void ParseAndExecute_MainMenu_Load_ShouldSetPendingActionToLoad()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;
        _state.PendingAction = PendingGameAction.None;

        // Act
        _sut.ParseAndExecute("load", _state);

        // Assert
        _state.PendingAction.Should().Be(PendingGameAction.Load);
        _state.Phase.Should().Be(GamePhase.MainMenu); // Should not change phase
    }

    #endregion

    #region Exploration Phase Tests

    [Theory]
    [InlineData("quit")]
    [InlineData("exit")]
    [InlineData("q")]
    public void ParseAndExecute_Exploration_QuitCommands_ShouldTransitionToQuit(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Quit);
    }

    [Theory]
    [InlineData("menu")]
    [InlineData("mainmenu")]
    public void ParseAndExecute_Exploration_MenuCommands_ShouldReturnToMainMenu(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;
        _state.IsSessionActive = true;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.MainMenu);
        _state.IsSessionActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("help")]
    [InlineData("?")]
    public void ParseAndExecute_Exploration_HelpCommands_ShouldDisplayHelp(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("EXPLORATION"))), Times.Once);
    }

    [Theory]
    [InlineData("look")]
    [InlineData("l")]
    public void ParseAndExecute_Exploration_LookCommands_ReturnsRequiresLook(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecute(command, _state);

        // Assert
        result.RequiresLook.Should().BeTrue();
    }

    [Theory]
    [InlineData("status")]
    [InlineData("stats")]
    public void ParseAndExecute_Exploration_StatusCommands_ShouldDisplayStatus(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;
        _state.TurnCount = 5;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("STATUS"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_UnknownCommand_ShouldDisplayError()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        _sut.ParseAndExecute("dance", _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown command"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_Save_ShouldSetPendingActionToSave()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;
        _state.PendingAction = PendingGameAction.None;

        // Act
        _sut.ParseAndExecute("save", _state);

        // Assert
        _state.PendingAction.Should().Be(PendingGameAction.Save);
        _state.Phase.Should().Be(GamePhase.Exploration); // Should not change phase
    }

    [Fact]
    public void ParseAndExecute_Exploration_Load_ShouldSetPendingActionToLoad()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;
        _state.PendingAction = PendingGameAction.None;

        // Act
        _sut.ParseAndExecute("load", _state);

        // Assert
        _state.PendingAction.Should().Be(PendingGameAction.Load);
        _state.Phase.Should().Be(GamePhase.Exploration); // Should not change phase
    }

    #endregion

    #region Combat Phase Tests

    [Theory]
    [InlineData("quit")]
    [InlineData("exit")]
    [InlineData("q")]
    public void ParseAndExecute_Combat_QuitCommands_ShouldTransitionToQuit(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Quit);
    }

    [Theory]
    [InlineData("flee")]
    [InlineData("run")]
    public void ParseAndExecute_Combat_FleeCommands_ShouldReturnToExploration(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Exploration);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("?")]
    public void ParseAndExecute_Combat_HelpCommands_ShouldDisplayHelp(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecute(command, _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("COMBAT"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Combat_UnknownCommand_ShouldDisplayError()
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecute("attack", _state);

        // Assert
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown"))), Times.Once);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void ParseAndExecute_ShouldLogDebug_WhenParsingCommand()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute("help", _state);

        // Assert
        VerifyLogLevel(LogLevel.Debug);
    }

    [Fact]
    public void ParseAndExecute_ShouldLogInformation_WhenStartingGame()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute("start", _state);

        // Assert
        VerifyLogLevel(LogLevel.Information);
    }

    [Fact]
    public void ParseAndExecute_ShouldLogInformation_WhenQuitting()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute("quit", _state);

        // Assert
        VerifyLogLevel(LogLevel.Information);
    }

    #endregion

    #region Case Insensitivity Tests

    [Theory]
    [InlineData("START")]
    [InlineData("start")]
    [InlineData("StArT")]
    [InlineData("  start  ")]
    public void ParseAndExecute_ShouldBeCaseInsensitiveAndTrimWhitespace(string input)
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecute(input, _state);

        // Assert
        _state.Phase.Should().Be(GamePhase.Exploration);
    }

    #endregion

    #region Movement Command Tests

    [Theory]
    [InlineData("north", Direction.North)]
    [InlineData("n", Direction.North)]
    [InlineData("south", Direction.South)]
    [InlineData("s", Direction.South)]
    [InlineData("east", Direction.East)]
    [InlineData("e", Direction.East)]
    [InlineData("west", Direction.West)]
    [InlineData("w", Direction.West)]
    [InlineData("up", Direction.Up)]
    [InlineData("u", Direction.Up)]
    [InlineData("down", Direction.Down)]
    [InlineData("d", Direction.Down)]
    public void ParseAndExecute_Exploration_DirectionAliases_ReturnsNavigationResult(string command, Direction expectedDirection)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecute(command, _state);

        // Assert
        result.RequiresNavigation.Should().BeTrue();
        result.NavigationDirection.Should().Be(expectedDirection);
    }

    [Theory]
    [InlineData("go north", Direction.North)]
    [InlineData("go south", Direction.South)]
    [InlineData("go east", Direction.East)]
    [InlineData("go west", Direction.West)]
    [InlineData("go up", Direction.Up)]
    [InlineData("go down", Direction.Down)]
    public void ParseAndExecute_Exploration_GoCommand_ReturnsNavigationResult(string command, Direction expectedDirection)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecute(command, _state);

        // Assert
        result.RequiresNavigation.Should().BeTrue();
        result.NavigationDirection.Should().Be(expectedDirection);
    }

    [Fact]
    public void ParseAndExecute_Exploration_GoInvalidDirection_DisplaysError()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecute("go sideways", _state);

        // Assert
        result.RequiresNavigation.Should().BeFalse();
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown direction"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_Exits_ReturnsRequiresLook()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecute("exits", _state);

        // Assert
        result.RequiresLook.Should().BeTrue();
    }

    [Fact]
    public void ParseAndExecute_Exploration_Menu_ClearsCurrentRoomId()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;
        _state.CurrentRoomId = Guid.NewGuid();

        // Act
        _sut.ParseAndExecute("menu", _state);

        // Assert
        _state.CurrentRoomId.Should().BeNull();
    }

    [Fact]
    public void ParseAndExecute_MainMenu_Start_ReturnsRequiresLook()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        var result = _sut.ParseAndExecute("start", _state);

        // Assert
        result.RequiresLook.Should().BeTrue();
    }

    [Fact]
    public void ParseAndExecute_Combat_Flee_ReturnsRequiresLook()
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        var result = _sut.ParseAndExecute("flee", _state);

        // Assert
        result.RequiresLook.Should().BeTrue();
    }

    [Fact]
    public void ParseAndExecute_EmptyInput_ReturnsNone()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecute("", _state);

        // Assert
        result.RequiresNavigation.Should().BeFalse();
        result.RequiresLook.Should().BeFalse();
        result.NavigationDirection.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private void VerifyLogLevel(LogLevel level)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
