using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Commands;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.37.2: Unit tests for Combat Commands
/// Tests: attack, ability, stance, block, parry, flee commands
/// Target: 80%+ code coverage
/// </summary>
[TestClass]
public class CombatCommandsTests
{
    private DiceService _diceService = null!;
    private CombatEngine _combatEngine = null!;
    private StanceService _stanceService = null!;

    [TestInitialize]
    public void Setup()
    {
        _diceService = new DiceService(42); // Fixed seed for deterministic tests
        var sagaService = new SagaService();
        var lootService = new LootService();
        var equipmentService = new EquipmentService();
        var hazardService = new HazardService(_diceService, new TraumaEconomyService());
        var currencyService = new CurrencyService();
        var statusEffectService = new AdvancedStatusEffectService(
            new Persistence.StatusEffectRepository(),
            new TraumaEconomyService(),
            _diceService);

        _combatEngine = new CombatEngine(
            _diceService,
            sagaService,
            lootService,
            equipmentService,
            hazardService,
            currencyService,
            statusEffectService);

        _stanceService = new StanceService();
    }

    // ============================================
    // AttackCommand Tests
    // ============================================

    [TestMethod]
    public void AttackCommand_NotInCombat_ReturnsError()
    {
        // Arrange
        var command = new AttackCommand(_combatEngine);
        var state = CreateGameState();
        state.CurrentPhase = GamePhase.Exploration; // Not in combat

        // Act
        var result = command.Execute(state, new[] { "enemy" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not in combat"));
    }

    [TestMethod]
    public void AttackCommand_NoTarget_ReturnsError()
    {
        // Arrange
        var command = new AttackCommand(_combatEngine);
        var state = CreateCombatState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Attack who"));
    }

    [TestMethod]
    public void AttackCommand_ValidTarget_ExecutesAttack()
    {
        // Arrange
        var command = new AttackCommand(_combatEngine);
        var state = CreateCombatState();

        // Act
        var result = command.Execute(state, new[] { "test_enemy" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Length > 0); // Should have combat log output
    }

    // ============================================
    // AbilityCommand Tests
    // ============================================

    [TestMethod]
    public void AbilityCommand_NotInCombat_ReturnsError()
    {
        // Arrange
        var command = new AbilityCommand(_combatEngine);
        var state = CreateGameState();
        state.CurrentPhase = GamePhase.Exploration;

        // Act
        var result = command.Execute(state, new[] { "power_strike" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("only use combat abilities during combat"));
    }

    [TestMethod]
    public void AbilityCommand_NoAbilityName_ReturnsError()
    {
        // Arrange
        var command = new AbilityCommand(_combatEngine);
        var state = CreateCombatState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Specify an ability"));
    }

    [TestMethod]
    public void AbilityCommand_UnknownAbility_ReturnsError()
    {
        // Arrange
        var command = new AbilityCommand(_combatEngine);
        var state = CreateCombatState();

        // Act
        var result = command.Execute(state, new[] { "nonexistent_ability" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("don't know the ability"));
    }

    // ============================================
    // StanceCommand Tests
    // ============================================

    [TestMethod]
    public void StanceCommand_ValidStance_ChangesStance()
    {
        // Arrange
        var command = new StanceCommand(_stanceService);
        var state = CreateGameState();

        // Act
        var result = command.Execute(state, new[] { "aggressive" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(StanceType.Aggressive, state.Player.ActiveStance.Type);
    }

    [TestMethod]
    public void StanceCommand_NoStanceArgument_ReturnsError()
    {
        // Arrange
        var command = new StanceCommand(_stanceService);
        var state = CreateGameState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Specify a stance"));
    }

    [TestMethod]
    public void StanceCommand_InvalidStance_ReturnsError()
    {
        // Arrange
        var command = new StanceCommand(_stanceService);
        var state = CreateGameState();

        // Act
        var result = command.Execute(state, new[] { "invalid" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Unknown stance"));
    }

    [TestMethod]
    public void StanceCommand_AlreadyInStance_ReturnsFalse()
    {
        // Arrange
        var command = new StanceCommand(_stanceService);
        var state = CreateGameState();
        state.Player.ActiveStance = Stance.CreateAggressiveStance();

        // Act
        var result = command.Execute(state, new[] { "aggressive" });

        // Assert
        Assert.IsFalse(result.Success);
    }

    // ============================================
    // BlockCommand Tests
    // ============================================

    [TestMethod]
    public void BlockCommand_NotInCombat_ReturnsError()
    {
        // Arrange
        var command = new BlockCommand(_combatEngine);
        var state = CreateGameState();
        state.CurrentPhase = GamePhase.Exploration;

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("only block during combat"));
    }

    [TestMethod]
    public void BlockCommand_InCombat_ExecutesDefend()
    {
        // Arrange
        var command = new BlockCommand(_combatEngine);
        var state = CreateCombatState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Length > 0); // Should have combat log output
    }

    // ============================================
    // ParryCommand Tests
    // ============================================

    [TestMethod]
    public void ParryCommand_NotInCombat_ReturnsError()
    {
        // Arrange
        var command = new ParryCommand(_combatEngine);
        var state = CreateGameState();
        state.CurrentPhase = GamePhase.Exploration;

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("only parry during combat"));
    }

    [TestMethod]
    public void ParryCommand_InCombat_PreparesParry()
    {
        // Arrange
        var command = new ParryCommand(_combatEngine);
        var state = CreateCombatState();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        // Result depends on whether counter-attack service is available
    }

    // ============================================
    // FleeCommand Tests
    // ============================================

    [TestMethod]
    public void FleeCommand_NotInCombat_ReturnsError()
    {
        // Arrange
        var command = new FleeCommand(_combatEngine);
        var state = CreateGameState();
        state.CurrentPhase = GamePhase.Exploration;

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not in combat"));
    }

    [TestMethod]
    public void FleeCommand_CannotFlee_ReturnsError()
    {
        // Arrange
        var command = new FleeCommand(_combatEngine);
        var state = CreateCombatState();
        state.Combat!.CanFlee = false;

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("cannot flee"));
    }

    [TestMethod]
    public void FleeCommand_CanFlee_AttemptsFlee()
    {
        // Arrange
        var command = new FleeCommand(_combatEngine);
        var state = CreateCombatState();
        state.Combat!.CanFlee = true;

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        // Flee success depends on FINESSE check (random)
    }

    // ============================================
    // Helper Methods
    // ============================================

    private GameState CreateGameState()
    {
        var state = new GameState();
        state.Player = new PlayerCharacter
        {
            Name = "Test Player",
            Attributes = new Attributes(3, 3, 3, 3, 3),
            HP = 50,
            MaxHP = 50,
            Stamina = 20,
            MaxStamina = 20,
            CharacterID = 1,
            ActiveStance = Stance.CreateCalculatedStance(),
            StanceShiftsRemaining = 1,
            Abilities = new List<Ability>()
        };

        state.CurrentRoom = new Room
        {
            RoomId = "TestRoom1",
            Name = "Test Room",
            Description = "A test room for unit testing",
            Exits = new Dictionary<string, string> { { "south", "PreviousRoom" } }
        };

        state.World = new GameWorld();
        state.World.Rooms.Add(state.CurrentRoom);

        // Add previous room for flee command
        state.World.Rooms.Add(new Room
        {
            RoomId = "PreviousRoom",
            Name = "Previous Room",
            Description = "Where you came from"
        });

        state.CurrentPhase = GamePhase.Exploration;

        return state;
    }

    private GameState CreateCombatState()
    {
        var state = CreateGameState();

        // Create enemy
        var enemy = new Enemy
        {
            Id = "test_enemy_1",
            Name = "Test Enemy",
            CurrentHP = 30,
            MaxHP = 30,
            Attributes = new Attributes(2, 2, 2, 2, 2)
        };

        // Initialize combat
        var enemies = new List<Enemy> { enemy };
        state.Combat = _combatEngine.InitializeCombat(state.Player, enemies, state.CurrentRoom, true, state.Player.CharacterID);
        state.CurrentPhase = GamePhase.Combat;

        // Ensure it's player's turn
        if (state.Combat.InitiativeOrder.Count > 0)
        {
            // Find player in initiative order
            for (int i = 0; i < state.Combat.InitiativeOrder.Count; i++)
            {
                if (state.Combat.InitiativeOrder[i].IsPlayer)
                {
                    state.Combat.CurrentTurnIndex = i;
                    break;
                }
            }
        }

        return state;
    }
}
