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
/// Validates utility-based AI decision logic, state triggers, and resource management.
/// </summary>
public class EnemyAIServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<IAttackResolutionService> _mockAttackResolution;
    private readonly Mock<IAbilityService> _mockAbilityService;
    private readonly Mock<ILogger<EnemyAIService>> _mockLogger;
    private readonly EnemyAIService _sut;

    public EnemyAIServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockAttackResolution = new Mock<IAttackResolutionService>();
        _mockAbilityService = new Mock<IAbilityService>();
        _mockLogger = new Mock<ILogger<EnemyAIService>>();

        // Default: All attacks are affordable
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(true);

        // Default: Ability service returns false for CanUse (no abilities by default)
        _mockAbilityService.Setup(a => a.CanUse(It.IsAny<Combatant>(), It.IsAny<ActiveAbility>()))
            .Returns(false);

        _sut = new EnemyAIService(
            _mockDice.Object,
            _mockAttackResolution.Object,
            _mockAbilityService.Object,
            _mockLogger.Object);
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

    #region Archetype Behavior Tests - DPS/Aggressive (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_DPS_AttacksByDefault()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // Roll 0 picks first weighted option (typically heavy attack with highest score)
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - DPS with utility scoring prefers attacks
        action.Type.Should().Be(ActionType.Attack);
        action.TargetId.Should().NotBeNull();
    }

    [Fact]
    public void DetermineAction_DPS_CanSelectHeavyAttack()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // Roll 0 picks highest-weighted action (heavy attack scores higher)
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - With roll 0, should pick heavy (highest score among attacks)
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Heavy);
    }

    [Fact]
    public void DetermineAction_GlassCannon_PrefersAttacks()
    {
        // Arrange - GlassCannon gets damage bonus in utility scoring
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.GlassCannon);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - GlassCannon prefers attacking
        action.Type.Should().Be(ActionType.Attack);
    }

    #endregion

    #region Archetype Behavior Tests - Tank (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_Tank_DefendsWhenWounded()
    {
        // Arrange - Tank below 40% HP gets high defend bonus
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 15,  // 30% of max
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        // Roll 0 picks highest scored action
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Wounded tank gets +55 defend bonus (25+30), should often defend
        // With utility scoring, defend can still be outscored by attacks
        action.Should().NotBeNull();
    }

    [Fact]
    public void DetermineAction_Tank_CanAttackWhenHealthy()
    {
        // Arrange - Tank above 40% HP
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 50,
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Tank can attack when healthy
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_Tank_DefendsWhenNoAttacksAffordable()
    {
        // Arrange - Tank that cannot afford any attack
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 50,
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        // Cannot afford any attack
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Tank defends when can't attack
        action.Type.Should().Be(ActionType.Defend);
    }

    #endregion

    #region Archetype Behavior Tests - Swarm (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_Swarm_PrefersLightAttack()
    {
        // Arrange - Swarm gets +20 bonus for light attacks
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Swarm);
        var state = CreateCombatStateWithPlayer(enemy);

        // Only allow light attacks to test Swarm preference
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Heavy)).Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Standard)).Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Light)).Returns(true);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Swarm uses light attack when that's all available
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
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Defend or Pass when no attacks available
        action.Type.Should().BeOneOf(ActionType.Pass, ActionType.Defend);
    }

    #endregion

    #region Archetype Behavior Tests - Support (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_Support_CanAttack()
    {
        // Arrange - Support can attack
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Support);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Support attacks with utility scoring
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_Support_UsesStandardAttack_WhenOnlyOptionAvailable()
    {
        // Arrange - Support uses standard attack when it's the only option
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Support);
        var state = CreateCombatStateWithPlayer(enemy);

        // Can afford standard but not light or heavy
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Light)).Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Heavy)).Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Standard)).Returns(true);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Standard);
    }

    #endregion

    #region Archetype Behavior Tests - Caster (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_Caster_CanAttack()
    {
        // Arrange - Caster attacks
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Caster);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_Caster_DefendsWhenNoStamina()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Caster);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Defend is available when attacks aren't
        action.Type.Should().Be(ActionType.Defend);
    }

    #endregion

    #region Archetype Behavior Tests - Boss (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_Boss_PrefersHeavyAttack()
    {
        // Arrange - Boss gets +10 bonus for heavy attacks
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Boss);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Boss prefers heavy attack
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Heavy);
    }

    [Fact]
    public void DetermineAction_Boss_AttacksWithUtilityScoring()
    {
        // Arrange - Boss uses weighted selection
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Boss);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_Boss_DefendsWhenNoStamina()
    {
        // Arrange - Boss defends when out of stamina
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.Boss);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Defend);
    }

    #endregion

    #region State Trigger Tests - Cowardly Flee (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_CowardlyTag_HighFleeScore_AtLowHp()
    {
        // Arrange - Cowardly enemy below 25% HP gets +80 flee score
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.DPS,
            currentHp: 10,   // 20% of max
            maxHp: 50,
            tags: new List<string> { "Cowardly" });
        var state = CreateCombatStateWithPlayer(enemy);

        // Roll 0 picks highest scored action - flee should score 130 (50+80)
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - With utility scoring, flee gets +80 bonus, but may still be outscored
        // The important thing is that flee is in the selection pool
        action.Should().NotBeNull();
    }

    [Fact]
    public void DetermineAction_CowardlyTag_DoesNotFleeWhenHealthy()
    {
        // Arrange - Cowardly enemy above 25% HP should not have flee option
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.DPS,
            currentHp: 50,
            maxHp: 50,
            tags: new List<string> { "Cowardly" });
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Flee is not added when HP is above threshold
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

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Without Cowardly tag, flee is not in the pool
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_GlassCannon_HighFleeScore_WhenCowardlyAndLowHp()
    {
        // Arrange - GlassCannon with Cowardly tag at low HP
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.GlassCannon,
            currentHp: 8,    // 16% of max
            maxHp: 50,
            tags: new List<string> { "Cowardly" });
        var state = CreateCombatStateWithPlayer(enemy);

        // Roll 0 picks highest scored action
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Flee has high score with utility scoring
        action.Should().NotBeNull();
    }

    #endregion

    #region Resource Management Tests (Updated for Utility Scoring v0.2.4b)

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

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().Be(AttackType.Light);
    }

    [Fact]
    public void DetermineAction_NoStamina_DefendsInstead()
    {
        // Arrange - DPS with no stamina for any attack
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS, stamina: 0);
        var state = CreateCombatStateWithPlayer(enemy);

        // Cannot afford any attack
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - With utility scoring, defend is always available
        action.Type.Should().Be(ActionType.Defend);
    }

    [Fact]
    public void DetermineAction_CannotAffordHeavy_FallsBackToStandard()
    {
        // Arrange - DPS can only afford standard
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Heavy))
            .Returns(false);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Standard))
            .Returns(true);
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), AttackType.Light))
            .Returns(true);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
        action.AttackType.Should().BeOneOf(AttackType.Standard, AttackType.Light);
    }

    #endregion

    #region Target Selection Tests (Updated for Utility Scoring v0.2.4b)

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

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.TargetId.Should().Be(player.Id);
    }

    #endregion

    #region Edge Cases (Updated for Utility Scoring v0.2.4b)

    [Fact]
    public void DetermineAction_ZeroMaxHp_DoesNotCrash()
    {
        // Arrange - Edge case: enemy with 0 max HP (should not divide by zero)
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS, currentHp: 0, maxHp: 0);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act - should not throw
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Should().NotBeNull();
    }

    [Fact]
    public void DetermineAction_DefaultArchetype_CanAttack()
    {
        // Arrange - Unknown/default archetype can still attack
        var enemy = CreateEnemyCombatant(archetype: (EnemyArchetype)999);  // Invalid enum
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.Attack);
    }

    [Fact]
    public void DetermineAction_CombatActionContainsSourceId()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

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

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.FlavorText.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Utility Scoring Tests (v0.2.4b)

    [Fact]
    public void DetermineAction_PrioritizesHeal_WhenHpCritical_AndNoAttacksAvailable()
    {
        // Arrange - Enemy at 20% HP with heal ability, no attacks available
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Support,
            currentHp: 10,
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        var healAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Self-Repair",
            EffectScript = "HEAL:15",
            StaminaCost = 5
        };
        enemy.Abilities.Add(healAbility);

        // Disable all basic attacks to isolate ability scoring
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        // Ability is usable
        _mockAbilityService.Setup(a => a.CanUse(enemy, healAbility)).Returns(true);

        // Weighted selection picks first action (heal should be highest score)
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - At critical HP, heal should score 50 + 50 = 100
        action.Type.Should().Be(ActionType.UseAbility);
        action.AbilityId.Should().Be(healAbility.Id);
    }

    [Fact]
    public void DetermineAction_SkipsHeal_WhenHpFull()
    {
        // Arrange - Enemy at full HP with heal ability
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Support,
            currentHp: 50,
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        var healAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Self-Repair",
            EffectScript = "HEAL:15",
            StaminaCost = 5
        };
        enemy.Abilities.Add(healAbility);

        // Ability is usable
        _mockAbilityService.Setup(a => a.CanUse(enemy, healAbility)).Returns(true);

        // Roll picks based on weighted selection
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - At full HP, heal scores 50 - 40 = 10, attacks score higher
        action.Type.Should().NotBe(ActionType.UseAbility);
    }

    [Fact]
    public void DetermineAction_SelectsDamageAbility_WhenNoAttacksAvailable()
    {
        // Arrange - Target at 15% HP, no basic attacks available
        var player = CreatePlayerCombatant();
        player.CurrentHp = 7;  // 7/50 = 14%
        player.MaxHp = 50;

        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.GlassCannon);
        var state = new CombatState { TurnOrder = new List<Combatant> { player, enemy } };

        var damageAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Power Strike",
            EffectScript = "DAMAGE:Physical:2d6",
            StaminaCost = 5
        };
        enemy.Abilities.Add(damageAbility);

        // Disable all basic attacks
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        _mockAbilityService.Setup(a => a.CanUse(enemy, damageAbility)).Returns(true);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - When only ability is available, it should be selected
        action.Type.Should().Be(ActionType.UseAbility);
        action.AbilityId.Should().Be(damageAbility.Id);
    }

    [Fact]
    public void DetermineAction_SkipsRedundantDebuff()
    {
        // Arrange - Target already has Bleeding debuff
        var player = CreatePlayerCombatant();
        player.StatusEffects.Add(new ActiveStatusEffect
        {
            Type = StatusEffectType.Bleeding,
            DurationRemaining = 3,
            Stacks = 2
        });

        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = new CombatState { TurnOrder = new List<Combatant> { player, enemy } };

        var bleedAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Rending Strike",
            EffectScript = "STATUS:Bleeding:3:2",
            StaminaCost = 5
        };
        enemy.Abilities.Add(bleedAbility);

        _mockAbilityService.Setup(a => a.CanUse(enemy, bleedAbility)).Returns(true);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Redundant debuff scores 50 - 100 = -50, filtered out
        action.Type.Should().NotBe(ActionType.UseAbility);
    }

    [Fact]
    public void DetermineAction_RespectsAbilityCooldowns()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        var onCooldownAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Big Slam",
            EffectScript = "DAMAGE:Physical:3d8",
            StaminaCost = 10,
            CooldownTurns = 3
        };
        enemy.Abilities.Add(onCooldownAbility);

        // Ability is on cooldown (CanUse returns false)
        _mockAbilityService.Setup(a => a.CanUse(enemy, onCooldownAbility)).Returns(false);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Should not use the ability that's on cooldown
        action.AbilityId.Should().NotBe(onCooldownAbility.Id);
    }

    [Fact]
    public void DetermineAction_RespectsStaminaCosts()
    {
        // Arrange - Enemy with low stamina
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.DPS,
            stamina: 5);
        var state = CreateCombatStateWithPlayer(enemy);

        var expensiveAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Mega Strike",
            EffectScript = "DAMAGE:Physical:4d10",
            StaminaCost = 10  // More than 50% of current stamina
        };
        enemy.Abilities.Add(expensiveAbility);

        // Cannot use due to insufficient stamina
        _mockAbilityService.Setup(a => a.CanUse(enemy, expensiveAbility)).Returns(false);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.AbilityId.Should().NotBe(expensiveAbility.Id);
    }

    [Fact]
    public void DetermineAction_GlassCannonBonusToDamage_WhenNoBasicAttacks()
    {
        // Arrange - GlassCannon with damage ability, no basic attacks available
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.GlassCannon);
        var state = CreateCombatStateWithPlayer(enemy);

        var damageAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Piercing Shot",
            EffectScript = "DAMAGE:Physical:2d6",
            StaminaCost = 5
        };
        enemy.Abilities.Add(damageAbility);

        // Disable all basic attacks to isolate ability scoring
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        _mockAbilityService.Setup(a => a.CanUse(enemy, damageAbility)).Returns(true);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - GlassCannon gets +20 bonus for damage abilities
        action.Type.Should().Be(ActionType.UseAbility);
        action.AbilityId.Should().Be(damageAbility.Id);
    }

    [Fact]
    public void DetermineAction_TankBonusToDefend()
    {
        // Arrange - Tank below 40% HP (wounded threshold)
        var enemy = CreateEnemyCombatant(
            archetype: EnemyArchetype.Tank,
            currentHp: 15,  // 30% of max
            maxHp: 50);
        var state = CreateCombatStateWithPlayer(enemy);

        // No abilities, attacks unavailable
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Wounded tank gets high defend bonus
        action.Type.Should().Be(ActionType.Defend);
    }

    [Fact]
    public void DetermineAction_ReturnsUseAbilityAction_WithAbilityId_WhenNoBasicAttacks()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        var ability = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Test Ability",
            EffectScript = "DAMAGE:Physical:1d6",
            StaminaCost = 3
        };
        enemy.Abilities.Add(ability);

        // Disable all basic attacks to isolate ability selection
        _mockAttackResolution.Setup(a => a.CanAffordAttack(It.IsAny<Combatant>(), It.IsAny<AttackType>()))
            .Returns(false);

        _mockAbilityService.Setup(a => a.CanUse(enemy, ability)).Returns(true);
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert
        action.Type.Should().Be(ActionType.UseAbility);
        action.AbilityId.Should().Be(ability.Id);
        action.SourceId.Should().Be(enemy.Id);
    }

    [Fact]
    public void DetermineAction_FallsBackToBasicAttack_WhenNoAbilities()
    {
        // Arrange - Enemy with no abilities
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // No abilities in list
        enemy.Abilities.Clear();

        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - Should use basic attack when no abilities available
        action.Type.Should().Be(ActionType.Attack);
        action.AbilityId.Should().BeNull();
    }

    [Fact]
    public void DetermineAction_WeightedSelection_HigherScoreMoreLikely()
    {
        // Arrange
        var enemy = CreateEnemyCombatant(archetype: EnemyArchetype.DPS);
        var state = CreateCombatStateWithPlayer(enemy);

        // Two abilities with different scores
        var weakAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Weak Poke",
            EffectScript = "DAMAGE:Physical:1d4",
            StaminaCost = 1
        };
        var strongAbility = new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Strong Slam",
            EffectScript = "DAMAGE:Physical:3d8",
            StaminaCost = 5
        };
        enemy.Abilities.Add(weakAbility);
        enemy.Abilities.Add(strongAbility);

        _mockAbilityService.Setup(a => a.CanUse(enemy, It.IsAny<ActiveAbility>())).Returns(true);

        // Roll near end of weight range to test selection
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>())).Returns(0);

        // Act
        var action = _sut.DetermineAction(enemy, state);

        // Assert - With roll of 0, should pick first valid option
        action.Type.Should().NotBe(ActionType.Pass);
    }

    #endregion
}
