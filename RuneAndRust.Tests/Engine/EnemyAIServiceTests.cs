using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the EnemyAIService class.
/// Validates archetype-based AI decision logic, state triggers, and resource management.
/// </summary>
public class EnemyAIServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<IAttackResolutionService> _mockAttackResolution;
    private readonly Mock<ILogger<EnemyAIService>> _mockLogger;
    private readonly EnemyAIService _sut;

    public EnemyAIServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockAttackResolution = new Mock<IAttackResolutionService>();
        _mockLogger = new Mock<ILogger<EnemyAIService>>();

        // Default: All attacks are affordable
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(true);

        _sut = new EnemyAIService(_mockDice.Object, _mockAttackResolution.Object, _mockLogger.Object);
    }

    #region Helper Methods

    private static Combatant CreatePlayerCombatant(string name = "Test Player")
    {
        var character = new CharacterEntity
        {
            Name = name,
            Might = 5,
            Finesse = 5,
            Sturdiness = 5,
            CurrentStamina = 60,
            MaxStamina = 60,
            CurrentHP = 50,
            MaxHP = 50
        };
        return Combatant.FromCharacter(character);
    }

    private static Combatant CreateEnemyCombatant(
        string name = "Test Enemy",
        EnemyArchetype archetype = EnemyArchetype.DPS,
        int currentHp = 50,
        int maxHp = 50,
        int stamina = 35,
        List<string>? tags = null)
    {
        var enemy = new Enemy
        {
            Name = name,
            Archetype = archetype,
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Might, 5 },
                { CharacterAttribute.Finesse, 3 },
                { CharacterAttribute.Sturdiness, 5 },
                { CharacterAttribute.Wits, 3 },
                { CharacterAttribute.Will, 3 }
            },
            MaxHp = maxHp,
            CurrentHp = currentHp,
            MaxStamina = stamina,
            CurrentStamina = stamina,
            Tags = tags ?? new List<string>()
        };
        return Combatant.FromEnemy(enemy);
    }

    private static CombatState CreateCombatStateWithPlayer(Combatant enemy)
    {
        var player = CreatePlayerCombatant();
        return new CombatState
        {
            TurnOrder = new List<Combatant> { player, enemy }
        };
    }

    #endregion

    #region Archetype Behavior Tests - DPS/Aggressive

    [Fact]
    public void DetermineAction_DPS_AttacksByDefault()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // Low roll - standard attack
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
        action.TargetId.Should().NotBeNull();
    }

    [Fact]
    public void DetermineAction_DPS_HeavyAttackOnHighRoll()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // High roll (>= 80) triggers heavy attack
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(85);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Heavy);
    }

    [Fact]
    public void DetermineAction_GlassCannon_BehavesLikeDPS()
    {
        // Arrange - GlassCannon uses aggressive logic
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.GlassCannon);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    #endregion

    #region Archetype Behavior Tests - Tank

    [Fact]
    public void DetermineAction_Tank_DefendsWhenWounded()
    {
        // Arrange - Tank below 40% HP should defend
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 15,  // 30% of max
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Defend);
        action.TargetId.Should().BeNull();
    }

    [Fact]
    public void DetermineAction_Tank_AttacksWhenHealthy_LowRoll()
    {
        // Arrange - Tank above 40% HP, roll < 60 = attack
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 50,
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(30);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    [Fact]
    public void DetermineAction_Tank_DefendsWhenHealthy_HighRoll()
    {
        // Arrange - Tank above 40% HP, roll >= 60 = defend
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 50,
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(65);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Defend);
    }

    #endregion

    #region Archetype Behavior Tests - Swarm

    [Fact]
    public void DetermineAction_Swarm_AlwaysLightAttack()
    {
        // Arrange - Swarm always uses light attacks
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Swarm);
        var state = CreateCombatStateWithPlayer(enemy);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Light);
    }

    [Fact]
    public void DetermineAction_Swarm_PassesWhenNoStamina()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Swarm, stamina: 0);
        var state = CreateCombatStateWithPlayer(enemy);

        // Cannot afford any attack
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Pass);
    }

    #endregion

    #region Archetype Behavior Tests - Support

    [Fact]
    public void DetermineAction_Support_PrefersLightAttack_LowRoll()
    {
        // Arrange - Support prefers light attacks (70% chance)
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Support);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Light);
    }

    [Fact]
    public void DetermineAction_Support_UsesStandardAttack_HighRoll()
    {
        // Arrange - Support uses standard attack on high roll (>= 70)
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Support);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(75);

        // Can afford standard but not light (to force standard path)
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Light))
            .Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Standard))
            .Returns(true);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    #endregion

    #region Archetype Behavior Tests - Caster

    [Fact]
    public void DetermineAction_Caster_UsesStandardAttack()
    {
        // Arrange - Caster uses standard attacks (ranged flavor)
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Caster);
        var state = CreateCombatStateWithPlayer(enemy);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
        action.FlavorText.Should().Contain("corrupted energy");
    }

    [Fact]
    public void DetermineAction_Caster_PassesWhenNoStamina()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Caster);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Pass);
        action.FlavorText.Should().Contain("gathers power");
    }

    #endregion

    #region Archetype Behavior Tests - Boss

    [Fact]
    public void DetermineAction_Boss_PrefersHeavyAttack()
    {
        // Arrange - Boss has 50% chance for heavy attack
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Boss);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(75);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Heavy);
    }

    [Fact]
    public void DetermineAction_Boss_UsesStandardOnLowRoll()
    {
        // Arrange - Boss uses standard attack when roll < 50
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Boss);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(30);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    [Fact]
    public void DetermineAction_Boss_DefendsWhenNoStamina()
    {
        // Arrange - Boss defends when out of stamina
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Boss);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Defend);
    }

    #endregion

    #region State Trigger Tests - Cowardly Flee

    [Fact]
    public void DetermineAction_CowardlyTag_FleesAtLowHp()
    {
        // Arrange - Cowardly enemy below 25% HP should flee
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.DPS,
            currentHp: 10,   // 20% of max
            maxHp: 50,
            tags: new List<string> { "Cowardly" });
        var state = CreateCombatStateWithPlayer(enemy);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Flee);
        action.FlavorText.Should().Contain("panics");
    }

    [Fact]
    public void DetermineAction_CowardlyTag_DoesNotFleeWhenHealthy()
    {
        // Arrange - Cowardly enemy above 25% HP should not flee
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.DPS,
            currentHp: 50,
            maxHp: 50,
            tags: new List<string> { "Cowardly" });
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_NoCowardlyTag_DoesNotFlee()
    {
        // Arrange - Non-cowardly enemy should not flee even at low HP
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.DPS,
            currentHp: 5,    // 10% of max
            maxHp: 50,
            tags: new List<string>());
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_GlassCannon_FleesWhenCowardlyAndLowHp()
    {
        // Arrange - GlassCannon with Cowardly tag at low HP
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.GlassCannon,
            currentHp: 8,    // 16% of max
            maxHp: 50,
            tags: new List<string> { "Cowardly" });
        var state = CreateCombatStateWithPlayer(enemy);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Flee);
    }

    #endregion

    #region Resource Management Tests

    [Fact]
    public void DetermineAction_LowStamina_FallsBackToLightAttack()
    {
        // Arrange - DPS that can only afford light attack
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // Cannot afford heavy or standard, only light
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Heavy))
            .Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Standard))
            .Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Light))
            .Returns(true);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Light);
    }

    [Fact]
    public void DetermineAction_NoStamina_PassesTurn()
    {
        // Arrange - DPS with no stamina for any attack
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS, stamina: 0);
        var state = CreateCombatStateWithPlayer(enemy);

        // Cannot afford any attack
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Pass);
        action.FlavorText.Should().Contain("exhausted");
    }

    [Fact]
    public void DetermineAction_CannotAffordHeavy_FallsBackToStandard()
    {
        // Arrange - DPS wants heavy but can only afford standard
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // High roll would trigger heavy, but cannot afford it
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(90);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Heavy))
            .Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Standard))
            .Returns(true);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    #endregion

    #region Target Selection Tests

    [Fact]
    public void DetermineAction_NoPlayer_PassesTurn()
    {
        // Arrange - No player in combat state
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = new CombatState
        {
            TurnOrder = new List<Combatant> { enemy }  // No player
        };

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Pass);
        action.TargetId.Should().BeNull();
    }

    [Fact]
    public void DetermineAction_FindsPlayerTarget()
    {
        // Arrange
        var player = CreatePlayerCombatant();
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = new CombatState
        {
            TurnOrder = new List<Combatant> { player, enemy }
        };

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.TargetId.Should().Be(player.Id);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DetermineAction_ZeroMaxHp_DoesNotCrash()
    {
        // Arrange - Edge case: enemy with 0 max HP (should not divide by zero)
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS, currentHp: 0, maxHp: 0);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act - should not throw
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Should().NotBeNull();
    }

    [Fact]
    public void DetermineAction_DefaultArchetype_UsesAggressiveLogic()
    {
        // Arrange - Unknown/default archetype falls back to aggressive
        var enemy = CreateEnemyCombatant(archetype: (EnemyArchetype)999);  // Invalid enum
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    [Fact]
    public void DetermineAction_CombatActionContainsSourceId()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.SourceId.Should().Be(enemy.Id);
    }

    [Fact]
    public void DetermineAction_AttackAction_HasFlavorText()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.FlavorText.Should().NotBeNullOrEmpty();
    }

    #endregion
}
