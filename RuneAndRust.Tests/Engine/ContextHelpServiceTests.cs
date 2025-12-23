using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;
using Enemy = RuneAndRust.Core.Entities.Enemy;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the ContextHelpService class (v0.3.9c).
/// Validates tip generation for status effects, resources, enemy tactics, and WITS-gating.
/// </summary>
public class ContextHelpServiceTests
{
    private readonly Mock<ILogger<ContextHelpService>> _mockLogger;
    private readonly ContextHelpService _sut;

    public ContextHelpServiceTests()
    {
        _mockLogger = new Mock<ILogger<ContextHelpService>>();
        _sut = new ContextHelpService(_mockLogger.Object);
    }

    #region Status Effect Tips Tests

    [Fact]
    public void Analyze_ReturnsTip_WhenBleedingActive()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ActiveStatusEffects.Add(StatusEffectType.Bleeding);
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Bleeding");
    }

    [Fact]
    public void Analyze_ReturnsTip_WhenPoisonedActive()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ActiveStatusEffects.Add(StatusEffectType.Poisoned);
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Poisoned");
    }

    [Fact]
    public void Analyze_ReturnsTip_WhenStunnedActive()
    {
        // Arrange
        var combatState = CreateCombatState();
        var player = combatState.TurnOrder.First(c => c.IsPlayer);
        player.StatusEffects.Add(new ActiveStatusEffect
        {
            Type = StatusEffectType.Stunned,
            DurationRemaining = 1
        });

        // Act
        var tips = _sut.AnalyzeCombat(combatState);

        // Assert
        tips.Should().Contain(t => t.Title == "Stunned" && t.Priority == HelpTip.PriorityCritical);
    }

    [Fact]
    public void Analyze_ReturnsTip_WhenVulnerableActive()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ActiveStatusEffects.Add(StatusEffectType.Vulnerable);
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Vulnerable");
    }

    [Fact]
    public void Analyze_ReturnsTip_WhenExhaustedActive()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ActiveStatusEffects.Add(StatusEffectType.Exhausted);
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Exhausted");
    }

    #endregion

    #region Resource Level Tips Tests

    [Fact]
    public void Analyze_ReturnsTip_WhenLowHP()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.MaxHP = 100;
        character.CurrentHP = 20; // 20% HP
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Low HP" && t.Priority == HelpTip.PriorityCritical);
    }

    [Fact]
    public void Analyze_ReturnsTip_WhenLowStamina()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.MaxStamina = 100;
        character.CurrentStamina = 15; // 15% Stamina
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Exhausted Stamina");
    }

    [Fact]
    public void Analyze_ReturnsTip_WhenHighStress()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.PsychicStress = 85; // 85% stress
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().Contain(t => t.Title == "Breaking Point" && t.Priority == HelpTip.PriorityCritical);
    }

    #endregion

    #region WITS-Gated Enemy Tips Tests

    [Fact]
    public void AnalyzeCombat_ReturnsTip_ForMechanicalEnemy_WhenHighWits()
    {
        // Arrange
        var combatState = CreateCombatStateWithEnemy("mechanical");
        var player = combatState.TurnOrder.First(c => c.IsPlayer);
        player.CharacterSource!.Wits = 4; // Above threshold of 3

        // Act
        var tips = _sut.AnalyzeCombat(combatState);

        // Assert
        tips.Should().Contain(t => t.Title == "Mechanical");
    }

    [Fact]
    public void AnalyzeCombat_ReturnsTip_ForArmoredEnemy_WhenAnalyzed()
    {
        // Arrange
        var combatState = CreateCombatStateWithEnemy("armored");
        var player = combatState.TurnOrder.First(c => c.IsPlayer);
        player.CharacterSource!.Wits = 1; // Below threshold

        var enemy = combatState.TurnOrder.First(c => !c.IsPlayer);
        enemy.StatusEffects.Add(new ActiveStatusEffect
        {
            Type = StatusEffectType.Analyzed,
            DurationRemaining = 1
        });

        // Act
        var tips = _sut.AnalyzeCombat(combatState);

        // Assert
        tips.Should().Contain(t => t.Title == "Armored");
    }

    [Fact]
    public void AnalyzeCombat_HidesTip_ForMechanicalEnemy_WhenLowWits()
    {
        // Arrange
        var combatState = CreateCombatStateWithEnemy("mechanical");
        var player = combatState.TurnOrder.First(c => c.IsPlayer);
        player.CharacterSource!.Wits = 2; // Below threshold of 3

        // Act
        var tips = _sut.AnalyzeCombat(combatState);

        // Assert
        tips.Should().NotContain(t => t.Title == "Mechanical",
            "tactical tips should be hidden when WITS < 3 and enemy not Analyzed");
    }

    [Fact]
    public void AnalyzeCombat_ReturnsTip_ForFlyingEnemy_WhenWitsThresholdMet()
    {
        // Arrange
        var combatState = CreateCombatStateWithEnemy("flying");
        var player = combatState.TurnOrder.First(c => c.IsPlayer);
        player.CharacterSource!.Wits = 3; // Exactly at threshold

        // Act
        var tips = _sut.AnalyzeCombat(combatState);

        // Assert
        tips.Should().Contain(t => t.Title == "Flying");
    }

    [Fact]
    public void AnalyzeCombat_WitsThreshold_IsThree()
    {
        // Assert
        _sut.WitsThreshold.Should().Be(3);
    }

    [Fact]
    public void AnalyzeCombat_AnalyzedStatus_BypassesWitsCheck()
    {
        // Arrange
        var combatState = CreateCombatStateWithEnemy("undead");
        var player = combatState.TurnOrder.First(c => c.IsPlayer);
        player.CharacterSource!.Wits = 0; // Minimum WITS

        var enemy = combatState.TurnOrder.First(c => !c.IsPlayer);
        enemy.StatusEffects.Add(new ActiveStatusEffect
        {
            Type = StatusEffectType.Analyzed,
            DurationRemaining = 1
        });

        // Act
        var tips = _sut.AnalyzeCombat(combatState);

        // Assert
        tips.Should().Contain(t => t.Title == "Undead",
            "Analyzed status should bypass WITS check");
    }

    #endregion

    #region Sorting and Limiting Tests

    [Fact]
    public void Analyze_SortsByPriority_Descending()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.CurrentHP = 20;              // Critical (10)
        character.MaxHP = 100;
        character.CurrentStamina = 15;         // Warning (8)
        character.MaxStamina = 100;
        character.ActiveStatusEffects.Add(StatusEffectType.Exhausted);  // Info (3)
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().BeInDescendingOrder(t => t.Priority);
    }

    [Fact]
    public void Analyze_LimitsResults_ToThreeTips()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.CurrentHP = 20;
        character.MaxHP = 100;
        character.CurrentStamina = 15;
        character.MaxStamina = 100;
        character.PsychicStress = 85;
        character.ActiveStatusEffects.Add(StatusEffectType.Bleeding);
        character.ActiveStatusEffects.Add(StatusEffectType.Poisoned);
        character.ActiveStatusEffects.Add(StatusEffectType.Vulnerable);
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().HaveCountLessThanOrEqualTo(3);
        _sut.MaxTips.Should().Be(3);
    }

    [Fact]
    public void Analyze_ReturnsEmpty_WhenNoConditions()
    {
        // Arrange
        var character = CreateTestCharacter(); // Healthy character
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().BeEmpty();
    }

    [Fact]
    public void Analyze_CombinesTips_FromMultipleSources()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.CurrentHP = 20;  // Resource tip
        character.MaxHP = 100;
        character.ActiveStatusEffects.Add(StatusEffectType.Bleeding);  // Status effect tip
        var state = CreateGameState(character);

        // Act
        var tips = _sut.Analyze(state);

        // Assert
        tips.Should().HaveCountGreaterThan(1);
        tips.Should().Contain(t => t.Title == "Bleeding");
        tips.Should().Contain(t => t.Title == "Low HP");
    }

    #endregion

    #region Helper Methods

    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Hero",
            CurrentHP = 100,
            MaxHP = 100,
            CurrentStamina = 100,
            MaxStamina = 100,
            PsychicStress = 0,
            Corruption = 0,
            Wits = 3,
            ActiveStatusEffects = new List<StatusEffectType>()
        };
    }

    private static GameState CreateGameState(Character character)
    {
        return new GameState
        {
            Phase = GamePhase.Exploration,
            CurrentCharacter = character,
            IsSessionActive = true
        };
    }

    private static CombatState CreateCombatState()
    {
        var playerCharacter = CreateTestCharacter();
        var playerCombatant = Combatant.FromCharacter(playerCharacter);

        var enemy = new Enemy
        {
            Id = Guid.NewGuid(),
            Name = "Test Enemy",
            CurrentHp = 50,
            MaxHp = 50,
            CurrentStamina = 50,
            MaxStamina = 50,
            Tags = new List<string>()
        };
        var enemyCombatant = Combatant.FromEnemy(enemy);

        return new CombatState
        {
            TurnOrder = new List<Combatant> { playerCombatant, enemyCombatant },
            RoundNumber = 1,
            TurnIndex = 0
        };
    }

    private static CombatState CreateCombatStateWithEnemy(string tag)
    {
        var playerCharacter = CreateTestCharacter();
        var playerCombatant = Combatant.FromCharacter(playerCharacter);

        var enemy = new Enemy
        {
            Id = Guid.NewGuid(),
            Name = "Tagged Enemy",
            CurrentHp = 50,
            MaxHp = 50,
            CurrentStamina = 50,
            MaxStamina = 50,
            Tags = new List<string> { tag }
        };
        var enemyCombatant = Combatant.FromEnemy(enemy);

        return new CombatState
        {
            TurnOrder = new List<Combatant> { playerCombatant, enemyCombatant },
            RoundNumber = 1,
            TurnIndex = 0
        };
    }

    #endregion
}
