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
        var bossConfig = await _configRepository.GetBossConfigurationAsync((int)boss.Type);

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
        var addConfig = await GetAddManagementConfigAsync((int)boss.Type, currentPhase);

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
        if (livingAdds.Count >= config.MaxSimultaneousAdds)
        {
            _logger.LogDebug(
                "Boss {BossId} already at max adds ({Count}/{Max})",
                boss.Id,
                livingAdds.Count,
                config.MaxSimultaneousAdds);
            return false;
        }

        // Check turn interval cooldown (if configured)
        if (config.SummonEveryNTurns.HasValue && _lastSummonTimes.ContainsKey(boss.EnemyID))
        {
            // TODO: Track actual turn count instead of time
            // For now, we'll skip the cooldown check as we don't have turn tracking
            _logger.LogDebug(
                "Boss {BossId} summon every {N} turns (turn tracking not yet implemented)",
                boss.Id,
                config.SummonEveryNTurns.Value);
        }

        // Check HP threshold triggers
        if (config.SummonAtHPThresholds != null && config.SummonAtHPThresholds.Any())
        {
            var hpPercent = (decimal)boss.CurrentHP / boss.MaxHP * 100m;

            // Check if any threshold is met
            bool thresholdMet = false;
            foreach (var threshold in config.SummonAtHPThresholds)
            {
                if (hpPercent <= threshold)
                {
                    thresholdMet = true;
                    break;
                }
            }

            if (!thresholdMet)
            {
                _logger.LogDebug(
                    "Boss {BossId} HP at {HP}%, no summon threshold met",
                    boss.Id,
                    hpPercent);
                return false;
            }
        }

        // Check if we should resummon if all adds died
        if (config.ResummonIfAllAddsDie && livingAdds.Count == 0)
        {
            _logger.LogDebug(
                "Boss {BossId} resummon triggered (all adds dead)",
                boss.Id);
            return true;
        }

        return true;
    }

    /// <summary>
    /// Summons adds for a boss.
    /// </summary>
    public async Task SummonAddsAsync(Enemy boss, AddManagementConfig config, BattlefieldState state)
    {
        var totalAdds = config.AddTypes.Sum(a => a.Count);

        _logger.LogInformation(
            "Boss {BossId} summoning {Count} adds ({Types})",
            boss.Id,
            totalAdds,
            string.Join(", ", config.AddTypes.Select(a => $"{a.Count}x {a.EnemyTypeName}")));

        // Display summon dialogue if configured
        if (!string.IsNullOrEmpty(config.SummonDialogue))
        {
            _logger.LogInformation(
                "Boss {BossId}: {Dialogue}",
                boss.Id,
                config.SummonDialogue);
        }

        // TODO: Integrate with actual enemy spawning system
        // For now, just log and track

        if (!_summonedAdds.ContainsKey(boss.EnemyID))
        {
            _summonedAdds[boss.EnemyID] = new List<int>();
        }

        int addIndex = 0;
        foreach (var addType in config.AddTypes)
        {
            for (int i = 0; i < addType.Count; i++)
            {
                // Create add (placeholder)
                var addId = GenerateAddId(boss.EnemyID, addIndex);

                _summonedAdds[boss.EnemyID].Add(addId);

                _logger.LogDebug(
                    "Summoned add {AddId} (type: {TypeName}, typeId: {TypeId}) with HP multiplier {HPMult} and damage multiplier {DmgMult}",
                    addId,
                    addType.EnemyTypeName,
                    addType.EnemyTypeId,
                    config.AddHPMultiplier,
                    config.AddDamageMultiplier);

                addIndex++;
            }
        }

        // Update last summon time
        _lastSummonTimes[boss.EnemyID] = DateTime.UtcNow;

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
        if (!_summonedAdds.ContainsKey(boss.EnemyID))
        {
            return new List<Enemy>();
        }

        var summonedAddIds = _summonedAdds[boss.EnemyID];

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
