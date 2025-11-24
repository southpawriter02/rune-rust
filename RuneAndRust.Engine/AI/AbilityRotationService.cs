using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing boss ability rotations.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class AbilityRotationService : IAbilityRotationService
{
    private readonly IAIConfigurationRepository _configRepository;
    private readonly ILogger<AbilityRotationService> _logger;

    // Track rotation indices per boss instance
    private readonly Dictionary<int, int> _rotationIndices = new();

    public AbilityRotationService(
        IAIConfigurationRepository configRepository,
        ILogger<AbilityRotationService> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets the ability rotation for a boss phase.
    /// </summary>
    public async Task<AbilityRotation?> GetPhaseRotationAsync(int bossTypeId, BossPhase phase)
    {
        return await _configRepository.GetAbilityRotationAsync(bossTypeId, phase);
    }

    /// <summary>
    /// Selects the next ability in the boss's rotation.
    /// </summary>
    public async Task<object> SelectNextAbilityInRotationAsync(
        Enemy boss,
        AbilityRotation rotation,
        BattlefieldState state)
    {
        if (rotation == null || rotation.Steps == null || rotation.Steps.Count == 0)
        {
            _logger.LogWarning(
                "No rotation configured for boss {BossId}, using basic attack",
                boss.Id);
            return CreateBasicAttack();
        }

        // Get current rotation index for this boss
        if (!_rotationIndices.ContainsKey(boss.Id))
        {
            _rotationIndices[boss.Id] = 0;
        }

        var currentIndex = _rotationIndices[boss.Id];
        var step = rotation.Steps[currentIndex];

        // Check if ability is available
        if (!IsAbilityAvailable(boss, step.AbilityId))
        {
            _logger.LogDebug(
                "Ability {AbilityId} not available for boss {BossId}, checking alternatives",
                step.AbilityId,
                boss.Id);

            // Try fallback ability
            if (step.FallbackAbilityId.HasValue &&
                IsAbilityAvailable(boss, step.FallbackAbilityId.Value))
            {
                _logger.LogDebug(
                    "Using fallback ability {AbilityId}",
                    step.FallbackAbilityId.Value);

                return await CreateAbilityAction(
                    boss,
                    step.FallbackAbilityId.Value,
                    step.Priority);
            }

            // Skip to next step if no fallback
            AdvanceRotation(boss.Id, rotation.Steps.Count);
            return await SelectNextAbilityInRotationAsync(boss, rotation, state);
        }

        // Advance rotation for next turn
        AdvanceRotation(boss.Id, rotation.Steps.Count);

        return await CreateAbilityAction(boss, step.AbilityId, step.Priority);
    }

    /// <summary>
    /// Checks if an ability is currently available (not on cooldown, enough resources).
    /// </summary>
    public bool IsAbilityAvailable(Enemy boss, int abilityId)
    {
        // TODO: In future, integrate with actual ability/cooldown system
        // For now, assume all abilities are available
        // This should check:
        // 1. Ability cooldown status
        // 2. Resource costs (mana, energy, etc.)
        // 3. Special requirements (HP threshold, etc.)

        _logger.LogDebug(
            "Checking ability {AbilityId} availability for boss {BossId}",
            abilityId,
            boss.Id);

        // Placeholder: Always available for now
        return true;
    }

    /// <summary>
    /// Resets the boss's rotation to the beginning.
    /// Called during phase transitions.
    /// </summary>
    public void ResetRotation(Enemy boss)
    {
        _logger.LogInformation(
            "Resetting rotation for boss {BossId}",
            boss.Id);

        _rotationIndices[boss.Id] = 0;
    }

    /// <summary>
    /// Advances the rotation index, wrapping around to start.
    /// </summary>
    private void AdvanceRotation(int bossId, int rotationLength)
    {
        _rotationIndices[bossId] = (_rotationIndices[bossId] + 1) % rotationLength;

        _logger.LogDebug(
            "Boss {BossId} rotation advanced to index {Index}",
            bossId,
            _rotationIndices[bossId]);
    }

    /// <summary>
    /// Creates a basic attack action (fallback when no rotation available).
    /// </summary>
    private object CreateBasicAttack()
    {
        // TODO: Return proper ability/action object
        // For now, return a placeholder
        return new
        {
            Type = "BasicAttack",
            Priority = 1
        };
    }

    /// <summary>
    /// Creates an ability action for the boss to execute.
    /// </summary>
    private async Task<object> CreateAbilityAction(Enemy boss, int abilityId, int priority)
    {
        _logger.LogDebug(
            "Boss {BossId} selected ability {AbilityId} with priority {Priority}",
            boss.Id,
            abilityId,
            priority);

        // TODO: Return proper ability/action object
        // For now, return a placeholder
        await Task.CompletedTask;

        return new
        {
            Type = "Ability",
            AbilityId = abilityId,
            Priority = priority,
            BossId = boss.Id
        };
    }
}
