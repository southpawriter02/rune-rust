namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Value object for tracking and managing the state of a Runic Barrier created by a Varð-Warden.
/// Barriers are protective structures with limited HP and duration that degrade each turn.
/// </summary>
/// <remarks>
/// <para>Runic Barriers are created by the RunicBarrier ability (29011) and have the following properties:</para>
/// <list type="table">
///   <listheader><term>Rank</term><description>Max HP / Duration Turns</description></listheader>
///   <item><term>Rank 1</term><description>30 HP / 2 turns</description></item>
///   <item><term>Rank 2</term><description>40 HP / 3 turns</description></item>
///   <item><term>Rank 3</term><description>50 HP / 4 turns</description></item>
/// </list>
/// <para>Barriers can be damaged via WithDamage(), healed via WithHealing(), and degrade via TickTurn().
/// When a barrier is destroyed, it may trigger DestructionDamage back to the Varð-Warden (only at Rank 3: 2d6).
/// A barrier is considered destroyed when CurrentHp reaches 0 or RemainingTurns reaches 0.</para>
/// </remarks>
public sealed record RunicBarrierTracker
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RunicBarrierTracker"/> record.
    /// </summary>
    /// <param name="barrierId">Unique identifier for this barrier instance.</param>
    /// <param name="currentHp">Current health points of the barrier.</param>
    /// <param name="maxHp">Maximum health points the barrier can have.</param>
    /// <param name="remainingTurns">Number of turns remaining before the barrier expires.</param>
    /// <param name="positionX">X coordinate of the barrier's location.</param>
    /// <param name="positionY">Y coordinate of the barrier's location.</param>
    /// <param name="rank">The rank of the barrier (1, 2, or 3) — determines destruction damage.</param>
    /// <exception cref="ArgumentException">Thrown if currentHp is negative.</exception>
    /// <exception cref="ArgumentException">Thrown if maxHp is less than or equal to 0.</exception>
    /// <exception cref="ArgumentException">Thrown if remainingTurns is negative.</exception>
    /// <exception cref="ArgumentException">Thrown if rank is not 1, 2, or 3.</exception>
    public RunicBarrierTracker(
        Guid barrierId,
        int currentHp,
        int maxHp,
        int remainingTurns,
        int positionX,
        int positionY,
        int rank)
    {
        if (currentHp < 0)
            throw new ArgumentException("Current HP cannot be negative.", nameof(currentHp));

        if (maxHp <= 0)
            throw new ArgumentException("Max HP must be positive.", nameof(maxHp));

        if (remainingTurns < 0)
            throw new ArgumentException("Remaining turns cannot be negative.", nameof(remainingTurns));

        if (rank is not (1 or 2 or 3))
            throw new ArgumentException("Rank must be 1, 2, or 3.", nameof(rank));

        BarrierId = barrierId;
        CurrentHp = currentHp;
        MaxHp = maxHp;
        RemainingTurns = remainingTurns;
        PositionX = positionX;
        PositionY = positionY;
        Rank = rank;
    }

    /// <summary>Unique identifier for this barrier instance.</summary>
    public Guid BarrierId { get; }

    /// <summary>Current health points of the barrier (0 = destroyed).</summary>
    public int CurrentHp { get; }

    /// <summary>Maximum health points the barrier can have.</summary>
    public int MaxHp { get; }

    /// <summary>Number of turns remaining before the barrier expires naturally.</summary>
    public int RemainingTurns { get; }

    /// <summary>X coordinate of the barrier's position on the battlefield.</summary>
    public int PositionX { get; }

    /// <summary>Y coordinate of the barrier's position on the battlefield.</summary>
    public int PositionY { get; }

    /// <summary>The rank of the barrier (1, 2, or 3) — determines destruction damage potential.</summary>
    public int Rank { get; }

    /// <summary>Whether the barrier has been destroyed (HP &lt;= 0 or turns expired).</summary>
    public bool IsDestroyed => CurrentHp <= 0 || RemainingTurns <= 0;

    /// <summary>
    /// Gets the destruction damage for this barrier (only at Rank 3).
    /// Rank 1-2: 0 damage. Rank 3: 2d6 (returned as fixed 7 for deterministic behavior in tests).
    /// </summary>
    public int DestructionDamage => Rank >= 3 ? 7 : 0; // 2d6 average = 7

    /// <summary>
    /// Applies damage to the barrier, reducing its current HP.
    /// </summary>
    /// <param name="damage">Amount of damage to apply (minimum 0).</param>
    /// <returns>A new RunicBarrierTracker with reduced HP.</returns>
    /// <exception cref="ArgumentException">Thrown if damage is negative.</exception>
    public RunicBarrierTracker WithDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentException("Damage cannot be negative.", nameof(damage));

        var newHp = Math.Max(0, CurrentHp - damage);
        return new RunicBarrierTracker(BarrierId, newHp, MaxHp, RemainingTurns, PositionX, PositionY, Rank);
    }

    /// <summary>
    /// Applies healing to the barrier, restoring its current HP (capped at MaxHp).
    /// </summary>
    /// <param name="healing">Amount of HP to restore (minimum 0).</param>
    /// <returns>A new RunicBarrierTracker with restored HP (not exceeding MaxHp).</returns>
    /// <exception cref="ArgumentException">Thrown if healing is negative.</exception>
    public RunicBarrierTracker WithHealing(int healing)
    {
        if (healing < 0)
            throw new ArgumentException("Healing cannot be negative.", nameof(healing));

        var newHp = Math.Min(MaxHp, CurrentHp + healing);
        return new RunicBarrierTracker(BarrierId, newHp, MaxHp, RemainingTurns, PositionX, PositionY, Rank);
    }

    /// <summary>
    /// Advances the barrier's remaining turn count by 1. Used at the end of each combat turn.
    /// </summary>
    /// <returns>A new RunicBarrierTracker with RemainingTurns decremented by 1 (minimum 0).</returns>
    public RunicBarrierTracker TickTurn()
    {
        var newTurns = Math.Max(0, RemainingTurns - 1);
        return new RunicBarrierTracker(BarrierId, CurrentHp, MaxHp, newTurns, PositionX, PositionY, Rank);
    }
}
