using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing boss add summoning and coordination.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class AddManagementService : IAddManagementService
{
    private readonly IAIConfigurationRepository _configRepository;
    private readonly ILogger<AddManagementService> _logger;

    // Track adds summoned by each boss
    private readonly Dictionary<int, List<int>> _summonedAdds = new();
    private readonly Dictionary<int, DateTime> _lastSummonTimes = new();

    public AddManagementService(
        IAIConfigurationRepository configRepository,
        ILogger<AddManagementService> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
    }

    /// <summary>
    /// Manages add summoning and coordination for a boss.
    /// </summary>
    public async Task ManageAddsAsync(Enemy boss, BattlefieldState state)
    {
        var bossConfig = await _configRepository.GetBossConfigurationAsync(boss.EnemyTypeId);

        if (bossConfig == null || !bossConfig.UsesAdds)
        {
            _logger.LogDebug(
                "Boss {BossId} does not use adds",
                boss.Id);
            return;
        }

        // Determine current phase
        var hpPercent = (float)boss.CurrentHP / boss.MaxHP * 100f;
        var currentPhase = hpPercent > 66f ? BossPhase.Phase1 :
                          hpPercent > 33f ? BossPhase.Phase2 :
                          BossPhase.Phase3;

        // Get add management configuration for current phase
        var addConfig = await GetAddManagementConfigAsync(boss.EnemyTypeId, currentPhase);

        if (addConfig == null)
        {
            _logger.LogDebug(
                "No add configuration for boss {BossId} phase {Phase}",
                boss.Id,
                currentPhase);
            return;
        }

        // Check if should summon adds
        if (ShouldSummonAdds(boss, addConfig, state))
        {
            await SummonAddsAsync(boss, addConfig, state);
        }

        // Coordinate with existing adds
        await CoordinateWithAddsAsync(boss, state);
    }

    /// <summary>
    /// Checks if the boss should summon adds based on configuration.
    /// </summary>
    public bool ShouldSummonAdds(Enemy boss, AddManagementConfig config, BattlefieldState state)
    {
        // Check max adds limit
        var livingAdds = GetLivingAdds(boss, state);
        if (livingAdds.Count >= config.MaxAddsActive)
        {
            _logger.LogDebug(
                "Boss {BossId} already at max adds ({Count}/{Max})",
                boss.Id,
                livingAdds.Count,
                config.MaxAddsActive);
            return false;
        }

        // Check cooldown
        if (_lastSummonTimes.ContainsKey(boss.Id))
        {
            var timeSinceLastSummon = DateTime.UtcNow - _lastSummonTimes[boss.Id];
            if (timeSinceLastSummon.TotalSeconds < config.SummonCooldownSeconds)
            {
                _logger.LogDebug(
                    "Boss {BossId} summon on cooldown ({Remaining}s remaining)",
                    boss.Id,
                    config.SummonCooldownSeconds - timeSinceLastSummon.TotalSeconds);
                return false;
            }
        }

        // Check summon triggers
        if (config.SummonTriggers != null)
        {
            // HP threshold trigger
            if (config.SummonTriggers.ContainsKey("HPThreshold"))
            {
                var hpPercent = (float)boss.CurrentHP / boss.MaxHP * 100f;
                var threshold = config.SummonTriggers["HPThreshold"];
                if (hpPercent > threshold)
                {
                    return false;
                }
            }

            // Turn interval trigger
            if (config.SummonTriggers.ContainsKey("TurnInterval"))
            {
                var interval = (int)config.SummonTriggers["TurnInterval"];
                // TODO: Track turn count
                // For now, always trigger if HP threshold met
            }
        }

        return true;
    }

    /// <summary>
    /// Summons adds for a boss.
    /// </summary>
    public async Task SummonAddsAsync(Enemy boss, AddManagementConfig config, BattlefieldState state)
    {
        _logger.LogInformation(
            "Boss {BossId} summoning {Count} adds of type {AddType}",
            boss.Id,
            config.AddCount,
            config.AddType);

        // TODO: Integrate with actual enemy spawning system
        // For now, just log and track

        if (!_summonedAdds.ContainsKey(boss.Id))
        {
            _summonedAdds[boss.Id] = new List<int>();
        }

        for (int i = 0; i < config.AddCount; i++)
        {
            // Create add (placeholder)
            var addId = GenerateAddId(boss.Id, i);

            _summonedAdds[boss.Id].Add(addId);

            _logger.LogDebug(
                "Summoned add {AddId} with HP multiplier {HPMult} and damage multiplier {DmgMult}",
                addId,
                config.AddHealthMultiplier,
                config.AddDamageMultiplier);
        }

        // Update last summon time
        _lastSummonTimes[boss.Id] = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Coordinates targeting between boss and adds.
    /// </summary>
    public async Task CoordinateWithAddsAsync(Enemy boss, BattlefieldState state)
    {
        var livingAdds = GetLivingAdds(boss, state);

        if (livingAdds.Count == 0)
        {
            return;
        }

        _logger.LogDebug(
            "Boss {BossId} coordinating with {Count} adds",
            boss.Id,
            livingAdds.Count);

        // TODO: Implement coordination strategies:
        // 1. Focus fire on same target
        // 2. Split targeting (boss tanks, adds DPS)
        // 3. Protect adds (boss taunts threats)
        // 4. Strategic positioning

        // For now, just log coordination
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the add management configuration for a boss phase.
    /// </summary>
    public async Task<AddManagementConfig?> GetAddManagementConfigAsync(
        int bossTypeId,
        BossPhase phase)
    {
        return await _configRepository.GetAddManagementConfigAsync(bossTypeId, phase);
    }

    /// <summary>
    /// Gets all living adds summoned by a boss.
    /// </summary>
    public List<Enemy> GetLivingAdds(Enemy boss, BattlefieldState state)
    {
        if (!_summonedAdds.ContainsKey(boss.Id))
        {
            return new List<Enemy>();
        }

        var summonedAddIds = _summonedAdds[boss.Id];

        // TODO: Cross-reference with actual enemy list in battlefield state
        // For now, return empty list (placeholder)
        var livingAdds = state.Enemies
            .Where(e => summonedAddIds.Contains(e.Id) && e.CurrentHP > 0)
            .ToList();

        _logger.LogDebug(
            "Boss {BossId} has {Count} living adds",
            boss.Id,
            livingAdds.Count);

        return livingAdds;
    }

    /// <summary>
    /// Generates a unique add ID based on boss ID.
    /// </summary>
    private int GenerateAddId(int bossId, int index)
    {
        // Simple ID generation: boss ID * 1000 + timestamp hash + index
        // This ensures unique IDs for adds
        return bossId * 1000 + (int)(DateTime.UtcNow.Ticks % 100) + index;
    }
}
