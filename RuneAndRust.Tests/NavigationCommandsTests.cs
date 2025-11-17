using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Commands;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.37.1: Unit tests for Core Navigation Commands
/// Tests: look, go, investigate, search commands
/// Target: 80%+ code coverage
/// </summary>
[TestClass]
public class NavigationCommandsTests
{
    // ============================================
    // LookCommand Tests
    // ============================================

    [TestMethod]
    public void LookCommand_NoArgs_DisplaysFullRoomDescription()
    {
        // Arrange
        var command = new LookCommand();
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Test Room"));
        Assert.IsTrue(result.Message.Contains("A test room for unit testing"));
        Assert.IsTrue(result.Message.Contains("Exits:"));
    }

    [TestMethod]
    public void LookCommand_WithTarget_ExaminesEnemy()
    {
        // Arrange
        var command = new LookCommand();
        var state = CreateTestGameState();
        state.CurrentRoom.Enemies.Add(new Enemy
        {
            Name = "Test Warden",
            Id = "test_warden_1",
            CurrentHP = 50,
            MaxHP = 100
        });

        // Act
        var result = command.Execute(state, new[] { "warden" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Test Warden"));
        Assert.IsTrue(result.Message.Contains("HP:"));
    }

    [TestMethod]
    public void LookCommand_InvalidTarget_ReturnsError()
    {
        // Arrange
        var command = new LookCommand();
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, new[] { "nonexistent" });

        // Assert
        Assert.IsTrue(result.Success); // Still succeeds, but message indicates not found
        Assert.IsTrue(result.Message.Contains("don't see"));
    }

    // ============================================
    // GoCommand Tests
    // ============================================

    [TestMethod]
    public void GoCommand_ValidExit_MovesCharacter()
    {
        // Arrange
        var command = new GoCommand();
        var state = CreateTestGameState();
        state.CurrentRoom.Exits.Add("north", "TestRoom2");
        state.World.Rooms.Add(new Room
        {
            RoomId = "TestRoom2",
            Name = "Test Room 2",
            Description = "Another test room"
        });

        // Act
        var result = command.Execute(state, new[] { "north" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Test Room 2", state.CurrentRoom.Name);
    }

    [TestMethod]
    public void GoCommand_InvalidDirection_ReturnsError()
    {
        // Arrange
        var command = new GoCommand();
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, new[] { "west" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("no exit"));
    }

    [TestMethod]
    public void GoCommand_DuringCombat_PreventMovement()
    {
        // Arrange
        var command = new GoCommand();
        var state = CreateTestGameState();
        state.CurrentPhase = GamePhase.Combat;
        state.CurrentRoom.Exits.Add("north", "TestRoom2");

        // Act
        var result = command.Execute(state, new[] { "north" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("cannot leave during combat"));
    }

    [TestMethod]
    public void GoCommand_NoDirection_ReturnsError()
    {
        // Arrange
        var command = new GoCommand();
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Go where"));
    }

    // ============================================
    // InvestigateCommand Tests
    // ============================================

    [TestMethod]
    public void InvestigateCommand_SuccessfulCheck_RevealsContent()
    {
        // Arrange
        var diceService = new DiceService(42); // Fixed seed for deterministic results
        var command = new InvestigateCommand(diceService);
        var state = CreateTestGameState();

        // Add investigatable terrain
        state.CurrentRoom.StaticTerrain.Add(new StaticTerrain
        {
            TerrainName = "Test Corpse",
            IsInteractive = true,
            InvestigationDC = 1, // Low DC to ensure success
            InvestigationSuccessText = "You found a secret!",
            InvestigationFailureText = "Nothing here.",
            HasBeenInvestigated = false
        });

        // Set high WITS for success
        state.Player.Attributes.Wits = 5;

        // Act
        var result = command.Execute(state, new[] { "corpse" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("WITS Check"));
    }

    [TestMethod]
    public void InvestigateCommand_AlreadyInvestigated_ReturnsError()
    {
        // Arrange
        var diceService = new DiceService();
        var command = new InvestigateCommand(diceService);
        var state = CreateTestGameState();

        state.CurrentRoom.StaticTerrain.Add(new StaticTerrain
        {
            TerrainName = "Old Corpse",
            IsInteractive = true,
            HasBeenInvestigated = true
        });

        // Act
        var result = command.Execute(state, new[] { "corpse" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("already"));
    }

    [TestMethod]
    public void InvestigateCommand_NoTarget_ReturnsError()
    {
        // Arrange
        var diceService = new DiceService();
        var command = new InvestigateCommand(diceService);
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Investigate what"));
    }

    [TestMethod]
    public void InvestigateCommand_InvalidTarget_ReturnsError()
    {
        // Arrange
        var diceService = new DiceService();
        var command = new InvestigateCommand(diceService);
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, new[] { "invalid" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("cannot investigate"));
    }

    // ============================================
    // SearchCommand Tests
    // ============================================

    [TestMethod]
    public void SearchCommand_ValidContainer_FindsLoot()
    {
        // Arrange
        var lootService = new LootService();
        var command = new SearchCommand(lootService);
        var state = CreateTestGameState();

        state.CurrentRoom.LootNodes.Add(new LootNode
        {
            NodeType = "chest",
            HasBeenLooted = false,
            Tier = 1,
            FlavorText = "An old chest"
        });

        // Act
        var result = command.Execute(state, new[] { "chest" });

        // Assert
        Assert.IsTrue(result.Success);
        // Loot is random, so just check that search was performed
    }

    [TestMethod]
    public void SearchCommand_AlreadySearched_ReturnsError()
    {
        // Arrange
        var lootService = new LootService();
        var command = new SearchCommand(lootService);
        var state = CreateTestGameState();

        state.CurrentRoom.LootNodes.Add(new LootNode
        {
            NodeType = "chest",
            HasBeenLooted = true,
            Tier = 1
        });

        // Act
        var result = command.Execute(state, new[] { "chest" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("already"));
    }

    [TestMethod]
    public void SearchCommand_NoTarget_ReturnsError()
    {
        // Arrange
        var lootService = new LootService();
        var command = new SearchCommand(lootService);
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Search what"));
    }

    [TestMethod]
    public void SearchCommand_InvalidTarget_ReturnsError()
    {
        // Arrange
        var lootService = new LootService();
        var command = new SearchCommand(lootService);
        var state = CreateTestGameState();

        // Act
        var result = command.Execute(state, new[] { "invalid" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("cannot search"));
    }

    // ============================================
    // CommandDispatcher Tests
    // ============================================

    [TestMethod]
    public void CommandDispatcher_LookCommand_Dispatches()
    {
        // Arrange
        var diceService = new DiceService();
        var lootService = new LootService();
        var dispatcher = new CommandDispatcher(diceService, lootService);
        var state = CreateTestGameState();

        var parsedCommand = new ParsedCommand
        {
            Type = CommandType.Look,
            RawInput = "look"
        };

        // Act
        var result = dispatcher.Dispatch(parsedCommand, state);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Test Room"));
    }

    [TestMethod]
    public void CommandDispatcher_UnknownCommand_ReturnsError()
    {
        // Arrange
        var diceService = new DiceService();
        var lootService = new LootService();
        var dispatcher = new CommandDispatcher(diceService, lootService);
        var state = CreateTestGameState();

        var parsedCommand = new ParsedCommand
        {
            Type = CommandType.Unknown,
            RawInput = "invalid"
        };

        // Act
        var result = dispatcher.Dispatch(parsedCommand, state);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Unknown command"));
    }

    // ============================================
    // Helper Methods
    // ============================================

    private GameState CreateTestGameState()
    {
        var state = new GameState();
        state.Player = new PlayerCharacter
        {
            Name = "Test Player",
            Attributes = new Attributes(3, 3, 3, 3, 3),
            HP = 50,
            MaxHP = 50,
            CharacterID = 1
        };

        state.CurrentRoom = new Room
        {
            RoomId = "TestRoom1",
            Name = "Test Room",
            Description = "A test room for unit testing",
            Exits = new Dictionary<string, string>(),
            Enemies = new List<Enemy>(),
            NPCs = new List<NPC>(),
            StaticTerrain = new List<StaticTerrain>(),
            LootNodes = new List<LootNode>(),
            DynamicHazards = new List<DynamicHazard>(),
            ItemsOnGround = new List<Equipment>()
        };

        state.World = new GameWorld();
        state.World.Rooms.Add(state.CurrentRoom);

        state.CurrentPhase = GamePhase.Exploration;

        return state;
    }
}
