using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using RuneAndRust.Engine.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine;

/// <summary>
/// Tactical AI integration for EnemyAI
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public partial class EnemyAI
{
    // v0.42.1: Tactical AI Services
    private readonly IThreatAssessmentService? _threatAssessmentService;
    private readonly ITargetSelectionService? _targetSelectionService;
    private readonly ISituationalAnalysisService? _situationalAnalysisService;
    private readonly IBehaviorPatternService? _behaviorPatternService;
    // Note: _aiLogger is defined in EnemyAI.Integration.cs partial class

    // Track current combat session for logging
    private Guid _currentSessionId = Guid.NewGuid();
    private Guid _currentEncounterId = Guid.NewGuid();
    private int _currentTurn = 0;

    /// <summary>
    /// Constructor with tactical AI services (v0.42.1)
    /// </summary>
    public EnemyAI(
        DiceService diceService,
        IThreatAssessmentService? threatAssessmentService = null,
        ITargetSelectionService? targetSelectionService = null,
        ISituationalAnalysisService? situationalAnalysisService = null,
        IBehaviorPatternService? behaviorPatternService = null,
        ILogger<EnemyAI>? logger = null,
        AdvancedStatusEffectService? statusEffectService = null,
        CounterAttackService? counterAttackService = null,
        CombatFlavorTextService? flavorTextService = null)
        : this(diceService, statusEffectService, counterAttackService, flavorTextService)
    {
        _threatAssessmentService = threatAssessmentService;
        _targetSelectionService = targetSelectionService;
        _situationalAnalysisService = situationalAnalysisService;
        _behaviorPatternService = behaviorPatternService;
        _aiLogger = logger;
    }

    /// <summary>
    /// v0.42.1: Intelligent target selection using tactical AI services.
    /// Falls back to original logic if tactical AI is not available.
    /// </summary>
    public async Task<object> SelectTargetIntelligentAsync(
        Enemy enemy,
        List<object> playerParty,
        BattlefieldGrid? grid,
        List<Enemy> allEnemies,
        int currentTurn)
    {
        // Update combat session tracking
        _currentTurn = currentTurn;

        // If tactical AI services are not available, fall back to original logic
        if (_targetSelectionService == null ||
            _threatAssessmentService == null ||
            _situationalAnalysisService == null ||
            _behaviorPatternService == null)
        {
            _aiLogger?.LogDebug(
                "Tactical AI services not available, using legacy target selection for {EnemyId}",
                enemy.Id);

            // Fall back to synchronous SelectTarget method
            return SelectTarget(enemy, playerParty, grid, new FormationService());
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Build battlefield state
            var state = BuildBattlefieldState(playerParty, allEnemies, grid, currentTurn);

            // Analyze situation
            var situation = _situationalAnalysisService.AnalyzeSituation(enemy, state);

            // Select target intelligently
            var livingTargets = playerParty.Where(t => IsTargetAlive(t)).ToList();

            if (!livingTargets.Any())
            {
                _aiLogger?.LogWarning("No living targets available for {EnemyId}", enemy.Id);
                return playerParty.First(); // Fallback to first target
            }

            var selectedTarget = await _targetSelectionService.SelectTargetAsync(
                enemy,
                livingTargets,
                state);

            stopwatch.Stop();

            if (selectedTarget == null)
            {
                _aiLogger?.LogWarning(
                    "Target selection returned null for {EnemyId}, using fallback",
                    enemy.Id);
                return livingTargets.First();
            }

            // Log decision with performance metrics
            var targetName = ExtractTargetName(selectedTarget);

            _aiLogger?.LogInformation(
                "AI Decision [{ElapsedMs}ms]: {EnemyName} ({Archetype}) → {TargetName} | " +
                "Situation: {Advantage} | HP: {HP:P0}",
                stopwatch.ElapsedMilliseconds,
                enemy.Name,
                enemy.AIArchetype,
                targetName,
                situation.Advantage,
                situation.SelfHPPercent);

            return selectedTarget;
        }
        catch (Exception ex)
        {
            _aiLogger?.LogError(ex,
                "Error in tactical target selection for {EnemyId}, falling back to legacy logic",
                enemy.Id);

            // Fall back to original logic on error
            return SelectTarget(enemy, playerParty, grid, new FormationService());
        }
    }

    /// <summary>
    /// v0.42.1: Builds a battlefield state snapshot for AI decision-making.
    /// </summary>
    private BattlefieldState BuildBattlefieldState(
        List<object> playerParty,
        List<Enemy> allEnemies,
        BattlefieldGrid? grid,
        int currentTurn)
    {
        var state = new BattlefieldState
        {
            Grid = grid,
            CurrentTurn = currentTurn,
            SessionId = _currentSessionId,
            EncounterId = _currentEncounterId
        };

        // Extract player characters from polymorphic list
        foreach (var target in playerParty)
        {
            if (target is PlayerCharacter player)
            {
                state.PlayerCharacters.Add(player);
            }
        }

        // Add all enemies
        state.Enemies.AddRange(allEnemies);

        return state;
    }

    /// <summary>
    /// v0.42.1: Checks if a polymorphic target is alive.
    /// </summary>
    private bool IsTargetAlive(object target)
    {
        return target switch
        {
            PlayerCharacter player => player.IsAlive,
            Enemy enemy => enemy.IsAlive,
            _ => false
        };
    }

    /// <summary>
    /// v0.42.1: Extracts the name from a polymorphic target.
    /// </summary>
    private string ExtractTargetName(object target)
    {
        return target switch
        {
            PlayerCharacter player => player.Name,
            Enemy enemy => enemy.Name,
            _ => "Unknown"
        };
    }

    /// <summary>
    /// v0.42.1: Initializes a new combat encounter (resets session tracking).
    /// </summary>
    public void InitializeEncounter(Guid? sessionId = null)
    {
        _currentSessionId = sessionId ?? Guid.NewGuid();
        _currentEncounterId = Guid.NewGuid();
        _currentTurn = 0;

        _aiLogger?.LogInformation(
            "New combat encounter initialized: Session={SessionId}, Encounter={EncounterId}",
            _currentSessionId, _currentEncounterId);
    }

    /// <summary>
    /// v0.42.1: Gets or assigns an AI archetype for an enemy.
    /// </summary>
    public async Task<AIArchetype> GetOrAssignArchetypeAsync(Enemy enemy)
    {
        // If behavior pattern service is available, use it
        if (_behaviorPatternService != null)
        {
            return await _behaviorPatternService.GetArchetypeAsync(enemy);
        }

        // Otherwise, return the enemy's assigned archetype or default
        if (enemy.AIArchetype == default(AIArchetype))
        {
            // Assign default based on enemy type
            var defaultArchetype = GetDefaultArchetypeForType(enemy.Type);
            enemy.AIArchetype = defaultArchetype;

            _aiLogger?.LogDebug(
                "Assigned default archetype {Archetype} to {EnemyName} ({Type})",
                defaultArchetype, enemy.Name, enemy.Type);
        }

        return enemy.AIArchetype;
    }

    /// <summary>
    /// v0.42.1: Gets the default archetype for an enemy type (fallback).
    /// </summary>
    private AIArchetype GetDefaultArchetypeForType(EnemyType type)
    {
        return type switch
        {
            EnemyType.CorruptedServitor => AIArchetype.Reckless,
            EnemyType.BlightDrone => AIArchetype.Cautious,
            EnemyType.RuinWarden => AIArchetype.Defensive,
            EnemyType.ScrapHound => AIArchetype.Aggressive,
            EnemyType.TestSubject => AIArchetype.Reckless,
            EnemyType.WarFrame => AIArchetype.Tactical,
            EnemyType.ForlornScholar => AIArchetype.Control,
            EnemyType.AethericAberration => AIArchetype.Tactical,
            EnemyType.MaintenanceConstruct => AIArchetype.Defensive,
            EnemyType.SludgeCrawler => AIArchetype.Aggressive,
            EnemyType.CorruptedEngineer => AIArchetype.Support,
            EnemyType.VaultCustodian => AIArchetype.Defensive,
            EnemyType.ForlornArchivist => AIArchetype.Control,
            EnemyType.OmegaSentinel => AIArchetype.Aggressive,
            EnemyType.CorrodedSentry => AIArchetype.Cautious,
            EnemyType.HuskEnforcer => AIArchetype.Reckless,
            EnemyType.ArcWelderUnit => AIArchetype.Aggressive,
            EnemyType.Shrieker => AIArchetype.Control,
            EnemyType.JotunReaderFragment => AIArchetype.Tactical,
            EnemyType.ServitorSwarm => AIArchetype.Aggressive,
            EnemyType.BoneKeeper => AIArchetype.Defensive,
            EnemyType.FailureColossus => AIArchetype.Aggressive,
            EnemyType.RustWitch => AIArchetype.Control,
            EnemyType.SentinelPrime => AIArchetype.Tactical,
            _ => AIArchetype.Tactical
        };
    }
}
