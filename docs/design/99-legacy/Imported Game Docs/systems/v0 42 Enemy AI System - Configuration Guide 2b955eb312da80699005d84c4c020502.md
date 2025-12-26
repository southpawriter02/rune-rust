# v0.42 Enemy AI System - Configuration Guide

**Version:** v0.42 Complete
**Date:** 2025-11-24
**System:** Enemy AI Improvements & Behavior Polish

---

## Table of Contents

1. [Dependency Injection Setup](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
2. [Service Configuration](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
3. [Database Initialization](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
4. [Difficulty Configuration](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
5. [Boss Configuration](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
6. [Archetype Tuning](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
7. [Performance Monitoring](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)
8. [Debug Mode](v0%2042%20Enemy%20AI%20System%20-%20Configuration%20Guide%202b955eb312da80699005d84c4c020502.md)

---

## Dependency Injection Setup

### Step 1: Register Repository

```csharp
services.AddSingleton<IAIConfigurationRepository>(sp =>
{
    var dataDirectory = configuration["DataDirectory"] ?? Environment.CurrentDirectory;
    return new AIConfigurationRepository(dataDirectory);
});

```

### Step 2: Register v0.42.1 Services (Tactical AI)

```csharp
// Threat assessment
services.AddScoped<IThreatAssessmentService, ThreatAssessmentService>();

// Target selection
services.AddScoped<ITargetSelectionService, TargetSelectionService>();

// Situational analysis
services.AddScoped<ISituationalAnalysisService, SituationalAnalysisService>();

// Behavior patterns
services.AddScoped<IBehaviorPatternService, BehaviorPatternService>();

```

### Step 3: Register v0.42.2 Services (Ability Usage)

```csharp
// Ability prioritization
services.AddScoped<IAbilityPrioritizationService, AbilityPrioritizationService>();

```

### Step 4: Register v0.42.3 Services (Boss AI)

```csharp
// Boss AI
services.AddScoped<IBossAIService, BossAIService>();

// Ability rotations
services.AddScoped<IAbilityRotationService, AbilityRotationService>();

// Add management
services.AddScoped<IAddManagementService, AddManagementService>();

// Adaptive difficulty
services.AddScoped<IAdaptiveDifficultyService, AdaptiveDifficultyService>();

```

### Step 5: Register v0.42.4 Services (Difficulty Scaling)

```csharp
// Difficulty scaling
services.AddSingleton<IDifficultyScalingService, DifficultyScalingService>();

// Challenge Sector AI
services.AddScoped<IChallengeSectorAIService, ChallengeSectorAIService>();

// Performance monitoring
services.AddSingleton<IAIPerformanceMonitor, AIPerformanceMonitor>();

// Debug service
services.AddSingleton<IAIDebugService, AIDebugService>();

```

### Step 6: Register Orchestrator

```csharp
services.AddScoped<EnemyAIOrchestrator>();

```

### Complete Example

```csharp
public static class AIServiceExtensions
{
    public static IServiceCollection AddEnemyAIServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Repository
        services.AddSingleton<IAIConfigurationRepository>(sp =>
        {
            var dataDirectory = configuration["DataDirectory"] ?? Environment.CurrentDirectory;
            return new AIConfigurationRepository(dataDirectory);
        });

        // v0.42.1: Tactical AI
        services.AddScoped<IThreatAssessmentService, ThreatAssessmentService>();
        services.AddScoped<ITargetSelectionService, TargetSelectionService>();
        services.AddScoped<ISituationalAnalysisService, SituationalAnalysisService>();
        services.AddScoped<IBehaviorPatternService, BehaviorPatternService>();

        // v0.42.2: Ability Usage
        services.AddScoped<IAbilityPrioritizationService, AbilityPrioritizationService>();

        // v0.42.3: Boss AI
        services.AddScoped<IBossAIService, BossAIService>();
        services.AddScoped<IAbilityRotationService, AbilityRotationService>();
        services.AddScoped<IAddManagementService, AddManagementService>();
        services.AddScoped<IAdaptiveDifficultyService, AdaptiveDifficultyService>();

        // v0.42.4: Difficulty Scaling
        services.AddSingleton<IDifficultyScalingService, DifficultyScalingService>();
        services.AddScoped<IChallengeSectorAIService, ChallengeSectorAIService>();
        services.AddSingleton<IAIPerformanceMonitor, AIPerformanceMonitor>();
        services.AddSingleton<IAIDebugService, AIDebugService>();

        // Orchestrator
        services.AddScoped<EnemyAIOrchestrator>();

        return services;
    }
}

// Usage in Startup.cs or Program.cs:
services.AddEnemyAIServices(configuration);

```

---

## Service Configuration

### Integrating with EnemyAI

### Option 1: Enable for All Enemies

```csharp
// In your combat initialization
var orchestrator = serviceProvider.GetRequiredService<EnemyAIOrchestrator>();
var logger = serviceProvider.GetRequiredService<ILogger<EnemyAI>>();

var enemyAI = new EnemyAI(diceService, statusEffectService, counterAttackService, flavorTextService);
enemyAI.EnableV042Orchestrator(orchestrator, logger);

```

### Option 2: Selective Enablement

```csharp
// Only enable for specific enemy types
if (enemy.Type == EnemyType.AethericAberration || enemy.IsBoss)
{
    enemyAI.EnableV042Orchestrator(orchestrator, logger);
}

```

### Option 3: Gradual Rollout

```csharp
// Enable based on NG+ tier
if (gameState.NGPlusTier >= 2)
{
    enemyAI.EnableV042Orchestrator(orchestrator, logger);
}

```

### Making AI Decisions

### With Orchestrator (Async)

```csharp
// Create battlefield state
var state = new BattlefieldState
{
    PlayerParty = players,
    Enemies = enemies
};

// Get action from orchestrator
var action = await enemyAI.DetermineActionV042Async(
    enemy,
    state,
    isBoss: enemy.IsBoss,
    challengeModifiers: activeSectorModifiers);

// Execute the action
var target = action.Target as PlayerCharacter;
var abilityId = action.SelectedAbilityId;

// action.Context contains decision reasoning
Console.WriteLine($"AI Reasoning: {action.Context?.Reasoning}");

```

### Without Orchestrator (Legacy)

```csharp
// Legacy synchronous API still works
var legacyAction = enemyAI.DetermineAction(enemy);

```

---

## Database Initialization

The database is automatically initialized on first use. Tables created:

### v0.42.1 Tables

- `AIThreatWeights` - Archetype-specific threat assessment weights
- `AIArchetypeConfiguration` - Archetype behavior configurations

### v0.42.3 Tables

- `BossConfiguration` - Boss encounter settings
- `BossPhaseTransition` - Phase transition configurations
- `AbilityRotationStep` - Boss ability rotations
- `AddManagementConfig` - Add summoning configurations

### v0.42.4 Tables

- `AIPerformanceMetrics` - Performance tracking data
- `AIDifficultyScalingLog` - Difficulty scaling logs

### Manual Initialization

```csharp
var repository = serviceProvider.GetRequiredService<IAIConfigurationRepository>();

// Reseed default configurations
await repository.SeedDefaultThreatWeightsAsync();
await repository.SeedDefaultArchetypeConfigurationsAsync();
await repository.SeedDefaultBossConfigurationsAsync();

```

---

## Difficulty Configuration

### Setting NG+ Tier

```csharp
var difficultyService = serviceProvider.GetRequiredService<IDifficultyScalingService>();

// Set current NG+ tier (0-5+)
difficultyService.SetNGPlusTier(gameState.NGPlusTier);

```

### Setting Endless Mode

```csharp
// Set current wave number
difficultyService.SetEndlessWave(currentWave);

```

### Setting Boss Gauntlet

```csharp
// Set current boss number
difficultyService.SetBossGauntletNumber(bossNumber);

```

### Setting Challenge Sector

```csharp
// Enable Challenge Sector mode
difficultyService.SetIsChallengeSector(true);

```

### Intelligence Level Mapping

| Difficulty | Intelligence | Error Rate | Behavior |
| --- | --- | --- | --- |
| NG+0 | 0 | 15% | Basic tactics, frequent errors |
| NG+1 | 1 | 10% | Improved tactics |
| NG+2 | 2 | 8% | Good tactics |
| NG+3 | 3 | 3% | Advanced tactics |
| NG+4 | 4 | 1% | Near-perfect play |
| NG+5 | 5 | 0% | Perfect play, exploit weaknesses |

**Challenge Sectors:** Base NG+ tier + 1 (capped at 5)

**Endless Mode:**

- Wave 1-10: Intelligence 1
- Wave 11-20: Intelligence 2
- Wave 21-30: Intelligence 3
- Wave 31-40: Intelligence 4
- Wave 40+: Intelligence 5

**Boss Gauntlet:**

- Boss 1-2: Intelligence 2
- Boss 3-4: Intelligence 3
- Boss 5-6: Intelligence 4
- Boss 7-8: Intelligence 5

---

## Boss Configuration

### Creating a Boss Configuration

```csharp
// Via database (recommended for designers)
// Add to runeandrust.db BossConfiguration table:

INSERT INTO BossConfiguration
    (BossTypeId, BossName, HasPhases, PhaseCount, UsesAdds, UsesAdaptiveDifficulty, BaseAggressionLevel)
VALUES
    (2001, 'Ancient Guardian', 1, 3, 1, 1, 5);

```

### Configuring Phase Transitions

```sql
INSERT INTO BossPhaseTransition
    (BossTypeId, ToPhase, HPThreshold, DialogueLine, TransitionAbilityId)
VALUES
    (2001, 2, 66.0, 'You dare challenge me?!', 101),
    (2001, 3, 33.0, 'I will not fall!', 102);

```

### Configuring Ability Rotations

```sql
-- Phase 1 rotation
INSERT INTO AbilityRotationStep
    (BossTypeId, Phase, StepOrder, AbilityId, FallbackAbilityId, Priority)
VALUES
    (2001, 1, 0, 201, 0, 1),  -- Ability 201, fallback to basic attack
    (2001, 1, 1, 202, 201, 1),
    (2001, 1, 2, 203, 201, 2);

-- Phase 2 rotation (more aggressive)
INSERT INTO AbilityRotationStep
    (BossTypeId, Phase, StepOrder, AbilityId, FallbackAbilityId, Priority)
VALUES
    (2001, 2, 0, 204, 201, 2),
    (2001, 2, 1, 205, 201, 2),
    (2001, 2, 2, 206, 201, 3);

```

### Configuring Add Management

```sql
INSERT INTO AddManagementConfig
    (BossTypeId, Phase, AddType, AddCount, MaxAddsActive, SummonCooldownSeconds, AddHealthMultiplier, AddDamageMultiplier)
VALUES
    (2001, 2, 1, 2, 4, 30.0, 0.5, 0.7),  -- Phase 2: Summon 2 melee adds, max 4 active
    (2001, 3, 2, 3, 6, 20.0, 0.3, 0.5);  -- Phase 3: Summon 3 ranged adds, max 6 active

```

---

## Archetype Tuning

### Viewing Current Configurations

```csharp
var repository = serviceProvider.GetRequiredService<IAIConfigurationRepository>();

var archetypeConfig = await repository.GetArchetypeConfigurationAsync(AIArchetype.Aggressive);

Console.WriteLine($"Archetype: {archetypeConfig.ArchetypeName}");
Console.WriteLine($"Damage Modifier: {archetypeConfig.DamageAbilityModifier}");
Console.WriteLine($"Utility Modifier: {archetypeConfig.UtilityAbilityModifier}");
Console.WriteLine($"Aggression Level: {archetypeConfig.AggressionLevel}");

```

### Updating Archetype Configuration

```csharp
// Modify configuration
archetypeConfig.DamageAbilityModifier = 1.6m; // Increase damage priority
archetypeConfig.AggressionLevel = 5; // Maximum aggression

// Save changes
await repository.UpdateArchetypeConfigurationAsync(archetypeConfig);

```

### Tuning Threat Weights

```csharp
var weights = await repository.GetThreatWeightsAsync(AIArchetype.Tactical);

// Adjust weights (must sum to reasonable values)
weights.DamageWeight = 0.35m;
weights.HPWeight = 0.30m;
weights.PositionWeight = 0.20m;
weights.AbilityWeight = 0.10m;
weights.StatusWeight = 0.05m;

await repository.UpdateThreatWeightsAsync(weights);

```

---

## Performance Monitoring

### Enabling Performance Monitoring

Performance monitoring is automatically enabled when using the orchestrator.

### Viewing Metrics

```csharp
var monitor = serviceProvider.GetRequiredService<IAIPerformanceMonitor>();

// Get all metrics
var metrics = monitor.GetMetrics();

foreach (var (operation, metric) in metrics)
{
    Console.WriteLine($"{operation}:");
    Console.WriteLine($"  Calls: {metric.TotalCalls}");
    Console.WriteLine($"  Average: {metric.AverageMs:F2}ms");
    Console.WriteLine($"  Max: {metric.MaxMs}ms");
}

```

### Generating Reports

```csharp
var orchestrator = serviceProvider.GetRequiredService<EnemyAIOrchestrator>();

// Generate performance summary
var summary = orchestrator.GeneratePerformanceSummary();
Console.WriteLine(summary);

```

### Performance Thresholds

**Target:** <50ms per enemy decision

**Warning Logged If:**

- Single decision >50ms
- 10 decisions >500ms

### Resetting Metrics

```csharp
monitor.ResetMetrics();

```

---

## Debug Mode

### Enabling Debug Mode

```csharp
var debugService = serviceProvider.GetRequiredService<IAIDebugService>();

// Enable verbose logging
debugService.EnableDebugMode();

```

Or via EnemyAI:

```csharp
enemyAI.EnableDebugMode();

```

### Debug Output

When enabled, each AI decision logs:

```json
{
  "Enemy": {
    "Id": 1,
    "Name": "Aberration",
    "AIArchetype": "Tactical",
    "HP": "500/1000"
  },
  "Decision": {
    "Target": "PlayerTank",
    "AbilityId": 5,
    "MoveTo": null,
    "AggressionModifier": 0.2,
    "Priority": 2
  },
  "Context": {
    "IntelligenceLevel": 4,
    "ThreatCount": 3,
    "AvailableAbilities": 8,
    "IsIntentionalError": false,
    "ErrorType": null
  },
  "Reasoning": "Tactical AI decision | Target threat: 85.3 | Ability score: 92.1"
}

```

### Generating Decision Reports

```csharp
// At end of combat encounter
var report = debugService.GenerateDecisionReport(encounterId);

Console.WriteLine($"Total Decisions: {report.TotalDecisions}");
Console.WriteLine($"Average Decision Time: {report.AverageDecisionTimeMs:F1}ms");
Console.WriteLine($"Intentional Errors: {report.IntentionalErrors}");
Console.WriteLine($"Average Intelligence: {report.AverageIntelligenceLevel:F1}");

Console.WriteLine("\\nMost Common Targets:");
foreach (var (target, count) in report.MostCommonTargets)
{
    Console.WriteLine($"  {target}: {count}");
}

```

### Disabling Debug Mode

```csharp
debugService.DisableDebugMode();

```

---

## Advanced Configuration

### Challenge Sector Modifiers

```csharp
var modifiers = new List<ChallengeSectorModifier>
{
    new ChallengeSectorModifier
    {
        Type = SectorModifierType.NoHealing,
        Name = "No Healing",
        Description = "Healing is disabled",
        AggressionModifier = 0.2m
    },
    new ChallengeSectorModifier
    {
        Type = SectorModifierType.DoubleSpeed,
        Name = "Double Speed",
        Description = "All actions twice as fast",
        AggressionModifier = 0.3m
    }
};

// Pass to orchestrator
var action = await enemyAI.DetermineActionV042Async(
    enemy,
    state,
    isBoss: false,
    challengeModifiers: modifiers);

```

### Custom Difficulty Scaling

```csharp
// Create custom scaling service
public class CustomDifficultyScaling : DifficultyScalingService
{
    public override async Task<int> GetAIIntelligenceLevelAsync()
    {
        // Custom logic here
        return customIntelligence;
    }
}

// Register in DI
services.AddSingleton<IDifficultyScalingService, CustomDifficultyScaling>();

```

---

## Troubleshooting

### Slow AI Decisions

Check performance metrics:

```csharp
var summary = orchestrator.GeneratePerformanceSummary();
Console.WriteLine(summary);

```

Optimize by:

1. Ensure database is not on network drive
2. Check for excessive logging
3. Reduce number of potential targets
4. Disable debug mode in production

### AI Not Using Advanced Tactics

Verify:

1. Orchestrator is enabled: `enemyAI.IsV042OrchestratorEnabled()`
2. Intelligence level is high enough: `await difficultyService.GetAIIntelligenceLevelAsync()`
3. Enemy has appropriate archetype assigned

### Boss Not Transitioning Phases

Check:

1. Boss configuration exists: `await bossService.GetBossConfigurationAsync(bossTypeId)`
2. Phase transitions configured: `await bossService.GetPhaseTransitionConfigAsync(bossTypeId, phase)`
3. Boss HP is crossing thresholds

### Database Errors

Reset database:

```csharp
// Delete runeandrust.db
// Restart application to recreate with default data

```

---

## Best Practices

1. **Always use dependency injection** - Don't manually instantiate services
2. **Enable orchestrator early** - Preferably during combat initialization
3. **Configure difficulty before combat** - Set NG+ tier, wave number, etc.
4. **Monitor performance in development** - Use debug mode and metrics
5. **Disable debug mode in production** - Significant performance impact
6. **Cache archetype configurations** - Repository handles this automatically
7. **Use async/await** - Don't block on async operations
8. **Handle null returns gracefully** - Boss configurations may not exist

---

## Example: Complete Combat Setup

```csharp
public async Task<CombatResult> RunCombatEncounterAsync(
    List<PlayerCharacter> players,
    List<Enemy> enemies,
    GameState gameState)
{
    // Setup services
    var orchestrator = _serviceProvider.GetRequiredService<EnemyAIOrchestrator>();
    var difficultyService = _serviceProvider.GetRequiredService<IDifficultyScalingService>();
    var debugService = _serviceProvider.GetRequiredService<IAIDebugService>();

    // Configure difficulty
    difficultyService.SetNGPlusTier(gameState.NGPlusTier);
    difficultyService.SetIsChallengeSector(gameState.InChallengeSector);

    // Setup EnemyAI
    var enemyAI = new EnemyAI(_diceService, _statusEffectService, _counterAttackService, _flavorTextService);
    enemyAI.EnableV042Orchestrator(orchestrator);

    // Enable debug in development
    if (_environment.IsDevelopment())
    {
        enemyAI.EnableDebugMode();
    }

    // Create battlefield state
    var state = new BattlefieldState
    {
        PlayerParty = players,
        Enemies = enemies
    };

    var encounterId = Guid.NewGuid();

    // Combat loop
    while (!IsCombatOver(players, enemies))
    {
        // Player turns...

        // Enemy turns
        foreach (var enemy in enemies.Where(e => e.CurrentHP > 0))
        {
            var action = await enemyAI.DetermineActionV042Async(
                enemy,
                state,
                isBoss: enemy.IsBoss,
                challengeModifiers: gameState.ActiveSectorModifiers);

            // Execute action
            await ExecuteEnemyActionAsync(action);

            // Update state
            state = UpdateBattlefieldState(players, enemies);
        }
    }

    // Generate reports
    var decisionReport = enemyAI.GenerateDecisionReport(encounterId);
    var performanceSummary = enemyAI.GeneratePerformanceSummary();

    _logger.LogInformation("Combat complete. {Decisions} AI decisions made.", decisionReport.TotalDecisions);

    return CreateCombatResult(players, enemies, decisionReport);
}

```

---

## Support & Further Reading

- **Implementation Summaries:** See `IMPLEMENTATION_v0.42.X.md` files
- **Architecture:** See individual service files for detailed documentation
- **Database Schema:** See `AIConfigurationRepository.cs`
- **Examples:** See `Tests.cs` files for usage examples

---

**End of Configuration Guide**