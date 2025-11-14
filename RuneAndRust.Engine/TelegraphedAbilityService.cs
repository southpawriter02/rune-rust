using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23.2: Manages telegraphed boss abilities with charge times, cooldowns,
/// and vulnerability windows
/// </summary>
public class TelegraphedAbilityService
{
    private static readonly ILogger _log = Log.ForContext<TelegraphedAbilityService>();
    private readonly DiceService _diceService;

    /// <summary>
    /// Active telegraphed abilities being charged (ability ID -> telegraphed ability)
    /// </summary>
    private Dictionary<string, TelegraphedAbility> _activeCharges = new();

    /// <summary>
    /// Active cooldowns for boss abilities (boss ID -> list of cooldowns)
    /// </summary>
    private Dictionary<string, List<AbilityCooldown>> _activeCooldowns = new();

    /// <summary>
    /// Current combat turn (for tracking)
    /// </summary>
    private int _currentTurn = 0;

    public TelegraphedAbilityService(DiceService diceService)
    {
        _diceService = diceService;
    }

    /// <summary>
    /// Initialize telegraphed ability tracking for a new combat
    /// </summary>
    public void InitializeCombat()
    {
        _activeCharges.Clear();
        _activeCooldowns.Clear();
        _currentTurn = 0;

        _log.Information("Telegraphed ability tracking initialized");
    }

    /// <summary>
    /// Start charging a telegraphed ability
    /// Returns combat log message
    /// </summary>
    public string StartChargingAbility(Enemy boss, BossAbility ability)
    {
        if (ability.Type != BossAbilityType.Telegraphed && ability.ChargeTurns <= 0)
        {
            _log.Warning("Attempted to charge non-telegraphed ability: {AbilityId}", ability.Id);
            return "";
        }

        // Check if ability is on cooldown
        if (IsAbilityOnCooldown(boss.Id, ability.Id))
        {
            _log.Debug("Ability on cooldown, cannot charge: {BossId}, {AbilityId}",
                boss.Id, ability.Id);
            return "";
        }

        var telegraphedAbility = new TelegraphedAbility
        {
            AbilityId = ability.Id,
            EnemyId = boss.Id,
            RemainingChargeTurns = ability.ChargeTurns,
            TotalChargeTurns = ability.ChargeTurns,
            Ability = ability,
            ChargeStartTurn = _currentTurn
        };

        string key = $"{boss.Id}_{ability.Id}";
        _activeCharges[key] = telegraphedAbility;

        string logMessage = $"\n⚠️ [WARNING] {boss.Name} begins charging {ability.Name}!\n";
        logMessage += $"   {ability.ChargeMessage}\n";
        logMessage += $"   ⏰ Executes in {ability.ChargeTurns} turn(s)!\n";

        _log.Information("Telegraphed ability charging started: {BossId}, {AbilityId}, ChargeTurns={Turns}",
            boss.Id, ability.Id, ability.ChargeTurns);

        return logMessage;
    }

    /// <summary>
    /// Process end of turn for telegraphed abilities (decrement charge timers)
    /// Returns list of abilities ready to execute
    /// </summary>
    public List<(Enemy boss, BossAbility ability)> ProcessEndOfTurn(List<Enemy> bosses)
    {
        _currentTurn++;

        var readyToExecute = new List<(Enemy boss, BossAbility ability)>();
        var keysToRemove = new List<string>();

        foreach (var kvp in _activeCharges)
        {
            var telegraphed = kvp.Value;

            // Skip if interrupted
            if (telegraphed.IsInterrupted)
            {
                keysToRemove.Add(kvp.Key);
                continue;
            }

            telegraphed.RemainingChargeTurns--;

            if (telegraphed.RemainingChargeTurns <= 0)
            {
                // Ability is ready to execute
                var boss = bosses.FirstOrDefault(b => b.Id == telegraphed.EnemyId);
                if (boss != null && telegraphed.Ability != null)
                {
                    readyToExecute.Add((boss, telegraphed.Ability));
                }

                keysToRemove.Add(kvp.Key);
            }
        }

        // Remove executed abilities
        foreach (var key in keysToRemove)
        {
            _activeCharges.Remove(key);
        }

        // Decrement cooldowns
        foreach (var bossId in _activeCooldowns.Keys.ToList())
        {
            var cooldowns = _activeCooldowns[bossId];
            for (int i = cooldowns.Count - 1; i >= 0; i--)
            {
                cooldowns[i].RemainingTurns--;
                if (cooldowns[i].RemainingTurns <= 0)
                {
                    cooldowns.RemoveAt(i);
                    _log.Debug("Cooldown expired: {BossId}, {AbilityId}",
                        bossId, cooldowns[i].AbilityId);
                }
            }
        }

        _log.Debug("End of turn processed: Telegraphed abilities ready={Count}",
            readyToExecute.Count);

        return readyToExecute;
    }

    /// <summary>
    /// Execute a telegraphed ability and start cooldown
    /// </summary>
    public string ExecuteTelegraphedAbility(Enemy boss, BossAbility ability, PlayerCharacter player)
    {
        string logMessage = $"\n╔═══════════════════════════════════════════════════════════════╗\n";
        logMessage += $"║ {ability.Name.ToUpper()}\n";
        logMessage += $"╚═══════════════════════════════════════════════════════════════╝\n";
        logMessage += $"{ability.ExecuteMessage}\n";

        // Deal damage
        if (ability.DamageDice > 0)
        {
            int damage = _diceService.RollDamage(ability.DamageDice) + ability.DamageBonus;

            if (ability.IsAoE)
            {
                logMessage += $"💥 AoE Attack: {damage} damage to all targets!\n";
                player.HP = Math.Max(0, player.HP - damage);
            }
            else
            {
                logMessage += $"💥 {damage} damage dealt!\n";
                player.HP = Math.Max(0, player.HP - damage);
            }

            _log.Information("Telegraphed ability executed: {BossId}, {AbilityId}, Damage={Damage}, IsAoE={IsAoE}",
                boss.Id, ability.Id, damage, ability.IsAoE);
        }

        // Apply status effects
        foreach (var statusEffect in ability.StatusEffects)
        {
            ApplyStatusEffect(player, statusEffect);
            logMessage += $"🔴 {player.Name} is [{statusEffect.StatusName}] for {statusEffect.Duration} turns!\n";
        }

        // Apply special effects
        if (ability.SpecialEffects != null)
        {
            logMessage += ApplySpecialEffects(boss, ability.SpecialEffects);
        }

        // Start cooldown
        if (ability.CooldownTurns > 0)
        {
            StartCooldown(boss.Id, ability.Id, ability.CooldownTurns);
            _log.Debug("Cooldown started: {BossId}, {AbilityId}, Duration={Turns}",
                boss.Id, ability.Id, ability.CooldownTurns);
        }

        // Apply vulnerability window
        if (ability.TriggersVulnerability && ability.VulnerabilityDuration > 0)
        {
            boss.VulnerableTurnsRemaining = ability.VulnerabilityDuration;
            logMessage += $"\n⚔️ {boss.Name} is [VULNERABLE] for {ability.VulnerabilityDuration} turn(s)! (+50% damage taken)\n";

            _log.Information("Vulnerability window applied: {BossId}, Duration={Turns}",
                boss.Id, ability.VulnerabilityDuration);
        }

        return logMessage;
    }

    /// <summary>
    /// Interrupt a charging telegraphed ability (e.g., via stun)
    /// </summary>
    public string InterruptAbility(Enemy boss, string abilityId)
    {
        string key = $"{boss.Id}_{abilityId}";

        if (_activeCharges.ContainsKey(key))
        {
            var telegraphed = _activeCharges[key];
            telegraphed.IsInterrupted = true;

            string logMessage = $"⚡ {boss.Name}'s {telegraphed.Ability?.Name} was INTERRUPTED!\n";

            _log.Information("Telegraphed ability interrupted: {BossId}, {AbilityId}",
                boss.Id, abilityId);

            _activeCharges.Remove(key);

            return logMessage;
        }

        return "";
    }

    /// <summary>
    /// Check if an ability is currently on cooldown
    /// </summary>
    public bool IsAbilityOnCooldown(string bossId, string abilityId)
    {
        if (!_activeCooldowns.ContainsKey(bossId))
            return false;

        return _activeCooldowns[bossId].Any(c => c.AbilityId == abilityId);
    }

    /// <summary>
    /// Get remaining cooldown turns for an ability
    /// </summary>
    public int GetCooldownRemaining(string bossId, string abilityId)
    {
        if (!_activeCooldowns.ContainsKey(bossId))
            return 0;

        var cooldown = _activeCooldowns[bossId].FirstOrDefault(c => c.AbilityId == abilityId);
        return cooldown?.RemainingTurns ?? 0;
    }

    /// <summary>
    /// Get all active telegraphed abilities for display
    /// </summary>
    public List<TelegraphedAbility> GetActiveTelegraphedAbilities()
    {
        return _activeCharges.Values.Where(t => !t.IsInterrupted).ToList();
    }

    /// <summary>
    /// Check if boss is currently charging an ability
    /// </summary>
    public bool IsBossChargingAbility(string bossId)
    {
        return _activeCharges.Values.Any(t => t.EnemyId == bossId && !t.IsInterrupted);
    }

    /// <summary>
    /// Start cooldown for an ability
    /// </summary>
    private void StartCooldown(string bossId, string abilityId, int duration)
    {
        if (!_activeCooldowns.ContainsKey(bossId))
        {
            _activeCooldowns[bossId] = new List<AbilityCooldown>();
        }

        _activeCooldowns[bossId].Add(new AbilityCooldown
        {
            AbilityId = abilityId,
            RemainingTurns = duration,
            TotalCooldown = duration
        });
    }

    /// <summary>
    /// Apply status effect to target
    /// </summary>
    private void ApplyStatusEffect(PlayerCharacter target, AbilityStatusEffect effect)
    {
        switch (effect.StatusName.ToLower())
        {
            case "stunned":
                // Stun logic handled by CombatEngine
                break;
            case "bleeding":
                // Bleeding logic handled by CombatEngine
                break;
            case "vulnerable":
                // Vulnerability applied separately
                break;
            default:
                _log.Warning("Unknown status effect: {StatusName}", effect.StatusName);
                break;
        }
    }

    /// <summary>
    /// Apply special effects (healing, buffs, summons)
    /// </summary>
    private string ApplySpecialEffects(Enemy boss, AbilitySpecialEffects effects)
    {
        string logMessage = "";

        // Healing
        if (effects.HealAmount > 0)
        {
            int healedAmount = Math.Min(effects.HealAmount, boss.MaxHP - boss.HP);
            boss.HP += healedAmount;
            logMessage += $"🔄 {boss.Name} heals for {healedAmount} HP ({boss.HP}/{boss.MaxHP})\n";
        }

        // Defense buff
        if (effects.DefenseBonus > 0 && effects.DefenseDuration > 0)
        {
            boss.DefenseBonus += effects.DefenseBonus;
            boss.DefenseTurnsRemaining = effects.DefenseDuration;
            logMessage += $"🛡️ {boss.Name} gains +{effects.DefenseBonus} Defense for {effects.DefenseDuration} turns\n";
        }

        // Summon adds (handled externally via AddWaveConfig)
        if (effects.SummonAdds != null)
        {
            logMessage += $"⚠️ {boss.Name} summons reinforcements!\n";
        }

        return logMessage;
    }

    /// <summary>
    /// Clear all tracking data
    /// </summary>
    public void Clear()
    {
        _activeCharges.Clear();
        _activeCooldowns.Clear();
        _currentTurn = 0;

        _log.Debug("Telegraphed ability service cleared");
    }
}
