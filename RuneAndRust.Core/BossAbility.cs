namespace RuneAndRust.Core;

/// <summary>
/// v0.23.2: Types of boss abilities
/// </summary>
public enum BossAbilityType
{
    /// <summary>
    /// Standard attack with no charge time
    /// </summary>
    Standard,

    /// <summary>
    /// Telegraphed attack that requires charging (1+ turns)
    /// </summary>
    Telegraphed,

    /// <summary>
    /// Ultimate ability with long cooldown, often grants vulnerability window after use
    /// </summary>
    Ultimate,

    /// <summary>
    /// Passive ability that's always active
    /// </summary>
    Passive
}

/// <summary>
/// v0.23.2: Defines a boss ability with telegraphing and cooldown mechanics
/// Boss abilities can be telegraphed (charged over multiple turns), have cooldowns,
/// and trigger vulnerability windows after use
/// </summary>
public class BossAbility
{
    /// <summary>
    /// Unique identifier for this ability
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name
    /// Example: "Total System Failure", "Emergency Protocols"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description shown in combat log
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of ability (Standard, Telegraphed, Ultimate, Passive)
    /// </summary>
    public BossAbilityType Type { get; set; } = BossAbilityType.Standard;

    /// <summary>
    /// Number of turns required to charge this ability (0 = instant)
    /// Telegraphed abilities typically have ChargeTurns >= 1
    /// </summary>
    public int ChargeTurns { get; set; } = 0;

    /// <summary>
    /// Message shown when ability starts charging
    /// Example: "The Forlorn Commander's core begins to overload! [WARNING]"
    /// </summary>
    public string ChargeMessage { get; set; } = string.Empty;

    /// <summary>
    /// Message shown when ability executes
    /// Example: "Total System Failure unleashed! Massive electrical discharge!"
    /// </summary>
    public string ExecuteMessage { get; set; } = string.Empty;

    /// <summary>
    /// Cooldown turns before ability can be used again (0 = no cooldown)
    /// </summary>
    public int CooldownTurns { get; set; } = 0;

    /// <summary>
    /// Whether this ability triggers a vulnerability window after use
    /// </summary>
    public bool TriggersVulnerability { get; set; } = false;

    /// <summary>
    /// Duration of vulnerability window in turns (if TriggersVulnerability = true)
    /// </summary>
    public int VulnerabilityDuration { get; set; } = 0;

    /// <summary>
    /// Damage dice for this ability (0 = no damage)
    /// </summary>
    public int DamageDice { get; set; } = 0;

    /// <summary>
    /// Flat damage bonus
    /// </summary>
    public int DamageBonus { get; set; } = 0;

    /// <summary>
    /// Whether this ability targets all enemies (AoE)
    /// </summary>
    public bool IsAoE { get; set; } = false;

    /// <summary>
    /// Status effects applied by this ability
    /// </summary>
    public List<AbilityStatusEffect> StatusEffects { get; set; } = new();

    /// <summary>
    /// Special effects (healing, buffs, etc.)
    /// </summary>
    public AbilitySpecialEffects? SpecialEffects { get; set; } = null;

    /// <summary>
    /// Can this ability be interrupted?
    /// </summary>
    public bool CanBeInterrupted { get; set; } = true;

    /// <summary>
    /// Minimum HP percentage required to use this ability (0-100)
    /// Example: 25 means ability only available when boss is below 25% HP
    /// </summary>
    public int MinimumHPPercentage { get; set; } = 0;

    /// <summary>
    /// Maximum HP percentage to use this ability (100 = no limit)
    /// Example: 50 means ability only available when boss is above 50% HP
    /// </summary>
    public int MaximumHPPercentage { get; set; } = 100;
}

/// <summary>
/// v0.23.2: Status effect applied by a boss ability
/// </summary>
public class AbilityStatusEffect
{
    /// <summary>
    /// Name of the status effect (Stunned, Bleeding, Vulnerable, etc.)
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// Duration in turns
    /// </summary>
    public int Duration { get; set; } = 0;

    /// <summary>
    /// Damage per turn (for DoT effects)
    /// </summary>
    public int DamagePerTurn { get; set; } = 0;
}

/// <summary>
/// v0.23.2: Special effects for boss abilities (healing, buffs, etc.)
/// </summary>
public class AbilitySpecialEffects
{
    /// <summary>
    /// HP healed by this ability
    /// </summary>
    public int HealAmount { get; set; } = 0;

    /// <summary>
    /// Defense bonus granted
    /// </summary>
    public int DefenseBonus { get; set; } = 0;

    /// <summary>
    /// Duration of defense bonus in turns
    /// </summary>
    public int DefenseDuration { get; set; } = 0;

    /// <summary>
    /// Summons add wave (enemy type and count)
    /// </summary>
    public AddWaveConfig? SummonAdds { get; set; } = null;

    /// <summary>
    /// Triggers environmental hazard
    /// </summary>
    public string? TriggerEnvironmentalHazard { get; set; } = null;
}
