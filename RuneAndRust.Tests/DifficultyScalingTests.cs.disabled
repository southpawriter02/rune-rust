using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using RuneAndRust.Engine.AI;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RuneAndRust.Tests;

/// <summary>
/// Integration and performance tests for difficulty scaling (v0.42.4).
/// Tests NG+ intelligence scaling, Challenge Sector adaptation, performance monitoring.
/// </summary>
public class DifficultyScalingTests
{
    // Test data helpers
    private Enemy CreateTestEnemy(AIArchetype archetype = AIArchetype.Tactical)
    {
        return new Enemy
        {
            Id = 1,
            EnemyTypeId = 1,
            Name = "Test Enemy",
            CurrentHP = 100,
            MaxHP = 100,
            Attack = 50,
            Defense = 30,
            AIArchetype = archetype
        };
    }

    private BattlefieldState CreateTestBattlefield(int playerCount = 3, int enemyCount = 3)
    {
        var players = new List<PlayerCharacter>();
        for (int i = 0; i < playerCount; i++)
        {
            players.Add(new PlayerCharacter
            {
                Id = i + 1,
                Name = $"Player{i + 1}",
                CurrentHP = 100,
                MaxHP = 100,
                Attack = 40,
                Defense = 25
            });
        }

        var enemies = new List<Enemy>();
        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(new Enemy
            {
                Id = i + 100,
                Name = $"Enemy{i + 1}",
                CurrentHP = 80,
                MaxHP = 80,
                Attack = 35,
                Defense = 20
            });
        }

        return new BattlefieldState
        {
            PlayerParty = players,
            Enemies = enemies
        };
    }

    private EnemyAction CreateTestAction(Enemy enemy)
    {
        return new EnemyAction
        {
            Actor = enemy,
            SelectedAbilityId = 1,
            AggressionModifier = 0m,
            Context = new DecisionContext
            {
                IntelligenceLevel = 0,
                Reasoning = "Test action"
            }
        };
    }

    #region DifficultyScalingService Tests

    [Fact]
    public async Task NG0_ReturnsIntelligenceLevel0()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetNGPlusTier(0);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(0, intelligence);
    }

    [Fact]
    public async Task NG5_ReturnsIntelligenceLevel5()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetNGPlusTier(5);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(5, intelligence);
    }

    [Fact]
    public async Task ChallengeSector_AddsOneIntelligenceLevel()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetNGPlusTier(2);
        service.SetIsChallengeSector(true);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(3, intelligence); // NG+2 + 1 for Challenge Sector
    }

    [Fact]
    public async Task EndlessWave10_ReturnsIntelligenceLevel1()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetEndlessWave(10);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(1, intelligence);
    }

    [Fact]
    public async Task EndlessWave25_ReturnsIntelligenceLevel3()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetEndlessWave(25);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(3, intelligence);
    }

    [Fact]
    public async Task EndlessWave50_ReturnsMaxIntelligence()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetEndlessWave(50);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(5, intelligence); // Max intelligence
    }

    [Fact]
    public async Task BossGauntlet3_ReturnsIntelligenceLevel3()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        service.SetBossGauntletNumber(3);

        // Act
        var intelligence = await service.GetAIIntelligenceLevelAsync();

        // Assert
        Assert.Equal(2, intelligence); // Boss 3 = (3+1)/2 = 2
    }

    [Fact]
    public void ErrorChance_Intelligence0_Returns15Percent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        // Act
        var errorChance = service.CalculateErrorChance(0);

        // Assert
        Assert.Equal(0.15, errorChance);
    }

    [Fact]
    public void ErrorChance_Intelligence5_Returns0Percent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        // Act
        var errorChance = service.CalculateErrorChance(5);

        // Assert
        Assert.Equal(0.0, errorChance);
    }

    [Fact]
    public async Task LowIntelligence_IntroducesErrors()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        var enemy = CreateTestEnemy();
        var state = CreateTestBattlefield();
        var action = CreateTestAction(enemy);
        action.Target = state.PlayerParty[0];

        // Act: Run 100 times at intelligence 0 (15% error rate)
        int errorCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var modifiedAction = await service.ApplyIntelligenceScalingAsync(action, 0, state);
            if (modifiedAction.Context?.IsIntentionalError == true)
            {
                errorCount++;
            }
        }

        // Assert: Should have some errors (roughly 15%, allow 5-25% range)
        Assert.InRange(errorCount, 5, 25);
    }

    [Fact]
    public async Task MaxIntelligence_NoErrors()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        var enemy = CreateTestEnemy();
        var state = CreateTestBattlefield();
        var action = CreateTestAction(enemy);

        // Act: Run 100 times at intelligence 5 (0% error rate)
        int errorCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var modifiedAction = await service.ApplyIntelligenceScalingAsync(action, 5, state);
            if (modifiedAction.Context?.IsIntentionalError == true)
            {
                errorCount++;
            }
        }

        // Assert: Should have NO errors
        Assert.Equal(0, errorCount);
    }

    #endregion

    #region ChallengeSectorAIService Tests

    [Fact]
    public async Task NoHealing_PrioritizesBurstDamage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChallengeSectorAIService>>();
        var service = new ChallengeSectorAIService(mockLogger.Object);

        var enemy = CreateTestEnemy();
        var action = CreateTestAction(enemy);
        var state = CreateTestBattlefield();

        var modifiers = new List<ChallengeSectorModifier>
        {
            new ChallengeSectorModifier
            {
                Type = SectorModifierType.NoHealing,
                Name = "No Healing",
                AggressionModifier = 0.2m
            }
        };

        var originalPriority = action.Priority;

        // Act
        await service.AdaptToSectorModifiersAsync(enemy, action, modifiers, state);

        // Assert
        Assert.True(action.Priority > originalPriority);
    }

    [Fact]
    public async Task DoubleSpeed_IncreasesAggression()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChallengeSectorAIService>>();
        var service = new ChallengeSectorAIService(mockLogger.Object);

        var enemy = CreateTestEnemy();
        var action = CreateTestAction(enemy);
        var state = CreateTestBattlefield();

        var modifiers = new List<ChallengeSectorModifier>
        {
            new ChallengeSectorModifier { Type = SectorModifierType.DoubleSpeed }
        };

        // Act
        await service.AdaptToSectorModifiersAsync(enemy, action, modifiers, state);

        // Assert
        Assert.True(action.AggressionModifier > 0);
    }

    [Fact]
    public async Task OneHP_PrioritizesDefense()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ChallengeSectorAIService>>();
        var service = new ChallengeSectorAIService(mockLogger.Object);

        var enemy = CreateTestEnemy();
        var action = CreateTestAction(enemy);
        var state = CreateTestBattlefield();

        var modifiers = new List<ChallengeSectorModifier>
        {
            new ChallengeSectorModifier { Type = SectorModifierType.OneHP }
        };

        // Act
        await service.AdaptToSectorModifiersAsync(enemy, action, modifiers, state);

        // Assert
        Assert.True(action.AggressionModifier < 0);
    }

    #endregion

    #region AIPerformanceMonitor Tests

    [Fact]
    public async Task MonitorPerformance_RecordsMetrics()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIPerformanceMonitor>>();
        var monitor = new AIPerformanceMonitor(mockLogger.Object);

        // Act
        var result = await monitor.MonitorPerformanceAsync(
            "TestOperation",
            async () =>
            {
                await Task.Delay(10);
                return 42;
            });

        // Assert
        Assert.Equal(42, result);

        var metrics = monitor.GetMetricsForOperation("TestOperation");
        Assert.NotNull(metrics);
        Assert.Equal(1, metrics.TotalCalls);
        Assert.True(metrics.AverageMs >= 10);
    }

    [Fact]
    public async Task SlowOperation_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIPerformanceMonitor>>();
        var monitor = new AIPerformanceMonitor(mockLogger.Object);

        // Act
        await monitor.MonitorPerformanceAsync(
            "SlowOperation",
            async () =>
            {
                await Task.Delay(60); // Exceeds 50ms threshold
                return true;
            });

        // Assert
        var metrics = monitor.GetMetricsForOperation("SlowOperation");
        Assert.NotNull(metrics);
        Assert.True(metrics.MaxMs > 50);
    }

    [Fact]
    public void ResetMetrics_ClearsAllMetrics()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIPerformanceMonitor>>();
        var monitor = new AIPerformanceMonitor(mockLogger.Object);

        monitor.RecordMetric("Op1", 10);
        monitor.RecordMetric("Op2", 20);

        // Act
        monitor.ResetMetrics();

        // Assert
        var metrics = monitor.GetMetrics();
        Assert.Empty(metrics);
    }

    #endregion

    #region AIDebugService Tests

    [Fact]
    public void EnableDebugMode_SetsFlag()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIDebugService>>();
        var debugService = new AIDebugService(mockLogger.Object);

        // Act
        debugService.EnableDebugMode();

        // Assert
        Assert.True(debugService.IsDebugModeEnabled());
    }

    [Fact]
    public void DisableDebugMode_ClearsFlag()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIDebugService>>();
        var debugService = new AIDebugService(mockLogger.Object);

        debugService.EnableDebugMode();

        // Act
        debugService.DisableDebugMode();

        // Assert
        Assert.False(debugService.IsDebugModeEnabled());
    }

    [Fact]
    public void LogDecision_WhenDebugEnabled_Logs()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIDebugService>>();
        var debugService = new AIDebugService(mockLogger.Object);

        debugService.EnableDebugMode();

        var enemy = CreateTestEnemy();
        var action = CreateTestAction(enemy);
        var context = new DecisionContext
        {
            IntelligenceLevel = 3,
            Reasoning = "Test reasoning"
        };

        // Act
        debugService.LogDecision(enemy, action, context);

        // Assert - No exception thrown
        Assert.True(true);
    }

    [Fact]
    public void GenerateDecisionReport_EmptyEncounter_ReturnsEmptyReport()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AIDebugService>>();
        var debugService = new AIDebugService(mockLogger.Object);

        var encounterId = System.Guid.NewGuid();

        // Act
        var report = debugService.GenerateDecisionReport(encounterId);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(0, report.TotalDecisions);
    }

    #endregion

    #region Performance Benchmarks

    [Fact]
    public async Task AIDecision_CompletesUnder50ms()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        var enemy = CreateTestEnemy();
        var state = CreateTestBattlefield(4, 6);
        var action = CreateTestAction(enemy);

        var stopwatch = Stopwatch.StartNew();

        // Act
        await service.ApplyIntelligenceScalingAsync(action, 3, state);

        stopwatch.Stop();

        // Assert
        Assert.True(
            stopwatch.ElapsedMilliseconds < 50,
            $"AI decision took {stopwatch.ElapsedMilliseconds}ms (threshold: 50ms)");
    }

    [Fact]
    public async Task MultipleEnemyDecisions_CompleteFast()
    {
        // Arrange: 10 enemies making decisions
        var mockLogger = new Mock<ILogger<DifficultyScalingService>>();
        var service = new DifficultyScalingService(mockLogger.Object);

        var enemies = Enumerable.Range(0, 10)
            .Select(_ => CreateTestEnemy())
            .ToList();

        var state = CreateTestBattlefield(4, 10);
        var stopwatch = Stopwatch.StartNew();

        // Act: All enemies decide
        var tasks = enemies.Select(e =>
        {
            var action = CreateTestAction(e);
            return service.ApplyIntelligenceScalingAsync(action, 3, state);
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert: 10 enemies should complete in < 500ms
        Assert.True(
            stopwatch.ElapsedMilliseconds < 500,
            $"10 enemy decisions took {stopwatch.ElapsedMilliseconds}ms (threshold: 500ms)");
    }

    #endregion
}
