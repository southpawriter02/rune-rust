using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.34.2: Test suite for Companion AI Service
/// Validates AI decision-making, stance behavior, target selection, and tactical positioning
/// </summary>
[TestFixture]
public class CompanionAIServiceTests
{
    private CompanionAIService _aiService;
    private CoverService _coverService;
    private FlankingService _flankingService;
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        _coverService = new CoverService();
        _flankingService = new FlankingService();
        _aiService = new CompanionAIService(_coverService, _flankingService, _logger);
    }

    [Test]
    public void SelectTarget_AggressiveStance_PrioritizesWoundedEnemies()
    {
        // Arrange
        var companion = CreateCompanion("Kara", "aggressive");
        var player = CreatePlayer();

        var enemies = new List<Enemy>
        {
            CreateEnemy("Warden A", 50, 100), // Wounded (50%)
            CreateEnemy("Warden B", 100, 100), // Full HP
            CreateEnemy("Warden C", 90, 100)  // Slightly wounded
        };

        // Act
        var target = _aiService.SelectTarget(companion, enemies, player);

        // Assert
        Assert.That(target, Is.Not.Null);
        Assert.That(target!.Name, Is.EqualTo("Warden A"), "Should target most wounded enemy");
        Assert.That(target.HP, Is.EqualTo(50));
    }

    [Test]
    public void SelectTarget_DefensiveStance_PrioritizesThreatsToPlayer()
    {
        // Arrange
        var companion = CreateCompanion("Runa", "defensive");
        var player = CreatePlayer();
        player.Position = new GridPosition(Zone.Player, Row.Front, 2);

        var enemies = new List<Enemy>
        {
            CreateEnemy("Cultist", 80, 100, new GridPosition(Zone.Enemy, Row.Front, 5)), // Far from player
            CreateEnemy("Draugr", 100, 100, new GridPosition(Zone.Player, Row.Front, 3)) // Adjacent to player
        };

        // Act
        var target = _aiService.SelectTarget(companion, enemies, player);

        // Assert
        Assert.That(target, Is.Not.Null);
        Assert.That(target!.Name, Is.EqualTo("Draugr"), "Should target enemy threatening player");
    }

    [Test]
    public void SelectAction_PassiveStance_ReturnsWait()
    {
        // Arrange
        var companion = CreateCompanion("Valdis", "passive");
        var player = CreatePlayer();
        var enemies = new List<Enemy> { CreateEnemy("Warden", 100, 100) };

        // Act
        var action = _aiService.SelectAction(companion, player, enemies);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action.ActionType, Is.EqualTo("Wait"));
        Assert.That(action.Reason, Does.Contain("Passive stance"));
    }

    [Test]
    public void EvaluateThreat_BossEnemy_HighThreat()
    {
        // Arrange
        var bossEnemy = CreateEnemy("Boss Draugr", 200, 200);
        bossEnemy.IsBoss = true;
        bossEnemy.BaseDamageDice = 3;

        var normalEnemy = CreateEnemy("Normal Cultist", 80, 100);
        normalEnemy.BaseDamageDice = 1;

        // Act
        var bossThreat = _aiService.EvaluateThreat(bossEnemy);
        var normalThreat = _aiService.EvaluateThreat(normalEnemy);

        // Assert
        Assert.That(bossThreat, Is.GreaterThan(normalThreat), "Boss should have higher threat");
        Assert.That(bossThreat, Is.GreaterThan(50), "Boss threat should be substantial");
    }

    [Test]
    public void EvaluateThreat_LowHPEnemy_ReducedThreat()
    {
        // Arrange
        var healthyEnemy = CreateEnemy("Healthy Warden", 100, 100);
        healthyEnemy.BaseDamageDice = 2;

        var dyingEnemy = CreateEnemy("Dying Warden", 20, 100); // 20% HP
        dyingEnemy.BaseDamageDice = 2; // Same damage as healthy

        // Act
        var healthyThreat = _aiService.EvaluateThreat(healthyEnemy);
        var dyingThreat = _aiService.EvaluateThreat(dyingEnemy);

        // Assert
        Assert.That(dyingThreat, Is.LessThan(healthyThreat), "Low HP enemy should have reduced threat");
    }

    [Test]
    public void ShouldUseAbility_AOEWithClusteredEnemies_UsesAOE()
    {
        // Arrange
        var companion = CreateCompanion("Bjorn", "aggressive");
        companion.CurrentStamina = 30;
        companion.Abilities.Add(new CompanionAbility
        {
            AbilityID = 34302,
            AbilityName = "Scrap Grenade",
            TargetType = "area_2x2",
            ResourceCostType = "Stamina",
            ResourceCost = 25,
            RangeType = "ranged"
        });

        var target = CreateEnemy("Cultist A", 80, 100, new GridPosition(Zone.Enemy, Row.Front, 3));
        var enemies = new List<Enemy>
        {
            target,
            CreateEnemy("Cultist B", 80, 100, new GridPosition(Zone.Enemy, Row.Front, 4)), // Adjacent column
            CreateEnemy("Cultist C", 80, 100, new GridPosition(Zone.Enemy, Row.Back, 3))  // Adjacent row
        };

        // Act
        var action = _aiService.ShouldUseAbility(companion, target, enemies);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action!.ActionType, Is.EqualTo("UseAbility"));
        Assert.That(action.AbilityName, Is.EqualTo("Scrap Grenade"));
        Assert.That(action.Reason, Does.Contain("AOE"));
    }

    [Test]
    public void ShouldUseAbility_InsufficientStamina_ReturnsNull()
    {
        // Arrange
        var companion = CreateCompanion("Kara", "aggressive");
        companion.CurrentStamina = 5; // Not enough for Shield Bash
        companion.Abilities.Add(new CompanionAbility
        {
            AbilityID = 34101,
            AbilityName = "Shield Bash",
            ResourceCostType = "Stamina",
            ResourceCost = 10,
            RangeType = "melee"
        });

        var target = CreateEnemy("Warden", 100, 100);
        var enemies = new List<Enemy> { target };

        // Act
        var action = _aiService.ShouldUseAbility(companion, target, enemies);

        // Assert
        Assert.That(action, Is.Null, "Should return null when insufficient stamina");
    }

    [Test]
    public void SelectAction_LowHP_RetreatsToSafety()
    {
        // Arrange
        var companion = CreateCompanion("Valdis", "defensive");
        companion.CurrentHitPoints = 12; // 25% of 50 max
        companion.MaxHitPoints = 50;
        companion.Position = new GridPosition(Zone.Player, Row.Front, 3);

        var player = CreatePlayer();
        player.Position = new GridPosition(Zone.Player, Row.Back, 2);

        var enemies = new List<Enemy>
        {
            CreateEnemy("Draugr", 100, 100, new GridPosition(Zone.Player, Row.Front, 4)) // Adjacent to companion
        };

        // Act
        var action = _aiService.SelectAction(companion, player, enemies);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action.ActionType, Is.EqualTo("Move"));
        Assert.That(action.Reason, Does.Contain("Retreat"));
    }

    [Test]
    public void ShouldUseAbility_HealingWhenLowHP_UsesHeal()
    {
        // Arrange
        var companion = CreateCompanion("Bjorn", "defensive");
        companion.CurrentHitPoints = 15; // 40% of 40 max
        companion.MaxHitPoints = 40;
        companion.CurrentStamina = 25;
        companion.Abilities.Add(new CompanionAbility
        {
            AbilityID = 34301,
            AbilityName = "Improvised Repair",
            ResourceCostType = "Stamina",
            ResourceCost = 20,
            DamageType = "Healing",
            RangeType = "ranged"
        });

        var target = CreateEnemy("Warden", 100, 100);
        var enemies = new List<Enemy> { target };

        // Act
        var action = _aiService.ShouldUseAbility(companion, target, enemies);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action!.ActionType, Is.EqualTo("UseAbility"));
        Assert.That(action.AbilityName, Is.EqualTo("Improvised Repair"));
        Assert.That(action.TargetSelf, Is.True);
        Assert.That(action.Reason, Does.Contain("heal"));
    }

    [Test]
    public void SelectAction_NoValidTargets_ReturnsWait()
    {
        // Arrange
        var companion = CreateCompanion("Kara", "aggressive");
        var player = CreatePlayer();
        var enemies = new List<Enemy>(); // No enemies

        // Act
        var action = _aiService.SelectAction(companion, player, enemies);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action.ActionType, Is.EqualTo("Wait"));
        Assert.That(action.Reason, Does.Contain("No valid targets"));
    }

    [Test]
    public void SelectTarget_MultipleWoundedEnemies_TargetsMostWounded()
    {
        // Arrange
        var companion = CreateCompanion("Einar", "aggressive");
        var player = CreatePlayer();

        var enemies = new List<Enemy>
        {
            CreateEnemy("Cultist A", 40, 100), // 40% HP
            CreateEnemy("Cultist B", 20, 100), // 20% HP - most wounded
            CreateEnemy("Cultist C", 35, 100)  // 35% HP
        };

        // Act
        var target = _aiService.SelectTarget(companion, enemies, player);

        // Assert
        Assert.That(target, Is.Not.Null);
        Assert.That(target!.Name, Is.EqualTo("Cultist B"), "Should target enemy with lowest HP");
        Assert.That(target.HP, Is.EqualTo(20));
    }

    [Test]
    public void ShouldUseAbility_HighDamageOnHighThreat_UsesHighDamage()
    {
        // Arrange
        var companion = CreateCompanion("Valdis", "aggressive");
        companion.CurrentAether = 50;
        companion.Abilities.Add(new CompanionAbility
        {
            AbilityID = 34401,
            AbilityName = "Spirit Bolt",
            Description = "Unleash psychic blast dealing 3d6 + WILL damage.",
            ResourceCostType = "Aether Pool",
            ResourceCost = 30,
            RangeType = "ranged"
        });

        var highThreatEnemy = CreateEnemy("Elite Draugr", 150, 150);
        highThreatEnemy.IsBoss = true;
        highThreatEnemy.BaseDamageDice = 3;

        var enemies = new List<Enemy> { highThreatEnemy };

        // Act
        var action = _aiService.ShouldUseAbility(companion, highThreatEnemy, enemies);

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action!.ActionType, Is.EqualTo("UseAbility"));
        Assert.That(action.AbilityName, Is.EqualTo("Spirit Bolt"));
        Assert.That(action.Reason, Does.Contain("high-threat"));
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private Companion CreateCompanion(string name, string stance)
    {
        return new Companion
        {
            CompanionID = 1,
            CompanionName = name,
            DisplayName = name,
            CurrentStance = stance,
            CurrentHitPoints = 50,
            MaxHitPoints = 50,
            CurrentStamina = 20,
            MaxStamina = 20,
            CurrentAether = 10,
            MaxAether = 10,
            MovementRange = 3,
            Abilities = new List<CompanionAbility>(),
            Position = new GridPosition(Zone.Player, Row.Front, 1),
            ResourceType = "Stamina"
        };
    }

    private PlayerCharacter CreatePlayer()
    {
        return new PlayerCharacter
        {
            Name = "Player",
            HP = 80,
            MaxHP = 100,
            Position = new GridPosition(Zone.Player, Row.Front, 2)
        };
    }

    private Enemy CreateEnemy(string name, int hp, int maxHp, GridPosition? position = null)
    {
        return new Enemy
        {
            Name = name,
            HP = hp,
            MaxHP = maxHp,
            BaseDamageDice = 1,
            DamageBonus = 0,
            Position = position ?? new GridPosition(Zone.Enemy, Row.Front, 5),
            IsBoss = false,
            IsChampion = false,
            IsForlorn = false
        };
    }
}
