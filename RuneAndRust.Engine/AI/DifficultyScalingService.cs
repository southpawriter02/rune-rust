using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for scaling AI intelligence based on difficulty (NG+, Endless, Challenge Sectors).
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class DifficultyScalingService : IDifficultyScalingService
{
    private readonly ILogger<DifficultyScalingService> _logger;
    private readonly Random _random = new();

    // Configurable difficulty state (set by game state)
    private int _ngPlusTier = 0;
    private int? _endlessWave = null;
    private int? _bossGauntletNumber = null;
    private bool _isChallengeSector = false;

    public DifficultyScalingService(ILogger<DifficultyScalingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sets the current NG+ tier (0-5+).
    /// </summary>
    public void SetNGPlusTier(int tier)
    {
        _ngPlusTier = Math.Max(0, tier);
        _logger.Debug("NG+ tier set to {Tier}", _ngPlusTier);
    }

    /// <summary>
    /// Sets the current Endless Mode wave.
    /// </summary>
    public void SetEndlessWave(int? wave)
    {
        _endlessWave = wave;
        _logger.Debug("Endless wave set to {Wave}", wave);
    }

    /// <summary>
    /// Sets the current Boss Gauntlet number.
    /// </summary>
    public void SetBossGauntletNumber(int? bossNumber)
    {
        _bossGauntletNumber = bossNumber;
        _logger.Debug("Boss Gauntlet number set to {Number}", bossNumber);
    }

    /// <summary>
    /// Sets whether currently in a Challenge Sector.
    /// </summary>
    public void SetIsChallengeSector(bool isChallengeSector)
    {
        _isChallengeSector = isChallengeSector;
        _logger.Debug("Challenge Sector status set to {Status}", isChallengeSector);
    }

    /// <inheritdoc/>
    public async Task<int> GetAIIntelligenceLevelAsync()
    {
        // Endless Mode has special scaling
        if (_endlessWave.HasValue)
        {
            var level = CalculateEndlessIntelligence(_endlessWave.Value);
            _logger.Debug("Endless Mode wave {Wave} -> Intelligence {Level}", _endlessWave.Value, level);
            return await Task.FromResult(level);
        }

        // Boss Gauntlet escalates per boss
        if (_bossGauntletNumber.HasValue)
        {
            var level = CalculateBossGauntletIntelligence(_bossGauntletNumber.Value);
            _logger.Debug("Boss Gauntlet #{Number} -> Intelligence {Level}", _bossGauntletNumber.Value, level);
            return await Task.FromResult(level);
        }

        // Challenge Sectors use base + modifier
        if (_isChallengeSector)
        {
            var baseTier = Math.Min(_ngPlusTier, 5);
            var level = Math.Min(baseTier + 1, 5);  // Challenge Sectors = +1 intelligence
            _logger.Debug("Challenge Sector (NG+{Tier}) -> Intelligence {Level}", _ngPlusTier, level);
            return await Task.FromResult(level);
        }

        // Standard NG+ scaling
        var ngLevel = Math.Min(_ngPlusTier, 5);
        _logger.Debug("NG+{Tier} -> Intelligence {Level}", _ngPlusTier, ngLevel);
        return await Task.FromResult(ngLevel);
    }

    private int CalculateEndlessIntelligence(int wave)
    {
        // Gradual scaling in Endless Mode
        if (wave <= 10) return 1;
        if (wave <= 20) return 2;
        if (wave <= 30) return 3;
        if (wave <= 40) return 4;
        return 5;  // Wave 40+: max intelligence
    }

    private int CalculateBossGauntletIntelligence(int bossNumber)
    {
        // Each boss in gauntlet is smarter
        // Boss 1-2: Intelligence 2
        // Boss 3-4: Intelligence 3
        // Boss 5-6: Intelligence 4
        // Boss 7-8: Intelligence 5
        return Math.Min((bossNumber + 1) / 2, 5);
    }

    /// <inheritdoc/>
    public async Task<EnemyAction> ApplyIntelligenceScalingAsync(
        EnemyAction action,
        int intelligenceLevel,
        BattlefieldState state)
    {
        // Low intelligence: introduce intentional mistakes
        if (intelligenceLevel < 3 && ShouldMakeError(intelligenceLevel))
        {
            var errorChance = CalculateErrorChance(intelligenceLevel);

            _logger.Debug(
                "AI {EnemyId} making tactical error (intelligence={Level}, errorChance={Chance:P0})",
                action.Actor.Id, intelligenceLevel, errorChance);

            // Make a suboptimal decision
            action = await MakeSuboptimalDecisionAsync(action, state);
        }

        // High intelligence: advanced tactics
        if (intelligenceLevel >= 4)
        {
            await ApplyAdvancedTacticsAsync(action, state);
        }

        // Max intelligence: exploit player weaknesses
        if (intelligenceLevel == 5)
        {
            await ExploitPlayerWeaknessesAsync(action, state);
        }

        return action;
    }

    /// <inheritdoc/>
    public double CalculateErrorChance(int intelligenceLevel)
    {
        return intelligenceLevel switch
        {
            0 => 0.15,  // 15% error rate
            1 => 0.10,  // 10% error rate
            2 => 0.08,  // 8% error rate
            3 => 0.03,  // 3% error rate
            4 => 0.01,  // 1% error rate
            5 => 0.00,  // No errors
            _ => 0.15
        };
    }

    /// <inheritdoc/>
    public bool ShouldMakeError(int intelligenceLevel)
    {
        var errorChance = CalculateErrorChance(intelligenceLevel);
        return _random.NextDouble() < errorChance;
    }

    private async Task<EnemyAction> MakeSuboptimalDecisionAsync(
        EnemyAction action,
        BattlefieldState state)
    {
        // Types of mistakes:
        // 1. Target suboptimal enemy (not highest threat)
        // 2. Use basic attack instead of better ability
        // 3. Poor positioning choice

        var mistakeType = _random.Next(1, 4);

        switch (mistakeType)
        {
            case 1:  // Wrong target
                var targets = state.PlayerParty.Where(c => c.CurrentHP > 0).ToList();
                if (targets.Any())
                {
                    action.Target = targets[_random.Next(targets.Count)];
                    action.Context ??= new DecisionContext();
                    action.Context.IsIntentionalError = true;
                    action.Context.ErrorType = "WrongTarget";
                    _logger.Debug("AI mistake: Wrong target selected");
                }
                break;

            case 2:  // Use basic attack
                action.SelectedAbilityId = 0;  // 0 = basic attack
                action.Context ??= new DecisionContext();
                action.Context.IsIntentionalError = true;
                action.Context.ErrorType = "BasicAttackInsteadOfAbility";
                _logger.Debug("AI mistake: Used basic attack instead of better ability");
                break;

            case 3:  // Poor positioning
                action.MoveTo = null;  // Don't move (even if should)
                action.Context ??= new DecisionContext();
                action.Context.IsIntentionalError = true;
                action.Context.ErrorType = "FailedToReposition";
                _logger.Debug("AI mistake: Failed to reposition");
                break;
        }

        return await Task.FromResult(action);
    }

    private async Task ApplyAdvancedTacticsAsync(EnemyAction action, BattlefieldState state)
    {
        // Advanced tactics for intelligence 4+:

        // 1. Focus fire (all enemies attack weakest target)
        var weakestPlayer = state.PlayerParty
            .Where(c => c.CurrentHP > 0)
            .OrderBy(c => (float)c.CurrentHP / c.MaxHP)
            .FirstOrDefault();

        if (weakestPlayer != null)
        {
            action.Target = weakestPlayer;
            _logger.Debug("Advanced tactic: Focus fire on weakest player {PlayerId}", weakestPlayer.Id);
        }

        // 2. Save powerful abilities for optimal moment
        if (action.SelectedAbilityId > 0)  // If using an ability
        {
            // TODO: Check if this is a powerful ability
            // For now, just log advanced tactic usage
            _logger.Debug("Advanced tactic: Intelligent ability usage");
        }

        await Task.CompletedTask;
    }

    private async Task ExploitPlayerWeaknessesAsync(EnemyAction action, BattlefieldState state)
    {
        // Max intelligence: identify and exploit weaknesses

        // Find player's weakest character (lowest HP%)
        var weakestPlayer = state.PlayerParty
            .Where(c => c.CurrentHP > 0)
            .OrderBy(c => (float)c.CurrentHP / c.MaxHP)
            .FirstOrDefault();

        if (weakestPlayer != null)
        {
            var hpPercent = (float)weakestPlayer.CurrentHP / weakestPlayer.MaxHP;

            // If player is weak, focus them down for the kill
            if (hpPercent < 0.4f)
            {
                action.Target = weakestPlayer;
                _logger.Debug(
                    "Max intelligence: Exploiting weak player {PlayerId} at {HP:P0} HP",
                    weakestPlayer.Id, hpPercent);

                // TODO: Use execute abilities if available
                // For now, just prioritize the weak target
            }
        }

        await Task.CompletedTask;
    }
}
