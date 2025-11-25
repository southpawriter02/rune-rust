using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing boss AI behavior including phase transitions and special mechanics.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class BossAIService : IBossAIService
{
    private readonly IAIConfigurationRepository _configRepository;
    private readonly ILogger<BossAIService> _logger;

    public BossAIService(
        IAIConfigurationRepository configRepository,
        ILogger<BossAIService> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
    }

    /// <summary>
    /// Determines the current phase based on boss HP.
    /// </summary>
    public BossPhase DeterminePhase(Enemy boss)
    {
        if (boss.CurrentHP <= 0)
        {
            return BossPhase.Phase3; // Already dead/dying
        }

        var hpPercent = (float)boss.CurrentHP / boss.MaxHP * 100f;

        if (hpPercent > 66f)
        {
            return BossPhase.Phase1; // Teaching phase (100-66%)
        }
        else if (hpPercent > 33f)
        {
            return BossPhase.Phase2; // Escalation phase (66-33%)
        }
        else
        {
            return BossPhase.Phase3; // Desperation phase (33-0%)
        }
    }

    /// <summary>
    /// Checks if the boss should transition to a new phase.
    /// </summary>
    public bool ShouldTransitionPhase(Enemy boss, BossPhase currentPhase)
    {
        var actualPhase = DeterminePhase(boss);

        // Transition if HP-based phase differs from current phase
        // Note: Phases only move forward, never backward
        return actualPhase > currentPhase;
    }

    /// <summary>
    /// Executes a phase transition (dialogue, special abilities, buffs).
    /// </summary>
    public async Task ExecutePhaseTransitionAsync(
        Enemy boss,
        BossPhase newPhase,
        BattlefieldState state)
    {
        _logger.LogInformation(
            "Boss {BossId} transitioning to {Phase}",
            boss.Id,
            newPhase);

        // Get phase transition configuration
        var transitionConfig = await GetPhaseTransitionConfigAsync((int)boss.Type, newPhase);

        if (transitionConfig == null)
        {
            _logger.LogWarning(
                "No phase transition config found for boss type {BossTypeId} phase {Phase}",
                boss.EnemyTypeId,
                newPhase);
            return;
        }

        // Display dialogue if configured
        if (!string.IsNullOrEmpty(transitionConfig.TransitionDialogue))
        {
            _logger.LogInformation(
                "Boss dialogue: {Dialogue}",
                transitionConfig.TransitionDialogue);

            // TODO: In future, integrate with dialogue/UI system
            // For now, just log it
        }

        // Apply phase bonuses (stat multipliers)
        if (!string.IsNullOrEmpty(transitionConfig.PhaseBonuses))
        {
            try
            {
                var phaseBonuses = JsonSerializer.Deserialize<Dictionary<string, decimal>>(transitionConfig.PhaseBonuses);

                if (phaseBonuses != null)
                {
                    if (phaseBonuses.ContainsKey("AttackMultiplier"))
                    {
                        var multiplier = phaseBonuses["AttackMultiplier"];
                        boss.BaseDamageDice = (int)(boss.BaseDamageDice * multiplier);
                        _logger.LogDebug("Boss attack increased by {Multiplier}x", multiplier);
                    }

                    if (phaseBonuses.ContainsKey("DefenseMultiplier"))
                    {
                        var multiplier = phaseBonuses["DefenseMultiplier"];
                        boss.Defense = (int)(boss.Defense * multiplier);
                        _logger.LogDebug("Boss defense increased by {Multiplier}x", multiplier);
                    }

                    if (phaseBonuses.ContainsKey("SpeedMultiplier"))
                    {
                        var multiplier = phaseBonuses["SpeedMultiplier"];
                        boss.Speed = (int)(boss.Speed * multiplier);
                        _logger.LogDebug("Boss speed bonus: {Multiplier}x", multiplier);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize PhaseBonuses JSON for boss type {BossTypeId}", boss.EnemyTypeId);
            }
        }

        // Execute transition ability if configured
        if (transitionConfig.TransitionAbilityId.HasValue)
        {
            _logger.LogInformation(
                "Boss executing transition ability {AbilityId}",
                transitionConfig.TransitionAbilityId.Value);

            // TODO: In future, integrate with ability execution system
            // For now, just log it
        }

        _logger.LogInformation(
            "Boss {BossId} successfully transitioned to {Phase}",
            boss.Id,
            newPhase);
    }

    /// <summary>
    /// Gets the phase transition configuration for a boss.
    /// </summary>
    public async Task<BossPhaseTransition?> GetPhaseTransitionConfigAsync(
        int bossTypeId,
        BossPhase toPhase)
    {
        return await _configRepository.GetBossPhaseTransitionAsync(bossTypeId, toPhase);
    }

    /// <summary>
    /// Gets the boss configuration for a boss type.
    /// </summary>
    public async Task<BossConfiguration?> GetBossConfigurationAsync(int bossTypeId)
    {
        return await _configRepository.GetBossConfigurationAsync(bossTypeId);
    }
}
