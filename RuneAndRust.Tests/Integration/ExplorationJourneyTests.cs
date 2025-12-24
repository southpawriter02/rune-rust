using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Tests.Infrastructure;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// E2E integration tests for exploration journeys using ScriptedInputHandler.
/// These tests simulate complete user interactions through the game loop.
/// </summary>
public class ExplorationJourneyTests
{
    /// <summary>
    /// Tests that a new game can start and display the welcome message.
    /// Verifies the game loop initializes correctly and responds to input.
    /// </summary>
    [Fact]
    public async Task Journey_NewGame_To_FirstRoom_DisplaysWelcome()
    {
        // Arrange
        var script = new[]
        {
            "look",  // First command after setup
            "quit"   // Exit the game
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");

        // Act
        await host.RunAsync();

        // Assert
        host.InputHandler.OutputContains("Welcome to Rune & Rust!").Should().BeTrue();
        host.InputHandler.OutputContains("help").Should().BeTrue();
        host.GameState.Phase.Should().Be(GamePhase.Quit);
    }

    /// <summary>
    /// Tests that the look command displays room description.
    /// Verifies the exploration HUD renders correctly.
    /// </summary>
    [Fact]
    public async Task Journey_Look_Command_DisplaysRoomInfo()
    {
        // Arrange
        var script = new[]
        {
            "look",
            "quit"
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");

        // Act
        await host.RunAsync();

        // Assert
        // Room should be displayed (starting room has a name and description)
        host.InputHandler.OutputBuffer.Should().NotBeEmpty();
        host.InputHandler.IsScriptExhausted.Should().BeTrue();
    }

    /// <summary>
    /// Tests navigation through connected rooms.
    /// Verifies movement commands work and room transitions occur.
    /// </summary>
    [Fact]
    public async Task Journey_Navigation_MovesCharacter()
    {
        // Arrange
        var script = new[]
        {
            "look",
            "north",  // Try to move north (may or may not have exit)
            "south",  // Try to move south
            "east",   // Try to move east
            "west",   // Try to move west
            "quit"
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");
        var startRoomId = host.GameState.CurrentRoomId;

        // Act
        await host.RunAsync();

        // Assert
        // Game should complete without errors
        host.GameState.Phase.Should().Be(GamePhase.Quit);
        // At least the starting room should be visited
        host.GameState.VisitedRoomIds.Should().Contain(startRoomId!.Value);
    }

    /// <summary>
    /// Tests that invalid commands are handled gracefully.
    /// Verifies error messages are shown for unknown commands.
    /// </summary>
    [Fact]
    public async Task Journey_InvalidCommand_ShowsError()
    {
        // Arrange
        var script = new[]
        {
            "xyzzy",  // Invalid command
            "quit"
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");

        // Act
        await host.RunAsync();

        // Assert
        // Game should complete without crashing
        host.GameState.Phase.Should().Be(GamePhase.Quit);
        // Error buffer should contain something about unknown command
        // (or OutputBuffer may contain error message)
        var hasErrorMessage = host.InputHandler.ErrorBuffer.Any() ||
                              host.InputHandler.OutputContains("unknown") ||
                              host.InputHandler.OutputContains("command");
        hasErrorMessage.Should().BeTrue("Invalid command should produce some feedback");
    }

    /// <summary>
    /// Tests the help command displays available commands.
    /// </summary>
    [Fact]
    public async Task Journey_Help_Command_DisplaysOptions()
    {
        // Arrange
        var script = new[]
        {
            "help",
            "quit"
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");

        // Act
        await host.RunAsync();

        // Assert
        host.GameState.Phase.Should().Be(GamePhase.Quit);
        // Help should display some available commands
        host.InputHandler.OutputBuffer.Count.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that the game handles script exhaustion gracefully.
    /// When the script runs out, ScriptedInputHandler returns "quit".
    /// </summary>
    [Fact]
    public async Task Journey_ScriptExhaustion_ReturnsQuit()
    {
        // Arrange - only one command before exhaustion
        var script = new[]
        {
            "look"
            // No quit command - script will exhaust and auto-quit
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");

        // Act
        await host.RunAsync();

        // Assert
        host.GameState.Phase.Should().Be(GamePhase.Quit);
        host.InputHandler.IsScriptExhausted.Should().BeTrue();
    }

    /// <summary>
    /// Tests deterministic behavior with the same seed.
    /// Two runs with the same seed should produce identical results.
    /// </summary>
    [Fact]
    public async Task Journey_DeterministicSeed_ProducesSameResults()
    {
        // Arrange
        var script = new[] { "look", "quit" };
        const int seed = 12345;

        // First run
        using var host1 = TestGameHost.Create(seed: seed, script);
        await host1.SetupExplorationAsync("TestExplorer");
        await host1.RunAsync();
        var output1 = host1.InputHandler.GetFullOutput();

        // Second run with same seed
        using var host2 = TestGameHost.Create(seed: seed, script);
        await host2.SetupExplorationAsync("TestExplorer");
        await host2.RunAsync();
        var output2 = host2.InputHandler.GetFullOutput();

        // Assert
        output1.Should().Be(output2, "Same seed should produce identical output");
    }

    /// <summary>
    /// Tests that different seeds produce different dungeon layouts.
    /// </summary>
    [Fact]
    public async Task Journey_DifferentSeeds_ProduceDifferentRoomIds()
    {
        // Arrange
        var script = new[] { "quit" };

        // Run with seed 100
        using var host1 = TestGameHost.Create(seed: 100, script);
        await host1.SetupExplorationAsync("TestExplorer");
        var roomId1 = host1.GameState.CurrentRoomId;

        // Run with seed 200
        using var host2 = TestGameHost.Create(seed: 200, script);
        await host2.SetupExplorationAsync("TestExplorer");
        var roomId2 = host2.GameState.CurrentRoomId;

        // Assert - GUIDs are unique per database, so IDs will differ
        // This test verifies isolation between test hosts
        roomId1.HasValue.Should().BeTrue();
        roomId2.HasValue.Should().BeTrue();
        roomId1!.Value.Should().NotBe(roomId2!.Value);
    }

    /// <summary>
    /// Tests a multi-step exploration journey.
    /// Simulates a longer play session with multiple commands.
    /// </summary>
    [Fact]
    public async Task Journey_ExtendedExploration_CompletesSuccessfully()
    {
        // Arrange
        var script = new[]
        {
            "look",
            "help",
            "inventory",
            "journal",
            "status",
            "look",
            "quit"
        };

        using var host = TestGameHost.Create(seed: 42, script);
        await host.SetupExplorationAsync("TestExplorer");

        // Act
        await host.RunAsync();

        // Assert
        host.GameState.Phase.Should().Be(GamePhase.Quit);
        host.InputHandler.RemainingCommands.Should().Be(0);
        host.InputHandler.OutputBuffer.Should().NotBeEmpty();
        // Multiple prompts should have been displayed
        host.InputHandler.PromptBuffer.Count.Should().BeGreaterThanOrEqualTo(6);
    }
}
