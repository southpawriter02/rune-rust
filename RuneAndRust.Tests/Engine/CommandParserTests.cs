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
    private readonly Mock<ICombatService> _mockCombatService;
    private readonly CommandParser _sut;
    private readonly GameState _state;

    public CommandParserTests()
    {
        _mockLogger = new Mock<ILogger<CommandParser>>();
        _mockInputHandler = new Mock<IInputHandler>();
        _mockCombatService = new Mock<ICombatService>();
        _state = new GameState();

        // Setup mock combat service to properly end combat
        _mockCombatService.Setup(c => c.EndCombat())
            .Callback(() => {
                _state.Phase = GamePhase.Exploration;
                _state.CombatState = null;
            });

        _sut = new CommandParser(
            _mockLogger.Object,
            _mockInputHandler.Object,
            _state,
            journalService: null,
            combatService: _mockCombatService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new CommandParser(_mockLogger.Object, _mockInputHandler.Object, _state);

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
        _sut.ParseAndExecuteAsync("", _state).GetAwaiter().GetResult();

        // Assert
        _state.Phase.Should().Be(initialPhase);
    }

    [Fact]
    public void ParseAndExecute_WhitespaceInput_ShouldNotChangeState()
    {
        // Arrange
        var initialPhase = _state.Phase;

        // Act
        _sut.ParseAndExecuteAsync("   ", _state).GetAwaiter().GetResult();

        // Assert
        _state.Phase.Should().Be(initialPhase);
    }

    [Fact]
    public void ParseAndExecute_NullInput_ShouldNotChangeState()
    {
        // Arrange
        var initialPhase = _state.Phase;

        // Act
        _sut.ParseAndExecuteAsync(null!, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("unknown", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("start", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("load", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("STATUS"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_UnknownCommand_ShouldDisplayError()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        _sut.ParseAndExecuteAsync("dance", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("save", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("load", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("COMBAT"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Combat_UnknownCommand_ShouldDisplayError()
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecuteAsync("attack", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("help", _state).GetAwaiter().GetResult();

        // Assert
        VerifyLogLevel(LogLevel.Debug);
    }

    [Fact]
    public void ParseAndExecute_ShouldLogInformation_WhenStartingGame()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecuteAsync("start", _state).GetAwaiter().GetResult();

        // Assert
        VerifyLogLevel(LogLevel.Information);
    }

    [Fact]
    public void ParseAndExecute_ShouldLogInformation_WhenQuitting()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecuteAsync("quit", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync(input, _state).GetAwaiter().GetResult();

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
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

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
        var result = _sut.ParseAndExecuteAsync("go sideways", _state).GetAwaiter().GetResult();

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
        var result = _sut.ParseAndExecuteAsync("exits", _state).GetAwaiter().GetResult();

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
        _sut.ParseAndExecuteAsync("menu", _state).GetAwaiter().GetResult();

        // Assert
        _state.CurrentRoomId.Should().BeNull();
    }

    [Fact]
    public void ParseAndExecute_MainMenu_Start_ReturnsRequiresLook()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        var result = _sut.ParseAndExecuteAsync("start", _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresLook.Should().BeTrue();
    }

    [Fact]
    public void ParseAndExecute_Combat_Flee_ReturnsRequiresLook()
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        var result = _sut.ParseAndExecuteAsync("flee", _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresLook.Should().BeTrue();
    }

    [Fact]
    public void ParseAndExecute_EmptyInput_ReturnsNone()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync("", _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresNavigation.Should().BeFalse();
        result.RequiresLook.Should().BeFalse();
        result.NavigationDirection.Should().BeNull();
    }

    #endregion

    #region Inventory Command Tests

    [Theory]
    [InlineData("inventory")]
    [InlineData("i")]
    [InlineData("pack")]
    public void ParseAndExecute_Exploration_InventoryCommands_ReturnsRequiresInventory(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresInventory.Should().BeTrue();
    }

    [Theory]
    [InlineData("equipment")]
    [InlineData("gear")]
    [InlineData("equipped")]
    public void ParseAndExecute_Exploration_EquipmentCommands_ReturnsRequiresEquipment(string command)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresEquipment.Should().BeTrue();
    }

    [Theory]
    [InlineData("equip Iron Sword", "iron sword")]
    [InlineData("bind Iron Sword", "iron sword")]
    [InlineData("equip health potion", "health potion")]
    public void ParseAndExecute_Exploration_EquipCommand_ReturnsEquipWithTarget(string command, string expectedTarget)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresEquip.Should().BeTrue();
        result.EquipTarget.Should().Be(expectedTarget);
    }

    [Theory]
    [InlineData("unequip mainhand", "mainhand")]
    [InlineData("unbind Iron Sword", "iron sword")]
    [InlineData("remove helmet", "helmet")]
    public void ParseAndExecute_Exploration_UnequipCommand_ReturnsUnequipWithTarget(string command, string expectedTarget)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresUnequip.Should().BeTrue();
        result.UnequipTarget.Should().Be(expectedTarget);
    }

    [Theory]
    [InlineData("drop Iron Sword", "iron sword")]
    [InlineData("discard junk", "junk")]
    public void ParseAndExecute_Exploration_DropCommand_ReturnsDropWithTarget(string command, string expectedTarget)
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync(command, _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresDrop.Should().BeTrue();
        result.DropTarget.Should().Be(expectedTarget);
    }

    [Fact]
    public void ParseAndExecute_Exploration_EquipWithoutTarget_DisplaysError()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act - "equip" alone (no space/target) goes to default handler
        var result = _sut.ParseAndExecuteAsync("equip", _state).GetAwaiter().GetResult();

        // Assert - treated as unknown command since no target
        result.RequiresEquip.Should().BeFalse();
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown command"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_UnequipWithoutTarget_DisplaysError()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act - "unequip" alone (no space/target) goes to default handler
        var result = _sut.ParseAndExecuteAsync("unequip", _state).GetAwaiter().GetResult();

        // Assert - treated as unknown command since no target
        result.RequiresUnequip.Should().BeFalse();
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown command"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_DropWithoutTarget_DisplaysError()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act - "drop" alone (no space/target) goes to default handler
        var result = _sut.ParseAndExecuteAsync("drop", _state).GetAwaiter().GetResult();

        // Assert - treated as unknown command since no target
        result.RequiresDrop.Should().BeFalse();
        _mockInputHandler.Verify(x => x.DisplayError(It.Is<string>(s => s.Contains("Unknown command"))), Times.Once);
    }

    [Fact]
    public void ParseAndExecute_Exploration_Objects_ReturnsRequiresListObjects()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        var result = _sut.ParseAndExecuteAsync("objects", _state).GetAwaiter().GetResult();

        // Assert
        result.RequiresListObjects.Should().BeTrue();
        result.RequiresInventory.Should().BeFalse(); // Should be separate from inventory
    }

    [Fact]
    public void ParseAndExecute_Exploration_Help_IncludesInventoryCommands()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act
        _sut.ParseAndExecuteAsync("help", _state).GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("Inventory"))), Times.Once);
        _mockInputHandler.Verify(x => x.DisplayMessage(It.Is<string>(s => s.Contains("equip"))), Times.AtLeastOnce);
    }

    #endregion

    #region Rest & Camp Commands (v0.3.2c)

    [Fact]
    public void Rest_WithoutRequiredServices_DisplaysErrorMessage()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act - Parser doesn't have rest service injected
        _sut.ParseAndExecuteAsync("rest", _state).GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("not available"))), Times.Once);
    }

    [Fact]
    public void Camp_WithoutRequiredServices_DisplaysErrorMessage()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act - Parser doesn't have rest service injected
        _sut.ParseAndExecuteAsync("camp", _state).GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("not available"))), Times.Once);
    }

    [Fact]
    public void Sleep_WithoutRequiredServices_DisplaysErrorMessage()
    {
        // Arrange
        _state.Phase = GamePhase.Exploration;

        // Act - Parser doesn't have rest service injected (sleep is alias for camp)
        _sut.ParseAndExecuteAsync("sleep", _state).GetAwaiter().GetResult();

        // Assert
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("not available"))), Times.Once);
    }

    [Fact]
    public void Rest_InMainMenuPhase_DoesNotTriggerRestCommand()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecuteAsync("rest", _state).GetAwaiter().GetResult();

        // Assert - Should be treated as unknown command in MainMenu
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("Unknown"))), Times.Once);
    }

    [Fact]
    public void Camp_InMainMenuPhase_DoesNotTriggerCampCommand()
    {
        // Arrange
        _state.Phase = GamePhase.MainMenu;

        // Act
        _sut.ParseAndExecuteAsync("camp", _state).GetAwaiter().GetResult();

        // Assert - Should be treated as unknown command in MainMenu
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("Unknown"))), Times.Once);
    }

    [Fact]
    public void Rest_InCombatPhase_DoesNotTriggerRestCommand()
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecuteAsync("rest", _state).GetAwaiter().GetResult();

        // Assert - Should be treated as unknown command in Combat
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("Unknown"))), Times.Once);
    }

    [Fact]
    public void Camp_InCombatPhase_DoesNotTriggerCampCommand()
    {
        // Arrange
        _state.Phase = GamePhase.Combat;

        // Act
        _sut.ParseAndExecuteAsync("camp", _state).GetAwaiter().GetResult();

        // Assert - Should be treated as unknown command in Combat
        _mockInputHandler.Verify(h => h.DisplayError(It.Is<string>(s => s.Contains("Unknown"))), Times.Once);
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
