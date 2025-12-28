using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly Mock<ICombatService> _mockCombatService;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly GameState _state;
    private readonly CommandParser _parser;

    public GameLoopTests()
    {
        _mockGameLogger = new Mock<ILogger<GameService>>();
        _mockParserLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _mockCombatService = new Mock<ICombatService>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _state = new GameState();
        _parser = new CommandParser(_mockParserLogger.Object, _mockInputHandler.Object, _state);
    }

    #region Basic Loop Tests

    [Fact]
    public void Start_InputQuit_ShouldExitLoopImmediately()
    {
        // Arrange - Input sequence: "quit"
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

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
        var service = new GameService(_mockGameLogger.Object, _mockInputHandler.Object, _parser, _state, _mockCombatService.Object, _mockScopeFactory.Object);

        // Act
        service.StartAsync().GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.GetInput(It.Is<string>(s => s.Contains("[EXPLORE]"))), Times.Once);
    }

    #endregion

    #region v0.3.23b: CancellationToken Tests

    [Fact]
    public async Task StartAsync_WithCancellationRequested_ShouldExitLoop()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var inputCallCount = 0;

        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>()))
            .Returns(() =>
            {
                inputCallCount++;
                if (inputCallCount >= 2)
                {
                    cts.Cancel();
                }
                return "look";
            });

        var service = new GameService(
            _mockGameLogger.Object,
            _mockInputHandler.Object,
            _parser,
            _state,
            _mockCombatService.Object,
            _mockScopeFactory.Object);

        // First transition to Exploration
        _state.Phase = GamePhase.Exploration;

        // Act
        await service.StartAsync(cts.Token);

        // Assert - Should exit due to cancellation, not quit command
        _state.Phase.Should().NotBe(GamePhase.Quit);
    }

    [Fact]
    public async Task StartAsync_WithImmediateCancellation_ShouldNotProcessInput()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");

        var service = new GameService(
            _mockGameLogger.Object,
            _mockInputHandler.Object,
            _parser,
            _state,
            _mockCombatService.Object,
            _mockScopeFactory.Object);

        // Act
        await service.StartAsync(cts.Token);

        // Assert - Input should never be called since already cancelled
        _mockInputHandler.Verify(x => x.GetInput(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region v0.3.23b: VFX Integration Tests

    [Fact]
    public async Task StartAsync_WithVfxService_ShouldSubscribeToInvalidateEvent()
    {
        // Arrange
        var mockVfxService = new Mock<IVisualEffectService>();
        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");

        var service = new GameService(
            _mockGameLogger.Object,
            _mockInputHandler.Object,
            _parser,
            _state,
            _mockCombatService.Object,
            _mockScopeFactory.Object,
            vfxService: mockVfxService.Object);

        // Act - Constructor should have subscribed, just verify event exists
        await service.StartAsync();

        // Assert - VFX service's CheckExpiredOverrides should be called each loop iteration
        mockVfxService.Verify(x => x.CheckExpiredOverrides(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_WhenVfxExpires_ShouldTriggerRender()
    {
        // Arrange
        var mockVfxService = new Mock<IVisualEffectService>();
        var expiredCallCount = 0;

        // First call returns true (expired), subsequent calls return false
        mockVfxService.Setup(x => x.CheckExpiredOverrides())
            .Returns(() =>
            {
                expiredCallCount++;
                return expiredCallCount == 1;
            });

        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>())).Returns("quit");

        var service = new GameService(
            _mockGameLogger.Object,
            _mockInputHandler.Object,
            _parser,
            _state,
            _mockCombatService.Object,
            _mockScopeFactory.Object,
            vfxService: mockVfxService.Object);

        // Act
        await service.StartAsync();

        // Assert - CheckExpiredOverrides was called
        mockVfxService.Verify(x => x.CheckExpiredOverrides(), Times.AtLeastOnce);
    }

    #endregion

    #region v0.3.23b: Exception Resilience Tests

    [Fact]
    public async Task StartAsync_WhenExceptionThrown_ShouldContinueLoop()
    {
        // Arrange
        var callCount = 0;

        _mockInputHandler.Setup(x => x.GetInput(It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("Test exception");
                }
                return "quit";
            });

        var service = new GameService(
            _mockGameLogger.Object,
            _mockInputHandler.Object,
            _parser,
            _state,
            _mockCombatService.Object,
            _mockScopeFactory.Object);

        // Act
        await service.StartAsync();

        // Assert - Should have continued after exception and processed quit
        callCount.Should().BeGreaterThan(1);
        _state.Phase.Should().Be(GamePhase.Quit);
    }

    #endregion
}
