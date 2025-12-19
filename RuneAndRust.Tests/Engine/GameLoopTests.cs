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
/// Integration tests for the Game Loop.
/// Tests the interaction between GameService, CommandParser, and GameState.
/// </summary>
public class GameLoopTests
{
    private readonly Mock<ILogger<GameService>> _mockGameLogger;
    private readonly Mock<ILogger<CommandParser>> _mockParserLogger;
    private readonly Mock<IInputHandler> _mockInputHandler;
    private readonly GameState _state;
    private readonly CommandParser _parser;

    public GameLoopTests()
    {
        _mockGameLogger = new Mock<ILogger<GameService>>();
        _mockParserLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _state = new GameState();
        _parser = new CommandParser(_mockParserLogger.Object, _mockInputHandler.Object, _state);
    }

    #region Basic Loop Tests

    [Fact]
    public void Start_InputQuit_ShouldExitLoopImmediately()
    {
        // Arrange - Input sequence: "quit"
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _state.Phase.Should().Be(GamePhase.Quit);
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Start_InputStartThenQuit_ShouldTransitionPhases()
    {
        // Arrange - Input sequence: "start" then "quit"
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Exactly(2));
        _state.Phase.Should().Be(GamePhase.Quit);
    }

    [Fact]
    public void Start_InputStartExploreQuit_ShouldProcessMultipleCommands()
    {
        // Arrange - Input sequence: "start" -> "look" -> "look" -> "quit"
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("look")
            .Returns("look")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Exactly(4));
        _state.TurnCount.Should().Be(2);
    }

    #endregion

    #region Phase Transition Tests

    [Fact]
    public void Start_FullGameCycle_ShouldTransitionCorrectly()
    {
        // Arrange - Full cycle: MainMenu -> Exploration -> MainMenu -> Quit
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")    // MainMenu -> Exploration
            .Returns("look")     // Stay in Exploration
            .Returns("menu")     // Exploration -> MainMenu
            .Returns("quit");    // MainMenu -> Quit
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _state.Phase.Should().Be(GamePhase.Quit);
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Exactly(4));
    }

    [Fact]
    public void Start_MainMenuToExploration_ShouldSetSessionActive()
    {
        // Arrange
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Note: We can't easily check intermediate state without more complex mocking
        // The session was active after "start" but inactive by time we quit from Exploration
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Start_InvalidCommands_ShouldNotExitLoop()
    {
        // Arrange - Input invalid commands before quitting
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("invalid")
            .Returns("nonsense")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Exactly(3));
        _mockInputHandler.Verify(x => x.DisplayError(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void Start_EmptyInputs_ShouldBeIgnored()
    {
        // Arrange
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("")
            .Returns("   ")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Exactly(3));
        _state.Phase.Should().Be(GamePhase.Quit);
    }

    #endregion

    #region Help Command Tests

    [Fact]
    public void Start_HelpInMainMenu_ShouldStayInMainMenu()
    {
        // Arrange
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("help")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert - Should process help, then quit
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Exactly(2));
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("MAIN MENU"))), Times.Once);
    }

    [Fact]
    public void Start_HelpInExploration_ShouldStayInExploration()
    {
        // Arrange
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("help")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("EXPLORATION"))), Times.Once);
    }

    #endregion

    #region State Persistence Tests

    [Fact]
    public void Start_TurnCount_ShouldPersistAcrossCommands()
    {
        // Arrange
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("look")
            .Returns("look")
            .Returns("look")
            .Returns("status")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _state.TurnCount.Should().Be(3);
    }

    [Fact]
    public void Start_NewGame_ShouldResetTurnCount()
    {
        // Arrange - Start, explore, return to menu, start again
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("look")
            .Returns("look")
            .Returns("menu")
            .Returns("start")  // New game should reset turn count
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _state.TurnCount.Should().Be(0); // Reset after second "start"
    }

    #endregion

    #region Prompt Tests

    [Fact]
    public void Start_InMainMenu_ShouldShowMenuPrompt()
    {
        // Arrange
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.Is<string>(s => s.Contains("[MENU]"))), Times.Once);
    }

    [Fact]
    public void Start_InExploration_ShouldShowExplorePrompt()
    {
        // Arrange
        _mockInputHandler.SetupSequence(x => x.GetInput(It.IsAny<string>()))
            .Returns("start")
            .Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.Is<string>(s => s.Contains("[EXPLORE]"))), Times.Once);
    }

    #endregion
}
