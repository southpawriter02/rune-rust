namespace RuneAndRust.Core;

/// <summary>
/// v0.23.2: Tracks an active telegraphed ability being charged by a boss
/// Telegraphed abilities warn the player 1+ turns before execution,
/// allowing counterplay (repositioning, interrupts, defensive preparation)
/// </summary>
public class TelegraphedAbility
{
    /// <summary>
    /// ID of the boss ability being charged
    /// </summary>
    public string AbilityId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the enemy charging this ability
    /// </summary>
    public string EnemyId { get; set; } = string.Empty;

    /// <summary>
    /// Remaining charge turns before execution (0 = executes this turn)
    /// </summary>
    public int RemainingChargeTurns { get; set; } = 0;

    /// <summary>
    /// Total charge time (for UI display)
    /// </summary>
    public int TotalChargeTurns { get; set; } = 0;

    /// <summary>
    /// Reference to the full ability definition
    /// </summary>
    public BossAbility? Ability { get; set; } = null;

    /// <summary>
    /// Whether this ability has been interrupted
    /// </summary>
    public bool IsInterrupted { get; set; } = false;

    /// <summary>
    /// Turn when charging started (for tracking)
    /// </summary>
    public int ChargeStartTurn { get; set; } = 0;
}

/// <summary>
/// v0.23.2: Tracks active cooldowns for boss abilities
/// </summary>
public class AbilityCooldown
{
    /// <summary>
    /// ID of the ability on cooldown
    /// </summary>
    public string AbilityId { get; set; } = string.Empty;

    /// <summary>
    /// Remaining cooldown turns
    /// </summary>
    public int RemainingTurns { get; set; } = 0;

    /// <summary>
    /// Total cooldown duration (for UI display)
    /// </summary>
    public int TotalCooldown { get; set; } = 0;
}
