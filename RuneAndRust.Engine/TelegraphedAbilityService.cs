using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23.2: Manages telegraphed boss abilities with charge times, interrupt mechanics,
/// vulnerability windows, and database persistence
/// </summary>
public class TelegraphedAbilityService
{
    private static readonly ILogger _log = Log.ForContext<TelegraphedAbilityService>();
    private readonly BossEncounterRepository _repository;
    private readonly DiceService _diceService;

    public TelegraphedAbilityService(BossEncounterRepository repository, DiceService diceService)
    {
        _repository = repository;
        _diceService = diceService;
    }

    // ═══════════════════════════════════════════════════════════
    // TELEGRAPH CHARGING
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start charging a telegraphed ability
    /// Returns combat log message with warning
    /// </summary>
    public string BeginTelegraph(Enemy boss, BossAbilityData ability, int currentTurn, List<string>? targetIds = null)
    {
        if (!ability.IsTelegraphed)
        {
            _log.Warning("Attempted to telegraph non-telegraphed ability: {AbilityName}", ability.AbilityName);
            return "";
        }

        _log.Information("Beginning telegraph: Boss={BossName}, Ability={AbilityName}, ChargeTurns={ChargeTurns}",
            boss.Name, ability.AbilityName, ability.TelegraphChargeTurns);

        // Start telegraph in database
        _repository.StartTelegraph(
            boss.Id,
            ability.BossAbilityId,
            currentTurn,
            ability.TelegraphChargeTurns,
            ability.InterruptDamageThreshold,
            targetIds
        );

        // Create warning message
        string logMessage = $"\n⚠️  [WARNING] {boss.Name} begins charging {ability.AbilityName}!\n";

        if (!string.IsNullOrEmpty(ability.TelegraphWarningMessage))
        {
            logMessage += $"   {ability.TelegraphWarningMessage}\n";
        }

        logMessage += $"   ⏰ Executes in {ability.TelegraphChargeTurns} turn(s)!\n";

        if (ability.InterruptDamageThreshold > 0)
        {
            logMessage += $"   🛡️  Can be interrupted with {ability.InterruptDamageThreshold}+ damage!\n";
        }

        return logMessage;
    }

    /// <summary>
    /// Process all active telegraphs at end of turn
    /// Returns list of abilities ready to execute
    /// </summary>
    public List<(Enemy boss, BossAbilityData ability)> ProcessActiveTelegraphs(List<Enemy> bosses, int currentTurn)
    {
        var readyToExecute = new List<(Enemy boss, BossAbilityData ability)>();

        foreach (var boss in bosses.Where(b => b.IsBoss))
        {
            var activeTelegraphs = _repository.GetActiveTelegraphs(boss.Id);

            foreach (var telegraph in activeTelegraphs)
            {
                // Update current turn
                _repository.UpdateTelegraphTurn(telegraph.TelegraphStateId, currentTurn);

                // Check if ready to execute
                if (currentTurn >= telegraph.ChargeCompleteTurn)
                {
                    var ability = _repository.GetBossAbility(telegraph.BossAbilityId);
                    if (ability != null)
                    {
                        readyToExecute.Add((boss, ability));
                        _repository.CompleteTelegraph(telegraph.TelegraphStateId);

                        _log.Information("Telegraph ready to execute: Boss={BossId}, Ability={AbilityName}",
                            boss.Id, ability.AbilityName);
                    }
                }
            }
        }

        return readyToExecute;
    }

    /// <summary>
    /// Execute a telegraphed ability (damage, status effects, special effects)
    /// </summary>
    public string ExecuteTelegraphedAbility(Enemy boss, BossAbilityData ability, PlayerCharacter player, CombatState combatState)
    {
        _log.Information("Executing telegraphed ability: Boss={BossName}, Ability={AbilityName}",
            boss.Name, ability.AbilityName);

        string logMessage = $"\n╔═══════════════════════════════════════════════════════════════╗\n";
        logMessage += $"║ ⚡ {ability.AbilityName.ToUpper()}\n";
        logMessage += $"╚═══════════════════════════════════════════════════════════════╝\n";

        if (!string.IsNullOrEmpty(ability.AbilityDescription))
        {
            logMessage += $"{ability.AbilityDescription}\n\n";
        }

        // Deal damage
        if (ability.BaseDamageDice > 0)
        {
            int damage = CalculateDamage(boss, ability);

            if (ability.TargetType == "AoE" || ability.TargetType == "All")
            {
                logMessage += $"💥 AoE Attack: {damage} {ability.DamageType} damage to all targets!\n";
                player.HP = Math.Max(0, player.HP - damage);
            }
            else
            {
                logMessage += $"💥 {damage} {ability.DamageType} damage dealt to {player.Name}!\n";
                player.HP = Math.Max(0, player.HP - damage);
            }

            _log.Debug("Telegraph damage dealt: Damage={Damage}, Type={Type}, AoE={IsAoE}",
                damage, ability.DamageType, ability.TargetType);
        }

        // Apply status effects
        if (!string.IsNullOrEmpty(ability.AppliesStatusEffects))
        {
            logMessage += ApplyStatusEffects(player, ability.AppliesStatusEffects);
        }

        // Apply special effects
        if (!string.IsNullOrEmpty(ability.SpecialEffects))
        {
            logMessage += ApplySpecialEffects(boss, combatState, ability.SpecialEffects);
        }

        // Apply vulnerability window if ultimate ability
        if (ability.IsUltimate && ability.VulnerabilityDurationTurns > 0)
        {
            ApplyVulnerabilityWindow(boss, ability.VulnerabilityDurationTurns, ability.VulnerabilityDamageMultiplier);
            logMessage += $"\n⚔️  {boss.Name} is [VULNERABLE] for {ability.VulnerabilityDurationTurns} turn(s)! " +
                         $"(+{(ability.VulnerabilityDamageMultiplier - 1.0f) * 100:F0}% damage taken)\n";

            _log.Information("Vulnerability window applied: Boss={BossId}, Duration={Duration}, Multiplier={Multiplier}",
                boss.Id, ability.VulnerabilityDurationTurns, ability.VulnerabilityDamageMultiplier);
        }

        return logMessage;
    }

    // ═══════════════════════════════════════════════════════════
    // INTERRUPT MECHANICS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Add interrupt damage to active telegraphs
    /// Returns true if telegraph was interrupted
    /// </summary>
    public string? CheckTelegraphInterrupt(Enemy boss, int damageDealt)
    {
        var activeTelegraphs = _repository.GetActiveTelegraphs(boss.Id);

        foreach (var telegraph in activeTelegraphs)
        {
            if (telegraph.InterruptDamageThreshold <= 0)
                continue;

            bool interrupted = _repository.AddInterruptDamage(telegraph.TelegraphStateId, damageDealt);

            if (interrupted)
            {
                _repository.InterruptTelegraph(telegraph.TelegraphStateId);

                var ability = _repository.GetBossAbility(telegraph.BossAbilityId);

                _log.Information("Telegraph interrupted: Boss={BossId}, Ability={AbilityName}, Damage={Damage}",
                    boss.Id, ability?.AbilityName, damageDealt);

                // Apply staggered status
                ApplyStaggeredStatus(boss, ability?.InterruptStaggerDuration ?? 2);

                return $"\n⚡ INTERRUPTED! {boss.Name}'s {ability?.AbilityName} was disrupted!\n" +
                       $"   {boss.Name} is [Staggered] for {ability?.InterruptStaggerDuration ?? 2} turn(s)!\n";
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════
    // VULNERABILITY WINDOW SYSTEM
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Apply vulnerability window to boss (increased damage taken)
    /// </summary>
    public void ApplyVulnerabilityWindow(Enemy boss, int duration, float damageMultiplier)
    {
        boss.VulnerableTurnsRemaining = duration;
        boss.VulnerabilityDamageMultiplier = damageMultiplier;

        _log.Information("Vulnerability window applied: Boss={BossId}, Duration={Duration}, Multiplier={Multiplier}",
            boss.Id, duration, damageMultiplier);
    }

    /// <summary>
    /// Calculate damage modifier for boss based on vulnerability
    /// </summary>
    public float GetVulnerabilityMultiplier(Enemy boss)
    {
        if (boss.VulnerableTurnsRemaining > 0)
        {
            return boss.VulnerabilityDamageMultiplier;
        }

        return 1.0f;
    }

    /// <summary>
    /// Process vulnerability window at end of turn
    /// </summary>
    public string? ProcessVulnerabilityWindow(Enemy boss)
    {
        if (boss.VulnerableTurnsRemaining > 0)
        {
            boss.VulnerableTurnsRemaining--;

            if (boss.VulnerableTurnsRemaining == 0)
            {
                boss.VulnerabilityDamageMultiplier = 1.0f;
                _log.Debug("Vulnerability window expired: Boss={BossId}", boss.Id);
                return $"{boss.Name}'s vulnerability window has ended.\n";
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all active telegraphs for a boss
    /// </summary>
    public List<BossTelegraphStateData> GetActiveTelegraphs(string bossId)
    {
        return _repository.GetActiveTelegraphs(bossId);
    }

    /// <summary>
    /// Check if boss is currently charging any ability
    /// </summary>
    public bool IsBossChargingAbility(string bossId)
    {
        var telegraphs = _repository.GetActiveTelegraphs(bossId);
        return telegraphs.Any();
    }

    /// <summary>
    /// Get turns remaining until telegraph executes
    /// </summary>
    public int GetTelegraphTurnsRemaining(BossTelegraphStateData telegraph, int currentTurn)
    {
        return Math.Max(0, telegraph.ChargeCompleteTurn - currentTurn);
    }

    /// <summary>
    /// Clear all telegraphs for a boss (when combat ends)
    /// </summary>
    public void ClearTelegraphs(string bossId)
    {
        _repository.ClearTelegraphs(bossId);
        _log.Information("Telegraphs cleared: Boss={BossId}", bossId);
    }

    // ═══════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate damage for telegraphed ability
    /// </summary>
    private int CalculateDamage(Enemy boss, BossAbilityData ability)
    {
        // Use custom formula if provided
        if (!string.IsNullOrEmpty(ability.DamageFormula))
        {
            // TODO: Parse and evaluate damage formula
            // For now, use standard calculation
        }

        // Standard damage calculation
        int damage = 0;
        for (int i = 0; i < ability.BaseDamageDice; i++)
        {
            damage += _diceService.RollD6();
        }

        damage += ability.DamageBonus;
        damage += boss.DamageBonus; // Add boss damage bonus

        return Math.Max(1, damage);
    }

    /// <summary>
    /// Apply status effects from JSON
    /// </summary>
    private string ApplyStatusEffects(PlayerCharacter target, string statusEffectsJson)
    {
        string logMessage = "";

        try
        {
            var statusEffects = JsonSerializer.Deserialize<List<TelegraphedStatusEffectData>>(statusEffectsJson);
            if (statusEffects == null) return logMessage;

            foreach (var effect in statusEffects)
            {
                logMessage += $"🔴 {target.Name} is [{effect.StatusName}] for {effect.Duration} turns!\n";

                _log.Debug("Status effect applied: Target={TargetName}, Status={StatusName}, Duration={Duration}",
                    target.Name, effect.StatusName, effect.Duration);

                // Status effect application logic would go here
                // (Integration with existing status effect system)
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse status effects: {Json}", statusEffectsJson);
        }

        return logMessage;
    }

    /// <summary>
    /// Apply special effects from JSON
    /// </summary>
    private string ApplySpecialEffects(Enemy boss, CombatState combatState, string specialEffectsJson)
    {
        string logMessage = "";

        try
        {
            var specialEffects = JsonSerializer.Deserialize<SpecialEffectDefinition>(specialEffectsJson);
            if (specialEffects == null) return logMessage;

            // Healing
            if (specialEffects.HealAmount > 0)
            {
                int healedAmount = Math.Min(specialEffects.HealAmount, boss.MaxHP - boss.HP);
                boss.HP += healedAmount;
                logMessage += $"🔄 {boss.Name} heals for {healedAmount} HP ({boss.HP}/{boss.MaxHP})\n";
            }

            // Defense buff
            if (specialEffects.DefenseBonus > 0 && specialEffects.DefenseDuration > 0)
            {
                boss.DefenseBonus += specialEffects.DefenseBonus;
                boss.DefenseTurnsRemaining = specialEffects.DefenseDuration;
                logMessage += $"🛡️ {boss.Name} gains +{specialEffects.DefenseBonus} Defense for {specialEffects.DefenseDuration} turns\n";
            }

            // Summon adds
            if (specialEffects.SummonCount > 0 && specialEffects.SummonType != null)
            {
                for (int i = 0; i < specialEffects.SummonCount; i++)
                {
                    var summoned = EnemyFactory.CreateEnemy(specialEffects.SummonType.Value);
                    combatState.Enemies.Add(summoned);
                }
                logMessage += $"⚠️ {boss.Name} summons {specialEffects.SummonCount}x {specialEffects.SummonType}!\n";
            }

            _log.Debug("Special effects applied: Boss={BossId}, Effects={Effects}", boss.Id, specialEffectsJson);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse special effects: {Json}", specialEffectsJson);
        }

        return logMessage;
    }

    /// <summary>
    /// Apply staggered status to boss (from interrupt)
    /// </summary>
    private void ApplyStaggeredStatus(Enemy boss, int duration)
    {
        // Staggered status: -2 to all attributes for duration
        boss.StaggeredTurnsRemaining = duration;

        _log.Information("Staggered status applied: Boss={BossId}, Duration={Duration}", boss.Id, duration);
    }
}

// ═══════════════════════════════════════════════════════════
// HELPER DATA MODELS
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Status effect data for telegraphed ability JSON deserialization
/// (Renamed to avoid conflict with RuneAndRust.Core.StatusEffectDefinition)
/// </summary>
public class TelegraphedStatusEffectData
{
    public string StatusName { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int DamagePerTurn { get; set; }
}

/// <summary>
/// Special effect definition for JSON deserialization
/// </summary>
public class SpecialEffectDefinition
{
    public int HealAmount { get; set; }
    public int DefenseBonus { get; set; }
    public int DefenseDuration { get; set; }
    public int SummonCount { get; set; }
    public EnemyType? SummonType { get; set; }
}
