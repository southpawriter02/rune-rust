using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using RuneAndRust.Engine.AI;
using RuneAndRust.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Tests;

/// <summary>
/// Unit tests for v0.42.1: Tactical AI Services
/// Tests threat assessment, target selection, and situational analysis
/// </summary>
[TestFixture]
public class TacticalAIServicesTests
{
    private IThreatAssessmentService _threatService = null!;
    private ITargetSelectionService _targetService = null!;
    private ISituationalAnalysisService _situationService = null!;
    private IBehaviorPatternService _behaviorService = null!;
    private IAIConfigurationRepository _configRepo = null!;

    [SetUp]
    public void Setup()
    {
        // Initialize repository with in-memory database
        _configRepo = new AIConfigurationRepository(Path.GetTempPath());

        // Initialize services
        _situationService = new SituationalAnalysisService(NullLogger<SituationalAnalysisService>.Instance);
        _threatService = new ThreatAssessmentService(
            NullLogger<ThreatAssessmentService>.Instance,
            _configRepo);
        _behaviorService = new BehaviorPatternService(NullLogger<BehaviorPatternService>.Instance);
        _targetService = new TargetSelectionService(
            NullLogger<TargetSelectionService>.Instance,
            _threatService,
            _behaviorService);
    }

    #region Threat Assessment Tests

    [Test]
    public async Task ThreatAssessment_AggressiveArchetype_PrioritizesDamageOutput()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Aggressive);
        var highDamagePlayer = CreatePlayer("HighDamage", hp: 100, might: 15); // High damage
        var lowHPPlayer = CreatePlayer("LowHP", hp: 20, might: 5); // Low HP but low damage

        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { highDamagePlayer, lowHPPlayer },
            enemies: new List<Enemy> { enemy });

        // Act
        var threat1 = await _threatService.AssessThreatAsync(enemy, highDamagePlayer, state);
        var threat2 = await _threatService.AssessThreatAsync(enemy, lowHPPlayer, state);

        // Assert
        Assert.Greater(threat1.TotalThreatScore, threat2.TotalThreatScore,
            "Aggressive archetype should prioritize high damage target over low HP target");
        Assert.AreEqual(AIArchetype.Aggressive, threat1.AssessorArchetype);
    }

    [Test]
    public async Task ThreatAssessment_SupportArchetype_PrioritizesLowHP()
    {
        // Arrange
        var supportEnemy = CreateEnemy(AIArchetype.Support);
        var healthyAlly = CreateEnemy(AIArchetype.Aggressive, hp: 100, maxHP: 100);
        var woundedAlly = CreateEnemy(AIArchetype.Aggressive, hp: 20, maxHP: 100);

        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter>(),
            enemies: new List<Enemy> { supportEnemy, healthyAlly, woundedAlly });

        // Act
        var threat1 = await _threatService.AssessThreatAsync(supportEnemy, healthyAlly, state);
        var threat2 = await _threatService.AssessThreatAsync(supportEnemy, woundedAlly, state);

        // Assert
        Assert.Greater(threat2.TotalThreatScore, threat1.TotalThreatScore,
            "Support archetype should prioritize wounded ally for healing");
    }

    [Test]
    public async Task ThreatAssessment_CalculatesAllFactors()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Tactical);
        var player = CreatePlayer("TestPlayer");
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { player },
            enemies: new List<Enemy> { enemy });

        // Act
        var assessment = await _threatService.AssessThreatAsync(enemy, player, state);

        // Assert
        Assert.IsNotNull(assessment);
        Assert.IsNotNull(assessment.FactorScores);
        Assert.That(assessment.FactorScores.Keys, Has.Member(ThreatFactor.DamageOutput));
        Assert.That(assessment.FactorScores.Keys, Has.Member(ThreatFactor.CurrentHP));
        Assert.That(assessment.FactorScores.Keys, Has.Member(ThreatFactor.Positioning));
        Assert.That(assessment.FactorScores.Keys, Has.Member(ThreatFactor.Abilities));
        Assert.That(assessment.FactorScores.Keys, Has.Member(ThreatFactor.StatusEffects));
        Assert.IsNotEmpty(assessment.Reasoning);
    }

    #endregion

    #region Target Selection Tests

    [Test]
    public async Task TargetSelection_ChoosesHighestThreat()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Tactical);
        var lowThreatPlayer = CreatePlayer("LowThreat", hp: 100, might: 5);
        var highThreatPlayer = CreatePlayer("HighThreat", hp: 100, might: 15);
        var mediumThreatPlayer = CreatePlayer("MediumThreat", hp: 100, might: 10);

        var targets = new List<object> { lowThreatPlayer, highThreatPlayer, mediumThreatPlayer };
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { lowThreatPlayer, highThreatPlayer, mediumThreatPlayer },
            enemies: new List<Enemy> { enemy });

        // Act
        var selected = await _targetService.SelectTargetAsync(enemy, targets, state);

        // Assert
        Assert.IsNotNull(selected);
        var selectedPlayer = selected as PlayerCharacter;
        Assert.IsNotNull(selectedPlayer);
        Assert.AreEqual("HighThreat", selectedPlayer!.Name,
            "Should select the highest threat target");
    }

    [Test]
    public async Task TargetSelection_FiltersDeadTargets()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Tactical);
        var alivePlayer = CreatePlayer("Alive", hp: 50);
        var deadPlayer = CreatePlayer("Dead", hp: 0); // Dead

        var targets = new List<object> { alivePlayer, deadPlayer };
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { alivePlayer, deadPlayer },
            enemies: new List<Enemy> { enemy });

        // Act
        var selected = await _targetService.SelectTargetAsync(enemy, targets, state);

        // Assert
        Assert.IsNotNull(selected);
        var selectedPlayer = selected as PlayerCharacter;
        Assert.AreEqual("Alive", selectedPlayer!.Name,
            "Should not select dead targets");
    }

    [Test]
    public async Task TargetSelection_RecklessArchetype_IgnoresPositioning()
    {
        // Arrange
        var recklessEnemy = CreateEnemy(AIArchetype.Reckless);
        var player = CreatePlayer("TestPlayer");

        var targets = new List<object> { player };
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { player },
            enemies: new List<Enemy> { recklessEnemy });

        // Act
        var selected = await _targetService.SelectTargetAsync(recklessEnemy, targets, state);

        // Assert
        Assert.IsNotNull(selected, "Reckless should select targets regardless of positioning");
    }

    [Test]
    public async Task SelectHealTarget_SelectsLowestHPAlly()
    {
        // Arrange
        var healer = CreateEnemy(AIArchetype.Support, hp: 100, maxHP: 100);
        var healthyAlly = CreateEnemy(AIArchetype.Aggressive, hp: 90, maxHP: 100);
        var woundedAlly = CreateEnemy(AIArchetype.Aggressive, hp: 30, maxHP: 100);
        var criticalAlly = CreateEnemy(AIArchetype.Aggressive, hp: 10, maxHP: 100);

        var allies = new List<Enemy> { healer, healthyAlly, woundedAlly, criticalAlly };
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter>(),
            enemies: allies);

        // Act
        var selected = await _targetService.SelectHealTargetAsync(healer, allies, state);

        // Assert
        Assert.IsNotNull(selected);
        Assert.AreEqual(10, selected!.HP, "Should select the most critically wounded ally");
    }

    #endregion

    #region Situational Analysis Tests

    [Test]
    public void SituationalAnalysis_DetectsOutnumbered()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Tactical);
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter>
            {
                CreatePlayer("P1"),
                CreatePlayer("P2"),
                CreatePlayer("P3")
            },
            enemies: new List<Enemy> { enemy }); // 1 vs 3

        // Act
        var context = _situationService.AnalyzeSituation(enemy, state);

        // Assert
        Assert.IsTrue(context.IsOutnumbered, "Should detect being outnumbered (1 vs 3)");
        Assert.AreEqual(1, context.AllyCount);
        Assert.AreEqual(3, context.EnemyCount);
    }

    [Test]
    public void SituationalAnalysis_DetectsLowHP()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Tactical, hp: 20, maxHP: 100); // 20% HP
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { CreatePlayer("P1") },
            enemies: new List<Enemy> { enemy });

        // Act
        var context = _situationService.AnalyzeSituation(enemy, state);

        // Assert
        Assert.IsTrue(context.IsLowHP, "Should detect low HP (20%)");
        Assert.AreEqual(0.2f, context.SelfHPPercent, 0.01f);
    }

    [Test]
    public void SituationalAnalysis_DetectsCriticalHP()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Tactical, hp: 10, maxHP: 100); // 10% HP
        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { CreatePlayer("P1") },
            enemies: new List<Enemy> { enemy });

        // Act
        var context = _situationService.AnalyzeSituation(enemy, state);

        // Assert
        Assert.IsTrue(context.IsCriticalHP, "Should detect critical HP (10%)");
        Assert.IsTrue(context.IsLowHP, "Critical HP should also be low HP");
    }

    [Test]
    public void CalculateAdvantage_StrongAdvantage()
    {
        // Arrange
        var context = new SituationalContext
        {
            AllyCount = 3,
            EnemyCount = 1, // Outnumbering
            SelfHPPercent = 0.9f, // Healthy
            HasHighGround = true, // Positional advantage
            IsInCover = true
        };

        // Act
        var advantage = _situationService.CalculateAdvantage(context);

        // Assert
        Assert.AreEqual(TacticalAdvantage.Strong, advantage,
            "Should calculate strong advantage (outnumbering + healthy + good position)");
    }

    [Test]
    public void CalculateAdvantage_Disadvantaged()
    {
        // Arrange
        var context = new SituationalContext
        {
            AllyCount = 1,
            EnemyCount = 3, // Outnumbered
            IsOutnumbered = true,
            SelfHPPercent = 0.2f, // Low HP
            IsLowHP = true,
            IsFlanked = true // Poor position
        };

        // Act
        var advantage = _situationService.CalculateAdvantage(context);

        // Assert
        Assert.AreEqual(TacticalAdvantage.Disadvantaged, advantage,
            "Should calculate disadvantaged (outnumbered + low HP + flanked)");
    }

    #endregion

    #region Behavior Pattern Tests

    [Test]
    public async Task BehaviorPattern_GetsArchetype()
    {
        // Arrange
        var enemy = CreateEnemy(AIArchetype.Aggressive);

        // Act
        var archetype = await _behaviorService.GetArchetypeAsync(enemy);

        // Assert
        Assert.AreEqual(AIArchetype.Aggressive, archetype);
    }

    [Test]
    public void BehaviorPattern_GetDefaultArchetype_ReturnsCorrectType()
    {
        // Arrange & Act
        var servitorArchetype = _behaviorService.GetDefaultArchetype(EnemyType.CorruptedServitor);
        var wardenArchetype = _behaviorService.GetDefaultArchetype(EnemyType.RuinWarden);
        var scholarArchetype = _behaviorService.GetDefaultArchetype(EnemyType.ForlornScholar);

        // Assert
        Assert.AreEqual(AIArchetype.Reckless, servitorArchetype, "Servitors should be Reckless");
        Assert.AreEqual(AIArchetype.Defensive, wardenArchetype, "Wardens should be Defensive");
        Assert.AreEqual(AIArchetype.Control, scholarArchetype, "Scholars should be Control");
    }

    [Test]
    public void BehaviorPattern_ArchetypeOverride_CriticalHP()
    {
        // Arrange
        var aggressiveEnemy = CreateEnemy(AIArchetype.Aggressive, hp: 10, maxHP: 100);
        var situation = new SituationalContext
        {
            IsCriticalHP = true,
            SelfHPPercent = 0.1f
        };

        // Act
        var override_archetype = _behaviorService.GetArchetypeOverride(aggressiveEnemy, situation);

        // Assert
        Assert.AreEqual(AIArchetype.Reckless, override_archetype,
            "Critical HP should trigger desperate Reckless behavior");
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task Integration_CompleteTargetSelectionFlow()
    {
        // Arrange: 3 player party vs 2 enemies
        var aggressiveEnemy = CreateEnemy(AIArchetype.Aggressive);
        var tacticalEnemy = CreateEnemy(AIArchetype.Tactical);

        var tank = CreatePlayer("Tank", hp: 100, might: 8); // Low damage
        var striker = CreatePlayer("Striker", hp: 80, might: 15); // High damage
        var healer = CreatePlayer("Healer", hp: 60, might: 5); // Low damage, low HP

        var state = CreateBattlefieldState(
            players: new List<PlayerCharacter> { tank, striker, healer },
            enemies: new List<Enemy> { aggressiveEnemy, tacticalEnemy });

        // Act
        var aggressiveTarget = await _targetService.SelectTargetAsync(
            aggressiveEnemy,
            new List<object> { tank, striker, healer },
            state);

        var tacticalTarget = await _targetService.SelectTargetAsync(
            tacticalEnemy,
            new List<object> { tank, striker, healer },
            state);

        // Assert
        Assert.IsNotNull(aggressiveTarget, "Aggressive enemy should select a target");
        Assert.IsNotNull(tacticalTarget, "Tactical enemy should select a target");

        // Aggressive should prioritize high damage (Striker)
        var aggressiveSelected = aggressiveTarget as PlayerCharacter;
        Assert.IsTrue(
            aggressiveSelected!.Name == "Striker" || aggressiveSelected.Name == "Tank",
            "Aggressive should prioritize Striker (high damage) or Tank");
    }

    #endregion

    #region Helper Methods

    private Enemy CreateEnemy(AIArchetype archetype, int hp = 100, int maxHP = 100)
    {
        return new Enemy
        {
            Id = Guid.NewGuid().ToString(),
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

    private PlayerCharacter CreatePlayer(string name, int hp = 100, int might = 10)
    {
        return new PlayerCharacter
        {
            Id = Guid.NewGuid(),
            Name = name,
            HP = hp,
            MaxHP = 100,
            Attributes = new Attributes { Might = might, Finesse = 10, Sturdiness = 10 },
            StatusEffects = new List<StatusEffect>()
        };
    }

    private BattlefieldState CreateBattlefieldState(
        List<PlayerCharacter> players,
        List<Enemy> enemies)
    {
        return new BattlefieldState
        {
            PlayerCharacters = players,
            Enemies = enemies,
            Grid = null, // No grid for basic tests
            CurrentTurn = 1,
            SessionId = Guid.NewGuid(),
            EncounterId = Guid.NewGuid()
        };
    }

    #endregion
}
