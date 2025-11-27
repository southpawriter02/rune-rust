using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Quests;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Commands;
using Moq;
using GameState = RuneAndRust.Engine.GameState;
using GamePhase = RuneAndRust.Engine.GamePhase;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.37.4: Unit tests for System Commands
/// Tests for: journal, saga, skills, rest, talk, command
/// </summary>
[TestClass]
public class SystemCommandsTests
{
    #region Test Helpers

    /// <summary>
    /// Create a basic test game state with player and room
    /// </summary>
    private GameState CreateTestGameState()
    {
        var player = new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Hero",
            Class = CharacterClass.Warrior,
            HP = 100,
            MaxHP = 110,
            Stamina = 80,
            MaxStamina = 80,
            CurrentMilestone = 3,
            CurrentLegend = 800,
            LegendToNextMilestone = 1000,
            ProgressionPoints = 5,
            PsychicStress = 25,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 3,
                Wits = 2,
                Will = 2,
                Sturdiness = 4
            },
            Abilities = new List<Ability>(),
            ActiveQuests = new List<Quest>(),
            CompletedQuests = new List<Quest>()
        };

        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Test Chamber",
            Description = "A test room for unit tests.",
            NPCs = new List<NPC>()
        };

        return new GameState
        {
            Player = player,
            CurrentRoom = room,
            CurrentPhase = GamePhase.Exploration
        };
    }

    /// <summary>
    /// Create a test quest
    /// </summary>
    private Quest CreateQuest(string title, QuestStatus status, QuestType type = QuestType.Side)
    {
        return new Quest
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = "Test quest description",
            Status = status,
            Type = type,
            Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    Description = "Complete test objective",
                    Current = 0,
                    Required = 1
                }
            }
        };
    }

    /// <summary>
    /// Create a test NPC
    /// </summary>
    private NPC CreateNPC(string name, bool isHostile = false)
    {
        return new NPC
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = "A test NPC",
            InitialGreeting = "Hello, traveler.",
            IsHostile = isHostile,
            HasBeenMet = false,
            Faction = FactionType.Independents
        };
    }

    /// <summary>
    /// Create a test ability
    /// </summary>
    private Ability CreateAbility(string name, int staminaCost = 10)
    {
        return new Ability
        {
            Name = name,
            Description = "Test ability",
            StaminaCost = staminaCost,
            APCost = 0
        };
    }

    #endregion

    #region JournalCommand Tests

    [TestMethod]
    public void Journal_NoActiveQuests_ShowsEmptyMessage()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new JournalCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("ACTIVE QUESTS: None"));
    }

    [TestMethod]
    public void Journal_WithActiveQuests_DisplaysQuests()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.ActiveQuests.Add(CreateQuest("The Broken Kernel", QuestStatus.Active, QuestType.Main));
        state.Player.ActiveQuests.Add(CreateQuest("Kara's Protocol", QuestStatus.Active, QuestType.Side));

        var command = new JournalCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("The Broken Kernel"));
        Assert.IsTrue(result.Message.Contains("Kara's Protocol"));
        Assert.IsTrue(result.Message.Contains("[Main Quest]"));
        Assert.IsTrue(result.Message.Contains("[Side Quest]"));
    }

    [TestMethod]
    public void Journal_WithCompletedQuests_ShowsCount()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.CompletedQuests.Add(CreateQuest("Completed Quest 1", QuestStatus.Completed));
        state.Player.CompletedQuests.Add(CreateQuest("Completed Quest 2", QuestStatus.Completed));
        state.Player.CompletedQuests.Add(CreateQuest("Completed Quest 3", QuestStatus.Completed));

        var command = new JournalCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("COMPLETED QUESTS: 3"));
    }

    #endregion

    #region SagaCommand Tests

    [TestMethod]
    public void Saga_DisplaysCharacterInfo()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Name = "Astrid the Rust-Breaker";
        state.Player.CurrentMilestone = 5;
        state.Player.ProgressionPoints = 3;

        var command = new SagaCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Astrid the Rust-Breaker"));
        Assert.IsTrue(result.Message.Contains("Milestone: 5"));
        Assert.IsTrue(result.Message.Contains("Available Points: 3 PP"));
    }

    [TestMethod]
    public void Saga_DisplaysAttributes()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Attributes.Might = 14;
        state.Player.Attributes.Finesse = 12;

        var command = new SagaCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("MIGHT"));
        Assert.IsTrue(result.Message.Contains("FINESSE"));
        Assert.IsTrue(result.Message.Contains("Cost: 1 PP"));
    }

    [TestMethod]
    public void Saga_DisplaysLegendProgress()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.CurrentLegend = 1200;
        state.Player.LegendToNextMilestone = 1500;

        var command = new SagaCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Legend: 1200/1500"));
    }

    #endregion

    #region SkillsCommand Tests

    [TestMethod]
    public void Skills_NoAbilities_ShowsEmptyMessage()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new SkillsCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("No abilities learned yet"));
    }

    [TestMethod]
    public void Skills_WithAbilities_DisplaysAbilities()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Abilities.Add(CreateAbility("Power Strike", 15));
        state.Player.Abilities.Add(CreateAbility("Shield Bash", 10));
        state.Player.Abilities.Add(CreateAbility("Warrior's Vigor", 0)); // Passive

        var command = new SkillsCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Power Strike") ||
                      result.Message.Contains("Shield Bash") ||
                      result.Message.Contains("Warrior's Vigor"));
    }

    [TestMethod]
    public void Skills_ShowsTotalAbilityCount()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Abilities.Add(CreateAbility("Ability 1"));
        state.Player.Abilities.Add(CreateAbility("Ability 2"));
        state.Player.Abilities.Add(CreateAbility("Ability 3"));

        var command = new SkillsCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Total Abilities: 3"));
    }

    #endregion

    #region RestCommand Tests

    [TestMethod]
    public void Rest_RestoresHPAndStamina()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.HP = 50;
        state.Player.MaxHP = 110;
        state.Player.Stamina = 20;
        state.Player.MaxStamina = 80;

        var command = new RestCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(110, state.Player.HP);
        Assert.AreEqual(80, state.Player.Stamina);
        Assert.IsTrue(result.Message.Contains("HP restored"));
        Assert.IsTrue(result.Message.Contains("Stamina restored"));
    }

    [TestMethod]
    public void Rest_DuringCombat_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        state.CurrentPhase = GamePhase.Combat;

        var command = new RestCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("cannot rest during combat"));
    }

    [TestMethod]
    public void Rest_ClearsTemporaryEffects()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.DefenseBonus = 5;
        state.Player.DefenseTurnsRemaining = 2;
        state.Player.BattleRageTurnsRemaining = 3;

        var command = new RestCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, state.Player.DefenseBonus);
        Assert.AreEqual(0, state.Player.DefenseTurnsRemaining);
        Assert.AreEqual(0, state.Player.BattleRageTurnsRemaining);
    }

    [TestMethod]
    public void Rest_PsychicStressUnchanged()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.PsychicStress = 45;

        var command = new RestCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(45, state.Player.PsychicStress); // Unchanged
        Assert.IsTrue(result.Message.Contains("Psychic Stress: 45/100 (unchanged)"));
    }

    [TestMethod]
    public void Rest_RestoresMysticAether()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Class = CharacterClass.Mystic;
        state.Player.AP = 10;
        state.Player.MaxAP = 50;

        var command = new RestCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(50, state.Player.AP);
        Assert.IsTrue(result.Message.Contains("Aether restored"));
    }

    #endregion

    #region TalkCommand Tests

    [TestMethod]
    public void Talk_NoNPCsInRoom_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new TalkCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("no one here to talk to"));
    }

    [TestMethod]
    public void Talk_NPCExists_InitiatesDialogue()
    {
        // Arrange
        var state = CreateTestGameState();
        var npc = CreateNPC("Grizelda");
        state.CurrentRoom.NPCs.Add(npc);

        var command = new TalkCommand();

        // Act
        var result = command.Execute(state, new[] { "Grizelda" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Grizelda"));
        Assert.IsTrue(npc.HasBeenMet);
    }

    [TestMethod]
    public void Talk_HostileNPC_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        var npc = CreateNPC("Hostile Bandit", isHostile: true);
        state.CurrentRoom.NPCs.Add(npc);

        var command = new TalkCommand();

        // Act
        var result = command.Execute(state, new[] { "Hostile", "Bandit" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("hostile"));
    }

    [TestMethod]
    public void Talk_NPCNotFound_ShowsAvailableNPCs()
    {
        // Arrange
        var state = CreateTestGameState();
        state.CurrentRoom.NPCs.Add(CreateNPC("NPC1"));
        state.CurrentRoom.NPCs.Add(CreateNPC("NPC2"));

        var command = new TalkCommand();

        // Act
        var result = command.Execute(state, new[] { "nonexistent" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Available NPCs"));
    }

    [TestMethod]
    public void Talk_WithToKeyword_ParsesCorrectly()
    {
        // Arrange
        var state = CreateTestGameState();
        var npc = CreateNPC("Merchant");
        state.CurrentRoom.NPCs.Add(npc);

        var command = new TalkCommand();

        // Act
        var result = command.Execute(state, new[] { "to", "Merchant" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Merchant"));
    }

    #endregion

    #region CompanionCommandCommand Tests

    [TestMethod]
    public void CompanionCommand_NotInCombat_ReturnsFailure()
    {
        // Arrange
        var mockCompanionService = new Mock<CompanionService>("test");
        var state = CreateTestGameState();
        state.CurrentPhase = GamePhase.Exploration;

        var command = new CompanionCommandCommand(mockCompanionService.Object);

        // Act
        var result = command.Execute(state, new[] { "Kara", "attack", "enemy" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("only command companions during combat"));
    }

    [TestMethod]
    public void CompanionCommand_InsufficientArguments_ReturnsFailure()
    {
        // Arrange
        var mockCompanionService = new Mock<CompanionService>("test");
        var state = CreateTestGameState();
        state.CurrentPhase = GamePhase.Combat;
        state.Combat = new CombatState();

        var command = new CompanionCommandCommand(mockCompanionService.Object);

        // Act
        var result = command.Execute(state, new[] { "Kara" }); // Missing action

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Usage: command"));
    }

    [TestMethod]
    public void CompanionCommand_CompanionNotFound_ReturnsFailure()
    {
        // Arrange
        var mockCompanionService = new Mock<CompanionService>("test");
        mockCompanionService
            .Setup(s => s.GetCompanionByName(It.IsAny<int>(), It.IsAny<string>()))
            .Returns((Companion?)null);

        var state = CreateTestGameState();
        state.CurrentPhase = GamePhase.Combat;
        state.Combat = new CombatState();

        var command = new CompanionCommandCommand(mockCompanionService.Object);

        // Act
        var result = command.Execute(state, new[] { "Nonexistent", "attack", "enemy" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not in your active party"));
    }

    #endregion
}
