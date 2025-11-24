namespace RuneAndRust.Core.AI;

/// <summary>
/// Analysis of player strategy patterns for adaptive difficulty.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public class PlayerStrategy
{
    /// <summary>
    /// True if players are staying in one location (not moving).
    /// Counter: Use AOE at their location to force movement.
    /// </summary>
    public bool IsCamping { get; set; }

    /// <summary>
    /// True if players are healing frequently.
    /// Counter: Target the healer, use burst damage.
    /// </summary>
    public bool IsHealingHeavily { get; set; }

    /// <summary>
    /// True if players are using ranged attacks exclusively (staying away).
    /// Counter: Use gap closers, ranged abilities.
    /// </summary>
    public bool IsKiting { get; set; }

    /// <summary>
    /// True if players are killing adds quickly.
    /// Counter: Increase boss aggression, protect adds.
    /// </summary>
    public bool IsPrioritizingAdds { get; set; }

    /// <summary>
    /// True if players are using a lot of defensive abilities.
    /// Counter: Use armor-piercing or unavoidable damage.
    /// </summary>
    public bool IsPlayingDefensively { get; set; }

    /// <summary>
    /// True if players are burning down the boss quickly (ignoring adds).
    /// Counter: Summon more adds, use defensive abilities.
    /// </summary>
    public bool IsBurningBoss { get; set; }

    /// <summary>
    /// Number of turns analyzed to determine strategy.
    /// </summary>
    public int TurnsAnalyzed { get; set; }

    /// <summary>
    /// Timestamp of analysis.
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}
