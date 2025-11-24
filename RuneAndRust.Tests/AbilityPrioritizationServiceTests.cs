using Microsoft.Extensions.Logging.Nullogger;
using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using RuneAndRust.Engine.AI;
using RuneAndRust.Persistence;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Tests;

/// <summary>
/// Unit tests for v0.42.2: Ability Prioritization Service
/// Tests ability scoring, selection, and archetype-specific modifiers
/// </summary>
[TestFixture]
public class AbilityPrioritizationServiceTests
{
    private IAbilityPrioritizationService _abilityService = null!;
    private IBehaviorPatternService _behaviorService = null!;
    private IAIConfigurationRepository _configRepo = null!;

    [SetUp]
    public void Setup()
    {
        // Initialize repository with in-memory database
        _configRepo = new AIConfigurationRepository(Path.GetTempPath());

        // Initialize services
        _behaviorService = new BehaviorPatternService(NullLogger<BehaviorPatternService>.Instance);
        _abilityService = new AbilityPrioritizationService(
            NullLogger<AbilityPrioritizationService>.Instance,
            _behaviorService,
            _configRepo);
    }

    #region Ability Scoring Tests

    [Test]
    public void CalculateDamageScore_HighDamageAbility_ReturnsHighScore()
    {
        // Arrange
        var highDamageAbility = new Ability
        {
            Name = "PowerStrike",
            DamageDice = 20, // Very high damage
            IgnoresArmor = true
        };

        var target = CreatePlayer("Target");

        // Act
        var score = _abilityService.CalculateDamageScore(highDamageAbility, target);

        // Assert
        Assert.Greater(score, 50f, "High damage ability should score high");
    }

    [Test]
    public void CalculateUtilityScore_ControlAbility_ReturnsHighScore()
    {
        // Arrange
        var stunAbility = new Ability
        {
            Name = "Stun",
            Type = AbilityType.Control,
            SkipEnemyTurn = true
        };

        var target = CreatePlayer("Target");
        var state = CreateBattlefieldState();

        // Act
        var score = _abilityService.CalculateUtilityScore(stunAbility, target, state);

        // Assert
        Assert.Greater(score, 20f, "Control ability should have high utility score");
    }

    [Test]
    public void CalculateEfficiencyScore_FreeAbility_ReturnsMaxScore()
    {
        // Arrange
        var freeAbility = new Ability
        {
            Name = "BasicAttack",
            StaminaCost = 0,
            APCost = 0
        };

        var enemy = CreateEnemy(AIArchetype.Tactical);

        // Act
        var score = _abilityService.CalculateEfficiencyScore(freeAbility, enemy);

        // Assert
        Assert.AreEqual(30f, score, "Free ability should have maximum efficiency");
    }

    [Test]
    public void CalculateEfficiencyScore_ExpensiveAbility_ReturnsLowScore()
    {
        // Arrange
        var expensiveAbility = new Ability
        {
            Name = "Ultimate",
            StaminaCost = 20
        };

        var enemy = CreateEnemy(AIArchetype.Tactical);

        // Act
        var score = _abilityService.CalculateEfficiencyScore(expensiveAbility, enemy);

        // Assert
        Assert.Less(score, 15f, "Expensive ability should have low efficiency");
    }

    #endregion

    #region Ability Category Tests

    [Test]
    public void GetAbilityCategory_AttackAbility_ReturnsDamage()
    {
        // Arrange
        var attackAbility = new Ability
        {
            Name = "Slash",
            Type = AbilityType.Attack
        };

        // Act
        var category = _abilityService.GetAbilityCategory(attackAbility);

        // Assert
        Assert.AreEqual(AbilityCategory.Damage, category);
    }

    [Test]
    public void GetAbilityCategory_DefenseAbility_ReturnsDefensive()
    {
        // Arrange
        var defenseAbility = new Ability
        {
            Name = "Shield",
            Type = AbilityType.Defense
        };

        // Act
        var category = _abilityService.GetAbilityCategory(defenseAbility);

        // Assert
        Assert.AreEqual(AbilityCategory.Defensive, category);
    }

    [Test]
    public void GetAbilityCategory_ControlAbility_ReturnsCrowdControl()
    {
        // Arrange
        var controlAbility = new Ability
        {
            Name = "Stun",
            Type = AbilityType.Control
        };

        // Act
        var category = _abilityService.GetAbilityCategory(controlAbility);

        // Assert
        Assert.AreEqual(AbilityCategory.CrowdControl, category);
    }

    #endregion

    #region Archetype Modifier Tests

    [Test]
    public async Task ScoreAbility_AggressiveArchetype_BoostsDamageAbilities()
    {
        // Arrange
        var damageAbility = new Ability
        {
            Name = "HeavyStrike",
            Type = AbilityType.Attack,
            DamageDice = 10
        };

        var aggressiveEnemy = CreateEnemy(AIArchetype.Aggressive);
        var target = CreatePlayer("Target");
        var state = CreateBattlefieldState();

        // Act
        var score = await _abilityService.ScoreAbilityAsync(damageAbility, aggressiveEnemy, target, state);

        // Assert
        Assert.Greater(score.ArchetypeModifier, 1.0f,
            "Aggressive archetype should boost damage abilities");
        Assert.Greater(score.TotalScore, 20f,
            "Aggressive should give high total score to damage abilities");
    }

    [Test]
    public async Task ScoreAbility_SupportArchetype_BoostsDefensiveAbilities()
    {
        // Arrange
        var healAbility = new Ability
        {
            Name = "Heal",
            Type = AbilityType.Defense,
            DefensePercent = 30
        };

        var supportEnemy = CreateEnemy(AIArchetype.Support);
        var target = CreatePlayer("Target");
        var state = CreateBattlefieldState();

        // Act
        var score = await _abilityService.ScoreAbilityAsync(healAbility, supportEnemy, target, state);

        // Assert
        Assert.Greater(score.ArchetypeModifier, 1.0f,
            "Support archetype should boost defensive abilities");
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task SelectOptimalAbility_NoAbilitiesAvailable_ReturnsBasicAttack()
    {
        // Arrange: Enemy with no available abilities
        var enemy = CreateEnemy(AIArchetype.Tactical);
        var target = CreatePlayer("Target");
        var state = CreateBattlefieldState();

        // Act
        var selected = await _abilityService.SelectOptimalAbilityAsync(enemy, target, state);

        // Assert
        Assert.IsNotNull(selected);
        var actionName = selected.ToString() ?? "";
        Assert.That(actionName.Contains("BasicAttack") || actionName.Contains("Basic"),
            "Should fallback to basic attack when no abilities available");
    }

    #endregion

    #region Helper Methods

    private Enemy CreateEnemy(AIArchetype archetype, int hp = 100, int maxHP = 100)
    {
        return new Enemy
        {
            Id = System.Guid.NewGuid().ToString(),
            Name = $"Enemy_{archetype}",
            Type = EnemyType.CorruptedServitor,
            AIArchetype = archetype,
            HP = hp,
            MaxHP = maxHP,
            BaseDamageDice = 2,
            DamageBonus = 5,
            Attributes = new Attributes { Might = 10, Finesse = 10, Sturdiness = 10 },
            StatusEffects = new List<StatusEffect>()
        };
    }

    private PlayerCharacter CreatePlayer(string name, int hp = 100)
    {
        return new PlayerCharacter
        {
            Id = System.Guid.NewGuid(),
            Name = name,
            HP = hp,
            MaxHP = 100,
            Attributes = new Attributes { Might = 10, Finesse = 10, Sturdiness = 10 },
            StatusEffects = new List<StatusEffect>()
        };
    }

    private BattlefieldState CreateBattlefieldState()
    {
        return new BattlefieldState
        {
            PlayerCharacters = new List<PlayerCharacter>(),
            Enemies = new List<Enemy>(),
            Grid = null,
            CurrentTurn = 1,
            SessionId = System.Guid.NewGuid(),
            EncounterId = System.Guid.NewGuid()
        };
    }

    #endregion
}
